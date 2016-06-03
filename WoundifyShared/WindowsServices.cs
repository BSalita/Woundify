using System;

namespace WoundifyShared
{
    class WindowsServices : WoundifyServices
    {
        private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        public WindowsServices(Settings.Service service) : base(service)
        {
            this.service = service;
        }

        public override async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextServiceAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();
            Log.WriteLine("audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);
            stopWatch.Start();
            response.sr = new ServiceResponse(this.ToString());
            response.sr.ResponseResult = await SpeechToText.SpeechToTextServiceAsync(audioBytes);
            response.sr.StatusCode = 200;
            stopWatch.Stop();
            response.sr.TotalElapsedMilliseconds = response.sr.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
            return response;
        }

        public override async System.Threading.Tasks.Task<TextToSpeechServiceResponse> TextToSpeechServiceAsync(string text, int sampleRate)
        {
            TextToSpeechServiceResponse response = new TextToSpeechServiceResponse();
            Log.WriteLine("text:" + text + " sampleRate:" + sampleRate);
            stopWatch.Start();
            response.sr = new ServiceResponse(this.ToString());
            response.sr.ResponseBytes = await TextToSpeech.TextToSpeechServiceAsync(text, sampleRate);
            response.sr.StatusCode = 200;
            stopWatch.Stop();
            response.sr.TotalElapsedMilliseconds = response.sr.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
            return response;
        }
    }
}
