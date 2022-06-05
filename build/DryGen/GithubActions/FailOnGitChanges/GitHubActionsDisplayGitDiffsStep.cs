using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.FailOnGitChanges
{
    public class GitHubActionsDisplayGitDiffsStep : GitHubActionsStep
    {
        internal static readonly string StepName = "Display any git diffs";
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"- name: {StepName}");
            using (writer.Indent())
            {
                writer.WriteLine("if: steps.get_changes.outputs.changed != 0");
                writer.WriteLine("run: git diff HEAD");
            }
        }
    }
}
