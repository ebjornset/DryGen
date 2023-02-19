using System;

namespace DryGen.MermaidFromCSharp.TypeFilters;

public class ExcludeSystemRuntimeCompilerServicesRefSafetyRulesAttributeTypeFilter : ITypeFilter
{
    public bool Accepts(Type type)
    {
        return type.FullName != "System.Runtime.CompilerServices.RefSafetyRulesAttribute";
    }
}
