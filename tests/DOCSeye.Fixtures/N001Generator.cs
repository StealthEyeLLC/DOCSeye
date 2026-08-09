using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DOCSeye.Core;
using DOCSeye.Storage.Sqlite;

namespace DOCSeye.Fixtures;

public sealed record FixtureObjectManifest(string Id,string Type,string? ParentId,string OrderKey,string? SemanticRole,bool RequiredRenderMapping);
public sealed record FixtureBoundaryManifest(string Id,string OwnerId,int ScalarOffset,string Affinity,string State);
public sealed record FixtureRangeManifest(string Id,string StartBoundaryId,string EndBoundaryId,bool AllowMultiInterval,string Kind,string State,string[][] Intervals);
public sealed record N001Manifest(
    string Fixture,string ArchitectureFreeze,string Seed,string FamilyId,string BranchId,string RevisionId,string SemanticRoot,
    IReadOnlyList<FixtureObjectManifest> Objects,IReadOnlyList<FixtureBoundaryManifest> Boundaries,IReadOnlyList<FixtureRangeManifest> Ranges,
    IReadOnlyDictionary<string,string[]> Sentinels,IReadOnlyDictionary<string,string> AssetDigests,IReadOnlyDictionary<string,int> Counts);
public sealed record N001FixtureResult(SemanticState State,N001Manifest Manifest,IReadOnlyDictionary<string,byte[]> AssetBytes);

public static class N001Generator
{
    public const ulong Seed = 0xD0C50001UL;

