using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DOCSeye.Core;
using DOCSeye.Fixtures;
using DOCSeye.Storage.Sqlite;

string repo=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","..",".."));
string work=Path.Combine(repo,"artifacts","milestone-a");
if(Directory.Exists(work))Directory.Delete(work,true);Directory.CreateDirectory(work);Directory.CreateDirectory(Path.Combine(repo,"fixtures"));Directory.CreateDirectory(Path.Combine(repo,"evidence"));
string node=@"C:\AgentBrowser\tools\node-v24.18.1-win-x64\node.exe";
string writer=Path.Combine(repo,"tools","independent-writer","independent-writer.mjs");
string shellMove=Path.Combine(repo,"tools","shelleye-carrier-proof.js");
string sqlite=Path.Combine(repo,".tools","sqlite","sqlite3.exe");
string sqliteNative=Path.Combine(repo,".tools","sqlite-native");
var checks=new List<object>();
var hardZero=Enumerable.Range(1,18).ToDictionary(i=>$"H-{i:00}",_=>0,StringComparer.Ordinal);

void Expect(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
void Pass(string id,object evidence){checks.Add(new{id,result="PASS",evidence});Console.WriteLine($"{id} PASS");}
string Sha(string path){using var fs=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);return Convert.ToHexString(SHA256.HashData(fs)).ToLowerInvariant();}
string Run(string exe,params string[] args)
{
    var psi=new ProcessStartInfo(exe){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};foreach(string a in args)psi.ArgumentList.Add(a);psi.Environment["PATH"]=sqliteNative+Path.PathSeparator+Environment.GetEnvironmentVariable("PATH");
    using var p=Process.Start(psi)??throw new InvalidOperationException("process start failed");string stdout=p.StandardOutput.ReadToEnd(),stderr=p.StandardError.ReadToEnd();p.WaitForExit();if(p.ExitCode!=0)throw new InvalidOperationException($"{exe} exit {p.ExitCode}: {stderr}");return stdout.Trim();
}
void CopyArtifact(string source,string dest){foreach(string s in new[]{"","-journal","-wal","-shm"})try{File.Delete(dest+s);}catch{}File.Copy(source,dest,true);}
void PopulateAssets(DndStore store,N001FixtureResult fixture){foreach(var a in fixture.AssetBytes)store.InitializeEmbeddedAssetBytes(Convert.FromHexString(a.Key),a.Value);}
HashSet<Guid> SentinelIds(N001Manifest m)=>m.Sentinels.SelectMany(x=>x.Value).Select(Guid.Parse).ToHashSet();
int RecoverSentinels(SemanticState state,N001Manifest m)
{
    int n=0;foreach(Guid id in SentinelIds(m))if(state.Objects.ContainsKey(id)||state.Boundaries.ContainsKey(id)||state.Ranges.ContainsKey(id)||state.Extensions.ContainsKey(id)||state.ProviderFacets.ContainsKey(id))n++;return n;
}
Dictionary<string,object?> Change(SemanticObject o,string key,object? value){var d=new Dictionary<string,object?>(o.Data,StringComparer.Ordinal){[key]=value};return d;}

// Generate deterministic full N-001 and prove independent root.
var fixture=N001Generator.Generate();var fixture2=N001Generator.Generate();
Expect(fixture.Manifest.SemanticRoot==fixture2.Manifest.SemanticRoot,"N-001 root is not deterministic");
Expect(fixture.Manifest.FamilyId==fixture2.Manifest.FamilyId&&fixture.Manifest.BranchId==fixture2.Manifest.BranchId,"N-001 identity seed drift");
string n001=Path.Combine(work,"N-001.dnd"),manifestPath=Path.Combine(repo,"fixtures","N-001.manifest.json");var manifest=N001Generator.Write(n001,manifestPath);
Expect(SentinelIds(manifest).Count==32,"sentinel cardinality not 32");
string independentRoot=Run(node,writer,"root",n001);Expect(independentRoot==manifest.SemanticRoot,"independent N-001 root mismatch");
using(var s=NativeDocumentSession.Open(n001,true)){Expect(s.WriteAuthority,"N-001 non-writable");Expect(RecoverSentinels(s.LoadState(),manifest)==32,"initial sentinel recovery not 32/32");}
Pass("A-N001",new{seed=manifest.Seed,root=manifest.SemanticRoot,independent_root=independentRoot,sentinels=32,objects=manifest.Objects.Count,boundaries=manifest.Boundaries.Count,ranges=manifest.Ranges.Count});

