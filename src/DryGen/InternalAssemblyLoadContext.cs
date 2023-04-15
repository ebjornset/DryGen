using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace DryGen;

internal class InternalAssemblyLoadContext: AssemblyLoadContext
{
    private readonly string inputFile;
    private readonly string inputDirectory;

    internal InternalAssemblyLoadContext(string inputFile)
    {
        this.inputFile = inputFile;
        inputDirectory = Path.GetDirectoryName(inputFile) ?? throw new OptionsException($"Could not determine directory from inputFile '{inputFile}'");
    }

    internal Assembly Load()
    {
        EnterContextualReflection();
        return LoadFromFile(inputFile);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        foreach (var extension in new[] { ".dll", ".exe" })
        {
            var assemblyFileName = $"{inputDirectory}{Path.DirectorySeparatorChar}{assemblyName.Name}{extension}";
            if (File.Exists(assemblyFileName))
            {
                return LoadFromFile(assemblyFileName);
            }
        }
        // We cant find the assembly file, let the runtime try to handle it
        return null;
    }

    private Assembly LoadFromFile(string assemblyFileName)
    {
        // It seems like LoadFromAssemblyPath lockes the file, and that makes our teste fail, so we load read the fine manually to a stream
        var assemblyBytes = File.ReadAllBytes(assemblyFileName);
        return LoadFromStream(new MemoryStream(assemblyBytes));
    }
}