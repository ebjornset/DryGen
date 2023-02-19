using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.TypeFilters;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;

namespace DryGen.UTests.Helpers;

public class TreeShakingContext
{
    private List<IncludeTypeNameTypeFilter>? treeShakingRoots;
    public IDiagramFilter DiagramFilter => new TreeShakingDiagramFilter(treeShakingRoots);

    public void AddTreeShakingRoots(Table table)
    {
        if (treeShakingRoots == null)
        {
            treeShakingRoots = new();
        }
        var newRoots = table.Rows.Select(x => new IncludeTypeNameTypeFilter(x[0]));
        treeShakingRoots.AddRange(newRoots);
    }
}
