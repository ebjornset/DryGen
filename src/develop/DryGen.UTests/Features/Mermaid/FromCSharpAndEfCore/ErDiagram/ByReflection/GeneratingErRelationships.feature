Feature: Generating Relationships in Er diagrams using reflection

To be able to generate Mermaid Er digrams from C# code
As a dry-gen user
I should be able to generate Mermaid relationships from C# type's references by reflection

Background:
	Given this include namespace filter '^Test$'
	And this exclude type name filter '^TestDbContext$'
	And the Er diagram attribute type exclusion 'ExcludeAll'

#Todo identifying relationship from key attribute
@Ignore 
Scenario: Generates ER relations for mandatory bidirectional one to one relationship
	Given this C# source code
		"""
		using System.ComponentModel.DataAnnotations;
		namespace Test
		{
			public class Customer
			{
				public int Id { get; set; }
				[Key]
				public Address Address { get; set; }
			}
			public class Address {
				public int Id { get; set; }
				public Customer Customer { get; set; }
			}
		}
		"""
	When I generate an ER diagram using reflection
	Then I should get this generated representation
		"""
		erDiagram
			Address
			Customer
			Address ||--|| Customer : ""

		"""
