using System;
using System.Runtime.CompilerServices; // for CallerFilePath, CallerMemberName, CallerLineNumber

// note: can use System.Diagnostics.Debugger.IsAttached to determine if debugging

namespace WoundifyShared
{
    // ToDo: output using MessageDialog
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
        public static System.IO.StreamWriter logFile;
        public static void Write(string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNum = 0)
        {
            string messageFormatted = string.Format("{0}.{1}({2}):{3}", System.IO.Path.GetFileNameWithoutExtension(sourceFilePath), memberName, sourceLineNum, message);
            Trace.Write(messageFormatted);
            if (logFile != null)
                logFile.Write(messageFormatted);
        }
        public static void WriteLine(string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNum = 0)
        {
            string messageFormatted = string.Format("{0}.{1}({2}):{3}", System.IO.Path.GetFileNameWithoutExtension(sourceFilePath), memberName, sourceLineNum, message);
            Trace.WriteLine(messageFormatted);
            if (logFile != null)
                logFile.WriteLine(messageFormatted);
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
