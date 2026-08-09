using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace DOCSeye.Core;

public static class DndConstants
{
    public const string ArchitectureFreeze = "0b10e8ed6ada2e0aaef3f1196de19bf7a4631bec";
    public const int FormatMajor = 1;
    public const int FormatMinor = 0;
    public static readonly string[] RootDomains = ["objects","boundaries","extensions","assets","relations","layout","commit"];
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
        Span<byte> b = stackalloc byte[16]; rng.Fill(b); b[6]=(byte)((b[6]&0x0f)|0x40); b[8]=(byte)((b[8]&0x3f)|0x80); return GuidFromRfcBytes(b);
    }
    public static byte[] RfcBytes(Guid id)
    {
        Span<byte> dotnet=stackalloc byte[16]; id.TryWriteBytes(dotnet); byte[] b=dotnet.ToArray();
        Array.Reverse(b,0,4); Array.Reverse(b,4,2); Array.Reverse(b,6,2); return b;
    }
    public static Guid GuidFromRfcBytes(ReadOnlySpan<byte> rfc)
    {
        if(rfc.Length!=16) throw new ArgumentException("uuid requires 16 bytes");
        byte[] b=rfc.ToArray(); Array.Reverse(b,0,4); Array.Reverse(b,4,2); Array.Reverse(b,6,2); return new Guid(b);
    }
    public static string Lower(Guid id)=>id.ToString("D").ToLowerInvariant();
    public static bool IsV4(Guid id)=> (RfcBytes(id)[6]>>4)==4 && (RfcBytes(id)[8]&0xC0)==0x80;
}

public sealed class RandomNumberGeneratorLike(ulong seed)
{
    private ulong s0=SplitMix(ref seed), s1=SplitMix(ref seed);
    private static ulong SplitMix(ref ulong x){x+=0x9E3779B97F4A7C15UL; ulong z=x; z=(z^(z>>30))*0xBF58476D1CE4E5B9UL;z=(z^(z>>27))*0x94D049BB133111EBUL;return z^(z>>31);}    
    private ulong Next(){ulong x=s0,y=s1;s0=y;x^=x<<23;s1=x^y^(x>>17)^(y>>26);return s1+y;}
    public void Fill(Span<byte> b){int o=0;while(o<b.Length){ulong n=Next();for(int i=0;i<8&&o<b.Length;i++,o++)b[o]=(byte)(n>>(i*8));}}
}

public readonly record struct OrderKey(UInt128 Value) : IComparable<OrderKey>
{
    public static readonly UInt128 InitialStep = (UInt128)1 << 120;
    public static OrderKey Initial(int ordinal)=>new(InitialStep*(UInt128)(ordinal+1));
    public static bool TryBetween(OrderKey? left, OrderKey? right, out OrderKey key)
    {
        UInt128 l=left?.Value ?? 0; UInt128 r=right?.Value ?? UInt128.MaxValue;
        if(r<=l+1){key=default;return false;} key=new OrderKey(l+(r-l)/2);return true;
    }
    public int CompareTo(OrderKey other)=>Value.CompareTo(other.Value);
    public byte[] Bytes(){byte[] b=new byte[16]; UInt128 v=Value; for(int i=15;i>=0;i--){b[i]=(byte)v;v>>=8;}return b;}
    public static OrderKey FromBytes(ReadOnlySpan<byte> b){if(b.Length!=16)throw new ArgumentException();UInt128 v=0;foreach(byte x in b)v=(v<<8)|x;return new(v);}
}

public sealed record SemanticObject(Guid Id,string Type,Guid? ParentId,OrderKey Order,string? Role,Dictionary<string,object?> Data,bool Retired=false)
{
    public SemanticObject WithData(string key,object? value){var d=new Dictionary<string,object?>(Data,StringComparer.Ordinal){[key]=value};return this with{Data=d};}
}

public sealed record TextBoundary(Guid Id,Guid OwnerId,int ScalarOffset,EdgeAffinity Affinity,AnchorState State=AnchorState.Live);
public sealed record RetainedRange(Guid Id,Guid StartBoundaryId,Guid EndBoundaryId,bool AllowMultiInterval,string Kind,string State="live");
public sealed record RetiredWitness(Guid ObjectId,RetiredResolution Resolution,IReadOnlyList<Guid> Successors,long ExpiresAfterSequence);
public sealed record AssetCommitment(byte[] Digest,long Length,string State,string? ShellReference=null);
public sealed record ProviderFacet(Guid Id,string Provider,string Kind,Guid? TargetId,byte[] ExactPayload,byte[] Digest,string Alignment,bool Required);

public sealed record ExtensionEnvelope(Guid ExtensionId,string NamespaceUri,string TypeName,int Major,int Minor,string PayloadEncoding,byte[] ExactPayload,byte[] PayloadDigest,ExtensionCoverageKind CoverageKind,Guid? TargetId,string? PropertyName,Guid? StartBoundaryId,Guid? EndBoundaryId,ExtensionEditPolicy EditPolicy,bool Required,string? Fallback)
{
    public bool DigestValid => CryptographicOperations.FixedTimeEquals(PayloadDigest,SHA256.HashData(ExactPayload));
}

public sealed record RevisionHead(Guid FamilyId,Guid BranchId,Guid RevisionId,long Sequence,byte[] SemanticRoot,IReadOnlyList<Guid> Parents,string Mode);
public sealed record PortableWriteHandle(Guid FamilyId,Guid BranchId,Guid ObjectId,Guid ExpectedRevisionId);
public sealed record SemanticDelta(Guid BranchId,Guid FromRevisionId,Guid ToRevisionId,IReadOnlyList<Guid> ChangedObjects,IReadOnlyList<Guid> ChangedBoundaries,IReadOnlyList<Guid> ChangedExtensions,IReadOnlyList<string> LayoutInvalidations,byte[] ExactBytes);
public sealed record TransactionResult(bool Success,string Classification,RevisionHead? Head,SemanticDelta? Delta,IReadOnlyList<string> Diagnostics);