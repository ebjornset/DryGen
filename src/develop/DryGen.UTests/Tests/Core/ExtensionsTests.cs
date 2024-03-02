using DryGen.Core;
using DryGen.DevUtils.Helpers;
using FluentAssertions;
using System;
using Xunit;

namespace DryGen.UTests.Tests.Core;

public class ExtensionsTests
{
    [Fact]
    public void LoadTypeByName_Should_Throw_TypeLoadException_For_Unknown_Type()
    {
        Action act = () => "non-existing-type".AsUniqueTestValue().LoadTypeByName();

        act.Should().ThrowExactly<DryGen.Core.TypeLoadException>();
    }

    [Fact]
    public void AsNonNull_Should_Throw_ArgumentNullException_For_Null_Value()
    {
        Action act = () => ((object?)null).AsNonNull();

        act.Should().ThrowExactly<ArgumentNullException>();
    }
}
