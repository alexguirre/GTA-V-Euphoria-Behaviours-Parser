namespace GTA_V_Euphoria_Behaviours_Parser
{
    // System
    using System;
    using System.IO;

    internal static class Logger
    {
        public delegate void LogEventHandler(LogLevel level, string enteredText, string formattedText);

        private static string filename;
        public static string FileName
        {
            get
            {
                return filename;
            }
            set
            {
                if (value == filename)
                    return;

                if(value == null)
                {
                    filename = null;
                    return;
                }

#if DEBUG
                if (File.Exists(value))
                    File.Delete(value);
                else if (!Directory.GetParent(value).Exists)
                    Directory.GetParent(value).Create();
#endif

                filename = value;
            }
        }


        public static bool CanLogToFile
        {
            get
            {
#if !DEBUG
                return false;
#endif
                return FileName != null;
            }
        }

        public static event LogEventHandler Logged;

        public static void Info(string text)
        {
            Log(LogLevel.INFO, text);
        }

        public static void Error(Exception exception)
        {
            Log(LogLevel.ERROR, exception.ToString());
        }

        public static void Debug(string text)
        {
#if DEBUG
            Log(LogLevel.DEBUG, text);
#endif
        }

        public static void Log(LogLevel level, string text)
        {
            string formattedText = $"[{DateTime.Now}] <{level}> {text}";

            if (CanLogToFile)
                File.AppendAllText(FileName, formattedText + "\n");

            Logged?.Invoke(level, text, formattedText);
        }
    }

    internal enum LogLevel
    {
        INFO,
        ERROR,
        DEBUG,
    }
}
