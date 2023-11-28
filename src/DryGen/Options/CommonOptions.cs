using CommandLine;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;

namespace DryGen.Options;

public abstract class CommonOptions : BaseOptions
{
    [YamlMember(Alias = Constants.OutputFileOption, ApplyNamingConventions = false)]
    [Option('o', Constants.OutputFileOption, HelpText = "Write the generated representation to this file.")]
    public string? OutputFile { get; set; }

    [YamlMember(Alias = Constants.ReplaceTokenInOutputFile, ApplyNamingConventions = false)]
    [Option(Constants.ReplaceTokenInOutputFile, HelpText = "Replace this token in the output file with the generated representation instead of just writing the generated representation to the specified output file.")]
    public string? ReplaceTokenInOutputFile { get; set; }

    public void InheritFrom(CommonOptions other)
    {
        CheckTypeMatches(other);
        var yamlMemberProperties = GetType().GetYamlMemberProperties(excludeDeprecated: false);
        foreach (var yamlMemberProperty in yamlMemberProperties)
        {
            var myValue = yamlMemberProperty.GetValue(this);
            if (myValue != null)
            {
                continue;
            }
            var otherValue = yamlMemberProperty.GetValue(other);
            if (otherValue == null)
            {
                continue;
            }
            yamlMemberProperty.SetValue(this, otherValue);
        }
    }

    [ExcludeFromCodeCoverage]
    private void CheckTypeMatches(CommonOptions other)
    {
        if (GetType() != other.GetType())
        {
            throw new OptionsException($"Type mismatch, expected '{GetType()}', but got '{other.GetType()}'");
        }
    }
}
