using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class SpellServices
    {
        public static System.Collections.Generic.List<ISpellService> PreferredOrderingSpellServices = new FindServices<ISpellService>(Options.commandservices["Spell"].preferredServices).PreferredOrderingOfServices;
        public static System.Collections.Generic.List<SpellServiceResponse> responses = new System.Collections.Generic.List<SpellServiceResponse>();

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<SpellServiceResponse>> RunAllPreferredSpellServicesAsync(string text)
        {
            return RunAllPreferredSpellServicesRun(text);
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<SpellServiceResponse>> RunAllPreferredSpellServicesAsync(byte[] bytes, int sampleRate)
        {
            return null;
        }

        public static System.Collections.Generic.List<SpellServiceResponse> RunAllPreferredSpellServicesRun(string text)
        {
            responses = new System.Collections.Generic.List<SpellServiceResponse>();
            // invoke each ISpellService and show what it can do.
            foreach (ISpellService STT in PreferredOrderingSpellServices)
            {
                System.Threading.Tasks.Task.Run(() => STT.SpellServiceAsync(text)).ContinueWith((c) =>
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
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ISpellServiceResponse>> RunAllPreferredSpellServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<ISpellServiceResponse> responses = new System.Collections.Generic.List<ISpellServiceResponse>();
            // invoke each ISpellService and show what it can do.
            foreach (ISpellService STT in PreferredISpellServices)
            {
                await STT.SpellServicesAsync(bytes, sampleRate).ContinueWith<ISpellServiceResponse>((c) =>
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
