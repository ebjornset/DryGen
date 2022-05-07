using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.SonarCloud
{
    public class GitHubActionsSetupJavaStep : GitHubActionsStep
    {
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- name: Setup JDK v 11 (for SonarCloud)");
            using (writer.Indent())
            {
                writer.WriteLine("uses: actions/setup-java@v1");
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine("java-version: 1.11");
                }
            }
        }
    }
}
