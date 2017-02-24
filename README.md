# Woundify
Woundify is a cognitive services (AI) tool used to compare, benchmark, script, demonstrate API calling, and consumption of major AI services. Woundify is written in C# and is compatible with Windows Console, WPF and UWP systems. Woundify supports AI services from ApiAI, ClarifAI, Google Cloud, Houndify, HPE Haven, IBM Watson, Microsoft Cognitive, and Wit.ai. Woundify was originally developed as a Windows client for the Houndify intent service but has expanded to include all known AI API providers.

## Requisites

Tasks:

1. Install Visual Studio 2015. The free Community Edition is fine.
2. Load this repo into Visual Studio.
3. Register for the AI services that you'll be using (Bing, Google, Houndify, HPE, IBM, etc.). URLs listed below.
4. No API access keys are provided. You'll need to add your own API access keys to either `WoundifyDefaultSettings.json` or  `WoundifySettings.json`. If `WoundifySettings.json` does not exist, create it by copying `WoundifyDefaultSettings.json` to `WoundifySettings.json`. `WoundifyDefaultlSettings.json` is loaded first and then `WoundifySettings.json` is loaded overriding the defaults. Recommended practice is to modify `WoundifySettings.json` and never modifying `WoundifyDefaultlSettings.json`.

The `WoundifyDefaultSettings.json` and `WoundifySettings.json` files contain properties used for each service such as API acccess keys. Services all use differing API access property names (e.g. Key, ClientID, Password, etc.). Register for the following services and insert their API access keys into the "services" section of the settings file. Be careful when modifying these files. Mistakes can cause unpredictable behaviors.

1. https://www.houndify.com
2. https://console.ng.bluemix.net/registration/?
3. https://cloud.google.com/speech/
4. https://cloud.google.com/translate/
5. https://www.microsoft.com/cognitive-services/en-us/speech-api
6. https://datamarket.azure.com/dataset/bing/microsofttranslatorspeech
7. https://datamarket.azure.com/dataset/bing/speechoutput
8. https://datamarket.azure.com/dataset/bing/microsofttranslator
9. https://dev.havenondemand.com/
10. https://www.ApiAi.com/
11. https://www.ClarifAI.com/
12. https://www.Wit.ai/

## Console Application
The woundify.exe is a console app tool for scripting audio, text and AI services. There are commands for recording audio, converting text-to-speech (TTS), converting speech-to-text (STT), and invoking many of the currently offered AI services.

> `Usage: woundify.exe [command]*`

woundify.exe accepts zero or more commands. See the chart below for an explanation of commands. Each command has zero or one argument. An argument may be text (e.g. "Hello World"), or @filename.ext (e.g. @HelloWorld.txt or @HelloWorld.wav). Files must be either text (.txt) or wave audio recording (.wav). Commands are usually chained together for greatest effect. The command processor is stack oriented; some commands push results on to the stack, others consume the stack. Executing woundify.exe without any argument will cause woundify to enter an internal command mode.

Examples:

> `woundify help` <br>Displays a list of commands.
  
> `woundify text "What's the weather in Paris?" intent speak`<br>Sends the text to Houndify and speaks the response.
  
> `woundify listen intent speak`<br>Listens to the microphone (default timeout), calls intent service, speaks the response.
  
> `woundify text @WeatherInParis.txt intent show`<br>Send the contents of file WeatherInParis.txt to Houndify and shows the stack.
  
> `woundify speech @WeatherInParis.wav intent show`<br>Sends the wave file to Houndify and shows the stack.
  
> `woundify wakeup intent speak loop`<br>Listens for wakeup word(s) (default "computer") and whatever follows, sends to Houndify, speaks response and loops back to wakeup. This is similar to the behavior of Houndify's mobile app or Amazon Echo.

> `woundify text "What hath God wrought?" parse` <br>Parse into grammatical units.
## Installation
There is no binary executable file available on this repos. You can create an executable using this repos with Visual Studio 2015 Community Edition (free).

## Terminology
* TOS means Top Of Stack. Woundify command processing is stack oriented.
* TTS means converts Text-To-Speech
* STT means converts Speech-To-Text
* UWP refers to Microsoft's Universal Windows Platform. See https://en.wikipedia.org/wiki/Universal_Windows_Platform

