using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.SonarCloud;

public class GitHubActionsBeginSonarCloudScanStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Begin SonarCloud scan");
        using (writer.Indent())
        {
            writer.WriteLine("run:");
            using (writer.Indent())
            {
                writer.WriteLine("dotnet-sonarscanner begin /k:\"ebjornset_DryGen\" /o:\"ebjornset\" /d:sonar.token=\"${{ secrets.SONAR_TOKEN }}\" /d:sonar.host.url=\"https://sonarcloud.io\" /d:sonar.coverageReportPaths=\"./.sonarqubecoverage/SonarQube.xml\"");
            }
        }
    }
}