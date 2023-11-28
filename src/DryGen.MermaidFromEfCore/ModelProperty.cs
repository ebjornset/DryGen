using DryGen.Core;
using System;
using System.Reflection;

namespace DryGen.MermaidFromEfCore;

public class ModelProperty : ModelElement
{
    private static Type? elementType;
    private bool? isKey;
    private bool? isForeignKey;
    private bool? isPrimaryKey;

    public ModelProperty(object property) : base(property)
    {
        ClrType = GetElementMandatoryPropertyValue<Type>(nameof(ClrType));
        Name = GetElementMandatoryPropertyValue<string>(nameof(Name));
        PropertyInfo = GetElementOptionalPropertyValue<PropertyInfo>(nameof(PropertyInfo));
    }

    public Type ClrType { get; }
    public string Name { get; }
    public PropertyInfo? PropertyInfo { get; }

    public bool IsKey() => isKey ??= GetElementMandatoryMethodValue<bool>("IsKey");

    public bool IsForeignKey() => isForeignKey ??= GetElementMandatoryMethodValue<bool>("IsForeignKey");

    public bool IsPrimaryKey() => isPrimaryKey ??= GetElementMandatoryMethodValue<bool>("IsPrimaryKey");

    protected override Type ElementType => elementType ??= IPropertyTypeName.LoadTypeByName();
}