using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;
using DOCSeye.Core;
using DOCSeye.Storage.Sqlite;

internal sealed record StorageDiff(int ChangedBlocks,long ChangedBlockBytes);
internal sealed record RouteMetrics(
    string Route,string AuthorityModel,string ArtifactPath,long InitialArtifactBytes,long FinalArtifactBytes,
    int ProgramHostCalls,int ProgramHostInvocations,int UsefulOperations,int QueryCalls,int ProviderRediscoveries,int CorrespondenceRecoveryAttempts,
    long ModelFacingRequestBytes,long ModelFacingResponseBytes,long SemanticDeltaBytes,long RepresentationDiffBytes,
    int LogicalRecordsTouched,int PhysicalRecordsTouched,long PhysicalBytesTouched,string PhysicalRecordUnit,
    long ColdOpenMs,long WarmQueryMs,long CommitMs,long ReconcileMs,long WorkflowLatencyMs,
    bool ExactTargetRecovered,int WrongTargetEdits,int StaleWrites,int AmbiguousWrites,string RetainedIdentity,
    int LocalChangedRepresentationRecords,int ExternalChangedRepresentationRecords,IReadOnlyList<string> LocalChangedRepresentationNames,IReadOnlyList<string> ExternalChangedRepresentationNames,
    object Detail);

internal sealed class CallMeter
{
    public int Calls {get;private set;} public long RequestBytes {get;private set;} public long ResponseBytes {get;private set;}
    public T Call<T>(string name,object? request,Func<T> action)
    {
        Calls++;RequestBytes+=JsonSerializer.SerializeToUtf8Bytes(new{name,request}).LongLength;T result=action();ResponseBytes+=JsonSerializer.SerializeToUtf8Bytes(new{name,result}).LongLength;return result;
    }
}

internal static class BenchmarkFixture
{
    public const int ParagraphCount=1200; public const ulong Seed=0xD0C5B001UL; public const string DuplicateText="Retained duplicate target must survive an external move without text-based rebound.";
    public static (SemanticState State,Guid Root,Guid Flow,Guid Target) CreateState()
    {
        var rng=new RandomNumberGeneratorLike(Seed);Guid Id()=>Ids.DeterministicV4(rng);var s=new SemanticState{FamilyId=Id(),BranchId=Id(),RevisionId=Id(),Sequence=0,Mode=AuthorityMode.NativeAuthored};Guid root=Id(),flow=Id();s.Objects[root]=new(root,"document",null,OrderKey.Initial(0),"document",new(StringComparer.Ordinal));s.Objects[flow]=new(flow,"flow",root,OrderKey.Initial(0),"main",new(StringComparer.Ordinal));Guid target=Guid.Empty;
        var duplicateOrdinals=new HashSet<int>{300,600,900};for(int i=0;i<ParagraphCount;i++){Guid id=Id();string text=duplicateOrdinals.Contains(i)?DuplicateText:Payload(i);if(i==600)target=id;s.Objects[id]=new(id,"text_block",flow,OrderKey.Initial(i+1),"body",new(StringComparer.Ordinal){{"text",text},{"benchmark_ordinal",i}});}if(target==Guid.Empty)throw new InvalidOperationException("benchmark target missing");return(s,root,flow,target);
    }
    private static string Payload(int ordinal)
    {
        byte[] seed=SHA256.HashData(Encoding.UTF8.GetBytes($"DOCSeye benchmark paragraph {ordinal:D4}"));var sb=new StringBuilder(768);int n=0;while(sb.Length<768){byte[] block=SHA256.HashData(seed.Concat(BitConverter.GetBytes(n++)).ToArray());foreach(byte b in block){sb.Append((char)('a'+(b%26)));if(sb.Length%13==0)sb.Append(' ');if(sb.Length>=768)break;}}return $"P{ordinal:D4} "+sb.ToString();
    }
    public static string ProviderId(Guid id)=>Convert.ToHexString(SHA256.HashData(Ids.RfcBytes(id)))[..8];
    public static void WriteDocx(SemanticState s,string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);if(File.Exists(path))File.Delete(path);using var doc=WordprocessingDocument.Create(path,WordprocessingDocumentType.Document);var main=doc.AddMainDocumentPart();main.Document=new W.Document();var body=new W.Body();main.Document.Append(body);const string w14="http://schemas.microsoft.com/office/word/2010/wordml";foreach(var o in s.Objects.Values.Where(o=>!o.Retired&&o.Type=="text_block"&&o.Role=="body").OrderBy(o=>o.Order)){string text=(string)o.Data["text"]!;var p=new W.Paragraph(new W.Run(new W.Text(text){Space=SpaceProcessingModeValues.Preserve}));p.SetAttribute(new OpenXmlAttribute("w14","paraId",w14,ProviderId(o.Id)));body.Append(p);}main.Document.Save();
    }
}

