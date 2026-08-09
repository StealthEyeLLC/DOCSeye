using System.Security.Cryptography;

namespace DOCSeye.Core;

public sealed class SemanticTransactionBuilder
{
    private readonly Func<Guid> newId;
    private readonly HashSet<Guid> transformedExtensions = [];
    public SemanticState BaseState { get; }
    public SemanticState State { get; }
    public List<SemanticOperationRecord> Operations { get; } = [];
    public List<string> RequestedMutationIds { get; } = [];
    public SortedSet<string> LayoutInvalidations { get; } = new(StringComparer.Ordinal);

    public SemanticTransactionBuilder(SemanticState source, Func<Guid>? newId = null)
    {
        BaseState=Branching.Clone(source); State=Branching.Clone(source); this.newId=newId??Ids.NewV4;
    }

    public Guid CreateObject(string type, Guid? parentId, string? role, Dictionary<string,object?> data, int? insertIndex=null, bool layoutSensitive=true)
    {
        if(parentId is Guid p && (!State.Objects.TryGetValue(p,out var parent)||parent.Retired)) throw Refuse("broken_reference","parent missing or retired");
        Guid id=newId();var order=AllocateOrder(parentId,insertIndex);
        State.Objects[id]=new(id,type,parentId,order,role,CloneData(data),false);
        Record("create_"+type,id,("parent_id",parentId),("role",role));if(layoutSensitive)InvalidateObject(id,parentId);return id;
    }

    public void SetObjectProperty(Guid id,string property,object? value,bool layoutSensitive=false,string operationKind="set_property")
    {
        var o=LiveObject(id);CheckExtensions(new(operationKind,id,property));var data=CloneData(o.Data);data[property]=CloneValue(value);State.Objects[id]=o with{Data=data};Record(operationKind,id,("property",property),("value",CloneValue(value)));if(layoutSensitive)InvalidateObject(id,o.ParentId);
    }

    public void SetObjectData(Guid id,Dictionary<string,object?> data,bool layoutSensitive=false,string operationKind="set_data")
    {
        var o=LiveObject(id);CheckExtensions(new(operationKind,id));State.Objects[id]=o with{Data=CloneData(data)};Record(operationKind,id);if(layoutSensitive)InvalidateObject(id,o.ParentId);
    }

    public void MoveObject(Guid id,Guid newParentId,int? insertIndex=null)
    {
        var o=LiveObject(id);_ = LiveObject(newParentId);if(id==newParentId||IsDescendant(newParentId,id))throw Refuse("invalid_structure","move would create containment cycle");CheckExtensions(new("move_object",id,Move:true));Guid? oldParent=o.ParentId;State.Objects[id]=o with{ParentId=newParentId,Order=AllocateOrder(newParentId,insertIndex,id)};Record("move_object",id,("old_parent_id",oldParent),("new_parent_id",newParentId));InvalidateObject(id,oldParent);InvalidateObject(id,newParentId);
    }

    public void ReorderObject(Guid id,int insertIndex)
    {
        var o=LiveObject(id);if(o.ParentId is not Guid parent)throw Refuse("invalid_structure","root object cannot be reordered");State.Objects[id]=o with{Order=AllocateOrder(parent,insertIndex,id)};Record("reorder_object",id,("parent_id",parent),("insert_index",insertIndex));InvalidateObject(id,parent);
    }

    public void InsertText(Guid textBlockId,int atScalar,string inserted,bool humanFacing=true)
    {
        var o=TextObject(textBlockId);string text=(string)o.Data["text"]!;CheckExtensions(new("insert_text",textBlockId,StartScalar:atScalar,EndScalar:atScalar));string updated=UnicodeText.Insert(text,atScalar,inserted,humanFacing);BoundaryTransform.ApplyInsertion(State,textBlockId,atScalar,inserted,LiveRanges());var data=CloneData(o.Data);data["text"]=updated;State.Objects[textBlockId]=o with{Data=data};Record("insert_text",textBlockId,("at_scalar",atScalar),("text",inserted));InvalidateObject(textBlockId,o.ParentId);
    }

