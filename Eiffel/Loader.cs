using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Eiffel.Mod;
using Eiffel.Mod.Content;
using Eiffel.Helpers;

namespace Eiffel
{
    public static class Loader
    {
        /// <summary>
        /// Path to the game folder.
        /// </summary>
        public static string GamePath { get; internal set; }
        public static string ModPath { get; internal set; }

        internal static Dictionary<string, Mod.Mod> ModList = new Dictionary<string, Mod.Mod>();

        public static string IgnoreListPath { get; set; }
        internal static HashSet<string> IgnoreList = new HashSet<string>();

        public static void Initialize()
        {
            Logger.Clear();
            Logger.Info("Initializing Eiffel!\n");

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
            {
                Logger.Verbose($"Loading {dir}!\n");
                LoadDirectory(Path.Combine(ModPath, dir));
            }
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
            Mod.Mod mod = new Mod.Mod();

            string manifestPath = Path.Combine(path, "eiffel.json");
            if (File.Exists(manifestPath))
            {
                string json = File.ReadAllText(manifestPath);
                mod.Info = JsonHelper.Deserialize<ModInfo>(json);
                if (mod.Info == null)
                {
                    Logger.Error($"ERROR: Failed to deserialize manifest at path {manifestPath}! Skipping.\n");
                    return;
                }
                if (mod.Info.Name == null || mod.Info.ID == null)
                {
                    Logger.Error($"ERROR: Mod name or ID at path {manifestPath} are empty! Skipping.\n");
                    return;
                }
                if (mod.Info.Version == null)
                {
                    Logger.Error($"ERROR: Mod version at path {manifestPath} is empty! Skipping.\n");
                }

                Logger.Info($"Successfully loaded {mod.Info.Name} version {mod.Info.Version}!\n");

                string assemblyPath = Path.Combine(path, mod.Info.Assembly);

                Logger.Verbose($"Loading assembly from {assemblyPath}!");

                mod.Assembly = new AssemblyContent(path, assemblyPath);

                mod.Assembly.LoadAssembly(assemblyPath);

                ModList.Add(mod.Info.ID, mod);
            }
        }
    }
}
