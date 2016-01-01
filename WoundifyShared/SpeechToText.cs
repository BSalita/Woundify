using System;
using System.Linq; // for SelectMany

namespace WoundifyShared
{
    class SpeechToText
    {
        private static System.Collections.Generic.List<ISpeechToTextService> PreferredISpeechToTextServices = new System.Collections.Generic.List<ISpeechToTextService>();
        static SpeechToText()
        {
            // We need to create a list of ISpeechToTextService objects ordered by user's preference settings.
            // Using reflection to get list of classes implementing ISpeechToTextService
            System.Collections.Generic.IEnumerable<Type> ISpeechToTextServiceTypes = AppDomain
                   .CurrentDomain
                   .GetAssemblies()
                   .SelectMany(assembly => assembly.GetTypes())
                   .Where(type => typeof(ISpeechToTextService).IsAssignableFrom(type));
            // Match user preference with available classes. Build list of ISpeechToTextService objects.
            foreach (string STT in Options.options.Services.APIs.SpeechToText.preferredSpeechToTextServices)
            {
                foreach (Type t in ISpeechToTextServiceTypes)
                {
                    // for each ISpeechToTextService requested, invoke it's constructor and drop it into the list.
                    if (STT == t.Name)
                        PreferredISpeechToTextServices.Add((ISpeechToTextService)t.GetConstructor(Type.EmptyTypes).Invoke(Type.EmptyTypes));
                }
            }
        }

        public static async System.Threading.Tasks.Task ShowAllPreferredSpeechToTextServices(string fileName)
        {
            byte[] bytes = await Helpers.ReadBytesFromFileAsync(fileName);
            int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
            await ShowAllPreferredSpeechToTextServices(bytes, sampleRate);
        }

        public static async System.Threading.Tasks.Task ShowAllPreferredSpeechToTextServices(byte[] bytes, int sampleRate)
        {
            // invoke each ISpeechToTextService and show what it can do.
            foreach (ISpeechToTextService STT in PreferredISpeechToTextServices)
            {
                System.Threading.Tasks.Task b = STT.SpeechToTextAsync(bytes, sampleRate).ContinueWith((c) =>
                {
                    if (string.IsNullOrEmpty(STT.ResponseResult) || STT.StatusCode != 200)
                        Console.WriteLine(STT.GetType().Name + " STT (async): Failed with StatusCode of " + STT.StatusCode);
                    else
                        Console.WriteLine(STT.GetType().Name + " STT (async):\"" + STT.ResponseResult + "\" Total " + STT.TotalElapsedMilliseconds + "ms Request " + STT.RequestElapsedMilliseconds + "ms");
                });
            }
        }

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
#else
        private static System.Threading.ManualResetEvent WakeUpEvent = new System.Threading.ManualResetEvent(false);
        //private static System.Speech.Recognition.SpeechRecognizedEventArgs WakeUpWordResults;

        public static async System.Threading.Tasks.Task<string> MicrophoneToTextAsync()
        {
            GoogleServices google = new GoogleServices();
            WindowsServices windows = new WindowsServices();
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
#if false // for testing
                await windows.SpeechToTextAsync(bytes, await Audio.GetSampleRateAsync(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName));
                Console.WriteLine("Windows STT (demo):\"" + windows.ResponseResult + "\" Total Elapsed ms:" + windows.TotalElapsedMilliseconds + " Request Elapsed ms:" + windows.RequestElapsedMilliseconds);
#endif
                System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
                string text = WakeUpWordResult.Text;
                foreach (ISpeechToTextService STT in PreferredISpeechToTextServices)
                {
                    // ISpeechToTextService STT = (ISpeechToTextService)constructor.Invoke(Type.EmptyTypes);
                    await STT.SpeechToTextAsync(bytes, await Audio.GetSampleRateAsync(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName));
                    if (STT.StatusCode == 200)
                    {
                        text = STT.ResponseResult;
                        Console.WriteLine(STT.GetType().Name + ":\"" + text + "\" Total Elapsed ms:" + STT.TotalElapsedMilliseconds + " Request Elapsed ms:" + STT.RequestElapsedMilliseconds);
                        return text;
                    }
                    else
                    {
                        Console.WriteLine(STT.GetType().Name + " not available.");
                    }
                }
                await windows.SpeechToTextAsync(bytes, await Audio.GetSampleRateAsync(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName));
                text = windows.ResponseResult;
                Console.WriteLine("Windows STT (default):\"" + text + "\" Total Elapsed ms:" + windows.TotalElapsedMilliseconds + " Request Elapsed ms:" + windows.RequestElapsedMilliseconds);
                return text;
            }
        }

