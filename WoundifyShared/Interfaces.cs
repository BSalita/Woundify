using System;

namespace WoundifyShared
{
    public class IServiceResponse
    {
        public string ResponseResult { get; set; }
        public string ResponseJson { get; set; }
        public string ResponseJsonFormatted { get; set; }
        public long TotalElapsedMilliseconds { get; set; }
        public long RequestElapsedMilliseconds { get; set; }
        public int StatusCode { get; set; }
    }
    public interface IIntentService
    {
        System.Threading.Tasks.Task<IIntentServiceResponse> IntentServiceAsync(string text);
        System.Threading.Tasks.Task<IIntentServiceResponse> IntentServiceAsync(byte[] audioBytes, int sampleRate);
    }
    public class IIntentServiceResponse
    {
        public IServiceResponse sr;
    }
    public interface IParseService
    {
        System.Threading.Tasks.Task<IParseServiceResponse> ParseServiceAsync(string text);
    }
    public class IParseServiceResponse
    {
        public IServiceResponse sr;
    }
    public interface ISpeechToTextService
    {
        System.Threading.Tasks.Task<ISpeechToTextServiceResponse> SpeechToTextAsync(byte[] audioBytes, int sampleRate);
    }
    public class ISpeechToTextServiceResponse
    {
        public IServiceResponse sr;
    }
    public class WoundifyServices : IIntentService, IParseService, ISpeechToTextService
    {

        public virtual System.Threading.Tasks.Task<IIntentServiceResponse> IntentServiceAsync(string text)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<IIntentServiceResponse> IntentServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<IParseServiceResponse> ParseServiceAsync(string text)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<IParseServiceResponse> ParseServiceAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
        public virtual System.Threading.Tasks.Task<ISpeechToTextServiceResponse> SpeechToTextAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
    }
}
