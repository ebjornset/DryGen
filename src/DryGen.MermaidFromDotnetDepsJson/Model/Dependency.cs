using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DryGen.MermaidFromDotnetDepsJson.Model;

internal class Dependency : BaseModelElement
{
    public Dependency(JProperty depdendencyProperty) : base(depdendencyProperty.Name)
    {
        RuntimeDependencyRefs = LoadDependencyRefs(depdendencyProperty);
    }

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

    private IReadOnlyList<DependencyRef> LoadDependencyRefs(JProperty targetProperty)
    {
        var targetPropertyObject = targetProperty.GetPropertyObject();
        if (targetPropertyObject?.TryGetPropertyObject("dependencies", out var dependenciesObject) != true)
        {
            return Array.Empty<DependencyRef>();
        }
        if (dependenciesObject?.Count == 0)
        {
            return Array.Empty<DependencyRef>();
        }
        var dependencyRefs = new List<DependencyRef>();
        if (dependenciesObject != null)
        {
            foreach (var dependencyToken in dependenciesObject.Children())
            {
                dependencyToken.CheckTypeIs(JTokenType.Property);
                var token = ((JProperty)dependencyToken).GetPropertyToken(JTokenType.String);
                var dependencyTokenProperty = (JProperty)dependencyToken;
                var dependencyRef = new DependencyRef($"{dependencyTokenProperty.Name}/{token}");
                dependencyRefs.Add(dependencyRef);
            }
        }
        return dependencyRefs;
    }
}
