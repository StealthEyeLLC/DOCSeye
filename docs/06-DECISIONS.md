# 06 — Frozen Decision Register

Status: **CANONICAL / FROZEN FOR BUILD 001**  
Canonical decision count: **64**

## 1. Reading this register

`N-001` through `N-064` are contiguous and active. A numbered experiment may choose a mechanism inside a decision's invariant; it may not weaken that invariant silently. A report recommendation is not evidence and a proposed benchmark is not a result.

## 2. Authority and scope decisions

| ID | Frozen decision | Evidence basis | Reopen only if |
| --- | --- | --- | --- |
| N-001 | Architecture is **NATIVE SEMANTIC AUTHORITY WITH NON-AUTHORITATIVE PROVIDER FACETS**. | ARCHITECTURAL INFERENCE | A Build 001 falsifier shows permanent costs exceed identity/transaction/provider-independence leverage. |
| N-002 | A validated DND is the sole editable semantic authority in native mode. | ARCHITECTURAL INFERENCE | Never by incidental provider behavior; only an explicit architecture replacement. |
| N-003 | Canonical DOCSeye retains explicit native-authored, converted/imported, and foreign-managed modes; one state has exactly one semantic authority. | ARCHITECTURAL INFERENCE | Mode complexity proves materially worse than forced conversion or DOCX-only utility under implementation evidence. |
| N-004 | Source capsules are authority only for historical source bytes/evidence; provider facets may constrain preservation/export but never override current native semantics. | ARCHITECTURAL INFERENCE | Never without replacing the one-authority invariant. |
| N-005 | DOCSeye freezes a native authored-information ontology, not a universal normalized AST. | ARCHITECTURAL INFERENCE | A broader object family becomes an explicit DOCSeye authored requirement without stealing a sibling domain. |
| N-006 | Spreadsheet computation, presentation animation, source/app/database state, live DOMs, full temporal media, and arbitrary provider objects remain outside the native core. | ARCHITECTURAL INFERENCE | Owning substrate boundaries are deliberately changed. |
| N-007 | The DND logical format is openly specified and independent conformance requires canonical vectors, validation, preservation/refusal, and a valid independent commit. | ARCHITECTURAL INFERENCE | E-06/E-08 prove the specification model infeasible, which is an architecture falsifier. |
| N-008 | Native v1 has no implicit executable content; parsing/rendering executes no code and fetches no remote content automatically. | ARCHITECTURAL INFERENCE | Never for generation 1. |

## 3. Identity, lifecycle, and time decisions

