using System;
using System.Net;

using System.Runtime.InteropServices.WindowsRuntime; // AsBuffer()

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WoundifyShared
{
    class BingServices : ISpeechToTextService
    {
        private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        public override string ResponseResult { get; set; }
        public override string ResponseJson { get; set; }
        public override string ResponseJsonFormatted { get; set; }
        public override long TotalElapsedMilliseconds { get; set; }
        public override long RequestElapsedMilliseconds { get; set; }
        public override int StatusCode { get; set; }

        public override async System.Threading.Tasks.Task SpeechToTextAsync(byte[] audioBytes, int sampleRate)
        {
            Log.WriteLine("audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);

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
                    return;
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
                    await PostAsyncSystemNet(new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
                else
                    await PostAsyncWindowsWeb(new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
#else
                await PostAsyncSystemNet(new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
#endif
            }
            catch (Exception ex)
            {
                Log.WriteLine("Bing.SpeechToText: Exception:" + ex.Message);
                if (ex.InnerException != null)
                    Log.WriteLine("InnerException:" + ex.InnerException);
            }
            stopWatch.Stop();
            TotalElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
        }

#if WINDOWS_UWP
        public async System.Threading.Tasks.Task PostAsyncWindowsWeb(Uri uri, byte[] audioBytes, int sampleRate, string contentType, string headerValue)
        {
            ResponseResult = null;
            ResponseJson = null;
            ResponseJsonFormatted = null;
            StatusCode = 0;
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
                    using (Windows.Web.Http.HttpResponseMessage response = await httpClient.PostAsync(uri, requestContent))
                    {
                        StatusCode = (int)response.StatusCode;
                        Log.WriteLine("After wav Post: StatusCode:" + StatusCode + " Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                        if (response.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                        {
                            string responseContents = await response.Content.ReadAsStringAsync();
                            string[] responseJsons = responseContents.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            foreach (string rj in responseJsons)
                            {
                                ResponseJson = rj;
                                JToken ResponseBodyToken = JObject.Parse(ResponseJson);
                                ResponseJsonFormatted = JsonConvert.SerializeObject(ResponseBodyToken, new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                                if (Options.options.debugLevel >= 4)
                                    Log.WriteLine("ResponseBodyJsonFormatted:" + ResponseJsonFormatted);
                                JToken tokResult = ProcessResponse(ResponseBodyToken);
                                if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
                                {
                                    ResponseResult = Options.options.Services.APIs.SpeechToText.missingResponse;
                                    if (Options.options.debugLevel >= 3)
                                        Log.WriteLine("ResponseResult:" + ResponseResult);
                                }
                                else
                                {
                                    ResponseResult = tokResult.ToString();
                                    if (Options.options.debugLevel >= 3)
                                        Log.WriteLine("ResponseResult:" + tokResult.Path + ": " + ResponseResult);
                                }
                            }
                        }
                        else
                        {
                            ResponseResult = response.ReasonPhrase;
                            Log.WriteLine("PostAsync Failed: StatusCode:" + response.ReasonPhrase + "(" + response.StatusCode.ToString() + ")");
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
        }
#endif

        public async System.Threading.Tasks.Task PostAsyncSystemNet(Uri uri, byte[] audioBytes, int sampleRate, string contentType, string headerValue)
        {
            ResponseResult = null;
            ResponseJson = null;
            ResponseJsonFormatted = null;
            StatusCode = 0;
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
                RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                using (System.Net.WebResponse wr = await request.GetResponseAsync())
                {
                    using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)wr)
                    {
                        RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds - RequestElapsedMilliseconds;
                        StatusCode = (int)response.StatusCode;
                        Log.WriteLine("After Post: StatusCode:" + StatusCode + " Total milliseconds:" + stopWatch.ElapsedMilliseconds + " Request milliseconds:" + RequestElapsedMilliseconds);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            ResponseJson = null;
                            using (System.IO.StreamReader sr = new System.IO.StreamReader(wr.GetResponseStream()))
                            {
                                ResponseJson = await sr.ReadToEndAsync();
                            }
                            JToken ResponseBodyToken = JObject.Parse(ResponseJson);
                            string ResponseJsonFormatted = JsonConvert.SerializeObject(ResponseBodyToken, new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                            if (Options.options.debugLevel >= 4)
                                Log.WriteLine("ResponseJsonFormatted:" + ResponseJsonFormatted);
                            JToken tokResult = ProcessResponse(ResponseBodyToken);
                            if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
                            {
                                ResponseResult = Options.options.Services.APIs.SpeechToText.missingResponse;
                                if (Options.options.debugLevel >= 3)
                                    Log.WriteLine("SpeechToTextResult:" + ResponseResult);
                            }
                            else
                            {
                                ResponseResult = tokResult.ToString();
                                if (Options.options.debugLevel >= 3)
                                    Log.WriteLine("ResponeBodyResult:" + tokResult.Path + ": " + ResponseResult);
                            }
                        }
                        else
                        {
                            ResponseResult = response.StatusDescription;
                            Log.WriteLine("PostAsync Failed: StatusCode:" + response.StatusDescription + "(" + response.StatusCode.ToString() + ")");
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
        }

        private static JToken ProcessResponse(JToken response)
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
