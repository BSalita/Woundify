using System;
using System.Collections.Generic;
using System.Text;

namespace WoundifyShared
{
    public class Settings
    {
        // this class is a concrete implementation of the settings json file. It is converted automatically by copying WoundifyDefaultSettings.json to clipboard, then in Visual Studio: Edit->Paste Special->Paste JSON as Classes.
        // comments within the json file seem to be causing "object" data type where it should be a concrete type ("services", "response", ...). This needs looking into.

        public class Rootobject
        {
            public string version { get; set; }
            public int debugLevel { get; set; }
            public string tempFolderPath { get; set; }
            public string logFilePath { get; set; }
            public string curlFilePath { get; set; }
            public string curlDefaults { get; set; }
            public float pauseSecondsDefault { get; set; }
            public Audio audio { get; set; }
            public Command[] commands { get; set; }
            public Commandservice[] commandServices { get; set; }
            public Geolocaton geolocaton { get; set; }
            public Locale locale { get; set; }
            public Wakeup wakeup { get; set; }
            public Apis APIs { get; set; }
            public Service[] services { get; set; }
        }

        public class Audio
        {
            public string transCodeFileName { get; set; }
            public string speechSynthesisFileName { get; set; }
            public int bitDepth { get; set; }
            public int channels { get; set; }
            public int samplingRate { get; set; }
            public Naudio NAudio { get; set; }
        }

        public class Naudio
        {
            public int desiredLatencyMilliseconds { get; set; }
            public int inputDeviceNumber { get; set; }
            public int waveInBufferMilliseconds { get; set; }
        }

        public class Geolocaton
        {
            public float longitude { get; set; }
            public float latitude { get; set; }
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
            public float confidence { get; set; }
            public float endSilenceTimeout { get; set; }
            public int initialSilenceTimeout { get; set; }
            public float listenTimeOut { get; set; }
            public bool preferLoopUntilWakeUpWordFound { get; set; }
            public string[] words { get; set; }
        }

        public class Apis
        {
            public bool preferSystemNet { get; set; }
            public bool preferChunkedEncodedRequests { get; set; }
        }

        public class Command
        {
            public string key { get; set; }
            public string name { get; set; }
            public string classInterface { get; set; }
            public string help { get; set; }
            public string commandService { get; set; }
            public string source { get; set; }
            public string target { get; set; }
        }

        public class Commandservice
        {
            public string key { get; set; }
            public string name { get; set; }
            public string[] preferredServices { get; set; }
            public int voiceAge { get; set; }
            public int voiceGender { get; set; }
            public int sampleRate { get; set; }
        }

        public class Service
        {
            public string name { get; set; }
            public string classInterface { get; set; }
            public Request[] requests { get; set; }
        }

        public class Request
        {
            public string argType { get; set; }
            public string method { get; set; }
            public bool preferChunkedEncodedRequests { get; set; }
            public bool PartialTranscriptsDesired { get; set; }
            public Uri uri { get; set; }
            public Header[] headers { get; set; }
            public Data data { get; set; }
            public Response response { get; set; }
        }

        public class Uri
        {
            public string scheme { get; set; }
            public string host { get; set; }
            public string path { get; set; }
            public string query { get; set; }
        }

        public class Data
        {
            public string type { get; set; }
            public string value { get; set; }
            public string source { get; set; }
            public string target { get; set; }
        }

        public class Response
        {
            public string missingResponse { get; set; }
            public string jsonPath { get; set; }
            public string xpath { get; set; }
            public string jq { get; set; }
            public string type { get; set; }
        }

        public class Header
        {
            public string Name { get; set; }
            public string Generic { get; set; }
            public string OcpApimSubscriptionKey { get; set; }
            public BearerAuthentication BearerAuthentication { get; set; }
            public HoundifyAuthentication HoundifyAuthentication { get; set; }
            public BasicAuthentication BasicAuthentication { get; set; }
        }

        public class BearerAuthentication
        {
            public string type { get; set; }
            public string clientID { get; set; }
            public string clientSecret { get; set; }
            public string uri { get; set; }
            public string grant { get; set; }
            public string key { get; set; }
            public string bearer { get; set; }
        }

        public class HoundifyAuthentication
        {
            public string ClientID { get; set; }
            public string ClientKey { get; set; }
            public string UserID { get; set; }
        }

        public class BasicAuthentication
        {
            public string type { get; set; }
            public string username { get; set; }
            public string password { get; set; }
        }

    }
}
