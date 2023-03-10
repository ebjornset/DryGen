using DryGen.MermaidFromDotnetDepsJson.Model;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DryGen.MermaidFromDotnetDepsJson;

public class MermaidC4ComponentDiagramFromDotnetDepsJsonGenerator
{
    public async Task<string> Generate(IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions options)
    {
        var target = await LoadValidTargetJson(options.InputFile);
        return GenerateDiagram(target);
    }

    private static string GenerateDiagram(Target target)
    {
        var sb = new StringBuilder().AppendLine("C4Component");
        if (target.RuntimeDependencies.Any())
        {
            var mainAssembly = target.RuntimeDependencies.First();
            sb.Append("title Component diagram for ").Append(mainAssembly.Name).Append(" v").Append(mainAssembly.Version)
                .Append(" running as ").Append(target.Name).Append(' ').AppendLine(target.Version);
        }
        foreach (var runtimeDependency in target.RuntimeDependencies)
        {
            // Component(alias, label, ?techn, ?descr, ?sprite, ?tags, ?link)
            sb.Append("Component(\"").Append(runtimeDependency.Id).Append("\", \"").Append(runtimeDependency.Name).Append("\", \"\", \"v").Append(runtimeDependency.Version).AppendLine("\")");
        }
        foreach (var fromDependency in target.RuntimeDependencies)
        {
            foreach (var toDependency in fromDependency.RuntimeDependencyRefs.Select(x => x.Dependency))
            {
                //Rel(from, to, label, ?techn, ?descr, ?sprite, ?tags, ?link)
                sb.Append("Rel(\"").Append(fromDependency.Id).Append("\", \"").Append(toDependency?.Id).AppendLine("\", \"\", \"\")");
            }
        }
        return sb.ToString();
    }

    private async Task<Target> LoadValidTargetJson(string? inputFile)
    {
        var depsJsonText = await File.ReadAllTextAsync(inputFile);
        var depsJson = JObject.Parse(depsJsonText);
        CheckForNull(depsJson);
        var target = Target.Load(depsJson);
        return target;
    }

    [ExcludeFromCodeCoverage] // Just a guard that we dont bother to test
    private static void CheckForNull(JObject depsJson)
    {
        if (depsJson == null)
        {
            throw $"Could not parse deps.json.".ToInvalidContentException();
        }
    }
}
