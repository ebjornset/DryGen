using CommandLine;
using DryGen.CSharpFromJsonSchema;
using YamlDotNet.Serialization;

namespace DryGen.Options
{

    [Verb(Constants.CSharpFromJsonSchema.Verb, HelpText = "Generate C# classes from a json schema (using NJsonSchema https://github.com/RicoSuter/NJsonSchema).")]
    public class CSharpFromJsonSchemaOptions : BaseOptions
    {
        [YamlMember(Alias = "schema-file-format", ApplyNamingConventions = false)]
        [Option("schema-file-format", HelpText = "What format should be used when reading the schema file? 'ByExtension' (default) treats files with extension 'yaml' or 'yml' as yaml, others as json. Use 'Yaml' or 'Json' to force the format explicitly.")]
        public JsonSchemaFileFormat SchemaFileFormat { get; set; }

        [YamlMember(Alias = "namespace", ApplyNamingConventions = false)]
        [Option("namespace", HelpText = "The namespace for the generated c# classes (default: 'CSharpFromJsonSchema').")]
        public string? Namespace { get; set; }

        [YamlMember(Alias = "root-classname", ApplyNamingConventions = false)]
        [Option("root-classname", HelpText = "The classname for the generated c# class representing the schema it self. Default is the schema title, or 'CSharpFromJsonSchema' if the schema has no title.")]
        public string? RootClassname { get; set; }

        [YamlMember(Alias = "array-type", ApplyNamingConventions = false)]
        [Option("array-type", HelpText = "The generic array .NET type (default: 'ICollection').")]
        public string? ArrayType { get; set; }

        [YamlMember(Alias = "array-instance-type", ApplyNamingConventions = false)]
        [Option("array-instance-type", HelpText = "The generic array .NET type which is used for ArrayType instances (default: 'Collection').")]
        public string? ArrayInstanceType { get; set; }
    }
}
