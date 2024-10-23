using DryGen.Core;
using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ErDiagram;
using DryGen.MermaidFromCSharp.TypeFilters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using TypeLoadException = DryGen.Core.TypeLoadException;

namespace DryGen.MermaidFromEfCore;

public class ErDiagramStructureBuilderByEfCore : IErDiagramStructureBuilder
{
	public IReadOnlyList<ErDiagramEntity> GenerateErStructure(Assembly assembly, IReadOnlyList<ITypeFilter> typeFilters, IReadOnlyList<IPropertyFilter> attributeFilters, INameRewriter? nameRewriter)
	{
		LoadEfCoreRequiredAssemblies();
		var dbContextType = LoadTypeByName(DBContextTypeName);
		var efCoreEntityTypes = new List<ModelEntityType>();
		var dbContextTypesFromAssembly = assembly
			.GetTypes()
			.Where(t => dbContextType.IsAssignableFrom(t) && !t.IsAbstract)
			.ToList();
		// We must make sure we include the synthetic Dictionary<string, object> Ef Core uses under the hood to realize many to many relations 
		// in the list of entities we generate relations from
		typeFilters = EfCoreFiltersIncludingManyToManyDictionaries(typeFilters); 
		foreach (var dbContextTypeFromAssembly in dbContextTypesFromAssembly)
		{
			efCoreEntityTypes.AddRange(LoadEfCoreEntitiesFromDbContextType(dbContextTypeFromAssembly, typeFilters));
		}
		var result = efCoreEntityTypes.Distinct(new ModelEntityTypeEqualityComparer())
			.Select(et => new EfCoreErEntity(nameRewriter?.Rewrite(et.ClrType.Name) ?? et.ClrType.Name, et))
			.OrderBy(nt => nt.Name)
			.ThenBy(nt => nt.Type.Name)
			.ThenBy(nt => nt.Type.Namespace).ToList();
		GenerateErStructure(result, attributeFilters);
		// Exclude any synthetic Dictionary<string, object> Ef Core uses under the hood to realize many to many relations from the final list of entities
		var excludeManyToManyDictionaryTypeFilter = new ExcludeManyToManyDictionaryTypeFilter();
		return result.Where(x => excludeManyToManyDictionaryTypeFilter.Accepts(x.Type)).ToList();
	}

	private static void GenerateErStructure(IList<EfCoreErEntity> entities, IReadOnlyList<IPropertyFilter> attributeFilters)
	{
		var enumEntities = new Dictionary<Type, EfCoreErEntity>();
		var entityLookup = entities.ToDictionary(x => x.Type, x => x);
		foreach (var entity in entities)
		{
			// We don't need attributes for the synthetic Dictionary<string, object> Ef Core uses under the hood to realize many to many relations,
			// since we will filter out these entities from the final list anyway
			if (entity.GetEntityType().ClrType != manyToManyDictionaryType)
			{
				GenerateErAttributes(attributeFilters, entity, enumEntities);
			}
			GenerateErRelationships(entityLookup, entity);
		}
		enumEntities.AppendToEntities(entities);
	}

	private static void GenerateErRelationships(Dictionary<Type, EfCoreErEntity> entityLookup, EfCoreErEntity entity)
	{
		if (entity.GetEntityType().ClrType == manyToManyDictionaryType)
		{
			GenerateErRelationshipManyToMany(entityLookup, entity);
		}
		else
		{
			GenerateErRelationshipsOneToMany(entityLookup, entity);
		}
	}

