using CommandLine;
using YamlDotNet.Serialization;

namespace DryGen
{

    public abstract class MermaidErDiagramFromCSharpBaseOptions : MermaidFromCSharpBaseOptions
    {

        [YamlMember(Alias = "exclude-all-attributes", ApplyNamingConventions = false)]
        [Option("exclude-all-attributes", HelpText = "Should all attributes be excluded from the diagram?")]
        public bool? ExcludeAllAttributes { get; set; }

        [YamlMember(Alias = "exclude-attribute-keytypes", ApplyNamingConventions = false)]
        [Option("exclude-attribute-keytypes", HelpText = "Should attributes key types be excluded from the diagram?")]
        public bool? ExcludeAttributeKeytypes { get; set; }

        [YamlMember(Alias = "exclude-attribute-comments", ApplyNamingConventions = false)]
        [Option("exclude-attribute-comments", HelpText = "Should attributes comments be excluded from the diagram?")]
        public bool? ExcludeAttributeComments { get; set; }

        [YamlMember(Alias = "exclude-foreignkey-attributes", ApplyNamingConventions = false)]
        [Option("exclude-foreignkey-attributes", HelpText = "Should foreign key attributes be excluded from the diagram?")]
        public bool? ExcludeForeignkeyAttributes { get; set; }

        [YamlMember(Alias = "exclude-all-relationships", ApplyNamingConventions = false)]
        [Option("exclude-all-relationships", HelpText = "Should all relationships be excluded from the diagram?")]
        public bool? ExcludeAllRelationships { get; set; }

        public ErStructureBuilderType StructureBuilder { get; private set; }

        protected MermaidErDiagramFromCSharpBaseOptions(ErStructureBuilderType structureBuilder)
        {
            StructureBuilder = structureBuilder;
        }

        public enum ErStructureBuilderType
        {
            Reflection,
            EfCore
        }
    }
}
