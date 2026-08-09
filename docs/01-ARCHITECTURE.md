# 01 — Architecture

Status: **FINAL / SYNTHESIZED / VERIFIED / FROZEN FOR BUILD 001**  
Architecture family: **NATIVE SEMANTIC AUTHORITY WITH NON-AUTHORITATIVE PROVIDER FACETS**

## 1. Architectural determination

The DOCX-first architecture frozen at `47b5280b71773ae497b5a56c4b590b9070b4bca5` is **SUPERSEDED AS DOCX-FIRST ARCHITECTURE BASELINE**.

Native authority clears the material-advantage threshold because it permanently relocates identity, revision control, retained anchors, atomic semantic transactions, extension safety, and bounded deltas from a provider/correspondence inference problem into the portable authored artifact. Those gains survive application replacement, provider absence, carrier movement, and renderer change. They materially improve exact targeting and local Program Host density rather than merely improving storage elegance.

The permanent costs—format stewardship, conformance, renderer mappings, DOCX conversion, extension governance, parser surface, and ecosystem friction—are accepted only with a narrow authored-information ontology, an open logical specification, an independent writer/validator, non-authoritative provider facets, and an explicit foreign-managed mode. Build 001 can still falsify that trade.

## 2. Authority architecture

### 2.1 Native mode

One validated DND logical state is the sole editable semantic authority. The artifact carries its family, branch, current revision/root, public object identities, retained boundaries, semantics, extensions, assets, and layout intent.

Provider source bytes and facets are evidence and interoperability constraints. A required or unknown facet may block an intersecting edit/export; it may not silently replace current native meaning. Layout and renders are derived realizations scoped to qualified revisions. SHELLeye owns the physical carrier, not authored identity.

### 2.2 Foreign-managed mode

Foreign-managed DOCX is retained because it preserves immediate utility for existing documents without forced conversion and keeps the strong old correspondence/preservation architecture available. The complexity is bounded by a hard mode discriminator:

| State | Editable semantic authority | Native identity |
| --- | --- | --- |
| unconverted DOCX | DOCX/provider state | external DOCSeye correspondence only; old baseline rules |
| converted/imported DOCX | DND | intrinsic; source DOCX optional non-authoritative capsule |
| native-authored | DND from creation | intrinsic |

The kernel, capability report, and Program Host must expose the mode. No operation may treat one state as both DOCX-authoritative and native-authoritative. Conversion is an explicit transaction that mints a native family/branch and records the source boundary.

### 2.3 Authority distinctions

- **semantic authority**: current native logical state, or the provider artifact in explicit foreign-managed mode;
- **provider-source evidence**: immutable source bytes, digest, and import provenance;
- **provider-specific semantic facet**: scoped provider meaning/correspondence that may constrain preservation/export;
- **layout authority**: authored native layout intent;
- **layout realization authority**: one provider/environment's qualified page/region result;
- **render authority**: derived output bytes plus attribution/validation;
- **physical carrier authority**: SHELLeye's coherent file object/publication facts.

## 3. Rejected and superseded alternatives

| Candidate | Determination | Reason |
| --- | --- | --- |
| DOCX-FIRST | **SUPERSEDED**, retained for foreign-managed mode | Strong interoperability baseline, but permanent semantic identity and transaction correctness remain dependent on external correspondence and provider normalization. |
| NATIVE-FIRST without capsules/facets | **REJECTED** | Converts unsupported foreign meaning into loss or opaque unscoped bytes and makes export claims dishonest. |
| TRUE DUAL/BRAIDED AUTHORITY | **REJECTED** | Two editable semantic authorities create irreducible conflict, stale-overwrite, and capability ambiguity. |
| NATIVE-FORMAT-LATER | **REJECTED for Build 001**, retained fallback if falsified | Defers the highest-leverage identity/anchor/extension questions until after provider coupling hardens. |
| universal canonical AST | **REJECTED** | Absorbs foreign domains, cannot preserve all provider semantics, and recreates a giant universal graph. |
| canonical Typst/HTML/DOM/OOXML | **REJECTED** | Confuses renderer or provider representation with authored semantic authority. |
| append-only historical DAG/CRDT core | **REJECTED for generation 1** | Sole intended operator does not justify permanent per-character/history cost; bounded parents/witnesses satisfy current requirements. |

