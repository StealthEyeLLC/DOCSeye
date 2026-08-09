# 05 — Research Baseline

Status: **CANONICAL SYNTHESIS TRACE / CURRENT THROUGH 2026-08-09**

This file preserves traceability without treating reports as votes or proposed experiments as results.

Evidence vocabulary used throughout the canonical set:

- **FACT** — directly observed repository, file, issue, Git, or host state;
- **CURRENT EXTERNAL EVIDENCE** — current primary specification/official-source evidence;
- **IMPLEMENTATION-DERIVED SIBLING EVIDENCE** — measured accepted sibling-build behavior;
- **ARCHITECTURAL INFERENCE** — conclusion derived from invariants/tradeoffs;
- **OPEN QUESTION — BUILD 001 EXPERIMENT REQUIRED** — a mechanism not yet measured, with invariant frozen;
- **SPECULATION** — unverified possibility, never an acceptance claim;
- **REJECTED** — evaluated and not selected;
- **SUPERSEDED** — formerly canonical and retained in history, but no longer current.

## 1. Repository facts and original baseline

**FACT:** before this synthesis, `main` was exactly `47b5280b71773ae497b5a56c4b590b9070b4bca5`. It had no later commits. That SHA completed the architecture freeze established by `f0b094…` and repaired the canonical rendering/freeze metadata.

**FACT:** the repository contained only `.gitignore`, `README.md`, canonical docs 00–08, and `docs/AUTHORITY.md`. There was no product source, fixture, package manifest, database, result document, or Build 001 implementation.

**FACT:** issues #1–#5 were open, had no labels/milestone/assignees/comments, and tracked the DOCX-first 30-experiment, 24-concept, 54-case, 60-call Build 001.

The old design followed three independent DOCX-centered passes and a synthesis. Its strongest ideas remain valuable: provider authority, exact correspondence, representation clocks, revision-conditioned atomic mutations, smallest-safe provider patches, preserved unknown content, separate layout/render revisions, rebuildable operating state, local Program Host execution, and conservative stale/ambiguous outcomes.

The native-format challenge was opened because the old design kept the most important authored-document identities and spans outside the portable artifact. It had to re-establish continuity across DOCX rewrites/provider behavior and made desktop Word mandatory for the frozen slice. The challenge asked whether moving authored authority into an open native artifact created permanent leverage large enough to justify stewardship and interop costs before implementation began.

## 2. Four independent native-format inputs

All four supplied reports were accessible in full. Neutral labels follow their order in the supplied bundle; they are evidence, not ballots.

### 2.1 Recommendation and load-bearing model

| Report | Final recommendation / architecture name | Load-bearing argument | Word conclusion |
| --- | --- | --- | --- |
| R1 | **NATIVE-FIRST WITH PRESERVED PROVIDER CAPSULES** | Intrinsic portable identity/transactions/anchors remove permanent correspondence tax while capsules preserve foreign evidence. | Not required for native/render/PDF/package work; only Microsoft-specific fidelity/oracle. |
| R2 | **Native semantic authority with asymmetric preserved provider facets** | One semantic authority plus asymmetric facets avoids dual truth and enables stable native operations. | Optional provider; native acceptance should be Word-free. |
| R3 | **NATIVE-AUTHORITATIVE / FACETED INTEROP** | Native state owns current semantics; provider facets/capsules preserve untranslatable meaning and foreign-managed mode preserves unconverted utility. | Optional later Microsoft provider/oracle, not native dependency. |
| R4 | **NATIVE AUTHORITY WITH PRESERVED INTEROPERABILITY CAPSULES** | Portable intrinsic identity, roots, retained boundaries, extension contracts, and derived layout make the core durable and independently implementable. | Hard Word-free native gate; Word later and qualified. |

### 2.2 Identity, branch, revision, and anchors

| Report | Identity | Branch/fork | Revision | Text/anchor model |
| --- | --- | --- | --- | --- |
| R1 | UUIDv7; branch-scoped compound identity | fork creates branch and may retain local object IDs | SHA-256 canonical-state identity; optional current Merkle tree; no permanent DAG | sparse first-class zero-width boundaries; BEFORE/AFTER |
| R2 | UUIDv4; family/branch/object tuple | fork/Save As retains object IDs; cross-branch paste remints | sequence + UUIDv4 revision + previous revision; explicitly no root/Merkle in v1 | text chunks plus explicit retained boundary objects; include/exclude edge policy |
| R3 | UUIDv4 for family/branch/revision/object/boundary | fork remints all branch-local realization IDs with `origin_ref` | UUID revision + sequence + SHA-256 Merkle semantic root + bounded parents | stable UTF-8 pieces/boundaries; multi-interval anchors; explicit edge policy |
| R4 | UUIDv7 | fork/new branch may retain local IDs; compound branch identity | sequence + opaque revision bound to parents/state root; SHA-256 root | immutable segments plus boundary records; logical inside/outside affinities |

