namespace DryGen.GithubActions.DotNet;

public class DotNetGitHubActionsAttribute : BaseGitHubActionsAttribute
{
    public DotNetGitHubActionsAttribute(string name, bool needsJava = true) : base(name, needsJava)
    {
    }
}