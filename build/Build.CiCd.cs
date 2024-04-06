using Nuke.Common;

namespace DryGen.Build;

public partial class Build
{
#pragma warning disable CA1822 // Mark members as static
    internal Target CiCd_Default => _ => _
        .Unlisted()
        .Requires(() => IsServerBuild)
        .DependsOn(Default)
        .DependsOn(GitWorkingCopyShouldBeClean)
        ;

    internal Target CiCd_PullRequest => _ => _
        .Unlisted()
        .DependsOn(CiCd_Default)
        ;

    internal Target CiCd_Build => _ => _
        .Unlisted()
        .DependsOn(CiCd_Default)
        ;

    internal Target CiCd_BuildDocs => _ => _
        .Unlisted()
        .DependsOn(CiCd_Default)
        .DependsOn(BuildDocs)
        .Before(Push)
        ;

    internal Target CiCd_Release => _ => _
        .Unlisted()
        .DependsOn(CiCd_Default)
        .DependsOn(CiCd_BuildDocs)
        .DependsOn(Push)
        ;
#pragma warning restore CA1822  // Mark members as static
}