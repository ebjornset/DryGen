Feature: Command line for generation Mermaid Er diagrams from C#, both by reflection and from EF Core

To be able to generate other representation of C# code
As a dry-gen user
I should be able to generate Mermaid ER diagrams from a .Net assembly with the verbs 'mermaid-er-diagram-from-csharp' and 'mermaid-er-diagram-from-efcore'

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
	And I should get this generated representation file
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
	And I should get this generated representation file
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
	And I should get this generated representation file
		"""
		erDiagram
			Customer
			Order
			Customer ||..|| Order : ""
		
		"""

Scenario: Should generate Er diagram with attributes excluded from type exclusion argument
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --attribute-type-exclusion     |
		| all                            |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		erDiagram
			Customer
			Order
			Customer ||..|| Order : ""
		
		"""

Scenario: Should generate Er diagram with attributes excluded from type exclusion argument over deprecated
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --attribute-type-exclusion     |
		| all                            |
		| --<Deprecated option>          |
		| <Deprecated option value>      |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		erDiagram
			Customer
			Order
			Customer ||..|| Order : ""
		
		"""
Examples:
	| Deprecated option             | Deprecated option value |
	| exclude-all-attributes        | false                   |
	| exclude-foreignkey-attributes | false                   |

Scenario: Should generate Er diagram with attribute key type excluded from argument
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --exclude-attribute-keytypes   |
		| true                           |
	Then I should get exit code '0'
	And I should get this generated representation file
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
	And I should get this generated representation file
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
	And I should get this generated representation file
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
	And I should get this generated representation file
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
	And I should get this generated representation file
		"""
		erDiagram
			Customer {
				int PublicProp PK
				int NullableProp "Null"
			}
			Order
		
		"""

Scenario: Should generate Er diagram with relationships excluded from type exclusion argument
	Given the Er diagram relationship exclusion 'All'
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --relationship-type-exclusion  |
		| all                            |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		erDiagram
			Customer {
				int PublicProp PK
				int NullableProp "Null"
			}
			Order
		
		"""

Scenario: Should generate Er diagram with relationships excluded from type exclusion argument over deprected
	Given the Er diagram relationship exclusion 'All'
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --relationship-type-exclusion  |
		| all                            |
		| --exclude-all-relationships    |
		| false                          |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		erDiagram
			Customer {
				int PublicProp PK
				int NullableProp "Null"
			}
			Order
		
		"""

Scenario: Should tree shake Er diagram from argument
	Given this C# source code compiled to a file
	# The commandline argument -i <this assembly filename> will be appended to the command line
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Order {
			}
			public class NoMatchOrder {
			}
			public class TestDbContext: DbContext {
				public DbSet<Order> Orders { get; set; }
				public DbSet<NoMatchOrder> NoMatchOrders { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Order>().HasNoKey();
		            modelBuilder.Entity<NoMatchOrder>().HasNoKey();
				}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                  |
		| <Verb>               |
		| --tree-shaking-roots |
		| ^Order$              |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		erDiagram
			Order
		
		"""
Examples:
	| Verb                           |
	| mermaid-er-diagram-from-csharp |
	| mermaid-er-diagram-from-efcore |

Scenario: Show deprecation warning for deprecated options when output file is specified
	When I call the program with this command line arguments
		| Arg                            |
		| mermaid-er-diagram-from-csharp |
		| --<Deprecated option>          |
		| <Deprecated option value>      |
	Then I should get exit code '0'
	And I should find the text "Warning! The option '<Deprecated option>' is deprecated. Use '<Replaced by option>' instead." in console out
Examples:
	| Deprecated option             | Replaced by option          | Deprecated option value |
	| exclude-all-attributes        | attribute-type-exclusion    | true                    |
	| exclude-foreignkey-attributes | attribute-type-exclusion    | false                   |
	| exclude-all-relationships     | relationship-type-exclusion | true                    |