### 2.3 Storage, rendering, interoperability, and Build 001 proposals

| Report | Storage / serialization | Rendering | DOCX interoperability | Build 001 thesis | Experiments | Hostile suite | Program Host |
| --- | --- | --- | --- | --- | ---: | ---: | --- |
| R1 | SQLite app file; rollback artifact/WAL runtime; deterministic CBOR; hashed assets | Typst paginated + HTML/browser | original capsule, scoped mapping/fidelity report | portable identities/anchors/extensions, runtime loss, Word-free render, bounded DOCX | 16 | 40 combined | 100 calls; ≥50 mutations; ≥18 families; 3 commits |
| R2 | SQLite rollback + deterministic CBOR; SHA assets; no v1 root | Typst + HTML/browser | asymmetric facets and two-axis fidelity/evidence | native authority, stable anchors, transaction/delta efficiency, Word-free bounded interop | 14 | 48 combined | 80 calls; ≥48 mutations; ≥16 families; commit count not fully fixed |
| R3 | SQLite/CBOR rollback artifact + external WAL | Typst + HTML; source capsule | explicit foreign-managed mode; detailed import/export states | native/faceted authority, independent writer, render and bounded DOCX | 21 numbered; one signature/encryption item expressly deferred, so 20 Build 001 | 46 combined/conditional | target 100, floor 80; ≥50 mutations; ≥20 families; 3 commits/3 deltas |
| R4 | SQLite rollback + deterministic CBOR; external WAL; state root | Typst + browser/HTML; alternate providers later | source capsule; clean semantic/evidence fidelity split | portable native truth, hostile edits, Word-free render, bounded DOCX | 20 | 32 native + 4 provider | 96 calls; 48 mutations; 16 families; 3 commits; 6 delta calls |

### 2.4 Hard metrics and stated falsifiers

R1 emphasized zero wrong target, branch collapse, anchor rebound, extension loss, partial commit, and false fidelity; it would abandon/rework native-first for renderer, span, container, DOCX, extension, clone, efficiency, or ecosystem-utility failure.

R2 emphasized exact cold recovery, zero wrong/stale/ambiguous/extension/partial writes, exact root-independent state behavior, and bounded operations; its falsifiers covered anchors, clone/fork, provider preservation, renderer, performance, and independent implementation.

R3 supplied the broadest identity/root/extension/interop zero set and falsified on native identity, text mapping, extension preservation, crash/root, renderer, practical DOCX, opaque-blob ontology, scale, Program Host advantage, or conformance failure.

R4 supplied the cleanest measurable split: hard zeros for wrong mutation, branch continuity, duplicate-ID stealing, stale/cross-branch writes, resurrection, anchor error, extension loss/scope escape, partial commits, delta gaps, render mismatch, invalid writable artifacts, and active-content execution; falsifiers covered anchors, copy/divergence, container, extensions, renderer, DOCX, ontology, scale, Program Host advantage, and independent specification.

**FACT:** none of the proposed experiment, rendering, crash, interop, or performance outcomes was measured in these reports.

## 3. Convergence and disagreements

### 3.1 Genuine convergence

The reports converged on one semantic authority, native artifact identity, bounded provider facets/capsules, layout/render separation, SQLite plus deterministic CBOR, rollback-journal portability, external rebuildable WAL indexes, Typst/browser provider candidates, Word-free native acceptance, no visual word processor, no permanent ledger, no universal AST, no Build 001 collaboration, and an independent native implementation.

This convergence is an input, not proof. Shared assumptions were actively tested below.

### 3.2 Significant disagreements

- UUIDv4 versus UUIDv7;
- retained object IDs versus full remint on fork;
- split/merge preserving a selected ID versus minting every result;
- revision UUID/sequence only versus mandatory semantic root/Merkle structure;
- exact retained-boundary persistence and affinity vocabulary;
- 14, 16, or 20 Build 001 experiments;
- 32+4, 40, 46, or 48 hostile cases;
- 80, 96, or 100 Program Host calls and different mutation-family floors;
- how explicitly foreign-managed DOCX belongs in the canonical product;
- exact extension-policy vocabulary and PDF accessibility gate.

