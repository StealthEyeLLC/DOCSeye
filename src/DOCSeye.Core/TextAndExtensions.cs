using System.Globalization;
using System.Text;

namespace DOCSeye.Core;

public static class UnicodeText
{
    public static string GraphemeUnicodeVersion => Unicode17GraphemeData.UnicodeVersion;
    public static int ScalarCount(string text) => text.EnumerateRunes().Count();

    public static int Utf16IndexFromScalar(string text, int scalar)
    {
        if (scalar < 0) throw new ArgumentOutOfRangeException(nameof(scalar));
        int s = 0, i = 0;
        foreach (var r in text.EnumerateRunes())
        {
            if (s == scalar) return i;
            i += r.Utf16SequenceLength;
            s++;
        }
        if (s == scalar) return text.Length;
        throw new ArgumentOutOfRangeException(nameof(scalar));
    }

    public static string SliceScalars(string text, int start, int length)
    {
        int a = Utf16IndexFromScalar(text, start), b = Utf16IndexFromScalar(text, start + length);
        return text[a..b];
    }

    public static bool IsGraphemeBoundary(string text, int scalar) => GraphemeScalarBoundaries(text).Contains(scalar);

    public static IReadOnlyList<int> GraphemeScalarBoundaries(string text)
    {
        var runes = text.EnumerateRunes().ToArray();
        var boundaries = new List<int> { 0 };
        for (int i = 1; i < runes.Length; i++) if (ShouldBreak(runes, i)) boundaries.Add(i);
        if (runes.Length > 0) boundaries.Add(runes.Length);
        return boundaries;
    }

    private static bool ShouldBreak(Rune[] r, int i)
    {
        string a = Gcb(r[i - 1].Value), b = Gcb(r[i].Value);
        if (a == "CR" && b == "LF") return false; // GB3
        if (IsControl(a)) return true;              // GB4
        if (IsControl(b)) return true;              // GB5
        if (a == "L" && (b is "L" or "V" or "LV" or "LVT")) return false; // GB6
        if ((a is "LV" or "V") && (b is "V" or "T")) return false;          // GB7
        if ((a is "LVT" or "T") && b == "T") return false;                    // GB8
        if (b is "Extend" or "ZWJ") return false;                                  // GB9
        if (b == "SpacingMark") return false;                                         // GB9a
        if (a == "Prepend") return false;                                             // GB9b
        if (Incb(r[i].Value) == "Consonant" && MatchesGb9c(r, i)) return false;       // GB9c
        if (IsExtendedPictographic(r[i].Value) && a == "ZWJ" && MatchesGb11(r, i)) return false; // GB11
        if (a == "Regional_Indicator" && b == "Regional_Indicator")
        {
            int count = 0;
            for (int j = i - 1; j >= 0 && Gcb(r[j].Value) == "Regional_Indicator"; j--) count++;
            if ((count & 1) == 1) return false; // GB12/13
        }
        return true; // GB999
    }

    private static bool MatchesGb9c(Rune[] r, int boundary)
    {
        int j = boundary - 1;
        bool sawLinker = false;
        while (j >= 0)
        {
            string p = Incb(r[j].Value);
            if (p == "Linker") { sawLinker = true; j--; continue; }
            if (p == "Extend") { j--; continue; }
            break;
        }
        return sawLinker && j >= 0 && Incb(r[j].Value) == "Consonant";
    }

    private static bool MatchesGb11(Rune[] r, int boundary)
    {
        int j = boundary - 2; // previous rune is ZWJ
        while (j >= 0 && Gcb(r[j].Value) == "Extend") j--;
        return j >= 0 && IsExtendedPictographic(r[j].Value);
    }

