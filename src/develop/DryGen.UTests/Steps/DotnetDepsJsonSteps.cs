using DryGen.DevUtils.Helpers;
using Reqnroll;

namespace DryGen.UTests.Steps;

[Binding]
public class DotnetDepsJsonSteps
{
    private readonly InputFileContext inputFileContext;

    public DotnetDepsJsonSteps(InputFileContext inputFileContext)
    {
        this.inputFileContext = inputFileContext;
    }

    [Given(@"this .Net depts json input file")]
    public void GivenThisDotnetDepsJsonInputFile(string multilineText)
    {
        inputFileContext.CreateInputFile(multilineText, "deps.json");
    }
}