using DryGen.Core;
using System;
using System.Collections.Generic;

namespace DryGen.MermaidFromCSharp.TypeFilters;

public class AllChildFiltersTypeFilter : AllChildFiltersFilter<Type>, ITypeFilter
{
    public AllChildFiltersTypeFilter(IReadOnlyList<ITypeFilter> children) : base(children)
    {
    }
}