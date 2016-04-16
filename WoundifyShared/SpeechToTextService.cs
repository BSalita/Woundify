using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class SpeechToTextServices
    {
        public static System.Collections.Generic.List<ISpeechToTextService> PreferredOrderedISpeechToTextServices = new System.Collections.Generic.List<ISpeechToTextService>();

        static SpeechToTextServices() // static constructor
        {
            // We need to create a list of ISpeechToTextService objects ordered by user's preference settings.
            // Using reflection to get list of classes implementing ISpeechToTextService
#if WINDOWS_UWP
            // We get the current assembly through the current class
            var currentAssembly = typeof(SpeechToText).GetType().GetTypeInfo().Assembly;

            // we filter the defined classes according to the interfaces they implement
            //System.Collections.Generic.IEnumerable<ISpeechToTextService> ISpeechToTextServiceTypes = currentAssembly.DefinedTypes.SelectMany(assembly => assembly.GetTypes()).Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(ISpeechToTextService))).ToList();
            System.Collections.Generic.IEnumerable<Type> ISpeechToTextServiceTypes = currentAssembly.DefinedTypes
                   .Select(type => typeof(IServiceResponse));
#else
            System.Collections.Generic.IEnumerable<Type> ISpeechToTextServiceTypes = AppDomain
                   .CurrentDomain
                   .GetAssemblies()
                   .SelectMany(assembly => assembly.GetTypes())
                   .Where(type => typeof(ISpeechToTextService).IsAssignableFrom(type));
#endif
            // Match user preference with available classes. Build list of ISpeechToTextService objects.
            foreach (string STT in Options.options.Services.APIs.SpeechToText.preferredSpeechToTextServices)
            {
                foreach (Type t in ISpeechToTextServiceTypes)
                {
                    // for each ISpeechToTextService requested, invoke it's constructor and drop it into the list.
                    if (STT == t.Name)
                        PreferredOrderedISpeechToTextServices.Add((ISpeechToTextService)t.GetConstructor(Type.EmptyTypes).Invoke(Type.EmptyTypes));
                }
            }
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ISpeechToTextServiceResponse>> RunAllPreferredSpeechToTextServices(string fileName)
        {
            byte[] bytes = await Helpers.ReadBytesFromFileAsync(fileName);
            int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
            return await RunAllPreferredSpeechToTextServices(bytes, sampleRate);
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ISpeechToTextServiceResponse>> RunAllPreferredSpeechToTextServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<ISpeechToTextServiceResponse> responses = new System.Collections.Generic.List<ISpeechToTextServiceResponse>();
            // invoke each ISpeechToTextService and show what it can do.
            foreach (ISpeechToTextService STT in PreferredOrderedISpeechToTextServices)
            {
                responses.Add(await STT.SpeechToTextAsync(bytes, sampleRate).ContinueWith<ISpeechToTextServiceResponse>((c) =>
                {
                    IServiceResponse r = c.Result.sr;
                    if (string.IsNullOrEmpty(r.ResponseResult) || r.StatusCode != 200)
                        Console.WriteLine(STT.GetType().Name + " STT (async): Failed with StatusCode of " + r.StatusCode);
                    else
                        Console.WriteLine(STT.GetType().Name + " STT (async):\"" + r.ResponseResult + "\" Total " + r.TotalElapsedMilliseconds + "ms Request " + r.RequestElapsedMilliseconds + "ms");
                    return c.Result;
                }));
            }
            return responses;
        }

#if false
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ISpeechToTextServiceResponse>> RunAllPreferredSpeechToTextServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<ISpeechToTextServiceResponse> responses = new System.Collections.Generic.List<ISpeechToTextServiceResponse>();
            // invoke each ISpeechToTextService and show what it can do.
            foreach (ISpeechToTextService STT in PreferredISpeechToTextServices)
            {
                await STT.SpeechToTextAsync(bytes, sampleRate).ContinueWith<ISpeechToTextServiceResponse>((c) =>
                {
                    if (string.IsNullOrEmpty(c.Result.sr.ResponseResult) || c.Result.sr.StatusCode != 200)
                        Console.WriteLine(STT.GetType().Name + " STT (async): Failed with StatusCode of " + c.Result.StatusCode);
                    else
                        Console.WriteLine(STT.GetType().Name + " STT (async):\"" + c.Result.ResponseResult + "\" Total " + c.Result.TotalElapsedMilliseconds + "ms Request " + c.Result.RequestElapsedMilliseconds + "ms");
                    responses.Add(c.Result);
                });
            }
            return responses;
        }
#endif
    }
}
