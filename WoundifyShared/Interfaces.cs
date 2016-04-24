using System;

namespace WoundifyShared
{
    public class ServiceResponse
    {
        public ServiceResponse(string _ServiceName)
        {
            ServiceName = _ServiceName;
        }
        public string ServiceName { get; set; }
        public string ResponseResult { get; set; }
        public string ResponseJson { get; set; }
        public string ResponseJsonFormatted { get; set; }
        public long TotalElapsedMilliseconds { get; set; }
        public long RequestElapsedMilliseconds { get; set; }
        public int StatusCode { get; set; }
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
    public interface ISpeechToTextService
    {
        System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextAsync(byte[] audioBytes, int sampleRate);
    }
    public class SpeechToTextServiceResponse
    {
        public ServiceResponse sr;
    }
    public class WoundifyServices : IIntentService, IParseService, ISpeechToTextService
    {
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
        public virtual System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextAsync(byte[] audioBytes, int sampleRate)
        {
            return null;
        }
    }
}
