# 10 - Build 002 Freeze

Status: **BUILD 002 CANDIDATE ARCHITECTURE - PROSPECTIVELY FROZEN**  
**NOT YET IMPLEMENTED**  
**NOT YET MEASURED**  
**NOT YET ACCEPTED**

Build title: **DOCSeye Build 002 - ODF 1.4 / LibreOffice Writer Provider-Neutrality Pressure Test**

## 1. Freeze identity

- repository: `StealthEyeLLC/DOCSeye`
- base branch: `main`
- base commit: `2ff0bde790ee35720edc54589f8651b0393734ff`
- base tree: `879d512b411ff7b4ac130579692c8e0facc30b9d`
- development branch: `build/build002-odf-provider-neutrality`
- freeze date: `2026-08-12`
- machine: `STEALTHEYELLC`
- operator: ChatGPT through the existing Eye/GitHub authorities
- Build 001: **IMPLEMENTED / ACCEPTED / MEASURED / CANONICAL**
- architecture family: **NATIVE SEMANTIC AUTHORITY WITH NON-AUTHORITATIVE PROVIDER FACETS**
- architecture amendments authorized by this freeze: **NONE**
- Build 003 authorization: **NONE**

Provider truth was reverified before this freeze. `main` had not advanced, only `main` existed before this branch was created, and the current canonical docs namespace ended at `docs/09-BUILD-001-RESULTS.md`.

The exact accepted Build 001 reproduction command was run before this freeze. A first attempt exposed only an environment prerequisite gap: the accepted SHELLeye carrier-proof pipe `\\.\pipe\shelleye-dev` was not running after the current reboot/login state. No DOCSeye source or provider drift was involved. After the existing accepted SHELLeye kernel runtime was started, the full Build 001 reproduction passed: E-01..E-20, A, B, native C 32/32, DOCX X 4/4, D 96/96, scale, comparative benchmark, zero-warning/zero-error build, and Word-zero. The repository was restored to the exact clean base tree before branch creation.

## 2. Goal and authority proposition

Build 002 proves or falsifies this proposition:

> One exact DND-native authored document can cross a materially different ODF 1.4 provider boundary, remain semantically exact where claimed, preserve or explicitly classify unsupported and unknown meaning, be observed and normalized by a real stable LibreOffice Writer provider without authority inversion, survive provider/runtime failure without false correspondence, generate multiple qualified representations from one exact native revision, retain all accepted Build 001 capability, and execute a dense cross-provider workflow without model micromanagement.

DND remains the sole editable semantic authority in native mode. ODT bytes, ODF XML, LibreOffice document objects, UNO proxies, Writer page numbers, bookmarks, reference marks, XML IDs, ODF change IDs, LibreOffice redline identifiers, paths, ZIP member names, XPaths, positions, text similarity, and provider process/session identity are never native semantic identity.

Foreign-managed editable ODT is excluded. Foreign ODT may be inspected read-only and explicitly converted/imported. Conversion mints native DND identity and makes DND sole editable authority; the original ODT may remain immutable source-capsule/provider evidence. Existing foreign-managed DOCX behavior is unchanged.

## 3. Frozen scope

Required provider/format scope:

- ODT text documents;
- ODF 1.4 standard-conforming documents;
- controlled ODF 1.4 Extended documents;
- FODT only as a deterministic debugging/test representation where useful;
- earlier ODF versions recognized/classified when encountered, without universal historical compatibility.

Explicitly excluded:

- ODS/Calc semantics;
- ODP/Impress semantics;
- general ODG/Draw semantics beyond document-contained figure occurrences;
- a universal ODF AST;
- a universal office ontology;
- foreign-managed editable ODT;
- Microsoft Word dependency;
- a permanent LibreOffice daemon;
- a second Windows SCM service;
- a verification agent or embedded model.

## 4. Frozen provider stack

Conceptual stack:

`DND -> direct ODF package/XML adapter -> ODF 1.4 conformance layer -> pinned independent validator -> stable LibreOffice Writer -> UNO semantic observation -> separate provider resave/render -> normalization/layout differential`

