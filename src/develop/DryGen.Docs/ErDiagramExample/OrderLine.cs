using System.Diagnostics.CodeAnalysis;

namespace DryGen.Docs.ErDiagramExample
{
    [ExcludeFromCodeCoverage] // Just for the generated er diagram example
    public class OrderLine
    {
        public int LineNumber { get; set; }
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
