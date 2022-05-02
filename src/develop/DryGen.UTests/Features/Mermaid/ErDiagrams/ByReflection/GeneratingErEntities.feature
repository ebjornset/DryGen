Feature: Generatating Entities in Er diagrams using reflection

A short summary of the feature

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
