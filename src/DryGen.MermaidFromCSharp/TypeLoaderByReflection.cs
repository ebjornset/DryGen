using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DryGen.MermaidFromCSharp
{
    public class TypeLoaderByReflection : ITypeLoader
    {
        public IReadOnlyList<NamedType> Load(Assembly assembly, IReadOnlyList<ITypeFilter>? typeFilters, INameRewriter? nameRewriter)
        {
            var classLookup = new Dictionary<Type, Type>();
            var classesFromAssembly = assembly
                .GetTypes()
                .Where(type => typeFilters.All(filter => filter.Accepts(type)))
                .ToList();
            foreach (var type in classesFromAssembly)
            {
                AddClassHierarchy(typeFilters, classLookup, type);
            }
            return classLookup
                .Select(entry => new NamedType(nameRewriter?.Rewrite(entry.Value.Name) ?? entry.Value.Name, entry.Value))
                .OrderBy(nt => nt.Name)
                .ThenBy(nt => nt.Type.Name)
                .ThenBy(nt => nt.Type.Namespace).ToArray();

            static void AddClassHierarchy(IReadOnlyList<ITypeFilter>? typeFilters, Dictionary<Type, Type> classLookup, Type type)
            {
                if (type == null || classLookup.ContainsKey(type) || typeFilters?.Any(filter => !filter.Accepts(type)) == true)
                {
                    return;
                }
                classLookup.Add(type, type);
                foreach (var candidate in type.GetDirectInterfaces())
                {
                    AddClassHierarchy(typeFilters, classLookup, candidate);
                }
                AddClassHierarchy(typeFilters, classLookup, type.BaseType);
            }
        }
    }
}
