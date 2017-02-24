using System;

namespace WoundifyShared
{
    class SpeechToText
    {
#if WINDOWS_UWP
        // As of this time, UWP only offers microphone input to SpeechRecognizer, not file input
        public static async System.Threading.Tasks.Task<string> MicrophoneToTextAsync()
        {
            Windows.Media.SpeechRecognition.SpeechRecognizer speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();
            speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;

            // Compile the dictation grammar by default.
            await speechRecognizer.CompileConstraintsAsync();

            // Start recognition.
            Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();

            Log.WriteLine("Text:" + speechRecognitionResult.Text);
            return speechRecognitionResult.Text;
        }

        private static void SpeechRecognizer_HypothesisGenerated(Windows.Media.SpeechRecognition.SpeechRecognizer sender, Windows.Media.SpeechRecognition.SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            Log.WriteLine(args.Hypothesis.Text.ToString());
        }

        public static async System.Threading.Tasks.Task<string> SpeechToTextAsync()
        {
            return await SpeechToTextAsync(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName);
        }

        public static async System.Threading.Tasks.Task<string> SpeechToTextAsync(string fileName)
        {
            return await SpeechToTextAsync(await Helpers.ReadBytesFromFileAsync(text));
        }

        public static async System.Threading.Tasks.Task<string> SpeechToTextAsync(byte[] bytes)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
            return await SpeechToTextAsync(stream);
        }

        public static async System.Threading.Tasks.Task<string> SpeechToTextAsync(System.IO.MemoryStream stream)
        {
            int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName);
            System.Collections.Generic.Dictionary<string, string> apiArgs = new System.Collections.Generic.Dictionary<string, string>() { { "sampleRate", sampleRate.ToString() } };
            foreach (ISpeechToTextService STT in SpeechToTextServices.PreferredOrderingSpeechToTextServices)
            {
                string text;
                ISpeechToTextServiceResponse r = await STT.SpeechToTextAsync(stream.ToArray(), apiArgs);
                if (r.StatusCode == 200)
                {
                    text = r.ResponseResult;
                    Console.WriteLine(r.service.Name + ":\"" + text + "\" Total Elapsed ms:" + r.TotalElapsedMilliseconds + " Request Elapsed ms:" + r.RequestElapsedMilliseconds);
                    return text;
                }
                else
                {
                    Console.WriteLine(r.service.Name + " not available.");
                }
            }
            throw new Exception("All SpeechToText responses have failed. Are you properly connected to the Internet?");
        }
