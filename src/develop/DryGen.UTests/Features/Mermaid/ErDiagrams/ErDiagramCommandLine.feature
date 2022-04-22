Feature: Command line for Er diagrams

A short summary of the feature

Background:
	Given this C# source code compiled to a file
	# The commandline argument -i <this assembly filename> will be appended to the command line
		"""
		using System.ComponentModel.DataAnnotations;
		namespace Test
		{
			public class Customer {
				[Key]
				public int PublicProp { get; set; }
				public int? NullableProp { get; set; }
			}
			public class Order {
				public Customer Customer { get; set; }
			}
			public enum Color
			{
				RED,
				BLUE,
				GREEN,
			}
		}
		"""
	And output is spesified as a command line argument
	# The commandline arguments -o and a temporary filename will be appended to the command line

Scenario: Should generate Er diagram from erdiagram command
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
	Then I should get exit code '0'
	And I should get this Mermaid code file
		"""
		erDiagram
			Customer {
				int PublicProp PK
				int NullableProp "Null"
			}
			Order
			Customer ||..|| Order : ""
		
		"""

Scenario: Should generate Er diagram with type inclusion from argument
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --include-typenames            |
		| ^Customer$                     |
	Then I should get exit code '0'
	And I should get this Mermaid code file
		"""
		erDiagram
			Customer {
				int PublicProp PK
				int NullableProp "Null"
			}
		
		"""

Scenario: Should generate Er diagram with attributes excluded from argument
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --exclude-all-attributes       |
		| true                           |
	Then I should get exit code '0'
	And I should get this Mermaid code file
		"""
		erDiagram
			Customer
			Order
			Customer ||..|| Order : ""
		
		"""

Scenario: Should generate Er diagram with attribute key type excluded from argument
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --exclude-attribute-keytypes   |
		| true                           |
	Then I should get exit code '0'
	And I should get this Mermaid code file
		"""
		erDiagram
			Customer {
				int PublicProp
				int NullableProp "Null"
			}
			Order
			Customer ||..|| Order : ""
		
		"""

Scenario: Should generate Er diagram with attribute comment excluded from argument
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --exclude-attribute-comments   |
		| true                           |
	Then I should get exit code '0'
	And I should get this Mermaid code file
		"""
		erDiagram
			Customer {
				int PublicProp PK
				int NullableProp
			}
			Order
			Customer ||..|| Order : ""
		
		"""

Scenario: Should generate Er diagram with foreign key attributes excluded from argument when using Ef Core
	Given this C# source code compiled to a file
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
				[Key]
				public int Id { get; set; } 
				public int AId { get; set; } // FK, should be removed
				public int BId { get; set; } // FK, should be removed
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
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                             |
		| mermaid-er-diagram-from-efcore  |
		| --exclude-foreignkey-attributes |
		| true                            |
	Then I should get exit code '0'
	And I should get this Mermaid code file
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
			}
			A ||..o{ C : ""
			B ||..o{ C : ""
		
		"""

Scenario: Should generate Er diagram with attribute excluded by property name from argument
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --exclude-propertynames        |
		| Nullable.*                     |
	Then I should get exit code '0'
	And I should get this Mermaid code file
		"""
		erDiagram
			Customer {
				int PublicProp PK
			}
			Order
			Customer ||..|| Order : ""
		
		"""

Scenario: Should generate Er diagram with relationships excluded from argument
	Given the Er diagram relationship exclusion 'All'
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --exclude-all-relationships    |
		| true                           |
	Then I should get exit code '0'
	And I should get this Mermaid code file
		"""
		erDiagram
			Customer {
				int PublicProp PK
				int NullableProp "Null"
			}
			Order
		
		"""