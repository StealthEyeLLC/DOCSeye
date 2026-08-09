using DOCSeye.Core;
using Microsoft.Data.Sqlite;

namespace DOCSeye.Storage.Sqlite;

public sealed partial class DndStore
{
    public TransactionResult CommitSemanticTransaction(SemanticTransactionBuilder builder,Guid expectedRevisionId,string idempotencyKey)
    {
        if(string.IsNullOrWhiteSpace(idempotencyKey))throw new ArgumentException("idempotency key required",nameof(idempotencyKey));
        var known=KnownIdempotentResult(idempotencyKey);if(known is not null)return known;
        var before=ReadHead();if(before.RevisionId!=expectedRevisionId)return new(false,"stale_revision",before,null,["expected revision mismatch"]);
        if(builder.BaseState.FamilyId!=before.FamilyId||builder.BaseState.BranchId!=before.BranchId||builder.BaseState.RevisionId!=before.RevisionId)return new(false,"stale_revision",before,null,["transaction base does not match current validated head"]);
        if(builder.State.FamilyId!=before.FamilyId||builder.State.BranchId!=before.BranchId)return new(false,"scope_change_forbidden",before,null,["semantic transaction cannot change family or branch"]);
        if(builder.Operations.Count==0)return new(false,"empty_transaction",before,null,["semantic transaction contains no operations"]);
        try{ValidateStaged(builder.State);}catch(SemanticRefusalException ex){return new(false,ex.Code,before,null,[ex.Message]);}catch(Exception ex){return new(false,"invalid",before,null,[ex.Message]);}

        SemanticState prior=LoadState();var changedObjects=Changed(prior.Objects,builder.State.Objects,EncodeObjectForPersistence);var changedBoundaries=Changed(prior.Boundaries,builder.State.Boundaries,CanonicalCbor.EncodeBoundary);var changedRanges=Changed(prior.Ranges,builder.State.Ranges,CanonicalCbor.EncodeRange);var changedExtensions=Changed(prior.Extensions,builder.State.Extensions,CanonicalCbor.EncodeExtension);var changedFacets=Changed(prior.ProviderFacets,builder.State.ProviderFacets,CanonicalCbor.EncodeProviderFacet);var changedRetired=Changed(prior.Retired,builder.State.Retired,CanonicalCbor.EncodeRetiredWitness);var changedOrigins=Changed(prior.OriginRefs,builder.State.OriginRefs,CanonicalCbor.EncodeOriginRef);var changedAssets=ChangedString(prior.Assets,builder.State.Assets,CanonicalCbor.EncodeAsset);
        if(!CapsulesEqual(prior,builder.State))return new(false,"immutable_source_capsule",before,null,["source capsules cannot be mutated by semantic transaction"]);

        using var tx=c.BeginTransaction();
        try
        {
            foreach(Guid id in changedObjects)ReplaceObject(tx,id,builder.State);
            foreach(Guid id in changedBoundaries)ReplaceBoundary(tx,id,builder.State);
            foreach(Guid id in changedRanges)ReplaceRange(tx,id,builder.State);
            foreach(Guid id in changedExtensions)ReplaceExtension(tx,id,builder.State);
            foreach(Guid id in changedFacets)ReplaceProviderFacet(tx,id,builder.State);
            foreach(Guid id in changedRetired)ReplaceRetired(tx,id,builder.State);
            foreach(Guid id in changedOrigins)ReplaceOrigin(tx,id,builder.State);
            foreach(string id in changedAssets)ReplaceAsset(tx,id,builder.State);

            var oldEntries=prior.RootEntries().ToDictionary(EntryKey,StringComparer.Ordinal);var newEntries=builder.State.RootEntries().ToDictionary(EntryKey,StringComparer.Ordinal);var merkle=new MerkleIndex(c,tx);int rootChanges=0;
            foreach(var old in oldEntries)if(!newEntries.ContainsKey(old.Key)){merkle.Delete(old.Value.Domain,old.Value.Key);rootChanges++;}
            foreach(var next in newEntries)if(!oldEntries.TryGetValue(next.Key,out var old)||!old.Value.SequenceEqual(next.Value.Value)){merkle.Upsert(next.Value);rootChanges++;}
            byte[] root=merkle.OverallRoot(),full=builder.State.ComputeRoot();if(!root.SequenceEqual(full))throw new InvalidOperationException("incremental/full root disagreement in semantic transaction");

            Guid revision=Ids.NewV4();long seq=before.Sequence+1;
            c.Exec(tx,"INSERT INTO revisions(revision_id,branch_id,sequence,semantic_root,parent1,parent2) VALUES($r,$b,$s,$h,$p,NULL)",( "$r",Ids.RfcBytes(revision)),("$b",Ids.RfcBytes(before.BranchId)),("$s",seq),("$h",root),("$p",Ids.RfcBytes(before.RevisionId)));
            c.Exec(tx,"UPDATE head SET revision_id=$r,sequence=$s,semantic_root=$h WHERE id=1",("$r",Ids.RfcBytes(revision)),("$s",seq),("$h",root));
            byte[] deltaBytes=EncodeSemanticTransactionDelta(before.RevisionId,revision,builder,changedObjects,changedBoundaries,changedRanges,changedExtensions,changedFacets,changedRetired,changedOrigins,changedAssets);
            c.Exec(tx,"INSERT INTO deltas(sequence,from_revision_id,to_revision_id,delta_cbor,acknowledged,expires_after_sequence) VALUES($s,$f,$t,$d,0,$e)",("$s",seq),("$f",Ids.RfcBytes(before.RevisionId)),("$t",Ids.RfcBytes(revision)),("$d",deltaBytes),("$e",seq+64));
            byte[] resultBytes=CanonicalCbor.Encode(new Dictionary<string,object?>(StringComparer.Ordinal){{"classification","committed"},{"revision_id",revision},{"semantic_root",root},{"from_revision_id",before.RevisionId},{"delta_sequence",seq}});
            c.Exec(tx,"INSERT INTO idempotency(key,revision_id,sequence,result_cbor,expires_after_sequence) VALUES($k,$r,$s,$b,$e)",("$k",idempotencyKey),("$r",Ids.RfcBytes(revision)),("$s",seq),("$b",resultBytes),("$e",seq+64));
            tx.Commit();
            builder.State.RevisionId=revision;builder.State.Sequence=seq;
            var head=new RevisionHead(before.FamilyId,before.BranchId,revision,seq,root,[before.RevisionId],before.Mode);var delta=new SemanticDelta(before.BranchId,before.RevisionId,revision,changedObjects,changedBoundaries,changedExtensions,builder.LayoutInvalidations.ToArray(),deltaBytes){ChangedRanges=changedRanges,ChangedAssets=changedAssets,ChangedProviderFacets=changedFacets,ChangedRetiredWitnesses=changedRetired};
            return new(true,"committed",head,delta,[$"operations={builder.Operations.Count}",$"root_entries_changed={rootChanges}",$"logical_records_changed={changedObjects.Count+changedBoundaries.Count+changedRanges.Count+changedExtensions.Count+changedFacets.Count+changedRetired.Count+changedOrigins.Count+changedAssets.Count}"]);
        }
        catch(SqliteException ex){try{tx.Rollback();}catch{}return new(false,ex.SqliteErrorCode==19?"invalid":"transaction_failed",before,null,[$"sqlite:{ex.SqliteErrorCode}:{ex.Message}"]);}
        catch(Exception ex){try{tx.Rollback();}catch{}return new(false,"transaction_failed",before,null,[ex.GetType().Name+":"+ex.Message]);}
    }

