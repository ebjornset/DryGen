using System.Reflection;

namespace DryGen.MermaidFromCSharp
{
    public interface IPropertyFilter
    {
        bool Accepts(PropertyInfo property);
    }
}
