using System;
using System.Text.RegularExpressions;

namespace DryGen.MermaidFromCSharp.TypeFilters;

public class ExcludeTypeNameTypeFilter : ITypeFilter
{
    private readonly Regex regex;

    public ExcludeTypeNameTypeFilter(string regex)
    {
        this.regex = regex.ToSingleLineCompiledRegexWithTimeout();
    }

    public bool Accepts(Type type)
    {
        var match = regex.Match(type.Name);
        return !match.Success;
    }
}
