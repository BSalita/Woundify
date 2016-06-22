using System;
using System.Collections.Generic;
using System.Linq;

namespace WoundifyShared
{
    class Options
    {
        public static Settings.Rootobject options;
        public static Dictionary<string, Settings.Command> commands = new Dictionary<string, Settings.Command>(StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, Settings.Commandservice> commandservices = new Dictionary<string, Settings.Commandservice>(StringComparer.OrdinalIgnoreCase);
        //public static Dictionary<string, Settings.Service> services = new Dictionary<string, Settings.Service>(StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, WoundifyServices> services = new Dictionary<string, WoundifyServices>(StringComparer.OrdinalIgnoreCase);
#if false
        public static BingServices bing;
        public static GoogleServices google;
        public static GoogleCloudServices googlecloud;
        public static IbmWatsonServices ibmwatson;
        public static HoundifyServices houndify;
        public static WindowsServices windows;
        public static WitServices wit;
        public static GeoLocation geoLocation;
#endif

        private static void SearchForSettings(string[] settingsJsonSearchPaths)
        {
            Console.WriteLine("Searching for settings files.");
            foreach (string path in settingsJsonSearchPaths)
            {
                Console.Write("Searching for " + path);
                if (System.IO.File.Exists(path))
                {
                    if (options == null)
                    {
                        Console.WriteLine(". File found. Initializing settings.");
                        options = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings.Rootobject>(System.IO.File.ReadAllText(path));
                    }
                    else
                    {
                        Console.WriteLine(". File found. Overriding settings.");
                        Newtonsoft.Json.JsonConvert.PopulateObject(System.IO.File.ReadAllText(path), options);
                    }
                    break;
                }
                else
                    Console.WriteLine(". File not found.");
            }
        }

        public static async System.Threading.Tasks.Task OptionsInit()
        {
#if WINDOWS_UWP
            string[] defaultSettingsJsonSearchPaths = {
                System.IO.Path.GetFullPath(System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, @".\")) + "WoundifyDefaultSettings.json",
               "WoundifyDefaultSettings.json"
            };
            string[] settingsJsonSearchPaths = {
                System.IO.Path.GetFullPath(System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, @".\")) + "WoundifySettings.json",
                "WoundifySettings.json"
            };
#else
            string[] defaultSettingsJsonSearchPaths = {
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + System.IO.Path.DirectorySeparatorChar + "WoundifyDefaultSettings.json",
                "WoundifyDefaultSettings.json"
            };
            string[] settingsJsonSearchPaths = {
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + System.IO.Path.DirectorySeparatorChar + "WoundifySettings.json",
                "WoundifySettings.json"
            };
#endif
            SearchForSettings(defaultSettingsJsonSearchPaths);
            SearchForSettings(settingsJsonSearchPaths);
            if (options == null)
                throw new Exception("Unable to read settings.json file. Program terminating.");
            if (string.IsNullOrEmpty(options.version))
                throw new Exception("Invalid settings.json file. Program terminating.");
            if (string.IsNullOrEmpty(options.tempFolderPath))
            {
#if WINDOWS_UWP
                options.tempFolderPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path; // otherwise Windows.Storage.KnownFolders.MusicLibrary.Path
#else
                options.tempFolderPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar + "Woundify"; // alternatively Windows.Storage.KnownFolders.MusicLibrary.Path
#endif
                if (!options.tempFolderPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                    options.tempFolderPath += System.IO.Path.DirectorySeparatorChar;
                Console.WriteLine("Temporary folder path:" + options.tempFolderPath);
            }
            if (!System.IO.Directory.Exists(options.tempFolderPath))
                System.IO.Directory.CreateDirectory(options.tempFolderPath);
            if (string.IsNullOrEmpty(options.logFilePath))
                options.logFilePath = options.tempFolderPath + "log.txt";
            Console.WriteLine("Log file:" + options.logFilePath);
            await Log.LogFileInitAsync();
            if (string.IsNullOrEmpty(options.curlFilePath))
                options.curlFilePath = options.tempFolderPath + "curls.txt";
            Console.WriteLine("curl file:" + options.curlFilePath);
#if WINDOWS_UWP
#else
            Log.logFile = new System.IO.StreamWriter(options.logFilePath);
            Log.logFile.AutoFlush = true; // flush after every write
#endif
#if false
            System.Collections.Generic.Dictionary<string, Type> ServiceTypes = AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && typeof(IService).IsAssignableFrom(type))
                    .ToDictionary(k => k.Name + "." + typeof(IService).Name, v => v, StringComparer.OrdinalIgnoreCase); // create key of Class+Interface
#endif
            foreach (Settings.Service service in Options.options.services)
            {
                services.Add(service.name, new WoundifyServices(service));
            }
            foreach (Settings.Command command in Options.options.commands)
            {
                commands.Add(command.key, command);
            }
            foreach (Settings.Commandservice s in Options.options.commandServices)
            {
                if (!commands.ContainsKey(s.key)) // must be a listed command
                    throw new NotImplementedException();
                foreach (string pref in s.preferredServices)
                    if (!services.ContainsKey(pref)) // must be a listed service
                        throw new NotImplementedException();
                commandservices.Add(s.key, s);
            }
        }
#if false
            // todo: obsolete? for initializing statics?
            bing = new BingServices();
            google = new GoogleServices();
            googlecloud = new GoogleCloudServices();
            ibmwatson = new IbmWatsonServices();
            houndify = new HoundifyServices();
            windows = new WindowsServices();
            wit = new WitServices();
            geoLocation = new GeoLocation();
#endif
    }
}
