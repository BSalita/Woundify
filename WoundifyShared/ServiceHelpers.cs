using System;
using System.Collections.Generic;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()

namespace WoundifyShared
{

    public class FindServices<T>
    {
        public System.Collections.Generic.List<T> PreferredOrderingOfServices = new System.Collections.Generic.List<T>();

#if false // obsolete?
        public FindServices(List<string> preferredServices)
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
            System.Collections.Generic.Dictionary<string, Type> ServiceTypes = AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && typeof(T).IsAssignableFrom(type))
                    .ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase); // might be better to use FullName.
#endif
            // Match user preferences with available classes. Build list of service objects.
            foreach (string prefs in preferredServices)
            {
                if (ServiceTypes.ContainsKey(prefs))
                    PreferredOrderingOfServices.Add((T)ServiceTypes[prefs].GetConstructor(Type.EmptyTypes).Invoke(Type.EmptyTypes));
            }
        }
#endif
        public FindServices(string[] preferredServices)
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
            System.Collections.Generic.Dictionary<string, Type> ServiceTypes = AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && typeof(T).IsAssignableFrom(type))
                    .ToDictionary(k => k.Name + "." + typeof(T).Name, v => v, StringComparer.OrdinalIgnoreCase); // create key of Class+Interface
#endif
            // Match user preferences with available classes. Build list of service objects.
            foreach (string prefs in preferredServices)
            {
                if (!Options.services.ContainsKey(prefs))
                    throw new NotImplementedException();
                Settings.Service service = Options.services[prefs]; // preferred service must exist
                if (ServiceTypes.ContainsKey(service.classInterface)) // use class that implements T
                {
                    PreferredOrderingOfServices.Add((T)ServiceTypes[service.classInterface].GetConstructor(new Type[] { new Settings.Service().GetType() }).Invoke(new object[] { service }));
                }
                else
                    throw new MissingMethodException(); // Class or interface does not exist 
            }
        }
    }
}