    public void DeleteText(Guid textBlockId,int startScalar,int count,bool humanFacing=true)
    {
        if(count<0)throw new ArgumentOutOfRangeException(nameof(count));var o=TextObject(textBlockId);string text=(string)o.Data["text"]!;int end=startScalar+count;CheckExtensions(new("delete_text",textBlockId,StartScalar:startScalar,EndScalar:end));string updated=UnicodeText.Delete(text,startScalar,count,humanFacing);var before=CaptureRangePositions(textBlockId);BoundaryTransform.ApplyDeletion(State,textBlockId,startScalar,count);ApplyDeletionLifecycle(textBlockId,startScalar,end,before);var data=CloneData(o.Data);data["text"]=updated;State.Objects[textBlockId]=o with{Data=data};Record("delete_text",textBlockId,("start_scalar",startScalar),("count",count));InvalidateObject(textBlockId,o.ParentId);
    }

    public void ReplaceText(Guid textBlockId,int startScalar,int count,string replacement,bool humanFacing=true)
    {
        if(count<0)throw new ArgumentOutOfRangeException(nameof(count));var o=TextObject(textBlockId);string text=(string)o.Data["text"]!;int end=startScalar+count;CheckExtensions(new("replace_text",textBlockId,StartScalar:startScalar,EndScalar:end));string deleted=UnicodeText.Delete(text,startScalar,count,humanFacing);string updated=UnicodeText.Insert(deleted,startScalar,replacement,humanFacing);var before=CaptureRangePositions(textBlockId);BoundaryTransform.ApplyDeletion(State,textBlockId,startScalar,count);ApplyDeletionLifecycle(textBlockId,startScalar,end,before);BoundaryTransform.ApplyInsertion(State,textBlockId,startScalar,replacement,LiveRanges());var data=CloneData(o.Data);data["text"]=updated;State.Objects[textBlockId]=o with{Data=data};Record("replace_text",textBlockId,("start_scalar",startScalar),("count",count),("replacement",replacement));InvalidateObject(textBlockId,o.ParentId);
    }

    public Guid RetainRange(Guid ownerId,int startScalar,int endScalar,string kind,EdgeAffinity startAffinity=EdgeAffinity.ExcludeAtEdge,EdgeAffinity endAffinity=EdgeAffinity.ExcludeAtEdge,bool allowMultiInterval=true)
    {
        var o=TextObject(ownerId);int length=UnicodeText.ScalarCount((string)o.Data["text"]!);if(startScalar<0||endScalar<startScalar||endScalar>length)throw Refuse("invalid_range","range outside owner text");if(startScalar==endScalar&&(startAffinity is not (EdgeAffinity.BeforeInsertion or EdgeAffinity.AfterInsertion)||endAffinity!=startAffinity))throw Refuse("invalid_range","collapsed range requires point affinity");Guid sb=newId(),eb=startScalar==endScalar?sb:newId();State.Boundaries[sb]=new(sb,ownerId,startScalar,startAffinity);if(eb!=sb)State.Boundaries[eb]=new(eb,ownerId,endScalar,endAffinity);Guid rid=newId();State.Ranges[rid]=new(rid,sb,eb,allowMultiInterval,kind){Intervals=[new(sb,eb)]};Record("retain_range",rid,("owner_id",ownerId),("start_scalar",startScalar),("end_scalar",endScalar),("kind",kind));return rid;
    }

