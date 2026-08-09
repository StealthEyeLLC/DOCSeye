# 06 — Frozen Decision Register

Status: **CANONICAL / FROZEN FOR BUILD 001**

## 1. Reading this register

Every decision below is active. “Frozen” means implementation must build against the semantic choice. A bounded experiment may choose a mechanism inside the decision's constraints. Reopening a decision requires an explicit canonical-doc change; a surprising library behavior is not permission to weaken correctness silently.

Evidence classes:

- **P** — primary specification or official API evidence;
- **S** — sibling repository evidence;
- **F** — first-principles architectural inference;
- **R** — convergence of the three independent reports;
- **E** — implementation experiment still required for the mechanism.

## 2. Permanent architecture decisions

| ID | Frozen decision | Evidence | Confidence | Rationale and rejected alternatives | Reopen only if |
| --- | --- | --- | --- | --- | --- |
| D-001 | DOCSeye is a persistent, preservation-first, provider-federated document correspondence and transaction substrate. | R/F | Very high | This uniquely combines agent continuity, exact mutation, provider truth, preservation, and local computation. Reject editor, wrapper, converter, and document DB definitions. | A simpler architecture proves equal identity, fidelity, layout, recovery, and efficiency under the hostile suite. |
| D-002 | Providers own their native representation truth; DOCSeye owns conservative correspondence and temporal coherence. | P/R/F | Very high | OOXML, live Word, PDF, layout, render, and filesystem expose incompatible authorities. Reject a single master object tree. | A provider-independent representation is shown to preserve all native truth and behavior without extension escape hatches. |
| D-003 | There is no universal canonical document AST. | P/R/F | Very high | A universal serialization model necessarily omits or normalizes richer provider structures. A small common correspondence protocol plus native facets is retained. | Lossless cross-format round-trip and native-operation equivalence are demonstrated for the full required feature sets. |
| D-004 | DOCSeye logical IDs are external and opaque by default. | P/R/F | Very high | Native IDs have different scopes and copy/export behavior. Reject physical path, hashes, `docId`, offsets, or XML nodes as logical identity. | A universal provider-native identity contract exists across all retained formats and branch operations. |
| D-005 | DOCSeye does not inject hidden IDs by default. | P/R/F | High | Injection is a mutation, clones on copy, can affect signatures, and is not portable. Existing intentional controls/bookmarks/bindings remain strong witnesses. | A managed-document mode proves explicit user-visible benefits and safe copy/signature semantics; it would be a separate operation mode. |
| D-006 | Build a full compact structural index for every current DOCX revision. | R/F | High | Documents are finite and deterministically enumerable; this minimizes model rediscovery. Reject sparse-only UI-style observation. | Large-document measurements show the compact index is impractical and a measured hybrid retains query guarantees. |
| D-007 | Promote only useful, evidence-backed concepts to durable cross-revision identity. | R/F | Very high | Full indexing and durable identity solve different problems. Reject permanent IDs for every run, XML node, coordinate, result, or OCR word. | An object family gains a documented or operation-native lifetime with concrete cross-turn value. |
| D-008 | Every retained binding resolves as exact_current, exact_rebased, stale, ambiguous, destroyed, or inferred; only exact states authorize mutation. | R/F/S | Very high | Persistence must not manufacture certainty. Reject fuzzy fallback and “best candidate” writes. | Never for Build 001; changing this would invalidate the central safety thesis. |
| D-009 | Provider-native IDs are object-specific scoped evidence, not DOCSeye IDs. | P/R/F | Very high | `paraId`, `textId`, `docId`, comment durable IDs, control IDs, bookmarks, Word live IDs, and PDF references have distinct scopes. | A specific object's documented lifetime expands; only that evidence rule may be updated. |
| D-010 | Logical document identity is authored-branch identity, not path or physical file identity. | P/R/F/S | Very high | Controlled saves can replace files; unrelated files can occupy the same path. | A new provider supplies stronger immutable branch semantics, added as evidence rather than replacing the abstraction. |
| D-011 | Rename/move preserves the document; controlled save preserves it; copy and Save As create a new document with DERIVED_FROM; DOCX-to-PDF creates a derivative. | P/R/F | High | This prevents cloned resident/provider IDs from collapsing branches. Reject path-continuation and family-ID identity. | Explicit operation semantics prove a Save As was actually a move, or future user-visible branch semantics require an explicit alternative operation. |
| D-012 | Temporal state uses separate document, representation, provider, layout, render, world-sequence, and physical-file clocks. | P/R/F | Very high | Unsaved Word, disk, fields, pagination, and render can be out of phase. Reject one universal revision and cargo-cult incarnation for every object. | A formal reduction preserves every coherence state and stale-write rule without ambiguity. |
| D-013 | Runs, absolute offsets, ordinals, coordinates, DOCX pages, rendered lines, search hits, and OCR words are revision-local by default. | P/R/F | Very high | These are representation locations or observations, not stable semantic identity. | An individual provider gives one type documented stable identity and it has useful retained operations. |
| D-014 | Retained spans use exact parent identity, named text projection, boundary affinity, native markers where present, contextual witnesses, and deterministic transform history. | P/R/F | Very high | Offset-only and phrase-only anchors fail duplicate-content adversaries. | A stronger native range primitive persists across cold restart with documented scope. |
| D-015 | Split and merge retire unrestricted old whole-object write authority and return explicit lineage/new concepts. | R/F | Very high | Choosing an arbitrary child or merged successor creates wrong-target capability. | A typed operation has an explicit continuation contract whose semantics are unambiguous. |
| D-016 | Lists are semantic projections over paragraph identity plus numbering truth; displayed markers are never identity. | P/R/F | Very high | OOXML numbering is computed from definitions, levels, instances, and overrides. | The provider format natively defines a stronger logical list object; add a provider facet. |
| D-017 | Tables expose physical cells and logical grid regions; row/column coordinates and visible values are locations/evidence only. | P/R/F | Very high | Merges, grid omissions, duplicate rows, and reordering defeat coordinates. | A provider documents stable row/cell keys across the tested lifetime. |
| D-018 | Styles preserve definitions and inheritance; effective formatting is a provider-identified computed projection. | P/R/F | High | Direct, inherited, numbering, table, theme, and application behavior differ. Reject storing effective formatting as source truth. | Provider behavior can be represented losslessly by a simpler frozen cascade. |
| D-019 | Comments and native tracked changes are first-class native document objects, separate from DOCSeye revision history. | P/R/F | Very high | Their anchors, threads, authors, states, and structural markup carry real semantics. Reject decoration/flat-text treatment and permanent accepted-change ledger. | Never for Build 001; provider support may broaden. |
| D-020 | Field instruction, cached result, staleness, and provider recalculation are separate facts. | P/R/F | Very high | A cached result can be stale; layout fields require a qualified engine. | A provider format has a different native field model, represented in its facet. |
| D-021 | Existing content controls and bookmarks are strong scoped anchors; tags/names alone are not identity. | P/R/F | Very high | Control IDs are strong in Word scope, while duplicate tags/names and recreation exist. Reject blanket hidden-control injection. | Experiments refine strength but cannot make tag/name alone sufficient. |
| D-022 | Figure occurrence and media asset are different concepts; OMML remains native Word math truth; opaque embeddings are preserved but not executed. | P/R/F | Very high | Asset reuse, placement, crop, wrapping, and equation structure are independent. Reject hash-as-figure ID or flattened equation text. | A provider's native model requires another facet, not collapse. |
| D-023 | Layout and render have provider/configuration-specific revisions separate from semantic revisions. | P/R/F | Very high | Reflow invalidates pages while semantic objects survive. Reject page number as semantic identity. | Never for reflowable formats; fixed-layout pages remain provider-native objects under their representation revision. |
| D-024 | PDF is a later fixed-layout/tagged/inference provider, not a DOCX-shaped editing model. | P/R/F | Very high | Tagged structure, content order, pages, object references, annotations, and OCR assurance differ fundamentally. | Common-core pressure tests show even document/representation/correspondence protocols are harmful; then split the substrate. |

