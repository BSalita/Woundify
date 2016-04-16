using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class IntentServices
    {
        public static System.Collections.Generic.List<IIntentService> PreferredOrderIntentServices = new System.Collections.Generic.List<IIntentService>();

        static IntentServices() // static constructor
        {
            // We need to create a list of IIntentService objects ordered by user's preference settings.
            // Using reflection to get list of classes implementing IIntentService
#if WINDOWS_UWP
            // We get the current assembly through the current class
            var currentAssembly = typeof(IntentServices).GetType().GetTypeInfo().Assembly;

            // we filter the defined classes according to the interfaces they implement
            //System.Collections.Generic.IEnumerable<IIntentService> IIntentServiceTypes = currentAssembly.DefinedTypes.SelectMany(assembly => assembly.GetTypes()).Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(IIntentService))).ToList();
            System.Collections.Generic.IEnumerable<Type> IIntentServiceTypes = currentAssembly.DefinedTypes
                   .Select(type => typeof(IServiceResponse));
#else
            System.Collections.Generic.IEnumerable<Type> IIntentServiceTypes = AppDomain
                   .CurrentDomain
                   .GetAssemblies()
                   .SelectMany(assembly => assembly.GetTypes())
                   .Where(type => typeof(IIntentService).IsAssignableFrom(type));
#endif
            // Match user preference with available classes. Build list of IIntentService objects.
            foreach (string STT in Options.options.Services.APIs.Intent.preferredIntentServices)
            {
                foreach (Type t in IIntentServiceTypes)
                {
                    // for each IIntentService requested, invoke it's constructor and drop it into the list.
                    if (STT == t.Name)
                        PreferredOrderIntentServices.Add((IIntentService)t.GetConstructor(Type.EmptyTypes).Invoke(Type.EmptyTypes));
                }
            }
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IIntentServiceResponse>> RunAllPreferredIntentServices(string text)
        {
            System.Collections.Generic.List<IIntentServiceResponse> responses = new System.Collections.Generic.List<IIntentServiceResponse>();
            // invoke each IIntentService and show what it can do.
            foreach (IIntentService STT in PreferredOrderIntentServices)
            {
                responses.Add(await STT.IntentServiceAsync(text).ContinueWith<IIntentServiceResponse>((c) =>
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

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IIntentServiceResponse>> RunAllPreferredIntentServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IIntentServiceResponse> responses = new System.Collections.Generic.List<IIntentServiceResponse>();
            // invoke each IIntentService and show what it can do.
            foreach (IIntentService STT in PreferredOrderIntentServices)
            {
                responses.Add(await STT.IntentServiceAsync(bytes, sampleRate).ContinueWith<IIntentServiceResponse>((c) =>
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
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IIntentServiceResponse>> RunAllPreferredIntentServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IIntentServiceResponse> responses = new System.Collections.Generic.List<IIntentServiceResponse>();
            // invoke each IIntentService and show what it can do.
            foreach (IIntentService STT in PreferredIIntentServices)
            {
                await STT.IntentServicesAsync(bytes, sampleRate).ContinueWith<IIntentServiceResponse>((c) =>
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
