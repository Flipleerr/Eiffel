using System;
using System.IO;
using Eiffel.Mod;
using Eiffel.Helpers;
using System.Collections.Generic;

namespace Eiffel
{
    public static class Loader
    {
        public static string ModPath { get; set; }
        static List<string> Mods { get; set; }

        public static void Initialize()
        {
            Logger.Clear("eiffel.log");
            Logger.Write("eiffel.log", "Initializing Eiffel!\n");

            
        }
    }
}
