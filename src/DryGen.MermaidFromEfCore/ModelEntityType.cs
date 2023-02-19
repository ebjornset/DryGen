using DryGen.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DryGen.MermaidFromEfCore;

public class ModelEntityType : ModelElement
{
    private static Type? elementType;
    private IEnumerable<ModelProperty>? properties;
    private IEnumerable<ModelForeignKey>? foreignKeys;

    public ModelEntityType(object entityType) : base(entityType)
    {
        ClrType = GetElementMandatoryPropertyValue<Type>(nameof(ClrType));
    }

    public Type ClrType { get; }
    public IEnumerable<ModelProperty> GetProperties() => properties ??= GetElementMandatoryMethodValue<IEnumerable<object>>(nameof(GetProperties)).Select(x => new ModelProperty(x));
    public IEnumerable<ModelForeignKey> GetForeignKeys() => foreignKeys ??= GetElementMandatoryMethodValue<IEnumerable<object>>(nameof(GetForeignKeys)).Select(x => new ModelForeignKey(x));
    protected override Type ElementType => elementType ??= IEntityTypeTypeName.LoadTypeByName();
}
