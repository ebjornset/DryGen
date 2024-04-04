using DryGen.UTests.Helpers;
using Reqnroll;

namespace DryGen.UTests.Steps;

[Binding]
public sealed class TreeShakingSteps
{
    private readonly TreeShakingContext treeShakingContext;

    public TreeShakingSteps(TreeShakingContext treeShakingContext)
    {
        this.treeShakingContext = treeShakingContext;
    }

    [Given(@"these tree shaking roots")]
    public void GivenTheseTreeShakingRoots(Table table)
    {
        treeShakingContext.AddTreeShakingRoots(table);
    }
}