using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.PowerShell;
using Nuke.Common.Utilities.Collections;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace DryGen.Build;

[ExcludeFromCodeCoverage]
[ShutdownDotNetAfterServerBuild]
public partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Clean, x => x.UTests, x => x.ITests, x => x.Docs);

    [Parameter("Configuration to build - Default is 'Release'")]
    internal readonly Configuration Configuration = Configuration.Release;

    [Parameter("The Nuget source url")]
    internal readonly string NuGetSource = "https://api.nuget.org/v3/index.json";

    [Parameter("The api key to use when pushing to Nuget")]
    [Secret]
    internal readonly string NuGetApiKey;

    [Solution] internal readonly Solution Solution;
    [GitRepository] internal readonly GitRepository GitRepository;
    [GitVersion] internal readonly GitVersion GitVersion;

    internal string Copyright;
    internal string Authors;
    private string ToolsDescription;
    private string TemplatesDescription;
    internal bool IsVersionTag;
#pragma warning disable S1075 // URIs should not be hardcoded
    private readonly string ProjectUrlInNugetPackage = "https://docs.drygen.dev/";
#pragma warning restore S1075 // URIs should not be hardcoded

    internal static AbsolutePath SourceDirectory => RootDirectory / "src";
    internal static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    internal static AbsolutePath SonarQubeCoverageDirectory => RootDirectory / ".sonarqubecoverage";

    internal Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj", "**/TestResults").ForEach(x => x.DeleteDirectory());
            ArtifactsDirectory.CreateOrCleanDirectory();
            SonarQubeCoverageDirectory.CreateOrCleanDirectory();
        });

    internal Target Init => _ => _
        .Executes(() =>
        {
            ToolsDescription = "A dotnet tool to generate other representations of a piece of knowlege from one representation.";
            TemplatesDescription = $".Net templates that make getting started with [dry-gen]({ProjectUrlInNugetPackage}) easy.";
            Authors = "Eirik Bjornset";
            Copyright = $"Copyright 2022-{DateTime.Today.Year} {Authors}";
            IsVersionTag = GitRepository != null && (GitRepository.Branch?.Contains("refs/tags/v", StringComparison.InvariantCultureIgnoreCase) ?? false);
            Log.Information("ToolsDescription = '{ToolsDescription}'", ToolsDescription);
            Log.Information("TemplatesDescription = '{TemplatesDescription}'", TemplatesDescription);
            Log.Information("Copyright = '{Copyright}'", Copyright);
            Log.Information("GitRepository = '{GitRepository}'", GitRepository);
            Log.Information("GitRepository.Branch = '{GitRepositoryBranch}'", GitRepository?.Branch);
            Log.Information("GitRepository.Tags = '{GitRepositoryTags}'", GitRepository?.Tags);
            Log.Information("IsVersionTag = '{IsVersionTag}'", IsVersionTag);
            Log.Information("GitVersion.NuGetVersionV2 = '{GitVersionNuGetVersionV2}'", GitVersion.NuGetVersionV2);
        });

    internal Target Restore => _ => _
        .After(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(Solution));
        });

    internal Target Compile => _ => _
        .DependsOn(Restore)
        .DependsOn(Init)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetCopyright(Copyright)
                .SetDescription(ToolsDescription)
                .EnableNoRestore());
        });

    internal Target UTests => _ => _
        .DependsOn(Compile)
        .Before(Pack)
        .Executes(() =>
        {
            DotNetTest(c => c
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetDataCollector("XPlat Code Coverage")
                .CombineWith(SourceDirectory.GlobFiles("**/*.UTests.csproj"), (settings, path) =>
                    settings.SetProjectFile(path)), degreeOfParallelism: 4, completeOnFailure: true);
        });

    internal Target Pack => _ => _
             .DependsOn(Init)
             .DependsOn(Compile)
             .Executes(() =>
             {
                 DotNetPack(s => s
                     .SetProject(Solution.GetProject("DryGen"))
                     .SetOutputDirectory(ArtifactsDirectory)
                     .SetConfiguration(Configuration)
                     .EnableNoBuild()
                     .EnableContinuousIntegrationBuild() // Necessary for deterministic builds
                     .SetAuthors(Authors)
                     .SetCopyright(Copyright)
                     .SetDescription(ToolsDescription)
                     .SetRepositoryUrl(GitRepository?.ToString())
                     .SetPackageProjectUrl(ProjectUrlInNugetPackage)
                     .SetVersion(GitVersion.NuGetVersionV2));
                 // Regenerate the .config/dotnet-tools.json in the templates with the latest version of dry-gen
                 var templateProjectDirectory = Solution.GetProject("DryGen.Templates").Directory;
                 foreach (var templateProject in Directory.GetDirectories(Path.Combine(templateProjectDirectory, "templates")))
                 {
                     var workingDirectory = Path.Combine(templateProjectDirectory, "templates", templateProject);
                     DotNet("new tool-manifest --force", workingDirectory: workingDirectory, logOutput: true, logInvocation: true);
                     DotNetToolUpdate(c => c
                         .SetPackageName("dry-gen")
                         .AddSources(ArtifactsDirectory)
                         .SetProcessWorkingDirectory(workingDirectory)
                         .SetVersion(GitVersion.NuGetVersionV2)
                         .SetConfigFile(Path.Combine(templateProjectDirectory, "Properties", "NuGet.Config"))
                                     );
                 }
                 // Rebuild the templates before we create the package, to use the newly generated .config/dotnet-tools.json in the templates
                 DotNetBuild(s => s
                     .SetProjectFile(Solution.GetProject("DryGen.Templates"))
                     .SetOutputDirectory(ArtifactsDirectory)
                     .SetConfiguration(Configuration)
                     .SetAssemblyVersion(GitVersion.AssemblySemVer)
                     .SetFileVersion(GitVersion.AssemblySemFileVer)
                     .SetInformationalVersion(GitVersion.InformationalVersion)
                     .SetCopyright(Copyright)
                     .SetDescription(ToolsDescription)
                     .SetProcessEnvironmentVariable("DryGenTemplatesRunAsTool", "some value")
                     .EnableNoRestore());
                 DotNetPack(s => s
                     .SetProject(Solution.GetProject("DryGen.Templates"))
                     .SetOutputDirectory(ArtifactsDirectory)
                     .SetConfiguration(Configuration)
                     .EnableNoBuild()
                     .EnableContinuousIntegrationBuild() // Necessary for deterministic builds
                     .SetAuthors(Authors)
                     .SetCopyright(Copyright)
                     .SetDescription(TemplatesDescription)
                     .SetRepositoryUrl(GitRepository?.ToString())
                     .SetPackageProjectUrl(ProjectUrlInNugetPackage)
                     .SetVersion(GitVersion.NuGetVersionV2));
             });

    internal Target ITests => _ => _
             .DependsOn(Pack)
             .DependsOn(Init)
             .Before(Docs)
             .Executes(() =>
             {
                 // Install the artifact as a local dotnet tool in the ITests project
                 var workingDirectory = GetProject("develop", "DryGen.ITests").Directory;
                 DotNet("new tool-manifest --force", workingDirectory: workingDirectory, logOutput: true, logInvocation: true);
                 DotNetToolUpdate(c => c
                     .SetPackageName("dry-gen")
                     .AddSources(ArtifactsDirectory)
                     .SetProcessWorkingDirectory(workingDirectory)
                     .SetVersion(GitVersion.NuGetVersionV2)
                     .SetConfigFile(Path.Combine(Path.Combine(workingDirectory, "Properties"), "NuGet.Config"))
                                 );
                 // Run the ITests in "dotnet tool" mode
                 DotNetTest(c => c
                     .SetConfiguration(Configuration)
                     .EnableNoBuild()
                     .SetDataCollector("XPlat Code Coverage")
                     .CombineWith(SourceDirectory.GlobFiles("**/*.ITests.csproj"), (settings, path) =>
                         settings
                             .SetProjectFile(path)
                                 .SetProcessEnvironmentVariable("DryGen.ITests.ToolInvocationSteps.RunAsTool", "some value")
                                 .SetProcessEnvironmentVariable("DryGen.ITests.ToolInvocationSteps.WorkingDirectory", workingDirectory))
                     , degreeOfParallelism: 4, completeOnFailure: true);
             });

    internal Target Docs => _ => _
        .DependsOn(Init)
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetRun(c => c
                .SetProjectFile(GetProject("develop", "DryGen.Docs"))
                .SetConfiguration(Configuration)
                .SetFramework("net6.0")
                .SetApplicationArguments($"--root-directory {RootDirectory}")
                .EnableNoBuild()
                .SetNoLaunchProfile(true)
                     );
        });

    internal Target Push => _ => _
        .DependsOn(UTests)
        .DependsOn(ITests)
        .DependsOn(Docs)
        .DependsOn(Pack)
        .OnlyWhenDynamic(() => IsVersionTag)
        .Requires(() => NuGetSource)
        .Requires(() => NuGetApiKey)
        .Requires(() => Configuration.Equals(Configuration.Release))
        .Executes(() =>
        {
            var packages = ArtifactsDirectory.GlobFiles("*.nupkg");
            Assert.NotEmpty(packages.ToList());
            DotNetNuGetPush(s => s
            .SetApiKey(NuGetApiKey)
            .EnableSkipDuplicate()
            .SetSource(NuGetSource)
            .EnableNoSymbols()
            .CombineWith(packages,
                (v, path) => v.SetTargetPath(path)));
        });

    internal Target Dev_InstallGlobalTool => _ => _
        .DependsOn(Init)
        .DependsOn(Pack)
        .Executes(() =>
        {
            try
            {
                DotNetToolUninstall(c => c.SetGlobal(true).SetPackageName("dry-gen").SetProcessLogOutput(false));
            }
            catch
            {
                // Noop, to prevent the build from stopping when dry-gen is not installed as a global tool (yet)
            }
            var workingDirectory = GetProject("develop", "DryGen.ITests").Directory;
            DotNetToolInstall(c => c
                .SetGlobal(true)
                .SetPackageName("dry-gen")
                .AddSources(ArtifactsDirectory)
                .SetProcessWorkingDirectory(workingDirectory)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetConfigFile(Path.Combine(Path.Combine(workingDirectory, "Properties"), "NuGet.Config"))
                             );
        });

    internal Target Dev_InstallTemplates => _ => _
        .DependsOn(Init)
        .DependsOn(Pack)
        .Executes(() =>
        {
            try
            {
                DotNet("new uninstall dry-gen.templates", logOutput: false, logInvocation: false);
            }
            catch
            {
                // Noop, to prevent the build from stopping when dry-gen.templates is not installed as a template (yet)
            }
            var toolsPackageName = Path.Combine(ArtifactsDirectory, $"dry-gen.templates.{GitVersion.NuGetVersionV2}.nupkg");
            DotNet($"new install \"{toolsPackageName}\"", logOutput: true, logInvocation: true);
        });

    internal Target Dev_StartDocsSite => _ => _
        .DependsOn(Init)
        .Executes(() =>
        {
            DocsSiteDirectory.CreateOrCleanDirectory();
            PowerShellTasks.PowerShell(
                arguments: "Start-Process -FilePath \"bundle\" -ArgumentList \"exec jekyll serve --drafts --incremental\"",
                workingDirectory: DocsDirectory
                                      );
        });

    private Project GetProject(string solutionFolderName, string projectName)
    {
        var solutionFolder = Solution.GetSolutionFolder(solutionFolderName) ?? throw new ArgumentException($"Solution folder '{solutionFolderName}' not found", nameof(solutionFolderName));
        return solutionFolder.GetProject(projectName) ?? throw new ArgumentException($"Project '{projectName}' noot found in solution folder '{solutionFolderName}'", nameof(projectName));
    }

    private static AbsolutePath DocsDirectory => RootDirectory / "docs";
    private static AbsolutePath DocsSiteDirectory => DocsDirectory / "_site";
}