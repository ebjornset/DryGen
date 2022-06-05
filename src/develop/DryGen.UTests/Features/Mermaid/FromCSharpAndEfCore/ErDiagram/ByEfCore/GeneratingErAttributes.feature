Feature: Generating Attributes in Er diagrams using Entity Framework Core

To be able to generate Mermaid Er digrams from C# code
As a dry-gen user
I should be able to generate Mermaid attributes from C# type's properties using metadata from Ef Core DbContexts

Background:
	Given this include namespace filter '^Test$'

Scenario: Generates ER attributes as PK mapped fluent with Ef Core
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class A
			{
				public int APropKey2 { get; set; }
				public int APropKey1 { get; set; }
			}
			public class TestDbContext: DbContext {
				public DbSet<A> Aaa { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<A>().HasKey( x => new { x.APropKey2, x.APropKey1 } );
				}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should get this generated representation
		"""
		erDiagram
			A {
				int APropKey1 PK
				int APropKey2 PK
			}
		
		"""

Scenario: Generates ER attributes as FK for properties used in foreign keys
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		using System.ComponentModel.DataAnnotations;
		namespace Test
		{
			public class A
			{
				[Key]
				public int Id { get; set; }
			}
			public class B
			{
				[Key]
				public int Id { get; set; }
			}
			public class C
			{
				public int AId { get; set; }
				public int BId { get; set; }
				public A A { get; set; }
				public B B { get; set; }
			}
			public class TestDbContext: DbContext {
				public DbSet<A> Aaa { get; set; }
				public DbSet<B> Bbb { get; set; }
				public DbSet<C> Ccc { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<C>().HasNoKey();
				}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should get this generated representation
		"""
		erDiagram
			A {
				int Id PK
			}
			B {
				int Id PK
			}
			C {
				int AId FK
				int BId FK
			}
			A ||..o{ C : ""
			B ||..o{ C : ""

		"""

Scenario: Sort ER attributes by PK, AK, FK, nullable and name
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		using System.ComponentModel.DataAnnotations;
		namespace Test
		{
			public class A
			{
				[Key]
				public int Id { get; set; }
			}
			public class B
			{
				[Key]
				public int Id { get; set; }
			}
			public class C
			{
				public int? A1Prop { get; set; } // Nullable non key, should be last
				public int A2Prop { get; set; } // Mandatory non key, should be forth
				[Key]
				public int Id { get; set; } // PK, should be first
				public int AId { get; set; } // FK, should be third
				public int BId { get; set; } // AK and FK, should be second
				public A A { get; set; }
				public B B { get; set; }
			}
			public class TestDbContext: DbContext {
				public DbSet<A> Aaa { get; set; }
				public DbSet<B> Bbb { get; set; }
				public DbSet<C> Ccc { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<C>().HasAlternateKey( x => x.BId );
					modelBuilder.Entity<C>().Ignore( x => x.B ).HasOne(x => x.B).WithMany().HasForeignKey(x => x.BId);
				}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should get this generated representation
		"""
		erDiagram
			A {
				int Id PK
			}
			B {
				int Id PK
			}
			C {
				int Id PK
				int BId FK "AK"
				int AId FK
				int A2Prop
				int A1Prop "Null"
			}
			A ||..o{ C : ""
			B ||--o{ C : ""

		"""
