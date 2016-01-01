using System;

namespace WoundifyShared
{
    class WindowsServices : ISpeechToTextService
    {
        private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        public override string ResponseResult { get; set; }
        public override string ResponseJson { get; set; }
        public override string ResponseJsonFormatted { get; set; }
        public override long TotalElapsedMilliseconds { get; set; }
        public override long RequestElapsedMilliseconds { get; set; }
        public override int StatusCode { get; set; }

        public override async System.Threading.Tasks.Task SpeechToTextAsync(byte[] audioBytes, int sampleRate)
        {
            Log.WriteLine("audio file length:" + audioBytes.Length + " sampleRate:" + sampleRate);

            stopWatch.Start();
            ResponseResult = await SpeechToText.SpeechToTextAsync(audioBytes);
            StatusCode = 200;
            stopWatch.Stop();
            TotalElapsedMilliseconds = RequestElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
            Log.WriteLine("Total: Elapsed milliseconds:" + stopWatch.ElapsedMilliseconds);
        }
    }
}
