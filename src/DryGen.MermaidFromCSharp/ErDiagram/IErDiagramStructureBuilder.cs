using System.Collections.Generic;
using System.Reflection;

namespace DryGen.MermaidFromCSharp.ErDiagram
{
    public interface IErDiagramStructureBuilder
    {
        IReadOnlyList<ErDiagramEntity> GenerateErStructure(Assembly assembly, IReadOnlyList<ITypeFilter> typeFilters, IReadOnlyList<IPropertyFilter> attributeFilters, INameRewriter nameRewriter);
    }
}
