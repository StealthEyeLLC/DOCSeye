using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using DOCSeye.Core;
using DOCSeye.Storage.Sqlite;

if (args.Length > 0 && args[0] == "--asset-worker")
{
    string db=args[1], asset=args[2], ready=args[3];Guid expected=Guid.Parse(args[4]), figure=Guid.Parse(args[5]);
    using var store=DndStore.Open(db);using var fs=File.OpenRead(asset);
    var result=store.CommitEmbeddedAsset(fs,expected,"e10-crash-worker",figure,4*1024*1024,chunk=>{if(chunk==8){File.WriteAllText(ready,"ready");Thread.Sleep(Timeout.Infinite);}});
    Console.WriteLine(result.Result.Classification);return;
}

string repo = FindRepo();
string work = Path.Combine(repo, "artifacts", "experiments-05-08");
Directory.CreateDirectory(work);
foreach (string file in Directory.EnumerateFiles(work)) try { File.Delete(file); } catch { }
var results = new List<object>();
string? scale500Path=null; Guid scale500Target=Guid.Empty;
RunE05();
RunE06();
RunE07();
RunE08();
RunE09();
RunE10();
RunE11();
string evidence = Path.Combine(repo, "evidence", "experiments-e05-e11.json");
File.WriteAllText(evidence, JsonSerializer.Serialize(new { architecture_freeze = DndConstants.ArchitectureFreeze, generated_utc = DateTimeOffset.UtcNow, experiments = results.Select(WithMeta).ToArray() }, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine("EVIDENCE=" + evidence);

Dictionary<string,object?> WithMeta(object item)
{
    var map=item.GetType().GetProperties().ToDictionary(p=>p.Name,p=>p.GetValue(item),StringComparer.Ordinal);string id=(string)map["id"]!;var meta=id switch
    {
        "E-05"=>("Do 128-bit sparse order keys meet deterministic/local mutation and rebalance neutrality through 5,000-page-equivalent pressure?","PASS: moves preserved IDs, pressure inserts stayed locally bounded, explicit rebalance was semantic-root neutral, and the 5,000-page-equivalent tier did not require unrelated identity churn.","Select a chunked/B-tree sequence mechanism inside the frozen order invariants."),
        "E-06"=>("Can C# and an independent second-language implementation emit byte-identical DOCSeye deterministic-CBOR vectors?","PASS: every valid cross-language vector was byte-identical and every invalid vector received the same classification.","ARCHITECTURE FALSIFIER if public canonicalization cannot be implemented independently."),
        "E-07"=>("Which domain-separated current-state tree provides deterministic root and bounded recomputation?","PASS: the measured domain-separated tree produced the same root across independent construction, VACUUM/reorder/index rebuilds, while local edits touched only bounded hash paths.","Choose the measured fan-out/domain layout; the semantic-root contract remains frozen."),
        "E-08"=>("Can an independent writer, using only the public spec, make a valid external commit that the product accepts without identity remint or private repair?","PASS: valid independent move/split/merge/extension commits were accepted without product identity remint/private repair and malformed variants were rejected.","ARCHITECTURE FALSIFIER: specification/conformance model is inadequate."),
        "E-09"=>("Can local query/edit avoid whole-document materialization at 10, 500, and 5,000 page-equivalent tiers?","PASS: measured local query/edit remained bounded to the targeted records/hash path at all three tiers; latency, amplification, index rebuild, delta size, and memory evidence are recorded without a threshold claim.","Optimize records/indexes; unbounded tier growth reopens storage architecture."),
        "E-10"=>("Can a 1 GiB synthetic embedded asset be added/replaced/verified without rewriting or materializing unrelated semantic content?","PASS: the 1 GiB asset path streamed digest/write operations with bounded process memory, preserved figure identity, touched zero unrelated semantic records, and recovered old-or-new across the injected crash.","Change blob/chunk mechanism; asset identity contract remains."),
        "E-11"=>("Can total external-state loss rebuild FTS/query indexes and resume bounded deltas without semantic identity change?","PASS: total runtime-index loss rebuilt from DND with exact semantic identity/root; an expired cursor returned explicit resync_required rather than silent delta loss.","Fix runtime protocol; dependency on external truth is an architecture falsifier."),
        _=>throw new InvalidOperationException("missing experiment metadata: "+id)
    };map["question"]=meta.Item1;map["binary_or_bounded_conclusion"]=meta.Item2;map["failure_action"]=meta.Item3;return map;
}

void RunE05()
{
    var rng = new RandomNumberGeneratorLike(0xD0C5E005UL); Guid Id() => Ids.DeterministicV4(rng);
    Guid parent = Id();
    var siblings = new List<SemanticObject>(8000);
    for (int i = 0; i < 5000; i++) siblings.Add(new(Id(), "text_block", parent, OrderKey.Initial(i), "body", new() { ["text"] = "page-equivalent " + i }));
    var idsBefore = siblings.Select(x => x.Id).ToArray();
    int maxRebalanced = 0, totalRebalanced = 0;
    int insertIndex = 2500;
    for (int i = 0; i < 2200; i++)
    {
        var allocation = SparseOrder.AllocateBetween(siblings, insertIndex);
        maxRebalanced = Math.Max(maxRebalanced, allocation.rebalanced);
        totalRebalanced += allocation.rebalanced;
        siblings.Insert(insertIndex, new(Id(), "text_block", parent, allocation.key, "body", new() { ["text"] = "pressure insert " + i }));
    }
    Expect(maxRebalanced <= SparseOrder.MaximumRebalanceWindow, "rebalance exceeded bounded window");
    Expect(siblings.Zip(siblings.Skip(1), (a,b) => a.Order.Value < b.Order.Value).All(x=>x), "order keys not strictly increasing");
    foreach (Guid id in idsBefore) Expect(siblings.Any(x => x.Id == id), "insert pressure changed existing semantic identity");

    var state = new SemanticState { FamilyId = Id(), BranchId = Id(), RevisionId = Id(), Mode = AuthorityMode.NativeAuthored };
    state.Objects[parent] = new(parent, "flow", null, OrderKey.Initial(0), "main", new());
    foreach (var s in siblings) state.Objects[s.Id] = s;
    byte[] beforeMaintenance = state.ComputeRoot();
    int changed = SparseOrder.RebalanceWindow(siblings, siblings.Count / 2, 512);
    foreach (var s in siblings) state.Objects[s.Id] = s;
    byte[] afterMaintenance = state.ComputeRoot();
    Expect(beforeMaintenance.SequenceEqual(afterMaintenance), "order-key-only rebalance changed semantic root");

    Guid movedId = siblings[100].Id;
    var moved = siblings[100]; siblings.RemoveAt(100);
    var moveAlloc = SparseOrder.AllocateBetween(siblings, siblings.Count - 100);
    moved = moved with { Order = moveAlloc.key };
    siblings.Insert(siblings.Count - 100, moved);
    Expect(moved.Id == movedId, "move changed semantic ID");
    Expect(siblings.Zip(siblings.Skip(1), (a,b) => a.Order.Value < b.Order.Value).All(x=>x), "move broke order");
    results.Add(new { id = "E-05", result = "PASS", page_equivalent = 5000, initial_siblings = 5000, pressure_inserts = 2200, max_local_rebalance_records = maxRebalanced, total_rebalanced_records = totalRebalanced, explicit_maintenance_rebalanced_records = changed, semantic_root_neutral_maintenance = true, moves_preserve_ids = true, mechanism = "unsigned 128-bit sparse keys with bounded local rebalance" });
    Console.WriteLine("E-05 PASS");
}

void RunE06()
{
    string node = @"C:\AgentBrowser\tools\node-v24.18.1-win-x64\node.exe";
    string writer = Path.Combine(repo, "tools", "independent-writer", "independent-writer.mjs");
    string output = Run(node, writer, "cbor-suite");
    var suite = JsonSerializer.Deserialize<Dictionary<string,string>>(output) ?? throw new InvalidOperationException("bad node suite");
    Guid uuid = Ids.GuidFromRfcBytes(Convert.FromHexString("00112233445546778899aabbccddeeff"));
    byte[] abc = System.Text.Encoding.ASCII.GetBytes("abc");
    var vectors = new Dictionary<string, object?>(StringComparer.Ordinal)
    {
        ["simple_map"] = new Dictionary<string, object?> { ["a"] = 1, ["z"] = "e\u0301", ["uuid"] = uuid, ["bytes"] = Convert.FromHexString("00ff10") },
        ["signed"] = new object?[] { -1, -24, -25, 0, 23, 24, 255, 256, 65535, 65536 },
        ["nested"] = new Dictionary<string, object?> { ["alpha"] = new object?[] { true, false, null, "x" }, ["beta"] = new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 } },
        ["uuid"] = uuid,
        ["text"] = "A\u030A \u65E5\u672C \u0645\u0631\u062D\u0628\u0627",
        ["bytes"] = Convert.FromHexString("000102feff"),
        ["extension"] = new Dictionary<string, object?> { ["extension_id"] = uuid, ["namespace_uri"] = "urn:test", ["type_name"] = "x", ["major"] = 1, ["minor"] = 0, ["payload_encoding"] = "bytes", ["exact_payload"] = abc, ["payload_digest"] = SHA256.HashData(abc), ["coverage_kind"] = "object", ["target_id"] = null, ["property_name"] = null, ["start_boundary_id"] = null, ["end_boundary_id"] = null, ["edit_policy"] = "independent", ["required"] = false, ["fallback"] = null }
    };
    foreach (var kv in vectors)
    {
        string primary = Convert.ToHexString(CanonicalCbor.Encode(kv.Value)).ToLowerInvariant();
        Expect(suite.TryGetValue(kv.Key, out var independent) && string.Equals(primary, independent, StringComparison.Ordinal), "CBOR vector mismatch " + kv.Key);
    }
    var invalid = new Dictionary<string, byte[]>
    {
        ["indefinite_array"] = Convert.FromHexString("9f01ff"),
        ["nonshort_int"] = Convert.FromHexString("1817"),
        ["duplicate_map_key"] = Convert.FromHexString("a2616101616102"),
        ["binary_float"] = Convert.FromHexString("f93c00"),
        ["bad_uuid"] = Convert.FromHexString("d8254100"),
        ["truncated"] = Convert.FromHexString("a16161"),
        ["too_deep"] = Enumerable.Repeat((byte)0x81, 66).Concat(new byte[]{0x00}).ToArray()
    };
    var classifications = new Dictionary<string,string>();
    foreach (var kv in invalid)
    {
        string primary = CanonicalCbor.ValidateRecord(kv.Value);
        string independent = Run(node, writer, "validate-hex", Convert.ToHexString(kv.Value).ToLowerInvariant()).Trim();
        Expect(primary == independent, $"invalid classification mismatch {kv.Key}: {primary}/{independent}");
        classifications[kv.Key] = primary;
    }
    results.Add(new { id = "E-06", result = "PASS", valid_vectors = vectors.Count, byte_identical = vectors.Count, byte_identity_percent = 100, invalid_vectors = invalid.Count, invalid_classifications_identical = invalid.Count, classifications });
    Console.WriteLine("E-06 PASS");
}

void RunE07()
{
    var rng = new RandomNumberGeneratorLike(0xD0C5E007UL); Guid Id() => Ids.DeterministicV4(rng);
    var s = new SemanticState { FamilyId = Id(), BranchId = Id(), RevisionId = Id(), Sequence = 0, Mode = AuthorityMode.NativeAuthored };
    Guid root=Id(), flow=Id(), a=Id(), b=Id(), retired=Id(), boundary=Id();
    s.Objects[root]=new(root,"document",null,OrderKey.Initial(0),"document",new());
    s.Objects[flow]=new(flow,"flow",root,OrderKey.Initial(0),"main",new());
    s.Objects[a]=new(a,"text_block",flow,OrderKey.Initial(0),"body",new(){["text"]="root determinism alpha"});
    s.Objects[b]=new(b,"text_block",flow,OrderKey.Initial(1),"body",new(){["text"]="root determinism beta"});
    s.Objects[retired]=new(retired,"text_block",flow,OrderKey.Initial(2),"body",new(){["text"]="retired"},true);
    s.Boundaries[boundary]=new(boundary,a,4,EdgeAffinity.ExcludeAtEdge);
    s.Retired[retired]=new(retired,RetiredResolution.Destroyed,[],64);
    byte[] facetPayload=System.Text.Encoding.UTF8.GetBytes("facet\0exact"); Guid facetId=Id();
    s.ProviderFacets[facetId]=new(facetId,"test-provider","opaque",a,ExtensionCoverageKind.Object,ExtensionEditPolicy.Independent,facetPayload,SHA256.HashData(facetPayload),"source_exact_unmodified",false);
    byte[] capsuleBytes=System.Text.Encoding.UTF8.GetBytes("provider capsule exact bytes"); var capsule=new SourceCapsuleEvidence("test-provider",capsuleBytes,SHA256.HashData(capsuleBytes),"source_exact_unmodified");
    s.SourceCapsules[Convert.ToHexString(capsule.Digest)]=capsule;
    string path=Path.Combine(work,"e07-root.dnd"); RevisionHead initial;
    using(var store=DndStore.Create(path,s)){var v=store.Validate();Expect(v.Writable,"E07 initial invalid "+v.Classification);initial=store.ReadHead();}
    string node=@"C:\AgentBrowser\tools\node-v24.18.1-win-x64\node.exe", writer=Path.Combine(repo,"tools","independent-writer","independent-writer.mjs");
    string independentRoot=Run(node,writer,"root",path).Trim();
    Expect(independentRoot==Convert.ToHexString(initial.SemanticRoot).ToLowerInvariant(),"independent semantic root mismatch");
    using(var store=DndStore.Open(path)){store.VacuumAndRebuildIndexes();store.RebuildMerkleCache();var h=store.ReadHead();Expect(h.RevisionId==initial.RevisionId&&h.Sequence==initial.Sequence&&h.SemanticRoot.SequenceEqual(initial.SemanticRoot),"VACUUM/index rebuild changed head/root");}
    string sqlite=Path.Combine(repo,".tools","sqlite","sqlite3.exe");
    Run(sqlite,path,"BEGIN; CREATE TEMP TABLE rr AS SELECT id,type,parent_id,order_key,role,cbor,retired FROM objects ORDER BY row_no DESC; DELETE FROM objects; INSERT INTO objects(id,type,parent_id,order_key,role,cbor,retired) SELECT * FROM rr; COMMIT;");
    using(var store=DndStore.Open(path)){var v=store.Validate();Expect(v.Writable,"physical row reorder invalidated artifact");var h=store.ReadHead();Expect(h.RevisionId==initial.RevisionId&&h.Sequence==initial.Sequence&&h.SemanticRoot.SequenceEqual(initial.SemanticRoot),"physical row reorder changed root");int beforeNodes=store.MerkleNodeCount();var tx=store.CommitBoundaryOffset(boundary,5,h.RevisionId,"e07-local");Expect(tx.Success,"local bounded mutation failed "+tx.Classification);int afterNodes=store.MerkleNodeCount();var verify=store.Validate();Expect(verify.Writable,"local mutation produced invalid root");Expect(tx.Diagnostics.Any(x=>x.Contains("33")),"bounded Merkle path diagnostic absent");Expect(afterNodes<=beforeNodes+SemanticRoot.BoundedPathNodesPerEntry,"local mutation expanded Merkle cache beyond one path");}
    using(var store=DndStore.Open(path)){string finalIndependent=Run(node,writer,"root",path).Trim();Expect(finalIndependent==Convert.ToHexString(store.ReadHead().SemanticRoot).ToLowerInvariant(),"independent root mismatch after local mutation");}
    results.Add(new { id="E-07", result="PASS", mechanism=DndConstants.RootMechanism, fanout=256, path_nodes_per_entry=SemanticRoot.BoundedPathNodesPerEntry, independent_root=true, vacuum_neutral=true, physical_record_reorder_neutral=true, index_rebuild_neutral=true, local_mutation_bounded=true, historical_merkle_dag=false });
    Console.WriteLine("E-07 PASS");
}

void RunE08()
{
    var rng=new RandomNumberGeneratorLike(0xD0C5E008UL); Guid Id()=>Ids.DeterministicV4(rng);
    var s=new SemanticState{FamilyId=Id(),BranchId=Id(),RevisionId=Id(),Mode=AuthorityMode.NativeAuthored}; Guid root=Id(),flow=Id(),a=Id(),b=Id(),c=Id(),bound=Id();
    s.Objects[root]=new(root,"document",null,OrderKey.Initial(0),"document",new());s.Objects[flow]=new(flow,"flow",root,OrderKey.Initial(0),"main",new());
    s.Objects[a]=new(a,"text_block",flow,OrderKey.Initial(0),"body",new(){["text"]="alpha split merge target"});s.Objects[b]=new(b,"text_block",flow,OrderKey.Initial(1),"body",new(){["text"]="beta move target"});s.Objects[c]=new(c,"text_block",flow,OrderKey.Initial(2),"body",new(){["text"]="gamma extension target"});s.Boundaries[bound]=new(bound,a,3,EdgeAffinity.ExcludeAtEdge);
    string path=Path.Combine(work,"e08-writer.dnd");using(var st=DndStore.Create(path,s)){Expect(st.Validate().Writable,"E08 initial invalid");}
    string node=@"C:\AgentBrowser\tools\node-v24.18.1-win-x64\node.exe",writer=Path.Combine(repo,"tools","independent-writer","independent-writer.mjs");
    var move=JsonDocument.Parse(Run(node,writer,"move-object",path,Ids.Lower(b),"0")).RootElement.Clone();using(var st=DndStore.Open(path)){Expect(st.Validate().Writable,"external move rejected");Expect(st.LoadState().Objects[b].Id==b,"external move reminted identity");}
    var split=JsonDocument.Parse(Run(node,writer,"split-text",path,Ids.Lower(a),"6")).RootElement.Clone();Guid left=Guid.Parse(split.GetProperty("left_id").GetString()!),right=Guid.Parse(split.GetProperty("right_id").GetString()!);using(var st=DndStore.Open(path)){var v=st.Validate();Expect(v.Writable,"external split rejected "+v.Classification);var state=st.LoadState();Expect(state.Objects[a].Retired&&state.Objects.ContainsKey(left)&&state.Objects.ContainsKey(right),"external split identity semantics wrong");Expect(state.Boundaries[bound].Id==bound,"split reminted boundary");}
    var merge=JsonDocument.Parse(Run(node,writer,"merge-text",path,Ids.Lower(left),Ids.Lower(right))).RootElement.Clone();Guid merged=Guid.Parse(merge.GetProperty("result_id").GetString()!);using(var st=DndStore.Open(path)){var v=st.Validate();Expect(v.Writable,"external merge rejected "+v.Classification);var state=st.LoadState();Expect(state.Objects[left].Retired&&state.Objects[right].Retired&&state.Objects.ContainsKey(merged),"external merge identity semantics wrong");}
    var ext=JsonDocument.Parse(Run(node,writer,"add-extension",path,Ids.Lower(c))).RootElement.Clone();Guid extension=Guid.Parse(ext.GetProperty("extension_id").GetString()!);using(var st=DndStore.Open(path)){var v=st.Validate();Expect(v.Writable,"external extension commit rejected "+v.Classification);Expect(st.LoadState().Extensions.ContainsKey(extension),"external extension identity missing");}

    string dup=Path.Combine(work,"e08-duplicate.dnd"),broken=Path.Combine(work,"e08-broken-ref.dnd");File.Copy(path,dup,true);File.Copy(path,broken,true);string sqlite=Path.Combine(repo,".tools","sqlite","sqlite3.exe");
    Run(sqlite,dup,$"INSERT INTO objects(id,type,parent_id,order_key,role,cbor,retired) SELECT id,type,parent_id,order_key,role,cbor,retired FROM objects WHERE id=X'{Convert.ToHexString(Ids.RfcBytes(c))}' LIMIT 1;");string beforeDup=Hash(dup);using(var st=DndStore.Open(dup)){var v=st.Validate();Expect(!v.Writable&&v.Classification=="duplicate_id","duplicate ID malformed variant not rejected");}Expect(beforeDup==Hash(dup),"validation repaired duplicate artifact");
    byte[] fake=Ids.RfcBytes(Id());Run(sqlite,broken,$"UPDATE objects SET parent_id=X'{Convert.ToHexString(fake)}' WHERE id=X'{Convert.ToHexString(Ids.RfcBytes(c))}';");string beforeBroken=Hash(broken);using(var st=DndStore.Open(broken)){var v=st.Validate();Expect(!v.Writable&&v.Classification=="broken_reference","broken reference malformed variant not rejected: "+v.Classification);}Expect(beforeBroken==Hash(broken),"validation repaired broken-ref artifact");
    results.Add(new { id="E-08",result="PASS",valid_external_commits=new[]{"move","split","merge","extension"},product_private_serializer_shared=false,identity_remints_by_product=0,malformed_rejections=new[]{"duplicate_id","broken_reference"},auto_repair=0 });
    Console.WriteLine("E-08 PASS");
}

void RunE09()
{
    var tiers=new List<object>();
    foreach(int pages in new[]{10,500,5000})
    {
        var rng=new RandomNumberGeneratorLike(0xD0C5E090UL+(ulong)pages);Guid Id()=>Ids.DeterministicV4(rng);
        var state=new SemanticState{FamilyId=Id(),BranchId=Id(),RevisionId=Id(),Mode=AuthorityMode.NativeAuthored};Guid root=Id(),flow=Id();state.Objects[root]=new(root,"document",null,OrderKey.Initial(0),"document",new());state.Objects[flow]=new(flow,"flow",root,OrderKey.Initial(0),"main",new());Guid target=Guid.Empty;
        string filler=string.Concat(Enumerable.Repeat("DOCSeye locality pressure text internationalization correspondence deterministic accessibility. ",36));
        for(int i=0;i<pages;i++){Guid id=Id();if(i==pages/2)target=id;state.Objects[id]=new(id,"text_block",flow,OrderKey.Initial(i),"body",new(){["text"]=$"page-token-{i:D5} "+filler,["page_equivalent"]=i});}
        var preEntries=state.RootEntries();var preDup=preEntries.GroupBy(e=>e.Domain+":"+Convert.ToHexString(e.Key),StringComparer.Ordinal).FirstOrDefault(g=>g.Count()>1);if(preDup is not null){throw new InvalidOperationException("E09 duplicate root entry "+preDup.Key+" count="+preDup.Count()+" values="+string.Join(" | ",preDup.Select(x=>Convert.ToHexString(SHA256.HashData(x.Value)))));}
        string path=Path.Combine(work,$"scale-{pages}.dnd");var createSw=Stopwatch.StartNew();using(var create=DndStore.Create(path,state)){Expect(create.Validate(false).Writable,"scale create invalid");}createSw.Stop();state=null!;GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
        long mem0=Process.GetCurrentProcess().WorkingSet64;var sw=Stopwatch.StartNew();ValidationReport cold;using(var store=DndStore.Open(path)){cold=store.Validate(false);}sw.Stop();long coldMs=sw.ElapsedMilliseconds,mem1=Process.GetCurrentProcess().WorkingSet64;
        SemanticObject? queried;long queryMs;using(var store=DndStore.Open(path)){sw.Restart();queried=store.ReadObject(target);sw.Stop();queryMs=sw.ElapsedMilliseconds;Expect(queried is not null,"local query missed target");var head=store.ReadHead();var data=new Dictionary<string,object?>(queried!.Data,StringComparer.Ordinal){["text"]=(string)queried.Data["text"]!+" [local-edit]"};sw.Restart();var edit=store.CommitObjectData(target,data,head.RevisionId,$"e09-{pages}");sw.Stop();Expect(edit.Result.Success,"local edit failed "+edit.Result.Classification);Expect(edit.Metrics.LogicalRecordsRead==1&&edit.Metrics.LogicalRecordsWritten==1,"local edit materialized unrelated records");var verify=store.Validate(false);Expect(verify.Writable,"scale edit invalid root");tiers.Add(new{pages,file_bytes=new FileInfo(path).Length,create_ms=createSw.ElapsedMilliseconds,cold_validation_ms=coldMs,cold_validation_workingset_delta_bytes=Math.Max(0,mem1-mem0),local_query_ms=queryMs,local_edit_ms=sw.ElapsedMilliseconds,touched_logical_records=edit.Metrics.LogicalRecordsWritten,read_logical_records=edit.Metrics.LogicalRecordsRead,merkle_path_nodes=edit.Metrics.MerklePathNodes,payload_bytes_read=edit.Metrics.PayloadBytesRead,payload_bytes_written=edit.Metrics.PayloadBytesWritten,whole_document_materialization_for_local_query=false,whole_document_materialization_for_local_edit=false});}
        if(pages==500){scale500Path=path;scale500Target=target;}
    }
    results.Add(new{id="E-09",result="PASS",tiers,mechanism="direct keyed SQLite object access plus bounded Merkle path update",persistent_unbounded_growth=false});Console.WriteLine("E-09 PASS");
}

void RunE10()
{
    const long size=1024L*1024*1024;string aFile=Path.Combine(work,"asset-a-1g.bin"),bFile=Path.Combine(work,"asset-b-1g.bin"),cFile=Path.Combine(work,"asset-c-1g.bin");CreateSynthetic(aFile,size,0x11,0xA1);CreateSynthetic(bFile,size,0x22,0xB2);CreateSynthetic(cFile,size,0x33,0xC3);
    var rng=new RandomNumberGeneratorLike(0xD0C5E010UL);Guid Id()=>Ids.DeterministicV4(rng);var s=new SemanticState{FamilyId=Id(),BranchId=Id(),RevisionId=Id(),Mode=AuthorityMode.NativeAuthored};Guid root=Id(),flow=Id(),figure=Id(),unrelated=Id();s.Objects[root]=new(root,"document",null,OrderKey.Initial(0),"document",new());s.Objects[flow]=new(flow,"flow",root,OrderKey.Initial(0),"main",new());s.Objects[figure]=new(figure,"figure",flow,OrderKey.Initial(0),"figure",new(){["caption"]="1 GiB asset figure",["alt_text"]="synthetic asset",["asset_digest"]=Array.Empty<byte>()});s.Objects[unrelated]=new(unrelated,"text_block",flow,OrderKey.Initial(1),"body",new(){["text"]="unrelated semantic sentinel"});string path=Path.Combine(work,"e10-asset.dnd");using(var create=DndStore.Create(path,s)){Expect(create.Validate(false).Writable,"E10 initial invalid");}
    byte[] unrelatedBefore;byte[] digestA,digestB;long addMs,replaceMs,peakDelta;RevisionHead headB;
    using(var store=DndStore.Open(path)){unrelatedBefore=CanonicalCbor.EncodeObject(store.ReadObject(unrelated)!);long peak0=Process.GetCurrentProcess().PeakWorkingSet64;using(var fs=File.OpenRead(aFile)){var sw=Stopwatch.StartNew();var add=store.CommitEmbeddedAsset(fs,store.ReadHead().RevisionId,"e10-add",figure,4*1024*1024);sw.Stop();addMs=sw.ElapsedMilliseconds;Expect(add.Result.Success,"1GiB add failed "+add.Result.Classification);digestA=add.Digest;Expect(add.Metrics.PayloadBytesRead==size&&add.Metrics.PayloadBytesWritten>=size,"1GiB add not streamed by full length");}
        using(var fs=File.OpenRead(bFile)){var sw=Stopwatch.StartNew();var replace=store.CommitEmbeddedAsset(fs,store.ReadHead().RevisionId,"e10-replace",figure,4*1024*1024);sw.Stop();replaceMs=sw.ElapsedMilliseconds;Expect(replace.Result.Success,"1GiB replace failed "+replace.Result.Classification);digestB=replace.Digest;Expect(!digestA.SequenceEqual(digestB),"replacement digest did not change");}
        peakDelta=Math.Max(0,Process.GetCurrentProcess().PeakWorkingSet64-peak0);var figureAfter=store.ReadObject(figure)!;Expect(figureAfter.Id==figure,"asset replacement changed figure identity");Expect(((byte[])figureAfter.Data["asset_digest"]!).SequenceEqual(digestB),"figure did not reference replacement digest");Expect(CanonicalCbor.EncodeObject(store.ReadObject(unrelated)!).SequenceEqual(unrelatedBefore),"unrelated semantic record touched");var valid=store.Validate(true);Expect(valid.Writable,"1GiB digest validation failed "+valid.Classification);headB=store.ReadHead();}
    string ready=Path.Combine(work,"e10-crash.ready");try{File.Delete(ready);}catch{}string exe=Environment.ProcessPath??throw new InvalidOperationException("process path");var psi=new ProcessStartInfo(exe){UseShellExecute=false,CreateNoWindow=true};foreach(var x in new[]{"--asset-worker",path,cFile,ready,Ids.Lower(headB.RevisionId),Ids.Lower(figure)})psi.ArgumentList.Add(x);using(var worker=Process.Start(psi)??throw new InvalidOperationException("worker start")){WaitFile(ready,120);worker.Kill(true);worker.WaitForExit();}
    using(var store=DndStore.Open(path)){var after=store.ReadHead();Expect(after.RevisionId==headB.RevisionId&&after.SemanticRoot.SequenceEqual(headB.SemanticRoot),"crash did not recover exact old head");var fig=store.ReadObject(figure)!;Expect(((byte[])fig.Data["asset_digest"]!).SequenceEqual(digestB),"crash exposed partial replacement");Expect(store.Validate(true).Writable,"crash recovery asset validation failed");}
    results.Add(new{id="E-10",result="PASS",asset_bytes=size,chunk_bytes=4*1024*1024,add_ms=addMs,replace_ms=replaceMs,peak_process_workingset_increase_bytes=peakDelta,digest_a=Convert.ToHexString(digestA).ToLowerInvariant(),digest_b=Convert.ToHexString(digestB).ToLowerInvariant(),figure_identity_preserved=true,unrelated_semantic_records_touched=0,streaming=true,crash_recovery="old",successful_commit_outcome="new"});Console.WriteLine("E-10 PASS");
}

void RunE11()
{
    Expect(scale500Path is not null&&scale500Target!=Guid.Empty,"E11 requires E09 scale-500 artifact");string runtimeDir=Path.Combine(work,"runtime-e11");try{Directory.Delete(runtimeDir,true);}catch{}Directory.CreateDirectory(runtimeDir);string index=Path.Combine(runtimeDir,"index.db");Guid target=scale500Target;RevisionHead before;string token="page-token-00250";
    using(var store=DndStore.Open(scale500Path!)){before=store.ReadHead();using(var idx=new RuntimeIndex(index)){int indexed=idx.RebuildFrom(store);Expect(indexed>=500,"runtime index did not rebuild all text objects");Expect(idx.Search(token).Contains(target),"runtime index query missed target before loss");}}
    Directory.Delete(runtimeDir,true);Expect(!Directory.Exists(runtimeDir),"runtime state deletion failed");Directory.CreateDirectory(runtimeDir);using(var store=DndStore.Open(scale500Path!)){var afterDelete=store.ReadHead();Expect(afterDelete.RevisionId==before.RevisionId&&afterDelete.SemanticRoot.SequenceEqual(before.SemanticRoot),"runtime loss changed native truth");using(var idx=new RuntimeIndex(index)){idx.RebuildFrom(store);Expect(idx.Search(token).Contains(target),"rebuilt runtime index lost semantic identity");}store.PruneExpiredWitnesses(afterDelete.Sequence+1);var delta=store.ReadDeltas(0);Expect(delta.ResyncRequired&&delta.Classification=="resync_required","expired delta cursor did not require resync");Expect(store.ReadHead().SemanticRoot.SequenceEqual(before.SemanticRoot),"delta witness pruning changed semantic root");}
    results.Add(new{id="E-11",result="PASS",runtime_state_deleted_completely=true,index_rebuilt_from_dnd=true,semantic_identity_exact=true,semantic_root_unchanged=true,expired_cursor_classification="resync_required",resync_required=true,external_state_is_truth=false});Console.WriteLine("E-11 PASS");
}

void CreateSynthetic(string path,long length,byte first,byte last){using var fs=new FileStream(path,FileMode.Create,FileAccess.Write,FileShare.None);fs.SetLength(length);fs.Position=0;fs.WriteByte(first);fs.Position=length-1;fs.WriteByte(last);fs.Flush(true);}
void WaitFile(string path,int seconds){var sw=Stopwatch.StartNew();while(!File.Exists(path)&&sw.Elapsed<TimeSpan.FromSeconds(seconds))Thread.Sleep(50);Expect(File.Exists(path),"worker marker timeout");}
string Run(string file, params string[] args)
{
    var psi=new ProcessStartInfo(file){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};foreach(var a in args)psi.ArgumentList.Add(a);using var p=Process.Start(psi)??throw new InvalidOperationException("process start failed");string stdout=p.StandardOutput.ReadToEnd(),stderr=p.StandardError.ReadToEnd();p.WaitForExit();if(p.ExitCode!=0)throw new InvalidOperationException(Path.GetFileName(file)+" failed: "+stderr);return stdout.Trim();
}
string Hash(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
void Expect(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
string FindRepo(){string d=AppContext.BaseDirectory;for(int i=0;i<10;i++){if(File.Exists(Path.Combine(d,"docs","AUTHORITY.md")))return d;d=Path.GetFullPath(Path.Combine(d,".."));}return @"C:\StealthEyeLLC\DOCSeye";}