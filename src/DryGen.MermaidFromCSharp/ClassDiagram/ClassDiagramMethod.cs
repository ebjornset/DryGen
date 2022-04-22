using System.Collections.Generic;

namespace DryGen.MermaidFromCSharp.ClassDiagram
{
    public class ClassDiagramMethod
    {
        public ClassDiagramMethod(string returnType, string methodName, string visibility, bool isStatic, bool isAbstract, IReadOnlyList<ClassDiagramMethodParameter> parameters)
        {
            ReturnType = returnType;
            MethodName = methodName;
            Visibility = visibility;
            IsStatic = isStatic;
            IsAbstract = isAbstract;
            Parameters = parameters;
        }

        public string ReturnType { get; }
        public string MethodName { get; }
        public string Visibility { get; }
        public bool IsStatic { get; }
        public bool IsAbstract { get; }
        public IReadOnlyList<ClassDiagramMethodParameter> Parameters { get; }
    }
}
