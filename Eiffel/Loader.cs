using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Paris.Engine;
using MonoMod.RuntimeDetour;
using Eiffel.Helpers;
using Eiffel.Mod;
using Eiffel.Mod.Content;

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

        private delegate Stream OpenStreamDelegate(ParisContentManager self, string assetName);

        public static string IgnoreListPath { get; set; }
        internal static HashSet<string> IgnoreList = new HashSet<string>();

        public static void Initialize()
        {
            // nuke NBug, it complains about Steamworks and crashes the entire game
            AppDomain.CurrentDomain.UnhandledException -= NBug.Handler.UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Logger.Error($"Unhandled exception: {e.ExceptionObject}");
            };

            AppDomain.CurrentDomain.AssemblyResolve += EiffelDependencyResolver;

            Logger.Clear();
            Logger.Info("Initializing Eiffel!");

            GamePath = Directory.GetCurrentDirectory();

            // the dirtiest hook of all time:
            var assetLoadHookTarget = typeof(ParisContentManager).GetMethod("OpenStream", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var assetLoadHookDelegate = new Func<OpenStreamDelegate, ParisContentManager, string, Stream>(OpenStreamHook);
            var assetLoadHook = new Hook(assetLoadHookTarget, assetLoadHookDelegate);
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
                Logger.Verbose($"Loading {dir}!");
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
                    Logger.Error($"Failed to deserialize manifest at path {manifestPath}! Skipping.");
                    return;
                }
                if (tempInfo.Name == null || tempInfo.ID == null)
                {
                    Logger.Error($"Mod name or ID at path {manifestPath} are empty! Skipping.");
                    return;
                }
                if (tempInfo.Version == null)
                {
                    Logger.Error($"Mod version at path {manifestPath} is empty! Skipping.");
                }

                Logger.Info($"Loading {tempInfo.Name} version {tempInfo.Version}!");
            }
            else
            {
                Logger.Error($"No manifest at {manifestPath} exists! Skipping.");
                return;
            }

            var assemblyPath = Path.Combine(path, tempInfo.Assembly);
            Assembly tempAssembly;
            Type modType;
            if (File.Exists(assemblyPath))
            {
                try
                {
                    Logger.Verbose($"Loading assembly at {assemblyPath}!");
                    tempAssembly = AssemblyHelper.LoadAssemblyFromFile(assemblyPath);
                    modType = AssemblyHelper.GetModType(tempAssembly);
                }
                catch (ReflectionTypeLoadException e)
                {
                    foreach (var ex in e.LoaderExceptions)
                        Logger.Error($"Loader exception: {ex?.Message}");
                    return;
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to load assembly! {e}");
                    return;
                }
            }
            else
            {
                Logger.Error($"Mod assembly at path {assemblyPath} does not exist! Skipping.");
                return;
            }

            if (modType == null)
            {
                Logger.Error($"No valid Mod class found in {assemblyPath}!");
                return;
            }

            try
            {
                var mod = (Mod.Mod)Activator.CreateInstance(modType);
                mod.Info = tempInfo;
                mod.Assembly = new AssemblyContent(path, tempAssembly);

                mod.OnLoad();

                mod.Content = new ModContentManager(Path.Combine(path, "Content"));
                mod.Content.EnumerateAssets();
                mod.Content.RegisterAssets(mod.Info.ID);

                ModList.Add(mod.Info.ID, mod);
            }
            catch (MissingMethodException e)
            {
                Logger.Error($"Missing method! {e}");
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to create a mod instance for {tempInfo.Name}! {e}");
            }
        }

        private static bool TryGetModReplacement(string assetName, out string replacement)
        {
            foreach (var mod in ModList.Values)
            {
                if (mod.Content.Assets.TryGetValue(assetName, out var asset))
                {
                    replacement = Path.Combine(
                        Path.GetDirectoryName(asset.AbsolutePath),
                        Path.GetFileNameWithoutExtension(asset.AbsolutePath)
                        );
                    return true;
                }
            }
            replacement = null;
            return false;
        }

        private static Stream OpenStreamHook(OpenStreamDelegate original, ParisContentManager self, string assetName)
        {
            if (Loader.TryGetModReplacement(assetName, out string replacement))
                assetName = replacement;
            return original(self, assetName);
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
