# 07 — Capability Matrix

Status: **PLANNED CONTRACT / NOT IMPLEMENTED**

## 1. Legend

| Mark | Meaning |
| --- | --- |
| **B1** | required and acceptance-tested in Build 001 |
| **Obs** | observed/indexed in Build 001 but not generally mutated |
| **Provider** | operation is routed to a qualified native provider |
| **Later** | permanent architecture accommodates it; not Build 001 |
| **No** | deliberately outside DOCSeye or rejected |

“Retain” means create a persistent logical correspondence concept with explicit resolution state. It never promises that the concept will have an exact binding after every external edit.

## 2. Format and representation matrix

| Representation | Native inspection | Current index | Retained concepts | Typed mutation | Layout/render | Build 001 role |
| --- | --- | --- | --- | --- | --- | --- |
| Transitional DOCX package | B1 | B1 | B1 | B1 | via Word | primary editable truth |
| Live desktop Word document | B1 | reconciled B1 | provider-epoch bindings | bounded Provider | B1 | mandatory native/live/layout authority |
| Word-exported PDF | Obs | affected pages/regions | derivative relation | No | B1 | derived fixed-format proof |
| Strict DOCX | experiment/accurate capability report | Later | Later | Later | Later | no silent conversion |
| DOCM | preservation experiment | opaque/native manifest | No new macro concepts | unrelated bounded edit experiment only | Word smoke | preserve, never execute VBA |
| General PDF | Later | Later | Later | annotations/forms/pages later | native fixed layout | separate provider |
| ODF | Later | Later | Later | Later | provider-specific | future provider |
| Static HTML | Later | Later | Later | Later | browser/render facet | persistent artifact only |
| Markdown | Later/correlated | Later | Later | provider/source-specific | derived | CODEeye boundary applies |
| XLSX | opaque/embedded correlation | No deep index | No deep concepts | No | No | DATAeye domain |
| PPTX | opaque/embedded correlation | No deep index | No deep concepts | No | No | presentation domain |
| Encrypted Office envelope | capability/error only | No without authorized access | document carrier only | No without provider access | No | truthful inaccessible state |

## 3. DOCX semantic-object matrix

| Object/facet | Index | Query | Retain | Build 001 mutations | Strongest planned exact evidence | Important non-identity evidence |
| --- | --- | --- | --- | --- | --- | --- |
| Document | B1 | B1 | intrinsic | open/sync/save transaction | controlled lineage; proven provider/item lineage | path, title, `docId`, hashes |
| Representation/package | B1 | B1 | intrinsic | copy-on-write commit | exact package/provider revision | ZIP metadata |
| Story/flow | B1 | B1 | B1 | via contained operations | provider part plus document lineage | display order |
| Section | B1 | B1 | B1 | inspect; structural effects through typed ops | operation lineage; exact boundary correspondence | ordinal/page |
| Paragraph | B1 | B1 | B1 | text insert/delete, style, split/merge lineage | operation lineage; scoped `paraId` plus corroboration | text, ordinal, page |
| Heading role | B1 | B1 | with paragraph | style/role via paragraph style operation | paragraph identity plus provider role | visible text |
| Text span | B1 | B1 | B1 | exact replacement/format/tracked insert/delete | known transforms; native markers; exact parent/boundaries | phrase, offsets |
| Run | provider-local | local inspection | normally No | changed as representation consequence | exact current XML node only | formatting similarity |
| List | B1 derived | B1 | B1 | insert/move/relevel/restart/definition | operation lineage; exact member continuity | `numId`, displayed marker |
| List item | B1 | B1 | B1 | text, insert, move, level | paragraph identity plus numbering facet | ordinal/display number |
| Table | B1 | B1 | B1 | structural row/cell operations | operation lineage; exact parent/subtree | ordinal/header text |
| Row | B1 | B1 | B1 | insert/move/delete | operation lineage; exact native row witness | index/visible values |
| Physical cell | B1 | B1 | provider binding | content/structural effects | row lineage plus exact native child | coordinate/text |
| Logical cell/region | B1 derived | B1 | B1 | set/merge/split where supported | operation lineage; row/table/topology/descendants | row/column coordinate |
| Style definition | B1 | B1 | B1 | apply and bounded definition mutation | document-scoped style ID plus lineage | visible name |
| Effective formatting | B1 derived | B1 | revision-local projection | No direct mutation | provider-computed at exact revision | visual appearance |
| Comment/thread | B1 | B1 | B1 | add/reply/resolve exact | operation lineage; modern durable ID; legacy ID+anchor | quoted text/author/time |
| Native tracked change | B1 | B1 | B1 | create insertion/deletion; accept exact | operation lineage; exact provider element/witnesses | author/text/time/index |
| Field | B1 | B1 | B1 | request qualified update/state change | exact owning structure/instruction | cached result alone |
| Content control | B1 | B1 | B1 | set exact value | Word control ID in proven document lineage | tag/title/text |
| Bookmark | B1 | B1 | B1 | exact add/update | exact paired markers and operation lineage | name/text |
| Hyperlink | B1 | B1 | B1 | add/update bounded link | exact relation and owning span/object | visible URL text |
| Footnote/endnote | B1 | B1 | B1 | inspect in B1 | native note ID plus exact document lineage | displayed ordinal |
| Figure occurrence | B1 | B1 | B1 | alt text; placement observation | operation/drawing anchor/relationship lineage | media hash/location |
| Media asset | B1 lazy | B1 | B1 where useful | preserve; no general B1 byte edit | exact part relation and digest | filename/perceptual similarity |
| OMML math | B1 | B1 | B1 | inspect/preserve | exact native subtree and parent lineage | rendered Unicode/text |
| Embedded/OLE/custom part | manifest/opaque | B1 metadata | only as opaque provider object | preserve, not execute | exact part/relationship | filename/content guess |
| Custom XML binding | B1 relation | B1 | with control | preserve/set through qualified control | exact mapping/control/provider lineage | XPath text alone |
| Header/footer inheritance | B1 derived | B1 | source semantic objects | inspect; effects through exact source edits | exact section/source relationship | visible duplicated content |

