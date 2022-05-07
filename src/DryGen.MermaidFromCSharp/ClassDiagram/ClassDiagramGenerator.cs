using DryGen.MermaidFromCSharp.TypeFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DryGen.MermaidFromCSharp.ClassDiagram
{

    public class ClassDiagramGenerator : IClassDiagramGenerator
    {
        private readonly ITypeLoader typeloader;
        private readonly ClassDiagramDirection? direction;
        private readonly ClassDiagramAttributeLevel attributeLevel;
        private readonly ClassDiagramMethodLevel methodLevel;
        private readonly bool excludeStaticAttributes;
        private readonly bool excludeStaticMethods;
        private readonly bool excludeMethodParams;

        public ClassDiagramGenerator(
            ITypeLoader typeloader,
            ClassDiagramDirection? direction,
            ClassDiagramAttributeLevel attributeLevel,
            ClassDiagramMethodLevel methodLevel,
            bool excludeStaticAttributes,
            bool excludeStaticMethods,
            bool excludeMethodParams)
        {
            this.typeloader = typeloader;
            this.direction = direction;
            this.attributeLevel = attributeLevel;
            this.methodLevel = methodLevel;
            this.excludeStaticAttributes = excludeStaticAttributes;
            this.excludeStaticMethods = excludeStaticMethods;
            this.excludeMethodParams = excludeMethodParams;
        }

        public string Generate(Assembly assembly, IReadOnlyList<ITypeFilter> typeFilters, IReadOnlyList<IPropertyFilter> attributeFilters, INameRewriter? nameRewriter)
        {
            var classes = typeloader.Load(assembly, ClassDiagramFilters(typeFilters), nameRewriter).Select(x => new ClassDiagramClass(x)).ToList();
            GenerateClassDiagramStructure(classes, attributeFilters);
            var result = GenerateClassDiagramMermaid(classes, nameRewriter);
            return result;
        }

        private void GenerateClassDiagramStructure(IReadOnlyList<ClassDiagramClass> classes, IReadOnlyList<IPropertyFilter> attributeFilters)
        {
            var classLookup = classes.ToDictionary(x => x.Type, x => x);
            foreach (var classDiagramClass in classes)
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
            foreach (var classDiagramClass in classes)
            {
                classDiagramClass.RemoveBidirectionalRelationshipDuplicates();
            }
        }

        private static void GenerateClassAssociationsCompositionsAndAggregations(IDictionary<Type, ClassDiagramClass> classLookup, ClassDiagramClass classDiagramClass)
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

        private static void GenerateClassInheritanceOrRealizationForInterfaces(IDictionary<Type, ClassDiagramClass> classLookup, ClassDiagramClass classDiagramClass)
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

        private static void GenerateClassInheritanceForBaseType(IDictionary<Type, ClassDiagramClass> classLookup, ClassDiagramClass classDiagramClass)
        {
            if (!classDiagramClass.Type.IsInterface && classDiagramClass.Type.BaseType != null && classLookup.ContainsKey(classDiagramClass.Type.BaseType))
            {
                classDiagramClass.AddRelationship(
                        ClassDiagramRelationshipCardinality.Unspecified,
                        ClassDiagramRelationshipType.Inheritance,
                        ClassDiagramRelationshipCardinality.Unspecified,
                        classLookup[classDiagramClass.Type.BaseType], string.Empty, string.Empty);
            }
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
            if (classLookup.ContainsKey(parameterType))
            {
                classDiagramClass.AddRelationship(
                    ClassDiagramRelationshipCardinality.Unspecified,
                    ClassDiagramRelationshipType.Dependency,
                    ClassDiagramRelationshipCardinality.Unspecified,
                    classLookup[parameterType], string.Empty, string.Empty);
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
            foreach (var method in classDiagramClass.Type.GetMethods(bindingFlags).Where(x => IsNotGetterOrSetterOrLocalFunction(x)))
            {
                if (method.IsPrivate && methodLevel != ClassDiagramMethodLevel.All)
                {
                    continue;
                }
                if (method.IsFamily && (methodLevel == ClassDiagramMethodLevel.Public || methodLevel == ClassDiagramMethodLevel.Internal))
                {
                    continue;
                }
                if (method.IsAssembly && methodLevel == ClassDiagramMethodLevel.Public)
                {
                    continue;
                }
                var returnTypeType = GetDataType(method.ReturnType);
                var methodName = method.Name;
                var visibility = GetVisibility(method);
                var isStatic = method.IsStatic;
                var isAbstract = method.IsAbstract;
                var parameters = method.GetParameters().Select(x => new ClassDiagramMethodParameter(GetDataType(x.ParameterType), x.Name)).ToList();
                classDiagramClass.AddMethod(new ClassDiagramMethod(returnTypeType, methodName, visibility, isStatic, isAbstract, parameters));
            }
        }

        private string GenerateClassDiagramMermaid(IEnumerable<ClassDiagramClass> classes, INameRewriter? nameRewriter)
        {
            var sb = new StringBuilder().AppendLine("classDiagram");
            AppendDirection(sb);
            AppendClasses(classes, nameRewriter, sb);
            AppendRelationships(classes, sb);
            return sb.ToString();
        }

        private void AppendDirection(StringBuilder sb)
        {
            if (direction != null && direction != ClassDiagramDirection.Default)
            {
                sb.Append("\tdirection ").AppendLine(direction.ToString());
            }
        }

        private void AppendClasses(IEnumerable<ClassDiagramClass> classes, INameRewriter? nameRewriter, StringBuilder sb)
        {
            foreach (var classDiagramClass in classes)
            {
                // Append class with any attributes
                var dataType = GetDataType(classDiagramClass.Type);
                sb.Append("\tclass ").Append(nameRewriter?.Rewrite(dataType) ?? dataType).AppendLine(" {");
                if (classDiagramClass.Type.IsInterface)
                {
                    sb.AppendLine("\t\t<<interface>>");
                }
                else if (classDiagramClass.Type.IsAbstract)
                {
                    sb.AppendLine("\t\t<<abstract>>");
                }
                if (classDiagramClass.Type.IsEnum)
                {
                    AppendEnumerationToClass(sb, classDiagramClass);
                }
                else
                {
                    AppendAttributesToClass(sb, classDiagramClass);
                    AppendMethodsToClass(sb, classDiagramClass);
                }
                sb.Append("\t").AppendLine("}");
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
                sb.Append("\t").Append("\t").Append(attribute.Visibility).Append(attribute.AttributeType).Append(' ').Append(attribute.AttributeName);
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
                sb.Append("\t").Append("\t").Append(method.Visibility).Append(method.MethodName).Append('(');
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

        private static void AppendRelationships(IEnumerable<ClassDiagramClass> classes, StringBuilder sb)
        {
            foreach (var classDiagramClass in classes)
            {
                foreach (var relationship in classDiagramClass.Relationships.OrderBy(x => x.PropertyName))
                {
                    sb.Append("\t").Append(classDiagramClass.Name).Append(relationship.GetRelationshipPattern()).Append(relationship.To.Name).AppendLine(relationship.GetRelationshipLabel());
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

        private IReadOnlyList<ITypeFilter> ClassDiagramFilters(IReadOnlyList<ITypeFilter> filters)
        {
            var result = new List<ITypeFilter> { new ExcludeNonPublicClassTypeFilter(), new ExcludeSystemObjectAndSystemEnumTypeFilter() };
            result.AddRange(filters);
            return result;
        }

        private static bool IsAttributePropertyType(IDictionary<Type, ClassDiagramClass> classLookup, PropertyInfo property)
        {
            return property.CanRead
                && !property.PropertyType.IsArray
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

        private static string GetDataType(Type type, string genericStartBracket = "~", string genericEndBracket = "~")
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
                    typeName = GetDataTypeForGenericType(type, genericStartBracket, genericEndBracket);
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
            // TODO The syntax don't allow ? in type so we must use comment for nullable types.
            // return nullableUnderlyingType == null ? result : $"{result}?";
            return result;

        }

        static string GetTypeName(string typeName)
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
                _ => typeName,
            };
        }

        private static string GetDataTypeForGenericType(Type type, string genericStartBracket, string genericEndBracket)
        {
            string typeName;
            var sb = new StringBuilder();
            sb.Append(type.Name[..type.Name.IndexOf('`')]).Append(genericStartBracket);
            var delimiter = string.Empty;
            foreach (var genericArgument in type.GetGenericArguments())
            {
                sb.Append(delimiter);
                var genericParameterConstraints = GetGenericParameterConstraints(genericArgument);
                if (genericParameterConstraints.Length > 0)
                {
                    AppendGenericParameterConstraints(sb, genericParameterConstraints);
                }
                else
                {
                    sb.Append(GetDataType(type: genericArgument, genericStartBracket: "Of", genericEndBracket: string.Empty));
                }
                delimiter = ",";
            }
            sb.Append(genericEndBracket);
            typeName = sb.ToString();
            return typeName;

            static Type[] GetGenericParameterConstraints(Type genericArgument)
            {
                return genericArgument.IsGenericParameter ? genericArgument.GetGenericParameterConstraints().ToArray() : Array.Empty<Type>();
            }

            static void AppendGenericParameterConstraints(StringBuilder sb, Type[] genericParameterConstraints)
            {
                var constraintDelimiter = string.Empty;
                foreach (var genericParameterConstraint in genericParameterConstraints)
                {
                    sb.Append(constraintDelimiter).Append(GetDataType(type: genericParameterConstraint, genericStartBracket: "Of", genericEndBracket: string.Empty));
                    constraintDelimiter = "'";
                }
            }
        }

        private static string GetVisibility(MethodInfo methodInfo)
        {
            string visibility = "";
            if (methodInfo.IsPublic)
                return "+";
            else if (methodInfo.IsPrivate)
                return "-";
            else
               if (methodInfo.IsFamily)
                visibility = "#";
            else if (methodInfo.IsAssembly)
                visibility += "~";
            return visibility;
        }

        private static readonly Type[] collectionTypes = { typeof(IEnumerable<>), typeof(ICollection<>), typeof(IList<>), typeof(IReadOnlyList<>), typeof(IReadOnlyCollection<>) };
    }
}
