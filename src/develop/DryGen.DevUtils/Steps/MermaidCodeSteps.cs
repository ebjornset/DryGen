using DryGen.DevUtils.Helpers;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace DryGen.DevUtils.Steps
{
    [Binding]
    public sealed class MermaidCodeSteps
    {
        private readonly MermaidCodeContext mermaidCodeContext;

        public MermaidCodeSteps(MermaidCodeContext mermaidCodeContext)
        {
            this.mermaidCodeContext = mermaidCodeContext;
        }

        [Then(@"I should get this Mermaid code")]
        public void ThenIShouldGetThisMermaidCode(string expectedMermaidCode)
        {
            mermaidCodeContext.MermaidCode.Should().Be(expectedMermaidCode);
        }

        [Then(@"I should get this Mermaid code file")]
        public void ThenIShouldGetThisMermaidCodeFile(string expectedMermaidCode)
        {
            mermaidCodeContext.MermaidCodeFromFile.Should().Be(expectedMermaidCode);
        }
    }
}