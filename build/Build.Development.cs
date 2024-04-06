using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.PowerShell;
using System.IO;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace DryGen.Build;

public partial class Build
{
    internal Target Dev_InstallGlobalTool => _ => _
        .DependsOn(Init)
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
}
