using System.Security.Cryptography;
using System.Text;
using DOCSeye.Core;

internal sealed partial class ProgramHostInvocation
{
    private object Mutate(string id,System.Text.Json.JsonElement p)
    {
        if(active is null)throw new InvalidOperationException("mutation_without_transaction");if(p.ValueKind==System.Text.Json.JsonValueKind.Object&&p.TryGetProperty("callId",out var supplied)&&supplied.GetString()!=id)throw new InvalidOperationException("mutation_call_id_mismatch");
        int before=active.Operations.Count;switch(id)
        {
            case "D.M-01": M01();break;case "D.M-02": M02();break;case "D.M-03": M03();break;case "D.M-04": M04();break;case "D.M-05": M05();break;case "D.M-06": M06();break;case "D.M-07": M07();break;case "D.M-08": M08();break;
            case "D.M-09": M09();break;case "D.M-10": M10();break;case "D.M-11": M11();break;case "D.M-12": M12();break;case "D.M-13": M13();break;case "D.M-14": M14();break;case "D.M-15": M15();break;case "D.M-16": M16();break;
            case "D.M-17": M17();break;case "D.M-18": M18();break;case "D.M-19": M19();break;case "D.M-20": M20();break;case "D.M-21": M21();break;case "D.M-22": M22();break;case "D.M-23": M23();break;case "D.M-24": M24();break;
            case "D.M-25": M25();break;case "D.M-26": M26();break;case "D.M-27": M27();break;case "D.M-28": M28();break;case "D.M-29": M29();break;case "D.M-30": M30();break;case "D.M-31": M31();break;case "D.M-32": M32();break;
            case "D.M-33": M33();break;case "D.M-34": M34();break;case "D.M-35": M35();break;case "D.M-36": M36();break;case "D.M-37": M37();break;case "D.M-38": M38();break;case "D.M-39": M39();break;case "D.M-40": M40();break;
            case "D.M-41": M41();break;case "D.M-42": M42();break;case "D.M-43": M43();break;case "D.M-44": M44();break;case "D.M-45": M45();break;case "D.M-46": M46();break;case "D.M-47": M47();break;case "D.M-48": M48();break;
            default:throw new InvalidOperationException("unknown_mutation_call");
        }
        active.RecordRequestedMutation(id);transactionRequested.Add(id);var ops=active.Operations.Skip(before).ToArray();mutationEffects[id]=new(id,ops.Select(x=>x.Kind).ToArray(),ops.Where(x=>x.TargetId is not null).Select(x=>Ids.Lower(x.TargetId!.Value)).ToArray());return new{classification="mutated",requestId=id,transaction=activeTransaction,operationKinds=ops.Select(x=>x.Kind).ToArray(),targetIds=ops.Where(x=>x.TargetId is not null).Select(x=>Ids.Lower(x.TargetId!.Value)).ToArray()};
    }
    private SemanticTransactionBuilder B()=>active??throw new InvalidOperationException("transaction_not_active");
    private DPlan DP()=>P();
    private void Created(string key,Guid id)=>DP().Created[key]=id;
    private Guid C(string key)=>DP().Created.TryGetValue(key,out var id)?id:throw new InvalidOperationException("created_target_missing:"+key);
    private Dictionary<string,object?> D(params (string,object?)[] items){var d=new Dictionary<string,object?>(StringComparer.Ordinal);foreach(var x in items)d[x.Item1]=x.Item2;return d;}
    private void TransformTopology(string id){var p=DP();B().TransformExtension(p.GenericTopologyExtension,Encoding.UTF8.GetBytes("DOCSeye D topology transform "+id));}

