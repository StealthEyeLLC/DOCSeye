using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using DOCSeye.Core;

namespace DOCSeye.Providers;

public sealed record ProviderEnvironment(
    string Provider,
    string ProviderVersion,
    string Locale,
    string HyphenationVersion,
    IReadOnlyDictionary<string,string> FontDigests,
    IReadOnlyDictionary<string,string> Configuration);
public sealed record LayoutRevisionRecord(
    string LayoutRevisionId,
    Guid FamilyId,
    Guid BranchId,
    Guid SemanticRevisionId,
    string SemanticRoot,
    string LayoutProfile,
    ProviderEnvironment Environment);
public sealed record RenderRegion(Guid ObjectId,string RegionId,int? Page,string? X,string? Y,int Occurrences=1);
public sealed record RenderArtifact(
    string Kind,string Path,string Sha256,LayoutRevisionRecord LayoutRevision,IReadOnlyDictionary<Guid,RenderRegion> Mapping,IReadOnlyList<string> Warnings,string ProviderSourcePath,string ProviderSourceSha256);

public static class LayoutQualification
{
    public static LayoutRevisionRecord Create(SemanticState state,RevisionHead head,string profile,ProviderEnvironment env)
    {
        var payload=new Dictionary<string,object?>(StringComparer.Ordinal)
        {
            ["family_id"]=head.FamilyId,["branch_id"]=head.BranchId,["semantic_revision_id"]=head.RevisionId,["semantic_root"]=head.SemanticRoot,["layout_profile"]=profile,["provider"]=env.Provider,["provider_version"]=env.ProviderVersion,["locale"]=env.Locale,["hyphenation_version"]=env.HyphenationVersion,
            ["fonts"]=env.FontDigests.OrderBy(x=>x.Key,StringComparer.Ordinal).ToDictionary(x=>x.Key,x=>(object?)x.Value,StringComparer.Ordinal),["configuration"]=env.Configuration.OrderBy(x=>x.Key,StringComparer.Ordinal).ToDictionary(x=>x.Key,x=>(object?)x.Value,StringComparer.Ordinal)
        };
        string id=Convert.ToHexString(SHA256.HashData(CanonicalCbor.Encode(payload))).ToLowerInvariant();return new(id,head.FamilyId,head.BranchId,head.RevisionId,Convert.ToHexString(head.SemanticRoot).ToLowerInvariant(),profile,env);
    }
}

