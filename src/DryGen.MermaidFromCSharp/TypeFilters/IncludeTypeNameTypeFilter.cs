using DryGen.Core;
using System;

namespace DryGen.MermaidFromCSharp.TypeFilters;


public class IncludeTypeNameTypeFilter : AbstractStringRegexFilter, ITypeFilter
{
    public IncludeTypeNameTypeFilter(string regex) : base(regex, shouldMatch: true) { }

    public bool Accepts(Type type)
    {
        return DoesAccept(type.Name);
    }
}
