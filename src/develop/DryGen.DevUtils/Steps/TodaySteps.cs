using DryGen.DevUtils.Helpers;
using Reqnroll;

namespace DryGen.DevUtils.Steps;

[Binding]
public sealed class TodaySteps
{
	private readonly TodayContext todayContext;

	public TodaySteps(TodayContext todayContext)
	{
		this.todayContext = todayContext;
	}

	[Given("today is {string}")]
	public void GivenTodayIs(string todayString)
	{
		todayContext.SetToday(todayString);
	}
}