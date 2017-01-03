using System;

namespace WoundifyShared
{
    class TextToSpeech
    {
#if WINDOWS_UWP
        public static async System.Threading.Tasks.Task<int> SynSpeechWriteToFileAsync(string text, string fileName)
        {
            using (Windows.Media.SpeechSynthesis.SpeechSynthesizer synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer())
            {
                try
                {
                    using (Windows.Media.SpeechSynthesis.SpeechSynthesisStream synthStream = await synth.SynthesizeTextToStreamAsync(text)) // doesn't handle special characters such as quotes
                    {
                        // TODO: obsolete to use DataReader? use await Windows.Storage.FileIO.Read...(file);
                        using (Windows.Storage.Streams.DataReader reader = new Windows.Storage.Streams.DataReader(synthStream))
                        {
                            await reader.LoadAsync((uint)synthStream.Size);
                            Windows.Storage.Streams.IBuffer buffer = reader.ReadBuffer((uint)synthStream.Size);
                            Windows.Storage.StorageFolder tempFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Options.options.tempFolderPath);
                            Windows.Storage.StorageFile srcFile = await tempFolder.CreateFileAsync(Options.options.audio.speechSynthesisFileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                            await Windows.Storage.FileIO.WriteBufferAsync(srcFile, buffer);
                            Windows.Storage.FileProperties.MusicProperties musicProperties = await srcFile.Properties.GetMusicPropertiesAsync();
                            Log.WriteLine("Bitrate:" + musicProperties.Bitrate);

                            Windows.Media.MediaProperties.MediaEncodingProfile profile = Windows.Media.MediaProperties.MediaEncodingProfile.CreateWav(Windows.Media.MediaProperties.AudioEncodingQuality.Low);
                            Windows.Media.Transcoding.MediaTranscoder transcoder = new Windows.Media.Transcoding.MediaTranscoder();
                            Windows.Storage.StorageFile destFile = await tempFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);

                            Windows.Media.Transcoding.PrepareTranscodeResult result = await transcoder.PrepareFileTranscodeAsync(srcFile, destFile, profile);
                            if (result.CanTranscode)
                            {
                                await result.TranscodeAsync();
                            }
                            else
                            {
                                Log.WriteLine("can't transcode file:" + result.FailureReason.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return 0;
        }

        public static async System.Threading.Tasks.Task<int> SynSpeechPlayAsync(string text)
        {
            using (Windows.Media.SpeechSynthesis.SpeechSynthesizer synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer())
            {
                Windows.Media.SpeechSynthesis.SpeechSynthesisStream synthStream = await synth.SynthesizeTextToStreamAsync(text); // doesn't handle special characters such as quotes
                await Audio.PlayFileAsync(synthStream, synthStream.ContentType);
            }
            return 0;
        }
        public static async System.Threading.Tasks.Task<string> TextToSpelledPronunciation(string text)
        {
            return text; // TODO: implement
        }
#else
        public static async System.Threading.Tasks.Task<Byte[]> TextToSpeechServiceAsync(string text, int sampleRate)
        {
            Log.WriteLine("text:\"" + text + "\"");
            //System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(Options.options.locale.language);
            using (System.Speech.Synthesis.SpeechSynthesizer synth = new System.Speech.Synthesis.SpeechSynthesizer())
            {
                // Explicitly specify audio settings. All services are ok with 16000/16/1. It's ok to cast options to enums as their values are identical.
                System.Speech.AudioFormat.SpeechAudioFormatInfo si = new System.Speech.AudioFormat.SpeechAudioFormatInfo(sampleRate, (System.Speech.AudioFormat.AudioBitsPerSample)WoundifyShared.Options.options.audio.bitDepth, (System.Speech.AudioFormat.AudioChannel)WoundifyShared.Options.options.audio.channels);
                // TODO: use memory based file instead
                synth.SetOutputToWaveFile(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName, si);
                synth.SelectVoiceByHints((System.Speech.Synthesis.VoiceGender)Options.commandservices["TextToSpeech"].voiceGender, (System.Speech.Synthesis.VoiceAge)Options.commandservices["TextToSpeech"].voiceAge);
                synth.Speak(text);
            }
            return await Helpers.ReadBytesFromFileAsync(Options.options.audio.speechSynthesisFileName);
        }

        public static async System.Threading.Tasks.Task TextToSpeechServiceAsync(string text)
        {
            //System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(Options.options.locale.language);
            Log.WriteLine("text:" + text);
            using (System.Speech.Synthesis.SpeechSynthesizer synth = new System.Speech.Synthesis.SpeechSynthesizer())
            {
                synth.SelectVoiceByHints((System.Speech.Synthesis.VoiceGender)Options.commandservices["TextToSpeech"].voiceAge);
                synth.Speak(text);
            }
        }

        public static async System.Threading.Tasks.Task<string> TextToSpelledPronunciation(string text)
        {
            //System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(Options.options.locale.language);
            using (System.Speech.Recognition.SpeechRecognitionEngine RecognitionEngine = new System.Speech.Recognition.SpeechRecognitionEngine())
            {
                RecognitionEngine.LoadGrammar(new System.Speech.Recognition.DictationGrammar());
                text = text.Replace(".", ""); // EmulateRecognize returns null if a period is in the text
                System.Speech.Recognition.RecognitionResult result = RecognitionEngine.EmulateRecognize(text);
                if (result == null)
                    throw new FormatException();
                string pronunciations = null;
                foreach (System.Speech.Recognition.RecognizedWordUnit w in result.Words)
                    pronunciations += w.Pronunciation + " ";
                return pronunciations;
            }
        }
#endif
    }
}
