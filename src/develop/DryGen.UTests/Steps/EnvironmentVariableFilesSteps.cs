using DryGen.DevUtils.Helpers;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps
{
    [Binding]
    public class EnvironmentVariableFilesSteps
    {
        private readonly EnvironmentVariableFilesContext environmentVariableFilesContext;

        public EnvironmentVariableFilesSteps(EnvironmentVariableFilesContext environmentVariableFilesContext)
        {
            this.environmentVariableFilesContext = environmentVariableFilesContext;
        }

        [Given(@"this file is referenced as the environment variable ""([^""]*)""")]
        public void GivenThisFileIsReferencedAsTheEnvironmentVariable(string enviromentvariable, string content)
        {
            environmentVariableFilesContext.WriteFileAsEnvironmentVariable(content, enviromentvariable);
        }
    }
}
