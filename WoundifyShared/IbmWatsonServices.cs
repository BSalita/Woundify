using System;
using System.Collections.Generic;
using System.Text;

namespace WoundifyShared
{
    class IbmWatsonServices : WoundifyServices
    {
        private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        public IbmWatsonServices(Settings.Service service) : base(service)
        {
        }

        public override async System.Threading.Tasks.Task<IdentifyLanguageServiceResponse> IdentifyLanguageServiceAsync(string text)
        {
            IdentifyLanguageServiceResponse response = new IdentifyLanguageServiceResponse();
            Log.WriteLine("IdentifyLanguageServiceAsync: text:" + text);

            stopWatch.Start();

            UriBuilder ub = new UriBuilder();
            ub.Scheme = service.request.uri.scheme;
            ub.Host = service.request.uri.host;
            ub.Path = service.request.uri.path;
            ub.Query = service.request.uri.query;
            System.Net.Http.HttpContent requestContent = null;
            List<Tuple<string, string>> DefaultRequestHeaders = new List<Tuple<string, string>>();
            switch (service.request.data.type)
            {
                case "string":
                    requestContent = new System.Net.Http.StringContent(text);
                    break;
                default:
                    throw new MissingFieldException();
            }
            foreach (Settings.Header h in service.request.headers)
            {
                switch (h.Name)
                {
                    case "Accept":
                        DefaultRequestHeaders.Add(new Tuple<string, string>("Accept", h.Accept));
                        break;
                    case "BasicAuthentication":
                        string userpass = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", h.BasicAuthentication.username, h.BasicAuthentication.password).Replace('+', '-').Replace('/', '_')));
                        //DefaultRequestHeaders.Add(new Tuple<string, string>("BasicAuthentication", userpass)); // if switch used
                        DefaultRequestHeaders.Add(new Tuple<string, string>("Authorization", "Basic " + userpass));
                        break;
                    case "Content-Type":
                        requestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(h.ContentType); // must add header AFTER contents are initialized
                        break;
                    default:
                        throw new MissingFieldException();
                }
            }
            HttpMethods http = new HttpMethods();
            response.sr = await http.PostSystemNet(ub.Uri, DefaultRequestHeaders, requestContent);
            Newtonsoft.Json.Linq.JToken tokResult = response.sr.ResponseBodyToken.SelectToken(service.response.jsonPath);
            if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
            {
                response.sr.ResponseResult = Options.services["IbmWatsonIdentifyLanguageService"].response.missingResponse;
                if (Options.options.debugLevel >= 3)
                    Log.WriteLine(response.sr.ResponseResult);
            }
            else
            {
                response.sr.ResponseResult = tokResult.ToString();
                if (Options.options.debugLevel >= 3)
                    Log.WriteLine(tokResult.Path + ": " + response.sr.ResponseResult);
            }
            return response;
        }