LibreOffice role: **OBSERVATIONAL / TRANSFORMATION / LAYOUT PROVIDER**.  
ODF role: **STANDARDIZED FOREIGN PROVIDER REPRESENTATION**.  
Neither is native authority.

Current stable provider was reverified from official The Document Foundation sources on 2026-08-12. The selected measured-baseline provider is the already installed stable 26.2.5 build:

- product: LibreOffice
- version: `26.2.5.2`
- build ID: `cd7284b4cbbfeb507e630c1aac019f4157393acb`
- architecture: Windows 64-bit
- channel: stable 26.2 line / 26.2.5 current release
- launcher: `C:\Program Files\LibreOffice\program\soffice.exe`
- launcher SHA-256: `40ebd75ff8c2d4a228166ffa9ac13b97b947f7ddef67152ae45cf4c98c1ae5e5`
- process binary: `C:\Program Files\LibreOffice\program\soffice.bin`
- process binary SHA-256: `30078f5fdfad18195e4333561d2440bf8a05af9e7a8aaf2cdf76776c737335de`
- CLI shim SHA-256: `fe41a4eb77ba51610f10bcbba2d849fbaec9f63a1fcbeda5f32d629bb8c49316`
- bundled Python SHA-256: `62aab98528514ce16def0b896fe9b756bd8f2589e641f2da204380fc08e75297`
- `pyuno.pyd` SHA-256: `b47723bf333b93f949a0dce1fabf2dc2f31a189e851a265c1ef5cb44b96b9ac1`

Every measured provider run must use an isolated disposable `UserInstallation`, no owner profile, no unrelated extensions, active-content suppression, no automatic external link/network update, a unique local UNO endpoint, and an on-demand provider process. Headless mode is the semantic-provider default. Interactive desktop state is not a core acceptance dependency.

## 5. ODF standards and validation contract

Normative ODF authority is OASIS OpenDocument 1.4. Before acceptance freeze the locally cached acceptance inputs must include and digest at minimum:

- `OpenDocument-v1.4-schema.rng`
- `OpenDocument-v1.4-manifest-schema.rng`
- `OpenDocument-v1.4-dsig-schema.rng`
- ODF 1.4 metadata/package-metadata OWL resources used by the implementation

Resources must come from the OASIS 1.4 OS resource set or an explicitly stronger later OASIS publication of the same standard and must be pinned locally before measured acceptance.

Validation layers are separate and non-substitutable:

1. DOCSeye secure ODT package preflight;
2. normative OASIS machine-readable schema validation;
3. a pinned independent ODF validator established during implementation and bound by digest in the acceptance freeze;
4. LibreOffice application observation;
5. DOCSeye semantic expected postconditions.

LibreOffice opening a file proves application compatibility only. Schema validity does not prove semantic fidelity. Semantic fidelity does not imply layout identity. Layout similarity does not prove semantic preservation.

## 6. Package, import, export, and security invariants

ODT is a ZIP package with ODF-specific constraints, not an arbitrary ZIP. Secure preflight must validate `mimetype`, package members, manifest relationships, path/name safety, duplicate-dangerous entries, bounded entry count, decompressed size/compression ratio, XML depth/count/text/metadata limits, and streaming large resources. XML external entity/DTD resolution is disabled. Malformed source input is preserved as failure evidence and is never silently repaired during import preflight.

ODF version/profile classification must distinguish at least `1.0`, `1.1`, `1.2`, `1.3`, `1.4`, `extended`, and `unknown/malformed`; Build 002 targets ODF 1.4.

Import is an explicit authority transition. Original source bytes/digest/length/package inventory/version/profile/validation/generator metadata are captured before LibreOffice opens the source. Meaningful features are classified as one of: `native_mapped`, `provider_facet`, `unknown_preserved`, `required_unsupported`, `unsupported`, or `blocked`. Optional unknown content may be preserved inertly. Fixture-required unknown meaning blocks an intersecting conversion/edit when it cannot be safely preserved. Macros/scripts, linked images/sections, and embedded objects never execute or fetch automatically.

The six accepted export semantic outcomes remain exactly:

- `exact_source_reuse`
- `preserved_patch`
- `translated_conformant`
- `translated_with_declared_loss`
- `unsupported`
- `blocked`

