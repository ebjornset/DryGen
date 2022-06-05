Feature: Generating Relationships in Er diagrams

To be able to generate Mermaid Er digrams from C# code
As a dry-gen user
I should be able to generate Mermaid relationships, either by reflection or from Ef Core DbContexts

Background:
	Given this include namespace filter '^Test$'
	And this exclude type name filter '^TestDbContext$'
	And the Er diagram attribute type exclusion 'All'

Scenario: Generates ER relations for all public properties referencing other know types
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Order
			{
				public int Id { get; set; }
				public Address Address { get; set; }
				internal Address InvoiceAddress { get; set; }	// Internal --> Filtered out
				protected Address DeliveryAddress { get; set; }	// Protected --> Filtered out
				private Address ConsigneeAddress { get; set; }	// Private --> Filtered out
			}
			public class Address {
				public int Id { get; set; }
			}
		
			public class TestDbContext: DbContext {
				public DbSet<Order> Orders { get; set; }
				public DbSet<Address> Addresses { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
				{
					modelBuilder.Entity<Order>().HasKey(x => x.Id);
					modelBuilder.Entity<Address>().HasKey(x => x.Id);
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			Address
			Order
			Address <From cadinality>..<To cardinality> Order : ""
		
		"""
Examples:
	| Structure builder | From cadinality | To cardinality | Comments                                            |
	| Reflection        | \|\|            | \|\|           |                                                     |
	| EfCore            | \|o             | o{             | Optional unidirectional relationship to one enitity |

Scenario: Generates ER relations for mandatory unidirectional relationship to one enitity
	Given this C# source code
		"""
		using System.ComponentModel.DataAnnotations;
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Order
			{
				public int Id { get; set; }
				[Required]
				public Address Address { get; set; }
			}
			public class Address {
				public int Id { get; set; }
			}
		
			public class TestDbContext: DbContext {
				public DbSet<Order> Orders { get; set; }
				public DbSet<Address> Addresses { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
				{
					modelBuilder.Entity<Order>().HasKey(x => x.Id);
					modelBuilder.Entity<Address>().HasKey(x => x.Id);
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			Address
			Order
			Address <From cadinality>..<To cardinality> Order : ""
		
		"""
Examples:
	| Structure builder | From cadinality | To cardinality |
	| Reflection        | \|\|            | \|\|           |
	| EfCore            | \|\|            | o{             |

Scenario: Generates ER relations with property name as label
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Order
			{
				public int Id { get; set; }
				public Address InvoiceAddress { get; set; }
				public Address DeliveryAddress { get; set; }
			}
			public class Address {
				public int Id { get; set; }
			}
		
			public class TestDbContext: DbContext {
				public DbSet<Order> Orders { get; set; }
				public DbSet<Address> Addresses { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
				{
					modelBuilder.Entity<Order>().HasKey(x => x.Id);
					modelBuilder.Entity<Address>().HasKey(x => x.Id);
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			Address
			Order
			Address <From cadinality>..<To cardinality> Order : "delivery"
			Address <From cadinality>..<To cardinality> Order : "invoice"
		
		"""
Examples:
	| Structure builder | From cadinality | To cardinality | Comments                                            |
	| Reflection        | \|\|            | \|\|           |                                                     |
	| EfCore            | \|o             | o{             | Optional unidirectional relationship to one enitity |

Scenario: Generates ER relations for all public collection properties referencing other know types
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		using System.Collections.Generic;
		namespace Test
		{
			public class Customer {
				public int Id { get; set; }
				public <Collection type><Order> Orders { get; set; }
			}
			public class Order {
				public int Id { get; set; }
			}
		
			public class TestDbContext: DbContext {
				public DbSet<Customer> Addresses { get; set; }
				public DbSet<Order> Orders { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
				{
					modelBuilder.Entity<Customer>().HasKey(x => x.Id);
					modelBuilder.Entity<Order>().HasKey(x => x.Id);
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			Customer
			Order
			Customer <From cadinality>..<To cardinality> Order : ""
		
		"""
Examples:
	| Collection type | Structure builder | From cadinality | To cardinality | Comments                                         |
	| IEnumerable     | Reflection        | \|\|            | o{             |                                                  |
	| ICollection     | Reflection        | \|\|            | o{             |                                                  |
	| IList           | Reflection        | \|\|            | o{             |                                                  |
	| IEnumerable     | EfCore            | \|o             | o{             | Many to one optional unidirectional relationship |
	| ICollection     | EfCore            | \|o             | o{             | Many to one optional unidirectional relationship |
	| IList           | EfCore            | \|o             | o{             | Many to one optional unidirectional relationship |

Scenario: Generates ER bidiractional relations for one to many relations only from the many side
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		using System.Collections.Generic;
		namespace Test
		{
			public class Customer {
				public int Id { get; set; }
				public IEnumerable<Order> Orders { get; set; }
			}
			public class Order {
				public int Id { get; set; }
				public Customer Customer { get; set; }
				public Product Product { get; set; }
			}
			public class Product {
				public int Id { get; set; }
				public IEnumerable<Order> Orders { get; set; }
			}
		
			public class TestDbContext: DbContext {
				public DbSet<Customer> Addresses { get; set; }
				public DbSet<Order> Orders { get; set; }
				public DbSet<Product> Products { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
				{
					modelBuilder.Entity<Customer>().HasKey(x => x.Id);
					modelBuilder.Entity<Order>().HasKey(x => x.Id);
					modelBuilder.Entity<Product>().HasKey(x => x.Id);
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			Customer
			Order
			Product
			Customer <From cadinality>..<To cardinality> Order : ""
			Product <From cadinality>..<To cardinality> Order : ""
		
		"""
Examples:
	| Structure builder | From cadinality | To cardinality | Comments                                         |
	| Reflection        | \|\|            | o{             |                                                  |
	| EfCore            | \|o             | o{             | Many to one optional bidirectional relationships |

Scenario: Generates no ER relations when Er diagram relationship level is none
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Order
			{
				public int Id { get; set; }
				public Address Address { get; set; }
			}
			public class Address {
				public int Id { get; set; }
			}
		
			public class TestDbContext: DbContext {
				public DbSet<Order> Orders { get; set; }
				public DbSet<Address> Addresses { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
				{
					modelBuilder.Entity<Order>().HasKey(x => x.Id);
					modelBuilder.Entity<Address>().HasKey(x => x.Id);
				}
			}
		}
		"""
	And the Er diagram relationship exclusion 'All'
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			Address
			Order
		
		"""
Examples:
	| Structure builder |
	| Reflection        |
	| EfCore            |

Scenario: Does not generate ER relation when one of the enitites is filtered out
	Given this exclude type name filter '^Excluded.*'
	And this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		using System.Collections.Generic;
		namespace Test
		{
			public class ExcludedOne {
				public int Id { get; set; }
			}
			public class ExcludedTwo
			{
				public int Id { get; set; }
				public IncludedTwo IncludedTwo { get; set; }
				public IEnumerable<IncludedOne> IncludedOnes { get; set; }
			}
			public class IncludedOne {
				public int Id { get; set; }
				public ExcludedOne ExcludedOne { get; set;}
			}
			public class IncludedTwo {
				public int Id { get; set; }
				public IEnumerable<ExcludedOne> ExcludedOnes { get; set; }
			}
		
			public class TestDbContext: DbContext {
				public DbSet<ExcludedOne> ExcludedOnes { get; set; }
				public DbSet<ExcludedTwo> ExcludedTwos { get; set; }
				public DbSet<IncludedOne> IncludedOnes { get; set; }
				public DbSet<IncludedTwo> IncludedTwos { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
				{
					modelBuilder.Entity<ExcludedOne>().HasKey(x => x.Id);
					modelBuilder.Entity<ExcludedTwo>().HasKey(x => x.Id);
					modelBuilder.Entity<IncludedOne>().HasKey(x => x.Id);
					modelBuilder.Entity<IncludedTwo>().HasKey(x => x.Id);
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			IncludedOne
			IncludedTwo
		
		"""
Examples:
	| Structure builder |
	| Reflection        |
	| EfCore            |