## 4. Native semantic model

The complete normative object/format rules are in [NATIVE-FORMAT.md](NATIVE-FORMAT.md). The architecture freezes these categories:

- root, flows/stories, sections, and typed blocks;
- text blocks with semantic roles and exact UTF-8 text;
- first-class lists/items and tables/rows/columns/cells/regions;
- links, citations, references, named anchors, and retained ranges;
- threads/comments/replies and active suggestions;
- bounded fields, controls, notes, figures/assets, and math;
- named styles, themes/tokens, direct overrides, layout profiles;
- extension objects and provider facets.

Public objects receive cross-revision UUIDv4 identity. Heading role, direct formatting, list marker policy, coordinate/span values, and similar attributes remain value-like. Assets use SHA-256 content identity. Pages, runs, integer-offset ranges, DOM nodes, layout regions, and search hits remain revision-local derived objects.

### 4.1 No universal foreign ontology

The ontology describes DOCSeye-authored information. It does not normalize every OOXML extension, every PDF object, spreadsheets, slide animation, executable code, databases, live DOMs, or temporal media. Unsupported foreign semantics remain explicitly scoped provider facets/capsules or inert opaque content. DATAeye, CODEeye, MEDIAeye, DESKTOPeye, eyeBROWSE, and SHELLeye ownership remains intact.

## 5. Identity, branch, and lifecycle architecture

### 5.1 Opaque identifiers

Family, branch, revision, object, and boundary identifiers use UUIDv4 encoded as RFC 9562 values. UUIDv7 was rejected for semantic IDs: index locality is useful but time/order leakage and implied chronology add no semantic information. Ordering and physical locality have separate mechanisms.

Within a resolved family, `(branch_id, native_object_id)` directly identifies an object. Portable external writes also carry `family_id` and `expected_revision_id`. A separate kernel logical-concept ID exists only for real cross-provider/cross-branch/lineage correspondence; ordinary native actuation has no redundant ID layer.

### 5.2 Family and branch

Path and byte equality never define authored intent. Save/move/rename preserve family/branch. A controlled replica is initially the same branch snapshot. Save As and explicit fork preserve family, create a branch, and remint every public semantic/boundary ID. Independent duplicate and template instantiation create a new family/branch and remint all IDs.

The full remint rule differs from the retain-local-ID proposals because it prevents accidental targeting by a bare ID and makes branch separation visible at every realization. Bounded `origin_ref` mappings preserve merge/correspondence ergonomics.

### 5.3 Physical clone paradox

`A.dnd` copied byte-for-byte to `B.dnd` initially yields two replicas of the same committed branch snapshot. Opening `B.dnd` does not mutate it. SHELLeye distinguishes carriers. If both later commit from the same base, DOCSeye classifies **`divergent_heads`**, stops combined write authority, and requires explicit fork or merge. No timestamp/path heuristic silently linearizes them.

### 5.4 Copy, split, merge, retirement

Copy/paste remints the copied public object/boundary subgraph. Immutable asset bytes may remain shared by digest. Cross-branch and cross-family paste always remint. Provider facets copy only through a declared safe transform.

Split retires the original and mints every successor. Merge retires every input and mints the result. This applies consistently to text blocks, list items, cells, and table regions. It rejects arbitrary “left side survives” continuity. Stable boundary IDs may survive exact owner transformations; object IDs do not.

Bounded witnesses classify old IDs as `destroyed`, `split`, `merged`, or after eligible GC `unknown_retired`. Live anchors, active suggestions, recent/gap-supported deltas, expected-revision windows, unresolved merge bases, and current references retain necessary witnesses. No similarity resurrection and no permanent genealogy are allowed.

## 6. Temporal and transaction architecture

### 6.1 Revision

Each head carries an opaque UUIDv4 `revision_id`, branch-local `revision_sequence`, SHA-256 `semantic_root`, and zero/one/two bounded parents. The root is semantic state identity; revision ID is commit identity; sequence is convenience, not causality. Divergent siblings may share a sequence.

