using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.FailOnGitChanges
{
    public class GitHubActionsFailOnGitChangesStep : GitHubActionsStep
    {
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- name: Fail if there are any git changes");
            using (writer.Indent())
            {
                writer.WriteLine("if: steps.get_changes.outputs.changed != 0");
                writer.WriteLine("uses: actions/github-script@v3");
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine("script: |");
                    using (writer.Indent())
                    {
                        writer.WriteLine($"core.setFailed('Git modifications found after <nuke>. Maybe <nuke [docs]> was omitted before commit? Check the output from the step \"{GitHubActionsDisplayGitDiffsStep.StepName}\" and/or \"{GitHubActionsListGitChangesStep.StepName}\" for details about the detected git changes.')");
                    }
                }
            }
        }
    }
}
