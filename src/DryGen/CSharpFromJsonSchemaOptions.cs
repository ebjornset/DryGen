using CommandLine;
using DryGen.CSharpFromJsonSchema;
using YamlDotNet.Serialization;

namespace DryGen
{

    [Verb(Constants.CSharpFromJsonSchema.Verb, HelpText = "Generate C# classes from a json schema (using NJsonSchema https://github.com/RicoSuter/NJsonSchema).")]
    public class CSharpFromJsonSchemaOptions : BaseOptions
    {
        [YamlMember(Alias = "schema-file-format", ApplyNamingConventions = false)]
        [Option("schema-file-format", HelpText = "What format should be used when reading the schema file? 'ByExtension' (default) treats files with extension 'yaml' or 'yml' as yaml, others as json. Use 'Yaml' or 'Json' to force the format explicitly. ")]
        public JsonSchemaFileFormat SchemaFileFormat { get; set; }
    }
}
