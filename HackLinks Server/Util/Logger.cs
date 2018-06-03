using System;
using System.IO;

namespace HackLinks_Server.Util
{
    public static class Logger
    {
        private static string logFile;
        private static StreamWriter file;

        public static string Archive { get; set; }

        /// <summary>
		/// Specifies the log levels that shouldn't be displayed
		/// on the command line.
		/// </summary>
		public static LogLevel Hide { get; set; }

        public static string LogFile
        {
            get => logFile;
            set
            {
                if (value != null)
                {
                    var directoryName = Path.GetDirectoryName(value);
                    if (directoryName != null && !Directory.Exists(directoryName))
                        Directory.CreateDirectory(directoryName);
                    if (File.Exists(value))
                    {
                        if (Archive != null)
                        {
                            if (!Directory.Exists(Archive))
                                Directory.CreateDirectory(Archive);
                            var str1 = Path.Combine(Archive, File.GetCreationTime(value).ToString("yyyy-MM-dd_hh-mm"));
                            var str2 = Path.Combine(str1, Path.GetFileName(value));
                            if (!Directory.Exists(str1))
                                Directory.CreateDirectory(str1);
                            if (File.Exists(str2))
                                File.Delete(str2);
                            File.Move(value, str2);
                        }

                        File.Delete(value);
                    }
                }

                logFile = value;
            }
        }

        public static void Info(string format, params object[] args)
        {
            WriteLine(LogLevel.Info, format, args);
        }

        public static void Warning(string format, params object[] args)
        {
            WriteLine(LogLevel.Warning, format, args);
        }

        public static void Error(string format, params object[] args)
        {
            WriteLine(LogLevel.Error, format, args);
        }

        public static void Debug(string format, params object[] args)
        {
            WriteLine(LogLevel.Debug, format, args);
        }

        public static void Debug(object obj)
        {
            WriteLine(LogLevel.Debug, obj.ToString());
        }

        public static void Status(string format, params object[] args)
        {
            WriteLine(LogLevel.Status, format, args);
        }

        public static void Exception(Exception ex, string description = null, params object[] args)
        {
            if (description != null)
                WriteLine(LogLevel.Error, description, args);
            WriteLine(LogLevel.Exception, ex.ToString());
        }

        private static void WriteLine(LogLevel level, string format, params object[] args)
        {
            Write(level, format + Environment.NewLine, args);
        }

        private static void Write(LogLevel level, string format, params object[] args)
        {
            lock (Console.Out)
            {
                if (!Hide.HasFlag(level))
                {
                    switch (level)
                    {
                        case LogLevel.Info: Console.ForegroundColor = ConsoleColor.White; break;
                        case LogLevel.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                        case LogLevel.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                        case LogLevel.Debug: Console.ForegroundColor = ConsoleColor.Cyan; break;
                        case LogLevel.Status: Console.ForegroundColor = ConsoleColor.Green; break;
                        case LogLevel.Exception: Console.ForegroundColor = ConsoleColor.DarkRed; break;
                    }

                    if (level != LogLevel.None)
                        Console.Write("[{0}]", level);

                    Console.ForegroundColor = ConsoleColor.Gray;

                    if (level != LogLevel.None)
                        Console.Write(" - ");

                    Console.Write(format, args);
                }

                if (logFile == null)
                    return;
                if (file == null)
                    file = new StreamWriter(logFile, true);
                file.Write(DateTime.Now + " ");
                if (level != LogLevel.None)
                    file.Write("[{0}] - ", level);
                file.Write(format, args);
                file.Flush();
            }
        }
    }
}
