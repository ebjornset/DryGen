using FluentAssertions;
using System;

namespace DryGen.DevUtils.Helpers;

public class ExceptionContext
{
    public Exception? Exception { get; private set; }

    public TResult? HarvestExceptionFrom<TResult>(Func<TResult> func)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            Exception = ex;
        }
        return default;
    }

	public void HarvestExceptionFrom(Action action)
	{
		HarvestExceptionFrom(() =>
		{
			action();
			return true;
		});
	}

	public void ExpectExceptionContainingTheText(string text)
    {
        Exception.Should().NotBeNull();
        Exception?.Message.Should().Contain(text);
    }

    public void ExpectNoException()
    {
        Exception.Should().BeNull();
    }
}
