﻿using DryGen.MermaidFromCSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DryGen.MermaidFromCSharp.TypeFilters
{
    public class AnyChildFiltersTypeFilter : ITypeFilter
    {
        private readonly IReadOnlyList<ITypeFilter> children;

        public AnyChildFiltersTypeFilter(IReadOnlyList<ITypeFilter> children)
        {
            this.children = children;
        }

        public bool Accepts(Type type)
        {
            return children.Count() == 0 || children.Any(x => x.Accepts(type));
        }
    }
}
