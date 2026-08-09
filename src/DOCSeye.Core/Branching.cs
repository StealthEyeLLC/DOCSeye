using System.Collections;
using System.Security.Cryptography;

namespace DOCSeye.Core;

public static class Branching
{
    public static BranchTransformResult Fork(SemanticState source, Func<Guid>? newId = null)
    {
        newId ??= Ids.NewV4;
        var map = new Dictionary<Guid, Guid>();
        Guid Remint(Guid id)
        {
            if (!map.TryGetValue(id, out var mapped)) { mapped = newId(); map[id] = mapped; }
            return mapped;
        }

        foreach (var id in source.Objects.Keys) Remint(id);
        foreach (var id in source.Boundaries.Keys) Remint(id);
        foreach (var id in source.Ranges.Keys) Remint(id);
        foreach (var id in source.Extensions.Keys) Remint(id);
        foreach (var id in source.ProviderFacets.Keys) Remint(id);

        var target = new SemanticState
        {
            FamilyId = source.FamilyId,
            BranchId = newId(),
            RevisionId = newId(),
            Sequence = 0,
            Mode = source.Mode
        };
        foreach (string capability in source.RequiredCapabilities) target.RequiredCapabilities.Add(capability);

        foreach (var o in source.Objects.Values)
            target.Objects[Remint(o.Id)] = new SemanticObject(
                Remint(o.Id), o.Type, o.ParentId is Guid parent ? Remint(parent) : null,
                o.Order, o.Role, RemapData(o.Data, map), o.Retired);

        foreach (var b in source.Boundaries.Values)
            target.Boundaries[Remint(b.Id)] = b with { Id = Remint(b.Id), OwnerId = Remint(b.OwnerId) };

        foreach (var r in source.Ranges.Values)
            target.Ranges[Remint(r.Id)] = r with
            {
                Id = Remint(r.Id), StartBoundaryId = Remint(r.StartBoundaryId), EndBoundaryId = Remint(r.EndBoundaryId)
            };

        foreach (var e in source.Extensions.Values)
        {
            Guid id = Remint(e.ExtensionId);
            target.Extensions[id] = e with
            {
                ExtensionId = id,
                TargetId = e.TargetId is Guid t ? Remint(t) : null,
                StartBoundaryId = e.StartBoundaryId is Guid sb ? Remint(sb) : null,
                EndBoundaryId = e.EndBoundaryId is Guid eb ? Remint(eb) : null
            };
        }

        foreach (var f in source.ProviderFacets.Values)
        {
            Guid id = Remint(f.Id);
            target.ProviderFacets[id] = f with { Id = id, TargetId = f.TargetId is Guid t ? Remint(t) : null };
        }

        foreach (var a in source.Assets) target.Assets[a.Key] = CloneAsset(a.Value);
        foreach (var c in source.SourceCapsules) target.SourceCapsules[c.Key] = CloneCapsule(c.Value);
        foreach (var w in source.Retired.Values)
            target.Retired[Remint(w.ObjectId)] = new(Remint(w.ObjectId), w.Resolution, w.Successors.Select(Remint).ToArray(), w.ExpiresAfterSequence);

        foreach (var pair in map)
        {
            string kind = source.Objects.ContainsKey(pair.Key) ? "object" :
                source.Boundaries.ContainsKey(pair.Key) ? "boundary" :
                source.Ranges.ContainsKey(pair.Key) ? "range" :
                source.Extensions.ContainsKey(pair.Key) ? "extension" : "provider_facet";
            target.OriginRefs[pair.Value] = new(pair.Value, kind, source.FamilyId, source.BranchId, source.RevisionId, pair.Key);
        }
        return new(target, map);
    }

