Feature: Type loading by reflection

To be able to generate other representation of C# code
As a dry-gen user
I should be able to use reflection to load C# types from an assembly

Background:
	Given this C# source code
		"""
		namespace Test
		{
			public class Customer {}
		}
		"""

Scenario: Includes inherited types  by default
	When I load the types to include in the diagram
	Then I get this list of types:
		| Namespace | Name     |
		| Test      | Customer |
		| System    | Object   |

Scenario: Includes inherited types when they matches the filters
	Given these include namespace filters
		| Regex  |
		| Test   |
		| System |
	When I load the types to include in the diagram
	Then I get this list of types:
		| Namespace | Name     |
		| Test      | Customer |
		| System    | Object   |
