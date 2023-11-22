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

    [Given(@"this content as an options file")]
    public void GivenThisContentAsACommandLineOptionsFile(string yaml)
    {
        optionsFileContext.WriteOptionsFile(yaml);
    }
}