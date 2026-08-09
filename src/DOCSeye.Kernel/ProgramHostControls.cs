using System.Text.Json;
using DOCSeye.Core;
using DOCSeye.Storage.Sqlite;

internal sealed partial class ProgramHostInvocation
{
    private object Transaction(string id,JsonElement p)=>id switch
    {
        "D.T-01"=>Begin(1),"D.T-02"=>Commit(1),"D.T-03"=>Begin(2),"D.T-04"=>Commit(2),"D.T-05"=>Begin(3),"D.T-06"=>Commit(3),_=>throw new InvalidOperationException("unknown_transaction_call")
    };
    private object Begin(int number)
    {
        if(active is not null)throw new InvalidOperationException("nested_transaction");activeTransaction=number;activeExpected=Session().ReadHead().RevisionId;active=Session().BeginTransaction();transactionRequested.Clear();return new{classification="begun",transaction=number,expectedRevisionId=Ids.Lower(activeExpected)};
    }
    private object Commit(int number)
    {
        if(active is null||activeTransaction!=number)throw new InvalidOperationException("transaction_scope_mismatch");int start=(number-1)*16+1;string[] expected=Enumerable.Range(start,16).Select(i=>$"D.M-{i:00}").ToArray();if(!active.RequestedMutationIds.SequenceEqual(expected,StringComparer.Ordinal))throw new InvalidOperationException("transaction_mutation_manifest_mismatch");
        var result=Session().CommitTransaction(active,activeExpected,$"D-T{number}");if(!result.Success||result.Head is null||result.Delta is null)throw new InvalidOperationException($"transaction_{number}_failed:{result.Classification}:{string.Join('|',result.Diagnostics)}");
        if(number==1){t1Head=result.Head;t1Delta=result.Delta;}else if(number==2){t2Head=result.Head;t2Delta=result.Delta;}else{t3Head=result.Head;t3Delta=result.Delta;}
        active=null;activeTransaction=0;Refresh();return new{classification="committed",transaction=number,head=Head(result.Head),requestedMutations=expected,deltaBytes=result.Delta.ExactBytes.Length,operationCount=result.Delta.OperationKinds.Count};
    }

    private object Delta(string id,JsonElement p)=>id switch
    {
        "D.G-01"=>ReadTransactionDelta(1,t1Head??throw new InvalidOperationException("t1 head missing")),"D.G-02"=>Ack(t1Head??throw new InvalidOperationException("t1 head missing"),1),
        "D.G-03"=>ReadTransactionDelta(2,t2Head??throw new InvalidOperationException("t2 head missing")),"D.G-04"=>Ack(t2Head??throw new InvalidOperationException("t2 head missing"),2),
        "D.G-05"=>ReadTransactionDelta(3,t3Head??throw new InvalidOperationException("t3 head missing")),"D.G-06"=>Ack(t3Head??throw new InvalidOperationException("t3 head missing"),3),_=>throw new InvalidOperationException("unknown_delta_call")
    };
    private object ReadTransactionDelta(int tx,RevisionHead expectedHead)
    {
        var read=Session().ReadTypedDeltas(deltaCursor);if(read.ResyncRequired)throw new InvalidOperationException("unexpected_delta_gap");var semantic=read.Deltas.Where(d=>d.Kind=="semantic_transaction").ToArray();if(semantic.Length!=1)throw new InvalidOperationException($"transaction_{tx}_delta_count:{semantic.Length}");var requested=semantic[0].RequestedMutationIds.ToArray();string[] expected=Enumerable.Range((tx-1)*16+1,16).Select(i=>$"D.M-{i:00}").ToArray();if(!requested.SequenceEqual(expected,StringComparer.Ordinal))throw new InvalidOperationException($"transaction_{tx}_delta_requested_manifest_mismatch");if(semantic[0].ToRevisionId!=expectedHead.RevisionId)throw new InvalidOperationException("delta_head_mismatch");observations[$"G{tx}"]=new{requested,bytes=semantic[0].ExactBytes.Length,cursor=deltaCursor,headSequence=read.HeadSequence};return new{classification="delta_read",transaction=tx,cursor=deltaCursor,headSequence=read.HeadSequence,requestedMutations=requested,bytes=semantic[0].ExactBytes.Length};
    }
    private object Ack(RevisionHead head,int tx){Session().AcknowledgeDeltasThrough(head.Sequence);deltaCursor=head.Sequence;return new{classification="delta_acknowledged",transaction=tx,throughSequence=head.Sequence};}

