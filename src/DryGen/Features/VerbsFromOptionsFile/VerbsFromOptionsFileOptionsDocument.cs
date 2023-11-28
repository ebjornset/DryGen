using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace DryGen.Features.VerbsFromOptionsFile;

public class VerbsFromOptionsFileOptionsDocument
{
    [YamlMember(Alias = "configuration", ApplyNamingConventions = false)]
    public IVerbsFromOptionsFileConfiguration? Configuration { get; set; }
    public int? DocumentNumber { get; set; }
    public VerbsFromOptionsFileOptionsDocument? ParentOptionsDocument { get; set; }

    public IVerbsFromOptionsFileConfiguration GetConfiguration()
    {
        return Configuration.AsNonNull();
    }

    public IList<VerbsFromOptionsFileOptionsDocument> GetOptionsDocumentsPath()
    {
        var parentOptionsDocuments = new List<VerbsFromOptionsFileOptionsDocument>();
        AddToOptionsDocumentsPath(parentOptionsDocuments, this);
        return parentOptionsDocuments;
    }

    public void PerformInheritOptionsFrom()
    {
        if (ParentOptionsDocument == null || inheritOptionsFromPerformed)
        {
            return;
        }
        ParentOptionsDocument.PerformInheritOptionsFrom();
        GetConfiguration().PerformInheritOptionsFrom(ParentOptionsDocument.GetConfiguration());
        inheritOptionsFromPerformed = true;
    }

    private bool inheritOptionsFromPerformed;

    private static void AddToOptionsDocumentsPath(List<VerbsFromOptionsFileOptionsDocument> parentOptionsDocuments, VerbsFromOptionsFileOptionsDocument? optionsDocument)
    {
        if (optionsDocument != null)
        {
            parentOptionsDocuments.Add(optionsDocument);
            AddToOptionsDocumentsPath(parentOptionsDocuments, optionsDocument.ParentOptionsDocument);
        }
    }
}