	private static void GenerateErRelationshipsOneToMany(Dictionary<Type, EfCoreErEntity> entityLookup, EfCoreErEntity entity)
	{
		foreach (var foreignKey in entity.GetEntityType().GetForeignKeys())
		{
			// Generate relationships from the principal side, since it makes the diagram flow from the principals to the dependents
			var principalType = foreignKey.PrincipalEntityType.ClrType;
			var isBidirectional = foreignKey.PrincipalToDependent != null;
			var navigation = (isBidirectional ? foreignKey.PrincipalToDependent : foreignKey.DependentToPrincipal)
							  ?? throw new ArgumentException($"Cant find navigation for foreignKey '{foreignKey}', when isBidirectional is '{isBidirectional}'");
			var propertyType = navigation.ClrType;
			var propertyName = navigation.Name;
			if (entityLookup.TryGetValue(principalType, out var principalEntity))
			{
				var isIdentifying = foreignKey.Properties?.All(p => p.IsKey()) == true;
				if (IsGenericRelationshipCollection(propertyType))
				{
					if (propertyType.GetGenericArguments().Length > 1)
					{
						continue;
					}
					CreateManyRelationshipForCollection(entity, foreignKey, propertyName, principalEntity, isIdentifying);
				}
				else
				{
					CreateRelationshipForNonCollection(entity, foreignKey, isBidirectional, propertyName, principalEntity, isIdentifying);
				}
			}
		}
	}

	private static void GenerateErRelationshipManyToMany(Dictionary<Type, EfCoreErEntity> entityLookup, EfCoreErEntity entity)
	{
		var properties = entity.GetEntityType().GetProperties().ToArray();
		var firstForeignKey = properties[0].GetElementMandatoryMethodValue<IEnumerable<object>>("GetContainingForeignKeys").Select(x => new ModelForeignKey(x)).Single();
		var firstPrincipalType = firstForeignKey.PrincipalEntityType.ClrType;
		var secondForeignKey = properties[^1].GetElementMandatoryMethodValue<IEnumerable<object>>("GetContainingForeignKeys").Select(x => new ModelForeignKey(x)).Single();
		var secondPrincipalType = secondForeignKey.PrincipalEntityType.ClrType;
		if (entityLookup.TryGetValue(firstPrincipalType, out var firstPrincipalEntity) && entityLookup.TryGetValue(secondPrincipalType, out var secondPrincipalEntity))
		{
			var secondPrincipalKeyPropertyName = new ModelRuntimeKey(secondForeignKey.GetElementMandatoryPropertyValue<object>("PrincipalKey")).Properties[0].Name;
			var secondForeignKeyPropertyName = secondForeignKey.Properties[0].Name;
			var label = secondForeignKeyPropertyName.EndsWith(secondPrincipalKeyPropertyName) ? secondForeignKeyPropertyName[..^secondPrincipalKeyPropertyName.Length].ToNormalizedRelationshipLabel() : string.Empty;
			firstPrincipalEntity.AddRelationship(secondPrincipalEntity, ErDiagramRelationshipCardinality.ZeroOrMore, ErDiagramRelationshipCardinality.ZeroOrMore, label, string.Empty, isIdentifying: false);
		}
	}

	private static void CreateManyRelationshipForCollection(EfCoreErEntity entity, ModelForeignKey foreignKey, string propertyName, EfCoreErEntity principalEntity, bool isIdentifying)
	{
		var labelBaseName = foreignKey.DependentToPrincipal?.Name ?? foreignKey.PrincipalToDependent?.Name ?? string.Empty;
		var label = labelBaseName.GetRelationshipLabel((foreignKey.DependentToPrincipal == null ? entity : principalEntity).Name, replaceEntityNameAtEndOfPropertyName: foreignKey.DependentToPrincipal != null);
		var fromCardianlity = foreignKey.IsRequired ? ErDiagramRelationshipCardinality.ExactlyOne : ErDiagramRelationshipCardinality.ZeroOrOne;
		var toCardianlity = ErDiagramRelationshipCardinality.ZeroOrMore;
		principalEntity.AddRelationship(entity, fromCardianlity, toCardianlity, label, propertyName, isIdentifying);
	}

	private static bool IsGenericRelationshipCollection(Type propertyType)
	{
		return propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().IsErDiagramRelationshipCollection();
	}

