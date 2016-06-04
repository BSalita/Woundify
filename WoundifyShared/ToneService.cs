using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class ToneServices
    {
        public static System.Collections.Generic.List<IToneService> PreferredOrderingToneServices = new FindServices<IToneService>(Options.commandservices["Tone"].preferredServices).PreferredOrderingOfServices;
        public static System.Collections.Generic.List<ToneServiceResponse> responses = new System.Collections.Generic.List<ToneServiceResponse>();

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ToneServiceResponse>> RunAllPreferredToneServicesAsync(string fileName)
        {
            byte[] bytes = await Helpers.ReadBytesFromFileAsync(fileName);
            int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
            return RunAllPreferredToneServicesRun(bytes, sampleRate);
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ToneServiceResponse>> RunAllPreferredToneServicesAsync(byte[] bytes, int sampleRate)
        {
            return RunAllPreferredToneServicesRun(bytes, sampleRate);
        }

        public static System.Collections.Generic.List<ToneServiceResponse> RunAllPreferredToneServicesRun(byte[] bytes, int sampleRate)
        {
            responses = new System.Collections.Generic.List<ToneServiceResponse>();
            // invoke each IToneService and show what it can do.
            foreach (IToneService STT in PreferredOrderingToneServices)
            {
                System.Threading.Tasks.Task.Run(() => STT.ToneServiceAsync(bytes, sampleRate)).ContinueWith((c) =>
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
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IToneServiceResponse>> RunAllPreferredToneServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IToneServiceResponse> responses = new System.Collections.Generic.List<IToneServiceResponse>();
            // invoke each IToneService and show what it can do.
            foreach (IToneService STT in PreferredIToneServices)
            {
                await STT.ToneServicesAsync(bytes, sampleRate).ContinueWith<IToneServiceResponse>((c) =>
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
