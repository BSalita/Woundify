using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class PersonalityServices
    {
        public static System.Collections.Generic.List<IPersonalityService> PreferredOrderingPersonalityServices = new FindServices<IPersonalityService>(Options.commandservices["Personality"].preferredServices).PreferredOrderingOfServices;
        public static System.Collections.Generic.List<PersonalityServiceResponse> responses = new System.Collections.Generic.List<PersonalityServiceResponse>();

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<PersonalityServiceResponse>> RunAllPreferredPersonalityServicesAsync(string fileName)
        {
            byte[] bytes = await Helpers.ReadBytesFromFileAsync(fileName);
            int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
            return RunAllPreferredPersonalityServicesRun(bytes, sampleRate);
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<PersonalityServiceResponse>> RunAllPreferredPersonalityServicesAsync(byte[] bytes, int sampleRate)
        {
            return RunAllPreferredPersonalityServicesRun(bytes, sampleRate);
        }

        public static System.Collections.Generic.List<PersonalityServiceResponse> RunAllPreferredPersonalityServicesRun(byte[] bytes, int sampleRate)
        {
            responses = new System.Collections.Generic.List<PersonalityServiceResponse>();
            // invoke each IPersonalityService and show what it can do.
            foreach (IPersonalityService STT in PreferredOrderingPersonalityServices)
            {
                System.Threading.Tasks.Task.Run(() => STT.PersonalityServiceAsync(bytes, sampleRate)).ContinueWith((c) =>
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