ODF evidence is orthogonal: target version/profile, producer path, validator result, provider profile, semantic fidelity, layout fidelity, and unknown preservation. Direct DOCSeye ODT and LibreOffice-resaved ODT are always separate artifacts. Byte equality across implementations is never required. `exact_source_reuse` must actually be byte exact. `preserved_patch` is claimed only for a narrowly proven class; otherwise deterministic translation is used.

Opening/parsing/rendering executes zero active content and performs zero unintended external fetches. Test controls must be process/profile scoped where possible; global network/firewall changes are not the default.

## 7. Semantic pressure areas

Build 002 must pressure the existing ontology without importing provider ontology:

- exact Unicode text semantics including whitespace representation differences;
- flows/subflows: body, headers, footers, frames, cells, footnotes, endnotes and other Writer text containers;
- sections, nested/restarted lists, authored tables/merges/repeated headers/nested content;
- links, bookmarks, reference marks, cross-references, retained DND boundaries;
- notes, bounded fields, controls, figures/assets and native Presentation MathML;
- named styles, automatic styles, defaults, direct formatting and page/master styles without minting a native object for every provider automatic style;
- comments and range/point anchoring under duplicate text and edits;
- tracked insertion/deletion and bounded format-change mapping; DND move may translate only as explicit composition/declared loss when no exact ODF peer exists;
- ODF metadata and provider-scoped RDF without creating a universal DOCSeye graph;
- qualified foreign namespaces/extensions, preserving optional unknown material and blocking required unsafe intersections.

LibreOffice modification events are dirty/reconciliation signals, not semantic deltas. Unsaved Writer state is explicit and never silently becomes DND truth.

## 8. O-001 flagship fixture

Implementation must create deterministic `O-001`, preferably as a hostile ODF 1.4 Extended Writer-domain ODT plus a standard-only control where useful. Generator reproducibility is required wherever byte determinism is claimed.

O-001 includes at minimum:

- multiple sections, body, heading levels, ordinary paragraphs, feasible multi-column pressure, header/footer variants;
- duplicate headings/paragraphs/copied-looking structures and identical visible text around distinct anchors;
- Latin, RTL Arabic/Hebrew, CJK, Devanagari, combining and normalization-sensitive sequences, emoji and ZWJ;
- named paragraph/character/inherited/automatic/direct formatting/page/master/list styles;
- nested/restarted/continued lists with numbering/indentation pressure;
- merged cells, repeated header row, nested authored content, lists/notes/references in cells, border/width differences and a bounded Writer-table formula/provider field;
- point/range bookmark, reference mark, internal/broken cross-reference and duplicate surrounding text;
- point/range annotations, reply/thread pressure where supported, resolved-state provider extension and comment-on-change pressure;
- tracked insert/delete/format/table/header-footer change and a DND-native move requiring composition/declared loss;
- approved page/date-time/document-property/sequence/cross-reference fields plus one deliberately provider-qualified dynamic field;
- current Writer content-control representations where practical, with standard/extension classification;
- footnote/endnote/note reference;
- raster, SVG, alt description, crop/scale, anchor, frame/text box and inert linked resource;
- deterministic Presentation MathML;
- ordinary/custom/provider/RDF/in-content metadata as useful;
- one optional unknown extension that must survive;
- one fixture-required unknown extension that must block an unsafe intersecting conversion/edit;
- inert active-content and external-resource probes proving zero execution/fetch.

## 9. Frozen Build 002 hostile suite

Cases are frozen before implementation-specific acceptance fixes. B2-O-47 is informational/non-gating; every other case is gating. Gating score is **63/63** and may not be downgraded after a failure.

