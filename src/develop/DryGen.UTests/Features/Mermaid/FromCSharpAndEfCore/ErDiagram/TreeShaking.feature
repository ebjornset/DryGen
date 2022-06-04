Feature: Tree shaking in Er diagrams

To be able to generate Entity Relationship diagrams for parts of a solution in an effective manner
As a dry-gen user
I want to be able to shake of uninteresting types from my Er diagrams

Background:
	Given these tree shaking roots
		| Regex    |
		| Customer |
	And the Er diagram attribute type exclusion 'All'

Scenario: Tree shaking should remove all entities when no types matches the roots
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Order {}
		
			public class TestDbContext: DbContext {
				public TestDbContext(DbContextOptions options) : base(options) {}
				public DbSet<Order> Orders { get; set; }
				protected override void OnModelCreating(ModelBuilder modelBuilder)
				{
					modelBuilder.Entity<Order>().HasNoKey();
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
		
		"""
Examples:
	| Structure builder |
	| Reflection        |
	| EfCore            |

Scenario: Tree shaking should keep related entities
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		using System.Collections.Generic;
		using System.ComponentModel.DataAnnotations;
		namespace Test
		{
			public class Customer {
				[Key]
				public int Id { get; set; }
				public IEnumerable<Order> Orders { get; set; }
			}
			public class Order {
				[Key]
				public int Id { get; set; }
				public Customer Customer { get; set; }
			}
			public class NonRelated {}
		
			public class TestDbContext: DbContext {
				public TestDbContext(DbContextOptions options) : base(options) {}
				public DbSet<Customer> Customers { get; set; }
				public DbSet<Order> Orders { get; set; }
				public DbSet<NonRelated> NonRelated { get; set; }
				protected override void OnModelCreating(ModelBuilder modelBuilder)
				{
					modelBuilder.Entity<NonRelated>().HasNoKey();
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
			Customer <From cadinality>..o{ Order : ""
		
		"""
Examples:
	| Structure builder | From cadinality |
	| Reflection        | \|\|            |
	| EfCore            | \|o             |
