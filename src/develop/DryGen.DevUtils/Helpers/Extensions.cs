using System;

namespace DryGen.DevUtils.Helpers
{
    public static class Extensions
    {
        public static string AsUniqueTestValue(this string value)
        {
            return $"{value}-{Guid.NewGuid()}";
        }
    }
}
