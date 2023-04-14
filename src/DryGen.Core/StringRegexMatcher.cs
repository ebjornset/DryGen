using System.Text.RegularExpressions;

namespace DryGen.Core;

public class StringRegexMatcher
{
    private readonly Regex regex;

    public StringRegexMatcher(string regex)
    {
        this.regex = regex.ToSingleLineCompiledRegexWithTimeout();
    }

    public bool DoesMatch(string value)
    {
        var match = regex.Match(value);
        return match.Success;
    }
}