    private static bool IsControl(string p) => p is "Control" or "CR" or "LF";
    private static string Gcb(int cp) => Lookup(Unicode17GraphemeData.Gcb, cp, "Other");
    private static string Incb(int cp) => Lookup(Unicode17GraphemeData.Incb, cp, "None");
    private static bool IsExtendedPictographic(int cp) => Lookup(Unicode17GraphemeData.ExtendedPictographic, cp, "") == "Extended_Pictographic";

    private static string Lookup(Unicode17GraphemeData.Range[] ranges, int cp, string fallback)
    {
        int lo = 0, hi = ranges.Length - 1;
        while (lo <= hi)
        {
            int mid = lo + ((hi - lo) >> 1);
            var x = ranges[mid];
            if (cp < x.Start) hi = mid - 1;
            else if (cp > x.End) lo = mid + 1;
            else return x.Value;
        }
        return fallback;
    }

    public static string Insert(string text, int scalar, string inserted, bool humanFacing = true)
    {
        if (humanFacing && !IsGraphemeBoundary(text, scalar)) throw new InvalidOperationException("invalid_grapheme_boundary");
        int u = Utf16IndexFromScalar(text, scalar);
        return text[..u] + inserted + text[u..];
    }

    public static string Delete(string text, int start, int count, bool humanFacing = true)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (humanFacing && (!IsGraphemeBoundary(text, start) || !IsGraphemeBoundary(text, start + count))) throw new InvalidOperationException("invalid_grapheme_boundary");
        int a = Utf16IndexFromScalar(text, start), b = Utf16IndexFromScalar(text, start + count);
        return text[..a] + text[b..];
    }
}
public static class BoundaryTransform
{
    public static void ApplyInsertion(SemanticState state,Guid owner,int at,string inserted,IEnumerable<RetainedRange> ranges)
    {
        int n=UnicodeText.ScalarCount(inserted);var roles=Roles(ranges);foreach(var id in state.Boundaries.Values.Where(b=>b.OwnerId==owner&&b.State==AnchorState.Live).Select(b=>b.Id).ToArray())
        {
            var b=state.Boundaries[id];if(b.ScalarOffset<at)continue;if(b.ScalarOffset>at){state.Boundaries[id]=b with{ScalarOffset=b.ScalarOffset+n};continue;}
            bool move=b.Affinity switch{EdgeAffinity.BeforeInsertion=>false,EdgeAffinity.AfterInsertion=>true,EdgeAffinity.IncludeAtEdge=>roles.TryGetValue(id,out var role)&&role=="end",EdgeAffinity.ExcludeAtEdge=>roles.TryGetValue(id,out var role2)&&role2=="start",_=>false};
            if(move)state.Boundaries[id]=b with{ScalarOffset=b.ScalarOffset+n};
        }
    }
    public static void ApplyDeletion(SemanticState state,Guid owner,int start,int count)
    {
        int end=start+count;foreach(var id in state.Boundaries.Values.Where(b=>b.OwnerId==owner&&b.State==AnchorState.Live).Select(b=>b.Id).ToArray())
        {
            var b=state.Boundaries[id];int p=b.ScalarOffset;int np=p<=start?p:p>=end?p-count:start;state.Boundaries[id]=b with{ScalarOffset=np};
        }
    }
    public static (Guid left,Guid right) SplitTextBlock(SemanticState state,Guid owner,int split,IEnumerable<RetainedRange> ranges,Func<Guid> newId,long lineageExpiry)
    {
        if(!state.Objects.TryGetValue(owner,out var o)||o.Retired||o.Type!="text_block")throw new InvalidOperationException("invalid_split_target");string text=(string)o.Data["text"]!;if(!UnicodeText.IsGraphemeBoundary(text,split))throw new InvalidOperationException("invalid_grapheme_boundary");int u=UnicodeText.Utf16IndexFromScalar(text,split);Guid l=newId(),r=newId();
        var ld=new Dictionary<string,object?>(o.Data,StringComparer.Ordinal){["text"]=text[..u]};var rd=new Dictionary<string,object?>(o.Data,StringComparer.Ordinal){["text"]=text[u..]};
        state.Objects[owner]=o with{Retired=true};state.Objects[l]=new(l,"text_block",o.ParentId,new OrderKey(o.Order.Value),o.Role,ld);state.Objects[r]=new(r,"text_block",o.ParentId,new OrderKey(o.Order.Value+1),o.Role,rd);
        var roles=Roles(ranges);foreach(var bid in state.Boundaries.Values.Where(b=>b.OwnerId==owner&&b.State==AnchorState.Live).Select(b=>b.Id).ToArray())
        {
            var b=state.Boundaries[bid];bool goRight=b.ScalarOffset>split||(b.ScalarOffset==split&&AtSplitGoesRight(b,roles.TryGetValue(bid,out var role)?role:null));state.Boundaries[bid]=goRight?b with{OwnerId=r,ScalarOffset=Math.Max(0,b.ScalarOffset-split)}:b with{OwnerId=l};
        }
        state.Retired[owner]=new(owner,RetiredResolution.Split,[l,r],lineageExpiry);return(l,r);
    }
    public static Guid MergeTextBlocks(SemanticState state,Guid left,Guid right,Func<Guid> newId,long lineageExpiry)
    {
        if(!state.Objects.TryGetValue(left,out var l)||!state.Objects.TryGetValue(right,out var r)||l.Retired||r.Retired||l.Type!="text_block"||r.Type!="text_block")throw new InvalidOperationException("invalid_merge_targets");string lt=(string)l.Data["text"]!,rt=(string)r.Data["text"]!;int ls=UnicodeText.ScalarCount(lt);Guid result=newId();var d=new Dictionary<string,object?>(l.Data,StringComparer.Ordinal){["text"]=lt+rt};state.Objects[left]=l with{Retired=true};state.Objects[right]=r with{Retired=true};state.Objects[result]=new(result,"text_block",l.ParentId,l.Order,l.Role,d);
        foreach(var bid in state.Boundaries.Values.Where(b=>b.OwnerId==left||b.OwnerId==right).Select(b=>b.Id).ToArray()){var b=state.Boundaries[bid];state.Boundaries[bid]=b.OwnerId==left?b with{OwnerId=result}:b with{OwnerId=result,ScalarOffset=ls+b.ScalarOffset};}
        state.Retired[left]=new(left,RetiredResolution.Merged,[result],lineageExpiry);state.Retired[right]=new(right,RetiredResolution.Merged,[result],lineageExpiry);return result;
    }
    private static bool AtSplitGoesRight(TextBoundary b,string? role)=>b.Affinity switch{EdgeAffinity.AfterInsertion=>true,EdgeAffinity.BeforeInsertion=>false,EdgeAffinity.IncludeAtEdge=>role=="start",EdgeAffinity.ExcludeAtEdge=>role=="start",_=>role=="start"};
    private static Dictionary<Guid,string> Roles(IEnumerable<RetainedRange> ranges){var d=new Dictionary<Guid,string>();foreach(var r in ranges)foreach(var interval in r.EffectiveIntervals){AddRole(d,interval.StartBoundaryId,"start");AddRole(d,interval.EndBoundaryId,"end");}return d;}
    private static void AddRole(Dictionary<Guid,string> roles,Guid id,string role){if(roles.TryGetValue(id,out var existing)&&existing!=role)throw new InvalidOperationException("boundary_role_conflict");roles[id]=role;}
}

