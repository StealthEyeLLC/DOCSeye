using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using DOCSeye.Core;

namespace DOCSeye.Providers;

public sealed record OdfFeatureClassification(string Feature,string Classification,string Detail);
public sealed record OdfImportResult(SemanticState State,OdfInspection Inspection,IReadOnlyList<OdfFeatureClassification> Features,IReadOnlyDictionary<string,Guid> ProviderCorrespondence);
public sealed record OdfExportPlan(ExportOutcome Outcome,string Classification,IReadOnlyList<string> DeclaredLosses);
public sealed record OdfExportResult(string Path,string Sha256,ExportOutcome Outcome,string Classification,Guid SemanticRevisionId,string SemanticRoot,IReadOnlyList<string> DeclaredLosses);

public static class OdfSemanticInterop
{
    private static XNamespace Office=>OdfNamespaces.Office;
    private static XNamespace Text=>OdfNamespaces.Text;
    private static XNamespace Table=>OdfNamespaces.Table;
    private static XNamespace Style=>OdfNamespaces.Style;
    private static XNamespace Draw=>OdfNamespaces.Draw;
    private static XNamespace Meta=>OdfNamespaces.Meta;
    private static XNamespace Dc=>OdfNamespaces.Dc;
    private static XNamespace Xml=>OdfNamespaces.Xml;
    private static XNamespace Svg=>OdfNamespaces.Svg;

    public static OdfImportResult Import(string path)
    {
        var inspection=OdfPackage.Inspect(path);
        if(!inspection.Valid)throw new InvalidDataException("odf_preflight_failed:"+string.Join("|",inspection.Diagnostics));
        if(inspection.IsFlat)throw new InvalidDataException("fodt_import_debug_only");
        byte[] source=File.ReadAllBytes(path);
        XDocument content=OdfPackage.ReadPackageXml(path,"content.xml",OdfPackage.MaxXmlBytes);
        XDocument? styles=OdfPackage.TryReadPackageXml(path,"styles.xml",OdfPackage.MaxXmlBytes);
        var state=new SemanticState{FamilyId=Ids.NewV4(),BranchId=Ids.NewV4(),RevisionId=Ids.NewV4(),Sequence=0,Mode=AuthorityMode.ConvertedImported};
        var features=new List<OdfFeatureClassification>();var corr=new Dictionary<string,Guid>(StringComparer.Ordinal);
        Guid document=Ids.NewV4();state.Objects[document]=new(document,"document",null,OrderKey.Initial(0),"document",new(StringComparer.Ordinal)
        {
            ["source_format"]="odt",["odf_version"]=inspection.Version,["odf_profile"]=inspection.Profile,["generator"]=inspection.Generator??""
        });
        var body=content.Descendants(Office+"text").FirstOrDefault()??throw new InvalidDataException("missing_office_text");
        int childOrder=0;foreach(var e in body.Elements())ImportElement(e,document,state,features,corr,ref childOrder);
        if(styles is not null)ImportStyles(styles,document,state,features,ref childOrder);
        byte[] sd=SHA256.HashData(source);state.SourceCapsules[Convert.ToHexString(sd)]=new("odf",source,sd,$"source:{inspection.Version}:{inspection.Profile}");
        byte[] providerPayload=OdfPackage.Utf8(JsonSerializer.Serialize(new{inspection.Sha256,inspection.Version,inspection.Profile,inspection.Generator,inspection.Entries}));
        Guid packageFacet=Ids.NewV4();state.ProviderFacets[packageFacet]=new(packageFacet,"odf","source-package",document,ExtensionCoverageKind.DocumentGlobal,ExtensionEditPolicy.Independent,providerPayload,SHA256.HashData(providerPayload),"source-evidence",false);
        if(inspection.UnknownNamespaceElements>0)features.Add(new("unknown_namespaces","unknown_preserved",$"elements={inspection.UnknownNamespaceElements}"));
        if(inspection.ActiveContentMarkers>0)features.Add(new("active_content","provider_facet",$"inert_markers={inspection.ActiveContentMarkers}"));
        if(inspection.ExternalReferences.Count>0)features.Add(new("external_references","provider_facet",$"inert_refs={inspection.ExternalReferences.Count}"));
        if(inspection.DuplicateProviderIds>0)features.Add(new("duplicate_provider_ids","provider_facet",$"duplicates={inspection.DuplicateProviderIds}; never writable native identity"));
        state.RequiredCapabilities.Add("odf-provider-facets-v1");
        return new(state,inspection,features,corr);
    }

