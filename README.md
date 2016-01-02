# Woundify
Woundify is a Windows client for the Houndify intent service. Woundify is written in C# and is compatible with Console, WPF and UWP systems.

## Console Application
The woundify.exe is a console app tool for scripting audio, text and Houndify operations. There are commands for recording audio, converting text-to-speech (TTS), converting speech-to-text (STT), and invoking Houndify intent services.

> Usage: woundify.exe [command]*

woundify.exe accepts zero or more commands. See the chart below for an explanation of commands. Each command has zero or one argument. An argument may be text (e.g. "Hello World"), or @filename.ext (e.g. @HelloWorld.txt or @HelloWorld.wav). Files must be either text (.txt) or wave audio recording (.wav). Commands are usually chained together for greatest effect. The command processor is stack oriented; some commands push results on to the stack, others consume the stack. Executing woundify.exe without any argument will cause woundify to enter command mode.

Examples:

> woundify text "What's the weather in Paris?" intent speak<br>Sends the text to Houndify and speaks the response.
  
> woundify listen intent speak<br>Listens to the microphone (default timeout) and speaks the response.
  
> woundify text @WeatherInParis.txt intent show<br>Send the contents of file WeatherInParis.txt to Houndify and shows the stack.
  
> woundify speech @WeatherInParis.wav intent show<br>Sends the wave file to Houndify and shows the stack.
  
> woundify wakeup intent speak loop<br>Listens for wakeup word(s) (default "computer") and whatever follows, sends to Houndify, speaks response and loops back to wakeup. This is similar behavior to Houndify's mobile app or Amazon Echo.

## Installation
There is no binary executable file available. You can create one from this repos using Visual Studio 2015 Community Edition (free).

## Terminology
* TOS means Top Of Stack. Woundify command processing is stack oriented.
* TTS means converts Text-To-Speech
* STT means converts Speech-To-Text
* UWP refers to Microsoft's Universal Windows Platform. See https://en.wikipedia.org/wiki/Universal_Windows_Platform

## Development
Developers can use Woundify as a standalone tool, as a tool for integrating into projects, or make use of its class libraries to create a custom project. This repos contains the entire source code of Woundify.

The source code for woundify is in C#. The classes contain a wealth of information. In particular, most operations are coded twice for maximum Windows support; WINT32 vs WinRT, System.Speech vs Windows.Media, System.IO vs Windows.Storage, System.Net.HTTP vs Windows.Web, System.Security.Cryptography vs Windows.Security.Cryptography, Console and WPF vs UWP. The source code contains the following capabilities:
* Authenticating to Bing, Google, Houndify services.
* Invoking Houndify intent API.
* Invoking speech-to-text APIs from Bing (Project Oxford), Google, Houndify.
* Parsing JSON responses from intent and STT services.
* Recording audio from microphone and writing to a stream or file.
* Playing audio from stream or file.
* Transcoding audio (WinRT only but can be done with NAudio too).
* Building audio graphs. (WinRT only).
* Listening for wakup words.
* Using JSON for requests, responses, objects, settings.
* Geolocating (WinRT only).
* Async/Await programming style for Console, WPF and UWP systems.

Dependencies:
* Windows 7+ for Console and WPF. Windows 10+ for UWP.
* Visual Studio 2015. Compatible with the free community edition. Needed for development only.
* Newtonsoft JSON available from Nuget.
* NAudio available from Nuget (only used for Console and WPF).

Development Notes:
* WoundifyUWP (UWP version only) needs a few app capabilities to be granted. Go to Package.appxmanifest->Capabilities and enable the following: Internet (Client), Location, Microphone
* Microsoft.Speech doesn't offer a grammar for dictation input, that's why System.Speech is used.
* When compiling source code, it is safe to ignore the Visual Studio warning CS1998 "This async method lacks `await` operators and will run synchronously. Consider using the `await` operator to await non-blocking API calls, or `await Task.Run(...)` to do CPU-bound work on a background thread."

Recommended Settings File Customization:
* It's best to create a WoundifySettings.json file to contain your customizations. The file will override any settings obtained from the WoundifyDefaultSettings.json file. Use the SETTINGS command to verify that your settings are correct.
* Geolocation is only implemented in UWP, and not implmented in Console and WPF app. To create a default geolocation, enter your location into the WoundifySettings.json file.
* Recommended to add authorizations for Google into the SpeechToText Google section of the settings file. Google provides the fastest and arguably the best STT. See http://stackoverflow.com/questions/23608863/google-speech-recognition-api
* Sign up at http://www.projectoxford.ai to get a subscription key. Use the subscription key, called Primary Key, as the ClientSecret in SpeechToText Bing section of the settings file.

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
| END | End program. Same as QUIT.
| HELP | Show help.
| INTENT | Pops stack and sends audio/text to Houndify's intent service. The response is pushed onto the stack.
| LISTEN | Record audio and push utterance onto stack.
| LOOP | Loop back to first command and continue execution.
| PAUSE | Pause for seconds specified in argument (or uses default).
| PRONOUNCE | Convert text at top of stack into spelled pronounciations.
| QUIT | Quit program. Same as END.
| REPLAY | Replay TOS. If TOS is text, display text. If audio, it plays the audio.
| RESPONSE | Push the last intent's response, always text, onto the stack.
| SETTINGS | Show or update settings. Use this command to verify settings such as defaults or authentication info.
| SHOW | Shows everything on the stack.
| SPEAK | Pops stack and speaks it.
| SPEECH | Push audio onto stack.
| TEXT | Push text onto stack.
| WAKEUP | Wait for wakeup word(s), convert rest of audio to text, push as text onto stack.

## Chart of Command Actions Based on Arguments

| Command           | No args TOS is .txt | No args TOS is .wav | Text arg            | @File.txt arg       | @File.wav Argument  |
| ----------------- | ------------------- | ------------------- |  ------------------ | ------------------- | ------------------- |
| END               |                     |                     |                     |                     |                     |
| HELP              |                     |                     |                     |                     |                     |
| INTENT            | Pop, send text      | Pop, send audio     | Send text           | Send text           | Send audio          |
| LISTEN            |                     |                     | TTS, push audio     | TTS, push audio     | Push audio          |
| LOOP              |                     |                     |                     |                     |                     |
| PAUSE             |                     |                     | Seconds to pause    |                     |                     |
| PRONOUNCE         | Pop, push pronounce | Pop, STT, push pronounce | Push pronounce | Push pronounce      | STT, push pronounce |
| QUIT              |                     |                     |                     |                     |                     |
| REPLAY            | Display TOS text    | Play TOS audio      |                     |                     |                     |
| RESPONSE          | Push text response  |                     |                     |                     |                     |
| SETTINGS          |                     |                     | JSON override       | JSON override       |                     |
| SHOW              | Display TOS text    | Play TOS audio      |                     |                     |                     |
| SPEAK             | Pop, TTS, play audio | Pop, play audio    | TTS, play audio     | TTS, play audio     | Play audio          |
| SPEECH            | Pop, TTS, push audio | no change          | TTS, push audio     | TTS, push audio     | Push audio          |
| TEXT              | Push text           | Pop, STT, push text | Push text           | STT, push text      | STT, push text      |
| WAKEUP            |                     |                     | Use as wakeup words | Use as wakeup words |                     |

Table Notes:
* Blank cells means no change
* TOS means Top Of Stack
* TTS means converts Text-To-Speech
* STT means converts Speech-To-Text
* Listen pushes a wave file (audio) whereas WakeUp first converts to text before pushing. This is because WakeUp needs to convert to text to understand the wakeup word.

