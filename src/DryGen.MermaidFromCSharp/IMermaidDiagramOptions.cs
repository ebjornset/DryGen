﻿using System.Collections.Generic;

namespace DryGen.MermaidFromCSharp;

public interface IMermaidDiagramOptions
{
    string? Title { get; }
    IEnumerable<string>? IncludeNamespaces { get; }
    IEnumerable<string>? IncludeTypeNames { get; }
    IEnumerable<string>? ExcludeTypeNames { get; }
    IEnumerable<string>? ExcludePropertyNames { get; }
    string? NameReplaceFrom { get; }
    string? NameReplaceTo { get; }
}