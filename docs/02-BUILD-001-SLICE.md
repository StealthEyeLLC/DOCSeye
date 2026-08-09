# 02 — Build 001 Slice

Status: **PLANNED / NOT IMPLEMENTED / ACCEPTANCE NOT RUN**

This file is the complete replacement Build 001 contract. It contains no result and authorizes no implementation in the architecture-freeze pass.

## 1. Thesis

> Prove that one portable native DOCSeye artifact carries authoritative semantic identities, family/branch/revision state, retained anchors, extension-preservation contracts, assets, and layout intent; survives total kernel and rebuildable-runtime loss, coherent movement, independent valid native edits, and replica divergence; executes exact revision-conditioned atomic transactions with compact gap-aware semantic deltas; renders attributable accessible HTML and paginated PDF with no Microsoft Word dependency; and crosses one bounded DOCX boundary without false fidelity claims.

## 2. Exact acceptance inventory

| Item | Frozen count |
| --- | ---: |
| implementation experiments | 20 |
| native hostile cases | 32 |
| DOCX-provider supplemental cases | 4 |
| Program Host calls | 96 |
| Program Host semantic mutation calls | 48 |
| mutation families | 16 |
| Program Host atomic transactions/commits | 3 |
| delta-consumption calls | 6 |
| material render calls | 2 |
| material DOCX export calls | 1 |
| refusal/recovery branches in D | 3 |
| Program Host invocations | 1 |
| intermediate model calls | 0 |

Native C and provider X are scored separately. All 32 native cases and all 4 provider cases are required for overall Build 001 acceptance, but no provider result can offset a native failure.

## 3. Deterministic fixtures

No fixture exists at freeze time. Implementation creates the following from a seed recorded in the results.

### 3.1 N-001 rich native fixture

N-001 is a deterministic ten-page-equivalent authored document containing exactly:

- one document root; one main flow with three sections; one sidebar flow; one reusable header flow and one footer flow;
- five headings across levels 1–3 and eighteen body paragraphs, including three byte-identical sentences in different objects and a repeated phrase in two paragraphs;
- six retained ranges: two overlapping, two with co-located but independently identified endpoints, one named point anchor, and one cross-block multi-interval range;
- two links; three citation occurrences over two bibliographic records; two named anchors and three cross-references;
- one two-level list with four items, one multi-block item, and explicit restart/start behavior;
- one 4×4 authored table with stable rows/columns/cells, two visually identical rows, one 2×2 merged region, semantic row/column headers, and one nested 2×2 table;
- four semantic roles, three named styles, six theme/design tokens, three direct overrides, and two layout profiles (US Letter and A4);
- two comment threads: one open root comment with two replies and one resolved root comment with one reply;
- six active suggestions: insert, delete, replace, move, style/property, and structural/table;
- six fields: metadata, fixed date, cross-reference, TOC, list of figures, and page count;
- eight controls: text, rich text, number, boolean, date, enum, repeating group, and object/reference selector;
- one footnote and one endnote, each with a body flow and exact reference occurrence;
- three figure occurrences over two embedded assets: two figures share one raster digest and one uses SVG; each has caption, alt text, and placement/wrap intent;
- one inline and one display math object with native Presentation MathML; the display object also has a TeX source facet;
- five extension instances: known optional object coverage, unknown optional disjoint property coverage, unknown required text-interval/must-understand coverage, unknown topology/generic-transform coverage, and a newer-minor optional extension with static fallback;
- exact text fixtures for English/OpenType features, Arabic RTL and mixed bidi, Japanese CJK, Devanagari, decomposed combining characters, emoji/ZWJ, and font fallback;
- two-column pressure, explicit break, keep-with-next/together, widow/orphan intent, section headers/footers/page numbering, figure wrap, table repeat/split intent, note placement, language/hyphenation, and enough deterministic reflow pressure to cross page/table/note boundaries;
- no native executable content and no automatically fetched remote asset.

Every object needed by A–D has a fixture manifest entry with expected type, parent/order, semantic role, and whether it requires render correspondence. The generator records all bytes/digests and can reproduce the same logical semantic root under each conforming implementation.

### 3.2 F-001 adversarial foreign DOCX fixture

F-001 is one deterministic OPC package containing:

- headings and named/direct styles;
- nested and restarted numbering;
- merged and nested tables;
- comments with replies;
- tracked insert, delete, move, and style change;
- metadata/cross-reference/page fields;
- text and enum content controls;
- bookmarks and internal references;
- section header/footer and page numbering;
- footnote and endnote;
- figure/media and OMML;
- custom XML;
- `AlternateContent` and unknown markup-compatibility content;
- one opaque custom part with a relationship from a supported part;
- one inert macro/OLE-style opaque payload that must never execute.

