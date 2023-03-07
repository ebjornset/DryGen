using DryGen.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DryGen.MermaidFromDotnetDepsJson;

public class MermaidC4ComponentDiagramFromDotnetDepsJsonGenerator
{
    public async Task<string> Generate(IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions options)
    {
        await Task.CompletedTask;
        var (targetJson, targetName) = await LoadValidTargetJson(options.InputFile);
        var runtimeDependencyJsonList = LoadRuntimeDependencyJsonList(targetJson, targetName);
        var sb = new StringBuilder().AppendLine("C4Component");
        foreach (var runtimeDependency in runtimeDependencyJsonList)
        {
            sb.Append("\tComponent(\"").Append(runtimeDependency.Name).Append("\", \"").Append(runtimeDependency.Name).AppendLine("\")");
        }
        return sb.ToString();
    }

    private IEnumerable<JProperty> LoadRuntimeDependencyJsonList(JObject targetJson, string targetName)
    {
        var targetProperties = targetJson.Properties();
        if (targetProperties?.Any() != true)
        {
            return Array.Empty<JProperty>();
        }
        List<JProperty> runtimeDependencies = new List<JProperty>();
        foreach (var targetProperty in targetProperties)
        {
            if (targetProperty.Count != 1)
            {
                throw new InvalidContentException(GetInvalidContentMessage($"TODO: 'targets' has unexpected Count '{targetProperty.Count}'."));
            }
            var targetObjectFirst = targetProperty.First;
            if (targetObjectFirst == null)
            {
                continue;
            }
            if (targetObjectFirst.Type != JTokenType.Object)
            {
                throw new InvalidContentException(GetInvalidContentMessage($"TODO: 'targets' is of unexpected type '{targetObjectFirst?.Type}', expected '{nameof(JTokenType.Object)}'."));
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
                throw new InvalidContentException(GetInvalidContentMessage($"TODO: 'runtime' has unexpected Count '{runtimeProperty.Count}'."));
            }
            var runtimeObjectFirst = runtimeProperty.First;
            if (runtimeObjectFirst == null)
            {
                continue;
            }
            if (runtimeObjectFirst.Type != JTokenType.Object)
            {
                throw new InvalidContentException(GetInvalidContentMessage($"TODO: 'runtime' is of unexpected type '{runtimeObjectFirst?.Type}', expected '{nameof(JTokenType.Object)}'."));
            }
            if (!runtimeObjectFirst.Any())
            {
                continue;
            }
            runtimeDependencies.Add(targetProperty);
        }
        if (!runtimeDependencies.Any())
        {
            throw new InvalidContentException(GetInvalidContentMessage($"Found no non empty runtime dependencies in target '{targetName}'."));
        }
        return runtimeDependencies;
    }

    private async Task<(JObject targetObject, string targetName)> LoadValidTargetJson(string? inputFile)
    {
        var depsJsonText = await File.ReadAllTextAsync(inputFile);
        var depsJson = JObject.Parse(depsJsonText);
        var targetsObject = GetTargetsObject(depsJson);
        var (targetObject, targetName) = GetTargetsObjectPropertyObject(targetsObject);
        return (targetObject, targetName);
    }

    private (JObject targetObject, string targetName) GetTargetsObjectPropertyObject(JObject targetsObject)
    {
        var targetsObjectProperties = targetsObject.Properties();
        if (targetsObjectProperties?.Any() != true)
        {
            throw new InvalidContentException(GetInvalidContentMessage("'targets' is empty."));
        }
        JObject? targetObject = null;
        string? targetName = null;
        foreach (var targetsObjectProperty in targetsObjectProperties)
        {
            if (targetsObjectProperty.Count != 1)
            {
                throw new InvalidContentException(GetInvalidContentMessage($"TODO: 'targets' has unexpected Count '{targetsObjectProperty.Count}'."));
            }
            var targetsObjectFirst = targetsObjectProperty.First;
            if (targetsObjectFirst == null)
            {
                continue;
            }
            if (targetsObjectFirst.Type != JTokenType.Object)
            {
                throw new InvalidContentException(GetInvalidContentMessage($"TODO: 'targets' is of unexpected type '{targetsObjectFirst?.Type}', expected '{nameof(JTokenType.Object)}'."));
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
            throw new InvalidContentException(GetInvalidContentMessage("Found no non empty 'target'."));
        }
        if (string.IsNullOrWhiteSpace(targetName))
        {
            throw new InvalidContentException(GetInvalidContentMessage("TODO: Is this possible?"));
        }
        return (targetObject, targetName);
    }

    private JObject GetTargetsObject(JObject depsJson)
    {
        JObject? targetsObject = null;
        var targetsProperty = depsJson.Property("targets") ?? throw new InvalidContentException(GetInvalidContentMessage("'targets' is missing."));
        if (targetsProperty.Count != 1)
        {
            throw new InvalidContentException(GetInvalidContentMessage($"TODO: 'targets' has unexpected Count '{targetsProperty.Count}', expected 1."));
        }
        var targetsFirst = targetsProperty.First;
        if (targetsFirst?.Type != JTokenType.Object)
        {
            throw new InvalidContentException(GetInvalidContentMessage($"'targets' is of unexpected type '{targetsFirst?.Type}', expected '{nameof(JTokenType.Object)}'."));
        }
        targetsObject = (JObject)targetsFirst;
        return targetsObject;
    }

    private string GetInvalidContentMessage(string message) => $"Invalid deps.json: {message}";
}