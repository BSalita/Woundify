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
        private static ServiceResponse PreferredServiceResponse;
        private static System.Collections.Generic.IEnumerable<ServiceResponse> AllServiceResponses;

        // set properties: Build Action = None, Copy to Output Directory = Copy Always

        // todo: change to use class of command (Func, -1/+1 (push, pop))
        public static System.Collections.Generic.Dictionary<string, verbAction> verbActionsAsync =
            new System.Collections.Generic.Dictionary<string, verbAction>(StringComparer.OrdinalIgnoreCase)
            {
                { "END", new verbAction() { actionFunc = verbEndAsync, stackChange = 0, helpTip = "End program. Same as QUIT." } },
                { "HELP", new verbAction() { actionFunc = verbHelpAsync, stackChange = 0, helpTip = "Show help." } },
                { "IDENTIFY", new verbAction() { actionFunc = verbIdentifyAsync, stackChange = 0, helpTip = "Pop stack passing to identify language service, push response." } },
                { "INTENT", new verbAction() { actionFunc = verbIntentAsync, stackChange = 0, helpTip = "Pop stack passing to intent service, push response." } },
                { "JSONPATH", new verbAction() { actionFunc = verbJsonPathAsync, stackChange = 0, helpTip = "Pop stack apply JsonPath, push result." } },
                { "LISTEN",new verbAction() { actionFunc = verbListenAsync, stackChange = +1, helpTip = "Listen and push utterance." } },
                { "LOOP", new verbAction() { actionFunc = verbLoopAsync, stackChange = 0, helpTip = "Loop to first command and repeat." } },
                { "PARSE", new verbAction() { actionFunc = verbParseAsync, stackChange = 0, helpTip = "Parse into phrase types. Show constituency tree." } },
                { "PAUSE", new verbAction() { actionFunc = verbPauseAsync, stackChange = 0, helpTip = "Pause for specified seconds." } },
                { "PERSONALITY", new verbAction() { actionFunc = verbPersonalityAsync, stackChange = 0, helpTip = "Pop stack passing to personality service, push response." } },
                { "PRONOUNCE", new verbAction() { actionFunc = verbPronounceAsync, stackChange = 0, helpTip = "Convert text at top of stack into spelled pronounciations." } },
                { "QUIT", new verbAction() { actionFunc = verbQuitAsync, stackChange = 0, helpTip = "Quit processing." } },
                { "REPLAY", new verbAction() { actionFunc = verbReplayAsync, stackChange = 0, helpTip = "Replay top of stack." } },
                { "RESPONSE", new verbAction() { actionFunc = verbResponseAsync, stackChange = +1, helpTip = "Push last intent response." } },
                { "SETTINGS", new verbAction() { actionFunc = verbSettingsAsync, stackChange = 0, helpTip = "Show or update settings." } },
                { "SHOW", new verbAction() { actionFunc = verbShowAsync, stackChange = 0, helpTip = "Show stack." } },
                { "SPEAK", new verbAction() { actionFunc = verbSpeakAsync, stackChange = -1, helpTip = "Pop stack (text or speech) and speak." } },
                { "SPEECH", new verbAction() { actionFunc = verbSpeechAsync, stackChange = +1, helpTip = "Push argument as speech." } },
                { "TEXT", new verbAction() { actionFunc = verbTextAsync, stackChange = +1, helpTip = "Push argument as text." } },
                { "TONE", new verbAction() { actionFunc = verbToneAsync, stackChange = 0, helpTip = "Pop stack passing to tone service, push response." } },
                { "TRANSLATE", new verbAction() { actionFunc = verbTranslateAsync, stackChange = 0, helpTip = "Pop stack passing to translate service, push response." } },
                { "WAKEUP", new verbAction() { actionFunc = verbWakeUpAsync, stackChange = +1, helpTip = "Wait for wakeup, convert to text, push remaining words onto stack." } },
            };

        public static async System.Threading.Tasks.Task<int> ExecuteCommandsAsync()
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
            return await ExecuteCommandsAsync();
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
                    reason = await ExecuteCommandsAsync();
                } while (reason == 0);
                return 0;
            }
