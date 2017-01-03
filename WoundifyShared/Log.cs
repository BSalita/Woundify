using System;
using System.Runtime.CompilerServices; // for CallerFilePath, CallerMemberName, CallerLineNumber

// note: can use System.Diagnostics.Debugger.IsAttached to determine if debugging

namespace WoundifyShared
{
    // TODO: output using MessageDialog
    //var messageDialog = new Windows.UI.Popups.MessageDialog(message);
    //await messageDialog.ShowAsync();
    class Console
    {
        public static void Write(string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNum = 0)
        {
#if WINDOWS_UWP
            Log.Write(string.Format("{0}", message));
#else
            if (!string.IsNullOrEmpty(System.Console.Title)) // detects if in Console mode
            {
                System.Console.Write(message);
                Log.Write(string.Format("{0}", message));
            }
#endif
        }
        public static void WriteLine(string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNum = 0)
        {
#if WINDOWS_UWP
            Log.WriteLine(string.Format("{0}", message));
#else
            if (!string.IsNullOrEmpty(System.Console.Title)) // detects if in Console mode
            {
                System.Console.WriteLine(message);
            }
#endif
        }
    }
    class Log
    {
#if WINDOWS_UWP
        public static Windows.Storage.Streams.DataWriter logFile;
        public static async System.Threading.Tasks.Task LogFileInitAsync()
        {
            Windows.Storage.StorageFolder tempFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Options.options.tempFolderPath);
            Windows.Storage.StorageFile sampleFile = await tempFolder.CreateFileAsync("log.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            Windows.Storage.Streams.IRandomAccessStream logStream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            logFile = new Windows.Storage.Streams.DataWriter(logStream.GetOutputStreamAt(0));
        }
#else
        public static System.IO.StreamWriter logFile;
        public static async System.Threading.Tasks.Task LogFileInitAsync()
        {
        }
#endif
        public static void Write(string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNum = 0)
        {
            string messageFormatted = string.Format("{0}.{1}({2}):{3}", System.IO.Path.GetFileNameWithoutExtension(sourceFilePath), memberName, sourceLineNum, message);
            Trace.Write(messageFormatted);
            if (logFile != null)
#if WINDOWS_UWP
                logFile.WriteString(messageFormatted);
#else
                logFile.Write(messageFormatted);
#endif
        }
        public static void WriteLine(string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNum = 0)
        {
            string messageFormatted = string.Format("{0}.{1}({2}):{3}", System.IO.Path.GetFileNameWithoutExtension(sourceFilePath), memberName, sourceLineNum, message);
            Trace.WriteLine(messageFormatted);
            if (logFile != null)
#if WINDOWS_UWP
                logFile.WriteString(messageFormatted + Environment.NewLine);
#else
                logFile.WriteLine(messageFormatted);
#endif
        }
    }
    class Trace
    {
        public static void Write(string messageFormatted)
        {
#if WINDOWS_UWP
            System.Diagnostics.Debug.Write(messageFormatted);
#else
            System.Diagnostics.Trace.Write(messageFormatted);
#endif
        }
        public static void WriteLine(string messageFormatted)
        {
#if WINDOWS_UWP
            System.Diagnostics.Debug.WriteLine(messageFormatted);
#else
            System.Diagnostics.Trace.WriteLine(messageFormatted);
#endif
        }
    }
}
