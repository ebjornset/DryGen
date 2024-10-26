using DryGen.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DryGen.MermaidFromEfCore;

public class ModelRuntimeKey : ModelElement
{
	private static Type? elementType;

	public ModelRuntimeKey(object foreignKey) : base(foreignKey)
	{
		Properties = GetElementMandatoryPropertyValue<IReadOnlyList<object>>(nameof(Properties)).Select(x => new ModelProperty(x)).ToArray();
	}

	public IReadOnlyList<ModelProperty> Properties { get; }
	protected override Type ElementType => elementType ??= RuntimeKeyTypeName.LoadTypeByName();
}