#else
        private static System.Threading.ManualResetEvent WakeUpEvent = new System.Threading.ManualResetEvent(false);
        //private static System.Speech.Recognition.SpeechRecognizedEventArgs WakeUpWordResults;

        public static async System.Threading.Tasks.Task<string> MicrophoneToTextAsync()
        {
            CallServices<ISpeechToTextService, ISpeechToTextServiceResponse> wSTT = new CallServices<ISpeechToTextService, ISpeechToTextServiceResponse>(null);
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(Options.options.locale.language);
            using (System.Speech.Recognition.SpeechRecognitionEngine RecognitionEngine = new System.Speech.Recognition.SpeechRecognitionEngine(ci))
            {
                RecognitionEngine.SetInputToDefaultAudioDevice();
                RecognitionEngine.LoadGrammar(new System.Speech.Recognition.DictationGrammar());
                System.Speech.Recognition.RecognitionResult WakeUpWordResult = RecognitionEngine.Recognize();
                if (WakeUpWordResult == null)
                    return null;
                using (System.IO.FileStream waveStream = new System.IO.FileStream(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName, System.IO.FileMode.Create))
                {
                    WakeUpWordResult.Audio.WriteToWaveStream(waveStream);
                    waveStream.Flush();
                    waveStream.Close();
                }
                byte[] bytes = await Helpers.ReadBytesFromFileAsync(Options.options.audio.speechSynthesisFileName);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
                string text = WakeUpWordResult.Text;
                int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName);
                System.Collections.Generic.Dictionary<string, string> apiArgs = new System.Collections.Generic.Dictionary<string, string>() { { "sampleRate", sampleRate.ToString() } };
                foreach (ISpeechToTextService STT in new FindServices<ISpeechToTextService>(Options.commandservices["SpeechToText"].preferredServices).PreferredOrderingOfServices)
                {
                    // ISpeechToTextService STT = (ISpeechToTextService)constructor.Invoke(Type.EmptyTypes);;
                    SpeechToTextServiceResponse r = await STT.SpeechToTextServiceAsync(bytes, apiArgs);
                    if (r.StatusCode == 200)
                    {
                        text = r.ResponseResult;
                        Console.WriteLine(r.Service.name + ":\"" + text + "\" Total Elapsed ms:" + r.TotalElapsedMilliseconds + " Request Elapsed ms:" + r.RequestElapsedMilliseconds);
                        return text;
                    }
                    else
                    {
                        Console.WriteLine(r.Service.name + " not available.");
                    }
                }
                CallServiceResponse<ISpeechToTextServiceResponse> response = await wSTT.CallServiceAsync(bytes, apiArgs);
                text = response.ResponseResult;
                Console.WriteLine("Windows STT (default):\"" + text + "\" Total Elapsed ms:" + response.TotalElapsedMilliseconds + " Request Elapsed ms:" + response.RequestElapsedMilliseconds);
                return text;
            }
        }

        public static async System.Threading.Tasks.Task<string[]> WaitForWakeUpWordThenRecognizeRemainingSpeechAsync(string[] WakeUpWords)
        {
            Console.WriteLine("Say the wakeup word (" + string.Join(" ", WakeUpWords) + ") followed by the request ...");
            CallServices<ISpeechToTextService, ISpeechToTextServiceResponse> wSTT = new CallServices<ISpeechToTextService, ISpeechToTextServiceResponse>(null);
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start(); // continues until return
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(Options.options.locale.language);
            while (true)
            {
                using (System.Speech.Recognition.SpeechRecognitionEngine RecognitionEngine = new System.Speech.Recognition.SpeechRecognitionEngine(ci))
                {
                    RecognitionEngine.SetInputToDefaultAudioDevice();

                    // build wakeup word grammar
                    System.Speech.Recognition.GrammarBuilder wakeUpWordBuilder = new System.Speech.Recognition.GrammarBuilder();
                    wakeUpWordBuilder.Append(new System.Speech.Recognition.Choices(WakeUpWords));

                    // build words-after-wakeup word grammar
                    System.Speech.Recognition.GrammarBuilder wordsAfterWakeUpWordBuilder = new System.Speech.Recognition.GrammarBuilder();
                    wordsAfterWakeUpWordBuilder.AppendWildcard();
                    System.Speech.Recognition.SemanticResultKey wordsAfterWakeUpWordKey = new System.Speech.Recognition.SemanticResultKey("wordsAfterWakeUpWordKey", wordsAfterWakeUpWordBuilder);
                    wakeUpWordBuilder.Append(new System.Speech.Recognition.SemanticResultKey("wordsAfterWakeUpWordKey", wordsAfterWakeUpWordBuilder));

                    // initialize recognizer, wait for result, save result to file
                    System.Speech.Recognition.Grammar g = new System.Speech.Recognition.Grammar(wakeUpWordBuilder);
                    RecognitionEngine.LoadGrammar(g);
                    if (Options.options.wakeup.initialSilenceTimeout == -1)
                        RecognitionEngine.InitialSilenceTimeout = TimeSpan.FromTicks(Int32.MaxValue); // never timeout
                    else
                        RecognitionEngine.InitialSilenceTimeout = TimeSpan.FromSeconds(Options.options.wakeup.initialSilenceTimeout); // timesout after this much silence
                    RecognitionEngine.EndSilenceTimeout = TimeSpan.FromSeconds(Options.options.wakeup.endSilenceTimeout); // maximum silence allowed after hearing wakeup word
#if true // experimenting with Babble and other timeouts
                    RecognitionEngine.BabbleTimeout = TimeSpan.FromSeconds(0);
#else
                    RecognitionEngine.BabbleTimeout = TimeSpan.FromTicks(UInt32.MaxValue);
#endif
                    System.Speech.Recognition.RecognitionResult WakeUpWordResult = RecognitionEngine.Recognize();
                    // RecognitionResult is null when some unidentified timeout expires around 30 seconds. Can't find a way to make timeouts infinite so just looping.
                    if (WakeUpWordResult == null)
                        continue;
                    using (System.IO.FileStream waveStream = new System.IO.FileStream(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName, System.IO.FileMode.Create))
                    {
                        WakeUpWordResult.Audio.WriteToWaveStream(waveStream);
                        waveStream.Flush();
                        waveStream.Close();
                    }

                    Console.WriteLine("Wake up word detected (" + WakeUpWordResult.Words[0].Text + "): confidence:" + WakeUpWordResult.Confidence + " Elapsed Ms:" + stopWatch.ElapsedMilliseconds);
                    if (WakeUpWordResult.Confidence >= Options.options.wakeup.confidence)
                    {
                        byte[] bytes = await Helpers.ReadBytesFromFileAsync(Options.options.audio.speechSynthesisFileName);
                        string text = WakeUpWordResult.Text;
                        int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName);
                        System.Collections.Generic.Dictionary<string, string> apiArgs = new System.Collections.Generic.Dictionary<string, string>() { { "sampleRate", sampleRate.ToString() } };
#if false // for testing
                        await windows.SpeechToTextAsync(bytes, apiArgs);
                        Console.WriteLine("Windows STT (demo):\"" + windows.ResponseResult + "\" Total Elapsed ms:" + windows.TotalElapsedMilliseconds + " Request Elapsed ms:" + windows.RequestElapsedMilliseconds);
#endif
                        System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
                        foreach (ISpeechToTextService STT in new FindServices<ISpeechToTextService>(Options.commandservices["SpeechToText"].preferredServices).PreferredOrderingOfServices)
                        {
                            // ISpeechToTextService STT = (ISpeechToTextService)constructor.Invoke(Type.EmptyTypes);
                            SpeechToTextServiceResponse r = await STT.SpeechToTextServiceAsync(bytes, apiArgs);
                            if (r.StatusCode == 200)
                            {
                                text = r.ResponseResult;
                                Console.WriteLine(r.Service.name + ":\"" + text + "\" Total Elapsed ms:" + r.TotalElapsedMilliseconds + " Request Elapsed ms:" + r.RequestElapsedMilliseconds);
                                return text.Split(" ".ToCharArray(), StringSplitOptions.None);
                            }
                            else
                            {
                                Console.WriteLine(r.Service.name + " not available.");
                            }
                        }
                        CallServiceResponse<ISpeechToTextServiceResponse> response = await wSTT.CallServiceAsync(bytes, apiArgs);
                        text = response.ResponseResult;
                        Console.WriteLine("Windows STT (default):\"" + text + "\" Total Elapsed ms:" + response.TotalElapsedMilliseconds + " Request Elapsed ms:" + response.RequestElapsedMilliseconds);
                        return text.Split(" ".ToCharArray(), StringSplitOptions.None);
                    }
                }
            }
        }

