using DryGen.DevUtils.Helpers;
using TechTalk.SpecFlow;

namespace DryGen.DevUtils.Steps;

[Binding]
public sealed class CompileCSharpCodeSteps
{
    private readonly AssemblyContext assemblyContext;

    public CompileCSharpCodeSteps(AssemblyContext assemblyContext)
    {
        this.assemblyContext = assemblyContext;
    }

    [Given(@"this C\# source code")]
    public void GivenThisCSharpSourceCode(string code)
    {
        assemblyContext.CompileCodeToMemory(code);
    }

    [Given(@"this C\# source code compiled to a file")]
    public void GivenThisCSourceCodeCompiledToAFile(string code)
    {
        assemblyContext.CompileCodeToFileAsInputFile(code);
    }

    [Given(@"this C\# source code compiled to a file that is referenced as the environment variable ""([^""]*)""")]
    public void GivenThisCSourceCodeCompiledToAFileThatIsReferencedAsTheEnvironmentVariable(string enviromentVariable, string code)
    {
        assemblyContext.CompileCodeToFileAsEnvironmentVariable(code, enviromentVariable);
    }
}