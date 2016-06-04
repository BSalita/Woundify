using System;

namespace WoundifyShared
{
    public class SpeechToTextServices
    {
        public static System.Collections.Generic.List<ISpeechToTextService> PreferredOrderingSpeechToTextServices = new FindServices<ISpeechToTextService>(Options.commandservices["SpeechToText"].preferredServices).PreferredOrderingOfServices;
        public static System.Collections.Generic.List<SpeechToTextServiceResponse> responses = new System.Collections.Generic.List<SpeechToTextServiceResponse>();

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<SpeechToTextServiceResponse>> RunAllPreferredSpeechToTextServicesAsync(string fileName)
        {
            byte[] bytes = await Helpers.ReadBytesFromFileAsync(fileName);
            int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
            return RunAllPreferredSpeechToTextServicesRun(bytes, sampleRate);
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<SpeechToTextServiceResponse>> RunAllPreferredSpeechToTextServicesAsync(byte[] bytes, int sampleRate)
        {
            return RunAllPreferredSpeechToTextServicesRun(bytes,sampleRate);
        }

        public static System.Collections.Generic.List<SpeechToTextServiceResponse> RunAllPreferredSpeechToTextServicesRun(byte[] bytes, int sampleRate)
        {
            responses = new System.Collections.Generic.List<SpeechToTextServiceResponse>();
            // invoke each ISpeechToTextService and show what it can do.
            foreach (ISpeechToTextService STT in PreferredOrderingSpeechToTextServices)
            {
                System.Threading.Tasks.Task.Run(() => STT.SpeechToTextServiceAsync(bytes, sampleRate)).ContinueWith((c) =>
               {
                   ServiceResponse r = c.Result.sr;
                   if (string.IsNullOrEmpty(r.ResponseResult) || r.StatusCode != 200)
                       Console.WriteLine(r.ServiceName + " STT (async): Failed with StatusCode of " + r.StatusCode);
                   else
                       Console.WriteLine(r.ServiceName + " STT (async):\"" + r.ResponseResult + "\" Total " + r.TotalElapsedMilliseconds + "ms Request " + r.RequestElapsedMilliseconds + "ms");
                   responses.Add(c.Result);
               });
            }
            return responses;
        }

#if false
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ISpeechToTextServiceResponse>> RunAllPreferredSpeechToTextServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<ISpeechToTextServiceResponse> responses = new System.Collections.Generic.List<ISpeechToTextServiceResponse>();
            // invoke each ISpeechToTextService and show what it can do.
            foreach (ISpeechToTextService STT in PreferredISpeechToTextServices)
            {
                await STT.SpeechToTextAsync(bytes, sampleRate).ContinueWith<ISpeechToTextServiceResponse>((c) =>
                {
                    if (string.IsNullOrEmpty(c.Result.sr.ResponseResult) || c.Result.sr.StatusCode != 200)
                        Console.WriteLine(r.ServiceName + " STT (async): Failed with StatusCode of " + c.Result.StatusCode);
                    else
                        Console.WriteLine(r.ServiceName + " STT (async):\"" + c.Result.ResponseResult + "\" Total " + c.Result.TotalElapsedMilliseconds + "ms Request " + c.Result.RequestElapsedMilliseconds + "ms");
                    responses.Add(c.Result);
                });
            }
            return responses;
        }
#endif
    }
}