public sealed class HtmlProvider
{
    public RenderArtifact Render(SemanticState state,RevisionHead head,string outputPath,IReadOnlyCollection<Guid> requiredMapping,ProviderEnvironment env,string profile="US Letter")
    {
        if(state.RevisionId!=head.RevisionId||!state.ComputeRoot().SequenceEqual(head.SemanticRoot))throw new InvalidOperationException("semantic_head_mismatch");Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(outputPath))!);var layout=LayoutQualification.Create(state,head,profile,env);var map=new Dictionary<Guid,RenderRegion>();var sb=new StringBuilder();var headerText=PageFurnitureText(state,"header");var footerText=PageFurnitureText(state,"footer");
        sb.Append("<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><title>DOCSeye Native Render</title>");
        Meta("docseye-family",Ids.Lower(head.FamilyId));Meta("docseye-branch",Ids.Lower(head.BranchId));Meta("docseye-revision",Ids.Lower(head.RevisionId));Meta("docseye-semantic-root",Convert.ToHexString(head.SemanticRoot).ToLowerInvariant());Meta("docseye-layout-revision",layout.LayoutRevisionId);Meta("docseye-provider",env.Provider+" "+env.ProviderVersion);
        sb.Append("<style>body{font-family:system-ui,sans-serif;max-width:58rem;margin:auto;line-height:1.45}table{border-collapse:collapse;width:100%}th,td{border:1px solid;padding:.3rem}figure{margin:1rem 0}aside{border-left:3px solid;padding-left:1rem}</style></head><body>");if(headerText is not null)Mark(headerText.Id,"header",E(headerText.Data.GetValueOrDefault("text") as string??string.Empty));else sb.Append("<header>DOCSeye native semantic render</header>");sb.Append("<main>");
        foreach(var obj in RenderOrder(state))RenderObject(obj);sb.Append("</main>");if(footerText is not null)Mark(footerText.Id,"footer",E(footerText.Data.GetValueOrDefault("text") as string??string.Empty)+" <code>"+E(Ids.Lower(head.RevisionId))+"</code>");else sb.Append("<footer>Source revision <code>").Append(E(Ids.Lower(head.RevisionId))).Append("</code></footer>");sb.Append("</body></html>");File.WriteAllText(outputPath,sb.ToString(),new UTF8Encoding(false));
        foreach(Guid id in requiredMapping)if(!map.ContainsKey(id))throw new InvalidOperationException("required_render_mapping_missing:"+Ids.Lower(id));string sha=FileHash(outputPath);return new("html",outputPath,sha,layout,map,[],outputPath,sha);

        void Meta(string name,string content)=>sb.Append("<meta name=\"").Append(E(name)).Append("\" content=\"").Append(E(content)).Append("\">");
        void Mark(Guid id,string tag,string inner,string? attrs=null){string region="obj_"+id.ToString("N");sb.Append('<').Append(tag).Append(" id=\"").Append(region).Append("\" data-docseye-object-id=\"").Append(Ids.Lower(id)).Append('"');if(!string.IsNullOrEmpty(attrs))sb.Append(' ').Append(attrs);sb.Append('>').Append(inner).Append("</").Append(tag).Append('>');map[id]=new(id,"#"+region,null,null,null);}
        void RenderObject(SemanticObject o)
        {
            if(o.Retired)return;string label="<span class=\"docseye-id\" hidden>"+E(Ids.Lower(o.Id))+"</span>";
            if(o.Type=="text_block")
            {
                if(IsNestedText(state,o)||IsPageFurnitureText(state,o))return;string text=E(o.Data.TryGetValue("text",out var tv)?tv as string??string.Empty:string.Empty);string attrs=ScriptAttrs(text);if(o.Role=="heading"){int level=Math.Clamp(Convert.ToInt32(o.Data.GetValueOrDefault("heading_level")??2),1,6);Mark(o.Id,"h"+level,text+label,attrs);}else Mark(o.Id,"p",text+label,attrs);return;
            }
            if(o.Type=="list_item")
            {string text=string.Join(" ",Children(state,o.Id,"text_block").Select(x=>E(x.Data["text"] as string??string.Empty)+ChildMarker(x)));Mark(o.Id,"div",text+label,"role=\"listitem\"");foreach(var child in Children(state,o.Id,"text_block"))map[child.Id]=new(child.Id,"#obj_"+o.Id.ToString("N"),null,null,null);return;}
            if(o.Type=="table"){RenderTable(o);return;}
            if(o.Type=="figure"){string alt=E(o.Data.GetValueOrDefault("alt_text") as string??"Figure"),caption=E(o.Data.GetValueOrDefault("caption") as string??"Figure");Mark(o.Id,"figure","<div role=\"img\" aria-label=\""+alt+"\">Figure</div><figcaption>"+caption+"</figcaption>"+label);return;}
            if(o.Type=="link"){string href=E(o.Data.GetValueOrDefault("href") as string??"#"),text=E(o.Data.GetValueOrDefault("label") as string??href);Mark(o.Id,"p","<a href=\""+href+"\">"+text+"</a>"+label);return;}
            if(o.Type=="note"){string body=DescendantText(state,o.Id);Mark(o.Id,"aside",E(body)+label,"role=\"note\"");foreach(var t in Descendants(state,o.Id).Where(x=>x.Type=="text_block"))map[t.Id]=new(t.Id,"#obj_"+o.Id.ToString("N"),null,null,null);return;}
            if(o.Type=="math"){string math=E(o.Data.GetValueOrDefault("presentation_mathml") as string??"Math");Mark(o.Id,"div","<code>"+math+"</code>"+label,"role=\"math\"");return;}
            if(o.Type=="field"){string cls=o.Data.GetValueOrDefault("class") as string??"field";string value=cls=="page_count"?"Page count is layout-dependent":(o.Data.GetValueOrDefault("cached_result")?.ToString()??cls);Mark(o.Id,"p",E(value)+label,"data-field-class=\""+E(cls)+"\"");return;}
        }
        void RenderTable(SemanticObject table)
        {
            var cells=Children(state,table.Id,"table_cell").Where(c=>!c.Retired).OrderBy(c=>Convert.ToInt32(c.Data.GetValueOrDefault("row")??0)).ThenBy(c=>Convert.ToInt32(c.Data.GetValueOrDefault("column")??0)).ToArray();var inner=new StringBuilder("<table><caption>Native table</caption><tbody>");foreach(var cell in cells){bool h=Convert.ToBoolean(cell.Data.GetValueOrDefault("row_header")??false)||Convert.ToBoolean(cell.Data.GetValueOrDefault("column_header")??false);string tag=h?"th":"td",scope=Convert.ToBoolean(cell.Data.GetValueOrDefault("column_header")??false)?" scope=\"col\"":Convert.ToBoolean(cell.Data.GetValueOrDefault("row_header")??false)?" scope=\"row\"":"";string id="obj_"+cell.Id.ToString("N");int rs=Convert.ToInt32(cell.Data.GetValueOrDefault("row_span")??1),cs=Convert.ToInt32(cell.Data.GetValueOrDefault("column_span")??1);inner.Append("<tr><").Append(tag).Append(scope).Append(" id=\"").Append(id).Append("\" data-docseye-object-id=\"").Append(Ids.Lower(cell.Id)).Append("\" rowspan=\"").Append(rs).Append("\" colspan=\"").Append(cs).Append("\">").Append(E(cell.Data.GetValueOrDefault("text") as string??string.Empty)).Append("</").Append(tag).Append("></tr>");map[cell.Id]=new(cell.Id,"#"+id,null,null,null);}inner.Append("</tbody></table>");Mark(table.Id,"section",inner.ToString());
        }
        string ChildMarker(SemanticObject child)=>"<span id=\"obj_"+child.Id.ToString("N")+"\" data-docseye-object-id=\""+Ids.Lower(child.Id)+"\" hidden></span>";
        static string E(string x)=>System.Net.WebUtility.HtmlEncode(x);
    }

    private static IEnumerable<SemanticObject> RenderOrder(SemanticState state)=>state.Objects.Values.Where(o=>!o.Retired&&ShouldRender(o)).OrderBy(o=>LogicalPath(state,o),StringComparer.Ordinal);
    private static bool ShouldRender(SemanticObject o)=>o.Type is "text_block" or "list_item" or "table" or "figure" or "link" or "note" or "math" or "field";
    private static string LogicalPath(SemanticState s,SemanticObject o){var parts=new Stack<string>();SemanticObject? cur=o;while(cur is not null){parts.Push(Convert.ToHexString(cur.Order.Bytes()));cur=cur.ParentId is Guid p&&s.Objects.TryGetValue(p,out var parent)?parent:null;}return string.Join('/',parts)+":"+Ids.Lower(o.Id);}
    private static IEnumerable<SemanticObject> Children(SemanticState s,Guid parent,string? type=null)=>s.Objects.Values.Where(o=>!o.Retired&&o.ParentId==parent&&(type is null||o.Type==type)).OrderBy(o=>o.Order);
    private static IEnumerable<SemanticObject> Descendants(SemanticState s,Guid root){var q=new Queue<Guid>();q.Enqueue(root);while(q.Count>0){Guid p=q.Dequeue();foreach(var c in Children(s,p)){yield return c;q.Enqueue(c.Id);}}}
    private static string DescendantText(SemanticState s,Guid root)=>string.Join(" ",Descendants(s,root).Where(o=>o.Type=="text_block").Select(o=>o.Data.GetValueOrDefault("text") as string??string.Empty));
    private static bool IsNestedText(SemanticState s,SemanticObject o)=>o.ParentId is Guid p&&s.Objects.TryGetValue(p,out var parent)&&(parent.Type=="list_item"||(parent.Type=="flow"&&parent.ParentId is Guid pp&&s.Objects.TryGetValue(pp,out var grand)&&grand.Type=="note"));
    private static bool IsPageFurnitureText(SemanticState s,SemanticObject o)=>o.ParentId is Guid p&&s.Objects.TryGetValue(p,out var parent)&&parent.Type=="flow"&&(parent.Role=="header"||parent.Role=="footer");
    private static SemanticObject? PageFurnitureText(SemanticState s,string role){var flow=s.Objects.Values.FirstOrDefault(o=>!o.Retired&&o.Type=="flow"&&o.Role==role);return flow is null?null:Children(s,flow.Id,"text_block").FirstOrDefault();}
    private static string ScriptAttrs(string text){bool ar=text.EnumerateRunes().Any(r=>r.Value is >=0x0600 and <=0x06FF),hi=text.EnumerateRunes().Any(r=>r.Value is >=0x0900 and <=0x097F),ja=text.EnumerateRunes().Any(r=>r.Value is >=0x3040 and <=0x9FFF);return ar?"lang=\"ar\" dir=\"rtl\"":hi?"lang=\"hi\"":ja?"lang=\"ja\"":"";}
    internal static string FileHash(string path){using var fs=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(fs)).ToLowerInvariant();}
}