    private object Reconcile(string id,JsonElement p)=>id switch
    {
        "D.R-01"=>R01(),"D.R-02"=>R02(),"D.R-03"=>R03(),"D.R-04"=>R04(),_=>throw new InvalidOperationException("unknown_reconcile_call")
    };
    private object R01(){var h=Session().ReadHead();if(t1Head is null||h.RevisionId==t1Head.RevisionId)throw new InvalidOperationException("external_head_not_detected");externalHead=h;return new{classification="external_head_detected",head=Head(h),priorValidatedRevision=Ids.Lower(t1Head.RevisionId)};}
    private object R02(){if(externalHead is null||t1Head is null)throw new InvalidOperationException("external detection missing");var v=Session().ReconcileExternal(true);var h=Session().ReadHead();if(!v.Writable||h.RevisionId!=externalHead.RevisionId||h.Parents.Count!=1||h.Parents[0]!=t1Head.RevisionId)throw new InvalidOperationException("external_not_exact_descendant");return new{classification="exact_descendant",validation=v.Classification,staleViewsInvalidated=true,from=Ids.Lower(t1Head.RevisionId),to=Ids.Lower(h.RevisionId)};}
    private object R03()
    {
        if(t1Head is null||externalHead is null)throw new InvalidOperationException("external heads missing");var read=Session().ReadTypedDeltas(t1Head.Sequence);if(read.ResyncRequired||read.Deltas.Count!=1||read.Deltas[0].Kind!="external_move")throw new InvalidOperationException("external_delta_reconciliation_mismatch");string runtime=Path.Combine(workDir,"runtime-index.db");int rebuilt;using(var store=DndStore.Open(dndPath))using(var index=new RuntimeIndex(runtime))rebuilt=index.RebuildFrom(store);if(rebuilt<=0)throw new InvalidOperationException("runtime_fragment_rebuild_empty");Session().AcknowledgeDeltasThrough(externalHead.Sequence);deltaCursor=externalHead.Sequence;return new{classification="runtime_fragments_rebuilt",externalDeltaKind=read.Deltas[0].Kind,deltaBytes=read.Deltas[0].ExactBytes.Length,indexedTextObjects=rebuilt,throughSequence=deltaCursor};
    }
    private object R04(){if(externalHead is null)throw new InvalidOperationException("external head missing");Refresh();int recovered=manifest!.Sentinels.SelectMany(x=>x.Value).Select(Guid.Parse).Count(ContainsPublicId);int total=manifest.Sentinels.SelectMany(x=>x.Value).Count();if(recovered!=total||!S().Objects.ContainsKey(DP().ExternalTarget)||Session().ReadHead().RevisionId!=externalHead.RevisionId)throw new InvalidOperationException("external_rebind_identity_loss");return new{classification="rebound_exact_head",head=Head(externalHead),sentinelsRecovered=recovered,sentinelsRequired=total,externalTargetId=Ids.Lower(DP().ExternalTarget),fullRediscovery=false};}