The root is a domain-separated Merkle-style current-state tree over canonical logical semantics, including identities, order, text/boundaries, relations, extensions, required assets/capsules, layout intent, and currently required lifecycle witnesses. It excludes SQLite physical layout, indexes, caches, renders, and history. Exact tree shape is E-07; the invariant is frozen.

`VACUUM`, page movement, index maintenance, and logically neutral record reorder do not create a revision or change the root.

### 6.2 Atomic transactions and recovery

Every write is typed, branch-scoped, and conditioned on `expected_revision_id`. Multi-operation commits are all-or-nothing and publish one revision/root, exact delta, and layout invalidation set. On recovery the artifact is the whole old or whole new committed semantic revision.

A stale revision, divergent head, ambiguous target, invalid artifact, or unsafe extension intersection returns a non-write classification. Post-commit response loss uses bounded idempotency/outcome witnesses; mutation is never blindly replayed. SHELLeye's coherent physical publication is a separate clock after SQLite commit.

### 6.3 Bounded deltas

Native deltas are intrinsic semantic changes, not provider diffs. Cursors are branch/revision scoped and bounded. Cursor expiry/gaps return `resync_required`; no silent gap is allowed. Runtime indexes update delta-first, and a local edit must not require full-document retransmission when bounded fragments suffice.

## 7. Ordering and text architecture

### 7.1 Order

Identity and order are independent. Build 001 begins with 128-bit sparse sibling keys and local rebalance. Canonical hashing uses logical order rather than maintenance keys. E-05 may select a chunked sequence alternative if necessary, but moves must retain IDs, inserts must not renumber unrelated IDs, and maintenance must be root-neutral.

### 7.2 Text and boundaries

Canonical text is exact UTF-8 with no silent Unicode normalization. Boundaries never split scalars; human operations default to extended grapheme clusters. Runtime rope/piece structures are not persistence.

Ordinary ranges remain revision-local. `retain_range` explicitly creates portable, identity-bearing zero-width boundaries and changes the revision. Overlapping/co-located ranges coexist without implicit identity sharing. Edge policies are logical include/exclude or before/after, not visual left/right.

Boundary semantics through insert/delete/replace/move/split/merge are frozen in the native-format document. A split may produce a multi-interval range for permitted anchor types. A comment never rebounds to duplicate quoted text and a cross-reference never retargets by similarity. E-01 chooses the storage mechanism and is an architecture falsifier if no bounded non-CRDT structure satisfies these rules.

### 7.3 Collaboration deferral

Live multi-user collaboration is outside Build 001. ProseMirror mappings, Yjs relative positions, Automerge, and Peritext demonstrate useful mechanisms but do not prove that DOCSeye requires permanent operation/character history for one operator. Future compatibility is preserved by explicit boundaries, branch/revision preconditions, and bounded parents. A CRDT/OT core is adopted only if E-01 proves the frozen semantics impossible without it.

## 8. Extension architecture

Unknown data is safe only when the system knows what it covers. Every extension declares identity, namespace/type/version, encoding/payload/digest, native references, coverage, required/optional status, edit policy, and fallback.

Coverage kinds are object, property, subtree, text interval, relation, topology region, layout profile, and document global. Policies are `independent`, `move_with_target`, `generic_transform`, `invalidate`, and `must_understand_before_edit`. Default unsafe intersection is refusal. This vocabulary was chosen over abstract `DECORATE/CONSTRAIN/OWN` because it states concrete behavior for move, copy, delete, split, merge, text edit, topology change, and export.

An older writer may edit around unknown optional content and preserve it exactly. It may not silently drop it. Unsupported required major capability removes write authority. Optional invalidation is explicit and uses/report fallback. The extension model is a core architecture falsifier in E-04/E-19.

## 9. Storage and serialization architecture

### 9.1 Logical records

Deterministic CBOR under the DOCSeye profile is canonical for logical records. It supplies compact binary values, independent implementation, unknown extension payloads, and deterministic hashing without making serialization the runtime object model. Canonical JSON was rejected for binary/number ambiguity and size; XML for verbosity/parser surface; Protobuf/FlatBuffers/Cap'n Proto for schema/unknown-field and canonicalization mismatch; MessagePack for the weaker standard deterministic profile.

