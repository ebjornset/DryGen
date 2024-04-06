using DryGen.GithubActions.GhPages;
namespace DryGen.GithubActions.NugetPush;

public class NugetPushGitHubActionsAttribute : GhPagesGitHubActionsAttribute
{
    public NugetPushGitHubActionsAttribute(string name) : base(name, needsJava: true)
    {
    }
}