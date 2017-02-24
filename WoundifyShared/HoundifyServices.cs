using System;
using System.Linq;

using System.Runtime.InteropServices.WindowsRuntime; // AsBuffer()

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WoundifyShared
{
    class HoundifyIntentServices : GenericCallServices // specific to Intent. Not used for Houndify SpeechToText.
    {
        public HoundifyIntentServices(Settings.Service service) : base(service)
        {
        }
        public override async System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>> CallServiceAsync(byte[] audioBytes, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            Log.WriteLine("HoundifyIntentServices: audioBytes.Length:" + audioBytes.Length);
            //IntentServiceResponse response = new IntentServiceResponse(service);
            CallServiceResponse<IGenericServiceResponse> response = new CallServiceResponse<IGenericServiceResponse>(service);
            response.Request = Array.Find(service.requests, p => p.argType == "binary");
            await HoundifyServices.HoundifyPostAsync(service, response, HttpMethods.MakeUri(service.requests[0], apiArgs, null, ""), audioBytes, apiArgs);
            return response;
        }

        public override async System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>> CallServiceAsync(string text, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            Log.WriteLine("HoundifyIntentServices: text:" + text);
            //IntentServiceResponse response = new IntentServiceResponse(service);
            CallServiceResponse<IGenericServiceResponse> response = new CallServiceResponse<IGenericServiceResponse>(service);
            response.Request = Array.Find(service.requests, p => p.argType == "text");
            await HoundifyServices.HoundifyPostAsync(service, response, HttpMethods.MakeUri(service.requests[0], apiArgs, null, text), new byte[0], apiArgs);
            return response;
        }

    }
    class HoundifySpeechToTextServices : GenericCallServices // specific to Intent. Not used for Houndify SpeechToText.
    {
        public HoundifySpeechToTextServices(Settings.Service service) : base(service)
        {
        }
        public override async System.Threading.Tasks.Task<CallServiceResponse<IGenericServiceResponse>> CallServiceAsync(byte[] audioBytes, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            Log.WriteLine("HoundifySpeechToTextServices: audioBytes.Length:" + audioBytes.Length);
            //IntentServiceResponse response = new IntentServiceResponse(service);
            CallServiceResponse<IGenericServiceResponse> response = new CallServiceResponse<IGenericServiceResponse>(service);
            response.Request = Array.Find(service.requests, p => p.argType == "binary");
            await HoundifyServices.HoundifyPostAsync(service, response, HttpMethods.MakeUri(service.requests[0], apiArgs, null, ""), audioBytes, apiArgs);
            return response;
        }
    }

    static class HoundifyServices
    {
        // save IntentConversationState (info about request and requestor) obtained from Houndify's ResultInfo for passing on to next Houndify request.
        private static JToken IntentConversationState = null; // need this to expire
                                                              // override is working but somewhat verbose. Need to explore other methods such as passing Type? Need to combine Command->Run->Call as they're all the same Type. Make tail of Command into virtual.
                                                              //public override async System.Threading.Tasks.Task<GenericCallServiceResponse<IHoundifyServiceResponse>> CallServiceAsync(byte[] bytes, System.Collections.Generic.Dictionary<string, string> apiArgs)

        public static async System.Threading.Tasks.Task HoundifyPostAsync(Settings.Service service, ServiceResponse response, Uri uri, byte[] RequestContentBytes, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            Log.WriteLine("Content length:" + RequestContentBytes.Length);

            string ClientID = service.requests[0].headers[0].HoundifyAuthentication.ClientID; // moot? throws Exception thrown: 'Microsoft.CSharp.RuntimeBinder.RuntimeBinderException' in Microsoft.CSharp.dll
            string ClientKey = service.requests[0].headers[0].HoundifyAuthentication.ClientKey; // moot? Exception thrown: 'Microsoft.CSharp.RuntimeBinder.RuntimeBinderException' in Microsoft.CSharp.dll
            string UserID = service.requests[0].headers[0].HoundifyAuthentication.UserID;
            JObject RequestBodyObject;

            RequestBodyObject = JObject.FromObject(new
            {
                Latitude = GeoLocation.latitude,
                Longitude = GeoLocation.longitude,
                City = GeoLocation.town,
                Country = GeoLocation.country,
                UserID = UserID,
                ClientID = ClientID,
                // audio specific
                PartialTranscriptsDesired = Options.services["HoundifyIntentAudioService"].service.requests[0].PartialTranscriptsDesired,
                //ConversationState = IntentConversationState,
            });
            if (IntentConversationState != null)
                RequestBodyObject.Add(IntentConversationState);

            string RequestBodyJson = JsonConvert.SerializeObject(RequestBodyObject); // no formatting. Could use ToString() but it formats (spaces, EOL).
            if (Options.options.debugLevel >= 4)
                Log.WriteLine("RequestBodyJson:" + RequestBodyJson);

            byte[] RequestBodyBytes = System.Text.Encoding.UTF8.GetBytes(RequestBodyJson).Concat(RequestContentBytes).ToArray();
            //byte[] RequestBodyBytes = System.Text.Encoding.UTF8.GetBytes(RequestBodyJson).Concat(ReadBytesFromFile("wb_male.wav")).ToArray();

            string RequestID = System.Guid.NewGuid().ToString("D"); // Houndify requires lower case?
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            string Timestamp = Math.Floor(t.TotalSeconds).ToString();

            string FriendlyClientKey = ClientKey.Replace('-', '+').Replace('_', '/'); // translate possible PHP encode style to FromBase64String style
#if WINDOWS_UWP
            Windows.Storage.Streams.IBuffer KeyBytes = Windows.Security.Cryptography.CryptographicBuffer.DecodeFromBase64String(FriendlyClientKey);
            Windows.Storage.Streams.IBuffer MessageBytes = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(UserID + ";" + RequestID + Timestamp, Windows.Security.Cryptography.BinaryStringEncoding.Utf8);
            Windows.Security.Cryptography.Core.MacAlgorithmProvider MacAlg = Windows.Security.Cryptography.Core.MacAlgorithmProvider.OpenAlgorithm(Windows.Security.Cryptography.Core.MacAlgorithmNames.HmacSha256);
            Windows.Security.Cryptography.Core.CryptographicHash MacHash = MacAlg.CreateHash(KeyBytes);
            MacHash.Append(MessageBytes);
            Windows.Storage.Streams.IBuffer MacHashBuf = MacHash.GetValueAndReset();
            string HashSignature = Windows.Security.Cryptography.CryptographicBuffer.EncodeToBase64String(MacHashBuf).Replace('+', '-').Replace('/', '_');
#else
            byte[] KeyBytes = Convert.FromBase64String(FriendlyClientKey); // TODO: shouldn't this be System.Text.Encoding.UTF8.GetBytes?
            byte[] MessageBytes = System.Text.Encoding.UTF8.GetBytes(UserID + ";" + RequestID + Timestamp);
            byte[] HashBytes = new System.Security.Cryptography.HMACSHA256(KeyBytes).ComputeHash(MessageBytes); // always length of 32?
            string HashSignature = Convert.ToBase64String(HashBytes).Replace('+', '-').Replace('/', '_');
#endif
            string HoundRequestAuthentication = UserID + ";" + RequestID;
            string HoundClientAuthentication = ClientID + ";" + Timestamp + ";" + HashSignature;

            if (Options.options.debugLevel >= 4)
            {
                Log.WriteLine("Uri:" + uri);
                Log.WriteLine("UserID:" + UserID);
                Log.WriteLine("RequestID:" + RequestID);
                Log.WriteLine("Timestamp:" + Timestamp);
                Log.WriteLine("ClientID:" + ClientID);
                Log.WriteLine("ClientKey:" + ClientKey);
                Log.WriteLine("FriendlyClientKey:" + FriendlyClientKey);
                Log.WriteLine("HashSignature:" + HashSignature);
                Log.WriteLine("HoundRequestAuthentication:" + HoundRequestAuthentication);
                Log.WriteLine("HoundClientAuthentication:" + HoundClientAuthentication);
            }
#if true
            // TODO: put curl into woundifysettings.json?
            Log.WriteLine("curl -X POST" + " --data-binary @computer.wav" + " --header \"Hound-Request-Authentication:" + HoundRequestAuthentication + "\" --header \"Hound-Client-Authentication:" + HoundClientAuthentication + "\" --header \"Hound-Request-Info:" + RequestBodyJson.Replace('"', '\'') + "\" " + uri);
#endif
            System.Collections.Generic.List<Tuple<string, string>> headers = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Hound-Request-Info-Length", RequestBodyJson.Length.ToString()),
                new Tuple<string, string>("Hound-Request-Authentication", HoundRequestAuthentication),
                new Tuple<string, string>("Hound-Client-Authentication", HoundClientAuthentication)
            };
            await HttpMethods.CallApiAsync(response, uri, RequestBodyBytes, apiArgs, headers);
            ProcessResponse(response.ResponseJToken);
            await HttpMethods.ExtractResultAsync(response);
        }

        private static JToken ProcessResponse(JToken response)
        {
            JToken tok = null;
            if (response == null)
                Log.WriteLine("Fail! - Response is null");
            else
            {
                if ((response.SelectToken("$.Status")) == null)
                    Log.WriteLine("Status property not found");
                else
                    Log.WriteLine("Status is " + response.SelectToken("$.Status").ToString());
                if ((tok = response.SelectToken("$.AllResults[0].ConversationState")) != null) // not sure why && tok.Children().Count doesn't compile
                    IntentConversationState = tok.Children().First();
            }
            return tok;
        }
    }
}