| ID | Frozen decision | Evidence basis | Reopen only if |
| --- | --- | --- | --- |
| N-009 | Family, branch, revision, public object, and retained-boundary IDs use opaque RFC 9562 UUIDv4, tag-37/16-byte canonical encoding; type is not embedded. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | Collision/privacy/locality measurements show a stronger opaque scheme without adding semantic chronology. |
| N-010 | Content hashes never serve as semantic object IDs; SHA-256 is for state integrity, canonical records, capsules, and immutable asset identity. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | Cryptographic guidance changes or a domain needs another integrity algorithm under an explicit versioned profile. |
| N-011 | Within a resolved family `(branch_id, object_id)` is direct actuation identity; portable writes add `family_id` and `expected_revision_id`. | ARCHITECTURAL INFERENCE | An independent implementation proves a smaller handle equally prevents cross-family/branch/stale writes. |
| N-012 | A second kernel logical-concept ID exists only for real correspondence/lineage information, not ordinary native objects. | ARCHITECTURAL INFERENCE | A concrete native actuation case adds information unavailable from branch/object identity. |
| N-013 | Family membership records authored intent; path and byte equality never define family. | IMPLEMENTATION-DERIVED SIBLING EVIDENCE + ARCHITECTURAL INFERENCE | Never; new carrier evidence may supplement but not replace intent. |
| N-014 | Save/move/rename preserve branch; controlled replica is same-branch snapshot; Save As/explicit fork create a same-family branch; duplicate/template/import rules are explicit. | ARCHITECTURAL INFERENCE | Tested user semantics require another explicit operation, not path inference. |
| N-015 | Every new branch remints every public semantic and retained-boundary ID and records bounded origin mappings. | ARCHITECTURAL INFERENCE | Build evidence shows equivalent cross-branch safety with retained IDs and materially lower permanent cost. |
| N-016 | A raw byte copy initially means another replica of the same branch snapshot; opening at another path does not mutate it. | ARCHITECTURAL INFERENCE | A filesystem/provider supplies explicit authored copy intent in a typed operation. |
| N-017 | Independently changed same-branch replicas observed together are `divergent_heads`; combined write authority stops until explicit fork or merge. | ARCHITECTURAL INFERENCE | Never for Build 001; silent linearization violates identity correctness. |
| N-018 | Within-, cross-branch, and cross-family content copy remints every copied public occurrence/boundary ID; immutable asset digests may be shared; provenance is bounded. | ARCHITECTURAL INFERENCE | A specific value-like object is demoted from public identity by decision amendment. |
| N-019 | Split retires the original and mints every successor; merge retires every input and mints the result for blocks, list items, cells, and regions. | ARCHITECTURAL INFERENCE | A typed object family proves one continuation is semantically inherent, not convenient. |
| N-020 | Retirement resolves as destroyed/split/merged/unknown_retired; bounded witnesses live only while referenced by current anchors/suggestions/deltas/windows/merge bases/relations. | ARCHITECTURAL INFERENCE | A required lifetime cannot be satisfied without a longer bounded retention rule. |
| N-021 | No similarity, embedding, diff, path, or quote may resurrect or authorize a retired/ambiguous target. | IMPLEMENTATION-DERIVED SIBLING EVIDENCE + ARCHITECTURAL INFERENCE | Never. |
| N-022 | A revision tuple contains UUIDv4 commit `revision_id`, branch-local `revision_sequence`, SHA-256 `semantic_root`, and bounded parent witnesses. | ARCHITECTURAL INFERENCE | E-07 shows one field adds no information or prevents independent conformance. |
| N-023 | Semantic root is a domain-separated Merkle-style current-state tree; exact tree mechanics are E-07; no permanent historical Merkle DAG is required. | ARCHITECTURAL INFERENCE + OPEN QUESTION — BUILD 001 EXPERIMENT REQUIRED | E-07 cannot produce deterministic bounded recomputation; mechanism must change or root decision reopen. |
| N-024 | Semantically neutral SQLite/index/record maintenance changes neither root nor DocumentRevision. | ARCHITECTURAL INFERENCE | Never; physical layout is explicitly non-semantic. |
| N-025 | Revision sequence is ordering convenience, not unique causality; divergent children may share it and parents prove bounded ancestry. | ARCHITECTURAL INFERENCE | A simpler causal witness proves equivalent divergence/merge safety. |

## 4. Transactions, text, and ontology decisions

