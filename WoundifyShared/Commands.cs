using System;
using System.Linq;

namespace WoundifyShared
{
    class verbAction
    {
        public Func<string[], System.Collections.Generic.Stack<string>, System.Collections.Generic.Stack<string>, System.Threading.Tasks.Task<int>> actionFunc;
        public int stackChange;
        public string helpTip;
    }

    class Commands
    {
        private static string[] lineArgs;
        private static System.Collections.Generic.Stack<string> operatorStack; // must reverse order for proper conversion to stack
        private static System.Collections.Generic.Stack<string> operandStack;
        private static Options forceOptionsConstructor = new Options();

        // set properties: Build Action = None, Copy to Output Directory = Copy Always

        // todo: change to use class of command (Func, -1/+1 (push, pop))
        public static System.Collections.Generic.Dictionary<string, verbAction> verbActionsAsync =
            new System.Collections.Generic.Dictionary<string, verbAction>()
            {
                { "END", new verbAction() { actionFunc = verbEndAsync, stackChange = 0, helpTip = "End processing." } },
                { "HELP", new verbAction() { actionFunc = verbHelpAsync, stackChange = 0, helpTip = "Show help." } },
                { "INTENT", new verbAction() { actionFunc = verbIntentAsync, stackChange = 0, helpTip = "Pop stack passing to intent service, push response." } },
                { "LISTEN",new verbAction() { actionFunc = verbListenAsync, stackChange = +1, helpTip = "Listen and push utterance." } },
                { "LOOP", new verbAction() { actionFunc = verbLoopAsync, stackChange = 0, helpTip = "Loop to first command and repeat." } },
                { "PAUSE", new verbAction() { actionFunc = verbPauseAsync, stackChange = 0, helpTip = "Pause for specified seconds." } },
                { "PRONOUNCE", new verbAction() { actionFunc = verbPronounceAsync, stackChange = 0, helpTip = "Convert text at top of stack into spelled pronounciations." } },
                { "QUIT", new verbAction() { actionFunc = verbQuitAsync, stackChange = 0, helpTip = "Quit processing." } },
                { "REPLAY", new verbAction() { actionFunc = verbReplayAsync, stackChange = 0, helpTip = "Replay top of stack." } },
                { "RESPONSE", new verbAction() { actionFunc = verbResponseAsync, stackChange = +1, helpTip = "Push last intent response." } },
                { "SETTINGS", new verbAction() { actionFunc = verbSettingsAsync, stackChange = 0, helpTip = "Show or update settings." } },
                { "SHOW", new verbAction() { actionFunc = verbShowAsync, stackChange = 0, helpTip = "Show stack." } },
                { "SPEAK", new verbAction() { actionFunc = verbSpeakAsync, stackChange = -1, helpTip = "Pop stack (text or speech) and speak." } },
                { "SPEECH", new verbAction() { actionFunc = verbSpeechAsync, stackChange = +1, helpTip = "Push argument as speech." } },
                { "TEXT", new verbAction() { actionFunc = verbTextAsync, stackChange = +1, helpTip = "Push argument as text." } },
                { "WAKEUP", new verbAction() { actionFunc = verbWakeUpAsync, stackChange = +1, helpTip = "Wait for wakeup, convert to text, push remaining words onto stack." } },
            };

        public static async System.Threading.Tasks.Task<int> ExecuteCommands()
        {
            int reason = 0;
            while (reason == 0 && operatorStack.Count() > 0)
            {
                reason = await SingleStepCommandsAsync();
            }
            return reason;
        }

        public static async System.Threading.Tasks.Task<int> ProcessArgsAsync(string line)
        {
            ProcessArgsReset(line);
            return await ExecuteCommands();
        }

        public static async System.Threading.Tasks.Task<int> ProcessArgsAsync(string[] args) // valid args include --ClientID "..."
        {
#if false // todo: implement override of json
            string settingsJson; // todo: need to implement settings file.
            if (Options.options.settings != null && Options.options.settings.First() == '@')
                if (System.IO.File.Exists(Options.options.settings.Substring(1)))
                    settingsJson = await Helpers.ReadTextFromFileAsync(Options.options.settings.Substring(1));
                else
                {
                    settingsJson = Options.options.defaultJsonSettings;
                    //throw new FileNotFoundException("Settings file not found: " + options.settings.Substring(1));
                }
            else
                settingsJson = Options.options.settings;
            //options = Newtonsoft.Json.JsonConvert.DeserializeObject<Options>(settingsJson);
#endif

#if !WINDOWS_UWP
            // no arguments on command line so prompt user for commands
            if (args.Length == 0)
            {
                int reason = 0;
                do
                {
                    Console.Write(">");
                    string line = System.Console.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        await verbHelpAsync(args, operatorStack, operandStack);
                        continue;
                    }
                    ProcessArgsReset(Helpers.ParseArguments(line, " ,".ToCharArray()));
                    reason = await ExecuteCommands();
                } while (reason == 0);
                return 0;
            }
#endif
            ProcessArgsReset(args);
            return await ExecuteCommands();
        }

