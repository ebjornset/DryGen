Feature: Command line for generating Mermaid Class diagrams from C#

To be able to generate other representation of C# code
As a dry-gen user
I should be able to generate Mermaid Class diagrams from a .Net assembly with the verb 'mermaid-class-diagram-from-csharp'

Background:
	Given this C# source code compiled to a file
	# The commandline argument -i <this assembly filename> will be appended to the command line
		"""
		using System.ComponentModel.DataAnnotations;
		namespace Test
		{
			public class Customer {
				[Key]
				public int PublicProp { get; set; }
				public int? NullableProp { get; set; }
			}
			public class Order {
				public Customer Customer { get; set; }
		
				public int PublicInstanceProp { get; set; }
				public static int PublicStaticProp { get; set; }
				internal int InternalInstanceProp { get; set; }
				internal static int InternalStaticProp { get; set; }
				protected int ProtectedInstanceProp { get; set; }
				protected static int ProtectedStaticProp { get; set; }
				private int PrivateInstanceProp { get; set; }
				private static int PrivateStaticProp { get; set; }
		
				public void PublicInstanceMethod() {}
				public static void PublicStaticMethod() {}
				internal void InternalInstanceMethod() {}
				internal static void InternalStaticMethod() {}
				protected void ProtectedInstanceMethod() {}
				protected static void ProtectedStaticMethod() {}
				private void PrivateInstanceMethod() {}
				private static void PrivateStaticMethod() {}
			}
			public enum Color
			{
				RED,
				BLUE,
				GREEN,
			}
		}
		"""
	And output is spesified as a command line argument
	# The commandline arguments -o and a temporary filename will be appended to the command line

Scenario: Should generate Class diagram from classdiagram command
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --attribute-level                 |
		| all                               |
		| --method-level                    |
		| all                               |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Color {
				<<enumeration>>
				RED
				BLUE
				GREEN
			}
			class Customer {
				+int PublicProp
				+int NullableProp
			}
			class Order {
				+int PublicInstanceProp
				+int PublicStaticProp$
				~int InternalInstanceProp
				~int InternalStaticProp$
				#int ProtectedInstanceProp
				#int ProtectedStaticProp$
				-int PrivateInstanceProp
				-int PrivateStaticProp$
				+PublicInstanceMethod()
				+PublicStaticMethod()$
				~InternalInstanceMethod()
				~InternalStaticMethod()$
				#ProtectedInstanceMethod()
				#ProtectedStaticMethod()$
				-PrivateInstanceMethod()
				-PrivateStaticMethod()$
			}
			Order --> "0..1" Customer
		
		"""

Scenario: Should generate Class diagram with type inclusion from argument
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --include-typenames               |
		| ^Customer$                        |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Customer {
				+int PublicProp
				+int NullableProp
			}
		
		"""

Scenario: Should generate Class diagram with all attributes and methods excluded from arguments
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --attribute-level                 |
		| none                              |
		| --method-level                    |
		| none                              |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Color {
				<<enumeration>>
			}
			class Customer {
			}
			class Order {
			}
			Order --> "0..1" Customer
		
		"""

Scenario: Should generate Class diagram with private, protected and internal attributes excluded from argument
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --attribute-level                 |
		| public                            |
		| --method-level                    |
		| none                              |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Color {
				<<enumeration>>
				RED
				BLUE
				GREEN
			}
			class Customer {
				+int PublicProp
				+int NullableProp
			}
			class Order {
				+int PublicInstanceProp
				+int PublicStaticProp$
			}
			Order --> "0..1" Customer
		
		"""

Scenario: Should generate Class diagram with private and protected attributes excluded from argument
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --attribute-level                 |
		| internal                          |
		| --method-level                    |
		| none                              |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Color {
				<<enumeration>>
				RED
				BLUE
				GREEN
			}
			class Customer {
				+int PublicProp
				+int NullableProp
			}
			class Order {
				+int PublicInstanceProp
				+int PublicStaticProp$
				~int InternalInstanceProp
				~int InternalStaticProp$
			}
			Order --> "0..1" Customer
		
		"""

Scenario: Should generate Class diagram with private attributes excluded from argument
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --attribute-level                 |
		| protected                         |
		| --method-level                    |
		| none                              |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Color {
				<<enumeration>>
				RED
				BLUE
				GREEN
			}
			class Customer {
				+int PublicProp
				+int NullableProp
			}
			class Order {
				+int PublicInstanceProp
				+int PublicStaticProp$
				~int InternalInstanceProp
				~int InternalStaticProp$
				#int ProtectedInstanceProp
				#int ProtectedStaticProp$
			}
			Order --> "0..1" Customer
		
		"""

