using System;
using Newtonsoft.Json.Linq;
using System.Web;

namespace WoundifyShared
{
    class MicrosoftCognitiveParseAnalyzersServices : GenericCallServices // not actually called as a command. Called only on first invocation of MicrosoftCognitiveParseServices
    {
        public MicrosoftCognitiveParseAnalyzersServices(Settings.Service service) : base(service)
        {
        }

        public static string[] ParseAnalysers; // Parse service only
        public static string ParseAnalyzerStringized; // Parse service only

        public override async System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>> CallServiceAsync(string text, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            // so far text and apiArgs are unused. The purpose of this call is to make a one-time invocation to retrieve Microsoft's parse analyzer IDs.
            Log.WriteLine("MicrosoftCognitiveParseAnalyzersServices: CallServiceAsync");
            CallServiceResponse<IGenericServiceResponse> response = new CallServiceResponse<IGenericServiceResponse>(service);
            response.Request = Array.Find(service.requests, p => p.argType == "text");
            await HttpMethods.CallApiAsync(response, text, apiArgs);
#if true // TODO: implement ExtractResultAsync?
            await HttpMethods.ExtractResultAsync(response);
            ParseAnalysers = response.ResponseResult.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries); // split at ", " into array
            ParseAnalyzerStringized = " \"" + string.Join("\", \"", ParseAnalysers) + "\" "; // rejoing array into a string. each item must be quoted and separated by a comma.
#else
            ParseAnalysers = new System.Collections.Generic.List<string>();
            foreach (JToken tokAnalyzerResult in response.ResponseJToken)
            {
                ParseAnalysers.Add(tokAnalyzerResult["id"].ToString());
                ParseAnalyzerStringized += "'" + tokAnalyzerResult["id"].ToString() + "', ";
            }
            ParseAnalyzerStringized = ParseAnalyzerStringized.Substring(0, ParseAnalyzerStringized.Length - 2); // remove trailing "', "
#endif
            return response;
        }
    }

    class MicrosoftCognitiveParseServices : GenericCallServices
    {
        public MicrosoftCognitiveParseServices(Settings.Service service) : base(service)
        {
        }
        public override async System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>> CallServiceAsync(string text, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            Log.WriteLine("MicrosoftCognitiveParseServices: Text:" + text);
            if (MicrosoftCognitiveParseAnalyzersServices.ParseAnalysers == null) // calling ParseAnalyzersService. It's a dependency of ParseService.
            {
                CallServiceResponse<IGenericServiceResponse> responseAnalyzers = new CallServiceResponse<IGenericServiceResponse>(service);
                MicrosoftCognitiveParseAnalyzersServices parseAnalyersService = new MicrosoftCognitiveParseAnalyzersServices(Options.services["MicrosoftCognitiveParseAnalyzersService"].service);
                responseAnalyzers = await parseAnalyersService.CallServiceAsync(text, apiArgs);
                if (MicrosoftCognitiveParseAnalyzersServices.ParseAnalysers == null || MicrosoftCognitiveParseAnalyzersServices.ParseAnalysers.Length == 0 || string.IsNullOrWhiteSpace(MicrosoftCognitiveParseAnalyzersServices.ParseAnalyzerStringized))
                    throw new InvalidOperationException(); // can't continue without at least one Analyzer
            }
            CallServiceResponse<IGenericServiceResponse> response = new CallServiceResponse<IGenericServiceResponse>(service);
            response.Request = Array.Find(service.requests, p => p.argType == "text");
            System.Collections.Generic.List<Tuple<string, string>> jsonSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("{AnalyzerStringized}", MicrosoftCognitiveParseAnalyzersServices.ParseAnalyzerStringized),
            };
            await HttpMethods.CallApiAsync(response, null, null, jsonSubstitutes, text, apiArgs); // example of using jsonSubstitutes
            await HttpMethods.ExtractResultAsync(response);
            Newtonsoft.Json.Linq.JArray arrayOfResults = Newtonsoft.Json.Linq.JArray.Parse(response.ResponseResult);
            foreach (Newtonsoft.Json.Linq.JToken s in arrayOfResults)
            {
                ConstituencyTreeNode root = ParseHelpers.ConstituencyTreeFromText(s.ToString());
                string textTree = ParseHelpers.TextFromConstituencyTree(root);
                if (textTree != s.ToString())
                    throw new FormatException();
                string words = ParseHelpers.WordsFromConstituencyTree(root);
                string[] printLines = ParseHelpers.FormatConstituencyTree(root);
                foreach (string p in printLines)
                    Console.WriteLine(p);
            }
            return response;
        }
    }
}
