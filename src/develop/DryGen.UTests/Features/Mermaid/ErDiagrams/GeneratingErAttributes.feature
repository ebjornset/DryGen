Feature: Generating Attributes in Er diagrams

A short summary of the feature

Background:
	Given this include namespace filter '^Test$'
	And this exclude type name filter '^TestDbContext$'
	And the Er diagram attribute type exclusion 'None'
	And the Er diagram relationship exclusion 'All'

Scenario: Generates ER attributes only for public properties
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Order
			{
				internal int InternalProp { get; set; }
				protected int ProtectedProp { get; set; }
				private int PrivateProp { get; set; }
			}
			public class TestDbContext: DbContext {
				public DbSet<Order> Orders { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Order>().HasNoKey();
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			Order
		
		"""
Examples:
	| Structure builder |
	| Reflection        |
	| EfCore            |

Scenario: Generates ER attributes only for properties with getter and setter
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		namespace Test
		{
			public class Order
			{   
				public int SetOnlyProp { set {} }
				public int GetOnlyProp => 0;
			}
			public class TestDbContext: DbContext {
				public DbSet<Order> Orders { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Order>().HasNoKey();
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			Order
		
		"""
Examples:
	| Structure builder |
	| Reflection        |
	| EfCore            |

Scenario: Generates ER attributes only for non collection properties
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		using System.ComponentModel.DataAnnotations.Schema;
		using System.Collections.Generic;
		namespace Test
		{
			public class Order
			{
				[NotMapped]
				public IEnumerable<int>[] EnumerableProp { get; set; }
				[NotMapped]
				public int[] ArrayProp { get; set; }
			}
			public class TestDbContext: DbContext {
				public DbSet<Order> Orders { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Order>().HasNoKey();
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			Order
		
		"""
Examples:
	| Structure builder |
	| Reflection        |
	| EfCore            |

Scenario: Generates ER attributes for well known type properties
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		using System;
		namespace Test
		{
			public class Order
			{
				public int IntProp { get; set; }
				public long LongProp { get; set; }
				public decimal DecimalProp { get; set; }
				public float FloatProp { get; set; }
				public double DoubleProp { get; set; }
				public bool BoolProp { get; set; }
				public string StringProp { get; set; }
				public DateTime DateTimeProp { get; set; }
				public DateTimeOffset DateTimeOffsetProp { get; set; }
			}
			public class TestDbContext: DbContext {
				public DbSet<Order> Orders { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Order>().HasNoKey();
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			Order {
				bool BoolProp
				datetimeoffset DateTimeOffsetProp
				datetime DateTimeProp
				decimal DecimalProp
				double DoubleProp
				float FloatProp
				int IntProp
				long LongProp
				string StringProp
			}
		
		"""
Examples:
	| Structure builder |
	| Reflection        |
	| EfCore            |

Scenario: Generates ER attributes with null comment for nullable well known type properties
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		using System;
		namespace Test
		{
			public class Order
			{
				public int? IntProp { get; set; }
				public long? LongProp { get; set; }
				public decimal? DecimalProp { get; set; }
				public float? FloatProp { get; set; }
				public double? DoubleProp { get; set; }
				public bool? BoolProp { get; set; }
				public DateTime? DateTimeProp { get; set; }
				public DateTimeOffset? DateTimeOffsetProp { get; set; }
			}
			public class TestDbContext: DbContext {
				public DbSet<Order> Orders { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            modelBuilder.Entity<Order>().HasNoKey();
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			Order {
				bool BoolProp "Null"
				datetimeoffset DateTimeOffsetProp "Null"
				datetime DateTimeProp "Null"
				decimal DecimalProp "Null"
				double DoubleProp "Null"
				float FloatProp "Null"
				int IntProp "Null"
				long LongProp "Null"
			}
		
		"""
Examples:
	| Structure builder |
	| Reflection        |
	| EfCore            |

Scenario: Generates ER attributes as PK for properties with key attribute
	Given this C# source code
		"""
		using Microsoft.EntityFrameworkCore;
		using System.ComponentModel.DataAnnotations;
		namespace Test
		{
			public class A
			{
				public int APropFirst { get; set; }
				[Key]
				public int APropKey { get; set; }
			}
			public class TestDbContext: DbContext {
				public DbSet<A> Aaaa { get; set; }
				public TestDbContext(DbContextOptions options) : base(options) {}
				protected override void OnModelCreating(ModelBuilder modelBuilder)
		        {
		            //modelBuilder.Entity<A>().HasNoKey();
				}
			}
		}
		"""
	When I generate an ER diagram using '<Structure builder>'
	Then I should get this generated representation
		"""
		erDiagram
			A {
				int APropKey PK
				int APropFirst
			}
		
		"""
Examples:
	| Structure builder |
	| Reflection        |
	| EfCore            |