    private static void ImportElement(XElement e,Guid parent,SemanticState state,List<OdfFeatureClassification> features,Dictionary<string,Guid> corr,ref int order)
    {
        if(e.Name==Text+"tracked-changes")
        {
            foreach(var cr in e.Descendants(Text+"changed-region"))
            {
                Guid id=Ids.NewV4();string kind=cr.Descendants(Text+"insertion").Any()?"tracked_insertion":cr.Descendants(Text+"deletion").Any()?"tracked_deletion":"tracked_format_change";
                state.Objects[id]=new(id,"suggestion",parent,OrderKey.Initial(order++),"review",new(StringComparer.Ordinal){{"kind",kind},{"provider_change_id",(string?)cr.Attribute(Text+"id")??""},{"text",cr.Value}});
                AddProviderIdFacet(state,id,"odf-change-id",(string?)cr.Attribute(Text+"id"),corr);
                features.Add(new(kind,"native_mapped",kind=="tracked_format_change"?"bounded provider fidelity; no invented complete property delta":"mapped review suggestion"));
            }
            return;
        }
        if(e.Name==Text+"section")
        {
            Guid id=AddObject(state,"section",parent,order++,"section",new(){{"name",(string?)e.Attribute(Text+"name")??""},{"style_name",(string?)e.Attribute(Text+"style-name")??""}});
            AddProviderIdFacet(state,id,"xml-id",(string?)e.Attribute(Xml+"id"),corr);int n=0;foreach(var c in e.Elements())ImportElement(c,id,state,features,corr,ref n);features.Add(new("section","native_mapped","authored section; provider lifecycle not native identity"));return;
        }
        if(e.Name==Text+"h"||e.Name==Text+"p")
        {
            string type=e.Name==Text+"h"?"heading":"paragraph";
            var data=new Dictionary<string,object?>(StringComparer.Ordinal){{"text",OdfPackage.ExtractText(e)},{"style_name",(string?)e.Attribute(Text+"style-name")??""}};
            if(type=="heading")data["level"]=int.TryParse((string?)e.Attribute(Text+"outline-level"),out int l)?l:1;
            Guid id=AddObject(state,type,parent,order++,type,data);AddProviderIdFacet(state,id,"xml-id",(string?)e.Attribute(Xml+"id"),corr);ImportInline(e,id,state,features,corr);features.Add(new(type,"native_mapped",type));return;
        }
        if(e.Name==Text+"list")
        {
            Guid id=AddObject(state,"list",parent,order++,"list",new(){{"style_name",(string?)e.Attribute(Text+"style-name")??""},{"continue_numbering",(string?)e.Attribute(Text+"continue-numbering")??"false"},{"continue_list",(string?)e.Attribute(Text+"continue-list")??""}});
            int n=0;foreach(var item in e.Elements(Text+"list-item")){Guid li=AddObject(state,"list_item",id,n++,"list-item",new());int m=0;foreach(var c in item.Elements())ImportElement(c,li,state,features,corr,ref m);}
            features.Add(new("list","native_mapped","nested membership and restart/continuation metadata"));return;
        }
        if(e.Name==Table+"table")
        {
            Guid table=AddObject(state,"table",parent,order++,"table",new(){{"name",(string?)e.Attribute(Table+"name")??""},{"style_name",(string?)e.Attribute(Table+"style-name")??""}});int r=0;
            foreach(var row in e.Elements(Table+"table-row"))
            {
                Guid rr=AddObject(state,"table_row",table,r++,"row",new());int cidx=0;
                foreach(var cell in row.Elements().Where(x=>x.Name==Table+"table-cell"||x.Name==Table+"covered-table-cell"))
                {
                    var cd=new Dictionary<string,object?>(StringComparer.Ordinal){{"col_span",OdfPackage.ParseInt(cell.Attribute(Table+"number-columns-spanned"),1)},{"row_span",OdfPackage.ParseInt(cell.Attribute(Table+"number-rows-spanned"),1)},{"formula",(string?)cell.Attribute(Table+"formula")??""},{"header",row.Ancestors(Table+"table-header-rows").Any()}};
                    Guid cc=AddObject(state,"table_cell",rr,cidx++,"cell",cd);int pc=0;foreach(var p in cell.Elements())ImportElement(p,cc,state,features,corr,ref pc);
                }
            }
            features.Add(new("table","native_mapped","authored-document table semantics; formulas provider-qualified"));return;
        }
        if(e.Name==Text+"table-of-content"||e.Name==Text+"alphabetical-index")
        {
            Guid id=AddObject(state,"field",parent,order++,"generated-field",new(){{"provider_dynamic",true},{"kind",e.Name.LocalName}});features.Add(new("dynamic_index","provider_facet","provider-evaluated generated state"));AddProviderIdFacet(state,id,"generated-provider-field",e.Name.LocalName,corr);return;
        }
        if(e.Name.Namespace==Draw)
        {
            string alt=string.Join(" ",e.Descendants(Svg+"desc").Select(x=>x.Value));Guid id=AddObject(state,"figure",parent,order++,"figure",new(){{"kind",e.Name.LocalName},{"name",(string?)e.Attribute(Draw+"name")??""},{"alt",alt},{"href",e.Descendants(Draw+"image").Attributes(OdfNamespaces.XLink+"href").FirstOrDefault()?.Value??""}});
            features.Add(new("figure","native_mapped","document figure occurrence only; no Draw world"));AddProviderIdFacet(state,id,"draw-object",(string?)e.Attribute(Draw+"name"),corr);return;
        }
        if(e.Name.Namespace==Text&&(e.Name==Text+"footnote"||e.Name==Text+"note"))
        {
            Guid id=AddObject(state,"note",parent,order++,"note",new(){{"class",(string?)e.Attribute(Text+"note-class")??"footnote"},{"text",e.Value}});features.Add(new("note","native_mapped","footnote/endnote"));AddProviderIdFacet(state,id,"note-id",(string?)e.Attribute(Text+"id"),corr);return;
        }
        if(e.Name.NamespaceName.Length>0&&!OdfNamespaces.Known.Contains(e.Name.NamespaceName)){PreserveUnknown(e,parent,state,features,required:e.Name.LocalName.Contains("required",StringComparison.OrdinalIgnoreCase));return;}
        foreach(var c in e.Elements())ImportElement(c,parent,state,features,corr,ref order);
    }

