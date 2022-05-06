using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions
{
    public class GitHubActionsSetupRubyStep : GitHubActionsStep
    {
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- name: Setup Ruby v 3.1 (for docs generation with Jekyll)");
            using (writer.Indent())
            {
                writer.WriteLine("uses: ruby/setup-ruby@v1");
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine("ruby-version: 3.1");
                }
            }
        }
    }
}