The profile forbids indefinite lengths, duplicate keys, non-shortest integers, binary floating semantic values, invalid UTF-8, and implicit normalization. UUID/digest encodings and resource limits are normative. E-06 requires byte-identical vectors from two independent implementations.

### 9.2 Container

SQLite is the canonical application-file container and transaction substrate. It beats ZIP for in-place atomic multi-object updates, a directory store for portable publication, an append-only custom container for complexity/history pressure, and a pure content-addressed DAG for mutable authored state. A pure relational schema was rejected as the public ontology; SQL mapping is container detail around canonical logical records.

The native artifact uses rollback-journal `DELETE` mode and quiescent one-file publication. WAL's `-wal`/`-shm` state makes an incautious live main-file copy incomplete. External rebuildable indexes use SQLite WAL and FTS. E-02/E-03 validate crash, backup, coherent copy, cloud/sync pressure, disk-full behavior, and SHELLeye publication before the container is trusted.

### 9.3 Canonical file and runtime state

The DND contains all current native truth and durable provider correspondence needed for a requested round trip. External state contains FTS, embeddings, query/layout/render caches, provider availability, process bindings, physical carriers, and open registries. Milestone A deletes all external state and proves exact recovery.

The `.dnd` suffix and provisional media type exist only for Build 001 dispatch. The architecture does not depend on the brand or suffix.

## 10. Validation, security, and active content

Validation strata are container, serialization/schema, identity/reference, semantic invariants, extension integrity, asset/capsule integrity, semantic root, then provider-specific validation. A failure before provider validation removes native write authority. Repair is explicit and may create a new branch/artifact if identity cannot be proved.

Parsing/rendering executes no code and performs no automatic network fetch. Resource limits cover malformed SQLite/CBOR, huge counts/strings/assets/extensions, nesting, recursion, decompression bombs, traversal, external XML entities, and remote URLs. Macros/OLE/ActiveX/scripts remain inert provider bytes. Validation failure never mints identity.

Signatures/encryption are deferred. Root/domain separation and extension points preserve future work; Build 001 implements no proprietary cryptography and no partial signatures.

## 11. Layout and render architecture

Native semantics plus layout intent produce a `LayoutRevision`. Layout intent covers page/media/column/break/keep/header/footer/numbering/figure/table/note/language/hyphenation/writing-mode properties without cloning Word quirks.

A layout revision is qualified by semantic revision, layout profile, provider/version, font manifest, locale, hyphenation data, and configuration. Pages/regions are layout-scoped. Reflow never changes semantic IDs.

DOCSeye owns semantic-to-layout and semantic-to-render contracts, mapping, capability reporting, source attribution, and correspondence. Existing engines own shaping/typesetting/rendering in Build 001:

- **Typst** is the first paginated provider candidate. Generated Typst is derived, never truth. Its warnings/gaps are surfaced. Current evidence shows tagged/PDF-UA facilities, but professional output is not assumed; E-12/E-13 measure the ceiling.
- **HTML + pinned Chrome for Testing/Chromium** is the continuous human/accessibility/web projection. The DOM is derived. Browser PDF is an optional comparison, not the native oracle.
- **PDF** is a derived, source-attributed render. The Build 001 target is tagged PDF/UA-1 for the required fixture, independently validated. A failed accessibility/typography/source-map gate is a render falsifier, not a reason to relabel output “professional.”

Typography fixtures cover Latin/OpenType, Arabic/RTL and mixed bidi, Japanese CJK, Devanagari, decomposed combining marks, emoji/ZWJ, font fallback, hyphenation, tables, and notes. Unsupported vertical writing or other untested areas remain capability-reported.

## 12. DOCX and Word provider architecture

### 12.1 Import and source capsules

DOCX import combines raw OPC/OOXML inspection with Open XML SDK interpretation. It maps supported native concepts, retains partially representable provider facets, scopes opaque unknowns with edit contracts, optionally embeds exact source bytes, records correspondence, and emits a capability report. Provider IDs remain evidence.

