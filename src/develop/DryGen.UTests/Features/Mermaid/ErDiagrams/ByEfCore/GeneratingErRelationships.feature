Feature: Generating Relationships in Er diagrams using Entity Framework Core

A short summary of the feature

Background:
	Given this include namespace filter '^Test$'
	And this exclude type name filter '^TestDbContext$'
	And the Er diagram attribute type exclusion 'All'


Scenario: Generates ER relations as identifying for key relationsskips and non-identifying for others
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class A
			{
				public int Id { get; set; }
				public int KeyBId { get;set; }
				public int NonKeyBId { get;set; }
				public B KeyB { get; set; }
				public B NonKeyB { get; set; }
			}
			public class B {
				public int Id { get; set; }
			}
		
			public class TestDbContext: DbContext {
				public DbSet<A> Aaa { get; set; }
				public DbSet<B> Bbb { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
				{
					modelBuilder.Entity<A>().HasKey(x => x.Id);
					modelBuilder.Entity<A>().HasAlternateKey(x => x.KeyBId);
					modelBuilder.Entity<A>().Ignore(x => x.KeyB).HasOne(x => x.KeyB).WithOne().HasForeignKey<A>( x => x.KeyBId);
					modelBuilder.Entity<B>().HasKey(x => x.Id);
				}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should get this Mermaid code
		"""
		erDiagram
			A
			B
			B ||--o{ A : "key"
			B ||..o{ A : "non key"
		
		"""

Scenario: Generates ER relations for mandatory bidirectional one to one relationship
	Given this C# source code
		"""
		using System.ComponentModel.DataAnnotations;
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Customer
			{
				public int Id { get; set; }
				[Required]
				public Address Address { get; set; }
			}
			public class Address {
				public int Id { get; set; }
				[Required]
				public Customer Customer { get; set; }
			}
		
			public class TestDbContext: DbContext {
				public DbSet<Customer> Orders { get; set; }
				public DbSet<Address> Addresses { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Customer>().HasKey(x => x.Id);
		            modelBuilder.Entity<Address>().HasKey(x => x.Id);
					modelBuilder.Entity<Address>().Ignore(x => x.Customer).HasOne(x => x.Customer).WithOne(x => x.Address).HasForeignKey<Customer>();
				}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should get this Mermaid code
		"""
		erDiagram
			Address
			Customer
			Address ||--|| Customer : ""
		
		"""

Scenario: Generates ER relations with labels when the property name and type name is different
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		using System.Collections.Generic;
		namespace Test
		{
			public class Customer {
				public int Id { get; set; }
				public IEnumerable<Order> Orders { get; set; } // Should have no name
				public IEnumerable<Order> PriorityOrders { get; set; }
				public IEnumerable<Order> OrdersThatHasLowPriority { get; set; }
				public IEnumerable<Order> Orderes { get; set; } // Should have no name, based on special plural rewrite rule [SingularName] -> [SingularName]es, e.g. Address -> Addresses
				public IEnumerable<Order> Ordeies { get; set; } // Should have no name, based on special plural rewrite rule [SingularName] -> [SingularNam]ies, e.g. Country -> Countries
				public Address Address { get; set; }
			}
			public class Order
			{
				public int Id { get; set; }
				public Address Address { get; set; }
				public Address DeliveryAddress { get; set; }
				public Address AddressForInvoice { get; set; }
				public Address DropOff { get; set; }
				public Product MainOrderedProduct { get; set; } // Bidirectional, so this singilar to plural property name should be used as the base for the label
			}
			public class Address {
				public int Id { get; set; }
				public Customer OwningCustomer { get; set; }
			}
			public class Product {
				public int Id { get; set; }
				public IEnumerable<Order> MainProductInOrders { get; set; }
			}
		
			public class TestDbContext: DbContext {
				public DbSet<Customer> Customers { get; set; }
				public DbSet<Order> Orders { get; set; }
				public DbSet<Address> Addresses { get; set; }
				public DbSet<Product> Products { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
				{
					modelBuilder.Entity<Customer>().HasKey(x => x.Id);
					modelBuilder.Entity<Order>().HasKey(x => x.Id);
					modelBuilder.Entity<Address>().HasKey(x => x.Id);
					modelBuilder.Entity<Product>().HasKey(x => x.Id);
					modelBuilder.Entity<Customer>().Ignore(x => x.Address).HasOne(x => x.Address).WithOne(x => x.OwningCustomer).HasForeignKey<Address>();
				}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should get this Mermaid code
		"""
		erDiagram
			Address
			Customer
			Order
			Product
			Address |o..o{ Order : ""
			Address |o..o{ Order : "address for invoice"
			Address |o..o{ Order : "delivery"
			Address |o..o{ Order : "drop off"
			Customer ||--o| Address : "owning"
			Customer |o..o{ Order : ""
			Customer |o..o{ Order : ""
			Customer |o..o{ Order : ""
			Customer |o..o{ Order : "orders that has low priority"
			Customer |o..o{ Order : "priority orders"
			Product |o..o{ Order : "main ordered"
		
		"""
