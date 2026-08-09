using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DOCSeye.Core;
using DOCSeye.Storage.Sqlite;

if (args.Length > 0 && args[0] == "--sqlite-worker")
{
    SqliteMechanismProbe.Worker(args[1], args[2], args[3], args[4]);
    return;
}

string repo = FindRepo();
string work = Path.Combine(repo, "artifacts", "early-experiments");
Directory.CreateDirectory(work);
foreach (string f in Directory.EnumerateFiles(work)) try { File.Delete(f); } catch { }
var results = new List<object>();

RunE01();
RunE02();
RunE03();
RunE04();

string evidencePath = Path.Combine(repo, "evidence", "early-experiments.json");
Directory.CreateDirectory(Path.GetDirectoryName(evidencePath)!);
File.WriteAllText(evidencePath, JsonSerializer.Serialize(new
{
    architecture_freeze = DndConstants.ArchitectureFreeze,
    generated_utc = DateTimeOffset.UtcNow,
    machine = Environment.MachineName,
    runtime = Environment.Version.ToString(),
    experiments = results
}, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"EVIDENCE={evidencePath}");

void RunE01()
{
    const ulong seed = 0xD0C5E001UL;
    var rng = new RandomNumberGeneratorLike(seed);
    Guid Id() => Ids.DeterministicV4(rng);
    var state = new SemanticState { FamilyId = Id(), BranchId = Id(), RevisionId = Id(), Sequence = 0, Mode = AuthorityMode.NativeAuthored };
    Guid root = Id(), flow = Id(), block = Id();
    string text = "A e\u0301 B \U0001F469\u200D\U0001F469\u200D\U0001F467\u200D\U0001F467 \u0645\u0631\u062D\u0628\u0627 duplicate duplicate";
    state.Objects[root] = new(root, "document", null, OrderKey.Initial(0), "document", new(StringComparer.Ordinal));
    state.Objects[flow] = new(flow, "flow", root, OrderKey.Initial(0), "main", new(StringComparer.Ordinal));
    state.Objects[block] = new(block, "text_block", flow, OrderKey.Initial(0), "body", new(StringComparer.Ordinal) { ["text"] = text });
    int phraseStart = text[..text.IndexOf("duplicate", StringComparison.Ordinal)].EnumerateRunes().Count();
    Guid b1 = Id(), b2 = Id(), b3 = Id(), b4 = Id();
    state.Boundaries[b1] = new(b1, block, phraseStart, EdgeAffinity.ExcludeAtEdge);
    state.Boundaries[b2] = new(b2, block, phraseStart, EdgeAffinity.IncludeAtEdge);
    state.Boundaries[b3] = new(b3, block, phraseStart + 9, EdgeAffinity.ExcludeAtEdge);
    state.Boundaries[b4] = new(b4, block, phraseStart + 9, EdgeAffinity.IncludeAtEdge);
    var ranges = new[]
    {
        new RetainedRange(Id(), b1, b3, true, "comment"),
        new RetainedRange(Id(), b2, b4, true, "style")
    };
    bool graphemeRefused = false;
    try { _ = UnicodeText.Delete("e\u0301", 0, 1, true); } catch (InvalidOperationException e) when (e.Message == "invalid_grapheme_boundary") { graphemeRefused = true; }
    Expect(graphemeRefused, "human-facing operation split decomposed grapheme");
    Expect(!UnicodeText.IsGraphemeBoundary("\U0001F469\u200D\U0001F469\u200D\U0001F467\u200D\U0001F467", 1), "ZWJ cluster interior misclassified as grapheme boundary");
    var graphemeConformance = RunUnicode17GraphemeConformance();
    Expect(graphemeConformance.passed == graphemeConformance.total && graphemeConformance.total > 500, "Unicode 17 GraphemeBreakTest conformance failure");

    BoundaryTransform.ApplyInsertion(state, block, phraseStart, "X", ranges);
    Expect(state.Boundaries[b1].ScalarOffset == phraseStart + 1, "exclude start must move after edge insertion");
    Expect(state.Boundaries[b2].ScalarOffset == phraseStart, "include start must remain before insertion");
    BoundaryTransform.ApplyDeletion(state, block, phraseStart + 2, 2);
    Expect(state.Boundaries[b1].Id != state.Boundaries[b2].Id, "co-located boundary identity aliased");

    var beforeSplit = state.Boundaries.ToDictionary(k => k.Key, v => v.Value.ScalarOffset);
    var split = BoundaryTransform.SplitTextBlock(state, block, phraseStart + 4, ranges, Id, 64);
    Expect(state.Objects[block].Retired, "split original did not retire");
    Expect(split.left != block && split.right != block && split.left != split.right, "split successor identity invalid");
    Guid merged = BoundaryTransform.MergeTextBlocks(state, split.left, split.right, Id, 64);
    Expect(merged != split.left && merged != split.right, "merge reused input identity");
    foreach (Guid boundaryId in new[] { b1, b2, b3, b4 })
        Expect(state.Boundaries[boundaryId].OwnerId == merged, "boundary did not survive merge with identity");

    string path = Path.Combine(work, "e01-boundaries.dnd");
    using (var store = DndStore.Create(path, state))
    {
        var v = store.Validate(); Expect(v.Writable && v.Classification == "valid", "E01 artifact validation failed: " + v.Classification);
    }
    Dictionary<Guid, TextBoundary> cold;
    using (var reopened = DndStore.Open(path))
    {
        var v = reopened.Validate(); Expect(v.Writable, "E01 cold reopen non-writable");
        cold = reopened.LoadState().Boundaries;
    }
    foreach (Guid boundaryId in new[] { b1, b2, b3, b4 })
        Expect(cold[boundaryId].Id == boundaryId && cold[boundaryId].OwnerId == merged, "cold restart lost boundary identity");

    string node = @"C:\AgentBrowser\tools\node-v24.18.1-win-x64\node.exe";
    string writer = Path.Combine(repo, "tools", "independent-writer", "independent-writer.mjs");
    int oldOffset = cold[b4].ScalarOffset;
    var ext = Process.Start(new ProcessStartInfo(node, Quote(writer) + " move-boundary " + Quote(path) + " " + Ids.Lower(b4) + " 1") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true })!;
    string extOut = ext.StandardOutput.ReadToEnd(), extErr = ext.StandardError.ReadToEnd(); ext.WaitForExit();
    Expect(ext.ExitCode == 0, "independent writer failed: " + extErr);
    using (var reopened = DndStore.Open(path))
    {
        var v = reopened.Validate(); Expect(v.Writable && v.Classification == "valid", "product rejected valid independent boundary commit: " + v.Classification);
        Expect(reopened.LoadState().Boundaries[b4].ScalarOffset == oldOffset + 1, "independent boundary commit missing");
        reopened.RebuildMerkleCache();
        Expect(reopened.Validate().Writable, "Merkle cache rebuild changed authority");
    }

    results.Add(new { id = "E-01", result = "PASS", mechanism = "non-CRDT scalar-position boundaries with explicit affinity and bounded split/merge witnesses", seed, grapheme_refusal = true, unicode_version = UnicodeText.GraphemeUnicodeVersion, grapheme_break_test = new { passed = graphemeConformance.passed, total = graphemeConformance.total }, cold_restart = true, independent_writer = JsonDocument.Parse(extOut).RootElement.Clone(), persistent_character_history = false, quote_rebound = 0 });
    Console.WriteLine("E-01 PASS");
}

