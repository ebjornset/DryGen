using DryGen.Features.CSharpFromJsonSchema;
using DryGen.Features.Mermaid.FromCsharp.ClassDiagram;
using DryGen.Features.Mermaid.FromCsharp.ErDiagram;
using DryGen.Features.Mermaid.FromDotnetDepsJson.C4ComponentDiagram;
using DryGen.Features.Mermaid.FromEfCore.ErDiagram;
using DryGen.Features.Mermaid.FromJsonSchema.ClassDiagram;
using DryGen.Features.Mermaid.FromJsonSchema.ErDiagram;
using DryGen.Features.OptionsFromCommandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DryGen.Features.VerbsFromOptionsFile;

internal static class VerbsFromOptionsFileOptionsDocumentsBuilder
{
    internal static IEnumerable<VerbsFromOptionsFileOptionsDocument> BuildOptionsDocuments(VerbsFromOptionsFileOptions options)
    {
        var deserializer = CreateYamlDeserializer();
        var optionsDocuments = ReadOptionsDocumentsFromYaml(options, deserializer);
        CheckForDuplicates(optionsDocuments);
        BuildInheritOptionsFromPaths(optionsDocuments);
        PerformInheritOptionsFrom(optionsDocuments);
        return optionsDocuments;
    }

    private static IDeserializer CreateYamlDeserializer()
    {
        return new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithTypeDiscriminatingNodeDeserializer((o) =>
            {
                IDictionary<string, Type> valueMappings = new Dictionary<string, Type>
                {
                    { Constants.CsharpFromJsonSchema.Verb, typeof(CSharpFromJsonSchemaConfiguration) },
                    { Constants.MermaidC4ComponentDiagramFromDotnetDepsJson.Verb, typeof(MermaidC4ComponentDiagramFromDotnetDepsJsonConfiguration) },
                    { Constants.MermaidClassDiagramFromCsharp.Verb, typeof(MermaidClassDiagramFromCsharpConfiguration) },
                    { Constants.MermaidClassDiagramFromJsonSchema.Verb, typeof(MermaidClassDiagramFromJsonSchemaConfiguration) },
                    { Constants.MermaidErDiagramFromCsharp.Verb, typeof(MermaidErDiagramFromCsharpConfiguration) },
                    { Constants.MermaidErDiagramFromEfCore.Verb, typeof(MermaidErDiagramFromEfCoreConfiguration) },
                    { Constants.MermaidErDiagramFromJsonSchema.Verb, typeof(MermaidErDiagramFromJsonSchemaConfiguration) },
                    { Constants.OptionsFromCommandline.Verb, typeof(OptionsFromCommandlineConfiguration) },
                };
                o.AddKeyValueTypeDiscriminator<IVerbsFromOptionsFileConfiguration>("verb", valueMappings);
            })
            .Build();
    }

    private static List<VerbsFromOptionsFileOptionsDocument> ReadOptionsDocumentsFromYaml(VerbsFromOptionsFileOptions options, IDeserializer deserializer)
    {
        if (string.IsNullOrWhiteSpace(options.OptionsFile))
        {
            throw new OptionsException("--options-file is mandatory");
        }
        var yaml = options.OptionsFile.ReadOptionsFileWithEnviromentVariableReplacement();
        var yamlParser = new YamlDotNet.Core.Parser(new StringReader(yaml));
        yamlParser.Consume<StreamStart>();
        var optionsDocuments = new List<VerbsFromOptionsFileOptionsDocument>();
        var documentNumber = 0;
        while (yamlParser.TryConsume<DocumentStart>(out _))
        {
            documentNumber++;
            VerbsFromOptionsFileOptionsDocument optionsDocument;
            try
            {
                optionsDocument = deserializer.Deserialize<VerbsFromOptionsFileOptionsDocument>(yamlParser);
            }
            catch (YamlException e)
            when (e.InnerException?.InnerException?.Message?.Contains("Cannot dynamically create an instance of type 'DryGen.Features.VerbsFromOptionsFile.IVerbsFromOptionsFileConfiguration'") == true)
            {
                throw new OptionsException($"Unknown 'verb' in document #{documentNumber}");
            }
            if (optionsDocument?.Configuration == null)
            {
                throw new OptionsException($"'configuration' is mandatory in document #{documentNumber}");
            }
            if (optionsDocument.Configuration?.GetOptions() == null)
            {
                throw new OptionsException($"'configuration.options' is mandatory in document #{documentNumber}");
            }
            optionsDocument.DocumentNumber = documentNumber;
            optionsDocuments.Add(optionsDocument);
            yamlParser.TryConsume<DocumentEnd>(out _);
        }
        return optionsDocuments;
    }