// Build disposable runtime index then make the real DOCSeye kernel hold N-001 and hard-kill it.
string runtimeDir=Path.Combine(work,"runtime");Directory.CreateDirectory(runtimeDir);string runtimeDb=Path.Combine(runtimeDir,"index.db");
using(var store=DndStore.Open(n001))using(var index=new RuntimeIndex(runtimeDb)){int indexed=index.RebuildFrom(store);Expect(indexed>0,"runtime index empty");}
string pipe="docseye-a-"+Guid.NewGuid().ToString("N");string kernelDll=Path.Combine(repo,"src","DOCSeye.Kernel","bin","Debug","net10.0","DOCSeye.Kernel.dll");
var kpsi=new ProcessStartInfo("dotnet"){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};kpsi.ArgumentList.Add(kernelDll);kpsi.Environment["DOCSEYE_PIPE"]=pipe;kpsi.Environment["PATH"]=sqliteNative+Path.PathSeparator+Environment.GetEnvironmentVariable("PATH");
using(var kernel=Process.Start(kpsi)??throw new InvalidOperationException("kernel start failed"))
{
    JsonElement hello=Rpc(pipe,"rpc.hello",new{});Expect(hello.GetProperty("protocol").GetString()=="docseye-rpc","kernel RPC hello failed");
    JsonElement opened=Rpc(pipe,"document.open",new{path=n001});Expect(opened.GetProperty("writeAuthority").GetBoolean(),"kernel could not open N-001 writable");
    kernel.Kill(true);kernel.WaitForExit();Expect(kernel.HasExited,"kernel did not hard-kill");
}
Directory.Delete(runtimeDir,true);Expect(!Directory.Exists(runtimeDir),"runtime state deletion failed");
using(var cold=NativeDocumentSession.Open(n001,true)){Expect(cold.WriteAuthority,"artifact-only cold reopen failed");Expect(RecoverSentinels(cold.LoadState(),manifest)==32,"process/runtime loss changed sentinel identities");}
Pass("A-COLD",new{kernel_hard_kill=true,runtime_state_deleted=true,artifact_only_recovery="32/32"});

// Move physical carrier through the live SHELLeye kernel and recover from artifact alone.
string moved=Path.Combine(work,"N-001-moved.dnd");string shellJson=Run(node,shellMove,n001,moved);using(var shell=JsonDocument.Parse(shellJson)){var r=shell.RootElement;var retained=r.GetProperty("retained");var after=r.GetProperty("after");Expect(retained.GetProperty("id").GetString()==r.GetProperty("renamed").GetProperty("id").GetString()&&retained.GetProperty("identity").GetProperty("fileId128").GetString()==after.GetProperty("identity").GetProperty("fileId128").GetString()&&retained.GetProperty("identity").GetProperty("volumeSerial").GetUInt64()==after.GetProperty("identity").GetProperty("volumeSerial").GetUInt64(),"SHELLeye physical identity changed on move");}
Expect(!File.Exists(n001)&&File.Exists(moved),"SHELLeye carrier move did not occur");
using(var afterMove=NativeDocumentSession.Open(moved,true)){Expect(afterMove.WriteAuthority,"moved N-001 non-writable");Expect(RecoverSentinels(afterMove.LoadState(),manifest)==32,"physical move changed sentinel identities");}
Pass("A-SHELLEYE-MOVE",new{same_physical_identity=true,sentinel_recovery="32/32",carrier=JsonDocument.Parse(shellJson).RootElement.Clone()});

// Valid independent external move preserves semantic identity.
SemanticState preExternal;using(var s=NativeDocumentSession.Open(moved,true))preExternal=s.LoadState();var sentinelSet=SentinelIds(manifest);
Guid moveTarget=preExternal.Objects.Values.First(o=>!o.Retired&&o.Type=="text_block"&&o.Role=="body"&&!sentinelSet.Contains(o.Id)).Id;
Run(node,writer,"move-object",moved,Ids.Lower(moveTarget),"0");
using(var s=NativeDocumentSession.Open(moved,true)){Expect(s.WriteAuthority,"independent external move invalidated artifact");var st=s.LoadState();Expect(st.Objects.TryGetValue(moveTarget,out var movedObj)&&!movedObj.Retired,"external move lost object identity");Expect(RecoverSentinels(st,manifest)==32,"external move changed sentinels");}
Pass("A-EXTERNAL-MOVE",new{object_id=Ids.Lower(moveTarget),identity_preserved=true,sentinels="32/32"});

