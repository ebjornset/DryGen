using DryGen.Core;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DryGen.MermaidFromEfCore;

public abstract class ModelElement
{
    private readonly object element;

    protected ModelElement(object element)
    {
        CheckElementType(element);
        this.element = element;
    }

    protected abstract Type ElementType { get; }

    protected TType GetElementMandatoryPropertyValue<TType>(string propertyName)
    {
        var result = GetElementOptionalPropertyValue<TType>(propertyName);
        return result ?? throw new TypeMemberException($"'{typeof(TType).FullName}' return null for mandatory property '{propertyName}'");
    }

    protected TType? GetElementOptionalPropertyValue<TType>(string propertyName)
    {
        var propertyInfo = GetElementPropertyInfo(propertyName);
        var result = propertyInfo?.GetValue(element);
        if (result == default)
        {
            return default;
        }
        CheckExpectedType<TType>(result);
        return (TType)result;
    }

    protected PropertyInfo GetElementPropertyInfo(string propertyName)
    {
        var propertyInfo = ModelElement.GetPropertyInfoFromType(ElementType, propertyName);
        return propertyInfo ?? throw new TypeMemberException($"'{ElementType.FullName}' does not have a property named '{propertyName}'");
    }

    protected TType GetElementMandatoryMethodValue<TType>(string methodName)
    {
        var result = GetElementOptionalMethodValue<TType>(methodName);
        return result ?? throw new TypeMemberException($"'{typeof(TType).FullName}' return null for mandatory method '{methodName}'");
    }

    protected TType? GetElementOptionalMethodValue<TType>(string methodName)
    {
        var methodInfo = GetElementMethodInfo(methodName);
        var result = methodInfo.Invoke(element, null);
        if (result == default)
        {
            return default;
        }
        CheckExpectedType<TType>(result);
        return (TType)result;
    }

    protected MethodInfo GetElementMethodInfo(string methodName)
    {
        var methodInfo = ModelElement.GetMethodInfoFromType(ElementType, methodName);
        return methodInfo ?? throw new TypeMemberException($"'{ElementType.FullName}' does not have a method named '{methodName}'");
    }

    private static PropertyInfo? GetPropertyInfoFromType(Type? type, string propertyName)
    {
        if (type == null)
        {
            return null;
        }
        var propertyInfo = type.GetProperty(propertyName);
        if (propertyInfo != null) { return propertyInfo; }
        foreach (var allInterface in type.GetInterfaces())
        {
            propertyInfo = ModelElement.GetPropertyInfoFromType(allInterface, propertyName);
            if (propertyInfo != null)
            {
                return propertyInfo;
            }
        }
        return ModelElement.GetPropertyInfoFromType(type.BaseType, propertyName);
    }

    private static MethodInfo? GetMethodInfoFromType(Type? type, string methodName)
    {
        if (type == null)
        {
            return null;
        }
        var methodInfo = type.GetMethod(methodName);
        if (methodInfo != null) { return methodInfo; }
        foreach (var allInterface in type.GetInterfaces())
        {
            methodInfo = ModelElement.GetMethodInfoFromType(allInterface, methodName);
            if (methodInfo != null)
            {
                return methodInfo;
            }
        }
        return ModelElement.GetMethodInfoFromType(type.BaseType, methodName);
    }

    [ExcludeFromCodeCoverage] // Just a guard rail for unexpected structures
    private static void CheckExpectedType<TType>(object? instance)
    {
        if (!typeof(TType).IsAssignableFrom(instance?.GetType()))
        {
            throw new TypeMemberException($"'{typeof(TType).FullName}' is not assignable from '{instance?.GetType().FullName}'");
        }
    }

    [ExcludeFromCodeCoverage] // Just a guard rail for unexpected structures
    private void CheckElementType(object element)
    {
        if (!ElementType.IsInstanceOfType(element))
        {
            throw new TypeMemberException($"'{ElementType.FullName}' is not assignable from '{element.GetType().FullName}'");
        }
    }

    protected const string IEntityTypeTypeName = "Microsoft.EntityFrameworkCore.Metadata.IEntityType";
    protected const string IForeignKeyTypeName = "Microsoft.EntityFrameworkCore.Metadata.IForeignKey";
    protected const string INavigationTypeName = "Microsoft.EntityFrameworkCore.Metadata.INavigation";
    protected const string IPropertyTypeName = "Microsoft.EntityFrameworkCore.Metadata.IProperty";
}