    public static N001FixtureResult Generate()
    {
        var rng = new RandomNumberGeneratorLike(Seed); Guid Id()=>Ids.DeterministicV4(rng);
        var s = new SemanticState { FamilyId=Id(), BranchId=Id(), RevisionId=Id(), Sequence=0, Mode=AuthorityMode.NativeAuthored };
        var siblingOrdinals = new Dictionary<Guid,int>();
        Guid Add(string type,Guid? parent,string? role,Dictionary<string,object?>? data=null)
        {
            Guid parentKey=parent??Guid.Empty; int ordinal=siblingOrdinals.TryGetValue(parentKey,out int n)?n:0; siblingOrdinals[parentKey]=ordinal+1; Guid id=Id();
            s.Objects[id]=new(id,type,parent,OrderKey.Initial(ordinal),role,data??new(StringComparer.Ordinal)); return id;
        }
        Dictionary<string,object?> D(params (string key,object? value)[] items){var d=new Dictionary<string,object?>(StringComparer.Ordinal);foreach(var x in items)d[x.key]=x.value;return d;}

        Guid root=Add("document",null,"document",D(("title","N-001 Native Semantic Authority Fixture"),("language","en-US"),("executable_content",false),("remote_auto_fetch",false)));
        Guid main=Add("flow",root,"main",D(("kind","main"))); Guid sidebar=Add("flow",root,"sidebar",D(("kind","sidebar")));
        Guid header=Add("flow",root,"header",D(("kind","reusable_header"))); Guid footer=Add("flow",root,"footer",D(("kind","footer")));
        Guid section1=Add("section",main,"section",D(("columns",1),("header_flow_id",header),("footer_flow_id",footer),("page_numbering","arabic"),("language","en-US")));
        Guid section2=Add("section",main,"section",D(("columns",2),("header_flow_id",header),("footer_flow_id",footer),("page_numbering","continue"),("language","en-US"),("hyphenation","en-US")));
        Guid section3=Add("section",main,"section",D(("columns",1),("header_flow_id",header),("footer_flow_id",footer),("page_numbering","continue"),("language","ja-JP")));
        _=Add("text_block",header,"header",D(("text","DOCSeye N-001 — deterministic native fixture"),("keep_together",true)));
        _=Add("text_block",footer,"footer",D(("text","Native Semantic Authority • page field follows")));
        _=Add("text_block",sidebar,"sidebar",D(("text","Sidebar flow: correspondence is identity-bound, not text-bound."),("wrap","sidebar")));

        Guid h1=Add("text_block",section1,"heading",D(("text","1. Identity and Authority"),("heading_level",1),("keep_with_next",true)));
        Guid h2=Add("text_block",section1,"heading",D(("text","1.1 Boundaries under pressure"),("heading_level",2),("keep_with_next",true)));
        Guid h3=Add("text_block",section2,"heading",D(("text","2. Structured Content"),("heading_level",1),("keep_with_next",true)));
        Guid h4=Add("text_block",section2,"heading",D(("text","2.1 Tables, review, and fields"),("heading_level",3),("keep_with_next",true)));
        Guid h5=Add("text_block",section3,"heading",D(("text","3. Layout and Interoperability"),("heading_level",2),("keep_with_next",true)));

        string duplicate="Identity must never follow duplicate text.";
        string repeated="correspondence pressure phrase";
        string[] bodyTexts=
        [
            "Native semantic authority remains inside the validated DND state.",
            duplicate,
            "Overlapping anchors exercise exact insertion and deletion policies without fuzzy recovery.",
            "The correspondence pressure phrase appears here once: "+repeated+".",
            "Arabic logical text: مرحبا بالعالم — DOCSeye يحافظ على الترتيب المنطقي.",
            "Decomposed sequence: cafe\u0301 and A\u030A stay exactly authored without normalization.",
            duplicate,
            "Mixed bidi: English ثم العربية then English, with logical affinity only.",
            "Emoji ZWJ cluster: 👩‍👩‍👧‍👧 remains one human-facing grapheme operation.",
            "Japanese CJK: 文書の意味論的な識別子はページ位置ではありません。",
            "Devanagari: दस्तावेज़ की पहचान पाठ की समानता से नहीं बदलती।",
            "The repeated phrase appears again under reflow pressure: "+repeated+".",
            duplicate,
            "Font fallback pressure includes العربية, 日本語, हिन्दी, emoji 🧭, and combining marks e\u0301.",
            "An early edit here is intended to reflow a later table, note, header, footer, and page field.",
            "Keep-together paragraph near a page boundary carries widow and orphan intent.",
            "A late section paragraph follows an explicit break and continues page numbering.",
            "Final body paragraph closes the ten-page-equivalent deterministic workload without executable content."
        ];
        var bodies=new List<Guid>();
        Guid[] bodyParents=[section1,section1,section1,section1,section1,section1,section2,section2,section2,section2,section2,section2,section3,section3,section3,section3,section3,section3];
        for(int i=0;i<bodyTexts.Length;i++) bodies.Add(Add("text_block",bodyParents[i],"body",D(("text",bodyTexts[i]),("widow_orphan",i==15?"2/2":"default"),("keep_together",i==15),("explicit_break",i==16?"page":"none"))));

        // Six retained ranges with overlap, independent co-location, point anchoring, and one cross-block range.
        int Len(Guid block)=>UnicodeText.ScalarCount((string)s.Objects[block].Data["text"]!);
        Guid B(Guid owner,int offset,EdgeAffinity affinity){Guid id=Id();s.Boundaries[id]=new(id,owner,offset,affinity);return id;}
        Guid b1=B(bodies[2],2,EdgeAffinity.ExcludeAtEdge), b2=B(bodies[2],24,EdgeAffinity.ExcludeAtEdge);
        Guid b3=B(bodies[2],10,EdgeAffinity.ExcludeAtEdge), b4=B(bodies[2],36,EdgeAffinity.IncludeAtEdge);
        Guid b5=B(bodies[5],4,EdgeAffinity.ExcludeAtEdge), b6=B(bodies[5],18,EdgeAffinity.ExcludeAtEdge);
        Guid b7=B(bodies[5],4,EdgeAffinity.IncludeAtEdge), b8=B(bodies[5],18,EdgeAffinity.IncludeAtEdge);
        Guid b9=B(bodies[7],8,EdgeAffinity.BeforeInsertion);
        Guid b10=B(bodies[8],3,EdgeAffinity.ExcludeAtEdge), b11=B(bodies[9],Math.Min(12,Len(bodies[9])),EdgeAffinity.ExcludeAtEdge), b12=B(bodies[10],2,EdgeAffinity.ExcludeAtEdge), b13=B(bodies[11],Math.Min(14,Len(bodies[11])),EdgeAffinity.ExcludeAtEdge);
        Guid R(Guid start,Guid end,bool multi,string kind){Guid id=Id();s.Ranges[id]=new(id,start,end,multi,kind);return id;}
        Guid r1=R(b1,b2,true,"comment"), r2=R(b3,b4,true,"style"), r3=R(b5,b6,true,"named_range"), r4=R(b7,b8,true,"suggestion"), r5=R(b9,b9,false,"named_point_anchor"); Guid r6=Id(); s.Ranges[r6]=new(r6,b10,b11,true,"cross_block_multi_interval"){Intervals=[new(b10,b11),new(b12,b13)]};

        // Links, citations, anchors, cross-references.
        Guid link1=Add("link",bodies[0],"link",D(("href","https://example.invalid/docseye/native"),("label","native authority")));
        Guid link2=Add("link",bodies[13],"link",D(("href","https://example.invalid/docseye/provider"),("label","provider evidence")));
        Guid bib1=Add("bibliographic_record",root,"bibliography",D(("key","RFC9562"),("title","Universally Unique IDentifiers (UUIDs)")));
        Guid bib2=Add("bibliographic_record",root,"bibliography",D(("key","RFC8949"),("title","Concise Binary Object Representation")));
        _=Add("citation",bodies[0],"citation",D(("record_id",bib1),("locator","§5.4")));
        _=Add("citation",bodies[6],"citation",D(("record_id",bib2),("locator","§4.2")));
        _=Add("citation",bodies[12],"citation",D(("record_id",bib1),("locator","§4.1")));
        Guid anchor1=Add("named_anchor",bodies[1],"named_anchor",D(("name","identity-anchor"),("range_id",r1)));
        Guid anchor2=Add("named_anchor",bodies[14],"named_anchor",D(("name","render-anchor"),("range_id",r5)));
        _=Add("cross_reference",bodies[3],"cross_reference",D(("target_id",anchor1),("display","Identity anchor")));
        _=Add("cross_reference",bodies[11],"cross_reference",D(("target_id",anchor2),("display","Render anchor")));
        _=Add("cross_reference",bodies[17],"cross_reference",D(("target_id",anchor1),("display","Identity anchor again")));

        // First-class two-level list with four total items and one multi-block item.
        Guid list=Add("list",section2,"list",D(("kind","ordered"),("start",3),("restart_intent","explicit")));
        Guid item1=Add("list_item",list,"list_item",D(("level",0),("ordinal_intent",3))); _=Add("text_block",item1,"list_item_text",D(("text","First list item starts explicitly at three.")));
        Guid item2=Add("list_item",list,"list_item",D(("level",0),("ordinal_intent",4))); _=Add("text_block",item2,"list_item_text",D(("text","Second list item owns a nested level.")));
        Guid nested=Add("list",item2,"list",D(("kind","ordered"),("start",1),("restart_intent","nested")));
        Guid item3=Add("list_item",nested,"list_item",D(("level",1),("ordinal_intent",1))); _=Add("text_block",item3,"list_item_text",D(("text","Nested third item.")));
        Guid item4=Add("list_item",list,"list_item",D(("level",0),("ordinal_intent",5),("multi_block",true))); _=Add("text_block",item4,"list_item_text",D(("text","Fourth item first block."))); _=Add("text_block",item4,"list_item_text",D(("text","Fourth item second block.")));

        // Authored 4x4 table, rows/columns/cells, 2x2 merged region, identical rows, and nested 2x2 table.
        Guid table=Add("table",section2,"table",D(("rows",4),("columns",4),("repeat_header",true),("allow_split",true)));
        var rows=new List<Guid>(); var cols=new List<Guid>();
        for(int i=0;i<4;i++) rows.Add(Add("table_row",table,"table_row",D(("index",i),("is_header",i==0))));
        for(int i=0;i<4;i++) cols.Add(Add("table_column",table,"table_column",D(("index",i),("is_header",i==0))));
        var cells=new Dictionary<(int r,int c),Guid>();
        Guid mergedCell=Add("table_cell",table,"table_cell",D(("row",0),("column",0),("row_span",2),("column_span",2),("row_header",true),("column_header",true),("text","Merged header region"))); cells[(0,0)]=mergedCell;
        for(int rr=0;rr<4;rr++) for(int cc=0;cc<4;cc++)
        {
            if(rr<2&&cc<2) continue;
            string text=(rr is 2 or 3)?$"duplicate-row-{cc}":$"r{rr}c{cc}";
            cells[(rr,cc)]=Add("table_cell",table,"table_cell",D(("row",rr),("column",cc),("row_span",1),("column_span",1),("row_header",cc==0),("column_header",rr==0),("text",text)));
        }
        Guid nestedHost=cells[(2,2)]; Guid nestedTable=Add("table",nestedHost,"table",D(("rows",2),("columns",2),("nested",true)));
        for(int i=0;i<2;i++) _=Add("table_row",nestedTable,"table_row",D(("index",i),("is_header",i==0)));
        for(int i=0;i<2;i++) _=Add("table_column",nestedTable,"table_column",D(("index",i),("is_header",i==0)));
        for(int rr=0;rr<2;rr++) for(int cc=0;cc<2;cc++) _=Add("table_cell",nestedTable,"table_cell",D(("row",rr),("column",cc),("row_span",1),("column_span",1),("text",$"nested-{rr}-{cc}")));

        // Roles, styles, tokens, direct overrides, and layout profiles.
        foreach(string role in new[]{"body","heading","caption","code_like"}) _=Add("semantic_role",root,"semantic_role",D(("name",role)));
        Guid style1=Add("named_style",root,"named_style",D(("name","Body Native"),("inherits",null),("font_token","font.body")));
        Guid style2=Add("named_style",root,"named_style",D(("name","Heading Native"),("inherits",style1),("font_token","font.heading")));
        Guid style3=Add("named_style",root,"named_style",D(("name","Caption Native"),("inherits",style1),("font_token","font.caption")));
        foreach(var token in new[]{("font.body","Segoe UI"),("font.heading","Segoe UI Semibold"),("font.caption","Segoe UI"),("space.body","12pt"),("color.text","#111111"),("color.accent","#2a5db0")}) _=Add("theme_token",root,"theme_token",D(("name",token.Item1),("value",token.Item2)));
        _=Add("direct_override",bodies[4],"direct_override",D(("property","language"),("value","ar")));
        _=Add("direct_override",bodies[9],"direct_override",D(("property","language"),("value","ja")));
        _=Add("direct_override",bodies[15],"direct_override",D(("property","keep_together"),("value",true)));
        Guid letter=Add("layout_profile",root,"layout_profile",D(("name","US Letter"),("width","8.5in"),("height","11in"),("margin","0.75in")));
        Guid a4=Add("layout_profile",root,"layout_profile",D(("name","A4"),("width","210mm"),("height","297mm"),("margin","18mm")));

        // Comment threads and ordered replies.
        Guid thread1=Add("comment_thread",root,"comment_thread",D(("state","open"),("target_range_id",r1))); Guid comment1=Add("comment",thread1,"comment",D(("author","fixture-author-a"),("timestamp","2026-01-01T00:00:00Z"),("body","Open thread root comment"),("target_range_id",r1),("quoted_display","Overlapping anchors")));
        Guid reply11=Add("comment",thread1,"comment",D(("author","fixture-author-b"),("timestamp","2026-01-01T00:01:00Z"),("body","First reply"),("reply_to",comment1))); _=Add("comment",thread1,"comment",D(("author","fixture-author-c"),("timestamp","2026-01-01T00:02:00Z"),("body","Second reply"),("reply_to",reply11)));
        Guid thread2=Add("comment_thread",root,"comment_thread",D(("state","resolved"),("target_range_id",r3))); Guid comment2=Add("comment",thread2,"comment",D(("author","fixture-author-a"),("timestamp","2026-01-02T00:00:00Z"),("body","Resolved thread root"),("target_range_id",r3))); _=Add("comment",thread2,"comment",D(("author","fixture-author-b"),("timestamp","2026-01-02T00:01:00Z"),("body","Resolved reply"),("reply_to",comment2)));

        Guid suggestionInsert=Guid.Empty;
        foreach(string kind in new[]{"insert","delete","replace","move","style_property","structural_table"})
        { Guid id=Add("suggestion",root,"suggestion",D(("kind",kind),("state","active"),("target_id",kind=="structural_table"?table:bodies[2]),("range_id",kind=="structural_table"?null:r4))); if(kind=="insert")suggestionInsert=id; }

        Guid fieldMetadata=Guid.Empty;
        foreach(var f in new[]{("metadata","document.title","manual"),("fixed_date","2026-01-01","manual"),("cross_reference","identity-anchor","on_semantic_commit"),("toc","headings","on_layout"),("list_of_figures","figures","on_layout"),("page_count","pages","on_layout")})
        { Guid id=Add("field",root,"field",D(("class",f.Item1),("source",f.Item2),("evaluation_policy",f.Item3),("cached_result",null),("result_semantic_revision",null),("staleness","stale"))); if(f.Item1=="metadata")fieldMetadata=id; }

        Guid controlEnum=Guid.Empty;
        foreach(string type in new[]{"text","rich_text","number","boolean","date","enum","repeating_group","object_reference_selector"})
        { object? value=type switch{"boolean"=>true,"number"=>7,"enum"=>"draft","repeating_group"=>new object?[]{"one","two"},"object_reference_selector"=>anchor1,_=>"fixture"}; Guid id=Add("control",root,"control",D(("control_type",type),("value",value),("constraints",new Dictionary<string,object?>(StringComparer.Ordinal){{"required",false},{"copy_policy","remint"}}),("external_binding","inert"))); if(type=="enum")controlEnum=id; }

        // Notes with body flows and exact reference occurrences.
        Guid footnote=Add("note",root,"footnote",D(("note_kind","footnote"))); Guid footFlow=Add("flow",footnote,"note_body",D(("kind","note_body"))); _=Add("text_block",footFlow,"note_body",D(("text","Footnote body under layout pressure."))); _=Add("note_reference",bodies[14],"note_reference",D(("note_id",footnote),("occurrence_kind","footnote")));
        Guid endnote=Add("note",root,"endnote",D(("note_kind","endnote"))); Guid endFlow=Add("flow",endnote,"note_body",D(("kind","note_body"))); _=Add("text_block",endFlow,"note_body",D(("text","Endnote body for exact occurrence identity."))); _=Add("note_reference",bodies[16],"note_reference",D(("note_id",endnote),("occurrence_kind","endnote")));

        // Embedded assets and figure occurrences.
        byte[] raster=Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Y9ZfFQAAAAASUVORK5CYII=");
        byte[] svg=Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"120\" height=\"80\" viewBox=\"0 0 120 80\"><rect width=\"120\" height=\"80\" fill=\"white\"/><circle cx=\"40\" cy=\"40\" r=\"24\" fill=\"black\"/><text x=\"70\" y=\"44\">N001</text></svg>");
        byte[] rasterDigest=SHA256.HashData(raster), svgDigest=SHA256.HashData(svg); s.Assets[Convert.ToHexString(rasterDigest)]=new(rasterDigest,raster.Length,"embedded"); s.Assets[Convert.ToHexString(svgDigest)]=new(svgDigest,svg.Length,"embedded");
        Guid fig1=Add("figure",section2,"figure",D(("asset_digest",rasterDigest),("caption","Shared raster figure A"),("alt_text","Black one-pixel raster fixture"),("placement","anchored"),("wrap","square"),("crop","none"),("sizing","2in")));
        Guid fig2=Add("figure",section2,"figure",D(("asset_digest",rasterDigest),("caption","Shared raster figure B"),("alt_text","Second occurrence of shared raster"),("placement","anchored"),("wrap","tight"),("crop","none"),("sizing","1.5in")));
        Guid fig3=Add("figure",section3,"figure",D(("asset_digest",svgDigest),("caption","SVG semantic figure"),("alt_text","Circle labeled N001"),("placement","inline"),("wrap","none"),("crop","none"),("sizing","120x80")));

