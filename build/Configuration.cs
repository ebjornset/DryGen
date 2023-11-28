using Nuke.Common.Tooling;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DryGen.Build;

[ExcludeFromCodeCoverage]
[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration
{
    public static readonly Configuration Debug = new() { Value = nameof(Debug) };
    public static readonly Configuration Release = new() { Value = nameof(Release) };

    public static implicit operator string(Configuration configuration)
    {
        return configuration.Value;
    }
}