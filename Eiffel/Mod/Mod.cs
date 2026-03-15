using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Eiffel.Mod.Content;

namespace Eiffel.Mod
{
    public abstract class Mod
    {
        public Mod()
        {
            // PLEASE KEEP THIS EMPTY THANKS!!
        }

        public ModInfo Info { get; set; }
        public ModContentManager Content { get; set; }
        public AssemblyContent Assembly { get;  set; }

        protected List<Hook> Hooks = new List<Hook>();

        public abstract void OnLoad();

        public void OnUnload() 
        {
            foreach (var hook in Hooks)
            {
                hook.Dispose();
            }
            Hooks.Clear();
            Logger.Info($"{Info.Name} has been unloaded!\n");
        }

        public void CreateHook(MethodBase target, Delegate hookDelegate)
        {
            Hooks.Add(new Hook(target, hookDelegate));
        }

        public bool NeedsUpdate => false;
    }
}
