# 07 — Capability Matrix

Status: **CANONICAL PREIMPLEMENTATION CONTRACT**

This matrix states architectural ownership and Build 001 intent. It is not a measured support claim.

## 1. Capability-state vocabulary

| State | Meaning |
| --- | --- |
| `supported_native` | operation is defined on native semantic truth and is in the frozen Build 001 slice |
| `supported_foreign_managed` | operation follows DOCX-first provider/correspondence authority |
| `supported_with_transform` | typed mapping exists and reports the transformation |
| `preserved_opaque` | bytes/value retained with scope but not natively understood |
| `fallback_only` | safe static/native fallback can render; semantic payload remains unknown |
| `unavailable_provider` | architecture supports a provider class but the qualified provider is not installed/available |
| `unsupported` | this implementation/profile has no defined operation |
| `blocked_required_extension` | required unknown semantics intersect the requested action |
| `stale_revision` | expected revision is no longer current |
| `divergent_heads` | same branch has unresolved independently committed heads |
| `invalid` | validation/integrity failure removes write authority |

`unavailable_provider` is not `unsupported`, and neither means native degradation. User authorization/permission is a separate system concern and not encoded here.

## 2. Authority-mode matrix

| Question | Native DND | Foreign-managed DOCX | Converted/imported DOCX |
| --- | --- | --- | --- |
| editable semantic authority | DND | DOCX/provider | DND |
| semantic IDs | intrinsic family/branch/object/boundary IDs | external DOCSeye correspondence + provider evidence | intrinsic native IDs; provider IDs evidence only |
| revision precondition | native `revision_id`/root | coherent provider/representation/physical clocks | native `revision_id`/root |
| atomic transaction | native SQLite semantic commit | old DOCX-first provider transaction/patch contract | native SQLite semantic commit |
| retained text anchors | native boundary objects | provider markers + correspondence/history | native boundary objects; imported markers may be facets |
| unknown semantics | extension envelope/coverage | preserve OOXML/MC/package content | scoped provider facets/capsule plus native extensions |
| source capsule | optional/not applicable | artifact itself is source authority | optional immutable evidence; default when round trip requested |
| renderer dependence | none for semantics | provider-specific | none for semantics |
| Word dependence | none | capability/profile dependent | none for native semantics; optional Microsoft observation |

Mode transition from foreign-managed to converted is explicit. There is no automatic transition back and no dual-authority state.

## 3. Native semantic capability

| Capability | Build 001 intent | Authority/notes |
| --- | --- | --- |
| root/flows/sections/blocks | `supported_native` | public identity for targetable objects |
| paragraphs/headings | `supported_native` | heading is semantic role, not font/style inference |
| text query by offsets | `supported_native` | revision-local only |
| retained ranges/boundaries | `supported_native` | explicit mutation; portable stable identity |
| lists/items/restart | `supported_native` | membership distinct from marker display |
| tables/rows/columns/cells/regions | `supported_native` | coordinates are locations; merge/split remint affected cells |
| styles/themes/direct overrides | `supported_native` | deterministic six-layer resolution |
| comments/threads/replies | `supported_native` | exact targets; orphan state |
| six suggestion types | `supported_native` | current content, not revision ledger |
| bounded fields | `supported_native` | explicit evaluation policy/staleness |
| eight control types | `supported_native` | external bindings inert/typed |
| notes/anchors/references/citations | `supported_native` | exact ID/boundary targeting |
| figures/assets/SVG | `supported_native` | figure occurrence separate from SHA-256 asset |
| MathML + TeX facet | `supported_native` after E-20 mechanism | OMML stays provider facet |
| generic drawing program | `unsupported` | SVG asset or provider facet instead |
| deep chart computation | `unsupported` | DATAeye-owned |
| extensions | `supported_native` under envelope | unknown disjoint preserved; unsafe intersection blocked/invalidate optional |
| active code/macros/OLE execution | `unsupported` | inert preservation only |
| live collaboration/CRDT sync | `unsupported` in Build 001 | future requirement only |

## 4. Identity and transaction capability

