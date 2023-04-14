using DryGen.Core;
using DryGen.MermaidFromDotnetDepsJson.Filters;
using System.Collections.Generic;

namespace DryGen.MermaidFromCSharp.TypeFilters;

public class AnyChildFiltersAssemblyNameFilter : AnyChildFiltersFilter<string>, IAssemblyNameFilter
{
    public AnyChildFiltersAssemblyNameFilter(IReadOnlyList<IAssemblyNameFilter> children) : base(children) { }
}
