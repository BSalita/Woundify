using System;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{
    public class ParseServices
    {
        public static System.Collections.Generic.List<IParseService> PreferredOrderedParseServices = new System.Collections.Generic.List<IParseService>();

        static ParseServices() // static constructor
        {
            // We need to create a list of IParseService objects ordered by user's preference settings.
            // Using reflection to get list of classes implementing IParseService
#if WINDOWS_UWP
            // We get the current assembly through the current class
            var currentAssembly = typeof(ParseServices).GetType().GetTypeInfo().Assembly;

            // we filter the defined classes according to the interfaces they implement
            //System.Collections.Generic.IEnumerable<IParseService> IParseServiceTypes = currentAssembly.DefinedTypes.SelectMany(assembly => assembly.GetTypes()).Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(IParseService))).ToList();
            System.Collections.Generic.IEnumerable<Type> IParseServiceTypes = currentAssembly.DefinedTypes
                   .Select(type => typeof(IServiceResponse));
#else
            System.Collections.Generic.IEnumerable<Type> IParseServiceTypes = AppDomain
                   .CurrentDomain
                   .GetAssemblies()
                   .SelectMany(assembly => assembly.GetTypes())
                   .Where(type => typeof(IParseService).IsAssignableFrom(type));
#endif
            // Match user preference with available classes. Build list of IParseService objects.
            foreach (string STT in Options.options.Services.APIs.Parse.preferredParseServices)
            {
                foreach (Type t in IParseServiceTypes)
                {
                    // for each IParseService requested, invoke it's constructor and drop it into the list.
                    if (STT == t.Name)
                        PreferredOrderedParseServices.Add((IParseService)t.GetConstructor(Type.EmptyTypes).Invoke(Type.EmptyTypes));
                }
            }
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IParseServiceResponse>> RunAllPreferredParseServices(string text)
        {
            System.Collections.Generic.List<IParseServiceResponse> responses = new System.Collections.Generic.List<IParseServiceResponse>();
            // invoke each IParseService and show what it can do.
            foreach (IParseService STT in PreferredOrderedParseServices)
            {
                responses.Add(await STT.ParseServiceAsync(text).ContinueWith<IParseServiceResponse>((c) =>
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
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<IParseServiceResponse>> RunAllPreferredParseServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<IParseServiceResponse> responses = new System.Collections.Generic.List<IParseServiceResponse>();
            // invoke each IParseService and show what it can do.
            foreach (IParseService STT in PreferredIParseServices)
            {
                await STT.ParseServicesAsync(bytes, sampleRate).ContinueWith<IParseServiceResponse>((c) =>
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
