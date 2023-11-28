using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.GhPages;

public class GitHubActionsPrepareGeneratedDocsForDeploymentOnBranchGhPagesStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Prepare generated docs for deployment on branch gh-pages");
        using (writer.Indent())
        {
            writer.WriteLine("uses: peaceiris/actions-gh-pages@v3");
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine("github_token: ${{ secrets.GITHUB_TOKEN }}");
                writer.WriteLine("publish_dir: ./_site");
            }
            writer.WriteLine("# If the Github Pages for the repository is configured correctly");
            writer.WriteLine("# the gh-pages branch should be published automatically within a minute or so...");
        }
    }
}