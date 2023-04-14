namespace DryGen.Core;

public abstract class AbstractStringRegexFilter
{
    private readonly StringRegexMatcher stringRegexMatcher;
    private readonly bool shouldMatch;

    protected AbstractStringRegexFilter(string regex, bool shouldMatch)
    {
        stringRegexMatcher = new StringRegexMatcher(regex);
        this.shouldMatch = shouldMatch;
    }

    protected bool DoesAccept(string value)
    {
        var result = stringRegexMatcher.DoesMatch(value);
        return shouldMatch ? result : !result;
    }
}
