# Woundify
Woundify is a Windows client for Houndify. Woundify is written in C# and is compatible with Console, WPF and UWP systems.

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

## Development
Developers can use woundify as a standalone tool, as a tool for larger projects, or use its class libraries to create their own project. This repos contains the entire source code.

The source code for woundify is in C#. The classes contain a wealth of information. In particular, most operations are coded for both WIN32 and UWP.
* How to authenticate to Bing, Google, Houndify services.
* Invoke Houndify intent API.
* Invoke speech-to-text APIs from Bing, Google, Houndify.
* Parse JSON responses.
* Record audio from microphone to file.
* Play audio to speaker or file.
* Transcode audio (UWP only).
* Build audio graphs. (UWP only).
* Listen for wakup words.
* JSON handling.
* Geolocation (UWP only).

Dependencies:
* Windows 7+ for Console and WPF. Windows 10+ for UWP.
* Visual Studio 2015. Compatible with the free community edition.
* Newtonsoft JSON available from Nuget.
* NAudio available from Nuget (only used for Console and WPF).

Development Notes:
* UWP: Go to Package.appxmanifest->Capabilities and enable the following: Internet (Client), Location, Microphone
* Microsoft.Speech doesn't offer a grammar for dictation input, that's why System.Speech is used.
* It is safe to ignore Visual Studio warning CS1998 "This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread."

Usage Notes:
* Geolocation is available only in UWP, and not implmented in Console and WPF app. To create a default location, enter your location into the settings file.
* Recommended to add authorizations for Google into the settings file. Goolge providers the fastest and best STT.
* Sign up at http://www.projectoxford.ai to get a subscription key. Use the subscription key as ClientSecret in settings file.

Program Usage:
Woundify tries to first load WoundifyDefaultSettings.json file, followed by WoundifySettings.json. Settings files can be either in the same directory as the executable or the local directory. The first settings file loaded will initialize and subsequent loadings will override existing values. 

## Limitations
* Microsoft's System.Speech APIs do a pretty bad job of speech-to-text (STT), thus Woundify tries to use Google's STT instead.
* Services all work with audio files in wave format (.wav), and mono (one channel), bit depth of 16, and sample rate of 16000.

## Troubleshooting:
* Make sure Houndify ClientKey and ClientSecret are entered into WoundifySettings.json or WoundifyDefaultSettings.json.

## Chart of commands

| Command           | No args TOS is .txt | No args TOS is .wav | Text arg            | @File.txt arg       | @File.wav Argument  |
| ----------------- | ------------------- | ------------------- |  ------------------ | ------------------- | ------------------- |
| END               |                     |                     |                     |                     |                     |
| HELP              |                     |                     |                     |                     |                     |
| INTENT            |                     |                     |                     |                     |                     |
| LISTEN            |                     |                     |                     |                     |                     |
| LOOP              |                     |                     |                     |                     |                     |
| PAUSE             |                     |                     |                     |                     |                     |
| PRONOUNCE         |                     |                     |                     |                     |                     |
| QUIT              |                     |                     |                     |                     |                     |
| REPLAY            |                     |                     |                     |                     |                     |
| RESPONSE          |                     |                     |                     |                     |                     |
| SETTINGS          |                     |                     |                     |                     |                     |
| SHOW              |                     |                     |                     |                     |                     |
| SPEAK             |                     |                     |                     |                     |                     |
| SPEECH            |                     |                     |                     |                     |                     |
| TEXT              |                     |                     |                     |                     |                     |
| WAKEUP            |                     |                     |                     |                     |                     |

Table Notes:
* Listen pushes a wave file (audio) whereas WakeUp first converts to text before pushing. This is because WakeUp needs to convert to text to understand the wakeup word.

