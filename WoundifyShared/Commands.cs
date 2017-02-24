using System;
using System.Linq;

namespace WoundifyShared
{
    class verbAction
    {
        public Func<string, string[], System.Collections.Generic.Stack<string>, System.Collections.Generic.Stack<string>, System.Collections.Generic.Dictionary<string, string>, System.Collections.Generic.IEnumerable<GenericCallServices>, System.Threading.Tasks.Task<int>> actionFunc;
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
        private static System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task> AllServiceResponseTasks;

        // set properties: Build Action = None, Copy to Output Directory = Copy Always

        // TODO: change to use class of command (Func, -1/+1 (push, pop))
        public static System.Collections.Generic.Dictionary<string, verbAction> verbActionsAsync = new System.Collections.Generic.Dictionary<string, verbAction>(StringComparer.OrdinalIgnoreCase);
#if false
        public static System.Collections.Generic.Dictionary<string, verbAction> verbActionsAsync =
        new System.Collections.Generic.Dictionary<string, verbAction>(StringComparer.OrdinalIgnoreCase)
            {
                { "END", new verbAction() { actionFunc = verbEndAsync, stackChange = 0, helpTip = "End program. Same as QUIT." } },
                { "ANNOTATE", new verbAction() { actionFunc = verbAnnotateAsync, stackChange = 0, helpTip = "Pop stack passing to annotate service, push response." } },
                { "ENTITIES", new verbAction() { actionFunc = verbEntitiesAsync, stackChange = 0, helpTip = "Pop stack passing to entities service, push response." } },
                { "GENERIC", new verbAction() { actionFunc = verbGenericAsync, stackChange = 0, helpTip = "Pop stack passing to generic service, push response." } },
                { "HELP", new verbAction() { actionFunc = verbHelpAsync, stackChange = 0, helpTip = "Show help." } },
                { "IDENTIFY", new verbAction() { actionFunc = verbIdentifyAsync, stackChange = 0, helpTip = "Pop stack passing to identify language service, push response." } },
                { "IMAGEAGE", new verbAction() { actionFunc = verbImageAgeGenderEthnicityAsync, stackChange = 0, helpTip = "Pop stack passing to image service, push response." } },
                { "IMAGEAPPAREL", new verbAction() { actionFunc = verbImageApparelAsync, stackChange = 0, helpTip = "Pop stack passing to image service, push response." } },
                { "IMAGECELEBRITY", new verbAction() { actionFunc = verbImageCelebrityAsync, stackChange = 0, helpTip = "Pop stack passing to image service, push response." } },
                { "IMAGECOLOR", new verbAction() { actionFunc = verbImageColorAsync, stackChange = 0, helpTip = "Pop stack passing to image service, push response." } },
                { "IMAGEFACE", new verbAction() { actionFunc = verbImageFaceDetectionAsync, stackChange = 0, helpTip = "Pop stack passing to image service, push response." } },
                { "IMAGEFOOD", new verbAction() { actionFunc = verbImageFoodAsync, stackChange = 0, helpTip = "Pop stack passing to image service, push response." } },
                { "IMAGEGENERAL", new verbAction() { actionFunc = verbImageGeneralAsync, stackChange = 0, helpTip = "Pop stack passing to image service, push response." } },
                { "IMAGENSFW", new verbAction() { actionFunc = verbImageNsfwAsync, stackChange = 0, helpTip = "Pop stack passing to image service, push response." } },
                { "IMAGETRAVEL", new verbAction() { actionFunc = verbImageTravelAsync, stackChange = 0, helpTip = "Pop stack passing to image service, push response." } },
                { "IMAGEWED", new verbAction() { actionFunc = verbImageWeddingsAsync, stackChange = 0, helpTip = "Pop stack passing to image service, push response." } },
                { "INTENT", new verbAction() { actionFunc = verbIntentAsync, stackChange = 0, helpTip = "Pop stack passing to intent service, push response." } },
                { "JSONPATH", new verbAction() { actionFunc = verbJsonPathAsync, stackChange = 0, helpTip = "Pop stack apply JsonPath, push result." } },
                { "LISTEN",new verbAction() { actionFunc = verbListenAsync, stackChange = +1, helpTip = "Listen and push utterance." } },
                { "LOOP", new verbAction() { actionFunc = verbLoopAsync, stackChange = 0, helpTip = "Loop to first command and repeat." } },
                //{ "PARAPHRASE", new verbAction() { actionFunc = verbParaphraseAsync, stackChange = 0, helpTip = "[Deprecated. No service available.] Pop stack passing to paraphrase service, push response." } },
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
                { "SPELL", new verbAction() { actionFunc = verbSpellAsync, stackChange = +1, helpTip = "Spell check argument." } },
                { "TEXT", new verbAction() { actionFunc = verbTextAsync, stackChange = +1, helpTip = "Push argument as text." } },
                { "TONE", new verbAction() { actionFunc = verbToneAsync, stackChange = 0, helpTip = "Pop stack passing to tone service, push response." } },
                { "TRANSLATE", new verbAction() { actionFunc = verbTranslateAsync, stackChange = 0, helpTip = "Pop stack passing to translate service, push response." } },
                { "WAKEUP", new verbAction() { actionFunc = verbWakeUpAsync, stackChange = +1, helpTip = "Wait for wakeup, convert to text, push remaining words onto stack." } },
                { "XPATH", new verbAction() { actionFunc = verbXPathAsync, stackChange = 0, helpTip = "Pop stack apply xpath, push result." } }
            };
#endif

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
            System.Collections.Generic.Dictionary<string, Type> ServiceTypes = AppDomain
        .CurrentDomain
        .GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => type.IsClass && typeof(IProcessACommand).IsAssignableFrom(type))
        .ToDictionary(k => k.Name + "." + typeof(IProcessACommand).Name, v => v, StringComparer.OrdinalIgnoreCase); // create key of Class+Interface

