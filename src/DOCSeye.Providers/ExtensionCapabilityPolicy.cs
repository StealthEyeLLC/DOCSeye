using DOCSeye.Core;

namespace DOCSeye.Providers;

public sealed record ExtensionCapabilityObservation(Guid ExtensionId,string NamespaceUri,string TypeName,CapabilityState State,bool Required,string? Fallback,string Classification);

public static class ExtensionCapabilityPolicy
{
    public static IReadOnlyList<ExtensionCapabilityObservation> Assess(SemanticState state,IReadOnlySet<string>? understoodNamespaces=null)
    {
        understoodNamespaces ??= new HashSet<string>(StringComparer.Ordinal);
        var result=new List<ExtensionCapabilityObservation>();
        foreach(var e in state.Extensions.Values.OrderBy(x=>Ids.Lower(x.ExtensionId),StringComparer.Ordinal))
        {
            CapabilityState capability;string classification;
            if(!e.DigestValid){capability=CapabilityState.Invalid;classification="invalid";}
            else if(understoodNamespaces.Contains(e.NamespaceUri)){capability=e.EditPolicy is ExtensionEditPolicy.MoveWithTarget or ExtensionEditPolicy.GenericTransform?CapabilityState.SupportedWithTransform:CapabilityState.Supported;classification="supported";}
            else if(e.Required&&e.EditPolicy is ExtensionEditPolicy.MustUnderstandBeforeEdit or ExtensionEditPolicy.Invalidate){capability=CapabilityState.BlockedRequiredExtension;classification="blocked_required_extension";}
            else if(!string.IsNullOrWhiteSpace(e.Fallback)){capability=CapabilityState.FallbackOnly;classification="fallback_only";}
            else {capability=CapabilityState.PreservedOpaque;classification="preserved_opaque";}
            result.Add(new(e.ExtensionId,e.NamespaceUri,e.TypeName,capability,e.Required,e.Fallback,classification));
        }
        return result;
    }
}