| ID | Frozen case |
| --- | --- |
| B2-O-01 | Classify source ODF version/profile before provider open. |
| B2-O-02 | Duplicate paragraph does not rebound identity. |
| B2-O-03 | Duplicate heading does not rebound identity. |
| B2-O-04 | Point bookmark correspondence. |
| B2-O-05 | Range bookmark correspondence. |
| B2-O-06 | Reference-mark correspondence. |
| B2-O-07 | Copied paragraph does not inherit native identity. |
| B2-O-08 | Copied section does not inherit native identity. |
| B2-O-09 | Provider Save As does not imply native branch continuity. |
| B2-O-10 | Physical file copy does not imply native family continuity. |
| B2-O-11 | External package mutation invalidates stale correspondence conservatively. |
| B2-O-12 | Reorder preserves only proven correspondence. |
| B2-O-13 | Split/merge never uses text similarity. |
| B2-O-14 | Paragraph lacking strong provider ID remains safely classified. |
| B2-O-15 | Duplicate/malformed provider IDs never become writable native identity. |
| B2-O-16 | Tracked insertion mapping. |
| B2-O-17 | Tracked deletion mapping. |
| B2-O-18 | Format-change limitation reported honestly. |
| B2-O-19 | DND replace uses explicit composition if necessary. |
| B2-O-20 | DND move never falsely claims an exact ODF peer. |
| B2-O-21 | Overlapping/nested review pressure. |
| B2-O-22 | Tracked changes inside authored table cells. |
| B2-O-23 | Tracked changes in header/footer scope. |
| B2-O-24 | LibreOffice redline identifier lifetime measured without native promotion. |
| B2-O-25 | Point comment. |
| B2-O-26 | Range comment. |
| B2-O-27 | Comment anchor movement. |
| B2-O-28 | Duplicate quotation does not attract comment. |
| B2-O-29 | Resolved-comment extension classified. |
| B2-O-30 | Missing exact thread/reply peer is reported rather than fabricated. |
| B2-O-31 | Normative Standard ODF 1.4 validation. |
| B2-O-32 | Controlled Extended ODF validation/classification. |
| B2-O-33 | Optional unknown content preserved. |
| B2-O-34 | Fixture-required unknown content blocks unsafe intersection. |
| B2-O-35 | Content-control extension classified. |
| B2-O-36 | Opaque embedded material remains inert/preserved or produces explicit loss. |
| B2-O-37 | Macro/script content never executes. |
| B2-O-38 | Automatic external network/link activity remains zero. |
| B2-O-39 | No-intentional-edit standard-profile LibreOffice resave normalization classified. |
| B2-O-40 | Extended-profile resave normalization classified. |
| B2-O-41 | Direct DOCSeye ODT versus LibreOffice-resaved ODT differential. |
| B2-O-42 | `exact_source_reuse` is byte exact when eligible. |
| B2-O-43 | `preserved_patch` claimed only inside an explicitly proven safe patch class. |
| B2-O-44 | Validator disagreement remains qualified evidence. |
| B2-O-45 | Current stable LibreOffice provider profile is pinned. |
| B2-O-46 | Prerelease provider cannot satisfy stable acceptance gate. |
| B2-O-47 | Narrow third-provider comparison, informational/non-gating unless an already available reproducible provider costs essentially zero complexity. |
| B2-O-48 | Package normalization separated from semantic change. |
| B2-O-49 | Writer layout qualified by provider/fonts/profile. |
| B2-O-50 | First/left/right header/footer/page-style pressure. |
| B2-O-51 | RTL/CJK/Indic/combining/ZWJ pressure. |
| B2-O-52 | Accessibility semantic structure preservation. |
| B2-O-53 | LibreOffice PDF/UA output independently validated and correctly attributed. |
| B2-O-54 | DOCSeye kernel restart reconstructs provider correspondence without false identity. |
| B2-O-55 | LibreOffice restart creates a new provider manifestation. |
| B2-O-56 | Isolated LibreOffice profile prevents provider-state contamination. |
| B2-O-57 | Unsaved provider state remains explicit and non-native. |
| B2-O-58 | Provider crash during export leaves native DND intact. |
| B2-O-59 | Response loss after provider save is reconciled rather than blindly replayed. |
| B2-O-60 | Moved/renamed ODT carrier does not alter authored identity rules. |
| B2-O-61 | Stale DND expected revision refuses. |
| B2-O-62 | Security/resource hostile corpus. |
| B2-O-63 | Scale regression: 10/500/5,000-page-equivalent, 1 GiB asset, and ODF-specific scale pressure. |
| B2-O-64 | Flagship Build 002 Program Host workflow plus complete Build 001 regression. |

