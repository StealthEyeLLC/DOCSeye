using System.Security.Cryptography;

namespace DOCSeye.Core;

public static class DndConstants
{
    public const string ArchitectureFreeze = "0b10e8ed6ada2e0aaef3f1196de19bf7a4631bec";
    public const int FormatMajor = 1;
    public const int FormatMinor = 0;
    public const string RootMechanism = "sha256-domain-separated-sparse-radix-256-v1";
    public static readonly string[] RootDomains =
    [
        "objects", "boundaries", "relations", "extensions", "assets",
        "providers", "lifecycle", "layout", "commit"
    ];
    public static readonly string[] DefaultRequiredCapabilities =
    [
        "dnd-generation-1", "uuid-v4-rfc9562", "deterministic-cbor-rfc8949",
        "retained-boundaries-v1", "extension-coverage-v1", "semantic-root-radix256-v1"
    ];
}

public enum AuthorityMode { NativeAuthored, ConvertedImported, ForeignManaged }
public enum AnchorState { Live, Collapsed, Orphaned, Destroyed, Ambiguous }
public enum RetiredResolution { Destroyed, Split, Merged, UnknownRetired }
public enum EdgeAffinity { IncludeAtEdge, ExcludeAtEdge, BeforeInsertion, AfterInsertion }
public enum ExtensionCoverageKind { Object, Property, Subtree, TextInterval, Relation, TopologyRegion, LayoutProfile, DocumentGlobal }
public enum ExtensionEditPolicy { Independent, MoveWithTarget, GenericTransform, Invalidate, MustUnderstandBeforeEdit }
public enum CapabilityState { Supported, SupportedWithTransform, PreservedOpaque, FallbackOnly, UnavailableProvider, Unsupported, BlockedRequiredExtension, Invalid }
public enum ExportOutcome { ExactSourceReuse, PreservedPatch, TranslatedConformant, TranslatedWithDeclaredLoss, Unsupported, Blocked }
public enum ObservationEvidence { NotObserved, PackageValidated, SchemaValidated, AlternateProviderObserved, MicrosoftObserved }

public static class Ids
{
    public static Guid NewV4() => Guid.NewGuid();

    public static Guid DeterministicV4(RandomNumberGeneratorLike rng)
    {
        Span<byte> bytes = stackalloc byte[16];
        rng.Fill(bytes);
        bytes[6] = (byte)((bytes[6] & 0x0f) | 0x40);
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);
        return GuidFromRfcBytes(bytes);
    }

    public static byte[] RfcBytes(Guid id)
    {
        Span<byte> dotnet = stackalloc byte[16];
        id.TryWriteBytes(dotnet);
        byte[] bytes = dotnet.ToArray();
        Array.Reverse(bytes, 0, 4);
        Array.Reverse(bytes, 4, 2);
        Array.Reverse(bytes, 6, 2);
        return bytes;
    }

    public static Guid GuidFromRfcBytes(ReadOnlySpan<byte> rfc)
    {
        if (rfc.Length != 16) throw new ArgumentException("uuid requires 16 bytes");
        byte[] bytes = rfc.ToArray();
        Array.Reverse(bytes, 0, 4);
        Array.Reverse(bytes, 4, 2);
        Array.Reverse(bytes, 6, 2);
        return new Guid(bytes);
    }

    public static string Lower(Guid id) => id.ToString("D").ToLowerInvariant();
    public static bool IsV4(Guid id) => (RfcBytes(id)[6] >> 4) == 4 && (RfcBytes(id)[8] & 0xC0) == 0x80;
}