    private static void ImportInline(XElement e,Guid owner,SemanticState state,List<OdfFeatureClassification> features,Dictionary<string,Guid> corr)
    {
        foreach(var b in e.Descendants().Where(x=>x.Name==Text+"bookmark"||x.Name==Text+"bookmark-start"||x.Name==Text+"reference-mark"||x.Name==Text+"reference-mark-start"))
        {
            string kind=b.Name.LocalName.Contains("bookmark",StringComparison.Ordinal)?"named_anchor":"reference_mark";Guid id=AddObject(state,kind,owner,state.Objects.Values.Count(x=>x.ParentId==owner),kind,new(){{"name",(string?)b.Attribute(Text+"name")??""},{"provider_range",b.Name.LocalName.EndsWith("start",StringComparison.Ordinal)}});AddProviderIdFacet(state,id,b.Name.LocalName,(string?)b.Attribute(Text+"name"),corr);features.Add(new(kind,"native_mapped","provider correspondence only; name not native identity"));
        }
        foreach(var a in e.Descendants(Office+"annotation"))
        {
            Guid id=AddObject(state,"comment",owner,state.Objects.Values.Count(x=>x.ParentId==owner),"comment",new(){{"text",a.Value},{"author",a.Descendants(Dc+"creator").FirstOrDefault()?.Value??""},{"provider_name",(string?)a.Attribute(Office+"name")??""},{"resolved",a.Attributes().Any(x=>x.Name.LocalName.Contains("resolved",StringComparison.OrdinalIgnoreCase)&&x.Value=="true")}});AddProviderIdFacet(state,id,"annotation-name",(string?)a.Attribute(Office+"name"),corr);features.Add(new("comment","native_mapped","point/range anchoring pressure; provider name evidence only"));
        }
        foreach(var f in e.Descendants().Where(x=>x.Name.Namespace==Text&&(x.Name.LocalName.Contains("page-number")||x.Name.LocalName.Contains("date")||x.Name.LocalName.Contains("sequence")||x.Name.LocalName.Contains("reference"))))features.Add(new("field","native_mapped","approved bounded field class:"+f.Name.LocalName));
        foreach(var x in e.Descendants().Where(x=>x.Name.NamespaceName.Length>0&&!OdfNamespaces.Known.Contains(x.Name.NamespaceName)))PreserveUnknown(x,owner,state,features,x.Name.LocalName.Contains("required",StringComparison.OrdinalIgnoreCase));
    }

