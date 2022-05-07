Feature: Generating Classes in Class diagrams

A short summary of the feature

Background:
	Given this include namespace filter '^Test$'

Scenario: Generates classes for all public classes
	Given this C# source code
		"""
		namespace Test
		{
			public class Customer {}
			class Order {} // Non public class, should be filtered out
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Customer {
			}
		
		"""

Scenario: Excludes inner classes from Class diagram
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
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Customer {
			}
		
		"""

Scenario: Generates interface annotation for interfaces
	Given this C# source code
		"""
		namespace Test
		{
			public interface ICustomer {}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class ICustomer {
				<<interface>>
			}
		
		"""

Scenario: Generates enumeration annotation for enums
	Given this C# source code
		"""
		namespace Test
		{
			public enum CustomerType {}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class CustomerType {
				<<enumeration>>
			}
		
		"""

Scenario: Generates abstract annotation for abstract classes
	Given this C# source code
		"""
		namespace Test
		{
			public abstract class Customer {}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Customer {
				<<abstract>>
			}
		
		"""

Scenario: Exclude object and enum automatic
	Given this C# source code
		"""
		namespace Test
		{
			public class Customer {}
			public enum CustomerType {}
		}
		"""
	And exactly these include namespace filters
		| Regex    |
		| ^Test$   |
		| ^System$ |
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Customer {
			}
			class CustomerType {
				<<enumeration>>
			}
		
		"""
