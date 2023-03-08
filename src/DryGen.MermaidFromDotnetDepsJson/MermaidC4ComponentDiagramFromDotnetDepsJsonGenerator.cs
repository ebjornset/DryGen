﻿using DryGen.MermaidFromDotnetDepsJson.Model;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DryGen.MermaidFromDotnetDepsJson;

public class MermaidC4ComponentDiagramFromDotnetDepsJsonGenerator
{
    public async Task<string> Generate(IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions options)
    {
        var target = await LoadValidTargetJson(options.InputFile);
        var sb = new StringBuilder().AppendLine("C4Component");
        foreach (var runtimeDependency in target.RuntimeDependencies)
        {
            // Component(alias, label, ?techn, ?descr, ?sprite, ?tags, ?link)
            sb.Append("\tComponent(\"").Append(runtimeDependency.Id).Append("\", \"").Append(runtimeDependency.Name).Append("\", \"\", \"v ").Append(runtimeDependency.Version).AppendLine("\")");
        }
        foreach (var fromDependency in target.RuntimeDependencies)
        {
            foreach(var toDependency in fromDependency.RuntimeDependencyRefs.Select( x => x.Dependency))
            {
                //Rel(from, to, label, ?techn, ?descr, ?sprite, ?tags, ?link)
                sb.Append("\tRel(\"").Append(fromDependency.Id).Append("\", \"").Append(toDependency?.Id).AppendLine("\", \"\", \"\")");
            }
        }
        return sb.ToString();
    }

    private async Task<Target> LoadValidTargetJson(string? inputFile)
    {
        var depsJsonText = await File.ReadAllTextAsync(inputFile);
        var depsJson = JObject.Parse(depsJsonText);
        if (depsJson == null)
        {
            throw $"Could not parse deps.json.".ToInvalidContentException();
        }
        var target = Target.Load(depsJson);
        return target;
    }
}
