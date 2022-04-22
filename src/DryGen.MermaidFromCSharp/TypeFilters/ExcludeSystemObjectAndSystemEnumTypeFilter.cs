using DryGen.MermaidFromCSharp;
using System;
using System.Collections.Generic;

namespace DryGen.MermaidFromCSharp.TypeFilters
{
    public class ExcludeSystemObjectAndSystemEnumTypeFilter : AllChildFiltersTypeFilter
    {
        public ExcludeSystemObjectAndSystemEnumTypeFilter() : base(
            new List<ITypeFilter> {
                new ExcludeSystemObjectTypeFilter(),
                new ExcludeSystemEnumTypeFilter()
            })
        {

        }
    }
}
