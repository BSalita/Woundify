using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class TranslateServices
    {
        public static System.Collections.Generic.List<ITranslateService> PreferredOrderTranslateServices = new FindServices<ITranslateService>(Options.commandservices["Translate"].preferredServices).PreferredOrderingOfServices;

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<TranslateServiceResponse>> RunAllPreferredTranslateServices(string text)
        {
            System.Collections.Generic.List<TranslateServiceResponse> responses = new System.Collections.Generic.List<TranslateServiceResponse>();
            // invoke each ITranslateService and show what it can do.
            foreach (ITranslateService STT in PreferredOrderTranslateServices)
            {
                responses.Add(await STT.TranslateServiceAsync(text).ContinueWith<TranslateServiceResponse>((c) =>
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

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<TranslateServiceResponse>> RunAllPreferredTranslateServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<TranslateServiceResponse> responses = new System.Collections.Generic.List<TranslateServiceResponse>();
            // invoke each ITranslateService and show what it can do.
            foreach (ITranslateService STT in PreferredOrderTranslateServices)
            {
                responses.Add(await STT.TranslateServiceAsync(bytes, sampleRate).ContinueWith<TranslateServiceResponse>((c) =>
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
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ITranslateServiceResponse>> RunAllPreferredTranslateServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<ITranslateServiceResponse> responses = new System.Collections.Generic.List<ITranslateServiceResponse>();
            // invoke each ITranslateService and show what it can do.
            foreach (ITranslateService STT in PreferredITranslateServices)
            {
                await STT.TranslateServicesAsync(bytes, sampleRate).ContinueWith<ITranslateServiceResponse>((c) =>
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
