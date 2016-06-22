using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class EntitiesServices
    {
        public static System.Collections.Generic.List<IEntitiesService> PreferredOrderingEntitiesServices = new FindServices<IEntitiesService>(Options.commandservices["Entities"].preferredServices).PreferredOrderingOfServices;
        public static System.Collections.Generic.List<EntitiesServiceResponse> responses = new System.Collections.Generic.List<EntitiesServiceResponse>();

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<EntitiesServiceResponse>> RunAllPreferredEntitiesServicesAsync(string text)
        {
            return RunAllPreferredEntitiesServicesRun(text);
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<EntitiesServiceResponse>> RunAllPreferredEntitiesServicesAsync(byte[] bytes, int sampleRate)
        {
            return null;
        }

        public static System.Collections.Generic.List<EntitiesServiceResponse> RunAllPreferredEntitiesServicesRun(string text)
        {
            responses = new System.Collections.Generic.List<EntitiesServiceResponse>();
            // invoke each IEntitiesService and show what it can do.
            foreach (IEntitiesService STT in PreferredOrderingEntitiesServices)
            {
                System.Threading.Tasks.Task.Run(() => STT.EntitiesServiceAsync(text)).ContinueWith((c) =>
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
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IEntitiesServiceResponse>> RunAllPreferredEntitiesServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IEntitiesServiceResponse> responses = new System.Collections.Generic.List<IEntitiesServiceResponse>();
            // invoke each IEntitiesService and show what it can do.
            foreach (IEntitiesService STT in PreferredIEntitiesServices)
            {
                await STT.EntitiesServicesAsync(bytes, sampleRate).ContinueWith<IEntitiesServiceResponse>((c) =>
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
