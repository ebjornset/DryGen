using DryGen.MermaidFromDotnetDepsJson.Model;
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
    public async Task<string> Generate(IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions options)
    {
        var target = await LoadValidTargetJson(options.InputFile);
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

    private static string GenerateDiagram(Target target, DiagramStructure diagramStructure)
    {
        var sb = new StringBuilder().AppendLine("C4Component");
        sb.Append("title Component diagram for ").Append(diagramStructure.MainAssembly.Name).Append(" v").Append(diagramStructure.MainAssembly.Version)
            .Append(" running on ").Append(target.Name).Append(' ').AppendLine(target.Version);
        AppendDiagramStructureElement(sb, diagramStructure);
        AppendRels(sb, target);
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

    private static void AppendDiagramStructureElement(StringBuilder sb, DiagramStructureElement diagramStructureElement)
    {
        AppendDependencies(sb, diagramStructureElement.PreGroupingDependencies);
        AppendContainerBoundaries(sb, diagramStructureElement.ContainerBoundaries);
        AppendDependencies(sb, diagramStructureElement.PostGroupingDependencies);
    }

    private static void AppendContainerBoundaries(StringBuilder sb, IEnumerable<ContainerBoundary>? containerBoundaries)
    {
        if (containerBoundaries?.Any() != true)
        {
            return;
        }
        foreach (var containerBoundary in containerBoundaries)
        {
            ///Container_Boundary(alias, label) {        }
            var shouldWriteBoundry = containerBoundary.HasMultipleChildren;
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
    }

    private static void AppendDependencies(StringBuilder sb, IEnumerable<Dependency>? dependencies)
    {
        if (dependencies?.Any() != true)
        {
            return;
        }
        foreach (var dependency in dependencies)
        {
            // Component(alias, label, ?techn, ?descr, ?sprite, ?tags, ?link)
            sb.Append("Component(\"").Append(dependency.Id).Append("\", \"").Append(dependency.Name).Append("\", \"")
                .Append(dependency.Technology).Append("\", \"v").Append(dependency.Version).AppendLine("\")");
        }
    }

    private static DiagramStructure CreateDiagramStructure(Target target)
    {
        var result = new DiagramStructure(target.RuntimeDependencies[0])
        {
            PreGroupingDependencies = target.RuntimeDependencies,
        };
        FillDiagramStructureElement(result, target.RuntimeDependencies, startIndex: 0);
        return result;
    }

    private static void FillDiagramStructureElement(DiagramStructureElement diagramStructureElement, IEnumerable<Dependency> dependencies, int startIndex)
    {
        var nonGroupedCandidates = dependencies.Where(x =>
        {
            var dotIndex = x.Name.IndexOf('.', startIndex);
            return dotIndex <= startIndex;
        }).ToList();
        var containerBoundaryCandidates = dependencies.Except(nonGroupedCandidates).ToList();
        var containerBoundaryGroups = containerBoundaryCandidates.GroupBy(x =>
            {
                var dotIndex = x.Name.IndexOf(".", startIndex);
                var key = x.Name[..dotIndex];
                return key;
            });
        if (!containerBoundaryGroups.Any())
        {
            diagramStructureElement.PreGroupingDependencies = nonGroupedCandidates;
            return;
        }
        var containerBoundaries = new List<ContainerBoundary>();
        foreach (var containerBoundaryGroup in containerBoundaryGroups)
        {
            var containerBoundary = new ContainerBoundary(containerBoundaryGroup.Key);
            containerBoundaries.Add(containerBoundary);
            var groupStartIndex = containerBoundaryGroup.Key.Length + 1;
            FillDiagramStructureElement(containerBoundary, containerBoundaryGroup, groupStartIndex);
        }
        // Move any Dependency that has the same name as a ContainerBoundary into that ContainerBoundary.
        var consolidateCandidates = nonGroupedCandidates.Where(x => containerBoundaries.Any(y => x.Name == y.Label)).ToList();
        foreach(var consolidateCandidate in consolidateCandidates)
        {
            var consolidateContainerBoundary = containerBoundaries.Single(x => x.Label == consolidateCandidate.Name);
            var consolidatePreGroupingDependencies = new List<Dependency> { consolidateCandidate };
            if (consolidateContainerBoundary.PreGroupingDependencies?.Any() == true)
            {
                consolidatePreGroupingDependencies.AddRange(consolidateContainerBoundary.PreGroupingDependencies);
            }
            consolidateContainerBoundary.PreGroupingDependencies = consolidatePreGroupingDependencies;
        }
        diagramStructureElement.PreGroupingDependencies = nonGroupedCandidates.Except(consolidateCandidates);
        diagramStructureElement.ContainerBoundaries = containerBoundaries;
    }

    private class DiagramStructureElement
    {
        public IEnumerable<Dependency>? PreGroupingDependencies { get; set; }
        public IEnumerable<Dependency>? PostGroupingDependencies { get; set; }
        public IEnumerable<ContainerBoundary>? ContainerBoundaries { get; set; }
        public bool HasMultipleChildren => ((PreGroupingDependencies?.Count() ?? 0) + 
            (PostGroupingDependencies?.Count() ?? 0) + 
            (ContainerBoundaries?.Count() ?? 0)) > 1;
    }

    private sealed class DiagramStructure : DiagramStructureElement
    {
        public DiagramStructure(Dependency mainAssembly)
        {
            MainAssembly = mainAssembly;
        }
        public Dependency MainAssembly { get; }
    }

    private sealed class ContainerBoundary : DiagramStructureElement
    {
        public ContainerBoundary(string name)
        {
            Alias = name;
            Label = name;
        }
        public string Alias { get; }
        public string Label { get; }
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