            foreach (System.Collections.Generic.KeyValuePair<string, Settings.Command> cmdkvp in Options.commands)
            {
                if (ServiceTypes.ContainsKey(cmdkvp.Value.classInterface)) // todo: use new property "CommandProcessor"
                {
                    if (Options.options.debugLevel >= 5)
                        Console.WriteLine(cmdkvp.Key);
                    Type st = ServiceTypes[cmdkvp.Value.classInterface];
                    IProcessACommand af = (IProcessACommand)st.GetConstructor(new Type[] { }).Invoke(new object[] { });
                    verbAction va = new verbAction() { actionFunc = af.verbCommandAsync, stackChange = af.stackChange, helpTip = cmdkvp.Value.help };
                    verbActionsAsync.Add(cmdkvp.Key, va);
                }
                else
                {
                    Console.WriteLine("No service for command \"" + cmdkvp.Key + "\"");
                    Log.WriteLine("No service for command \"" + cmdkvp.Key + "\"");
                    //throw new Exception("ProcessArgsAsync: No service for command \"" + cmdkvp.Key + "\"");
                }
            }

#if false // TODO: implement override of json
            string settingsJson; // TODO: need to implement settings file.
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
                        HelpCommand hc = new HelpCommand();
                        string name = "help";
                        System.Collections.Generic.Dictionary<string, string> apiArgs = new System.Collections.Generic.Dictionary<string, string>();
                        System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices = new FindServices<IGenericCallServices>(Options.commandservices[name].preferredServices).PreferredOrderingOfServices;
                        await hc.verbCommandAsync(name, args, operatorStack, operandStack, apiArgs, PreferredOrderingGenericServices);
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
            if (Commands.verbActionsAsync.ContainsKey(action))
            {
                Console.WriteLine("Executing command:" + action);
                System.Collections.Generic.Dictionary<string, string> apiArgs = new System.Collections.Generic.Dictionary<string, string>();
                reason = await Commands.verbActionsAsync[action].actionFunc(action, lineArgs, operatorStack, operandStack, apiArgs, null);
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

        public class GenericCommand : GenericRunServices, IProcessACommand
        {
            public virtual int stackChange { get; set; } = 0;
            public virtual async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                string text;
                byte[] bytes;
                string fileName;
                string stackFileName;
                System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>>> AllGenericServiceResponseTasks;
                System.Collections.Generic.IEnumerable<IGenericCallServices> runs;

                if (PreferredOrderingGenericServices == null)
                    PreferredOrderingGenericServices = new FindServices<IGenericCallServices>(Options.commandservices[name].preferredServices).PreferredOrderingOfServices;
                if (PreferredOrderingGenericServices.Count() == 0)
                    throw new Exception("Generic: No services for:" + name);
                runs = PreferredOrderingGenericServices.TakeWhile((value, index) => index == 0 || Options.options.debugLevel >= 3);
                AllServiceResponseTasks = AllGenericServiceResponseTasks = null;

                if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
                {
                    text = fileName = operatorStack.Pop();
                    stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
                    if (fileName.First() == '@')
                    {
                        fileName = fileName.Substring(1); // remove '@'
                        string ext = System.IO.Path.GetExtension(fileName);
                        string[] imageExtensions = { ".img", ".jpeg", ".jpg", ".png" };
                        if (imageExtensions.Contains(ext))
                        {
                            System.Collections.Generic.IEnumerable<IGenericCallServices> binaryRuns = runs.Where(run => run.CompatibileArgTypesPerProvider["binary"] > 0);
                            if (binaryRuns.Count() == 0)
                                throw new Exception("No services available for argument of type binary:" + text);
                            bytes = System.IO.File.ReadAllBytes(fileName); // TODO: implement local file scheme (non-tempFolder directory)
                            var v = RunAllPreferredGenericServicesAsync(binaryRuns, bytes, apiArgs);
                            AllServiceResponseTasks = AllGenericServiceResponseTasks = RunAllPreferredGenericServicesAsync(binaryRuns, bytes, apiArgs);
                        }
                        else if (fileName.EndsWith(".wav"))
                        {
                            System.Collections.Generic.IEnumerable<IGenericCallServices> binaryRuns = runs.Where(run => run.CompatibileArgTypesPerProvider["binary"] > 0);
                            if (binaryRuns.Count() == 0)
                                throw new Exception("No services available for argument of type binary:" + text);
                            bytes = System.IO.File.ReadAllBytes(fileName); // TODO: implement local file scheme (non-tempFolder directory)
                            int sampleRate = await Audio.GetSampleRateAsync(fileName);
                            apiArgs.Add("sampleRate", sampleRate.ToString());
                            AllServiceResponseTasks = AllGenericServiceResponseTasks = RunAllPreferredGenericServicesAsync(binaryRuns, bytes, apiArgs);
                        }
                        else if (fileName.EndsWith(".json") || fileName.EndsWith(".txt")) // maybe this should be default?
                        {
                            System.Collections.Generic.IEnumerable<IGenericCallServices> textRuns = runs.Where(run => run.CompatibileArgTypesPerProvider["text"] > 0);
                            if (textRuns.Count() == 0)
                                throw new Exception("No services available for argument of type text:" + text);
                            text = System.IO.File.ReadAllText(fileName); // TODO: implement local file name scheme
                            AllServiceResponseTasks = AllGenericServiceResponseTasks = RunAllPreferredGenericServicesAsync(textRuns, text, apiArgs);
                            //await runs.First().CallServiceAsync("hello world", apiArgs);
                        }
                        else
                        {
                            throw new Exception("Generic: Unknown file extension:" + fileName);
                        }
                    }
                    else
                    {
                        if (text.StartsWith("http://") || text.StartsWith("https://"))
                        {
                            System.Collections.Generic.IEnumerable<IGenericCallServices> urlRuns = runs.Where(run => run.CompatibileArgTypesPerProvider["url"] > 0);
                            if (urlRuns.Count() == 0)
                                throw new Exception("No services available for argument of type url:" + text);
                            AllServiceResponseTasks = AllGenericServiceResponseTasks = RunAllPreferredGenericServicesAsync(urlRuns, new Uri(text), apiArgs);
                        }
                        else
                        {
                            System.Collections.Generic.IEnumerable<IGenericCallServices> textRuns = runs.Where(run => run.CompatibileArgTypesPerProvider["text"] > 0);
                            if (textRuns.Count() == 0)
                                throw new Exception("No services available for argument of type text:" + text);
                            AllServiceResponseTasks = AllGenericServiceResponseTasks = RunAllPreferredGenericServicesAsync(textRuns, text, apiArgs);
                        }
                    }
                }
                else // use operand on stack
                {
                    if (operandStack.Count == 0)
                        throw new Exception("Text: Nothing on stack");
                    stackFileName = operandStack.Pop();
                    if (stackFileName.EndsWith(".txt"))
                    {
                        text = await Helpers.ReadTextFromFileAsync(stackFileName); // TODO: implement local file scheme (non-tempFolder directory)
                        System.Collections.Generic.IEnumerable<IGenericCallServices> textRuns = runs.Where(run => run.CompatibileArgTypesPerProvider["text"] > 0);
                        if (textRuns.Count() == 0)
                            throw new Exception("No services available for argument of type text:" + text);
                        AllServiceResponseTasks = AllGenericServiceResponseTasks = RunAllPreferredGenericServicesAsync(textRuns, text, apiArgs);
                    }
                    else if (stackFileName.EndsWith(".wav"))
                    {
                        System.Collections.Generic.IEnumerable<IGenericCallServices> binaryRuns = runs.Where(run => run.CompatibileArgTypesPerProvider["binary"] > 0);
                        if (binaryRuns.Count() == 0)
                            throw new Exception("No services available for argument of type binary:" + name);
                        bytes = System.IO.File.ReadAllBytes(stackFileName); // TODO: implement local file scheme (non-tempFolder directory)
                        int sampleRate = await Audio.GetSampleRateAsync(stackFileName);
                        apiArgs.Clear();
                        apiArgs.Add("sampleRate", sampleRate.ToString());
                        AllServiceResponseTasks = AllGenericServiceResponseTasks = RunAllPreferredGenericServicesAsync(binaryRuns, bytes, apiArgs);
                    }
                    else
                    {
                        throw new Exception("Generic: Unknown file extension:" + stackFileName);
                    }
                }
                AllServiceResponses = AllGenericServiceResponseTasks.Select(sr => sr.Result);
                text = AllGenericServiceResponseTasks.First().Result.ResponseResult;
                await Helpers.WriteTextToFileAsync(stackFileName, text);
                operandStack.Push(stackFileName);
                return 0;
            }
        }

