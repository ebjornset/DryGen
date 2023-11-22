using CommandLine;
using DryGen.Options;
using YamlDotNet.Serialization;

namespace DryGen.Features.VerbsFromOptionsFile;

[Verb(Constants.VerbsFromOptionsFile.Verb, HelpText = "Execute several verbs in one run. You specify each verb in a separate yaml document in the --options-file. The yaml documents should be separated by --- (three hyphes) on its own line in the file.")]
public class VerbsFromOptionsFileOptions : BaseOptions
{
}
