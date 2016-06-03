using System;
using System.Collections.Generic;
using System.Text;

namespace WoundifyShared
{
    public class HttpMethods
    {
        private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

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

        public async System.Threading.Tasks.Task<ServiceResponse> PostSystemNet(Uri uri, List<Tuple<string, string>> DefaultRequestHeaders, System.Net.Http.HttpContent requestContent, bool binaryOnly = false)
        {
            ServiceResponse response = new ServiceResponse(this.ToString());
            using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
            {
                foreach (Tuple<string, string> t in DefaultRequestHeaders)
                {
#if true
                    httpClient.DefaultRequestHeaders.Add(t.Item1, t.Item2);
#else
                    switch (t.Item1)
                    {
                        case "Accept":
                            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            break;
                        case "BasicAuthentication":
                            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", t.Item2);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
#endif
                }
                // Using HttpClient to grab chunked encoding (partial) responses.
                if (Options.options.APIs.preferChunkedEncodedRequests)
                {
                    Log.WriteLine("Using chunked encoding");
                    httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
                    if (httpClient.DefaultRequestHeaders.TransferEncodingChunked.Value)
                        Log.WriteLine("attempt to add chunked to header failed. Ignoring chunked.");
                    else
                        requestContent.Headers.ContentLength = 0;
                }
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
                            if (binaryOnly)
                                await PostAsyncSystemNetBinaryReader(rr, response);
                            else
                                await PostAsyncSystemNetStreamReader(rr, response);
                        }
                    }
                    else
                    {
                        response.ResponseResult = rm.ReasonPhrase;
                        Log.WriteLine("PostAsync Failed: StatusCode:" + rm.ReasonPhrase + "(" + response.StatusCode.ToString() + ")");
                    }
                }
                stopWatch.Stop();
                response.TotalElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                Log.WriteLine("After response: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
            }
            return response;
        }

#if false
        public async System.Threading.Tasks.Task<ServiceResponse> PostSystemNet(Uri uri, System.Net.Http.Headers.AuthenticationHeaderValue authentication, System.Net.Http.StringContent requestContent)
        {
            using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = authentication;
                // Using HttpClient to grab chunked encoding (partial) responses.
                if (Options.options.APIs.preferChunkedEncodedRequests)
                {
                    Log.WriteLine("Using chunked encoding");
                    httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
                    if (httpClient.DefaultRequestHeaders.TransferEncodingChunked.Value)
                        Log.WriteLine("attempt to add chunked to header failed. Ignoring chunked.");
                    else
                        requestContent.Headers.ContentLength = 0;
                }
                Log.WriteLine("Before post: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
                ServiceResponse response = new ServiceResponse(this.ToString());
                response.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
                using (System.Net.Http.HttpResponseMessage rm = await httpClient.PostAsync(uri, requestContent))
                {
                    return await PostAsyncSystemNet(rm, response);
                }
            }
        }
#endif

        public async System.Threading.Tasks.Task PostAsyncSystemNetBinaryReader(System.IO.Stream rr, ServiceResponse response)
        {
            try
            {
                using (System.IO.BinaryReader r = new System.IO.BinaryReader(rr)) // if needed, there is a constructor which will leave the stream open
                {
                    response.ResponseBytes = r.ReadBytes(1000000);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Exception:" + ex.Message);
                if (ex.InnerException != null)
                    Log.WriteLine("InnerException:" + ex.InnerException);
            }
        }

        public async System.Threading.Tasks.Task PostAsyncSystemNetStreamReader(System.IO.Stream rr, ServiceResponse response)
        {
            try
            {
                using (System.IO.StreamReader r = new System.IO.StreamReader(rr)) // if needed, there is a constructor which will leave the stream open
                {
                    // Google Cloud partial results are incomplete json strings while Google Web's are complete json of initial results
                    string ResponseBodyBlob = await r.ReadToEndAsync();
                    if (Options.options.debugLevel >= 4)
                        Log.WriteLine("ResponseBodyBlob:" + ResponseBodyBlob);
                    response.ResponseBodyBlob = ResponseBodyBlob;
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
                    response.ResponseBodyToken = ResponseBodyToken;
                    response.ResponseJson = ResponseBodyBlob.Substring(ResponseBodyString.Length);
                    response.ResponseJsonFormatted = Newtonsoft.Json.JsonConvert.SerializeObject(ResponseBodyToken, new Newtonsoft.Json.JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });
                    if (Options.options.debugLevel >= 4)
                        Log.WriteLine(response.ResponseJsonFormatted);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Exception:" + ex.Message);
                if (ex.InnerException != null)
                    Log.WriteLine("InnerException:" + ex.InnerException);
            }
        }
    }
}