The source package is independently assembled and hashed. It is not generated by Microsoft Word for native acceptance. Build 001 imports it with an embedded exact source capsule.

## 4. Implementation stack and preflight

Architecture-level stack:

- .NET 10 LTS/C# kernel;
- Node 24 LTS Program Host, using the measured installed executable by absolute path;
- SQLite application file in rollback-journal `DELETE` mode;
- deterministic CBOR logical records and SHA-256 roots/assets;
- external SQLite WAL/FTS runtime indexes;
- Typst paginated provider;
- HTML plus Chrome for Testing/Chromium provider;
- Open XML SDK plus raw OPC/XML provider;
- LibreOffice alternate-provider smoke;
- SHELLeye coherent snapshot/publication integration.

Before source work, implementation must record the latest stable supported patch of every provisioned dependency, provenance, package/binary digest, license, fonts, locale, hyphenation data, and renderer configuration. Select supported stable patches at implementation start; do not inherit incidental report versions. Pin the accepted environment after E-02, E-06, E-12, E-13, and E-16 establish suitability.

As of the freeze evidence, SQLite 3.53.4, Typst 0.15.1, Open XML SDK 3.5.1, .NET 10, and Node 24 LTS are current reference points, not mandatory dependency pins. No dependency is installed by this synthesis.

## 5. Numbered implementation experiments

Experiments answer bounded questions. They do not authorize changing a frozen invariant silently.

| ID | Binary or bounded question | Passing evidence | Failure action |
| --- | --- | --- | --- |
| E-01 | Can a non-CRDT canonical text structure preserve the frozen retained-boundary/affinity/delete/split/merge semantics with bounded current witnesses? | deterministic model/property tests, cold restart, external writer, and anchor killer all exact | **ARCHITECTURE FALSIFIER**: test a bounded CRDT-like mechanism; if permanent character history is required, reopen native text architecture before other features |
| E-02 | Does selected SQLite rollback-journal operation provide old-or-new semantic revision/root across process kill, power-loss simulation, disk full, and post-commit response loss? | every cut recovers exactly old or new; no acknowledged partial; idempotency query exact | **ARCHITECTURE FALSIFIER**: change journal/container mechanism; if no application-file profile passes, reopen native container |
| E-03 | Can a quiescent DND be published/copied as one coherent file while live/sync copies are detected or coordinated? | backup/snapshot/SHELLeye procedures never accept main-file-only incoherence; raw replica semantics exact | mechanism change; persistent false continuity is an architecture falsifier |
| E-04 | Can unknown extensions declare enough machine-readable coverage and transformation behavior to allow disjoint edits while refusing every unsafe intersection? | object/property/subtree/range/topology/global vectors preserve exact payloads or refuse | **ARCHITECTURE FALSIFIER**: reopen extension envelope or native-first if unknown semantics cannot be bounded |
| E-05 | Do 128-bit sparse order keys meet deterministic/local mutation and rebalance neutrality through 5,000-page-equivalent pressure? | moves preserve IDs; local inserts avoid unrelated churn; rebalance keeps root | select chunked/B-tree sequence mechanism inside frozen order invariants |
| E-06 | Can C# and an independent second-language implementation emit byte-identical DOCSeye deterministic-CBOR vectors? | 100% byte identity on valid vectors and identical invalid classifications | **ARCHITECTURE FALSIFIER** if public canonicalization cannot be implemented independently |
| E-07 | Which domain-separated current-state tree provides deterministic root and bounded recomputation? | same root across implementations/VACUUM/reorder/index changes; local edit touches bounded hash path | choose measured fan-out/domain layout; root contract remains frozen |
| E-08 | Can an independent writer, using only the public spec, make a valid external commit that the product accepts without identity remint or private repair? | valid move/split/merge/extension commit accepted; malformed variants rejected | **ARCHITECTURE FALSIFIER**: specification/conformance model is inadequate |
| E-09 | Can local query/edit avoid whole-document materialization at 10, 500, and 5,000 page-equivalent tiers? | report memory, touched records/bytes, amplification, index rebuild, delta size, and latency; locality remains bounded | optimize records/indexes; unbounded tier growth reopens storage architecture |
| E-10 | Can a 1 GiB synthetic embedded asset be added/replaced/verified without rewriting or materializing unrelated semantic content? | streaming digest/write, bounded memory, old-or-new crash outcome, unrelated record bytes untouched | change blob/chunk mechanism; asset identity contract remains |
| E-11 | Can total external-state loss rebuild FTS/query indexes and resume bounded deltas without semantic identity change? | artifact-only rebuild exact; expired/gap cursor explicitly resyncs; no silent delta loss | fix runtime protocol; dependency on external truth is an architecture falsifier |
| E-12 | Can Typst express the fixture's required layout intent and return stable semantic-object-to-region mappings under full reflow? | required layout cases render or explicitly capability-report; source maps exact | **RENDERER FALSIFIER**: test another qualified paginated provider; do not build a typesetter by default |
| E-13 | Can the paginated provider meet the required typography and tagged PDF/UA-1 profile? | independent PDF validation plus script/font/fallback/reading-order fixtures and recorded warnings | **RENDERER FALSIFIER** if no existing provider can satisfy the frozen acceptance ceiling |
| E-14 | Can pinned Chrome for Testing/Chromium produce accessible continuous HTML with exact source attributes and deterministic tested projection? | DOM/accessibility checks and 100% required-object mapping, no semantic dependence on DOM | change HTML mapping/provider mechanism |
| E-15 | Can F-001 import into native concepts plus scoped facets/capsule without silent loss or dual authority? | feature-by-feature import report, exact capsule digest, valid native root/correspondence | change importer/facet mapping; widespread opaque-core mapping triggers ontology falsifier |
| E-16 | Can the native state export a practically useful DOCX without Word, with honest outcome/evidence states and preserved unknowns? | package/schema validation, source-capsule patch/translation report, no silent loss | **INTEROP FALSIFIER**: strengthen provider or choose DOCX-first/native-format-later fallback before acceptance |
| E-17 | Does current LibreOffice independently open, render, save/reopen the exported DOCX without repair while evidence remains correctly scoped? | recorded version/environment, alternate-provider observation, normalized diff report | provider limitation is reported; a materially unusable export contributes to E-16 falsification |
| E-18 | Do parser limits reject malformed SQLite/CBOR/XML/ZIP, bombs, traversal, recursion, huge counts/strings/extensions, remote fetch, and active content safely? | deterministic non-write classifications; zero execution/fetch; bounded resources | fix parser before any writable acceptance; unsafe open/render is a hard failure |
| E-19 | Can older/newer generation-1 implementations preserve unknown minor data, block unsupported required major capability, and evolve schema without silent drop? | cross-version vectors and fallback/intersection matrix all exact | **ARCHITECTURE FALSIFIER** if open evolution cannot preserve unknown meaning |
| E-20 | Can native Presentation MathML be canonicalized/edited independently while optional TeX and OMML facets retain clear precedence and preservation? | independent canonical vectors; native edits make stale facets explicit; export reports exact | choose another bounded math semantic representation; no universal algebra or OMML authority |