| ID | Frozen decision | Evidence basis | Reopen only if |
| --- | --- | --- | --- |
| N-026 | Every semantic write is typed, branch-scoped, expected-revision conditioned, and all-or-nothing; one logical commit publishes one revision/root/delta. | IMPLEMENTATION-DERIVED SIBLING EVIDENCE + ARCHITECTURAL INFERENCE | Never for Build 001. |
| N-027 | Recovery yields exactly the old or new committed semantic revision; ambiguous response loss uses bounded idempotency query and never blind replay. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | Container mechanism changes while preserving the invariant. |
| N-028 | Deltas are bounded, semantic, and gap-aware; expiry returns `resync_required`, not a silent gap or permanent ledger. | IMPLEMENTATION-DERIVED SIBLING EVIDENCE + ARCHITECTURAL INFERENCE | A provider offers stronger bounded history without changing client correctness. |
| N-029 | Logical order is independent of identity; v1 starts with 128-bit sparse keys, local rebalance, and root-neutral maintenance; E-05 may select a bounded alternative. | OPEN QUESTION — BUILD 001 EXPERIMENT REQUIRED | E-05 requires another mechanism satisfying the same invariants. |
| N-030 | Canonical text is exact valid UTF-8; no silent Unicode normalization; boundaries never split scalars and human operations default to extended grapheme clusters. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | Unicode conformance changes under an explicit format version. |
| N-031 | Ordinary query ranges are revision-local; `retain_range` explicitly creates identity-bearing zero-width boundaries and a semantic revision. Reads never materialize boundaries. | ARCHITECTURAL INFERENCE | E-01 finds a stronger explicit primitive with the same read/no-write and portability properties. |
| N-032 | Range edges use logical include/exclude and collapsed points use before/after insertion; defaults are anchor-type specific and bidi-independent. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | E-01 exposes an inconsistent default, requiring explicit amended semantics before implementation continues. |
| N-033 | Anchor states are live/collapsed/orphaned/destroyed/ambiguous; type-specific delete/replace/move rules are normative and ambiguous never writes. | ARCHITECTURAL INFERENCE | Never without replacing exact-targeting thesis. |
| N-034 | Boundaries may retain their IDs through exact block split/merge owner mappings; allowed range types may become multi-interval; quote similarity never maps. | ARCHITECTURAL INFERENCE + OPEN QUESTION — BUILD 001 EXPERIMENT REQUIRED | E-01 proves a frozen mapping impossible; reopen text mechanism/semantics explicitly. |
| N-035 | Live collaboration, OT, and CRDT persistence are deferred; no permanent per-character operation history exists in v1. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | A real multi-operator requirement or E-01 falsification proves it necessary. |
| N-036 | Public identity belongs to targetable cross-revision semantic objects; roles/direct formatting/coordinates are values; pages/runs/offsets are revision-local derived objects. | ARCHITECTURAL INFERENCE | A type gains a concrete independent lifecycle/operation. |
| N-037 | Lists and list items are first-class; items contain one or more blocks; semantic membership is separate from display numbering/restart rules. | ARCHITECTURAL INFERENCE | A simpler model passes all native/import operations without copying Word numbering ontology. |
| N-038 | Tables have public table/row/column/cell identities and logical regions; coordinates are locations; merge/split follows all-remint lifecycle; deep computation belongs to DATAeye. | ARCHITECTURAL INFERENCE | A measured operation family requires a refined topology object while preserving boundaries. |
| N-039 | Semantic role, named style, theme/token, direct override, and layout/media variant are distinct; v1 uses a small deterministic six-layer resolution, not full CSS. | ARCHITECTURAL INFERENCE | Native authored requirements need a broader but still deterministic style model. |
| N-040 | Threads/comments/replies are native objects with exact targets, author/timestamp, open/resolved state, and orphan behavior; quoted text is display evidence only. | ARCHITECTURAL INFERENCE | Provider support broadens via facets, not by changing target identity. |
| N-041 | Suggestions are current document content of six frozen types, not revision history; accept/reject is typed and resolution leaves no permanent accepted ledger. | ARCHITECTURAL INFERENCE | A new authored suggestion type is explicitly added. |
| N-042 | Fields are bounded typed computations with separated source/dependencies/result/revisions/staleness and explicit evaluation policy; arbitrary code is forbidden. | ARCHITECTURAL INFERENCE | A bounded new class is approved without absorbing CODEeye/DATAeye. |
| N-043 | Native controls are text, rich text, number, boolean, date, enum, repeating group, and object/reference selector; external bindings are inert/typed and DATAeye-bounded. | ARCHITECTURAL INFERENCE | A concrete authored form requirement adds a bounded type. |
| N-044 | Footnotes/endnotes are note objects with reference occurrences; named anchors are aliases over stable identity; cross-references never retarget by similarity; citations remain a minimum abstraction. | ARCHITECTURAL INFERENCE | A provider facet extends, rather than replaces, native semantics. |
| N-045 | Assets and figure occurrences are distinct; embedded assets use SHA-256 content addressing; linked/remote/unresolved states are explicit; parse never fetches. | ARCHITECTURAL INFERENCE | Integrity guidance changes or a new asset state is required. |
| N-046 | Native math authority is Presentation MathML; TeX source and OMML are optional non-authoritative facets; E-20 freezes canonicalization/edit precedence. | CURRENT EXTERNAL EVIDENCE + OPEN QUESTION — BUILD 001 EXPERIMENT REQUIRED | E-20 shows MathML cannot support required native edits independently; choose another bounded math model. |
| N-047 | v1 has no generic native drawing program; SVG is inert asset content; chart occurrence may be native but data/computation belongs to DATAeye; OLE/executables remain inert. | ARCHITECTURAL INFERENCE | A native authored operation requires bounded drawing primitives. |

