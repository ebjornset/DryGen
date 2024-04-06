using Nuke.Common;
using Nuke.Common.Git;

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
        .DependsOn(SonarCloudBegin)
        .DependsOn(SonarCloudEnd)
        ;

    internal Target CiCd_Build => _ => _
        .Unlisted()
        .DependsOn(CiCd_Default)
        .DependsOn(SonarCloudBegin)
        .DependsOn(SonarCloudEnd)
        ;

    internal Target CiCd_BuildDocs => _ => _
        .Unlisted()
        .DependsOn(CiCd_Default)
        .DependsOn(BuildDocs)
        ;

    internal Target CiCd_Release => _ => _
        .Unlisted()
        .DependsOn(CiCd_Default)
        .DependsOn(SonarCloudBegin)
        .DependsOn(SonarCloudEnd)
        .DependsOn(BuildDocs)
        .DependsOn(Push)
        ;

    internal Target CiCd_TagVersion => _ => _
        .Unlisted()
        .Requires(() => GitRepository.IsOnMainBranch())
        .DependsOn(CiCd_Default)
        .DependsOn(BuildDocs)
        ;

#pragma warning restore CA1822  // Mark members as static
}