## 4. Assurance and write eligibility

Observation origin and mutation resolution are deliberately separate axes.

| Observation origin | Meaning | May support exact mutation by itself? |
| --- | --- | --- |
| `native_stored` | directly encoded in the current provider artifact | only with exact object binding/preconditions |
| `native_live` | current native application state | only within the proven provider epoch/revision |
| `provider_computed` | provider-derived formatting/layout/result | no; target must be semantic/native exact |
| `reconstructed_exact` | deterministically reconstructed from proven lineage | yes, if resolution is `exact_rebased` |
| `heuristic_structure` | inferred grouping/reading order | no |
| `ocr_derived` | recognition over rendered pixels | no |
| `visual_inferred` | vision-derived observation | no |

| Resolution | Read behavior | Write behavior |
| --- | --- | --- |
| `exact_current` | current object returned | permitted with revision/precondition checks |
| `exact_rebased` | current object plus rebase evidence returned | permitted with revision/precondition checks |
| `stale` | old concept and reason may be inspected | rejected |
| `ambiguous` | candidates may be returned for user/model selection as a new action | old handle rejected |
| `destroyed` | lifecycle result returned | rejected |
| `inferred` | derived observation returned with assurance | rejected until explicitly bound to exact native truth |

## 5. Transaction operation matrix

| Operation family | Package provider | Word provider | Build 001 acceptance |
| --- | --- | --- | --- |
| exact plain text replacement/insertion | preferred when safe | when Word owns live state | B1 |
| direct character/paragraph formatting | bounded typed patch | live-state route | B1 |
| paragraph style assignment | bounded typed patch | live-state route | B1 |
| list structure/definition | typed OOXML where proven | provider fallback if needed | B1 bounded set |
| table row/cell mutation | typed topology-aware patch | provider fallback if needed | B1 bounded set |
| comment add/reply/resolve | package where exact modern/legacy semantics proven | mandatory fallback/native behavior | B1 |
| native tracked insert/delete/accept | package only if experiments prove output | qualified Word path expected | B1 |
| content-control value | package where binding semantics safe | qualified Word path | B1 |
| bookmark upsert | bounded package edit | live-state route | B1 |
| hyperlink add | bounded part plus relationship edit | live-state route | B1 |
| field update/recalculation | request flag only, not result authority | authoritative Word update | B1 via Word |
| figure alt text | bounded drawing-property edit | live-state route | B1 |
| arbitrary drawing/chart/OLE reconstruction | No | No general promise | Later/unsupported |
| raw XML/ZIP escape hatch | expert diagnostic only | n/a | never counts toward B1 Program Host |

## 6. Query, delta, layout, and render matrix

| Capability | Build 001 | Contract |
| --- | --- | --- |
| Heading/section queries | B1 | compact hierarchy and semantic slices |
| Full-text search | B1 | local FTS; embeddings optional later and never identity |
| Table predicate/topology query | B1 | operates on logical topology, not tab text |
| Comment/change/control/field queries | B1 | native/provider facets and explicit projections |
| Changes since revision | B1 | bounded semantic delta, explicit `resync_required` gap |
| Document/provider synchronization | B1 | named scopes and coherent revision result |
| Layout wait | B1 | result must name Word provider/configuration and source revision |
| Affected-page mapping | B1 | layout-revision-scoped pages/regions correlated to semantic objects |
| Page rendering | B1 | affected pages only in D; pixels are render truth |
| Cross-engine pagination equality | No | engines remain separately labeled |

## 7. Failure/capability outcomes

The gateway and SDK return typed outcomes, including:

- `stale_document_revision`;
- `stale_provider_revision`;
- `stale_target`;
- `ambiguous_target`;
- `destroyed_target`;
- `unsupported_exact_mutation`;
- `unsafe_preservation_boundary`;
- `provider_unavailable` or `provider_ahead`;
- `package_invalid`;
- `signature_impact`;
- `encrypted_inaccessible`;
- `resync_required`;
- `layout_not_current`.

None is converted into a best-effort mutation. A capability matrix is truthful only when unsupported and uncertain states remain observable.
