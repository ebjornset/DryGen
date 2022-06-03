using DryGen.Docs;
using DryGen.UTests.Helpers;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps
{
    [Binding]
    public sealed class DocsSteps
    {
        public DocsSteps(ConsoleContext consoleContext)
        {
            this.consoleContext = consoleContext;
        }

        private readonly ConsoleContext consoleContext;

        [When(@"I generate the docs verb menu")]
        public void WhenIGenerateTheDocsVerbMenu()
        {
            VerbMenuGenerator.GenerateVerbMenu(consoleContext.OutWriter);
        }

        [When(@"I generate the docs markdown for the verb ""([^""]*)""")]
        public void WhenIGenerateTheDocsMarkdownForTheVerb(string verb)
        {
            VerbMarkdowGenerator.GenerateVerbMarkdown(verb, consoleContext.OutWriter);
        }
    }
}