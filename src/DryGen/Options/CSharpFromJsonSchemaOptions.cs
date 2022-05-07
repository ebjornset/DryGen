using CommandLine;
using DryGen.CSharpFromJsonSchema;
using YamlDotNet.Serialization;

namespace DryGen.Options
{

    [Verb(Constants.CSharpFromJsonSchema.Verb, HelpText = "Generate C# classes from a json schema (using NJsonSchema https://github.com/RicoSuter/NJsonSchema).")]
    public class CSharpFromJsonSchemaOptions : BaseOptions
    {
        [YamlMember(Alias = "schema-file-format", ApplyNamingConventions = false)]
        [Option("schema-file-format", HelpText = "What format should be used when reading the schema file? 'ByExtension' (default) treats files with extension 'yaml' or 'yml' as yaml, others as json. Use 'Yaml' or 'Json' to force the format explicitly. ")]
        public JsonSchemaFileFormat SchemaFileFormat { get; set; }

        [YamlMember(Alias = "namespace", ApplyNamingConventions = false)]
        [Option("namespace", HelpText = "The namespace for the generated c# classes.")]
        public string? Namespace { get; set; }

        [YamlMember(Alias = "root-classname", ApplyNamingConventions = false)]
        [Option("root-classname", HelpText = "The classname for the generated c# class representing the schema it self.")]
        public string? RootClassname { get; set; }
    }
}
