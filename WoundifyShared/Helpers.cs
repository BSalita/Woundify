using System;

namespace WoundifyShared
{
    class Helpers
    {
        public static string[] ParseArguments(string commandLine, char[] separators, bool removeQuotes = false)
        {
            char[] parmChars = commandLine.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"')
                {
                    inQuote = !inQuote;
                    if (removeQuotes)
                        parmChars[index] = '\n';
                }
                if (!inQuote)
                    if (Array.IndexOf(separators, parmChars[index]) != -1)
                        parmChars[index] = '\n';
            }
            return (new string(parmChars)).Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }

#if WINDOWS_UWP
        public static async System.Threading.Tasks.Task<string> ReadTextFromFileAsync(string fileName)
        {
            try
            {
                Windows.Storage.StorageFolder tempFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Options.options.tempFolderPath);
                Windows.Storage.StorageFile file = await tempFolder.GetFileAsync(fileName);
                return await Windows.Storage.FileIO.ReadTextAsync(file);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
                return null;
            }
        }
        public static async System.Threading.Tasks.Task<byte[]> ReadBytesFromFileAsync(string fileName)
        {
            try
            {
                Windows.Storage.StorageFolder tempFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Options.options.tempFolderPath);
                Windows.Storage.StorageFile file = await tempFolder.GetFileAsync(fileName);
                using (Windows.Storage.Streams.IRandomAccessStream stream = await file.OpenReadAsync())
                {
                    using (Windows.Storage.Streams.DataReader reader = new Windows.Storage.Streams.DataReader(stream.GetInputStreamAt(0)))
                    {
                        await reader.LoadAsync((uint)stream.Size);
                        byte[] bytes = new byte[stream.Size];
                        reader.ReadBytes(bytes);
                        return bytes;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
                return null;
            }
        }
#else
        public static async System.Threading.Tasks.Task<byte[]> ReadBytesFromFileAsync(string fileName)
        {
            return System.IO.File.ReadAllBytes(Options.options.tempFolderPath + fileName);
        }
        public static async System.Threading.Tasks.Task<string> ReadTextFromFileAsync(string fileName)
        {
            return System.IO.File.ReadAllText(Options.options.tempFolderPath + fileName);
        }
#endif

#if WINDOWS_UWP
        public static async System.Threading.Tasks.Task WriteTextToFileAsync(string fileName, string text)
        {
            try
            {
                Windows.Storage.StorageFolder tempFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Options.options.tempFolderPath);
                Windows.Storage.StorageFile file = await tempFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(file, text);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
            }
        }
        public static async System.Threading.Tasks.Task WriteBytesToFileAsync(string fileName, byte[] bytes)
        {
            try
            {
                Windows.Storage.StorageFolder tempFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Options.options.tempFolderPath);
                Windows.Storage.StorageFile file = await tempFolder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteBytesAsync(file, bytes);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
            }
        }
#else
        public static async System.Threading.Tasks.Task WriteBytesToFileAsync(string fileName, byte[] bytes)
        {
            System.IO.File.WriteAllBytes(Options.options.tempFolderPath + fileName, bytes);
        }
        public static async System.Threading.Tasks.Task WriteTextToFileAsync(string fileName, string text)
        {
            System.IO.File.WriteAllText(Options.options.tempFolderPath + fileName, text);
        }

#endif
    }
}
