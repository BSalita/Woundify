using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class AnnotateServices
    {
        public static System.Collections.Generic.List<IAnnotateService> PreferredOrderingAnnotateServices = new FindServices<IAnnotateService>(Options.commandservices["Annotate"].preferredServices).PreferredOrderingOfServices;
        public static System.Collections.Generic.List<AnnotateServiceResponse> responses = new System.Collections.Generic.List<AnnotateServiceResponse>();

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<AnnotateServiceResponse>> RunAllPreferredAnnotateServicesAsync(string text)
        {
            return RunAllPreferredAnnotateServicesRun(text);
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<AnnotateServiceResponse>> RunAllPreferredAnnotateServicesAsync(byte[] bytes, int sampleRate)
        {
            return null;
        }

        public static System.Collections.Generic.List<AnnotateServiceResponse> RunAllPreferredAnnotateServicesRun(string text)
        {
            responses = new System.Collections.Generic.List<AnnotateServiceResponse>();
            // invoke each IAnnotateService and show what it can do.
            foreach (IAnnotateService STT in PreferredOrderingAnnotateServices)
            {
                System.Threading.Tasks.Task.Run(() => STT.AnnotateServiceAsync(text)).ContinueWith((c) =>
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
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IAnnotateServiceResponse>> RunAllPreferredAnnotateServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IAnnotateServiceResponse> responses = new System.Collections.Generic.List<IAnnotateServiceResponse>();
            // invoke each IAnnotateService and show what it can do.
            foreach (IAnnotateService STT in PreferredIAnnotateServices)
            {
                await STT.AnnotateServicesAsync(bytes, sampleRate).ContinueWith<IAnnotateServiceResponse>((c) =>
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
