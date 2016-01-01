# Woundify
Woundify is a Windows client for Houndify. Woundify is written in C# and is compatible with Console, WPF and UWP systems.

## Console Application
The woundify.exe is a console app for scripting audio, text and Houndify. There are commands for making combinations of audio recordings, text-to-speech (TTS), speech-to-text (STT), making Houndify requests, and working with Houndify responses.

> Usage: woundify.exe [command]*
> woundify.exe accepts zero or more commands. See the chart below commands and their syntax. Each command has zero or one argument. An argument may be text (e.g. "Hello World"), or @<filename> (e.g. @HelloWorld.txt or @HelloWorld.wav). Files must be either text (.txt) or wave audio recording (.wav). commands are usually chained together. The command processor is stack oriented. Some commands push results on to the stack while others consume the stack.

## Development
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
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |
| Content Cell      | Content Cell        | Content Cell        | Content Cell        | Content Cell        | Content Cell        |

Table Notes:
* Listen pushes a wave file (audio) whereas WakeUp first converts to text before pushing. This is because WakeUp needs to convert to text to understand the wakeup word.
* 