E-01 through E-04 run before broad ontology implementation. E-12 and E-16 run as soon as the minimum mapper can express their killer fixture, not after all native features.

## 6. Milestone A — portable native world identity and recovery

### 6.1 Procedure

1. Generate N-001 and validate its root.
2. Record a 32-ID sentinel set: root (1), flows (2), sections (2), text blocks (4), list/list items (3), table/rows/cells (8), retained boundaries (4), thread/comment (2), suggestion (1), field (1), control (1), note (1), figure (1), and named style (1).
3. Close the kernel; delete the entire external runtime state; move the artifact through SHELLeye; reopen from artifact only.
4. Apply a valid independent-writer move and split; repeat cold recovery.
5. Raw-copy the committed artifact, change both replicas from the same base, present both, and require `divergent_heads`.
6. Explicitly fork one divergent head and verify full ID remint plus bounded origin mapping; explicitly merge a fresh pair and verify bounded parents/mappings.
7. Inject duplicate object and boundary IDs in separate invalid artifacts and verify neither steals a sentinel identity.

### 6.2 Acceptance

- 32/32 sentinel identities recover exactly before typed retirement;
- every typed retirement returns the specified split/merge/destroyed state and never a similar replacement;
- external runtime deletion, process death, path move, and valid external reorder/move do not change surviving identity;
- raw copies remain same-branch replicas until divergence; divergence is never linearized silently;
- fork remints 100% of public objects and boundaries and preserves exact bounded origin mappings;
- merge targets a validated explicit branch and records at most two parents;
- duplicate-ID or invalid-root artifacts are never accepted writable;
- all A hard-zero metrics remain zero.

**A identity killer:** byte-identical replica divergence followed by coexistence, a stale pre-divergence handle, explicit fork, cross-branch paste, and duplicate-ID injection. Any silent continuity or write is an architecture failure.

## 7. Milestone B — transactions, deltas, anchors, and rendering

### 7.1 Required transactions

B runs three coherent multi-operation transactions over N-001:

- **B-T1:** structure/text/list/table/style/comment/field/layout changes plus two overlapping retained ranges;
- **B-T2:** exact independent external commit reconciliation, then suggestion/control/note/citation/figure/math/extension mutations;
- **B-T3:** split and merge text/table objects, resolve review content, update layout pressure, and exercise boundary/extension policies.

Each uses `expected_revision_id`, publishes one revision/root, returns an exact semantic delta and layout invalidation set, and leaves no partial state. One stale T1 revision is retried against the T2 head and must be refused. One unknown-required interval intersection must be refused.

