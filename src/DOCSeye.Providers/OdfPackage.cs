using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DOCSeye.Providers;

public sealed record OdfEntryEvidence(string Name,long CompressedLength,long Length,string Sha256);
public sealed record OdfInspection(
    string Path,string Sha256,long Length,bool IsFlat,string MediaType,string Version,string Profile,string? Generator,
    IReadOnlyList<OdfEntryEvidence> Entries,IReadOnlyList<string> Diagnostics,IReadOnlyList<string> ExternalReferences,
    int ActiveContentMarkers,int UnknownNamespaceElements,int DuplicateProviderIds,bool PackageValid,bool XmlSafe)
{
    public bool Valid => PackageValid && XmlSafe;
}

public static class OdfNamespaces
{
    public static readonly XNamespace Office="urn:oasis:names:tc:opendocument:xmlns:office:1.0";
    public static readonly XNamespace Text="urn:oasis:names:tc:opendocument:xmlns:text:1.0";
    public static readonly XNamespace Table="urn:oasis:names:tc:opendocument:xmlns:table:1.0";
    public static readonly XNamespace Style="urn:oasis:names:tc:opendocument:xmlns:style:1.0";
    public static readonly XNamespace Draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0";
    public static readonly XNamespace XLink="http://www.w3.org/1999/xlink";
    public static readonly XNamespace Meta="urn:oasis:names:tc:opendocument:xmlns:meta:1.0";
    public static readonly XNamespace Dc="http://purl.org/dc/elements/1.1/";
    public static readonly XNamespace Xml="http://www.w3.org/XML/1998/namespace";
    public static readonly XNamespace Manifest="urn:oasis:names:tc:opendocument:xmlns:manifest:1.0";
    public static readonly XNamespace Fo="urn:oasis:names:tc:opendocument:xmlns:fo-compatible:1.0";
    public static readonly XNamespace Svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0";
    public static readonly XNamespace Number="urn:oasis:names:tc:opendocument:xmlns:number:1.0";
    public static readonly XNamespace Presentation="urn:oasis:names:tc:opendocument:xmlns:presentation:1.0";
    public static readonly XNamespace Math="http://www.w3.org/1998/Math/MathML";
    public static readonly HashSet<string> Known=new(StringComparer.Ordinal)
    {
        Office.NamespaceName,Text.NamespaceName,Table.NamespaceName,Style.NamespaceName,Draw.NamespaceName,XLink.NamespaceName,
        Meta.NamespaceName,Dc.NamespaceName,Xml.NamespaceName,Manifest.NamespaceName,Fo.NamespaceName,Svg.NamespaceName,
        Number.NamespaceName,Presentation.NamespaceName,Math.NamespaceName,
        "http://www.w3.org/2000/xmlns/","http://www.w3.org/2001/XMLSchema-instance"
    };
}

public static class OdfPackage
{
    public const string OdtMediaType="application/vnd.oasis.opendocument.text";
    public const long MaxXmlBytes=64L*1024*1024;
    public const long MaxMetadataBytes=16L*1024*1024;
    public const long MaxTotalExpandedBytes=8L*1024*1024*1024;
    public const int MaxEntries=50000;
    public const double MaxCompressionRatio=2000.0;
    public const int MaxXmlDepth=256;
    public const long MaxElements=5_000_000;
    public const long MaxAttributes=20_000_000;