| Capability | Build 001 intent | Required outcome |
| --- | --- | --- |
| save/move/rename | `supported_native` | family/branch/object identity stable |
| controlled replica | `supported_native` | same branch snapshot until divergence |
| raw-copy divergence | `supported_native` | `divergent_heads`, no write until explicit resolution |
| Save As / explicit fork | `supported_native` | same family, new branch, full public-ID/boundary remint |
| independent duplicate/template | `supported_native` | new family/branch/full remint |
| within-branch copy | `supported_native` | copied subgraph reminted; immutable asset may share |
| cross-branch/family paste | `supported_native` | incoming occurrence IDs reminted |
| split/merge | `supported_native` | all affected semantic objects reminted; exact bounded lineage |
| stale write | `stale_revision` | zero mutation |
| ambiguous target | `unsupported` for write | explicit ambiguous/non-write result |
| multi-operation transaction | `supported_native` | one old-or-new commit/revision/root/delta |
| post-commit response loss | `supported_native` | bounded idempotency outcome query; no blind replay |
| bounded delta | `supported_native` | exact; gap/expiry returns resync |
| artifact-only recovery | `supported_native` | identity/semantics exact after runtime deletion |

## 5. Layout, HTML, and PDF capability

| Capability | Provider | Build 001 requirement |
| --- | --- | --- |
| layout intent | DND | semantic authority for page/media/column/break/keep/header/footer/table/note/language intent |
| paginated realization | Typst candidate | qualified LayoutRevision, warnings, source-region mapping; E-12 gate |
| continuous accessible view | HTML + pinned Chromium | semantic HTML, source attributes, accessibility mapping; DOM non-authoritative |
| web export | HTML | derived output with revision/root attribution |
| paginated PDF | Typst candidate | tagged PDF/UA-1, independently validated, exact source/layout attribution |
| browser PDF | Chromium | optional comparison; not native oracle |
| Microsoft layout/PDF | Word | `unavailable_provider` on STEALTHEYELLC; optional future observation |
| pages/regions | layout provider | LayoutRevision-scoped, never semantic identity |
| vertical writing beyond tested fixture | provider capability | honestly `unsupported`/reported unless E-12 proves support |

Native capability remains `supported_native` when Word is `unavailable_provider`.

## 6. DOCX capability

| Capability | Build 001 boundary | Outcome/evidence |
| --- | --- | --- |
| unconverted DOCX inspection/edit | historical foreign-managed mode | `supported_foreign_managed` under DOCX-first baseline; outside replacement native A–D |
| F-001 import | required X-01 | native concepts + scoped facets/opaque content + exact capsule/report |
| untouched source reuse | required X-02 | `exact_source_reuse` + package/schema evidence as run |
| bounded preserved patch | required X-02 | `preserved_patch` with untouched preservation report |
| translated export | required X-02/X-04 | `translated_conformant` or `translated_with_declared_loss` |
| unknown required intersection | required X-03 | `blocked` unless a declared safe transform applies |
| OPC/package validation | required X-04 | `package_validated` |
| OOXML schema validation | required X-04 | `schema_validated` |
| LibreOffice open/save smoke | required X-04 | `alternate_provider_observed`; exact profile recorded |
| Word open/save/layout | not Build 001 | `unavailable_provider`, later conditional `microsoft_observed` only |
| broad DOCX feature coverage | `unsupported` in Build 001 | later provider expansion, never implied by F-001 |

Canonical semantic export outcomes are exactly: `exact_source_reuse`, `preserved_patch`, `translated_conformant`, `translated_with_declared_loss`, `unsupported`, and `blocked`.

## 7. ODF and other providers

| Provider | Build 001 | Architectural status |
| --- | --- | --- |
| ODF 1.4 | `unsupported` | peer future import/export provider |
| LibreOffice as ODF renderer | not required | possible future provider |
| LibreOffice as DOCX observer | required X-04 | alternate-provider evidence only |
| Microsoft Word | not required/present | optional future Microsoft oracle/provider |
| PDF input | not Build 001 | future fixed-layout provider, not native reflow model |
| DocLang | not authority/provider in Build 001 | possible future AI-facing derived representation after scoped decision |

## 8. Capability-report minimum fields

Every operation/report includes:

- mode and semantic authority;
- family/branch/revision/root;
- operation/object/coverage scope;
- capability state and reason;
- required provider and provider availability;
- transformation/loss/fallback plan;
- extension/provider-facet effects;
- source-capsule alignment where relevant;
- layout/render/provider environment and warnings where relevant;
- semantic translation outcome and separate observation evidence for export;
- no Boolean “high fidelity” field.

## 9. Status truth

Every item above is a frozen intended capability or non-capability. Build 001 has not started, so no row may be read as a passing implementation result.
