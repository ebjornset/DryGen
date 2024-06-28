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
        .DependsOn(VerifyCleanWorkingCopyAfterBuild)
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
        ;

    internal Target CiCd_Release => _ => _
        .Unlisted()
        .DependsOn(CiCd_Build)
        .DependsOn(PushPackagesToNuget)
        ;

    internal Target CiCd_TagVersion => _ => _
        .Unlisted()
        .Requires(() => GitRepository.IsOnMainBranch())
        .DependsOn(CiCd_Build)
        .DependsOn(PushVersionTag)
        ;

#pragma warning restore CA1822  // Mark members as static
}