using System;
using System.Collections.Generic;
using System.Text;

namespace WoundifyShared
{
    class IbmWatsonServices : WoundifyServices
    {
        public IbmWatsonServices(Settings.Service service) : base(service)
        {
        }

        public override async System.Threading.Tasks.Task<IdentifyLanguageServiceResponse> IdentifyLanguageServiceAsync(string text)
        {
            IdentifyLanguageServiceResponse response = new IdentifyLanguageServiceResponse();
            Log.WriteLine("IdentifyLanguageServiceAsync: text:" + text);
            response.sr = await PostAsync(service, text);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

        public override async System.Threading.Tasks.Task<PersonalityServiceResponse> PersonalityServiceAsync(string text)
        {
            PersonalityServiceResponse response = new PersonalityServiceResponse();
            Log.WriteLine("PersonalityServiceAsync: text:" + text);
            response.sr = await PostAsync(service, text);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

        public override async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();
            Log.WriteLine("SpeechToTextServiceAsync: audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);
            response.sr = await PostAsync(service, audioBytes);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

        public override async System.Threading.Tasks.Task<TextToSpeechServiceResponse> TextToSpeechServiceAsync(string text, int sampleRate)
        {
            TextToSpeechServiceResponse response = new TextToSpeechServiceResponse();
            Log.WriteLine("TextToSpeechServiceAsync: text:" + text);
            response.sr = await PostAsync(service, text);
            // result is byte[]
            return response;
        }

        public override async System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(string text)
        {
            ToneServiceResponse response = new ToneServiceResponse();
            Log.WriteLine("ToneServiceAsync: text:" + text);
            response.sr = await PostAsync(service, text);
            await ExtractResultAsync(service, response.sr);
            return response;
        }

        public override async System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(string text, string source, string target)
        {
            TranslateServiceResponse response = new TranslateServiceResponse();
            Log.WriteLine("TranslateServiceAsync: text:" + text);
            System.Collections.Generic.List<Tuple<string, string>> uriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("{source}", source),
                new Tuple<string, string>("{target}", target)
            };

            response.sr = await PostAsync(service, uriSubstitutes, null, null, text);
            await ExtractResultAsync(service, response.sr);
            return response;
        }
    }
}
