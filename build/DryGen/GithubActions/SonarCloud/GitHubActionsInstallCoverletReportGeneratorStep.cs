using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.SonarCloud;

public class GitHubActionsInstallCoverletReportGeneratorStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Install/Update Coverlet Report Generator (for SonarCloud)");
        using (writer.Indent())
        {
            writer.WriteLine("run:");
            using (writer.Indent())
            {
                writer.WriteLine("dotnet tool update dotnet-reportgenerator-globaltool --global");
            }
        }
    }
}
