using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DocFX;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.Pwsh;
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

	[Parameter("Configuration to build. NB! Default is 'Release' both for local and server build, so GenerateDocs always uses the same source.")]
	internal readonly Configuration Configuration = Configuration.Release;

	[Parameter("The version number for the version tag. Must be on the format a[.b[.c[-<prerelease name>.d]]], where a, b, c and d are integers and <prerelease name> describes the prerelease, e.g. 'alpha', 'beta' or 'prerelease'", List = false)]
	internal readonly string Version;

	[Parameter("The Nuget source url", List = false)]
	internal readonly string NuGetSource = "https://api.nuget.org/v3/index.json";

	[Parameter("The api key to use when pushing to Nuget", List = false)]
	[Secret]
	internal readonly string NuGetApiKey;

	[Parameter("The token to use when running SonarClound analyzis", List = false)]
	[Secret]
	internal readonly string SonarToken;

	[Solution] internal readonly Solution Solution;
	[GitRepository] internal readonly GitRepository GitRepository;
	[GitVersion] internal readonly GitVersion GitVersion;

	internal Target Default => _ => _
		.DependsOn(Clean)
		.DependsOn(UnitTests)
		.DependsOn(IntegrationTests)
		.DependsOn(GenerateDocs)
		.DependsOn(BuildDocs)
		;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
	internal Target Clean => _ => _
		.Executes(() =>
		{
			SourceDirectory.GlobDirectories("**/bin", "**/obj", "**/TestResults").ForEach(x => x.DeleteDirectory());
			ArtifactsDirectory.CreateOrCleanDirectory(recurse: true);
			DocsGeneratedDirectory.CreateOrCleanDirectory(recurse: true);
			DocsMergedDirectory.CreateOrCleanDirectory(recurse: true);
			DocsSiteDirectory.CreateOrCleanDirectory(recurse: true);
		});
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
#pragma warning restore CA1822 // Mark members as static

	internal Target Init => _ => _
		.Executes(() =>
		{
			TemplatesDescription = $".Net templates that make getting started with [dry-gen]({ProjectUrlInNugetPackage}) easy.";
			Copyright = $"Copyright 2022-{DateTime.Today.Year} {Authors}";
			Log.Information(new StringBuilder().AppendLine()
			.AppendLine("ToolsDescription = '{ToolsDescription}'")
			.AppendLine("TemplatesDescription = '{TemplatesDescription}")
			.AppendLine("Copyright = '{Copyright}'")
			.AppendLine("GitRepository = '{GitRepository}'")
			.AppendLine("GitRepository.Branch = '{GitRepositoryBranch}'")
			.AppendLine("GitRepository.IsOnVersionTag = '{GitRepositoryIsOnVersionTag}'")
			.AppendLine("GitRepository.IsOnMainBranch = '{GitRepositoryIsOnMainBranch}'")
			.AppendLine("GitVersion.NuGetVersionV2 = '{GitVersionNuGetVersionV2}'")
			.AppendLine("GitVersion.MajorMinorPatch = '{GitVersionMajorMinorPatch}'")
			.ToString(), ToolsDescription, TemplatesDescription, Copyright, GitRepository, GitRepository?.Branch, GitRepository?.IsOnVersionTag(), GitRepository?.IsOnMainBranch(), GitVersion.NuGetVersionV2, GitVersion.MajorMinorPatch);
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

	internal Target UnitTests => _ => _
		.DependsOn(Compile)
		.Produces(UnitTestsResultsDirectory)
		.Executes(() =>
		{
			foreach (var dotNetVersion in DotNetVersions) {
				DotNetTest(c => c
					.SetConfiguration(Configuration)
					.SetFramework(dotNetVersion)
					.EnableNoBuild()
					.SetDataCollector("XPlat Code Coverage;Format=opencover")
					.CombineWith(SourceDirectory.GlobFiles("**/*.UTests.csproj"), (settings, path) =>
						settings.SetProjectFile(path)), degreeOfParallelism: 4, completeOnFailure: true);
			}
		});

	internal Target Pack => _ => _
			 .DependsOn(Compile)
			 .After(UnitTests)
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

	internal Target IntegrationTests => _ => _
			 .DependsOn(Pack)
			 .Produces(IntergrationTestsResultsDirectory)
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
				 foreach (var dotNetVersion in DotNetVersions) {
					DotNetTest(c => c
						.SetConfiguration(Configuration)
						.SetFramework(dotNetVersion)
						.EnableNoBuild()
						.SetDataCollector("XPlat Code Coverage;Format=opencover")
						.CombineWith(SourceDirectory.GlobFiles("**/*.ITests.csproj"), (settings, path) =>
							settings
								.SetProjectFile(path)
									.SetProcessEnvironmentVariable("DryGen.ITests.ToolInvocationSteps.RunAsTool", "some value")
									.SetProcessEnvironmentVariable("DryGen.ITests.ToolInvocationSteps.WorkingDirectory", workingDirectory))
						, degreeOfParallelism: 4, completeOnFailure: true);
				 }
			 });

	internal Target GenerateDocs => _ => _
		.DependsOn(Init)
		.Requires(() => Configuration.Equals(Configuration.Release))
		.After(UnitTests)
		.After(IntegrationTests)
		.Executes(() =>
		{
			DocsGeneratedDirectory.CreateOrCleanDirectory(recurse: true);
			DotNetRun(c => c
				.SetProjectFile(GetProject("develop", "DryGen.Docs"))
				.SetConfiguration(Configuration)
				.SetFramework("net8.0")
				.SetApplicationArguments($"--root-directory {RootDirectory}")
				.EnableNoBuild()
				.SetNoLaunchProfile(true)
			);
		});

	internal Target BuildDocs => _ => _
		.DependsOn(Init)
		.After(GenerateDocs)
		.After(SetupDocfx)
		.After(SetupPwsh)
		.Executes(() =>
		{
			DocsMergedDirectory.CreateOrCleanDirectory(recurse: true);
			DocsSiteDirectory.CreateOrCleanDirectory(recurse: true);
			CopyToMergedDocs(DocsGeneratedDirectory);
			CopyToMergedDocs(DocsSrcDirectory);
			StartProcess("docfx", "build \"" + (DocsMergedDirectory / "docfx.json").ToString() + "\"");
		});

	internal Target PushPackagesToNuget => _ => _
		.Unlisted()
		.DependsOn(Default)
		.DependsOn(BuildDocs)
		.DependsOn(VerifyCleanWorkingCopyAfterBuild)
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

	internal Target TagVersion => _ => _
		.Unlisted()
		.After(Clean)
		.DependsOn(VerifyCleanWorkingCopyBeforeBuild)
		.Requires(() => Configuration.Equals(Configuration.Release))
		.Requires(() => Version)
		.Requires(() => GitRepository.IsOnMainBranch())
		.Requires(() => ProperNextVersionNumber())
		.Requires(() => ReleaseNotesFromToday())
		.Before(Init)
		.Executes(() =>
		{
			GitTasks.Git($"tag {Version.ToVersionTagName()}");
		});

	internal Target PushVersionTag => _ => _
		.Unlisted()
		.DependsOn(TagVersion)
		.DependsOn(Default)
		.DependsOn(VerifyCleanWorkingCopyAfterBuild)
		.Executes(() =>
		{
			GitTasks.Git($"push origin {Version.ToVersionTagName()}");
		});


