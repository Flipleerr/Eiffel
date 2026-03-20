using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

internal class Program
{
    static readonly List<string> assemblyPaths = new List<string>()
    {
        "Game.exe",
        "ParisEngine.dll",
    };

    private static void Main(string[] args)
    {
        Console.WriteLine("Backing up original assemblies...");

        foreach (string path in assemblyPaths)
        {
            if (File.Exists(Path.Combine("Backup", path)))
            {
                Console.WriteLine($"{path} is already backed up!");
                continue;
            }
            else
            {
                Directory.CreateDirectory("Backup");
                File.Copy(path, Path.Combine("Backup", path));
                Console.WriteLine($"{path} backed up!");
            }
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(".");

        var readerParams = new ReaderParameters() { AssemblyResolver = resolver };
        var gameAsm = AssemblyDefinition.ReadAssembly(assemblyPaths[0], readerParams);
        var engineAsm = AssemblyDefinition.ReadAssembly(assemblyPaths[1], readerParams);

        var module = gameAsm.MainModule;
        var programType = module.Types.First(t => t.Name == "Program");
        var mainMethod = module.EntryPoint;

        var mainIlProcessor = mainMethod.Body.GetILProcessor();
        
        var firstInstruction = mainMethod.Body.Instructions.First();
        
        var loaderAssembly = AssemblyDefinition.ReadAssembly("Eiffel.dll");
        var loaderType = loaderAssembly.MainModule.Types.First(t => t.Name == "Loader");

        var initMethod = loaderType.Methods.First(m => m.Name == "Initialize");
        var loadMethod = loaderType.Methods.First(m => m.Name == "Load");
        
        var initRef = module.ImportReference(initMethod);
        var loadRef = module.ImportReference(loadMethod);
        
        var initCall = mainIlProcessor.Create(OpCodes.Call, initRef);
        var loadCall = mainIlProcessor.Create(OpCodes.Call, loadRef);
        
        var tempfirst = mainMethod.Body.Instructions.First();

        Console.WriteLine("Patching: " + mainMethod.FullName);

        if (tempfirst.OpCode == OpCodes.Call && 
            tempfirst.Operand is MethodReference mr && 
            mr.Name == "Initialize" && 
            mr.DeclaringType.FullName == "Eiffel.Loader")
        {
            Console.WriteLine("Already patched!");
            return;
        }
        else
        {
            mainIlProcessor.InsertBefore(firstInstruction, initCall);
            mainIlProcessor.InsertAfter(initCall, loadCall);
        }

        gameAsm.Write("Game_patched.exe");
        gameAsm.Dispose();

        var engineModule = engineAsm.MainModule;
        var contentManagerType = engineModule.Types.First(t => t.FullName == "Paris.Engine.ParisContentManager");
        var contentManagerMethod = contentManagerType.Methods.FirstOrDefault(m => m.Name == "OpenStream");
        var contentManagerIlProcessor = contentManagerMethod.Body.GetILProcessor();

        var contentReplacementMethod = loaderType.Methods.FirstOrDefault(m => m.Name == "TryGetModReplacement");
        var contentReplacementRef = engineModule.ImportReference(contentReplacementMethod);

        // i guess this works... not very clean though.
        contentManagerIlProcessor.InsertBefore(
            contentManagerMethod.Body.Instructions[0], 
            contentManagerIlProcessor.Create(OpCodes.Ldarg_1));
        contentManagerIlProcessor.InsertAfter(
            contentManagerMethod.Body.Instructions[0], 
            contentManagerIlProcessor.Create(OpCodes.Call, contentReplacementRef));
        contentManagerIlProcessor.InsertAfter(
            contentManagerMethod.Body.Instructions[1], 
            contentManagerIlProcessor.Create(OpCodes.Starg_S, contentManagerMethod.Parameters[0]));

        engineAsm.Write("ParisEngine_patched.dll");
        engineAsm.Dispose();

        resolver.Dispose();
        File.Replace("Game_patched.exe", "Game.exe", null);
        File.Replace("ParisEngine_patched.dll", "ParisEngine.dll", null);

        Console.WriteLine("Done!");
    }
}