void RunE02()
{
    string path = Path.Combine(work, "e02-atomicity.db");
    SqliteMechanismProbe.Initialize(path);

    string ready = Path.Combine(work, "e02-kill.ready");
    using (var p = StartWorker(path, "kill-new", ready, "hold"))
    {
        WaitFile(ready); p.Kill(true); p.WaitForExit();
    }
    Expect(SqliteMechanismProbe.Read(path) == "old", "process kill exposed partial/new uncommitted state");

    File.Delete(ready);
    using (var p = StartWorker(path, "power-new", ready, "failfast-before-commit")) { WaitFile(ready); p.WaitForExit(); }
    Expect(SqliteMechanismProbe.Read(path) == "old", "simulated power loss exposed partial/new uncommitted state");

    var full = SqliteMechanismProbe.DiskFull(path);
    Expect(full.FullRaised && full.ValueAfter == "old", "disk full did not rollback atomically");

    File.Delete(ready);
    using (var p = StartWorker(path, "committed-after-loss", ready, "postcommit-loss")) { WaitFile(ready); p.WaitForExit(); }
    Expect(SqliteMechanismProbe.Read(path) == "committed-after-loss", "postcommit response loss lost committed state");
    Expect(SqliteMechanismProbe.ReadWitness(path, "commit-key") == "committed-after-loss", "idempotency witness did not identify committed outcome");

    results.Add(new { id = "E-02", result = "PASS", journal_mode = "DELETE", synchronous = "FULL", process_kill = "old", simulated_power_loss = "old", disk_full = "old", postcommit_response_loss = "new_with_witness", partial_acknowledgements = 0 });
    Console.WriteLine("E-02 PASS");
}

