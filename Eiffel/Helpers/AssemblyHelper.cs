using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Eiffel.Helpers
{
    public static class AssemblyHelper
    {
        public static Type GetModType(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Logger.Warn($"Failed to get mod assembly type! {e}\n");

                types = e.Types.Where(t => t != null).ToArray();
            }

            return types.FirstOrDefault(t =>
                typeof(Mod.Mod).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                t.IsClass);
        }

        public static Assembly LoadAssemblyFromFile(string path)
        {
            return Assembly.LoadFrom(path);
        }
    }
}
