# 05 — Research Baseline and Synthesis Record

Status: **SYNTHESIZED / VERIFIED / FROZEN FOR BUILD 001**
Research date: **2026-08-09**
Implementation experiments performed in this pass: **none**

## 1. Inputs

The freeze synthesizes three independent first-principles DOCSeye research reports supplied in full on 2026-08-09. They are called R1, R2, and R3 here only to compare their claims; report order does not grant authority.

The pass also used:

- current primary standards and official vendor/API documentation for load-bearing claims;
- read-only inspection of the public StealthEyeLLC sibling repositories CODEeye, SHELLeye, DESKTOPeye, and eyeBROWSE;
- the supplied GitHub authority note;
- first-principles falsification against wrong-target, preservation, provider-coherence, and context-efficiency requirements.

The reports are evidence. After this freeze they do not independently override the canonical repository.

## 2. Convergence

All three reports converge strongly on the permanent architecture:

1. DOCSeye is a persistent correspondence and transaction substrate, not a human editor or library wrapper.
2. Provider-native representations retain authority; there is no universal serialization AST.
3. External DOCSeye IDs are primary, while native IDs are scoped witnesses.
4. Full current-revision indexing combines with selective durable promotion.
5. Exact deterministic rebase is permitted; fuzzy write rebinding is forbidden.
6. Untouched/unsupported native truth must survive accepted unrelated edits.
7. Open XML SDK is a typed provider component, not the fidelity boundary.
8. Word is a strong native/layout provider, not sole semantic or identity authority.
9. Semantic, representation, provider, layout, and render state require separate clocks.
10. DOCX Transitional is the decisive editable Build 001 format; PDF is not a co-primary editor.
11. SQLite WAL plus FTS5 is sufficient operating persistence; a graph database and permanent ledger are rejected.
12. A non-agentic Node Program Host should execute dozens of typed local operations per model turn.

This unusually strong agreement justifies freezing the spine. The remaining disputes mostly concerned exact Build 001 numbers and mechanism choices.

## 3. Current primary-source verification

The synthesis rechecked claims whose current state could materially change the design.