    public Guid RetainMultiIntervalRange(IReadOnlyList<(Guid ownerId,int startScalar,int endScalar)> intervals,string kind,EdgeAffinity startAffinity=EdgeAffinity.ExcludeAtEdge,EdgeAffinity endAffinity=EdgeAffinity.ExcludeAtEdge)
    {
        if(intervals.Count<2)throw Refuse("invalid_range","multi-interval range needs at least two intervals");var pairs=new List<RangeInterval>();foreach(var interval in intervals){var o=TextObject(interval.ownerId);int length=UnicodeText.ScalarCount((string)o.Data["text"]!);if(interval.startScalar<0||interval.endScalar<interval.startScalar||interval.endScalar>length)throw Refuse("invalid_range","interval outside owner text");Guid sb=newId(),eb=interval.startScalar==interval.endScalar?sb:newId();State.Boundaries[sb]=new(sb,interval.ownerId,interval.startScalar,startAffinity);if(eb!=sb)State.Boundaries[eb]=new(eb,interval.ownerId,interval.endScalar,endAffinity);pairs.Add(new(sb,eb));}Guid rid=newId();State.Ranges[rid]=new(rid,pairs[0].StartBoundaryId,pairs[0].EndBoundaryId,true,kind){Intervals=pairs.ToArray()};Record("retain_multi_interval_range",rid,("interval_count",pairs.Count),("kind",kind));return rid;
    }

    public void ReleaseRange(Guid rangeId)
    {
        if(!State.Ranges.TryGetValue(rangeId,out var r)||r.State=="destroyed")throw Refuse("not_found","range not live");State.Ranges[rangeId]=r with{State="destroyed"};foreach(Guid bid in r.EffectiveIntervals.SelectMany(i=>new[]{i.StartBoundaryId,i.EndBoundaryId}).Distinct())if(!State.Ranges.Values.Any(x=>x.Id!=rangeId&&x.State!="destroyed"&&x.EffectiveIntervals.Any(i=>i.StartBoundaryId==bid||i.EndBoundaryId==bid))&&State.Boundaries.TryGetValue(bid,out var b))State.Boundaries[bid]=b with{State=AnchorState.Destroyed};Record("release_range",rangeId);
    }

    public void SetBoundaryAffinity(Guid boundaryId,EdgeAffinity affinity)
    {
        if(!State.Boundaries.TryGetValue(boundaryId,out var b)||b.State is AnchorState.Destroyed or AnchorState.Ambiguous)throw Refuse("not_found","boundary not writable");State.Boundaries[boundaryId]=b with{Affinity=affinity};Record("set_boundary_affinity",boundaryId,("affinity",CanonicalCbor.AffinityToken(affinity)));
    }

    public (Guid Left,Guid Right) SplitText(Guid textBlockId,int splitScalar)
    {
        var o=TextObject(textBlockId);CheckExtensions(new("split_text",textBlockId,StartScalar:splitScalar,EndScalar:splitScalar));var result=BoundaryTransform.SplitTextBlock(State,textBlockId,splitScalar,LiveRanges(),newId,State.Sequence+128);Record("split_text",textBlockId,("left_id",result.left),("right_id",result.right),("split_scalar",splitScalar));InvalidateObject(textBlockId,o.ParentId);InvalidateObject(result.left,o.ParentId);InvalidateObject(result.right,o.ParentId);return(result.left,result.right);
    }

    public Guid MergeText(Guid leftId,Guid rightId)
    {
        var l=TextObject(leftId);var r=TextObject(rightId);if(l.ParentId!=r.ParentId)throw Refuse("invalid_merge_targets","text merge requires same parent");CheckExtensions(new("merge_text",leftId));CheckExtensions(new("merge_text",rightId));Guid result=BoundaryTransform.MergeTextBlocks(State,leftId,rightId,newId,State.Sequence+128);Record("merge_text",result,("left_id",leftId),("right_id",rightId));InvalidateObject(result,l.ParentId);return result;
    }