### 7.2 Acceptance

- all requested semantic postconditions: 100%;
- unrequested semantic mutations: 0;
- stale/unsafe requests classified before write: 100%;
- each commit produces one exact gap-aware delta; a deliberate expired cursor returns `resync_required`;
- six retained ranges and their independent boundary IDs obey all edge/deletion/split/merge rules;
- unknown disjoint extensions remain exact; unsafe intersections refuse or follow declared generic transform;
- layout invalidation includes every affected profile and excludes semantically unrelated profiles;
- continuous HTML and PDF/UA-1 are attributable to exact semantic/layout revisions;
- all fixture objects marked `required_render_mapping` have 100% semantic-to-region mapping;
- native A–D run with Word absent and every Word dependency counter zero.

**B span killer:** co-located overlapping boundaries through edge insertions, partial/complete deletion, a block split and merge, duplicate quotation, decomposed combining text, emoji/ZWJ, and mixed bidi; no quote-based rebound is permitted.

**B render killer:** one early edit forces later-page, table, footnote, header/footer, and page-field reflow while semantic IDs remain stable and every output/source revision and required accessibility mapping remains exact.

## 8. Milestone C — 32-case native hostile suite

Every row is one scored case. Parameter vectors within a row test one invariant and do not inflate cardinality.

| ID | Setup | Adversary | Expected classification/result | Wrong-result condition | Hard metrics |
| --- | --- | --- | --- | --- | --- |
| C-01 | committed N-001 and 32 sentinels | kill kernel, delete all runtime state, move carrier, reopen | exact artifact-only recovery | any remint, missing live ID, or path-derived identity | H-01, H-07, P-01 |
| C-02 | valid head/root | `VACUUM`, rebuild indexes, reorder physical/canonical records without logical change | same revision/root and semantics | revision/root or object change from storage maintenance | H-01, H-13, P-01 |
| C-03 | duplicate paragraphs and rows | move paragraph and reorder duplicate rows | same moved IDs at new locations | coordinate/text match steals or remints identity | H-01, H-03 |
| C-04 | retained handle to a paragraph | delete object, create byte-identical replacement | old ID `destroyed`/later `unknown_retired`; new ID live | old handle mutates replacement or similarity resurrects | H-07, H-05 |
| C-05 | block and duplicate-row source | within-branch deep copy twice | every copied occurrence/boundary ID reminted; asset digest may share | clone reuses semantic ID or aliases a comment/control | H-03, H-01 |
| C-06 | one live branch with retained ranges | Save As/explicit fork | same family, new branch, 100% public ID/boundary remint, exact origin map | any source handle actuates fork or ID retained | H-02, H-06 |
| C-07 | native fixture and template source | independent duplicate and template instantiation | new family/branch/all public IDs; bounded provenance only | byte equality collapses families or genealogy required forever | H-02, H-03 |
| C-08 | byte-identical A/B replicas | commit different changes from same base, later coexist; then fork/merge | `divergent_heads`, writes stopped; explicit resolution only | timestamp/path silently chooses head or mixes changes | H-02, H-04, H-05 |
| C-09 | source and forked branch | cross-branch paste plus stale source-branch handle on pasted object | paste subgraph reminted; stale handle refused | pasted object retains source ID or wrong branch mutates | H-06, H-01 |
| C-10 | independently rewritten DND | duplicate one public object ID and break one typed reference | validation failure; artifact non-writable | duplicate steals target or broken ref auto-repaired | H-03, H-15 |
| C-11 | valid retained anchors/root | duplicate a boundary ID and supply mismatched semantic root | validation failure; artifact non-writable | boundary alias accepted or root silently regenerated | H-08, H-15 |
| C-12 | comment/style/suggestion/named anchors | insert at every start/end edge under include/exclude and point before/after policies | exact logical inclusion matrix | visual-left/right, nondeterministic expansion, wrong anchor | H-08, H-13 |
| C-13 | overlapping ranges with co-located independent boundaries | edit/release one range and insert at shared position | other boundary identities/policies unaffected | physical interning couples identities or lifecycles | H-08, H-01 |
| C-14 | retained ranges over unique and repeated text | partial delete then typed replacement | exact shrink/replacement mapping | quote search or range jumps to duplicate | H-08, H-13 |
| C-15 | comment/style/reference/extension targets | delete complete range, then owner block | typed collapsed/orphaned/destroyed/unresolved/refused states | any target rebounds or required extension is lost | H-07, H-08, H-09 |
| C-16 | range crossing split point | split text block at/coincident with boundaries | old block retired; new block IDs; boundary IDs exact; allowed range multi-interval | arbitrary survivor identity or hidden heuristic mapping | H-01, H-08 |
| C-17 | two blocks with anchors/comments | merge, then move result; GC lineage after dependencies expire | inputs `merged`, result new, boundaries exact; later `unknown_retired` only | selected input survives or GC enables fuzzy recovery | H-07, H-08 |
| C-18 | duplicate quotation plus decomposed combining, emoji/ZWJ, Arabic/mixed bidi | independent valid scalar/grapheme edits and one stale external edit | logical Unicode-safe exact mapping or explicit stale/ambiguous refusal | split scalar/grapheme by human op, visual affinity, or quote rebound | H-04, H-05, H-08 |
| C-19 | unknown optional object/property extension disjoint from target | unrelated semantic edit and record rewrite | payload/value/digest/coverage byte/value exact | payload dropped/normalized or disjoint edit refused without reason | H-09, H-13 |
| C-20 | unknown move-with-target and generic-transform extensions | move and typed copy/remint target | exact move/remap under contract | stale references, reused IDs, or scope escapes | H-09, H-10 |
| C-21 | unknown required text-interval extension | insert/delete/split across covered boundary without capability | `blocked_required_extension`, no commit | edit proceeds, payload lost, or scope guessed | H-09, H-10, H-11 |
| C-22 | topology-region extension over merged table | cell delete/merge/split with/without declared transform | exact generic transform or refusal/invalidate optional | topology mutates beyond declared coverage | H-10, H-13 |
| C-23 | optional and required extensions with fallback | corrupt payload; remove required capability; render fallback | invalid/non-writable for digest corruption; required blocked; optional fallback reported | corrupt writable, required silently ignored, or active payload executed | H-09, H-15, H-16 |
| C-24 | valid handle at revision R | commit R+1, then submit R-conditioned mutation | `stale_revision`, zero write | stale mutation applied/rebased by guess | H-04, H-13 |
| C-25 | two writers at one base and later observed heads | concurrent commit and request against unresolved head | `divergent_heads`; write refused pending explicit resolution | mixed/last-writer state or target on wrong head | H-02, H-04, H-11 |
| C-26 | one multi-object transaction including streamed asset | inject failure before semantic write, during object/blob/root/head publication, and disk full | each recovery exactly old or new; no acknowledged partial | mixed objects/assets/root or partial acknowledgment | H-11, P-04 |
| C-27 | commit succeeds but response is lost | retry same idempotency key, then expired-key query | exact committed outcome; later `outcome_unknown`, no replay | duplicate mutation or false success/failure | H-11, H-13 |
| C-28 | semantic commit succeeds | external index update fails; delete index and rebuild; then expire a delta cursor | semantic head remains valid; rebuild exact; cursor `resync_required` | rollback semantics for cache, identity loss, or silent delta gap | H-12, H-13, P-01 |
| C-29 | paginated N-001 | early edit changes all later page breaks and page-count field | new LayoutRevision; semantic IDs stable; field/result staleness exact | page identity treated semantic or old page field labeled current | H-01, H-14 |
| C-30 | script/font fixture and pinned layout profile | change font manifest; shape Arabic, CJK, Indic, combining, emoji | distinct LayoutRevision, capability warnings, correct logical text | unqualified render reuse, lost text, or unsupported feature hidden | H-14, P-06 |
| C-31 | large table and footnote near page edge | edit forces table split/repeat-header and footnote relocation | qualified deterministic layout or explicit provider limitation | semantic table/note identity changes or layout gap hidden | H-01, H-14 |
| C-32 | valid semantic and layout revisions | request render with stale/mismatched revision and remove required tags/source map | mismatch refused; accessibility/source-map validation fails | wrong revision emitted/accepted or mapping omission passes | H-14, H-15, P-07 |

