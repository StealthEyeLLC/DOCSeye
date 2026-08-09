# 00 — Charter

Status: **CANONICAL / FROZEN FOR BUILD 001**

## 1. Product thesis

DOCSeye is ChatGPT's native persistent authored-information substrate.

It gives ChatGPT durable, exact, revision-aware access to authored document semantics across turns, process loss, carrier movement, derived renders, and bounded provider interchange. For a native document, the portable artifact—not a process, path, index, browser tree, Word session, or model memory—originates semantic identity and current truth.

DOCSeye is not a visual word processor. It is not a universal document converter, a giant cross-substrate graph, a permanent action ledger, an agent swarm, or a wrapper around Microsoft Word.

## 2. Primary operator and execution model

ChatGPT is the intended operator. It plans against typed objects and revision-conditioned transactions. A local, non-agentic Program Host executes dense workflows with no embedded model and returns typed results, deltas, capability reports, refusals, and render references.

The model may reason; the kernel must prove. No verification-agent architecture is permitted. Independent validators and independent format implementations are test or provider components, not additional decision-making agents.

## 3. Persistent-world invariants

1. **Artifact-originated identity.** Native family, branch, revision, object, and retained-boundary identity travel in the DND.
2. **Exact targeting.** A write requires an exact live target in the expected branch and revision or returns a conservative non-write classification.
3. **False rebound is worse than ambiguity.** Duplicate text, similar structure, a familiar path, or matching bytes never authorizes semantic continuity by itself.
4. **Authority is explicit.** One state has one editable semantic authority. Native mode and foreign-managed mode cannot silently overlap.
5. **Transactions are old-or-new.** An acknowledged commit is a whole semantic revision. A partial acknowledged revision is impossible by contract.
6. **Operating state is rebuildable.** Native truth does not depend on FTS, embeddings, caches, process bindings, or the Program Host database.
7. **Deltas are bounded and gap-aware.** A missing cursor produces an explicit resynchronization requirement, never a silent omission.
8. **History is bounded.** Current state, current references, active suggestions, merge bases, recent deltas, and required retirement witnesses may persist; an unbounded action ledger is not required.
9. **Derived layout does not own semantics.** Pages, shaped runs, regions, HTML, and PDF are scoped to a layout or render revision.
10. **Unknown meaning is protected.** Unknown extensions are preserved and their declared coverage controls whether an edit may proceed.

## 4. Authored-information boundary

The native ontology covers reflowable authored documents: structured flows and blocks, text, lists, tables, relations, styles, review content, computed fields, controls, notes, figures, assets, math, layout intent, and bounded extensions.

It does not attempt to normalize every foreign representation. In particular, the native core does not absorb:

- spreadsheets as full computational models;
- presentation animation and slide-show semantics;
- source repositories and executable applications;
- arbitrary database state;
- live browser DOMs and sessions;
- full temporal audio/video models;
- every OOXML/ODF/PDF extension or object;
- embedded scripts, macros, OLE execution, or arbitrary field code.

Foreign features that do not fit honestly remain scoped provider facets, opaque objects, inert source-capsule content, or explicitly unsupported capabilities.

## 5. Cross-substrate ownership

| Substrate | Owns | DOCSeye interaction |
| --- | --- | --- |
| SHELLeye | physical carrier identity, coherent file snapshots, movement, copy, and atomic publication | DOCSeye binds but never substitutes physical identity for family/branch/object identity |
| DESKTOPeye | windows, focus, caret, app UI, and user-visible takeover | DOCSeye may coordinate a provider session but does not derive semantic identity from UI state |
| CODEeye | repository, source-file, symbol, and engineering semantics | DOCSeye may embed or link a static representation; executable/source authority stays with CODEeye |
| DATAeye | computational tables, datasets, query models, and data binding | DOCSeye owns authored table presentation and bounded fields/controls, not deep computation |
| eyeBROWSE | live web documents, DOM state, sessions, and browser actions | DOCSeye owns derived/static HTML representations only |
| MEDIAeye | full temporal media and media editing semantics | DOCSeye owns figure occurrences and asset references, not the media timeline |

Shared identifiers may express correspondence. They do not erase domain ownership.

## 6. Interoperability promise

DOCX and future ODF providers are explicit mappings, not native truth in native mode. Import reports what was represented, faceted, preserved, or blocked. Export reports translation outcome separately from which application or validator observed the result. Microsoft Word remains optional Microsoft-specific fidelity infrastructure; it is not a hidden dependency of native acceptance.

Foreign-managed DOCX remains available for users who choose not to convert. In that mode DOCX is authoritative and the historical DOCX-first correspondence rules apply. Conversion is explicit and irreversible as an authority transition unless the user later exports a new provider representation.

## 7. Open format and stewardship

The DND logical format is openly specified. A conforming independent implementation must be able to decode canonical records, validate identities/references/root, preserve unsupported optional extensions, refuse unsafe required extensions, produce the published deterministic vectors, make a valid independent commit, and survive round-trip through the primary implementation without semantic drift.

SQLite is the application-file/container substrate and deterministic CBOR is the canonical logical record encoding. Neither SQLite page layout nor the initial SQL schema is the public semantic ontology.

## 8. Build discipline

Build 001 proves or falsifies the permanent native core. It does not build a visual editor, collaboration system, universal foreign-format translator, Word clone, or typography engine. Architecture-threatening experiments run early. If a frozen invariant is falsified, implementation stops and reopens only the affected architectural question; it does not quietly weaken the invariant.

Build 001 remains planned and unimplemented. Its exact scope is in [02-BUILD-001-SLICE.md](02-BUILD-001-SLICE.md).
