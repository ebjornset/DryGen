Feature: Generatating Entities in Er diagrams using Entity Framework Core

To be able to generate Mermaid Er digrams from C# code
As a dry-gen user
I should be able to generate Mermaid entities from C# types using metadata from Ef Core DbContexts

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

Scenario: Generates ER entities for generic types with - in the name in stead of ~
	#NB! We should probably generate a more meaningful name based on the type of the bound generic parameter instead of the internal class number (1 in this example)
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class PrimitiveDto<T> {
				public T Value { get; set; }
			}
			public class TestDbContext: DbContext {
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
				{
					modelBuilder.Entity<PrimitiveDto<int>>().HasNoKey();
				}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should get this generated representation
		"""
		erDiagram
			PrimitiveDto-1 {
				int Value
			}
		
		"""