C accepts only if all 32 rows execute, every expected classification/result is exact, H-01–H-18 remain zero where applicable, P-01–P-07 are satisfied for the native scope, and every case records old/new artifact digests, revision/root, target IDs, provider profile, and adversary injection point. A provider limitation is acceptable only where the row permits an explicit limitation; it never converts a semantic/identity failure into a pass.

## 9. DOCX-provider supplement — 4 cases

| ID | Setup | Adversary | Expected result | Wrong-result condition | Metrics |
| --- | --- | --- | --- | --- | --- |
| X-01 | import F-001 with round-trip requested | unsupported MC/custom part/OMML/content-control features | native mappings plus scoped facets, exact embedded capsule/digest, feature capability report | silent disappearance, dual authority, or provider ID used as native ID | H-09, H-13, X-P1 |
| X-02 | untouched converted state, then one disjoint supported edit | source reuse first; bounded patch second | `exact_source_reuse`, then `preserved_patch`; untouched entries/payloads exact | vague fidelity, whole-package normalization, or source capsule overrides native edit | H-09, H-13, X-P2 |
| X-03 | native edit intersects unknown required provider coverage | attempt DOCX export | `blocked` or `translated_with_declared_loss` only after explicit authorized transform; no silent loss | export reports conformant/exact while dropping/overwriting unknown | H-09, H-10, H-18 |
| X-04 | Word absent; exported package available | validate OPC/schema and open/save/reopen with pinned LibreOffice | package/schema evidence plus `alternate_provider_observed`; normalization diff reported | label as `microsoft_observed`, execute active content, or hide repair/loss | H-16, H-17, H-18 |

