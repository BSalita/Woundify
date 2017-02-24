using System;
using System.Linq;

using System.Runtime.InteropServices.WindowsRuntime; // AsBuffer()

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WoundifyShared
{
    class HpeHavenSpeechToTextServices : GenericCallServices // needed to deal with async JobID stuff
    {
        public HpeHavenSpeechToTextServices(Settings.Service service) : base(service)
        {
        }
        public override async System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>> CallServiceAsync(byte[] audioBytes, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            int sampleRate = int.Parse(apiArgs["sampleRate"]);
            Log.WriteLine("audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);
            CallServiceResponse<IGenericServiceResponse> response = new CallServiceResponse<IGenericServiceResponse>(service);
            response.Request = Array.Find(service.requests, p => p.argType == "binary");

            await HttpMethods.CallApiAsync(response, null, null, null, audioBytes, apiArgs);
            string JobID = response.ResponseJToken.SelectToken(".jobID").ToString();
            Dictionary<string, string> dict = Helpers.stringToDictionary(service.requests[0].data.value, '&', '=');
            string ApiKey = dict["apikey"];
            // TODO: move url to settings file
            string JobUrl = $"https://api.havenondemand.com/1/job/result/{JobID}?apikey={ApiKey}"; // using C# 6.0 string interpolation
            await HttpMethods.CallApiAuthAsync(response, new Uri(JobUrl), "", null);
            await HttpMethods.ExtractResultAsync(response);
            return response;
        }
    }
}
