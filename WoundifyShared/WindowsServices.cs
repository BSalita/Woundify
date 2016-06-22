using System;

namespace WoundifyShared
{
    class WindowsServices : WoundifyServices
    {
        public WindowsServices(Settings.Service service) : base(service)
        {
            this.service = service;
        }

        public override async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();

            Log.WriteLine("audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);
            response.sr = new ServiceResponse(this.ToString());
            response.sr.stopWatch.Start();
            response.sr.ResponseResult = await SpeechToText.SpeechToTextServiceAsync(audioBytes);
            response.sr.StatusCode = 200;
            response.sr.stopWatch.Stop();
            response.sr.TotalElapsedMilliseconds = response.sr.RequestElapsedMilliseconds = response.sr.stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total Elapsed milliseconds:" + response.sr.TotalElapsedMilliseconds);
            return response;
        }

        public override async System.Threading.Tasks.Task<TextToSpeechServiceResponse> TextToSpeechServiceAsync(string text, int sampleRate)
        {
            TextToSpeechServiceResponse response = new TextToSpeechServiceResponse();
            Log.WriteLine("text:" + text + " sampleRate:" + sampleRate);
            response.sr = new ServiceResponse(this.ToString());
            response.sr.stopWatch.Start();
            response.sr.ResponseBytes = await TextToSpeech.TextToSpeechServiceAsync(text, sampleRate);
            response.sr.StatusCode = 200;
            response.sr.stopWatch.Stop();
            response.sr.TotalElapsedMilliseconds = response.sr.RequestElapsedMilliseconds = response.sr.stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total Elapsed milliseconds:" + response.sr.TotalElapsedMilliseconds);
            return response;
        }
    }
}