## Development
Developers can use Woundify as a standalone tool, as a tool for integrating into projects, or make use of its class libraries to create a custom project. This repos contains the entire source code of Woundify.

The source code for woundify is in C#. The classes contain a wealth of information. In particular, most operations are coded twice for maximum Windows support; Win32 vs WinRT, System.Speech vs Windows.Media, System.IO vs Windows.Storage, System.Net.HTTP vs Windows.Web, System.Security.Cryptography vs Windows.Security.Cryptography, Console and WPF vs UWP. The source code contains the following capabilities:
* Authenticating to Bing (OAuth 2), Google (OAuth 2), Houndify services (propriatary), IBM Watson (basic) Wit (OAuth 2).
* Invoking Houndify intent API.
* Invoking speech-to-text APIs from Bing (Project Oxford), Google, Houndify, IBM Watson, Wit.
* Parsing JSON responses from intent and STT services.
* Recording audio from microphone and writing to a stream or file.
* Playing audio from stream or file.
* Transcoding audio (WinRT only but can be done with NAudio too).
* Building audio graphs. (WinRT only).
* Listening for wakup words.
* Using JSON for requests, responses, objects, settings.
* Geolocating (WinRT only).
* Async/Await programming style for Console, WPF and UWP systems.
* Using reflection to obtain a list of classes implementing a specific interface.
* Specifying a preferred ordering of API calls via a JSON settings file.
* Parse text into Penn Treebank using Linguistic API from Microsoft Cognitive Services
* No vendor SDKs used. Explicit HTTP calls only.

Dependencies:
* Windows 7+ for Console and WPF. Windows 10+ for UWP.
* Visual Studio 2015. Compatible with the free community edition. Needed for development only.
* Newtonsoft JSON available from Nuget.
* NAudio available from Nuget (only used for Console and WPF).

Development Notes:
* WoundifyUWP (UWP version only) needs a few app capabilities to be granted. Go to Package.appxmanifest->Capabilities and enable the following: Internet (Client), Location, Microphone
* Microsoft.Speech doesn't offer a grammar for dictation input, that's why System.Speech is used.
* When compiling source code, it is safe to ignore the Visual Studio warning CS1998 "This async method lacks `await` operators and will run synchronously. Consider using the `await` operator to await non-blocking API calls, or `await Task.Run(...)` TODO CPU-bound work on a background thread."

Recommended Settings File Customization:
* It's best to create a WoundifySettings.json file to contain your customizations. The file will override any settings obtained from the WoundifyDefaultSettings.json file. Use the SETTINGS command to verify that your settings are correct.
* Geolocation is only implemented in UWP, and not implmented in Console and WPF app. To create a default geolocation, enter your location into the WoundifySettings.json file.
* Recommended to add authorizations for Google into the IdentifyLanguage Google section of the settings file. Google provides the fastest and arguably the best STT. See http://stackoverflow.com/questions/23608863/google-speech-recognition-api
* Sign up at http://www.projectoxford.ai to get a subscription key. Use the subscription key, called Primary Key, as the ClientSecret in IdentifyLanguage Bing section of the settings file.

Program Usage:
Woundify first tries to load WoundifyDefaultSettings.json file, followed by WoundifySettings.json. Settings files can be either in the same directory as the executable or the local directory. The first settings file loaded will initialize setting values. Subbsequent loading of settings files will override existing values. 

## Limitations
* Microsoft's System.Speech APIs, their legacy STT APIs, do a pretty bad job of speech-to-text (STT), thus Woundify tries to use Google's STT instead. Microsoft seems to be on the verge of updating their STT engine so something better should be available in 2016. It may be in the form of local machine software or it may only work over the Internet.
* Services all work with audio files in wave format (.wav), and mono (one channel), bit depth of 16, and sample rate of 16000.

## Troubleshooting:
* Make sure Houndify ClientKey and ClientSecret are entered into a customized WoundifySettings.json file (best) or just modify the WoundifyDefaultSettings.json file.