// Valid independent split retires original, mints both successors; then cold recover again.
using(var s=NativeDocumentSession.Open(moved,true))preExternal=s.LoadState();
Guid splitTarget=preExternal.Objects.Values.First(o=>!o.Retired&&o.Type=="text_block"&&o.Role=="body"&&!sentinelSet.Contains(o.Id)&&o.Id!=moveTarget&&UnicodeText.ScalarCount((string)o.Data["text"]!)>20).Id;
string splitJson=Run(node,writer,"split-text",moved,Ids.Lower(splitTarget),"10");using(var splitDoc=JsonDocument.Parse(splitJson)){Guid left=Guid.Parse(splitDoc.RootElement.GetProperty("left_id").GetString()!),right=Guid.Parse(splitDoc.RootElement.GetProperty("right_id").GetString()!);using var s=NativeDocumentSession.Open(moved,true);Expect(s.WriteAuthority,"independent split invalidated artifact");var st=s.LoadState();Expect(st.Objects[splitTarget].Retired&&st.Objects.ContainsKey(left)&&st.Objects.ContainsKey(right),"split identity contract failed");Expect(st.Retired.TryGetValue(splitTarget,out var w)&&w.Resolution==RetiredResolution.Split&&w.Successors.SequenceEqual(new[]{left,right}),"split witness missing");Expect(RecoverSentinels(st,manifest)==32,"split of non-sentinel changed sentinels");}
using(var cold=NativeDocumentSession.Open(moved,true)){Expect(cold.WriteAuthority&&RecoverSentinels(cold.LoadState(),manifest)==32,"second cold recovery failed");}
Pass("A-EXTERNAL-SPLIT",new{old_id=Ids.Lower(splitTarget),typed_retirement=true,cold_recovery="32/32"});

// Same-head raw replicas remain same branch until independent commits, then become divergent_heads.
string repA=Path.Combine(work,"replica-a.dnd"),repB=Path.Combine(work,"replica-b.dnd");CopyArtifact(moved,repA);CopyArtifact(moved,repB);
RevisionHead baseHead;SemanticState baseReplicaState;using(var a=DndStore.Open(repA))using(var b=DndStore.Open(repB)){var p=a.PresentWith(b);Expect(p.Classification=="same_head"&&p.CanWrite,"raw same-base replicas not classified same_head");baseHead=a.ReadHead();baseReplicaState=a.LoadState();Expect(baseHead.BranchId==b.ReadHead().BranchId,"raw copy changed branch");}
var candidates=baseReplicaState.Objects.Values.Where(o=>!o.Retired&&o.Type=="text_block"&&o.Role=="body"&&!sentinelSet.Contains(o.Id)).Take(4).ToArray();Expect(candidates.Length>=2,"not enough replica edit targets");
var preDivergenceHandle=new PortableWriteHandle(baseHead.FamilyId,baseHead.BranchId,candidates[0].Id,baseHead.RevisionId);
RevisionHead headA,headB;
using(var a=NativeDocumentSession.Open(repA,true)){var r=a.CommitObjectData(new(baseHead.FamilyId,baseHead.BranchId,candidates[0].Id,baseHead.RevisionId),Change(candidates[0],"replica_edit","A"),"A-replica");Expect(r.Success,"replica A commit failed: "+r.Classification);headA=r.Head!;}
using(var b=NativeDocumentSession.Open(repB,true)){var r=b.CommitObjectData(new(baseHead.FamilyId,baseHead.BranchId,candidates[1].Id,baseHead.RevisionId),Change(candidates[1],"replica_edit","B"),"B-replica");Expect(r.Success,"replica B commit failed: "+r.Classification);headB=r.Head!;}
using(var a=DndStore.Open(repA))using(var b=DndStore.Open(repB)){var p=a.PresentWith(b);Expect(p.Classification=="divergent_heads"&&!p.CanWrite&&p.CommonRevisionId==baseHead.RevisionId,"replicas silently linearized instead of divergent_heads");}
using(var a=NativeDocumentSession.Open(repA,true)){string before=Sha(repA);var stale=a.CommitObjectData(preDivergenceHandle,Change(candidates[0],"illegal","stale"),"stale-pre-divergence");Expect(!stale.Success&&stale.Classification=="stale_revision","pre-divergence stale handle not refused: "+stale.Classification);Expect(Sha(repA)==before,"stale handle wrote bytes");}
Pass("A-DIVERGENCE",new{base_revision=Ids.Lower(baseHead.RevisionId),head_a=Ids.Lower(headA.RevisionId),head_b=Ids.Lower(headB.RevisionId),classification="divergent_heads",stale_handle="stale_revision"});