        public static async System.Threading.Tasks.Task<string[]> WaitForWakeUpWordThenRecognizeRemainingSpeechAsync(string[] WakeUpWords)
        {
            Console.WriteLine("Say the wakeup word (" + string.Join(" ", WakeUpWords) + ") followed by the request ...");
            GoogleServices google = new GoogleServices();
            WindowsServices windows = new WindowsServices();
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
#if false // for testing
                        await windows.SpeechToTextAsync(bytes, await Audio.GetSampleRateAsync(Options.options.audio.speechSynthesisFileName));
                        Console.WriteLine("Windows STT (demo):\"" + windows.ResponseResult + "\" Total Elapsed ms:" + windows.TotalElapsedMilliseconds + " Request Elapsed ms:" + windows.RequestElapsedMilliseconds);
#endif
                        System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
                        string text = WakeUpWordResult.Text;
                        foreach (ISpeechToTextService STT in PreferredISpeechToTextServices)
                        {
                            // ISpeechToTextService STT = (ISpeechToTextService)constructor.Invoke(Type.EmptyTypes);
                            await STT.SpeechToTextAsync(bytes, await Audio.GetSampleRateAsync(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName));
                            if (STT.StatusCode == 200)
                            {
                                text = STT.ResponseResult;
                                Console.WriteLine(STT.GetType().Name + ":\"" + text + "\" Total Elapsed ms:" + STT.TotalElapsedMilliseconds + " Request Elapsed ms:" + STT.RequestElapsedMilliseconds);
                                return text.Split(" ".ToCharArray(), StringSplitOptions.None);
                            }
                            else
                            {
                                Console.WriteLine(STT.GetType().Name + " not available.");
                            }
                        }
                        await windows.SpeechToTextAsync(bytes, await Audio.GetSampleRateAsync(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName));
                        text = windows.ResponseResult;
                        Console.WriteLine("Windows STT (default):\"" + text + "\" Total Elapsed ms:" + windows.TotalElapsedMilliseconds + " Request Elapsed ms:" + windows.RequestElapsedMilliseconds);
                        return text.Split(" ".ToCharArray(), StringSplitOptions.None);
                    }
                }
            }
        }

        public static async System.Threading.Tasks.Task<string> SpeechToTextAsync()
        {
            return await SpeechToTextAsync(Options.options.tempFolderPath + Options.options.audio.speechSynthesisFileName);
        }

        public static async System.Threading.Tasks.Task<string> SpeechToTextAsync(string fileName)
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

        public static async System.Threading.Tasks.Task<string> SpeechToTextAsync(byte[] bytes)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
            return await SpeechToTextAsync(stream);
        }

        public static async System.Threading.Tasks.Task<string> SpeechToTextAsync(System.IO.MemoryStream stream)
        {
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(Options.options.locale.language);
            using (System.Speech.Recognition.SpeechRecognitionEngine RecognitionEngine = new System.Speech.Recognition.SpeechRecognitionEngine(ci))
            {
                RecognitionEngine.SetInputToWaveStream(stream);
                RecognitionEngine.LoadGrammar(new System.Speech.Recognition.DictationGrammar());
                System.Speech.Recognition.RecognitionResult result = RecognitionEngine.Recognize();
                return result.Text;
            }
        }

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
                        string firstWordUpper = words[0].ToUpper();
                        foreach (string wakeupword in WakeUpWords)
                        {
                            if (firstWordUpper == wakeupword.ToUpper())
                            {
                                return words;
                            }
                        }
                    }
                }
            }
        }
#endif
    }
}