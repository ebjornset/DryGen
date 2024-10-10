using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace DryGen.MermaidFromDotnetDepsJson.DeptsModel;

internal class Dependency : BaseModelElement
{
    public Dependency(string id, JsonObject depdendencyProperty, bool isMainAssembly, bool findTechnology) : base(id)
    {
        RuntimeDependencyRefs = LoadDependencyRefs(depdendencyProperty);
        Technology = findTechnology ? FindTechnology(depdendencyProperty) : string.Empty;
        IsMainAssembly = isMainAssembly;
    }

    public string Technology { get; }
    public bool IsMainAssembly { get; }

    public IReadOnlyList<DependencyRef> RuntimeDependencyRefs { get; private set; }

    public void RemoveNonRuntimeDependencyRefs(Target target)
    {
        var runtimeDependencyRefs = new List<DependencyRef>();
        foreach (var dependencyRef in RuntimeDependencyRefs)
        {
            var runtimeDependency = target.RuntimeDependencies.SingleOrDefault(x => x.Id == dependencyRef.Id);
            if (runtimeDependency == null)
            {
                continue;
            }
            dependencyRef.Dependency = runtimeDependency;
            runtimeDependencyRefs.Add(dependencyRef);
        }
        RuntimeDependencyRefs = runtimeDependencyRefs;
    }

    private static IReadOnlyList<DependencyRef> LoadDependencyRefs(JsonObject targetPropertyObject)
    {
        if (!targetPropertyObject.TryGetPropertyObject("dependencies", out var dependenciesObject) || dependenciesObject == null || dependenciesObject.Count == 0)
        {
            return Array.Empty<DependencyRef>();
        }
        var dependencyRefs = new List<DependencyRef>();
        var enumerator = dependenciesObject.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var dependencyToken = enumerator.Current.Value?.AsValue();
            var dependencyRef = new DependencyRef($"{enumerator.Current.Key}/{dependencyToken}");
            dependencyRefs.Add(dependencyRef);
        }
        return dependencyRefs;
    }

    private static string FindTechnology(JsonObject dependencyObject)
    {
        if (dependencyObject.TryGetPropertyObject("native", out var nativeObject) && nativeObject?.Count > 0)
        {
            return "native";
        }
        // At this point we (assume we) know that we have a runtime property with at least one none null property,
        // since we only create a Dependency when we find a non null runtime properties
        var runtimeObject = dependencyObject.GetPropertyObject("runtime");
        runtimeObject.CheckForChildren();
        var runtimeFirstName = runtimeObject.GetFirstPropertyName();
        if (runtimeFirstName.StartsWith("lib/"))
        {
            var nextSlashOffset = runtimeFirstName.IndexOf('/', 4);
            if (nextSlashOffset > -1)
            {
                return runtimeFirstName[4..nextSlashOffset];
            }
        }
        var lastDotIndex = runtimeFirstName.LastIndexOf('.');
        if (lastDotIndex > -1)
        {
            return runtimeFirstName[(lastDotIndex + 1)..];
        }
        return string.Empty;
    }
}