#pragma warning disable CA1822 // Mark members as static
	internal Target VerifyCleanWorkingCopyBeforeBuild => _ => _
		.Unlisted()
		.Before(Clean)
		.Before(Restore)
		.Executes(() =>
		{
			LogChangesAndFailIfGitWorkingCopyIsNotClean();
		});

	internal Target VerifyCleanWorkingCopyAfterBuild => _ => _
		.Unlisted()
		.After(GenerateDocs)
		.After(BuildDocs)
		.Executes(() =>
		{
			LogChangesAndFailIfGitWorkingCopyIsNotClean();
		});
#pragma warning restore CA1822 // Mark members as static

	internal Target SonarCloudBegin => _ => _
		.Unlisted()
		.Requires(() => SonarToken)
		.After(Clean)
		.After(Init)
		.Before(Restore)
		.Executes(() =>
		{
			SonarScannerTasks.SonarScannerBegin(s => s
				.SetProjectKey("ebjornset_DryGen")
				.SetOrganization("ebjornset")
				.SetVersion(GitVersion.MajorMinorPatch)
				.SetServer("https://sonarcloud.io")
				.SetToken(SonarToken)
				.SetOpenCoverPaths("**/TestResults/**/coverage.opencover.xml")
			);
		});

	internal Target SonarCloudEnd => _ => _
		.Unlisted()
		.DependsOn(SonarCloudBegin)
		.DependsOn(UnitTests)
		.DependsOn(IntegrationTests)
		.Requires(() => SonarToken)
		.Before(GenerateDocs)
		.Executes(() =>
		{
			SonarScannerTasks.SonarScannerEnd(s => s
				.SetToken(SonarToken)
			);
		});


	internal Target SetupPwsh => _ => _
		.DependsOn(Init)
		.Executes(() =>
		{
			DotNetToolInstall(c => c.SetGlobal(true).SetPackageName(PwshPackageName).SetVersion(PwshVersion));
		});

	internal Target SetupDocfx => _ => _
		.DependsOn(Init)
		.Executes(() =>
		{
			DotNetToolInstall(c => c.SetGlobal(true).SetPackageName(DocfxPackageName).SetVersion(DocfxVersion));
		});

	internal Target SetupToolChain => _ => _
		.DependsOn(SetupPwsh)
		.DependsOn(SetupDocfx);

	private Project GetProject(string solutionFolderName, string projectName)
	{
		var solutionFolder = Solution.GetSolutionFolder(solutionFolderName) ?? throw new ArgumentException($"Solution folder '{solutionFolderName}' not found", nameof(solutionFolderName));
		return solutionFolder.GetProject(projectName) ?? throw new ArgumentException($"Project '{projectName}' noot found in solution folder '{solutionFolderName}'", nameof(projectName));
	}

	private bool ProperNextVersionNumber()
	{
		// Here we can be smart and check that Version is the next possible SemVer version number as an improvement
		// For now we just verifies that the git tag does not exist
		var versionTagName = Version.ToVersionTagName();
		var gitOutput = GitTasks.Git("tag --list", logOutput: false, logInvocation: false);
		if (gitOutput.Any(x => string.Equals(x.Text, versionTagName, StringComparison.InvariantCultureIgnoreCase)))
		{
			Log.Error("Version tag '{VersionTagName}' already exists!", versionTagName);
			return false;
		}
		return true;
	}

	private bool ReleaseNotesFromToday()
	{
		var today = DateTime.Today.ToString("yyyy-MM-dd");
		var releaseNotesFileName = $"{today}-v-{Version}.md";
		if (!DocsTemplatesReleaseNotesDirectory.ContainsFile(releaseNotesFileName))
		{
			Log.Error("Release notes '{ReleaseNotesFileName}' is mising!", DocsTemplatesReleaseNotesDirectory / releaseNotesFileName);
			return false;
		}
		return true;
	}

	private static void CopyToMergedDocs(AbsolutePath source)
	{
		PwshTasks.Pwsh(c => c
			.SetCommand("Copy-Item  -Path \"" + (source / "*").ToString() + "\" -Destination \"" + DocsMergedDirectory.ToString() + "\" -Recurse -Force")
		);
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

	private static AbsolutePath SourceDirectory => RootDirectory / "src";
	private static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
	private static AbsolutePath DocsDirectory => RootDirectory / "docs";
	private static AbsolutePath DocsGeneratedDirectory => DocsDirectory / "_generated";
	private static AbsolutePath DocsMergedDirectory => DocsDirectory / "_merged";
	private static AbsolutePath DocsSiteDirectory => DocsDirectory / "_site";
	private static AbsolutePath DocsSrcDirectory => DocsDirectory / "src";
	private static AbsolutePath DocsTemplatesDirectory => DocsDirectory / "templates";
	private static AbsolutePath DocsTemplatesReleaseNotesDirectory => DocsTemplatesDirectory / "releasenotes";
	private static AbsolutePath UnitTestsResultsDirectory => SourceDirectory / "develop" / "DryGen.UTests" / "TestResults";
	private static AbsolutePath IntergrationTestsResultsDirectory => SourceDirectory / "develop" / "DryGen.ITests" / "TestResults";

	private const string Authors = "Eirik Bjornset";
	private const string ToolsDescription = "A dotnet tool to generate other representations of a piece of knowlege from one representation.";
	private const string DrygenPackageName = "dry-gen";
#pragma warning disable S1075 // URIs should not be hardcoded
	private const string ProjectUrlInNugetPackage = "https://docs.drygen.dev/";
#pragma warning restore S1075 // URIs should not be hardcoded
	private string Copyright;
	private string TemplatesDescription;

	private const string PwshPackageName = "Powershell";
	private const string PwshVersion = "7.4.5";
	private const string DocfxPackageName = "docfx";
	private const string DocfxVersion = "2.77.0";
	private readonly string[] DotNetVersions = ["net8.0", "net9.0"];
}