    public TransactionResult? QueryIdempotency(string key)
    {
        var head=ReadHead();using var cmd=c.CreateCommand();cmd.CommandText="SELECT revision_id,sequence,result_cbor,expires_after_sequence FROM idempotency WHERE key=$k";cmd.Parameters.AddWithValue("$k",key);using var r=cmd.ExecuteReader();if(!r.Read())
        {
            using var expired=c.CreateCommand();expired.CommandText="SELECT 1 FROM expired_idempotency WHERE key=$k";expired.Parameters.AddWithValue("$k",key);return expired.ExecuteScalar() is null?null:new(false,"outcome_unknown",head,null,["idempotency witness expired"]);
        }
        long expires=r.GetInt64(3);if(expires<head.Sequence)return new(false,"outcome_unknown",head,null,["idempotency witness expired"]);Guid revision=Ids.GuidFromRfcBytes((byte[])r[0]);long seq=r.GetInt64(1);var map=(Dictionary<string,object?>)CanonicalCbor.Decode((byte[])r[2])!;byte[] root=(byte[])map["semantic_root"]!;Guid from=(Guid)map["from_revision_id"]!;var knownHead=new RevisionHead(head.FamilyId,head.BranchId,revision,seq,root,[from],head.Mode);return new(true,"committed_known",knownHead,null,["idempotency witness matched committed outcome"]);
    }