## 5. Extension, storage, validation, and rendering decisions

| ID | Frozen decision | Evidence basis | Reopen only if |
| --- | --- | --- | --- |
| N-048 | Every extension envelope declares ID, namespace/type/version, encoding/payload/digest, references, coverage, policy, fallback, and required/optional status. | ARCHITECTURAL INFERENCE | E-04 finds a smaller envelope with equal safe intersection behavior. |
| N-049 | Extension coverage kinds are object/property/subtree/text-interval/relation/topology/layout/document; policies are independent/move-with-target/generic-transform/invalidate/must-understand. | ARCHITECTURAL INFERENCE | E-04 requires a revised operational vocabulary before implementation continues. |
| N-050 | Unknown payloads preserve exact bytes/canonical values; disjoint edits may proceed; default unknown intersection refuses; required unsupported major capability removes write authority. | ARCHITECTURAL INFERENCE | Never without a replacement that prevents silent loss/scope escape. |
| N-051 | Canonical logical records use the frozen RFC 8949 deterministic-CBOR DOCSeye profile and independent byte-exact vectors. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | E-06 falsifies independent deterministic implementation; select another open canonical serializer. |
| N-052 | SQLite is the application-file/transaction container, not the semantic ontology; pure ZIP/directory/append-log/DAG/relational-ontology alternatives are rejected. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | E-02/E-03 falsify the application-file profile and another container passes all invariants. |
| N-053 | Portable DND uses rollback-journal `DELETE` mode and quiescent one-file publication; rebuildable external runtime/index SQLite uses WAL/FTS. | CURRENT EXTERNAL EVIDENCE + OPEN QUESTION — BUILD 001 EXPERIMENT REQUIRED | E-02/E-03 select a safer mode/profile while preserving portability and old/new atomicity. |
| N-054 | DND contains all current truth and durable round-trip correspondence; FTS/embeddings/caches/process/physical registries are external and deletable. | ARCHITECTURAL INFERENCE | Never for native truth; a durable provider facet may be moved in-artifact explicitly. |
| N-055 | `.dnd` and `application/vnd.docseye.native+sqlite` are provisional Build 001 dispatch identifiers, not semantic inputs or final registration. | ARCHITECTURAL INFERENCE | Public collision/MIME review selects a final name. |
| N-056 | Validation strata are container, serialization/schema, identity/reference, semantics, extension, asset/capsule, root, then provider; lower failure removes write authority and repair is explicit. | ARCHITECTURAL INFERENCE | A validator proves a reordered layer preserves the same authority boundary. |
| N-057 | Parser resource limits, no XXE/traversal/bombs/implicit fetch/active execution, and no silent identity mint on validation failure are intrinsic format safety. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | Limits may be raised by measurement; safety invariant never. |
| N-058 | Native layout intent is semantic; `LayoutRevision` qualifies semantic revision/profile/provider/version/fonts/locale/hyphenation/config; pages/regions are layout-scoped. | ARCHITECTURAL INFERENCE | A fixed-layout native document type is added separately. |
| N-059 | DOCSeye owns semantic-to-layout/render contracts and mappings; existing engines render; Typst is first paginated candidate and HTML/Chromium the continuous provider. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | E-12/E-14 choose another provider while preserving ownership. |
| N-060 | PDF is derived and source-attributed; Build 001 requires independently validated tagged PDF/UA-1 and 100% required-object source mapping, with typography capabilities honestly reported. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | E-13 proves the profile impossible across qualified existing providers, reopening the render gate. |

## 6. Interoperability, stack, and Build decisions

