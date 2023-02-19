namespace DryGen.MermaidFromCSharp.ClassDiagram;

public class ClassDiagramMethodParameter
{
    public ClassDiagramMethodParameter(string parameterType, string parameterName)
    {
        ParameterType = parameterType;
        ParameterName = parameterName;
    }

    public string ParameterType { get; }
    public string ParameterName { get; }
}
