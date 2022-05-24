using CommandLine;
using DryGen.MermaidFromCSharp;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace DryGen.Options
{
    public abstract class MermaidFromCSharpBaseOptions : BaseOptions, IMermaidDiagramOptions
    {
        [YamlMember(Alias = "include-namespaces", ApplyNamingConventions = false)]
        [Option("include-namespaces", Separator = ';', HelpText = "A list of regular expressions for namespaces to include.")]
        public IEnumerable<string>? IncludeNamespaces { get; set; }

        [YamlMember(Alias = "include-typenames", ApplyNamingConventions = false)]
        [Option("include-typenames", Separator = ';', HelpText = "A list of regular expressions for type names to include.")]
        public IEnumerable<string>? IncludeTypeNames { get; set; }

        [YamlMember(Alias = "exclude-typenames", ApplyNamingConventions = false)]
        [Option("exclude-typenames", Separator = ';', HelpText = "A list of regular expressions for type names to exclude.")]
        public IEnumerable<string>? ExcludeTypeNames { get; set; }

        [YamlMember(Alias = "exclude-propertynames", ApplyNamingConventions = false)]
        [Option("exclude-propertynames", Separator = ';', HelpText = "A list of regular expressions for property names to exclude from each type.")]
        public IEnumerable<string>? ExcludePropertyNames { get; set; }

        [YamlMember(Alias = "name-replace-from", ApplyNamingConventions = false)]
        [Option("name-replace-from", HelpText = "A string to replace in all class/entity names.")]
        public string? NameReplaceFrom { get; set; }

        [YamlMember(Alias = "name-replace-to", ApplyNamingConventions = false)]
        [Option("name-replace-to", HelpText = "The string to replace with in all class/entity names.")]
        public string? NameReplaceTo { get; set; }
    }
}