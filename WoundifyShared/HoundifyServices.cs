using System;
using System.Linq;

using System.Runtime.InteropServices.WindowsRuntime; // AsBuffer()

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WoundifyShared
{
    class HoundifyServices : WoundifyServices
    {
        // save IntentConversationState (info about request and requestor) obtained from Houndify's ResultInfo for passing on to next Houndify request.
        private static JToken IntentConversationState = null;
        private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        public override async System.Threading.Tasks.Task<IIntentServiceResponse> IntentServiceAsync(string text)
        {
            IIntentServiceResponse response = new IIntentServiceResponse();
            dynamic settings = Options.options.Services.APIs.Intent.HoundifyIntent;
            UriBuilder ub = new UriBuilder();
            ub.Scheme = "https";
            ub.Host = "api.houndify.com";
            ub.Path = "v1/text";
            ub.Query = "query=" + System.Uri.EscapeDataString(text.Trim());
            response.sr = await PostAsync(ub, new byte[0], settings);
            return response;
        }

        public override async System.Threading.Tasks.Task<IIntentServiceResponse> IntentServiceAsync(byte[] audioBytes, int sampleRate)
        {
            IIntentServiceResponse response = new IIntentServiceResponse();
            dynamic settings = Options.options.Services.APIs.Intent.HoundifyIntent;
            UriBuilder ub = new UriBuilder();
            ub.Scheme = "https";
            ub.Host = "api.houndify.com";
            ub.Path = "v1/audio";
            response.sr = await PostAsync(ub, audioBytes, settings, sampleRate);
            return response;
        }

        public override async System.Threading.Tasks.Task<ISpeechToTextServiceResponse> SpeechToTextAsync(byte[] audioBytes, int sampleRate)
        {
            ISpeechToTextServiceResponse response = new ISpeechToTextServiceResponse();
            dynamic settings = Options.options.Services.APIs.SpeechToText.HoundifySpeechToText;
            UriBuilder ub = new UriBuilder();
            ub.Scheme = "https";
            ub.Host = "api.houndify.com";
            ub.Path = "v1/audio";
            response.sr = await PostAsync(ub, audioBytes, settings, sampleRate);
            return response;
        }

#if WINDOWS_UWP
        public async System.Threading.Tasks.Task<IServiceResponse> PostAsyncWindowsWeb(Uri uri, string RequestBodyJson, byte[] RequestBodyBytes, string HoundRequestAuthentication, string HoundClientAuthentication)
        {
        IServiceResponse response = new IServiceResponse();
            try
            {
                // Using HttpClient to grab chunked encoding (partial) responses.
                using (Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Hound-Request-Info-Length", RequestBodyJson.Length.ToString());
                    httpClient.DefaultRequestHeaders.Add("Hound-Request-Authentication", HoundRequestAuthentication); // user-id;request-id
                    httpClient.DefaultRequestHeaders.Add("Hound-Client-Authentication", HoundClientAuthentication); // client-id;timestamp;signature

                    Windows.Web.Http.IHttpContent requestContent = null;
                    if (Options.options.Services.APIs.PreferChunkedEncodedRequests)
                    {
                        // using chunked transfer requests
                        Log.WriteLine("Using chunked encoding");
                        Windows.Storage.Streams.InMemoryRandomAccessStream contentStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                        // todo: obsolete to use DataWriter? use await Windows.Storage.FileIO.Write..(file);
                        Windows.Storage.Streams.DataWriter dw = new Windows.Storage.Streams.DataWriter(contentStream);
                        dw.WriteBytes(RequestBodyBytes);
                        await dw.StoreAsync();
                        // GetInputStreamAt(0) forces chunked transfer (sort of undocumented behavior).
                        requestContent = new Windows.Web.Http.HttpStreamContent(contentStream.GetInputStreamAt(0));
                    }
                    else
                    {
                        requestContent = new Windows.Web.Http.HttpBufferContent(RequestBodyBytes.AsBuffer());
                    }
                    // houndify doesn't need Content-Type? // requestContent.Headers.Add("Content-Type", contentType);

                    Log.WriteLine("Before Post: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                    response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                    using(Windows.Web.Http.HttpResponseMessage rm = await httpClient.PostAsync(uri, requestContent))
                    {
                        response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds - response.RequestElapsedMilliseconds;
                        response.StatusCode = (int)rm.StatusCode;
                        Log.WriteLine("After Post: StatusCode:" + response.StatusCode + " Total milliseconds:" + stopWatch.ElapsedMilliseconds + " Request milliseconds:" + response.RequestElapsedMilliseconds);
                        if (rm.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                        {
                            string responseContents = await rm.Content.ReadAsStringAsync();
                            string[] responseJsons = responseContents.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            foreach (string rj in responseJsons)
                            {
                                response.ResponseJson = rj;
                                JToken ResponseBodyToken = JObject.Parse(response.ResponseJson);
                                response.ResponseJsonFormatted = JsonConvert.SerializeObject(ResponseBodyToken, new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                                if (Options.options.debugLevel >= 4)
                                    Log.WriteLine(response.ResponseJsonFormatted);
                                JToken tokResult = ProcessResponse(ResponseBodyToken);
                                if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
                                {
                                    response.ResponseResult = Options.options.Services.APIs.SpeechToText.missingResponse;
                                    if (Options.options.debugLevel >= 3)
                                        Log.WriteLine("ResponseResult:" + response.ResponseResult);
                                }
                                else
                                {
                                    response.ResponseResult = tokResult.ToString();
                                    if (Options.options.debugLevel >= 3)
                                        Log.WriteLine("ResponseResult:" + tokResult.Path + ": " + response.ResponseResult);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Exception:" + ex.Message);
                if (ex.InnerException != null)
                    Log.WriteLine("InnerException:" + ex.InnerException);
            }
        return response;
        }
#endif

        public async System.Threading.Tasks.Task<IServiceResponse> PostAsyncSystemNet(Uri uri, string RequestBodyJson, byte[] RequestBodyBytes, string HoundRequestAuthentication, string HoundClientAuthentication)
        {
            IServiceResponse response = new IServiceResponse();
            try
            {
                // Using HttpClient to grab chunked encoding (partial) responses.
                using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Hound-Request-Info-Length", RequestBodyJson.Length.ToString());
                    httpClient.DefaultRequestHeaders.Add("Hound-Request-Authentication", HoundRequestAuthentication); // user-id;request-id
                    httpClient.DefaultRequestHeaders.Add("Hound-Client-Authentication", HoundClientAuthentication); // client-id;timestamp;signature
                    if (Options.options.Services.APIs.PreferChunkedEncodedRequests)
                    {
                        Log.WriteLine("Using chunked encoding");
                        httpClient.DefaultRequestHeaders.Add("Content-Length", "0");
                        httpClient.DefaultRequestHeaders.Add("Transfer-Encoding", "chunked");
                    }
                    Log.WriteLine("Before Post: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                    response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                    using (System.Net.Http.HttpResponseMessage rm = await httpClient.PostAsync(uri, new System.Net.Http.ByteArrayContent(RequestBodyBytes)))
                    {
                        response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds - response.RequestElapsedMilliseconds;
                        response.StatusCode = (int)rm.StatusCode;
                        Log.WriteLine("After Post: StatusCode:" + response.StatusCode + " Total milliseconds:" + stopWatch.ElapsedMilliseconds + " Request milliseconds:" + response.RequestElapsedMilliseconds);
                        if (rm.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            using (System.IO.Stream rr = await rm.Content.ReadAsStreamAsync())
                            {
                                using (System.IO.StreamReader r = new System.IO.StreamReader(rr)) // if needed, there is a constructor which will leave the stream open
                                {
                                    // attempts to display individual partial responses. However, some lame assumptions needed to do so.
                                    while (!r.EndOfStream)
                                    {
                                        string ResponseBodyBlob = r.ReadLine();
                                        if (Options.options.debugLevel >= 4)
                                            Log.WriteLine("ResponseBodyBlob:" + ResponseBodyBlob);
                                        string ResponseBodyString = null;
                                        JToken ResponseBodyToken = null;
                                        while (ResponseBodyBlob != "")
                                        {
                                            try
                                            {
                                                ResponseBodyToken = JObject.Parse(ResponseBodyBlob);
                                                ResponseBodyString = ResponseBodyBlob;
                                            }
                                            catch (JsonReaderException ex) when (ex.HResult == -2146233088)
                                            {
                                                Log.WriteLine("Exception:" + ex.Message);
                                                ResponseBodyString = ResponseBodyBlob.Substring(0, ex.LinePosition);
                                                ResponseBodyToken = JObject.Parse(ResponseBodyString);
                                            }
                                            response.ResponseJson = ResponseBodyBlob = ResponseBodyBlob.Substring(ResponseBodyString.Length);
                                            response.ResponseJsonFormatted = JsonConvert.SerializeObject(ResponseBodyToken, new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                                            if (Options.options.debugLevel >= 4)
                                                Log.WriteLine(response.ResponseJsonFormatted);
                                            JToken tokResult = ProcessResponse(ResponseBodyToken);
                                            if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
                                            {
                                                response.ResponseResult = Options.options.Services.APIs.SpeechToText.missingResponse;
                                                if (Options.options.debugLevel >= 3)
                                                    Log.WriteLine("ResponseResult:" + response.ResponseResult);
                                            }
                                            else
                                            {
                                                response.ResponseResult = tokResult.ToString();
                                                if (Options.options.debugLevel >= 3)
                                                    Log.WriteLine(tokResult.Path + ": " + response.ResponseResult);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            response.ResponseJson = rm.ReasonPhrase;
                            Log.WriteLine("SendAsync Failed: StatusCode:" + rm.ReasonPhrase + "(" + response.StatusCode.ToString() + ")");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Exception:" + ex.Message);
                if (ex.InnerException != null)
                    Log.WriteLine("InnerException:" + ex.InnerException);
            }
            return response;
        }

        public async System.Threading.Tasks.Task<IServiceResponse> PostAsync(UriBuilder ub, byte[] RequestContentBytes, dynamic settings, int sampleRate = 0)
        {
            IServiceResponse response = new IServiceResponse();
            Log.WriteLine("Content length:" + RequestContentBytes.Length);
            stopWatch.Start();

            string ClientID = settings.ClientID; // moot? throws Exception thrown: 'Microsoft.CSharp.RuntimeBinder.RuntimeBinderException' in Microsoft.CSharp.dll
            string ClientKey = settings.ClientKey; // moot? Exception thrown: 'Microsoft.CSharp.RuntimeBinder.RuntimeBinderException' in Microsoft.CSharp.dll
            string UserID = settings.UserID;

            JObject RequestBodyObject = JObject.FromObject(new
            {
                Latitude = GeoLocation.latitude,
                Longitude = GeoLocation.longitude,
                City = GeoLocation.town,
                Country = GeoLocation.country,
                UserID = UserID,
                ClientID = ClientID,
                // audio specific
                PartialTranscriptsDesired = Options.options.Services.APIs.Intent.HoundifyIntent.PartialTranscriptsDesired
            });

            if (IntentConversationState != null)
                RequestBodyObject.Add(IntentConversationState);

            string RequestBodyJson = JsonConvert.SerializeObject(RequestBodyObject); // no formatting. Could use ToString() but it formats (spaces, EOL).
            if (Options.options.debugLevel >= 4)
                Log.WriteLine("RequestBodyJson:" + RequestBodyJson);

            byte[] RequestBodyBytes = System.Text.Encoding.UTF8.GetBytes(RequestBodyJson).Concat(RequestContentBytes).ToArray();
            //byte[] RequestBodyBytes = System.Text.Encoding.UTF8.GetBytes(RequestBodyJson).Concat(ReadBytesFromFile("wb_male.wav")).ToArray();

            string RequestID = System.Guid.NewGuid().ToString("D"); // Houndify requires lower case?
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            string Timestamp = Math.Floor(t.TotalSeconds).ToString();

            string FriendlyClientKey = ClientKey.Replace('-', '+').Replace('_', '/'); // translate possible PHP encode style to FromBase64String style
#if WINDOWS_UWP
            Windows.Storage.Streams.IBuffer KeyBytes = Windows.Security.Cryptography.CryptographicBuffer.DecodeFromBase64String(FriendlyClientKey);
            Windows.Storage.Streams.IBuffer MessageBytes = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(UserID + ";" + RequestID + Timestamp, Windows.Security.Cryptography.BinaryStringEncoding.Utf8);
            Windows.Security.Cryptography.Core.MacAlgorithmProvider MacAlg = Windows.Security.Cryptography.Core.MacAlgorithmProvider.OpenAlgorithm(Windows.Security.Cryptography.Core.MacAlgorithmNames.HmacSha256);
            Windows.Security.Cryptography.Core.CryptographicHash MacHash = MacAlg.CreateHash(KeyBytes);
            MacHash.Append(MessageBytes);
            Windows.Storage.Streams.IBuffer MacHashBuf = MacHash.GetValueAndReset();
            string HashSignature = Windows.Security.Cryptography.CryptographicBuffer.EncodeToBase64String(MacHashBuf).Replace('+', '-').Replace('/', '_');
#else
            byte[] KeyBytes = Convert.FromBase64String(FriendlyClientKey); // todo: shouldn't this be System.Text.Encoding.UTF8.GetBytes?
            byte[] MessageBytes = System.Text.Encoding.UTF8.GetBytes(UserID + ";" + RequestID + Timestamp);
            byte[] HashBytes = new System.Security.Cryptography.HMACSHA256(KeyBytes).ComputeHash(MessageBytes); // always length of 32?
            string HashSignature = Convert.ToBase64String(HashBytes).Replace('+', '-').Replace('/', '_');
#endif
            string HoundRequestAuthentication = UserID + ";" + RequestID;
            string HoundClientAuthentication = ClientID + ";" + Timestamp + ";" + HashSignature;

            if (Options.options.debugLevel >= 4)
            {
                Log.WriteLine("Uri:" + ub.Uri);
                Log.WriteLine("UserID:" + UserID);
                Log.WriteLine("RequestID:" + RequestID);
                Log.WriteLine("Timestamp:" + Timestamp);
                Log.WriteLine("ClientID:" + ClientID);
                Log.WriteLine("ClientKey:" + ClientKey);
                Log.WriteLine("FriendlyClientKey:" + FriendlyClientKey);
                Log.WriteLine("HashSignature:" + HashSignature);
                Log.WriteLine("HoundRequestAuthentication:" + HoundRequestAuthentication);
                Log.WriteLine("HoundClientAuthentication:" + HoundClientAuthentication);
            }
#if true
            Log.WriteLine("curl -X POST --data-binary @computer.wav --header \"Hound-Request-Authentication:" + HoundRequestAuthentication + "\" --header \"Hound-Client-Authentication:" + HoundClientAuthentication + "\" --header \"Hound-Request-Info:" + RequestBodyJson.Replace('"', '\'') + "\" " + ub.Uri);
#endif

#if WINDOWS_UWP
            if (Options.options.Services.APIs.PreferSystemNet)
                response = await PostAsyncSystemNet(ub.Uri, RequestBodyJson, RequestBodyBytes, HoundRequestAuthentication, HoundClientAuthentication);
            else
                response = await PostAsyncWindowsWeb(ub.Uri, RequestBodyJson, RequestBodyBytes, HoundRequestAuthentication, HoundClientAuthentication);
#else
            response = await PostAsyncSystemNet(ub.Uri, RequestBodyJson, RequestBodyBytes, HoundRequestAuthentication, HoundClientAuthentication);
#endif
            stopWatch.Stop();
            response.TotalElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
            return response;
        }

        private JToken ProcessResponse(JToken response)
        {
            JToken tok = null;
            if (response == null)
                Log.WriteLine("Fail! - Response is null");
            else
            {
                if ((response.SelectToken("$.Status")) == null)
                    Log.WriteLine("Status property not found");
                else
                    Log.WriteLine("Status is " + response.SelectToken("$.Status").ToString());
                if ((tok = response.SelectToken("$.AllResults.ConversationState")) != null && (tok = tok.Parent) != null) // applies to many formats?
                    IntentConversationState = tok;
                JToken tokFormat;
                if ((tokFormat = response.SelectToken("$.Format")) != null)
                {
                    Tuple<string, string>[] tuple = new Tuple<string, string>[] {
                        new Tuple<string, string>("SoundHoundVoiceSearchParialTranscript", "$.PartialTranscript"),
                        new Tuple<string, string>("SoundHoundVoiceSearchResult", "$.AllResults..SpokenResponseLong")
                    };
                    foreach (Tuple<string, string> t in tuple)
                    {
                        if (tokFormat.ToString() == t.Item1)
                            if ((tok = response.SelectToken(t.Item2)) != null) // && (tok = tok.Parent) != null)
                                break;
                    }
                }
            }
            return tok;
        }

    }
}