    public Guid InsertListItem(Guid listId,int insertIndex,IReadOnlyList<string> blocks,int level=0,int? ordinalIntent=null)
    {
        var list=LiveObject(listId);if(list.Type!="list")throw Refuse("invalid_structure","target is not list");Guid item=CreateObject("list_item",listId,"list_item",new(StringComparer.Ordinal){{"level",level},{"ordinal_intent",ordinalIntent}},insertIndex);foreach(string text in blocks)CreateObject("text_block",item,"list_item_text",new(StringComparer.Ordinal){{"text",text}});Record("list_insert_item",item,("list_id",listId),("blocks",blocks.Count));return item;
    }

    public void SetListRestart(Guid listOrItemId,int start,string intent="explicit")
    {
        SetObjectProperty(listOrItemId,"start",start,true,"list_restart");SetObjectProperty(listOrItemId,"restart_intent",intent,true,"list_restart_intent");
    }

    public (Guid Row,IReadOnlyList<Guid> Cells) InsertTableRow(Guid tableId,int rowIndex,IReadOnlyList<string> cellTexts)
    {
        var table=LiveObject(tableId);if(table.Type!="table")throw Refuse("invalid_structure","target is not table");CheckTopologyExtensions(new("table_insert_row",tableId,TopologyMutation:true));Guid row=CreateObject("table_row",tableId,"table_row",new(StringComparer.Ordinal){{"index",rowIndex},{"is_header",false}},rowIndex);var cells=new List<Guid>();for(int c=0;c<cellTexts.Count;c++)cells.Add(CreateObject("table_cell",tableId,"table_cell",new(StringComparer.Ordinal){{"row",rowIndex},{"column",c},{"row_span",1},{"column_span",1},{"row_header",false},{"column_header",false},{"text",cellTexts[c]}}));Record("table_insert_row",row,("table_id",tableId),("cell_count",cells.Count));LayoutInvalidations.Add("table:"+Ids.Lower(tableId));return(row,cells);
    }

    public Guid MergeTableCells(Guid tableId,IReadOnlyList<Guid> cellIds,int row,int column,int rowSpan,int columnSpan,string text)
    {
        _=LiveObject(tableId);if(cellIds.Count<2)throw Refuse("invalid_structure","cell merge needs multiple cells");CheckTopologyExtensions(new("table_merge_cells",tableId,TopologyMutation:true));foreach(Guid id in cellIds){var cell=LiveObject(id);if(cell.Type!="table_cell")throw Refuse("invalid_structure","merge input not cell");State.Objects[id]=cell with{Retired=true};State.Retired[id]=new(id,RetiredResolution.Merged,[],State.Sequence+128);}Guid result=CreateObject("table_cell",tableId,"table_cell",new(StringComparer.Ordinal){{"row",row},{"column",column},{"row_span",rowSpan},{"column_span",columnSpan},{"row_header",false},{"column_header",false},{"text",text}});foreach(Guid id in cellIds)State.Retired[id]=State.Retired[id] with{Successors=[result]};Record("table_merge_cells",result,("table_id",tableId),("input_ids",cellIds.Cast<object?>().ToArray()));return result;
    }

    public IReadOnlyList<Guid> SplitTableCell(Guid tableId,Guid cellId,int rows,int columns)
    {
        _=LiveObject(tableId);var cell=LiveObject(cellId);if(cell.Type!="table_cell")throw Refuse("invalid_structure","split input not cell");CheckTopologyExtensions(new("table_split_cell",tableId,TopologyMutation:true));int row=Convert.ToInt32(cell.Data["row"]),col=Convert.ToInt32(cell.Data["column"]);State.Objects[cellId]=cell with{Retired=true};var result=new List<Guid>();for(int rr=0;rr<rows;rr++)for(int cc=0;cc<columns;cc++)result.Add(CreateObject("table_cell",tableId,"table_cell",new(StringComparer.Ordinal){{"row",row+rr},{"column",col+cc},{"row_span",1},{"column_span",1},{"row_header",false},{"column_header",false},{"text",$"split-{rr}-{cc}"}}));State.Retired[cellId]=new(cellId,RetiredResolution.Split,result,State.Sequence+128);Record("table_split_cell",cellId,("table_id",tableId),("successors",result.Cast<object?>().ToArray()));return result;
    }