    public static IReadOnlyDictionary<Guid, Guid> PasteObjectSubgraph(SemanticState target, SemanticState source, Guid sourceObjectId, Func<Guid>? newId = null, bool copyAnnotations = false)
    {
        newId ??= Ids.NewV4;
        if (!source.Objects.TryGetValue(sourceObjectId, out var root) || root.Retired) throw new InvalidOperationException("paste_source_missing");
        var subtree = new HashSet<Guid> { sourceObjectId };
        bool changed;
        do
        {
            changed = false;
            foreach (var o in source.Objects.Values.Where(o => !o.Retired && o.ParentId is Guid p && subtree.Contains(p)))
                if (subtree.Add(o.Id)) changed = true;
        } while (changed);

        var map = subtree.ToDictionary(id => id, _ => newId());
        Guid? targetParent = root.ParentId is Guid p && target.Objects.ContainsKey(p) ? p : target.Objects.Values.FirstOrDefault(o => !o.Retired && o.Type == "flow")?.Id;
        int ordinal = targetParent is Guid tp ? target.Objects.Values.Count(o => !o.Retired && o.ParentId == tp) : target.Objects.Count;

        foreach (var sourceId in subtree.OrderBy(id => source.Objects[id].Order).ThenBy(Ids.Lower, StringComparer.Ordinal))
        {
            var o = source.Objects[sourceId];
            Guid current = map[sourceId];
            Guid? parent = sourceId == sourceObjectId ? targetParent : o.ParentId is Guid old && map.TryGetValue(old, out var mapped) ? mapped : targetParent;
            var order = sourceId == sourceObjectId ? OrderKey.Initial(ordinal) : o.Order;
            target.Objects[current] = new(current, o.Type, parent, order, o.Role, RemapData(o.Data, map), false);
        }

        if(copyAnnotations)
        {
            var boundaryMap = new Dictionary<Guid, Guid>();
            foreach (var b in source.Boundaries.Values.Where(b => subtree.Contains(b.OwnerId) && b.State is AnchorState.Live or AnchorState.Collapsed or AnchorState.Orphaned))
            {
                Guid id = newId(); boundaryMap[b.Id] = id;
                target.Boundaries[id] = b with { Id = id, OwnerId = map[b.OwnerId] };
            }
            foreach (var r in source.Ranges.Values.Where(r => boundaryMap.ContainsKey(r.StartBoundaryId) && boundaryMap.ContainsKey(r.EndBoundaryId)))
            {
                Guid id = newId();
                target.Ranges[id] = r with { Id = id, StartBoundaryId = boundaryMap[r.StartBoundaryId], EndBoundaryId = boundaryMap[r.EndBoundaryId] };
            }
        }
        return map;
    }


    public static BranchTransformResult IndependentDuplicate(SemanticState source, Func<Guid>? newId = null)
    {
        newId ??= Ids.NewV4; var transformed=Fork(source,newId); transformed.State.FamilyId=newId(); return transformed;
    }

    public static BranchTransformResult InstantiateTemplate(SemanticState source, Func<Guid>? newId = null)
    {
        newId ??= Ids.NewV4; var transformed=Fork(source,newId); transformed.State.FamilyId=newId(); return transformed;
    }
    public static SemanticState MergeDisjoint(SemanticState @base, SemanticState left, SemanticState right, Guid revisionId)
    {
        if (@base.FamilyId != left.FamilyId || @base.FamilyId != right.FamilyId || @base.BranchId != left.BranchId || @base.BranchId != right.BranchId)
            throw new InvalidOperationException("merge_scope_mismatch");
        EnsureNonObjectStateUnchanged(@base, left, right);
        var merged = Clone(@base);
        foreach (Guid id in @base.Objects.Keys)
        {
            if (!left.Objects.TryGetValue(id, out var l) || !right.Objects.TryGetValue(id, out var r)) throw new InvalidOperationException("merge_shape_changed");
            var b = @base.Objects[id];
            bool lc = !CanonicalCbor.EncodeObject(l).SequenceEqual(CanonicalCbor.EncodeObject(b));
            bool rc = !CanonicalCbor.EncodeObject(r).SequenceEqual(CanonicalCbor.EncodeObject(b));
            if (lc && rc && !CanonicalCbor.EncodeObject(l).SequenceEqual(CanonicalCbor.EncodeObject(r))) throw new InvalidOperationException("merge_conflict");
            var chosen = lc ? l : rc ? r : b;
            merged.Objects[id] = CloneObject(chosen);
        }
        merged.RevisionId = revisionId;
        merged.Sequence = Math.Max(left.Sequence, right.Sequence) + 1;
        return merged;
    }

