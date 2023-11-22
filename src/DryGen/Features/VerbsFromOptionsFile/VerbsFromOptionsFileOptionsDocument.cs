using YamlDotNet.Serialization;

namespace DryGen.Features.VerbsFromOptionsFile;

public class VerbsFromOptionsFileOptionsDocument
{
    [YamlMember(Alias = "configuration", ApplyNamingConventions = false)]
    public IVerbsFromOptionsFileConfiguration? Configuration { get; set; }
}
