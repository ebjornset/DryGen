namespace DryGen.MermaidFromDotnetDepsJson.DeptsModel;

internal class DependencyRef : BaseModelElement
{
    public DependencyRef(string id) : base(id) { }
    public Dependency? Dependency { get; set; }
}
