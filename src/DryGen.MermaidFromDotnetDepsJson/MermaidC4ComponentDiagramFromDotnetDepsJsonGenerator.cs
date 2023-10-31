using DryGen.MermaidFromDotnetDepsJson.DeptsModel;
using DryGen.MermaidFromDotnetDepsJson.DiagramModel;
using DryGen.MermaidFromDotnetDepsJson.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DryGen.MermaidFromDotnetDepsJson;

public class MermaidC4ComponentDiagramFromDotnetDepsJsonGenerator
{
    private readonly RelationLevel relationsLevel;
    private readonly BoundaryLevel boundariesLevel;
    private readonly bool excludeVersion;
    private readonly bool excludeTechn;
    private readonly string? title;
    private readonly int? shapeInRow;
    private readonly int? boundaryInRow;
    private readonly IReadOnlyList<IAssemblyNameFilter> assemblyNameFilters;

    public MermaidC4ComponentDiagramFromDotnetDepsJsonGenerator(IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions options, IReadOnlyList<IAssemblyNameFilter>? assemblyNameFilters)
    {
        relationsLevel = options.RelationLevel ?? default;
        boundariesLevel = options.BoundaryLevel ?? default;
        excludeVersion = options.ExcludeVersion ?? default;
        excludeTechn = options.ExcludeTechn ?? default;
        title = options.Title;
        shapeInRow = options.ShapeInRow;
        boundaryInRow = options.BoundaryInRow;
        this.assemblyNameFilters = assemblyNameFilters?.Any() == true ? assemblyNameFilters : Array.Empty<IAssemblyNameFilter>();
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
        var depsJson = JsonNode.Parse(depsJsonText)?.AsObject();
        depsJson = depsJson.CheckForNull();
        var target = Target.Load(depsJson, findTechnologies: !excludeTechn, assemblyNameFilters);
        return target;
    }

    private string GenerateDiagram(Target target, DiagramStructure diagramStructure)
    {
        var sb = new StringBuilder().AppendLine("C4Component");
        AppendTitle(target, diagramStructure, sb);
        AppendDiagramStructureElement(sb, diagramStructure);
        AppendRels(sb, diagramStructure, target);
        AppendUpdateLayoutConfig(sb);
        return sb.ToString();
    }

    private void AppendUpdateLayoutConfig(StringBuilder sb)
    {
        if (shapeInRow.HasValue || boundaryInRow.HasValue)
        {
            sb.Append("UpdateLayoutConfig($c4ShapeInRow = \"").Append(shapeInRow ?? 4).Append("\", $c4BoundaryInRow = \"").Append(boundaryInRow ?? 2).AppendLine("\")");
        }
    }

    private void AppendTitle(Target target, DiagramStructure diagramStructure, StringBuilder sb)
    {
        if (string.IsNullOrEmpty(title))
        {
            sb.Append("title Component diagram for ").Append(diagramStructure.MainAssembly.Name).Append(" v").Append(diagramStructure.MainAssembly.Version)
                .Append(" running on ").Append(target.Name).Append(' ').AppendLine(target.Version);
        }
        else
        {
            sb.Append("title ").AppendLine(title);
        }
    }

    private void AppendRels(StringBuilder sb, DiagramStructure diagramStructure, Target target)
    {
        switch (relationsLevel)
        {
            case RelationLevel.All:
                AppendAllRels(sb, target);
                break;
            case RelationLevel.InterBoundary:
                AppendInterBoundaryRels(sb, diagramStructure);
                break;
            case RelationLevel.IntraBoundary:
                AppendIntraBoundaryRels(sb, diagramStructure);
                break;
            case RelationLevel.None:
            default:
                break;
        }
    }

    private static void AppendAllRels(StringBuilder sb, Target target)
    {
        foreach (var fromDependency in target.RuntimeDependencies)
        {
            foreach (var toDependency in fromDependency.RuntimeDependencyRefs.Select(x => x.Dependency))
            {
                AppendRel(sb, fromDependency, toDependency);
            }
        }
    }

    private static void AppendInterBoundaryRels(StringBuilder sb, DiagramStructure diagramStructure)
    {
        AppendBoundryFilteredRels(sb, diagramStructure, IsInterBoundary);
    }

    private static void AppendIntraBoundaryRels(StringBuilder sb, DiagramStructure diagramStructure)
    {
        AppendBoundryFilteredRels(sb, diagramStructure, IsIntraBoundary);
    }

    private static void AppendBoundryFilteredRels(StringBuilder sb, DiagramStructure diagramStructure, Func<DependencyRef, List<Dependency>, bool> filter)
    {
        var sameBoundaryDependencies = BuildBoundaryDependencyLists(diagramStructure);
        foreach (var boundaryDependencies in sameBoundaryDependencies)
        {
            foreach (var fromDependency in boundaryDependencies)
            {
                foreach (var toDependency in fromDependency.RuntimeDependencyRefs.Where(x => filter(x, boundaryDependencies)).Select(x => x.Dependency))
                {
                    AppendRel(sb, fromDependency, toDependency);
                }
            }
        }
    }

    private static bool IsInterBoundary(DependencyRef dependencyRef, List<Dependency> boundaryDependencies)
    {
        return boundaryDependencies.TrueForAll(y => y != dependencyRef.Dependency);
    }

