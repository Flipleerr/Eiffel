using System;
using Microsoft.Xna.Framework;
using Eiffel.Mod.Content;

namespace Eiffel.Mod
{
    public class Mod
    {
        public ModInfo Info { get; set; }
        public ModContent Content { get; set; }
        public AssemblyContent Assembly { get;  set; }

        public void OnLoad() { }
        public void OnUnload() { }
        public void OnUpdate(GameTime time) { }

        public void Log(Logger.LogLevel level, string contents)
        {
            switch (level)
            {
                case Logger.LogLevel.Normal:
                    Logger.Info($"({Info.ID})" + contents);
                    break;
                case Logger.LogLevel.Warn:
                    Logger.Warn($"({Info.ID})" + contents);
                    break;
                case Logger.LogLevel.Error:
                    Logger.Error($"({Info.ID})" + contents);
                    break;
                case Logger.LogLevel.Verbose:
                    Logger.Verbose($"({Info.ID})" + contents);
                    break;
            }
        }
    }
}