## 10. Frozen flagship Program Host workflow

The flagship is one Program Host invocation, zero embedded model, zero model turns between primitives, and **78 exact typed operations**. The count is the result of the concrete sequence below, not a target chosen independently of the work.

1. `provider.libreoffice.profile`
2. `odf.inspect_source`
3. `odf.classify_version_profile`
4. `odf.package_preflight`
5. `odf.validate_normative`
6. `odf.validate_independent`
7. `odf.import`
8. `odf.source_capsule.inspect`
9. `odf.feature_classification`
10. `native.query.root`
11. `native.query.duplicate_paragraphs`
12. `native.query.duplicate_headings`
13. `native.query.bookmarks`
14. `native.query.reference_marks`
15. `native.query.comments`
16. `native.query.suggestions`
17. `native.query.styles`
18. `native.query.tables`
19. `native.query.notes`
20. `native.query.fields_controls`
21. `native.tx.begin`
22. `native.mutate.text_insert`
23. `native.mutate.text_delete`
24. `native.mutate.paragraph_move`
25. `native.mutate.list_restart`
26. `native.mutate.table_cell_text`
27. `native.mutate.style_property`
28. `native.mutate.comment_add`
29. `native.mutate.comment_resolve`
30. `native.mutate.suggestion_insert`
31. `native.mutate.field_update`
32. `native.mutate.control_update`
33. `native.mutate.figure_alt`
34. `native.tx.commit`
35. `native.delta.consume`
36. `native.tx.begin`
37. `native.mutate.text_replace`
38. `native.mutate.paragraph_copy`
39. `native.mutate.section_reorder`
40. `native.mutate.list_continue`
41. `native.mutate.table_header`
42. `native.mutate.direct_format`
43. `native.mutate.comment_reply`
44. `native.mutate.suggestion_delete`
45. `native.mutate.note_text`
46. `native.mutate.cross_reference`
47. `native.mutate.math_facet`
48. `native.mutate.layout_intent`
49. `native.tx.commit`
50. `native.delta.consume`
51. `native.query.revision_postconditions`
52. `native.refusal.required_unknown_intersection`
53. `native.refusal.stale_revision`
54. `native.delta.consume`
55. `odf.export_direct`
56. `odf.package_preflight_export`
57. `odf.validate_normative_export`
58. `odf.validate_independent_export`
59. `provider.libreoffice.start`
60. `provider.libreoffice.connect`
61. `provider.libreoffice.load`
62. `provider.libreoffice.observe_core`
63. `provider.libreoffice.observe_subflows`
64. `provider.libreoffice.observe_review`
65. `provider.libreoffice.observe_styles_fields`
66. `provider.libreoffice.resave`
67. `odf.validate_resave`
68. `provider.diff`
69. `provider.libreoffice.render_pdf`
70. `provider.libreoffice.close`
71. `provider.libreoffice.stop`
72. `render.html`
73. `render.typst_pdf`
74. `native.delta.consume`
75. `native.kernel_restart_recover`
76. `provider.libreoffice.restart_reobserve`
77. `security.audit_active_external_word_zero`
78. `workflow.compact_result`

This sequence contains 24 native semantic mutations, 16+ semantic mutation families, two atomic commits, four delta consumptions, direct ODT import/export/validation, real LibreOffice observation/resave/differential/render, independent HTML and Typst/PDF representations, recovery checks, and zero intermediate model calls. No operation may be skipped and counted as success.

## 11. Build 001 regression floor

Build 002 cannot be accepted if Build 001 regresses. Final measured acceptance must rerun the exact accepted Build 001 reproduction, including:

- E-01 through E-20;
- native hostile suite 32/32;
- DOCX supplement 4/4;
- exact Build 001 Program Host 96/96;
- HTML/PDF accessibility and Typst behavior;
- 10/500/5,000 page-equivalent tiers;
- 1 GiB streamed asset path;
- Microsoft Word participation/dependency: zero.

## 12. Hard-zero correctness metrics

Every final result must record an explicit integer for each:

