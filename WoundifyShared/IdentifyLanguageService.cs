using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class IdentifyLanguageServices
    {
        public static System.Collections.Generic.List<IIdentifyLanguageService> PreferredOrderIdentifyLanguageServices = new FindServices<IIdentifyLanguageService>(Options.commandservices["Identify"].preferredServices).PreferredOrderingOfServices;

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IdentifyLanguageServiceResponse>> RunAllPreferredIdentifyLanguageServices(string text)
        {
            System.Collections.Generic.List<IdentifyLanguageServiceResponse> responses = new System.Collections.Generic.List<IdentifyLanguageServiceResponse>();
            // invoke each IIdentifyLanguageService and show what it can do.
            foreach (IIdentifyLanguageService STT in PreferredOrderIdentifyLanguageServices)
            {
                responses.Add(await STT.IdentifyLanguageServiceAsync(text).ContinueWith<IdentifyLanguageServiceResponse>((c) =>
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

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IdentifyLanguageServiceResponse>> RunAllPreferredIdentifyLanguageServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IdentifyLanguageServiceResponse> responses = new System.Collections.Generic.List<IdentifyLanguageServiceResponse>();
            // invoke each IIdentifyLanguageService and show what it can do.
            foreach (IIdentifyLanguageService STT in PreferredOrderIdentifyLanguageServices)
            {
                responses.Add(await STT.IdentifyLanguageServiceAsync(bytes, sampleRate).ContinueWith<IdentifyLanguageServiceResponse>((c) =>
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
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IIdentifyLanguageServiceResponse>> RunAllPreferredIdentifyLanguageServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IIdentifyLanguageServiceResponse> responses = new System.Collections.Generic.List<IIdentifyLanguageServiceResponse>();
            // invoke each IIdentifyLanguageService and show what it can do.
            foreach (IIdentifyLanguageService STT in PreferredIIdentifyLanguageServices)
            {
                await STT.IdentifyLanguageServicesAsync(bytes, sampleRate).ContinueWith<IIdentifyLanguageServiceResponse>((c) =>
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
