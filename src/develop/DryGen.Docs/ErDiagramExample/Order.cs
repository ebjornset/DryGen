using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DryGen.Docs.ErDiagramExample
{
    [ExcludeFromCodeCoverage] // Just for the generated er diagram example
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public ICollection<OrderLine> Lines { get; set; }
    }
}
