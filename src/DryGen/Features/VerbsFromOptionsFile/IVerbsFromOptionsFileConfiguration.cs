using DryGen.Options;

namespace DryGen.Features.VerbsFromOptionsFile;

public interface IVerbsFromOptionsFileConfiguration
{
    public string? Name { get; }
    public string? Verb { get; }
    public CommonOptions? GetOptions();
}