    private object Verify(string id,JsonElement p)=>id switch
    {
        "D.V-01"=>V01(),"D.V-02"=>V02(),"D.V-03"=>V03(),"D.V-04"=>V04(),"D.V-05"=>V05(),"D.V-06"=>V06(),_=>throw new InvalidOperationException("unknown_verify_call")
    };
    private object V01()
    {
        var s=S();var p=DP();string arabic=(string)s.Objects[p.ArabicBody].Data["text"]!;
        var checks=new Dictionary<string,bool>(StringComparer.Ordinal){
            ["appendix"]=s.Objects.ContainsKey(C("appendix")),["arabic_insert"]=arabic.Contains("\u0625\u0636\u0627\u0641\u064a\u0629",StringComparison.Ordinal),["range1"]=s.Ranges[C("range1")].State=="live",
            ["nested_item"]=s.Objects.ContainsKey(C("nestedItem")),["inserted_row"]=s.Objects.ContainsKey(C("insertedRow")),["theme"]=Equals(s.Objects[p.ThemeToken].Data.GetValueOrDefault("value"),"#3157A4"),
            ["thread"]=Equals(s.Objects[C("thread")].Data.GetValueOrDefault("state"),"open"),["suggestion"]=s.Objects.ContainsKey(C("replaceSuggestion")),["field"]=s.Objects.ContainsKey(C("countField")),
            ["control"]=Equals(s.Objects[p.ControlEnum].Data.GetValueOrDefault("value"),"review"),["footnote"]=s.Objects.ContainsKey(C("newFootnote")),["link"]=s.Objects.ContainsKey(C("newLink")),
            ["figure"]=s.Objects.ContainsKey(C("newFigure")),["math"]=s.Objects.ContainsKey(C("newMath")),["margin"]=Equals(s.Objects[p.Section2].Data.GetValueOrDefault("margin"),"0.82in"),["extension"]=s.Extensions.ContainsKey(C("movingExtension"))};
        var failed=checks.Where(x=>!x.Value).Select(x=>x.Key).ToArray();if(failed.Length>0)throw new InvalidOperationException("transaction_1_postcondition_failure:"+string.Join(',',failed));postconditionChecksPassed+=16;return new{classification="postconditions_exact",transaction=1,requested=16,checks=checks.Count};
    }    private object V02()
    {
        if(initialHead is null)throw new InvalidOperationException("initial head missing");string before=HashFile(dndPath);var st=S();var b=new SemanticTransactionBuilder(st);b.SetObjectProperty(DP().SafeBody,"v02_stale_attempt",true);var r=Session().CommitTransaction(b,initialHead.RevisionId,"D-V02-stale");string after=HashFile(dndPath);staleWriteCount=before==after?0:1;if(r.Success||r.Classification!="stale_revision"||staleWriteCount!=0)throw new InvalidOperationException("stale_refusal_failed");staleClassification=r.Classification;return new{classification=r.Classification,writes=staleWriteCount,artifactDigest=after};
    }
    private object V03()
    {
        string before=HashFile(dndPath);var st=S();var e=st.Extensions[DP().RequiredIntervalExtension];int a=st.Boundaries[e.StartBoundaryId!.Value].ScalarOffset,b=st.Boundaries[e.EndBoundaryId!.Value].ScalarOffset;string cls="";try{var tx=new SemanticTransactionBuilder(st);tx.InsertText(e.TargetId!.Value,(a+b)/2,"X",true);}catch(SemanticRefusalException ex){cls=ex.Code;}string after=HashFile(dndPath);requiredWriteCount=before==after?0:1;if(cls!="blocked_required_extension"||requiredWriteCount!=0)throw new InvalidOperationException("required_extension_refusal_failed:"+cls);requiredClassification=cls;return new{classification=cls,writes=requiredWriteCount,artifactDigest=after,extensionId=Ids.Lower(e.ExtensionId)};
    }
    private object V04()
    {
        var s=S();var p=DP();var r2=s.Ranges[C("range2")];var r1=s.Ranges[C("range1")];bool colocated=s.Boundaries[r1.StartBoundaryId].ScalarOffset==s.Boundaries[r2.StartBoundaryId].ScalarOffset&&r1.StartBoundaryId!=r2.StartBoundaryId;var repeating=(s.Objects[p.ControlRepeating].Data.GetValueOrDefault("value") as object?[])??[];
        var checks=new Dictionary<string,bool>(StringComparer.Ordinal){
            ["duplicate_parent"]=s.Objects[p.SecondDuplicate].ParentId==C("appendix"),
            ["duplicate_text"]=((string)s.Objects[p.SecondDuplicate].Data["text"]!).Contains("second instance replaced",StringComparison.Ordinal),
            ["range2_live"]=r2.State=="live",["colocated"]=colocated,["list_restart"]=Convert.ToInt32(s.Objects[p.NestedList].Data.GetValueOrDefault("start")??0)==7,
            ["merged_cell"]=s.Objects.ContainsKey(C("mergedInsertedCell"))&&!s.Objects[C("mergedInsertedCell")].Retired,["style"]=Equals(s.Objects[p.FirstDuplicate].Data.GetValueOrDefault("style_id"),p.NamedStyle),
            ["reply"]=s.Objects.ContainsKey(C("reply")),["suggestion_accept"]=s.Objects[p.SuggestionInsert].Retired,["field_policy"]=Equals(s.Objects[C("countField")].Data.GetValueOrDefault("evaluation_policy"),"on_semantic_commit"),
            ["repeating"]=repeating.Length==3,["endnote_move"]=s.Objects[p.EndnoteReference].ParentId==p.AnchorBody,["citation"]=s.Objects.ContainsKey(C("newCitation")),["figure_asset"]=AssetValue(s.Objects[C("newFigure")])==p.SvgDigest,
            ["display_math"]=((string)s.Objects[p.DisplayMath].Data["presentation_mathml"]!).Contains("msup",StringComparison.Ordinal),["page_break"]=s.Objects.ContainsKey(C("pageBreak")),["extension_move"]=s.Extensions[C("movingExtension")].TargetId==p.SecondDuplicate};
        var failed=checks.Where(x=>!x.Value).Select(x=>x.Key).ToArray();if(failed.Length>0)throw new InvalidOperationException("transaction_2_postcondition_failure:"+string.Join(',',failed));postconditionChecksPassed+=16;return new{classification="postconditions_exact",transaction=2,requested=16,checks=checks.Count,coLocatedIndependent=true};
    }    private object V05()
    {
        var s=S();var p=DP();var nested=s.Objects.Values.Where(o=>!o.Retired&&o.ParentId==p.NestedList&&o.Type=="list_item").OrderBy(o=>o.Order).ToArray();var tex=s.ProviderFacets[p.TexFacet];bool ok=s.Objects.ContainsKey(C("mergedText"))&&s.Ranges[C("range2")].State=="destroyed"&&nested[0].Id==C("nestedItem")&&s.Objects[p.OriginalMergedCell].Retired&&s.Retired[p.OriginalMergedCell].Resolution==RetiredResolution.Split&&s.Retired[p.OriginalMergedCell].Successors.Count==4&&s.Objects.ContainsKey(C("directOverride"))&&Equals(s.Objects[C("thread")].Data.GetValueOrDefault("state"),"resolved")&&s.Objects[p.SuggestionStyle].Retired&&Equals(s.Objects[p.FieldMetadata].Data.GetValueOrDefault("staleness"),"fresh")&&Equals(s.Objects[p.ControlReference].Data.GetValueOrDefault("value"),p.NamedAnchor2)&&Equals(s.Objects[p.CrossReference].Data.GetValueOrDefault("target_id"),p.NamedAnchor2)&&Equals(s.Objects[C("newCitation")].Data.GetValueOrDefault("locator"),"D.2 exact")&&Equals(s.Objects[C("newFigure")].Data.GetValueOrDefault("caption"),"D final figure caption")&&tex.Alignment=="source_updated_for_final_math"&&Equals(s.Objects[p.HeaderText].Data.GetValueOrDefault("text"),"DOCSeye D final header")&&s.Extensions[C("copiedTopologyExtension")].TargetId==p.NestedTable;if(!ok)throw new InvalidOperationException("transaction_3_postcondition_failure");postconditionChecksPassed+=16;return new{classification="postconditions_exact",transaction=3,requested=16,checks=16,typedSplitSuccessors=4};
    }
    private object V06()
    {
        var h=Session().ReadHead();var s=S();if(t3Head is null||h.RevisionId!=t3Head.RevisionId||!s.ComputeRoot().SequenceEqual(h.SemanticRoot))throw new InvalidOperationException("final_head_root_mismatch");var requested=new[]{t1Delta,t2Delta,t3Delta}.SelectMany(d=>d!.RequestedMutationIds).ToArray();var expected=Enumerable.Range(1,48).Select(i=>$"D.M-{i:00}").ToArray();if(!requested.SequenceEqual(expected,StringComparer.Ordinal)||requested.Distinct(StringComparer.Ordinal).Count()!=48||mutationEffects.Count!=48)throw new InvalidOperationException("requested_mutation_coverage_failure");var deltaOps=new[]{t1Delta,t2Delta,t3Delta}.SelectMany(d=>d!.OperationKinds).ToArray();var effectOps=expected.SelectMany(id=>mutationEffects[id].OperationKinds).ToArray();if(!deltaOps.SequenceEqual(effectOps,StringComparer.Ordinal))throw new InvalidOperationException("unrequested_low_level_operation_detected");return new{classification="final_exact",head=Head(h),requestedMutations=48,uniqueRequestedMutations=48,deltaRequestedCoverage=48,unrequestedSemanticMutations=0,staleRefusal=staleClassification,requiredExtensionRefusal=requiredClassification,lowLevelOperations=deltaOps.Length};
    }

    private bool ContainsPublicId(Guid id){var s=S();return s.Objects.ContainsKey(id)||s.Boundaries.ContainsKey(id)||s.Ranges.ContainsKey(id)||s.Extensions.ContainsKey(id)||s.ProviderFacets.ContainsKey(id);}
    private static string AssetValue(SemanticObject o)=>o.Data.GetValueOrDefault("asset_digest") switch{byte[] b=>Hex(b),string s=>s,_=>""};
}
