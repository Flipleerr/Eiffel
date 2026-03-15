using System;
using System.Collections.Generic;
using System.IO;
using Paris.Engine.AssetPacks;

namespace Eiffel.Mod.Content
{
    public class ModContentManager
    {
        public string ModContentDirectory { get; protected set; }
        public bool IsLoaded { get; protected set; }

        private AssetPack Pack;
        public readonly Dictionary<string, ModAsset> Assets = new Dictionary<string, ModAsset>(StringComparer.InvariantCultureIgnoreCase);

        public ModContentManager(string directory)
        {
            ModContentDirectory = directory;
        }

        public void EnumerateAssets()
        {
            if (!Directory.Exists(ModContentDirectory)) return;

            Assets.Clear();
            var root = new DirectoryInfo(ModContentDirectory);

            foreach (var file in root.GetFiles("*", SearchOption.AllDirectories))
            {
                string relative = GetRelativePath(ModContentDirectory, file.FullName);
                string path = Path.Combine(
                        Path.GetDirectoryName(relative) ?? "",
                        Path.GetFileNameWithoutExtension(relative)
                    ).Replace('/', '\\').TrimStart('\\');

                Assets[path] = new ModAsset(path, file.FullName);
            }
        }

        public void RegisterAssets(string modName)
        {
            if (Assets.Count == 0 || ModContentDirectory == null)
                return;

            if (Pack != null)
                // AssetPackManager.Singleton.DisablePack(Pack);

            // var items = new List<AssetPackItem>();
            foreach (var asset in Assets.Values)
            {
                
                string replacement = Path.Combine(
                    Path.GetDirectoryName(asset.AbsolutePath),
                    Path.GetFileNameWithoutExtension(asset.AbsolutePath)
                );

                Logger.Verbose($"Current asset path is {asset.Path}, replacement path is {replacement}");

                // items.Add(new AssetPackItem
                // {
                //     OriginalPath = asset.Path,
                //     ReplacementPath = replacement
                // });
            }

            Pack = new AssetPack();
            Pack.SourcePath = ModContentDirectory;
            //  Pack.Items = items;
            Pack.Init();

            // AssetPackManager.Singleton.PacksLoadedByPath[modName] = Pack;
            // AssetPackManager.Singleton.EnablePack(Pack, AssetPackEnableFlags.Default);

            IsLoaded = true;
        }

        public void UnloadAssets(string modName)
        {
            if (Pack != null)
            {
                AssetPackManager.Singleton.DisablePack(Pack);
                Pack = null;
                IsLoaded = false;
            }
        }

        private string GetRelativePath(string relative, string path)
        {
            Uri fromUri = new Uri(relative.EndsWith(Path.DirectorySeparatorChar.ToString())
            ? relative
            : relative + Path.DirectorySeparatorChar);
            Uri toUri = new Uri(path);
            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', '\\');
        }
    }
 
    public class ModAsset
    {
        public string Path { get; protected set; }
        public string AbsolutePath { get; protected set; }
        public Type Type = null;
        public string Format = null;

        public ModAsset(string relative, string absolute)
        {
            Path = relative;
            AbsolutePath = absolute;
        }
    }
}
