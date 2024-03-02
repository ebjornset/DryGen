using DryGen.Core;
using FluentAssertions;
using System;
using Xunit;

namespace DryGen.UTests.Tests.Core;

public class ExtensionsTests
{
    [Fact]
    public void AsNonNull_SHould_Throw_ArgumentNullException_For_Null_Vallue()
    {
        Action act = () => ((object?)null).AsNonNull();

        act.Should().ThrowExactly<ArgumentNullException>();
    }
}
