Feature: Generating Methods in Class diagrams

A short summary of the feature

Background:
	Given this include namespace filter '^Test$'

Scenario: Generates methods with visibility modifier for methods
	Given this C# source code
		"""
		namespace Test
		{
			public class A
			{
				public int PublicMethod() { return 0; }
				internal int InternalMethod() { return 0; }
				protected int ProtectedMethod() { return 0; }
				private int PrivateMethod() { return 0; }
				int DefaultMethod() { return 0; } // Default access level for a class member is private
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
				+PublicMethod() int
				~InternalMethod() int
				#ProtectedMethod() int
				-PrivateMethod() int
				-DefaultMethod() int
			}
		
		"""

Scenario: Generates methods with static modifier for methods
	Given this C# source code
		"""
		namespace Test
		{
			public class A
			{
				static public int PublicMethod() { return 0; }
				static internal int InternalMethod() { return 0; }
				static protected int ProtectedMethod() { return 0; }
				static private int PrivateMethod() { return 0; }
				static int DefaultMethod() { return 0; } // Default access level for a class member is private
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
				+PublicMethod()$ int
				~InternalMethod()$ int
				#ProtectedMethod()$ int
				-PrivateMethod()$ int
				-DefaultMethod()$ int
			}
		
		"""

Scenario: Generates methods with abstract modifier for methods
	Given this C# source code
		"""
		namespace Test
		{
			public abstract class A
			{
				public abstract int PublicMethod();
				internal abstract int InternalMethod();
				protected abstract int ProtectedMethod();
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
				<<abstract>>
				+PublicMethod()* int
				~InternalMethod()* int
				#ProtectedMethod()* int
			}
		
		"""

Scenario: Generates methods only for methods declared directly in the type
	Given this C# source code
		"""
		namespace Test
		{
			public class A
			{
				public int AMethod() { return 0; }
			}
			public class B : A
			{
				public int BMethod() { return 0; }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
				+AMethod() int
			}
			class B {
				+BMethod() int
			}
			B --|> A
		
		"""

Scenario: Generates void methods without return type
	Given this C# source code
		"""
		namespace Test
		{
			public class A
			{
				public void AMethod() { }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
				+AMethod()
			}
		
		"""

Scenario: Does not generate methods for getters and setters
	Given this C# source code
		"""
		namespace Test
		{
			public class A
			{
				public int AProp { get; set; }
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
		
		"""

Scenario: Does not generate methods for local functions
	Given this C# source code
		"""
		namespace Test
		{
			public class A
			{
				public void AMethod() { 
					static void LocalMethod() {}
				}
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
				+AMethod()
			}
		
		"""


Scenario: Generates methods parameters
	Given this C# source code
		"""
		namespace Test
		{
			public class A
			{
				public void AMethod(A aParam, int intParam) { }
			}
		}
		"""
	When I generate a Class diagram
	Then I should get this generated representation
		"""
		classDiagram
			class A {
				+AMethod(A aParam, int intParam)
			}
		
		"""
