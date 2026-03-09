// introducing, the worst logging system of all time:
using System;
using System.IO;

namespace Eiffel
{
    public static class Logger
    {
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
        {
            File.AppendAllText("eiffel.log", $"{DateTime.Now} INFO: " + contents);
        }

        public static void Warn(string contents)
        {
            File.AppendAllText("eiffel.log", $"{DateTime.Now} WARNING: " + contents);
        }

        public static void Error(string contents)
        {
            File.AppendAllText("eiffel.log", $"{DateTime.Now} ERROR: " + contents);  
        }

        public static void Verbose(string contents)
        {
            File.AppendAllText("eiffel.log", $"{DateTime.Now} VERBOSE: " + contents);
        }
    }
}
