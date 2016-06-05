using System;

using System.Runtime.InteropServices.WindowsRuntime; // AsBuffer()
using System.Threading.Tasks;

namespace WoundifyShared
{
    class GoogleCloudServices : WoundifyServices
    {
        private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        public GoogleCloudServices(Settings.Service service) : base(service)
        {
        }

        public override async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();
            Log.WriteLine("audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);

            stopWatch.Start();

            UriBuilder ub = new UriBuilder();
            ub.Scheme = service.request.uri.scheme;
            ub.Host = service.request.uri.host;
            ub.Path = service.request.uri.path;
            ub.Query = service.request.uri.query.Replace("{key}", service.request.headers[1].BearerAuthentication.key); // todo: replace [1] with dictionary lookup
#if WINDOWS_UWP
            if (Options.options.Services.APIs.PreferSystemNet)
               response.sr = await PostAsyncSystemNet(ub.Uri, audioBytes, sampleRate);
            else
                response.sr = await PostAsyncWindowsWeb(ub.Uri, audioBytes, sampleRate);
#else
            response.sr = await PostAsyncSystemNet(ub.Uri, audioBytes, sampleRate);
#endif
            stopWatch.Stop();
            response.sr.TotalElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
            return response;
        }

        public override async System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(string text)
        {
            TranslateServiceResponse response = new TranslateServiceResponse();
            Log.WriteLine("text:" + text);

            stopWatch.Start();

            UriBuilder ub = new UriBuilder();
            ub.Scheme = service.request.uri.scheme;
            ub.Host = service.request.uri.host;
            ub.Path = service.request.uri.path;
            string query = service.request.uri.query;
            query = query.Replace("{key}", service.request.headers[0].BearerAuthentication.key);// todo: replace [0] with dictionary lookup
            query = query.Replace("{text}", System.Uri.EscapeDataString(text.Trim()));
            query = query.Replace("{source}", service.request.data.source);
            query = query.Replace("{target}", service.request.data.target);
            ub.Query = query;
#if WINDOWS_UWP
            if (Options.options.Services.APIs.PreferSystemNet)
               response.sr = await PostAsyncSystemNet(ub.Uri, audioBytes, sampleRate);
            else
                response.sr = await PostAsyncWindowsWeb(ub.Uri, audioBytes, sampleRate);
#else
            System.Collections.Generic.List<Tuple<string, string>> Headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                // nothing to new for now
            };
            response.sr = await GetAsyncSystemNet(ub.Uri, Headers);
            Newtonsoft.Json.Linq.JToken tokResult = response.sr.ResponseBodyToken.SelectToken("data.translations[0].translatedText"); // service.response.jsonPath);
            if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
            {
                response.sr.ResponseResult = Options.services["GoogleCloudTranslateService"].response.missingResponse;
                if (Options.options.debugLevel >= 3)
                    Log.WriteLine(response.sr.ResponseResult);
            }
            else
            {
                response.sr.ResponseResult = tokResult.ToString();
                if (Options.options.debugLevel >= 3)
                    Log.WriteLine(tokResult.Path + ": " + response.sr.ResponseResult);
            }
#endif
            stopWatch.Stop();
            response.sr.TotalElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
            return response;
        }

#if WINDOWS_UWP
        public async System.Threading.Tasks.Task<IServiceResponse> PostAsyncWindowsWeb(Uri uri, byte[] audioBytes, int sampleRate)
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

