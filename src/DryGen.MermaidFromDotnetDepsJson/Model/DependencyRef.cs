namespace DryGen.MermaidFromDotnetDepsJson.Model;

internal class DependencyRef : BaseModelElement
{
    public DependencyRef(string id) : base(id) { }
    public Dependency? Dependency { get; set; }
}
