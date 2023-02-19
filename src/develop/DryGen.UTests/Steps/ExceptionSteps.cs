using DryGen.DevUtils.Helpers;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Steps;

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
        exceptionContext.ExpectExceptionContainingTheText(text);
    }

    [Then(@"I should not get an exception")]
    [Then(@"I should get no exceptions")]
    public void ThenIShouldNotGetAnException()
    {
        exceptionContext.ExpectNoException();
    }
}