void RunE03()
{
    var rng = new RandomNumberGeneratorLike(0xD0C5E003UL); Guid Id() => Ids.DeterministicV4(rng);
    var s = new SemanticState { FamilyId = Id(), BranchId = Id(), RevisionId = Id(), Sequence = 0, Mode = AuthorityMode.NativeAuthored };
    Guid root = Id(), block = Id();
    s.Objects[root] = new(root, "document", null, OrderKey.Initial(0), "document", new());
    s.Objects[block] = new(block, "text_block", root, OrderKey.Initial(0), "body", new() { ["text"] = "coherent carrier" });
    string source = Path.Combine(work, "e03-source.dnd");
    string published = Path.Combine(work, "e03-published.dnd");
    string moved = Path.Combine(work, "e03-moved-by-shelleye.dnd");
    string rawReplica = Path.Combine(work, "e03-raw-replica.dnd");
    RevisionHead sourceHead;
    using (var st = DndStore.Create(source, s))
    {
        sourceHead = st.ReadHead();
        st.BackupTo(published);
    }
    using (var pub = DndStore.Open(published))
    {
        var v = pub.Validate(); Expect(v.Writable, "SQLite backup publication not independently valid");
        var h = pub.ReadHead(); Expect(h.RevisionId == sourceHead.RevisionId && h.SemanticRoot.SequenceEqual(sourceHead.SemanticRoot), "backup did not preserve exact committed snapshot");
    }
    Expect(!File.Exists(published + "-wal") && !File.Exists(published + "-shm"), "portable backup depends on WAL/SHM");

    File.Copy(source, rawReplica, true);
    using (var raw = DndStore.Open(rawReplica))
    {
        var v = raw.Validate(); Expect(v.Writable, "quiescent raw replica is not a valid same-snapshot replica");
        var h = raw.ReadHead();
        Expect(h.FamilyId == sourceHead.FamilyId && h.BranchId == sourceHead.BranchId && h.RevisionId == sourceHead.RevisionId, "raw copy rewrote embedded semantic identity");
    }

    string carrierScript = Path.Combine(repo, "tools", "shelleye-carrier-proof.js");
    string node = @"C:\AgentBrowser\tools\node-v24.18.1-win-x64\node.exe";
    if (File.Exists(moved)) File.Delete(moved);
    using var carrier = Process.Start(new ProcessStartInfo(node, Quote(carrierScript) + " " + Quote(published) + " " + Quote(moved)) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true })!;
    string carrierOut = carrier.StandardOutput.ReadToEnd(), carrierErr = carrier.StandardError.ReadToEnd(); carrier.WaitForExit();
    Expect(carrier.ExitCode == 0, "SHELLeye carrier proof failed: " + carrierErr);
    using (var movedStore = DndStore.Open(moved))
    {
        var v = movedStore.Validate(); Expect(v.Writable, "SHELLeye-moved artifact lost native write authority: " + v.Classification);
        var h = movedStore.ReadHead(); Expect(h.RevisionId == sourceHead.RevisionId && h.SemanticRoot.SequenceEqual(sourceHead.SemanticRoot), "physical move changed semantic head/root");
    }

    results.Add(new
    {
        id = "E-03",
        result = "PASS",
        mechanism = "SQLite online backup for coherent publication plus SHELLeye retained physical carrier identity/movement",
        source_revision = Ids.Lower(sourceHead.RevisionId),
        semantic_root = Convert.ToHexString(sourceHead.SemanticRoot).ToLowerInvariant(),
        quiescent_raw_replica_same_snapshot = true,
        raw_copy_identity_rewrite = 0,
        live_publication_path = "coordinated_backup_only",
        wal_shm_required = false,
        shelleye = JsonDocument.Parse(carrierOut).RootElement.Clone()
    });
    Console.WriteLine("E-03 PASS");
}
void RunE04()
{
    var rng = new RandomNumberGeneratorLike(0xD0C5E004UL); Guid Id() => Ids.DeterministicV4(rng);
    var s = new SemanticState { FamilyId = Id(), BranchId = Id(), RevisionId = Id(), Mode = AuthorityMode.NativeAuthored };
    Guid root = Id(), a = Id(), b = Id(); s.Objects[root] = new(root, "document", null, OrderKey.Initial(0), "document", new()); s.Objects[a] = new(a, "text_block", root, OrderKey.Initial(0), "body", new() { ["text"] = "abcdefghij" }); s.Objects[b] = new(b, "text_block", root, OrderKey.Initial(1), "body", new() { ["text"] = "other" });
    Guid bs = Id(), be = Id(); s.Boundaries[bs] = new(bs, a, 2, EdgeAffinity.ExcludeAtEdge); s.Boundaries[be] = new(be, a, 7, EdgeAffinity.ExcludeAtEdge);
    ExtensionEnvelope E(ExtensionCoverageKind k, Guid? target, ExtensionEditPolicy p, bool req, string tag, string? prop = null, Guid? sb = null, Guid? eb = null)
    { byte[] payload = System.Text.Encoding.UTF8.GetBytes("opaque:" + tag + ":\u0000exact"); return new(Id(), "urn:docseye:test:" + tag, tag, 1, 0, "bytes", payload, SHA256.HashData(payload), k, target, prop, sb, eb, p, req, null); }
    var objectExt = E(ExtensionCoverageKind.Object, a, ExtensionEditPolicy.Independent, false, "object");
    var propExt = E(ExtensionCoverageKind.Property, a, ExtensionEditPolicy.MustUnderstandBeforeEdit, true, "property", "secret");
    var subtreeExt = E(ExtensionCoverageKind.Subtree, root, ExtensionEditPolicy.Independent, false, "subtree");
    var rangeExt = E(ExtensionCoverageKind.TextInterval, a, ExtensionEditPolicy.MustUnderstandBeforeEdit, true, "range", null, bs, be);
    var topologyExt = E(ExtensionCoverageKind.TopologyRegion, b, ExtensionEditPolicy.GenericTransform, false, "topology");
    var globalExt = E(ExtensionCoverageKind.DocumentGlobal, null, ExtensionEditPolicy.MustUnderstandBeforeEdit, true, "global");
    foreach (var e in new[] { objectExt, propExt, subtreeExt, rangeExt, topologyExt }) s.Extensions[e.ExtensionId] = e;
    var payloads = s.Extensions.ToDictionary(x => x.Key, x => x.Value.ExactPayload.ToArray());

    var disjoint = ExtensionPolicyEngine.Check(s, new EditIntent("property_set", b, "known"));
    Expect(disjoint.Allowed, "safe disjoint edit blocked");
    var propertyHit = ExtensionPolicyEngine.Check(s, new EditIntent("property_set", a, "secret"));
    Expect(!propertyHit.Allowed && propertyHit.Classification == "blocked_required_extension", "required property intersection not refused");
    var rangeHit = ExtensionPolicyEngine.Check(s, new EditIntent("text_delete", a, null, bs, be));
    Expect(!rangeHit.Allowed && rangeHit.Classification == "blocked_required_extension", "required interval intersection not refused");
    var topo = ExtensionPolicyEngine.Check(s, new EditIntent("table_split", b, null, null, null, TopologyMutation: true));
    Expect(topo.Allowed && topo.RequiresTransform, "declared topology generic transform not selected");
    s.Extensions[globalExt.ExtensionId] = globalExt;
    var global = ExtensionPolicyEngine.Check(s, new EditIntent("property_set", b, "known"));
    Expect(!global.Allowed && global.Classification == "blocked_required_extension", "required global coverage did not block");
    s.Extensions.Remove(globalExt.ExtensionId);
    foreach (var kv in payloads) Expect(s.Extensions[kv.Key].ExactPayload.SequenceEqual(kv.Value) && s.Extensions[kv.Key].DigestValid, "opaque extension payload changed");

    results.Add(new { id = "E-04", result = "PASS", coverage = new[] { "object", "property", "subtree", "text_interval", "topology_region", "document_global" }, disjoint_edits = "allowed", required_intersections = "blocked_required_extension", generic_transform = "required", exact_payload_preservation = true });
    Console.WriteLine("E-04 PASS");
}

