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

        public BingServices(Settings.Service service) : base(service)
        {
        }

        public override async System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(string text)
        {
            if (Analyzers == null)
            {
                GetAnalyzers().Wait();
                if (Analyzers == null || Analyzers.Count == 0)
                    throw new InvalidOperationException(); // can't continue without at least one Analyzer
            }
            ParseServiceResponse response = new ParseServiceResponse();
            Log.WriteLine("Bing: parse:" + text);
            stopWatch.Start();
            UriBuilder ub = new UriBuilder();
            ub.Scheme = service.request.uri.scheme;
            ub.Host = service.request.uri.host;
            ub.Path = service.request.uri.path;
            text = service.request.data.value.Replace("{AnalyzerStringized}", AnalyzerStringized).Replace("{text}", text);
            System.Collections.Generic.List<Tuple<string, string>> Headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", service.request.headers[1].ContentType), // todo: need dictionary lookup instead of hardcoding
                new Tuple<string, string>("Ocp-Apim-Subscription-Key", service.request.headers[2].OcpApimSubscriptionKey) // todo: need dictionary lookup instead of hardcoding
            };
            // todo: maybe a dictionary of headers should be passed including content-type. Then PostAsync can do if (dict.Contains("Content-Type")) headers.add(dict...)
            // todo: maybe should init a header class, add values and pass to Post?
            response.sr = await PostAsyncSystemNet(ub.Uri, text, Headers);
            stopWatch.Stop();
            response.sr.TotalElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
            return response;
        }

        private async System.Threading.Tasks.Task<ServiceResponse> GetAnalyzers()
        {
            Log.WriteLine("Bing: GetAnalyzers");
            Analyzers = new System.Collections.Generic.List<string>();
            UriBuilder ub = new UriBuilder();
            ub.Scheme = service.request.uri.scheme; // same as Parse
            ub.Host = service.request.uri.host; // same as Parse
            ub.Path = "linguistics/v1.0/analyzers"; // can't use service.request.uri.path because it's for Parse
            System.Collections.Generic.List<Tuple<string, string>> Headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Ocp-Apim-Subscription-Key", service.request.headers[2].OcpApimSubscriptionKey) // todo: get from settings
            };
            return await GetAsyncSystemNet(ub.Uri, Headers);
        }

        public override async System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(byte[] audioBytes, int sampleRate)
        {
            ParseServiceResponse response = new ParseServiceResponse();
#if false // todo
#endif
            return response;
        }

        public override async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();
            Log.WriteLine("Bing: audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);

            stopWatch.Start();


            /* URI Params. Refer to the README file for more information. */
            string query = null;
            query += @"scenarios=smd";                                  // websearch is the other main option.
            query += @"&appid=D4D52672-91D7-4C74-8AD8-42B1D98141A5";     // You must use this ID.
            query += @"&locale=" + Options.options.locale.language;      // We support several other languages.  Refer to README file.
            query += @"&device.os=wp7";
            query += @"&version=3.0";
            query += @"&format=json";
            query += @"&instanceid=565D69FF-E928-4B7E-87DA-9A750B96D9E3";
            query += @"&requestid=" + Guid.NewGuid().ToString();

            UriBuilder ub = new UriBuilder();
            ub.Scheme = service.request.uri.scheme;
            ub.Host = service.request.uri.host;
            ub.Path = service.request.uri.path;
            ub.Query = query;
            // Note: Sign up at http://www.projectoxford.ai to get a subscription key.
            // Use the subscription key, called Primary Key, as the Client secret below.
            // todo: looks like I've implemented an old method of calling, albeit it seems to work. Should I update to: https://onedrive.live.com/prev?id=9a8c02c3b59e575!115&cid=09A8C02C3B59E575&parId=root&authkey=!AOE9yiNn9bOskFE&v=TextFileEditor
            Authentication auth = new Authentication();
            string clientID = service.request.headers[1].BearerAuthentication.clientID;
            string clientSecret = service.request.headers[1].BearerAuthentication.clientSecret;
            await auth.PerformAuthenticationAsync(clientID, clientSecret);

            try
            {
                AccessTokenInfo token = auth.GetAccessToken();
                if (token == null)
                {
                    Log.WriteLine("Invalid authentication token");
                    return response;
                }
                Log.WriteLine("Token: {0}\n" + token.access_token);

                System.Collections.Generic.List<Tuple<string, string>> Headers = new System.Collections.Generic.List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("Accept", service.request.headers[0].Accept),
                    new Tuple<string, string>("Authorization", "Bearer " + token.access_token),
                    new Tuple<string, string>("Content-Type", service.request.headers[2].ContentType.Replace("{sampleRate}",sampleRate.ToString())), // 403 if wrong
                };

                if (Options.options.debugLevel >= 4)
                    Log.WriteLine("Request Uri: " + ub.Uri);
#if WINDOWS_UWP
                if (Options.options.Services.APIs.PreferSystemNet)
                    response.sr = await PostAsyncSystemNet(new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
                else
                    response.sr = await PostAsyncWindowsWeb(new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
#else
                response.sr = await PostAsyncSystemNet(ub.Uri, audioBytes, sampleRate, Headers);
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

        public async System.Threading.Tasks.Task<ServiceResponse> GetAsyncSystemNet(Uri uri, System.Collections.Generic.List<Tuple<string, string>> Headers)
        {
            ServiceResponse response = new ServiceResponse(this.ToString());
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
                        if (response.ResponseJson[0] != '[')
                            throw new FormatException("Expecting array:" + response.ResponseJson);
                        JArray ResponseBodyToken = JArray.Parse(response.ResponseJson);
                        foreach (JToken tokAnalyzerResult in ResponseBodyToken)
                        {
                            Analyzers.Add(tokAnalyzerResult["id"].ToString());
                            AnalyzerStringized += "'" + tokAnalyzerResult["id"].ToString() + "', ";
                        }
                        AnalyzerStringized = AnalyzerStringized.Substring(0, AnalyzerStringized.Length - 2); // remove trailing "', "
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
                    if (Options.options.APIs.preferChunkedEncodedRequests)
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

        public async System.Threading.Tasks.Task<ServiceResponse> PostAsyncSystemNet(Uri uri, string text, System.Collections.Generic.List<Tuple<string, string>> Headers)
        {
            ServiceResponse response = new ServiceResponse(this.ToString());
            try
            {
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);

                request.Method = "POST";

                foreach (Tuple<string, string> h in Headers)
                {
                    switch (h.Item1)
                    {
                        case "Accept":
                            request.Accept = h.Item2;
                            break;
                        case "Content-Type":
                            request.ContentType = h.Item2;
                            break;
                        default:
                            request.Headers[h.Item1] = h.Item2;
                        break;
                    }
                }

                // can't seem to set - request.Expect = "100-continue"; // in header of sample
                //request.Host = host; // doesn't seem to be needed
                //request.ProtocolVersion = HttpVersion.Version11; // doesn't seem to be needed

                using (System.IO.Stream requestStream = await request.GetRequestStreamAsync())
                {
                    if (Options.options.APIs.preferChunkedEncodedRequests)
                    {
                        Log.WriteLine("Using chunked encoding");
                        request.Headers["Content-Length"] = "0";
                        request.Headers["Transfer-Encoding"] = "chunked";
                    }
                    byte[] btext = System.Text.Encoding.UTF8.GetBytes(text); // Is there a string overload for Write() so we don't have to do this?
                    requestStream.Write(btext, 0, btext.Length);
                }

#if false // todo: unfinished. Need to implement ContentType, Accept, Method, Headers
                Log.WriteLine("curl -X POST --data \"" + text + "\" --header ""...""" + "\"" + uri);
#endif

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
                            if (response.ResponseJson[0] != '[')
                                throw new FormatException("Expecting array:" + response.ResponseJson);
                            JArray ResponseBodyToken = JArray.Parse(response.ResponseJson);
                            string ResponseJsonFormatted = JsonConvert.SerializeObject(ResponseBodyToken, new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                            if (Options.options.debugLevel >= 4)
                                Log.WriteLine("ResponseJsonFormatted:" + ResponseJsonFormatted);
                            JToken tokResult = ProcessResponseParse(ResponseBodyToken);
                            // todo: temp, restore JToken tokResult = ProcessResponseSpeech(ResponseBodyToken);
                            if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
                            {
                                response.ResponseResult = Options.services["BingSpeechToTextService"].response.missingResponse;
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

        public async System.Threading.Tasks.Task<ServiceResponse> PostAsyncSystemNet(Uri uri, byte[] audioBytes, int sampleRate, System.Collections.Generic.List<Tuple<string, string>> Headers)
        {
            ServiceResponse response = new ServiceResponse(this.ToString());
            try
            {
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);

                request.Method = "POST";

                foreach (Tuple<string, string> h in Headers)
                {
                    switch (h.Item1)
                    {
                        case "Accept":
                            request.Accept = h.Item2;
                            break;
                        case "Content-Type":
                            request.ContentType = h.Item2;
                            break;
                        default:
                            request.Headers[h.Item1] = h.Item2;
                        break;
                    }
                }

                // can't seem to set - request.Expect = "100-continue"; // in header of sample
                //request.Host = host; // doesn't seem to be needed
                //request.ProtocolVersion = HttpVersion.Version11; // doesn't seem to be needed
                using (System.IO.Stream requestStream = await request.GetRequestStreamAsync())
                {
                    if (Options.options.APIs.preferChunkedEncodedRequests)
                    {
                        Log.WriteLine("Using chunked encoding");
                        request.Headers["Content-Length"] = "0";
                        request.Headers["Transfer-Encoding"] = "chunked";
                    }
                    requestStream.Write(audioBytes, 0, audioBytes.Length);
                }

#if false // todo: unfinished. Need to implement ContentType, Accept, Method, Headers
                Log.WriteLine("curl -X POST --data \"" + text + "\" --header ""...""" + "\"" + uri);
#endif

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
                                response.ResponseResult = Options.services["BingSpeechToTextService"].response.missingResponse;
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
                foreach (JToken tokAnalyzerResult in response)
                {
                    if (tokAnalyzerResult["analyzerId"].ToString() == "22a6b758-420f-4745-8a3c-46835a67c0d2") // Tree result
                    {
#if true
                        tok = tokAnalyzerResult.SelectToken("result");
#else
                            foreach (JToken tokResult in tokAnalyzerResult.SelectToken("result"))
                            {
                                tok = tokResult;
                            }
#endif
                        break;
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
                    if (Options.options.APIs.preferChunkedEncodedRequests)
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
