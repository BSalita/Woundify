using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class ParseServices
    {
        public static System.Collections.Generic.List<IParseService> PreferredOrderingParseServices = new FindServices<IParseService>(Options.commandservices["Parse"].preferredServices).PreferredOrderingOfServices;
        public static System.Collections.Generic.List<ParseServiceResponse> responses = new System.Collections.Generic.List<ParseServiceResponse>();

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ParseServiceResponse>> RunAllPreferredParseServicesAsync(string text)
        {
            return RunAllPreferredParseServicesRun(text);
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ParseServiceResponse>> RunAllPreferredParseServicesAsync(byte[] bytes, int sampleRate)
        {
            return null;
        }

        public static System.Collections.Generic.List<ParseServiceResponse> RunAllPreferredParseServicesRun(string text)
        {
            responses = new System.Collections.Generic.List<ParseServiceResponse>();
            // invoke each IParseService and show what it can do.
            foreach (IParseService STT in PreferredOrderingParseServices)
            {
                System.Threading.Tasks.Task.Run(() => STT.ParseServiceAsync(text)).ContinueWith((c) =>
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
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IParseServiceResponse>> RunAllPreferredParseServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IParseServiceResponse> responses = new System.Collections.Generic.List<IParseServiceResponse>();
            // invoke each IParseService and show what it can do.
            foreach (IParseService STT in PreferredIParseServices)
            {
                await STT.ParseServicesAsync(bytes, sampleRate).ContinueWith<IParseServiceResponse>((c) =>
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
