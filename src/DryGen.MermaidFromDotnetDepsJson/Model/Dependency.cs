using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DryGen.MermaidFromDotnetDepsJson.Model
{
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
            if (targetProperty?.Count != 1)
            {
                throw $"TODO: 'targets' has unexpected Count '{targetProperty?.Count}'.".ToInvalidContentException();
            }
            var targetObjectFirst = targetProperty.First;
            if (targetObjectFirst == null)
            {
                throw $"TODO: 'targets' has unexpected Count '{targetObjectFirst}'.".ToInvalidContentException();
            }
            if (targetObjectFirst.Type != JTokenType.Object)
            {
                throw $"TODO: 'targets' is of unexpected type '{targetObjectFirst?.Type}', expected '{nameof(JTokenType.Object)}'.".ToInvalidContentException();
            }
            if (!targetObjectFirst.Any())
            {
                throw $"TODO: 'targets' is of unexpected type '{targetObjectFirst?.Type}', expected '{nameof(JTokenType.Object)}'.".ToInvalidContentException();
            }
            var targetObjectFirstProperty = (JObject)targetObjectFirst;
            var dependenciesProperty = targetObjectFirstProperty.Property("dependencies");
            if (dependenciesProperty == null)
            {
                return Array.Empty<DependencyRef>();
            }
            if (dependenciesProperty.Count != 1)
            {
                throw $"TODO: 'runtime' has unexpected Count '{dependenciesProperty.Count}'.".ToInvalidContentException();
            }
            var dependenciesObjectFirst = dependenciesProperty.First;
            if (dependenciesObjectFirst == null)
            {
                throw $"TODO: 'runtime' has unexpected Count '{dependenciesObjectFirst}'.".ToInvalidContentException();
            }
            if (dependenciesObjectFirst.Type != JTokenType.Object)
            {
                throw $"TODO: 'runtime' is of unexpected type '{dependenciesObjectFirst?.Type}', expected '{nameof(JTokenType.Object)}'.".ToInvalidContentException();
            }
            if (!dependenciesObjectFirst.Any())
            {
                return Array.Empty<DependencyRef>();
            }
            var dependencyRefs = new List<DependencyRef>();
            foreach (var dependencyToken in dependenciesObjectFirst.Children())
            {
                if (dependencyToken.Type != JTokenType.Property)
                {
                    throw $"TODO: 'targets' is of unexpected type '{dependencyToken?.Type}', expected '{nameof(JTokenType.Property)}'.".ToInvalidContentException();
                }
                if (dependencyToken.Count() != 1)
                {
                    throw $"TODO: 'targets' has unexpected Count '{dependencyToken.Count()}'.".ToInvalidContentException();
                }
                var dependencyTokenFirst = dependencyToken.First;
                if (dependencyTokenFirst == null)
                {
                    throw $"TODO: 'targets' has unexpected Count '{dependencyTokenFirst}'.".ToInvalidContentException();
                }
                if (dependencyTokenFirst.Type != JTokenType.String)
                {
                    throw $"TODO: 'targets' is of unexpected type '{dependencyTokenFirst?.Type}', expected '{nameof(JTokenType.String)}'.".ToInvalidContentException();
                }
                var dependencyTokenProperty = (JProperty)dependencyToken;
                var dependencyRef = new DependencyRef($"{dependencyTokenProperty.Name}/{dependencyTokenFirst}");
                dependencyRefs.Add(dependencyRef);
            }
            return dependencyRefs;
        }
    }
}
