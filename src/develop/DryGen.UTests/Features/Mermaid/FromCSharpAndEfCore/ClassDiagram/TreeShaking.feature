Feature: Tree shaking in Class diagrams

To be able to generate Class diagrams for parts of a solution in an effective manner
As a dry-gen user
I want to be able to shake of uninteresting types from my Class diagrams

Background:
	Given these tree shaking roots
		| Regex     |
		| ICustomer |

Scenario: Tree shaking should remove all classes when no types matches the roots
	Given this C# source code
		"""
		namespace Test
		{
			public interface IOrder {}
			public class Order: IOrder {}
		}
		"""
	When I generate a Class diagram
	Then I should get no exceptions
	And I should get this generated representation
		"""
		classDiagram
		
		"""

Scenario: Tree shakeing should keep classes implementing interfaces
	Given this C# source code
		"""
		namespace Test
		{
			public interface IOrder {}
			public interface ICustomer {}
			public class Order: IOrder {}
			public class Customer: ICustomer {}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Customer {
			}
			class ICustomer {
				<<interface>>
			}
			Customer ..|> ICustomer
		
		"""

