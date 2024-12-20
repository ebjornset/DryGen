﻿using DryGen.Core;
using DryGen.MermaidFromCSharp.TypeFilters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DryGen.MermaidFromCSharp.ClassDiagram;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
    Justification = "We need to access private members to generate detailed class diagrams.")]
public class ClassDiagramGenerator : IClassDiagramGenerator
{
    private readonly ITypeLoader typeloader;
    private readonly ClassDiagramDirection? direction;
    private readonly ClassDiagramAttributeLevel attributeLevel;
    private readonly ClassDiagramMethodLevel methodLevel;
    private readonly bool excludeStaticAttributes;
    private readonly bool excludeStaticMethods;
    private readonly bool excludeMethodParams;
    private readonly string? title;

    public ClassDiagramGenerator(
        ITypeLoader typeloader,
        IMermaidClassDiagramFromCSharpOptions options)
    {
        this.typeloader = typeloader;
        direction = options.Direction;
        attributeLevel = options.AttributeLevel ?? default;
        methodLevel = options.MethodLevel ?? default;
        excludeStaticAttributes = options.ExcludeStaticAttributes ?? default;
        excludeStaticMethods = options.ExcludeStaticMethods ?? default;
        excludeMethodParams = options.ExcludeMethodParams ?? default;
        title = options.Title;
    }

    public string Generate(Assembly assembly, IReadOnlyList<ITypeFilter> typeFilters, IReadOnlyList<IPropertyFilter> attributeFilters, INameRewriter? nameRewriter, IDiagramFilter diagramFilter)
    {
        IEnumerable<ClassDiagramClass> classDiagramClasses = typeloader.Load(assembly, ClassDiagramFilters(typeFilters), nameRewriter).Select(x => new ClassDiagramClass(x)).ToList();
        GenerateClassDiagramStructure(classDiagramClasses, attributeFilters);
        classDiagramClasses = diagramFilter.Filter(classDiagramClasses);
        classDiagramClasses = ConvertExtensionMethodsToInstanceMethodsOnKnownTypes(classDiagramClasses);
        var result = GenerateClassDiagramMermaid(classDiagramClasses, nameRewriter);
        return result;
    }

    private void GenerateClassDiagramStructure(IEnumerable<ClassDiagramClass> classDiagramClasses, IReadOnlyList<IPropertyFilter> attributeFilters)
    {
        var classLookup = classDiagramClasses.ToDictionary(x => x.Type, x => x);
        foreach (var classDiagramClass in classDiagramClasses)
        {
            if (classDiagramClass.Type.IsEnum)
            {
                continue;
            }
            GenerateClassAttributes(classLookup, classDiagramClass, attributeFilters);
            GenerateClassMethods(classDiagramClass);
            GenerateClassDependencies(classLookup, classDiagramClass);
            GenerateClassAssociationsCompositionsAndAggregations(classLookup, classDiagramClass);
            GenerateClassInheritanceOrRealizationForInterfaces(classLookup, classDiagramClass);
            GenerateClassInheritanceForBaseType(classLookup, classDiagramClass);
        }
        foreach (var classDiagramClass in classDiagramClasses)
        {
            classDiagramClass.RemoveBidirectionalRelationshipDuplicates();
        }
        foreach (var classDiagramClass in classDiagramClasses)
        {
            classDiagramClass.MergeTwoOneToManyIntoOneMayToMany();
        }
    }

    private static ClassDiagramClass[] ConvertExtensionMethodsToInstanceMethodsOnKnownTypes(IEnumerable<ClassDiagramClass> classDiagramClasses)
    {
        var classLookup = classDiagramClasses.ToDictionary(x => x.Type, x => x);
        var removedExtensionClasses = new List<ClassDiagramClass>();
        foreach (var extensionClass in classDiagramClasses.Where(x => x.Type.IsExtensionType()))
        {
            foreach (var extensionMethod in extensionClass.Methods.Where(x => x.MethodInfo.IsExtensionMethod()).ToArray())
            {
                var extendedType = extensionMethod.MethodInfo.GetParameters()[0].ParameterType;
                if (classLookup.TryGetValue(extendedType, out var extendedClass))
                {
                    extensionClass.PromoteMethodToExtendedClass(extensionMethod, extendedClass);
                }
            }
            if (!extensionClass.Methods.Any())
            {
                removedExtensionClasses.Add(extensionClass);
            }
        }
        return classDiagramClasses.Except(removedExtensionClasses).ToArray();
    }