        public static async System.Threading.Tasks.Task<int> SingleStepCommandsAsync()
        {
            int reason = 0;
            if (operatorStack.Count == 0)
            {
                return (-11);
            }
            string action = operatorStack.Pop();
            string actionUpper = action.ToUpper();
            if (Commands.verbActionsAsync.ContainsKey(actionUpper))
            {
                Console.WriteLine("Executing command:" + action);
                reason = await Commands.verbActionsAsync[actionUpper].actionFunc(lineArgs, operatorStack, operandStack);
            }
            else
            {
                Console.WriteLine("Not a command:" + action + " Try help.");
                reason = 0;
            }
            return reason;
        }

        public static void ProcessArgsReset(string line)
        {
            ProcessArgsReset(Helpers.ParseArguments(line, " ".ToCharArray()));
        }

        public static void ProcessArgsReset(string[] args)
        {
            lineArgs = args;
            operatorStack = new System.Collections.Generic.Stack<string>(lineArgs.Reverse());
            operandStack = new System.Collections.Generic.Stack<string>();
        }

        private static async System.Threading.Tasks.Task<int> verbEndAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            operatorStack.Clear();
            return 1;
        }

        private static async System.Threading.Tasks.Task<int> verbHelpAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            // todo: string usage = Options.options.GetUsage();
            //Console.WriteLine(usage);
            Console.WriteLine("Commands:");
            foreach (System.Collections.Generic.KeyValuePair<string, verbAction> v in verbActionsAsync)
                Console.WriteLine(v.Key.PadRight(16) + v.Value.helpTip);
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbIntentAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            byte[] bytes;

            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek().ToUpper()))
            {
                text = fileName = operatorStack.Pop();
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        await Options.houndify.IntentAsync(text);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        int sampleRate = await Audio.GetSampleRateAsync(fileName.Substring(1));
                        if (Options.options.debugLevel >= 4)
                            await SpeechToText.ShowAllPreferredSpeechToTextServices(bytes, sampleRate);
                        await Options.houndify.IntentAsync(bytes, sampleRate);
                        Console.WriteLine("Intent result (from audio):\"" + Options.houndify.ResponseResult + "\" StatusCode:" + Options.houndify.StatusCode + " Total ms:" + Options.houndify.TotalElapsedMilliseconds + " Request ms:" + Options.houndify.RequestElapsedMilliseconds);
                    }
                    else
                    {
                        throw new ApplicationException("Intent: Unknown file extension:" + fileName);
                    }
                }
                else
                {
                    await Options.houndify.IntentAsync(text);
                }
            }
            else
            {
                fileName = operandStack.Pop();
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                    await Options.houndify.IntentAsync(text);
                    Console.WriteLine("Intent result (text):\"" + Options.houndify.ResponseResult + "\" StatusCode:" + Options.houndify.StatusCode + " Total ms:" + Options.houndify.TotalElapsedMilliseconds + " Request ms:" + Options.houndify.RequestElapsedMilliseconds);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    bytes = await Helpers.ReadBytesFromFileAsync(fileName);
                    int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
                    if (Options.options.debugLevel >= 4)
                        await SpeechToText.ShowAllPreferredSpeechToTextServices(bytes, sampleRate);
                    await Options.houndify.IntentAsync(bytes, sampleRate);
                    Console.WriteLine("Intent result (audio):\"" + Options.houndify.ResponseResult + "\" StatusCode:" + Options.houndify.StatusCode + " Total ms:" + Options.houndify.TotalElapsedMilliseconds + " Request ms:" + Options.houndify.RequestElapsedMilliseconds);
                }
                else
                {
                    throw new ApplicationException("Intent: Unknown file extension:" + fileName);
                }
            }
            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
            await Helpers.WriteTextToFileAsync(stackFileName, Options.houndify.ResponseResult);
            operandStack.Push(stackFileName);
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbListenAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            byte[] bytes;

            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek().ToUpper()))
            {
                text = fileName = operatorStack.Pop();
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        await TextToSpeech.SynSpeechWriteToFileAsync(text, stackFileName);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        await Helpers.WriteBytesToFileAsync(stackFileName, bytes);
                    }
                    else
                    {
                        throw new ApplicationException("Listen: Unknown file extension:" + fileName);
                    }
                }
                else
                {
                    await TextToSpeech.SynSpeechWriteToFileAsync(text, stackFileName);
                }
            }
            else
                await Audio.MicrophoneToFileAsync(stackFileName, TimeSpan.FromSeconds(Options.options.wakeup.listenTimeOut));
            if (Options.options.debugLevel >= 3)
                await SpeechToText.ShowAllPreferredSpeechToTextServices(stackFileName);
            operandStack.Push(stackFileName);
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbLoopAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            operatorStack.Clear();
            foreach (string a in args.Reverse())
                operatorStack.Push(a);
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbPauseAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            double pauseSecs = Options.options.pauseSecondsDefault;
            if (operatorStack.Count > 0)
            {
                text = operatorStack.Peek();
                if (double.TryParse(text, out pauseSecs))
                    operatorStack.Pop();
                else
                    pauseSecs = Options.options.pauseSecondsDefault;
            }
            Console.WriteLine("Pausing for " + pauseSecs.ToString() + " seconds.");
            System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(pauseSecs)).Wait();
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbPronounceAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            byte[] bytes;

            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek().ToUpper()))
            {
                text = fileName = operatorStack.Pop();
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        text = await SpeechToText.SpeechToTextAsync(bytes);
                    }
                    else
                    {
                        throw new ApplicationException("Listen: Unknown file extension:" + fileName);
                    }
                }
                else
                {
                    // already in text
                }
            }
            else
            {
                fileName = operandStack.Pop();
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    text = await SpeechToText.SpeechToTextAsync(fileName);
                }
                else
                {
                    throw new ApplicationException("Listen: Unknown file extension:" + fileName);
                }
            }
            text = await TextToSpeech.TextToSpelledPronunciation(text);
            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
            await Helpers.WriteTextToFileAsync(stackFileName, text);
            operandStack.Push(stackFileName);
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbQuitAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            operatorStack.Clear();
            return 1;
        }

        private static async System.Threading.Tasks.Task<int> verbReplayAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            fileName = operandStack.Peek();
            if (fileName.EndsWith(".txt"))
            {
                text = await Helpers.ReadTextFromFileAsync(fileName);
                Console.WriteLine(text);
            }
            else if (fileName.EndsWith(".wav"))
            {
                Log.WriteLine("Replaying wave file.");
                await Audio.PlayFileAsync(Options.options.tempFolderPath + fileName);
            }
            else
            {
                throw new ApplicationException("Replay: Unknown file extension:" + fileName);
            }
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbResponseAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string stackFileName;
            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
            await Helpers.WriteTextToFileAsync(stackFileName, Options.houndify.ResponseResult);
            operandStack.Push(stackFileName);
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbSettingsAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text = operatorStack.Peek();
            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(text.ToUpper()))
            {
                if (text.First() == '@')
                {
                    string fn = text.Substring(1);
                    Console.WriteLine("Overriding current settings contents of file:" + fn);
                    text = System.IO.File.ReadAllText(fn);
                }
                Console.WriteLine("Overriding current settings with " + text);
                Newtonsoft.Json.JsonConvert.PopulateObject(text, Options.options);
            }
            else
            {
                string formattedSettings = Newtonsoft.Json.JsonConvert.SerializeObject(Options.options, Newtonsoft.Json.Formatting.Indented);
                Console.WriteLine(formattedSettings);
            }
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbShowAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            foreach (string item in operandStack)
            {
                fileName = item;
                Console.WriteLine(fileName + ":");
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                    Console.WriteLine(text);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    await Audio.PlayFileAsync(Options.options.tempFolderPath + fileName);
                    // todo: do wave file to text
                }
                else
                {
                    throw new ApplicationException("Show: Unknown file extension:" + fileName);
                }
            }
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbSpeakAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek().ToUpper()))
            {
                text = fileName = operatorStack.Pop();
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        Log.WriteLine("Converting text to speech:" + text);
                        await TextToSpeech.SynSpeechPlayAsync(text);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        Log.WriteLine("Playing wave file.");
                        await Audio.PlayFileAsync(fileName.Substring(1));
                    }
                    else
                    {
                        throw new ApplicationException("Speak: Unknown file extension:" + fileName);
                    }
                }
                else
                    await TextToSpeech.SynSpeechPlayAsync(text);
            }
            else
            {
                fileName = operandStack.Pop();
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                    Log.WriteLine("Converting text to speech:" + text);
                    await TextToSpeech.SynSpeechPlayAsync(text);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    Log.WriteLine("Playing wave file.");
                    await Audio.PlayFileAsync(Options.options.tempFolderPath + fileName);
                }
                else
                {
                    throw new ApplicationException("Speak: Unknown file extension:" + fileName);
                }
            }
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbSpeechAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            byte[] bytes;
            string fileName;
            string stackFileName;
            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek().ToUpper()))
            {
                text = fileName = operatorStack.Pop();
                stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        await TextToSpeech.SynSpeechWriteToFileAsync(text, stackFileName);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        await Helpers.WriteBytesToFileAsync(stackFileName, bytes);
                    }
                    else
                    {
                        throw new ApplicationException("Speak: Unknown file extension:" + fileName);
                    }
                }
                else if (char.IsDigit(text[0])) // todo: deprecate seconds of speech arg?
                {
                    double speechTime;
                    if (double.TryParse(text, out speechTime))
                        operatorStack.Pop();
                    else
                        speechTime = Options.options.wakeup.listenTimeOut;
                    await Audio.MicrophoneToFileAsync(fileName, TimeSpan.FromSeconds(speechTime));
                }
                else
                    await TextToSpeech.SynSpeechWriteToFileAsync(text, stackFileName);
            }
            else
            {
                fileName = operandStack.Pop();
                stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                    await TextToSpeech.SynSpeechWriteToFileAsync(text, stackFileName);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    // do nothing
                }
                else
                {
                    throw new ApplicationException("Speak: Unknown file extension:" + fileName);
                }
            }
            operandStack.Push(stackFileName);
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbTextAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            byte[] bytes;
            string fileName;
            string stackFileName;
            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek().ToUpper()))
            {
                text = fileName = operatorStack.Pop();
                stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        text = await SpeechToText.SpeechToTextAsync(bytes);
                    }
                    else
                    {
                        throw new ApplicationException("Text: Unknown file extension:" + fileName);
                    }
                }
                else
                {
                    // do nothing
                }
            }
            else
            {
                fileName = operandStack.Pop();
                stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                    text = await SpeechToText.SpeechToTextAsync(bytes);
                }
                else
                {
                    throw new ApplicationException("Text: Unknown file extension:" + fileName);
                }
                operandStack.Push(stackFileName);
            }
            await Helpers.WriteTextToFileAsync(stackFileName, text);
            operandStack.Push(stackFileName);
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbWakeUpAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            string[] WakeUpWords;
            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek().ToUpper()))
            {
                text = fileName = operatorStack.Pop();
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                    }
                    else
                    {
                        throw new ApplicationException("WakeUp: expecting .txt extension:" + fileName);
                    }
                }
                WakeUpWords = Helpers.ParseArguments(text, " ,".ToCharArray(), true); // comma separated list of wake up words/phrases
            }
            else
            {
                WakeUpWords = Options.options.wakeup.words.ToArray();
            }
            int action = 0;
            do
            {
                string[] words;
#if WINDOWS_UWP
                words = await SpeechToText.LoopUntilWakeUpWordFoundThenRecognizeRemainingSpeechAsync(WakeUpWords);
#else
                if (Options.options.wakeup.preferLoopUntilWakeUpWordFound)
                    words = await SpeechToText.LoopUntilWakeUpWordFoundThenRecognizeRemainingSpeechAsync(WakeUpWords);
                else
                    words = await SpeechToText.WaitForWakeUpWordThenRecognizeRemainingSpeechAsync(WakeUpWords);
#endif
                Console.WriteLine("Heard WakeUp word:\"" + words[0] + "\" followed by:\"" + string.Join(" ", words.Skip(1)) + "\"");
                if (words.Length > 1)
                {
                    action = await ProfferedCommands.ProfferCommandAsync(words, args, operatorStack, operandStack);
                    if (action == 0)
                    {
                        stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
                        text = string.Join(" ", words.Skip(1));
                        await Helpers.WriteTextToFileAsync(stackFileName, text);
                        operandStack.Push(stackFileName);
                        return 0;
                    }
                }
            } while (action <= 0);
            return action;
        }
    }
}
