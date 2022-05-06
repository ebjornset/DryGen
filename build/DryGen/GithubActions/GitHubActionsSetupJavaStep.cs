using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions
{
    public class GitHubActionsSetupJavaStep : GitHubActionsStep
    {
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- name: Setup JDK v 11 (for SonarCloud)");
            writer.WriteLine("  uses: actions/setup-java@v1");
            using (writer.Indent())
            {
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine("java-version: 1.11");
                }
            }
        }
    }
}
