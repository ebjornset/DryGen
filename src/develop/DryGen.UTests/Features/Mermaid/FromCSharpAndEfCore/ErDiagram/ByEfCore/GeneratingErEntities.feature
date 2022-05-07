Feature: Generatating Entities in Er diagrams using Entity Framework Core

A short summary of the feature

Background:
	Given this include namespace filter '^Test$'

Scenario: Generates ER entities for IEntity in the DbContexts Model
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Customer {}	// Referenced in the DbContext -> Should be included
			public class Order {}		// Not referenced in the DbContext or by other entities -> Should be filtered out
			public class TestDbContext: DbContext {
				public DbSet<Customer> Customers { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Customer>().HasNoKey();
				}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should get this generated representation
		"""
		erDiagram
			Customer
		
		"""

Scenario: Generates ER entities for an IEntity used in several DBContexts only once
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Customer {}
			public class TestDbContextA: DbContext {
				public DbSet<Customer> Customers { get; set; }
				public TestDbContextA(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Customer>().HasNoKey();
				}
			}
			public class TestDbContextB: DbContext {
				public DbSet<Customer> Customers { get; set; }
				public TestDbContextB(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Customer>().HasNoKey();
				}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should get this generated representation
		"""
		erDiagram
			Customer
		
		"""

Scenario: Generates ER entities for an IEntity from inherited DBContext only once
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Customer {}
			public abstract class TestDbContext: DbContext {
				public DbSet<Customer> Customers { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Customer>().HasNoKey();
				}
			}
			public class TestDbContextA: TestDbContext {
				public TestDbContextA(DbContextOptions options) : base(options) {}
			}
			public class TestDbContextB: TestDbContext {
				public TestDbContextB(DbContextOptions options) : base(options) {}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should get this generated representation
		"""
		erDiagram
			Customer
		
		"""
