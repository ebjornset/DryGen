Feature: Loading the Entity Framework Core DbContext

To be able to generate Mermaid Er digrams from C# code using the metadata in Ef Core DbContexts
As a dry-gen user
I should be able to load metadata from Ef Core DbContexts

Scenario: Should get exception if no DbContext ctor takes option as an argument
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class TestDbContext: DbContext {
				public TestDbContext(<Ctor args>) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder) {}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should get an exception containing the text "TestDbContext has no public constructor with DbContextOptions as a parameter"
Examples:
	| Ctor args    |
	|              |
	| int intParam |

Scenario: Should not get exception if a DbContext ctor takes option as an argument
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class TestDbContext: DbContext {
				public TestDbContext(<Ctor args>) : base(optionsParam) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder) {}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should not get an exception
Examples:
	| Ctor args                                                                                   |
	| DbContextOptions<TestDbContext> optionsParam                                                |
	| DbContextOptions<TestDbContext> optionsParam, int intParam                                  |
	| int intParam, DbContextOptions<TestDbContext> optionsParam                                  |
	| DbContextOptions<TestDbContext> optionsParam, DbContextOptions<TestDbContext> optionsParam2 |

Scenario: Should not look at abstract DbContext
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public abstract class TestDbContext: DbContext {
				public TestDbContext(int intParam) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder) {}
			}
		}
		"""
	When I generate an ER diagram using EF Core
	Then I should not get an exception
