namespace DryGen.MermaidFromCSharp.ErDiagram
{
    public class ErDiagramAttribute
    {
        public ErDiagramAttribute(string attributeType, string attributeName, bool isNullable, bool isPrimaryKey, bool isAlternateKey = false, bool isForeignKey = false)
        {
            AttributeType = attributeType;
            AttributeName = attributeName;
            IsNullable = isNullable;
            IsPrimaryKey = isPrimaryKey;
            IsAlternateKey = isAlternateKey;
            IsForeignKey = isForeignKey;
        }

        public string AttributeType { get; }
        public string AttributeName { get; }
        public bool IsNullable { get; }
        public bool IsPrimaryKey { get; }
        public bool IsAlternateKey { get; }
        public bool IsForeignKey { get; }
    }
}