| ID | Frozen decision | Evidence basis | Reopen only if |
| --- | --- | --- | --- |
| N-061 | DOCX import uses raw OPC/OOXML plus Open XML SDK to map native concepts, scoped facets/opaque objects, optional exact source capsule, correspondence, and feature report; unsupported content never disappears. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | E-15 changes the mechanism while preserving authority/preservation. |
| N-062 | DOCX export uses six semantic outcomes and separate observation evidence; Word is optional Microsoft-specific infrastructure and native A–D/X acceptance is formally Word-free; ODF is later and LibreOffice is X-04 alternate evidence. | CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | E-16 falsifies practical Word-free interop, requiring explicit DOCX-first/native-later reconsideration. |
| N-063 | Kernel baseline is .NET 10/C#; Node 24 Program Host is non-agentic; providers are SQLite/CBOR, Typst, pinned Chromium, Open XML SDK/raw OPC, LibreOffice smoke, and SHELLeye; preflight pins latest stable supported patches, not report-incidental versions. | FACT + CURRENT EXTERNAL EVIDENCE + ARCHITECTURAL INFERENCE | Platform support or early experiments require a deliberate compatible substitution. |
| N-064 | Replacement Build 001 is exactly 20 experiments, 32 native C cases, 4 DOCX X cases, and one 96-call Host run with 48 mutations/16 families/3 commits/6 delta calls; source/fixtures/packages/results do not exist at freeze. | ARCHITECTURAL INFERENCE | Only an explicit preimplementation decision amendment or a falsifier; never padding or implementation convenience. |

## 7. Supersession of the old 50 decisions

The old decisions remain immutable at the DOCX-first baseline SHA. This table adjudicates, rather than erases, each one.

