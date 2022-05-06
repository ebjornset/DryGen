using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions
{
    public class GitHubActionsGenerateDocsWithJekyllStep : GitHubActionsStep
    {
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- name: Generate docs with Jekyll");
            writer.WriteLine("  uses: limjh16/jekyll-action-ts@v2");
            using (writer.Indent())
            {
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine("enable_cache: true");
                    writer.WriteLine("jekyll_src: ./docs");
                }
            }
        }
    }
}