The source capsule is embedded by default when round-trip preservation is requested and optional otherwise. It is immutable source evidence. After native edits it may be aligned or stale but never current semantic authority.

### 12.2 Export and evidence

Export reports one semantic outcome: `exact_source_reuse`, `preserved_patch`, `translated_conformant`, `translated_with_declared_loss`, `unsupported`, or `blocked`. It separately reports `package_validated`, `schema_validated`, `alternate_provider_observed`, and only when actually tested `microsoft_observed`.

Untouched provider facets may constrain a preserved patch or force refusal/downgrade. They may never override current native semantics. Schema-valid does not mean Word-observed; LibreOffice evidence does not mean Microsoft evidence.

### 12.3 Word independence

Native Build 001 A–D acceptance runs on STEALTHEYELLC with desktop Word absent:

- `WINWORD.EXE` required: 0;
- Word processes: 0;
- Word COM calls: 0;
- Word APIs: 0;
- Word-produced native acceptance artifacts: 0.

Word absence is not native capability degradation. It is `unavailable_provider` only for Microsoft-specific capabilities.

Word can later provide Microsoft conformance observation, layout/field/revisions/comments/control behavior, normalization/save, and Microsoft PDF export on a suitable licensed Windows environment. It must never become a hidden mandatory dependency or an unsupported unattended server architecture.

ODF 1.4 is architecturally a peer future provider, not Build 001 scope. LibreOffice is an alternate-provider DOCX smoke in the separately scored supplement and never Microsoft evidence.

## 13. Kernel, runtime indexes, and Program Host

The C#/.NET kernel validates and mutates typed native objects. External SQLite WAL state supplies rebuildable FTS and acceleration. Queries return exact typed IDs/revisions, bounded fragments, effective-style provenance, anchor state, extension coverage, and capability state.

The Node 24 Program Host is one deterministic local invocation with no model. Counted acceptance calls must use the typed SDK. Raw SQL, raw CBOR, raw OOXML/ZIP, VBA, or giant hidden provider scripts do not count. Provider-specific operations remain separately typed.

Milestone D is exactly 96 meaningful calls: 18 query/retain, 48 semantic mutations across 16 families, 6 transaction controls for 3 commits, 6 delta consumptions, 6 postcondition/refusal checks, 4 external-head reconciliation calls, 4 layout/render calls, and 4 export/capability calls. There are zero repeated no-op/getter padding calls and zero intermediate model calls.

## 14. Implementation stack and preflight rule

Architecture-level Build 001 choices are:

- C# on .NET 10 LTS for kernel/provider contracts;
- installed Node 24 LTS by absolute path for Program Host;
- SQLite native artifact plus external SQLite WAL/FTS;
- deterministic CBOR;
- Typst paginated provider;
- HTML plus pinned Chrome for Testing/Chromium;
- Open XML SDK plus raw OPC/XML DOCX provider;
- LibreOffice alternate-provider smoke;
- SHELLeye physical publication/snapshot coordination.

Implementation must select the latest stable supported patch at preflight, record version, provenance, binary/package digest, fonts, locale, and configuration, then pin the acceptance environment. Exact research-observed minor versions are evidence, not architecture, except the measured installed Node path/version recorded in the platform document. No dependency is installed by this freeze.

## 15. Cross-substrate architecture

- SHELLeye owns physical files, coherent snapshots, movement, copies, and atomic replacement.
- DESKTOPeye owns windows/focus/caret/UI state.
- CODEeye owns source and repository semantics.
- eyeBROWSE owns live web/DOM/session state.
- DATAeye owns computational data/table semantics and external data binding.
- MEDIAeye owns full temporal media.

DOCSeye may correlate or embed representations but does not seize ownership. There is no permanent universal StealthEye graph merely because relations exist.

## 16. What Build 001 can still falsify

Build 001 can falsify the boundary model, replica/fork identity, SQLite application-file profile, extension intersection model, independent conformance specification, renderer ceiling, practical Word-free DOCX export, large-document locality, or Program Host advantage. If an architecture-level falsifier fails, implementation stops and reopens only that numbered question. It must not quietly patch around a failed invariant, claim acceptance, or proceed to a broader build.
