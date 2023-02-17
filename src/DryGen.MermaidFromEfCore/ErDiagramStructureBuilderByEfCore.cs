using DryGen.MermaidFromCSharp;
using DryGen.MermaidFromCSharp.ErDiagram;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

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
        foreach (var dbContextTypeFromAssembly in dbContextTypesFromAssembly)
        {
            efCoreEntityTypes.AddRange(LoadEfCoreEntitiesFromDbContextType(dbContextTypeFromAssembly, typeFilters));
        }
        var result = efCoreEntityTypes.Distinct(new ModelEntityTypeEqualityComparer())
            .Select(et => new EfCoreErEntity(nameRewriter?.Rewrite(et.ClrType.Name) ?? et.ClrType.Name, et))
            .OrderBy(nt => nt.Name)
            .ThenBy(nt => nt.Type.Name)
            .ThenBy(nt => nt.Type.Namespace).ToArray();
        GenerateErStructure(result, attributeFilters);
        return result;
    }

    private static void GenerateErStructure(IReadOnlyList<EfCoreErEntity> entities, IReadOnlyList<IPropertyFilter> attributeFilters)
    {
        var entityLookup = entities.ToDictionary(x => x.Type, x => x);
        foreach (var entity in entities)
        {
            GenerateErAttributes(attributeFilters, entity);
            GenerateErRelationships(entityLookup, entity);
        }
    }

    private static void GenerateErRelationships(Dictionary<Type, EfCoreErEntity> entityLookup, EfCoreErEntity entity)
    {
        foreach (var foreignKey in entity.EntityType.GetForeignKeys())
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
            var isPrincipalToDependentRequired = foreignKey.PrincipalToDependent?.PropertyInfo?.GetCustomAttribute(typeof(RequiredAttribute)) != null;
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

    private static void GenerateErAttributes(IReadOnlyList<IPropertyFilter> attributeFilters, EfCoreErEntity entity)
    {
        foreach (var efProperty in entity.EntityType.GetProperties().Where(p => attributeFilters.All(f => p?.PropertyInfo != null && f.Accepts(p.PropertyInfo))))
        {
            var propertyType = efProperty.ClrType;
            if (propertyType.IsErDiagramAttributePropertyType())
            {
                var attributeType = propertyType.GetErDiagramAttributeTypeName();
                var attributeName = efProperty.Name;
                var isNullable = Nullable.GetUnderlyingType(propertyType) != null;
                var isPrimaryKey = efProperty.IsPrimaryKey();
                var isAlternateKey = !isPrimaryKey && efProperty.IsKey();
                var isForeignKey = efProperty.IsForeignKey();
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
        var contextCtor = dbContextType.GetConstructors().FirstOrDefault(x => x.GetParameters().Any(y => dbContextOptionsType.IsAssignableFrom(y.ParameterType)));
        if (contextCtor == null)
        {
            throw new ArgumentException($"{dbContextType.Name} has no public constructor with DbContextOptions as a parameter");
        }
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
            if (entityType == null)
            {
                continue;
            }
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
            if (alreadyLoadedEfCoreRequiredAssemblies[i] != true)
            {
                AppDomain.CurrentDomain.Load(EfCoreRequiredAssemblyNames[i]);
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
        throw new EfCoreTypeException($"Could not load Ef Code type '{name}'");
    }

    private sealed class EfCoreErEntity : ErDiagramEntity
    {
        public EfCoreErEntity(string name, ModelEntityType entityType) : base(name, entityType.ClrType)
        {
            EntityType = entityType;
        }

        public ModelEntityType EntityType { get; }
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

    private abstract class ModelElement
    {
        private readonly object element;
        protected ModelElement(object element)
        {
            if (!ElementType.IsInstanceOfType(element))
            {
                throw new EfCoreTypeException($"'{ElementType.FullName}' is not assignable from '{element.GetType().FullName}'");
            }
            this.element = element;
        }

        protected abstract Type ElementType { get; }

        protected TType GetElementMandatoryPropertyValue<TType>(string propertyName)
        {
            var result = GetElementOptionalPropertyValue<TType>(propertyName);
            if (result == null)
            {
                throw new EfCoreTypeException($"'{typeof(TType).FullName}' return null for mandatory property '{propertyName}'");
            }
            return result;
        }

        protected TType? GetElementOptionalPropertyValue<TType>(string propertyName)
        {
            var propertyInfo = GetElementPropertyInfo(propertyName);
            var result = propertyInfo?.GetValue(element);
            if (result == default)
            {
                return default;
            }
            if (!typeof(TType).IsAssignableFrom(result?.GetType()))
            {
                throw new EfCoreTypeException($"'{typeof(TType).FullName}' is not assignable from '{result?.GetType().FullName}'");
            }
            return (TType)result;
        }

        protected PropertyInfo GetElementPropertyInfo(string propertyName)
        {
            var propertyInfo = GetPropertyInfoFromType(ElementType, propertyName);
            if (propertyInfo == null)
            {
                throw new EfCoreTypeException($"'{ElementType.FullName}' does not have a property named '{propertyName}'");
            }
            return propertyInfo;
        }

        protected TType GetElementMandatoryMethodValue<TType>(string methodName)
        {
            var result = GetElementOptionalMethodValue<TType>(methodName);
            if (result == null)
            {
                throw new EfCoreTypeException($"'{typeof(TType).FullName}' return null for mandatory method '{methodName}'");
            }
            return result;
        }

        protected TType? GetElementOptionalMethodValue<TType>(string methodName)
        {
            var methodInfo = GetElementMethodInfo(methodName);
            var result = methodInfo.Invoke(element, null);
            if (result == default)
            {
                return default;
            }
            if (!typeof(TType).IsAssignableFrom(result?.GetType()))
            {
                throw new EfCoreTypeException($"'{typeof(TType).FullName}' is not assignable from '{result?.GetType().FullName}'");
            }
            return (TType)result;
        }

        protected MethodInfo GetElementMethodInfo(string methodName)
        {
            var methodInfo = GetMethodInfoFromType(ElementType, methodName);
            if (methodInfo == null)
            {
                throw new EfCoreTypeException($"'{ElementType.FullName}' does not have a method named '{methodName}'");
            }
            return methodInfo;
        }

        private PropertyInfo? GetPropertyInfoFromType(Type? type, string propertyName)
        {
            if (type == null)
            {
                return null;
            }
            var propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo != null) { return propertyInfo; }
            foreach (var allInterface in type.GetInterfaces())
            {
                propertyInfo = GetPropertyInfoFromType(allInterface, propertyName);
                if (propertyInfo != null)
                {
                    return propertyInfo;
                }
            }
            return GetPropertyInfoFromType(type.BaseType, propertyName);
        }

        private MethodInfo? GetMethodInfoFromType(Type? type, string methodName)
        {
            if (type == null)
            {
                return null;
            }
            var methodInfo = type.GetMethod(methodName);
            if (methodInfo != null) { return methodInfo; }
            foreach (var allInterface in type.GetInterfaces())
            {
                methodInfo = GetMethodInfoFromType(allInterface, methodName);
                if (methodInfo != null)
                {
                    return methodInfo;
                }
            }
            return GetMethodInfoFromType(type.BaseType, methodName);
        }
    }

    private sealed class ModelEntityType : ModelElement
    {
        private static Type? elementType;
        private IEnumerable<ModelProperty>? properties;
        private IEnumerable<ModelForeignKey>? foreignKeys;

        public ModelEntityType(object entityType) : base(entityType)
        {
            ClrType = GetElementMandatoryPropertyValue<Type>("ClrType");
        }

        public Type ClrType { get; }
        public IEnumerable<ModelProperty> GetProperties() => properties ??= GetElementMandatoryMethodValue<IEnumerable<object>>("GetProperties").Select(x => new ModelProperty(x));
        public IEnumerable<ModelForeignKey> GetForeignKeys() => foreignKeys ??= GetElementMandatoryMethodValue<IEnumerable<object>>("GetForeignKeys").Select(x => new ModelForeignKey(x));
        protected override Type ElementType => elementType ??= LoadTypeByName(IEntityTypeTypeName);
    }


    private sealed class ModelForeignKey : ModelElement
    {
        private static Type? elementType;

        public ModelForeignKey(object foreignKey) : base(foreignKey)
        {
            PrincipalEntityType = new(GetElementMandatoryPropertyValue<object>("PrincipalEntityType"));
            Properties = GetElementMandatoryPropertyValue<IReadOnlyList<object>>("Properties").Select(x => new ModelProperty(x)).ToArray();
            IsRequired = GetElementMandatoryPropertyValue<bool>("IsRequired");
            var principalToDependent = GetElementOptionalPropertyValue<object>("PrincipalToDependent");
            PrincipalToDependent = principalToDependent == null ? null : new ModelNavigation(principalToDependent);
            var dependentToPrincipal = GetElementOptionalPropertyValue<object>("DependentToPrincipal");
            DependentToPrincipal = dependentToPrincipal == null ? null : new ModelNavigation(dependentToPrincipal);
        }

        public ModelEntityType PrincipalEntityType { get; }
        public IReadOnlyList<ModelProperty> Properties { get; }
        public ModelNavigation? PrincipalToDependent { get; }
        public ModelNavigation? DependentToPrincipal { get; }
        public bool IsRequired { get; }
        protected override Type ElementType => elementType ??= LoadTypeByName(IForeignKeyTypeName);
    }

    private sealed class ModelNavigation : ModelElement
    {
        private static Type? elementType;

        public ModelNavigation(object navigation) : base(navigation)
        {
            ClrType = GetElementMandatoryPropertyValue<Type>("ClrType");
            Name = GetElementMandatoryPropertyValue<string>("Name");
            PropertyInfo = GetElementOptionalPropertyValue<PropertyInfo>("PropertyInfo");
        }

        public Type ClrType { get; }
        public string Name { get; }
        public PropertyInfo? PropertyInfo { get; }
        protected override Type ElementType => elementType ??= LoadTypeByName(INavigationTypeName);
    }

    private sealed class ModelProperty : ModelElement
    {
        private static Type? elementType;
        private bool? isKey;
        private bool? isForeignKey;
        private bool? isPrimaryKey;

        public ModelProperty(object property) : base(property)
        {
            ClrType = GetElementMandatoryPropertyValue<Type>("ClrType");
            Name = GetElementMandatoryPropertyValue<string>("Name");
            PropertyInfo = GetElementOptionalPropertyValue<PropertyInfo>("PropertyInfo");
        }

        public Type ClrType { get; }
        public string Name { get; }
        public PropertyInfo? PropertyInfo { get; }
        public bool IsKey() => isKey ??= GetElementMandatoryMethodValue<bool>("IsKey");
        public bool IsForeignKey() => isForeignKey ??= GetElementMandatoryMethodValue<bool>("IsForeignKey");
        public bool IsPrimaryKey() => isPrimaryKey ??= GetElementMandatoryMethodValue<bool>("IsPrimaryKey");
        protected override Type ElementType => elementType ??= LoadTypeByName(IPropertyTypeName);
    }

    private const string DBContextTypeName = "Microsoft.EntityFrameworkCore.DbContext";
    private const string DBContextOptionsTypeName = "Microsoft.EntityFrameworkCore.DbContextOptions";
    private const string DbContextOptionsBuilderOpenTypeName = "Microsoft.EntityFrameworkCore.DbContextOptionsBuilder`1";
    private const string DbContextOptionsBuilderTypeName = "Microsoft.EntityFrameworkCore.DbContextOptionsBuilder";
    private const string InMemoryDbContextOptionsExtensionsTypeName = "Microsoft.EntityFrameworkCore.InMemoryDbContextOptionsExtensions";
    private const string InMemoryDbContextOptionsBuilderTypeName = "Microsoft.EntityFrameworkCore.Infrastructure.InMemoryDbContextOptionsBuilder";
    private const string IModelTypeName = "Microsoft.EntityFrameworkCore.Metadata.IModel";
    private const string IEntityTypeTypeName = "Microsoft.EntityFrameworkCore.Metadata.IEntityType";
    private const string IForeignKeyTypeName = "Microsoft.EntityFrameworkCore.Metadata.IForeignKey";
    private const string INavigationTypeName = "Microsoft.EntityFrameworkCore.Metadata.INavigation";
    private const string IPropertyTypeName = "Microsoft.EntityFrameworkCore.Metadata.IProperty";

    private static readonly string[] EfCoreRequiredAssemblyNames = new[] { "Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.InMemory" };
}