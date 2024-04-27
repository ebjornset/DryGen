using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.PowerShell;
using Serilog;
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
        .After(GenerateDocs)
        .Executes(() =>
        {
            DocsGeneratedDirectory.CreateDirectory();
            DocsMergedDirectory.CreateDirectory();
            DocsSiteDirectory.CreateDirectory();
            WatchAndRun(DocsTemplatesDirectory, GetProject("develop", "DryGen.Docs").Directory / "bin" / "Release" / "net6.0" / "DryGen.Docs", "--root-directory", RootDirectory);
            WatchAndRun(DocsGeneratedDirectory, "powershell", "Copy-Item", DocsGeneratedDirectory / "*", DocsMergedDirectory, "-Recurse", "-Force");
            WatchAndRun(DocsSrcDirectory, "powershell", "Copy-Item", DocsSrcDirectory / "*", DocsMergedDirectory, "-Recurse", "-Force");
            WatchAndRun(DocsMergedDirectory, "docfx", "build", DocsMergedDirectory / "docfx.json");
            PowerShellTasks.PowerShell(
                arguments: "Start-Process -FilePath docfx -ArgumentList \"serve --port " + DocsPort + " --open-browser\"",
                workingDirectory: DocsSiteDirectory);
        });

    private static void WatchAndRun(string path, string command, params string[] arguments)
    {
        var sb = new StringBuilder()
            .Append("Start-Process -FilePath powershell -ArgumentList ")
            .Append('"').Append(BuildDirectory / "watch.ps1").Append("\", ")
            .Append("\" -Path ").Append(path).Append("\", ")
            .Append("\" -Command ").Append(command).Append("\", ")
            .Append("\" -Arguments ").Append(string.Join(',', arguments)).Append('"')
            ;
        PowerShellTasks.PowerShell(sb.ToString());
    }
}
