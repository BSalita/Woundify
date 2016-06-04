using System;
using System.Collections.Generic;
using System.Text;

namespace WoundifyShared
{
    public class Settings
    {
        // this class is a concrete implementation of the settings file. It is converted automatically by copying WoundifyDefaultSettings.json to clipboard, then in Visual Studio: Edit->Paste Special->Paste JSON as Classes.
#if true

        public class Rootobject
        {
            public string version { get; set; }
            public int debugLevel { get; set; }
            public string tempFolderPath { get; set; }
            public string logFilePath { get; set; }
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
            public string help { get; set; }
            public string commandService { get; set; }
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
            public string curl { get; set; }
            public Request request { get; set; }
            public Response response { get; set; }
        }

        public class Request
        {
            public string method { get; set; }
            public bool preferChunkedEncodedRequests { get; set; }
            public Uri uri { get; set; }
            public Header[] headers { get; set; }
            public Data data { get; set; }
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

        public class Header
        {
            public string Name { get; set; }
            public string Accept { get; set; }
            public string ContentType { get; set; }
            public string OcpApimSubscriptionKey { get; set; }
            public Bearerauthentication BearerAuthentication { get; set; }
            public Houndifyauthentication HoundifyAuthentication { get; set; }
            public Basicauthentication BasicAuthentication { get; set; }
        }

        public class Bearerauthentication
        {
            public string type { get; set; }
            public string clientID { get; set; }
            public string clientSecret { get; set; }
            public string key { get; set; }
            public string bearer { get; set; }
        }

        public class Houndifyauthentication
        {
            public string ClientID { get; set; }
            public string ClientKey { get; set; }
            public string UserID { get; set; }
        }

        public class Basicauthentication
        {
            public string type { get; set; }
            public string username { get; set; }
            public string password { get; set; }
        }

        public class Response
        {
            public string missingResponse { get; set; }
            public string jsonPath { get; set; }
            public bool PartialTranscriptsDesired { get; set; }
        }
#else
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

        public class GoogleCloudSpeechToText
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

        public class IbmWatsonSpeechToText
        {
            public bool callSpeechToText { get; set; }
            public string username { get; set; }
            public string password { get; set; }
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
            public GoogleCloudSpeechToText GoogleCloudSpeechToText { get; set; }
            public HoundifySpeechToText HoundifySpeechToText { get; set; }
            public IbmWatsonSpeechToText IbmWatsonSpeechToText { get; set; }
            public WindowsSpeechToText WindowsSpeechToText { get; set; }
            public WitSpeechToText WitSpeechToText { get; set; }
        }

        public class IbmWatsonTextToSpeech
        {
            public bool callSpeechToText { get; set; }
            public string username { get; set; }
            public string password { get; set; }
        }

        public class WindowsTextToSpeech
        {
            public bool callSpeechToText { get; set; }
            public int voiceAge { get; set; }
            public int voiceGender { get; set; }
        }

        public class TextToSpeech
        {
            public string missingResponse { get; set; }
            public System.Collections.Generic.List<string> preferredTextToSpeechServices { get; set; }
            public IbmWatsonTextToSpeech IbmWatsonTextToSpeech { get; set; }
            public WindowsTextToSpeech WindowsTextToSpeech { get; set; }
        }

        public class Tone
        {
            public string missingResponse { get; set; }
            public System.Collections.Generic.List<string> preferredToneServices { get; set; }
            public IbmWatsonTextToSpeech IbmWatsonTone { get; set; }
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
            public TextToSpeech TextToSpeech { get; set; }
            public Tone Tone { get; set; }
        }

        public class Services
        {
            public APIs APIs { get; set; }
        }

        public class Rootobject
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
#endif
    }
}