## 3. Mutation, preservation, and recovery decisions

| ID | Frozen decision | Evidence | Confidence | Rationale and rejected alternatives | Reopen only if |
| --- | --- | --- | --- | --- | --- |
| D-025 | The fundamental mutation unit is one expected-revision typed document transaction over one pinned coherent provider snapshot. | R/F | Very high | Resolve-then-reopen by text/path permits target substitution. Reject unversioned individual edits. | Never for Build 001. |
| D-026 | Multi-operation transactions are all-or-nothing and publish one DocumentRevision per logical commit. | R/F | High | Agent bulk work needs coherent concurrency and compact deltas. Reject one revision per low-level XML edit. | A future collaborative provider requires a documented compound-revision mapping while preserving atomic caller semantics. |
| D-027 | Automatic rebase is exact and deterministic only; otherwise return stale/conflict/ambiguous. | P/R/F | Very high | DOCSeye lacks an omniscient collaborative transformation server and cannot guess duplicate targets. | Never for Build 001. |
| D-028 | Revalidate authoritative provider and physical state immediately before commit. | R/F/S | Very high | A correct base can become stale during transaction preparation. Reject blind overwrite. | A provider supplies equivalent atomic compare-and-swap inside its commit, which satisfies rather than removes this requirement. |
| D-029 | Every mutation has an independent semantic-effect envelope and serialization footprint. Both must pass. | P/R/F | Very high | Semantic correctness does not guarantee native preservation and minimal bytes do not guarantee target correctness. | Never for Build 001. |
| D-030 | Untouched package entry payloads are copied unchanged; touched XML uses the smallest safe exact patch or bounded subtree reconstruction preserving unknown/MC content. | P/R/F/E | High | Reject subset regeneration and whole-package serializer normalization. Lexical preservation is preferred, not universally assumed. | Experiments identify a safer provider-native mechanism that meets the same contract. |
| D-031 | If unsupported/unknown structure intersects an edit boundary that cannot be preserved, reject or route to a qualified provider. | R/F | Very high | Powerful but lossy editing violates the preservation thesis. | Provider capability improves enough to prove the operation contract. |
| D-032 | Semantic diff/change detection never proves identity by itself. | R/F | Very high | “Removed Approved / added Approved” is not succession evidence. Reject diff- or embedding-authorized writes. | Never for Build 001. |
| D-033 | External recovery first obtains a stable coherent provider snapshot, then indexes, binds strong keys/operation lineage, attempts exact structural correspondence, and classifies the rest. | P/R/F/S | Very high | File events can be partial and similarity candidates unsafe. | Provider offers a stronger atomic change feed; incorporate it as evidence. |
| D-034 | Deltas are bounded and gap-aware; expired cursors return `resync_required`. | R/F | Very high | Silent gaps corrupt agent state; a permanent ledger is unnecessary. | A future provider supplies durable history, which remains provider truth rather than changing the DOCSeye contract. |
| D-035 | SQLite WAL plus FTS5 stores operating correspondence/current indexes/bounded deltas, not canonical full document truth or permanent history. | P/R/F | High | Relational current state fits the workload; graph/event-store machinery adds divergence risk. | Measured Build 001 workload demonstrates a concrete inability, accompanied by a migration decision. |
| D-036 | Pin a corrected SQLite version and record it. | P | Very high | Avoid the documented WAL-reset defect in affected builds. | Never; the exact corrected version may advance. |