    public Guid AddExtension(ExtensionEnvelope extension)
    {
        if(State.Extensions.ContainsKey(extension.ExtensionId))throw Refuse("duplicate_id","extension id exists");if(extension.ExactPayload.LongLength>64L*1024*1024)throw Refuse("resource_limit","extension payload exceeds 64 MiB");if(!extension.DigestValid)throw Refuse("invalid","extension digest invalid");State.Extensions[extension.ExtensionId]=extension;Record("add_extension",extension.ExtensionId,("coverage",CanonicalCbor.CoverageToken(extension.CoverageKind)),("required",extension.Required));return extension.ExtensionId;
    }

    public Guid CopyTransformableExtension(Guid extensionId,IReadOnlyDictionary<Guid,Guid> remap)
    {
        if(!State.Extensions.TryGetValue(extensionId,out var e))throw Refuse("not_found","extension missing");try{var copy=ExtensionPolicyEngine.RemapForCopy(e,remap,newId);State.Extensions[copy.ExtensionId]=copy;Record("copy_extension",copy.ExtensionId,("source_extension_id",extensionId));return copy.ExtensionId;}catch(InvalidOperationException ex){throw Refuse(ex.Message,ex.Message);}
    }

    public void TransformExtension(Guid extensionId,byte[] transformedPayload)
    {
        if(!State.Extensions.TryGetValue(extensionId,out var e))throw Refuse("not_found","extension missing");
        if(e.EditPolicy!=ExtensionEditPolicy.GenericTransform)throw Refuse(e.Required?"blocked_required_extension":"unsupported","extension does not permit generic transform");
        byte[] payload=transformedPayload.ToArray();State.Extensions[extensionId]=e with{ExactPayload=payload,PayloadDigest=SHA256.HashData(payload)};transformedExtensions.Add(extensionId);Record("transform_extension",extensionId,("payload_bytes",payload.Length));
    }
    public void MoveExtensionWithOwner(Guid extensionId,Guid newTargetId)
    {
        if(!State.Extensions.TryGetValue(extensionId,out var e))throw Refuse("not_found","extension missing");if(e.EditPolicy is not (ExtensionEditPolicy.MoveWithTarget or ExtensionEditPolicy.GenericTransform))throw Refuse(e.Required?"blocked_required_extension":"unsupported","extension cannot move");_ = LiveObject(newTargetId);State.Extensions[extensionId]=e with{TargetId=newTargetId};Record("move_extension",extensionId,("new_target_id",newTargetId));
    }

    public void ResolveSuggestion(Guid suggestionId,string decision)
    {
        if(decision is not ("accept" or "reject"))throw new ArgumentOutOfRangeException(nameof(decision));var s=LiveObject(suggestionId);if(s.Type!="suggestion")throw Refuse("invalid_structure","target is not suggestion");State.Objects[suggestionId]=s with{Retired=true};State.Retired[suggestionId]=new(suggestionId,RetiredResolution.Destroyed,[],State.Sequence+128);if(s.Data.GetValueOrDefault("range_id") is Guid rangeId&&State.Ranges.TryGetValue(rangeId,out var targetRange)){State.Ranges[rangeId]=targetRange with{State="destroyed"};foreach(Guid boundaryId in targetRange.EffectiveIntervals.SelectMany(i=>new[]{i.StartBoundaryId,i.EndBoundaryId}).Distinct())if(!State.Ranges.Values.Any(r=>r.Id!=rangeId&&r.State!="destroyed"&&r.EffectiveIntervals.Any(i=>i.StartBoundaryId==boundaryId||i.EndBoundaryId==boundaryId))&&State.Boundaries.TryGetValue(boundaryId,out var boundary))State.Boundaries[boundaryId]=boundary with{State=AnchorState.Destroyed};}Record("suggestion_"+decision,suggestionId);InvalidateObject(suggestionId,s.ParentId);
    }

