using Eiffel;
using Eiffel.Mod;
using Paris;
using Paris.Engine.AssetPacks;
using Paris.Game.Menu;
using System;
using System.Reflection;

namespace ExampleMod
{
    public class Example : Mod
    {
        public Example() : base()
        {

        }

        public override void OnLoad()
        {
            var target = typeof(MainMenu).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
            var hook = new Action<Action<MainMenu>, MainMenu>(MenuInitHook);
            CreateHook(target, hook);
        }

        private static void MenuInitHook(Action<MainMenu> originalMethod, MainMenu self)
        {
            Logger.Info("Hello from ExampleMod!");
            originalMethod(self);
        }
    }
}