	private static void CreateRelationshipForNonCollection(EfCoreErEntity entity, ModelForeignKey foreignKey, bool isBidirectional, string propertyName, EfCoreErEntity principalEntity, bool isIdentifying)
	{
		var fromCardianlity = foreignKey.IsRequired ? ErDiagramRelationshipCardinality.ExactlyOne : ErDiagramRelationshipCardinality.ZeroOrOne;
		ErDiagramRelationshipCardinality toCardianlity;
		if (isBidirectional)
		{
			var isPrincipalToDependentRequired = foreignKey.PrincipalToDependent?.PropertyInfo?.IsRequiredProperty() == true;
			toCardianlity = isPrincipalToDependentRequired ? ErDiagramRelationshipCardinality.ExactlyOne : ErDiagramRelationshipCardinality.ZeroOrOne;
		}
		else
		{
			toCardianlity = ErDiagramRelationshipCardinality.ZeroOrMore;
		}
		var labelBaseName = foreignKey.DependentToPrincipal?.Name ?? foreignKey.PrincipalToDependent?.Name ?? string.Empty;
		var label = labelBaseName.GetRelationshipLabel((foreignKey.DependentToPrincipal == null ? entity : principalEntity).Name, replaceEntityNameAtEndOfPropertyName: true);
		principalEntity.AddRelationship(entity, fromCardianlity, toCardianlity, label, propertyName, isIdentifying);
	}

	private static void GenerateErAttributes(IReadOnlyList<IPropertyFilter> attributeFilters, EfCoreErEntity entity, Dictionary<Type, EfCoreErEntity> enumEntities)
	{
		foreach (var efProperty in entity.EntityType?.GetProperties().Where(p => p?.PropertyInfo != null && attributeFilters.All(f => f.Accepts(p.PropertyInfo)))
			?? throw new TypeMemberException($"EntityType was null for entity '{entity.Name}'"))
		{
			var propertyType = efProperty.ClrType;
			if (propertyType.IsErDiagramAttributePropertyType())
			{
				var attributeType = propertyType.GetErDiagramAttributeTypeName();
				var attributeName = efProperty.Name;
				var nullableUnderlyingType = Nullable.GetUnderlyingType(propertyType);
				var isNullable = nullableUnderlyingType != null;
				var isPrimaryKey = efProperty.IsPrimaryKey();
				var isAlternateKey = !isPrimaryKey && efProperty.IsKey();
				var enumType = nullableUnderlyingType ?? propertyType;
				var isEnum = enumType.AddToEntityAsRelationshipIfEnum(attributeName, isNullable, entity, enumEntities, x => new EfCoreErEntity(x.Name, x));
				var isForeignKey = efProperty.IsForeignKey() || isEnum;
				entity.AddAttribute(new ErDiagramAttribute(attributeType, attributeName, isNullable, isPrimaryKey, isAlternateKey, isForeignKey));
			}
		}
	}

