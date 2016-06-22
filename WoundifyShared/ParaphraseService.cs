﻿using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class ParaphraseServices
    {
        public static System.Collections.Generic.List<IParaphraseService> PreferredOrderingParaphraseServices = new FindServices<IParaphraseService>(Options.commandservices["Paraphrase"].preferredServices).PreferredOrderingOfServices;
        public static System.Collections.Generic.List<ParaphraseServiceResponse> responses = new System.Collections.Generic.List<ParaphraseServiceResponse>();

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ParaphraseServiceResponse>> RunAllPreferredParaphraseServicesAsync(string text)
        {
            return RunAllPreferredParaphraseServicesRun(text);
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ParaphraseServiceResponse>> RunAllPreferredParaphraseServicesAsync(byte[] bytes, int sampleRate)
        {
            return null;
        }

        public static System.Collections.Generic.List<ParaphraseServiceResponse> RunAllPreferredParaphraseServicesRun(string text)
        {
            responses = new System.Collections.Generic.List<ParaphraseServiceResponse>();
            // invoke each IParaphraseService and show what it can do.
            foreach (IParaphraseService STT in PreferredOrderingParaphraseServices)
            {
                System.Threading.Tasks.Task.Run(() => STT.ParaphraseServiceAsync(text)).ContinueWith((c) =>
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
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IParaphraseServiceResponse>> RunAllPreferredParaphraseServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IParaphraseServiceResponse> responses = new System.Collections.Generic.List<IParaphraseServiceResponse>();
            // invoke each IParaphraseService and show what it can do.
            foreach (IParaphraseService STT in PreferredIParaphraseServices)
            {
                await STT.ParaphraseServicesAsync(bytes, sampleRate).ContinueWith<IParaphraseServiceResponse>((c) =>
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
