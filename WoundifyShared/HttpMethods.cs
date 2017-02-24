using System;
using System.Collections.Generic;
using System.Text;

namespace WoundifyShared
{
    public class HttpMethods
    {
        private static Authentication ImageAuth = null;
        private static ulong httpCallCount = 0;
        static HttpMethods()
        {
            foreach (string f in System.IO.Directory.EnumerateFiles(".", "curl-*.*"))
            {
                System.IO.File.Delete(f);
            }
            ImageAuth = new Authentication();
        }

#if WINDOWS_UWP // major bit rot
        public virtual async System.Threading.Tasks.Task<IServiceResponse> PostAsyncWindowsWeb(Uri uri, byte[] audioBytes, apiArgs)
        {
        IServiceResponse response = new IServiceResponse(service);
            try
            {
                // Using HttpClient to grab chunked encoding (partial) responses.
                using (Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient())
                {
                    Log.WriteLine("before requestContent");
                    Log.WriteLine("after requestContent");
                    // rate must be specified but doesn't seem to need to be accurate.

                    Windows.Web.Http.IHttpContent requestContent = null;
                    if (Options.options.APIs.preferChunkedEncodedRequests)
                    {
                        // using chunked transfer requests
                        Log.WriteLine("Using chunked encoding");
                        Windows.Storage.Streams.InMemoryRandomAccessStream contentStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                        // TODO: obsolete to use DataWriter? use await Windows.Storage.FileIO.Write..(file);
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
                    requestContent.Headers.Add("Content-Type", "audio/l16; rate=" + sampleRate.ToString()); // must add header AFTER contents are initialized

                    Log.WriteLine("Before Post: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                    response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                    using (Windows.Web.Http.HttpResponseMessage hrm = await httpClient.PostAsync(uri, requestContent))
                    {
                        response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds - response.RequestElapsedMilliseconds;
                        response.StatusCode = (int)hrm.StatusCode;
                        Log.WriteLine("After Post: StatusCode:" + response.StatusCode + " Total milliseconds:" + stopWatch.ElapsedMilliseconds + " Request milliseconds:" + response.RequestElapsedMilliseconds);
                        if (hrm.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
                        {
                            string responseContents = await hrm.Content.ReadAsStringAsync();
                            string[] responseJsons = responseContents.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            foreach (string rj in responseJsons)
                            {
                                response.ResponseJson = rj;
                                Newtonsoft.Json.Linq.JToken ResponseBodyToken = Newtonsoft.Json.Linq.JObject.Parse(response.ResponseJson);
                                response.ResponseJsonFormatted = Newtonsoft.Json.JsonConvert.SerializeObject(ResponseBodyToken, new Newtonsoft.Json.JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                                if (Options.options.debugLevel >= 4)
                                    Log.WriteLine(response.ResponseJsonFormatted);
                                Newtonsoft.Json.Linq.JToken tokResult = ProcessResponse(ResponseBodyToken);
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
        public static Uri MakeUri(Settings.Request request, Dictionary<string, string> apiArgs, List<Tuple<string, string>> uriSubstitutes = null, string text = null)
        {
            UriBuilder ub = new UriBuilder();
            string scheme = request.uri.scheme;
            string host = request.uri.host;
            string path = request.uri.path;
            string query = request.uri.query;

            if (apiArgs != null)
            {
                foreach (KeyValuePair<string, string> r in apiArgs)
                {
                    // so far only query needs substitutes
                    string k = "{" + r.Key + "}";
                    scheme = scheme.Replace(k, r.Value);
                    host = host.Replace(k, r.Value);
                    path = path.Replace(k, r.Value);
                    if (query == null)
                        query = string.Empty;
                    else
                        query = query.Replace(k, r.Value);
                }
            }

            if (uriSubstitutes == null)
            {
                uriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("{guid}", Guid.NewGuid().ToString()), // Microsoft SpeechToText
                    new Tuple<string, string>("{locale}", Options.options.locale.language) // Microsoft SpeechToText
                };
                if (text != null)
                    uriSubstitutes.Add(new Tuple<string, string>("{text}", text));
            }

            if (uriSubstitutes != null)
            {
                foreach (Tuple<string, string> r in uriSubstitutes)
                {
                    // so far only query needs substitutes
                    scheme = scheme.Replace(r.Item1, r.Item2);
                    host = host.Replace(r.Item1, r.Item2);
                    path = path.Replace(r.Item1, r.Item2);
                    if (query == null)
                        query = string.Empty;
                    else
                        query = query.Replace(r.Item1, r.Item2);
                }
            }
            ub.Scheme = scheme;
            ub.Host = host;
            ub.Path = path;
            ub.Query = query;
            ub.Query = query;
            return ub.Uri;
        }

        private static List<string> MakePostDataSubstitutes(Settings.Request request, string text, byte[] bytes, System.Collections.Generic.Dictionary<string, string> apiArgs, List<Tuple<string, string>> postDataSubstitutes)
        {
            if (request == null || request.data == null || request.data.type == null) // some POSTs have no data
                return new List<string> { text };

            if (text != null && bytes != null)
                throw new Exception("MakePostDataSubstitutes: Both text and bytes are not null");

            string data = text;
            if (request.data.value != null)
                data = request.data.value;

            if (postDataSubstitutes != null)
            {
                foreach (Tuple<string, string> r in postDataSubstitutes)
                {
                    data = data.Replace(r.Item1, r.Item2);
                }
            }

            string fileName = null;
            if (apiArgs.ContainsKey("fileName"))
                fileName = apiArgs["fileName"];

            switch (request.data.type)
            {
                case "ascii":
                    return new List<string> { data };
                case "base64":
                    // TODO: use Helpers.stringToDictionary()
                    if (data.Contains("=")) // TODO: use text or data?
                    {
                        List<string> base64content = new List<string>();
                        foreach (string multicontent in data.Split('&'))
                        {
                            string[] namecontent = multicontent.Split('=');
                            if (namecontent.Length != 2)
                                throw new FormatException();
                            base64content.Add(namecontent[0] + "=" + namecontent[1].Replace("{text}", Convert.ToBase64String(bytes)));
                        }
                        return base64content;
                    }
                    else
                    {
                        return new List<string> { data.Replace("{text}", Convert.ToBase64String(bytes)) };
                    }
                case "binary":
                    return null;
                case "json":
                    // TODO: pass list of substitutes instead of hard coding?
                    // TODO: issue with replacing "'" with "\\'". Seems to work when json properties are delimited with single-quotes but not with double-quotes. 
                    if (data.Contains("{base64}") ^ bytes != null)
                        throw new Exception("json: bytes ^ {base64} mismatch"); // can occur if string/url argument is passed to binary data API
                    if ((data.Contains("{text}") || data.Contains("{url}")) ^ text != null) // todo: remove when url is implemented
                        throw new Exception("json: text ^ {text} mismatch");
                    // pass Uri here so request use Url instead of text
                    //if (data.Contains("{url}") ^ text != null)
                    //throw new Exception("json: text ^ {url} mismatch");
                    if (bytes != null)
                        return new List<string> { data.Replace("{guid}", Guid.NewGuid().ToString()).Replace("{base64}", Convert.ToBase64String(bytes)) };
                    else if (text != null)
                        return new List<string> { data.Replace("{guid}", Guid.NewGuid().ToString()).Replace("{url}", text).Replace("{text}", text.Replace("\"", "\\\"")) }; // bing sentiment uses guid. Replace '"' with '\"'
                    else
                        throw new Exception("json: expecting bytes or text");
                case "multipart":
                    // TODO: use Helpers.stringToDictionary()
                    if (!data.Contains("=")) // && isMultipart(headers)?
                        throw new Exception("multipart is missing =");
                    List<string> multipartcontent = new List<string>();
                    foreach (string multicontent in data.Split('&'))
                    {
                        string[] namecontent = multicontent.Split('=');
                        if (namecontent.Length != 2)
                            throw new FormatException();
                        if (fileName != null)
                            multipartcontent.Add(namecontent[0] + "=" + namecontent[1].Replace("{text}", fileName)); // hpe haven ocr
                        else if (bytes != null)
                            multipartcontent.Add(namecontent[0] + "=" + namecontent[1].Replace("{text}", Convert.ToBase64String(bytes))); // bing spell, Microsoft Detect
                        else if (text != null)
                            multipartcontent.Add(namecontent[0] + "=" + namecontent[1].Replace("{text}", text)); // bing spell, Microsoft Detect
                    }
                    return multipartcontent;
                case "raw":
                    return new List<string> { data };
                case "string":
                    return new List<string> { data };
                case "urlencode":
                    // TODO: use Helpers.stringToDictionary()
                    if (text == null)
                        throw new Exception("urlencode: expecting text");
                    if (data.Contains("=")) // && isMultipart(headers)?
                    {
                        List<string> urlencodecontent = new List<string>();
                        foreach (string multicontent in data.Split('&'))
                        {
                            string[] namecontent = multicontent.Split('=');
                            if (namecontent.Length != 2)
                                throw new FormatException();
                            urlencodecontent.Add(namecontent[0] + "=" + (namecontent[1][0] == '@' ? namecontent[1] : System.Web.HttpUtility.UrlEncode(namecontent[1].Replace("{text}", text))));
                        }
                        return urlencodecontent;
                    }
                    else
                    {
                        return new List<string> { System.Web.HttpUtility.UrlEncode(data) };
                    }
                case "xml":
                    // TODO: pass list of substitutes instead of hard coding?
                    if (bytes == null)
                        return new List<string> { data.Replace("{guid}", Guid.NewGuid().ToString()).Replace("{text}", text) }; // bing sentiment uses guid
                    else
                        return new List<string> { data.Replace("{guid}", Guid.NewGuid().ToString()).Replace("{text}", Convert.ToBase64String(bytes)) }; // bing sentiment uses guid
                default:
                    throw new MissingFieldException();
            }
        }

        private static List<Tuple<string, string>> MakeHeaders(Settings.Request request)
        {
            List<Tuple<string, string>> headers = new List<Tuple<string, string>>();
            if (request == null || request.headers == null)
                return headers;
            foreach (Settings.Header h in request.headers)
            {
                // TODO: need to expand request headers to include any header. Use Value instead of "Generic" or concrete name (Accept, ContentType)
                switch (h.Name)
                {
                    case "BearerAuthentication":
                        AccessTokenInfo accessTokenInfo = ImageAuth.PerformAuthenticationAsync(request, h).Result; // TODO: make this method async so don't have to do Wait?
                        headers.Add(new Tuple<string, string>("Authorization", "Bearer " + accessTokenInfo.access_token));
                        break;
                    case "BasicAuthentication":
                        string userpass = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", h.BasicAuthentication.username, h.BasicAuthentication.password).Replace('+', '-').Replace('/', '_')));
                        headers.Add(new Tuple<string, string>("Authorization", "Basic " + userpass));
                        break;
                    case "HoundifyAuthentication": // will be passed in but must skip, don't let default
                        break;
                    case "OcpApimSubscriptionKey":
                        headers.Add(new Tuple<string, string>("Ocp-Apim-Subscription-Key", h.OcpApimSubscriptionKey)); // needs rewrite. see above.
                        break;
                    default:
                        if (h.Generic == null)
                            throw new Exception("Header: Generic is null");
                        headers.Add(new Tuple<string, string>(h.Name, h.Generic));
                        break;
                }
            }
            return headers;
        }

        public static async System.Threading.Tasks.Task CallApiAsync(ServiceResponse response, List<Tuple<string, string>> uriSubstitutes, List<Tuple<string, string>> headers)
        {
            await CallApiAsyncInternal(response, null, uriSubstitutes, headers, null, new byte[0], null);
        }

        public static async System.Threading.Tasks.Task CallApiAsync(ServiceResponse response, byte[] bytes, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            await CallApiAsyncInternal(response, null, null, null, null, bytes, apiArgs);
        }

        public static async System.Threading.Tasks.Task CallApiAsync(ServiceResponse response, string text, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            await CallApiAsyncInternal(response, null, null, null, null, text, apiArgs);
        }

        public static async System.Threading.Tasks.Task CallApiAsync(ServiceResponse response, Uri url, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            await CallApiAsyncInternal(response, null, null, null, null, url.ToString(), apiArgs);
        }

        public static async System.Threading.Tasks.Task CallApiAsync(ServiceResponse response, Uri uri, byte[] bytes, System.Collections.Generic.Dictionary<string, string> apiArgs, List<Tuple<string, string>> headers)
        {
            await CallApiAsyncInternal(response, uri, null, headers, null, bytes, apiArgs);
        }

        public static async System.Threading.Tasks.Task CallApiAsync(ServiceResponse response, Uri uri, string text, System.Collections.Generic.Dictionary<string, string> apiArgs, List<Tuple<string, string>> headers)
        {
            await CallApiAsyncInternal(response, uri, null, headers, null, text, apiArgs);
        }

        public static async System.Threading.Tasks.Task CallApiAsync(ServiceResponse response, List<Tuple<string, string>> UriSubstitutes, List<Tuple<string, string>> headers, List<Tuple<string, string>> postDataSubstitutes, byte[] bytes, System.Collections.Generic.Dictionary<string, string> apiArgs, int maxResponseLength = 10000000)
        {
            await CallApiAsyncInternal(response, null, UriSubstitutes, headers, postDataSubstitutes, bytes, apiArgs);
        }

        public static async System.Threading.Tasks.Task CallApiAsync(ServiceResponse response, List<Tuple<string, string>> UriSubstitutes, List<Tuple<string, string>> headers, List<Tuple<string, string>> postDataSubstitutes, string text, System.Collections.Generic.Dictionary<string, string> apiArgs, int maxResponseLength = 10000000)
        {
            await CallApiAsyncInternal(response, null, UriSubstitutes, headers, postDataSubstitutes, text, apiArgs);
        }

        private static async System.Threading.Tasks.Task CallApiAsyncInternal(ServiceResponse response, Uri uri, List<Tuple<string, string>> UriSubstitutes, List<Tuple<string, string>> headers, List<Tuple<string, string>> postDataSubstitutes, byte[] bytes, System.Collections.Generic.Dictionary<string, string> apiArgs, int maxResponseLength = 10000000)
        {
            switch (response.Request.method)
            {
                case "get":
                    await GetAsync(response, uri, UriSubstitutes, headers, postDataSubstitutes, null, apiArgs);
                    break;
                case "post":
                    await PostAsync(response, uri, UriSubstitutes, headers, postDataSubstitutes, bytes, apiArgs);
                    break;
                default:
                    throw new Exception("Unsupported Http method:" + response.Request.method);
            }
        }

        private static async System.Threading.Tasks.Task CallApiAsyncInternal(ServiceResponse response, Uri uri, List<Tuple<string, string>> UriSubstitutes, List<Tuple<string, string>> headers, List<Tuple<string, string>> postDataSubstitutes, string text, System.Collections.Generic.Dictionary<string, string> apiArgs, int maxResponseLength = 10000000)
        {
            switch (response.Request.method)
            {
                case "get":
                    await GetAsync(response, uri, UriSubstitutes, headers, postDataSubstitutes, text, apiArgs);
                    break;
                case "post":
                    await PostAsync(response, uri, UriSubstitutes, headers, postDataSubstitutes, text, apiArgs);
                    break;
                default:
                    throw new Exception("Unsupported Http method:" + response.Request.method);
            }
        }

        // for oAuth calls
        public static async System.Threading.Tasks.Task CallApiAuthAsync(ServiceResponse response, Uri uri, string text, List<Tuple<string, string>> headers, int maxResponseLength = 10000000)
        {
            await MakePostCurl(uri, headers, new List<string>() { text }, null, false);
            System.Net.Http.HttpContent requestContent = new System.Net.Http.StringContent(text);
            await SystemNetAsync(response, "POST", uri, headers, requestContent, false, maxResponseLength);
        }

        private static async System.Threading.Tasks.Task GetAsync(ServiceResponse response, Uri uri, List<Tuple<string, string>> UriSubstitutes, List<Tuple<string, string>> headers, List<Tuple<string, string>> postDataSubstitutes, string text, System.Collections.Generic.Dictionary<string, string> apiArgs, int maxResponseLength = 10000000)
        {
            Settings.Request request = response.Request;
            bool binaryResponse = request.response.type == "binary";
            if (uri == null)
                uri = MakeUri(request, apiArgs, UriSubstitutes, text);
            if (headers == null)
                headers = MakeHeaders(request);
            List<string> texts = MakePostDataSubstitutes(request, text, null, apiArgs, postDataSubstitutes);
            if (texts.Count != 1)
                throw new Exception("multipart GET not allowed");
            await MakeGetCurl(uri, headers, request.response.jq, binaryResponse);
#if WINDOWS_UWP
            ServiceResponse response;
            if (Options.options.Services.APIs.PreferSystemNet)
                await SystemNetAsync("GET", uri, headers, new System.Net.Http.ByteArrayContent(requestContent), binaryResponse, maxResponseLength);
            else
                await WindowsWebAsync("GET", new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
#else
            await SystemNetAsync(response, "GET", uri, headers, null, binaryResponse, maxResponseLength);
#endif
            response.FileName = "curl-" + httpCallCount;
        }

        private static async System.Threading.Tasks.Task PostAsync(ServiceResponse response, Uri uri, List<Tuple<string, string>> UriSubstitutes, List<Tuple<string, string>> headers, List<Tuple<string, string>> postDataSubstitutes, byte[] bytes, System.Collections.Generic.Dictionary<string, string> apiArgs, int maxResponseLength = 10000000)
        {
            Settings.Request request = response.Request;
            bool binaryResponse = request.response.type == "binary";
            string fileName = null;
            if (apiArgs.ContainsKey("fileName"))
                fileName = apiArgs["fileName"];

            if (uri == null)
                uri = MakeUri(request, apiArgs, UriSubstitutes);
            if (headers == null)
                headers = MakeHeaders(request);
            List<string> texts = MakePostDataSubstitutes(request, null, bytes, apiArgs, postDataSubstitutes);
            System.Net.Http.HttpContent requestContent;
            if (texts == null)
            {
                await MakePostCurl(uri, headers, bytes, request.response.jq, binaryResponse);
                requestContent = new System.Net.Http.ByteArrayContent(bytes);
            }
            else
            {
                if (texts.Count == 1 && !isMultipart(headers))
                {
                    await MakePostCurl(uri, headers, texts, request.response.jq, binaryResponse);
                    if (texts[0] == null) // Houndify text intent
                        requestContent = new System.Net.Http.ByteArrayContent(bytes);
                    else
                        requestContent = new System.Net.Http.StringContent(texts[0]);
                    await SystemNetAsync(response, "POST", uri, headers, requestContent, binaryResponse, maxResponseLength);
                    return;
                }
                else
                {
                    List<KeyValuePair<string, string>> lkvp = new List<KeyValuePair<string, string>>();
                    foreach (string s in texts)
                        lkvp.Add(new KeyValuePair<string, string>(s.Substring(0, s.IndexOf("=")), s.Substring(s.IndexOf("=") + 1)));
                    await MakePostMultiPartCurl(uri, headers, lkvp, request.response.jq, binaryResponse);

                    System.Net.Http.MultipartFormDataContent multiPartRequestContent = new System.Net.Http.MultipartFormDataContent();
                    foreach (KeyValuePair<string, string> kvp in lkvp)
                    {
                        System.Net.Http.HttpContent ht;
                        switch (kvp.Key)
                        {
                            case "file":
                                ht = new System.Net.Http.ByteArrayContent(bytes);
                                ht.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream"); // optional for HPE Haven?
                                multiPartRequestContent.Add(ht, "\"file\"", "\"" + fileName + "\"");
                                break;
                            default:
                                ht = new System.Net.Http.StringContent(kvp.Value);
                                ht.Headers.ContentType = null;
                                multiPartRequestContent.Add(ht, '"' + kvp.Key + '"');
                                break;
                        }
                    }
                    await SystemNetAsync(response, "POST", uri, headers, multiPartRequestContent, binaryResponse, maxResponseLength);
                    response.FileName = "curl-" + httpCallCount;
                    return;
                }
            }
#if WINDOWS_UWP
            if (Options.options.Services.APIs.PreferSystemNet)
                await SystemNetAsync("POST", uri, headers, new System.Net.Http.ByteArrayContent(requestContent), binaryResponse, maxResponseLength);
            else
                await WindowsWebAsync("POST", new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
#else
            await SystemNetAsync(response, "POST", uri, headers, requestContent, binaryResponse, maxResponseLength);
#endif
            response.FileName = "curl-" + httpCallCount;
        }

        private static async System.Threading.Tasks.Task PostAsync(ServiceResponse response, Uri uri, List<Tuple<string, string>> UriSubstitutes, List<Tuple<string, string>> headers, List<Tuple<string, string>> postDataSubstitutes, string text, System.Collections.Generic.Dictionary<string, string> apiArgs, int maxResponseLength = 10000000)
        {
            Settings.Request request = response.Request;
            bool binaryResponse = request.response.type == "binary";
            if (uri == null)
                uri = MakeUri(request, apiArgs, UriSubstitutes, text);
            if (headers == null)
                headers = MakeHeaders(request);
            List<string> data = MakePostDataSubstitutes(request, text, null, apiArgs, postDataSubstitutes);
            if (data.Count == 1 && !isMultipart(headers))
            {
                await MakePostCurl(uri, headers, data, request == null ? null : request.response.jq, binaryResponse);
                System.Net.Http.HttpContent requestContent = new System.Net.Http.StringContent(data[0]);
                await SystemNetAsync(response, "POST", uri, headers, requestContent, binaryResponse, maxResponseLength);
                response.FileName = "curl-" + httpCallCount;
                return;
            }
            else
            {
#if true // MicrosoftCognitiveInsightService can use multipart? See Post Byte[] overload above for helpful details.
                throw new Exception("PostAsync: multipart/form-data not implemented. Doesn't seem to work with System.Net.Http");
#else
                List<KeyValuePair<string, string>> kvps = new List<KeyValuePair<string, string>>();
                foreach (string s in data)
                    kvps.Add(new KeyValuePair<string, string>(s.Substring(0, s.IndexOf("=")), s.Substring(s.IndexOf("=") + 1)));
                await MakePostMultiPartCurl(uri, headers, kvps, request.response.jq, binaryResponse);
                List<KeyValuePair<string, string>> newkvps = new List<KeyValuePair<string, string>>();
                foreach (KeyValuePair<string, string> kvp in kvps)
                {
                    string s = kvp.Value;
                    if (kvp.Value[0] == '@')
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(kvp.Value.Substring(1));
                        s = Convert.ToBase64String(bytes);
                    }
                    newkvps.Add(new KeyValuePair<string, string>(kvp.Key, s));
                }
                System.Net.Http.MultipartFormDataContent multiPartRequestContent = new System.Net.Http.MultipartFormDataContent();
                multiPartRequestContent.Add(new System.Net.Http.FormUrlEncodedContent(newkvps));
                await SystemNetAsync(response, "POST", uri, headers, multiPartRequestContent, binaryResponse, maxResponseLength);
                response.FileName = "curl-" + httpCallCount;
#endif
            }
#if WINDOWS_UWP // major bit rot. Multi-part not implemented.
            if (Options.options.Services.APIs.PreferSystemNet)
                response = await SystemNetAsync("POST", uri, headers, new System.Net.Http.ByteArrayContent(requestContent), binaryResponse, maxResponseLength);
            else
                response = await WindowsWebAsync("POST", new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);#else
            response = await SystemNetAsync("POST", uri, headers, requestContent, binaryResponse, maxResponseLength);
            response.FileName = "curl-" + httpCallCount;
            return response;
#endif
        }

        private static async System.Threading.Tasks.Task SystemNetAsync(ServiceResponse response, string method, Uri uri, List<Tuple<string, string>> headers, System.Net.Http.HttpContent requestContent, bool binaryResponse, int maxResponseLength)
        {
            long t = 0;
            response.stopWatch.Start();
            try
            {
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);

                request.Method = method;


                if (requestContent != null)
                {
                    if (requestContent.GetType() == typeof(System.Net.Http.MultipartFormDataContent))
                        request.ContentType = requestContent.Headers.ContentType.ToString();
                    using (System.IO.Stream requestStream = await request.GetRequestStreamAsync())
                    {
#if false
                        System.IO.Stream s = await requestContent.ReadAsStreamAsync();
                        long? l = requestContent.Headers.ContentLength;
                        if (l > int.MaxValue)
                            throw new ArgumentOutOfRangeException();
                        int i = (int)l;
                        byte[] buffer = new byte[i];
                        int len = await s.ReadAsync(buffer, 0, i);
                        //System.IO.StreamReader sr = new System.IO.StreamReader(requestContent.ReadAsStreamAsync());
                        //await requestContent.CopyToAsync(sr);
#endif
                        await requestContent.CopyToAsync(requestStream);
                    }
                }

                if (headers != null)
                {
                    foreach (Tuple<string, string> h in headers)
                    {
                        switch (h.Item1)
                        {
                            case "Accept": // must use explicity Accept property otherwise an error is thrown
                                request.Accept = h.Item2;
                                break;
                            case "Content-Type": // must use explicity ContentType property otherwise an error is thrown
                                request.ContentType = h.Item2;
                                break;
                            case "User-Agent": // must use explicity ContentType property otherwise an error is thrown
                                request.UserAgent = h.Item2;
                                break;
                            default:
                                request.Headers[h.Item1] = h.Item2;
                                break;
                        }
                    }
                }

                t = response.stopWatch.ElapsedMilliseconds;
                using (System.Net.WebResponse wr = await request.GetResponseAsync())
                {
                    response.RequestElapsedMilliseconds = response.stopWatch.ElapsedMilliseconds - t;
                    Log.WriteLine("Request Elapsed milliseconds:" + response.RequestElapsedMilliseconds);
                    using (System.Net.HttpWebResponse hwr = (System.Net.HttpWebResponse)wr)
                    {
                        response.StatusCode = (int)hwr.StatusCode;
                        Log.WriteLine("Request StatusCode:" + response.StatusCode);
                        if (hwr.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.ResponseJson = null;
                            using (System.IO.Stream rr = wr.GetResponseStream())
                            {
                                if (binaryResponse)
                                    await PostSystemNetAsyncBinaryReader(rr, maxResponseLength, response);
                                else
                                    await PostSystemNetAsyncStreamReader(rr, response);
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
            response.FileName = "curl-" + httpCallCount;
            response.stopWatch.Stop();
            response.TotalElapsedMilliseconds = response.stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total Elapsed milliseconds:" + response.TotalElapsedMilliseconds);
        }

        public static async System.Threading.Tasks.Task PostSystemNetAsyncBinaryReader(System.IO.Stream rr, int maxResponseLength, ServiceResponse response)
        {
            try
            {
                using (System.IO.BinaryReader r = new System.IO.BinaryReader(rr)) // if needed, there is a constructor which will leave the stream open
                {
                    response.ResponseBytes = r.ReadBytes(maxResponseLength);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Exception:" + ex.Message);
                if (ex.InnerException != null)
                    Log.WriteLine("InnerException:" + ex.InnerException);
            }
        }

        public static async System.Threading.Tasks.Task PostSystemNetAsyncStreamReader(System.IO.Stream rr, ServiceResponse response)
        {
            try
            {
                using (System.IO.StreamReader r = new System.IO.StreamReader(rr)) // if needed, there is a constructor which will leave the stream open
                {
                    // Google Cloud partial results are incomplete json strings while Google Web's are complete json of initial results
                    response.ResponseString = await r.ReadToEndAsync();
                    if (Options.options.debugLevel >= 4)
                        Log.WriteLine("ResponseString:" + response.ResponseString);
                    if (string.IsNullOrWhiteSpace(response.ResponseString))
                        throw new MissingFieldException();
                    if (response.ResponseString[0] == '{')
                        response.ResponseJToken = Newtonsoft.Json.Linq.JObject.Parse(response.ResponseString);
                    else if (response.ResponseString[0] == '[')
                        response.ResponseJToken = Newtonsoft.Json.Linq.JArray.Parse(response.ResponseString);
                    else
                        throw new MissingFieldException();
                    response.ResponseJsonFormatted = Newtonsoft.Json.JsonConvert.SerializeObject(response.ResponseJToken, new Newtonsoft.Json.JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                    if (Options.options.debugLevel >= 4)
                        Log.WriteLine("ResponseJsonFormatted:" + response.ResponseJsonFormatted);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Exception:" + ex.Message);
                if (ex.InnerException != null)
                    Log.WriteLine("InnerException:" + ex.InnerException);
            }
        }

        public static async System.Threading.Tasks.Task ExtractResultAsync(ServiceResponse response)
        {
            switch (response.Request.response.type)
            {
                case "json":
                    if (response.Request.response.jsonPath == null) // json routine but no jsonPath specified. just ignore.
                        return;
                    Settings.Request request = response.Request;
                    if (response.ResponseJToken == null || string.IsNullOrEmpty(response.ResponseJToken.ToString()))
                    {
                        response.ResponseResult = request.response.missingResponse;
                        if (Options.options.debugLevel >= 3)
                            Log.WriteLine(response.ResponseResult);
                    }
                    else
                    {
                        // not sure how to handle array result
                        //response.ResponseResult = response.ResponseJToken.SelectTokens(service.response.jsonPath).ToString();
                        // jq.net doesn't seem to be supported. I don't know how to instantiate it as there's no constructor. I haven't found a substitute.
                        IEnumerable<Newtonsoft.Json.Linq.JToken> jtoks = response.ResponseJToken.SelectTokens(request.response.jsonPath);
                        response.ResponseResult = string.Empty;
                        foreach (Newtonsoft.Json.Linq.JToken jtok in jtoks)
                            response.ResponseResult += ", " + jtok.ToString();
                        if (response.ResponseResult.Length > 0)
                            response.ResponseResult = response.ResponseResult.Substring(2);
                        if (Options.options.debugLevel >= 3)
                            Log.WriteLine(response.ResponseJToken.Path + ": " + response.ResponseResult);
                    }
                    break;
                case "xml":
                    // not implemented/debugged
                    response.ResponseXml.LoadXml(response.ResponseResult);
                    response.ResponseXmlNodeList = response.ResponseXml.SelectNodes(response.Request.response.xpath);
                    response.ResponseXmlFormatted = response.ResponseXmlNodeList.ToString(); // must be wrong
                    break;
                default:
                    break;
            }
        }

        public static bool isMultipart(List<Tuple<string, string>> headers)
        {
            if (headers != null)
            {
                foreach (Tuple<string, string> h in headers) // makes argument for changing header to a dictionary
                {
                    if (h.Item1.ToLower() == "content-type" && h.Item2.ToLower() == "multipart/form-data")
                        return true;
                }
            }
            return false;
        }

        // determine if get or post. if post, is data string, json, binary, urlencoded?
        public static async System.Threading.Tasks.Task<string> MakeGetCurl(Uri uri, List<Tuple<string, string>> headers, string jq, bool binaryResponse)
        {
            string fileName = "curl-" + ++httpCallCount;
            return await MakeCurl(fileName, uri, headers, jq, null, binaryResponse);
        }

        public static async System.Threading.Tasks.Task<string> MakePostCurl(Uri uri, List<Tuple<string, string>> headers, byte[] bytes, string jq, bool binaryResponse)
        {
            string fileName = "curl-" + ++httpCallCount;
            System.IO.File.WriteAllBytes(fileName + ".bin", bytes);
            string data = "--data-binary \"@" + fileName + ".bin\""; // create unique filename with data for reuse? use original text/file? what about data-ascii, data-raw, data-urlencode?
            return await MakeCurl(fileName, uri, headers, jq, data, binaryResponse);
        }

        public static async System.Threading.Tasks.Task<string> MakePostCurl(Uri uri, List<Tuple<string, string>> headers, List<string> text, string jq, bool binaryResponse)
        {
            string fileName = "curl-" + ++httpCallCount;
            string data = string.Empty;
            if (text.Count != 1)
                throw new FormatException();
            System.IO.File.WriteAllText(fileName + ".txt", text[0]);
            data += "--data \"@" + fileName + ".txt\" "; // create unique filename with data for reuse? use original text/file? what about data-ascii, data-raw, data-urlencode?
            return await MakeCurl(fileName, uri, headers, jq, data, binaryResponse);
        }

        public static async System.Threading.Tasks.Task<string> MakePostMultiPartCurl(Uri uri, List<Tuple<string, string>> headers, List<KeyValuePair<string, string>> text, string jq, bool binaryResponse)
        {
            string fileName = "curl-" + ++httpCallCount;
            string data = string.Empty;
            for (int i = 0; i < text.Count; ++i)
            {
                if (text[i].Value[0] == '@') // todo: obsolete?
                {
                    data += "--form \"" + text[i].Key + "=" + text[i].Value + "\" "; // create unique filename with data for reuse? use original text/file? what about data-ascii, data-raw, data-urlencode?
                }
                else
                {
                    System.IO.File.WriteAllText(fileName + "." + i.ToString() + ".txt", text[i].Value);
                    data += "--form \"" + text[i].Key + "=@" + fileName + "." + i.ToString() + ".txt\" "; // create unique filename with data for reuse? use original text/file? what about data-ascii, data-raw, data-urlencode?
                }
            }
            return await MakeCurl(fileName, uri, headers, jq, data, binaryResponse);
        }

        public static async System.Threading.Tasks.Task<string> MakeCurl(string fileName, Uri uri, List<Tuple<string, string>> headers, string jq, string data, bool binaryResponse)
        {
            try
            {
                string curl;
                if (string.IsNullOrWhiteSpace(Options.options.curlDefaults))
                    curl = "curl";
                else
                    curl = Options.options.curlDefaults; // e.g. - x 127.0.0.1:8888 - k - v--libcurl<filename>.curl
                if (data != null)
                    curl += " " + data;
                if (headers != null)
                {
                    foreach (Tuple<string, string> t in headers)
                    {
                        switch (t.Item1) // some headers must be specified as curl switches
                        {
                            case "BearerAuthentication": // e.g. IBM Watson
                                curl += " -u \"" + t.Item1 + ": " + t.Item2.Replace("\"", "\\\"") + "\"";
                                break;
                            default:
                                curl += " -H \"" + t.Item1 + ": " + t.Item2.Replace("\"", "\\\"") + "\"";
                                break;
                        }
                    }
                }
                string responseFile = fileName + (binaryResponse ? "-response.bin" : "-response.txt");
                curl += " \"" + uri.AbsoluteUri + "\" -o " + responseFile;
                if (!binaryResponse)
                {
                    if (jq == null)
                    {
                        curl += "\ntype " + responseFile;
                    }
                    else
                    {
                        string jqFile = fileName + "-jq.txt";
                        curl += "\njq \"" + jq + "\" " + responseFile + " > " + jqFile;
                        curl += "\ntype " + jqFile;
                    }
                }
                Log.WriteLine(curl);
                //Console.WriteLine(curl);
                System.IO.File.WriteAllText(fileName + ".bat", curl.Replace("%", "%%"));
                //System.IO.File.AppendAllText(Options.options.curlFilePath, curl);

                return curl;
            }
            catch (Exception ex)
            {
                do
                {
                    Console.WriteLine("WoundifyConsole: Exception:" + ex.Message);
                }
                while ((ex = ex.InnerException) != null);
            }
            return null;
        }
    }
}