    public void RetireObject(Guid id,RetiredResolution resolution=RetiredResolution.Destroyed)
    {
        if(resolution is RetiredResolution.Split or RetiredResolution.Merged)throw Refuse("invalid_retirement","split/merge retirement requires typed operation");
        var o=LiveObject(id);CheckExtensions(new("retire_object",id));
        if(State.Objects.Values.Any(x=>!x.Retired&&x.ParentId==id))throw Refuse("invalid_structure","object with live children requires typed structural retirement");
        var ownedBoundaries=State.Boundaries.Values.Where(b=>b.OwnerId==id&&b.State!=AnchorState.Destroyed).Select(b=>b.Id).ToArray();
        if(ownedBoundaries.Length>0)throw Refuse("retained_boundary_requires_typed_retirement","object owns retained boundaries");
        State.Objects[id]=o with{Retired=true};State.Retired[id]=new(id,resolution,[],State.Sequence+128);Record("retire_object",id,("resolution",CanonicalCbor.RetiredToken(resolution)));InvalidateObject(id,o.ParentId);
    }

    public int GarbageCollectRetiredWitnesses(long retentionFloor)
    {
        int changed=0;foreach(Guid id in State.Retired.Values.Where(w=>w.Resolution!=RetiredResolution.UnknownRetired&&w.ExpiresAfterSequence<retentionFloor).Select(w=>w.ObjectId).ToArray())
        {
            var witness=State.Retired[id];State.Retired[id]=witness with{Resolution=RetiredResolution.UnknownRetired,Successors=Array.Empty<Guid>(),ExpiresAfterSequence=long.MaxValue};changed++;
        }
        if(changed>0)Record("gc_retired_witnesses",null,("retention_floor",retentionFloor),("changed",changed));return changed;
    }

    public StreamedAssetDescriptor AttachEmbeddedAssetCommitment(byte[] digest,long length,Guid? figureObjectId=null,int chunkBytes=1024*1024)
    {
        if(digest.Length!=32)throw Refuse("invalid_asset","SHA-256 digest requires 32 bytes");if(length<0)throw Refuse("invalid_asset","negative asset length");if(chunkBytes<64*1024||chunkBytes>8*1024*1024)throw new ArgumentOutOfRangeException(nameof(chunkBytes));
        string key=Convert.ToHexString(digest);State.Assets[key]=new(digest.ToArray(),length,"embedded");
        if(figureObjectId is Guid figure){var o=LiveObject(figure);if(o.Type!="figure")throw Refuse("invalid_asset_target","asset target is not a figure");var data=CloneData(o.Data);data["asset_digest"]=digest.ToArray();State.Objects[figure]=o with{Data=data};InvalidateObject(figure,o.ParentId);}
        Record("attach_embedded_asset",figureObjectId,("digest",digest.ToArray()),("length",length),("chunk_bytes",chunkBytes));LayoutInvalidations.Add("asset");return new(digest.ToArray(),length,figureObjectId,chunkBytes);
    }
    public void RecordRequestedMutation(string requestId)
    {
        if(string.IsNullOrWhiteSpace(requestId))throw new ArgumentException("request id required",nameof(requestId));if(RequestedMutationIds.Contains(requestId,StringComparer.Ordinal))throw Refuse("duplicate_requested_mutation",requestId);RequestedMutationIds.Add(requestId);
    }
    public void UpdateNativeMath(Guid mathId,string presentationMathMl)
    {
        var o=LiveObject(mathId);if(o.Type!="math")throw Refuse("invalid_math_target","target is not math");string canonical=MathSemanticPolicy.CanonicalizePresentationMathMl(presentationMathMl);var data=CloneData(o.Data);data["presentation_mathml"]=canonical;State.Objects[mathId]=o with{Data=data};foreach(Guid facetId in State.ProviderFacets.Values.Where(f=>f.TargetId==mathId).Select(f=>f.Id).ToArray()){var facet=State.ProviderFacets[facetId];State.ProviderFacets[facetId]=facet with{Alignment="stale_after_native_math_edit"};}Record("update_native_math",mathId,("presentation_mathml",canonical));InvalidateObject(mathId,o.ParentId);
    }
    public void UpdateProviderFacet(Guid facetId,byte[] payload,string alignment)
    {
        if(!State.ProviderFacets.TryGetValue(facetId,out var facet))throw Refuse("not_found","provider facet missing");byte[] exact=payload.ToArray();State.ProviderFacets[facetId]=facet with{ExactPayload=exact,Digest=SHA256.HashData(exact),Alignment=alignment};Record("provider_facet_update",facetId,("payload_bytes",exact.Length),("alignment",alignment));
    }
    public void SetProviderFacetAlignment(Guid facetId,string alignment)
    {
        if(!State.ProviderFacets.TryGetValue(facetId,out var facet))throw Refuse("not_found","provider facet missing");State.ProviderFacets[facetId]=facet with{Alignment=alignment};Record("provider_facet_alignment",facetId,("alignment",alignment));
    }

