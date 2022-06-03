using CommandLine;
using DryGen.CSharpFromJsonSchema;
using YamlDotNet.Serialization;

namespace DryGen.Options
{
    [Verb(Constants.CSharpFromJsonSchema.Verb, HelpText = "Generate C# classes from a json schema (using NJsonSchema https://github.com/RicoSuter/NJsonSchema).")]
    public class CSharpFromJsonSchemaOptions : FromJsonSchemaBaseOptions, ICSharpFromJsonSchemaOptions
    {
        [YamlMember(Alias = "namespace", ApplyNamingConventions = false)]
        [Option("namespace", HelpText = "The namespace for the generated c# classes (default: 'CSharpFromJsonSchema').")]
        public string? Namespace { get; set; }

        [YamlMember(Alias = "array-type", ApplyNamingConventions = false)]
        [Option("array-type", HelpText = "The generic array .NET type (default: 'ICollection').")]
        public string? ArrayType { get; set; }

        [YamlMember(Alias = "array-instance-type", ApplyNamingConventions = false)]
        [Option("array-instance-type", HelpText = "The generic array .NET type which is used for ArrayType instances (default: 'Collection').")]
        public string? ArrayInstanceType { get; set; }
    }
}