    private void M01(){var p=DP();Created("appendix",B().CreateObject("section",p.MainFlow,"section",D(("columns",1),("name","Appendix D"),("language","en-US"))));}
    private void M02(){var p=DP();string text=(string)B().State.Objects[p.ArabicBody].Data["text"]!;B().InsertText(p.ArabicBody,UnicodeText.ScalarCount(text),"\u0020\u0641\u0642\u0631\u0629\u0020\u0639\u0631\u0628\u064a\u0629\u0020\u0625\u0636\u0627\u0641\u064a\u0629\u0020\u0645\u062d\u062f\u062f\u0629\u0020\u0627\u0644\u062d\u062f\u0648\u062f\u002e",true);}
    private void M03(){var p=DP();Created("range1",B().RetainRange(p.AnchorBody,5,24,"comment",EdgeAffinity.IncludeAtEdge,EdgeAffinity.ExcludeAtEdge,true));}
    private void M04(){var p=DP();Guid item=B().InsertListItem(p.NestedList,1,["D nested item first block.","D nested item second block."],1,2);Created("nestedItem",item);var blocks=B().State.Objects.Values.Where(o=>!o.Retired&&o.ParentId==item&&o.Type=="text_block").OrderBy(o=>o.Order).ToArray();Created("mergeBlockLeft",blocks[0].Id);Created("mergeBlockRight",blocks[1].Id);}
    private void M05(){var p=DP();TransformTopology("M-05");var row=B().InsertTableRow(p.MainTable,4,["D-row-0","D-row-1","D-row-2","D-row-3"]);Created("insertedRow",row.Row);for(int i=0;i<row.Cells.Count;i++)Created("insertedCell"+i,row.Cells[i]);}
    private void M06(){B().SetObjectProperty(DP().ThemeToken,"value","#3157A4",true,"theme_token_update");}
    private void M07(){var p=DP();Guid thread=B().CreateObject("comment_thread",p.Root,"comment_thread",D(("state","open"),("target_range_id",C("range1"))));Guid root=B().CreateObject("comment",thread,"comment",D(("author","program-host"),("timestamp","2026-08-09T00:00:00Z"),("body","D transaction one thread root"),("target_range_id",C("range1"))));Created("thread",thread);Created("threadRoot",root);}
    private void M08(){var p=DP();Created("replaceSuggestion",B().CreateObject("suggestion",p.Root,"suggestion",D(("kind","replace"),("state","active"),("target_id",p.AnchorBody),("range_id",C("range1")))));}
    private void M09(){var p=DP();Created("countField",B().CreateObject("field",p.Root,"field",D(("class","bounded_count"),("source","body.text_blocks"),("evaluation_policy","manual"),("cached_result",null),("staleness","stale"),("bound",1000))));}
    private void M10(){B().SetObjectProperty(DP().ControlEnum,"value","review",false,"control_set_enum");}
    private void M11(){var p=DP();Guid note=B().CreateObject("note",p.Root,"footnote",D(("note_kind","footnote")));Guid flow=B().CreateObject("flow",note,"note_body",D(("kind","note_body")));Guid text=B().CreateObject("text_block",flow,"note_body",D(("text","Program Host added footnote body.")));Guid reference=B().CreateObject("note_reference",p.SafeBody,"note_reference",D(("note_id",note),("occurrence_kind","footnote")));Created("newFootnote",note);Created("newFootFlow",flow);Created("newFootText",text);Created("newFootRef",reference);}
    private void M12(){var p=DP();Created("newLink",B().CreateObject("link",p.SafeBody,"link",D(("href","https://example.invalid/docseye/d"),("label","Program Host link"))));}
    private void M13(){var p=DP();Created("newFigure",B().CreateObject("figure",p.Section2,"figure",D(("asset_digest",Convert.FromHexString(p.RasterDigest)),("caption","D shared raster figure"),("alt_text","Shared raster from native asset"),("placement","anchored"),("wrap","square"))));}
    private void M14(){var p=DP();Created("newMath",B().CreateObject("math",p.SafeBody,"math",D(("display",false),("presentation_mathml","<math xmlns=\"http://www.w3.org/1998/Math/MathML\"><mi>d</mi><mo>+</mo><mn>1</mn></math>"))));}
    private void M15(){B().SetObjectProperty(DP().Section2,"margin","0.82in",true,"layout_margin");}
    private void M16(){var p=DP();byte[] payload=Encoding.UTF8.GetBytes("D known optional move-with-target extension");Guid id=Ids.NewV4();B().AddExtension(new(id,"urn:docseye:d:known","move-with-target",1,0,"bytes",payload,SHA256.HashData(payload),ExtensionCoverageKind.Object,p.FirstDuplicate,null,null,null,ExtensionEditPolicy.MoveWithTarget,false,null));Created("movingExtension",id);}

