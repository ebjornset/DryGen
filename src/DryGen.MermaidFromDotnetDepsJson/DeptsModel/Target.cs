using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;

namespace DryGen.MermaidFromDotnetDepsJson.DeptsModel;

internal class Target : BaseModelElement
{
    public Target(JsonObject targetObject, string id, bool findTechnology) : base(id, delimiter: ",Version=")
    {
        RuntimeDependencies = LoadDependencies(targetObject, findTechnology);
        BindDependencies();
    }

    public IReadOnlyList<Dependency> RuntimeDependencies { get; private set; }

    internal static Target Load(JsonObject depsObject, bool findTechnologies)
    {
        var (targetObject, id) = GetTargetsObjectPropertyObject(depsObject);
        return new Target(targetObject, id, findTechnologies);
    }

    private IReadOnlyList<Dependency> LoadDependencies(JsonObject targetObject, bool findTechnology)
    {
        var enumerator = targetObject.GetEnumerator();
        var runtimeDependencies = new List<Dependency>();
        var isMainAssembly = true;
        while (enumerator.MoveNext())
        {
            var targetPropertyObject = enumerator.Current.Value?.AsObject();
            if (targetPropertyObject == null || !targetPropertyObject.TryGetPropertyObject("runtime", out var runtimeObject) || runtimeObject == null || runtimeObject.Count == 0)
            {
                continue;
            }
            var dependency = new Dependency(enumerator.Current.Key, targetPropertyObject, isMainAssembly, findTechnology);
            runtimeDependencies.Add(dependency);
            isMainAssembly = false;
        }
        if (!runtimeDependencies.Any())
        {
            throw $"Found no non empty runtime dependencies in target '{Id}'.".ToInvalidContentException();
        }
        return runtimeDependencies;
    }

    private void BindDependencies()
    {
        foreach (var runtimeDependency in RuntimeDependencies)
        {
            runtimeDependency.RemoveNonRuntimeDependencyRefs(this);
        }
    }

    private static (JsonObject targetObject, string targetName) GetTargetsObjectPropertyObject(JsonObject depsObject)
    {
        if (!depsObject.TryGetPropertyValue("targets", out var targetsNode) || targetsNode == null)
        {
            throw "'targets' is missing.".ToInvalidContentException();
        }
        var targetsObject = targetsNode.AsObject();
        var enumeartor = targetsObject.GetEnumerator();
        var isEmpty = true;
        JsonObject? targetObject = null;
        string? targetName = null;
        while (enumeartor.MoveNext())
        {
            isEmpty = false;
            targetObject = enumeartor.Current.Value?.AsObject();
            if ((targetObject?.Count ?? 0) > 0)
            {
                targetName = enumeartor.Current.Key;
                break;
            }
            targetObject = null;
        }
        if (isEmpty)
        {
            throw "'targets' is empty.".ToInvalidContentException();
        }
        if (targetObject == null)
        {
            throw "Found no non empty 'target'.".ToInvalidContentException();
        }
        targetName = GetValidTargetName(targetName);
        return (targetObject, targetName);
    }

    [ExcludeFromCodeCoverage] // Sanity check for unexpected structure. Havent been able to make this fail, but still there might exist such cases...
    private static string GetValidTargetName(string? targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName))
        {
            throw "Found non empty 'target' without name".ToInvalidContentException();
        }
        return targetName;
    }
}