public sealed class TypstPdfProvider
{
    public RenderArtifact Render(SemanticState state,RevisionHead head,string outputDir,IReadOnlyCollection<Guid> requiredMapping,ProviderEnvironment env,string typstExe,string veraPdfBat,string javaHome,string semanticFigureSvg,string profile="US Letter")
    {
        if(state.RevisionId!=head.RevisionId||!state.ComputeRoot().SequenceEqual(head.SemanticRoot))throw new InvalidOperationException("semantic_head_mismatch");Directory.CreateDirectory(outputDir);string source=System.IO.Path.Combine(outputDir,"render.typ"),pdf=System.IO.Path.Combine(outputDir,"render.pdf"),figure=System.IO.Path.Combine(outputDir,"semantic-figure.svg");File.Copy(semanticFigureSvg,figure,true);var layout=LayoutQualification.Create(state,head,profile,env);var labels=new Dictionary<Guid,string>();var sb=new StringBuilder();
        var headerText=PageFurnitureText(state,"header");var footerText=PageFurnitureText(state,"footer");string? headerLabel=headerText is null?null:"d_"+headerText.Id.ToString("N"),footerLabel=footerText is null?null:"d_"+footerText.Id.ToString("N");if(headerText is not null)labels[headerText.Id]=headerLabel!;if(footerText is not null)labels[footerText.Id]=footerLabel!;string headerMarkup=headerText is null?"DOCSeye":"#("+TypstString(headerText.Data.GetValueOrDefault("text") as string??string.Empty)+") <"+headerLabel+">";string footerMarkup=footerText is null?"Native semantic revision "+Ids.Lower(head.RevisionId)[..8]:"#("+TypstString(footerText.Data.GetValueOrDefault("text") as string??string.Empty)+") <"+footerLabel+">";
        sb.AppendLine("#set document(title: \"DOCSeye Native Semantic Render\", author: \"StealthEyeLLC\")");sb.AppendLine("#set page(paper: \"us-letter\", margin: (x: 0.72in, y: 0.68in), numbering: \"1\", header: context ["+headerMarkup+"], footer: context ["+footerMarkup+" - #counter(page).display()])");sb.AppendLine("#set text(font: (\"Segoe UI\", \"Nirmala UI\", \"Yu Gothic\", \"Segoe UI Emoji\", \"Arial\"), size: 11pt, lang: \"en\")");sb.AppendLine("#set par(justify: true, leading: 0.8em)");sb.AppendLine("#metadata((family: \""+Ids.Lower(head.FamilyId)+"\", branch: \""+Ids.Lower(head.BranchId)+"\", revision: \""+Ids.Lower(head.RevisionId)+"\", root: \""+Convert.ToHexString(head.SemanticRoot).ToLowerInvariant()+"\", layout: \""+layout.LayoutRevisionId+"\")) <docseye_source>");
        foreach(var obj in state.Objects.Values.Where(o=>!o.Retired&&ShouldRender(o)).OrderBy(o=>LogicalPath(state,o),StringComparer.Ordinal))RenderObject(obj);File.WriteAllText(source,sb.ToString(),new UTF8Encoding(false));
        Run(typstExe,["compile","--features","a11y-extras","--pdf-standard","ua-1","--font-path",@"C:\Windows\Fonts",source,pdf],null);var mapping=QueryMappings();var requiredMissing=requiredMapping.Where(id=>!mapping.ContainsKey(id)).ToArray();if(requiredMissing.Length>0)throw new InvalidOperationException("required_render_mapping_missing:"+string.Join(',',requiredMissing.Select(Ids.Lower)));
        string validator=Run(veraPdfBat,["-f","ua1","--format","json","--maxfailuresdisplayed","50",pdf],new Dictionary<string,string>{{"JAVA_HOME",javaHome},{"PATH",System.IO.Path.Combine(javaHome,"bin")+System.IO.Path.PathSeparator+Environment.GetEnvironmentVariable("PATH")}});using(var doc=JsonDocument.Parse(validator)){var result=doc.RootElement.GetProperty("report").GetProperty("jobs")[0].GetProperty("validationResult")[0];if(!result.GetProperty("compliant").GetBoolean())throw new InvalidOperationException("verapdf_pdfua1_failed:"+result.GetRawText());}
        return new("pdf",pdf,HtmlProvider.FileHash(pdf),layout,mapping,[],source,HtmlProvider.FileHash(source));

        void Label(Guid id){string l="d_"+id.ToString("N");labels[id]=l;sb.Append("#metadata(\"docseye-object:"+Ids.Lower(id)+"\") <"+l+">").AppendLine();}
        void RenderObject(SemanticObject o)
        {
            if(o.Type=="text_block")
            {
                if(IsNestedText(state,o)||IsPageFurnitureText(state,o))return;string text=o.Data.GetValueOrDefault("text") as string??string.Empty;if(o.Role=="heading"){int level=Math.Clamp(Convert.ToInt32(o.Data.GetValueOrDefault("heading_level")??2),1,6);sb.Append("#heading(level: ").Append(level).Append(")[#(").Append(TypstString(text)).Append(")] ");Label(o.Id);}else{sb.Append("#par[").Append(ScriptContent(text)).Append("] ");Label(o.Id);}return;
            }
            if(o.Type=="list_item")
            {var textChildren=Children(state,o.Id,"text_block").ToArray();sb.Append("- ");foreach(var child in textChildren){sb.Append("#(").Append(TypstString(child.Data.GetValueOrDefault("text") as string??string.Empty)).Append(") ");LabelInline(child.Id);}Label(o.Id);return;}
            if(o.Type=="table"){RenderTable(o);return;}
            if(o.Type=="figure"){string alt=o.Data.GetValueOrDefault("alt_text") as string??"Figure",caption=o.Data.GetValueOrDefault("caption") as string??"Figure";sb.Append("#figure(image(\"semantic-figure.svg\", width: 38%, alt: ").Append(TypstString(alt)).Append("), caption: [#(").Append(TypstString(caption)).Append(")]) ");Label(o.Id);return;}
            if(o.Type=="link"){string href=o.Data.GetValueOrDefault("href") as string??"https://example.invalid",label=o.Data.GetValueOrDefault("label") as string??href;sb.Append("#par[#link(").Append(TypstString(href)).Append(")[#(").Append(TypstString(label)).Append(")]] ");Label(o.Id);return;}
            if(o.Type=="note"){string body=DescendantText(state,o.Id);sb.Append("#par[Note #footnote[#(").Append(TypstString(body)).Append(") ");foreach(var t in Descendants(state,o.Id).Where(x=>x.Type=="text_block"))LabelInline(t.Id);sb.Append("]] ");Label(o.Id);return;}
            if(o.Type=="math"){string math=o.Data.GetValueOrDefault("presentation_mathml") as string??"Math";sb.Append("#par[Math semantic source: #(").Append(TypstString(math)).Append(")] ");Label(o.Id);return;}
            if(o.Type=="field"){string cls=o.Data.GetValueOrDefault("class") as string??"field";if(cls=="page_count")sb.Append("#context [Page count: #counter(page).final().first()] ");else sb.Append("#par[Field: #(").Append(TypstString(cls)).Append(")] ");Label(o.Id);return;}
        }
        void RenderTable(SemanticObject table)
        {
            int columns=Math.Max(1,Convert.ToInt32(table.Data.GetValueOrDefault("columns")??4));var cells=Children(state,table.Id,"table_cell").Where(c=>!c.Retired).OrderBy(c=>Convert.ToInt32(c.Data.GetValueOrDefault("row")??0)).ThenBy(c=>Convert.ToInt32(c.Data.GetValueOrDefault("column")??0)).ToArray();var headers=cells.Where(c=>Convert.ToInt32(c.Data.GetValueOrDefault("row")??0)==0).Take(columns).ToArray();sb.AppendLine("#figure(");sb.Append(" table(columns: ").Append(columns).AppendLine(", inset: 4pt, stroke: 0.5pt,");if(headers.Length>0){sb.Append(" table.header(repeat: true,");foreach(var cell in headers){sb.Append(" [#(").Append(TypstString(cell.Data.GetValueOrDefault("text") as string??string.Empty)).Append(") ");LabelInline(cell.Id);sb.Append("],");}sb.AppendLine("),");}
            foreach(var cell in cells.Except(headers)){sb.Append(" [#(").Append(TypstString(cell.Data.GetValueOrDefault("text") as string??string.Empty)).Append(") ");LabelInline(cell.Id);sb.AppendLine("],");}sb.AppendLine(" ), caption: [Native authored table],");sb.Append(") ");Label(table.Id);
        }
        void LabelInline(Guid id){string l="d_"+id.ToString("N");labels[id]=l;sb.Append("#metadata(\"docseye-object:"+Ids.Lower(id)+"\") <"+l+">");}
        IReadOnlyDictionary<Guid,RenderRegion> QueryMappings()
        {
            var wanted=requiredMapping.Where(labels.ContainsKey).Select(id=>(id,label:labels[id])).ToArray();string expr="("+string.Join(',',wanted.Select(x=>"<"+x.label+">"))+ ",).map(x => { let items=query(x); let p=items.first().location().position(); (page:p.page,x:p.x,y:p.y,count:items.len()) })";string json=Run(typstExe,["eval",expr,"--in",source,"--target","paged","--font-path",@"C:\Windows\Fonts"],null);using var doc=JsonDocument.Parse(json);var result=new Dictionary<Guid,RenderRegion>();int i=0;foreach(var e in doc.RootElement.EnumerateArray()){var w=wanted[i++];result[w.id]=new(w.id,"typst:"+w.label,e.GetProperty("page").GetInt32(),e.GetProperty("x").ToString(),e.GetProperty("y").ToString(),e.GetProperty("count").GetInt32());}return result;
        }
    }