Scenario: Should generate Class diagram with attributes excluded by property name from argument
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --exclude-propertynames           |
		| .*Prop$                           |
		| --method-level                    |
		| none                              |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Color {
				<<enumeration>>
				RED
				BLUE
				GREEN
			}
			class Customer {
			}
			class Order {
			}
			Order --> "0..1" Customer
		
		"""

Scenario: Should generate Class diagram with private, protected and internal methods excluded from argument
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --attribute-level                 |
		| none                              |
		| --method-level                    |
		| public                            |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Color {
				<<enumeration>>
			}
			class Customer {
			}
			class Order {
				+PublicInstanceMethod()
				+PublicStaticMethod()$
			}
			Order --> "0..1" Customer
		
		"""

Scenario: Should generate Class diagram with private and protected methods excluded from argument
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --attribute-level                 |
		| none                              |
		| --method-level                    |
		| internal                          |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Color {
				<<enumeration>>
			}
			class Customer {
			}
			class Order {
				+PublicInstanceMethod()
				+PublicStaticMethod()$
				~InternalInstanceMethod()
				~InternalStaticMethod()$
			}
			Order --> "0..1" Customer
		
		"""

Scenario: Should generate Class diagram with private methods excluded from argument
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --attribute-level                 |
		| none                              |
		| --method-level                    |
		| protected                         |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Color {
				<<enumeration>>
			}
			class Customer {
			}
			class Order {
				+PublicInstanceMethod()
				+PublicStaticMethod()$
				~InternalInstanceMethod()
				~InternalStaticMethod()$
				#ProtectedInstanceMethod()
				#ProtectedStaticMethod()$
			}
			Order --> "0..1" Customer
		
		"""

Scenario: Should generate Class diagram with direction from argument
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --direction                       |
		| <Direction>                       |
		| --attribute-level                 |
		| none                              |
		| --method-level                    |
		| none                              |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			direction <Direction>
			class Color {
				<<enumeration>>
			}
			class Customer {
			}
			class Order {
			}
			Order --> "0..1" Customer
		
		"""
Examples:
	| Direction |
	| TB        |
	| BT        |
	| LR        |
	| RL        |

Scenario: Should exclude static attributes from Class diagram from argument
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --attribute-level                 |
		| all                               |
		| --exclude-static-attributes       |
		| true                              |
		| --method-level                    |
		| none                              |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Color {
				<<enumeration>>
				RED
				BLUE
				GREEN
			}
			class Customer {
				+int PublicProp
				+int NullableProp
			}
			class Order {
				+int PublicInstanceProp
				~int InternalInstanceProp
				#int ProtectedInstanceProp
				-int PrivateInstanceProp
			}
			Order --> "0..1" Customer
		
		"""

Scenario: Should exclude static methods from Class diagram from argument
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --attribute-level                 |
		| none                              |
		| --exclude-static-methods          |
		| true                              |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Color {
				<<enumeration>>
			}
			class Customer {
			}
			class Order {
				+PublicInstanceMethod()
				~InternalInstanceMethod()
				#ProtectedInstanceMethod()
				-PrivateInstanceMethod()
			}
			Order --> "0..1" Customer
		
		"""

Scenario: Should exclude methods parameters from Class diagram from argument
	Given this C# source code compiled to a file
	# The commandline argument -i <this assembly filename> will be appended to the command line
		"""
		namespace Test
		{
			public class A {
				public void NoArgMethod() {}
				public void OneArgMethod(int one) {}
				public void TwoArgMethod(int one, int two) {}
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --exclude-method-params           |
		| true                              |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class A {
				+NoArgMethod()
				+OneArgMethod(1 param)
				+TwoArgMethod(2 params)
			}
		
		"""

Scenario: Should tree shake Class diagram from argument
	Given this C# source code compiled to a file
	# The commandline argument -i <this assembly filename> will be appended to the command line
		"""
		namespace Test
		{
			public class Order {
			}
			public class NoMatchOrder {
			}
			public class OrderNoMatch {
			}
		}
		"""
	When I call the program with this command line arguments
		| Arg                               |
		| mermaid-class-diagram-from-csharp |
		| --tree-shaking-roots              |
		| ^Order$                           |
	Then I should get exit code '0'
	And I should get this generated representation file
		"""
		classDiagram
			class Order {
			}
		
		"""
