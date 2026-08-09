using System.Security.Cryptography;
using System.Text;

namespace DOCSeye.Core;

public readonly record struct RootEntry(string Domain, byte[] Key, byte[] Value)
{
    public static RootEntry ForObject(SemanticObject o) => new("objects", SemanticRoot.Key("object", Ids.RfcBytes(o.Id)), CanonicalCbor.EncodeObject(o));
    public static RootEntry ForBoundary(TextBoundary b) => new("boundaries", SemanticRoot.Key("boundary", Ids.RfcBytes(b.Id)), CanonicalCbor.EncodeBoundary(b));
    public static RootEntry ForRange(RetainedRange r) => new("boundaries", SemanticRoot.Key("retained-range", Ids.RfcBytes(r.Id)), CanonicalCbor.EncodeRange(r));
    public static RootEntry ForOriginRef(OriginRef r) => new("lifecycle", SemanticRoot.Key("origin-ref", Ids.RfcBytes(r.CurrentId)), CanonicalCbor.EncodeOriginRef(r));
    public static RootEntry ForExtension(ExtensionEnvelope e) => new("extensions", SemanticRoot.Key("extension", Ids.RfcBytes(e.ExtensionId)), CanonicalCbor.EncodeExtension(e));
    public static RootEntry ForAsset(AssetCommitment a) => new("assets", SemanticRoot.Key("asset", a.Digest), CanonicalCbor.EncodeAsset(a));
    public static RootEntry ForProviderFacet(ProviderFacet f) => new("providers", SemanticRoot.Key("provider-facet", Ids.RfcBytes(f.Id)), CanonicalCbor.EncodeProviderFacet(f));
    public static RootEntry ForSourceCapsule(SourceCapsuleEvidence c) => new("providers", SemanticRoot.Key("source-capsule", c.Digest), CanonicalCbor.EncodeSourceCapsule(c));
    public static RootEntry ForRetired(RetiredWitness w) => new("lifecycle", SemanticRoot.Key("retired-object", Ids.RfcBytes(w.ObjectId)), CanonicalCbor.EncodeRetiredWitness(w));
}

public static class SemanticRoot
{
    private static readonly byte[] LeafPrefix = Encoding.ASCII.GetBytes("DOCSeye:DND1:leaf\0");
    private static readonly byte[] NodePrefix = Encoding.ASCII.GetBytes("DOCSeye:DND1:node\0");
    private static readonly byte[] EmptyPrefix = Encoding.ASCII.GetBytes("DOCSeye:DND1:empty\0");
    private static readonly byte[] OverallPrefix = Encoding.ASCII.GetBytes("DOCSeye:DND1:semantic-root\0");

    public static byte[] Key(string kind, ReadOnlySpan<byte> idOrDigest) => Hash(
        Encoding.ASCII.GetBytes("DOCSeye:DND1:key\0"),
        Encoding.ASCII.GetBytes(kind),
        new byte[] { 0 },
        idOrDigest.ToArray());

