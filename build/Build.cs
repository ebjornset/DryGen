using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.PowerShell;
using Nuke.Common.Tools.SonarScanner;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace DryGen.Build;

[ExcludeFromCodeCoverage(Justification = "Only used for building the system, and is almost impossible to test")]
[ShutdownDotNetAfterServerBuild]
public partial class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Default);

    [Parameter("Configuration to build - Default is 'Release'")]
    internal readonly Configuration Configuration = Configuration.Release;

    [Parameter("The Nuget source url", List = false)]
    internal readonly string NuGetSource = "https://api.nuget.org/v3/index.json";

    [Parameter("The api key to use when pushing to Nuget")]
    [Secret]
    internal readonly string NuGetApiKey;

    [Parameter("The token to use when running SonarClound analyzis")]
    [Secret]
    internal readonly string SonarToken;

    [Solution] internal readonly Solution Solution;
    [GitRepository] internal readonly GitRepository GitRepository;
    [GitVersion] internal readonly GitVersion GitVersion;

    internal string Copyright;
    internal string Authors;
    private const string ToolsDescription = "A dotnet tool to generate other representations of a piece of knowlege from one representation.";
    private string TemplatesDescription;
#pragma warning disable S1075 // URIs should not be hardcoded
    private readonly string ProjectUrlInNugetPackage = "https://docs.drygen.dev/";
#pragma warning restore S1075 // URIs should not be hardcoded

    internal static AbsolutePath SourceDirectory => RootDirectory / "src";
    internal static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    internal static AbsolutePath SonarQubeCoverageDirectory => RootDirectory / ".sonarqubecoverage";

    internal Target Default => _ => _
        .DependsOn(Clean)
        .DependsOn(UTests)
        .DependsOn(ITests)
        .DependsOn(GenerateDocs)
        ;

#pragma warning disable CA1822 // Mark members as static
    internal Target Clean => _ => _
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj", "**/TestResults").ForEach(x => x.DeleteDirectory());
            ArtifactsDirectory.CreateOrCleanDirectory();
            SonarQubeCoverageDirectory.CreateOrCleanDirectory();
        });
#pragma warning restore CA1822 // Mark members as static

    internal Target Init => _ => _
        .Executes(() =>
        {
            TemplatesDescription = $".Net templates that make getting started with [dry-gen]({ProjectUrlInNugetPackage}) easy.";
            Authors = "Eirik Bjornset";
            Copyright = $"Copyright 2022-{DateTime.Today.Year} {Authors}";
            Log.Information(new StringBuilder().AppendLine()
            .AppendLine("ToolsDescription = '{ToolsDescription}'")
            .AppendLine("TemplatesDescription = '{TemplatesDescription}")
            .AppendLine("Copyright = '{Copyright}'")
            .AppendLine("GitRepository = '{GitRepository}'")
            .AppendLine("GitRepository.Branch = '{GitRepositoryBranch}'")
            .AppendLine("GitRepository.Tags = '{GitRepositoryTags}'")
            .AppendLine("GitRepository.IsOnVersionTag = '{GitRepositoryIsOnVersionTag}'")
            .AppendLine("GitRepository.IsOnMainBranch = '{GitRepositoryIsOnMainBranch}'")
            .AppendLine("GitVersion.NuGetVersionV2 = '{GitVersionNuGetVersionV2}'")
            .AppendLine("GitVersion.MajorMinorPatch = '{GitVersionMajorMinorPatch}'")
            .ToString(), ToolsDescription, TemplatesDescription, Copyright, GitRepository, GitRepository?.Branch, GitRepository?.Tags, GitRepository?.IsOnVersionTag(), GitRepository?.IsOnMainBranch(), GitVersion.NuGetVersionV2, GitVersion.MajorMinorPatch);
        });

    internal Target Restore => _ => _
        .After(Clean)
        .DependsOn(Init)
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(Solution));
        });

    internal Target Compile => _ => _
        .DependsOn(Restore)
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
             .After(UTests)
             .Produces(ArtifactsDirectory / "*.nupkg")
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
                         .SetPackageName(DrygenPackageName)
                         .AddSources(ArtifactsDirectory)
                         .SetProcessWorkingDirectory(workingDirectory)
                         .SetVersion(GitVersion.NuGetVersionV2)
                         .SetConfigFile(Path.Combine(templateProjectDirectory, "Properties", "NuGet.Config"))
                                     );
                 }
                 // Rebuild the templates before we create the package, to use the newly generated .config/dotnet-tools.json in the templates
                 DotNetBuild(s => s
                     .SetProjectFile(Solution.GetProject("DryGen.Templates"))
                     .SetOutputDirectory(TemporaryDirectory)
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
             .Executes(() =>
             {
                 // Install the artifact as a local dotnet tool in the ITests project
                 var workingDirectory = GetProject("develop", "DryGen.ITests").Directory;
                 DotNet("new tool-manifest --force", workingDirectory: workingDirectory, logOutput: true, logInvocation: true);
                 DotNetToolUpdate(c => c
                     .SetPackageName(DrygenPackageName)
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

    internal Target GenerateDocs => _ => _
        .DependsOn(Compile)
        .After(ITests)
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

    internal Target BuildDocs => _ => _
        .After(GenerateDocs)
        .Before(Push)
        .Executes(() =>
        {
            ProcessTasks.StartProcess("bundle", arguments: string.Join(' ', "install", "--jobs=4", "--retry=3"), workingDirectory: DocsDirectory.ToString(), logOutput: true, logInvocation: true).AssertZeroExitCode();
            ProcessTasks.StartProcess("bundle", arguments: string.Join(' ', "exec", "jekyll", "build"), workingDirectory: DocsDirectory.ToString(), logOutput: true, logInvocation: true).AssertZeroExitCode();
        });

    internal Target Push => _ => _
        .DependsOn(Default)
        .DependsOn(GitWorkingCopyShouldBeClean)
        .Requires(() => GitRepository.IsOnVersionTag())
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
                (v, path) => v.SetTargetPath(path)),
            degreeOfParallelism: 2
            );
        });

