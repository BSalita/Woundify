using System;

using System.Runtime.InteropServices.WindowsRuntime; // AsBuffer()
using System.Threading.Tasks;

namespace WoundifyShared
{
    class GoogleCloudServices : WoundifyServices
    {
        public GoogleCloudServices(Settings.Service service) : base(service)
        {
        }

        public override async System.Threading.Tasks.Task<AnnotateServiceResponse> AnnotateServiceAsync(string text)
        {
            AnnotateServiceResponse response = new AnnotateServiceResponse();
            Log.WriteLine("text:" + text);

            System.Collections.Generic.List<Tuple<string, string>> uriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("{key}", service.request.headers[0].BearerAuthentication.key) // todo: replace [1] with dictionary lookup
            };
            System.Collections.Generic.List<Tuple<string, string>> headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", service.request.headers[1].ContentType), // todo: need dictionary lookup instead of hardcoding
            };
            response.sr = await PostAsync(service, uriSubstitutes, headers, null, text);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

        public override async System.Threading.Tasks.Task<EntitiesServiceResponse> EntitiesServiceAsync(string text)
        {
            EntitiesServiceResponse response = new EntitiesServiceResponse();
            Log.WriteLine("text:" + text);

            System.Collections.Generic.List<Tuple<string, string>> uriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("{key}", service.request.headers[0].BearerAuthentication.key) // todo: replace [1] with dictionary lookup
            };
            System.Collections.Generic.List<Tuple<string, string>> headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", service.request.headers[1].ContentType), // todo: need dictionary lookup instead of hardcoding
            };
            response.sr = await PostAsync(service, uriSubstitutes, headers, null, text);
            await ExtractResultAsync(service, response.sr); // TODO: not sure which JSON should be result
            return response;
        }

        public override async System.Threading.Tasks.Task<IdentifyLanguageServiceResponse> IdentifyLanguageServiceAsync(string text)
        {
            IdentifyLanguageServiceResponse response = new IdentifyLanguageServiceResponse();
            Log.WriteLine("text:" + text);

            System.Collections.Generic.List<Tuple<string, string>> uriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("{key}", service.request.headers[0].BearerAuthentication.key) // todo: replace [1] with dictionary lookup
            };
            System.Collections.Generic.List<Tuple<string, string>> headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", service.request.headers[1].ContentType), // todo: need dictionary lookup instead of hardcoding
            };
            response.sr = await PostAsync(service, null, uriSubstitutes, headers, text);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

        public override async System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(string text)
        {
            ParseServiceResponse response = new ParseServiceResponse();
            Log.WriteLine("text:" + text);

            System.Collections.Generic.List<Tuple<string, string>> uriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("{key}", service.request.headers[0].BearerAuthentication.key) // todo: replace [1] with dictionary lookup
            };
            System.Collections.Generic.List<Tuple<string, string>> headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", service.request.headers[1].ContentType), // todo: need dictionary lookup instead of hardcoding
            };
            response.sr = await PostAsync(service, uriSubstitutes, headers, null, text);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

        public override async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();
            Log.WriteLine("audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);

            System.Collections.Generic.List<Tuple<string, string>> uriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("{key}", service.request.headers[1].BearerAuthentication.key) // todo: replace [1] with dictionary lookup
            };
            System.Collections.Generic.List<Tuple<string, string>> headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", service.request.headers[0].ContentType), // ;bits=16;rate=" + sampleRate.ToString()); // 403 if wrong
            };
            response.sr = await PostAsync(service, uriSubstitutes, headers, null, audioBytes);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

        public override async System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(string text)
        {
            ToneServiceResponse response = new ToneServiceResponse();
            Log.WriteLine("text:" + text);

            System.Collections.Generic.List<Tuple<string, string>> uriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("{key}", service.request.headers[0].BearerAuthentication.key) // todo: replace [1] with dictionary lookup
            };
            System.Collections.Generic.List<Tuple<string, string>> headers = new System.Collections.Generic.List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("Content-Type", service.request.headers[1].ContentType), // todo: need dictionary lookup instead of hardcoding
                };
            response.sr = await PostAsync(service, uriSubstitutes, headers, null, text);
            await ExtractResultAsync(service, response.sr); // TODO: not sure which JSON should be result
            return response;
        }

        public override async System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(string text, string source, string target)
        {
            TranslateServiceResponse response = new TranslateServiceResponse();
            Log.WriteLine("text:" + text);

            System.Collections.Generic.List<Tuple<string, string>> UriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("{key}", service.request.headers[0].BearerAuthentication.key),
                    new Tuple<string, string>("{text}", System.Uri.EscapeDataString(text.Trim())),
                    new Tuple<string, string>("{source}", source),
                    new Tuple<string, string>("{target}", target) // todo: replace [1] with dictionary lookup
                };
            response.sr = await GetAsync(service, UriSubstitutes, null);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

    }
}