    private static void GenerateClassAssociationsCompositionsAndAggregations(Dictionary<Type, ClassDiagramClass> classLookup, ClassDiagramClass classDiagramClass)
    {
        foreach (var property in classDiagramClass.Type.GetProperties(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly))
        {
            if (!property.CanRead)
            {
                continue;
            }
            var (isRelation, isCollection, isNullable, propertyType) = GetPropertyTypeRelationshipInfo(classLookup, property.PropertyType);
            if (!isRelation)
            {
                continue;
            }
            var fromCardinality = ClassDiagramRelationshipCardinality.Unspecified;
            if (isCollection)
            {
                var toClass = classLookup[propertyType.GetGenericArguments()[0]];
                var label = property.Name.GetRelationshipLabel(toClass.Name, true);
                classDiagramClass.AddRelationship(
                    fromCardinality,
                    ClassDiagramRelationshipType.Composition,
                    ClassDiagramRelationshipCardinality.ZeroOrMore,
                    toClass, label, property.Name);
            }
            else
            {
                var toClass = classLookup[propertyType];
                var label = property.Name.GetRelationshipLabel(toClass.Name, true);
                var toCardinality = property.GetAssociationToCardinality(isNullable);
                classDiagramClass.AddRelationship(
                    fromCardinality,
                    ClassDiagramRelationshipType.Association,
                    toCardinality,
                    toClass, label, string.Empty);
            }
        }
    }

    private static (bool isRelation, bool isCollection, bool isNullable, Type propertyType) GetPropertyTypeRelationshipInfo(IDictionary<Type, ClassDiagramClass> classLookup, Type propertyType)
    {
        if (classLookup.ContainsKey(propertyType))
        {
            return (isRelation: true, isCollection: false, isNullable: false, propertyType);
        }
        else if (IsCollectionOfKnownType(classLookup, propertyType))
        {
            return (isRelation: true, isCollection: true, isNullable: false, propertyType);
        }
        var nullableUnderlyingType = Nullable.GetUnderlyingType(propertyType);
        if (nullableUnderlyingType != null)
        {
            var nullableResult = GetPropertyTypeRelationshipInfo(classLookup, nullableUnderlyingType);
            if (nullableResult.isRelation)
            {
                return (isRelation: true, nullableResult.isCollection, isNullable: true, nullableUnderlyingType);
            }
        }
        return (false, false, false, typeof(object));
    }

    private static bool IsCollectionOfKnownType(IDictionary<Type, ClassDiagramClass> classLookup, Type type)
    {
        return type.IsGenericType
            && collectionTypes.Contains(type.GetGenericTypeDefinition())
            && type.GetGenericArguments().Length == 1
            && classLookup.ContainsKey(type.GetGenericArguments()[0]);
    }

    private static void GenerateClassInheritanceOrRealizationForInterfaces(Dictionary<Type, ClassDiagramClass> classLookup, ClassDiagramClass classDiagramClass)
    {
        foreach (var directInterface in classDiagramClass.Type.GetDirectInterfaces().Where(directInterface => classLookup.ContainsKey(directInterface)))
        {
            var relationshipType = classDiagramClass.Type.IsInterface ? ClassDiagramRelationshipType.Inheritance : ClassDiagramRelationshipType.Realization;
            classDiagramClass.AddRelationship(
                ClassDiagramRelationshipCardinality.Unspecified,
                relationshipType,
                ClassDiagramRelationshipCardinality.Unspecified,
                classLookup[directInterface], string.Empty, string.Empty);
        }
    }