    public void ForceExpireDeltasBefore(long sequence)=>c.Exec(null,"DELETE FROM deltas WHERE sequence<$s",("$s",sequence));

    private TransactionResult? KnownIdempotentResult(string key)=>QueryIdempotency(key);
    private static string EntryKey(RootEntry e)=>e.Domain+":"+Convert.ToHexString(e.Key);
    private static byte[] EncodeObjectForPersistence(SemanticObject o)=>[..CanonicalCbor.EncodeObject(o),..o.Order.Bytes()];

    private static IReadOnlyList<Guid> Changed<T>(IReadOnlyDictionary<Guid,T> old,IReadOnlyDictionary<Guid,T> next,Func<T,byte[]> encode)
    {
        var result=new HashSet<Guid>(old.Keys.Except(next.Keys));foreach(var pair in next)if(!old.TryGetValue(pair.Key,out var prior)||!encode(prior).SequenceEqual(encode(pair.Value)))result.Add(pair.Key);return result.OrderBy(Ids.Lower,StringComparer.Ordinal).ToArray();
    }
    private static IReadOnlyList<string> ChangedString<T>(IReadOnlyDictionary<string,T> old,IReadOnlyDictionary<string,T> next,Func<T,byte[]> encode)
    {
        var result=new HashSet<string>(old.Keys.Except(next.Keys),StringComparer.Ordinal);foreach(var pair in next)if(!old.TryGetValue(pair.Key,out var prior)||!encode(prior).SequenceEqual(encode(pair.Value)))result.Add(pair.Key);return result.OrderBy(x=>x,StringComparer.Ordinal).ToArray();
    }
    private static bool CapsulesEqual(SemanticState a,SemanticState b)=>a.SourceCapsules.Count==b.SourceCapsules.Count&&a.SourceCapsules.All(x=>b.SourceCapsules.TryGetValue(x.Key,out var y)&&x.Value.Provider==y.Provider&&x.Value.Alignment==y.Alignment&&x.Value.Digest.SequenceEqual(y.Digest)&&x.Value.ExactBytes.SequenceEqual(y.ExactBytes));

    private void ReplaceObject(SqliteTransaction tx,Guid id,SemanticState s){c.Exec(tx,"DELETE FROM objects WHERE id=$i",("$i",Ids.RfcBytes(id)));if(s.Objects.TryGetValue(id,out var v))InsertObject(tx,v);}
    private void ReplaceBoundary(SqliteTransaction tx,Guid id,SemanticState s){c.Exec(tx,"DELETE FROM boundaries WHERE id=$i",("$i",Ids.RfcBytes(id)));if(s.Boundaries.TryGetValue(id,out var v))InsertBoundary(tx,v);}
    private void ReplaceRange(SqliteTransaction tx,Guid id,SemanticState s){c.Exec(tx,"DELETE FROM retained_ranges WHERE id=$i",("$i",Ids.RfcBytes(id)));if(s.Ranges.TryGetValue(id,out var v))InsertRange(tx,v);}
    private void ReplaceExtension(SqliteTransaction tx,Guid id,SemanticState s){c.Exec(tx,"DELETE FROM extensions WHERE id=$i",("$i",Ids.RfcBytes(id)));if(s.Extensions.TryGetValue(id,out var v))InsertExtension(tx,v);}
    private void ReplaceProviderFacet(SqliteTransaction tx,Guid id,SemanticState s){c.Exec(tx,"DELETE FROM provider_facets WHERE id=$i",("$i",Ids.RfcBytes(id)));if(s.ProviderFacets.TryGetValue(id,out var v))InsertProviderFacet(tx,v);}
    private void ReplaceRetired(SqliteTransaction tx,Guid id,SemanticState s){c.Exec(tx,"DELETE FROM retired_witnesses WHERE object_id=$i",("$i",Ids.RfcBytes(id)));if(s.Retired.TryGetValue(id,out var v))InsertRetired(tx,v);}
    private void ReplaceOrigin(SqliteTransaction tx,Guid id,SemanticState s){c.Exec(tx,"DELETE FROM origin_refs WHERE current_id=$i",("$i",Ids.RfcBytes(id)));if(s.OriginRefs.TryGetValue(id,out var v))InsertOriginRef(tx,v);}
    private void ReplaceAsset(SqliteTransaction tx,string id,SemanticState s){byte[] digest=Convert.FromHexString(id);c.Exec(tx,"DELETE FROM assets WHERE digest=$d",("$d",digest));if(s.Assets.TryGetValue(id,out var v))InsertAsset(tx,v);}

