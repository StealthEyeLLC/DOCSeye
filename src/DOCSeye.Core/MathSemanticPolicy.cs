using System.Xml;
using System.Xml.Linq;

namespace DOCSeye.Core;

public static class MathSemanticPolicy
{
    public const string PresentationMathMlNamespace="http://www.w3.org/1998/Math/MathML";
    public const int MaxMathMlCharacters=4*1024*1024;

    public static string CanonicalizePresentationMathMl(string input)
    {
        if(string.IsNullOrWhiteSpace(input))throw new SemanticRefusalException("invalid_mathml","MathML required");
        var settings=new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=MaxMathMlCharacters,MaxCharactersFromEntities=0,IgnoreComments=true,IgnoreProcessingInstructions=true};
        XDocument doc;try{using var sr=new StringReader(input);using var reader=XmlReader.Create(sr,settings);doc=XDocument.Load(reader,LoadOptions.PreserveWhitespace);}catch(XmlException ex){throw new SemanticRefusalException("invalid_mathml",ex.Message);}
        if(doc.Root is null||doc.Root.Name.LocalName!="math"||doc.Root.Name.NamespaceName!=PresentationMathMlNamespace)throw new SemanticRefusalException("invalid_mathml","root must be Presentation MathML math element");
        CanonicalizeElement(doc.Root);return doc.Root.ToString(SaveOptions.DisableFormatting);
    }

    private static void CanonicalizeElement(XElement element)
    {
        foreach(var child in element.Elements())CanonicalizeElement(child);
        foreach(var node in element.Nodes().OfType<XText>().Where(x=>string.IsNullOrWhiteSpace(x.Value)&&element.Name.LocalName!="mtext").ToArray())node.Remove();
        var attributes=element.Attributes().Where(a=>!a.IsNamespaceDeclaration).OrderBy(a=>a.Name.NamespaceName,StringComparer.Ordinal).ThenBy(a=>a.Name.LocalName,StringComparer.Ordinal).Select(a=>new XAttribute(a.Name,a.Value)).ToArray();
        var namespaceAttributes=element.Attributes().Where(a=>a.IsNamespaceDeclaration).OrderBy(a=>a.Name.LocalName,StringComparer.Ordinal).Select(a=>new XAttribute(a)).ToArray();element.RemoveAttributes();foreach(var a in namespaceAttributes)element.Add(a);foreach(var a in attributes)element.Add(a);
    }
}
