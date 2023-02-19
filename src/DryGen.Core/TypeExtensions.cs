using System;
using System.Linq;

namespace DryGen.Core
{
    public static class TypeExtensions
    {
        public static Type LoadTypeByName(this string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }
            throw new TypeLoadException($"Could not load type '{typeName}'");
        }
    }
}
