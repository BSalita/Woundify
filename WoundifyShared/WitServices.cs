using System;

namespace WoundifyShared
{
    class WitServices : WoundifyServices
    {
        public WitServices(Settings.Service service) : base(service)
        {
        }

        public override async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            System.Collections.Generic.List<Tuple<string, string>> Headers = new System.Collections.Generic.List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("Authorization", "Bearer " + service.request.headers[0].BearerAuthentication.bearer),
                    new Tuple<string, string>("Content-Type", service.request.headers[1].ContentType), // ;bits=16;rate=" + sampleRate.ToString()); // 403 if wrong
            };
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();
            response = await SpeechToTextServiceAsync(audioBytes, sampleRate, Headers);
            await ExtractResultAsync(service, response.sr);
            return response;
        }
    }
}