    private void M17(){B().MoveObject(DP().SecondDuplicate,C("appendix"),0);}
    private void M18(){var p=DP();string text=(string)B().State.Objects[p.SecondDuplicate].Data["text"]!;B().ReplaceText(p.SecondDuplicate,0,UnicodeText.ScalarCount(text),"Identity must never follow duplicate text - second instance replaced.",true);}
    private void M19(){var p=DP();Created("range2",B().RetainRange(p.AnchorBody,5,34,"style",EdgeAffinity.ExcludeAtEdge,EdgeAffinity.IncludeAtEdge,true));}
    private void M20(){B().SetListRestart(DP().NestedList,7,"explicit");}
    private void M21(){var p=DP();TransformTopology("M-21");Created("mergedInsertedCell",B().MergeTableCells(p.MainTable,[C("insertedCell0"),C("insertedCell1")],4,0,1,2,"D merged inserted row region"));}
    private void M22(){var p=DP();B().SetObjectProperty(p.FirstDuplicate,"style_id",p.NamedStyle,false,"apply_named_style");}
    private void M23(){Created("reply",B().CreateObject("comment",C("thread"),"comment",D(("author","program-host-reply"),("timestamp","2026-08-09T00:01:00Z"),("body","D transaction two reply"),("reply_to",C("threadRoot")))));}
    private void M24(){B().ResolveSuggestion(DP().SuggestionInsert,"accept");}
    private void M25(){B().SetObjectProperty(C("countField"),"evaluation_policy","on_semantic_commit",false,"field_policy");}
    private void M26(){var p=DP();var o=B().State.Objects[p.ControlRepeating];var current=(o.Data.GetValueOrDefault("value") as object?[])??[];B().SetObjectProperty(p.ControlRepeating,"value",current.Concat(new object?[]{"three"}).ToArray(),false,"control_append_repeating_item");}
    private void M27(){B().MoveObject(DP().EndnoteReference,DP().AnchorBody,0);}
    private void M28(){var p=DP();Created("newCitation",B().CreateObject("citation",p.SafeBody,"citation",D(("record_id",p.Bibliographic),("locator","D.1"))));}
    private void M29(){var p=DP();B().SetObjectProperty(C("newFigure"),"asset_digest",Convert.FromHexString(p.SvgDigest),true,"figure_replace_asset");}
    private void M30(){B().SetObjectProperty(DP().DisplayMath,"presentation_mathml","<math xmlns=\"http://www.w3.org/1998/Math/MathML\" display=\"block\"><msup><mi>d</mi><mn>2</mn></msup></math>",true,"math_semantics_update");}
    private void M31(){var p=DP();Created("pageBreak",B().CreateObject("page_break",p.Section2,"page_break",D(("kind","page"),("explicit",true))));}
    private void M32(){B().MoveExtensionWithOwner(C("movingExtension"),DP().SecondDuplicate);}

    private void M33(){Created("mergedText",B().MergeText(C("mergeBlockLeft"),C("mergeBlockRight")));}
    private void M34(){var p=DP();var r=B().State.Ranges[p.DecomposedNamedRange];var sb=B().State.Boundaries[r.StartBoundaryId];var eb=B().State.Boundaries[r.EndBoundaryId];int start=Math.Min(sb.ScalarOffset,eb.ScalarOffset),end=Math.Max(sb.ScalarOffset,eb.ScalarOffset);B().DeleteText(p.DecomposedBody,start,end-start,true);}
    private void M35(){B().ReleaseRange(C("range2"));}
    private void M36(){B().ReorderObject(C("nestedItem"),0);}
    private void M37(){var p=DP();TransformTopology("M-37");var cells=B().SplitTableCell(p.MainTable,p.OriginalMergedCell,2,2);for(int i=0;i<cells.Count;i++)Created("splitCell"+i,cells[i]);}
    private void M38(){var p=DP();Created("directOverride",B().CreateObject("direct_override",p.FirstDuplicate,"direct_override",D(("property","emphasis"),("value","strong"))));}
    private void M39(){B().SetObjectProperty(C("thread"),"state","resolved",false,"comment_thread_resolve");}
    private void M40(){B().ResolveSuggestion(DP().SuggestionStyle,"reject");}
    private void M41(){var p=DP();var o=B().State.Objects[p.FieldMetadata];var data=new Dictionary<string,object?>(o.Data,StringComparer.Ordinal){["cached_result"]="N-001 Native Semantic Authority Fixture",["staleness"]="fresh",["evaluation_inputs"]=new Dictionary<string,object?>(StringComparer.Ordinal){{"document.title","N-001 Native Semantic Authority Fixture"},{"source_revision",activeExpected}}};B().SetObjectData(p.FieldMetadata,data,false,"field_evaluate_metadata");}
    private void M42(){B().SetObjectProperty(DP().ControlReference,"value",DP().NamedAnchor2,false,"control_set_object_reference");}
    private void M43(){B().SetObjectProperty(DP().CrossReference,"target_id",DP().NamedAnchor2,false,"cross_reference_retarget");}
    private void M44(){B().SetObjectProperty(C("newCitation"),"locator","D.2 exact",false,"citation_locator_update");}
    private void M45(){var id=C("newFigure");var o=B().State.Objects[id];var data=new Dictionary<string,object?>(o.Data,StringComparer.Ordinal){["caption"]="D final figure caption",["alt_text"]="D final accessible figure description"};B().SetObjectData(id,data,true,"figure_caption_alt_update");}
    private void M46(){B().UpdateProviderFacet(DP().TexFacet,Encoding.UTF8.GetBytes("\\frac{d^2}{dx^2}"),"source_updated_for_final_math");}
    private void M47(){var p=DP();var o=B().State.Objects[p.HeaderText];var data=new Dictionary<string,object?>(o.Data,StringComparer.Ordinal){["text"]="DOCSeye D final header",["numbering_intent"]="continue_arabic"};B().SetObjectData(p.HeaderText,data,true,"header_content_numbering_intent");}
    private void M48(){var p=DP();Created("copiedTopologyExtension",B().CopyTransformableExtension(p.GenericTopologyExtension,new Dictionary<Guid,Guid>{{p.MainTable,p.NestedTable}}));}
}