	private static IEnumerable<ModelEntityType> LoadEfCoreEntitiesFromDbContextType(Type dbContextType, IReadOnlyList<ITypeFilter> typeFilters)
	{
		var dbContextOptionsType = LoadTypeByName(DBContextOptionsTypeName);
		var optionsBuilderOpenType = LoadTypeByName(DbContextOptionsBuilderOpenTypeName);
		var optionsBuilderClosedType = optionsBuilderOpenType.MakeGenericType(dbContextType);
		var optionsBuilderCtor = optionsBuilderClosedType.GetConstructors().Single(x => x.GetParameters().Length == 0);
		var optionsBuilderType = LoadTypeByName(DbContextOptionsBuilderTypeName);
		var optionsBuilder = optionsBuilderCtor.Invoke(null);
		var inMemoryDbContextOptionsExtensionsType = LoadTypeByName(InMemoryDbContextOptionsExtensionsTypeName);
		var actionOpenType = typeof(Action<>);
		var inMemoryDbContextOptionsBuilderType = LoadTypeByName(InMemoryDbContextOptionsBuilderTypeName);
		var actionClosedType = actionOpenType.MakeGenericType(inMemoryDbContextOptionsBuilderType);
		var useInMemoryDatabaseMethod = inMemoryDbContextOptionsExtensionsType
			.GetMethod("UseInMemoryDatabase", BindingFlags.Static | BindingFlags.Public, new Type[] { optionsBuilderType, typeof(string), actionClosedType });
		var inMemoryOptionsBuilder = useInMemoryDatabaseMethod?.Invoke(null, new object?[] { optionsBuilder, Guid.NewGuid().ToString(), null });
		var optionsProperty = optionsBuilderType.GetProperty("Options");
		var options = optionsProperty?.GetValue(inMemoryOptionsBuilder);
		var contextCtor = Array.Find(dbContextType.GetConstructors(), x => Array.Exists(x.GetParameters(), y => dbContextOptionsType.IsAssignableFrom(y.ParameterType)))
			?? throw new ArgumentException($"{dbContextType.Name} has no public constructor with DbContextOptions as a parameter");
		var ctorParameters = new object?[contextCtor.GetParameters().Length];
		var offset = 0;
		foreach (var parameterType in contextCtor.GetParameters().Select(parameterInfo => parameterInfo.ParameterType))
		{
			if (dbContextOptionsType.IsAssignableFrom(parameterType))
			{
				ctorParameters[offset] = options;
			}
			else
			{
				ctorParameters[offset] = parameterType.IsValueType ? Activator.CreateInstance(parameterType) : null;
			}
			offset++;
		}
		var dbContext = contextCtor.Invoke(ctorParameters);
		var modelProperty = dbContext.GetType().GetProperty("Model");
		var model = modelProperty?.GetValue(dbContext);
		var iModelType = LoadTypeByName(IModelTypeName);
		var getEntityTypesMethod = iModelType.GetMethod("GetEntityTypes");
		var entityTypes = (IEnumerable?)getEntityTypesMethod?.Invoke(model, null);
		if (entityTypes == null)
		{
			return Array.Empty<ModelEntityType>();
		}
		var result = new List<ModelEntityType>();
		foreach (var entityType in entityTypes)
		{
			var candidate = new ModelEntityType(entityType);
			if (typeFilters.All(filter => filter.Accepts(candidate.ClrType)))
			{
				result.Add(candidate);
			}
		}
		return result;
	}

	private static void LoadEfCoreRequiredAssemblies()
	{
		var alreadyLoadedEfCoreRequiredAssemblies = new bool?[EfCoreRequiredAssemblyNames.Length];
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
		{
			var assemblyName = assembly.GetName().Name;
			for (var i = 0; i < alreadyLoadedEfCoreRequiredAssemblies.Length; i++)
			{
				if (EfCoreRequiredAssemblyNames[i] == assemblyName)
				{
					alreadyLoadedEfCoreRequiredAssemblies[i] = true;
					break;
				}
			}
		}
		for (var i = 0; i < alreadyLoadedEfCoreRequiredAssemblies.Length; i++)
		{
			TryLoadEfCoreAssebly(alreadyLoadedEfCoreRequiredAssemblies, i);
		}
	}

	[ExcludeFromCodeCoverage] // It's to tedious to create a test case where any of the EF Core assemblies are missing. This has been tested (once) manually
	private static void TryLoadEfCoreAssebly(bool?[] alreadyLoadedEfCoreRequiredAssemblies, int i)
	{
		if (alreadyLoadedEfCoreRequiredAssemblies[i] != true)
		{
			try
			{
				AppDomain.CurrentDomain.Load(EfCoreRequiredAssemblyNames[i]);
			}
			catch (FileNotFoundException ex)
			{
				if (ex.Message.Contains(EfCoreRequiredAssemblyNames[i]))
				{
					var sb = new StringBuilder().Append("Could not load the '").Append(EfCoreRequiredAssemblyNames[i]).AppendLine("' assembly");
					sb.Append(EfCoreRequiredAssemblyErrorFixSuggestions[i]);
					throw new AssemblyLoadException(sb.ToString());
				}
				throw;
			}
		}
	}

	private static Type LoadTypeByName(string name)
	{
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
		{
			var type = assembly.GetType(name);
			if (type != null)
			{
				return type;
			}
		}
		throw new TypeLoadException($"Could not load Ef Core type '{name}'");
	}