        public async System.Threading.Tasks.Task<ServiceResponse> GetAsyncSystemNet(Uri uri, System.Collections.Generic.List<Tuple<string, string>> Headers)
        {
            ServiceResponse response = new ServiceResponse(this.ToString());
            try
            {
                using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    foreach (Tuple<string, string> h in Headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(h.Item1, h.Item2);
                    }
                    Log.WriteLine("Before Get: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                    response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                    using (System.Net.Http.HttpResponseMessage rm = await httpClient.GetAsync(uri))
                    {
                        response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds - response.RequestElapsedMilliseconds;
                        response.StatusCode = (int)rm.StatusCode;
                        Log.WriteLine("After Get: StatusCode:" + response.StatusCode + " Total milliseconds:" + stopWatch.ElapsedMilliseconds + " Request milliseconds:" + response.RequestElapsedMilliseconds);
                        if (rm.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            using (System.IO.Stream rr = await rm.Content.ReadAsStreamAsync())
                            {
                                using (System.IO.StreamReader r = new System.IO.StreamReader(rr)) // if needed, there is a constructor which will leave the stream open
                                {
                                    string ResponseBodyBlob = string.Empty;
                                    // Google Cloud partial results are incomplete json strings while Google Web's are complete json of initial results
                                    while (!r.EndOfStream) // todo: is this best way of handling chunked? Seems too syncronous.
                                    {
                                        ResponseBodyBlob += r.ReadLine();
                                        if (Options.options.debugLevel >= 4)
                                            Log.WriteLine("ResponseBodyBlob:" + ResponseBodyBlob);
                                    }
                                    string ResponseBodyString = string.Empty;
                                    Newtonsoft.Json.Linq.JToken ResponseBodyToken = null;
                                    try
                                    {
                                        ResponseBodyToken = Newtonsoft.Json.Linq.JObject.Parse(ResponseBodyBlob);
                                        ResponseBodyString = ResponseBodyBlob;
                                    }
                                    // todo: obsolete?
                                    catch (Newtonsoft.Json.JsonReaderException ex) when (ex.HResult == -2146233088)
                                    {
                                        Log.WriteLine(ex.Message);
                                        ResponseBodyString = ResponseBodyBlob.Substring(0, ex.LinePosition);
                                        ResponseBodyToken = Newtonsoft.Json.Linq.JObject.Parse(ResponseBodyString);
                                    }
                                    response.ResponseJson = ResponseBodyBlob; // = ResponseBodyBlob.Substring(ResponseBodyString.Length);
                                    response.ResponseJsonFormatted = Newtonsoft.Json.JsonConvert.SerializeObject(ResponseBodyToken, new Newtonsoft.Json.JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                                    if (Options.options.debugLevel >= 4)
                                        Log.WriteLine(response.ResponseJsonFormatted);
                                    //Newtonsoft.Json.Linq.JToken tokResult = ProcessResponse(ResponseBodyToken);
                                    if (response.ResponseJson == null) // todo: not right for Parse
                                        Log.WriteLine("Fail! - Response is null");
                                    else
                                    {
                                        if (response.ResponseJson[0] != '{')
                                            throw new FormatException("Invalid JSON response: not an object:" + response.ResponseJson);
                                        response.ResponseBodyToken = Newtonsoft.Json.Linq.JObject.Parse(response.ResponseJson);
                                    }
                                }
                            }
                        }
                        else
                        {
                            response.ResponseResult = rm.ReasonPhrase;
                            Log.WriteLine("GetAsync Failed: StatusCode:" + rm.ReasonPhrase + "(" + response.StatusCode.ToString() + ")");
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

        public async System.Threading.Tasks.Task<ServiceResponse> PostAsyncSystemNet(Uri uri, byte[] audioBytes, int sampleRate)
        {
            ServiceResponse response = new ServiceResponse(this.ToString());
            try
            {
                // Using HttpClient to grab chunked encoding (partial) responses.
                using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    string base64AudioBytes = Convert.ToBase64String(audioBytes).Replace('+', '-').Replace('/', '_');
                    string requestJson = "{'initialRequest':{'encoding':'LINEAR16','sampleRate':16000},'audioRequest':{'content':'" + base64AudioBytes + "'}}";
                    System.Net.Http.StringContent requestContent = new System.Net.Http.StringContent(requestJson);
#if false
                    requestContent.Headers.Add("Content-Type", "application/json"); // must add header AFTER contents are initialized
#else
                    requestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"); // must add header AFTER contents are initialized
#endif
                    if (Options.options.APIs.preferChunkedEncodedRequests)
                    {
                        Log.WriteLine("Using chunked encoding");
                        httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
                        if (httpClient.DefaultRequestHeaders.TransferEncodingChunked.Value)
                            Log.WriteLine("attempt to add chunked to header failed. Ignoring chunked.");
                        else
                            requestContent.Headers.ContentLength = 0;
                    }

#if false // todo: unfinished. Need to implement ContentType, Accept, Method, Headers
// curl --header "Content-Type: application/json" --data-binary "@{file}" "https://speech.googleapis.com/v1/speech:recognize?key={key}"
                    Log.WriteLine("curl -X POST --data \"" + text + "\" --header ""...""" + "\"" + uri);
#endif

                    Log.WriteLine("Before post: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                    response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                    using (System.Net.Http.HttpResponseMessage rm = await httpClient.PostAsync(uri, requestContent))
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
                                    string ResponseBodyBlob = string.Empty;
                                    // Google Cloud partial results are incomplete json strings while Google Web's are complete json of initial results
                                    while (!r.EndOfStream) // todo: is this best way of handling chunked? Seems too syncronous.
                                    {
                                        ResponseBodyBlob += r.ReadLine();
                                        if (Options.options.debugLevel >= 4)
                                            Log.WriteLine("ResponseBodyBlob:" + ResponseBodyBlob);
                                    }
                                    string ResponseBodyString = string.Empty;
                                    Newtonsoft.Json.Linq.JToken ResponseBodyToken = null;
                                    try
                                    {
                                        ResponseBodyToken = Newtonsoft.Json.Linq.JObject.Parse(ResponseBodyBlob);
                                        ResponseBodyString = ResponseBodyBlob;
                                    }
                                    // todo: obsolete?
                                    catch (Newtonsoft.Json.JsonReaderException ex) when (ex.HResult == -2146233088)
                                    {
                                        Log.WriteLine(ex.Message);
                                        ResponseBodyString = ResponseBodyBlob.Substring(0, ex.LinePosition);
                                        ResponseBodyToken = Newtonsoft.Json.Linq.JObject.Parse(ResponseBodyString);
                                    }
                                    response.ResponseJson = ResponseBodyBlob = ResponseBodyBlob.Substring(ResponseBodyString.Length);
                                    response.ResponseJsonFormatted = Newtonsoft.Json.JsonConvert.SerializeObject(ResponseBodyToken, new Newtonsoft.Json.JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                                    if (Options.options.debugLevel >= 4)
                                        Log.WriteLine(response.ResponseJsonFormatted);
                                    Newtonsoft.Json.Linq.JToken tokResult = ProcessResponse(ResponseBodyToken);
                                    if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
                                    {
                                        response.ResponseResult = Options.services["GoogleCloudSpeechToTextService"].response.missingResponse;
                                        if (Options.options.debugLevel >= 3)
                                            Log.WriteLine(response.ResponseResult);
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
                        else
                        {
                            response.ResponseResult = rm.ReasonPhrase;
                            Log.WriteLine("PostAsync Failed: StatusCode:" + rm.ReasonPhrase + "(" + response.StatusCode.ToString() + ")");
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

        private static Newtonsoft.Json.Linq.JToken ProcessResponse(Newtonsoft.Json.Linq.JToken response)
        {
            Newtonsoft.Json.Linq.JToken tok = null;
            if (response == null)
                Log.WriteLine("Fail! - Response is null");
            else
            {
                if ((response.SelectToken("responses")) == null)
                    Log.WriteLine("Fail! - result is null");
                else
                {
                    foreach (Newtonsoft.Json.Linq.JToken res in response.SelectToken("responses"))
                    {
                        foreach (Newtonsoft.Json.Linq.JToken result in res.SelectToken("results"))
                        {
                            foreach (Newtonsoft.Json.Linq.JToken a in result.SelectToken("alternatives"))
                            {
                                if ((tok = a.SelectToken("transcript")) != null)
                                    break;
                            }
                            if (tok != null)
                                break;
                        }
                    }
                }
            }
            return tok;
        }
    }
}
