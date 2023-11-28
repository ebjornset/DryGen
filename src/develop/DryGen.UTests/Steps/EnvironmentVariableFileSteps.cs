using DryGen.DevUtils.Helpers;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps
{
    [Binding]
    public class EnvironmentVariableFileSteps
    {
        private readonly EnvironmentVariableFileContext environmentVariableFileContext;

        public EnvironmentVariableFileSteps(EnvironmentVariableFileContext environmentVariableFileContext)
        {
            this.environmentVariableFileContext = environmentVariableFileContext;
        }

        [Given(@"this file is referenced as the environment variable ""([^""]*)""")]
        public void GivenThisFileIsReferencedAsTheEnvironmentVariable(string enviromentVariable, string content)
        {
            environmentVariableFileContext.WriteFileAsEnvironmentVariable(content, enviromentVariable);
        }
    }
}