using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eiffel.Mod;
using Eiffel.Mod.Content;
using Eiffel.Helpers;
using NBug;

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
            // nuke NBug, it complains about Steamworks and crashes the entire game
            AppDomain.CurrentDomain.UnhandledException -= NBug.Handler.UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Logger.Error($"Unhandled exception: {e.ExceptionObject}\n");
            };

            AppDomain.CurrentDomain.AssemblyResolve += EiffelDependencyResolver;

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
            string manifestPath = Path.Combine(path, "eiffel.json");
            ModInfo tempInfo = new ModInfo();

            if (File.Exists(manifestPath))
            {
                string json = File.ReadAllText(manifestPath);
                tempInfo = JsonHelper.Deserialize<ModInfo>(json);
                if (tempInfo == null)
                {
                    Logger.Error($"Failed to deserialize manifest at path {manifestPath}! Skipping.\n");
                    return;
                }
                if (tempInfo.Name == null || tempInfo.ID == null)
                {
                    Logger.Error($"Mod name or ID at path {manifestPath} are empty! Skipping.\n");
                    return;
                }
                if (tempInfo.Version == null)
                {
                    Logger.Error($"Mod version at path {manifestPath} is empty! Skipping.\n");
                }

                Logger.Info($"Loading {tempInfo.Name} version {tempInfo.Version}!\n");
            }
            else
            {
                Logger.Error($"No manifest at {manifestPath} exists! Skipping.\n");
                return;
            }

            var assemblyPath = Path.Combine(path, tempInfo.Assembly);
            Assembly tempAssembly;
            Type modType;
            if (File.Exists(assemblyPath))
            {
                try
                {
                    Logger.Verbose($"Loading assembly at {assemblyPath}!\n");
                    tempAssembly = AssemblyHelper.LoadAssemblyFromFile(assemblyPath);
                    Logger.Verbose($"Getting type name for assembly {tempAssembly.GetName()}!\n");
                    modType = AssemblyHelper.GetModType(tempAssembly);
                }
                catch (ReflectionTypeLoadException e)
                {
                    foreach (var ex in e.LoaderExceptions)
                        Logger.Error($"Loader exception: {ex?.Message}\n");
                    return;
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to load assembly! {e}\n");
                    return;
                }
            }
            else
            {
                Logger.Error($"Mod assembly at path {assemblyPath} is empty! Skipping.\n");
                return;
            }

            if (modType == null)
            {
                Logger.Error($"No valid Mod class found in {assemblyPath}!\n");
                return;
            }

            try
            {
                var mod = (Mod.Mod)Activator.CreateInstance(modType);
                mod.Info = tempInfo;
                mod.Assembly = new AssemblyContent(path, tempAssembly);
                mod.OnLoad();
                ModList.Add(mod.Info.ID, mod);
            }
            catch (MissingMethodException e)
            {
                Logger.Error($"Missing method! {e}\n");
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to create a mod instance for {tempInfo.Name}!\n");
            }
        }

        private static Assembly EiffelDependencyResolver(object sender, ResolveEventArgs args)
        {
            string assemblyName = new AssemblyName(args.Name).Name;

            var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);
            if (loadedAssembly != null) return loadedAssembly;

            string requestedPath = args.RequestingAssembly != null
                ? Path.GetDirectoryName(args.RequestingAssembly.Location)
                : null;

            List<string> searchPaths = new List<string>
            {
                AppDomain.CurrentDomain.BaseDirectory,
                ModPath,
                requestedPath
            };

            foreach (string basePath in searchPaths)
            {
                if (string.IsNullOrEmpty(basePath)) continue;
                string dllPath = Path.Combine(basePath, $"{assemblyName}.dll");

                if (File.Exists(dllPath))
                {
                    return AssemblyHelper.LoadAssemblyFromFile(dllPath);
                }
            }

            return null;
        }
    }
}
