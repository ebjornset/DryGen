using DryGen.Options;

namespace DryGen.Features.VerbsFromOptionsFile;

public interface IVerbsFromOptionsFileConfiguration
{
    public string? Name { get; }
    public string? Verb { get; }
    public string? InheritOptionsFrom { get; }
    public CommonOptions? GetOptions();
    public void PerformInheritOptionsFrom(IVerbsFromOptionsFileConfiguration other);
}