        Guid inlineMath=Add("math",bodies[10],"math",D(("display",false),("presentation_mathml","<math xmlns=\"http://www.w3.org/1998/Math/MathML\"><mrow><mi>x</mi><mo>+</mo><mn>1</mn></mrow></math>")));
        Guid displayMath=Add("math",section3,"math",D(("display",true),("presentation_mathml","<math xmlns=\"http://www.w3.org/1998/Math/MathML\" display=\"block\"><mfrac><mi>a</mi><mi>b</mi></mfrac></math>")));
        byte[] tex=Encoding.UTF8.GetBytes("\\frac{a}{b}"); Guid texFacet=Id(); s.ProviderFacets[texFacet]=new(texFacet,"tex","source",displayMath,ExtensionCoverageKind.Object,ExtensionEditPolicy.Invalidate,tex,SHA256.HashData(tex),"source_exact_unmodified",false);

        // Extension coverage and policies.
        void Ext(string ns,string name,int major,int minor,byte[] payload,ExtensionCoverageKind coverage,Guid? target,string? prop,Guid? start,Guid? end,ExtensionEditPolicy policy,bool required,string? fallback=null)
        { Guid id=Id(); s.Extensions[id]=new(id,ns,name,major,minor,"bytes",payload,SHA256.HashData(payload),coverage,target,prop,start,end,policy,required,fallback); }
        Ext("https://stealtheye.ai/docseye/ext/known","known-object",1,0,Encoding.UTF8.GetBytes("known optional"),ExtensionCoverageKind.Object,bodies[0],null,null,null,ExtensionEditPolicy.Independent,false);
        Ext("urn:unknown:property","unknown-property",1,0,new byte[]{1,2,3,4},ExtensionCoverageKind.Property,bodies[1],"provider_hint",null,null,ExtensionEditPolicy.Independent,false);
        Ext("urn:unknown:required-range","required-range",1,0,new byte[]{9,8,7,6},ExtensionCoverageKind.TextInterval,bodies[2],null,b1,b2,ExtensionEditPolicy.MustUnderstandBeforeEdit,true);
        Ext("urn:unknown:topology","table-topology",1,0,new byte[]{0x51,0x52,0x53},ExtensionCoverageKind.TopologyRegion,table,null,null,null,ExtensionEditPolicy.GenericTransform,false);
        Ext("urn:unknown:newer-minor","future-optional",1,7,new byte[]{0x61,0x62},ExtensionCoverageKind.Object,fig3,null,null,null,ExtensionEditPolicy.Independent,false,"static fallback: SVG figure");

