using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class IntentServices
    {
        public static System.Collections.Generic.List<IIntentService> PreferredOrderIntentServices = new FindServices<IIntentService>(Options.options.Services.APIs.Intent.preferredIntentServices).PreferredOrderingOfServices;

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IntentServiceResponse>> RunAllPreferredIntentServices(string text)
        {
            System.Collections.Generic.List<IntentServiceResponse> responses = new System.Collections.Generic.List<IntentServiceResponse>();
            // invoke each IIntentService and show what it can do.
            foreach (IIntentService STT in PreferredOrderIntentServices)
            {
                responses.Add(await STT.IntentServiceAsync(text).ContinueWith<IntentServiceResponse>((c) =>
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

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IntentServiceResponse>> RunAllPreferredIntentServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IntentServiceResponse> responses = new System.Collections.Generic.List<IntentServiceResponse>();
            // invoke each IIntentService and show what it can do.
            foreach (IIntentService STT in PreferredOrderIntentServices)
            {
                responses.Add(await STT.IntentServiceAsync(bytes, sampleRate).ContinueWith<IntentServiceResponse>((c) =>
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
