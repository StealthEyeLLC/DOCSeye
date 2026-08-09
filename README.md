# DOCSeye

DOCSeye is ChatGPT's persistent authored-information substrate at StealthEyeLLC. Its canonical architecture is **NATIVE SEMANTIC AUTHORITY WITH NON-AUTHORITATIVE PROVIDER FACETS**.

For a native-mode document, one portable DOCSeye Native Document (DND) is the sole editable semantic authority. DOCX, ODF, HTML, PDF, Typst, browser DOMs, and Microsoft Word are bounded providers or derived representations; none silently becomes a second native semantic authority.

## Frozen status

| Item | Status |
| --- | --- |
| Architecture | **FINAL / SYNTHESIZED / VERIFIED / FROZEN FOR BUILD 001** |
| Architecture family | **NATIVE SEMANTIC AUTHORITY WITH NON-AUTHORITATIVE PROVIDER FACETS** |
| Prior DOCX-first freeze | `47b5280b71773ae497b5a56c4b590b9070b4bca5` — **SUPERSEDED AS DOCX-FIRST ARCHITECTURE BASELINE** |
| Build 001 | **PLANNED / NOT IMPLEMENTED** |
| Product implementation | **NOT STARTED** |
| Build 001 acceptance | **NOT RUN** |
| Microsoft Word | **NOT REQUIRED FOR NATIVE BUILD 001 ACCEPTANCE; OPTIONAL FOR MICROSOFT-SPECIFIC INTEROP/CONFORMANCE** |

The superseded baseline remains a valid, permanently retained design alternative in Git history. It was not an implementation failure. The native design supersedes it because portable intrinsic identity, revision-conditioned transactions, retained anchors, extension safety, semantic deltas, and renderer/provider independence provide material permanent leverage before implementation begins.

## Authority modes

DOCSeye exposes an explicit mode boundary:

- **Native-authored** — DND is semantic authority from creation.
- **Converted/imported DOCX** — DND becomes sole editable semantic authority; an original DOCX may remain as non-authoritative source evidence and provider facets.
- **Foreign-managed DOCX** — an intentionally unconverted DOCX remains provider authority and uses the preserved DOCX-first correspondence architecture. It is not simultaneously a native document.

Conversion is an explicit transaction. No document may be silently both DOCX-authoritative and native-authoritative.

## Build 001 proof

Build 001 is frozen to prove one portable native artifact, not merely a new extension:

> Prove that one portable native DOCSeye artifact carries authoritative semantic identities, family/branch/revision state, retained anchors, extension-preservation contracts, assets, and layout intent; survives total kernel and rebuildable-runtime loss, coherent movement, independent valid native edits, and replica divergence; executes exact revision-conditioned atomic transactions with compact gap-aware semantic deltas; renders attributable accessible HTML and paginated PDF with no Microsoft Word dependency; and crosses one bounded DOCX boundary without false fidelity claims.

The acceptance slice contains:

- 20 numbered implementation experiments;
- 32 separately scored native hostile cases;
- 4 separately scored DOCX-provider supplemental cases;
- one 96-call Program Host invocation with 48 semantic mutations, 16 mutation families, 3 atomic commits, 6 delta consumptions, and zero intermediate model calls.

No experiment has run and no result is claimed.

## Canonical documentation

Read in this order:

1. [Charter](docs/00-CHARTER.md)
2. [Architecture](docs/01-ARCHITECTURE.md)
3. [Build 001 slice](docs/02-BUILD-001-SLICE.md)
4. [STEALTHEYELLC platform](docs/03-PLATFORM-STEALTHEYELLC.md)
5. [Roadmap](docs/04-ROADMAP.md)
6. [Research baseline](docs/05-RESEARCH-BASELINE.md)
7. [Decision register](docs/06-DECISIONS.md)
8. [Capability matrix](docs/07-CAPABILITY-MATRIX.md)
9. [Workflow pressure tests](docs/08-WORKFLOW-PRESSURE-TESTS.md)
10. [Native format](docs/NATIVE-FORMAT.md)
11. [Authority and status](docs/AUTHORITY.md)

`docs/09-BUILD-001-RESULTS.md` must remain absent until Build 001 is implemented and accepted. This repository contains architecture/specification text only: no product source, test project, fixture, native database, generated binary, dependency manifest, or result artifact.

## Non-negotiable boundaries

- ChatGPT is the primary operator; the local Program Host is deterministic and contains no model.
- Persistent semantic identity is not a path, byte hash, provider handle, integer offset, page, run, or search result.
- Exact target or conservative refusal beats plausible rebound.
- Current semantic state and bounded recovery witnesses are retained; no permanent action ledger is required.
- SHELLeye owns physical carriers/publication; DESKTOPeye owns UI state; CODEeye owns source semantics; DATAeye owns computational tables/data; eyeBROWSE owns live web state; MEDIAeye owns full temporal media.
- Opening or rendering a document never executes native or foreign active content and never fetches remote assets automatically.

The next authorized DOCSeye work is Build 001 implementation against this freeze. Another generic architecture pass is unnecessary unless a numbered architecture falsifier fails.
