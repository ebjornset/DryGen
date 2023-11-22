using DryGen.Options;

namespace DryGen.Features.VerbsFromOptionsFile;

public interface IVerbsFromOptionsFileConfiguration
{
    public string? Verb { get; }
    public CommonOptions? GetOptions();
}

