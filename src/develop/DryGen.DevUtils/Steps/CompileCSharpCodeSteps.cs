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
        assemblyContext.CompileCodeToFile(code);
    }
}