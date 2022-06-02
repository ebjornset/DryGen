using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.FailOnGitChanges
{
    public class GitHubActionsListGitChangesStep : GitHubActionsStep
    {
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- name: List any git changes");
            using (writer.Indent())
            {
                writer.WriteLine("if: steps.get_changes.outputs.changed != 0");
                writer.WriteLine("run: git status --porcelain");
            }
        }
    }
}
