namespace DryGen.MermaidFromCSharp.NameRewriters;

public class ReplaceNameRewriter : INameRewriter
{
    private readonly string replace;
    private readonly string replacement;

    public ReplaceNameRewriter(string replace, string replacement)
    {
        this.replace = replace;
        this.replacement = replacement;
    }

    public string Rewrite(string name)
    {
        var result = replace?.Length == 0 ? name : name.Replace(replace, replacement);
        return result;
    }
}