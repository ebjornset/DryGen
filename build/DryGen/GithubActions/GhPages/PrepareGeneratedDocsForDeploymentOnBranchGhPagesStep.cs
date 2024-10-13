using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace DryGen.GithubActions.GhPages;

public class UploadGeneratedDocsAsGhPagesArtifactStep : GitHubActionsStep
{
	public override void Write(CustomFileWriter writer)
	{
		writer.WriteLine("- name: Upload generated docs as github-pages artifact");
		using (writer.Indent())
		{
			writer.WriteLine("uses: actions/upload-pages-artifact@v3");
			writer.WriteLine("with:");
			using (writer.Indent())
			{
				writer.WriteLine("path: ./docs/_site");
			}
		}
	}
}
