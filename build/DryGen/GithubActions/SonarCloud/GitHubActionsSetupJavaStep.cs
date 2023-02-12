using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.SonarCloud
{
    public class GitHubActionsSetupJavaStep : GitHubActionsStep
    {
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- name: Setup JDK (for SonarCloud)");
            using (writer.Indent())
            {
                writer.WriteLine("uses: actions/setup-java@v3");
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine("distribution: zulu");
                    writer.WriteLine("java-version: 17");
                }
            }
        }
    }
}
