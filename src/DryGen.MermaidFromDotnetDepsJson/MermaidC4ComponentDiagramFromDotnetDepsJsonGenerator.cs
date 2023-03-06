using System.Text;
using System.Threading.Tasks;

namespace DryGen.MermaidFromDotnetDepsJson;

public class MermaidC4ComponentDiagramFromDotnetDepsJsonGenerator
{
    public async Task<string> Generate(IMermaidC4ComponentDiagramFromDotnetDepsJsonOptions options)
    {
        await Task.CompletedTask;
        var sb = new StringBuilder().AppendLine("C4Component");
        return sb.ToString();
    }
}