    private static void PreserveUnknown(XElement e,Guid target,SemanticState state,List<OdfFeatureClassification> features,bool required)
    {
        byte[] payload=OdfPackage.Utf8(e.ToString(SaveOptions.DisableFormatting));Guid id=Ids.NewV4();state.Extensions[id]=new(id,e.Name.NamespaceName,e.Name.LocalName,1,0,"xml",payload,SHA256.HashData(payload),ExtensionCoverageKind.Subtree,target,null,null,null,required?ExtensionEditPolicy.MustUnderstandBeforeEdit:ExtensionEditPolicy.MoveWithTarget,required,required?null:"preserved opaque ODF extension");features.Add(new(e.Name.ToString(),required?"required_unsupported":"unknown_preserved",required?"must block intersecting unsafe edit":"preserved inertly"));
    }

    private static void ImportStyles(XDocument styles,Guid document,SemanticState state,List<OdfFeatureClassification> features,ref int order)
    {
        foreach(var s in styles.Descendants(Style+"style"))
        {
            string family=(string?)s.Attribute(Style+"family")??"",name=(string?)s.Attribute(Style+"name")??"";bool automatic=s.Ancestors(Office+"automatic-styles").Any();
            if(automatic){features.Add(new("automatic_style","provider_facet",name));AddProviderPayloadFacet(state,document,"odf","automatic-style",s.ToString(SaveOptions.DisableFormatting),false);continue;}
            Guid id=AddObject(state,"style",document,order++,"style",new(){{"name",name},{"family",family},{"parent_style",(string?)s.Attribute(Style+"parent-style-name")??""}});AddProviderPayloadFacet(state,id,"odf","style-xml",s.ToString(SaveOptions.DisableFormatting),false);features.Add(new("named_style","native_mapped",name));
        }
        foreach(var page in styles.Descendants(Style+"master-page")){AddProviderPayloadFacet(state,document,"odf","master-page",page.ToString(SaveOptions.DisableFormatting),false);features.Add(new("page_master_style","provider_facet","translated into existing layout intent only"));}
    }

