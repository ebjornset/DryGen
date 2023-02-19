using System;

namespace DryGen.MermaidFromCSharp;

public interface ITypeFilter
{
    bool Accepts(Type type);
}
