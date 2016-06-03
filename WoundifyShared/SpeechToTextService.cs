using System;

namespace WoundifyShared
{
    public class SpeechToTextServices
    {
        public static System.Collections.Generic.List<ISpeechToTextService> PreferredOrderedISpeechToTextServices = new FindServices<ISpeechToTextService>(Options.commandservices["SpeechToText"].preferredServices).PreferredOrderingOfServices;

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<SpeechToTextServiceResponse>> RunAllPreferredSpeechToTextServices(string fileName)
        {
            byte[] bytes = await Helpers.ReadBytesFromFileAsync(fileName);
            int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
            return await RunAllPreferredSpeechToTextServices(bytes, sampleRate);
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<SpeechToTextServiceResponse>> RunAllPreferredSpeechToTextServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<SpeechToTextServiceResponse> responses = new System.Collections.Generic.List<SpeechToTextServiceResponse>();
            // invoke each ISpeechToTextService and show what it can do.
            foreach (ISpeechToTextService STT in PreferredOrderedISpeechToTextServices)
            {
                responses.Add(await STT.SpeechToTextServiceAsync(bytes, sampleRate).ContinueWith<SpeechToTextServiceResponse>((c) =>
                {
                    ServiceResponse r = c.Result.sr;
                    if (string.IsNullOrEmpty(r.ResponseResult) || r.StatusCode != 200)
                        Console.WriteLine(r.ServiceName + " STT (async): Failed with StatusCode of " + r.StatusCode);
                    else
                        Console.WriteLine(r.ServiceName + " STT (async):\"" + r.ResponseResult + "\" Total " + r.TotalElapsedMilliseconds + "ms Request " + r.RequestElapsedMilliseconds + "ms");
                    return c.Result;
                }));
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
