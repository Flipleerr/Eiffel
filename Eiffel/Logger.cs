using Microsoft.Xna.Framework;
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
            File.AppendAllText("eiffel.log", "INFO:" + contents);
        }

        public static void Warn(string contents)
        {
            File.AppendAllText("eiffel.log", "WARNING:" + contents);
        }

        public static void Error(string contents)
        {
            File.AppendAllText("eiffel.log", "ERROR:" + contents);
        }

        public static void Verbose(string contents)
        {
            File.AppendAllText("eiffel.log", "VERBOSE:" + contents);
        }
    }
}