	private static List<ITypeFilter> EfCoreFiltersIncludingManyToManyDictionaries(IReadOnlyList<ITypeFilter> filters)
	{
		var result = new List<ITypeFilter>
		{
			new AnyChildFiltersTypeFilter(new ITypeFilter[] { new AllChildFiltersTypeFilter(filters), new IncludeManyToManyDictionaryTypeFilter() })
		};
		return result;
	}

	private sealed class IncludeManyToManyDictionaryTypeFilter : ITypeFilter
	{
		public bool Accepts(Type type)
		{
			return type == manyToManyDictionaryType;
		}
	}

	private sealed class ExcludeManyToManyDictionaryTypeFilter : ITypeFilter
	{
		public bool Accepts(Type type)
		{
			return type != manyToManyDictionaryType;
		}
	}

	private static readonly Type manyToManyDictionaryType = typeof(Dictionary<string, object>);

	private sealed class EfCoreErEntity : ErDiagramEntity
	{
		public EfCoreErEntity(string name, ModelEntityType entityType) : base(name, entityType.ClrType)
		{
			EntityType = entityType;
		}

		public EfCoreErEntity(string name, Type type) : base(name, type)
		{
		}

		public ModelEntityType? EntityType { get; }

		public ModelEntityType GetEntityType()
		{
			return EntityType ?? throw new TypeMemberException($"EntityType was null for entity '{Name}'");
		}
	}

	private sealed class ModelEntityTypeEqualityComparer : IEqualityComparer<ModelEntityType>
	{
		public bool Equals(ModelEntityType? x, ModelEntityType? y)
		{
			return x?.ClrType == y?.ClrType;
		}

		public int GetHashCode(ModelEntityType? obj)
		{
			return obj?.ClrType.GetHashCode() ?? 0;
		}
	}

	private const string DBContextTypeName = "Microsoft.EntityFrameworkCore.DbContext";
	private const string DBContextOptionsTypeName = "Microsoft.EntityFrameworkCore.DbContextOptions";
	private const string DbContextOptionsBuilderOpenTypeName = "Microsoft.EntityFrameworkCore.DbContextOptionsBuilder`1";
	private const string DbContextOptionsBuilderTypeName = "Microsoft.EntityFrameworkCore.DbContextOptionsBuilder";
	private const string InMemoryDbContextOptionsExtensionsTypeName = "Microsoft.EntityFrameworkCore.InMemoryDbContextOptionsExtensions";
	private const string InMemoryDbContextOptionsBuilderTypeName = "Microsoft.EntityFrameworkCore.Infrastructure.InMemoryDbContextOptionsBuilder";
	private const string IModelTypeName = "Microsoft.EntityFrameworkCore.Metadata.IModel";

	private static readonly string[] EfCoreRequiredAssemblyNames =
		new[]
		{
"Microsoft.EntityFrameworkCore",
			"Microsoft.EntityFrameworkCore.InMemory" };

	private static readonly string[] EfCoreRequiredAssemblyErrorFixSuggestions =
		new[]
		{
@"
You can either copy it manually to the target folder or force .Net to copy it to the build output folder
with the 'CopyLocalLockFileAssemblies' property in your .csproj file, e.g.

  <PropertyGroup>
    <!-- Existing properties -->
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  </PropertyGroup>

NB! If you choose to copy the file manually, you must also make sure to copy all its dependencies."
,
@"
dry-gen requires this assembly to be able to introspect your model.
You can either copy it manually to the folder or include it as a package reference in your .csproj file.
It's sufficent to include it as a private asset, (with the same version as the rest of your Ef Core dependencies) e.g.

  <ItemGroup>
    <!-- Other packagge references -->
    <PackageReference Include='Microsoft.EntityFrameworkCore.InMemory' Version='X.Y.Z'>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

If its already included as a package reference you can try to force .Net to copy
it to the build output folder with the 'CopyLocalLockFileAssemblies'
propert in your .csproj file, e.g.

  <PropertyGroup>
    <!-- Existing properties -->
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  </PropertyGroup>

NB! If you choose to copy the file manually, you must also make sure to copy all its dependencies."
,
		};
}