#endif
            ProcessArgsReset(args);
            return await ExecuteCommandsAsync();
        }

        public static async System.Threading.Tasks.Task<int> SingleStepCommandsAsync()
        {
            int reason = 0;
            if (operatorStack.Count == 0)
            {
                return (-11);
            }
            string action = operatorStack.Pop();
            string actionUpper = action;
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
            ProcessArgsReset(Helpers.ParseArguments(line, " ".ToCharArray(), true));
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

        private static async System.Threading.Tasks.Task<int> verbIdentifyAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            byte[] bytes;
            IdentifyLanguageServiceResponse r;
            System.Collections.Generic.IEnumerable<IdentifyLanguageServiceResponse> AllIdentifyLanguageServiceResponses;

            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
            {
                text = fileName = operatorStack.Pop();
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt")) // todo: implement json same as Translate?
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllIdentifyLanguageServiceResponses = await IdentifyLanguageServices.RunAllPreferredIdentifyLanguageServicesAsync(text)).Select(sr => sr.sr);
                        r = await IdentifyLanguageServices.PreferredOrderingIdentifyLanguageServices[0].IdentifyLanguageServiceAsync(text);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        int sampleRate = await Audio.GetSampleRateAsync(fileName.Substring(1));
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllIdentifyLanguageServiceResponses = await IdentifyLanguageServices.RunAllPreferredIdentifyLanguageServicesAsync(bytes, sampleRate)).Select(sr => sr.sr);
                        r = await IdentifyLanguageServices.PreferredOrderingIdentifyLanguageServices[0].IdentifyLanguageServiceAsync(bytes, sampleRate);
                        if (Options.options.debugLevel >= 4)
                            Console.WriteLine("Identify result (from audio):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                    }
                    else
                    {
                        throw new Exception("Identify: Unknown file extension:" + fileName);
                    }
                }
                else
                {
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllIdentifyLanguageServiceResponses = await IdentifyLanguageServices.RunAllPreferredIdentifyLanguageServicesAsync(text)).Select(sr => sr.sr);
                    r = await IdentifyLanguageServices.PreferredOrderingIdentifyLanguageServices[0].IdentifyLanguageServiceAsync(text);
                }
            }
            else
            {
                fileName = operandStack.Pop();
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllIdentifyLanguageServiceResponses = await IdentifyLanguageServices.RunAllPreferredIdentifyLanguageServicesAsync(text)).Select(sr => sr.sr);
                    r = await IdentifyLanguageServices.PreferredOrderingIdentifyLanguageServices[0].IdentifyLanguageServiceAsync(text);
                    if (Options.options.debugLevel >= 4)
                        Console.WriteLine("Identify result (text):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    bytes = await Helpers.ReadBytesFromFileAsync(fileName);
                    int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllIdentifyLanguageServiceResponses = await IdentifyLanguageServices.RunAllPreferredIdentifyLanguageServicesAsync(bytes, sampleRate)).Select(sr => sr.sr);
                    r = await IdentifyLanguageServices.PreferredOrderingIdentifyLanguageServices[0].IdentifyLanguageServiceAsync(bytes, sampleRate);
                    if (Options.options.debugLevel >= 4)
                        Console.WriteLine("Identify result (audio):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                }
                else
                {
                    throw new Exception("Identify: Unknown file extension:" + fileName);
                }
            }
            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
            await Helpers.WriteTextToFileAsync(stackFileName, r.sr.ResponseResult);
            operandStack.Push(stackFileName);
            PreferredServiceResponse = r.sr;
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbIntentAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            byte[] bytes;
            IntentServiceResponse r;
            System.Collections.Generic.IEnumerable<IntentServiceResponse> AllIntentServiceResponses;

            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
            {
                text = fileName = operatorStack.Pop();
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllIntentServiceResponses = await IntentServices.RunAllPreferredIntentServicesAsync(text)).Select(sr => sr.sr);
                        r = await IntentServices.PreferredOrderingIntentServices[1].IntentServiceAsync(text); // todo: implement dictionary. for now [0] is audio and for now [1] is text
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        int sampleRate = await Audio.GetSampleRateAsync(fileName.Substring(1));
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllIntentServiceResponses = await IntentServices.RunAllPreferredIntentServicesAsync(bytes, sampleRate)).Select(sr => sr.sr);
                        r = await IntentServices.PreferredOrderingIntentServices[0].IntentServiceAsync(bytes, sampleRate); // todo: implement dictionary.  for now [0] is audio and for now [1] is text
                        if (Options.options.debugLevel >= 4)
                            Console.WriteLine("Intent result (from audio):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                    }
                    else
                    {
                        throw new Exception("Intent: Unknown file extension:" + fileName);
                    }
                }
                else
                {
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllIntentServiceResponses = await IntentServices.RunAllPreferredIntentServicesAsync(text)).Select(sr => sr.sr);
                    r = await IntentServices.PreferredOrderingIntentServices[1].IntentServiceAsync(text);
                }
            }
            else
            {
                fileName = operandStack.Pop();
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllIntentServiceResponses = await IntentServices.RunAllPreferredIntentServicesAsync(text)).Select(sr => sr.sr);
                    r = await IntentServices.PreferredOrderingIntentServices[1].IntentServiceAsync(text);
                    if (Options.options.debugLevel >= 4)
                        Console.WriteLine("Intent result (text):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    bytes = await Helpers.ReadBytesFromFileAsync(fileName);
                    int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllIntentServiceResponses = await IntentServices.RunAllPreferredIntentServicesAsync(bytes, sampleRate)).Select(sr => sr.sr);
                    r = await IntentServices.PreferredOrderingIntentServices[0].IntentServiceAsync(bytes, sampleRate);
                    if (Options.options.debugLevel >= 4)
                        Console.WriteLine("Intent result (audio):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                }
                else
                {
                    throw new Exception("Intent: Unknown file extension:" + fileName);
                }
            }
            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
            await Helpers.WriteTextToFileAsync(stackFileName, r.sr.ResponseResult);
            operandStack.Push(stackFileName);
            PreferredServiceResponse = r.sr;
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbJsonPathAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            if (operatorStack.Count > 0)
            {
                string fileName = operandStack.Pop();
                if (fileName.EndsWith(".txt"))
                {
                    string jsonPath = operatorStack.Pop();
                    string text = await Helpers.ReadTextFromFileAsync(fileName);
#if true // which to use? stack or PreferredServiceResponse?
                    Newtonsoft.Json.Linq.JToken tokResult = PreferredServiceResponse.ResponseBodyToken.SelectToken(jsonPath);
#else
                    Newtonsoft.Json.Linq.JToken tokResult = text.ResponseBodyToken.SelectToken(jsonPath);
#endif
                    string stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
                    await Helpers.WriteTextToFileAsync(stackFileName, tokResult.ToString());
                    operandStack.Push(stackFileName);
                }
            }
            else
            {
                Console.WriteLine("JsonPath has no arguement.");
            }
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbListenAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            byte[] bytes;
            System.Collections.Generic.IEnumerable<TextToSpeechServiceResponse> AllTextToSpeechServiceResponses;
            TextToSpeechServiceResponse r;

            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
            {
                text = fileName = operatorStack.Pop();
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllTextToSpeechServiceResponses = await TextToSpeechServices.RunAllPreferredTextToSpeechServicesAsync(text)).Select(sr => sr.sr);
                        r = await TextToSpeechServices.PreferredOrderingTextToSpeechServices[0].TextToSpeechServiceAsync(text, Options.commandservices["TextToSpeech"].sampleRate);
                        await Helpers.WriteBytesToFileAsync(stackFileName, r.sr.ResponseBytes);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        await Helpers.WriteBytesToFileAsync(stackFileName, bytes);
                    }
                    else
                    {
                        throw new Exception("Listen: Unknown file extension:" + fileName);
                    }
                }
                else if (char.IsDigit(text[0])) // todo: deprecate seconds of listening arg?
                {
                    double listenTimeOut;
                    if (!double.TryParse(text, out listenTimeOut))
                    {
                        Console.WriteLine("Invalid number. Defaulting to settings file.");
                        listenTimeOut = Options.options.wakeup.listenTimeOut;
                    }
                    await Audio.MicrophoneToFileAsync(stackFileName, TimeSpan.FromSeconds(listenTimeOut));
                }
                else
                {
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllTextToSpeechServiceResponses = await TextToSpeechServices.RunAllPreferredTextToSpeechServicesAsync(text)).Select(sr => sr.sr);
                    r = await TextToSpeechServices.PreferredOrderingTextToSpeechServices[0].TextToSpeechServiceAsync(text, Options.commandservices["TextToSpeech"].sampleRate);
                    await Helpers.WriteBytesToFileAsync(stackFileName, r.sr.ResponseBytes);
                }
            }
            else
                await Audio.MicrophoneToFileAsync(stackFileName, TimeSpan.FromSeconds(Options.options.wakeup.listenTimeOut));
            if (Options.options.debugLevel >= 3)
                await SpeechToTextServices.RunAllPreferredSpeechToTextServicesAsync(stackFileName);
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

        private static async System.Threading.Tasks.Task<int> verbParseAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            byte[] bytes;
            ParseServiceResponse r;
            System.Collections.Generic.IEnumerable<ParseServiceResponse> AllParseServiceResponses;

            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
            {
                text = fileName = operatorStack.Pop();
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllParseServiceResponses = await ParseServices.RunAllPreferredParseServicesAsync(text)).Select(sr => sr.sr);
                        r = await ParseServices.PreferredOrderingParseServices[0].ParseServiceAsync(text);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        int sampleRate = await Audio.GetSampleRateAsync(fileName.Substring(1));
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllParseServiceResponses = await ParseServices.RunAllPreferredParseServicesAsync(text)).Select(sr => sr.sr);
                        r = await ParseServices.PreferredOrderingParseServices[0].ParseServiceAsync(text);
                    }
                    else
                    {
                        throw new Exception("Parse: Unknown file extension:" + fileName);
                    }
                }
                else
                {
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllParseServiceResponses = await ParseServices.RunAllPreferredParseServicesAsync(text)).Select(sr => sr.sr);
                    r = await ParseServices.PreferredOrderingParseServices[0].ParseServiceAsync(text);
                }
            }
            else
            {
                fileName = operandStack.Pop();
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllParseServiceResponses = await ParseServices.RunAllPreferredParseServicesAsync(text)).Select(sr => sr.sr);
                    r = await ParseServices.PreferredOrderingParseServices[0].ParseServiceAsync(text);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    bytes = await Helpers.ReadBytesFromFileAsync(fileName);
                    int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
                    //                    if (Options.options.debugLevel >= 4)
                    //                        AllServiceResponses = (AllParseServiceResponses = await ParseServices.RunAllPreferredParseServicesAsync(bytes, sampleRate)).Select(sr => sr.sr);
                    r = await ParseServices.PreferredOrderingParseServices[0].ParseServiceAsync(bytes, sampleRate);
                }
                else
                {
                    throw new Exception("Parse: Unknown file extension:" + fileName);
                }
            }
            if (Options.options.debugLevel >= 4)
                Console.WriteLine("Parse result:\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
            await Helpers.WriteTextToFileAsync(stackFileName, r.sr.ResponseResult);
            operandStack.Push(stackFileName);
            PreferredServiceResponse = r.sr;
            Newtonsoft.Json.Linq.JArray arrayOfResults = Newtonsoft.Json.Linq.JArray.Parse(r.sr.ResponseResult);
            foreach (Newtonsoft.Json.Linq.JToken s in arrayOfResults)
            {
                ConstituencyTreeNode root = ParseHelpers.ConstituencyTreeFromText(s.ToString());
                text = ParseHelpers.TextFromConstituencyTree(root);
                if (text != s.ToString())
                    throw new FormatException();
                string words = ParseHelpers.WordsFromConstituencyTree(root);
                string[] printLines = ParseHelpers.FormatConstituencyTree(root);
                foreach (string p in printLines)
                    Console.WriteLine(p);
            }
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

        private static async System.Threading.Tasks.Task<int> verbPersonalityAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            byte[] bytes;
            PersonalityServiceResponse r;
            System.Collections.Generic.IEnumerable<PersonalityServiceResponse> AllPersonalityServiceResponses;

            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
            {
                text = fileName = operatorStack.Pop();
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".json") || fileName.EndsWith(".txt")) // todo: support json elsewhere?
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllPersonalityServiceResponses = await PersonalityServices.RunAllPreferredPersonalityServicesAsync(text)).Select(sr => sr.sr);
                        r = await PersonalityServices.PreferredOrderingPersonalityServices[0].PersonalityServiceAsync(text);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        int sampleRate = await Audio.GetSampleRateAsync(fileName.Substring(1));
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllPersonalityServiceResponses = await PersonalityServices.RunAllPreferredPersonalityServicesAsync(bytes, sampleRate)).Select(sr => sr.sr);
                        r = await PersonalityServices.PreferredOrderingPersonalityServices[0].PersonalityServiceAsync(bytes, sampleRate);
                        if (Options.options.debugLevel >= 4)
                            Console.WriteLine("Personality result (from audio):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                    }
                    else
                    {
                        throw new Exception("Personality: Unknown file extension:" + fileName);
                    }
                }
                else
                {
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllPersonalityServiceResponses = await PersonalityServices.RunAllPreferredPersonalityServicesAsync(text)).Select(sr => sr.sr);
                    r = await PersonalityServices.PreferredOrderingPersonalityServices[0].PersonalityServiceAsync(text);
                }
            }
            else
            {
                fileName = operandStack.Pop();
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllPersonalityServiceResponses = await PersonalityServices.RunAllPreferredPersonalityServicesAsync(text)).Select(sr => sr.sr);
                    r = await PersonalityServices.PreferredOrderingPersonalityServices[0].PersonalityServiceAsync(text);
                    if (Options.options.debugLevel >= 4)
                        Console.WriteLine("Personality result (text):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    bytes = await Helpers.ReadBytesFromFileAsync(fileName);
                    int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllPersonalityServiceResponses = await PersonalityServices.RunAllPreferredPersonalityServicesAsync(bytes, sampleRate)).Select(sr => sr.sr);
                    r = await PersonalityServices.PreferredOrderingPersonalityServices[0].PersonalityServiceAsync(bytes, sampleRate);
                    if (Options.options.debugLevel >= 4)
                        Console.WriteLine("Personality result (audio):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                }
                else
                {
                    throw new Exception("Personality: Unknown file extension:" + fileName);
                }
            }
            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
            await Helpers.WriteTextToFileAsync(stackFileName, r.sr.ResponseResult);
            operandStack.Push(stackFileName);
            PreferredServiceResponse = r.sr;
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbPronounceAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            byte[] bytes;

            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
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
                        text = await SpeechToText.SpeechToTextServiceAsync(bytes);
                    }
                    else
                    {
                        throw new Exception("Listen: Unknown file extension:" + fileName);
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
                    text = await SpeechToText.SpeechToTextServiceAsync(fileName);
                }
                else
                {
                    throw new Exception("Listen: Unknown file extension:" + fileName);
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
                throw new Exception("Replay: Unknown file extension:" + fileName);
            }
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbResponseAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string stackFileName;
            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
            if (Options.options.debugLevel >= 4)
                foreach (ServiceResponse sr in AllServiceResponses)
                    Log.WriteLine(sr.ServiceName + ":" + sr.ResponseResult);
            await Helpers.WriteTextToFileAsync(stackFileName, PreferredServiceResponse.ResponseResult);
            operandStack.Push(stackFileName);
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbSettingsAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
            {
                string text = operatorStack.Pop();
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
                    throw new Exception("Show: Unknown file extension:" + fileName);
                }
            }
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbSpeakAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            System.Collections.Generic.IEnumerable<TextToSpeechServiceResponse> AllTextToSpeechServiceResponses;
            TextToSpeechServiceResponse r;

            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
            {
                text = fileName = operatorStack.Pop();
                stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        Log.WriteLine("Converting text to speech:" + text);
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllTextToSpeechServiceResponses = await TextToSpeechServices.RunAllPreferredTextToSpeechServicesAsync(text)).Select(sr => sr.sr);
                        r = await TextToSpeechServices.PreferredOrderingTextToSpeechServices[0].TextToSpeechServiceAsync(text, Options.commandservices["TextToSpeech"].sampleRate);
                        await Helpers.WriteBytesToFileAsync(stackFileName, r.sr.ResponseBytes);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        Log.WriteLine("Playing wave file.");
                        await Audio.PlayFileAsync(fileName.Substring(1));
                    }
                    else
                    {
                        throw new Exception("Speak: Unknown file extension:" + fileName);
                    }
                }
                else
                {
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllTextToSpeechServiceResponses = await TextToSpeechServices.RunAllPreferredTextToSpeechServicesAsync(text)).Select(sr => sr.sr);
                    r = await TextToSpeechServices.PreferredOrderingTextToSpeechServices[0].TextToSpeechServiceAsync(text, Options.commandservices["TextToSpeech"].sampleRate);
                    await Helpers.WriteBytesToFileAsync(stackFileName, r.sr.ResponseBytes);
                }
            }
            else
            {
                fileName = operandStack.Pop();
                stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                    Log.WriteLine("Converting text to speech:" + text);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllTextToSpeechServiceResponses = await TextToSpeechServices.RunAllPreferredTextToSpeechServicesAsync(text)).Select(sr => sr.sr);
                    r = await TextToSpeechServices.PreferredOrderingTextToSpeechServices[0].TextToSpeechServiceAsync(text, Options.commandservices["TextToSpeech"].sampleRate);
                    await Helpers.WriteBytesToFileAsync(stackFileName, r.sr.ResponseBytes);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    Log.WriteLine("Playing wav file.");
                    await Audio.PlayFileAsync(Options.options.tempFolderPath + fileName);
                }
                else
                {
                    throw new Exception("Speak: Unknown file extension:" + fileName);
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
            System.Collections.Generic.IEnumerable<TextToSpeechServiceResponse> AllTextToSpeechServiceResponses;
            TextToSpeechServiceResponse r;

            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
            {
                text = fileName = operatorStack.Pop();
                stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllTextToSpeechServiceResponses = await TextToSpeechServices.RunAllPreferredTextToSpeechServicesAsync(text)).Select(sr => sr.sr);
                        r = await TextToSpeechServices.PreferredOrderingTextToSpeechServices[0].TextToSpeechServiceAsync(text, Options.commandservices["TextToSpeech"].sampleRate);
                        await Helpers.WriteBytesToFileAsync(stackFileName, r.sr.ResponseBytes);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        await Helpers.WriteBytesToFileAsync(stackFileName, bytes);
                    }
                    else
                    {
                        throw new Exception("Speak: Unknown file extension:" + fileName);
                    }
                }
                else if (char.IsDigit(text[0])) // todo: deprecate seconds of speech arg?
                {
                    double listenTimeOut;
                    if (!double.TryParse(text, out listenTimeOut))
                    {
                        Console.WriteLine("Invalid number. Defaulting to settings file.");
                        listenTimeOut = Options.options.wakeup.listenTimeOut;
                    }
                    await Audio.MicrophoneToFileAsync(stackFileName, TimeSpan.FromSeconds(listenTimeOut));
                }
                else
                {
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllTextToSpeechServiceResponses = await TextToSpeechServices.RunAllPreferredTextToSpeechServicesAsync(text)).Select(sr => sr.sr);
                    r = await TextToSpeechServices.PreferredOrderingTextToSpeechServices[0].TextToSpeechServiceAsync(text, Options.commandservices["TextToSpeech"].sampleRate);
                    await Helpers.WriteBytesToFileAsync(stackFileName, r.sr.ResponseBytes);
                }
            }
            else
            {
                fileName = operandStack.Pop();
                stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllTextToSpeechServiceResponses = await TextToSpeechServices.RunAllPreferredTextToSpeechServicesAsync(text)).Select(sr => sr.sr);
                    r = await TextToSpeechServices.PreferredOrderingTextToSpeechServices[0].TextToSpeechServiceAsync(text, Options.commandservices["TextToSpeech"].sampleRate);
                    await Helpers.WriteBytesToFileAsync(stackFileName, r.sr.ResponseBytes);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    // do nothing
                }
                else
                {
                    throw new Exception("Speak: Unknown file extension:" + fileName);
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
            System.Collections.Generic.IEnumerable<SpeechToTextServiceResponse> AllSpeechToTextServiceResponses;

            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
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
                        int sampleRate = await Audio.GetSampleRateAsync(fileName.Substring(1));
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllSpeechToTextServiceResponses = await SpeechToTextServices.RunAllPreferredSpeechToTextServicesAsync(bytes, sampleRate)).Select(sr => sr.sr);
                        SpeechToTextServiceResponse sttr = await SpeechToTextServices.PreferredOrderingSpeechToTextServices[0].SpeechToTextServiceAsync(bytes, sampleRate);
                        text = sttr.sr.ResponseResult;
                    }
                    else
                    {
                        throw new Exception("Text: Unknown file extension:" + fileName);
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
                    bytes = await Helpers.ReadBytesFromFileAsync(fileName); // todo: implement local file scheme (non-tempFolder directory)
                    text = await SpeechToText.SpeechToTextServiceAsync(bytes);
                }
                else
                {
                    throw new Exception("Text: Unknown file extension:" + fileName);
                }
                operandStack.Push(stackFileName);
            }
            await Helpers.WriteTextToFileAsync(stackFileName, text);
            operandStack.Push(stackFileName);
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbToneAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            byte[] bytes;
            ToneServiceResponse r;
            System.Collections.Generic.IEnumerable<ToneServiceResponse> AllToneServiceResponses;

            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
            {
                text = fileName = operatorStack.Pop();
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllToneServiceResponses = await ToneServices.RunAllPreferredToneServicesAsync(text)).Select(sr => sr.sr);
                        r = await ToneServices.PreferredOrderingToneServices[0].ToneServiceAsync(text);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        int sampleRate = await Audio.GetSampleRateAsync(fileName.Substring(1));
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllToneServiceResponses = await ToneServices.RunAllPreferredToneServicesAsync(bytes, sampleRate)).Select(sr => sr.sr);
                        r = await ToneServices.PreferredOrderingToneServices[0].ToneServiceAsync(bytes, sampleRate);
                        if (Options.options.debugLevel >= 4)
                            Console.WriteLine("Tone result (from audio):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                    }
                    else
                    {
                        throw new Exception("Tone: Unknown file extension:" + fileName);
                    }
                }
                else
                {
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllToneServiceResponses = await ToneServices.RunAllPreferredToneServicesAsync(text)).Select(sr => sr.sr);
                    r = await ToneServices.PreferredOrderingToneServices[0].ToneServiceAsync(text);
                }
            }
            else
            {
                fileName = operandStack.Pop();
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllToneServiceResponses = await ToneServices.RunAllPreferredToneServicesAsync(text)).Select(sr => sr.sr);
                    r = await ToneServices.PreferredOrderingToneServices[0].ToneServiceAsync(text);
                    if (Options.options.debugLevel >= 4)
                        Console.WriteLine("Tone result (text):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    bytes = await Helpers.ReadBytesFromFileAsync(fileName);
                    int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllToneServiceResponses = await ToneServices.RunAllPreferredToneServicesAsync(bytes, sampleRate)).Select(sr => sr.sr);
                    r = await ToneServices.PreferredOrderingToneServices[0].ToneServiceAsync(bytes, sampleRate);
                    if (Options.options.debugLevel >= 4)
                        Console.WriteLine("Tone result (audio):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                }
                else
                {
                    throw new Exception("Tone: Unknown file extension:" + fileName);
                }
            }
            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
            await Helpers.WriteTextToFileAsync(stackFileName, r.sr.ResponseResult);
            operandStack.Push(stackFileName);
            PreferredServiceResponse = r.sr;
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbTranslateAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            byte[] bytes;
            TranslateServiceResponse r;
            System.Collections.Generic.IEnumerable<TranslateServiceResponse> AllTranslateServiceResponses;

            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
            {
                text = operatorStack.Pop();
                if (text.Contains("="))
                {
                    // need cross-API solution for specifying language pairs.
                    Settings.Service service = Options.services["IbmWatsonTranslateService"];
                    string[] languagePairs = text.Split('=');
                    if (languagePairs.Length == 2)
                    {
                        service.request.data.source = languagePairs[0];
                        service.request.data.target = languagePairs[1];
                        Console.WriteLine("Using language pair:" + languagePairs[0] + "=" + languagePairs[1]);
                    }
                    else
                        Console.WriteLine("Invalid language pair:" + text + ". Try en-es.");
                }
                else
                    operatorStack.Push(text);
            }

            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
            {
                text = fileName = operatorStack.Pop();
                if (fileName.First() == '@')
                {
                    if (fileName.EndsWith(".txt")) // implement json same as Identify?
                    {
                        text = System.IO.File.ReadAllText(fileName.Substring(1)); // todo: implement local file name scheme
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllTranslateServiceResponses = await TranslateServices.RunAllPreferredTranslateServicesAsync(text)).Select(sr => sr.sr);
                        r = await TranslateServices.PreferredOrderingTranslateServices[0].TranslateServiceAsync(text);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        bytes = System.IO.File.ReadAllBytes(fileName.Substring(1)); // todo: implement local file scheme (non-tempFolder directory)
                        int sampleRate = await Audio.GetSampleRateAsync(fileName.Substring(1));
                        if (Options.options.debugLevel >= 4)
                            AllServiceResponses = (AllTranslateServiceResponses = await TranslateServices.RunAllPreferredTranslateServicesAsync(bytes, sampleRate)).Select(sr => sr.sr);
                        r = await TranslateServices.PreferredOrderingTranslateServices[0].TranslateServiceAsync(bytes, sampleRate);
                        if (Options.options.debugLevel >= 4)
                            Console.WriteLine("Translate result (from audio):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                    }
                    else
                    {
                        throw new Exception("Translate: Unknown file extension:" + fileName);
                    }
                }
                else
                {
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllTranslateServiceResponses = await TranslateServices.RunAllPreferredTranslateServicesAsync(text)).Select(sr => sr.sr);
                    r = await TranslateServices.PreferredOrderingTranslateServices[0].TranslateServiceAsync(text);
                }
            }
            else
            {
                fileName = operandStack.Pop();
                if (fileName.EndsWith(".txt"))
                {
                    text = await Helpers.ReadTextFromFileAsync(fileName);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllTranslateServiceResponses = await TranslateServices.RunAllPreferredTranslateServicesAsync(text)).Select(sr => sr.sr);
                    r = await TranslateServices.PreferredOrderingTranslateServices[0].TranslateServiceAsync(text);
                    if (Options.options.debugLevel >= 4)
                        Console.WriteLine("Translate result (text):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    bytes = await Helpers.ReadBytesFromFileAsync(fileName);
                    int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
                    if (Options.options.debugLevel >= 4)
                        AllServiceResponses = (AllTranslateServiceResponses = await TranslateServices.RunAllPreferredTranslateServicesAsync(bytes, sampleRate)).Select(sr => sr.sr);
                    r = await TranslateServices.PreferredOrderingTranslateServices[0].TranslateServiceAsync(bytes, sampleRate);
                    if (Options.options.debugLevel >= 4)
                        Console.WriteLine("Translate result (audio):\"" + r.sr.ResponseResult + "\" StatusCode:" + r.sr.StatusCode + " Total ms:" + r.sr.TotalElapsedMilliseconds + " Request ms:" + r.sr.RequestElapsedMilliseconds);
                }
                else
                {
                    throw new Exception("Translate: Unknown file extension:" + fileName);
                }
            }
            stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
            await Helpers.WriteTextToFileAsync(stackFileName, r.sr.ResponseResult);
            operandStack.Push(stackFileName);
            PreferredServiceResponse = r.sr;
            return 0;
        }

        private static async System.Threading.Tasks.Task<int> verbWakeUpAsync(string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            string text;
            string fileName;
            string stackFileName;
            string[] WakeUpWords;
            if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
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
                        throw new Exception("WakeUp: expecting .txt extension:" + fileName);
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
