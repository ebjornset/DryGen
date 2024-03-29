﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DryGen.MermaidFromCSharp;

public class NamedType : IDiagramType
{
    public NamedType(string name, Type type)
    {
        Name = name.Replace('`', '-');
        Type = type;
    }

    public string Name { get; private set; }
    public Type Type { get; private set; }

    public bool IsRelatedToAny(IEnumerable<IDiagramType> types)
    {
        return types.Any(x => IsRelatedTo(x));
    }

    [ExcludeFromCodeCoverage()] // This should in theory never happend.
    protected virtual bool IsRelatedTo(IDiagramType type)
    {
        throw new NotImplementedException($"'{nameof(IsRelatedTo)}' should be implemented in all subclasses and should never be accessed directly on a '{typeof(NamedType).FullName}'");
    }
}