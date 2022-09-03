using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
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

    public static int Main() => Execute<Build>(x => x.Clean, x => x.UTests, x => x.ITests, x => x.Docs, x => x.Specs);

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
    string Description;
    internal bool IsVersionTag;
#pragma warning disable S1075 // URIs should not be hardcoded
    private readonly string ProjectUrlInNugetPackage = "https://docs.drygen.net/";
#pragma warning restore S1075 // URIs should not be hardcoded

    internal static AbsolutePath SourceDirectory => RootDirectory / "src";
    internal static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    internal static AbsolutePath SonarQubeCoverageDirectory => RootDirectory / ".sonarqubecoverage";

    internal Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj", "**/TestResults").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
            EnsureCleanDirectory(SonarQubeCoverageDirectory);
        });

    internal Target Init => _ => _
        .Executes(() =>
        {
            var endyear = DateTime.Today.Year;
            var endYearPattern = endyear > 2022 ? $"-{endyear}" : string.Empty;
            Description = "A dotnet tool to generate other representations of a piece of knowlege from one representation.";
            Authors = "Eirik Bjornset";
            Copyright = $"Copyright 2022{endYearPattern} {Authors}";
            IsVersionTag = GitRepository != null && GitRepository.Branch.Contains("refs/tags/v", StringComparison.InvariantCultureIgnoreCase);
            Serilog.Log.Information("Description = {description}", Description);
            Serilog.Log.Information("Copyright = {copyright}", Copyright);
            Serilog.Log.Information("GitRepository = {GitRepository}", GitRepository);
            Serilog.Log.Information("GitRepository.Branch = {GitRepositoryBranch}", GitRepository.Branch);
            Serilog.Log.Information("GitRepository.Tags = {GitRepositoryTags}", GitRepository.Tags);
            Serilog.Log.Information("IsVersionTag = {isVersionTag}", IsVersionTag);
            Serilog.Log.Information("GitVersion.NuGetVersionV2 = {GitVersionNuGetVersionV2}", GitVersion.NuGetVersionV2);
        });

    internal Target Restore => _ => _
       .After(Clean)
       .Executes(() =>
       {
           DotNetRestore(s => s
               .SetProjectFile(Solution));
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
                .SetDescription(Description)
                .EnableNoRestore());
        });

    internal Target UTests => _ => _
        .DependsOn(Compile)
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
            .DependsOn(Compile)
            .DependsOn(Init)
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
                    .SetDescription(Description)
                    .SetRepositoryUrl(GitRepository.ToString())
                    .SetPackageProjectUrl(ProjectUrlInNugetPackage)
                    .SetVersion(GitVersion.NuGetVersionV2));
            });

    internal Target ITests => _ => _
            .DependsOn(Pack)
            .DependsOn(Init)
            .Executes(() =>
            {
                // Install the artifact as a local dotnet tool in the ITests project
                var workingDirectory = Solution.GetProject("DryGen.ITests").Directory;
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

    internal Target Specs => _ => _
            .DependsOn(UTests)
            .DependsOn(ITests)
            .Executes(() =>
            {
                DotNetToolUpdate(c => c
                    .SetGlobal(true)
                    .SetPackageName("SpecFlow.Plus.LivingDoc.CLI")
                    );
                var subprojectNames = new Dictionary<string, string> { { "UTests", "Unit tests" }, { "ITests", "Integration tests" } };
                foreach (var testProject in new[] { "UTests", "ITests" })
                {
                    var arguments = new[] {
                        "test-assembly",
                        "--title",
                        $"\"DryGen {subprojectNames[testProject]}\"",
                        $"./**/*.{testProject}/bin/{Configuration}/netcoreapp3.1/*.{testProject}.dll",
                        "-t",
                        $"./**/*.{testProject}/bin/{Configuration}/netcoreapp3.1/.specflow.livingdoc.data.json",
                        "--output",
                        $"{DocsDirectory}/about/specs/drygen-{testProject.ToLowerInvariant()}.html",
                    };
                    ProcessTasks.StartProcess("livingdoc", string.Join(' ', arguments), logOutput: true, logInvocation: true);
                }
            });

    internal Target Docs => _ => _
        .DependsOn(Compile)
        .DependsOn(Specs)
        .Executes(() =>
        {
            DotNetRun(c => c
                .SetProjectFile(Solution.GetProject("DryGen.Docs"))
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
       .DependsOn(Specs)
       .DependsOn(Pack)
       .OnlyWhenDynamic(() => IsVersionTag)
       .Requires(() => NuGetSource)
       .Requires(() => NuGetApiKey)
       .Requires(() => Configuration.Equals(Configuration.Release))
       .Executes(() =>
       {
           var packages = GlobFiles(ArtifactsDirectory, "*.nupkg");
           Assert.NotEmpty(packages.ToList());
           DotNetNuGetPush(s => s
            .SetApiKey(NuGetApiKey)
            .EnableSkipDuplicate()
            .SetSource(NuGetSource)
            .EnableNoSymbols()
            .CombineWith(packages,
                (v, path) => v.SetTargetPath(path)));
       });

    internal Target GlobalTool => _ => _
        .DependsOn(Init)
        .DependsOn(Pack)
        .Executes(() =>
        {
            try
            {
                DotNetToolUninstall(c => c
                    .SetGlobal(true)
                    .SetPackageName("dry-gen")
                    .SetProcessLogOutput(false)
                    );
            }
            catch
            {
                // Noop, to prevent the build from stopping when dry-gen is not installed as a global tool (yet)
            }
            var workingDirectory = Solution.GetProject("DryGen.ITests").Directory;
            DotNetToolInstall(c => c
                .SetGlobal(true)
                .SetPackageName("dry-gen")
                .AddSources(ArtifactsDirectory)
                .SetProcessWorkingDirectory(workingDirectory)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetConfigFile(Path.Combine(Path.Combine(workingDirectory, "Properties"), "NuGet.Config"))
                );
        });

    private static string DocsDirectory => Path.Combine(RootDirectory, "docs").Replace("\\", "/");
}
