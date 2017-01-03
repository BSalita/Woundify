using System;
using System.Collections.Generic;
using System.Text;

namespace WoundifyShared
{
    public class HttpMethods
    {
        private static ulong httpCallCount = 0;
        static HttpMethods()
        {
            foreach (string f in System.IO.Directory.EnumerateFiles(".", "curl-*.*"))
            {
                System.IO.File.Delete(f);
            }
        }

#if WINDOWS_UWP // major bit rot
        public virtual async System.Threading.Tasks.Task<IServiceResponse> PostAsyncWindowsWeb(Uri uri, byte[] audioBytes, int sampleRate)
        {
        IServiceResponse response = new IServiceResponse();
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
        public Uri MakeUri(Settings.Service service, List<Tuple<string, string>> uriSubstitutes)
        {
            UriBuilder ub = new UriBuilder();
            string scheme = service.request.uri.scheme;
            string host = service.request.uri.host;
            string path = service.request.uri.path;
            string query = service.request.uri.query;
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
            return ub.Uri;
        }

        private List<string> MakePostDataSubstitutes(Settings.Service service, string text, byte[] bytes, List<Tuple<string, string>> postDataSubstitutes)
        {
            if (service == null)
                return new List<string> { text };

            string data = service.request.data.value;
            if (data == null)
                data = text;
            else if (postDataSubstitutes != null)
            {
                foreach (Tuple<string, string> r in postDataSubstitutes)
                {
                    data = data.Replace(r.Item1, r.Item2);
                }
            }
            switch (service.request.data.type)
            {
                case "ascii":
                    return new List<string> { data };
                case "base64":
                    // TODO: use Helpers.stringToDictionary()
                    if (data.Contains("=")) // TODO: use text or data?
                    {
                        List<string> content = new List<string>();
                        foreach (string multicontent in data.Split('&'))
                        {
                            string[] namecontent = multicontent.Split('=');
                            if (namecontent.Length != 2)
                                throw new FormatException();
                            content.Add(namecontent[0] + "=" + namecontent[1].Replace("{text}", Convert.ToBase64String(bytes)));
                        }
                        return content;
                    }
                    else
                    {
                        return new List<string> { data.Replace("{text}", Convert.ToBase64String(bytes)) };
                    }
                case "binary":
                    return null;
                case "json":
                    // TODO: pass list of substitutes instead of hard coding?
                    return new List<string> { data.Replace("{guid}", Guid.NewGuid().ToString()).Replace("{text}", text) }; // bing sentiment uses guid
                case "raw":
                    return new List<string> { data };
                case "string":
                    return new List<string> { data };
                case "urlencode":
                    // TODO: use Helpers.stringToDictionary()
                    data = data.Replace("{text}", text); // bing spell
                    if (data.Contains("="))
                    {
                        List<string> content = new List<string>();
                        foreach (string multicontent in data.Split('&'))
                        {
                            string[] namecontent = multicontent.Split('=');
                            if (namecontent.Length != 2)
                                throw new FormatException();
                            content.Add(namecontent[0] + "=" + System.Web.HttpUtility.UrlEncode(namecontent[1]));
                        }
                        return content;
                    }
                    else
                    {
                        return new List<string> { System.Web.HttpUtility.UrlEncode(data) };
                    }
                default:
                    throw new MissingFieldException();
            }
        }

        private List<Tuple<string, string>> MakeHeaders(Settings.Service service)
        {
            List<Tuple<string, string>> headers = new List<Tuple<string, string>>();
            if (service == null || service.request == null || service.request.headers == null)
                return headers;
            foreach (Settings.Header h in service.request.headers)
            {
                // TODO: need to expand request headers to include any header. Use Value instead of concrete name (Accept, ContentType)
                switch (h.Name)
                {
                    case "Accept":
                        headers.Add(new Tuple<string, string>("Accept", h.Accept));
                        break;
                    case "BearerAuthentication":
                        // contained within uriSubstitutes
                        break;
                    case "BasicAuthentication":
                        string userpass = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", h.BasicAuthentication.username, h.BasicAuthentication.password).Replace('+', '-').Replace('/', '_')));
                        headers.Add(new Tuple<string, string>("Authorization", "Basic " + userpass));
                        break;
                    case "Content-Type":
                        headers.Add(new Tuple<string, string>("Content-Type", h.ContentType));
                        break;
                    default:
                        throw new MissingFieldException();
                }
            }
            return headers;
        }

        public virtual async System.Threading.Tasks.Task<ServiceResponse> GetAsync(Settings.Service service)
        {
            return await GetAsync(service, null, null, null);
        }

        public virtual async System.Threading.Tasks.Task<ServiceResponse> GetAsync(Settings.Service service, List<Tuple<string, string>> uriSubstitutes, List<Tuple<string, string>> headers)
        {
            return await GetAsync(service, null, uriSubstitutes, headers);
        }

        public virtual async System.Threading.Tasks.Task<ServiceResponse> GetAsync(Settings.Service service, Uri uri, List<Tuple<string, string>> uriSubstitutes, List<Tuple<string, string>> headers)
        {
            if (uri == null)
                uri = MakeUri(service, uriSubstitutes);
            if (headers == null)
                headers = MakeHeaders(service);
            await MakeGetCurl(uri, headers);
#if WINDOWS_UWP
            ServiceResponse response;
            if (Options.options.Services.APIs.PreferSystemNet)
                response = await SystemNetAsync("GET", uri, headers, new System.Net.Http.ByteArrayContent(requestContent), binaryResponse, maxResponseLength);
            else
                response = await WindowsWebAsync("GET", new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
#else
            ServiceResponse response = await SystemNetAsync("GET", uri, headers);
#endif
            return response;
        }

        public virtual async System.Threading.Tasks.Task<ServiceResponse> PostAsync(Settings.Service service, byte[] bytes)
        {
            return await PostAsync(service, null, null, null, null, bytes);
        }

        public virtual async System.Threading.Tasks.Task<ServiceResponse> PostAsync(Settings.Service service, string text)
        {
            return await PostAsync(service, null, null, null, null, text);
        }

        public virtual async System.Threading.Tasks.Task<ServiceResponse> PostAsync(Uri uri, byte[] bytes)
        {
            return await PostAsync(null, uri, null, null, null, bytes);
        }

        public virtual async System.Threading.Tasks.Task<ServiceResponse> PostAsync(Uri uri, string text)
        {
            return await PostAsync(null, uri, null, null, null, text);
        }

        public virtual async System.Threading.Tasks.Task<ServiceResponse> PostAsync(Uri uri, byte[] bytes, List<Tuple<string, string>> headers)
        {
            return await PostAsync(null, uri, null, headers, null, bytes);
        }

        public virtual async System.Threading.Tasks.Task<ServiceResponse> PostAsync(Settings.Service service, Uri uri, byte[] bytes, List<Tuple<string, string>> headers)
        {
            return await PostAsync(service, uri, null, headers, null, bytes);
        }

        public virtual async System.Threading.Tasks.Task<ServiceResponse> PostAsync(Uri uri, string text, List<Tuple<string, string>> headers)
        {
            return await PostAsync(null, uri, null, headers, null, text);
        }

        public virtual async System.Threading.Tasks.Task<ServiceResponse> PostAsync(Settings.Service service, List<Tuple<string, string>> UriSubstitutes, List<Tuple<string, string>> headers, List<Tuple<string, string>> postDataSubstitutes, byte[] bytes, bool binaryResponse = false, int maxResponseLength = 10000000)
        {
            return await PostAsync(service, null, UriSubstitutes, headers, postDataSubstitutes, bytes);
        }

        public virtual async System.Threading.Tasks.Task<ServiceResponse> PostAsync(Settings.Service service, List<Tuple<string, string>> UriSubstitutes, List<Tuple<string, string>> headers, List<Tuple<string, string>> postDataSubstitutes, string text, bool binaryResponse = false, int maxResponseLength = 10000000)
        {
            return await PostAsync(service, null, UriSubstitutes, headers, postDataSubstitutes, text);
        }

        internal async System.Threading.Tasks.Task<ServiceResponse> PostAsync(Settings.Service service, Uri uri, List<Tuple<string, string>> UriSubstitutes, List<Tuple<string, string>> headers, List<Tuple<string, string>> postDataSubstitutes, byte[] bytes, bool binaryResponse = false, int maxResponseLength = 10000000)
        {
            if (uri == null)
                uri = MakeUri(service, UriSubstitutes);
            if (headers == null)
                headers = MakeHeaders(service);
            List<string> text = MakePostDataSubstitutes(service, null, bytes, postDataSubstitutes);
            System.Net.Http.HttpContent requestContent;
            if (text == null)
            {
                await MakePostCurl(uri, headers, bytes, binaryResponse);
                requestContent = new System.Net.Http.ByteArrayContent(bytes);
            }
            else
            {
                if (text.Count == 1)
                {
                    await MakePostCurl(uri, headers, text, binaryResponse);
                    requestContent = new System.Net.Http.StringContent(text[0]);
                    return await SystemNetAsync("POST", uri, headers, requestContent, binaryResponse, maxResponseLength);
                }
                else
                {
                    List<KeyValuePair<string, string>> lkvp = new List<KeyValuePair<string, string>>();
                    foreach (string s in text)
                        lkvp.Add(new KeyValuePair<string, string>(s.Substring(0, s.IndexOf("=")), s.Substring(s.IndexOf("=") + 1)));
                    await MakePostMultiPartCurl(uri, headers, lkvp, binaryResponse);

                    System.Net.Http.MultipartFormDataContent multiPartRequestContent = new System.Net.Http.MultipartFormDataContent();
                    foreach (KeyValuePair<string, string> kvp in lkvp)
                    {
                        System.Net.Http.HttpContent ht;
                        switch (kvp.Key)
                        {
                            case "file":
                                ht = new System.Net.Http.ByteArrayContent(bytes);
                                ht.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                                multiPartRequestContent.Add(ht, "\"file\"", "\"file.wav\"");
                                break;
                            default:
                                ht = new System.Net.Http.StringContent(kvp.Value);
                                ht.Headers.ContentType = null;
                                multiPartRequestContent.Add(ht, '"' + kvp.Key + '"');
                                break;
                        }
                    }
                    return await SystemNetAsync("POST", uri, headers, multiPartRequestContent, binaryResponse, maxResponseLength);
                }
            }
#if WINDOWS_UWP
            ServiceResponse response;
            if (Options.options.Services.APIs.PreferSystemNet)
                response = await SystemNetAsync("POST", uri, headers, new System.Net.Http.ByteArrayContent(requestContent), binaryResponse, maxResponseLength);
            else
                response = await WindowsWebAsync("POST", new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);
#else
            ServiceResponse response = await SystemNetAsync("POST", uri, headers, requestContent, binaryResponse, maxResponseLength);
#endif
            return response;
        }

        internal async System.Threading.Tasks.Task<ServiceResponse> PostAsync(Settings.Service service, Uri uri, List<Tuple<string, string>> UriSubstitutes, List<Tuple<string, string>> headers, List<Tuple<string, string>> postDataSubstitutes, string text, bool binaryResponse = false, int maxResponseLength = 10000000)
        {
            if (uri == null)
                uri = MakeUri(service, UriSubstitutes);
            if (headers == null)
                headers = MakeHeaders(service);
            List<string> data = MakePostDataSubstitutes(service, text, null, postDataSubstitutes);
            if (data.Count == 1)
            {
                await MakePostCurl(uri, headers, data, binaryResponse);
                System.Net.Http.HttpContent requestContent = new System.Net.Http.StringContent(data[0]);
                return await SystemNetAsync("POST", uri, headers, requestContent, binaryResponse, maxResponseLength);
            }
            else
            {
                List<KeyValuePair<string, string>> kvp = new List<KeyValuePair<string, string>>();
                foreach (string s in data)
                    kvp.Add(new KeyValuePair<string, string>(s.Substring(0, s.IndexOf("=")), s.Substring(s.IndexOf("=") + 1)));
                await MakePostMultiPartCurl(uri, headers, kvp, binaryResponse);
                System.Net.Http.MultipartFormDataContent multiPartRequestContent = new System.Net.Http.MultipartFormDataContent();
                multiPartRequestContent.Add(new System.Net.Http.FormUrlEncodedContent(kvp));
                return await SystemNetAsync("POST", uri, headers, multiPartRequestContent, binaryResponse, maxResponseLength);
            }
#if WINDOWS_UWP // major bit rot. Multi-part not implemented.
            ServiceResponse response;
            if (Options.options.Services.APIs.PreferSystemNet)
                response = await SystemNetAsync("POST", uri, headers, new System.Net.Http.ByteArrayContent(requestContent), binaryResponse, maxResponseLength);
            else
                response = await WindowsWebAsync("POST", new Uri(requestUri), audioBytes, sampleRate, contentType, headerValue);#else
            ServiceResponse response = await SystemNetAsync("POST", uri, headers, requestContent, binaryResponse, maxResponseLength);
            return response;
#endif
        }

        internal async System.Threading.Tasks.Task<ServiceResponse> SystemNetAsync(string method, Uri uri, List<Tuple<string, string>> headers, System.Net.Http.HttpContent requestContent = null, bool binaryResponse = false, int maxResponseLength = 10000000)
        {
            ServiceResponse response = new ServiceResponse(this.ToString());
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
            response.stopWatch.Stop();
            response.TotalElapsedMilliseconds = response.stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total Elapsed milliseconds:" + response.TotalElapsedMilliseconds);
            return response;
        }

        public virtual async System.Threading.Tasks.Task PostSystemNetAsyncBinaryReader(System.IO.Stream rr, int maxResponseLength, ServiceResponse response)
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

        public virtual async System.Threading.Tasks.Task PostSystemNetAsyncStreamReader(System.IO.Stream rr, ServiceResponse response)
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

        public virtual async System.Threading.Tasks.Task ExtractResultAsync(Settings.Service service, ServiceResponse response)
        {
            if (response.ResponseJToken == null || string.IsNullOrEmpty(response.ResponseJToken.ToString()))
            {
                response.ResponseResult = service.response.missingResponse;
                if (Options.options.debugLevel >= 3)
                    Log.WriteLine(response.ResponseResult);
            }
            else
            {
                // not sure how to handle array result
                //response.ResponseResult = response.ResponseJToken.SelectTokens(service.response.jsonPath).ToString();
                IEnumerable<Newtonsoft.Json.Linq.JToken> jtoks = response.ResponseJToken.SelectTokens(service.response.jsonPath);
                response.ResponseResult = string.Empty;
                foreach (Newtonsoft.Json.Linq.JToken jtok in jtoks)
                    response.ResponseResult += ", " + jtok.ToString();
                if (response.ResponseResult.Length > 0)
                    response.ResponseResult = response.ResponseResult.Substring(2);
                if (Options.options.debugLevel >= 3)
                    Log.WriteLine(response.ResponseJToken.Path + ": " + response.ResponseResult);
            }
        }

        // determine if get or post. if post, is data string, json, binary, urlencoded?
        public virtual async System.Threading.Tasks.Task<string> MakeGetCurl(Uri uri, List<Tuple<string, string>> headers)
        {
            string fileName = "curl-" + ++httpCallCount;
            return await MakeCurl(fileName, uri, headers);
        }

        public virtual async System.Threading.Tasks.Task<string> MakePostCurl(Uri uri, List<Tuple<string, string>> headers, byte[] bytes, bool binaryResponse = false)
        {
            string fileName = "curl-" + ++httpCallCount;
            System.IO.File.WriteAllBytes(fileName + ".bin", bytes);
            string data = "--data-binary \"@" + fileName + ".bin\""; // create unique filename with data for reuse? use original text/file? what about data-ascii, data-raw, data-urlencode?
            return await MakeCurl(fileName, uri, headers, data);
        }

        public virtual async System.Threading.Tasks.Task<string> MakePostCurl(Uri uri, List<Tuple<string, string>> headers, List<string> text, bool binaryResponse = false)
        {
            string fileName = "curl-" + ++httpCallCount;
            string data = string.Empty;
            if (text.Count != 1)
                throw new FormatException();
            System.IO.File.WriteAllText(fileName + ".txt", text[0]);
            data += "--data \"@" + fileName + ".txt\" "; // create unique filename with data for reuse? use original text/file? what about data-ascii, data-raw, data-urlencode?
            return await MakeCurl(fileName, uri, headers, data);
        }

        public virtual async System.Threading.Tasks.Task<string> MakePostMultiPartCurl(Uri uri, List<Tuple<string, string>> headers, List<KeyValuePair<string, string>> text, bool binaryResponse = false)
        {
            string fileName = "curl-" + ++httpCallCount;
            string data = string.Empty;
            for (int i = 0; i < text.Count; ++i)
            {
                System.IO.File.WriteAllText(fileName + "." + i.ToString() + ".txt", text[i].Value);
                data += "--form \"" + text[i].Key + "=@" + fileName + "." + i.ToString() + ".txt\" "; // create unique filename with data for reuse? use original text/file? what about data-ascii, data-raw, data-urlencode?
            }
            return await MakeCurl(fileName, uri, headers, data);
        }

        public virtual async System.Threading.Tasks.Task<string> MakeCurl(string fileName, Uri uri, List<Tuple<string, string>> headers, string data = null)
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
                curl += " \"" + uri.AbsoluteUri + "\"";
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