| Claim | Verified result | Architectural use |
| --- | --- | --- |
| .NET support | [.NET 10 is LTS](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support) | kernel baseline |
| Node support | [Node 24 is LTS](https://nodejs.org/en/about/previous-releases) | Program Host baseline |
| Open XML SDK | [3.5.1 is the current release](https://github.com/dotnet/Open-XML-SDK/releases) | typed provider pin |
| SQLite WAL defect | documented defect fixed in [3.51.3](https://sqlite.org/releaselog/3_51_3.html); current release is documented separately by SQLite | corrected version required |
| `w15:docId` | identifies a [set of documents derived from a common source](https://learn.microsoft.com/en-us/openspecs/office_standards/ms-docx/b5058d55-0aa8-44e0-9a37-0c84b6e9f68b) | family/derivation witness, never branch identity |
| `w14:paraId` | documented scoped paragraph identity rules and AlternateContent exception | provider-scoped witness |
| `w14:textId` | documented paragraph-version/text correspondence rules | version witness, not logical ID |
| Office.js paragraph IDs | [differ across sessions and coauthors](https://learn.microsoft.com/en-us/javascript/api/word/word.paragraph?view=word-js-preview) | provider-epoch only |
| content-control ID | [unique and nonchanging](https://learn.microsoft.com/en-us/office/vba/api/word.contentcontrol.id) in Word's documented scope | strong native witness |
| modern comments | extension provides a [durable ID](https://learn.microsoft.com/en-us/openspecs/office_standards/ms-docx/a7b57225-42e5-43e7-8d98-d90eabf3ca25) | strong comment witness |
| Markup Compatibility | [preprocessing can select/discard alternatives and compatibility markup](https://learn.microsoft.com/en-us/office/open-xml/general/introduction-to-markup-compatibility) | typed load/save is not the preservation boundary |
| Word layout | official [repagination](https://learn.microsoft.com/en-us/office/vba/api/word.document.repaginate), [range information](https://learn.microsoft.com/en-us/office/vba/api/word.wdinformation), and [Page](https://learn.microsoft.com/en-us/javascript/api/word/word.page?view=word-js-preview) APIs exist | mandatory layout experiment/provider |
| Word comparison | [CompareDocuments](https://learn.microsoft.com/en-us/office/vba/api/word.application.comparedocuments) covers rich differences | optional diff evidence, not identity authority |
| Word PDF export | [ExportAsFixedFormat3](https://learn.microsoft.com/en-us/office/vba/api/word.document.exportasfixedformat3) improves tagging | derived render proof |
| OPC/OOXML | [ECMA-376](https://ecma-international.org/publications-and-standards/standards/ecma-376/) and Microsoft's [OPC overview](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/opc/open-packaging-conventions-overview) confirm package/relationship/MC separation | provider architecture |
| Tagged PDF | [PDF Association guidance](https://pdfa.org/techniques-for-accessible-pdf-background/) distinguishes author-provided logical structure from mere appearance | separate PDF facet |
| Untagged extraction | [MuPDF structured-text options](https://mupdf.readthedocs.io/en/1.27.2/reference/common/stext-options.html) expose reading-order/paragraph inference | heuristic assurance required |

No current source turned a native identifier into a universal durable logical ID. No current source justified treating Word layout, stored OOXML, and PDF fixed layout as one truth.

## 4. Sibling audit

The sibling repositories establish a useful family pattern without dictating DOCSeye's ontology:

- CODEeye separates persistent logical engineering concepts from current provider bindings and demonstrates why historical correspondence must not contaminate current exact actuation.
- SHELLeye owns physical-file identity and exact replacement semantics, supporting the document/file separation.
- DESKTOPeye is a documentation-first frozen architecture baseline and owns application/UI reality rather than document semantics.
- eyeBROWSE owns live browser/DOM/network reality.

The audit found no existing DOCSeye repository, implementation branch, pull request, issue, or competing canonical artifact. That made a new public documentation-only repository the correct baseline. Sibling architecture was treated as corroboration, not copied terminology.

## 5. Falsification results

The synthesis tried to replace the recommendation with simpler designs.

| Attack | Result |
| --- | --- |
| Native paragraph IDs already solve identity | Rejected: their scope and lifetime are narrower than DOCSeye continuity. |
| Inject IDs into every document | Rejected: copies, signatures, foreign formats/editors, and semantic contamination remain. |
| Use Word for everything | Rejected as sole authority; retained as a mandatory bounded provider for Build 001. |
| Use a universal document AST | Rejected: it makes provider-only truth lossy or secondary. |
| Make every object revision-local | Too conservative: exact operation lineage and strong native witnesses have real agent value. |
| Persist every parsed object | Rejected: it manufactures durability for offsets, runs, coordinates, and incidental XML. |
| Use fuzzy similarity after external edits | Rejected for writes: it cannot meet zero wrong-target acceptance. |
| Use a normal typed serializer | Rejected as fidelity boundary: MC and unknown/opaque content can be changed or lost. |
| Require byte-lexical preservation for every touched subtree | Not frozen: useful mechanism, but disproportionate and unproven as a universal invariant. |
| Preserve packages without a correspondence world | Insufficient: exact bytes do not create durable semantic targets. |
| Store canonical document state in a graph DB | Rejected: duplicates provider truth and still does not prove identity. |
| Treat PDF as a DOCX-shaped editor | Rejected: fixed layout/tagging/inference have different semantics. |

The decisive change from falsification is the two-dimensional mutation contract: an accepted edit must satisfy both its semantic-effect envelope and its serialization footprint. Minimal lexical patching is one way to meet the footprint, not the definition of correctness.

## 6. Disagreement register

The following table resolves every material disagreement that could change Build 001 scope or semantics.

| # | Reports said | Primary-source constraint | First-principles test | Frozen ruling | Confidence |
| --- | --- | --- | --- | --- | --- |
| 1 | Hostile suite counts varied between 44 and 48; one report grouped several compounds. | No source determines project test count. | Every distinct failure mode must have an independently nameable oracle without padding. | **54 deterministic cases**, derived from the complete union after deduplication. | High |
| 2 | Milestone A retained “several,” 24, or at least 25 concepts. | No source determines count. | The set must span every identity-bearing family needed by A while remaining auditable. | **Exactly 24 retained concepts** with a frozen family allocation. | High |
| 3 | A recovery outcomes were qualitative in two reports and quantified differently/implicitly in another. | No source determines oracle distribution. | A must prove both positive recovery and principled loss of continuity. | Unchanged bytes: **24/24 exact**. Observer gap: **14 exact, 6 stale/destroyed, 4 ambiguous**. | High |
| 4 | Program Host floor was at least 40, target 50–60, or an exact 58-step proposal. | No source determines count. | The workflow must reach the top of the stated target and include all decisive branches. | **Exactly 60 typed SDK calls**. External adversary is uncounted. | High |
| 5 | Mutation floors ranged from 24 to an implicit larger set. | No source determines count. | Queries alone cannot prove a programmable transaction world. | The exact D workflow contains **29 mutations**, 15 object families, and two commits. | High |
| 6 | Word was called optional, later/strong provider, or mandatory for layout/compatibility. | Official APIs establish unique Word behavior but not cold identity or raw preservation. | A Word-fidelity thesis without real Word can pass a false package-only demo. | Word adapter is **mandatory and isolated in Build 001**, bounded to native/live/layout/compatibility roles; not sole truth. | High |
| 7 | Save As was either always derivative or explicitly deferred. | `docId` is family evidence, not branch identity. | Letting a copied carrier silently inherit logical identity risks cloned handles. | **Save As creates a new document branch with DERIVED_FROM**, unless a separately explicit move/rename operation proves no branch. | High |
| 8 | Preservation was described as a general contract or as a two-boundary contract. | OPC/MC facts show semantic correctness and serialization footprint are independent. | A semantically correct edit can still destroy unknown package truth; a minimal patch can still target the wrong semantic object. | Freeze **semantic-effect envelope plus serialization footprint** as independent commit gates. | Very high |
| 9 | Token/CST or lexical splicing ranged from central mandatory mechanism to an open experiment. | No specification guarantees an SDK/library's lexical footprint. | Correctness must survive replacement of the patching implementation. | Lexical splice is preferred where exact; bounded subtree reconstruction is allowed; **the universal token structure remains experimental**. | High |
| 10 | Temporal terminology varied: carrier incarnation, representation braid, and coherence vector. | Live Word, disk package, layout, and render can demonstrably diverge. | One revision counter cannot express these states; incarnating every object adds fake clocks. | Freeze DocumentRevision, DocumentRepresentation, RepresentationIncarnation/Revision, ProviderEpoch/Revision, LayoutRevision, RenderRevision, WorldSequence, and PhysicalFileRevision. | High |
| 11 | OpenXML provider isolation was implied in some topology drawings and in-process in others. | SDK is managed provider machinery; no current evidence requires process isolation. | Isolation adds complexity without addressing a demonstrated failure boundary. | OpenXML provider is **in-process initially**; Word is isolated. Revisit only from experiment evidence. | Medium-high |
| 12 | Fixture length ranged roughly 20–30 to 30–40 pages. | No source dictates length. | Feature/adversary density and reflow sensitivity matter more than page inflation. | Freeze **approximately 20–30 Word-generated page-sensitive pages** with the full feature inventory. | Medium-high |
| 13 | Primary identity killer was sometimes the duplicate paragraph/span and sometimes duplicate table cell. | No source ranks tests. | Tables combine coordinate drift, topology, duplicate content, and weak native keys; spans isolate wrong-target risk more cleanly. | Overall identity killer is **C-27 duplicate row/cell**; wrong-target span killer is **C-20**. Both are mandatory. | High |
| 14 | PDF was a later provider, a possible separate product, or part of the permanent substrate. | Tagged and untagged PDF expose native/fixed/inferred truths unlike DOCX. | A small common document/representation protocol is useful, but common edit semantics are not. | PDF remains a **later provider/facet**, Build 001 render only; split later if the common core becomes distorting. | Medium-high |
| 15 | SQLite was recommended generally; one report noted a current WAL defect/fix. | SQLite documents the corrected release. | A persistence choice must not pin a known-corruptible build. | Use WAL/FTS5 and **pin a corrected SQLite release, at least the documented fixed line**; record the exact version. | Very high |
| 16 | COM, Office.js Page, exported PDF, or combinations were proposed for semantic/page mapping. | Official APIs establish capabilities but not which produces sufficient exact mapping in the target Word build. | Provider contract can freeze while API mechanism remains empirical. | Word semantic-to-layout mapping is mandatory; **exact COM versus Office.js versus export mechanism is experiment E23/E25**. | High |
| 17 | SDK/runtime versions were stated generically or at different current points. | Current release/support pages are authoritative and time-sensitive. | Reproducibility requires a frozen initial pin plus recorded resolution. | Baseline **.NET 10 LTS, Node 24 LTS, Open XML SDK 3.5.1, corrected SQLite**; preflight records exact installed/resolved versions. | High |

## 7. Frozen synthesis

The final architecture is:

> A persistent, preservation-first, provider-federated document correspondence world with full current-revision structural indexing, sparse durable semantic promotion, object-specific identity evidence, exact revision-aware typed transactions, two-dimensional mutation contracts, separate semantic/representation/provider/layout/render clocks, bounded deltas and waits, and a non-agentic local Program Host.

The freeze deliberately does not claim that implementation experiments have proved native-ID behavior, lexical-patch feasibility, Word layout mapping, or package acceptance. Those are exactly the bounded questions in Build 001. What is frozen is the correctness meaning the mechanisms must satisfy.

## 8. Evidence limits

- Current vendor documentation is current external evidence, not a promise that future Word builds never change behavior.
- Normative scope does not prove empirical stability across every editor operation.
- Sibling repositories support substrate-boundary reasoning but do not validate document-specific identity.
- No experiment in the reports or this synthesis establishes acceptance.
- Public source links support the claims nearest them; this repository paraphrases rather than reproduces source text.

Consequently, architecture is frozen while product implementation and Build 001 acceptance remain **NOT STARTED / NOT RUN**.
