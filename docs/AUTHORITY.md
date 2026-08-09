# Canonical Authority and Status

Status: **CANONICAL**

## 1. Source hierarchy

For the current architecture, authority descends in this order:

1. the commit on `main` that first contains this complete native-authority canonical set (the publication SHA reported and remotely verified at freeze time);
2. [01-ARCHITECTURE.md](01-ARCHITECTURE.md) for system authority, identity, time, storage, render/provider, and substrate boundaries;
3. [NATIVE-FORMAT.md](NATIVE-FORMAT.md) for normative DND logical-format semantics;
4. [02-BUILD-001-SLICE.md](02-BUILD-001-SLICE.md) for exact implementation experiments and A–D/X acceptance;
5. [06-DECISIONS.md](06-DECISIONS.md) for 64 numbered frozen decisions and old-decision adjudication;
6. the remaining canonical documents for charter, platform, roadmap, research trace, capabilities, and workflow pressure;
7. replacement GitHub Build 001 issues as tracking views of the canonical slice.

If a tracking issue, provider output, generated artifact, report, or comment conflicts with the committed canonical documents, the committed canonical documents win until an explicit later canonical amendment.

The freeze SHA is not written into its own tree, avoiding an impossible self-referential commit. Remote publication verification and the final synthesis report record the exact SHA.

## 2. Historical authority

`47b5280b71773ae497b5a56c4b590b9070b4bca5` is permanently labeled:

> **SUPERSEDED AS DOCX-FIRST ARCHITECTURE BASELINE**

It remains the strongest completed DOCX-first candidate and the historical authority for the original provider-federated architecture and Build 001 issues #1–#5. It is not failed, invalid, rewritten, squashed, or erased. Its issue bodies retain their original meaning.

The current native freeze supersedes that SHA only for canonical future DOCSeye architecture and replacement Build 001 implementation.

## 3. Canonical file set

Required current files:

- `.gitignore`
- `README.md`
- `docs/00-CHARTER.md`
- `docs/01-ARCHITECTURE.md`
- `docs/02-BUILD-001-SLICE.md`
- `docs/03-PLATFORM-STEALTHEYELLC.md`
- `docs/04-ROADMAP.md`
- `docs/05-RESEARCH-BASELINE.md`
- `docs/06-DECISIONS.md`
- `docs/07-CAPABILITY-MATRIX.md`
- `docs/08-WORKFLOW-PRESSURE-TESTS.md`
- `docs/NATIVE-FORMAT.md`
- `docs/AUTHORITY.md`

`docs/09-BUILD-001-RESULTS.md` MUST remain absent until implementation and acceptance actually occur.

## 4. Current truth

| Item | Status |
| --- | --- |
| Architecture | **FINAL / SYNTHESIZED / VERIFIED / FROZEN FOR BUILD 001** |
| Architecture family | **NATIVE SEMANTIC AUTHORITY WITH NON-AUTHORITATIVE PROVIDER FACETS** |
| Prior DOCX-first freeze | `47b5280b71773ae497b5a56c4b590b9070b4bca5` — **SUPERSEDED AS DOCX-FIRST BASELINE** |
| Build 001 | **PLANNED / NOT IMPLEMENTED** |
| Product implementation | **NOT STARTED** |
| Build 001 acceptance | **NOT RUN** |
| Microsoft Word | **NOT REQUIRED FOR NATIVE BUILD 001 ACCEPTANCE / OPTIONAL FOR MICROSOFT-SPECIFIC INTEROP/CONFORMANCE** |

No experiment, hostile suite, Program Host workflow, renderer quality, PDF conformance, DOCX round trip, LibreOffice observation, or Word compatibility result is claimed.

## 5. Change discipline

A future canonical amendment must:

- identify the numbered decision/experiment/falsifier being changed;
- preserve the DOCX-first baseline and this freeze in history;
- distinguish mechanism evidence from architecture change;
- update every affected canonical file and issue coherently;
- never rewrite history or relabel an unrun test as accepted.

Implementation convenience does not override the freeze. If a numbered architecture falsifier fails, implementation stops and reopens only the affected question.