No disagreement was resolved by frequency or arithmetic average.

## 4. Current primary-source re-verification

Only synthesis-changing facts were rechecked.

### 4.1 Storage, serialization, identity, and hashing

- **CURRENT EXTERNAL EVIDENCE:** SQLite documents rollback and WAL as different atomicity mechanisms; a WAL database's `-wal` and `-shm` participate in live state, while rollback mode uses a rollback journal. SQLite's database-file documentation explicitly warns recovery files cannot be ignored. The current stable release observed was 3.53.4 (2026-07-24), which included a prior WAL-reset corruption fix. Sources: [SQLite WAL](https://sqlite.org/wal.html), [file format](https://sqlite.org/fileformat.html), [current release](https://sqlite.org/releaselog/current.html).
- **CURRENT EXTERNAL EVIDENCE:** RFC 8949 defines CBOR and core deterministic encoding; the profile still needs application-specific duplicate-key, number, UUID, Unicode, and size rules. Source: [RFC 8949](https://www.rfc-editor.org/rfc/rfc8949.html).
- **CURRENT EXTERNAL EVIDENCE:** RFC 9562 defines UUIDv4 and v7 and acknowledges v4 database-locality costs. It does not make v7 semantically preferable. Source: [RFC 9562](https://www.rfc-editor.org/rfc/rfc9562.html).
- **CURRENT EXTERNAL EVIDENCE:** SHA-256 remains specified by NIST's Secure Hash Standard. Source: [FIPS 180-4](https://csrc.nist.gov/pubs/fips/180-4/upd1/final).

### 4.2 Text, layout, and rendering

- **CURRENT EXTERNAL EVIDENCE:** Unicode supplies explicit normalization forms and extended grapheme-cluster rules; normalization can change binary representation, so preserving code points and using UAX #29 for human operations are separate choices. Sources: [UAX #15](https://www.unicode.org/reports/tr15/), [UAX #29](https://www.unicode.org/reports/tr29/).
- **CURRENT EXTERNAL EVIDENCE:** HarfBuzz shapes Unicode sequences into positioned glyphs across writing systems; shaping capability is not a full layout/render guarantee. Source: [HarfBuzz manual](https://harfbuzz.github.io/).
- **CURRENT EXTERNAL EVIDENCE:** Typst 0.15.1 was current and Typst documents tagged/PDF accessibility and PDF standards support, but its own guidance distinguishes tagging from complete accessible authoring. Sources: [Typst 0.15.1](https://typst.app/docs/changelog/0.15.1/), [PDF reference](https://typst.app/docs/reference/pdf/), [accessibility guide](https://typst.app/docs/guides/accessibility/).
- **CURRENT EXTERNAL EVIDENCE:** Chromium exposes print-to-PDF/tagged-PDF machinery, while CSS Paged Media defines page boxes, margins, size/orientation, headers/footers, and page numbering. Neither fact proves the DOCSeye fixture's professional pagination. Sources: [Chromium tagged-PDF change](https://chromium.googlesource.com/chromium/src/+/924b3e0416e60354db27db961635cbc5a80e3fe1), [CSS Paged Media](https://www.w3.org/TR/css-page-3/).
- **CURRENT EXTERNAL EVIDENCE:** PDF/UA-1 and PDF/UA-2 define accessible-PDF requirements, not conversion procedures. Build 001 therefore requires independent validation rather than assuming a tagged output is conforming. Source: [ISO 14289-2 overview](https://www.iso.org/standard/82278.html).

### 4.3 Office and provider formats

- **CURRENT EXTERNAL EVIDENCE:** Open XML SDK 3.5.1 was the latest observed release and provides strongly typed OOXML/OPC access, but its own repository says it closely follows ISO 29500 and is not a higher-level abstraction/fidelity theorem. Sources: [Open XML SDK docs](https://learn.microsoft.com/en-us/office/open-xml/open-xml-sdk), [3.5.1 release](https://github.com/dotnet/Open-XML-SDK/releases/tag/v3.5.1).
- **CURRENT EXTERNAL EVIDENCE:** OPC is a ZIP/XML/Web-based packaging architecture under ISO/IEC 29500/ECMA-376. Source: [OPC fundamentals](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/opc/open-packaging-conventions-overview).
- **CURRENT EXTERNAL EVIDENCE:** Microsoft describes unattended Office automation as “AS IS” with unexpected behaviors that applications must handle; this does not support a hidden server dependency. Source: [Microsoft unattended automation considerations](https://learn.microsoft.com/en-us/office/client-developer/integration/considerations-unattended-automation-office-microsoft-365-for-unattended-rpa).
- **CURRENT EXTERNAL EVIDENCE:** ODF 1.4 became an OASIS Standard on 2025-10-06. It is a peer provider candidate, not a reason to expand Build 001. Source: [ODF 1.4](https://www.oasis-open.org/standard/open-document-format-for-office-applications-opendocument-version-1-4/).
- **CURRENT EXTERNAL EVIDENCE:** LibreOffice documents command-line alien-format conversion and PDF export. Its observation is independently useful but is not Microsoft evidence. Sources: [conversion filters](https://help.libreoffice.org/latest/en-US/text/shared/guide/convertfilters.html), [PDF CLI parameters](https://help.libreoffice.org/latest/en-US/text/shared/guide/pdf_params.html).

### 4.4 AI-native and collaboration references

- **CURRENT EXTERNAL EVIDENCE:** DocLang is an emerging open AI-native constrained-XML/token-oriented specification that includes semantic/layout/geometry concepts. It is relevant evidence that AI-facing representations are evolving, but it does not supply DOCSeye's transactional family/branch/anchor/container contract and is not selected as authority. Sources: [DocLang repository](https://github.com/doclang-project/doclang), [LF AI & Data announcement](https://www.linuxfoundation.org/press/lf-ai-data-foundation-launches-doclang-specification-working-group-to-advance-an-open-standard-for-ai-native-documents).
- **CURRENT EXTERNAL EVIDENCE:** ProseMirror maps revision-local positions through transactions; Yjs relative positions can resolve or become null after deletion; Peritext uses stable character identifiers and append-only formatting spans for collaborative intent. These validate that anchors need explicit semantics, not that a one-operator DND needs a CRDT. Sources: [ProseMirror guide](https://prosemirror.net/docs/guide/), [Yjs relative positions](https://docs.yjs.dev/api/relative-positions), [Peritext paper](https://www.inkandswitch.com/peritext/static/cscw-publication.pdf).

## 5. Sibling evidence

- **IMPLEMENTATION-DERIVED SIBLING EVIDENCE:** eyeBROWSE recovered persistent document-resident identity after kernel loss and completed a 33-operation local workflow. This supports domain-resident identity and a compact Program Host, not its web object model.
- **IMPLEMENTATION-DERIVED SIBLING EVIDENCE:** CODEeye accepted with zero false rebounds/wrong-target mutations under its hostile suite and a 45-operation Program Host workflow. This supports separate logical/provider identity, exact targeting, bounded deltas, and conservative failure.
- **IMPLEMENTATION-DERIVED SIBLING EVIDENCE:** SHELLeye passed 25/25 hostile cases with zero false rebounds/wrong-object mutations and a 52-operation workflow; it also changed a native-handle assumption when implementation evidence falsified it. This supports separate physical identity and explicit correction on falsification.
- **FACT:** DESKTOPeye did not provide a canonical accepted Build 001 result document at inspection time, so its boundaries are architectural sibling evidence, not claimed implementation acceptance.

Measured sibling behavior supports persistent domain objects, exact targeting, false-rebound avoidance, domain/provider authority, separate physical identity, delta-first operation, local Program Host execution, and no permanent action ledger. It does not justify copying browser/source/filesystem object models into documents.

## 6. Disagreement adjudication

| Question | Resolution | First-principles invariant |
| --- | --- | --- |
| native vs DOCX authority | native sole semantic authority in native mode; foreign-managed DOCX retained explicitly | one editable truth plus permanent intrinsic identity/transaction leverage |
| UUID v4/v7 | UUIDv4 | semantic IDs must be opaque; locality/order are separate and time leakage adds no semantic value |
| fork object IDs | remint all public objects/boundaries, bounded origin mapping | eliminate bare-ID/cross-branch accidents; pay bounded mapping once |
| split/merge IDs | retire all inputs/originals and mint all results | no arbitrary side gets false continuity |
| revision model | UUIDv4 commit ID + branch sequence + SHA-256 semantic root + bounded parents | distinguish commit identity, ordering convenience, state identity, and bounded ancestry |
| root | mandatory Merkle-style current state; exact tree E-07 | neutral storage rewrites must not change semantic state; local updates need bounded recomputation |
| boundary | explicit `retain_range` creates identity-bearing zero-width boundaries | reads do not mutate; portable anchors require explicit lifetime/affinity |
| extension policy | coverage plus operational five-policy vocabulary | an older implementation must detect intersection and know safe action without payload knowledge |
| SQLite/CBOR | rollback-journal SQLite application file + deterministic-CBOR logical records; external WAL indexes | portable atomic container is separate from public ontology and caches |
| renderer ownership | DOCSeye owns contract/mapping; existing engines typeset/render | avoid building a word processor/typography engine while preserving attributable output |
| fidelity | six semantic outcomes plus separate observation-evidence set | translation and provider observation answer different questions |
| hostile cardinality | 32 native + 4 provider | distinct native failure modes must not be diluted by compatibility cases |
| Program Host cardinality | 96 / 48 mutations / 16 families / 3 commits | fully enumerated lifecycle coverage and arithmetic, not round-number preference |

## 7. Shared blind spots and falsification attempt

The four reports shared plausible blind spots:

1. **SQLite optimism.** A database can be a reliable application file while live copy/cloud-sync practices still violate coherence. E-02/E-03 are early killers.
2. **Anchor optimism.** “Stable boundaries” can hide an unbounded operation-history or CRDT requirement. E-01 is first and architectural.
3. **Extension optimism.** Opaque preservation without edit coverage simply defers silent loss. The freeze requires machine-readable coverage and refusal; E-04/E-19 can still reject it.
4. **Renderer optimism.** Producing a PDF does not prove typography, accessibility, source mapping, or professional pagination. E-12/E-13 are renderer falsifiers.
5. **Interop optimism.** A source capsule can become a covert second authority and schema validity can be mislabeled as Word fidelity. Normative authority and two-axis outcomes prevent that; E-15/E-16 test usefulness.
6. **Ontology expansion.** Native-first can become a universal AST or a bag of opaque provider blobs. The authored boundary and F-07 falsifier prevent either success-by-relabeling.
7. **Clone identity.** Embedded IDs necessarily clone in a raw copy. The design acknowledges replicas then stops on divergence rather than mutating on open.
8. **Open-spec optimism.** A format is not open merely because prose exists. Independent canonical vectors/writer/commit are mandatory in E-06/E-08.
9. **Security surface.** SQLite, CBOR, XML/ZIP, SVG, fonts, and capsules expand parsers. E-18 and hard-zero active-content behavior are required.

The leading architecture survived this falsification only as a conditional freeze: each shared blind spot maps to an early experiment or hard gate. No proposed result is treated as passed.

## 8. Candidate comparison and final inference

| Candidate | Permanent leverage | Permanent cost/risk | Determination |
| --- | --- | --- | --- |
| DOCX-first | immediate ecosystem/Word fidelity; mature provider truth | external correspondence for native semantics; Word-coupled acceptance; provider/layout dependence | **SUPERSEDED baseline**, retained foreign-managed mode/fallback |
| pure native-first | intrinsic identity/transactions | foreign loss and island risk | **REJECTED** |
| true dual/braided authority | preserves both editable forms | unavoidable conflict/staleness; ambiguous capability | **REJECTED** |
| native-format-later | defers stewardship | hardens correspondence/provider coupling before testing highest leverage | **REJECTED now**, explicit falsifier fallback |
| native authority + non-authoritative provider facets | intrinsic identity/transactions plus honest preservation and explicit foreign mode | stewardship, conformance, renderer/interop work, parser surface | **SELECTED** |

**ARCHITECTURAL INFERENCE:** the selected design materially beats the DOCX-first baseline because intrinsic portable identity, exact branch/revision-conditioned writes, retained anchors, semantic roots/deltas, extension intersection safety, and renderer/provider independence are permanent capabilities that cannot be added cleanly as a late sidecar without reintroducing dual authority. The cost remains justified only if Build 001 passes its independent-format, renderer, scale, extension, and DOCX falsifiers.

## 9. What remains experimental

The semantics are frozen; the following mechanisms remain **OPEN QUESTION — BUILD 001 EXPERIMENT REQUIRED**:

- stable persistent text structure under the boundary contract;
- SQLite crash/coherent-copy/publication profile;
- sparse ordering storage versus bounded alternative;
- semantic-root tree fan-out/domain layout;
- exact MathML canonicalization/facet precedence;
- renderer ceiling, accessible PDF, and practical Word-free DOCX export;
- concrete performance/resource limits and index/root amplification.

No further generic architecture pass is required after publication. Failure of a numbered falsifier reopens only the affected decision.
