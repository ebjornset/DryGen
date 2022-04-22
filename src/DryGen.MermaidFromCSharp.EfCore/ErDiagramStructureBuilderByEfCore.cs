using DryGen.MermaidFromCSharp.ErDiagram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DryGen.MermaidFromCSharp.EfCore
{
    public class ErDiagramStructureBuilderByEfCore : IErDiagramStructureBuilder
    {
        public IReadOnlyList<ErDiagramEntity> GenerateErStructure(Assembly assembly, IReadOnlyList<ITypeFilter> typeFilters, IReadOnlyList<IPropertyFilter> attributeFilters, INameRewriter nameRewriter)
        {
            var efCoreEntityTypes = new List<IEntityType>();
            var dbContextTypesFromAssembly = assembly
                .GetTypes()
                .Where(t => typeof(DbContext).IsAssignableFrom(t) && ! t.IsAbstract)
                .ToList();
            foreach (var dbContextType in dbContextTypesFromAssembly)
            {
                efCoreEntityTypes.AddRange(LoadEfCoreEntitiesFromDbContextType(dbContextType, typeFilters));
            }
            var result = efCoreEntityTypes.Distinct(new EntityTypeEqualityComparer())
                .Select(et => new EfCoreErEntity(nameRewriter.Rewrite(et.ClrType.Name), et))
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
                if (entityLookup.ContainsKey(principalType))
                {
                    var principalEntity = entityLookup[principalType];
                    var isIdentifying = foreignKey.Properties?.All(p => p.IsKey()) == true;
                    if (propertyType.IsGenericType
                        && propertyType.GetGenericTypeDefinition().IsErDiagramRelationshipCollection())
                    {
                        if (propertyType.GetGenericArguments().Length > 1)
                        {
                            continue;
                        }
                        var labelBaseName = foreignKey.DependentToPrincipal?.Name ?? foreignKey.PrincipalToDependent?.Name ?? string.Empty;
                        var label = labelBaseName.GetRelationshipLabel((foreignKey.DependentToPrincipal == null ? entity : principalEntity).Name, replaceEntityNameAtEndOfPropertyName: foreignKey.DependentToPrincipal != null);
                        var fromCardianlity = foreignKey.IsRequired ? ErDiagramRelationshipCardinality.ExactlyOne : ErDiagramRelationshipCardinality.ZeroOrOne;
                        var toCardianlity = ErDiagramRelationshipCardinality.ZeroOrMore;
                        principalEntity.AddRelationship(entity, fromCardianlity, toCardianlity, label, propertyName, isIdentifying);
                    }
                    else
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
                }
            }
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

        private static IEnumerable<IEntityType> LoadEfCoreEntitiesFromDbContextType(Type dbContextType, IReadOnlyList<ITypeFilter> typeFilters)
        {
            var dbContextOptionsType = typeof(DbContextOptions);
            var optionsBuilderOpenType = typeof(DbContextOptionsBuilder<>);
            var optionsBuilderType = optionsBuilderOpenType.MakeGenericType(dbContextType);
            var optionsBuilderCtor = optionsBuilderType.GetConstructors().Where(x => x.GetParameters().Length == 0).Single();
            var optionsBuilder = (DbContextOptionsBuilder)optionsBuilderCtor.Invoke(null);
            var options = optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var contextCtor = dbContextType.GetConstructors().Where(x => x.GetParameters().Any(y => dbContextOptionsType.IsAssignableFrom(y.ParameterType))).FirstOrDefault();
            if (contextCtor == null)
            {
                throw new Exception($"{dbContextType.Name} has no public constructor with DbContextOptions as a parameter");
            }
            var ctorParameters = new object?[contextCtor.GetParameters().Length];
            var offset = 0;
            foreach (var parameterInfo in contextCtor.GetParameters())
            {
                var parameterType = parameterInfo.ParameterType;
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
            var dbContext = (DbContext)contextCtor.Invoke(ctorParameters);
            return dbContext.Model.GetEntityTypes().Where(et => typeFilters.All(filter => filter.Accepts(et.ClrType)));
        }

        private class EfCoreErEntity : ErDiagramEntity
        {
            public EfCoreErEntity(string name, IEntityType entityType) : base(name, entityType.ClrType)
            {
                EntityType = entityType;
            }

            public IEntityType EntityType { get; }
        }

        private class EntityTypeEqualityComparer : IEqualityComparer<IEntityType>
        {
            public bool Equals(IEntityType? x, IEntityType? y)
            {
                return (x?.ClrType == y?.ClrType) == true;
            }

            public int GetHashCode(IEntityType? obj)
            {
                return obj?.ClrType.GetHashCode() ?? 0;
            }
        }
    }
}
