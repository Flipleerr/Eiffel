using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Eiffel.Mod.Content
{
    public class AssemblyContent : ModContent
    {
        public Assembly AssemblyData;
        public string AssemblyName => AssemblyData.GetName().Name;

        public AssemblyContent(string directory, string dllPath) : base(directory)
        {
            
        }

        public void LoadAssembly(string path)
        {
            try
            {
                var asmData = File.ReadAllBytes(path);
                AssemblyData = Assembly.Load(asmData);

                var modType = AssemblyData.GetTypes()
                    .FirstOrDefault(t => typeof(Mod).IsAssignableFrom(t) && !t.IsAbstract);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load assembly {path}! {e}");
                return;
            }
        }
    }
}