Provider supplement positive requirements:

- **X-P1:** every F-001 feature classified = 100%; capsule digest exact = 100%;
- **X-P2:** every untouched preserved entry/payload required by the plan is byte/value exact = 100%;
- no supplemental result is called Microsoft-observed.

## 10. Milestone D — one 96-call Program Host invocation

The test harness may pause the invocation to let the independent public-spec mutator publish one valid external commit. That external action is neither a Program Host call nor one of the 48 counted Host mutations.

### 10.1 Exact call arithmetic

| Call class | IDs | Count |
| --- | --- | ---: |
| query/inspect/plan | Q-01–Q-18 | 18 |
| semantic mutations | M-01–M-48 | 48 |
| transaction control | T-01–T-06 | 6 |
| delta consumption | G-01–G-06 | 6 |
| postcondition/refusal | V-01–V-06 | 6 |
| external-head reconciliation | R-01–R-04 | 4 |
| layout/render | L-01–L-04 | 4 |
| DOCX export/capability | O-01–O-04 | 4 |
| **Total** |  | **96** |

### 10.2 Query/inspect/plan calls

| ID | Meaningful typed call |
| --- | --- |
| Q-01 | open and validate N-001 from artifact |
| Q-02 | report native/provider capabilities and mode |
| Q-03 | inspect family, branch, head revision, and root |
| Q-04 | query headings/sections by semantic role |
| Q-05 | query all byte-identical sentences with distinct IDs |
| Q-06 | inspect table topology, rows, columns, cells, and regions |
| Q-07 | inspect list membership/nesting/restart rules |
| Q-08 | inspect comment threads, replies, target states |
| Q-09 | inspect all six suggestion types |
| Q-10 | inspect field source/dependency/result/staleness |
| Q-11 | inspect eight control types/constraints/values |
| Q-12 | inspect notes, anchors, references, and citations |
| Q-13 | inspect figures, asset digests, and math facets |
| Q-14 | query effective style with provenance |
| Q-15 | inspect both layout profiles and invalidation inputs |
| Q-16 | inspect extension coverage/policy/capability matrix |
| Q-17 | resolve a revision-local scalar and grapheme range |
| Q-18 | plan retained ranges and exact transaction targets |

### 10.3 Exact 48-mutation manifest

There are 16 operation families and exactly one mutation from each family in each of three transactions. The table cell order defines M IDs: T1 cells are M-01–M-16, T2 cells M-17–M-32, T3 cells M-33–M-48.

| Family | Transaction 1 | Transaction 2 | Transaction 3 |
| --- | --- | --- | --- |
| F-01 structure | create appendix section | move second duplicate-sentence block into appendix | merge two designated adjacent text blocks, minting result |
| F-02 text | insert Arabic clause at exact boundary | replace only the second duplicate sentence | delete designated decomposed-combining phrase by retained range |
| F-03 retained anchors | retain first overlapping range | retain second overlapping range with independent co-located edge | release the designated temporary retained range |
| F-04 lists | insert a multi-block list item | set explicit restart/start rule | reorder one nested item without ID change |
| F-05 tables | insert one row/cells | merge two designated cells, minting region | split the original 2×2 merged region, minting cells |
| F-06 roles/styles | update one theme token | apply a named style without changing role | add one direct override |
| F-07 comments | open a thread on retained range | add a reply | resolve the thread |
| F-08 suggestions | create replace suggestion | accept the fixture insert suggestion | reject the fixture style/property suggestion |
| F-09 fields | add bounded count field | change one field evaluation policy | evaluate deterministic metadata field with explicit inputs |
| F-10 controls | set enum value | append repeating-group item | set object/reference selector by exact ID |
| F-11 notes/references | add footnote occurrence/body | move endnote reference occurrence | retarget one cross-reference to exact named anchor |
| F-12 citations/links | add link occurrence | add citation occurrence | update exact citation locator |
| F-13 figures/assets | add figure sharing existing asset digest | replace one figure's asset without changing figure ID | update one caption and alt-text property transactionally |
| F-14 math | insert inline MathML object | update display MathML semantics | update its TeX source facet and mark provider alignment |
| F-15 layout intent | change section margin | insert explicit page break | update header content/numbering intent |
| F-16 extensions | attach known optional object extension | move move-with-target extension with owner | copy generic-transform extension with complete remap |