    public static SemanticState Clone(SemanticState source)
    {
        var target = new SemanticState
        {
            FamilyId = source.FamilyId, BranchId = source.BranchId, RevisionId = source.RevisionId,
            Sequence = source.Sequence, Mode = source.Mode
        };
        target.RequiredCapabilities.Clear(); foreach (string capability in source.RequiredCapabilities) target.RequiredCapabilities.Add(capability);
        foreach (var o in source.Objects) target.Objects[o.Key] = CloneObject(o.Value);
        foreach (var b in source.Boundaries) target.Boundaries[b.Key] = b.Value;
        foreach (var r in source.Ranges) target.Ranges[r.Key] = r.Value;
        foreach (var e in source.Extensions) target.Extensions[e.Key] = e.Value with { ExactPayload = e.Value.ExactPayload.ToArray(), PayloadDigest = e.Value.PayloadDigest.ToArray() };
        foreach (var a in source.Assets) target.Assets[a.Key] = CloneAsset(a.Value);
        foreach (var f in source.ProviderFacets) target.ProviderFacets[f.Key] = f.Value with { ExactPayload = f.Value.ExactPayload.ToArray(), Digest = f.Value.Digest.ToArray() };
        foreach (var c in source.SourceCapsules) target.SourceCapsules[c.Key] = CloneCapsule(c.Value);
        foreach (var w in source.Retired) target.Retired[w.Key] = w.Value with { Successors = w.Value.Successors.ToArray() };
        foreach (var o in source.OriginRefs) target.OriginRefs[o.Key] = o.Value;
        return target;
    }

    public static HashSet<Guid> PublicIds(SemanticState state)
    {
        var ids = new HashSet<Guid>(state.Objects.Keys);
        ids.UnionWith(state.Boundaries.Keys); ids.UnionWith(state.Ranges.Keys); ids.UnionWith(state.Extensions.Keys); ids.UnionWith(state.ProviderFacets.Keys);
        return ids;
    }

    private static SemanticObject CloneObject(SemanticObject o) => o with { Data = CloneData(o.Data) };
    private static AssetCommitment CloneAsset(AssetCommitment a) => a with { Digest = a.Digest.ToArray() };
    private static SourceCapsuleEvidence CloneCapsule(SourceCapsuleEvidence c) => c with { ExactBytes = c.ExactBytes.ToArray(), Digest = c.Digest.ToArray() };
    private static Dictionary<string, object?> CloneData(Dictionary<string, object?> data) => data.ToDictionary(k => k.Key, v => CloneValue(v.Value), StringComparer.Ordinal);
    private static Dictionary<string, object?> RemapData(Dictionary<string, object?> data, IReadOnlyDictionary<Guid, Guid> map) => data.ToDictionary(k => k.Key, v => RemapValue(v.Value, map), StringComparer.Ordinal);
    private static object? CloneValue(object? value) => value switch
    {
        null => null, byte[] b => b.ToArray(), Guid g => g,
        Dictionary<string, object?> d => CloneData(d),
        IReadOnlyDictionary<string, object?> d => d.ToDictionary(k => k.Key, v => CloneValue(v.Value), StringComparer.Ordinal),
        object?[] a => a.Select(CloneValue).ToArray(),
        _ => value
    };
    private static object? RemapValue(object? value, IReadOnlyDictionary<Guid, Guid> map) => value switch
    {
        null => null, Guid g when map.TryGetValue(g, out var m) => m, Guid g => g, byte[] b => b.ToArray(),
        Dictionary<string, object?> d => d.ToDictionary(k => k.Key, v => RemapValue(v.Value, map), StringComparer.Ordinal),
        IReadOnlyDictionary<string, object?> d => d.ToDictionary(k => k.Key, v => RemapValue(v.Value, map), StringComparer.Ordinal),
        object?[] a => a.Select(v => RemapValue(v, map)).ToArray(),
        _ => value
    };

    private static void EnsureNonObjectStateUnchanged(SemanticState b, SemanticState l, SemanticState r)
    {
        static string Fingerprint(SemanticState s)
        {
            using var h = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            foreach (var e in s.RootEntries().Where(e => e.Domain is not "objects" and not "relations" and not "commit").OrderBy(e => e.Domain, StringComparer.Ordinal).ThenBy(e => Convert.ToHexString(e.Key), StringComparer.Ordinal))
            { h.AppendData(e.Key); h.AppendData(e.Value); }
            return Convert.ToHexString(h.GetHashAndReset());
        }
        string x = Fingerprint(b); if (Fingerprint(l) != x || Fingerprint(r) != x) throw new InvalidOperationException("merge_non_object_change");
    }
}