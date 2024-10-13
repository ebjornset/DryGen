using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Text;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace DryGen.Build;

public partial class Build
{
	[Parameter("The port to use when starting the docs site locally. Default = 8086")]
	internal readonly int DocsPort = 8086;

	private static AbsolutePath BuildDirectory => RootDirectory / "build";

	internal Target Dev_InstallGlobalTool => _ => _
		.DependsOn(Pack)
		.Executes(() =>
		{
			try
			{
				DotNetToolUninstall(c => c.SetGlobal(true).SetPackageName(DrygenPackageName).SetProcessLogOutput(false));
			}
			catch
			{
				// Noop, to prevent the build from stopping when dry-gen is not installed as a global tool (yet)
			}
			var workingDirectory = GetProject("develop", "DryGen.ITests").Directory;
			DotNetToolInstall(c => c
				.SetGlobal(true)
				.SetPackageName(DrygenPackageName)
				.AddSources(ArtifactsDirectory)
				.SetProcessWorkingDirectory(workingDirectory)
				.SetVersion(GitVersion.NuGetVersionV2)
				.SetConfigFile(Path.Combine(Path.Combine(workingDirectory, "Properties"), "NuGet.Config"))
							 );
		});

	internal Target Dev_InstallTemplates => _ => _
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
		.After(SetupDocfx)
		.After(BuildDocs)
		.Executes(() =>
		{
			DocsGeneratedDirectory.CreateDirectory();
			DocsMergedDirectory.CreateDirectory();
			DocsSiteDirectory.CreateDirectory();
			WatchAndRun(DocsTemplatesDirectory, GetProject("develop", "DryGen.Docs").Directory / "bin" / Configuration / "net8.0" / "DryGen.Docs", "--root-directory", RootDirectory);
			WatchAndCopy(DocsGeneratedDirectory, DocsMergedDirectory);
			WatchAndCopy(DocsSrcDirectory, DocsMergedDirectory);
			WatchAndRun(DocsMergedDirectory, "docfx", "build", DocsMergedDirectory / "docfx.json");
			StartProcess("docfx", "serve --open-browser --port " + DocsPort, DocsSiteDirectory);
		});

	private static void WatchAndRun(string path, string command, params string[] arguments)
	{
		var sb = new StringBuilder()
			.Append("-Command ")
			.Append('"').Append(BuildDirectory / "watch.ps1")
			.Append("  -Path ").Append(path)
			.Append(" -Command ").Append(command)
			.Append(" -RunAtStartup")
			.Append(" -Arguments ").Append(string.Join(',', arguments)).Append('"')
			;
		StartProcess("pwsh", sb.ToString());
	}

	private static void WatchAndCopy(string path, string destination)
	{
		var sb = new StringBuilder()
			.Append("-Command ")
			.Append('"').Append(BuildDirectory / "watch-and-copy.ps1")
			.Append("  -Path ").Append(path)
			.Append(" -Destination ").Append(destination)
			.Append(" -RunAtStartup\"")
			;
		StartProcess("pwsh", sb.ToString());
	}

	private static void StartProcess(string fileName, string arguments, string workingDirectoy = null)
	{
		var processStartInfo = new ProcessStartInfo
		{
			FileName = fileName,
			Arguments = arguments,
			WindowStyle = ProcessWindowStyle.Minimized,
			UseShellExecute = true,
		};
		Log.Debug("{FileName} {Arguments}", fileName, arguments);
		if (workingDirectoy != null)
		{
			processStartInfo.WorkingDirectory = workingDirectoy;
			Log.Debug("@{WorkingDirectoy}", workingDirectoy);
		}
		Process.Start(processStartInfo);
	}
}
