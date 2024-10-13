using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.GhPages;

public class DeployGeneratedDocsAsGhPagesArtifactStep : GitHubActionsStep
{
	public override void Write(CustomFileWriter writer)
	{
		writer.WriteLine("- name: Deploy generated docs github-pages artifact");
		using (writer.Indent())
		{
			writer.WriteLine("uses: actions/deploy-pages@v4");
		}
	}
}