    private static void CheckForDuplicates(List<VerbsFromOptionsFileOptionsDocument> optionsDocuments)
    {
        var duplicates = optionsDocuments.Where(x => !string.IsNullOrWhiteSpace(x.Configuration?.Name)).GroupBy(x => x.Configuration?.Name, x => x).Where(x => x.Count() > 1).ToList();
        if (duplicates.Any())
        {
            var names = string.Join(", ", duplicates.Select(x => $"'{x.Key}'"));
            var message = $"duplicate name(s): {names}";
            throw new OptionsException(message);
        }
    }

    private static void BuildInheritOptionsFromPaths(IEnumerable<VerbsFromOptionsFileOptionsDocument> optionsDocuments)
    {
        optionsDocuments = optionsDocuments.Where(x => x.Configuration != null).ToList();
        var lookup = optionsDocuments.Where(x => !string.IsNullOrWhiteSpace(x.Configuration.AsNonNull().Name)).ToDictionary(x => x.Configuration.AsNonNull().Name.AsNonNull());
        foreach (var optionsDocument in optionsDocuments)
        {
            var inheritOptionsFrom = optionsDocument.Configuration?.InheritOptionsFrom;
            if (!string.IsNullOrWhiteSpace(inheritOptionsFrom))
            {
                CheckInheritOptionsFromExists(lookup, optionsDocument, inheritOptionsFrom);
                var referencedDocument = lookup[inheritOptionsFrom];
                CheckInheritOptionsFromSelfReference(optionsDocument, referencedDocument);
                CheckInheritOptionsFromReferenceRing(optionsDocument, referencedDocument);
                CheckInheritOptionsFromVerbMismatch(optionsDocument, referencedDocument);
                optionsDocument.ParentOptionsDocument = referencedDocument;
            }
        }
    }

    private static void CheckInheritOptionsFromExists(Dictionary<string, VerbsFromOptionsFileOptionsDocument> lookup, VerbsFromOptionsFileOptionsDocument optionsDocument, string inheritOptionsFrom)
    {
        if (!lookup.ContainsKey(inheritOptionsFrom))
        {
            throw new OptionsException($"name '{inheritOptionsFrom}' refrenced in 'inherits-options-from' in document #{optionsDocument.DocumentNumber} not found");
        }
    }

    private static void CheckInheritOptionsFromSelfReference(VerbsFromOptionsFileOptionsDocument optionsDocument, VerbsFromOptionsFileOptionsDocument referencedDocument)
    {
        if (referencedDocument == optionsDocument)
        {
            throw new OptionsException($"document #{optionsDocument.DocumentNumber} 'inherit-options-from' it self");
        }
    }

    private static void CheckInheritOptionsFromReferenceRing(VerbsFromOptionsFileOptionsDocument optionsDocument, VerbsFromOptionsFileOptionsDocument referencedDocument)
    {
        var optionsDocumentsPath = referencedDocument.GetOptionsDocumentsPath();
        if (optionsDocumentsPath.Contains(optionsDocument))
        {
            optionsDocumentsPath.Insert(0, optionsDocument);
            var path = string.Join("' -> '", optionsDocumentsPath.Select(x => x.Configuration.AsNonNull().Name));
            throw new OptionsException($"ring found in 'inherit-options-from' in document #{optionsDocument.DocumentNumber}: '{path}'");
        }
    }

    private static void CheckInheritOptionsFromVerbMismatch(VerbsFromOptionsFileOptionsDocument optionsDocument, VerbsFromOptionsFileOptionsDocument referencedDocument)
    {
        if (optionsDocument.Configuration?.Verb != referencedDocument.Configuration?.Verb)
        {
            throw new OptionsException($"document #{optionsDocument.DocumentNumber} 'inherits-options-from' references wrong verb, expected '{optionsDocument.Configuration?.Verb}', but found '{referencedDocument.Configuration?.Verb}'");
        }
    }

    private static void PerformInheritOptionsFrom(IEnumerable<VerbsFromOptionsFileOptionsDocument> optionsDocuments)
    {
        foreach (var optionsDocument in optionsDocuments)
        {
            optionsDocument.PerformInheritOptionsFrom();
        }
    }
}