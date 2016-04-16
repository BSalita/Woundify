using System;
using System.Net;

using System.Runtime.InteropServices.WindowsRuntime; // AsBuffer()

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WoundifyShared
{
    class BingServices : WoundifyServices
    {
        private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        private static System.Collections.Generic.List<string> Analyzers;
        private static string AnalyzerStringized;

        public BingServices()
        {
            if (Analyzers == null)
                GetAnalyzers().Wait();
        }

        public override async System.Threading.Tasks.Task<IParseServiceResponse> ParseServiceAsync(string text)
        {
            IParseServiceResponse response = new IParseServiceResponse();
            Log.WriteLine("Bing: parse:" + text);
            dynamic settings = Options.options.Services.APIs.Parse.BingParse;
            UriBuilder ub = new UriBuilder();
            ub.Scheme = "https";
            ub.Host = "api.projectoxford.ai";
            ub.Path = "linguistics/v1.0/analyze";
            text = "{'language' : 'en', 'analyzerIds' : [" + AnalyzerStringized + "], 'text' :'" + text + "'}";
            System.Collections.Generic.List<Tuple<string, string>> Headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Ocp-Apim-Subscription-Key", settings.OcpApimSubscriptionKey) // todo: get from settings
            };
            // todo: maybe a dictionary of headers should be passed including content-type. Then PostAsync can do if (dict.Contains("Content-Type")) headers.add(dict...)
            // todo: maybe should init a header class, add values and pass to Post?
            response.sr = await PostAsyncSystemNet(ub.Uri, text, "application/json", Headers);
            return response;
        }

        private async System.Threading.Tasks.Task<IServiceResponse> GetAnalyzers()
        {
            Log.WriteLine("Bing: GetAnalyzers");
            Analyzers = new System.Collections.Generic.List<string>();
            dynamic settings = Options.options.Services.APIs.Parse.BingParse;
            UriBuilder ub = new UriBuilder();
            ub.Scheme = "https";
            ub.Host = "api.projectoxford.ai";
            ub.Path = "linguistics/v1.0/analyzers";
            System.Collections.Generic.List<Tuple<string, string>> Headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Ocp-Apim-Subscription-Key", settings.OcpApimSubscriptionKey) // todo: get from settings
            };
            return await GetAsyncSystemNet(ub.Uri, Headers);
        }

        public override async System.Threading.Tasks.Task<IParseServiceResponse> ParseServiceAsync(byte[] audioBytes, int sampleRate)
        {
            IParseServiceResponse response = new IParseServiceResponse();
#if false // todo
            dynamic settings = Options.options.Services.APIs.Intent.HoundifyIntent;
            UriBuilder ub = new UriBuilder();
            ub.Scheme = "https";
            ub.Host = "api.houndify.com";
            ub.Path = "v1/audio";
            response.sr = await PostAsyncSystemNet(new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
#endif
            return response;
        }

        public override async System.Threading.Tasks.Task<ISpeechToTextServiceResponse> SpeechToTextAsync(byte[] audioBytes, int sampleRate)
        {
            ISpeechToTextServiceResponse response = new ISpeechToTextServiceResponse();
            Log.WriteLine("Bing: audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);

            stopWatch.Start();

            dynamic settings = Options.options.Services.APIs.SpeechToText.BingSpeechToText;

            string requestUri = "https://speech.platform.bing.com/recognize";

            AccessTokenInfo token;
            string headerValue;

            // Note: Sign up at http://www.projectoxford.ai to get a subscription key.
            // Use the subscription key, called Primary Key, as the Client secret below.
            // todo: looks like I've implemented an old method of calling, albeit it seems to work. Should I update to: https://onedrive.live.com/prev?id=9a8c02c3b59e575!115&cid=09A8C02C3B59E575&parId=root&authkey=!AOE9yiNn9bOskFE&v=TextFileEditor
            Authentication auth = new Authentication();
            string ClientID = settings.ClientID;
            string clientSecret = settings.clientSecret;
            await auth.PerformAuthenticationAsync(ClientID, clientSecret);

            /* URI Params. Refer to the README file for more information. */
            requestUri += @"?scenarios=smd";                                  // websearch is the other main option.
            requestUri += @"&appid=D4D52672-91D7-4C74-8AD8-42B1D98141A5";     // You must use this ID.
            requestUri += @"&locale=" + Options.options.locale.language;      // We support several other languages.  Refer to README file.
            requestUri += @"&device.os=wp7";
            requestUri += @"&version=3.0";
            requestUri += @"&format=json";
            requestUri += @"&instanceid=565D69FF-E928-4B7E-87DA-9A750B96D9E3";
            requestUri += @"&requestid=" + Guid.NewGuid().ToString();

            // if samplerate is wrong, gives 403 bad request error
            string contentType = @"audio/wav; codec=""audio/pcm""; samplerate=" + sampleRate.ToString();

            try
            {
                token = auth.GetAccessToken();
                if (token == null)
                {
                    Log.WriteLine("Invalid authentication token");
                    return response;
                }
                Log.WriteLine("Token: {0}\n" + token.access_token);

                /*
                 * Create a header with the access_token property of the returned token
                 */
                headerValue = "Bearer " + token.access_token;

                if (Options.options.debugLevel >= 4)
                    Log.WriteLine("Request Uri: " + requestUri);
#if WINDOWS_UWP
                if (Options.options.Services.APIs.PreferSystemNet)
                    response.sr = await PostAsyncSystemNet(new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
                else
                    response.sr = await PostAsyncWindowsWeb(new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
#else
                response.sr = await PostAsyncSystemNet(new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
#endif
            }
            catch (Exception ex)
            {
                Log.WriteLine("Bing.SpeechToText: Exception:" + ex.Message);
                if (ex.InnerException != null)
                    Log.WriteLine("InnerException:" + ex.InnerException);
            }
            stopWatch.Stop();
            response.sr.TotalElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
            return response;
        }

        public async System.Threading.Tasks.Task<IServiceResponse> GetAsyncSystemNet(Uri uri, System.Collections.Generic.List<Tuple<string, string>> Headers)
        {
            IServiceResponse response = new IServiceResponse();
            using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
            {
                try
                {
                    foreach (Tuple<string, string> h in Headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(h.Item1, h.Item2);
                    }
                    response.ResponseJson = await httpClient.GetStringAsync(uri);
                    if (response.ResponseJson == null) // todo: not right for Parse
                        Log.WriteLine("Fail! - Response is null");
                    else
                    {
                        if (response.ResponseJson[0] != '{')
                            response.ResponseJson = "{ 'root':" + response.ResponseJson + "}"; // Analyse API doesn't return outer braces. Why?
                        JObject ResponseBodyToken = JObject.Parse(response.ResponseJson);
                        if ((ResponseBodyToken.SelectToken("$.root")) == null)
                            Log.WriteLine("Fail! - Status is null");
                        else
                        {
                            foreach (JToken tokAnalyzerResult in ResponseBodyToken.SelectToken("$.root"))
                            {
                                Analyzers.Add(tokAnalyzerResult["id"].ToString());
                                AnalyzerStringized += "'" + tokAnalyzerResult["id"].ToString() + "', ";
                            }
                            AnalyzerStringized = AnalyzerStringized.Substring(0, AnalyzerStringized.Length - 2); // remove trailing "', "
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine("Bing.GetAnalyzers: Exception:" + ex.Message);
                    if (ex.InnerException != null)
                        Log.WriteLine("InnerException:" + ex.InnerException);
                }
            }
            return response;
        }

#if WINDOWS_UWP
        public async System.Threading.Tasks.Task<IServiceResponse> PostAsyncWindowsWeb(Uri uri, byte[] audioBytes, int sampleRate, string contentType, string headerValue)
        {
            IServiceResponse response = new IServiceResponse();
            try
            {
                // Using HttpClient to grab chunked encoding (partial) requests.
                using (Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient())
                {
                    /*byte[]*/
                    //RequestContentBytes = await Helpers.ReadBytesFromFile(audioFile);

                    // headers seem to be tricky, inconsistent in Windows.Web.Http namespace

                    httpClient.DefaultRequestHeaders.Accept.Add(new Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("application/json;text/xml"));
                    httpClient.DefaultRequestHeaders["Authorization"] = headerValue;
                    // alternate form - httpClient.DefaultRequestHeaders.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", token.access_token);
                    httpClient.DefaultRequestHeaders["Expect"] = "100-continue"; // in header of sample
                    // httpClient.DefaultRequestHeaders["Host"] = host; // doesn't seem to be needed
                    // httpClient.DefaultRequestHeaders["ProtocolVersion"] = HttpVersion.Version11; // doesn't seem to be needed

                    Windows.Web.Http.IHttpContent requestContent = null;
                    if (Options.options.Services.APIs.PreferChunkedEncodedRequests)
                    {
                        // using chunked transfer requests
                        Log.WriteLine("Using chunked encoding");
                        Windows.Storage.Streams.InMemoryRandomAccessStream contentStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                        // todo: obsolete to use DataWriter? use await Windows.Storage.FileIO.Write..(file);
                        Windows.Storage.Streams.DataWriter dw = new Windows.Storage.Streams.DataWriter(contentStream);
                        dw.WriteBytes(audioBytes);
                        await dw.StoreAsync();
                        // GetInputStreamAt(0) forces chunked transfer (sort of undocumented behavior).
                        requestContent = new Windows.Web.Http.HttpStreamContent(contentStream.GetInputStreamAt(0));
                    }
                    else
                    {
                        requestContent = new Windows.Web.Http.HttpBufferContent(audioBytes.AsBuffer());
                    }
                    requestContent.Headers.Add("Content-Type", contentType);

                    Log.WriteLine("Before wav Post: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                    using (Windows.Web.Http.HttpResponseMessage hrm = await httpClient.PostAsync(uri, requestContent))
                    {
                        response.StatusCode = (int)hrm.StatusCode;
                        Log.WriteLine("After wav Post: StatusCode:" + response.StatusCode + " Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                        if (hrm.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                        {
                            string responseContents = await hrm.Content.ReadAsStringAsync();
                            string[] responseJsons = responseContents.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            foreach (string rj in responseJsons)
                            {
                                response.ResponseJson = rj;
                                JToken ResponseBodyToken = JObject.Parse(response.ResponseJson);
                                response.ResponseJsonFormatted = JsonConvert.SerializeObject(ResponseBodyToken, new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                                if (Options.options.debugLevel >= 4)
                                    Log.WriteLine("ResponseBodyJsonFormatted:" + response.ResponseJsonFormatted);
                                JToken tokResult = ProcessResponseSpeech(ResponseBodyToken);
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
                        else
                        {
                            response.ResponseResult = hrm.ReasonPhrase;
                            Log.WriteLine("PostAsync Failed: StatusCode:" + hrm.ReasonPhrase + "(" + response.StatusCode.ToString() + ")");
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

        public async System.Threading.Tasks.Task<IServiceResponse> PostAsyncSystemNet(Uri uri, string text, string contentType, System.Collections.Generic.List<Tuple<string, string>> Headers)
        {
            IServiceResponse response = new IServiceResponse();
            try
            {
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);

                request.Method = "POST";

                // make headers same as project oxford sample headers
                request.ContentType = contentType;
                request.Accept = @"application/json;text/xml";
                foreach (Tuple<string, string> h in Headers)
                {
                    request.Headers[h.Item1] = h.Item2;
                }
                // can't seem to set - request.Expect = "100-continue"; // in header of sample
                //request.Host = host; // doesn't seem to be needed
                //request.ProtocolVersion = HttpVersion.Version11; // doesn't seem to be needed

                using (System.IO.Stream requestStream = await request.GetRequestStreamAsync())
                {
                    if (Options.options.Services.APIs.PreferChunkedEncodedRequests)
                    {
                        Log.WriteLine("Using chunked encoding");
                        request.Headers["Content-Length"] = "0";
                        request.Headers["Transfer-Encoding"] = "chunked";
                    }
                    byte[] btext = System.Text.Encoding.UTF8.GetBytes(text); // Is there a string overload for Write() so we don't have to do this?
                    requestStream.Write(btext, 0, btext.Length);
                }

                Log.WriteLine("Before Post: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                using (System.Net.WebResponse wr = await request.GetResponseAsync())
                {
                    using (System.Net.HttpWebResponse hwr = (System.Net.HttpWebResponse)wr)
                    {
                        response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds - response.RequestElapsedMilliseconds;
                        response.StatusCode = (int)hwr.StatusCode;
                        Log.WriteLine("After Post: StatusCode:" + response.StatusCode + " Total milliseconds:" + stopWatch.ElapsedMilliseconds + " Request milliseconds:" + response.RequestElapsedMilliseconds);
                        if (hwr.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            using (System.IO.StreamReader sr = new System.IO.StreamReader(wr.GetResponseStream()))
                            {
                                response.ResponseJson = await sr.ReadToEndAsync();
                            }
                            if (response.ResponseJson[0] != '{')
                                response.ResponseJson = "{ 'root':" + response.ResponseJson + "}"; // Analyse API doesn't return outer braces. Why?
                            JObject ResponseBodyToken = JObject.Parse(response.ResponseJson);
                            string ResponseJsonFormatted = JsonConvert.SerializeObject(ResponseBodyToken, new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                            if (Options.options.debugLevel >= 4)
                                Log.WriteLine("ResponseJsonFormatted:" + ResponseJsonFormatted);
                            JToken tokResult = ProcessResponseParse(ResponseBodyToken);
                            // todo: temp, restore JToken tokResult = ProcessResponseSpeech(ResponseBodyToken);
                            if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
                            {
                                response.ResponseResult = Options.options.Services.APIs.SpeechToText.missingResponse;
                                if (Options.options.debugLevel >= 3)
                                    Log.WriteLine("SpeechToTextResult:" + response.ResponseResult);
                            }
                            else
                            {
                                response.ResponseResult = tokResult.ToString();
                                if (Options.options.debugLevel >= 3)
                                    Log.WriteLine("ResponeBodyResult:" + tokResult.Path + ": " + response.ResponseResult);
                            }
                        }
                        else
                        {
                            response.ResponseResult = hwr.StatusDescription;
                            Log.WriteLine("PostAsync Failed: StatusCode:" + hwr.StatusDescription + "(" + response.StatusCode.ToString() + ")");
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

        public async System.Threading.Tasks.Task<IServiceResponse> PostAsyncSystemNet(Uri uri, byte[] audioBytes, int sampleRate, string contentType, string headerValue)
        {
            IServiceResponse response = new IServiceResponse();
            try
            {
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);

                request.Method = "POST";

                // make headers same as project oxford sample headers
                request.ContentType = contentType;
                request.Accept = @"application/json;text/xml";
                request.Headers["Authorization"] = headerValue;
                // can't seem to set - request.Expect = "100-continue"; // in header of sample
                //request.Host = host; // doesn't seem to be needed
                //request.ProtocolVersion = HttpVersion.Version11; // doesn't seem to be needed

                using (System.IO.Stream requestStream = await request.GetRequestStreamAsync())
                {
                    if (Options.options.Services.APIs.PreferChunkedEncodedRequests)
                    {
                        Log.WriteLine("Using chunked encoding");
                        request.Headers["Content-Length"] = "0";
                        request.Headers["Transfer-Encoding"] = "chunked";
                    }
                    requestStream.Write(audioBytes, 0, audioBytes.Length);
                }

                Log.WriteLine("Before Post: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                using (System.Net.WebResponse wr = await request.GetResponseAsync())
                {
                    using (System.Net.HttpWebResponse hwr = (System.Net.HttpWebResponse)wr)
                    {
                        response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds - response.RequestElapsedMilliseconds;
                        response.StatusCode = (int)hwr.StatusCode;
                        Log.WriteLine("After Post: StatusCode:" + response.StatusCode + " Total milliseconds:" + stopWatch.ElapsedMilliseconds + " Request milliseconds:" + response.RequestElapsedMilliseconds);
                        if (hwr.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.ResponseJson = null;
                            using (System.IO.StreamReader sr = new System.IO.StreamReader(wr.GetResponseStream()))
                            {
                                response.ResponseJson = await sr.ReadToEndAsync();
                            }
                            JToken ResponseBodyToken = JObject.Parse(response.ResponseJson);
                            string ResponseJsonFormatted = JsonConvert.SerializeObject(ResponseBodyToken, new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                            if (Options.options.debugLevel >= 4)
                                Log.WriteLine("ResponseJsonFormatted:" + ResponseJsonFormatted);
                            JToken tokResult = ProcessResponseSpeech(ResponseBodyToken);
                            if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
                            {
                                response.ResponseResult = Options.options.Services.APIs.SpeechToText.missingResponse;
                                if (Options.options.debugLevel >= 3)
                                    Log.WriteLine("SpeechToTextResult:" + response.ResponseResult);
                            }
                            else
                            {
                                response.ResponseResult = tokResult.ToString();
                                if (Options.options.debugLevel >= 3)
                                    Log.WriteLine("ResponeBodyResult:" + tokResult.Path + ": " + response.ResponseResult);
                            }
                        }
                        else
                        {
                            response.ResponseResult = hwr.StatusDescription;
                            Log.WriteLine("PostAsync Failed: StatusCode:" + hwr.StatusDescription + "(" + response.StatusCode.ToString() + ")");
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

        private static JToken ProcessResponseParse(JToken response)
        {
            JToken tok = null;
            if (response == null) // todo: not right for Parse
                Log.WriteLine("Fail! - Response is null");
            else
            {
                if ((response.SelectToken("$.root")) == null)
                    Log.WriteLine("Fail! - Status is null");
                else
                {
                    foreach (JToken tokAnalyzerResult in response.SelectToken("$.root"))
                    {
                        if (tokAnalyzerResult["analyzerId"].ToString() == "22a6b758-420f-4745-8a3c-46835a67c0d2") // Tree result
                        {
                            foreach (JToken tokResult in tokAnalyzerResult.SelectToken("result"))
                            {
                                tok = tokResult;
                            }
                            break;
                        }
                    }
                }
            }
            return tok;
        }

        private static JToken ProcessResponseSpeech(JToken response)
        {
            JToken tok = null;
            if (response == null)
                Log.WriteLine("Fail! - Response is null");
            else
            {
                if ((response.SelectToken("$.header.status")) == null)
                    Log.WriteLine("Fail! - Status is null");
                else
                    Log.WriteLine("Status is " + response.SelectToken("header.status").ToString());
                JToken tokFormat;
                if ((tokFormat = response.SelectToken("$.header.name")) != null)
                {
#if false // todo: implement alternatives
                    foreach (Tuple<string, string> t in options.ResultText)
                    {
                        if (tokFormat.ToString() == t.Item1)
                            if ((tok = response.SelectToken(t.Item2)) != null) // && (tok = tok.Parent) != null)
                                break;
                    }
#else
                    tok = tokFormat;
#endif
                }
            }
            return tok;
        }


        public class AccessTokenInfo
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string expires_in { get; set; }
            public string scope { get; set; }
        }

        /*
         * This class demonstrates how to get a valid O-auth token.
         */
        public class Authentication
        {
            public static readonly string AccessUri = "https://oxford-speech.cloudapp.net/token/issueToken";
            public static int StatusCode;
            private string ClientID;
            private string clientSecret;
            private string request;
            private AccessTokenInfo token;
            private System.Threading.Timer accessTokenRenewer;

            //Access token expires every 10 minutes. Renew it every 9 minutes only.
            private const int RefreshTokenDuration = 9;

            public async System.Threading.Tasks.Task PerformAuthenticationAsync(string ClientID, string clientSecret)
            {
                this.ClientID = ClientID;
                this.clientSecret = clientSecret;
                /*
                  * If ClientID or client secret has special characters, encode before sending request
                  */
                this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope={2}",
                                              System.Net.WebUtility.UrlEncode(ClientID),
                                              System.Net.WebUtility.UrlEncode(clientSecret),
                                              System.Net.WebUtility.UrlEncode("https://speech.platform.bing.com"));

                this.token = await HttpPostAsync(AccessUri, this.request);

                // renew the token every specfied minutes
                accessTokenRenewer = new System.Threading.Timer(new System.Threading.TimerCallback(OnTokenExpiredCallbackAsync),
                                               this,
                                               TimeSpan.FromMinutes(RefreshTokenDuration),
                                               TimeSpan.FromMilliseconds(-1));
            }

            public AccessTokenInfo GetAccessToken()
            {
                return this.token;
            }

            private async System.Threading.Tasks.Task RenewAccessTokenAsync()
            {
                AccessTokenInfo newAccessToken = await HttpPostAsync(AccessUri, this.request);
                //swap the new token with old one
                //Note: the swap is thread unsafe
                this.token = newAccessToken;
                Log.WriteLine(string.Format("Renewed token for user: {0} is: {1}",
                                  this.ClientID,
                                  this.token.access_token));
            }

            private async void OnTokenExpiredCallbackAsync(object stateInfo)
            {
                try
                {
                    await RenewAccessTokenAsync();
                }
                catch (Exception ex)
                {
                    Log.WriteLine("Failed renewing access token. Details:" + ex.Message);
                }
                finally
                {
                    try
                    {
                        accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine("Failed to reschedule the timer to renew access token. Details:" + ex.Message);
                    }
                }
            }

            private async System.Threading.Tasks.Task<AccessTokenInfo> HttpPostAsync(string accessUri, string requestDetails)
            {
#if WINDOWS_UWP
                if (Options.options.Services.APIs.PreferSystemNet)
                    return await HttpPostAsyncSystemNet(accessUri, requestDetails);
                else
                    return await HttpPostAsyncWindowsWeb(accessUri, requestDetails);
#else
                return await HttpPostAsyncSystemNet(accessUri, requestDetails);
#endif
            }

#if WINDOWS_UWP
            private async System.Threading.Tasks.Task<AccessTokenInfo> HttpPostAsyncWindowsWeb(string accessUri, string requestDetails)
            {
                StatusCode = 0;
                try
                {
                    using (Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient())
                    {
                        // don't bother with chunked encoding for such a small message
                        Windows.Web.Http.HttpStringContent requestContent = new Windows.Web.Http.HttpStringContent(requestDetails);
                        requestContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/x-www-form-urlencoded"); // must add header AFTER contents are initialized
                        Log.WriteLine("Before refresh token Post: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                        Windows.Web.Http.HttpResponseMessage response = await httpClient.PostAsync(new Uri(accessUri), requestContent);
                        {
                            StatusCode = (int)response.StatusCode;
                            Log.WriteLine("After refresh token Post: StatusCode:" + StatusCode + " Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                            if (response.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                            {
                                AccessTokenInfo token = JsonConvert.DeserializeObject<AccessTokenInfo>(await response.Content.ReadAsStringAsync());
                                return token;
                            }
                            else
                            {
                                Log.WriteLine("PostAsync Failed: StatusCode:" + response.ReasonPhrase + "(" + response.StatusCode.ToString() + ")");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine("PostAsync failed:" + ex.Message);
                }

                return null;
            }
#endif
            private async System.Threading.Tasks.Task<AccessTokenInfo> HttpPostAsyncSystemNet(string accessUri, string requestDetails)
            {
                StatusCode = 0;
                //Prepare OAuth request 
                System.Net.WebRequest webRequest = System.Net.WebRequest.Create(accessUri);
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.Method = "POST";
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(requestDetails);
                //webRequest.ContentLength = bytes.Length;
                using (System.IO.Stream outputStream = await webRequest.GetRequestStreamAsync())
                {
                    if (Options.options.Services.APIs.PreferChunkedEncodedRequests)
                    {
                        Log.WriteLine("Using chunked encoding");
                        webRequest.Headers["Content-Length"] = "0";
                        webRequest.Headers["Transfer-Encoding"] = "chunked";
                    }
                    outputStream.Write(bytes, 0, bytes.Length);
                }
                using (System.Net.HttpWebResponse webResponse = (System.Net.HttpWebResponse)await webRequest.GetResponseAsync())
                {
                    StatusCode = (int)webResponse.StatusCode;
                    Log.WriteLine("After refresh token Post: StatusCode:" + StatusCode + " Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                    if (webResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(webResponse.GetResponseStream()))
                        {
                            string responseJson = await sr.ReadToEndAsync();
                            AccessTokenInfo token = JsonConvert.DeserializeObject<AccessTokenInfo>(responseJson);
                            return token;
                        }
                    }
                    else
                    {
                        Log.WriteLine("PostAsync Failed: StatusCode:" + webResponse.StatusDescription + "(" + webResponse.StatusCode + ")");
                    }
                }
                return null;
            }
        }
    }
}