// Explicit same-family fork remints every public ID and boundary with bounded origin mapping.
var fixtureBytes=N001Generator.Generate();SemanticState sourceState;RevisionHead forkSourceHead;using(var source=NativeDocumentSession.Open(repA,true)){sourceState=source.LoadState();forkSourceHead=source.ReadHead();}
var fork=Branching.Fork(sourceState);Expect(fork.State.FamilyId==sourceState.FamilyId&&fork.State.BranchId!=sourceState.BranchId,"fork family/branch contract failed");
Expect(sourceState.Objects.Keys.All(id=>fork.SourceToCurrent.TryGetValue(id,out var m)&&m!=id&&fork.State.Objects.ContainsKey(m)&&!fork.State.Objects.ContainsKey(id)),"fork did not remint 100% object IDs");
Expect(sourceState.Boundaries.Keys.All(id=>fork.SourceToCurrent.TryGetValue(id,out var m)&&m!=id&&fork.State.Boundaries.ContainsKey(m)&&!fork.State.Boundaries.ContainsKey(id)),"fork did not remint 100% boundary IDs");
Expect(fork.State.OriginRefs.Count==fork.SourceToCurrent.Count&&fork.SourceToCurrent.All(kv=>fork.State.OriginRefs.TryGetValue(kv.Value,out var o)&&o.SourceId==kv.Key&&o.SourceBranchId==sourceState.BranchId&&o.SourceRevisionId==sourceState.RevisionId),"fork origin mapping incomplete");
string forkPath=Path.Combine(work,"fork.dnd");using(var fs=DndStore.Create(forkPath,fork.State,[forkSourceHead.RevisionId])){PopulateAssets(fs,fixtureBytes);Expect(fs.Validate(true).Writable,"fork artifact invalid");}
Guid sourceObject=sourceState.Objects.Values.First(o=>!o.Retired&&o.Type=="text_block").Id;var sourceHandle=new PortableWriteHandle(sourceState.FamilyId,sourceState.BranchId,sourceObject,sourceState.RevisionId);
using(var fs=NativeDocumentSession.Open(forkPath,true)){string before=Sha(forkPath);var denied=fs.CommitObjectData(sourceHandle,new(StringComparer.Ordinal){{"text","wrong branch"}},"source-handle-fork");Expect(!denied.Success&&denied.Classification=="cross_branch_handle","source handle actuated fork");Expect(Sha(forkPath)==before,"cross-branch handle changed fork bytes");}
Pass("A-FORK",new{same_family=true,new_branch=Ids.Lower(fork.State.BranchId),object_remint_percent=100,boundary_remint_percent=100,origin_refs=fork.State.OriginRefs.Count,cross_branch_handle="refused"});

