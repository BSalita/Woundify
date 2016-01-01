using System;

namespace WoundifyShared
{
    public abstract class IntentService : ISpeechToTextService
    {
        abstract public System.Threading.Tasks.Task IntentAsync(string text);
        abstract public System.Threading.Tasks.Task IntentAsync(byte[] audioBytes, int sampleRate);
    }
    public abstract class ISpeechToTextService : IResponse
    {
        abstract public System.Threading.Tasks.Task SpeechToTextAsync(byte[] audioBytes, int sampleRate);
    }
    public abstract class IResponse
    {
        abstract public string ResponseResult { get; set; }
        abstract public string ResponseJson { get; set; }
        abstract public string ResponseJsonFormatted { get; set; }
        abstract public long TotalElapsedMilliseconds { get; set; }
        abstract public long RequestElapsedMilliseconds { get; set; }
        abstract public int StatusCode { get; set; }
    }
}