    public static OdfInspection Inspect(string path)
    {
        path=Path.GetFullPath(path);
        if(!File.Exists(path))throw new FileNotFoundException(path);
        string digest=HashFile(path);long len=new FileInfo(path).Length;
        if(Path.GetExtension(path).Equals(".fodt",StringComparison.OrdinalIgnoreCase))return InspectFlat(path,digest,len);
        var diag=new List<string>();var external=new SortedSet<string>(StringComparer.Ordinal);var entries=new List<OdfEntryEvidence>();
        bool packageValid=true,xmlSafe=true;int active=0,unknown=0,duplicateIds=0;string version="unknown/malformed",profile="unknown/malformed";string? generator=null;
        try
        {
            VerifyFirstMimetypeHeader(path,diag);
            using var fs=File.OpenRead(path);using var zip=new ZipArchive(fs,ZipArchiveMode.Read,false,Encoding.UTF8);
            if(zip.Entries.Count==0||zip.Entries.Count>MaxEntries)throw new InvalidDataException("odf_entry_count_limit");
            var names=new HashSet<string>(StringComparer.Ordinal);long expanded=0;
            foreach(var e in zip.Entries)
            {
                ValidateEntryName(e.FullName);
                if(!names.Add(e.FullName))throw new InvalidDataException("duplicate_package_entry:"+e.FullName);
                expanded=checked(expanded+e.Length);if(expanded>MaxTotalExpandedBytes)throw new InvalidDataException("expanded_size_limit");
                if(e.CompressedLength>0&&e.Length/(double)e.CompressedLength>MaxCompressionRatio)throw new InvalidDataException("compression_ratio_limit:"+e.FullName);
                entries.Add(new(e.FullName,e.CompressedLength,e.Length,HashEntry(e)));
            }
            var mt=zip.GetEntry("mimetype")??throw new InvalidDataException("missing_mimetype");
            using(var r=new StreamReader(mt.Open(),Encoding.ASCII,false,1024,true)){string s=r.ReadToEnd();if(s!=OdtMediaType)throw new InvalidDataException("wrong_mimetype:"+s);}
            var manifest=zip.GetEntry("META-INF/manifest.xml")??throw new InvalidDataException("missing_manifest");
            _=ReadSafeXml(manifest,MaxMetadataBytes,out _,out _,out _);
            var content=zip.GetEntry("content.xml")??throw new InvalidDataException("missing_content_xml");
            var contentDoc=ReadSafeXml(content,MaxXmlBytes,out int a,out int u,out int dup);active+=a;unknown+=u;duplicateIds+=dup;
            version=(string?)contentDoc.Root?.Attribute(OdfNamespaces.Office+"version")??"unknown/malformed";
            profile=ClassifyProfile(version,contentDoc);CollectExternal(contentDoc,external);
            foreach(string n in new[]{"styles.xml","meta.xml","settings.xml"})if(zip.GetEntry(n) is ZipArchiveEntry xe)
            {
                var xd=ReadSafeXml(xe,n=="meta.xml"?MaxMetadataBytes:MaxXmlBytes,out a,out u,out dup);active+=a;unknown+=u;duplicateIds+=dup;CollectExternal(xd,external);
                if(n=="meta.xml")generator=xd.Descendants(OdfNamespaces.Meta+"generator").FirstOrDefault()?.Value;
            }
            foreach(var e in zip.Entries)if(e.FullName.StartsWith("Basic/",StringComparison.OrdinalIgnoreCase)||e.FullName.StartsWith("Scripts/",StringComparison.OrdinalIgnoreCase))active++;
        }
        catch(Exception ex){packageValid=false;xmlSafe=false;diag.Add(ex.Message);}
        return new(path,digest,len,false,OdtMediaType,NormalizeVersion(version),profile,generator,entries,diag,external.ToArray(),active,unknown,duplicateIds,packageValid,xmlSafe);
    }

    private static OdfInspection InspectFlat(string path,string digest,long len)
    {
        var diag=new List<string>();var external=new SortedSet<string>(StringComparer.Ordinal);bool safe=true;string v="unknown/malformed",p="unknown/malformed";string? generator=null;int active=0,unknown=0,dup=0;
        try{using var fs=File.OpenRead(path);var doc=ReadSafeXml(fs,MaxXmlBytes,out active,out unknown,out dup);v=(string?)doc.Root?.Attribute(OdfNamespaces.Office+"version")??"unknown/malformed";p=ClassifyProfile(v,doc);generator=doc.Descendants(OdfNamespaces.Meta+"generator").FirstOrDefault()?.Value;CollectExternal(doc,external);}catch(Exception ex){safe=false;diag.Add(ex.Message);}
        return new(path,digest,len,true,"application/vnd.oasis.opendocument.text-flat-xml",NormalizeVersion(v),p,generator,Array.Empty<OdfEntryEvidence>(),diag,external.ToArray(),active,unknown,dup,safe,safe);
    }