    private static void GenerateClassInheritanceForBaseType(Dictionary<Type, ClassDiagramClass> classLookup, ClassDiagramClass classDiagramClass)
    {
        if (classDiagramClass.Type.IsInterface)
        {
            return;
        }
        var baseType = classDiagramClass.Type.BaseType;
        baseType = GetNonClosedGenericBaseType(baseType, classLookup);
        if (baseType == null)
        {
            return;
        }
        classDiagramClass.AddRelationship(
            ClassDiagramRelationshipCardinality.Unspecified,
                ClassDiagramRelationshipType.Inheritance,
                ClassDiagramRelationshipCardinality.Unspecified,
                classLookup[baseType], string.Empty, string.Empty);
    }

    private static Type? GetNonClosedGenericBaseType(Type? baseType, IDictionary<Type, ClassDiagramClass> classLookup)
    {
        if (baseType == null)
        {
            return null;
        }
        if (excludeClosedGenericTypeTypeFilter.Accepts(baseType))
        {
            return classLookup.ContainsKey(baseType) ? baseType : null;
        }
        return classLookup.Values.FirstOrDefault(x => x.Type.Name == baseType.Name)?.Type;
    }

    private static void GenerateClassDependencies(IDictionary<Type, ClassDiagramClass> classLookup, ClassDiagramClass classDiagramClass)
    {
        foreach (var constructor in classDiagramClass.Type.GetConstructors(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly))
        {
            foreach (var parameterType in constructor.GetParameters().Select(parameter => parameter.ParameterType))
            {
                AddDependency(classLookup, classDiagramClass, parameterType);
            }
        }
    }

    private static void AddDependency(IDictionary<Type, ClassDiagramClass> classLookup, ClassDiagramClass classDiagramClass, Type parameterType)
    {
        if (classLookup.TryGetValue(parameterType, out var classLookupParameterType))
        {
            classDiagramClass.AddRelationship(
                ClassDiagramRelationshipCardinality.Unspecified,
                ClassDiagramRelationshipType.Dependency,
                ClassDiagramRelationshipCardinality.Unspecified,
				classLookupParameterType, string.Empty, string.Empty);
        }
        else if (parameterType.IsGenericType)
        {
            foreach (var genericArgument in parameterType.GetGenericArguments())
            {
                AddDependency(classLookup, classDiagramClass, genericArgument);
            }
        }
    }

    private void GenerateClassAttributes(IDictionary<Type, ClassDiagramClass> classLookup, ClassDiagramClass classDiagramClass, IReadOnlyList<IPropertyFilter> attributeFilters)
    {
        if (attributeLevel == ClassDiagramAttributeLevel.None)
        {
            return;
        }
        var bindingFlags = BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.DeclaredOnly;
        if (!excludeStaticAttributes)
        {
            bindingFlags |= BindingFlags.Static;
        }
        if (attributeLevel != ClassDiagramAttributeLevel.Public)
        {
            bindingFlags |= BindingFlags.NonPublic;
        }
        foreach (var property in classDiagramClass.Type.GetProperties(bindingFlags).Where(p => attributeFilters.All(f => f.Accepts(p))).Where(property => IsAttributePropertyType(classLookup, property)))
        {
            var attributeType = GetDataType(property.PropertyType);
            var attributeName = property.Name;
            var getAccessor = property.GetAccessors(true)[0];
            if (getAccessor.IsPrivate && attributeLevel != ClassDiagramAttributeLevel.All)
            {
                continue;
            }
            if (getAccessor.IsFamily && (attributeLevel == ClassDiagramAttributeLevel.Public || attributeLevel == ClassDiagramAttributeLevel.Internal))
            {
                continue;
            }
            if (getAccessor.IsAssembly && attributeLevel == ClassDiagramAttributeLevel.Public)
            {
                continue;
            }
            var visibility = GetVisibility(getAccessor);
            var isStatic = getAccessor.IsStatic;
            classDiagramClass.AddAttribute(new ClassDiagramAttribute(attributeType, attributeName, visibility, isStatic));
        }
    }