    private static bool IsIntraBoundary(DependencyRef dependencyRef, List<Dependency> boundaryDependencies)
    {
        return boundaryDependencies.Exists(y => y == dependencyRef.Dependency);
    }

    private static void AppendRel(StringBuilder sb, Dependency fromDependency, Dependency? toDependency)
    {
        //Rel(from, to, label, ?techn, ?descr, ?sprite, ?tags, ?link)
        sb.Append("Rel(\"").Append(fromDependency.Id).Append("\", \"").Append(toDependency?.Id).AppendLine("\", \"\", \"\")");
    }

    private static List<List<Dependency>> BuildBoundaryDependencyLists(DiagramStructure diagramStructure)
    {
        var result = new List<List<Dependency>>();
        var unboundariedDependencies = new List<Dependency>();
        foreach (var diagramElement in diagramStructure.Elements)
        {
            if (diagramElement is Component ungroupedComponent)
            {
                unboundariedDependencies.Add(ungroupedComponent.Dependency);
                continue;
            }
            var boundaryDependencies = new List<Dependency>();
            GetAllBoundaryDependencies(boundaryDependencies, (ContainerBoundary)diagramElement);
            result.Add(boundaryDependencies);
        }
        if (unboundariedDependencies.Any())
        {
            result.Insert(0, unboundariedDependencies);
        }
        return result;
    }

    private static void GetAllBoundaryDependencies(List<Dependency> boundaryDependencies, ContainerBoundary containerBoundary)
    {
        foreach (var diagramElement in containerBoundary.Elements)
        {
            if (diagramElement is Component ungroupedComponent)
            {
                boundaryDependencies.Add(ungroupedComponent.Dependency);
                continue;
            }
            GetAllBoundaryDependencies(boundaryDependencies, (ContainerBoundary)diagramElement);
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
        var shouldWriteBoundry = boundariesLevel != BoundaryLevel.None && (containerBoundary.DontSuppress || containerBoundary.HasMultipleChildren);
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

    private DiagramStructure CreateDiagramStructure(Target target)
    {
        var result = new DiagramStructure(target.RuntimeDependencies[0]);
        if (boundariesLevel == BoundaryLevel.InternalExternal)
        {
            FillDiagramStructureElementWithInternalExternalBoundaries(result, target.RuntimeDependencies);
        }
        else
        {
            FillDiagramStructureElementWithRealBoundaries(result, target.RuntimeDependencies, startIndex: 0, useStandaloneDependencies: true);
        }
        return result;
    }

    private void FillDiagramStructureElementWithInternalExternalBoundaries(DiagramStructure result, IEnumerable<Dependency> dependencies)
    {
        var internalAssembliesBoundary = new ContainerBoundary("Internal assemblies", dontSuppress: false);
        var externalDependenciesBoundary = new ContainerBoundary("External dependencies", dontSuppress: false);
        var mainAssembly = result.MainAssembly;
        string mainAssemblyNamePrefix;
        var mainAssemblyPrefixLength = mainAssembly.Name.IndexOf('.');
        if (mainAssemblyPrefixLength == -1)
        {
            mainAssemblyNamePrefix = mainAssembly.Name;
        }
        else
        {
            mainAssemblyNamePrefix = mainAssembly.Name[..mainAssemblyPrefixLength];
        }
        foreach (var dependency in dependencies)
        {
            if (dependency.Name.StartsWith(mainAssemblyNamePrefix))
            {
                internalAssembliesBoundary.Elements.Add(new Component(dependency));
            }
            else
            {
                externalDependenciesBoundary.Elements.Add(new Component(dependency));
            }
        }
        result.Elements.Add(internalAssembliesBoundary);
        result.Elements.Add(externalDependenciesBoundary);
    }

    private static void FillDiagramStructureElementWithRealBoundaries(DiagramStructureElement diagramStructureElement, IEnumerable<Dependency> dependencies, int startIndex, bool useStandaloneDependencies)
    {
        var childrenLists = new List<(string, List<Dependency>)>();
        var groups = new Dictionary<string, List<Dependency>>();
        var standaloneDependenciesBoundry = useStandaloneDependencies ? new ContainerBoundary("Standalone dependencies", dontSuppress: true) : null;
        BuildGroups(dependencies, startIndex, childrenLists, groups);
        CreateDiagramElements(diagramStructureElement, useStandaloneDependencies, childrenLists, standaloneDependenciesBoundry);
        if (standaloneDependenciesBoundry?.Elements.Any() == true)
        {
            diagramStructureElement.Elements.Add(standaloneDependenciesBoundry);
        }
    }

    private static void CreateDiagramElements(DiagramStructureElement diagramStructureElement, bool useStandaloneDependencies, List<(string, List<Dependency>)> childrenLists, ContainerBoundary? standaloneDependenciesBoundry)
    {
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
            FillDiagramStructureElementWithRealBoundaries(containerBoundary, children, groupStartIndex, useStandaloneDependencies: false);
            diagramStructureElement.Elements.Add(containerBoundary);
        }
    }

    private static void BuildGroups(IEnumerable<Dependency> dependencies, int startIndex, List<(string, List<Dependency>)> childrenLists, Dictionary<string, List<Dependency>> groups)
    {
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
    }
}
