using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DOCSeye.Core;
using DOCSeye.Providers;
using DOCSeye.Storage.Sqlite;

internal sealed record HostCallRecord(int Ordinal,string Id,string Class,long DurationMs,int RequestBytes,int ResponseBytes,string Classification);
internal sealed record MutationEffect(string RequestId,string[] OperationKinds,string[] TargetIds);
internal sealed class ManifestObjectLite{public string Id{get;set;}="";public string Type{get;set;}="";public string? ParentId{get;set;}public string? SemanticRole{get;set;}public bool RequiredRenderMapping{get;set;}}
internal sealed class ManifestLite{public string ArchitectureFreeze{get;set;}="";public List<ManifestObjectLite> Objects{get;set;}=[];public Dictionary<string,string[]> Sentinels{get;set;}=new(StringComparer.Ordinal);}

internal sealed partial class ProgramHostInvocation : IDisposable
{
    private static readonly string[] ExpectedOrder=BuildExpectedOrder();
    private readonly List<HostCallRecord> ledger=[];
    private readonly Dictionary<string,MutationEffect> mutationEffects=new(StringComparer.Ordinal);
    private readonly Dictionary<string,object> observations=new(StringComparer.Ordinal);
    private NativeDocumentSession? session;
    private SemanticState? state;
    private SemanticState? initialState;
    private RevisionHead? initialHead,t1Head,externalHead,t2Head,t3Head;
    private SemanticDelta? t1Delta,t2Delta,t3Delta;
    private SemanticTransactionBuilder? active;
    private DPlan? plan;
    private ManifestLite? manifest;
    private Guid activeExpected;
    private int activeTransaction;
    private long deltaCursor;
    private string dndPath="",manifestPath="",workDir="",evidencePath="",typstExe="",veraPdfBat="",javaHome="",figureSvg="";
    private int nodePid;
    private string invocationId="";
    private string staleClassification="",requiredClassification="";
    private LayoutRevisionRecord? plannedLayout;
    private RenderArtifact? htmlRender,pdfRender;
    private NativeDocxExportPlan? docxPlan;
    private NativeDocxExportResult? docxExport;
    private DocxValidationResult? docxValidation;
    private readonly List<string> transactionRequested=[];
    private int staleWriteCount=-1,requiredWriteCount=-1,postconditionChecksPassed;
    private int activeContentExecutionCount=0,remoteFetchCount=0,wordComCalls=0,wordApiCalls=0;

    public void Dispose()=>session?.Dispose();

    public object Invoke(string id,JsonElement p)
    {
        if(ledger.Count>=ExpectedOrder.Length)throw new InvalidOperationException("program_host_call_overflow");
        string expected=ExpectedOrder[ledger.Count];if(!string.Equals(id,expected,StringComparison.Ordinal))throw new InvalidOperationException($"program_host_call_order:{expected}!={id}");
        int requestBytes=Encoding.UTF8.GetByteCount(id)+(p.ValueKind==JsonValueKind.Undefined?0:Encoding.UTF8.GetByteCount(p.GetRawText()));var sw=Stopwatch.StartNew();
        object result=id[2] switch
        {
            'Q'=>Query(id,p),'M'=>Mutate(id,p),'T'=>Transaction(id,p),'G'=>Delta(id,p),'V'=>Verify(id,p),'R'=>Reconcile(id,p),'L'=>Layout(id,p),'O'=>Output(id,p),_=>throw new InvalidOperationException("program_host_unknown_class")
        };
        sw.Stop();string json=JsonSerializer.Serialize(result);ledger.Add(new(ledger.Count+1,id,id.Substring(2,1),sw.ElapsedMilliseconds,requestBytes,Encoding.UTF8.GetByteCount(json),ResultClassification(result)));
        if(id=="D.O-04")WriteEvidence();
        return result;
    }