    private void GenerateClassMethods(ClassDiagramClass classDiagramClass)
    {
        if (methodLevel == ClassDiagramMethodLevel.None)
        {
            return;
        }
        var bindingFlags = BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.DeclaredOnly;
        if (!excludeStaticMethods)
        {
            bindingFlags |= BindingFlags.Static;
        }
        if (methodLevel != ClassDiagramMethodLevel.Public)
        {
            bindingFlags |= BindingFlags.NonPublic;
        }
        foreach (var methodInfo in classDiagramClass.Type.GetMethods(bindingFlags).Where(x => IsNotGetterOrSetterOrLocalFunction(x)))
        {
            if (IsMethodWithToLowVisibility(methodInfo))
            {
                continue;
            }
            if (IsSyntheticCompilerGeneratedMethod(methodInfo))
            {
                continue;
            }
            var returnTypeType = GetDataType(methodInfo.ReturnType);
            var methodName = methodInfo.Name;
            var visibility = GetVisibility(methodInfo);
            var isStatic = methodInfo.IsStatic;
            var isAbstract = methodInfo.IsAbstract;
            var parameters = methodInfo.GetParameters().Select(x => new ClassDiagramMethodParameter(GetDataType(x.ParameterType), x.Name.AsNonNull())).ToList();
            classDiagramClass.AddMethod(new ClassDiagramMethod(returnTypeType, methodName, visibility, isStatic, isAbstract, parameters, methodInfo));
        }
    }

    private bool IsMethodWithToLowVisibility(MethodInfo method)
    {
        return (method.IsPrivate && methodLevel != ClassDiagramMethodLevel.All)
            || (method.IsFamily && (methodLevel == ClassDiagramMethodLevel.Public || methodLevel == ClassDiagramMethodLevel.Internal))
            || (method.IsAssembly && methodLevel == ClassDiagramMethodLevel.Public);
    }

    private static bool IsSyntheticCompilerGeneratedMethod(MethodInfo method)
    {
        return method.IsPrivate && method.Name.StartsWith('<');
    }

    private string GenerateClassDiagramMermaid(IEnumerable<ClassDiagramClass> classDiagramClasses, INameRewriter? nameRewriter)
    {
        var sb = new StringBuilder().AppendDiagramTitle(title).AppendLine("classDiagram");
        AppendDirection(sb);
        AppendClasses(classDiagramClasses, nameRewriter, sb);
        AppendRelationships(classDiagramClasses, nameRewriter, sb);
        return sb.ToString();
    }

    private void AppendDirection(StringBuilder sb)
    {
        if (direction != null && direction != ClassDiagramDirection.Default)
        {
            sb.Append("\tdirection ").AppendLine(direction.ToString());
        }
    }

    private void AppendClasses(IEnumerable<ClassDiagramClass> classDiagramClasses, INameRewriter? nameRewriter, StringBuilder sb)
    {
        foreach (var classDiagramClass in classDiagramClasses)
        {
            var classContentBuilder = new StringBuilder();
            // Append class with any attributes
            var dataType = GetDataType(classDiagramClass.Type, nameRewriter);
            sb.Append("\tclass ").Append(dataType);
            if (classDiagramClass.Type.IsInterface)
            {
                classContentBuilder.AppendLine("\t\t<<interface>>");
            }
            else if (classDiagramClass.Type.IsAbstract)
            {
                classContentBuilder.AppendLine("\t\t<<abstract>>");
            }
            if (classDiagramClass.Type.IsEnum)
            {
                AppendEnumerationToClass(classContentBuilder, classDiagramClass);
            }
            else
            {
                AppendAttributesToClass(classContentBuilder, classDiagramClass);
                AppendMethodsToClass(classContentBuilder, classDiagramClass);
            }
            if (classContentBuilder.Length > 0)
            {
                sb.AppendLine(" {");
                sb.Append(classContentBuilder);
                sb.Append('\t').Append('}');
            }
            sb.AppendLine();
        }
    }

    private void AppendEnumerationToClass(StringBuilder sb, ClassDiagramClass classDiagramClass)
    {
        sb.AppendLine("\t\t<<enumeration>>");
        if (attributeLevel != ClassDiagramAttributeLevel.None)
        {
            foreach (var enumName in classDiagramClass.Type.GetEnumNames())
            {
                sb.Append("\t\t").AppendLine(enumName);
            }
        }
    }

