---
layout: post
title: Version 1.3.0 is released
subtitle: Support for assembly name filters when generating Mermaid C4 Component diagrams from .Net deps.json files
summary: |-
  Support for assembly name filters when generating Mermaid C4 Component diagrams from .Net deps.json files
hero_height: is-fullwidth
---

### In this version

- New options `--include-assemblynames` and `--exclude-assemblynames` for the verb [mermaid-c4component-diagram-from-dotnet-deps-json](/verbs/mermaid-c4component-diagram-from-dotnet-deps-json/).
- Removed all dependencies on external assemblies when generating Mermaid diagrams from C# code, to prevent version clashes that could happen if the user code is referencing other versions of any of these assemblies.
- Loads user assemblies in a separate AssemblyLoadContext when generating Mermaid diagrams from C# code, to prevent a possible version clash on the Microsoft.Extensions.Primitives assembly.

Have fun!