        public override async System.Threading.Tasks.Task<PersonalityServiceResponse> PersonalityServiceAsync(string text)
        {
            PersonalityServiceResponse response = new PersonalityServiceResponse();
            Log.WriteLine("PersonalityServiceAsync: text:" + text);

            stopWatch.Start();

            UriBuilder ub = new UriBuilder();
            ub.Scheme = service.request.uri.scheme;
            ub.Host = service.request.uri.host;
            ub.Path = service.request.uri.path;
            ub.Query = service.request.uri.query;
            System.Net.Http.HttpContent requestContent = null;
            List<Tuple<string, string>> DefaultRequestHeaders = new List<Tuple<string, string>>();
            switch (service.request.data.type)
            {
                case "string":
                    requestContent = new System.Net.Http.StringContent(text);
                    break;
                default:
                    throw new MissingFieldException();
            }
            foreach (Settings.Header h in service.request.headers)
            {
                switch (h.Name)
                {
                    case "Accept":
                        DefaultRequestHeaders.Add(new Tuple<string, string>("Accept", h.Accept));
                        break;
                    case "BasicAuthentication":
                        string userpass = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", h.BasicAuthentication.username, h.BasicAuthentication.password).Replace('+', '-').Replace('/', '_')));
                        //DefaultRequestHeaders.Add(new Tuple<string, string>("BasicAuthentication", userpass)); // if switch used
                        DefaultRequestHeaders.Add(new Tuple<string, string>("Authorization", "Basic " + userpass));
                        break;
                    case "Content-Type":
                        requestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(h.ContentType); // must add header AFTER contents are initialized
                        break;
                    default:
                        throw new MissingFieldException();
                }
            }
            HttpMethods http = new HttpMethods();
            response.sr = await http.PostSystemNet(ub.Uri, DefaultRequestHeaders, requestContent);
            Newtonsoft.Json.Linq.JToken tokResult = response.sr.ResponseBodyToken.SelectToken(service.response.jsonPath);
            if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
            {
                response.sr.ResponseResult = Options.services["IbmWatsonPersonalityService"].response.missingResponse;
                if (Options.options.debugLevel >= 3)
                    Log.WriteLine(response.sr.ResponseResult);
            }
            else
            {
                response.sr.ResponseResult = tokResult.ToString();
                if (Options.options.debugLevel >= 3)
                    Log.WriteLine(tokResult.Path + ": " + response.sr.ResponseResult);
            }
            stopWatch.Stop();
            response.sr.TotalElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            Log.WriteLine("PersonalityServiceAsync: Total: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
            return response;
        }

        public override async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();
            Log.WriteLine("SpeechToTextServiceAsync: audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);

            stopWatch.Start();

            UriBuilder ub = new UriBuilder();
            ub.Scheme = service.request.uri.scheme;
            ub.Host = service.request.uri.host;
            ub.Path = service.request.uri.path;
            ub.Query = service.request.uri.query;
            System.Net.Http.HttpContent requestContent = null;
            List<Tuple<string, string>> DefaultRequestHeaders = new List<Tuple<string, string>>();
            switch (service.request.data.type)
            {
                case "binary":
                    requestContent = new System.Net.Http.ByteArrayContent(audioBytes);
                    break;
                case "base64":
                    string base64AudioBytes = Convert.ToBase64String(audioBytes).Replace('+', '-').Replace('/', '_');
                    string requestJson = "{'initialRequest':{'encoding':'LINEAR16','sampleRate':16000},'audioRequest':{'content':'" + base64AudioBytes + "'}}";
                    requestContent = new System.Net.Http.StringContent(requestJson);
                    break;
                default:
                    throw new MissingFieldException();
            }
            foreach (Settings.Header h in service.request.headers)
            {
                switch (h.Name)
                {
                    case "Accept":
                        DefaultRequestHeaders.Add(new Tuple<string, string>("Accept", h.Accept));
                        break;
                    case "BasicAuthentication":
                        string userpass = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", h.BasicAuthentication.username, h.BasicAuthentication.password).Replace('+', '-').Replace('/', '_')));
                        //DefaultRequestHeaders.Add(new Tuple<string, string>("BasicAuthentication", userpass)); // if switch used
                        DefaultRequestHeaders.Add(new Tuple<string, string>("Authorization", "Basic " + userpass));
                        break;
                    case "Content-Type":
                        requestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(h.ContentType); // must add header AFTER contents are initialized
                        break;
                    default:
                        throw new MissingFieldException();
                }
            }
            HttpMethods http = new HttpMethods();
            response.sr = await http.PostSystemNet(ub.Uri, DefaultRequestHeaders, requestContent);
            Newtonsoft.Json.Linq.JToken tokResult = ProcessResponse(response.sr.ResponseBodyToken);
            if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
            {
                response.sr.ResponseResult = Options.services["IbmWatsonSpeechToTextService"].response.missingResponse;
                if (Options.options.debugLevel >= 3)
                    Log.WriteLine(response.sr.ResponseResult);
            }
            else
            {
                response.sr.ResponseResult = tokResult.ToString();
                if (Options.options.debugLevel >= 3)
                    Log.WriteLine(tokResult.Path + ": " + response.sr.ResponseResult);
            }
            stopWatch.Stop();
            response.sr.TotalElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            Log.WriteLine("SpeechToTextServiceAsync: Total: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
            return response;
        }

        private static Newtonsoft.Json.Linq.JToken ProcessResponse(Newtonsoft.Json.Linq.JToken response)
        {
            Newtonsoft.Json.Linq.JToken tok = null;
            if (response == null)
                Log.WriteLine("Fail! - Response is null");
            else
            {
                if ((response.SelectToken("results")) == null)
                    Log.WriteLine("Fail! - result is null");
                else
                {
                    foreach (Newtonsoft.Json.Linq.JToken result in response.SelectToken("results"))
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
            return tok;
        }

        public override async System.Threading.Tasks.Task<TextToSpeechServiceResponse> TextToSpeechServiceAsync(string text, int sampleRate)
        {
            TextToSpeechServiceResponse response = new TextToSpeechServiceResponse();
            Log.WriteLine("TextToSpeechServiceAsync: text:" + text);

            stopWatch.Start();

            UriBuilder ub = new UriBuilder();
            ub.Scheme = service.request.uri.scheme;
            ub.Host = service.request.uri.host;
            ub.Path = service.request.uri.path;
            ub.Query = service.request.uri.query;
            System.Net.Http.HttpContent requestContent = null;
            List<Tuple<string, string>> DefaultRequestHeaders = new List<Tuple<string, string>>();
            switch (service.request.data.type)
            {
                case "string":
                    string jsonText = service.request.data.value;
                    jsonText = jsonText.Replace("{text}", text);
                    requestContent = new System.Net.Http.StringContent(jsonText);
                    break;
                default:
                    throw new MissingFieldException();
            }
            foreach (Settings.Header h in service.request.headers)
            {
                switch (h.Name)
                {
                    case "Accept":
                        DefaultRequestHeaders.Add(new Tuple<string, string>("Accept", h.Accept));
                        break;
                    case "BasicAuthentication":
                        string userpass = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", h.BasicAuthentication.username, h.BasicAuthentication.password).Replace('+', '-').Replace('/', '_')));
                        //DefaultRequestHeaders.Add(new Tuple<string, string>("BasicAuthentication", userpass)); // if switch used
                        DefaultRequestHeaders.Add(new Tuple<string, string>("Authorization", "Basic " + userpass));
                        break;
                    case "Content-Type":
                        requestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(h.ContentType); // must add header AFTER contents are initialized
                        break;
                    default:
                        throw new MissingFieldException();
                }
            }
            HttpMethods http = new HttpMethods();
            response.sr = await http.PostSystemNet(ub.Uri, DefaultRequestHeaders, requestContent, true);

            return response;
        }

        public override async System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(string text)
        {
            ToneServiceResponse response = new ToneServiceResponse();
            Log.WriteLine("ToneServiceAsync: text:" + text);

            stopWatch.Start();

            UriBuilder ub = new UriBuilder();
            ub.Scheme = service.request.uri.scheme;
            ub.Host = service.request.uri.host;
            ub.Path = service.request.uri.path;
            ub.Query = service.request.uri.query;
            System.Net.Http.HttpContent requestContent = null;
            List<Tuple<string, string>> DefaultRequestHeaders = new List<Tuple<string, string>>();
            switch (service.request.data.type)
            {
                case "string":
                    string jsonText = service.request.data.value;
                    jsonText = jsonText.Replace("{text}", text);
                    requestContent = new System.Net.Http.StringContent(jsonText);
                    break;
                default:
                    throw new MissingFieldException();
            }
            foreach (Settings.Header h in service.request.headers)
            {
                switch (h.Name)
                {
                    case "Accept":
                        DefaultRequestHeaders.Add(new Tuple<string, string>("Accept", h.Accept));
                        break;
                    case "BasicAuthentication":
                        string userpass = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", h.BasicAuthentication.username, h.BasicAuthentication.password).Replace('+', '-').Replace('/', '_')));
                        //DefaultRequestHeaders.Add(new Tuple<string, string>("BasicAuthentication", userpass)); // if switch used
                        DefaultRequestHeaders.Add(new Tuple<string, string>("Authorization", "Basic " + userpass));
                        break;
                    case "Content-Type":
                        requestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(h.ContentType); // must add header AFTER contents are initialized
                        break;
                    default:
                        throw new MissingFieldException();
                }
            }
            HttpMethods http = new HttpMethods();
            response.sr = await http.PostSystemNet(ub.Uri, DefaultRequestHeaders, requestContent);
            Newtonsoft.Json.Linq.JToken tokResult = response.sr.ResponseBodyToken.SelectToken(service.response.jsonPath);
            if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
            {
                response.sr.ResponseResult = Options.services["IbmWatsonToneService"].response.missingResponse;
                if (Options.options.debugLevel >= 3)
                    Log.WriteLine(response.sr.ResponseResult);
            }
            else
            {
                response.sr.ResponseResult = tokResult.ToString();
                if (Options.options.debugLevel >= 3)
                    Log.WriteLine(tokResult.Path + ": " + response.sr.ResponseResult);
            }
            return response;
        }

        public override async System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(string text)
        {
            TranslateServiceResponse response = new TranslateServiceResponse();
            Log.WriteLine("TranslateServiceAsync: text:" + text);

            stopWatch.Start();

            UriBuilder ub = new UriBuilder();
            ub.Scheme = service.request.uri.scheme;
            ub.Host = service.request.uri.host;
            ub.Path = service.request.uri.path;
            ub.Query = service.request.uri.query;
            System.Net.Http.HttpContent requestContent = null;
            List<Tuple<string, string>> DefaultRequestHeaders = new List<Tuple<string, string>>();
            switch (service.request.data.type)
            {
                case "string":
                    string jsonText = service.request.data.value;
                    jsonText = jsonText.Replace("{text}", text);
                    requestContent = new System.Net.Http.StringContent(jsonText);
                    break;
                default:
                    throw new MissingFieldException();
            }
            foreach (Settings.Header h in service.request.headers)
            {
                switch (h.Name)
                {
                    case "Accept":
                        DefaultRequestHeaders.Add(new Tuple<string, string>("Accept", h.Accept));
                        break;
                    case "BasicAuthentication":
                        string userpass = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", h.BasicAuthentication.username, h.BasicAuthentication.password).Replace('+', '-').Replace('/', '_')));
                        //DefaultRequestHeaders.Add(new Tuple<string, string>("BasicAuthentication", userpass)); // if switch used
                        DefaultRequestHeaders.Add(new Tuple<string, string>("Authorization", "Basic " + userpass));
                        break;
                    case "Content-Type":
                        requestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(h.ContentType); // must add header AFTER contents are initialized
                        break;
                    default:
                        throw new MissingFieldException();
                }
            }
            HttpMethods http = new HttpMethods();
            response.sr = await http.PostSystemNet(ub.Uri, DefaultRequestHeaders, requestContent);
#if false
            response.sr.ResponseResult = response.sr.ResponseBodyBlob; // text/plain return
#else
            Newtonsoft.Json.Linq.JToken tokResult = response.sr.ResponseBodyToken.SelectToken(service.response.jsonPath);
            if (tokResult == null || string.IsNullOrEmpty(tokResult.ToString()))
            {
                response.sr.ResponseResult = Options.services["IbmWatsonTranslateService"].response.missingResponse;
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
            return response;
        }
    }
}