    private static byte[] EncodeSemanticTransactionDelta(Guid from,Guid to,SemanticTransactionBuilder builder,IReadOnlyList<Guid> objects,IReadOnlyList<Guid> boundaries,IReadOnlyList<Guid> ranges,IReadOnlyList<Guid> extensions,IReadOnlyList<Guid> facets,IReadOnlyList<Guid> retired,IReadOnlyList<Guid> origins,IReadOnlyList<string> assets)
    {
        object?[] operations=builder.Operations.Select(o=>(object?)new Dictionary<string,object?>(StringComparer.Ordinal){{"kind",o.Kind},{"target_id",o.TargetId},{"detail",o.Detail}}).ToArray();return CanonicalCbor.Encode(new Dictionary<string,object?>(StringComparer.Ordinal)
        {
            ["kind"]="semantic_transaction",["from_revision_id"]=from,["to_revision_id"]=to,["operations"]=operations,
            ["changed_objects"]=objects.Cast<object?>().ToArray(),["changed_boundaries"]=boundaries.Cast<object?>().ToArray(),["changed_ranges"]=ranges.Cast<object?>().ToArray(),["changed_extensions"]=extensions.Cast<object?>().ToArray(),["changed_provider_facets"]=facets.Cast<object?>().ToArray(),["changed_retired_witnesses"]=retired.Cast<object?>().ToArray(),["changed_origin_refs"]=origins.Cast<object?>().ToArray(),["changed_assets"]=assets.Cast<object?>().ToArray(),["layout_invalidations"]=builder.LayoutInvalidations.Cast<object?>().ToArray()
        });
    }

    private static void ValidateStaged(SemanticState s)
    {
        if(!Ids.IsV4(s.FamilyId)||!Ids.IsV4(s.BranchId)||!Ids.IsV4(s.RevisionId))throw new SemanticRefusalException("invalid_identity");var publicIds=new HashSet<Guid>();
        foreach(Guid id in s.Objects.Keys.Concat(s.Boundaries.Keys).Concat(s.Ranges.Keys).Concat(s.Extensions.Keys).Concat(s.ProviderFacets.Keys))if(!Ids.IsV4(id)||!publicIds.Add(id))throw new SemanticRefusalException("duplicate_id");
        var live=s.Objects.Values.Where(o=>!o.Retired).ToDictionary(o=>o.Id);foreach(var o in live.Values)if(o.ParentId is Guid p&&!live.ContainsKey(p))throw new SemanticRefusalException("broken_reference");if(live.Values.Where(o=>o.ParentId is not null).GroupBy(o=>(o.ParentId,Convert.ToHexString(o.Order.Bytes()))).Any(g=>g.Count()>1))throw new SemanticRefusalException("invalid_order");
        foreach(var b in s.Boundaries.Values){if(b.ScalarOffset<0||(!live.ContainsKey(b.OwnerId)&&b.State is AnchorState.Live or AnchorState.Collapsed or AnchorState.Orphaned))throw new SemanticRefusalException("invalid_boundary");}
        foreach(var range in s.Ranges.Values.Where(r=>r.State!="destroyed")){if(range.EffectiveIntervals.Count==0||(!range.AllowMultiInterval&&range.EffectiveIntervals.Count!=1))throw new SemanticRefusalException("invalid_range");foreach(var interval in range.EffectiveIntervals)if(!s.Boundaries.ContainsKey(interval.StartBoundaryId)||!s.Boundaries.ContainsKey(interval.EndBoundaryId))throw new SemanticRefusalException("broken_reference");}
        foreach(var e in s.Extensions.Values)if(!e.DigestValid)throw new SemanticRefusalException("invalid_extension");foreach(var f in s.ProviderFacets.Values)if(!f.DigestValid)throw new SemanticRefusalException("invalid_provider_facet");
    }
}
