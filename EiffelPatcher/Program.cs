using Mono.Cecil;

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

        if (!Directory.Exists("Backup"))
        {
            Directory.CreateDirectory("Backup");
            foreach (string path in assemblyPaths)
            {
                File.Copy(path, Path.Combine("Backup", path));
                Console.WriteLine("Backed up: " + path);
            }
        }
        else
        {
            Console.WriteLine("Already backed up!");
        }

        // patching shits go here
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(".");

        var readerParams = new ReaderParameters() { AssemblyResolver = resolver };
        var asm = AssemblyDefinition.ReadAssembly(assemblyPaths[0], readerParams);

        var module = asm.MainModule;
        var programType = module.Types.First(t => t.Name == "Program");
        var mainMethod = module.EntryPoint;

        var il = mainMethod.Body.GetILProcessor();
        var firstInstruction = mainMethod.Body.Instructions[0];

        var loaderAssembly = AssemblyDefinition.ReadAssembly("Eiffel.dll");
        var loaderType = loaderAssembly.MainModule.Types.First(t => t.Name == "Loader");
        var initMethod = loaderType.Methods.First(m => m.Name == "Initialize");
        var loadMethod = loaderType.Methods.First(m => m.Name == "Load");

        var initRef = module.ImportReference(initMethod);
        var loadRef = module.ImportReference(loadMethod);
        var initCall = il.Create(Mono.Cecil.Cil.OpCodes.Call, initRef);
        var loadCall = il.Create(Mono.Cecil.Cil.OpCodes.Call, loadRef);

        Console.WriteLine("Patching: " + mainMethod.FullName);

        il.InsertBefore(firstInstruction, initCall);
        il.InsertAfter(initCall, loadCall);

        Console.WriteLine("Instructions after patching:");
        foreach (var instr in mainMethod.Body.Instructions.Take(8))
            Console.WriteLine($"  {instr}");

        asm.Write("Game_patched.exe");
        Console.WriteLine("Done!");
    }
}