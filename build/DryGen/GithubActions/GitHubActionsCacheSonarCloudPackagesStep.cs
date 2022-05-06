using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions
{
    public class GitHubActionsCacheSonarCloudPackagesStep : GitHubActionsStep
    {
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- name: Cache SonarCloud packages");
            using (writer.Indent())
            {
                writer.WriteLine("uses: actions/cache@v2");
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine(@"path: ~/sonar/cache");
                    writer.WriteLine("key: ${{ runner.os }}-sonar");
                    writer.WriteLine("restore-keys: ${{ runner.os }}-sonar");
                }
            }
        }
    }
}
