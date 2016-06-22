using System;

namespace WoundifyShared
{
    public class ServiceResponse
    {
        public ServiceResponse(string _ServiceName)
        {
            ServiceName = _ServiceName;
        }
        public string ResponseString { get; set; } // for string responses
        public byte[] ResponseBytes { get; set; } // for byte[] responses
        public string ResponseJson { get; set; } // for JSON responses
        public Newtonsoft.Json.Linq.JToken ResponseJToken { get; set; } // JToken if response was JSON
        public string ResponseJsonFormatted { get; set; } // formatted Json, if Json response
        public string ResponseResult { get; set; } // result
        public string ServiceName { get; set; }
        public int StatusCode { get; set; }
        public long RequestElapsedMilliseconds { get; set; }
        public long TotalElapsedMilliseconds { get; set; }
        internal System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
    }
    public class WoundifyService
    {
        public Settings.Service service;
        public WoundifyService(Settings.Service service)
        {
            this.service = service;
        }
        public virtual async System.Threading.Tasks.Task<ServiceResponse> ServiceAsync(string text)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<ServiceResponse> ServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
        public ServiceResponse sr;
    }
    public interface IService
    {
    }
    public interface IAnnotateService : IService
    {
        System.Threading.Tasks.Task<AnnotateServiceResponse> AnnotateServiceAsync(string text);
        System.Threading.Tasks.Task<AnnotateServiceResponse> AnnotateServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class AnnotateServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface IEntitiesService : IService
    {
        System.Threading.Tasks.Task<EntitiesServiceResponse> EntitiesServiceAsync(string text);
        System.Threading.Tasks.Task<EntitiesServiceResponse> EntitiesServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class EntitiesServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface IIdentifyLanguageService : IService
    {
        System.Threading.Tasks.Task<IdentifyLanguageServiceResponse> IdentifyLanguageServiceAsync(string text);
        System.Threading.Tasks.Task<IdentifyLanguageServiceResponse> IdentifyLanguageServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class IdentifyLanguageServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface IIntentService : IService
    {
        System.Threading.Tasks.Task<IntentServiceResponse> IntentServiceAsync(string text);
        System.Threading.Tasks.Task<IntentServiceResponse> IntentServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class IntentServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface IParaphraseService : IService
    {
        System.Threading.Tasks.Task<ParaphraseServiceResponse> ParaphraseServiceAsync(string text);
        System.Threading.Tasks.Task<ParaphraseServiceResponse> ParaphraseServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class ParaphraseServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface IParseAnalyzersService : IService
    {
        System.Threading.Tasks.Task<ParseAnalyzersServiceResponse> ParseAnalyzersServiceAsync();
    }
    public class ParseAnalyzersServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface IParseService : IService
    {
        System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(string text);
        System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class ParseServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface IPersonalityService : IService
    {
        System.Threading.Tasks.Task<PersonalityServiceResponse> PersonalityServiceAsync(string text);
        System.Threading.Tasks.Task<PersonalityServiceResponse> PersonalityServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class PersonalityServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface ISpeechToTextService : IService
    {
        System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class SpeechToTextServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface ISpellService : IService
    {
        System.Threading.Tasks.Task<SpellServiceResponse> SpellServiceAsync(string text);
        System.Threading.Tasks.Task<SpellServiceResponse> SpellServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class SpellServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface ITextToSpeechService : IService
    {
        System.Threading.Tasks.Task<TextToSpeechServiceResponse> TextToSpeechServiceAsync(string text, int sampleRate);
    }
    public class TextToSpeechServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface IToneService : IService
    {
        System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(string text);
        System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class ToneServiceResponse
    {
        public ServiceResponse sr;
    }
    public interface ITranslateService : IService
    {
        System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(string text, string source, string target);
        System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(byte[] audioBytes, int sampleRate, string source, string target);
    }
    public class TranslateServiceResponse
    {
        public ServiceResponse sr;
    }
#if false
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
        public virtual System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(byte[] audioBytes, int sampleRate)
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
#endif

#if true
    public class WoundifyServices : HttpMethods, IAnnotateService, IEntitiesService, IIdentifyLanguageService, IIntentService, IParaphraseService, IParseService, IPersonalityService, ISpeechToTextService, ISpellService, ITextToSpeechService, IToneService, ITranslateService
    {
        public Settings.Service service;
        public WoundifyServices(Settings.Service service)
        {
            this.service = service;
        }
        public virtual async System.Threading.Tasks.Task<AnnotateServiceResponse> AnnotateServiceAsync(string text)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<AnnotateServiceResponse> AnnotateServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse sttr = await SpeechToTextServices.PreferredOrderingSpeechToTextServices[0].SpeechToTextServiceAsync(audioBytes, sampleRate);
            return await AnnotateServices.PreferredOrderingAnnotateServices[0].AnnotateServiceAsync(sttr.sr.ResponseResult);
        }
        public virtual async System.Threading.Tasks.Task<EntitiesServiceResponse> EntitiesServiceAsync(string text)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<EntitiesServiceResponse> EntitiesServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse sttr = await SpeechToTextServices.PreferredOrderingSpeechToTextServices[0].SpeechToTextServiceAsync(audioBytes, sampleRate);
            return await EntitiesServices.PreferredOrderingEntitiesServices[0].EntitiesServiceAsync(sttr.sr.ResponseResult);
        }
        public virtual async System.Threading.Tasks.Task<IdentifyLanguageServiceResponse> IdentifyLanguageServiceAsync(string text)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<IdentifyLanguageServiceResponse> IdentifyLanguageServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse sttr = await SpeechToTextServices.PreferredOrderingSpeechToTextServices[0].SpeechToTextServiceAsync(audioBytes, sampleRate);
            return await IdentifyLanguageServices.PreferredOrderingIdentifyLanguageServices[0].IdentifyLanguageServiceAsync(sttr.sr.ResponseResult);
        }
        public virtual async System.Threading.Tasks.Task<IntentServiceResponse> IntentServiceAsync(string text)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<IntentServiceResponse> IntentServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse sttr = await SpeechToTextServices.PreferredOrderingSpeechToTextServices[0].SpeechToTextServiceAsync(audioBytes, sampleRate);
            return await IntentServices.PreferredOrderingIntentServices[0].IntentServiceAsync(sttr.sr.ResponseResult);
        }
        public virtual async System.Threading.Tasks.Task<ParaphraseServiceResponse> ParaphraseServiceAsync(string text)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<ParaphraseServiceResponse> ParaphraseServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse sttr = await SpeechToTextServices.PreferredOrderingSpeechToTextServices[0].SpeechToTextServiceAsync(audioBytes, sampleRate);
            return await ParaphraseServices.PreferredOrderingParaphraseServices[0].ParaphraseServiceAsync(sttr.sr.ResponseResult);
        }
        public virtual async System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(string text)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<ParseServiceResponse> ParseServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse sttr = await SpeechToTextServices.PreferredOrderingSpeechToTextServices[0].SpeechToTextServiceAsync(audioBytes, sampleRate);
            return await ParseServices.PreferredOrderingParseServices[0].ParseServiceAsync(sttr.sr.ResponseResult);
        }
        public virtual async System.Threading.Tasks.Task<PersonalityServiceResponse> PersonalityServiceAsync(string text)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<PersonalityServiceResponse> PersonalityServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse sttr = await SpeechToTextServices.PreferredOrderingSpeechToTextServices[0].SpeechToTextServiceAsync(audioBytes, sampleRate);
            return await PersonalityServices.PreferredOrderingPersonalityServices[0].PersonalityServiceAsync(sttr.sr.ResponseResult);
        }
        public virtual async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate, System.Collections.Generic.List<Tuple<string, string>> Headers)
        {
            UriBuilder ub = new UriBuilder();
            ub.Scheme = service.request.uri.scheme;
            ub.Host = service.request.uri.host;
            ub.Path = service.request.uri.path;
            ub.Query = service.request.uri.query;
            return await SpeechToTextServiceAsync(ub.Uri, audioBytes, sampleRate, Headers);
        }
        public virtual async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(Uri uri, byte[] audioBytes, int sampleRate, System.Collections.Generic.List<Tuple<string, string>> headers)
        {
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();
            Log.WriteLine("audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);
            response.sr = await PostAsync(uri, audioBytes, headers);
            return response;
        }
        public virtual async System.Threading.Tasks.Task<SpellServiceResponse> SpellServiceAsync(string text)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<SpellServiceResponse> SpellServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse sttr = await SpeechToTextServices.PreferredOrderingSpeechToTextServices[0].SpeechToTextServiceAsync(audioBytes, sampleRate);
            return await SpellServices.PreferredOrderingSpellServices[0].SpellServiceAsync(sttr.sr.ResponseResult);
        }
        public virtual async System.Threading.Tasks.Task<TextToSpeechServiceResponse> TextToSpeechServiceAsync(string text, int sampleRate)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(string text)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<ToneServiceResponse> ToneServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse sttr = await SpeechToTextServices.PreferredOrderingSpeechToTextServices[0].SpeechToTextServiceAsync(audioBytes, sampleRate);
            return await ToneServices.PreferredOrderingToneServices[0].ToneServiceAsync(sttr.sr.ResponseResult);
        }
        public virtual async System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(string text, string source, string target)
        {
            return null;
        }
        public virtual async System.Threading.Tasks.Task<TranslateServiceResponse> TranslateServiceAsync(byte[] audioBytes, int sampleRate, string source, string target)
        {
            SpeechToTextServiceResponse sttr = await SpeechToTextServices.PreferredOrderingSpeechToTextServices[0].SpeechToTextServiceAsync(audioBytes, sampleRate);
            return await TranslateServices.PreferredOrderingTranslateServices[0].TranslateServiceAsync(sttr.sr.ResponseResult, source, target);
        }
    }
#endif
}
