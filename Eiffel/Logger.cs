// introducing, the worst logging system of all time:
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;

namespace Eiffel
{
    public static class Logger
    {
        private const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        public static ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();

        public enum LogLevel
        {
            Normal,
            Error,
            Warn,
            Verbose
        }

        public static void Clear()
        {
            if (File.Exists("eiffel.log"))
                File.Delete("eiffel.log");
        }

        public static void Info(string contents)
            => Log(LogLevel.Normal, contents);

        public static void Warn(string contents) 
            => Log(LogLevel.Warn, contents);

        public static void Error(string contents)
            => Log(LogLevel.Error, contents);

        public static void Verbose(string contents) 
            => Log(LogLevel.Verbose, contents);

        public static void Log(LogLevel level, string contents)
        {
            switch (level)
            {
                case LogLevel.Normal:
                    File.AppendAllText("eiffel.log", $"{DateTime.Now} INFO: " + contents);
#if DEBUG
                    Console.WriteLine($"{DateTime.Now} INFO: " + contents + Environment.NewLine);
#endif
                    break;
                case LogLevel.Error:
                    File.AppendAllText("eiffel.log", $"{DateTime.Now} ERROR: " + contents + Environment.NewLine);
#if DEBUG
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{DateTime.Now} ERROR: " + contents + Environment.NewLine);
                    Console.ForegroundColor = ConsoleColor.White;
#endif
                    break;
                case LogLevel.Warn:
                    File.AppendAllText("eiffel.log", $"{DateTime.Now} WARNING: " + contents + Environment.NewLine);
#if DEBUG
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{DateTime.Now} WARNING: " + contents + Environment.NewLine);
                    Console.ForegroundColor = ConsoleColor.White;
#endif
                    break;
                case LogLevel.Verbose:
                    File.AppendAllText("eiffel.log", $"{DateTime.Now} VERBOSE: " + contents + Environment.NewLine);
#if DEBUG
                    Console.WriteLine($"{DateTime.Now} VERBOSE: " + contents + Environment.NewLine);
#endif
                    break;

                default:
                    File.AppendAllText("eiffel.log", $"{DateTime.Now} INFO: " + contents + Environment.NewLine);
#if DEBUG
                    Console.WriteLine($"{DateTime.Now} VERBOSE: " + contents + Environment.NewLine);
#endif
                    break;
            }
        }

        // debug only
        public static void SetupConsole()
        {
            if (!AttachConsole(ATTACH_PARENT_PROCESS))
            {
                AllocConsole();
            }

            var stdOut = GetStdHandle(-11);

            SetStdHandle(-11, stdOut);

            var standardOutput = new StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);

            Logger.Info("Debug console created!");
        }
    }
}
