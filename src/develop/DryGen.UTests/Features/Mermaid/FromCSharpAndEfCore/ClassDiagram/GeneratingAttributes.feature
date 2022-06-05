Feature: Generating Attributes in Class diagrams

To be able to generate Mermaid Class diagrams that represente my C# code
As a dry-gen user
I should be able to generate Mermaid attribute information from the C# type's properties

Background:
	Given this include namespace filter '^Test$'

Scenario: Generates attributes for properties that has a getter
	Given this C# source code
		"""
		namespace Test
		{
			public class Order
			{
				public int GetProp { get; }
				public int GetSetProp { get; set; }
				public int SetProp { set {} } // Only set -> Should be excluded
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Order {
				+int GetProp
				+int GetSetProp
			}
		
		"""

Scenario: Generates attributes with visibility modifier for properties
	Given this C# source code
		"""
		namespace Test
		{
			public class Order
			{
				public int PublicProp { get; set;}
				internal string InternalProp { get; set; }
				protected decimal ProtectedProp { get; set; }
				private float PrivateProp { get; set; }
				bool DefaultProp { get; set; } // Default access level for a class member is private
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Order {
				+int PublicProp
				~string InternalProp
				#decimal ProtectedProp
				-float PrivateProp
				-bool DefaultProp
			}
		
		"""

Scenario: Generates attributes with static modifier for properties
	Given this C# source code
		"""
		namespace Test
		{
			public class Order
			{
				public static int PublicProp { get; set;}
				internal static int InternalProp { get; set; }
				protected static int ProtectedProp { get; set; }
				private static int PrivateProp { get; set; }
				static double DefaultProp { get; set; } // Default access level for a class member is private
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class Order {
				+int PublicProp$
				~int InternalProp$
				#int ProtectedProp$
				-int PrivateProp$
				-double DefaultProp$
			}
		
		"""

Scenario: Generates attributes only for properties declared directly in the type
	Given this C# source code
		"""
		namespace Test
		{
			public class A
			{
				public int AProp { get; set;}
			}
			public class B : A
			{
				public int BProp { get; set;}
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
				+int AProp
			}
			class B {
				+int BProp
			}
			B --|> A
		
		"""

Scenario: Generates private attributes for explicit implemented interface properties
	Given this C# source code
		"""
		namespace Test
		{
			public interface A
			{
				int AProp { get; set;}
			}
			public class B : A
			{
				int A.AProp { get; set;}
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
				<<interface>>
				+int AProp
			}
			class B {
				-int Test.A.AProp
			}
			B ..|> A
		
		"""

Scenario: Generates enum vales
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
		
		"""