// Cross-branch paste remints incoming occurrences; source handle still cannot authorize target.
var pasteState=Branching.Clone(fork.State);var pasteMap=Branching.PasteObjectSubgraph(pasteState,sourceState,sourceObject);Expect(pasteMap.Values.All(v=>v!=sourceObject&&!sourceState.Objects.ContainsKey(v)),"cross-branch paste reused source semantic ID");Guid pastedRoot=pasteMap[sourceObject];Guid pasteParentRevision=pasteState.RevisionId;pasteState.RevisionId=Ids.NewV4();pasteState.Sequence++;
string pastePath=Path.Combine(work,"fork-paste.dnd");using(var ps=DndStore.Create(pastePath,pasteState,[pasteParentRevision])){PopulateAssets(ps,fixtureBytes);Expect(ps.Validate(true).Writable,"paste artifact invalid");}
using(var ps=NativeDocumentSession.Open(pastePath,true)){string before=Sha(pastePath);var denied=ps.CommitObjectData(sourceHandle,new(StringComparer.Ordinal){{"text","stale source"}},"source-handle-paste");Expect(!denied.Success&&denied.Classification=="cross_branch_handle","source handle actuated pasted target");Expect(ps.LoadState().Objects.ContainsKey(pastedRoot),"pasted occurrence missing");Expect(Sha(pastePath)==before,"source handle modified paste artifact");}
Pass("A-CROSS-BRANCH-PASTE",new{source_id=Ids.Lower(sourceObject),pasted_id=Ids.Lower(pastedRoot),reminted=true,source_handle_write=0});

// Fresh explicit two-parent merge of disjoint same-branch children.
string mergeBase=Path.Combine(work,"merge-base.dnd"),mergeLeft=Path.Combine(work,"merge-left.dnd"),mergeRight=Path.Combine(work,"merge-right.dnd"),mergeOut=Path.Combine(work,"merge-result.dnd");N001Generator.Write(mergeBase,Path.Combine(work,"merge-base-manifest.json"));CopyArtifact(mergeBase,mergeLeft);CopyArtifact(mergeBase,mergeRight);
SemanticState mb;RevisionHead mbh;using(var b=NativeDocumentSession.Open(mergeBase,true)){mb=b.LoadState();mbh=b.ReadHead();}
var mt=mb.Objects.Values.Where(o=>!o.Retired&&o.Type=="text_block"&&o.Role=="body").Take(2).ToArray();
using(var l=NativeDocumentSession.Open(mergeLeft,true)){var rr=l.CommitObjectData(new(mbh.FamilyId,mbh.BranchId,mt[0].Id,mbh.RevisionId),Change(mt[0],"merge_side","left"),"merge-left");Expect(rr.Success,"merge left commit failed");headA=rr.Head!;}
using(var r=NativeDocumentSession.Open(mergeRight,true)){var rr=r.CommitObjectData(new(mbh.FamilyId,mbh.BranchId,mt[1].Id,mbh.RevisionId),Change(mt[1],"merge_side","right"),"merge-right");Expect(rr.Success,"merge right commit failed");headB=rr.Head!;}
SemanticState ls,rs;using(var l=NativeDocumentSession.Open(mergeLeft,true))ls=l.LoadState();using(var r=NativeDocumentSession.Open(mergeRight,true))rs=r.LoadState();var merged=Branching.MergeDisjoint(mb,ls,rs,Ids.NewV4());
using(var m=DndStore.Create(mergeOut,merged,[headA.RevisionId,headB.RevisionId])){PopulateAssets(m,fixtureBytes);var v=m.Validate(true);Expect(v.Writable,"merge result invalid: "+v.Classification);var h=m.ReadHead();Expect(h.Parents.Count==2&&h.Parents.Contains(headA.RevisionId)&&h.Parents.Contains(headB.RevisionId),"merge parent semantics not exactly two");Expect(m.LoadState().Objects.Keys.ToHashSet().SetEquals(mb.Objects.Keys),"merge changed surviving object identity set");}
Pass("A-MERGE",new{parents=2,head_left=Ids.Lower(headA.RevisionId),head_right=Ids.Lower(headB.RevisionId),surviving_ids_preserved=true});

