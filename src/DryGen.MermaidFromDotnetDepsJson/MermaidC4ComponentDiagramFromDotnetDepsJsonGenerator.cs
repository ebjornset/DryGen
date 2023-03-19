using DryGen.MermaidFromDotnetDepsJson.DeptsModel;
using DryGen.MermaidFromDotnetDepsJson.DiagramModel;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DryGen.MermaidFromDotnetDepsJson;

public class MermaidC4ComponentDiagramFromDotnetDepsJsonGenerator
{
    private readonly RelationsLevel relationsLevel;
    private readonly BoundariesLevel boundariesLevel;
    private readonly bool excludeVersion;
    private readonly bool excludeTechn;

    public MermaidC4ComponentDiagramFromDotnetDepsJsonGenerator(IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions options)
    {
        relationsLevel = options.RelationsLevel ?? default;
        boundariesLevel = options.BoundariesLevel ?? default;
        excludeVersion = options.ExcludeVersion ?? default;
        excludeTechn = options.ExcludeTechn ?? default;
    }

    public async Task<string> Generate(string? inputFile)
    {
        var target = await LoadValidTargetJson(inputFile);
        var diagramStructure = CreateDiagramStructure(target);
        return GenerateDiagram(target, diagramStructure);
    }

    private async Task<Target> LoadValidTargetJson(string? inputFile)
    {
        var depsJsonText = await File.ReadAllTextAsync(inputFile);
        var depsJson = JObject.Parse(depsJsonText);
        CheckForNull(depsJson);
        var target = Target.Load(depsJson);
        return target;
    }

    private string GenerateDiagram(Target target, DiagramStructure diagramStructure)
    {
        var sb = new StringBuilder().AppendLine("C4Component");
        sb.Append("title Component diagram for ").Append(diagramStructure.MainAssembly.Name).Append(" v").Append(diagramStructure.MainAssembly.Version)
            .Append(" running on ").Append(target.Name).Append(' ').AppendLine(target.Version);
        AppendDiagramStructureElement(sb, diagramStructure);
        if (relationsLevel == RelationsLevel.All)
        {
            AppendRels(sb, target);
        }
        return sb.ToString();
    }

    private static void AppendRels(StringBuilder sb, Target target)
    {
        foreach (var fromDependency in target.RuntimeDependencies)
        {
            foreach (var toDependency in fromDependency.RuntimeDependencyRefs.Select(x => x.Dependency))
            {
                //Rel(from, to, label, ?techn, ?descr, ?sprite, ?tags, ?link)
                sb.Append("Rel(\"").Append(fromDependency.Id).Append("\", \"").Append(toDependency?.Id).AppendLine("\", \"\", \"\")");
            }
        }
    }

    private void AppendDiagramStructureElement(StringBuilder sb, DiagramStructureElement diagramStructureElement)
    {
        foreach (var element in diagramStructureElement.Elements)
        {
            if (element is ContainerBoundary containerBoundary)
            {
                AppendContainerBoundary(sb, containerBoundary);
            }
            else if (element is Component component)
            {
                AppendComponent(sb, component);
            }
        }
    }

    private void AppendContainerBoundary(StringBuilder sb, ContainerBoundary containerBoundary)
    {
        ///Container_Boundary(alias, label) {        }
        var shouldWriteBoundry = boundariesLevel == BoundariesLevel.All && (containerBoundary.DontSuppress || containerBoundary.HasMultipleChildren);
        if (shouldWriteBoundry)
        {
            sb.Append("Container_Boundary(\"").Append(containerBoundary.Alias).Append("\", \"").Append(containerBoundary.Label).AppendLine("\") {");
        }
        AppendDiagramStructureElement(sb, containerBoundary);
        if (shouldWriteBoundry)
        {
            sb.AppendLine("}");
        }
    }

    private void AppendComponent(StringBuilder sb, Component component)
    {
        var dependency = component.Dependency;
        // Component(alias, label, ?techn, ?descr, ?sprite, ?tags, ?link)
        sb.Append("Component(\"").Append(dependency.Id).Append("\", \"").Append(dependency.Name).Append("\", \"");
        if (!excludeTechn)
        {
            sb.Append(dependency.Technology);
        }
        sb.Append("\", \"");
        if (!excludeVersion)
        {
            sb.Append('v').Append(dependency.Version);
        }
        sb.AppendLine("\")");
    }

    private static DiagramStructure CreateDiagramStructure(Target target)
    {
        var result = new DiagramStructure(target.RuntimeDependencies[0]);
        FillDiagramStructureElement(result, target.RuntimeDependencies, startIndex: 0, useStandaloneDependencies: true);
        return result;
    }

    private static void FillDiagramStructureElement(DiagramStructureElement diagramStructureElement, IEnumerable<Dependency> dependencies, int startIndex, bool useStandaloneDependencies)
    {
        var childrenLists = new List<(string, List<Dependency>)>();
        var groups = new Dictionary<string, List<Dependency>>();
        var standaloneDependenciesBoundry = useStandaloneDependencies ? new ContainerBoundary("Standalone dependencies", dontSuppress: true) : null;
        foreach (var dependency in dependencies)
        {
            var dotIndex = dependency.Name.Length > startIndex ? dependency.Name.IndexOf('.', startIndex) : -1;
            var groupName = dotIndex > startIndex ? dependency.Name[..dotIndex] : dependency.Name;
            List<Dependency> groupList;
            if (groups.ContainsKey(groupName))
            {
                groupList = groups[groupName];
            }
            else
            {
                groupList = new List<Dependency>();
                childrenLists.Add((groupName, groupList));
                groups.Add(groupName, groupList);
            }
            groupList.Add(dependency);
        }
        foreach (var (groupName, children) in childrenLists)
        {
            if (children.Count == 1)
            {
                var dependencyCoponent = new Component(children[0]);
                if (dependencyCoponent.Dependency.IsMainAssembly || !useStandaloneDependencies)
                {
                    diagramStructureElement.Elements.Add(dependencyCoponent);
                }
                else
                {
                    standaloneDependenciesBoundry?.Elements.Add(dependencyCoponent);
                }
                continue;
            }
            var containerBoundary = new ContainerBoundary(groupName, dontSuppress: false);
            var groupStartIndex = groupName.Length + 1;
            FillDiagramStructureElement(containerBoundary, children, groupStartIndex, useStandaloneDependencies: false);
            diagramStructureElement.Elements.Add(containerBoundary);
        }
        if (standaloneDependenciesBoundry?.Elements.Any() == true)
        {
            diagramStructureElement.Elements.Add(standaloneDependenciesBoundry);
        }
    }


    [ExcludeFromCodeCoverage] // Just a guard that we don't bother to test
    private static void CheckForNull(JObject depsJson)
    {
        if (depsJson == null)
        {
            throw $"Could not parse deps.json.".ToInvalidContentException();
        }
    }
}
