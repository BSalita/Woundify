using System;

namespace WoundifyShared
{
    public class TextToSpeechServices
    {
        public static System.Collections.Generic.List<ITextToSpeechService> PreferredOrderedTextToSpeechServices = new FindServices<ITextToSpeechService>(Options.commandservices["TextToSpeech"].preferredServices).PreferredOrderingOfServices;

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<TextToSpeechServiceResponse>> RunAllPreferredTextToSpeechServices(string fileName)
        {
            string text = await Helpers.ReadTextFromFileAsync(fileName);
            int sampleRate = await Audio.GetSampleRateAsync(Options.options.tempFolderPath + fileName);
            return await RunAllPreferredTextToSpeechServices(text, sampleRate);
        }

        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<TextToSpeechServiceResponse>> RunAllPreferredTextToSpeechServices(string text, int sampleRate)
        {
            System.Collections.Generic.List<TextToSpeechServiceResponse> responses = new System.Collections.Generic.List<TextToSpeechServiceResponse>();
            // invoke each ITextToSpeechService and show what it can do.
            foreach (ITextToSpeechService TTS in PreferredOrderedTextToSpeechServices)
            {
                responses.Add(await TTS.TextToSpeechServiceAsync(text, sampleRate).ContinueWith<TextToSpeechServiceResponse>((c) =>
                {
                    ServiceResponse r = c.Result.sr;
                    if (string.IsNullOrEmpty(r.ResponseResult) || r.StatusCode != 200)
                        Console.WriteLine(r.ServiceName + " TTS (async): Failed with StatusCode of " + r.StatusCode);
                    else
                        Console.WriteLine(r.ServiceName + " TTS (async):\"" + r.ResponseResult + "\" Total " + r.TotalElapsedMilliseconds + "ms Request " + r.RequestElapsedMilliseconds + "ms");
                    return c.Result;
                }));
            }
            return responses;
        }

#if false
        public static async System.Threading.Tasks.Task<System.Collections.Generic.List<ITextToSpeechServiceResponse>> RunAllPreferredTextToSpeechServices(byte[] bytes, int sampleRate)
        {
            System.Collections.Generic.List<ITextToSpeechServiceResponse> responses = new System.Collections.Generic.List<ITextToSpeechServiceResponse>();
            // invoke each ITextToSpeechService and show what it can do.
            foreach (ITextToSpeechService TTS in PreferredITextToSpeechServices)
            {
                await TTS.TextToSpeechServiceAsync(Settings.Service servicebytes, sampleRate).ContinueWith<ITextToSpeechServiceResponse>((c) =>
                {
                    if (string.IsNullOrEmpty(c.Result.sr.ResponseResult) || c.Result.sr.StatusCode != 200)
                        Console.WriteLine(r.ServiceName + " TTS (async): Failed with StatusCode of " + c.Result.StatusCode);
                    else
                        Console.WriteLine(r.ServiceName + " TTS (async):\"" + c.Result.ResponseResult + "\" Total " + c.Result.TotalElapsedMilliseconds + "ms Request " + c.Result.RequestElapsedMilliseconds + "ms");
                    responses.Add(c.Result);
                });
            }
            return responses;
        }
#endif
    }
}