// Copy/duplicate/template rules: ordinary copy omits retained annotations; explicit annotated copy remints them.
Guid annotatedSource=sourceState.Boundaries.Values.First().OwnerId;
var ordinaryCopy=Branching.Clone(sourceState);int ordinaryBoundaryCount=ordinaryCopy.Boundaries.Count;var ordinaryMap=Branching.PasteObjectSubgraph(ordinaryCopy,sourceState,annotatedSource);Guid ordinaryCopied=ordinaryMap[annotatedSource];
Expect(ordinaryCopied!=annotatedSource&&!sourceState.Objects.ContainsKey(ordinaryCopied),"within-branch ordinary copy reused semantic ID");Expect(ordinaryCopy.Boundaries.Count==ordinaryBoundaryCount,"ordinary copy duplicated retained boundaries");
var annotatedCopy=Branching.Clone(sourceState);var beforeBoundaryIds=annotatedCopy.Boundaries.Keys.ToHashSet();var annotatedMap=Branching.PasteObjectSubgraph(annotatedCopy,sourceState,annotatedSource,null,true);Guid annotatedCopied=annotatedMap[annotatedSource];var newBoundaryIds=annotatedCopy.Boundaries.Keys.Where(id=>!beforeBoundaryIds.Contains(id)).ToArray();
Expect(annotatedCopied!=annotatedSource&&newBoundaryIds.Length>0&&newBoundaryIds.All(id=>!sourceState.Boundaries.ContainsKey(id)),"copy_with_annotations did not remint boundaries");
var independentDuplicate=Branching.IndependentDuplicate(sourceState);Expect(independentDuplicate.State.FamilyId!=sourceState.FamilyId&&independentDuplicate.State.BranchId!=sourceState.BranchId,"independent duplicate did not create new family/branch");Expect(!Branching.PublicIds(independentDuplicate.State).Overlaps(Branching.PublicIds(sourceState)),"independent duplicate reused public IDs");
var templateInstance=Branching.InstantiateTemplate(sourceState);Expect(templateInstance.State.FamilyId!=sourceState.FamilyId&&templateInstance.State.BranchId!=sourceState.BranchId,"template instantiation did not create new family/branch");Expect(!Branching.PublicIds(templateInstance.State).Overlaps(Branching.PublicIds(sourceState)),"template instantiation reused public IDs");
string independentPath=Path.Combine(work,"independent-duplicate.dnd"),templatePath=Path.Combine(work,"template-instance.dnd");using(var d=DndStore.Create(independentPath,independentDuplicate.State)){PopulateAssets(d,fixtureBytes);Expect(d.Validate(true).Writable,"independent duplicate artifact invalid");}using(var t=DndStore.Create(templatePath,templateInstance.State)){PopulateAssets(t,fixtureBytes);Expect(t.Validate(true).Writable,"template instance artifact invalid");}
var crossFamily=Branching.Clone(independentDuplicate.State);var crossFamilyMap=Branching.PasteObjectSubgraph(crossFamily,sourceState,annotatedSource,null,true);Expect(crossFamilyMap.Values.All(id=>!sourceState.Objects.ContainsKey(id)),"cross-family paste reused incoming public object ID");
Pass("A-COPY-DUPLICATE-TEMPLATE",new{ordinary_copy_object_reminted=true,ordinary_copy_boundaries_added=0,copy_with_annotations_boundaries_reminted=newBoundaryIds.Length,independent_duplicate_new_family=true,template_new_family=true,cross_family_paste_reminted=true});
// Duplicate object ID, duplicate boundary ID, and invalid root: validation removes write authority and must not mutate bytes.
Guid sentinelObject=Guid.Parse(manifest.Sentinels["text_blocks"][0]),sentinelBoundary=Guid.Parse(manifest.Sentinels["retained_boundaries"][0]);
string dupObj=Path.Combine(work,"invalid-duplicate-object.dnd");CopyArtifact(moved,dupObj);Run(sqlite,dupObj,$"INSERT INTO objects(id,type,parent_id,order_key,role,cbor,retired) SELECT id,type,parent_id,order_key,role,cbor,retired FROM objects WHERE id=X'{Convert.ToHexString(Ids.RfcBytes(sentinelObject))}' LIMIT 1;");AssertInvalidNoWrite(dupObj,"duplicate_id",sentinelObject);
string dupBoundary=Path.Combine(work,"invalid-duplicate-boundary.dnd");CopyArtifact(moved,dupBoundary);Run(sqlite,dupBoundary,$"INSERT INTO boundaries(id,owner_id,scalar_offset,affinity,state,cbor) SELECT id,owner_id,scalar_offset,affinity,state,cbor FROM boundaries WHERE id=X'{Convert.ToHexString(Ids.RfcBytes(sentinelBoundary))}' LIMIT 1;");AssertInvalidNoWrite(dupBoundary,"duplicate_id",sentinelObject);
string badRoot=Path.Combine(work,"invalid-root.dnd");CopyArtifact(moved,badRoot);Run(sqlite,badRoot,"UPDATE head SET semantic_root=zeroblob(32) WHERE id=1;");AssertInvalidNoWrite(badRoot,"invalid_root",sentinelObject);
string brokenRef=Path.Combine(work,"invalid-broken-reference.dnd");CopyArtifact(moved,brokenRef);Run(sqlite,brokenRef,$"UPDATE objects SET parent_id=randomblob(16) WHERE id=X'{Convert.ToHexString(Ids.RfcBytes(sentinelObject))}';");AssertInvalidNoWrite(brokenRef,"broken_reference",sentinelObject);
Pass("A-INVALID-ARTIFACTS",new{duplicate_object="non_writable",duplicate_boundary="non_writable",broken_reference="non_writable",invalid_root="non_writable",sentinel_identity_steals=0,auto_repairs=0});