        // Explicit layout intent relations as ordinary semantic records.
        _=Add("layout_intent",section2,"layout_intent",D(("two_columns",true),("figure_wrap",true),("table_repeat_header",true),("table_split",true),("note_placement","footnote-bottom"),("language_hyphenation","en-US"),("profile_ids",new object?[]{letter,a4})));
        _=Add("layout_intent",bodies[14],"layout_intent",D(("reflow_trigger",true),("page_field_dependency",true),("keep_with_next",false)));

        // Fix the TeX facet key: the facet's dictionary key must equal its own public ID.
        foreach(var bad in s.ProviderFacets.Where(kv=>kv.Key!=kv.Value.Id).ToArray()){s.ProviderFacets.Remove(bad.Key);s.ProviderFacets[bad.Value.Id]=bad.Value;}

        var sentinels = new Dictionary<string,Guid[]>(StringComparer.Ordinal)
        {
            ["root"]=[root], ["flows"]=[main,sidebar], ["sections"]=[section1,section2], ["text_blocks"]=[bodies[0],bodies[5],bodies[10],bodies[15]],
            ["list_list_items"]=[list,item1,item3],
            ["table_rows_cells"]=[table,rows[0],rows[2],mergedCell,cells[(2,0)],cells[(2,1)],cells[(3,0)],cells[(3,1)]],
            ["retained_boundaries"]=[b1,b3,b5,b7], ["thread_comment"]=[thread1,comment1], ["suggestion"]=[suggestionInsert], ["field"]=[fieldMetadata],
            ["control"]=[controlEnum], ["note"]=[footnote], ["figure"]=[fig1], ["named_style"]=[style1]
        };
        if(sentinels.Sum(x=>x.Value.Length)!=32) throw new InvalidOperationException("sentinel cardinality drift");

