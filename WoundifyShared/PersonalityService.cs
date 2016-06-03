using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class PersonalityServices
    {
        public static System.Collections.Generic.List<IPersonalityService> PreferredOrderPersonalityServices = new FindServices<IPersonalityService>(Options.commandservices["Personality"].preferredServices).PreferredOrderingOfServices;

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<PersonalityServiceResponse>> RunAllPreferredPersonalityServices(string text)
        {
            System.Collections.Generic.List<PersonalityServiceResponse> responses = new System.Collections.Generic.List<PersonalityServiceResponse>();
            // invoke each IPersonalityService and show what it can do.
            foreach (IPersonalityService STT in PreferredOrderPersonalityServices)
            {
                responses.Add(await STT.PersonalityServiceAsync(text).ContinueWith<PersonalityServiceResponse>((c) =>
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

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<PersonalityServiceResponse>> RunAllPreferredPersonalityServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<PersonalityServiceResponse> responses = new System.Collections.Generic.List<PersonalityServiceResponse>();
            // invoke each IPersonalityService and show what it can do.
            foreach (IPersonalityService STT in PreferredOrderPersonalityServices)
            {
                responses.Add(await STT.PersonalityServiceAsync(bytes, sampleRate).ContinueWith<PersonalityServiceResponse>((c) =>
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
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IPersonalityServiceResponse>> RunAllPreferredPersonalityServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IPersonalityServiceResponse> responses = new System.Collections.Generic.List<IPersonalityServiceResponse>();
            // invoke each IPersonalityService and show what it can do.
            foreach (IPersonalityService STT in PreferredIPersonalityServices)
            {
                await STT.PersonalityServicesAsync(bytes, sampleRate).ContinueWith<IPersonalityServiceResponse>((c) =>
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
