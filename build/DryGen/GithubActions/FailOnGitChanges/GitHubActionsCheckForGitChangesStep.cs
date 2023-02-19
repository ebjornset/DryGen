using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.FailOnGitChanges;

public class GitHubActionsCheckForGitChangesStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Check if there are any git changes");
        using (writer.Indent())
        {
            writer.WriteLine("id: get_changes");
            writer.WriteLine("run: echo \"::set-output name=changed::$(git status --porcelain | wc -l)\"");
        }
    }
}
