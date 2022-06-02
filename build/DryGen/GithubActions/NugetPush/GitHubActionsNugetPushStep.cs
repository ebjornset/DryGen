using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.NugetPush
{
    public class GitHubActionsNugetPushStep : GitHubActionsStep
    {
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- name: Run './build.cmd Push'");
            using (writer.Indent())
            {
                writer.WriteLine("run: ./build.cmd Push");
                writer.WriteLine("env:");
                using (writer.Indent())
                {
                    writer.WriteLine("NUGET_API_KEY: ${{ secrets.NUGET_API_KEY }}");
                }
            }
        }
    }
}
