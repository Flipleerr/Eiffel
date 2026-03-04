using System;
using System.Collections.Generic;
using System.IO;

namespace Eiffel.Mod.Content
{
    public class ModContent
    {
        public string ModDirectory { get; protected set; }
        public bool IsLoaded { get; protected set; }

        protected ModContent(string directory)
        {
            ModDirectory = directory;
        }

        // assumes path is relative, probably a bad idea. add checks later
        public Stream OpenAsset(string path)
            => File.OpenRead(Path.Combine(ModDirectory, path));

        public IEnumerable<string> EnumerateAssets()
            => Directory.EnumerateFiles(ModDirectory, "*", SearchOption.AllDirectories);
    }
}
