Feature: Generating relationships in Class diagrams

A short summary of the feature

Background:
	Given this include namespace filter '^Test$'

Scenario: Generates realization for classes implemeting interfaces
	Given this C# source code
		"""
		namespace Test
		{
			public interface IOrder {}
			public interface IDeliverable {}
			public class BaseOrder: IOrder, IDeliverable {}
			public class Order: BaseOrder {}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class BaseOrder {
			}
			class IDeliverable {
				<<interface>>
			}
			class IOrder {
				<<interface>>
			}
			class Order {
			}
			BaseOrder ..|> IOrder
			BaseOrder ..|> IDeliverable
			Order --|> BaseOrder
		
		"""

Scenario: Generates inheritance for interfaces extending other interfaces
	Given this C# source code
		"""
		namespace Test
		{
			public interface IOrder : IDeliverable, IShippable {}
			public interface IDeliverable {}
			public interface IShippable {}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class IDeliverable {
				<<interface>>
			}
			class IOrder {
				<<interface>>
			}
			class IShippable {
				<<interface>>
			}
			IOrder --|> IDeliverable
			IOrder --|> IShippable
		
		"""

Scenario: Generates inheritance for subclasses
	Given this C# source code
		"""
		namespace Test
		{
			public class A : B {}
			public class B : C {}
			public class C {}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			class C {
			}
			A --|> B
			B --|> C
		
		"""

Scenario: Generates associations for scalar instance properties with a getter referencing other know types
	Given this C# source code
		"""
		namespace Test
		{
			public class A {
				public B B { get; }
				public C C { set {} } // Only set -> excluded
			}
			public class B {
				public C C { get; set; }
			}
			public class C {
				public static A A { get; set; } // Static -> Should be excluded
				public object O { get; set; } // Object is filtered out by the namespace filter -> Should be an attribute
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			class C {
				+object O
			}
			A --> "0..1" B
			B --> "0..1" C
		
		"""

Scenario: Generates composition for instance collection properties with a getter referencing other know types
	Given this C# source code
		"""
		using System;
		using System.Collections.Generic;
		namespace Test
		{
			public class Customer {
				public <collection type><Order> GetOrders { get; }
				public <collection type><Order> GetSetOrders { get; set; }
				public <collection type><Order> SetOrders { set {} } // Only set -> Should be excluded
				public static <collection type><Order> StaticOrders { get; set; } // Static -> Should be excluded
				public <collection type><object> Objects { get; set; } // Object is filtered out by the namespace filter -> Should be an attribute
			}
			public class Order {}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Customer {
				+<collection type>~object~ Objects
			}
			class Order {
			}
			Customer *-- "*" Order : get
			Customer *-- "*" Order : get set
		
		"""
Examples:
	| collection type     |
	| IEnumerable         |
	| ICollection         |
	| IList               |
	| IReadOnlyList       |
	| IReadOnlyCollection |

Scenario: Generates aggregation for instance collection properties with bidirectional scalar property in the other type
	Given this C# source code
		"""
		using System.Collections.Generic;
		namespace Test
		{
			public class Customer {
				public IEnumerable<Order> Orders { get; set; }
			}
			public class Order {
				public Customer Customer { get; set; }
				public Product Product { get; set; }
			}
			public class Product {
				public IEnumerable<Order> Orders { get; set; }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Customer {
			}
			class Order {
			}
			class Product {
			}
			Customer "0..1" o-- "*" Order
			Product "0..1" o-- "*" Order
		
		"""
Examples:
	| collection type |
	| IEnumerable     |
	| ICollection     |
	| IList           |

Scenario: Generates dependency for scalar constructor parameters that are not any other relationship type
	Given this C# source code
		"""
		using System.Collections.Generic;
		namespace Test
		{
			public class A : E, D {
				public A(B b, C c, D d, E e, F f, G g) {} 
				public B B { get; set; } // B should be an association
				public IEnumerable<C> Cs { get; set; } // C should be an composition
				public IEnumerable<F> Fs { get; set; } // F should be an composition, since the relationship is bidirectional
			}
			public class B {
			}
			public class C {
			}
			public interface D {
			}
			public class E {
			}
			public class F {
				public A A { get; set; }
			}
			public class G {
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			class C {
			}
			class D {
				<<interface>>
			}
			class E {
			}
			class F {
			}
			class G {
			}
			A ..> G
			A --> "0..1" B
			A ..|> D
			A --|> E
			A *-- "*" C
			A "0..1" o-- "*" F
		
		"""

Scenario: Generates relationships for nullable scalar properties to other diagram classes
	Given this C# source code
		"""
		namespace Test
		{
			public class A
			{
			}
			public class B {
				public A? A { get; set; }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			B --> "0..1" A
		
		"""

Scenario: Generates one relationship for bidirectional scalar properties between two diagram classes
	Given this C# source code
		"""
		namespace Test
		{
			public class A
			{
				public B? B { get; set; }
				public C C { get; set; }
				public D? D { get; set; }
			}
			public class B {
				public A? A { get; set; }
			}
			public class C {
				public A A { get; set; }
			}
			public class D {
				public A A { get; set; }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			class C {
			}
			class D {
			}
			A "0..1" <--> "0..1" B
			A "0..1" <--> "0..1" C
			A "0..1" <--> "0..1" D
		
		"""

Scenario: Generates relationships for nullable collection properties to other diagram classes
	Given this C# source code
		"""
		using System.Collections.Generic;
		namespace Test
		{
			public class A
			{
			}
			public class B {
				public IEnumerable<A>? Aaa { get; set; }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			B *-- "*" A : aaa
		
		"""

