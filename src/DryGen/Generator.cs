using CommandLine;
using CommandLine.Text;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ClassDiagram;
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
using DryGen.CSharpFromJsonSchema;
using DryGen.Options;
using DryGen.MermaidFromJsonSchema;
using System.Runtime.Loader;

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
                with.HelpWriter = null;
            });
            this.outWriter = outWriter;
            this.errorWriter = errorWriter;
        }

        public int Run(string[] args)
        {
            var parserResult = parser.ParseArguments<
                CSharpFromJsonSchemaOptions,
                MermaidClassDiagramFromCSharpOptions,
                MermaidClassDiagramFromJsonSchemaOptions,
                MermaidErDiagramFromCSharpOptions,
                MermaidErDiagramFromEfCoreOptions,
                MermaidErDiagramFromJsonSchemaOptions,
                OptionsFromCommandlineOptions
                 >(args);
            return parserResult.MapResult(
              (CSharpFromJsonSchemaOptions options) => GenerateCSharpFromJsonSchema(options, args),
              (MermaidClassDiagramFromCSharpOptions options) => GenerateMermaidClassDiagramFropmCSharp(options, args),
              (MermaidClassDiagramFromJsonSchemaOptions options) => GenerateMermaidClassDiagramFromJsonSchema(options, args),
              (MermaidErDiagramFromCSharpOptions options) => GenerateMermaidErDiagramFromCSharp(options, args),
              (MermaidErDiagramFromEfCoreOptions options) => GenerateMermaidErDiagramFromEfCore(options, args),
              (MermaidErDiagramFromJsonSchemaOptions options) => GenerateMermaidErDiagramFromJsonSchema(options, args),
              (OptionsFromCommandlineOptions options) => GenerateOptionsFromCommandline(options, args),
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
                h.OptionComparison = HelpText.RequiredThenAlphaComparison;
                h.AddEnumValuesToHelpText = true;
                h.MaximumDisplayWidth = 100;
                h.AddDashesToOption = true;
                var helpText = HelpText.DefaultParsingErrorsHandler(result, h);
                return helpText;
            }, e => e, verbsIndex: true);
            errorWriter.WriteLine(helpText);
            return 1;
        }

        private int ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay<TOptions>(TOptions options, string[] args, string resultRepresentation, Func<TOptions, string> resultFunc) where TOptions : BaseOptions, new()
        {
            return ExecuteWithExceptionHandlingAndHelpDisplay(options, options =>
            {
                string? existingRepresentation = null;
                options = GetOptionsFromFileWithCommandlineOptionsAsOverrides(options, args);
                if (string.IsNullOrWhiteSpace(options.OutputFile) && !string.IsNullOrWhiteSpace(options.ReplaceTokenInOutputFile))
                {
                    throw new OptionsException("'replace-token-in-output-file' specified when 'output-file' is missing.");
                }
                if (!string.IsNullOrEmpty(options.OutputFile))
                {
                    if (string.IsNullOrWhiteSpace(options.ReplaceTokenInOutputFile))
                    {
                        outWriter.WriteLine($"Generating {resultRepresentation} to file '{options.OutputFile}'");
                    }
                    else
                    {
                        existingRepresentation = ReadExistingRepresentationFromOutputFileAndValidateReplaceToken(resultRepresentation, options.OutputFile, options.ReplaceTokenInOutputFile);
                    }
                }
                var resultReprersentation = resultFunc(options);
                WriteGeneratedRepresentationToConsoleOrFile(options, resultReprersentation, existingRepresentation);
                return 0;
            });
        }

        public string ReadExistingRepresentationFromOutputFileAndValidateReplaceToken(string resultRepresentation, string outputFile, string replaceTokenInOutputFile, bool verbose = true)
        {
            var existingRepresentation = File.ReadAllText(outputFile) ?? string.Empty;
            if (!existingRepresentation?.Contains(replaceTokenInOutputFile) == true)
            {
                throw new OptionsException($"'replace-token-in-output-file' '{replaceTokenInOutputFile}' was not found in output file '{outputFile}'");
            }
            if (verbose)
            {
                outWriter.WriteLine($"Replacing the 'magic token' '{replaceTokenInOutputFile}' with {resultRepresentation} in file '{outputFile}'");
            }
#pragma warning disable CS8603 // Possible null reference return.
            // No, we can't get null reference return here, since we use ?? string.Empty.
            return existingRepresentation;
#pragma warning restore CS8603 // Possible null reference return.
        }

        private int ExecuteWithExceptionHandlingAndHelpDisplay<TOptions>(TOptions options, Func<TOptions, int> verbFunc) where TOptions : BaseOptions, new()
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

        private int GenerateMermaidErDiagramFromCSharp(MermaidErDiagramFromCSharpOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "Mermaid ER diagram", options =>
            {
                var diagramGenerator = new ErDiagramGenerator(options);
                if (!string.IsNullOrEmpty(options.OutputFile))
                {
                    WarnIfDeprecatedIsUsed(options.ExcludeAllAttributes.HasValue, Constants.MermaidErDiagramFromCsharp.ExcludeAllAttributesOption, Constants.MermaidErDiagramFromCsharp.AttributeTypeExclusionOption);
                    WarnIfDeprecatedIsUsed(options.ExcludeForeignkeyAttributes.HasValue, Constants.MermaidErDiagramFromCsharp.ExcludeForeignkeyAttributesOption, Constants.MermaidErDiagramFromCsharp.AttributeTypeExclusionOption);
                    WarnIfDeprecatedIsUsed(options.ExcludeAllRelationships.HasValue, Constants.MermaidErDiagramFromCsharp.ExcludeAllRelationshipsOption, Constants.MermaidErDiagramFromCsharp.RelationshipTypeExclusionOption);
                }
                return GenerateMermaidDiagramFromCSharp(options, diagramGenerator);
            });
        }

        private void WarnIfDeprecatedIsUsed(bool isDeprecatedOptionUsed, string deprecatedOption, string replacedByOption)
        {
            if (isDeprecatedOptionUsed)
            {
                outWriter.WriteLine($"Warning! The option '{deprecatedOption}' is deprecated. Use '{replacedByOption}' instead.");
            }
        }

        private int GenerateMermaidErDiagramFromEfCore(MermaidErDiagramFromEfCoreOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "Mermaid ER diagram", options =>
            {
                var diagramGenerator = new ErDiagramGenerator(options);
                return GenerateMermaidDiagramFromCSharp(options, diagramGenerator);
            });

        }

        private int GenerateMermaidClassDiagramFropmCSharp(MermaidClassDiagramFromCSharpOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "Mermaid class diagram", options =>
            {
                var diagramGenerator = new ClassDiagramGenerator(new TypeLoaderByReflection(), options);
                return GenerateMermaidDiagramFromCSharp(options, diagramGenerator);
            });
        }

        private int GenerateCSharpFromJsonSchema(CSharpFromJsonSchemaOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "C# code", options =>
            {
                var generator = new CSharpFromJsonSchemaGenerator();
                return generator.Generate(options).Result;
            });
        }

        private int GenerateMermaidClassDiagramFromJsonSchema(MermaidClassDiagramFromJsonSchemaOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "Mermaid class diagram", options =>
            {
                var generator = new MermaidClassDiagramFromJsonSchemaGenerator();
                var treeShakingDiagramFilter = GetTreeShakingDiagramFilter(options.TreeShakingRoots);
                return generator.Generate(options, treeShakingDiagramFilter).Result;
            });
        }

        private int GenerateMermaidErDiagramFromJsonSchema(MermaidErDiagramFromJsonSchemaOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "Mermaid ER diagram", options =>
            {
                var generator = new MermaidErDiagramFromJsonSchemaGenerator();
                var treeShakingDiagramFilter = GetTreeShakingDiagramFilter(options.TreeShakingRoots);
                return generator.Generate(options, treeShakingDiagramFilter).Result;
            });
        }

        private int GenerateOptionsFromCommandline(OptionsFromCommandlineOptions options, string[] args)
        {
            return ExecuteWithOptionsFromFileExceptionHandlingAndHelpDisplay(options, args, "dry-gen options", options =>
            {
                return OptionsFromCommandlineGenerator.Generate(options);
            });
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

        private void WriteGeneratedRepresentationToConsoleOrFile(BaseOptions options, string generatedRepresentation, string? existingRepresentation)
        {
            if (string.IsNullOrEmpty(options.OutputFile))
            {
                outWriter.Write(generatedRepresentation);
            }
            else
            {
                var outputFile = Path.GetFullPath(options.OutputFile);
                CreateMissingOutputDirectory(outputFile);
                if (!string.IsNullOrWhiteSpace(existingRepresentation) && !string.IsNullOrWhiteSpace(options.ReplaceTokenInOutputFile))
                {
                    generatedRepresentation = existingRepresentation.Replace(options.ReplaceTokenInOutputFile, generatedRepresentation);
                }
                File.WriteAllText(outputFile, generatedRepresentation);
            }
        }

        [ExcludeFromCodeCoverage] // We uses the tmp files feature from the .Net runtime so the tests don't have any issues with directory names
        private static void CreateMissingOutputDirectory(string outputFile)
        {
            var outputDirectory = Path.GetDirectoryName(outputFile);
            if (outputDirectory == null)
            {
                throw new InvalidOperationException($"Can't get directory from output file'{outputFile}'.");
            }
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
        }

        private static string GenerateMermaidDiagramFromCSharp(MermaidFromCSharpBaseOptions options, IDiagramGenerator diagramGenerator)
        {
            var assembly = LoadAsseblyFromFile(options.InputFile);
            var namespaceFilters = options.IncludeNamespaces?.Select(x => new IncludeNamespaceTypeFilter(x)).ToArray() ?? Array.Empty<IncludeNamespaceTypeFilter>();
            var typeFilters = new List<ITypeFilter> { new AnyChildFiltersTypeFilter(namespaceFilters) };
            if (options.IncludeTypeNames?.Any() == true)
            {
                var typeNameFilters = options.IncludeTypeNames.Select(x => new IncludeTypeNameTypeFilter(x)).ToArray();
                typeFilters.Add(new AnyChildFiltersTypeFilter(typeNameFilters));
            }
            if (options.ExcludeTypeNames?.Any() == true)
            {
                var typeNameFilters = options.ExcludeTypeNames.Select(x => new ExcludeTypeNameTypeFilter(x)).ToArray();
                typeFilters.Add(new AllChildFiltersTypeFilter(typeNameFilters));
            }
            var excludePropertyNamesFilters = options.ExcludePropertyNames?.Select(x => new ExcludePropertyNamePropertyFilter(x)).ToArray() ?? Array.Empty<IPropertyFilter>();
            var nameRewriter = new ReplaceNameRewriter(options.NameReplaceFrom ?? string.Empty, options.NameReplaceTo ?? string.Empty);
            var treeShakingDiagramFilter = GetTreeShakingDiagramFilter(options.TreeShakingRoots);
            return diagramGenerator.Generate(assembly, typeFilters, excludePropertyNamesFilters, nameRewriter, treeShakingDiagramFilter);
        }

        private static Assembly LoadAsseblyFromFile(string? inputFile)
        {
            /// It seems like Assembly.Load from a file name will hold the file open, 
            /// and thus our tests cannot clean up by deleting the tmp files they uses, so we read the file to memory our self...
            if (string.IsNullOrWhiteSpace(inputFile))
            {
                throw new InvalidOperationException("Input file must be specified as the option -i/--input-file on the command line, or as input-file in the option file.");
            }
            var inputDirectory = Path.GetDirectoryName(inputFile) ?? throw new InvalidOperationException($"Could not determin directory from inputFile '{inputFile}'");
            AssemblyResolvingHelper.SetupAssemblyResolving(inputDirectory);
            var assemblyBytes = File.ReadAllBytes(inputFile);
            var assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(assemblyBytes));
            return assembly;
        }

        private static TreeShakingDiagramFilter GetTreeShakingDiagramFilter(IEnumerable<string>? treeShakingRoots)
        {
            var treeShakingRootsFilters = treeShakingRoots?.Any() == true ? treeShakingRoots.Select(x => new IncludeTypeNameTypeFilter(x)).ToArray() : null;
            var treeShakingDiagramFilter = new TreeShakingDiagramFilter(treeShakingRootsFilters);
            return treeShakingDiagramFilter;
        }

        [ExcludeFromCodeCoverage] // The loading of assembly dependencies is so difficult to trigger in an automated test, since we need two asseblies in the same directory with dependencies, so this is tested manually (once).
        private static class AssemblyResolvingHelper
        {
            internal static void SetupAssemblyResolving(string inputDirectory)
            {
                AssemblyLoadContext.Default.Resolving += (assemblyContext, assemblyName) =>
                {
                    foreach (var extension in new[] { ".dll", ".exe" })
                    {
                        var assemblyFileName = $"{inputDirectory}{Path.DirectorySeparatorChar}{assemblyName.Name}{extension}";
                        if (File.Exists(assemblyFileName))
                        {
                            var assemblyBytes = File.ReadAllBytes(assemblyFileName);
                            return assemblyContext.LoadFromStream(new MemoryStream(assemblyBytes));
                        }
                    }
                    // We cant find the assembly file, let the runtime try to handle it
                    return null;
                };
            }
        }
    }
}