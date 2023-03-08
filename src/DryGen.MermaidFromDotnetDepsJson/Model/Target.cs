using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DryGen.MermaidFromDotnetDepsJson.Model
{

    internal class Target : BaseModelElement
    {
        public Target(JObject targetObject, string id) : base(id, '=')
        {
            RuntimeDependencies = LoadDependencies(targetObject);
            BindDependencies();
        }

        public IReadOnlyList<Dependency> RuntimeDependencies { get; private set; }

        internal static Target Load(JObject depsObject)
        {
            var targetsObject = GetTargetsObject(depsObject);
            var (targetObject, id) = GetTargetsObjectPropertyObject(targetsObject);
            return new Target(targetObject, id);
        }

        private IReadOnlyList<Dependency> LoadDependencies(JObject targetJson)
        {
            var targetProperties = targetJson.Properties();
            if (targetProperties?.Any() != true)
            {
                return Array.Empty<Dependency>();
            }
            List<Dependency> runtimeDependencies = new List<Dependency>();
            foreach (var targetProperty in targetProperties)
            {
                if (targetProperty?.Count != 1)
                {
                    throw $"TODO: 'targets' has unexpected Count '{targetProperty?.Count}'.".ToInvalidContentException();
                }
                var targetObjectFirst = targetProperty.First;
                if (targetObjectFirst == null)
                {
                    continue;
                }
                if (targetObjectFirst.Type != JTokenType.Object)
                {
                    throw $"TODO: 'targets' is of unexpected type '{targetObjectFirst?.Type}', expected '{nameof(JTokenType.Object)}'.".ToInvalidContentException();
                }
                if (!targetObjectFirst.Any())
                {
                    continue;
                }
                var targetObjectFirstProperty = (JObject)targetObjectFirst;
                var runtimeProperty = targetObjectFirstProperty.Property("runtime");
                if (runtimeProperty == null)
                {
                    continue;
                }
                if (runtimeProperty.Count != 1)
                {
                    throw $"TODO: 'runtime' has unexpected Count '{runtimeProperty.Count}'.".ToInvalidContentException();
                }
                var runtimeObjectFirst = runtimeProperty.First;
                if (runtimeObjectFirst == null)
                {
                    continue;
                }
                if (runtimeObjectFirst.Type != JTokenType.Object)
                {
                    throw $"TODO: 'runtime' is of unexpected type '{runtimeObjectFirst?.Type}', expected '{nameof(JTokenType.Object)}'.".ToInvalidContentException();
                }
                if (!runtimeObjectFirst.Any())
                {
                    continue;
                }
                var dependency = new Dependency(targetProperty);
                runtimeDependencies.Add(dependency);
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

        private static JObject GetTargetsObject(JObject depsJson)
        {
            var targetsProperty = depsJson.Property("targets") ?? throw "'targets' is missing.".ToInvalidContentException();
            if (targetsProperty.Count != 1)
            {
                throw $"TODO: 'targets' has unexpected Count '{targetsProperty.Count}', expected 1.".ToInvalidContentException();
            }
            var targetsFirst = targetsProperty.First;
            if (targetsFirst?.Type != JTokenType.Object)
            {
                throw $"'targets' is of unexpected type '{targetsFirst?.Type}', expected '{nameof(JTokenType.Object)}'.".ToInvalidContentException();
            }
            var targetsObject = (JObject)targetsFirst;
            return targetsObject;
        }

        private static (JObject targetObject, string targetName) GetTargetsObjectPropertyObject(JObject targetsObject)
        {
            var targetsObjectProperties = targetsObject.Properties();
            if (targetsObjectProperties?.Any() != true)
            {
                throw "'targets' is empty.".ToInvalidContentException();
            }
            JObject? targetObject = null;
            string? targetName = null;
            foreach (var targetsObjectProperty in targetsObjectProperties)
            {
                if (targetsObjectProperty.Count != 1)
                {
                    throw $"TODO: 'targets' has unexpected Count '{targetsObjectProperty.Count}'.".ToInvalidContentException();
                }
                var targetsObjectFirst = targetsObjectProperty.First;
                if (targetsObjectFirst == null)
                {
                    continue;
                }
                if (targetsObjectFirst.Type != JTokenType.Object)
                {
                    throw $"TODO: 'targets' is of unexpected type '{targetsObjectFirst?.Type}', expected '{nameof(JTokenType.Object)}'.".ToInvalidContentException();
                }
                if (!targetsObjectFirst.Any())
                {
                    continue;
                }
                targetObject = (JObject)targetsObjectFirst;
                targetName = targetsObjectProperty.Name;
                break;
            }
            if (targetObject == null)
            {
                throw "Found no non empty 'target'.".ToInvalidContentException();
            }
            if (string.IsNullOrWhiteSpace(targetName))
            {
                throw "TODO: Is this possible?".ToInvalidContentException();
            }
            return (targetObject, targetName);
        }
    }
}
