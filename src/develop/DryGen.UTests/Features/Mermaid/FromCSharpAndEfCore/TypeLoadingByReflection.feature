Feature: Type loading by refelction

A short summary of the feature

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
