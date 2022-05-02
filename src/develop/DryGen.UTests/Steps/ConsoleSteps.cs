using DryGen.UTests.Helpers;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps
{
    [Binding]
    public sealed class ConsoleSteps
    {
        public ConsoleSteps(ConsoleContext consoleContext)
        {
            this.consoleContext = consoleContext;
        }

        private readonly ConsoleContext consoleContext;

        [Then(@"I should find the text ""([^""]*)"" in console error")]
        public void ThenIShouldFindTheTextInConsoleError(string text)
        {
            var consoleError = consoleContext.ErrorWriter.ToString();
            consoleError.Should().Contain(text);
        }

        [Then(@"I should find the text ""([^""]*)"" in console out")]
        public void ThenIShouldFindTheTextInConsoleOut(string text)
        {
            var consoleOut = consoleContext.OutWriter.ToString();
            consoleOut.Should().Contain(text);
        }

        [Then(@"console out should contain the text")]
        public void ThenConsoleOutShouldContainTheText(string multilineText)
        {
            var outputs = consoleContext.OutWriter.ToString()?.Replace("\r\n", "\n");
            multilineText = multilineText.Replace("\r\n", "\n");
            outputs.Should().Be(multilineText);
        }
    }
}