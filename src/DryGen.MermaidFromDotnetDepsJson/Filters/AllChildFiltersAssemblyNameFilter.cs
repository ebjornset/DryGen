using DryGen.Core;
using DryGen.MermaidFromDotnetDepsJson.Filters;
using System.Collections.Generic;

namespace DryGen.MermaidFromCSharp.TypeFilters;

public class AllChildFiltersAssemblyNameFilter : AllChildFiltersFilter<string>, IAssemblyNameFilter
{
    public AllChildFiltersAssemblyNameFilter(IReadOnlyList<IAssemblyNameFilter> children) : base(children) { }
}
