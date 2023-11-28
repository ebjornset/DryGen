using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.SonarCloud;

public class GitHubActionsRunCoverletReportGeneratorStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Run Coverlet Report Generator (for SonarCloud)");
        using (writer.Indent())
        {
            writer.WriteLine("run:");
            using (writer.Indent())
            {
                writer.WriteLine("reportgenerator \"-reports:**/TestResults/**/coverage.cobertura.xml\" \"-targetdir:.sonarqubecoverage\" \"-reporttypes:SonarQube\"");
            }
        }
    }
}