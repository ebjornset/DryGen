using CommandLine;
using CommandLine.Text;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ClassDiagram;
using DryGen.MermaidFromCSharp.EfCore;
using DryGen.MermaidFromCSharp.ErDiagram;
using DryGen.MermaidFromCSharp.NameRewriters;
using DryGen.MermaidFromCSharp.PropertyFilters;
using DryGen.MermaidFromCSharp.TypeFilters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DryGen
{
    public class Generator
    {
        private readonly Parser parser;
        private readonly TextWriter outWriter;
        private readonly TextWriter errorWriter;

        public Generator(TextWriter outWriter, TextWriter errorWriter)
        {
            parser = new Parser(with =>
            {
                with.CaseInsensitiveEnumValues = true;
            });
            this.outWriter = outWriter;
            this.errorWriter = errorWriter;
        }

        public int Run(string[] args)
        {
            var parserResult = parser.ParseArguments<MermaidErDiagramFromCSharpOptions, MermaidErDiagramFromEfCoreOptions, MermaidClassDiagramFromCSharpOptions>(args);
            return parserResult.MapResult(
              (MermaidErDiagramFromCSharpOptions options) => GenerateErDiagramFromCSharp(options, args),
              (MermaidErDiagramFromEfCoreOptions options) => GenerateErDiagramFromEfCore(options, args),
              (MermaidClassDiagramFromCSharpOptions options) => GenerateClassDiagramFropmCSharp(options, args),
              errors => DisplayHelp(parserResult));
        }

        private int DisplayHelp<T>(ParserResult<T> result)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                result.WithNotParsed(errors =>
                {
                    if (errors.Any(x => x.Tag == ErrorType.BadVerbSelectedError))
                    {
                        h.AddPostOptionsText("Rerun the command with a valid verb and --help to get more help information about the spesific verb.");
                    }
                });
                h.AutoHelp = false;     // hides --help
                h.AutoVersion = false;  // hides --version
                var helpText = HelpText.DefaultParsingErrorsHandler(result, h);
                return helpText;
            }, e => e, verbsIndex: true);
            errorWriter.WriteLine(helpText);
            return 1;
        }

        private int ExecureWithExceptionAndHelpDisplay<TOptions>(TOptions options, Func<TOptions, int> verbFunc) where TOptions : BaseOptions, new()
        {
            try
            {
                return verbFunc(options);
            }
            catch (Exception ex)
            {
                var verbAttribute = options.GetType().GetCustomAttribute<VerbAttribute>();
                if (verbAttribute != null)
                {
                    errorWriter.WriteLine($"VERB: {verbAttribute.Name} ({verbAttribute.HelpText})");
                }
                errorWriter.WriteLine();
                errorWriter.WriteLine($"ERROR: {ex.Message}");
                errorWriter.WriteLine($"Rerun the command with --help to get more help information");
                return 1;
            }
        }

        private int GenerateErDiagramFromCSharp(MermaidErDiagramFromCSharpOptions options, string[] args)
        {
            return ExecureWithExceptionAndHelpDisplay(options, options =>
            {
                options = GetOptionsFromFileWithCommandlineOptionsAsOverrides(options, args);
                var structureBuilder =
                     options.StructureBuilder == MermaidErDiagramFromCSharpOptions.ErStructureBuilderType.EfCore ?
                     (IErDiagramStructureBuilder)new ErDiagramStructureBuilderByEfCore() :
                     new ErDiagramStructureBuilderByReflection();
                var attributeTypeExclusion = GetAttributeTypeExclusions(options);
                var attributeDetailExclusions = GetAttributeDetailExclusions(options);
                var relationshipLevel = options.ExcludeAllRelationships ?? default ? ErDiagramRelationshipTypeExclusion.All : ErDiagramRelationshipTypeExclusion.None;
                var diagramGenerator = new ErDiagramGenerator(structureBuilder, attributeTypeExclusion, attributeDetailExclusions, relationshipLevel);
                return GenerateDiagramFromCSharp(options, diagramGenerator);
            });
        }

        private int GenerateErDiagramFromEfCore(MermaidErDiagramFromEfCoreOptions options, string[] args)
        {
            return ExecureWithExceptionAndHelpDisplay(options, options =>
            {
                options = GetOptionsFromFileWithCommandlineOptionsAsOverrides(options, args);
                var structureBuilder =
                     options.StructureBuilder == MermaidErDiagramFromCSharpBaseOptions.ErStructureBuilderType.EfCore ?
                     (IErDiagramStructureBuilder)new ErDiagramStructureBuilderByEfCore() :
                     new ErDiagramStructureBuilderByReflection();
                var attributeTypeExclusion = GetAttributeTypeExclusions(options);
                var attributeDetailExclusions = GetAttributeDetailExclusions(options);
                var relationshipLevel = options.ExcludeAllRelationships ?? default ? ErDiagramRelationshipTypeExclusion.All : ErDiagramRelationshipTypeExclusion.None;
                var diagramGenerator = new ErDiagramGenerator(structureBuilder, attributeTypeExclusion, attributeDetailExclusions, relationshipLevel);
                return GenerateDiagramFromCSharp(options, diagramGenerator);
            });

        }

        private int GenerateClassDiagramFropmCSharp(MermaidClassDiagramFromCSharpOptions options, string[] args)
        {
            return ExecureWithExceptionAndHelpDisplay(options, options =>
            {
                options = GetOptionsFromFileWithCommandlineOptionsAsOverrides(options, args);
                var diagramGenerator = new ClassDiagramGenerator(
                    new TypeLoaderByReflection(),
                    options.AttributeLevel ?? default,
                    options.MethodLevel ?? default,
                    options.Direction ?? default,
                    excludeStaticAttributes: options.ExcludeStaticAttributes ?? default,
                    excludeStaticMethods: options.ExcludeStaticMethods ?? default,
                    excludeMethodParams: options.ExcludeMethodParams ?? default);
                return GenerateDiagramFromCSharp(options, diagramGenerator);
            });
        }

        private int GenerateDiagramFromCSharp(MermaidFromCSharpBaseOptions cSharpOptions, IDiagramGenerator diagramGenerator)
        {
            if (!string.IsNullOrEmpty(cSharpOptions.OutputFile))
            {
                outWriter.WriteLine($"Generating mermaid diagram to file '{cSharpOptions.OutputFile}'");
            }
            var assembly = Assembly.LoadFrom(cSharpOptions.AssemblyFile ?? throw new InvalidOperationException("Input file must be specified as the option -i/--input-file on the command line, or as input-file in the option file."));
            var namespaceFilters = cSharpOptions.IncludeNamespaces?.Select(x => new IncludeNamespaceTypeFilter(x)).ToArray() ?? Array.Empty<IncludeNamespaceTypeFilter>();
            var typeFilters = new List<ITypeFilter> { new AnyChildFiltersTypeFilter(namespaceFilters) };
            if (cSharpOptions.IncludeTypeNames?.Any() == true)
            {
                var typeNameFilters = cSharpOptions.IncludeTypeNames.Select(x => new IncludeTypeNameTypeFilter(x)).ToArray();
                typeFilters.Add(new AnyChildFiltersTypeFilter(typeNameFilters));
            }
            if (cSharpOptions.ExcludeTypeNames?.Any() == true)
            {
                var typeNameFilters = cSharpOptions.ExcludeTypeNames.Select(x => new ExcludeTypeNameTypeFilter(x)).ToArray();
                typeFilters.Add(new AllChildFiltersTypeFilter(typeNameFilters));
            }
            var excludePropertyNamesFilters = cSharpOptions.ExcludePropertyNames?.Select(x => new ExcludePropertyNamePropertyFilter(x)).ToArray() ?? Array.Empty<IPropertyFilter>();
            var nameRewriter = new ReplaceNameRewriter(cSharpOptions.NameReplaceFrom ?? string.Empty, cSharpOptions.NameReplaceTo ?? string.Empty);
            var mermaid = diagramGenerator.Generate(assembly, typeFilters, excludePropertyNamesFilters, nameRewriter);
            WriteMermaidToConsoleOrFile(cSharpOptions, mermaid);
            return 0;
        }

        private TOptions GetOptionsFromFileWithCommandlineOptionsAsOverrides<TOptions>(TOptions commandlineOptions, string[] args) where TOptions : BaseOptions
        {
            if (!string.IsNullOrEmpty(commandlineOptions.OptionsFile))
            {
                var deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
                var yaml = File.ReadAllText(commandlineOptions.OptionsFile);
                var optionsFromFile = deserializer.Deserialize<TOptions>(yaml);
                // Must read the options from the file twice, since the command line parser clear the collection when the option is not specified on the command line
                var parserResult = parser.ParseArguments(() => deserializer.Deserialize<TOptions>(yaml), args);
                CheckForReparseProblem(parserResult);
                var result = ((Parsed<TOptions>)parserResult).Value;
                ReplaceEmptyIEnumerabeStrings(result, optionsFromFile);
                return result;
            }
            return commandlineOptions;
        }

        [ExcludeFromCodeCoverage] // This should in theory never happend, and cannot be tested
        private void CheckForReparseProblem<TOptions>(ParserResult<TOptions> parserResult) where TOptions : BaseOptions
        {
            if (parserResult.Tag == ParserResultType.NotParsed)
            {
                DisplayHelp(parserResult);
                throw new ArgumentException("Parse problem in reparse after options has been loaded from file");
            }
        }

        private static void ReplaceEmptyIEnumerabeStrings<TOptions>(TOptions target, TOptions source) where TOptions : BaseOptions
        {
            foreach (var property in typeof(TOptions).GetProperties())
            {
                var propertyType = property.PropertyType;
                if (propertyType == typeof(IEnumerable<string>))
                {
                    var targetCollection = (IEnumerable<string>?)property.GetValue(target);
                    var sourceCollection = (IEnumerable<string>?)property.GetValue(source);
                    if (targetCollection?.Any() != true && sourceCollection?.Any() == true)
                    {
                        property.SetValue(target, sourceCollection);
                    }
                }
            }
        }

        private void WriteMermaidToConsoleOrFile(BaseOptions options, string mermaid)
        {
            if (string.IsNullOrEmpty(options.OutputFile))
            {
                outWriter.Write(mermaid);
            }
            else
            {
                var outputFile = Path.GetFullPath(options.OutputFile);
                var mermaidDirectory = Path.GetDirectoryName(outputFile);
                if (mermaidDirectory == null)
                {
                    throw new InvalidOperationException($"Can't get directory from output file'{outputFile}'.");
                }
                if (!Directory.Exists(mermaidDirectory))
                {
                    Directory.CreateDirectory(mermaidDirectory);
                }
                File.WriteAllText(outputFile, mermaid);
            }
        }

        private static ErDiagramAttributeTypeExclusion GetAttributeTypeExclusions(MermaidErDiagramFromCSharpBaseOptions options)
        {
            if (options.ExcludeAllAttributes ?? default)
            {
                return ErDiagramAttributeTypeExclusion.All;
            }
            if (options.ExcludeForeignkeyAttributes ?? default)
            {
                return ErDiagramAttributeTypeExclusion.Foreignkeys;
            }
            return ErDiagramAttributeTypeExclusion.None;
        }

        private static ErDiagramAttributeDetailExclusions GetAttributeDetailExclusions(MermaidErDiagramFromCSharpBaseOptions options)
        {
            var result = ErDiagramAttributeDetailExclusions.None;
            if (options.ExcludeAttributeKeytypes ?? default)
            {
                result |= ErDiagramAttributeDetailExclusions.KeyTypes;
            }
            if (options.ExcludeAttributeComments ?? default)
            {
                result |= ErDiagramAttributeDetailExclusions.Comments;
            }
            return result;
        }
    }
}