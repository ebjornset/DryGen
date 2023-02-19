using System.Reflection;
using System.Text.RegularExpressions;

namespace DryGen.MermaidFromCSharp.PropertyFilters;

public class ExcludePropertyNamePropertyFilter : IPropertyFilter
{
    private readonly Regex regex;

    public ExcludePropertyNamePropertyFilter(string regex)
    {
        this.regex = regex.ToSingleLineCompiledRegexWithTimeout();
    }

    public bool Accepts(PropertyInfo property)
    {
        var match = regex.Match(property.Name);
        return !match.Success;
    }
}