public sealed class RandomNumberGeneratorLike(ulong seed)
{
    private ulong s0 = SplitMix(ref seed), s1 = SplitMix(ref seed);
    private static ulong SplitMix(ref ulong x)
    {
        x += 0x9E3779B97F4A7C15UL;
        ulong z = x;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
    private ulong Next()
    {
        ulong x = s0, y = s1;
        s0 = y;
        x ^= x << 23;
        s1 = x ^ y ^ (x >> 17) ^ (y >> 26);
        return s1 + y;
    }
    public void Fill(Span<byte> bytes)
    {
        int offset = 0;
        while (offset < bytes.Length)
        {
            ulong value = Next();
            for (int i = 0; i < 8 && offset < bytes.Length; i++, offset++)
                bytes[offset] = (byte)(value >> (i * 8));
        }
    }
}

public readonly record struct OrderKey(UInt128 Value) : IComparable<OrderKey>
{
    public static readonly UInt128 InitialStep = (UInt128)1 << 96;
    public static OrderKey Initial(int ordinal) => new(InitialStep * (UInt128)(ordinal + 1));
    public static bool TryBetween(OrderKey? left, OrderKey? right, out OrderKey key)
    {
        UInt128 l = left?.Value ?? 0;
        UInt128 r = right?.Value ?? UInt128.MaxValue;
        if (r <= l + 1) { key = default; return false; }
        key = new OrderKey(l + (r - l) / 2);
        return true;
    }
    public int CompareTo(OrderKey other) => Value.CompareTo(other.Value);
    public byte[] Bytes()
    {
        byte[] bytes = new byte[16];
        UInt128 value = Value;
        for (int i = 15; i >= 0; i--) { bytes[i] = (byte)value; value >>= 8; }
        return bytes;
    }
    public static OrderKey FromBytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 16) throw new ArgumentException("order key requires 16 bytes");
        UInt128 value = 0;
        foreach (byte b in bytes) value = (value << 8) | b;
        return new(value);
    }
}

public static class SparseOrder
{
    public const int DefaultRebalanceWindow = 256;
    public const int MaximumRebalanceWindow = 4096;

    public static (OrderKey key, int rebalanced) AllocateBetween(IList<SemanticObject> siblings, int insertIndex)
    {
        if (insertIndex < 0 || insertIndex > siblings.Count) throw new ArgumentOutOfRangeException(nameof(insertIndex));
        OrderKey? left = insertIndex > 0 ? siblings[insertIndex - 1].Order : null;
        OrderKey? right = insertIndex < siblings.Count ? siblings[insertIndex].Order : null;
        if (OrderKey.TryBetween(left, right, out var key)) return (key, 0);

        int window = Math.Min(DefaultRebalanceWindow, siblings.Count);
        while (window <= Math.Min(MaximumRebalanceWindow, siblings.Count))
        {
            int changed = RebalanceWindow(siblings, insertIndex, window);
            left = insertIndex > 0 ? siblings[insertIndex - 1].Order : null;
            right = insertIndex < siblings.Count ? siblings[insertIndex].Order : null;
            if (OrderKey.TryBetween(left, right, out key)) return (key, changed);
            if (window == MaximumRebalanceWindow || window == siblings.Count) break;
            window = Math.Min(Math.Min(window * 2, MaximumRebalanceWindow), siblings.Count);
        }
        throw new InvalidOperationException("order_space_exhausted");
    }

    public static int RebalanceWindow(IList<SemanticObject> siblings, int around, int requestedWindow)
    {
        if (siblings.Count == 0) return 0;
        int window = Math.Min(Math.Max(2, requestedWindow), siblings.Count);
        int start = Math.Max(0, around - window / 2);
        int end = Math.Min(siblings.Count, start + window);
        start = Math.Max(0, end - window);
        UInt128 lower = start == 0 ? 0 : siblings[start - 1].Order.Value;
        UInt128 upper = end == siblings.Count ? UInt128.MaxValue : siblings[end].Order.Value;
        UInt128 step = (upper - lower) / (UInt128)(window + 1);
        if (step <= 1) return 0;
        int changed = 0;
        for (int i = start; i < end; i++)
        {
            var next = new OrderKey(lower + step * (UInt128)(i - start + 1));
            if (siblings[i].Order != next)
            {
                siblings[i] = siblings[i] with { Order = next };
                changed++;
            }
        }
        return changed;
    }
}
public sealed record SemanticObject(
    Guid Id,
    string Type,
    Guid? ParentId,
    OrderKey Order,
    string? Role,
    Dictionary<string, object?> Data,
    bool Retired = false)
{
    public SemanticObject WithData(string key, object? value)
    {
        var data = new Dictionary<string, object?>(Data, StringComparer.Ordinal) { [key] = value };
        return this with { Data = data };
    }
}

