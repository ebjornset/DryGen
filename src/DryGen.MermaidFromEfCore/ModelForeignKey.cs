using DryGen.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DryGen.MermaidFromEfCore;

public class ModelForeignKey : ModelElement
{
    private static Type? elementType;

    public ModelForeignKey(object foreignKey) : base(foreignKey)
    {
        PrincipalEntityType = new(GetElementMandatoryPropertyValue<object>(nameof(PrincipalEntityType)));
        Properties = GetElementMandatoryPropertyValue<IReadOnlyList<object>>(nameof(Properties)).Select(x => new ModelProperty(x)).ToArray();
        IsRequired = GetElementMandatoryPropertyValue<bool>(nameof(IsRequired));
        var principalToDependent = GetElementOptionalPropertyValue<object>(nameof(PrincipalToDependent));
        PrincipalToDependent = principalToDependent == null ? null : new ModelNavigation(principalToDependent);
        var dependentToPrincipal = GetElementOptionalPropertyValue<object>(nameof(DependentToPrincipal));
        DependentToPrincipal = dependentToPrincipal == null ? null : new ModelNavigation(dependentToPrincipal);
    }

    public ModelEntityType PrincipalEntityType { get; }
    public IReadOnlyList<ModelProperty> Properties { get; }
    public ModelNavigation? PrincipalToDependent { get; }
    public ModelNavigation? DependentToPrincipal { get; }
    public bool IsRequired { get; }
    protected override Type ElementType => elementType ??= IForeignKeyTypeName.LoadTypeByName();
}