| Old ID | Treatment | Canonical successor | Reason |
| --- | --- | --- | --- |
| OLD-D-001 | REPLACE | N-001 | product center moves from provider-federated correspondence to native authority |
| OLD-D-002 | DEMOTE TO INTEROP | N-003, N-004, N-061 | provider truth remains only in foreign-managed/provider scopes |
| OLD-D-003 | KEEP | N-005, N-006 | no universal AST remains foundational |
| OLD-D-004 | AMEND | N-009–N-012 | native IDs are intrinsic; external concept IDs only where informative |
| OLD-D-005 | DEMOTE TO INTEROP | N-003, N-061 | no hidden provider injection remains a foreign-managed/import rule |
| OLD-D-006 | DEMOTE TO INTEROP | N-054, N-061 | full DOCX index is provider/runtime behavior, not native truth |
| OLD-D-007 | KEEP | N-036 | public identity remains selective and useful |
| OLD-D-008 | AMEND | N-021, N-026, N-033 | native resolution states replace correspondence-specific vocabulary while exact-only write remains |
| OLD-D-009 | DEMOTE TO INTEROP | N-004, N-061 | provider IDs remain scoped evidence |
| OLD-D-010 | AMEND | N-013–N-017 | family/branch/replica semantics are now intrinsic and explicit |
| OLD-D-011 | REPLACE | N-014–N-018 | Save As/fork/duplicate now use the full-remint family rules |
| OLD-D-012 | AMEND | N-022–N-028, N-058 | native revision/root becomes central; provider/layout clocks remain separate |
| OLD-D-013 | KEEP | N-036, N-058 | runs/offsets/pages/observations remain revision-local |
| OLD-D-014 | REPLACE | N-030–N-034 | native retained boundaries replace provider/history-based anchors |
| OLD-D-015 | AMEND | N-019–N-021 | all affected split/merge results remint with bounded lineage |
| OLD-D-016 | DEMOTE TO INTEROP | N-037, N-061 | OOXML numbering becomes provider mapping; native lists are first-class |
| OLD-D-017 | AMEND | N-038 | native rows/columns/cells/regions have intrinsic identity |
| OLD-D-018 | AMEND | N-039 | native deterministic styles replace provider-only effective projection |
| OLD-D-019 | KEEP | N-040, N-041 | comments/suggestions remain content, not revision history |
| OLD-D-020 | KEEP | N-042 | field source/result/staleness separation retained and strengthened |
| OLD-D-021 | DEMOTE TO INTEROP | N-031, N-044, N-061 | native anchors own identity; controls/bookmarks remain provider evidence |
| OLD-D-022 | AMEND | N-045–N-047 | asset/figure distinction retained; MathML replaces OMML as native truth |
| OLD-D-023 | KEEP | N-058–N-060 | layout/render revisions remain separate |
| OLD-D-024 | KEEP | N-060 | PDF remains derived/fixed-layout provider output |
| OLD-D-025 | KEEP | N-026 | expected-revision typed transaction retained |
| OLD-D-026 | KEEP | N-026, N-027 | multi-operation atomic commit retained |
| OLD-D-027 | KEEP | N-021, N-026 | only exact deterministic behavior may write |
| OLD-D-028 | AMEND | N-026, N-053, N-056 | revalidate authoritative native/container state before commit |
| OLD-D-029 | AMEND | N-048–N-050, N-061 | semantic effect plus extension/provider preservation envelopes |
| OLD-D-030 | DEMOTE TO INTEROP | N-061, N-062 | smallest safe OPC/XML patch remains DOCX-provider behavior |
| OLD-D-031 | KEEP | N-049, N-050, N-062 | unsafe unknown intersection still refuses/routes |
| OLD-D-032 | KEEP | N-021 | diff never proves identity |
| OLD-D-033 | DEMOTE TO INTEROP | N-003, N-061 | provider snapshot/recovery remains foreign-managed/import behavior |
| OLD-D-034 | KEEP | N-028 | bounded gap-aware deltas retained |
| OLD-D-035 | REPLACE | N-052–N-054 | SQLite now also carries native truth; external WAL remains rebuildable operating state |
| OLD-D-036 | AMEND | N-053, N-063 | use latest stable supported preflight patch and test; do not freeze incidental version |
| OLD-D-037 | REPLACE | N-001, N-064 | Build 001 editable authority becomes DND; DOCX is bounded supplement |
| OLD-D-038 | REPLACE | N-062 | Word changes from mandatory to optional Microsoft-specific provider |
| OLD-D-039 | DEMOTE TO INTEROP | N-061, N-063 | Open XML SDK remains provider layer under preflight version rule |
| OLD-D-040 | AMEND | N-063 | .NET/Node retained; native container/render/interop components added |
| OLD-D-041 | AMEND | N-063, N-064 | Host must use typed native SDK; raw native/provider escapes do not count |
| OLD-D-042 | REPLACE | N-064 | 30 experiments replaced by exact 20 native freeze experiments |
| OLD-D-043 | REPLACE | N-064 | old 24-concept A replaced by exact 32-sentinel native A |
| OLD-D-044 | REPLACE | N-064 | old 54 combined cases replaced by 32 native + 4 separate provider cases |
| OLD-D-045 | REPLACE | N-064 | old 60-call workflow replaced by enumerated 96-call native workflow |
| OLD-D-046 | KEEP | N-064 | freeze remains documentation-only and unimplemented |
| OLD-D-047 | KEEP | N-013, N-053 | SHELLeye physical ownership remains separate |
| OLD-D-048 | KEEP | N-005, N-058 | DESKTOPeye UI ownership remains separate |
| OLD-D-049 | KEEP | N-005, N-006 | CODEeye/eyeBROWSE ownership remains separate |
| OLD-D-050 | KEEP | N-006, N-038, N-047 | DATAeye/presentation boundaries remain separate |

Treatment totals: **KEEP 18 / AMEND 13 / DEMOTE TO INTEROP 9 / REPLACE 10 / DELETE 0 = 50**.

No old principle is deleted casually. Decisions replaced as Build cardinalities or authority rules remain historically available at `47b5280b71773ae497b5a56c4b590b9070b4bca5`.

## 8. Decision discipline

Implementation findings are classified as:

1. mechanism selection inside a frozen invariant;
2. provider/capability limitation with honest refusal/report;
3. version-qualified behavior;
4. architecture falsifier requiring stop and numbered amendment.

Only item 4 reopens this register. Convenience, deadlines, majority report preference, or a library default do not.
