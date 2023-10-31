Feature: Tree shaking in Class diagrams

To be able to generate Class diagrams for parts of a solution in an effective manner
As a dry-gen user
I should be able to shake of uninteresting types from my Class diagrams

Background:
	Given these tree shaking roots
		| Regex     |
		| ICustomer |
		| ^Order    |

Scenario: Tree shaking should remove all classes when no types matches the roots
	Given this C# source code
		"""
		namespace Test
		{
			public interface IOrder {}
			public class NoMatchOrder: IOrder {}
		}
		"""
	When I generate a Class diagram
	Then I should get no exceptions
	And I should get this generated representation
		"""
		classDiagram
		
		"""

Scenario: Tree shakeing should keep classes implementing interfaces and vice versa
	Given this C# source code
		"""
		namespace Test
		{
			public interface IOrder {}
			public interface ICustomer {}
			public interface INoMatch {}
			public class Order: IOrder {}
			public class Customer: ICustomer {}
			public class NoMatch: INoMatch {}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Customer
			class ICustomer {
				<<interface>>
			}
			class IOrder {
				<<interface>>
			}
			class Order
			Customer ..|> ICustomer
			Order ..|> IOrder
		
		"""

Scenario: Tree shakeing should keep classes for scalar instance properties with a getter referencing other know types
	Given this C# source code
		"""
		namespace Test
		{
			public class Order {
				public Customer Customer { get; }
			}
			public class Customer {
			}
			public class NoMatch {}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Customer
			class Order
			Order --> "0..1" Customer
		
		"""

Scenario: Tree shakeing should keep classes for instance collection properties with a getter referencing other know types
	Given this C# source code
		"""
		using System.Collections.Generic;
		using System.Collections.ObjectModel;
		namespace Test
		{
			public class Customer {
				public IEnumerable<Order> GetOrders { get; }
				public IEnumerable<Order> GetSetOrders { get; set; }
			}
			public class Order {}
			public class NoMatch {}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Customer
			class Order
			Customer *-- "*" Order : get
			Customer *-- "*" Order : get set
		
		"""

Scenario: Tree shakeing should keep classes with dependency for scalar constructor parameters
	Given this C# source code
		"""
		namespace Test
		{
			public class Customer {
			}
			public class Order {
				public Order(Customer customer) {} 
			}
			public class NoMatch {
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Customer
			class Order
			Order ..> Customer
		
		"""