    private static void AppendAttributesToClass(StringBuilder sb, ClassDiagramClass classDiagramClass)
    {
        foreach (var attribute in classDiagramClass.Attributes)
        {
            sb.Append('\t').Append('\t').Append(attribute.Visibility).Append(attribute.AttributeType).Append(' ').Append(attribute.AttributeName);
            if (attribute.IsStatic)
            {
                sb.Append('$');
            }
            sb.AppendLine();
        }
    }

    private void AppendMethodsToClass(StringBuilder sb, ClassDiagramClass classDiagramClass)
    {
        foreach (var method in classDiagramClass.Methods)
        {
            sb.Append('\t').Append('\t').Append(method.Visibility).Append(method.MethodName).Append('(');
            if (excludeMethodParams)
            {
                AppedParamsSummaryToClassMethod(sb, method);
            }
            else
            {
                AppendParamsToClassMethod(sb, method);
            }
            sb.Append(')');
            if (method.IsStatic)
            {
                sb.Append('$');
            }
            if (method.IsAbstract)
            {
                sb.Append('*');
            }
            if (!string.IsNullOrEmpty(method.ReturnType))
            {
                sb.Append(' ').Append(method.ReturnType);
            }
            sb.AppendLine();
        }
    }

    private static void AppendParamsToClassMethod(StringBuilder sb, ClassDiagramMethod method)
    {
        var delimiter = string.Empty;
        foreach (var parameter in method.Parameters)
        {
            sb.Append(delimiter).Append(parameter.ParameterType).Append(' ').Append(parameter.ParameterName);
            delimiter = ", ";
        }
    }

    private static void AppedParamsSummaryToClassMethod(StringBuilder sb, ClassDiagramMethod method)
    {
        if (method.Parameters.Count == 1)
        {
            sb.Append("1 param");
        }
        else if (method.Parameters.Count > 1)
        {
            sb.Append($"{method.Parameters.Count} params");
        }
    }

    private static void AppendRelationships(IEnumerable<ClassDiagramClass> classes, INameRewriter? nameRewriter, StringBuilder sb)
    {
        foreach (var classDiagramClass in classes)
        {
            foreach (var relationship in classDiagramClass.Relationships.OrderBy(x => x.PropertyName))
            {
                var dataTypeFrom = GetDataType(classDiagramClass.Type, nameRewriter);
                var dataTypeTo = GetDataType(relationship.To.Type, nameRewriter);
                sb.Append('\t').Append(dataTypeFrom).Append(relationship.GetRelationshipPattern()).Append(dataTypeTo).AppendLine(relationship.GetRelationshipLabel());
            }
        }
    }

    private static bool IsNotGetterOrSetterOrLocalFunction(MethodInfo method)
    {
        var methodName = method.Name;
        return !methodName.StartsWith("get_")
            && !methodName.StartsWith("set_")
            && !methodName.Contains(".get_")
            && !methodName.Contains(".set_")
            && !methodName.Contains(">g__");
    }

    private static List<ITypeFilter> ClassDiagramFilters(IReadOnlyList<ITypeFilter> filters)
    {
        var result = new List<ITypeFilter> { new ExcludeNonPublicClassTypeFilter(), new ExcludeSystemObjectAndSystemEnumTypeFilter(), new ExcludeClosedGenericTypeTypeFilter() };
        result.AddRange(filters);
        return result;
    }

    private static bool IsAttributePropertyType(IDictionary<Type, ClassDiagramClass> classLookup, PropertyInfo property)
    {
        return property.CanRead
            && (!property.PropertyType.IsArray || property.PropertyType.IsAssignableFrom(typeof(byte[])))
            && IsAttributePropertyType(classLookup, property.PropertyType);
    }

    private static bool IsAttributePropertyType(IDictionary<Type, ClassDiagramClass> classLookup, Type propertyType)
    {
        if (classLookup.ContainsKey(propertyType) || IsCollectionOfKnownType(classLookup, propertyType))
        {
            return false;
        }
        var nullableUnderlyingType = Nullable.GetUnderlyingType(propertyType);
        if (nullableUnderlyingType != null)
        {
            return IsAttributePropertyType(classLookup, nullableUnderlyingType);
        }
        return true;
    }

