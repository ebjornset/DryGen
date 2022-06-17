using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DryGen.Docs.ErDiagramExample
{
    [ExcludeFromCodeCoverage] // Just for the generated er diagram example
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
