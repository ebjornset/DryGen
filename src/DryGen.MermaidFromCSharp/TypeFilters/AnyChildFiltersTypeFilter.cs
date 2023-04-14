using DryGen.Core;
using System;
using System.Collections.Generic;

namespace DryGen.MermaidFromCSharp.TypeFilters;

public class AnyChildFiltersTypeFilter : AnyChildFiltersFilter<Type>, ITypeFilter
{
    public AnyChildFiltersTypeFilter(IReadOnlyList<ITypeFilter> children) : base(children) { }
}
