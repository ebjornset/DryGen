using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DryGen.Docs.ErDiagramExample
{

    [ExcludeFromCodeCoverage] // Just for the generated er diagram example
    public class ExampleDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public ExampleDbContext(DbContextOptions options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasKey(x => x.Id);
            modelBuilder.Entity<Order>().HasKey(x => x.Id);
            modelBuilder.Entity<OrderLine>().HasKey(x => new { x.OrderId, x.LineNumber });
            modelBuilder.Entity<Product>().HasKey(x => x.Id);
        }
    }
}
