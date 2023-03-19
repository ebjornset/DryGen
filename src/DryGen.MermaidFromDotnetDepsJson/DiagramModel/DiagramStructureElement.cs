using System.Collections.Generic;
using System.Linq;

namespace DryGen.MermaidFromDotnetDepsJson.DiagramModel;

internal abstract class DiagramStructureElement : IDiagramElement
{
    protected DiagramStructureElement()
    {
        Elements = new List<IDiagramElement>();
    }

    internal IList<IDiagramElement> Elements { get; set; }

    internal bool HasMultipleChildren =>
        Elements.Count(x => x is Component) > 1 ||
        Elements.Count(x => x is Component) == 1 && Elements.Count() > 1;
}
