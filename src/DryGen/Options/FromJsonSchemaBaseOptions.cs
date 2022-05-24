using CommandLine;
using DryGen.CSharpFromJsonSchema;
using YamlDotNet.Serialization;

namespace DryGen.Options
{
    public abstract class FromJsonSchemaBaseOptions : BaseOptions, IFromJsonSchemaOptions
    {
        [YamlMember(Alias = "schema-file-format", ApplyNamingConventions = false)]
        [Option("schema-file-format", HelpText = "What format should be used when reading the schema file? 'ByExtension' (default) treats files with extension 'yaml' or 'yml' as yaml, others as json. Use 'Yaml' or 'Json' to force the format explicitly.")]
        public JsonSchemaFileFormat SchemaFileFormat { get; set; }

        [YamlMember(Alias = "root-classname", ApplyNamingConventions = false)]
        [Option("root-classname", HelpText = "The classname for the class representing the schema it self. Default is the schema title, or 'ClassFromJsonSchema' if the schema has no title.")]
        public string? RootClassname { get; set; }
    }
}
