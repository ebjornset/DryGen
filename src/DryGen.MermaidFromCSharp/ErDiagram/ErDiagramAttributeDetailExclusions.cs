using System;

namespace DryGen.MermaidFromCSharp.ErDiagram
{
    [Flags]
    public enum ErDiagramAttributeDetailExclusions
    {
        None = 0,
        KeyTypes = 1,
        Comments = 2,
        KeyTypesAndComments = 4,
    }
}
