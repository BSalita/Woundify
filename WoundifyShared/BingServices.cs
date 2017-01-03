using System;
using Newtonsoft.Json.Linq;
using System.Web;

namespace WoundifyShared
{
    class BingServices : WoundifyServices
    {
        public BingServices(Settings.Service service) : base(service)
        {
        }

        public override async System.Threading.Tasks.Task<IdentifyLanguageServiceResponse> IdentifyLanguageServiceAsync(string text)
        {
            IdentifyLanguageServiceResponse response = new IdentifyLanguageServiceResponse();
            Log.WriteLine("Bing: Identify language:" + text);
            System.Collections.Generic.List<Tuple<string, string>> uriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("{text}", System.Uri.EscapeDataString(text)),
                new Tuple<string, string>("{guid}", Guid.NewGuid().ToString()) // TODO: replace [1] with dictionary lookup
            };
            System.Collections.Generic.List<Tuple<string, string>> headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", service.request.headers[0].ContentType), // TODO: need dictionary lookup instead of hardcoding
                new Tuple<string, string>("Ocp-Apim-Subscription-Key", service.request.headers[1].OcpApimSubscriptionKey) // TODO: need dictionary lookup instead of hardcoding
            };
            response.sr = await PostAsync(service, uriSubstitutes, headers, null, text);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

        // Paraphrase API has been withdrawn.

        private static System.Collections.Generic.List<string> ParseAnalysers; // Parse service only
        private static string ParseAnalyzerStringized; // Parse service only