    public void AddLayoutInvalidation(string token){if(string.IsNullOrWhiteSpace(token))throw new ArgumentException("invalidation token required",nameof(token));LayoutInvalidations.Add(token);}

    private SemanticObject LiveObject(Guid id)=>State.Objects.TryGetValue(id,out var o)&&!o.Retired?o:throw Refuse("not_found","object missing or retired");
    private SemanticObject TextObject(Guid id){var o=LiveObject(id);if(o.Type!="text_block"||!o.Data.ContainsKey("text")||o.Data["text"] is not string)throw Refuse("invalid_text_target","target is not text block");return o;}
    private IEnumerable<RetainedRange> LiveRanges()=>State.Ranges.Values.Where(r=>r.State!="destroyed");
    private static SemanticRefusalException Refuse(string code,string? message=null)=>new(code,message);
    private void CheckExtensions(EditIntent edit){var d=ExtensionPolicyEngine.Check(State,edit);if(!d.Allowed)throw Refuse(d.Classification,$"{d.Classification}: {string.Join(",",d.Intersecting.Select(Ids.Lower))}");}
    private void CheckTopologyExtensions(EditIntent edit){var d=ExtensionPolicyEngine.Check(State,edit);if(!d.Allowed)throw Refuse(d.Classification,$"{d.Classification}: {string.Join(",",d.Intersecting.Select(Ids.Lower))}");if(d.RequiresTransform){var missing=d.Intersecting.Where(id=>State.Extensions.TryGetValue(id,out var e)&&e.EditPolicy==ExtensionEditPolicy.GenericTransform&&!transformedExtensions.Contains(id)).ToArray();if(missing.Length>0)throw Refuse("extension_transform_required",$"extension_transform_required: {string.Join(",",missing.Select(Ids.Lower))}");}}
    private bool IsDescendant(Guid candidate,Guid ancestor){Guid? p=candidate;var seen=new HashSet<Guid>();while(p is Guid id&&seen.Add(id)&&State.Objects.TryGetValue(id,out var o)){if(o.ParentId==ancestor)return true;p=o.ParentId;}return false;}

    private OrderKey AllocateOrder(Guid? parent,int? insertIndex,Guid? excludeId=null)
    {
        var siblings=State.Objects.Values.Where(o=>!o.Retired&&o.ParentId==parent&&o.Id!=excludeId).OrderBy(o=>o.Order).ThenBy(o=>Ids.Lower(o.Id),StringComparer.Ordinal).ToList();int idx=Math.Clamp(insertIndex??siblings.Count,0,siblings.Count);var allocation=SparseOrder.AllocateBetween(siblings,idx);if(allocation.rebalanced>0)foreach(var sibling in siblings)State.Objects[sibling.Id]=sibling;return allocation.key;
    }

