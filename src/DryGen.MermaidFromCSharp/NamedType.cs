using System;
using System.Collections.Generic;
using System.Linq;

namespace DryGen.MermaidFromCSharp
{
    public class NamedType : IDiagramType
    {
        public NamedType(string name, Type type)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }
            Name = name;
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string Name { get; private set; }
        public Type Type { get; private set; }

        public bool IsRelatedToAny(IEnumerable<IDiagramType> types)
        {
            return types.Any(x => IsRelatedTo(x));
        }

        protected virtual bool IsRelatedTo(IDiagramType type)
        {
            return false;
        }
    }
}
