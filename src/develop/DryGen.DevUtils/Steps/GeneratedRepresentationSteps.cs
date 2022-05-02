using DryGen.DevUtils.Helpers;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace DryGen.DevUtils.Steps
{
    [Binding]
    public sealed class GeneratedRepresentationSteps
    {
        private readonly GeneratedRepresentationContext generatedRepresentationContext;

        public GeneratedRepresentationSteps(GeneratedRepresentationContext generatedRepresentationContext)
        {
            this.generatedRepresentationContext = generatedRepresentationContext;
        }

        [Then(@"I should get this generated representation")]
        public void ThenIShouldGetThisGeneratedRepresentation(string expectedGeneratedRepresentation)
        {
            expectedGeneratedRepresentation = expectedGeneratedRepresentation.Replace("\r\n", "\n");
            var actualRepresentation = generatedRepresentationContext.GeneratedRepresentation?.Replace("\r\n", "\n");
            actualRepresentation.Should().Be(expectedGeneratedRepresentation);
        }

        [Then(@"I should get this generated representation file")]
        public void ThenIShouldGetThisGeneratedRepresentationFile(string expectedGeneratedRepresentation)
        {
            expectedGeneratedRepresentation = expectedGeneratedRepresentation.Replace("\r\n", "\n");
            var actualRepresentation = generatedRepresentationContext.GeneratedRepresentationFromFile.Replace("\r\n", "\n");
            actualRepresentation.Should().Be(expectedGeneratedRepresentation);
        }
    }
}