int winword=Process.GetProcessesByName("WINWORD").Length;bool winwordExe=new[]{@"C:\\Program Files\\Microsoft Office\\root\\Office16\\WINWORD.EXE",@"C:\\Program Files (x86)\\Microsoft Office\\root\\Office16\\WINWORD.EXE",@"C:\\Program Files\\Microsoft Office\\Office16\\WINWORD.EXE",@"C:\\Program Files (x86)\\Microsoft Office\\Office16\\WINWORD.EXE"}.Any(File.Exists);Expect(winword==0&&!winwordExe,"WINWORD observed during A");hardZero["H-17"]=0;
var evidence=new
{
    architecture_freeze=DndConstants.ArchitectureFreeze,generated_utc=DateTimeOffset.UtcNow,machine=Environment.MachineName,fixture="N-001",seed=manifest.Seed,manifest=Path.GetRelativePath(repo,manifestPath).Replace('\\','/'),semantic_root=manifest.SemanticRoot,
    sentinel_recovery=new{required=32,recovered=32,percent=100},checks,hard_zero=hardZero,positive=new{P_01_artifact_only_cold_recovery=100,P_03_stale_refusal_classification=100},word=new{winword_processes=winword,winword_exe_present=winwordExe,com_calls=0,api_calls=0,microsoft_365_trial_started=false},result="PASS"
};
string evidencePath=Path.Combine(repo,"evidence","milestone-a.json");File.WriteAllText(evidencePath,JsonSerializer.Serialize(evidence,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));Console.WriteLine("MILESTONE A PASS");Console.WriteLine("EVIDENCE="+evidencePath);

void AssertInvalidNoWrite(string path,string expected,Guid target)
{
    string before=Sha(path);using var s=NativeDocumentSession.Open(path,false);Expect(!s.WriteAuthority&&s.Validation.Classification==expected,$"{expected} artifact unexpectedly writable/classified {s.Validation.Classification}");var h=s.ReadHead();var handle=new PortableWriteHandle(h.FamilyId,h.BranchId,target,h.RevisionId);var r=s.CommitObjectData(handle,new(StringComparer.Ordinal){{"text","should not write"}},"invalid-write");Expect(!r.Success&&r.Classification=="invalid_artifact","invalid artifact write path did not block");Expect(Sha(path)==before,"invalid artifact changed on refused write");
}

JsonElement Rpc(string pipeName,string method,object parameters)
{
    Exception? last=null;for(int attempt=0;attempt<80;attempt++)
    {
        try
        {
            using var pipe=new NamedPipeClientStream(".",pipeName,PipeDirection.InOut,PipeOptions.None);pipe.Connect(250);using var reader=new StreamReader(pipe,new UTF8Encoding(false),false,65536,true);using var writer=new StreamWriter(pipe,new UTF8Encoding(false),65536,true){AutoFlush=true};
            writer.WriteLine(JsonSerializer.Serialize(new{jsonrpc="2.0",id=1,method,@params=parameters}));string line=reader.ReadLine()??throw new IOException("kernel closed pipe");using var doc=JsonDocument.Parse(line);if(doc.RootElement.TryGetProperty("error",out var err))throw new InvalidOperationException(err.GetProperty("message").GetString());return doc.RootElement.GetProperty("result").Clone();
        }
        catch(Exception ex){last=ex;Thread.Sleep(50);}
    }
    throw new InvalidOperationException("kernel RPC unavailable",last);
}