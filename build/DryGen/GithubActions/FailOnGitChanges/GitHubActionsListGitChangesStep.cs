using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.FailOnGitChanges
{
    public class GitHubActionsListGitChangesStep : GitHubActionsStep
    {
        internal static readonly string StepName = "List any git changes";
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"- name: {StepName}");
            using (writer.Indent())
            {
                writer.WriteLine("if: steps.get_changes.outputs.changed != 0");
                writer.WriteLine("run: git status --porcelain");
            }
            writer.WriteLine($"- name: Read file contents (for debuging of why a file has changed)");
            using (writer.Indent())
            {
                writer.WriteLine("uses: andstor/file-reader-action@v1");
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine("path: \"docs/examples/Mermaid-Class-diagrams.md\"");
                }
            }
        }
    }
}
