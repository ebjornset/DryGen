using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DryGen.Templates.Mermaid;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Order> Orders { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public bool? IsCompleted { get; set; }
    public Customer Customer { get; set; }
    public ICollection<OrderLine> Lines { get; set; }
}

public class OrderLine
{
    public int LineNumber { get; set; }
    public int Quantity { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ProductCategoryId { get; set; }
    public ProductCategory ProductCategory { get; set; }
    public ICollection<Stock> InStock { get; set; }
}

public class ProductCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Product> Products { get; set; }
}

public class Warehouse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Stock> ProductsInStock { get; set; }
}

public class Stock
{
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }
    public int Quantity { get; set; }
}

public class ExampleDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public ExampleDbContext(DbContextOptions options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasKey(x => x.Id);
        modelBuilder.Entity<Order>().HasKey(x => x.Id);
        modelBuilder.Entity<OrderLine>().HasKey(x => new { x.OrderId, x.LineNumber });
        modelBuilder.Entity<Product>().HasKey(x => x.Id);
        modelBuilder.Entity<ProductCategory>().HasKey(x => x.Id);
        modelBuilder.Entity<Warehouse>().HasKey(x => x.Id);
        modelBuilder.Entity<Stock>().HasKey(x => new { x.ProductId, x.WarehouseId });
    }
}
