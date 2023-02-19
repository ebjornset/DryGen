using CommandLine;
using System.Linq;
using System.Reflection;

namespace DryGen;

public class OptionMetadata
{
    public OptionMetadata(CustomAttributeData optionAttribute)
    {
        LongName = optionAttribute.ConstructorArguments.Single(x => x.ArgumentType == typeof(string)).Value?.ToString()?.Replace("\"", string.Empty) ?? string.Empty;
        LongName = $"--{LongName}";
        ShortName = optionAttribute.ConstructorArguments.SingleOrDefault(x => x.ArgumentType == typeof(char)).Value?.ToString()?.Replace("\"", string.Empty) ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(ShortName))
        {
            ShortName = $"-{ShortName}";
        }
        Description = optionAttribute.NamedArguments.Single(x => x.MemberName == nameof(OptionAttribute.HelpText)).TypedValue.ToString()?.Replace("\"", string.Empty) ?? string.Empty;
    }

    public string? LongName { get; private set; }
    public string? ShortName { get; private set; }
    public string? Description { get; private set; }
    public string? Type { get; set; }
}