    private static string GetDataType(Type type, INameRewriter? nameRewriter = null, string genericStartBracket = "~", string genericEndBracket = "~", HashSet<Type>? seenTypes = null)
    {
        if (type == typeof(void))
        {
            return string.Empty;
        }
        string typeName;
        var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
        if (nullableUnderlyingType == null)
        {
            if (type.IsGenericType)
            {
                typeName = GetDataTypeForGenericType(type, genericStartBracket, genericEndBracket, seenTypes);
            }
            else
            {
                typeName = type.Name;
            }
        }
        else
        {
            typeName = nullableUnderlyingType.Name;
        }
        var result = GetTypeName(typeName);
        if (nameRewriter != null)
        {
            result = nameRewriter.Rewrite(result);
        }
        return result;
    }

    private static string GetTypeName(string typeName)
    {
        return typeName switch
        {
            "Int32" => "int",
            "Int64" => "long",
            "Single" => "float",
            "Boolean" => "bool",
            "String" => "string",
            "Decimal" => "decimal",
            "Object" => "object",
            "Double" => "double",
            "Byte[]" => "Blob",
            _ => typeName,
        };
    }

    private static string GetDataTypeForGenericType(Type type, string genericStartBracket, string genericEndBracket, HashSet<Type>? seenTypes)
    {
        seenTypes ??= new HashSet<Type>();
        string typeName;
        var sb = new StringBuilder();
        sb.Append(type.Name[..type.Name.IndexOf('`')]).Append(genericStartBracket);
        var delimiter = string.Empty;
        seenTypes.Add(type);
        foreach (var genericArgument in type.GetGenericArguments())
        {
            sb.Append(delimiter);
            var genericParameterConstraints = GetGenericParameterConstraints(genericArgument);
            if (genericParameterConstraints.Length > 0)
            {
                AppendGenericParameterConstraints(sb, genericParameterConstraints, seenTypes, genericArgument);
            }
            else
            {
                sb.Append(GetDataType(type: genericArgument, genericStartBracket: "Of", genericEndBracket: string.Empty));
            }
            delimiter = ",";
        }
        seenTypes.Remove(type);
        sb.Append(genericEndBracket);
        typeName = sb.ToString();
        return typeName;

        static Type[] GetGenericParameterConstraints(Type genericArgument)
        {
            return genericArgument.IsGenericParameter ? genericArgument.GetGenericParameterConstraints().ToArray() : Array.Empty<Type>();
        }

        static void AppendGenericParameterConstraints(StringBuilder sb, Type[] genericParameterConstraints, HashSet<Type> seenTypes, Type genericArgument)
        {
            var constraintDelimiter = string.Empty;
            foreach (var genericParameterConstraint in genericParameterConstraints)
            {
                var type = seenTypes.Contains(genericParameterConstraint) ? genericArgument  : genericParameterConstraint;
                var dataType = GetDataType(type: type, genericStartBracket: "Of", genericEndBracket: string.Empty, seenTypes: seenTypes);
                sb.Append(constraintDelimiter).Append(dataType);
                constraintDelimiter = "'";
            }
        }
    }

    private static string GetVisibility(MethodInfo methodInfo)
    {
        string visibility = "";
        if (methodInfo.IsPublic)
        {
            return "+";
        }
        else if (methodInfo.IsPrivate)
        {
            return "-";
        }
        else if (methodInfo.IsFamily)
        {
            visibility = "#";
        }
        else if (methodInfo.IsAssembly)
        {
            visibility += "~";
        }

        return visibility;
    }

    private static readonly Type[] collectionTypes = { typeof(IEnumerable<>), typeof(ICollection<>), typeof(IList<>), typeof(IReadOnlyList<>), typeof(IReadOnlyCollection<>), typeof(List<>), typeof(Collection<>) };
    private static readonly ExcludeClosedGenericTypeTypeFilter excludeClosedGenericTypeTypeFilter = new();
}