#if false
        public static async System.Threading.Tasks.Task<string> SpeechToTextServiceAsync()
        {
            return await SpeechToTextServiceAsync(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName);
        }
#endif

        public static async System.Threading.Tasks.Task<string> SpeechToTextServicesAsync(string fileName, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            if (System.IO.File.Exists(Options.options.tempFolderPath + fileName))
            {
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(Options.options.locale.language);
                using (System.Speech.Recognition.SpeechRecognitionEngine RecognitionEngine = new System.Speech.Recognition.SpeechRecognitionEngine(ci))
                {
                    RecognitionEngine.SetInputToWaveFile(Options.options.tempFolderPath + fileName);
                    RecognitionEngine.LoadGrammar(new System.Speech.Recognition.DictationGrammar());
                    System.Speech.Recognition.RecognitionResult result = RecognitionEngine.Recognize();
                    return result.Text;
                }
            }
            return null;
        }

        public static async System.Threading.Tasks.Task<string> SpeechToTextServiceAsync(byte[] bytes, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
            return await SpeechToTextServiceAsync(stream);
        }

        public static async System.Threading.Tasks.Task<string> SpeechToTextServiceAsync(System.IO.MemoryStream stream)
        {
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(Options.options.locale.language);
            using (System.Speech.Recognition.SpeechRecognitionEngine RecognitionEngine = new System.Speech.Recognition.SpeechRecognitionEngine(ci))
            {
                RecognitionEngine.SetInputToWaveStream(stream);
                RecognitionEngine.LoadGrammar(new System.Speech.Recognition.DictationGrammar());
                System.Speech.Recognition.RecognitionResult result = RecognitionEngine.Recognize();
                if (result == null)
                    return "Speech.RecognitionEngine.Recognize returned null result";
                return result.Text;
            }
        }
#endif

        // loops until STT finds wakeup word.
        public static async System.Threading.Tasks.Task<string[]> LoopUntilWakeUpWordFoundThenRecognizeRemainingSpeechAsync(string[] WakeUpWords)
        {
            while (true)
            {
                Console.WriteLine("Say the wakeup word (" + string.Join(" ", WakeUpWords) + ") followed by the request ...");
                string text = await SpeechToText.MicrophoneToTextAsync();
                if (text != null)
                {
                    if (Options.options.debugLevel >= 3)
                        Log.WriteLine("Heard:\"" + text + "\"");
                    string[] words = text.Split(" ".ToCharArray(), StringSplitOptions.None);
                    if (words.Length > 0)
                    {
                        string firstWordUpper = words[0];
                        foreach (string wakeupword in WakeUpWords)
                        {
                            if (firstWordUpper == wakeupword)
                            {
                                return words;
                            }
                        }
                    }
                }
            }
        }
    }
}