        bool RenderMap(SemanticObject o)=>o.Type is "text_block" or "list_item" or "table" or "table_cell" or "figure" or "note" or "link" or "math";
        var objectManifest=s.Objects.Values.OrderBy(o=>o.ParentId).ThenBy(o=>o.Order).ThenBy(o=>Ids.Lower(o.Id),StringComparer.Ordinal)
            .Select(o=>new FixtureObjectManifest(Ids.Lower(o.Id),o.Type,o.ParentId is Guid p?Ids.Lower(p):null,Convert.ToHexString(o.Order.Bytes()).ToLowerInvariant(),o.Role,RenderMap(o))).ToArray();
        var boundaryManifest=s.Boundaries.Values.OrderBy(b=>Ids.Lower(b.Id),StringComparer.Ordinal).Select(b=>new FixtureBoundaryManifest(Ids.Lower(b.Id),Ids.Lower(b.OwnerId),b.ScalarOffset,CanonicalCbor.AffinityToken(b.Affinity),b.State.ToString().ToLowerInvariant())).ToArray();
        var rangeManifest=s.Ranges.Values.OrderBy(r=>Ids.Lower(r.Id),StringComparer.Ordinal).Select(r=>new FixtureRangeManifest(Ids.Lower(r.Id),Ids.Lower(r.StartBoundaryId),Ids.Lower(r.EndBoundaryId),r.AllowMultiInterval,r.Kind,r.State,r.EffectiveIntervals.Select(i=>new[]{Ids.Lower(i.StartBoundaryId),Ids.Lower(i.EndBoundaryId)}).ToArray())).ToArray();
        var sentinelText=sentinels.ToDictionary(k=>k.Key,v=>v.Value.Select(Ids.Lower).ToArray(),StringComparer.Ordinal);
        var counts=new Dictionary<string,int>(StringComparer.Ordinal)
        {
            ["document_root"]=s.Objects.Values.Count(o=>o.Type=="document"), ["headings"]=s.Objects.Values.Count(o=>o.Type=="text_block"&&o.Role=="heading"), ["body_paragraphs"]=s.Objects.Values.Count(o=>o.Type=="text_block"&&o.Role=="body"),
            ["retained_ranges"]=s.Ranges.Count, ["links"]=s.Objects.Values.Count(o=>o.Type=="link"), ["citations"]=s.Objects.Values.Count(o=>o.Type=="citation"), ["bibliographic_records"]=s.Objects.Values.Count(o=>o.Type=="bibliographic_record"),
            ["comment_threads"]=s.Objects.Values.Count(o=>o.Type=="comment_thread"), ["suggestions"]=s.Objects.Values.Count(o=>o.Type=="suggestion"), ["fields"]=s.Objects.Values.Count(o=>o.Type=="field"), ["controls"]=s.Objects.Values.Count(o=>o.Type=="control"),
            ["figures"]=s.Objects.Values.Count(o=>o.Type=="figure"), ["embedded_assets"]=s.Assets.Count, ["extensions"]=s.Extensions.Count, ["named_styles"]=s.Objects.Values.Count(o=>o.Type=="named_style"), ["theme_tokens"]=s.Objects.Values.Count(o=>o.Type=="theme_token"), ["direct_overrides"]=s.Objects.Values.Count(o=>o.Type=="direct_override"), ["layout_profiles"]=s.Objects.Values.Count(o=>o.Type=="layout_profile")
        };
        foreach(var kv in new Dictionary<string,int>{{"document_root",1},{"headings",5},{"body_paragraphs",18},{"retained_ranges",6},{"links",2},{"citations",3},{"bibliographic_records",2},{"comment_threads",2},{"suggestions",6},{"fields",6},{"controls",8},{"figures",3},{"embedded_assets",2},{"extensions",5},{"named_styles",3},{"theme_tokens",6},{"direct_overrides",3},{"layout_profiles",2}})
            if(counts[kv.Key]!=kv.Value) throw new InvalidOperationException($"N-001 count drift {kv.Key}: {counts[kv.Key]} != {kv.Value}");