## 4. Provider, stack, and Build 001 decisions

| ID | Frozen decision | Evidence | Confidence | Rationale and rejected alternatives | Reopen only if |
| --- | --- | --- | --- | --- | --- |
| D-037 | Build 001 editable format is modern Word-generated Transitional DOCX; PDF is derived render only. | R/F | Very high | DOCX exercises the richest decisive text-document semantics without a false multi-format slice. | Build 001 is replaced by a separately approved thesis before implementation begins. |
| D-038 | Build 001 includes a mandatory isolated Word native/layout provider, but Word is not sole semantic, identity, or preservation authority. | P/R/F | Very high | Real Word compatibility/layout/native behavior are decisive; process lifetime and normalization must not own the kernel. | Target-host preflight proves Word unavailable, requiring an explicit scope/freeze change—not a silent omission. |
| D-039 | Open XML SDK 3.5.1 is the typed interpretation/validation layer, with direct OPC/XML preservation machinery beside it. | P/R/F | High | SDK leverage is strong; typed serialization/MC processing is not a fidelity theorem. | A later compatible SDK is deliberately pinned or experiments justify another provider library while preserving contracts. |
| D-040 | C#/.NET 10 is the kernel baseline; Node 24 is the non-agentic Program Host baseline. | P/R/F | High | This combines Microsoft document ecosystem leverage with compact local programming. | Platform support or measured implementation results justify a deliberate migration. |
| D-041 | The Program Host uses the real typed DOCSeye SDK; raw ZIP, giant OpenXML scripts, or out-of-kernel Word/VBA do not count. | R/F | Very high | Otherwise a single call could conceal the architecture rather than prove it. | Never for Build 001 acceptance. |
| D-042 | Build 001 has exactly 30 bounded experiments. They resolve mechanisms and do not reopen frozen semantics. | R/F | High | The reports identified concrete uncertainties; a finite manifest prevents research drift. | A new blocking mechanism uncertainty is added by canonical decision before implementation and the exact count is deliberately revised. |
| D-043 | Milestone A retains exactly 24 concepts; unchanged restart is 24 exact; observer gap is 14 exact, 6 stale/destroyed, 4 ambiguous. | R/F | High | A fixed oracle proves useful recovery and conservative failure. | Fixture construction demonstrates an internal inconsistency before implementation, requiring explicit amendment. |
| D-044 | Milestone C contains exactly 54 deterministic hostile cases and every required zero metric is absolute. | R/F | Very high | The count is the deduplicated union of distinct failure modes. Statistical success is inappropriate for wrong-target and preservation safety. | Only an explicit canonical manifest revision before the affected implementation. |
| D-045 | Milestone D is exactly 60 meaningful typed calls: 29 mutations, 15 object families, two commits, two deltas, one external reconciliation, layout wait/render, and zero intermediate model calls. | R/F | High | It reaches the research target and proves dense local programmability without hidden raw scripts. | Only an explicit pre-implementation workflow amendment preserving or strengthening the proof. |
| D-046 | This freeze is documentation-only; source, fixtures, dependencies, runtime artifacts, and results document do not exist yet. | F | Very high | Architecture completion must not be misreported as product completion. | Implementation begins in a later authorized pass; status must then change truthfully. |

