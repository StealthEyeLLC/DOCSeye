using DOCSeye.Core;

namespace DOCSeye.Storage.Sqlite;

public sealed class NativeDocumentSession : IDisposable
{
    private readonly DndStore store;
    private RevisionHead? validatedHead;
    public ValidationReport Validation { get; private set; }
    public bool WriteAuthority => Validation.Writable && validatedHead is not null;
    public string Path => store.Path;

    private NativeDocumentSession(DndStore store, ValidationReport validation)
    {
        this.store=store; Validation=validation; validatedHead=validation.Writable?store.ReadHead():null;
    }

    public static NativeDocumentSession Open(string path,bool verifyAssets=true)
    {
        var store=DndStore.Open(path); var validation=store.Validate(verifyAssets); if(validation.Writable)store.ReconcilePrivateMerkleCache(); return new(store,validation);
    }

    public void Dispose()=>store.Dispose();
    public RevisionHead ReadHead()=>store.ReadHead();
    public SemanticState LoadState()=>store.LoadState();
    public SemanticObject? ReadObject(Guid id)=>store.ReadObject(id);
    public IReadOnlyList<SemanticObject> QueryObjects(string? type=null,string? role=null,int limit=10000)=>store.QueryObjects(type,role,limit);
    public DeltaReadResult ReadDeltas(long cursorSequence)=>store.ReadDeltas(cursorSequence);
    public void AcknowledgeDeltasThrough(long sequence)=>store.AcknowledgeDeltasThrough(sequence);

    public ValidationReport ReconcileExternal(bool verifyAssets=true)
    {
        Validation=store.Validate(verifyAssets); if(Validation.Writable)store.ReconcilePrivateMerkleCache(); validatedHead=Validation.Writable?store.ReadHead():null; return Validation;
    }

    public SemanticTransactionBuilder BeginTransaction() => new(LoadState());

    public TransactionResult CommitTransaction(SemanticTransactionBuilder builder,Guid expectedRevisionId,string idempotencyKey)
    {
        if(!WriteAuthority) return new(false,"invalid_artifact",TryHead(),null,[Validation.Classification]);
        var current=store.ReadHead();
        if(expectedRevisionId!=current.RevisionId)return new(false,"stale_revision",current,null,["expected revision mismatch"]);
        if(validatedHead!.RevisionId!=current.RevisionId || !validatedHead.SemanticRoot.SequenceEqual(current.SemanticRoot))
            return new(false,"external_change_requires_reconciliation",current,null,["validated head changed outside session"]);
        var result=store.CommitSemanticTransaction(builder,expectedRevisionId,idempotencyKey);if(result.Success&&result.Head is not null)validatedHead=result.Head;return result;
    }

    public TransactionResult? QueryIdempotency(string key)=>store.QueryIdempotency(key);
    public void PruneExpiredWitnesses(long keepFromSequence)=>store.PruneExpiredWitnesses(keepFromSequence);
    public TransactionResult CommitObjectData(PortableWriteHandle handle,Dictionary<string,object?> newData,string idempotencyKey)
    {
        if(!WriteAuthority) return new(false,"invalid_artifact",TryHead(),null,[Validation.Classification]);
        var current=store.ReadHead();
        if(validatedHead!.RevisionId!=current.RevisionId || !validatedHead.SemanticRoot.SequenceEqual(current.SemanticRoot))
            return new(false,"external_change_requires_reconciliation",current,null,["validated head changed outside session"]);
        var result=store.CommitObjectData(handle,newData,idempotencyKey);
        if(result.Success) validatedHead=result.Head;
        return result;
    }

    public TransactionResult CommitBoundaryOffset(Guid boundaryId,int newOffset,Guid expectedRevisionId,string idempotencyKey)
    {
        if(!WriteAuthority) return new(false,"invalid_artifact",TryHead(),null,[Validation.Classification]);
        var current=store.ReadHead();
        if(validatedHead!.RevisionId!=current.RevisionId || !validatedHead.SemanticRoot.SequenceEqual(current.SemanticRoot))
            return new(false,"external_change_requires_reconciliation",current,null,["validated head changed outside session"]);
        var result=store.CommitBoundaryOffset(boundaryId,newOffset,expectedRevisionId,idempotencyKey);
        if(result.Success) validatedHead=result.Head;
        return result;
    }

    public (TransactionResult Result,LocalMutationMetrics Metrics,byte[] Digest) CommitEmbeddedAsset(Stream source,Guid expectedRevisionId,string idempotencyKey,Guid? figureObjectId=null,int chunkBytes=1024*1024,Action<int>? chunkProgress=null)
    {
        if(!WriteAuthority) return(new(false,"invalid_artifact",TryHead(),null,[Validation.Classification]),new(0,0,0,0,0),[]);
        var current=store.ReadHead();
        if(validatedHead!.RevisionId!=current.RevisionId || !validatedHead.SemanticRoot.SequenceEqual(current.SemanticRoot))
            return(new(false,"external_change_requires_reconciliation",current,null,["validated head changed outside session"]),new(0,0,0,0,0),[]);
        var result=store.CommitEmbeddedAsset(source,expectedRevisionId,idempotencyKey,figureObjectId,chunkBytes,chunkProgress);
        if(result.Result.Success) validatedHead=result.Result.Head;
        return result;
    }

    private RevisionHead? TryHead(){try{return store.ReadHead();}catch{return null;}}
}