internal static class MeasureUtil
{
    public static StorageDiff BlockDiff(byte[] before,byte[] after,int block=4096){int max=Math.Max(before.Length,after.Length),changed=0;long bytes=0;for(int offset=0;offset<max;offset+=block){int bn=Math.Min(block,Math.Max(0,before.Length-offset)),an=Math.Min(block,Math.Max(0,after.Length-offset));bool same=bn==an&&before.AsSpan(offset,bn).SequenceEqual(after.AsSpan(offset,an));if(!same){changed++;bytes+=Math.Max(bn,an);}}return new(changed,bytes);}
    public static Dictionary<string,byte[]> ZipEntries(string path){using var z=ZipFile.OpenRead(path);return z.Entries.Where(e=>!string.IsNullOrEmpty(e.Name)).ToDictionary(e=>e.FullName,e=>{using var s=e.Open();using var m=new MemoryStream();s.CopyTo(m);return m.ToArray();},StringComparer.Ordinal);}
    public static (string[] Names,long Bytes) EntryDiff(IReadOnlyDictionary<string,byte[]> before,IReadOnlyDictionary<string,byte[]> after){var names=before.Keys.Union(after.Keys,StringComparer.Ordinal).Where(k=>!before.TryGetValue(k,out var a)||!after.TryGetValue(k,out var b)||!a.SequenceEqual(b)).OrderBy(x=>x,StringComparer.Ordinal).ToArray();long bytes=names.Sum(k=>Math.Max(before.TryGetValue(k,out var a)?a.LongLength:0,after.TryGetValue(k,out var b)?b.LongLength:0));return(names,bytes);}
    public static long JsonBytes(object value)=>JsonSerializer.SerializeToUtf8Bytes(value).LongLength;
    public static byte[] ReadShared(string path){using var fs=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);using var ms=new MemoryStream();fs.CopyTo(ms);return ms.ToArray();}
}

