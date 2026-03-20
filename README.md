# Eiffel
An experimental mod loader for Scott Pilgrim EX, based on [MonoMod](https://github.com/MonoMod/MonoMod)

# Project structure
Eiffel consists of two components:
- **Eiffel**: the mod loader/API itself. It handles mod content management and assembly loading/hooking
- **EiffelPatcher**: a Cecil-based patcher that injects a few Eiffel calls into the game's Main function, just enough for Eiffel to take over
- **ExampleMod**: a simple mod that hooks the main menu, and logs a message to it ("Hello from ExampleMod!")

# Mod structure
Every mod consists of a manifest file (`eiffel.json`), and an assembly (something like `Dummy.dll`), content folder (`Content\`), or both.
Here's an example manifest:
```json
{
	"Name": "ExampleMod",
	"ID": "ExampleMod",
	"Version": "1.0.0",
	"MinimumEiffelVersion": "1.0.0",
	"Assembly": "ExampleMod.dll",
	"Dependencies": []
}
```
By default, Eiffel will load content from `Content\`.

When developing mods, it's useful to include a .pdb file with your mod. These allow you to easily debug your mod (and you'll have to often, due to the fragile nature of hooking).

If your mod doesn't load, chances are it threw an exception. Check `eiffel.log` to see if any errors have been logged.

# Installation
No release builds at the moment, so you're gonna have to build it yourself.

Copy all files from `EiffelPatcher/bin/x64/(BUILD TARGET)/net10.0` and `Eiffel/bin/(BUILD TARGET)/` (*EXCLUDING* Game.exe, ParisEngine.dll, NBug.dll, NLog.dll and FNA.dll) to your game folder. Then, run EiffelPatcher.exe, wait for it to close and launch Game_patched.exe. This is your modded game executable.

# Building
Build using your favorite IDE. EiffelPatcher targets .NET 10, while Eiffel and ExampleMod target .NET Framework 4.7 to match the game, so make sure you have the SDKs or targeting packs for both.