    public static OdfExportPlan Plan(SemanticState state)
    {
        var losses=new SortedSet<string>(StringComparer.Ordinal);
        if(state.Objects.Values.Any(o=>!o.Retired&&o.Type=="suggestion"&&Equals(o.Data.GetValueOrDefault("kind"),"move")))losses.Add("native move suggestion translated as delete+insert composition");
        if(state.Objects.Values.Any(o=>!o.Retired&&o.Type=="field"&&Equals(o.Data.GetValueOrDefault("provider_dynamic"),true)))losses.Add("provider-dynamic field exported as static/qualified representation");
        if(state.Extensions.Values.Any(e=>e.Required))return new(ExportOutcome.Blocked,"blocked",["required unknown provider meaning must be understood before general translation"]);
        return losses.Count==0?new(ExportOutcome.TranslatedConformant,"translated_conformant",[]):new(ExportOutcome.TranslatedWithDeclaredLoss,"translated_with_declared_loss",losses.ToArray());
    }

    public static OdfExportResult Export(SemanticState state,RevisionHead head,string path,string? exactSourceSha256=null)
    {
        var plan=Plan(state);if(plan.Outcome==ExportOutcome.Blocked)throw new SemanticRefusalException("blocked_required_extension",string.Join(";",plan.DeclaredLosses));
        if(exactSourceSha256 is not null&&state.SourceCapsules.TryGetValue(exactSourceSha256.ToUpperInvariant(),out var cap))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);File.WriteAllBytes(path,cap.ExactBytes);return new(path,OdfPackage.HashFile(path),ExportOutcome.ExactSourceReuse,"exact_source_reuse",head.RevisionId,Convert.ToHexString(head.SemanticRoot).ToLowerInvariant(),[]);
        }
        var resources=new SortedDictionary<string,(byte[] bytes,CompressionLevel level)>(StringComparer.Ordinal)
        {
            ["META-INF/manifest.xml"]=(OdfPackage.Utf8(BuildManifestXml()),CompressionLevel.Optimal),
            ["content.xml"]=(OdfPackage.Utf8(BuildContentXml(state)),CompressionLevel.Optimal),
            ["meta.xml"]=(OdfPackage.Utf8(BuildMetaXml(head)),CompressionLevel.Optimal),
            ["settings.xml"]=(OdfPackage.Utf8(BuildSettingsXml()),CompressionLevel.Optimal),
            ["styles.xml"]=(OdfPackage.Utf8(BuildStylesXml(state)),CompressionLevel.Optimal)
        };
        OdfPackage.WritePackage(path,resources);return new(path,OdfPackage.HashFile(path),plan.Outcome,plan.Classification,head.RevisionId,Convert.ToHexString(head.SemanticRoot).ToLowerInvariant(),plan.DeclaredLosses);
    }

    public static OdfExportResult ExactSourceReuse(SemanticState state,RevisionHead head,string sourceSha256,string path)
    {
        if(!state.SourceCapsules.TryGetValue(sourceSha256.ToUpperInvariant(),out var cap))throw new InvalidOperationException("source_capsule_not_found");Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);File.WriteAllBytes(path,cap.ExactBytes);return new(path,OdfPackage.HashFile(path),ExportOutcome.ExactSourceReuse,"exact_source_reuse",head.RevisionId,Convert.ToHexString(head.SemanticRoot).ToLowerInvariant(),[]);
    }

    private static string BuildContentXml(SemanticState s)
    {
        var body=new XElement(Office+"text");var root=s.Objects.Values.FirstOrDefault(o=>!o.Retired&&o.Type=="document"&&o.ParentId is null);
        if(root is not null)foreach(var o in DescendantsInOrder(s,root.Id))body.Add(RenderObject(s,o));
        var doc=new XDocument(new XDeclaration("1.0","UTF-8",null),new XElement(Office+"document-content",NsAttrs(),new XAttribute(Office+"version","1.4"),new XElement(Office+"body",body)));return OdfPackage.SerializeXml(doc);
    }

    private static IEnumerable<SemanticObject> DescendantsInOrder(SemanticState s,Guid parent)=>s.Objects.Values.Where(o=>!o.Retired&&o.ParentId==parent).OrderBy(o=>o.Order).ThenBy(o=>Ids.Lower(o.Id),StringComparer.Ordinal);
    private static XElement RenderObject(SemanticState s,SemanticObject o)
    {
        if(o.Type=="heading")return new XElement(Text+"h",new XAttribute(Text+"outline-level",Convert.ToInt32(o.Data.GetValueOrDefault("level")??1)),o.Data.GetValueOrDefault("text")?.ToString()??"");
        if(o.Type=="paragraph")return new XElement(Text+"p",o.Data.GetValueOrDefault("text")?.ToString()??"");
        if(o.Type=="section")return new XElement(Text+"section",new XAttribute(Text+"name",o.Data.GetValueOrDefault("name")?.ToString()??("section-"+Ids.Lower(o.Id)[..8])),DescendantsInOrder(s,o.Id).Select(c=>RenderObject(s,c)));
        if(o.Type=="list")return new XElement(Text+"list",DescendantsInOrder(s,o.Id).Where(x=>x.Type=="list_item").Select(li=>new XElement(Text+"list-item",DescendantsInOrder(s,li.Id).Select(c=>RenderObject(s,c)))));
        if(o.Type=="table")return new XElement(Table+"table",new XAttribute(Table+"name",o.Data.GetValueOrDefault("name")?.ToString()??("table-"+Ids.Lower(o.Id)[..8])),DescendantsInOrder(s,o.Id).Where(x=>x.Type=="table_row").Select(row=>new XElement(Table+"table-row",DescendantsInOrder(s,row.Id).Where(x=>x.Type=="table_cell").Select(cell=>new XElement(Table+"table-cell",new XAttribute(Office+"value-type","string"),DescendantsInOrder(s,cell.Id).Select(c=>RenderObject(s,c)))))));
        if(o.Type=="comment")return new XElement(Office+"annotation",new XAttribute(Office+"name",Ids.Lower(o.Id)),new XElement(Dc+"creator",o.Data.GetValueOrDefault("author")?.ToString()??""),new XElement(Text+"p",o.Data.GetValueOrDefault("text")?.ToString()??""));
        if(o.Type=="note")return new XElement(Text+"note",new XAttribute(Text+"note-class",o.Data.GetValueOrDefault("class")?.ToString()??"footnote"),new XElement(Text+"note-citation","1"),new XElement(Text+"note-body",new XElement(Text+"p",o.Data.GetValueOrDefault("text")?.ToString()??"")));
        if(o.Type=="figure")return new XElement(Draw+"frame",new XAttribute(Draw+"name",o.Data.GetValueOrDefault("name")?.ToString()??("figure-"+Ids.Lower(o.Id)[..8])),new XElement(Svg+"desc",o.Data.GetValueOrDefault("alt")?.ToString()??""));
        if(o.Type=="suggestion")return new XElement(Text+"p",$"[review:{o.Data.GetValueOrDefault("kind")}] {o.Data.GetValueOrDefault("text")}");
        if(o.Type=="field")return new XElement(Text+"p",$"[field:{o.Data.GetValueOrDefault("kind")}]" );
        return new XElement(Text+"p",DescendantsInOrder(s,o.Id).Select(c=>RenderObject(s,c)));
    }

    private static string BuildStylesXml(SemanticState s)
    {
        var named=new XElement(Office+"styles");foreach(var o in s.Objects.Values.Where(o=>!o.Retired&&o.Type=="style"))named.Add(new XElement(Style+"style",new XAttribute(Style+"name",o.Data.GetValueOrDefault("name")?.ToString()??("S"+Ids.Lower(o.Id)[..8])),new XAttribute(Style+"family",o.Data.GetValueOrDefault("family")?.ToString()??"paragraph")));
        return OdfPackage.SerializeXml(new XDocument(new XDeclaration("1.0","UTF-8",null),new XElement(Office+"document-styles",NsAttrs(),new XAttribute(Office+"version","1.4"),named,new XElement(Office+"automatic-styles"),new XElement(Office+"master-styles"))));
    }
    private static string BuildMetaXml(RevisionHead h)=>OdfPackage.SerializeXml(new XDocument(new XDeclaration("1.0","UTF-8",null),new XElement(Office+"document-meta",NsAttrs(),new XAttribute(Office+"version","1.4"),new XElement(Office+"meta",new XElement(Meta+"generator","DOCSeye Build 002"),new XElement(Meta+"user-defined",new XAttribute(Meta+"name","DND-Revision"),Ids.Lower(h.RevisionId)),new XElement(Meta+"user-defined",new XAttribute(Meta+"name","DND-Semantic-Root"),Convert.ToHexString(h.SemanticRoot).ToLowerInvariant())))));
    private static string BuildSettingsXml()=>OdfPackage.SerializeXml(new XDocument(new XDeclaration("1.0","UTF-8",null),new XElement(Office+"document-settings",NsAttrs(),new XAttribute(Office+"version","1.4"),new XElement(Office+"settings"))));
    private static string BuildManifestXml()=>OdfPackage.SerializeXml(new XDocument(new XDeclaration("1.0","UTF-8",null),new XElement(OdfNamespaces.Manifest+"manifest",new XAttribute(XNamespace.Xmlns+"manifest",OdfNamespaces.Manifest.NamespaceName),new XAttribute(OdfNamespaces.Manifest+"version","1.4"),new XElement(OdfNamespaces.Manifest+"file-entry",new XAttribute(OdfNamespaces.Manifest+"full-path","/"),new XAttribute(OdfNamespaces.Manifest+"media-type",OdfPackage.OdtMediaType),new XAttribute(OdfNamespaces.Manifest+"version","1.4")),new[]{"content.xml","styles.xml","meta.xml","settings.xml"}.Select(n=>new XElement(OdfNamespaces.Manifest+"file-entry",new XAttribute(OdfNamespaces.Manifest+"full-path",n),new XAttribute(OdfNamespaces.Manifest+"media-type","text/xml"))))));
    private static object[] NsAttrs()=>new object[]{new XAttribute(XNamespace.Xmlns+"office",Office.NamespaceName),new XAttribute(XNamespace.Xmlns+"text",Text.NamespaceName),new XAttribute(XNamespace.Xmlns+"table",Table.NamespaceName),new XAttribute(XNamespace.Xmlns+"style",Style.NamespaceName),new XAttribute(XNamespace.Xmlns+"draw",Draw.NamespaceName),new XAttribute(XNamespace.Xmlns+"meta",Meta.NamespaceName),new XAttribute(XNamespace.Xmlns+"dc",Dc.NamespaceName),new XAttribute(XNamespace.Xmlns+"svg",Svg.NamespaceName)};

    private static Guid AddObject(SemanticState s,string type,Guid? parent,int order,string? role,Dictionary<string,object?> data){Guid id=Ids.NewV4();s.Objects[id]=new(id,type,parent,OrderKey.Initial(order),role,new(data,StringComparer.Ordinal));return id;}
    private static void AddProviderIdFacet(SemanticState s,Guid target,string kind,string? value,Dictionary<string,Guid> corr){if(string.IsNullOrWhiteSpace(value))return;byte[] p=OdfPackage.Utf8(value);Guid f=Ids.NewV4();s.ProviderFacets[f]=new(f,"odf",kind,target,ExtensionCoverageKind.Object,ExtensionEditPolicy.Independent,p,SHA256.HashData(p),"provider-correspondence-evidence",false);if(!corr.ContainsKey(value))corr[value]=target;}
    private static void AddProviderPayloadFacet(SemanticState s,Guid target,string provider,string kind,string value,bool required){byte[] p=OdfPackage.Utf8(value);Guid f=Ids.NewV4();s.ProviderFacets[f]=new(f,provider,kind,target,ExtensionCoverageKind.Object,ExtensionEditPolicy.Independent,p,SHA256.HashData(p),"provider-evidence",required);}
}
