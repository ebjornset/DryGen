using System;
using System.Collections.Generic;

namespace DryGen.MermaidFromCSharp
{
    public interface IDiagramType
    {
        string Name { get; }
        Type Type { get; }
        bool IsRelatedToAny(IEnumerable<IDiagramType> types);
    }
}