#pragma warning disable CA1822 // Mark members as static
    internal Target GitWorkingCopyShouldBeClean => _ => _
        .Unlisted()
        .After(GenerateDocs)
        .After(BuildDocs)
        .Before(Push)
        .Executes(() =>
        {
            LogChangesAndFailIfGitWorkingCopyIsNotClean();
        });
#pragma warning restore CA1822 // Mark members as static

    internal Target SonarCloudBegin => _ => _
        .Unlisted()
        .Requires(() => SonarToken)
        .Before(Restore)
        .Executes(() =>
        {
            SonarScannerTasks.SonarScannerBegin(s => s
                .SetProjectKey("ebjornset_DryGen")
                .SetOrganization("ebjornset")
                .SetVersion(GitVersion.MajorMinorPatch)
                .SetServer("https://sonarcloud.io")
                .SetToken(SonarToken)
            );
        });

    internal Target SonarCloudEnd => _ => _
        .Unlisted()
        .Requires(() => SonarToken)
        .After(UTests)
        .After(ITests)
        .Executes(() =>
        {
            SonarScannerTasks.SonarScannerEnd(s => s
                .SetToken(SonarToken)
            );
        });

    private Project GetProject(string solutionFolderName, string projectName)
    {
        var solutionFolder = Solution.GetSolutionFolder(solutionFolderName) ?? throw new ArgumentException($"Solution folder '{solutionFolderName}' not found", nameof(solutionFolderName));
        return solutionFolder.GetProject(projectName) ?? throw new ArgumentException($"Project '{projectName}' noot found in solution folder '{solutionFolderName}'", nameof(projectName));
    }

    private static void LogChangesAndFailIfGitWorkingCopyIsNotClean()
    {
        if (!GitTasks.GitHasCleanWorkingCopy())
        {
            const string message = "Git working copy is not clean!";
            var sb = new StringBuilder().AppendLine(message).AppendLine();
            var gitOutput = GitTasks.Git("status --short", logOutput: false, logInvocation: false);
            foreach (var line in gitOutput)
            {
                sb.AppendLine(line.Text);
            }
            Log.Error(sb.ToString());
            Assert.Fail(message);
        }
    }

    private static AbsolutePath DocsDirectory => RootDirectory / "docs";
    private static AbsolutePath DocsSiteDirectory => DocsDirectory / "_site";
    private const string DrygenPackageName = "dry-gen";
}