using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.DotNet;

public class UploadTestResultsArtifactsStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"- name: Upload test results artifacts");
        using (writer.Indent())
        {
            writer.WriteLine("uses: actions/upload-artifact@v3");
            writer.WriteLine("if: always()");
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine($"name: TestResults");
                writer.WriteLine($"retention-days: 30");
                writer.WriteLine($"path: '**/TestResults/**/*'");
            }
        }
    }
}