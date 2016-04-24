using System;

namespace WoundifyShared 
{
    class WindowsServices : WoundifyServices
    {
        private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        public override async System.Threading.Tasks.Task<SpeechToTextServiceResponse> SpeechToTextAsync(byte[] audioBytes, int sampleRate)
        {
            SpeechToTextServiceResponse response = new SpeechToTextServiceResponse();
            Log.WriteLine("audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);
            stopWatch.Start();
            response.sr = new ServiceResponse(this.ToString());
            response.sr.ResponseResult = await SpeechToText.SpeechToTextAsync(audioBytes);
            response.sr.StatusCode = 200;
            stopWatch.Stop();
            response.sr.TotalElapsedMilliseconds = response.sr.RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
            return response;
        }
    }
}