        public class EndCommand : IProcessACommand
        {
            public int stackChange { get; set; } = 0;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                operatorStack.Clear();
                return 1;
            }
        }

        public class HelpCommand : IProcessACommand
        {
            public int stackChange { get; set; } = 0;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                // TODO: string usage = Options.options.GetUsage();
                //Console.WriteLine(usage);
                Console.WriteLine("Commands:");
                foreach (System.Collections.Generic.KeyValuePair<string, verbAction> v in verbActionsAsync)
                    Console.WriteLine(v.Key.PadRight(16) + v.Value.helpTip);
                return 0;
            }
        }

        public class JsonPathCommand : IProcessACommand
        {
            public int stackChange { get; set; } = 0;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                if (operatorStack.Count > 0)
                {
                    string fileName = operandStack.Pop();
                    if (fileName.EndsWith(".txt"))
                    {
                        string jsonPath = operatorStack.Pop();
                        string text = await Helpers.ReadTextFromFileAsync(fileName);
#if true // which to use? stack or PreferredServiceResponse?
                        Newtonsoft.Json.Linq.JToken tokResult = PreferredServiceResponse.ResponseJToken.SelectToken(jsonPath);
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
        }

        public class ListenCommand : GenericRunServices, IProcessACommand
        {
            public int stackChange { get; set; } = +1;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                string text;
                string stackFileName;
                double listenTimeOut = Options.options.wakeup.listenTimeOut;

                stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
                if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
                {
                    text = operatorStack.Pop();
                    if (!char.IsDigit(text[0]))
                    {
                        throw new Exception("Listen: Expecting timeout value but found:" + text);
                    }
                    if (!double.TryParse(text, out listenTimeOut))
                    {
                        Console.WriteLine("Listen: Invalid number:" + text + ". Defaulting to value in settings file:" + listenTimeOut);
                    }
                }
                await Audio.MicrophoneToFileAsync(stackFileName, TimeSpan.FromSeconds(listenTimeOut));
                operandStack.Push(stackFileName);
                return 0;
            }
        }

        public class LoopCommand : IProcessACommand
        {
            public int stackChange { get; set; } = 0;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                operatorStack.Clear();
                foreach (string a in args.Reverse())
                    operatorStack.Push(a);
                return 0;
            }
        }

        public class ParseCommand : GenericCommand
        {
            public override async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                TextCommand t = new TextCommand();
                await t.verbCommandAsync("text", args, operatorStack, operandStack, apiArgs, null);
                return await base.verbCommandAsync(name, args, operatorStack, operandStack, apiArgs, PreferredOrderingGenericServices);
#if false
                Newtonsoft.Json.Linq.JArray arrayOfResults = Newtonsoft.Json.Linq.JArray.Parse(AllServiceResponses.First().ResponseResult);
                foreach (Newtonsoft.Json.Linq.JToken s in arrayOfResults)
                {
                    ConstituencyTreeNode root = ParseHelpers.ConstituencyTreeFromText(s.ToString());
                    string text = ParseHelpers.TextFromConstituencyTree(root);
                    if (text != s.ToString())
                        throw new FormatException();
                    string words = ParseHelpers.WordsFromConstituencyTree(root);
                    string[] printLines = ParseHelpers.FormatConstituencyTree(root);
                    foreach (string p in printLines)
                        Console.WriteLine(p);
                }
                return 0;
#endif
            }
        }

        public class PauseCommand : IProcessACommand
        {
            public int stackChange { get; set; } = 0;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
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
        }