    private object Query(string id,JsonElement p)=>id switch
    {
        "D.Q-01"=>Q01(p),"D.Q-02"=>Q02(),"D.Q-03"=>Q03(),"D.Q-04"=>Q04(),"D.Q-05"=>Q05(),"D.Q-06"=>Q06(),"D.Q-07"=>Q07(),"D.Q-08"=>Q08(),"D.Q-09"=>Q09(),"D.Q-10"=>Q10(),"D.Q-11"=>Q11(),"D.Q-12"=>Q12(),"D.Q-13"=>Q13(),"D.Q-14"=>Q14(),"D.Q-15"=>Q15(),"D.Q-16"=>Q16(),"D.Q-17"=>Q17(),"D.Q-18"=>Q18(),_=>throw new InvalidOperationException("unknown_query_call")
    };

    private object Q01(JsonElement p)
    {
        dndPath=Full(p,"dndPath");manifestPath=Full(p,"manifestPath");workDir=Full(p,"workDir");evidencePath=Full(p,"evidencePath");typstExe=Full(p,"typstExe");veraPdfBat=Full(p,"veraPdfBat");javaHome=Full(p,"javaHome");figureSvg=Full(p,"figureSvg");nodePid=p.GetProperty("nodePid").GetInt32();invocationId=p.GetProperty("invocationId").GetString()??throw new InvalidOperationException("invocationId required");
        Directory.CreateDirectory(workDir);manifest=JsonSerializer.Deserialize<ManifestLite>(File.ReadAllText(manifestPath))??throw new InvalidOperationException("manifest parse failed");if(manifest.ArchitectureFreeze!=DndConstants.ArchitectureFreeze)throw new InvalidOperationException("manifest_architecture_mismatch");
        session=NativeDocumentSession.Open(dndPath,true);if(!session.WriteAuthority)throw new InvalidOperationException("n001_not_writable:"+session.Validation.Classification);state=session.LoadState();initialHead=session.ReadHead();
        return new{classification="valid",writeAuthority=true,manifestArchitecture=manifest.ArchitectureFreeze,head=Head(initialHead),nodePid,invocationId};
    }
    private object Q02(){var s=S();var caps=DndConstants.DefaultRequiredCapabilities.Select(x=>new{name=x,state="supported"}).ToArray();return new{classification="supported",mode=SemanticState.ModeToken(s.Mode),nativeCapabilities=caps,providerCapabilities=new{html="supported",pdf="supported",docx="supported_with_declared_loss",word="unavailable_provider"}};}
    private object Q03(){var h=Session().ReadHead();return new{classification="inspected",familyId=Ids.Lower(h.FamilyId),branchId=Ids.Lower(h.BranchId),revisionId=Ids.Lower(h.RevisionId),semanticRoot=Hex(h.SemanticRoot),sequence=h.Sequence};}
    private object Q04(){var s=S();var headings=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="text_block"&&o.Role=="heading").OrderBy(o=>o.Order).Select(ObjView).ToArray();var sections=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="section").OrderBy(o=>o.Order).Select(ObjView).ToArray();return new{classification="inspected",headings,sections};}
    private object Q05(){var s=S();var groups=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="text_block"&&o.Data.GetValueOrDefault("text") is string).GroupBy(o=>(string)o.Data["text"]!).Where(g=>g.Count()>1).Select(g=>new{text=g.Key,ids=g.Select(x=>Ids.Lower(x.Id)).ToArray()}).ToArray();if(groups.Length==0)throw new InvalidOperationException("duplicate_sentence_fixture_missing");return new{classification="inspected",groups};}
    private object Q06(){var s=S();var tables=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="table").Select(t=>new{id=Ids.Lower(t.Id),rows=s.Objects.Values.Count(o=>!o.Retired&&o.ParentId==t.Id&&o.Type=="table_row"),columns=s.Objects.Values.Count(o=>!o.Retired&&o.ParentId==t.Id&&o.Type=="table_column"),cells=s.Objects.Values.Where(o=>!o.Retired&&o.ParentId==t.Id&&o.Type=="table_cell").Select(c=>new{id=Ids.Lower(c.Id),row=c.Data.GetValueOrDefault("row"),column=c.Data.GetValueOrDefault("column"),rowSpan=c.Data.GetValueOrDefault("row_span"),columnSpan=c.Data.GetValueOrDefault("column_span")}).ToArray()}).ToArray();return new{classification="inspected",tables};}
    private object Q07(){var s=S();var lists=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="list").Select(l=>new{id=Ids.Lower(l.Id),parentId=l.ParentId is Guid p?Ids.Lower(p):null,start=l.Data.GetValueOrDefault("start"),restart=l.Data.GetValueOrDefault("restart_intent"),items=s.Objects.Values.Where(o=>!o.Retired&&o.ParentId==l.Id&&o.Type=="list_item").OrderBy(o=>o.Order).Select(ObjView).ToArray()}).ToArray();return new{classification="inspected",lists};}
    private object Q08(){var s=S();var threads=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="comment_thread").Select(t=>new{id=Ids.Lower(t.Id),state=t.Data.GetValueOrDefault("state"),targetRange=t.Data.GetValueOrDefault("target_range_id"),comments=s.Objects.Values.Where(o=>!o.Retired&&o.ParentId==t.Id&&o.Type=="comment").OrderBy(o=>o.Order).Select(ObjView).ToArray()}).ToArray();return new{classification="inspected",threads};}
    private object Q09(){var s=S();var suggestions=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="suggestion").OrderBy(o=>o.Order).Select(o=>new{id=Ids.Lower(o.Id),kind=o.Data.GetValueOrDefault("kind"),state=o.Data.GetValueOrDefault("state"),target=o.Data.GetValueOrDefault("target_id")}).ToArray();if(suggestions.Length!=6)throw new InvalidOperationException("expected_six_suggestions");return new{classification="inspected",suggestions};}
    private object Q10(){var s=S();return new{classification="inspected",fields=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="field").Select(o=>new{id=Ids.Lower(o.Id),@class=o.Data.GetValueOrDefault("class"),source=o.Data.GetValueOrDefault("source"),policy=o.Data.GetValueOrDefault("evaluation_policy"),result=o.Data.GetValueOrDefault("cached_result"),staleness=o.Data.GetValueOrDefault("staleness")}).ToArray()};}
    private object Q11(){var s=S();var controls=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="control").Select(o=>new{id=Ids.Lower(o.Id),type=o.Data.GetValueOrDefault("control_type"),value=o.Data.GetValueOrDefault("value"),constraints=o.Data.GetValueOrDefault("constraints")}).ToArray();if(controls.Length!=8)throw new InvalidOperationException("expected_eight_controls");return new{classification="inspected",controls};}
    private object Q12(){var s=S();return new{classification="inspected",notes=s.Objects.Values.Where(o=>!o.Retired&&o.Type is "note" or "note_reference").Select(ObjView).ToArray(),anchors=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="named_anchor").Select(ObjView).ToArray(),references=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="cross_reference").Select(ObjView).ToArray(),citations=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="citation").Select(ObjView).ToArray()};}
    private object Q13(){var s=S();return new{classification="inspected",figures=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="figure").Select(o=>new{id=Ids.Lower(o.Id),asset=AssetText(o.Data.GetValueOrDefault("asset_digest")),caption=o.Data.GetValueOrDefault("caption"),alt=o.Data.GetValueOrDefault("alt_text")}).ToArray(),assets=s.Assets.Values.Select(a=>new{digest=Hex(a.Digest),a.Length,a.State}).ToArray(),math=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="math").Select(ObjView).ToArray(),facets=s.ProviderFacets.Values.Select(f=>new{id=Ids.Lower(f.Id),f.Provider,f.Kind,target=f.TargetId is Guid t?Ids.Lower(t):null,f.Alignment}).ToArray()};}
    private object Q14(){var s=S();var target=s.Objects.Values.First(o=>!o.Retired&&o.Type=="text_block"&&o.Role=="body");var direct=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="direct_override"&&o.ParentId==target.Id).Select(ObjView).ToArray();var named=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="named_style").Select(ObjView).ToArray();return new{classification="inspected",target=Ids.Lower(target.Id),semanticRole=target.Role,namedStyles=named,directOverrides=direct,provenance="role + named-style graph + direct override"};}
    private object Q15(){var s=S();return new{classification="inspected",profiles=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="layout_profile").Select(ObjView).ToArray(),inputs=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="layout_intent").Select(ObjView).ToArray()};}
    private object Q16(){var s=S();var assessed=ExtensionCapabilityPolicy.Assess(s);return new{classification="inspected",extensions=s.Extensions.Values.Select(e=>new{id=Ids.Lower(e.ExtensionId),coverage=CoverageToken(e.CoverageKind),policy=PolicyToken(e.EditPolicy),e.Required,target=e.TargetId is Guid t?Ids.Lower(t):null,digestValid=e.DigestValid}).ToArray(),capabilities=assessed};}
    private object Q17(){var s=S();var arabic=s.Objects.Values.First(o=>!o.Retired&&o.Type=="text_block"&&o.Data.GetValueOrDefault("text") is string t&&t.Contains("Arabic logical text",StringComparison.Ordinal));var decomposed=s.Objects.Values.First(o=>!o.Retired&&o.Type=="text_block"&&o.Data.GetValueOrDefault("text") is string t&&t.Contains("Decomposed sequence",StringComparison.Ordinal));string at=(string)arabic.Data["text"]!,dt=(string)decomposed.Data["text"]!;return new{classification="resolved",revisionId=Ids.Lower(Session().ReadHead().RevisionId),arabic=new{id=Ids.Lower(arabic.Id),scalarEnd=UnicodeText.ScalarCount(at),graphemeBoundaries=UnicodeText.GraphemeScalarBoundaries(at)},decomposed=new{id=Ids.Lower(decomposed.Id),scalarCount=UnicodeText.ScalarCount(dt),graphemeBoundaries=UnicodeText.GraphemeScalarBoundaries(dt)}};}
    private object Q18()
    {
        var s=S();plan=DPlan.Discover(s);initialState=Branching.Clone(s);initialHead??=Session().ReadHead();var required=RequiredFixtureIds();
        return new{classification="planned",transactionTargets=plan.Describe(),requiredRenderFixtureIds=required.Select(Ids.Lower).ToArray(),externalAction=new{command="move-object",targetId=Ids.Lower(plan.ExternalTarget),index=0},retainedRanges=new{firstOwner=Ids.Lower(plan.AnchorBody),firstStart=5,firstEnd=24,secondStart=5,secondEnd=34},exactTransactionCount=3,exactMutationCount=48};
    }

    private SemanticState S()=>state??throw new InvalidOperationException("document_not_open");
    private NativeDocumentSession Session()=>session??throw new InvalidOperationException("document_not_open");
    private DPlan P()=>plan??throw new InvalidOperationException("targets_not_planned");
    private void Refresh()=>state=Session().LoadState();
    private static object ObjView(SemanticObject o)=>new{id=Ids.Lower(o.Id),o.Type,parentId=o.ParentId is Guid p?Ids.Lower(p):null,o.Role,data=o.Data};
    private static object Head(RevisionHead h)=>new{familyId=Ids.Lower(h.FamilyId),branchId=Ids.Lower(h.BranchId),revisionId=Ids.Lower(h.RevisionId),sequence=h.Sequence,semanticRoot=Hex(h.SemanticRoot),parents=h.Parents.Select(Ids.Lower).ToArray(),h.Mode};
    private static string Full(JsonElement p,string name)=>Path.GetFullPath(p.GetProperty(name).GetString()??throw new InvalidOperationException(name+" required"));
    private static string Hex(byte[] b)=>Convert.ToHexString(b).ToLowerInvariant();
    private static string AssetText(object? v)=>v switch{byte[] b=>Hex(b),string s=>s,_=>v?.ToString()??""};
    private static string CoverageToken(ExtensionCoverageKind kind)=>kind switch{ExtensionCoverageKind.Object=>"object",ExtensionCoverageKind.Property=>"property",ExtensionCoverageKind.Subtree=>"subtree",ExtensionCoverageKind.TextInterval=>"text_interval",ExtensionCoverageKind.Relation=>"relation",ExtensionCoverageKind.TopologyRegion=>"topology_region",ExtensionCoverageKind.LayoutProfile=>"layout_profile",ExtensionCoverageKind.DocumentGlobal=>"document_global",_=>throw new ArgumentOutOfRangeException(nameof(kind))};
    private static string PolicyToken(ExtensionEditPolicy policy)=>policy switch{ExtensionEditPolicy.Independent=>"independent",ExtensionEditPolicy.MoveWithTarget=>"move_with_target",ExtensionEditPolicy.GenericTransform=>"generic_transform",ExtensionEditPolicy.Invalidate=>"invalidate",ExtensionEditPolicy.MustUnderstandBeforeEdit=>"must_understand_before_edit",_=>throw new ArgumentOutOfRangeException(nameof(policy))};    private static string ResultClassification(object x){var prop=x.GetType().GetProperty("classification")??x.GetType().GetProperty("Classification");return prop?.GetValue(x)?.ToString()??"ok";}
    private Guid[] RequiredFixtureIds()=>manifest!.Objects.Where(o=>o.RequiredRenderMapping).Select(o=>Guid.Parse(o.Id)).ToArray();
    private static string HashFile(string path){using var fs=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);return Hex(SHA256.HashData(fs));}
    private static string[] BuildExpectedOrder()
    {
        var ids=new List<string>();for(int i=1;i<=18;i++)ids.Add($"D.Q-{i:00}");ids.Add("D.T-01");for(int i=1;i<=16;i++)ids.Add($"D.M-{i:00}");ids.Add("D.T-02");ids.AddRange(["D.G-01","D.G-02","D.V-01","D.V-02","D.V-03","D.R-01","D.R-02","D.R-03","D.R-04","D.T-03"]);for(int i=17;i<=32;i++)ids.Add($"D.M-{i:00}");ids.Add("D.T-04");ids.AddRange(["D.G-03","D.G-04","D.V-04","D.T-05"]);for(int i=33;i<=48;i++)ids.Add($"D.M-{i:00}");ids.Add("D.T-06");ids.AddRange(["D.G-05","D.G-06","D.V-05","D.V-06","D.L-01","D.L-02","D.L-03","D.L-04","D.O-01","D.O-02","D.O-03","D.O-04"]);if(ids.Count!=96)throw new InvalidOperationException("frozen_call_arithmetic_drift");return ids.ToArray();
    }

    private sealed class DPlan
    {
        public required Guid Root,MainFlow,Section2,FirstDuplicate,SecondDuplicate,ArabicBody,DecomposedBody,DecomposedNamedRange,AnchorBody,NestedList,ExistingNestedItem,MainTable,NestedTable,OriginalMergedCell,ThemeToken,NamedStyle,SuggestionInsert,SuggestionStyle,FieldMetadata,ControlEnum,ControlRepeating,ControlReference,EndnoteReference,CrossReference,NamedAnchor1,NamedAnchor2,Bibliographic,DisplayMath,TexFacet,HeaderText,GenericTopologyExtension,RequiredIntervalExtension,ExternalTarget,SafeBody,RasterFigure;
        public required string RasterDigest,SvgDigest;
        public readonly Dictionary<string,Guid> Created=new(StringComparer.Ordinal);
        public static DPlan Discover(SemanticState s)
        {
            Guid Obj(Func<SemanticObject,bool> f)=>s.Objects.Values.First(o=>!o.Retired&&f(o)).Id;SemanticObject O(Func<SemanticObject,bool> f)=>s.Objects.Values.First(o=>!o.Retired&&f(o));
            var duplicates=s.Objects.Values.Where(o=>!o.Retired&&o.Type=="text_block"&&Equals(o.Data.GetValueOrDefault("text"),"Identity must never follow duplicate text.")).OrderBy(o=>Ids.Lower(o.Id),StringComparer.Ordinal).ToArray();if(duplicates.Length<2)throw new InvalidOperationException("duplicate target discovery failed");
            var mainTable=O(o=>o.Type=="table"&&Convert.ToInt32(o.Data.GetValueOrDefault("rows")??0)==4);var nestedTable=O(o=>o.Type=="table"&&o.Id!=mainTable.Id&&o.ParentId is Guid p&&s.Objects.TryGetValue(p,out var parent)&&parent.Type=="table_cell");var fig1=O(o=>o.Type=="figure"&&Equals(o.Data.GetValueOrDefault("caption"),"Shared raster figure A"));var fig3=O(o=>o.Type=="figure"&&Equals(o.Data.GetValueOrDefault("caption"),"SVG semantic figure"));var display=O(o=>o.Type=="math"&&Convert.ToBoolean(o.Data.GetValueOrDefault("display")??false));
            return new DPlan{
                Root=Obj(o=>o.Type=="document"),MainFlow=Obj(o=>o.Type=="flow"&&o.Role=="main"),Section2=Obj(o=>o.Type=="section"&&Convert.ToInt32(o.Data.GetValueOrDefault("columns")??0)==2),FirstDuplicate=duplicates[0].Id,SecondDuplicate=duplicates[1].Id,ArabicBody=Obj(o=>o.Type=="text_block"&&o.Data.GetValueOrDefault("text") is string t&&t.Contains("Arabic logical text",StringComparison.Ordinal)),DecomposedBody=Obj(o=>o.Type=="text_block"&&o.Data.GetValueOrDefault("text") is string t&&t.Contains("Decomposed sequence",StringComparison.Ordinal)),AnchorBody=Obj(o=>o.Type=="text_block"&&o.Data.GetValueOrDefault("text") is string t&&t.Contains("An early edit here",StringComparison.Ordinal)),NestedList=Obj(o=>o.Type=="list"&&o.ParentId is Guid p&&s.Objects.TryGetValue(p,out var parent)&&parent.Type=="list_item"),ExistingNestedItem=Obj(o=>o.Type=="list_item"&&o.ParentId is Guid p&&s.Objects.TryGetValue(p,out var parent)&&parent.Type=="list"&&parent.ParentId is Guid pp&&s.Objects.TryGetValue(pp,out var grand)&&grand.Type=="list_item"),DecomposedNamedRange=s.Ranges.Values.First(r=>r.Kind=="named_range"&&s.Boundaries[r.StartBoundaryId].OwnerId==Obj(o=>o.Type=="text_block"&&o.Data.GetValueOrDefault("text") is string t&&t.Contains("Decomposed sequence",StringComparison.Ordinal))).Id,MainTable=mainTable.Id,NestedTable=nestedTable.Id,OriginalMergedCell=Obj(o=>o.Type=="table_cell"&&o.ParentId==mainTable.Id&&Convert.ToInt32(o.Data.GetValueOrDefault("row_span")??1)==2&&Convert.ToInt32(o.Data.GetValueOrDefault("column_span")??1)==2),ThemeToken=Obj(o=>o.Type=="theme_token"&&Equals(o.Data.GetValueOrDefault("name"),"color.accent")),NamedStyle=Obj(o=>o.Type=="named_style"&&Equals(o.Data.GetValueOrDefault("name"),"Body Native")),SuggestionInsert=Obj(o=>o.Type=="suggestion"&&Equals(o.Data.GetValueOrDefault("kind"),"insert")),SuggestionStyle=Obj(o=>o.Type=="suggestion"&&Equals(o.Data.GetValueOrDefault("kind"),"style_property")),FieldMetadata=Obj(o=>o.Type=="field"&&Equals(o.Data.GetValueOrDefault("class"),"metadata")),ControlEnum=Obj(o=>o.Type=="control"&&Equals(o.Data.GetValueOrDefault("control_type"),"enum")),ControlRepeating=Obj(o=>o.Type=="control"&&Equals(o.Data.GetValueOrDefault("control_type"),"repeating_group")),ControlReference=Obj(o=>o.Type=="control"&&Equals(o.Data.GetValueOrDefault("control_type"),"object_reference_selector")),EndnoteReference=Obj(o=>o.Type=="note_reference"&&Equals(o.Data.GetValueOrDefault("occurrence_kind"),"endnote")),CrossReference=Obj(o=>o.Type=="cross_reference"&&Equals(o.Data.GetValueOrDefault("display"),"Identity anchor again")),NamedAnchor1=Obj(o=>o.Type=="named_anchor"&&Equals(o.Data.GetValueOrDefault("name"),"identity-anchor")),NamedAnchor2=Obj(o=>o.Type=="named_anchor"&&Equals(o.Data.GetValueOrDefault("name"),"render-anchor")),Bibliographic=Obj(o=>o.Type=="bibliographic_record"&&Equals(o.Data.GetValueOrDefault("key"),"RFC9562")),DisplayMath=display.Id,TexFacet=s.ProviderFacets.Values.First(f=>f.TargetId==display.Id&&f.Provider=="tex").Id,HeaderText=Obj(o=>o.Type=="text_block"&&o.ParentId is Guid p&&s.Objects.TryGetValue(p,out var parent)&&parent.Type=="flow"&&parent.Role=="header"),GenericTopologyExtension=s.Extensions.Values.First(e=>e.CoverageKind==ExtensionCoverageKind.TopologyRegion&&e.TargetId==mainTable.Id).ExtensionId,RequiredIntervalExtension=s.Extensions.Values.First(e=>e.Required&&e.CoverageKind==ExtensionCoverageKind.TextInterval).ExtensionId,ExternalTarget=Obj(o=>o.Type=="text_block"&&o.Data.GetValueOrDefault("text") is string t&&t.Contains("Mixed bidi",StringComparison.Ordinal)),SafeBody=Obj(o=>o.Type=="text_block"&&o.Role=="body"&&o.Data.GetValueOrDefault("text") is string t&&t.Contains("Keep-together paragraph",StringComparison.Ordinal)),RasterFigure=fig1.Id,RasterDigest=AssetText(fig1.Data.GetValueOrDefault("asset_digest")),SvgDigest=AssetText(fig3.Data.GetValueOrDefault("asset_digest"))};
        }
        public object Describe()=>new{structure=new{FirstDuplicate=Ids.Lower(FirstDuplicate),SecondDuplicate=Ids.Lower(SecondDuplicate),MainFlow=Ids.Lower(MainFlow)},text=new{ArabicBody=Ids.Lower(ArabicBody),DecomposedBody=Ids.Lower(DecomposedBody)},anchors=new{AnchorBody=Ids.Lower(AnchorBody)},table=new{MainTable=Ids.Lower(MainTable),OriginalMergedCell=Ids.Lower(OriginalMergedCell)},extensions=new{GenericTopologyExtension=Ids.Lower(GenericTopologyExtension),RequiredIntervalExtension=Ids.Lower(RequiredIntervalExtension)}};
    }
}
