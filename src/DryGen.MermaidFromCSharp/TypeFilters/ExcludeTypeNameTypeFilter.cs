using DryGen.Core;
using System;

namespace DryGen.MermaidFromCSharp.TypeFilters;

public class ExcludeTypeNameTypeFilter : AbstractStringRegexFilter, ITypeFilter
{
    public ExcludeTypeNameTypeFilter(string regex) : base(regex, shouldMatch: false) { }

    public bool Accepts(Type type)
    {
        return DoesAccept(type.Name);
    }
}
