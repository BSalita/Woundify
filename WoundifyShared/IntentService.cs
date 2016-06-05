using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class IntentServices
    {
        public static System.Collections.Generic.List<IIntentService> PreferredOrderingIntentServices = new FindServices<IIntentService>(Options.commandservices["Intent"].preferredServices).PreferredOrderingOfServices;
        public static System.Collections.Generic.List<IntentServiceResponse> responses = new System.Collections.Generic.List<IntentServiceResponse>();

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IntentServiceResponse>> RunAllPreferredIntentServicesAsync(string text)
        {
            return RunAllPreferredIntentServicesRun(text);
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IntentServiceResponse>> RunAllPreferredIntentServicesAsync(byte[] bytes, int sampleRate)
        {
            return null;
        }

        public static System.Collections.Generic.List<IntentServiceResponse> RunAllPreferredIntentServicesRun(string text)
        {
            responses = new System.Collections.Generic.List<IntentServiceResponse>();
            // invoke each IIntentService and show what it can do.
            foreach (IIntentService STT in PreferredOrderingIntentServices)
            {
                System.Threading.Tasks.Task.Run(() => STT.IntentServiceAsync(text)).ContinueWith((c) =>
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
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IIntentServiceResponse>> RunAllPreferredIntentServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IIntentServiceResponse> responses = new System.Collections.Generic.List<IIntentServiceResponse>();
            // invoke each IIntentService and show what it can do.
            foreach (IIntentService STT in PreferredIIntentServices)
            {
                await STT.IntentServicesAsync(bytes, sampleRate).ContinueWith<IIntentServiceResponse>((c) =>
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
