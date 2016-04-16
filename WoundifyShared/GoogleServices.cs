﻿using System;

using System.Runtime.InteropServices.WindowsRuntime; // AsBuffer()
using System.Threading.Tasks;

namespace WoundifyShared
{
    class GoogleServices : WoundifyServices
    {
        private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        public override async System.Threading.Tasks.Task<ISpeechToTextServiceResponse> SpeechToTextAsync(byte[] audioBytes, int sampleRate)
        {
            ISpeechToTextServiceResponse response = new ISpeechToTextServiceResponse();
            Log.WriteLine("audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);

            stopWatch.Start();

            dynamic settings = Options.options.Services.APIs.SpeechToText.GoogleSpeechToText;

            UriBuilder ub = new UriBuilder();
            ub.Scheme = "https";
            ub.Host = "www.google.com";
            ub.Path = "speech-api/v2/recognize";
            Log.WriteLine("before key");
            ub.Query = "output=json&lang=" + Options.options.locale.language + "&key=" + settings.key;
            Log.WriteLine("after key" + ub.Query);
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

        public async System.Threading.Tasks.Task<IServiceResponse> PostAsyncSystemNet(Uri uri, byte[] audioBytes, int sampleRate)
        {
            IServiceResponse response = new IServiceResponse();
            try
            {
                // Using HttpClient to grab chunked encoding (partial) responses.
                // Using HttpClient to grab chunked encoding (partial) responses.
                using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    System.Net.Http.ByteArrayContent requestContent = new System.Net.Http.ByteArrayContent(audioBytes);
                    requestContent.Headers.Add("Content-Type", "audio/l16; rate=" + sampleRate.ToString()); // must add header AFTER contents are initialized
                    if (Options.options.Services.APIs.PreferChunkedEncodedRequests)
                    {
                        Log.WriteLine("Using chunked encoding");
                        if (requestContent.Headers.TryAddWithoutValidation("Transfer-Encoding", "chunked"))
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
                                using (System.IO.StreamReader r = new System.IO.StreamReader(rr)) // if needed, there is a constructor which will leave the stream open
                                {
                                    // attempts to display individual partial responses. However, some lame assumptions needed to do so.
                                    while (!r.EndOfStream)
                                    {
                                        string ResponseBodyBlob = r.ReadLine();
                                        if (Options.options.debugLevel >= 4)
                                            Log.WriteLine("ResponseBodyBlob:" + ResponseBodyBlob);
                                        string ResponseBodyString = null;
                                        Newtonsoft.Json.Linq.JToken ResponseBodyToken = null;
                                        while (ResponseBodyBlob != "")
                                        {
                                            try
                                            {
                                                ResponseBodyToken = Newtonsoft.Json.Linq.JObject.Parse(ResponseBodyBlob);
                                                ResponseBodyString = ResponseBodyBlob;
                                            }
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
                                                response.ResponseResult = Options.options.Services.APIs.SpeechToText.missingResponse;
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
                if ((response.SelectToken("$.result")) == null)
                    Log.WriteLine("Fail! - result is null");
                else
                {
                    foreach (Newtonsoft.Json.Linq.JToken r in response.SelectToken("$.result"))
                    {
                        foreach (Newtonsoft.Json.Linq.JToken a in r.SelectToken("$.alternative"))
                        {
                            if ((tok = a.SelectToken("$.transcript")) != null)
                                break;
                        }
                        if (tok != null)
                            break;
                    }
                }
            }
            return tok;
        }
    }
}
