using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.SonarCloud;

public class GitHubActionsEndSonarCloudScanStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: End SonarCloud scan");
        using (writer.Indent())
        {
            writer.WriteLine("run:");
            using (writer.Indent())
            {
                writer.WriteLine("dotnet-sonarscanner end /d:sonar.login=\"${{ secrets.SONAR_TOKEN }}\"");
            }
            writer.WriteLine("env:");
            using (writer.Indent())
            {
                writer.WriteLine("GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}");
            }
        }
    }
}