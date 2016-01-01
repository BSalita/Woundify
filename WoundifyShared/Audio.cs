using System;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls; // for MediaElement
#endif

namespace WoundifyShared
{
    class Audio
    {
#if WINDOWS_UWP
        private static MediaElement mediaElement; // MediaElement is sensitive to async issues. Needs special care to prevent random bombs.
        static Audio()
        {
            Log.WriteLine("called");
            mediaElement = new MediaElement();
            mediaElement.MediaOpened += MediaElement_MediaOpened;
            mediaElement.MediaEnded += MediaElement_MediaEnded;
        }

        private static void MediaElement_MediaEnded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Log.WriteLine("called");
        }

        private static void MediaElement_MediaOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Log.WriteLine("called");
            mediaElement.Play(); // Play can only commence once the source is opened. Otherwise it causes weird errors throughout the application.
        }

        public static async System.Threading.Tasks.Task PlayFileAsync(Windows.Storage.Streams.IRandomAccessStream stream, string ContentType)
        {
            Log.WriteLine("ContentType:" + ContentType);
            mediaElement.Stop();
            mediaElement.SetSource(stream, ContentType);
            mediaElement.Play();
        }

        public static async System.Threading.Tasks.Task PlayFileAsync(string fileName)
        {
            Log.WriteLine("fileName:" + fileName);
            Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(Options.options.tempFolderPath + fileName);
            // MediaElement control has more features
            mediaElement.Stop();
            Windows.Media.Core.MediaSource mediaSource = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
            mediaElement.SetPlaybackSource(mediaSource);
        }

        public static async System.Threading.Tasks.Task<int> GetSampleRateAsync(string fileName)
        {
            Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(Options.options.tempFolderPath + fileName);
            Windows.Storage.FileProperties.MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();
            Log.WriteLine("Synthesized Speech wave file: sample rate:" + musicProperties.Bitrate / Options.options.audio.bitDepth);
            return (int)musicProperties.Bitrate / Options.options.audio.bitDepth;
        }

        public static async System.Threading.Tasks.Task MicrophoneToFileAsync(string fileName, TimeSpan timeout)
        {
            Log.WriteLine("fileName:" + fileName + " timeout:" + timeout);
            AudioUtilitiesUWP.Audio audio = new AudioUtilitiesUWP.Audio(); // todo: make permanent graphs of: mic-to-file, mic-to-speaker, file-to-speaker
            await audio.CreateAudioGraphAsync(Options.options.tempFolderPath + fileName);
            audio.Start();
            System.Threading.Tasks.Task.Delay(timeout).Wait();
            audio.Stop();
            audio.Dispose();
        }
#else
        private static NAudio.Wave.WaveFileWriter waveInFile = null;

        public static async System.Threading.Tasks.Task PlayFileAsync(string fileName)
        {
            using (NAudio.Wave.WaveFileReader mic = new NAudio.Wave.WaveFileReader(fileName))
            {
                NAudio.Wave.WaveOut waveOut = new NAudio.Wave.WaveOut();
                waveOut.DesiredLatency = Options.options.audio.NAudio.desiredLatencyMilliseconds;
                waveOut.Init(mic);
                waveOut.Play();
                while (waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                    System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
            }
        }

        public static async System.Threading.Tasks.Task MicrophoneToFileAsync(string fileName, TimeSpan timeout)
        {
            Console.Write("Listening for " + timeout.TotalSeconds + " seconds ...");
            // create wave input using microphone
            using (NAudio.Wave.WaveInEvent waveIn = new NAudio.Wave.WaveInEvent())
            {
                waveIn.DeviceNumber = Options.options.audio.NAudio.inputDeviceNumber;
                waveIn.BufferMilliseconds = Options.options.audio.NAudio.waveInBufferMilliseconds;
                waveIn.WaveFormat = new NAudio.Wave.WaveFormat(Options.options.audio.samplingRate, (int)Options.options.audio.bitDepth, Options.options.audio.channels); // usually only mono (one channel) is supported
                waveIn.DataAvailable += WaveIn_DataAvailable; // use event to fill buffer
                using (waveInFile = new NAudio.Wave.WaveFileWriter(Options.options.tempFolderPath + fileName, waveIn.WaveFormat))
                {
                    waveIn.StartRecording();

                    //Console.WriteLine("Hit enter when finished recording.");
                    //Console.ReadKey();
                    System.Threading.Tasks.Task.Delay(timeout).Wait();

                    waveIn.StopRecording();

                    waveInFile.Close();
                }
                Console.WriteLine("");
            }
        }

        private static void WaveIn_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            if (Options.options.debugLevel >= 4)
                Log.WriteLine("Received buffer: length=" + e.BytesRecorded.ToString());
            else
                Console.Write("*");
            // can't derive waveInFile from WaveInEvent so must use class property

            // add received wave audio to waveProvider buffer
            //if (waveInProvider != null)
            //waveInProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);

            // add received wave audio to memory stream
            if (waveInFile != null)
            {
                waveInFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveInFile.Flush();
            }
        }

        public static async System.Threading.Tasks.Task<int> GetSampleRateAsync(string fileName)
        {
            using (NAudio.Wave.WaveFileReader file = new NAudio.Wave.WaveFileReader(fileName))
            {
                return file.WaveFormat.SampleRate;
            }
        }

#endif
    }
}
