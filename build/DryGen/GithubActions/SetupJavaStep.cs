using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;
using Octokit;

namespace DryGen.GithubActions;

public class SetupJavaStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Setup Java (for SonarCloud)");
        using (writer.Indent())
        {
            writer.WriteLine("uses: actions/setup-java@v4");
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine("distribution: temurin");
                writer.WriteLine("java-version: 20");
                writer.WriteLine("java-package: jre");
            }
        }
    }
}