using DryGen.Core;
using System;
using System.Reflection;

namespace DryGen.MermaidFromEfCore;

public class ModelNavigation : ModelElement
{
    private static Type? elementType;

    public ModelNavigation(object navigation) : base(navigation)
    {
        ClrType = GetElementMandatoryPropertyValue<Type>(nameof(ClrType));
        Name = GetElementMandatoryPropertyValue<string>(nameof(Name));
        PropertyInfo = GetElementOptionalPropertyValue<PropertyInfo>(nameof(PropertyInfo));
    }

    public Type ClrType { get; }
    public string Name { get; }
    public PropertyInfo? PropertyInfo { get; }
    protected override Type ElementType => elementType ??= INavigationTypeName.LoadTypeByName();
}
