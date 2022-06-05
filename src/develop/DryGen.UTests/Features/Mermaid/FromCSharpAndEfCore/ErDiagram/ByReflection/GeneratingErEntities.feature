Feature: Generatating Entities in Er diagrams using reflection

To be able to generate Mermaid Er digrams from C# code
As a dry-gen user
I should be able to generate Mermaid entities from C# types by reflection

Background:
	Given this include namespace filter '^Test$'

Scenario: Generates ER entities for all public classes
	Given this C# source code
		"""
		namespace Test
		{
			public class Customer {}
			class Order {} // Non public class, should be filtered out
		}
		"""
	When I generate an ER diagram using reflection
	Then I should get this generated representation
		"""
		erDiagram
			Customer
		
		"""

Scenario: Excludes inner classes from ER diagram
	Given this C# source code
		"""
		namespace Test
		{
			public class Customer
			{
				public class Order {} // Inner class, should be filtered out
			}
		}
		"""
	When I generate an ER diagram using reflection
	Then I should get this generated representation
		"""
		erDiagram
			Customer
		
		"""
