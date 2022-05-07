using DryGen.MermaidFromCSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DryGen.MermaidFromCSharp.TypeFilters
{
    public class AllChildFiltersTypeFilter : ITypeFilter
    {
        private readonly IReadOnlyList<ITypeFilter> children;

        public AllChildFiltersTypeFilter(IReadOnlyList<ITypeFilter> children)
        {
            this.children = children;
        }

        public bool Accepts(Type type)
        {
            return !children.Any() || children.All(x => x.Accepts(type));
        }
    }
}
