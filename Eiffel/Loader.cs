using System;
using System.IO;
using Eiffel.Mod;
using Eiffel.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Eiffel
{
    public static class Loader
    {
        /// <summary>
        /// Path to the game folder.
        /// </summary>
        public static string GamePath { get; internal set; }
        public static string ModPath { get; internal set; }

        internal static Dictionary<string, Info> ModList = new Dictionary<string, Info>();

        public static string IgnoreListPath { get; set; }
        internal static HashSet<string> IgnoreList = new HashSet<string>();

        public static void Initialize()
        {
            Logger.Clear("eiffel.log");
            Logger.Write("eiffel.log", "Initializing Eiffel!\n");

            GamePath = Directory.GetCurrentDirectory();
        }

        public static void Load()
        {
            ModPath = Path.Combine(GamePath, "Mods");
            IgnoreListPath = Path.Combine(ModPath, "ignorelist.txt");

            // ...
            if (File.Exists(IgnoreListPath))
            {
                IgnoreList = new HashSet<string>(
                    File.ReadAllLines(IgnoreListPath)
                    .Select(l => (l.StartsWith("#") ? "" : l).Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l)
                    ));
            }
            else
            {
                using (StreamWriter writer = File.CreateText(IgnoreListPath))
                {
                    writer.WriteLine("# This is the ignore list. Any mod listed here will not be loaded.");
                    writer.WriteLine("# Any line with starting with # will be ignored.");
                    writer.WriteLine("# Example: SomeModFolder");
                }
            }

            string[] directories = Directory
                .GetDirectories(ModPath)
                .Select(Path.GetFileName)
                .Where(file => ShouldLoad(file))
                .ToArray();

            foreach (string dir in directories)
                LoadDirectory(Path.Combine(ModPath, dir));
        }

        static bool ShouldLoad(string path)
        {
            if (IgnoreList.Contains(path))
                return false;
            else
                return true;
        }

        static void LoadDirectory(string path)
        {
            string manifestPath = Path.Combine(path, "eiffel.json");
            if (File.Exists(manifestPath))
            {
                string json = File.ReadAllText(manifestPath);
                Info modInfo = JsonHelper.Deserialize<Info>(json);
                if (modInfo == null)
                {
                    Logger.Write("eiffel.log", $"ERROR: Failed to deserialize manifest at path {manifestPath}! Skipping.\n");
                    return;
                }
                if (modInfo.Name == null || modInfo.ID == null)
                {
                    Logger.Write("eiffel.log", $"ERROR: Mod name or ID at path {manifestPath} are empty! Skipping.\n");
                    return;
                }
                if (modInfo.Version == null)
                {
                    Logger.Write("eiffel.log", $"ERROR: Mod version at path {manifestPath} is empty! Skipping.\n");
                }

                Logger.Write("eiffel.log", $"Successfully loaded {modInfo.Name} (ID: {modInfo.ID})!\n");
                ModList.Add(modInfo.ID, modInfo);
            }
        }
    }
}
