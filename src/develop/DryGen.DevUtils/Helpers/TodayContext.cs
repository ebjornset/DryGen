using System;
using System.Globalization;

namespace DryGen.DevUtils.Helpers;

public class TodayContext
{
	private DateTime? today;

	public DateTime Today => today ?? DateTime.Today;

	public void SetToday(string todayString)
	{
		if (today.HasValue)
		{
			throw new PropertyAlreadySetException(nameof(Today));
		}
		today = DateTime.ParseExact(todayString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
	}
}