        string rootHex=Convert.ToHexString(s.ComputeRoot()).ToLowerInvariant();
        var manifest=new N001Manifest("N-001",DndConstants.ArchitectureFreeze,$"0x{Seed:X16}",Ids.Lower(s.FamilyId),Ids.Lower(s.BranchId),Ids.Lower(s.RevisionId),rootHex,objectManifest,boundaryManifest,rangeManifest,sentinelText,
            new Dictionary<string,string>(StringComparer.Ordinal){{"raster",Convert.ToHexString(rasterDigest).ToLowerInvariant()},{"svg",Convert.ToHexString(svgDigest).ToLowerInvariant()}},counts);
        return new(s,manifest,new Dictionary<string,byte[]>(StringComparer.Ordinal){{Convert.ToHexString(rasterDigest),raster},{Convert.ToHexString(svgDigest),svg}});
    }

    public static N001Manifest Write(string dndPath,string manifestPath)
    {
        var fixture=Generate(); Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(dndPath))!); Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(manifestPath))!);
        using(var store=DndStore.Create(dndPath,fixture.State))
        {
            foreach(var pair in fixture.AssetBytes) store.InitializeEmbeddedAssetBytes(Convert.FromHexString(pair.Key),pair.Value);
            var v=store.Validate(true); if(!v.Writable) throw new InvalidOperationException("N-001 validation failed: "+v.Classification+" "+string.Join("; ",v.Diagnostics));
            if(!Convert.ToHexString(v.ComputedRoot!).Equals(fixture.Manifest.SemanticRoot,StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("N-001 root drift after persistence");
        }
        File.WriteAllText(manifestPath,JsonSerializer.Serialize(fixture.Manifest,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));
        return fixture.Manifest;
    }
}