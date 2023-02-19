using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.SonarCloud;

public class GitHubActionsInstallSonarCloudScannerStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Install/Update SonarCloud scanner");
        using (writer.Indent())
        {
            writer.WriteLine("run:");
            using (writer.Indent())
            {
                writer.WriteLine("dotnet tool update dotnet-sonarscanner --global");
            }
        }
    }
}