internal static class NativeRoute
{
    public static RouteMetrics Run(string work)
    {
        Directory.CreateDirectory(work);var fixture=BenchmarkFixture.CreateState();string path=Path.Combine(work,"native.dnd");using(var store=DndStore.Create(path,fixture.State)){}long initialBytes=new FileInfo(path).Length;var meter=new CallMeter();var workflow=Stopwatch.StartNew();long coldOpen;NativeDocumentSession session;
        var sw=Stopwatch.StartNew();session=meter.Call("open_validate",new{path="native.dnd"},()=>NativeDocumentSession.Open(path,true));sw.Stop();coldOpen=sw.ElapsedMilliseconds;using(session)
        {
            var candidates=meter.Call("query_duplicates",new{text=BenchmarkFixture.DuplicateText},()=>session.QueryObjects("text_block","body",int.MaxValue).Where(o=>Equals(o.Data.GetValueOrDefault("text"),BenchmarkFixture.DuplicateText)).OrderBy(o=>o.Order).Select(o=>o.Id).ToArray());if(candidates.Length!=3||candidates[1]!=fixture.Target)throw new InvalidOperationException("native duplicate query target drift");Guid retained=meter.Call("retain",new{candidateIndex=1},()=>fixture.Target);
            sw.Restart();for(int i=0;i<20;i++)_ = session.QueryObjects("text_block","body",int.MaxValue).Count(o=>Equals(o.Data.GetValueOrDefault("text"),BenchmarkFixture.DuplicateText));sw.Stop();long warm=sw.ElapsedMilliseconds;
            Guid expected=meter.Call("begin",new{retained=Ids.Lower(retained)},()=>session.ReadHead().RevisionId);string replacement=BenchmarkFixture.DuplicateText+" [native edited]";var tx=session.BeginTransaction();meter.Call("replace",new{target=Ids.Lower(retained)},()=>{string old=(string)tx.State.Objects[retained].Data["text"]!;tx.ReplaceText(retained,0,UnicodeText.ScalarCount(old),replacement,true);return new{oldScalars=UnicodeText.ScalarCount(old),newScalars=UnicodeText.ScalarCount(replacement)};});byte[] beforeLocal=MeasureUtil.ReadShared(path);sw.Restart();var commit=meter.Call("commit",new{expected=Ids.Lower(expected)},()=>session.CommitTransaction(tx,expected,"benchmark-native-local"));sw.Stop();long commitMs=sw.ElapsedMilliseconds;if(!commit.Success||commit.Head is null||commit.Delta is null)throw new InvalidOperationException("native local commit failed:"+commit.Classification);byte[] afterLocal=MeasureUtil.ReadShared(path);var localDiff=MeasureUtil.BlockDiff(beforeLocal,afterLocal);var localHead=commit.Head;long localDelta=commit.Delta.ExactBytes.LongLength;
            var delta=meter.Call("delta_read",new{cursor=0L},()=>session.ReadTypedDeltas(0));if(delta.ResyncRequired||delta.Deltas.Count!=1||delta.Deltas[0].ToRevisionId!=localHead.RevisionId)throw new InvalidOperationException("native local delta mismatch");session.AcknowledgeDeltasThrough(localHead.Sequence);
            byte[] beforeExternal=MeasureUtil.ReadShared(path);TransactionResult externalResult;using(var external=NativeDocumentSession.Open(path,true)){var eh=external.ReadHead();var et=external.BeginTransaction();et.MoveObject(retained,fixture.Flow,0);externalResult=external.CommitTransaction(et,eh.RevisionId,"benchmark-native-external");if(!externalResult.Success||externalResult.Head is null||externalResult.Delta is null)throw new InvalidOperationException("native external move failed:"+externalResult.Classification);}byte[] afterExternal=MeasureUtil.ReadShared(path);var externalDiff=MeasureUtil.BlockDiff(beforeExternal,afterExternal);long externalDelta=externalResult.Delta!.ExactBytes.LongLength;
            sw.Restart();var reconcile=meter.Call("reconcile",new{from=Ids.Lower(localHead.RevisionId)},()=>session.ReconcileExternal(true));sw.Stop();long reconcileMs=sw.ElapsedMilliseconds;if(!reconcile.Writable)throw new InvalidOperationException("native reconcile failed:"+reconcile.Classification);var externalDeltas=session.ReadTypedDeltas(localHead.Sequence);if(externalDeltas.ResyncRequired||externalDeltas.Deltas.Count!=1)throw new InvalidOperationException("native external delta mismatch");session.AcknowledgeDeltasThrough(externalResult.Head!.Sequence);
            var inspected=meter.Call("inspect_retained",new{id=Ids.Lower(retained)},()=>session.ReadObject(retained));if(inspected is null||inspected.Retired||!Equals(inspected.Data.GetValueOrDefault("text"),replacement))throw new InvalidOperationException("native retained target recovery failed");meter.Call("summarize",new{},()=>new{exact=true,wrongTarget=0,stale=0,ambiguous=0});workflow.Stop();int logical=commit.Delta.ChangedObjects.Count+externalResult.Delta!.ChangedObjects.Count;int physical=localDiff.ChangedBlocks+externalDiff.ChangedBlocks;long physicalBytes=localDiff.ChangedBlockBytes+externalDiff.ChangedBlockBytes;
            return new("native","native DND semantic authority",path,initialBytes,new FileInfo(path).Length,meter.Calls,1,10,2,0,0,meter.RequestBytes,meter.ResponseBytes,localDelta+externalDelta,physicalBytes,logical,physical,physicalBytes,"4096-byte DND file blocks",coldOpen,warm,commitMs,reconcileMs,workflow.ElapsedMilliseconds,true,0,0,0,Ids.Lower(retained),localDiff.ChangedBlocks,externalDiff.ChangedBlocks,[],[],new{initial_revision=Ids.Lower(expected),local_revision=Ids.Lower(localHead.RevisionId),external_revision=Ids.Lower(externalResult.Head!.RevisionId),external_reconcile=reconcile.Classification,full_provider_rediscovery=false});
        }
    }
}