public sealed record TextBoundary(Guid Id, Guid OwnerId, int ScalarOffset, EdgeAffinity Affinity, AnchorState State = AnchorState.Live);
public sealed record RetainedRange(Guid Id, Guid StartBoundaryId, Guid EndBoundaryId, bool AllowMultiInterval, string Kind, string State = "live");
public sealed record RetiredWitness(Guid ObjectId, RetiredResolution Resolution, IReadOnlyList<Guid> Successors, long ExpiresAfterSequence);
public sealed record AssetCommitment(byte[] Digest, long Length, string State, string? ShellReference = null);

public sealed record ProviderFacet(
    Guid Id,
    string Provider,
    string Kind,
    Guid? TargetId,
    ExtensionCoverageKind CoverageKind,
    ExtensionEditPolicy EditPolicy,
    byte[] ExactPayload,
    byte[] Digest,
    string Alignment,
    bool Required)
{
    public bool DigestValid => CryptographicOperations.FixedTimeEquals(Digest, SHA256.HashData(ExactPayload));
}

public sealed record SourceCapsuleEvidence(
    string Provider,
    byte[] ExactBytes,
    byte[] Digest,
    string Alignment)
{
    public long Length => ExactBytes.LongLength;
    public bool DigestValid => CryptographicOperations.FixedTimeEquals(Digest, SHA256.HashData(ExactBytes));
}

public sealed record ExtensionEnvelope(
    Guid ExtensionId,
    string NamespaceUri,
    string TypeName,
    int Major,
    int Minor,
    string PayloadEncoding,
    byte[] ExactPayload,
    byte[] PayloadDigest,
    ExtensionCoverageKind CoverageKind,
    Guid? TargetId,
    string? PropertyName,
    Guid? StartBoundaryId,
    Guid? EndBoundaryId,
    ExtensionEditPolicy EditPolicy,
    bool Required,
    string? Fallback)
{
    public bool DigestValid => CryptographicOperations.FixedTimeEquals(PayloadDigest, SHA256.HashData(ExactPayload));
}

public sealed record RevisionHead(
    Guid FamilyId,
    Guid BranchId,
    Guid RevisionId,
    long Sequence,
    byte[] SemanticRoot,
    IReadOnlyList<Guid> Parents,
    string Mode);

public sealed record PortableWriteHandle(Guid FamilyId, Guid BranchId, Guid ObjectId, Guid ExpectedRevisionId);
public sealed record SemanticDelta(
    Guid BranchId,
    Guid FromRevisionId,
    Guid ToRevisionId,
    IReadOnlyList<Guid> ChangedObjects,
    IReadOnlyList<Guid> ChangedBoundaries,
    IReadOnlyList<Guid> ChangedExtensions,
    IReadOnlyList<string> LayoutInvalidations,
    byte[] ExactBytes);
public sealed record TransactionResult(bool Success, string Classification, RevisionHead? Head, SemanticDelta? Delta, IReadOnlyList<string> Diagnostics);
public sealed record LocalMutationMetrics(
    int LogicalRecordsRead,
    int LogicalRecordsWritten,
    int MerklePathNodes,
    long PayloadBytesRead,
    long PayloadBytesWritten);

public sealed record DeltaReadResult(
    string Classification,
    long CursorSequence,
    long HeadSequence,
    IReadOnlyList<byte[]> Deltas,
    bool ResyncRequired);