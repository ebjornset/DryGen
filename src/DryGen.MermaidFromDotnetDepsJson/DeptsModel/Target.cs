using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DryGen.MermaidFromDotnetDepsJson.DeptsModel;

internal class Target : BaseModelElement
{
    public Target(JObject targetObject, string id, bool findTechnology) : base(id, delimiter: ",Version=")
    {
        RuntimeDependencies = LoadDependencies(targetObject, findTechnology);
        BindDependencies();
    }

    public IReadOnlyList<Dependency> RuntimeDependencies { get; private set; }

    internal static Target Load(JObject depsObject, bool findTechnologies)
    {
        var (targetObject, id) = GetTargetsObjectPropertyObject(depsObject);
        return new Target(targetObject, id, findTechnologies);
    }

    private IReadOnlyList<Dependency> LoadDependencies(JObject targetObject, bool findTechnology)
    {
        var targetProperties = targetObject.Properties();
        var runtimeDependencies = new List<Dependency>();
        var isMainAssembly = true;
        foreach (var targetProperty in targetProperties)
        {
            var targetPropertyObject = targetProperty.GetPropertyObject();
            if (!targetPropertyObject.TryGetPropertyObject("runtime", out var runtimeObject))
            {
                continue;
            }
            if (runtimeObject?.Count == 0)
            {
                continue;
            }
            var dependency = new Dependency(targetProperty, isMainAssembly, findTechnology);
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

    private static (JObject targetObject, string targetName) GetTargetsObjectPropertyObject(JObject depsObject)
    {
        var targetsProperty = depsObject.Property("targets") ?? throw "'targets' is missing.".ToInvalidContentException();
        var targetsObject = targetsProperty.GetPropertyObject();
        var targetsObjectProperties = targetsObject.Properties();
        if (targetsObjectProperties?.Any() != true)
        {
            throw "'targets' is empty.".ToInvalidContentException();
        }
        JObject? targetObject = null;
        string? targetName = null;
        foreach (var targetsObjectProperty in targetsObjectProperties)
        {
            targetObject = targetsObjectProperty.GetPropertyObject();
            if (targetObject.Count == 0)
            {
                targetObject = null;
                continue;
            }
            targetName = targetsObjectProperty.Name;
            break;
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
