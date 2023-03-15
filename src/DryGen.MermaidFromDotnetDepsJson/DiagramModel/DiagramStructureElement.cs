using System.Collections.Generic;
using System.Linq;

namespace DryGen.MermaidFromDotnetDepsJson.DiagramModel;

internal abstract class DiagramStructureElement : DiagramElement
{
    protected DiagramStructureElement()
    {
        Elements = new List<DiagramElement>();
    }

    internal IList<DiagramElement> Elements { get; set; }

    internal bool HasMultipleChildren =>
        Elements.Count(x => x is Component) > 1 ||
        Elements.Count(x => x is Component) == 1 && Elements.Count() > 1
        ;
}