    private static string Run(string file,IReadOnlyList<string> args,IReadOnlyDictionary<string,string>? env)
    {
        bool batch=file.EndsWith(".bat",StringComparison.OrdinalIgnoreCase)||file.EndsWith(".cmd",StringComparison.OrdinalIgnoreCase);var psi=new ProcessStartInfo(batch?Environment.GetEnvironmentVariable("ComSpec")??"cmd.exe":file){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};if(batch){psi.ArgumentList.Add("/d");psi.ArgumentList.Add("/c");psi.ArgumentList.Add(file);}foreach(string a in args)psi.ArgumentList.Add(a);if(env is not null)foreach(var e in env)psi.Environment[e.Key]=e.Value;using var p=Process.Start(psi)??throw new InvalidOperationException("provider_start_failed");var stdoutTask=p.StandardOutput.ReadToEndAsync();var stderrTask=p.StandardError.ReadToEndAsync();if(!p.WaitForExit(60000)){try{p.Kill(true);}catch{}throw new TimeoutException("provider_timeout:"+System.IO.Path.GetFileName(file));}Task.WaitAll(stdoutTask,stderrTask);string stdout=stdoutTask.Result,stderr=stderrTask.Result;if(p.ExitCode!=0)throw new InvalidOperationException($"provider_failed:{System.IO.Path.GetFileName(file)}:{p.ExitCode}:{stderr}");return stdout.Trim();
    }
    private static string TypstString(string value){var sb=new StringBuilder("\"");foreach(var rune in value.EnumerateRunes()){int v=rune.Value;if(v=='\\')sb.Append("\\\\");else if(v=='\"')sb.Append("\\\"");else if(v is >=0x20 and <=0x7E)sb.Append((char)v);else sb.Append("\\u{").Append(v.ToString("X",CultureInfo.InvariantCulture)).Append('}');}return sb.Append('"').ToString();}
    private static string ScriptContent(string text){string v="#("+TypstString(text)+")";bool ar=text.EnumerateRunes().Any(r=>r.Value is >=0x0600 and <=0x06FF),hi=text.EnumerateRunes().Any(r=>r.Value is >=0x0900 and <=0x097F),ja=text.EnumerateRunes().Any(r=>r.Value is >=0x3040 and <=0x9FFF);return ar?"#text(lang: \"ar\", dir: rtl)["+v+"]":hi?"#text(lang: \"hi\")["+v+"]":ja?"#text(lang: \"ja\")["+v+"]":v;}
    private static IEnumerable<SemanticObject> Children(SemanticState s,Guid parent,string? type=null)=>s.Objects.Values.Where(o=>!o.Retired&&o.ParentId==parent&&(type is null||o.Type==type)).OrderBy(o=>o.Order);
    private static IEnumerable<SemanticObject> Descendants(SemanticState s,Guid root){var q=new Queue<Guid>();q.Enqueue(root);while(q.Count>0){Guid p=q.Dequeue();foreach(var c in Children(s,p)){yield return c;q.Enqueue(c.Id);}}}
    private static string DescendantText(SemanticState s,Guid root)=>string.Join(" ",Descendants(s,root).Where(o=>o.Type=="text_block").Select(o=>o.Data.GetValueOrDefault("text") as string??string.Empty));
    private static bool IsNestedText(SemanticState s,SemanticObject o)=>o.ParentId is Guid p&&s.Objects.TryGetValue(p,out var parent)&&(parent.Type=="list_item"||(parent.Type=="flow"&&parent.ParentId is Guid pp&&s.Objects.TryGetValue(pp,out var grand)&&grand.Type=="note"));
    private static bool IsPageFurnitureText(SemanticState s,SemanticObject o)=>o.ParentId is Guid p&&s.Objects.TryGetValue(p,out var parent)&&parent.Type=="flow"&&(parent.Role=="header"||parent.Role=="footer");
    private static SemanticObject? PageFurnitureText(SemanticState s,string role){var flow=s.Objects.Values.FirstOrDefault(o=>!o.Retired&&o.Type=="flow"&&o.Role==role);return flow is null?null:Children(s,flow.Id,"text_block").FirstOrDefault();}
    private static bool ShouldRender(SemanticObject o)=>o.Type is "text_block" or "list_item" or "table" or "figure" or "link" or "note" or "math" or "field";
    private static string LogicalPath(SemanticState s,SemanticObject o){var parts=new Stack<string>();SemanticObject? cur=o;while(cur is not null){parts.Push(Convert.ToHexString(cur.Order.Bytes()));cur=cur.ParentId is Guid p&&s.Objects.TryGetValue(p,out var parent)?parent:null;}return string.Join('/',parts)+":"+Ids.Lower(o.Id);}
}
