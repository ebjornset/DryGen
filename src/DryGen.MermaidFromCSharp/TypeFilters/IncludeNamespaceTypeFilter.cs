using System;
using System.Text.RegularExpressions;

namespace DryGen.MermaidFromCSharp.TypeFilters;

public class IncludeNamespaceTypeFilter : ITypeFilter
{
    private readonly Regex regex;

    public IncludeNamespaceTypeFilter(string regex)
    {
        this.regex = regex.ToSingleLineCompiledRegexWithTimeout();
    }

    public bool Accepts(Type type)
    {
        var match = regex.Match(type.Namespace);
        return match.Success;
    }
}
