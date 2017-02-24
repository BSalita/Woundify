using System;

namespace WoundifyShared
{
    class ProfferedCommands
    {
        private static System.Collections.Generic.Dictionary<string, Func<string[], string[], System.Collections.Generic.Stack<string>, System.Collections.Generic.Stack<string>, System.Threading.Tasks.Task<int>>> commandActions =
            new System.Collections.Generic.Dictionary<string, Func<string[], string[], System.Collections.Generic.Stack<string>, System.Collections.Generic.Stack<string>, System.Threading.Tasks.Task<int>>>(StringComparer.OrdinalIgnoreCase)
            {
                { "END", ProfferCommandEndAsync },
                { "HELP", ProfferCommandHelpAsync },
                { "QUIT", ProfferCommandQuitAsync },
            };

        public static async System.Threading.Tasks.Task<int> ProfferCommandAsync(string[] words, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            int action = 0;
            foreach (System.Collections.Generic.KeyValuePair<string, Func<string[], string[], System.Collections.Generic.Stack<string>, System.Collections.Generic.Stack<string>, System.Threading.Tasks.Task<int>>> c in ProfferedCommands.commandActions)
            {
                if (c.Key == words[1])
                {
                    action = await c.Value(words, args, operatorStack, operandStack);
                    break;
                }
            }
            return action;
        }

        private static async System.Threading.Tasks.Task<int> ProfferCommandEndAsync(string[] words, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            return 1;
        }

        private static async System.Threading.Tasks.Task<int> ProfferCommandHelpAsync(string[] words, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            if (Options.options.debugLevel >= 4)
                Log.WriteLine("Processing help request. words.Length=" + words.Length);
            // HELP command not implemented.
            if (words.Length > 2)
            {
                // parse out HELP command.
                switch (words[2])
                {
                    case "WHAT":
                        await WhatCanISayAsync();
                        break;
                    default:
                        await WhatCanISayAsync();
                        break;
                }
            }
            else
                await WhatCanISayAsync();

            return -1;
        }

        private static async System.Threading.Tasks.Task<int> ProfferCommandQuitAsync(string[] words, string[] args, System.Collections.Generic.Stack<string> operatorStack, System.Collections.Generic.Stack<string> operandStack)
        {
            return 1;
        }

        private static async System.Threading.Tasks.Task WhatCanISayAsync()
        {
            System.Collections.Generic.Dictionary<string, string> apiArgs = new System.Collections.Generic.Dictionary<string, string>();
            await TextToSpeech.TextToSpeechServiceAsync("Ask me about the weather, a Wikipedia entry, a definition or quit or end.", apiArgs);
        }
    }
}