public sealed record EditIntent(string Kind,Guid? TargetId=null,string? PropertyName=null,Guid? StartBoundaryId=null,Guid? EndBoundaryId=null,bool TopologyMutation=false,bool Copy=false,bool Move=false);
public sealed record ExtensionDecision(bool Allowed,string Classification,bool RequiresTransform,IReadOnlyList<Guid> Intersecting);

public static class ExtensionPolicyEngine
{
    public static ExtensionDecision Check(SemanticState state,EditIntent edit)
    {
        var hits=state.Extensions.Values.Where(e=>Intersects(state,e,edit)).ToList();bool transform=false;
        foreach(var e in hits)
        {
            if(!e.DigestValid)return new(false,"invalid",false,[e.ExtensionId]);
            if(e.EditPolicy==ExtensionEditPolicy.MustUnderstandBeforeEdit&&e.Required)return new(false,"blocked_required_extension",false,[e.ExtensionId]);
            if(e.EditPolicy==ExtensionEditPolicy.MustUnderstandBeforeEdit)return new(false,"unsupported",false,[e.ExtensionId]);
            if(e.EditPolicy==ExtensionEditPolicy.Invalidate&&e.Required)return new(false,"blocked_required_extension",false,[e.ExtensionId]);
            if(e.EditPolicy is ExtensionEditPolicy.MoveWithTarget or ExtensionEditPolicy.GenericTransform)transform=true;
        }
        return new(true,"allowed",transform,hits.Select(x=>x.ExtensionId).ToArray());
    }
    public static bool Intersects(SemanticState state,ExtensionEnvelope e,EditIntent edit)
    {
        if(e.CoverageKind==ExtensionCoverageKind.DocumentGlobal)return true;
        if(e.CoverageKind==ExtensionCoverageKind.LayoutProfile)return edit.Kind.StartsWith("layout",StringComparison.Ordinal);
        if(e.CoverageKind==ExtensionCoverageKind.Property)return e.TargetId==edit.TargetId&&(edit.PropertyName is null||e.PropertyName==edit.PropertyName);
        if(e.CoverageKind is ExtensionCoverageKind.Object or ExtensionCoverageKind.Relation)return e.TargetId==edit.TargetId;
        if(e.CoverageKind==ExtensionCoverageKind.Subtree){if(e.TargetId==edit.TargetId)return true;Guid? p=edit.TargetId;while(p is Guid id&&state.Objects.TryGetValue(id,out var o)){if(o.ParentId==e.TargetId)return true;p=o.ParentId;}return false;}
        if(e.CoverageKind==ExtensionCoverageKind.TopologyRegion)return edit.TopologyMutation&&e.TargetId==edit.TargetId;
        if(e.CoverageKind==ExtensionCoverageKind.TextInterval)
        {
            if(e.TargetId!=edit.TargetId)return false;if(e.StartBoundaryId is not Guid sb||e.EndBoundaryId is not Guid eb||!state.Boundaries.TryGetValue(sb,out var s)||!state.Boundaries.TryGetValue(eb,out var end))return true;
            if(edit.StartBoundaryId is Guid es&&state.Boundaries.TryGetValue(es,out var isb)&&edit.EndBoundaryId is Guid ee&&state.Boundaries.TryGetValue(ee,out var ieb))return isb.ScalarOffset<=end.ScalarOffset&&ieb.ScalarOffset>=s.ScalarOffset;
            return edit.Kind.Contains("text",StringComparison.Ordinal)||edit.Kind.Contains("split",StringComparison.Ordinal)||edit.Kind.Contains("delete",StringComparison.Ordinal);
        }
        return true;
    }
    public static ExtensionEnvelope RemapForCopy(ExtensionEnvelope e,IReadOnlyDictionary<Guid,Guid> map,Func<Guid> newId)
    {
        if(e.EditPolicy is not (ExtensionEditPolicy.GenericTransform or ExtensionEditPolicy.MoveWithTarget))throw new InvalidOperationException(e.Required?"blocked_required_extension":"unsupported");Guid? Map(Guid? id)=>id is Guid g&&map.TryGetValue(g,out var n)?n:id;
        return e with{ExtensionId=newId(),TargetId=Map(e.TargetId),StartBoundaryId=Map(e.StartBoundaryId),EndBoundaryId=Map(e.EndBoundaryId)};
    }
}