(int passed, int total) RunUnicode17GraphemeConformance()
{
    string path = Path.Combine(repo, "data", "unicode", "17.0.0", "GraphemeBreakTest.txt");
    int passed = 0, total = 0;
    foreach (string sourceLine in File.ReadLines(path))
    {
        string line = sourceLine.Split('#', 2)[0].Trim();
        if (line.Length == 0) continue;
        string[] tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var expected = new List<int>();
        var textBuilder = new StringBuilder();
        int scalar = 0, i = 0;
        while (i < tokens.Length)
        {
            string marker = tokens[i++];
            if (marker.Length == 1 && marker[0] == '\u00F7') expected.Add(scalar);
            else if (!(marker.Length == 1 && marker[0] == '\u00D7')) throw new InvalidOperationException("unexpected grapheme test marker U+" + ((int)marker[0]).ToString("X4"));
            if (i >= tokens.Length) break;
            int cp = Convert.ToInt32(tokens[i++], 16);
            textBuilder.Append(new Rune(cp).ToString());
            scalar++;
        }
        total++;
        var actual = UnicodeText.GraphemeScalarBoundaries(textBuilder.ToString());
        if (actual.SequenceEqual(expected)) passed++;
        else throw new InvalidOperationException($"Unicode 17 GraphemeBreakTest mismatch case {total}: expected [{string.Join(',', expected)}] actual [{string.Join(',', actual)}] source={sourceLine}");
    }
    return (passed, total);
}
Process StartWorker(string db, string value, string ready, string mode)
{
    string exe = Environment.ProcessPath ?? throw new InvalidOperationException("missing process path");
    var psi = new ProcessStartInfo(exe) { UseShellExecute = false, CreateNoWindow = true };
    psi.ArgumentList.Add("--sqlite-worker"); psi.ArgumentList.Add(db); psi.ArgumentList.Add(value); psi.ArgumentList.Add(ready); psi.ArgumentList.Add(mode);
    return Process.Start(psi) ?? throw new InvalidOperationException("worker did not start");
}
void WaitFile(string path) { var sw = Stopwatch.StartNew(); while (!File.Exists(path) && sw.Elapsed < TimeSpan.FromSeconds(15)) Thread.Sleep(20); Expect(File.Exists(path), "worker ready marker timeout"); }
void Expect(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
string Quote(string value) => "\"" + value.Replace("\"", "\\\"") + "\"";
string FindRepo() { string d = AppContext.BaseDirectory; for (int i = 0; i < 8; i++) { if (File.Exists(Path.Combine(d, "docs", "AUTHORITY.md"))) return d; d = Path.GetFullPath(Path.Combine(d, "..")); } return @"C:\StealthEyeLLC\DOCSeye"; }