### 10.4 Control, delta, refusal, reconciliation, render, and export calls

- **T-01/T-02:** begin/commit transaction 1 with one expected revision.
- **T-03/T-04:** begin/commit transaction 2 against the reconciled external head.
- **T-05/T-06:** begin/commit transaction 3.
- **G-01/G-02:** read and acknowledge the exact transaction-1 delta.
- **G-03/G-04:** read and acknowledge the exact transaction-2 delta.
- **G-05/G-06:** read and acknowledge the exact transaction-3 delta.
- **V-01:** assert all transaction-1 semantic postconditions.
- **V-02:** attempt one stale-revision mutation and require refusal.
- **V-03:** attempt one unknown-required interval intersection and require refusal.
- **V-04:** assert all transaction-2 semantic postconditions.
- **V-05:** assert all transaction-3 semantic postconditions.
- **V-06:** assert final head/root, 48 requested mutations, zero unrequested changes, and exact refusal classifications.
- **R-01:** detect the independent writer's coherent new artifact head.
- **R-02:** validate/classify it as an exact descendant and invalidate stale runtime views.
- **R-03:** rebuild affected query/index fragments from its semantic delta/state.
- **R-04:** rebind the Program Host to the exact artifact head and sentinel IDs.
- **L-01:** request the pinned LayoutRevision and warnings.
- **L-02:** render continuous accessible HTML.
- **L-03:** render tagged PDF/UA-1.
- **L-04:** verify source attribution and required semantic-object/region correspondence.
- **O-01:** plan DOCX export and per-feature capability/outcome ceiling.
- **O-02:** perform exactly one Word-free DOCX export.
- **O-03:** run OPC/package/schema validation and request alternate-provider test input.
- **O-04:** retrieve the final semantic-outcome and observation-evidence report.

D therefore has exactly two material render calls, one material export call, two refusal branches (stale and required-extension), one external recovery/reconciliation branch, three commits, and six delta-consumption calls. Raw SQL, CBOR, ZIP/XML, OOXML, VBA, or provider process scripting cannot count toward any call.

### 10.5 D acceptance

D passes only when one Node Program Host invocation executes every Q, M, T, G, V, R, L, and O call exactly once in the frozen class arithmetic, with zero intermediate model calls; all 48 successful semantic mutations appear once in the three exact deltas and final state; all three commits are atomic and revision-conditioned; V-02/V-03 perform zero writes; the independent external commit is reconciled without ID loss or full rediscovery; HTML and PDF carry the exact final source; the DOCX plan/export/report uses no raw escape; and every hard-zero metric remains zero. A repeated getter, no-op, hidden batch script, or unenumerated state-changing call invalidates the cardinality rather than being ignored.

## 11. Independent native mutator/rewriter

Build 001 includes a small second implementation in a language/code path that shares no product-private object/serializer code. It uses only the public native-format specification and canonical vectors. Its roles are:

- construct canonical valid records and roots;
- reorder records and make semantically neutral rewrites;
- make valid move, split, merge, copy, boundary, extension, and external commit operations;
- create raw same-branch replicas and divergent heads;
- generate duplicate IDs, broken references, malformed CBOR, invalid roots, corrupt assets/extensions, stale revisions, and resource-limit inputs;
- prove the main implementation does not require private repair or reminting to accept a valid external writer.

It is a conformance oracle, not a verification agent and not product implementation reuse.

## 12. Hard-zero metrics

All counters below must be instrumented by test case, transaction, target ID, expected/actual revision, and artifact digest. Required value is zero.

| ID | Hard-zero metric |
| --- | --- |
| H-01 | wrong native object mutations |
| H-02 | false branch continuity / silent replica linearization |
| H-03 | duplicate-ID stealing or copied semantic-ID reuse |
| H-04 | stale revision writes |
| H-05 | ambiguous writes |
| H-06 | cross-branch stale-handle mutations |
| H-07 | destroyed/retired-object resurrection |
| H-08 | wrong anchor/boundary mutations |
| H-09 | unknown-extension/provider-facet payload losses |
| H-10 | unknown-extension coverage/scope escapes |
| H-11 | acknowledged partial commits |
| H-12 | silent delta gaps |
| H-13 | unrequested semantic mutations |
| H-14 | render/source or layout/source revision mismatches |
| H-15 | invalid artifacts accepted writable |
| H-16 | active content executed or remote content fetched on open/render |
| H-17 | native acceptance Word dependencies: WINWORD processes, COM/API calls, or Word-produced expected artifacts |
| H-18 | false DOCX fidelity/evidence classifications |

## 13. Positive metrics and reporting