    public static byte[] Compute(IEnumerable<RootEntry> entries)
    {
        var grouped = entries.GroupBy(e => e.Domain, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        var roots = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (string domain in DndConstants.RootDomains)
            roots[domain] = ComputeDomain(domain, grouped.TryGetValue(domain, out var list) ? list : Array.Empty<RootEntry>());
        return CombineDomainRoots(roots);
    }

    public static byte[] CombineDomainRoots(IReadOnlyDictionary<string, byte[]> roots)
    {
        using var ms = new MemoryStream();
        ms.Write(OverallPrefix);
        foreach (string domain in DndConstants.RootDomains)
        {
            WritePart(ms, Encoding.ASCII.GetBytes(domain));
            WritePart(ms, roots.TryGetValue(domain, out var hash) ? hash : EmptyHash(domain, 0));
        }
        return SHA256.HashData(ms.ToArray());
    }

    public static byte[] ComputeDomain(string domain, IReadOnlyCollection<RootEntry> entries)
    {
        if (!DndConstants.RootDomains.Contains(domain, StringComparer.Ordinal))
            throw new InvalidOperationException("unknown semantic-root domain");
        var root = new TrieNode();
        foreach (var entry in entries)
        {
            if (!string.Equals(entry.Domain, domain, StringComparison.Ordinal)) throw new InvalidOperationException("domain mismatch");
            if (entry.Key.Length != 32) throw new InvalidOperationException("root key must be 32 bytes");
            Insert(root, entry.Key, 0, LeafHash(domain, entry.Key, entry.Value));
        }
        return HashNode(domain, 0, root);
    }

    private sealed class TrieNode
    {
        public SortedDictionary<byte, TrieNode> Children { get; } = new();
        public byte[]? Leaf { get; set; }
    }

    private static void Insert(TrieNode node, byte[] key, int depth, byte[] leaf)
    {
        if (depth == 32)
        {
            if (node.Leaf is not null) throw new InvalidOperationException("duplicate semantic root key");
            node.Leaf = leaf;
            return;
        }
        if (!node.Children.TryGetValue(key[depth], out var child))
        {
            child = new TrieNode();
            node.Children[key[depth]] = child;
        }
        Insert(child, key, depth + 1, leaf);
    }

    private static byte[] HashNode(string domain, int depth, TrieNode node)
    {
        if (depth == 32) return node.Leaf ?? EmptyHash(domain, depth);
        if (node.Children.Count == 0) return EmptyHash(domain, depth);
        using var ms = new MemoryStream();
        ms.Write(NodePrefix);
        WritePart(ms, Encoding.ASCII.GetBytes(domain));
        ms.WriteByte((byte)depth);
        foreach (var child in node.Children)
        {
            ms.WriteByte(child.Key);
            ms.Write(HashNode(domain, depth + 1, child.Value));
        }
        return SHA256.HashData(ms.ToArray());
    }

    public static byte[] LeafHash(string domain, byte[] key, byte[] value) => Hash(
        LeafPrefix, Encoding.ASCII.GetBytes(domain), new byte[] { 0 }, key, SHA256.HashData(value));

    public static byte[] EmptyHash(string domain, int depth) => Hash(
        EmptyPrefix, Encoding.ASCII.GetBytes(domain), new byte[] { 0, checked((byte)depth) });

    public static byte[] InternalHash(string domain, int depth, IEnumerable<(byte child, byte[] hash)> children)
    {
        var ordered = children.OrderBy(c => c.child).ToArray();
        if (ordered.Length == 0) return EmptyHash(domain, depth);
        using var ms = new MemoryStream();
        ms.Write(NodePrefix);
        WritePart(ms, Encoding.ASCII.GetBytes(domain));
        ms.WriteByte((byte)depth);
        foreach (var child in ordered)
        {
            ms.WriteByte(child.child);
            ms.Write(child.hash);
        }
        return SHA256.HashData(ms.ToArray());
    }

    public static int BoundedPathNodesPerEntry => 33;

    private static byte[] Hash(params byte[][] parts)
    {
        using var ms = new MemoryStream();
        foreach (var part in parts) ms.Write(part);
        return SHA256.HashData(ms.ToArray());
    }

    private static void WritePart(Stream stream, byte[] part)
    {
        Span<byte> length = stackalloc byte[4];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(length, part.Length);
        stream.Write(length);
        stream.Write(part);
    }
}

public sealed class SemanticState
{
    public Guid FamilyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid RevisionId { get; set; }
    public long Sequence { get; set; }
    public AuthorityMode Mode { get; set; }
    public Dictionary<Guid, SemanticObject> Objects { get; } = new();
    public Dictionary<Guid, TextBoundary> Boundaries { get; } = new();
    public Dictionary<Guid, RetainedRange> Ranges { get; } = new();
    public Dictionary<Guid, ExtensionEnvelope> Extensions { get; } = new();
    public Dictionary<string, AssetCommitment> Assets { get; } = new(StringComparer.Ordinal);
    public Dictionary<Guid, ProviderFacet> ProviderFacets { get; } = new();
    public Dictionary<string, SourceCapsuleEvidence> SourceCapsules { get; } = new(StringComparer.Ordinal);
    public Dictionary<Guid, RetiredWitness> Retired { get; } = new();
    public Dictionary<Guid, OriginRef> OriginRefs { get; } = new();
    public SortedSet<string> RequiredCapabilities { get; } = new(DndConstants.DefaultRequiredCapabilities, StringComparer.Ordinal);

    public IReadOnlyList<RootEntry> RootEntries()
    {
        var entries = new List<RootEntry>();
        entries.AddRange(Objects.Values.Where(o => !o.Retired).Select(RootEntry.ForObject));
        entries.AddRange(Boundaries.Values.Where(b => b.State is AnchorState.Live or AnchorState.Collapsed or AnchorState.Orphaned).Select(RootEntry.ForBoundary));
        entries.AddRange(Ranges.Values.Where(r => !string.Equals(r.State, "destroyed", StringComparison.Ordinal)).Select(RootEntry.ForRange));
        entries.AddRange(Extensions.Values.Select(RootEntry.ForExtension));
        entries.AddRange(Assets.Values.Select(RootEntry.ForAsset));
        entries.AddRange(ProviderFacets.Values.Select(RootEntry.ForProviderFacet));
        entries.AddRange(SourceCapsules.Values.Select(RootEntry.ForSourceCapsule));
        entries.AddRange(Retired.Values.Select(RootEntry.ForRetired));
        entries.AddRange(OriginRefs.Values.Select(RootEntry.ForOriginRef));

        foreach (var group in Objects.Values.Where(o => !o.Retired && o.ParentId is not null).GroupBy(o => o.ParentId!.Value))
        {
            object?[] orderedIds = group.OrderBy(o => o.Order)
                .ThenBy(o => Ids.Lower(o.Id), StringComparer.Ordinal)
                .Select(o => (object?)o.Id)
                .ToArray();
            var relation = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["parent_id"] = group.Key,
                ["ordered_child_ids"] = orderedIds
            };
            entries.Add(new RootEntry("relations", KeyForRelation(group.Key), CanonicalCbor.Encode(relation)));
        }

        var format = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["format_major"] = DndConstants.FormatMajor,
            ["format_minor"] = DndConstants.FormatMinor,
            ["root_mechanism"] = DndConstants.RootMechanism,
            ["family_id"] = FamilyId,
            ["branch_id"] = BranchId,
            ["mode"] = ModeToken(Mode),
            ["required_capabilities"] = RequiredCapabilities.Cast<object?>().ToArray()
        };
        entries.Add(new RootEntry("commit", SemanticRoot.Key("branch", Ids.RfcBytes(BranchId)), CanonicalCbor.Encode(format)));
        return entries;
    }

    public byte[] ComputeRoot() => SemanticRoot.Compute(RootEntries());
    public static byte[] KeyForRelation(Guid parent) => SemanticRoot.Key("children", Ids.RfcBytes(parent));
    public static string ModeToken(AuthorityMode mode) => mode switch
    {
        AuthorityMode.NativeAuthored => "native-authored",
        AuthorityMode.ConvertedImported => "converted/imported",
        AuthorityMode.ForeignManaged => "foreign-managed",
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };
}