#if false // keeping as sample of specialization
        public class PronounceCommand : PronounceCallServices, IProcessACommand
        {
            public virtual int stackChange { get; set; } = 0;
            public virtual async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                return await new GenericCommand().verbCommandAsync(name, args, operatorStack, operandStack, apiArgs, null); // new FindServices<IGenericCallServices>(Options.commandservices["Pronounce"].preferredServices).PreferredOrderingOfServices);
            }
        }
#endif

        public class QuitCommand : IProcessACommand
        {
            public int stackChange { get; set; } = 0;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                operatorStack.Clear();
                return 1;
            }
        }

        public class ReplayCommand : IProcessACommand
        {
            public int stackChange { get; set; } = 0;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
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
        }

        public class ResponseCommand : IProcessACommand
        {
            public int stackChange { get; set; } = +1;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                string stackFileName;
                stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
                if (Options.options.debugLevel >= 4)
                    foreach (ServiceResponse sr in AllServiceResponses)
                        Log.WriteLine(sr.Service.name + ":" + sr.ResponseResult);
                await Helpers.WriteTextToFileAsync(stackFileName, PreferredServiceResponse.ResponseResult);
                operandStack.Push(stackFileName);
                return 0;
            }
        }

        public class SettingsCommand : IProcessACommand
        {
            public int stackChange { get; set; } = 0;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
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
        }

        public class ShowCommand : IProcessACommand
        {
            public int stackChange { get; set; } = 0;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
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
                        // TODO: do wave file to text
                    }
                    else
                    {
                        throw new Exception("Show: Unknown file extension:" + fileName);
                    }
                }
                return 0;
            }
        }

        public class SpeakCommand : GenericRunServices, IProcessACommand
        {
            public int stackChange { get; set; } = -1;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                string text;
                byte[] bytes;
                string fileName;
                string stackFileName;
                System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>>> AllTextToSpeechServiceResponseTasks;
                System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingTextToSpeechServices = new FindServices<IGenericCallServices>(Options.commandservices["TextToSpeech"].preferredServices).PreferredOrderingOfServices;
                System.Collections.Generic.IEnumerable<IGenericCallServices> runs = PreferredOrderingTextToSpeechServices.TakeWhile((value, index) => index == 0 || Options.options.debugLevel >= 4);
                CallServiceResponse<IGenericServiceResponse> r;
                int sampleRate = Options.commandservices["TextToSpeech"].sampleRate;
                apiArgs.Add("sampleRate", sampleRate.ToString());

                stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
                if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
                {
                    text = fileName = operatorStack.Pop();
                    if (fileName.First() == '@')
                    {
                        fileName = fileName.Substring(1);
                        if (fileName.EndsWith(".txt"))
                        {
                            text = System.IO.File.ReadAllText(fileName); // TODO: implement local file name scheme
                            Log.WriteLine("Converting text to speech:" + text);
                            AllServiceResponseTasks = AllTextToSpeechServiceResponseTasks = RunAllPreferredGenericServicesAsync(runs, text, apiArgs);
                            r = AllTextToSpeechServiceResponseTasks.First().Result;
                            bytes = r.ResponseBytes;
                            await Helpers.WriteBytesToFileAsync(stackFileName, bytes);
                            Log.WriteLine("Playing wav file (" + r.ResponseBytes.Length + ").");
                            await Audio.PlayFileAsync(Options.options.tempFolderPath + stackFileName);
                        }
                        else if (fileName.EndsWith(".wav"))
                        {
                            Log.WriteLine("Playing wave file.");
                            await Audio.PlayFileAsync(fileName);
                        }
                        else
                        {
                            throw new Exception("Speak: Unknown file extension:" + fileName);
                        }
                    }
                    else
                    {
                        AllServiceResponseTasks = AllTextToSpeechServiceResponseTasks = RunAllPreferredGenericServicesAsync(runs, text, apiArgs);
                        r = AllTextToSpeechServiceResponseTasks.First().Result;
                        bytes = r.ResponseBytes;
                        await Helpers.WriteBytesToFileAsync(stackFileName, bytes);
                        Log.WriteLine("Playing wav file (" + r.ResponseBytes.Length + ").");
                        await Audio.PlayFileAsync(Options.options.tempFolderPath + stackFileName);
                    }
                }
                else // input coming from operand stack
                {
                    fileName = operandStack.Pop();
                    if (fileName.EndsWith(".txt"))
                    {
                        text = await Helpers.ReadTextFromFileAsync(fileName);
                        Log.WriteLine("Converting text to speech:" + text);
                        AllServiceResponseTasks = AllTextToSpeechServiceResponseTasks = RunAllPreferredGenericServicesAsync(runs, text, apiArgs);
                        r = AllTextToSpeechServiceResponseTasks.First().Result;
                        bytes = r.ResponseBytes;
                        await Helpers.WriteBytesToFileAsync(stackFileName, bytes);
                        Log.WriteLine("Playing wav file (" + r.ResponseBytes.Length + ").");
                        await Audio.PlayFileAsync(Options.options.tempFolderPath + stackFileName);
                    }
                    else if (fileName.EndsWith(".wav"))
                    {
                        Log.WriteLine("Playing wav file.");
                        await Audio.PlayFileAsync(fileName);
                    }
                    else
                    {
                        throw new Exception("Speak: Unknown file extension:" + fileName);
                    }
                }
                return 0;
            }
        }

        public class SpeechCommand : GenericRunServices, IProcessACommand
        {
            public int stackChange { get; set; } = +1;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                string text;
                byte[] bytes;
                string fileName;
                string stackFileName;
                System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>>> AllTextToSpeechServiceResponseTasks;
                System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingTextToSpeechServices = new FindServices<IGenericCallServices>(Options.commandservices["TextToSpeech"].preferredServices).PreferredOrderingOfServices;
                System.Collections.Generic.IEnumerable<IGenericCallServices> runs = PreferredOrderingTextToSpeechServices.TakeWhile((value, index) => index == 0 || Options.options.debugLevel >= 4);
                CallServiceResponse<IGenericServiceResponse> r;
                int sampleRate = Options.commandservices["TextToSpeech"].sampleRate;
                apiArgs.Add("sampleRate", sampleRate.ToString());

                if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
                {
                    text = fileName = operatorStack.Pop();
                    stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
                    if (fileName.First() == '@')
                    {
                        fileName = fileName.Substring(1);
                        if (fileName.EndsWith(".txt"))
                        {
                            text = System.IO.File.ReadAllText(fileName); // TODO: implement local file name scheme
                            AllServiceResponseTasks = AllTextToSpeechServiceResponseTasks = RunAllPreferredGenericServicesAsync(runs, text, apiArgs);
                            r = AllTextToSpeechServiceResponseTasks.First().Result;
                            bytes = r.ResponseBytes;
                            await Helpers.WriteBytesToFileAsync(stackFileName, bytes);
                        }
                        else if (fileName.EndsWith(".wav"))
                        {
                            bytes = System.IO.File.ReadAllBytes(fileName); // TODO: implement local file scheme (non-tempFolder directory)
                            await Helpers.WriteBytesToFileAsync(stackFileName, bytes);
                        }
                        else
                        {
                            throw new Exception("Speak: Unknown file extension:" + fileName);
                        }
                    }
                    else
                    {
                        AllServiceResponseTasks = AllTextToSpeechServiceResponseTasks = RunAllPreferredGenericServicesAsync(runs, text, apiArgs);
                        r = AllTextToSpeechServiceResponseTasks.First().Result;
                        bytes = r.ResponseBytes;
                        await Helpers.WriteBytesToFileAsync(stackFileName, bytes);
                    }
                }
                else // use operand on stack
                {
                    if (operandStack.Count == 0)
                        throw new Exception("Generic: Nothing on stack");
                    stackFileName = operandStack.Pop();
                    if (stackFileName.EndsWith(".txt"))
                    {
                        text = await Helpers.ReadTextFromFileAsync(stackFileName); // TODO: implement local file name scheme
                        AllServiceResponseTasks = AllTextToSpeechServiceResponseTasks = RunAllPreferredGenericServicesAsync(runs, text, apiArgs);
                        r = AllTextToSpeechServiceResponseTasks.First().Result;
                        bytes = r.ResponseBytes;
                        stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".wav";
                        await Helpers.WriteBytesToFileAsync(stackFileName, bytes);
                    }
                    else if (stackFileName.EndsWith(".wav"))
                    {
                        bytes = await Helpers.ReadBytesFromFileAsync(stackFileName); // TODO: implement local file scheme (non-tempFolder directory)
                    }
                    else
                    {
                        throw new Exception("Speech: Unknown file extension:" + stackFileName);
                    }
                }
                operandStack.Push(stackFileName);
                if (Options.options.debugLevel >= 3)
                {
                    System.Collections.Generic.Dictionary<string, string> newApiArgs = new System.Collections.Generic.Dictionary<string, string>();
                    int newSampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + stackFileName);
                    newApiArgs.Add("sampleRate", newSampleRate.ToString());
                    System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingSpeechToTextServices = new FindServices<IGenericCallServices>(Options.commandservices["SpeechToText"].preferredServices).PreferredOrderingOfServices;
                    System.Collections.Generic.IEnumerable<IGenericCallServices> newRuns = PreferredOrderingSpeechToTextServices.TakeWhile((value, index) => index == 0 || Options.options.debugLevel >= 4);
                    if (newRuns.Count() == 0)
                        throw new Exception("No services available for argument of type binary:" + name);
                    System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>>> AllSpeechToTextServiceResponses = RunAllPreferredGenericServicesAsync(newRuns, bytes, newApiArgs);
                    CallServiceResponse<IGenericServiceResponse> sttr = AllSpeechToTextServiceResponses.First().Result;
                    text = sttr.ResponseResult;
                    Console.WriteLine("Speech: text:" + text);
                }
                return 0;
            }
        }

        public class TextCommand : GenericCommand
        {
            public override int stackChange { get; set; } = +1;
            public override async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                string text;
                byte[] bytes;
                string fileName;
                string stackFileName;
                System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>>> AllTextServiceResponseTasks;

                if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
                {
                    stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
                    text = fileName = operatorStack.Pop();
                    if (fileName.First() == '@')
                    {
                        fileName = fileName.Substring(1);
                        if (fileName.EndsWith(".txt"))
                        {
                            text = System.IO.File.ReadAllText(fileName); // TODO: implement local file name scheme
                        }
                        else if (fileName.EndsWith(".wav"))
                        {
                            bytes = System.IO.File.ReadAllBytes(fileName); // TODO: implement local file scheme (non-tempFolder directory)
                            System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingSpeechToTextServices = new FindServices<IGenericCallServices>(Options.commandservices["SpeechToText"].preferredServices).PreferredOrderingOfServices;
                            System.Collections.Generic.IEnumerable<IGenericCallServices> runs = PreferredOrderingSpeechToTextServices.TakeWhile((value, index) => index == 0 || Options.options.debugLevel >= 3);
                            System.Collections.Generic.IEnumerable<IGenericCallServices> binaryRuns = runs.Where(run => run.CompatibileArgTypesPerProvider["binary"] > 0);
                            if (binaryRuns.Count() == 0)
                                throw new Exception("No services available for argument of type binary:" + text);
                            int sampleRate = await Audio.GetSampleRateAsync(fileName);
                            apiArgs.Add("sampleRate", sampleRate.ToString());
                            AllServiceResponseTasks = AllTextServiceResponseTasks = RunAllPreferredGenericServicesAsync(runs, bytes, apiArgs);
                            CallServiceResponse<IGenericServiceResponse> sttr = AllTextServiceResponseTasks.First().Result;
                            text = sttr.ResponseResult;
                        }
                        else
                        {
                            throw new Exception("Generic: Unknown file extension:" + fileName);
                        }
                    }
                    else
                    {
                        if (text.StartsWith("http://") || text.StartsWith("https://"))
                        {
                            throw new Exception("Url not supported for text.");
                        }
                        else
                        {
                            // do nothing. already text
                        }
                    }
                }
                else // use operand on stack
                {
                    if (operandStack.Count == 0)
                        throw new Exception("Text: Nothing on stack");
                    stackFileName = operandStack.Pop();
                    if (stackFileName.EndsWith(".txt"))
                    {
                        text = await Helpers.ReadTextFromFileAsync(stackFileName); // TODO: implement local file scheme (non-tempFolder directory)
                    }
                    else if (stackFileName.EndsWith(".wav"))
                    {
                        bytes = await Helpers.ReadBytesFromFileAsync(stackFileName); // TODO: implement local file scheme (non-tempFolder directory)
                        System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingSpeechToTextServices = new FindServices<IGenericCallServices>(Options.commandservices["SpeechToText"].preferredServices).PreferredOrderingOfServices;
                        System.Collections.Generic.IEnumerable<IGenericCallServices> runs = PreferredOrderingSpeechToTextServices.TakeWhile((value, index) => index == 0 || Options.options.debugLevel >= 4);
                        System.Collections.Generic.IEnumerable<IGenericCallServices> binaryRuns = runs.Where(run => run.CompatibileArgTypesPerProvider["binary"] > 0);
                        if (binaryRuns.Count() == 0)
                            throw new Exception("No services available for argument of type binary:" + stackFileName);
                        int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + stackFileName); // need "byte[] bytes" overload to avoid double read.
                        apiArgs.Add("sampleRate", sampleRate.ToString());
                        AllServiceResponseTasks = AllTextServiceResponseTasks = RunAllPreferredGenericServicesAsync(runs, bytes, apiArgs);
                        CallServiceResponse<IGenericServiceResponse> sttr = AllTextServiceResponseTasks.First().Result;
                        text = sttr.ResponseResult;
                    }
                    else
                    {
                        throw new Exception("Generic: Unknown file extension:" + stackFileName);
                    }
                    stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
                }
                await Helpers.WriteTextToFileAsync(stackFileName, text);
                operandStack.Push(stackFileName);
                return 0;
            }
        }

        public class TranslateCommand : GenericCommand
        {
            public override async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                string source = Options.commands["Translate"].source;
                string target = Options.commands["Translate"].target;

                if (operatorStack.Count > 0 && !verbActionsAsync.ContainsKey(operatorStack.Peek()))
                {
                    string text = operatorStack.Pop();
                    if (text.Contains("="))
                    {
                        // need cross-API solution for specifying language pairs. Use both Commands and every TranslateService?
                        string[] languagePairs = text.Split('=');
                        if (languagePairs.Length == 2)
                        {
                            source = languagePairs[0];
                            target = languagePairs[1];
                            Console.WriteLine("Using language pair:" + languagePairs[0] + "=" + languagePairs[1]);
                        }
                        else
                            Console.WriteLine("Invalid language pair:" + text + ". Try en-es.");
                    }
                    else
                        operatorStack.Push(text);
                }

                apiArgs.Add("source", source);
                apiArgs.Add("target", target);
                return await base.verbCommandAsync(name, args, operatorStack, operandStack, apiArgs, PreferredOrderingGenericServices);
            }
        }

        public class WakeUpCommand : IProcessACommand
        {
            public int stackChange { get; set; } = +1;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
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
                            text = System.IO.File.ReadAllText(fileName.Substring(1)); // TODO: implement local file name scheme
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

        public class XPathCommand : IProcessACommand
        {
            // todo: not implemented/debugged
            public int stackChange { get; set; } = 0;
            public async System.Threading.Tasks.Task<int> verbCommandAsync(string name, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack, System.Collections.Generic.Dictionary<string, string> apiArgs, System.Collections.Generic.IEnumerable<IGenericCallServices> PreferredOrderingGenericServices)
            {
                if (operatorStack.Count > 0)
                {
                    string fileName = operandStack.Pop();
                    if (fileName.EndsWith(".txt"))
                    {
                        string xpath = operatorStack.Pop();
                        string text = await Helpers.ReadTextFromFileAsync(fileName);
#if true // which to use? stack or PreferredServiceResponse?
                        System.Xml.XmlNodeList xmlNodes = PreferredServiceResponse.ResponseXml.SelectNodes(xpath);
                        string xmlResult = xmlNodes.ToString(); // must be wrong
#else
                        XDocument xml = XDocument.Parse(popstack);
#endif
                        string stackFileName = "stack" + (operandStack.Count + 1).ToString() + ".txt";
                        await Helpers.WriteTextToFileAsync(stackFileName, xmlResult.ToString());
                        operandStack.Push(stackFileName);
                    }
                }
                else
                {
                    Console.WriteLine("XPath has no arguement.");
                }
                return 0;
            }
        }
    }
}