## 5. Cross-substrate decisions

| ID | Frozen decision | Evidence | Confidence | Rationale and rejected alternatives | Reopen only if |
| --- | --- | --- | --- | --- | --- |
| D-047 | SHELLeye owns physical files and atomic replacement; DOCSeye owns logical document identity and semantic mutation. | S/F | Very high | Path/file semantics and authored-document semantics are not interchangeable. | Sibling contracts change explicitly. |
| D-048 | DESKTOPeye owns app/window/focus/UI state; DOCSeye owns semantic ranges and provider-native document operations. | S/F | Very high | A caret or Word window is not a durable semantic target. | Sibling contracts change explicitly. |
| D-049 | CODEeye owns source/engineering semantics; eyeBROWSE owns live web state; DOCSeye may correlate static document representations. | S/F | High | Ownership follows semantic operation, not filename extension. Reject a universal StealthEye graph. | A cross-substrate protocol is explicitly frozen later. |
| D-050 | Spreadsheet and presentation semantic cores remain future DATAeye/presentation-substrate concerns. | P/R/F | Very high | Shared OPC packaging is implementation reuse, not ontology equivalence. | Those sibling substrate boundaries are deliberately redefined. |

## 6. Decision discipline

An implementation finding should be classified as one of:

1. **mechanism refinement** — choose a documented option inside a frozen decision;
2. **capability limitation** — return unsupported while preserving frozen safety;
3. **provider-version profile** — record current behavior without generalizing it;
4. **architecture contradiction** — stop and propose a numbered decision amendment.

Only item 4 reopens this register. Implementation convenience, deadline pressure, or library defaults do not.
