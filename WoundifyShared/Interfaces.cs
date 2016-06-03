using System;

namespace WoundifyShared
{
    public class ServiceResponse
    {
        public ServiceResponse(string _ServiceName)
        {
            ServiceName = _ServiceName;
        }
        public string ResponseBodyBlob { get; set; }
        public Newtonsoft.Json.Linq.JToken ResponseBodyToken { get; set; }
        public long RequestElapsedMilliseconds { get; set; }
        public string ResponseJson { get; set; }
        public string ResponseJsonFormatted { get; set; }
        public string ResponseResult { get; set; }
        public byte[] ResponseBytes { get; set; }
        public string ServiceName { get; set; }
        public int StatusCode { get; set; }
        public long TotalElapsedMilliseconds { get; set; }
    }
    public interface IIdentifyLanguageService
    {
        System.Threading.Tasks.Task<IdentifyLanguageServiceResponse> IdentifyLanguageServiceAsync(string text);
        System.Threading.Tasks.Task<IdentifyLanguageServiceResponse> IdentifyLanguageServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class IdentifyLanguageServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface IIntentService
    {
        System.Threading.Tasks.Task<IntentServiceResponse> IntentServiceAsync(string text);
        System.Threading.Tasks.Task<IntentServiceResponse> IntentServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class IntentServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface IParseService
    {
        System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(string text);
    }
    public class ParseServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface IPersonalityService
    {
        System.Threading.Tasks.Task<PersonalityServiceResponse> PersonalityServiceAsync(string text);
        System.Threading.Tasks.Task<PersonalityServiceResponse> PersonalityServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class PersonalityServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface ISpeechToTextService
    {
        System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class SpeechToTextServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface ITextToSpeechService
    {
        System.Threading.Tasks.Task<TextToSpeechServiceResponse> TextToSpeechServiceAsync(string text, int sampleRate);
    }
    public class TextToSpeechServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface IToneService
    {
        System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(string text);
        System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class ToneServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface ITranslateService
    {
        System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(string text);
        System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class TranslateServiceResponse
    {
        public ServiceResponse sr;
    }
    public class GenericService
    {
        public Settings.Service service { get; set; }
        public GenericService(Settings.Service service)
        {
            this.service = service;
        }
    }
    public class IntentService : GenericService, IIntentService
    {
        public IntentService(Settings.Service service) : base(service)
        {
            this.service = service;
        }
        public virtual System.Threading.Tasks.Task<IntentServiceResponse> IntentServiceAsync(string text)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<IntentServiceResponse> IntentServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
    }
    public class ParseService : GenericService, IParseService
    {
        public ParseService(Settings.Service service) : base(service)
        {
            this.service = service;
        }
        public virtual System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(string text)
        {
            return null;
        }
    }
    public class SpeechToTextService : GenericService, ISpeechToTextService
    {
        public SpeechToTextService(Settings.Service service) : base(service)
        {
            this.service = service;
        }
        public virtual System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
    }
    public class TextToSpeechService : GenericService, ITextToSpeechService
    {
        public TextToSpeechService(Settings.Service service) : base(service)
        {
            this.service = service;
        }
        public virtual System.Threading.Tasks.Task<TextToSpeechServiceResponse> TextToSpeechServiceAsync(string text, int sampleRate)
        {
            return null;
        }
    }
    public class ToneService : GenericService, IToneService
    {
        public ToneService(Settings.Service service) : base(service)
        {
            this.service = service;
        }
        public virtual System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(string text)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
    }

#if true
    public class WoundifyServices : IIdentifyLanguageService, IIntentService, IParseService, IPersonalityService, ISpeechToTextService, ITextToSpeechService, IToneService, ITranslateService
    {
        public Settings.Service service;
        public WoundifyServices(Settings.Service service)
        {
            this.service = service;
        }
        public virtual System.Threading.Tasks.Task<IdentifyLanguageServiceResponse> IdentifyLanguageServiceAsync(string text)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<IdentifyLanguageServiceResponse> IdentifyLanguageServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<IntentServiceResponse> IntentServiceAsync(string text)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<IntentServiceResponse> IntentServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(string text)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<PersonalityServiceResponse> PersonalityServiceAsync(string text)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<PersonalityServiceResponse> PersonalityServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<TextToSpeechServiceResponse> TextToSpeechServiceAsync(string text, int sampleRate)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(string text)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(string text)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
    }
#endif
}
