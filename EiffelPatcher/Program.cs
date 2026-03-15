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
                break;
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
        var asm = AssemblyDefinition.ReadAssembly(assemblyPaths[0], readerParams);

        var module = asm.MainModule;
        var programType = module.Types.First(t => t.Name == "Program");
        var mainMethod = module.EntryPoint;

        var il = mainMethod.Body.GetILProcessor();
        
        var firstInstruction = mainMethod.Body.Instructions.First();
        
        var loaderAssembly = AssemblyDefinition.ReadAssembly("Eiffel.dll");
        var loaderType = loaderAssembly.MainModule.Types.First(t => t.Name == "Loader");

        var initMethod = loaderType.Methods.First(m => m.Name == "Initialize");
        var loadMethod = loaderType.Methods.First(m => m.Name == "Load");
        
        var initRef = module.ImportReference(initMethod);
        var loadRef = module.ImportReference(loadMethod);
        
        var initCall = il.Create(OpCodes.Call, initRef);
        var loadCall = il.Create(OpCodes.Call, loadRef);
        
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
            il.InsertBefore(firstInstruction, initCall);
            il.InsertAfter(initCall, loadCall);
        }

        asm.Write("Game_patched.exe");

        Console.WriteLine("Done!");
    }
}