| ID | Required value |
| --- | ---: |
| P-01 artifact-only cold recovery exactness | 100% |
| P-02 requested semantic postconditions | 100% |
| P-03 stale/refusal classifications | 100% |
| P-04 old-or-new crash outcomes | 100% |
| P-05 required extension/facet preservation | 100% |
| P-06 delta coverage for committed semantics | 100% |
| P-07 semantic-to-render mapping for fixture objects marked required | 100% |

Implementation reports, but does not pre-claim, latency, peak memory, model-facing bytes, provider rediscoveries, query count, operation density, correspondence recoveries, semantic delta size, touched records/bytes, and local mutation amplification.

### 13.1 Comparative benchmark

Run the same supported workflow against the old DOCX-first operating model and the native model. Record identical hardware/environment and measure:

- model-facing bytes;
- provider rediscoveries;
- query calls;
- useful operations per Program Host call/invocation;
- correspondence-recovery attempts;
- semantic delta bytes versus representation diff bytes;
- latency and peak memory;
- logical/physical records and bytes touched.

No numeric performance threshold is frozen before measurement. Native advantage is falsified if it yields no material safety or operational improvement across the measured workflow while imposing its permanent format costs.

### 13.2 Size tiers

Use deterministic 10-, 500-, and 5,000-page-equivalent native documents plus the 1 GiB synthetic embedded-asset case. Measure cold validation, index rebuild, local paragraph query/edit, root recomputation, delta size, render planning, and asset replacement. A one-paragraph mutation must not require whole-document semantic materialization or rewrite unless E-09 proves that unavoidable and architecture is reopened.

## 14. Architecture falsifiers and required response

| ID | Falsifier | Required response |
| --- | --- | --- |
| F-01 | boundary/anchor semantics cannot be met without unbounded history or wrong rebound | stop; test one bounded CRDT-like mechanism; reopen text architecture; use native-format-later if permanent history is unavoidable |
| F-02 | raw-copy divergence, fork remint, or merge cannot prevent false continuity | stop and reopen family/branch model; fall back to DOCX-first if intrinsic branch safety is not specifiable |
| F-03 | SQLite/application-file profile cannot deliver coherent portable old-or-new state | change container/journal mechanism; if none passes, choose native-format-later or DOCX-first |
| F-04 | extension coverage cannot support safe disjoint edits and exact unknown preservation | reopen extension/native ontology; native-first cannot freeze without a safe replacement |
| F-05 | no existing renderer meets required layout/typography/tagged-PDF/source-map ceiling | try another provider contract implementation; if all require a hidden Word dependency or native typesetter, reopen render/Build thesis |
| F-06 | bounded Word-free DOCX interoperability is practically unusable or falsely classifiable | strengthen provider; if still failed, preserve DOCX-first or choose native-format-later before acceptance |
| F-07 | native-authored fixture core requires opaque provider blobs, or imported supported core is predominantly uneditable opaque content | reopen ontology/import boundary; do not relabel opaque storage as native semantics |
| F-08 | large-document/local-asset behavior requires unbounded materialization/amplification | change record/order/root mechanism; persistent failure reopens native container/model |
| F-09 | comparative Program Host benchmark shows no material safety or operational advantage | reopen material-advantage determination; DOCX-first remains the baseline fallback |
| F-10 | an independent implementation cannot encode, validate, preserve, and commit the public format | stop; repair open specification/canonicalization; native freeze cannot be accepted without it |

An architecture falsifier blocks Build 001 completion. Implementation may change a mechanism inside a frozen invariant; it may not lower the invariant or continue under a quiet exception.

## 15. Exact Word-free acceptance condition

Native Milestones A–D run on STEALTHEYELLC while desktop Microsoft Word remains uninstalled:

- no `WINWORD.EXE` present or required;
- Word process count = 0;
- `Word.Application` COM calls/activation = 0;
- Word API calls = 0;
- Word-generated expected/native acceptance artifacts = 0;
- Microsoft 365 trial not started.

Word-provider capability is `unavailable_provider`; native capability remains supported. X-01–X-04 are also Word-free.

## 16. Conditional future Word suite — specified, not run

After Build 001, on a separately suitable licensed Windows environment, an optional suite may test open-without-repair, save/reopen, field update, repagination, comments, revisions, controls, layout, Microsoft PDF export, and normalization comparison. Every observation records Word version/build, Windows build, fonts/digests, locale, compatibility mode, and relevant renderer/printer settings. Results are `microsoft_observed` only for that profile and never change native semantic authority.

## 17. Non-goals

Build 001 does not include:

- a visual editor or word processor;
- live multi-user collaboration/CRDT sync;
- a full typography engine;
- broad DOCX compatibility beyond F-001 and the four-case supplement;
- ODF provider implementation;
- Microsoft Word installation/trial or Microsoft acceptance;
- signatures/encryption implementation;
- arbitrary field/control code or automatic remote binding;
- complete drawing/chart/OLE execution;
- Build 002 design, source, fixtures, packages, or results claims.
