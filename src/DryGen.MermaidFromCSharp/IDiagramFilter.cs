using System.Collections.Generic;

namespace DryGen.MermaidFromCSharp;

public interface IDiagramFilter
{
    IEnumerable<TDiagramType> Filter<TDiagramType>(IEnumerable<TDiagramType> types) where TDiagramType : IDiagramType;
}
