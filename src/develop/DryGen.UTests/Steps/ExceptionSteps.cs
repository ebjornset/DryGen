using DryGen.UTests.Helpers;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps
{
    [Binding]
    public sealed class ExceptionSteps
    {
        private readonly ExceptionContext exceptionContext;

        public ExceptionSteps(ExceptionContext exceptionContext)
        {
            this.exceptionContext = exceptionContext;
        }

        [Then(@"I should get an exception containing the text ""([^""]*)""")]
        public void ThenIShouldGetAnExceptionContainingTheText(string text)
        {
            exceptionContext.Exception.Should().NotBeNull();
            exceptionContext.Exception?.Message.Should().Contain(text);
        }

        [Then(@"I should not get an exception")]
        public void ThenIShouldNotGetAnException()
        {
            exceptionContext.Exception.Should().BeNull();
        }
    }
}