Scenario: Generates relationships to enums
	Given this C# source code
		"""
		namespace Test
		{
			public enum Color
			{
				RED,
				BLUE,
				GREEN,
			}
			public class Shape {
				public Color ForegroundColor { get; set; }
				public Color? BackgroundColor { get; set; }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Color {
				<<enumeration>>
				RED
				BLUE
				GREEN
			}
			class Shape {
			}
			Shape --> "0..1" Color : foreground
			Shape --> "0..1" Color : background
		
		"""

Scenario: Generates undefined to one or zero for optional scalar unidirectional relationship
	Given this C# source code
		"""
		namespace Test
		{
			public class A
			{
				public B B { get; set; }
			}
			public class B {
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			A --> "0..1" B
		
		"""

Scenario: Generates one or zero to one or zero for non required scalar bidirectional relationship
	Given this C# source code
		"""
		namespace Test
		{
			public class A
			{
				public B B { get; set; }
			}
			public class B {
				public A A { get; set; }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			A "0..1" <--> "0..1" B
		
		"""

Scenario: Generates undefined to exactly one for scalar non required scalar unidirectional relationship
	Given this C# source code
		"""
		using System.ComponentModel.DataAnnotations;
		namespace Test
		{
			public class A
			{
				[Required]
				public B B { get; set; }
			}
			public class B {
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			A --> "1" B
		
		"""

Scenario: Generates exactly one to exactly one for scalar non required scalar bidirectional relationship
	Given this C# source code
		"""
		using System.ComponentModel.DataAnnotations;
		namespace Test
		{
			public class A
			{
				[Required]
				public B B { get; set; }
			}
			public class B {
				[Required]
				public A A { get; set; }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			A "1" <--> "1" B
		
		"""

Scenario: Generates undefined to zero or more composition for collection unidirectional relationship
	Given this C# source code
		"""
		using System.Collections.Generic;
		namespace Test
		{
			public class A
			{
				public IEnumerable<B> Bbb { get; set; }
			}
			public class B {
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			A *-- "*" B : bbb
		
		"""
Scenario: Generates zero or one to zero or more aggregation for collection to optional scalar unidirectional relationship
	Given this C# source code
		"""
		using System.Collections.Generic;
		namespace Test
		{
			public class A
			{
				public C? C { get;set; }
				public IEnumerable<B> Bbb { get; set; }
			}
			public class B {
				public A? A { get;set; }
			}
			public class C {
				public IEnumerable<A> Aaa { get; set; }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			class C {
			}
			A "0..1" o-- "*" B : bbb
			C "0..1" o-- "*" A : aaa
		
		"""
Scenario: Generates exactly one to zero or more aggregation for collection to mandatory scalar bidirectional relationship
	Given this C# source code
		"""
		using System.ComponentModel.DataAnnotations;
		using System.Collections.Generic;
		namespace Test
		{
			public class A
			{
				[Required]
				public C C { get;set; }
				public IEnumerable<B> Bbb { get; set; }
			}
			public class B {
				[Required]
				public A A { get;set; }
			}
			public class C {
				public IEnumerable<A> Aaa { get; set; }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			class C {
			}
			A "1" o-- "*" B : bbb
			C "1" o-- "*" A : aaa
		
		"""

Scenario: Generates scalar relationship with JsonProperty attribute, with Required Always, as exactly one relationship
	Given this C# source code
		"""
		using Newtonsoft.Json;
		using System.Collections.Generic;
		namespace Test
		{
			public class A
			{
				[JsonProperty(Required = Required.Always)]
				public C C { get;set; }
				public IEnumerable<B> Bbb { get; set; }
			}
			public class B {
				[JsonProperty(Required = Required.DisallowNull)]
				public A A { get;set; }
			}
			public class C {
				public IEnumerable<A> Aaa { get; set; }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
			}
			class B {
			}
			class C {
			}
			A "0..1" o-- "*" B : bbb
			C "1" o-- "*" A : aaa
		
		"""

Scenario: Generates ER relations with labels when the property name and type name is different
	Given this C# source code
		"""
		using System.Collections.Generic;
		namespace Test
		{
			public class Customer {
				public IEnumerable<Order> Orders { get; set; } // Should have no name
				public IEnumerable<Order> PriorityOrders { get; set; }
				public IEnumerable<Order> OrdersThatHasLowPriority { get; set; }
				public IEnumerable<Order> Orderes { get; set; } // Should have no name, based on special plural rewrite rule [SingularName] -> [SingularName]es, e.g. Address -> Addresses
				public IEnumerable<Order> Ordeies { get; set; } // Should have no name, based on special plural rewrite rule [SingularName] -> [SingularNam]ies, e.g. Country -> Countries
				public Address Address { get; set; }
			}
			public class Order
			{
				public Address Address { get; set; }
				public Address DeliveryAddress { get; set; }
				public Address AddressForInvoice { get; set; }
				public Address DropOff { get; set; }
				public Product MainOrderedProduct { get; set; }
			}
			public class Address {
				public Customer OwningCustomer { get; set; }
			}
			public class Product {
				public IEnumerable<Order> MainProductInOrders { get; set; }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Address {
			}
			class Customer {
			}
			class Order {
			}
			class Product {
			}
			Address "0..1" <--> "0..1" Customer : owning
			Customer *-- "*" Order
			Customer *-- "*" Order
			Customer *-- "*" Order
			Customer *-- "*" Order : orders that has low priority
			Customer *-- "*" Order : priority
			Order --> "0..1" Address
			Order --> "0..1" Address : delivery
			Order --> "0..1" Address : address for invoice
			Order --> "0..1" Address : drop off
			Product "0..1" o-- "*" Order : main product in
		
		"""