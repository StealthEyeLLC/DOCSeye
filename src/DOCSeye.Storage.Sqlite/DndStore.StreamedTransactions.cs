using System.Security.Cryptography;
using DOCSeye.Core;
using Microsoft.Data.Sqlite;

namespace DOCSeye.Storage.Sqlite;

public sealed partial class DndStore
{
    public TransactionResult CommitSemanticTransactionWithAsset(
        SemanticTransactionBuilder builder,
        Stream source,
        Guid expectedRevisionId,
        string idempotencyKey,
        Guid? figureObjectId=null,
        int chunkBytes=1024*1024,
        TransactionFaultPoint faultPoint=TransactionFaultPoint.None)
    {
        if(string.IsNullOrWhiteSpace(idempotencyKey))throw new ArgumentException("idempotency key required",nameof(idempotencyKey));
        if(chunkBytes<64*1024||chunkBytes>8*1024*1024)throw new ArgumentOutOfRangeException(nameof(chunkBytes));
        string? spool=null;Stream input=source;long originalPosition=0;
        try
        {
            if(!source.CanSeek)
            {
                spool=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"docseye-atomic-asset-"+Guid.NewGuid().ToString("N")+".bin");
                using(var fs=File.Create(spool))source.CopyTo(fs);input=File.OpenRead(spool);
            }
            originalPosition=input.Position;using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);byte[] buffer=new byte[chunkBytes];long length=0;int read;
            while((read=input.Read(buffer,0,buffer.Length))>0){hash.AppendData(buffer,0,read);length+=read;}
            byte[] digest=hash.GetHashAndReset();input.Position=originalPosition;
            var descriptor=builder.AttachEmbeddedAssetCommitment(digest,length,figureObjectId,chunkBytes);
            return CommitPreparedSemanticTransactionWithAsset(builder,input,descriptor,expectedRevisionId,idempotencyKey,faultPoint);
        }
        finally
        {
            if(input!=source)input.Dispose();if(source.CanSeek)try{source.Position=originalPosition;}catch{}if(spool is not null)try{File.Delete(spool);}catch{}
        }
    }

    private TransactionResult CommitPreparedSemanticTransactionWithAsset(
        SemanticTransactionBuilder builder,
        Stream input,
        StreamedAssetDescriptor asset,
        Guid expectedRevisionId,
        string idempotencyKey,
        TransactionFaultPoint faultPoint)
    {
        var known=KnownIdempotentResult(idempotencyKey);if(known is not null)return known;
        var before=ReadHead();if(before.RevisionId!=expectedRevisionId)return new(false,"stale_revision",before,null,["expected revision mismatch"]);
        if(builder.BaseState.FamilyId!=before.FamilyId||builder.BaseState.BranchId!=before.BranchId||builder.BaseState.RevisionId!=before.RevisionId)return new(false,"stale_revision",before,null,["transaction base does not match current validated head"]);
        if(builder.State.FamilyId!=before.FamilyId||builder.State.BranchId!=before.BranchId)return new(false,"scope_change_forbidden",before,null,["semantic transaction cannot change family or branch"]);
        try{ValidateStaged(builder.State);}catch(SemanticRefusalException ex){return new(false,ex.Code,before,null,[ex.Message]);}catch(Exception ex){return new(false,"invalid",before,null,[ex.Message]);}
        string assetKey=Convert.ToHexString(asset.Digest);if(!builder.State.Assets.TryGetValue(assetKey,out var stagedAsset)||stagedAsset.Length!=asset.Length||!stagedAsset.Digest.SequenceEqual(asset.Digest))return new(false,"invalid_asset",before,null,["staged asset commitment mismatch"]);

        SemanticState prior=LoadState();var changedObjects=Changed(prior.Objects,builder.State.Objects,EncodeObjectForPersistence);var changedBoundaries=Changed(prior.Boundaries,builder.State.Boundaries,CanonicalCbor.EncodeBoundary);var changedRanges=Changed(prior.Ranges,builder.State.Ranges,CanonicalCbor.EncodeRange);var changedExtensions=Changed(prior.Extensions,builder.State.Extensions,CanonicalCbor.EncodeExtension);var changedFacets=Changed(prior.ProviderFacets,builder.State.ProviderFacets,CanonicalCbor.EncodeProviderFacet);var changedRetired=Changed(prior.Retired,builder.State.Retired,CanonicalCbor.EncodeRetiredWitness);var changedOrigins=Changed(prior.OriginRefs,builder.State.OriginRefs,CanonicalCbor.EncodeOriginRef);var changedAssets=ChangedString(prior.Assets,builder.State.Assets,CanonicalCbor.EncodeAsset);
        if(!CapsulesEqual(prior,builder.State))return new(false,"immutable_source_capsule",before,null,["source capsules cannot be mutated by semantic transaction"]);

        using var tx=c.BeginTransaction();
        try
        {
            Inject(TransactionFaultPoint.BeforeSemanticWrite);
            foreach(Guid id in changedObjects)ReplaceObject(tx,id,builder.State);foreach(Guid id in changedBoundaries)ReplaceBoundary(tx,id,builder.State);foreach(Guid id in changedRanges)ReplaceRange(tx,id,builder.State);foreach(Guid id in changedExtensions)ReplaceExtension(tx,id,builder.State);foreach(Guid id in changedFacets)ReplaceProviderFacet(tx,id,builder.State);foreach(Guid id in changedRetired)ReplaceRetired(tx,id,builder.State);foreach(Guid id in changedOrigins)ReplaceOrigin(tx,id,builder.State);foreach(string id in changedAssets)ReplaceAsset(tx,id,builder.State);
            Inject(TransactionFaultPoint.AfterSemanticRecords);

            c.Exec(tx,"DELETE FROM asset_chunks WHERE digest=$d",("$d",asset.Digest));byte[] buffer=new byte[asset.ChunkBytes];int chunk=0,read;long written=0;
            while((read=input.Read(buffer,0,buffer.Length))>0)
            {
                byte[] bytes=buffer[..read];c.Exec(tx,"INSERT INTO asset_chunks(digest,chunk_index,data) VALUES($d,$i,$b)",("$d",asset.Digest),("$i",chunk),("$b",bytes));written+=read;chunk++;
                if(chunk==1){Inject(TransactionFaultPoint.DuringAssetWrite);Inject(TransactionFaultPoint.DiskFull);}
            }
            if(written!=asset.Length)throw new IOException("asset stream length changed between hash and transaction");

            var oldEntries=prior.RootEntries().ToDictionary(EntryKey,StringComparer.Ordinal);var newEntries=builder.State.RootEntries().ToDictionary(EntryKey,StringComparer.Ordinal);var merkle=new MerkleIndex(c,tx);int rootChanges=0;
            foreach(var old in oldEntries)if(!newEntries.ContainsKey(old.Key)){merkle.Delete(old.Value.Domain,old.Value.Key);rootChanges++;}
            foreach(var next in newEntries)if(!oldEntries.TryGetValue(next.Key,out var old)||!old.Value.SequenceEqual(next.Value.Value)){merkle.Upsert(next.Value);rootChanges++;}
            byte[] root=merkle.OverallRoot(),full=builder.State.ComputeRoot();if(!root.SequenceEqual(full))throw new InvalidOperationException("incremental/full root disagreement in streamed semantic transaction");
            Inject(TransactionFaultPoint.AfterRoot);

            Guid revision=Ids.NewV4();long seq=before.Sequence+1;c.Exec(tx,"INSERT INTO revisions(revision_id,branch_id,sequence,semantic_root,parent1,parent2) VALUES($r,$b,$s,$h,$p,NULL)",( "$r",Ids.RfcBytes(revision)),("$b",Ids.RfcBytes(before.BranchId)),("$s",seq),("$h",root),("$p",Ids.RfcBytes(before.RevisionId)));c.Exec(tx,"UPDATE head SET revision_id=$r,sequence=$s,semantic_root=$h WHERE id=1",("$r",Ids.RfcBytes(revision)),("$s",seq),("$h",root));
            byte[] deltaBytes=EncodeSemanticTransactionDelta(before.RevisionId,revision,builder,changedObjects,changedBoundaries,changedRanges,changedExtensions,changedFacets,changedRetired,changedOrigins,changedAssets);c.Exec(tx,"INSERT INTO deltas(sequence,from_revision_id,to_revision_id,delta_cbor,acknowledged,expires_after_sequence) VALUES($s,$f,$t,$d,0,$e)",( "$s",seq),("$f",Ids.RfcBytes(before.RevisionId)),("$t",Ids.RfcBytes(revision)),("$d",deltaBytes),("$e",seq+64));byte[] resultBytes=CanonicalCbor.Encode(new Dictionary<string,object?>(StringComparer.Ordinal){{"classification","committed"},{"revision_id",revision},{"semantic_root",root},{"from_revision_id",before.RevisionId},{"delta_sequence",seq},{"asset_digest",asset.Digest}});c.Exec(tx,"INSERT INTO idempotency(key,revision_id,sequence,result_cbor,expires_after_sequence) VALUES($k,$r,$s,$b,$e)",( "$k",idempotencyKey),("$r",Ids.RfcBytes(revision)),("$s",seq),("$b",resultBytes),("$e",seq+64));
            Inject(TransactionFaultPoint.AfterHeadWrite);tx.Commit();builder.State.RevisionId=revision;builder.State.Sequence=seq;
            var head=new RevisionHead(before.FamilyId,before.BranchId,revision,seq,root,[before.RevisionId],before.Mode);var delta=new SemanticDelta(before.BranchId,before.RevisionId,revision,changedObjects,changedBoundaries,changedExtensions,builder.LayoutInvalidations.ToArray(),deltaBytes){ChangedRanges=changedRanges,ChangedAssets=changedAssets,ChangedProviderFacets=changedFacets,ChangedRetiredWitnesses=changedRetired};return new(true,"committed",head,delta,[$"operations={builder.Operations.Count}",$"asset_bytes={asset.Length}",$"asset_chunks={chunk}",$"root_entries_changed={rootChanges}"]);
        }
        catch(Exception ex)
        {
            try{tx.Rollback();}catch{}string classification=faultPoint==TransactionFaultPoint.DiskFull?"disk_full":"transaction_failed";return new(false,classification,before,null,[ex.GetType().Name+":"+ex.Message]);
        }

        void Inject(TransactionFaultPoint point)
        {
            if(faultPoint!=point)return;if(point==TransactionFaultPoint.DiskFull)throw new IOException("simulated_disk_full");throw new InvalidOperationException("injected_fault:"+point);
        }
    }
}