using System.Diagnostics.CodeAnalysis;

namespace DryGen.Docs.ErDiagramExample
{
    [ExcludeFromCodeCoverage] // Just for the generated er diagram example
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