- false native rebounds = 0
- provider ID incorrectly promoted to native identity = 0
- wrong-object native writes = 0
- stale-revision accepted writes = 0
- cross-branch writes = 0
- silent fixture-required unknown loss = 0
- silent optional unknown drop outside an explicit invalidate/report policy = 0
- unreported semantic translation loss = 0
- provider result attributed to wrong DND revision = 0
- provider result attributed to wrong provider profile = 0
- provider process/session treated as native authority = 0
- active-content execution = 0
- unintended external network fetch = 0
- Microsoft Word calls/dependencies = 0
- wrong comment/suggestion rebound = 0

Any nonzero value is a gating acceptance failure.

## 13. Correctness, scale, and evidence discipline

Correctness thresholds are frozen; speculative speed thresholds are not. Measure absolute DND, ODF package, validation, LibreOffice startup/load/UNO traversal/save/render, memory/working-set, package-member touch, streamed bytes and model-facing bytes separately. Add deterministic ODF pressure for automatic styles, review changes, comments, deep lists/tables, frames and metadata/RDF.

The final implementation checkpoint must contain all execution-affecting code, pass clean release build/tests/fixture reproducibility/JS syntax checks/`git diff --check`, and have a clean worktree. Measured acceptance is built from the exact provider-published candidate. No untracked runtime dependency may be required without being recorded and pinned.

Before the first measured Build 002 acceptance action, create `docs/11-BUILD-002-ACCEPTANCE-FREEZE.md` binding the exact implementation commit/tree, O-001 digests, schemas, independent validator/runtime, LibreOffice binary/profile, acceptance commands, 63 gating cases plus B2-O-47 status, the exact 78-operation workflow, hard-zero metrics and Build 001 regression procedure. Prefer that commit to change only docs/11.

After acceptance begins, candidate code cannot change under the same freeze. Any execution-affecting defect fails that candidate attempt; repair requires a new implementation commit, new acceptance freeze and new durable measured run. Every failed run is preserved.

Only after a measured run exists may `docs/12-BUILD-002-RESULTS.md` be created. It must report every B2-O case individually, provider normalization, semantic-loss classifications, performance/scale, hard zeros, Program Host result, Build 001 regression, failures/limitations and exact reproduction commands.

## 14. Frozen architecture falsifiers

| ID | Falsifier |
| --- | --- |
| F2-01 | ODF exercises materially no new provider pressure beyond DOCX. |
| F2-02 | Useful capability is genuinely only a tiny supplement; do not inflate architecture. |
| F2-03 | UNO is too weak/unstable to contribute meaningful semantic evidence; reduce provider pressure rather than invent authority. |
| F2-04 | Foreign-managed ODT complexity is required; do not implement it in Build 002. |
| F2-05 | Meaningful ODF support requires Calc/Impress/Draw/general ODF-AST ontology expansion. |
| F2-06 | Exact preservation is impossible for a feature; classify declared loss rather than invent fidelity. |
| F2-07 | An abstraction exists only for cosmetic DOCX/ODF symmetry. |
| F2-08 | Final ODF acceptance requires Microsoft Word. |
| F2-09 | ODF support leaks sibling ontology/authority. |
| F2-10 | Provider complexity destroys Program Host advantage without compensating capability/correctness. |
| F2-11 | Writer/ODT state silently overrides current DND meaning. |
| F2-12 | A provider identifier causes a wrong native target. |

If an accepted native architecture rule is actually falsified, implementation stops the affected path and records **ARCHITECTURE_FALSIFIER**. Ordinary package/XML/UNO/test/normalization defects are implementation defects and may be repaired without reopening architecture.

## 15. Promotion rule

Allowed final measured classifications are exactly:

- `COMPLETE / ACCEPTED`
- `IMPLEMENTED / ACCEPTANCE FAILED`
- `IMPLEMENTED / ACCEPTANCE BLOCKED`
- `ARCHITECTURE FALSIFIED`

Only `COMPLETE / ACCEPTED` authorizes canonical status updates and promotion to `main`. Promotion preserves Build 001 history and prospective/measured evidence lineage; no squash, rewrite or force-push is authorized.
