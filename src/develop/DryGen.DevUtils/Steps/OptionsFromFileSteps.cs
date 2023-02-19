using DryGen.DevUtils.Helpers;
using TechTalk.SpecFlow;

namespace DryGen.DevUtils.Steps;

[Binding]
public sealed class OptionsFromFileSteps
{
    private readonly OptionsFileContext optionsFileContext;

    public OptionsFromFileSteps(OptionsFileContext optionsFileContext)
    {
        this.optionsFileContext = optionsFileContext;
    }

    [Given(@"this input file as a command line option")]
    public void GivenThisInputFileAsACommandLineOption(string yaml)
    {
        optionsFileContext.WriteOptionsFile(yaml);
    }
}