using System;

namespace WoundifyShared
{
    public interface IService
    {
    }
    public interface IServiceResponse
    {
        Settings.Service Service { get; set; }
        Settings.Request Request { get; set; }
        string ResponseString { get; set; } // for string responses
        byte[] ResponseBytes { get; set; } // for byte[] responses
        string ResponseJson { get; set; } // for JSON responses
        Newtonsoft.Json.Linq.JToken ResponseJToken { get; set; } // JToken if response was JSON
        string ResponseJsonFormatted { get; set; } // formatted Json, if Json response
        System.Xml.XmlDocument ResponseXml { get; set; }
        System.Xml.XmlNodeList ResponseXmlNodeList { get; set; }
        string ResponseXmlFormatted { get; set; }
        string ResponseResult { get; set; } // result
        string FileName { get; set; }
        int StatusCode { get; set; }
        long RequestElapsedMilliseconds { get; set; }
        long TotalElapsedMilliseconds { get; set; }
        System.Diagnostics.Stopwatch stopWatch { get; set; }
    }
    public class ServiceResponse : IServiceResponse
    {
        public Settings.Service Service { get; set; }
        public Settings.Request Request { get; set; }
        public string ResponseString { get; set; } // for string responses
        public byte[] ResponseBytes { get; set; } // for byte[] responses
        public string ResponseJson { get; set; } // for JSON responses
        public Newtonsoft.Json.Linq.JToken ResponseJToken { get; set; } // JToken if response was JSON
        public string ResponseJsonFormatted { get; set; } // formatted Json, if Json response
        public System.Xml.XmlDocument ResponseXml { get; set; }
        public System.Xml.XmlNodeList ResponseXmlNodeList { get; set; }
        public string ResponseXmlFormatted { get; set; }
        public string ResponseResult { get; set; } // result
        public string FileName { get; set; }
        public int StatusCode { get; set; }
        public long RequestElapsedMilliseconds { get; set; }
        public long TotalElapsedMilliseconds { get; set; }
        public System.Diagnostics.Stopwatch stopWatch { get; set; } = new System.Diagnostics.Stopwatch();
    }
    public interface IProcessACommand
    {
        int stackChange { get; set; }
        System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices);
    }

    // todo: obsolete all of the following?
#if false // sample non-Generic call services
    public interface IPronounceCallServices : ICallServices<IPronounceServiceResponse>
    {
    }
    public class PronounceCallServices : RunServices<IPronounceService, IPronounceServiceResponse>
    {
    }
#endif
    public interface IIntentService : IService
    {
    }
    public class IntentServiceResponse : GenericServiceResponse
    {
    }
    public interface IParseAnalyzersService : IService
    {
    }
    public class ParseAnalyzersServiceResponse : ParseServiceResponse
    {
    }
    public interface IParseService : IService
    {
        System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(string text, System.Collections.Generic.Dictionary<string, string> apiArgs);
    }
    public class ParseServiceResponse : GenericServiceResponse
    {
    }
    public interface IPronounceService : IGenericService
    {
    }
    public interface IPronounceServiceResponse : IGenericServiceResponse
    {
    }
    public class PronounceServiceResponse : GenericServiceResponse
    {
    }
    public interface ISpeechToTextService : IService
    {
        System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, System.Collections.Generic.Dictionary<string, string> apiArgs);
        System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(Uri uri, System.Collections.Generic.Dictionary<string, string> apiArgs);
    }
    public interface ISpeechToTextServiceResponse : IGenericServiceResponse
    {
    }
    public class SpeechToTextServices : RunServices<ISpeechToTextService, ISpeechToTextServiceResponse>
    {
    }
    public class SpeechToTextServiceResponse : GenericServiceResponse, ISpeechToTextServiceResponse
    {
    }
    public interface ITextToSpeechService : IService
    {
        System.Threading.Tasks.Task<TextToSpeechServiceResponse> TextToSpeechServiceAsync(string text, System.Collections.Generic.Dictionary<string, string> apiArgs);
        System.Threading.Tasks.Task<TextToSpeechServiceResponse> TextToSpeechServiceAsync(Uri url, System.Collections.Generic.Dictionary<string, string> apiArgs);
    }
    public class TextToSpeechServices : RunServices<ITextToSpeechService, TextToSpeechServiceResponse>
    {
    }
    public interface ITextToSpeechServiceResponse : IGenericServiceResponse
    {
    }
    public class TextToSpeechServiceResponse : GenericServiceResponse, ITextToSpeechServiceResponse
    {
    }
}
