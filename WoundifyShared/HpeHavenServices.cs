using System;
using System.Linq;

using System.Runtime.InteropServices.WindowsRuntime; // AsBuffer()

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WoundifyShared
{
    class HpeHavenServices : WoundifyServices
    {
        public HpeHavenServices(Settings.Service service) : base(service)
        {
        }

        public override async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();
            Log.WriteLine("audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);

            response.sr = await PostAsync(service, null, null, null, audioBytes);
            string JobID = response.sr.ResponseJToken.SelectToken(".jobID").ToString();
            Dictionary<string, string> dict = Helpers.stringToDictionary(service.request.data.value, '&', '=');
            string ApiKey = dict["apikey"];
            // TODO: move url to settings file
            string JobUrl = $"https://api.havenondemand.com/1/job/result/{JobID}?apikey={ApiKey}"; // using C# 6.0 string interpolation
            response.sr = await GetAsync(service, new Uri(JobUrl), null, null);
            await ExtractResultAsync(service, response.sr);
            return response;
        }
    }
}
