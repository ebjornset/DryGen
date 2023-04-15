using DryGen.Core;
using System.Reflection;

namespace DryGen.MermaidFromCSharp.PropertyFilters;

public class ExcludePropertyNamePropertyFilter : AbstractStringRegexFilter, IPropertyFilter
{
    public ExcludePropertyNamePropertyFilter(string regex) : base(regex, shouldMatch: false) { }

    public bool Accepts(PropertyInfo property)
    {
        return DoesAccept(property.Name);
    }
}