## Description of commands
| Command           | Description |
| ----------------- | ---------------------------------------------------------------------------------------------------------- |
| ANNOTATE | Pops stack and sends audio/text to annotate service. The response is pushed onto the stack.
| END | End program. Same as QUIT.
| ENTITIES | Pops stack and sends audio/text to entities service. The response is pushed onto the stack.
| HELP | Show help.
| IDENTIFY | Pops stack and sends audio/text to language identification service. The response is pushed onto the stack.
| INTENT | Pops stack and sends audio/text to intent service. The response is pushed onto the stack.
| JSONPATH | Apply a JsonPath to the previous response.
| LISTEN | Record audio and push utterance onto stack.
| LOOP | Loop back to first command and continue execution.
| PAUSE | Pause for seconds specified in argument (or uses default).
| PARSE | Pops stack and sends audio/text to parse service. The response is pushed onto the stack.
| PERSONALITY | Pops stack and sends audio/text to personality characteristics service. The response is pushed onto the stack.
| PRONOUNCE | Convert text at top of stack into spelled pronounciations.
| QUIT | Quit program. Same as END.
| REPLAY | Replay TOS. If TOS is text, display text. If audio, it plays the audio.
| RESPONSE | Push the last intent's response, always text, onto the stack.
| SETTINGS | Show or update settings. Use this command to verify settings such as defaults or authentication info.
| SHOW | Shows everything on the stack.
| SPEAK | Pops stack and speaks it.
| SPEECH | Push audio onto stack.
| TEXT | Push text onto stack.
| TONE | Pops stack and sends audio/text to tone of voice (happy, angry) service. The response is pushed onto the stack.
| TRANSLATE | Pops stack and sends audio/text to language translation service. The response is pushed onto the stack.
| WAKEUP | Wait for wakeup word(s), convert rest of audio to text, push as text onto stack.

## Chart of Command Actions Based on Arguments

| Command           | No args TOS is .txt | No args TOS is .wav | Text arg            | @File.txt arg       | @File.wav Argument  |
| ----------------- | ------------------- | ------------------- |  ------------------ | ------------------- | ------------------- |
| ANNOTATE          | Pop, send text      | Pop, send audio     | Send text           | Send text           | Send audio          |
| END               |                     |                     |                     |                     |                     |
| ENTITIES          | Pop, send text      | Pop, send audio     | Send text           | Send text           | Send audio          |
| HELP              |                     |                     |                     |                     |                     |
| IDENTIFY          | Pop, send text      | Pop, send audio     | Send text           | Send text           | Send audio          |
| INTENT            | Pop, send text      | Pop, send audio     | Send text           | Send text           | Send audio          |
| JSONPATH          |                     |                     | Apply JsonPath push |                     |                     |
| LISTEN            |                     |                     | TTS, push audio     | TTS, push audio     | Push audio          |
| LOOP              |                     |                     |                     |                     |                     |
| PARSE             | Pop, send text      | Pop, STT, send Text | Send text           | Send text           | STT, send text      |
| PAUSE             |                     |                     | Seconds to pause    |                     |                     |
| PERSONALITY       | Pop, send text      | Pop, send audio     | Send text           | Send text           | Send audio          |
| PRONOUNCE         | Pop, push pronounce | Pop, STT, push pronounce | Push pronounce | Push pronounce      | STT, push pronounce |
| QUIT              |                     |                     |                     |                     |                     |
| REPLAY            | Display TOS text    | Play TOS audio      |                     |                     |                     |
| RESPONSE          | Push text response  |                     |                     |                     |                     |
| SETTINGS          |                     |                     | JSON override       | JSON override       |                     |
| SHOW              | Display TOS text    | Play TOS audio      |                     |                     |                     |
| SPEAK             | Pop, TTS, play audio | Pop, play audio    | TTS, play audio     | TTS, play audio     | Play audio          |
| SPEECH            | Pop, TTS, push audio | no change          | TTS, push audio     | TTS, push audio     | Push audio          |
| TEXT              | Push text           | Pop, STT, push text | Push text           | STT, push text      | STT, push text      |
| TONE              | Pop, send text      | Pop, send audio     | Send text           | Send text           | Send audio          |
| TRANSLATE         | Pop, send text      | Pop, send audio     | Send text           | Send text           | Send audio          |
| WAKEUP            |                     |                     | Use as wakeup words | Use as wakeup words |                     |

Table Notes:
* Blank cells means no change
* TOS means Top Of Stack
* TTS means converts Text-To-Speech
* STT means converts Speech-To-Text
* Listen pushes a wave file (audio) whereas WakeUp first converts to text before pushing. This is because WakeUp needs to convert to text to understand the wakeup word.