    public static XDocument ReadPackageXml(string path,string name,long limit)
    {
        using var ms=new MemoryStream(ReadEntryBytes(path,name,limit),false);return ReadSafeXml(ms,limit,out _,out _,out _);
    }
    public static XDocument? TryReadPackageXml(string path,string name,long limit){try{return ReadPackageXml(path,name,limit);}catch{return null;}}
    public static byte[] ReadEntryBytes(string path,string name,long limit)
    {
        using var fs=File.OpenRead(path);using var z=new ZipArchive(fs,ZipArchiveMode.Read,false,Encoding.UTF8);var e=z.GetEntry(name)??throw new InvalidDataException("missing_entry:"+name);if(e.Length>limit)throw new InvalidDataException("entry_size_limit:"+name);using var s=e.Open();using var ms=new MemoryStream();s.CopyTo(ms);return ms.ToArray();
    }
    public static void ExtractEntry(string package,string name,string output,long limit){File.WriteAllBytes(output,ReadEntryBytes(package,name,limit));}

    public static XDocument ReadSafeXml(Stream s,long limit,out int active,out int unknown,out int duplicateIds)
    {
        byte[] bytes;
        if(s is MemoryStream m&&m.TryGetBuffer(out var seg))bytes=seg.AsSpan(0,(int)m.Length).ToArray();else{using var ms=new MemoryStream();s.CopyTo(ms);if(ms.Length>limit)throw new InvalidDataException("xml_size_limit");bytes=ms.ToArray();}
        if(bytes.LongLength>limit)throw new InvalidDataException("xml_size_limit");
        var settings=new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=limit,MaxCharactersFromEntities=0,IgnoreComments=false};
        long elements=0,attrs=0;int maxDepth=0;var ids=new HashSet<string>(StringComparer.Ordinal);int dup=0,act=0,unk=0;
        using(var probe=new MemoryStream(bytes,false))using(var reader=XmlReader.Create(probe,settings))
        while(reader.Read())
        {
            if(reader.NodeType!=XmlNodeType.Element)continue;elements++;attrs+=reader.AttributeCount;maxDepth=Math.Max(maxDepth,reader.Depth);
            if(elements>MaxElements||attrs>MaxAttributes||maxDepth>MaxXmlDepth)throw new InvalidDataException("xml_complexity_limit");
            if(reader.NamespaceURI.Length>0&&!OdfNamespaces.Known.Contains(reader.NamespaceURI))unk++;
            string? xid=reader.GetAttribute("id",OdfNamespaces.Xml.NamespaceName);if(xid is not null&&!ids.Add(xid))dup++;
            if(reader.NamespaceURI==OdfNamespaces.Office.NamespaceName&&reader.LocalName=="scripts")act++;
            if(reader.LocalName.Contains("script",StringComparison.OrdinalIgnoreCase)&&reader.NamespaceURI.Contains("script",StringComparison.OrdinalIgnoreCase))act++;
        }
        using var input=new MemoryStream(bytes,false);using var r2=XmlReader.Create(input,settings);var doc=XDocument.Load(r2,LoadOptions.PreserveWhitespace|LoadOptions.SetLineInfo);active=act;unknown=unk;duplicateIds=dup;return doc;
    }
    private static XDocument ReadSafeXml(ZipArchiveEntry e,long limit,out int active,out int unknown,out int duplicateIds){if(e.Length>limit)throw new InvalidDataException("xml_size_limit:"+e.FullName);using var s=e.Open();return ReadSafeXml(s,limit,out active,out unknown,out duplicateIds);}

    public static void WritePackage(string path,IReadOnlyDictionary<string,(byte[] bytes,CompressionLevel level)> entries)
    {
        path=Path.GetFullPath(path);Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.Delete(path);using var fs=File.Create(path);using var zip=new ZipArchive(fs,ZipArchiveMode.Create,false,Encoding.UTF8);
        var mt=zip.CreateEntry("mimetype",CompressionLevel.NoCompression);mt.LastWriteTime=StableTime();using(var s=mt.Open()){byte[] b=Encoding.ASCII.GetBytes(OdtMediaType);s.Write(b);}
        foreach(var pair in entries.OrderBy(x=>x.Key,StringComparer.Ordinal)){ValidateEntryName(pair.Key);var e=zip.CreateEntry(pair.Key,pair.Value.level);e.LastWriteTime=StableTime();using var s=e.Open();s.Write(pair.Value.bytes);}
    }

    public static Dictionary<string,byte[]> ReadComparable(string path)
    {
        using var fs=File.OpenRead(path);using var z=new ZipArchive(fs,ZipArchiveMode.Read,false,Encoding.UTF8);return z.Entries.ToDictionary(e=>e.FullName,e=>{using var s=e.Open();using var ms=new MemoryStream();s.CopyTo(ms);return ms.ToArray();},StringComparer.Ordinal);
    }
    public static string NormalizeVersion(string v)=>v switch{"1.0"=>"1.0","1.1"=>"1.1","1.2"=>"1.2","1.3"=>"1.3","1.4"=>"1.4",_=>"unknown/malformed"};
    public static string ClassifyProfile(string version,XDocument doc){string v=NormalizeVersion(version);bool ext=doc.Descendants().Any(x=>x.Name.NamespaceName.Length>0&&!OdfNamespaces.Known.Contains(x.Name.NamespaceName));return v=="1.4"?(ext?"extended":"standard"):v;}
    public static void CollectExternal(XDocument d,ISet<string> target){foreach(var a in d.Descendants().Attributes(OdfNamespaces.XLink+"href")){string v=a.Value;if(Uri.TryCreate(v,UriKind.Absolute,out var u)&&u.Scheme is "http" or "https" or "ftp")target.Add(v);}}
    public static string HashFile(string p){using var s=File.OpenRead(p);return Convert.ToHexString(SHA256.HashData(s)).ToLowerInvariant();}
    public static string Hash(byte[] b)=>Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant();
    public static byte[] Utf8(string s)=>new UTF8Encoding(false).GetBytes(s);
    public static DateTimeOffset StableTime()=>new(2020,1,1,0,0,0,TimeSpan.Zero);
    public static int ParseInt(XAttribute? a,int fallback)=>a is not null&&int.TryParse(a.Value,out int n)?n:fallback;
    public static string ExtractText(XElement e)
    {
        var sb=new StringBuilder();foreach(var n in e.DescendantNodesAndSelf()){if(n is XText t)sb.Append(t.Value);else if(n is XElement x&&x.Name==OdfNamespaces.Text+"s")sb.Append(' ',Math.Max(1,ParseInt(x.Attribute(OdfNamespaces.Text+"c"),1)));else if(n is XElement tab&&tab.Name==OdfNamespaces.Text+"tab")sb.Append('\t');else if(n is XElement br&&br.Name==OdfNamespaces.Text+"line-break")sb.Append('\n');}return sb.ToString();
    }
    public static string SerializeXml(XDocument d){using var sw=new Utf8StringWriter();d.Save(sw,SaveOptions.DisableFormatting);return sw.ToString();}
    public sealed class Utf8StringWriter:StringWriter{public override Encoding Encoding=>new UTF8Encoding(false);}

    public static void ValidateEntryName(string name)
    {
        if(string.IsNullOrEmpty(name)||name.StartsWith('/')||name.StartsWith('\\')||name.Contains('\\')||name.Contains(':'))throw new InvalidDataException("unsafe_entry_name:"+name);
        foreach(string s in name.Split('/'))if(s==".."||s==".")throw new InvalidDataException("path_traversal_entry:"+name);
    }
    private static void VerifyFirstMimetypeHeader(string path,List<string> diag)
    {
        using var fs=File.OpenRead(path);byte[] h=new byte[30];if(fs.Read(h,0,h.Length)!=30||BitConverter.ToUInt32(h,0)!=0x04034b50)throw new InvalidDataException("invalid_zip_local_header");ushort method=BitConverter.ToUInt16(h,8),nameLen=BitConverter.ToUInt16(h,26),extraLen=BitConverter.ToUInt16(h,28);byte[] name=new byte[nameLen];if(fs.Read(name,0,name.Length)!=name.Length)throw new InvalidDataException("truncated_first_entry");string n=Encoding.UTF8.GetString(name);if(n!="mimetype"||method!=0||extraLen!=0)throw new InvalidDataException($"mimetype_header_invalid:name={n};method={method};extra={extraLen}");diag.Add("mimetype_first_uncompressed_no_extra");
    }
    private static string HashEntry(ZipArchiveEntry e){using var s=e.Open();using var h=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);byte[] b=new byte[1024*1024];int n;while((n=s.Read(b,0,b.Length))>0)h.AppendData(b,0,n);return Convert.ToHexString(h.GetHashAndReset()).ToLowerInvariant();}
}
