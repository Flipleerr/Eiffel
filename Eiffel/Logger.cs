using System;
using System.IO;

namespace Eiffel
{
    public static class Logger
    {
        public static void Clear(string path)
        {
            File.Delete("eiffel.log");
        }
        
        public static void Write(string path, string contents)
        {
            File.AppendAllText(path, contents);
        }
    }
}
