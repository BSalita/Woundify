// Deprecated service? The sole API, Speech-to-Text, appears to be deprecated in favor or Google Cloud Speech-to-Text.

using System;

using System.Runtime.InteropServices.WindowsRuntime; // AsBuffer()
using System.Threading.Tasks;

namespace WoundifyShared
{
    class GoogleServices : WoundifyServices
    {
        public GoogleServices(Settings.Service service) : base(service)
        {
        }

        public override async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();
            Log.WriteLine("audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);

            System.Collections.Generic.List<Tuple<string, string>> uriSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("{language}", Options.options.locale.language ),
                new Tuple<string, string>("{key}", service.request.headers[1].BearerAuthentication.key) // todo: replace [1] with dictionary lookup
            };
            response.sr = await PostAsync(service, uriSubstitutes, null, null, audioBytes);
            await ExtractResultAsync(service, response.sr);
            return response;
        }
    }
}