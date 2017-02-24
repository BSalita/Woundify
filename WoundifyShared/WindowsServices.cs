using System;

namespace WoundifyShared
{
    class WindowsPronounceService : GenericCallServices
    {
        public WindowsPronounceService(Settings.Service service) : base(service)
        {
        }
 
        public override async System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>> CallServiceAsync(string text, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            Log.WriteLine("text:" + text);
            CallServiceResponse<IGenericServiceResponse> response = new CallServiceResponse<IGenericServiceResponse>(service);

            response.Service = service;
            response.Request = Array.Find(service.requests, p => p.argType == "text");
            response.stopWatch.Start();
            response.ResponseResult = await TextToSpeech.TextToSpelledPronunciationServiceAsync(text, apiArgs);
            response.StatusCode = 200;
            response.stopWatch.Stop();
            response.TotalElapsedMilliseconds = response.RequestElapsedMilliseconds = response.stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total Elapsed milliseconds:" + response.TotalElapsedMilliseconds);
            return response;
        }
    }

    class WindowsSpeechToTextService : GenericCallServices
    {
        public WindowsSpeechToTextService(Settings.Service service) : base(service)
        {
        }
        public override async System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>> CallServiceAsync(byte[] audioBytes, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            Log.WriteLine("audio file length:" + audioBytes.Length);
            CallServiceResponse<IGenericServiceResponse> response = new CallServiceResponse<IGenericServiceResponse>(service);

            response.Service = service;
            response.Request = Array.Find(service.requests, p => p.argType == "binary");
            response.stopWatch.Start();
            response.ResponseResult = await SpeechToText.SpeechToTextServiceAsync(audioBytes, apiArgs);
            response.StatusCode = 200;
            response.stopWatch.Stop();
            response.TotalElapsedMilliseconds = response.RequestElapsedMilliseconds = response.stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total Elapsed milliseconds:" + response.TotalElapsedMilliseconds);
            return response;
        }
    }

    class WindowsTextToSpeechService : GenericCallServices
    {
        public WindowsTextToSpeechService(Settings.Service service) : base(service)
        {
        }
        public override async System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>> CallServiceAsync(string text, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            Log.WriteLine("text:" + text);
            CallServiceResponse<IGenericServiceResponse> response = new CallServiceResponse<IGenericServiceResponse>(service);

            response.Service = service;
            response.Request = Array.Find(service.requests, p => p.argType == "text");
            response.stopWatch.Start();
            response.ResponseBytes = await TextToSpeech.TextToSpeechServiceAsync(text, apiArgs);
            response.StatusCode = 200;
            response.stopWatch.Stop();
            response.TotalElapsedMilliseconds = response.RequestElapsedMilliseconds = response.stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total Elapsed milliseconds:" + response.TotalElapsedMilliseconds);
            return response;
        }
    }
}
