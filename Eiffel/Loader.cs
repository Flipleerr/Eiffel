using System;
using System.IO;

namespace Eiffel
{
    public static class Loader
    {
        public static void Initialize()
        {
            File.AppendAllText("eiffel.log", "Loader.Initialize() called!");
        }
    }
}