        private async System.Threading.Tasks.Task<ParseAnalyzersServiceResponse> ParseAnalyzersServiceAsync()
        {
            ParseAnalyzersServiceResponse response = new ParseAnalyzersServiceResponse();
            Log.WriteLine("Bing: ParseAnalyzersServiceAsync");

            ParseAnalysers = new System.Collections.Generic.List<string>();
            System.Collections.Generic.List<Tuple<string, string>> Headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Ocp-Apim-Subscription-Key", service.request.headers[0].OcpApimSubscriptionKey) // same as Parse
            };
            response.sr = await GetAsync(service, null, Headers);
#if false // TODO: implement ExtractResultAsync?
            await ExtractResultAsync(service, response.sr);
#else
            foreach (JToken tokAnalyzerResult in response.sr.ResponseJToken)
            {
                ParseAnalysers.Add(tokAnalyzerResult["id"].ToString());
                ParseAnalyzerStringized += "'" + tokAnalyzerResult["id"].ToString() + "', ";
            }
            ParseAnalyzerStringized = ParseAnalyzerStringized.Substring(0, ParseAnalyzerStringized.Length - 2); // remove trailing "', "
#endif
            return response;
        }

        public override async System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(string text)
        {
            if (ParseAnalysers == null) // calling ParseAnalyzersService. It's a dependency of ParseService.
            {
                ParseAnalyzersServiceResponse responseAnalyzers = new ParseAnalyzersServiceResponse();
                BingServices parseAnalyersService = new BingServices(Options.services["BingParseAnalyzersService"].service);
                responseAnalyzers = await parseAnalyersService.ParseAnalyzersServiceAsync();
                if (ParseAnalysers == null || ParseAnalysers.Count == 0 || string.IsNullOrWhiteSpace(ParseAnalyzerStringized))
                    throw new InvalidOperationException(); // can't continue without at least one Analyzer
            }
            ParseServiceResponse response = new ParseServiceResponse();
            Log.WriteLine("Bing: Parse:" + text);
            System.Collections.Generic.List<Tuple<string, string>> jsonSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("{AnalyzerStringized}", ParseAnalyzerStringized),
            };
            System.Collections.Generic.List<Tuple<string, string>> headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Accept", service.request.headers[0].Accept), // TODO: need dictionary lookup instead of hardcoding
                new Tuple<string, string>("Content-Type", service.request.headers[1].ContentType), // TODO: need dictionary lookup instead of hardcoding
                new Tuple<string, string>("Ocp-Apim-Subscription-Key", service.request.headers[2].OcpApimSubscriptionKey) // TODO: need dictionary lookup instead of hardcoding
            };
            response.sr = await PostAsync(service, null, headers, jsonSubstitutes, text);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

        public override async System.Threading.Tasks.Task<SpellServiceResponse> SpellServiceAsync(string text)
        {
            SpellServiceResponse response = new SpellServiceResponse();
            Log.WriteLine("Bing: Spell:" + text);
            System.Collections.Generic.List<Tuple<string, string>> headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", service.request.headers[0].ContentType), // TODO: need dictionary lookup instead of hardcoding
                new Tuple<string, string>("Ocp-Apim-Subscription-Key", service.request.headers[1].OcpApimSubscriptionKey) // TODO: need dictionary lookup instead of hardcoding
            };
            response.sr = await PostAsync(service, null, headers, null, text);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

        private Authentication speechToTextAuth = null; // = sr;
        public override async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();

            // TODO: put into OAuthHelpers.cs?
            Settings.BearerAuthentication BearerAuth = service.request.headers[1].BearerAuthentication;
            ServiceResponse sr = new ServiceResponse(this.ToString());
            Uri accessUri = new Uri(BearerAuth.uri);
            System.Collections.Generic.List<Tuple<string, string>> header = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", "application/x-www-form-urlencoded"),
                new Tuple<string, string>("Ocp-Apim-Subscription-Key", service.request.headers[1].OcpApimSubscriptionKey) // TODO: need dictionary lookup instead of hardcoding
            };
            sr = await PostAsync(accessUri, "", header);
            string BearerToken = sr.ResponseString;

            try
            {
                Log.WriteLine("Bearer: {0}\n" + BearerToken);

                System.Collections.Generic.List<Tuple<string, string>> UriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("{locale}", Options.options.locale.language),
                    new Tuple<string, string>("{guid}", Guid.NewGuid().ToString()) // TODO: replace [1] with dictionary lookup
                };
                System.Collections.Generic.List<Tuple<string, string>> headers = new System.Collections.Generic.List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("Accept", service.request.headers[0].Accept),
                    new Tuple<string, string>("Authorization", "Bearer " + BearerToken),
                    new Tuple<string, string>("Content-Type", service.request.headers[2].ContentType.Replace("{sampleRate}",sampleRate.ToString())), // 403 if wrong
                };
                response.sr = await PostAsync(service, UriSubstitutes, headers, null, audioBytes);
                await ExtractResultAsync(service, response.sr);
            }
            catch (Exception ex)
            {
                Log.WriteLine("Bing.SpeechToText: Exception:" + ex.Message);
                if (ex.InnerException != null)
                    Log.WriteLine("InnerException:" + ex.InnerException);
            }
            return response;
        }

        public override async System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(string text)
        {
            ToneServiceResponse response = new ToneServiceResponse();
            Log.WriteLine("Bing: Sentiment:" + text);
            System.Collections.Generic.List<Tuple<string, string>> Headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", service.request.headers[0].ContentType), // TODO: need dictionary lookup instead of hardcoding
                new Tuple<string, string>("Ocp-Apim-Subscription-Key", service.request.headers[1].OcpApimSubscriptionKey) // TODO: need dictionary lookup instead of hardcoding
            };
            // TODO: maybe a dictionary of headers should be passed including content-type. Then PostAsync can do if (dict.Contains("Content-Type")) headers.add(dict...)
            // TODO: maybe should init a header class, add values and pass to Post?
            response.sr = await PostAsync(service, null, Headers, null, text);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

        private Authentication translateAuth = null; // = sr;
        public override async System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(string text, string source, string target)
        {
            TranslateServiceResponse response = new TranslateServiceResponse();
            Log.WriteLine("Bing: Translate:" + text);

            if (translateAuth == null) // Must obtain an initial Bearer token and set timer for subsequent refreshes
            {
                translateAuth = new Authentication();
                await translateAuth.PerformAuthenticationAsync(service.request.headers[0].BearerAuthentication);
            }

            AccessTokenInfo token = translateAuth.GetAccessToken();
            if (token == null)
            {
                Log.WriteLine("Invalid authentication token");
                return response;
            }
            Log.WriteLine("Token: {0}\n" + token.access_token);


            System.Collections.Generic.List<Tuple<string, string>> UriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
                    {
                        new Tuple<string, string>("{text}", System.Uri.EscapeDataString(text.Trim())),
                        new Tuple<string, string>("{source}", source),
                        new Tuple<string, string>("{target}", target),
                    };
            System.Collections.Generic.List<Tuple<string, string>> headers = new System.Collections.Generic.List<Tuple<string, string>>()
                    {
                        new Tuple<string, string>("Authorization", "Bearer " + token.access_token),
                        //new Tuple<string, string>("Accept", "application/json"), // still returns XML - How to get Json?
                        //new Tuple<string, string>("Content-Type", "text/plain") // still returns XML - How to get Json?
                    };

            response.sr = await GetAsync(service, UriSubstitutes, headers);
            System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
            xd.LoadXml(response.sr.ResponseString);
            response.sr.ResponseResult = xd["string"].InnerText;
            return response;
        }
    }
}
