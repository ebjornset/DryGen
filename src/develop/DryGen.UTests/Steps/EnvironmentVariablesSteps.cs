using DryGen.DevUtils.Helpers;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps
{
    [Binding]
    public sealed class EnvironmentVariablesSteps
    {
        private readonly EnvironmentVariableContext environmentVariableContext;

        public EnvironmentVariablesSteps(EnvironmentVariableContext environmentVariableContext)
        {
            this.environmentVariableContext = environmentVariableContext;
        }

        [Given(@"the environment variable ""([^""]*)"" has the value ""([^""]*)""")]
        public void GivenTheEnvironmentVariableHasTheValue(string environmentVariable, string? value)
        {
            environmentVariableContext.SetEnvironmentVariable(environmentVariable, value);
        }
    }
}