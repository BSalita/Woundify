using System;

namespace WoundifyShared
{
    class Options
    {
        public static Settings.RootObject options;
#if false
        public static BingServices bing;
        public static GoogleServices google;
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
                        options = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings.RootObject>(System.IO.File.ReadAllText(path));
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
            await Log.LogFileInitAsync();
            Console.WriteLine("Log file:" + options.logFilePath);
#if WINDOWS_UWP
#else
            Log.logFile = new System.IO.StreamWriter(options.logFilePath);
            Log.logFile.AutoFlush = true; // flush after every write
#endif
#if false
            // todo: obsolete? for initializing statics?
            bing = new BingServices();
            google = new GoogleServices();
            houndify = new HoundifyServices();
            windows = new WindowsServices();
            wit = new WitServices();
            geoLocation = new GeoLocation();
#endif
        }
    }

    public class Settings
    {
        public class NAudio
        {
            public int desiredLatencyMilliseconds { get; set; }
            public int inputDeviceNumber { get; set; }
            public int waveInBufferMilliseconds { get; set; }
        }

        public class Audio
        {
            public string transCodeFileName { get; set; }
            public string speechSynthesisFileName { get; set; }
            public int bitDepth { get; set; }
            public int channels { get; set; }
            public int samplingRate { get; set; }
            public NAudio NAudio { get; set; }
        }

        public class Command
        {
            public string Key { get; set; }
            public string Name { get; set; }
            public string Help { get; set; }
        }

        public class Geolocaton
        {
            public double longitude { get; set; }
            public double latitude { get; set; }
            public int accuracyInMeters { get; set; }
            public string town { get; set; }
            public string region { get; set; }
            public string regionCode { get; set; }
            public string country { get; set; }
            public string countryCode { get; set; }
        }

        public class Locale
        {
            public string language { get; set; }
        }

        public class Wakeup
        {
            public double confidence { get; set; }
            public double endSilenceTimeout { get; set; }
            public double initialSilenceTimeout { get; set; }
            public double listenTimeOut { get; set; }
            public bool preferLoopUntilWakeUpWordFound { get; set; }
            public System.Collections.Generic.List<string> words { get; set; }
        }

        public class BingSpeechToText
        {
            public bool callSpeechToText { get; set; }
            public string ClientID { get; set; }
            public string clientSecret { get; set; }
        }

        public class GoogleSpeechToText
        {
            public bool callSpeechToText { get; set; }
            public string key { get; set; }
        }

        public class HoundifySpeechToText
        {
            public bool callSpeechToText { get; set; }
            public string ClientID { get; set; }
            public string ClientKey { get; set; }
            public string UserID { get; set; }
        }

        public class WindowsSpeechToText
        {
            public bool callSpeechToText { get; set; }
            public int voiceAge { get; set; }
            public int voiceGender { get; set; }
        }

        public class WitSpeechToText
        {
            public string Bearer { get; set; }
        }

        public class SpeechToText
        {
            public string missingResponse { get; set; }
            public System.Collections.Generic.List<string> preferredSpeechToTextServices { get; set; }
            public BingSpeechToText BingSpeechToText { get; set; }
            public GoogleSpeechToText GoogleSpeechToText { get; set; }
            public HoundifySpeechToText HoundifySpeechToText { get; set; }
            public WindowsSpeechToText WindowsSpeechToText { get; set; }
            public WitSpeechToText WitSpeechToText { get; set; }
        }

        public class HoundifyIntent
        {
            public string ClientID { get; set; }
            public string ClientKey { get; set; }
            public string UserID { get; set; }
            public bool PartialTranscriptsDesired { get; set; }
        }

        public class Intent
        {
            public System.Collections.Generic.List<string> preferredIntentServices { get; set; }
            public HoundifyIntent HoundifyIntent { get; set; }
        }

        public class BingParse
        {
            public string OcpApimSubscriptionKey { get; set; }
        }

        public class Parse
        {
            public System.Collections.Generic.List<string> preferredParseServices { get; set; }
            public BingParse BingParse { get; set; }
        }

        public class APIs
        {
            public bool PreferSystemNet { get; set; }
            public bool PreferChunkedEncodedRequests { get; set; }
            public Intent Intent { get; set; }
            public Parse Parse { get; set; }
            public SpeechToText SpeechToText { get; set; }
        }

        public class Services
        {
            public APIs APIs { get; set; }
        }

        public class RootObject
        {
            public string version { get; set; }
            public int debugLevel { get; set; }
            public string tempFolderPath { get; set; }
            public string logFilePath { get; set; }
            public double pauseSecondsDefault { get; set; }
            public Audio audio { get; set; }
            public System.Collections.Generic.List<Command> commands { get; set; }
            public Geolocaton geolocaton { get; set; }
            public IntentServices intentServices { get; set; }
            public Locale locale { get; set; }
            public Wakeup wakeup { get; set; }
            public Services Services { get; set; }
        }
    }
}
