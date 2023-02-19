using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DryGen.MermaidFromCSharp.ClassDiagram;

public class ClassDiagramMethod
{
    private readonly List<ClassDiagramMethodParameter> parameters;

    public ClassDiagramMethod(string returnType, string methodName, string visibility, bool isStatic, bool isAbstract, List<ClassDiagramMethodParameter> parameters, MethodInfo methodInfo)
    {
        ReturnType = returnType;
        MethodName = methodName;
        Visibility = visibility;
        IsStatic = isStatic;
        IsAbstract = isAbstract;
        this.parameters = parameters;
        MethodInfo = methodInfo;
    }

    public string ReturnType { get; }
    public string MethodName { get; }
    public string Visibility { get; }
    public bool IsStatic { get; private set; }
    public bool IsAbstract { get; }
    public IReadOnlyList<ClassDiagramMethodParameter> Parameters => parameters;
    public MethodInfo MethodInfo { get; }

    internal void ConvertToExtensionMethod()
    {
        parameters.Remove(parameters.First());
        IsStatic = false;
    }
}
