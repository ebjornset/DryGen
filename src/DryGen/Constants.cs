namespace DryGen;

public static class Constants
{
    public const string DeprecatedNotice = "Deprecated";
    public const string InputFileOption = "input-file";
    public const string OutputFileOption = "output-file";
    public const string OptionsFileOption = "options-file";
    public const string ReplaceTokenInOutputFile = "replace-token-in-output-file";

    public static class MermaidClassDiagramFromCsharp
    {
        public const string Verb = "mermaid-class-diagram-from-csharp";
    }

    public static class MermaidClassDiagramFromJsonSchema
    {
        public const string Verb = "mermaid-class-diagram-from-json-schema";
    }

    public static class MermaidErDiagramFromCsharp
    {
        public const string Verb = "mermaid-er-diagram-from-csharp";
        public const string AttributeTypeExclusionOption = "attribute-type-exclusion";
        public const string RelationshipTypeExclusionOption = "relationship-type-exclusion";
    }

    public static class MermaidErDiagramFromEfCore
    {
        public const string Verb = "mermaid-er-diagram-from-efcore";
    }

    public static class MermaidErDiagramFromJsonSchema
    {
        public const string Verb = "mermaid-er-diagram-from-json-schema";
    }

    public static class MermaidC4ComponentDiagramFromDotnetDepsJson
    {
        public const string Verb = "mermaid-c4component-diagram-from-dotnet-deps-json";
    }

    public static class CSharpFromJsonSchema
    {
        public const string Verb = "csharp-from-json-schema";
    }

    public static class OptionsFromCommandline
    {
        public const string Verb = "options-from-commandline";
    }
}
