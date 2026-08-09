using System.Collections;
using System.Formats.Cbor;
using System.Text;

namespace DOCSeye.Core;

public static class CanonicalCbor
{
    private sealed class ByteComparer : IComparer<byte[]>
    {
        public static readonly ByteComparer Instance=new();
        public int Compare(byte[]? a,byte[]? b){if(a is null)return b is null?0:-1;if(b is null)return 1;int n=Math.Min(a.Length,b.Length);for(int i=0;i<n;i++){int c=a[i].CompareTo(b[i]);if(c!=0)return c;}return a.Length.CompareTo(b.Length);}
    }
    public static byte[] Encode(object? value){var w=new CborWriter(CborConformanceMode.Canonical);Write(w,value);return w.Encode();}
    public static byte[] EncodeObject(SemanticObject o)
    {
        var d=new Dictionary<string,object?>(StringComparer.Ordinal)
        {
            ["id"]=o.Id,["type"]=o.Type,["parent_id"]=o.ParentId,["role"]=o.Role,["retired"]=o.Retired,["data"]=o.Data
        };
        return Encode(d);
    }
    public static byte[] EncodeRange(RetainedRange r)=>Encode(new Dictionary<string,object?>
    {
        ["id"]=r.Id,["start_boundary_id"]=r.StartBoundaryId,["end_boundary_id"]=r.EndBoundaryId,
        ["allow_multi_interval"]=r.AllowMultiInterval,["kind"]=r.Kind,["state"]=r.State
    });
    public static byte[] EncodeOriginRef(OriginRef r)=>Encode(new Dictionary<string,object?>
    {
        ["current_id"]=r.CurrentId,["kind"]=r.Kind,["source_family_id"]=r.SourceFamilyId,
        ["source_branch_id"]=r.SourceBranchId,["source_revision_id"]=r.SourceRevisionId,["source_id"]=r.SourceId
    });    public static byte[] EncodeBoundary(TextBoundary b)=>Encode(new Dictionary<string,object?>
    {
        ["id"]=b.Id,["owner_id"]=b.OwnerId,["scalar_offset"]=b.ScalarOffset,["affinity"]=AffinityToken(b.Affinity),["state"]=b.State.ToString().ToLowerInvariant()
    });
    public static byte[] EncodeExtension(ExtensionEnvelope e)=>Encode(new Dictionary<string,object?>
    {
        ["extension_id"]=e.ExtensionId,["namespace_uri"]=e.NamespaceUri,["type_name"]=e.TypeName,["major"]=e.Major,["minor"]=e.Minor,["payload_encoding"]=e.PayloadEncoding,["exact_payload"]=e.ExactPayload,["payload_digest"]=e.PayloadDigest,["coverage_kind"]=CoverageToken(e.CoverageKind),["target_id"]=e.TargetId,["property_name"]=e.PropertyName,["start_boundary_id"]=e.StartBoundaryId,["end_boundary_id"]=e.EndBoundaryId,["edit_policy"]=PolicyToken(e.EditPolicy),["required"]=e.Required,["fallback"]=e.Fallback
    });
    public static byte[] EncodeAsset(AssetCommitment a)=>Encode(new Dictionary<string,object?>{{"digest",a.Digest},{"length",a.Length},{"state",a.State},{"shell_reference",a.ShellReference}});
    public static byte[] EncodeProviderFacet(ProviderFacet f)=>Encode(new Dictionary<string,object?>
    {
        ["id"]=f.Id,["provider"]=f.Provider,["kind"]=f.Kind,["target_id"]=f.TargetId,
        ["coverage_kind"]=CoverageToken(f.CoverageKind),["edit_policy"]=PolicyToken(f.EditPolicy),
        ["payload_digest"]=f.Digest,["alignment"]=f.Alignment,["required"]=f.Required
    });
    public static byte[] EncodeSourceCapsule(SourceCapsuleEvidence c)=>Encode(new Dictionary<string,object?>
    {
        ["provider"]=c.Provider,["digest"]=c.Digest,["length"]=c.Length,["alignment"]=c.Alignment
    });
    public static byte[] EncodeRetiredWitness(RetiredWitness w)=>Encode(new Dictionary<string,object?>
    {
        ["object_id"]=w.ObjectId,["resolution"]=RetiredToken(w.Resolution),
        ["successors"]=w.Successors.Cast<object?>().ToArray(),["expires_after_sequence"]=w.ExpiresAfterSequence
    });
    public static string RetiredToken(RetiredResolution r)=>r switch
    {
        RetiredResolution.Destroyed=>"destroyed",RetiredResolution.Split=>"split",RetiredResolution.Merged=>"merged",RetiredResolution.UnknownRetired=>"unknown_retired",_=>throw new ArgumentOutOfRangeException()
    };
    public static string AffinityToken(EdgeAffinity a)=>a switch{EdgeAffinity.IncludeAtEdge=>"include_at_edge",EdgeAffinity.ExcludeAtEdge=>"exclude_at_edge",EdgeAffinity.BeforeInsertion=>"before_insertion",EdgeAffinity.AfterInsertion=>"after_insertion",_=>throw new ArgumentOutOfRangeException()};
    public static string CoverageToken(ExtensionCoverageKind k)=>k switch{ExtensionCoverageKind.Object=>"object",ExtensionCoverageKind.Property=>"property",ExtensionCoverageKind.Subtree=>"subtree",ExtensionCoverageKind.TextInterval=>"text_interval",ExtensionCoverageKind.Relation=>"relation",ExtensionCoverageKind.TopologyRegion=>"topology_region",ExtensionCoverageKind.LayoutProfile=>"layout_profile",ExtensionCoverageKind.DocumentGlobal=>"document_global",_=>throw new ArgumentOutOfRangeException()};
    public static string PolicyToken(ExtensionEditPolicy k)=>k switch{ExtensionEditPolicy.Independent=>"independent",ExtensionEditPolicy.MoveWithTarget=>"move_with_target",ExtensionEditPolicy.GenericTransform=>"generic_transform",ExtensionEditPolicy.Invalidate=>"invalidate",ExtensionEditPolicy.MustUnderstandBeforeEdit=>"must_understand_before_edit",_=>throw new ArgumentOutOfRangeException()};
    private static void Write(CborWriter w,object? v)
    {
        switch(v)
        {
            case null:w.WriteNull();return;
            case bool b:w.WriteBoolean(b);return;
            case byte x:w.WriteUInt64(x);return;
            case ushort x:w.WriteUInt64(x);return;
            case uint x:w.WriteUInt64(x);return;
            case ulong x:w.WriteUInt64(x);return;
            case sbyte x:w.WriteInt64(x);return;
            case short x:w.WriteInt64(x);return;
            case int x:w.WriteInt64(x);return;
            case long x:w.WriteInt64(x);return;
            case string s:ValidateText(s);w.WriteTextString(s);return;
            case byte[] b:w.WriteByteString(b);return;
            case ReadOnlyMemory<byte> m:w.WriteByteString(m.Span);return;
            case Guid g:w.WriteTag((CborTag)37);w.WriteByteString(Ids.RfcBytes(g));return;
            case Enum e:w.WriteTextString(e.ToString().ToLowerInvariant());return;
            case IDictionary<string,object?> map:WriteMap(w,map);return;
            case IReadOnlyDictionary<string,object?> ro:WriteMap(w,ro.ToDictionary(x=>x.Key,x=>x.Value,StringComparer.Ordinal));return;
            case IEnumerable seq when v is not string && v is not byte[]:
                var a=seq.Cast<object?>().ToArray();w.WriteStartArray(a.Length);foreach(var item in a)Write(w,item);w.WriteEndArray();return;
            default: throw new InvalidOperationException($"Unsupported canonical CBOR type {v.GetType().FullName}");
        }
    }
    private static void WriteMap(CborWriter w,IDictionary<string,object?> map)
    {
        var items=new List<(byte[] encodedKey,string key,object? value)>();var seen=new HashSet<string>(StringComparer.Ordinal);
        foreach(var kv in map){if(!seen.Add(kv.Key))throw new InvalidOperationException("duplicate map key");if(kv.Key.Any(c=>c>0x7f))throw new InvalidOperationException("schema keys must be ASCII");var kw=new CborWriter(CborConformanceMode.Canonical);kw.WriteTextString(kv.Key);items.Add((kw.Encode(),kv.Key,kv.Value));}
        items.Sort((a,b)=>ByteComparer.Instance.Compare(a.encodedKey,b.encodedKey));w.WriteStartMap(items.Count);foreach(var i in items){w.WriteTextString(i.key);Write(w,i.value);}w.WriteEndMap();
    }
    public static void ValidateText(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (char.IsHighSurrogate(c))
            {
                if (i + 1 >= s.Length || !char.IsLowSurrogate(s[i + 1])) throw new InvalidOperationException("unpaired surrogate");
                i++;
            }
            else if (char.IsLowSurrogate(c)) throw new InvalidOperationException("unpaired surrogate");
        }
        _ = new UTF8Encoding(false, true).GetBytes(s);
    }

    public static object? Decode(ReadOnlySpan<byte> bytes)
    {
        var r=new CborReader(bytes.ToArray(),CborConformanceMode.Canonical);var v=DecodeValue(r);if(r.PeekState()!=CborReaderState.Finished)throw new InvalidOperationException("multiple CBOR roots");return v;
    }
    private static object? DecodeValue(CborReader r)
    {
        switch(r.PeekState())
        {
            case CborReaderState.UnsignedInteger:return r.ReadUInt64();
            case CborReaderState.NegativeInteger:return r.ReadInt64();
            case CborReaderState.ByteString:return r.ReadByteString();
            case CborReaderState.TextString:return r.ReadTextString();
            case CborReaderState.Boolean:return r.ReadBoolean();
            case CborReaderState.Null:r.ReadNull();return null;
            case CborReaderState.Tag:
                var tag=r.ReadTag();if((ulong)tag==37){var b=r.ReadByteString();if(b.Length!=16)throw new InvalidOperationException("invalid uuid");return Ids.GuidFromRfcBytes(b);}return new TaggedValue((ulong)tag,DecodeValue(r));
            case CborReaderState.StartArray:
                r.ReadStartArray();var a=new List<object?>();while(r.PeekState()!=CborReaderState.EndArray)a.Add(DecodeValue(r));r.ReadEndArray();return a.ToArray();
            case CborReaderState.StartMap:
                r.ReadStartMap();var d=new Dictionary<string,object?>(StringComparer.Ordinal);while(r.PeekState()!=CborReaderState.EndMap){string k=r.ReadTextString();if(!d.TryAdd(k,DecodeValue(r)))throw new InvalidOperationException("duplicate key");}r.ReadEndMap();return d;
            default:throw new InvalidOperationException("unsupported CBOR state");
        }
    }
    public sealed record TaggedValue(ulong Tag,object? Value);
    public static string ValidateRecord(ReadOnlySpan<byte> bytes,int maxDepth=64,int maxEntries=1_000_000,int maxScalarBytes=64*1024*1024)
    {
        try{var r=new CborReader(bytes.ToArray(),CborConformanceMode.Canonical);int entries=0;ReadValue(r,0,maxDepth,maxEntries,maxScalarBytes,ref entries);if(r.PeekState()!=CborReaderState.Finished)return "malformed_cbor";return "valid";}
        catch(CborContentException){return "malformed_cbor";}catch(InvalidOperationException){return "malformed_cbor";}catch(OverflowException){return "resource_limit";}
    }
    private static void ReadValue(CborReader r,int depth,int maxDepth,int maxEntries,int maxScalarBytes,ref int entries)
    {
        if(depth>maxDepth)throw new OverflowException();if(entries++>maxEntries)throw new OverflowException();
        switch(r.PeekState())
        {
            case CborReaderState.UnsignedInteger:r.ReadUInt64();break;
            case CborReaderState.NegativeInteger:r.ReadInt64();break;
            case CborReaderState.ByteString:if(r.ReadByteString().Length>maxScalarBytes)throw new OverflowException();break;
            case CborReaderState.TextString:var s=r.ReadTextString();if(Encoding.UTF8.GetByteCount(s)>maxScalarBytes)throw new OverflowException();break;
            case CborReaderState.Boolean:r.ReadBoolean();break;
            case CborReaderState.Null:r.ReadNull();break;
            case CborReaderState.Tag:
                var tag=r.ReadTag();if((ulong)tag==37){if(r.PeekState()!=CborReaderState.ByteString)throw new InvalidOperationException();var u=r.ReadByteString();if(u.Length!=16)throw new InvalidOperationException();}else ReadValue(r,depth+1,maxDepth,maxEntries,maxScalarBytes,ref entries);break;
            case CborReaderState.StartArray:
                int? n=r.ReadStartArray();int count=0;while(r.PeekState()!=CborReaderState.EndArray){ReadValue(r,depth+1,maxDepth,maxEntries,maxScalarBytes,ref entries);if(++count>maxEntries)throw new OverflowException();}r.ReadEndArray();if(n is int nn && nn!=count)throw new InvalidOperationException();break;
            case CborReaderState.StartMap:
                int? m=r.ReadStartMap();var keys=new HashSet<string>(StringComparer.Ordinal);int mc=0;while(r.PeekState()!=CborReaderState.EndMap){if(r.PeekState()!=CborReaderState.TextString)throw new InvalidOperationException();string k=r.ReadTextString();if(k.Any(c=>c>0x7f)||!keys.Add(k))throw new InvalidOperationException();ReadValue(r,depth+1,maxDepth,maxEntries,maxScalarBytes,ref entries);if(++mc>maxEntries)throw new OverflowException();}r.ReadEndMap();if(m is int mm && mm!=mc)throw new InvalidOperationException();break;
            case CborReaderState.HalfPrecisionFloat:case CborReaderState.SinglePrecisionFloat:case CborReaderState.DoublePrecisionFloat:throw new InvalidOperationException("binary float forbidden");
            default:throw new InvalidOperationException("unsupported CBOR state");
        }
    }
}