﻿using DryGen.MermaidFromCSharp.TypeFilters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DryGen.MermaidFromCSharp;

public class TreeShakingDiagramFilter : IDiagramFilter
{
    private readonly AnyChildFiltersTypeFilter? treeShakingRootsFilter;

    public TreeShakingDiagramFilter(IReadOnlyList<ITypeFilter>? treeShakingRoots)
    {
        if (treeShakingRoots?.Any() == true)
        {
            treeShakingRootsFilter = new AnyChildFiltersTypeFilter(treeShakingRoots);
        }
    }

    public IEnumerable<TDiagramType> Filter<TDiagramType>(IEnumerable<TDiagramType> types) where TDiagramType : IDiagramType
    {
        if (treeShakingRootsFilter == null)
        {
            return types;
        }

        var result = types.Where(x => treeShakingRootsFilter.Accepts(x.Type)).ToList();
        if (result.Count == 0)
        {
            return Array.Empty<TDiagramType>();
        }
        do
        {
            var nonRootedTypes = GetNonRootedTypes(allTypes: types, rootedTypes: result);
            var newRootedTypes = nonRootedTypes.Where(x => x.IsRelatedToAny(result.Cast<IDiagramType>())).Select(x => (TDiagramType)x);
            if (!newRootedTypes.Any())
            {
                break;
            }
            result.AddRange(newRootedTypes);
        }
        while (true);
        return result.OrderBy(x => x.Name);
    }

    private static IEnumerable<IDiagramType> GetNonRootedTypes<TDiagramType>(IEnumerable<TDiagramType> allTypes, IEnumerable<TDiagramType> rootedTypes) where TDiagramType : IDiagramType
    {
        return allTypes.Except(rootedTypes).Cast<IDiagramType>();
    }
}