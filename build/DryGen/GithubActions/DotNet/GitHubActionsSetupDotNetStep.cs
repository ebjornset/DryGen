using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.DotNet
{
    public class GitHubActionsSetupDotNetStep : GitHubActionsStep
    {
        public string Version { get; init; }

        public GitHubActionsSetupDotNetStep(string version)
        {
            Version = version;
        }

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"- name: Setup .Net {Version}");
            using (writer.Indent())
            {
                writer.WriteLine("uses: actions/setup-dotnet@v1");
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine($"dotnet-version: {Version}");
                }
            }
        }
    }
}
