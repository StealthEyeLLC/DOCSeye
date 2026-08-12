using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace DOCSeye.Providers;

public sealed record OdfValidationResult(string Layer,bool Valid,int ExitCode,IReadOnlyList<string> Diagnostics,string? ValidatorSha256=null);
public sealed record OdfNormalizationItem(string Category,string Path,string Detail);
public sealed record OdfNormalizationReport(string LeftSha256,string RightSha256,bool ByteEqual,bool SemanticEquivalent,IReadOnlyList<OdfNormalizationItem> Items);

public static class OdfValidation
{
    public static OdfValidationResult ValidateContentWithJing(string odtPath,string schemaPath,string jingJar,string javaExe,string workDir)
    {
        Directory.CreateDirectory(workDir);string xml=Path.Combine(workDir,"content.xml");OdfPackage.ExtractEntry(odtPath,"content.xml",xml,OdfPackage.MaxXmlBytes);return ValidateXmlWithJing(xml,schemaPath,jingJar,javaExe,workDir,"independent-jing-content");
    }
    public static OdfValidationResult ValidateManifestWithJing(string odtPath,string schemaPath,string jingJar,string javaExe,string workDir)
    {
        Directory.CreateDirectory(workDir);string xml=Path.Combine(workDir,"manifest.xml");OdfPackage.ExtractEntry(odtPath,"META-INF/manifest.xml",xml,OdfPackage.MaxMetadataBytes);return ValidateXmlWithJing(xml,schemaPath,jingJar,javaExe,workDir,"independent-jing-manifest");
    }
    public static OdfValidationResult ValidateStylesWithJing(string odtPath,string schemaPath,string jingJar,string javaExe,string workDir)
    {
        Directory.CreateDirectory(workDir);string xml=Path.Combine(workDir,"styles.xml");OdfPackage.ExtractEntry(odtPath,"styles.xml",xml,OdfPackage.MaxXmlBytes);return ValidateXmlWithJing(xml,schemaPath,jingJar,javaExe,workDir,"independent-jing-styles");
    }
    public static OdfValidationResult ValidateMetaWithJing(string odtPath,string schemaPath,string jingJar,string javaExe,string workDir)
    {
        Directory.CreateDirectory(workDir);string xml=Path.Combine(workDir,"meta.xml");OdfPackage.ExtractEntry(odtPath,"meta.xml",xml,OdfPackage.MaxMetadataBytes);return ValidateXmlWithJing(xml,schemaPath,jingJar,javaExe,workDir,"independent-jing-meta");
    }
    public static OdfValidationResult ValidateSettingsWithJing(string odtPath,string schemaPath,string jingJar,string javaExe,string workDir)
    {
        Directory.CreateDirectory(workDir);string xml=Path.Combine(workDir,"settings.xml");OdfPackage.ExtractEntry(odtPath,"settings.xml",xml,OdfPackage.MaxMetadataBytes);return ValidateXmlWithJing(xml,schemaPath,jingJar,javaExe,workDir,"independent-jing-settings");
    }
    private static OdfValidationResult ValidateXmlWithJing(string xml,string schema,string jar,string java,string wd,string layer)
    {
        var psi=new ProcessStartInfo(java){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true,WorkingDirectory=wd};foreach(string a in new[]{"-jar",jar,schema,xml})psi.ArgumentList.Add(a);
        using var p=Process.Start(psi)??throw new InvalidOperationException("jing_start_failed");string o=p.StandardOutput.ReadToEnd()+"\n"+p.StandardError.ReadToEnd();p.WaitForExit();return new(layer,p.ExitCode==0,p.ExitCode,o.Split('\n',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries),OdfPackage.HashFile(jar));
    }

    public static OdfNormalizationReport Diff(string left,string right)
    {
        var items=new List<OdfNormalizationItem>();string lh=OdfPackage.HashFile(left),rh=OdfPackage.HashFile(right);var l=OdfPackage.ReadComparable(left);var r=OdfPackage.ReadComparable(right);
        foreach(string n in l.Keys.Union(r.Keys,StringComparer.Ordinal).OrderBy(x=>x,StringComparer.Ordinal))
        {
            if(!l.TryGetValue(n,out var lb)){items.Add(new("package_entry_added",n,"right only"));continue;}
            if(!r.TryGetValue(n,out var rb)){items.Add(new("package_entry_removed",n,"left only"));continue;}
            if(lb.SequenceEqual(rb))continue;
            if(n.EndsWith(".xml",StringComparison.OrdinalIgnoreCase))
            {
                string category=XmlSemanticEquivalent(lb,rb)?"xml_lexical_or_namespace_normalization":"semantic_or_unknown_xml_change";
                if(n=="meta.xml")category="provider_metadata_mutation";else if(n=="styles.xml")category="automatic_style_or_style_normalization";
                items.Add(new(category,n,$"{OdfPackage.Hash(lb)[..12]}->{OdfPackage.Hash(rb)[..12]}"));
            }
            else items.Add(new("package_byte_normalization",n,$"{lb.Length}->{rb.Length}"));
        }
        bool textEq=TextSignature(left)==TextSignature(right);
        bool semeq=textEq&&items.All(x=>x.Category is "xml_lexical_or_namespace_normalization" or "provider_metadata_mutation" or "automatic_style_or_style_normalization" or "package_byte_normalization" or "package_entry_added" or "package_entry_removed");
        if(semeq&&lh!=rh)items.Add(new("semantic_equivalent_rewrite","document","normalized package differs while authored text signature is equivalent"));
        if(!textEq)items.Add(new("semantic_loss","authored_text","paragraph/heading semantic text signature changed"));
        return new(lh,rh,lh==rh,semeq,items);
    }

    public static string TextSignature(string path)
    {
        var d=OdfPackage.ReadPackageXml(path,"content.xml",OdfPackage.MaxXmlBytes);return OdfPackage.Hash(OdfPackage.Utf8(string.Join("\n",d.Descendants().Where(x=>x.Name==OdfNamespaces.Text+"p"||x.Name==OdfNamespaces.Text+"h").Select(OdfPackage.ExtractText))));
    }
    private static bool XmlSemanticEquivalent(byte[] a,byte[] b)
    {
        try{using var ma=new MemoryStream(a);using var mb=new MemoryStream(b);var xa=OdfPackage.ReadSafeXml(ma,OdfPackage.MaxXmlBytes,out _,out _,out _);var xb=OdfPackage.ReadSafeXml(mb,OdfPackage.MaxXmlBytes,out _,out _,out _);return CanonicalElement(xa.Root)==CanonicalElement(xb.Root);}catch{return false;}
    }
    private static string CanonicalElement(XElement? e)
    {
        if(e is null)return "";var sb=new StringBuilder();void W(XElement x){sb.Append('{').Append(x.Name.NamespaceName).Append('}').Append(x.Name.LocalName);foreach(var a in x.Attributes().Where(a=>!a.IsNamespaceDeclaration).OrderBy(a=>a.Name.NamespaceName).ThenBy(a=>a.Name.LocalName))sb.Append(" @{").Append(a.Name.NamespaceName).Append('}').Append(a.Name.LocalName).Append('=').Append(a.Value);sb.Append('>');foreach(var n in x.Nodes()){if(n is XElement c)W(c);else if(n is XText t)sb.Append(t.Value);}sb.Append("</>");}W(e);return sb.ToString();
    }
}