    private Dictionary<Guid,List<(Guid rangeId,int start,int end,string kind)>> CaptureRangePositions(Guid owner)
    {
        var result=new Dictionary<Guid,List<(Guid,int,int,string)>>();foreach(var r in LiveRanges()){foreach(var interval in r.EffectiveIntervals){if(!State.Boundaries.TryGetValue(interval.StartBoundaryId,out var s)||!State.Boundaries.TryGetValue(interval.EndBoundaryId,out var e)||s.OwnerId!=owner||e.OwnerId!=owner)continue;if(!result.TryGetValue(r.Id,out var list)){list=[];result[r.Id]=list;}list.Add((r.Id,Math.Min(s.ScalarOffset,e.ScalarOffset),Math.Max(s.ScalarOffset,e.ScalarOffset),r.Kind));}}return result;
    }

    private void ApplyDeletionLifecycle(Guid owner,int deleteStart,int deleteEnd,Dictionary<Guid,List<(Guid rangeId,int start,int end,string kind)>> before)
    {
        foreach(var pair in before)
        {
            if(!State.Ranges.TryGetValue(pair.Key,out var range)||range.State=="destroyed")continue;bool anyComplete=pair.Value.Any(x=>deleteStart<=x.start&&deleteEnd>=x.end&&x.end>x.start);bool allComplete=pair.Value.Count>0&&pair.Value.All(x=>deleteStart<=x.start&&deleteEnd>=x.end);if(!anyComplete)continue;
            string next=range.Kind switch
            {
                "comment" when allComplete=>"orphaned",
                "style" when allComplete=>"destroyed",
                "suggestion" when allComplete=>"destroyed",
                "field" or "field_reference" or "cross_reference" when allComplete=>"unresolved",
                "named_range" when allComplete=>"collapsed",
                _ when allComplete=>"destroyed",
                _=>range.State
            };
            State.Ranges[pair.Key]=range with{State=next};
            if(next=="destroyed")
            {
                foreach(Guid boundaryId in range.EffectiveIntervals.SelectMany(i=>new[]{i.StartBoundaryId,i.EndBoundaryId}).Distinct())
                    if(State.Boundaries.TryGetValue(boundaryId,out var boundary))State.Boundaries[boundaryId]=boundary with{State=AnchorState.Destroyed};
            }
            else if(next=="orphaned")
            {
                foreach(Guid boundaryId in range.EffectiveIntervals.SelectMany(i=>new[]{i.StartBoundaryId,i.EndBoundaryId}).Distinct())
                    if(State.Boundaries.TryGetValue(boundaryId,out var boundary))State.Boundaries[boundaryId]=boundary with{State=AnchorState.Orphaned};
            }
        }
    }

    private void InvalidateObject(Guid id,Guid? parent){LayoutInvalidations.Add("object:"+Ids.Lower(id));if(parent is Guid p)LayoutInvalidations.Add("flow:"+Ids.Lower(p));}
    private void Record(string kind,Guid? target,params (string key,object? value)[] detail){var d=new Dictionary<string,object?>(StringComparer.Ordinal);foreach(var x in detail)d[x.key]=CloneValue(x.value);Operations.Add(new(kind,target,d));}
    private static Dictionary<string,object?> CloneData(IReadOnlyDictionary<string,object?> d)=>d.ToDictionary(k=>k.Key,v=>CloneValue(v.Value),StringComparer.Ordinal);
    private static object? CloneValue(object? value)=>value switch{null=>null,byte[] b=>b.ToArray(),Dictionary<string,object?> d=>CloneData(d),IReadOnlyDictionary<string,object?> d=>CloneData(d),object?[] a=>a.Select(CloneValue).ToArray(),IEnumerable<Guid> ids=>ids.Cast<object?>().ToArray(),_=>value};
}