internal static class DocxFirstRoute
{
    private const string W14="http://schemas.microsoft.com/office/word/2010/wordml";
    private sealed record P(string Id,string Text,int Ordinal);
    private static List<P> Scan(string path){using var doc=WordprocessingDocument.Open(path,false);var body=doc.MainDocumentPart?.Document?.Body??throw new InvalidOperationException("DOCX body missing");return body.Elements<W.Paragraph>().Select((p,i)=>new P(p.GetAttribute("paraId",W14).Value??throw new InvalidOperationException("provider paragraph ID missing"),p.InnerText,i)).ToList();}
    public static RouteMetrics Run(string work)
    {
        Directory.CreateDirectory(work);var fixture=BenchmarkFixture.CreateState();string path=Path.Combine(work,"docx-first.docx");BenchmarkFixture.WriteDocx(fixture.State,path);long initialBytes=new FileInfo(path).Length;var meter=new CallMeter();var workflow=Stopwatch.StartNew();int rediscoveries=0,recoveries=0;var sw=Stopwatch.StartNew();List<P> index=[];meter.Call("open_validate",new{path="docx-first.docx"},()=>{rediscoveries++;index=Scan(path);return new{classification="valid",paragraphs=index.Count,representation="docx"};});sw.Stop();long coldOpen=sw.ElapsedMilliseconds;var candidates=meter.Call("query_duplicates",new{text=BenchmarkFixture.DuplicateText},()=>index.Where(p=>p.Text==BenchmarkFixture.DuplicateText).OrderBy(p=>p.Ordinal).ToArray());if(candidates.Length!=3)throw new InvalidOperationException("docx duplicate query count");string retained=meter.Call("retain",new{candidateIndex=1},()=>candidates[1].Id);if(retained!=BenchmarkFixture.ProviderId(fixture.Target))throw new InvalidOperationException("docx target identity mismatch");sw.Restart();for(int i=0;i<20;i++)_ = index.Count(p=>p.Text==BenchmarkFixture.DuplicateText);sw.Stop();long warm=sw.ElapsedMilliseconds;
        string expected=meter.Call("begin",new{retained},()=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant());string replacement=BenchmarkFixture.DuplicateText+" [docx edited]";meter.Call("replace",new{target=retained},()=>new{oldScalars=UnicodeText.ScalarCount(BenchmarkFixture.DuplicateText),newScalars=UnicodeText.ScalarCount(replacement)});byte[] beforeLocal=File.ReadAllBytes(path);var beforeLocalEntries=MeasureUtil.ZipEntries(path);sw.Restart();meter.Call("commit",new{expected},()=>{using var doc=WordprocessingDocument.Open(path,true);var main=doc.MainDocumentPart??throw new InvalidOperationException("DOCX main part missing");var body=main.Document?.Body??throw new InvalidOperationException("DOCX body missing");var p=body.Elements<W.Paragraph>().Single(x=>x.GetAttribute("paraId",W14).Value==retained);p.RemoveAllChildren<W.Run>();p.Append(new W.Run(new W.Text(replacement){Space=SpaceProcessingModeValues.Preserve}));main.Document!.Save();return new{classification="committed",representation="docx"};});sw.Stop();long commitMs=sw.ElapsedMilliseconds;byte[] afterLocal=File.ReadAllBytes(path);var afterLocalEntries=MeasureUtil.ZipEntries(path);var localBlock=MeasureUtil.BlockDiff(beforeLocal,afterLocal);var localEntry=MeasureUtil.EntryDiff(beforeLocalEntries,afterLocalEntries);long localSemantic=MeasureUtil.JsonBytes(new{kind="paragraph_text_replace",provider_id=retained,old_text=BenchmarkFixture.DuplicateText,new_text=replacement});meter.Call("delta_read",new{revision=Convert.ToHexString(SHA256.HashData(afterLocal)).ToLowerInvariant()},()=>new{kind="paragraph_text_replace",bytes=localSemantic,changed=1});
        byte[] beforeExternal=File.ReadAllBytes(path);var beforeExternalEntries=MeasureUtil.ZipEntries(path);using(var doc=WordprocessingDocument.Open(path,true)){var main=doc.MainDocumentPart??throw new InvalidOperationException("DOCX main part missing");var body=main.Document?.Body??throw new InvalidOperationException("DOCX body missing");var p=body.Elements<W.Paragraph>().Single(x=>x.GetAttribute("paraId",W14).Value==retained);p.Remove();body.InsertAt(p,0);main.Document!.Save();}byte[] afterExternal=File.ReadAllBytes(path);var afterExternalEntries=MeasureUtil.ZipEntries(path);var externalBlock=MeasureUtil.BlockDiff(beforeExternal,afterExternal);var externalEntry=MeasureUtil.EntryDiff(beforeExternalEntries,afterExternalEntries);long externalSemantic=MeasureUtil.JsonBytes(new{kind="paragraph_move",provider_id=retained,new_ordinal=0});sw.Restart();meter.Call("reconcile",new{priorRetained=retained},()=>{rediscoveries++;recoveries++;index=Scan(path);return new{classification="provider_rescan_exact",paragraphs=index.Count,retainedFound=index.Any(p=>p.Id==retained)};});sw.Stop();long reconcileMs=sw.ElapsedMilliseconds;var recovered=index.SingleOrDefault(p=>p.Id==retained);if(recovered is null||recovered.Text!=replacement||recovered.Ordinal!=0)throw new InvalidOperationException("docx retained provider identity recovery failed");var inspected=meter.Call("inspect_retained",new{id=retained},()=>new{id=recovered.Id,textHash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(recovered.Text))).ToLowerInvariant(),ordinal=recovered.Ordinal});meter.Call("summarize",new{},()=>new{exact=true,wrongTarget=0,stale=0,ambiguous=0});workflow.Stop();int physicalRecords=localEntry.Names.Length+externalEntry.Names.Length;long physicalBytes=localEntry.Bytes+externalEntry.Bytes;long representation=localBlock.ChangedBlockBytes+externalBlock.ChangedBlockBytes;
        return new("docx_first","reconstructed frozen DOCX-first operating model: DOCX representation truth + retained w14:paraId provider witness",path,initialBytes,new FileInfo(path).Length,meter.Calls,1,10,2,rediscoveries,recoveries,meter.RequestBytes,meter.ResponseBytes,localSemantic+externalSemantic,representation,2,physicalRecords,physicalBytes,"changed uncompressed OPC part payloads",coldOpen,warm,commitMs,reconcileMs,workflow.ElapsedMilliseconds,true,0,0,0,retained,localEntry.Names.Length,externalEntry.Names.Length,localEntry.Names,externalEntry.Names,new{baseline="executable reconstruction because commit 47b5280 was architecture-only / not implemented",provider_witness="w14:paraId",external_recovery="full provider rescan by retained provider witness",artifact_block_diff_bytes=representation,changed_part_payload_bytes=physicalBytes,inspect=inspected});
    }
}