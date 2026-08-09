# 03 — STEALTHEYELLC Platform Contract

Status: **FROZEN FOR BUILD 001**
Target: **Windows STEALTHEYELLC workstation**
Measured target-host preflight: **REQUIRED BEFORE IMPLEMENTATION**

## 1. Purpose

This document separates the frozen platform contract from facts that must be measured on the target workstation. DOCSeye is designed for the STEALTHEYELLC Windows environment and must integrate with sibling substrates without treating their availability as an excuse to weaken standalone correctness.

The architecture is portable at the correspondence-kernel boundary. Build 001 is intentionally Windows- and Word-specific because its decisive proof includes real Microsoft Word behavior and pagination.

## 2. Frozen baseline

| Component | Build 001 baseline | Role |
| --- | --- | --- |
| Operating system | Current supported 64-bit Windows on STEALTHEYELLC | Word automation, filesystem integration, local IPC |
| Kernel | C# on .NET 10 LTS | correspondence, revisions, index, transactions, preservation coordinator |
| Typed OOXML layer | Open XML SDK 3.5.1 | schema-aware parsing, validation, streaming, typed provider facets |
| Preservation layer | Direct OPC plus namespace/MC-aware XML machinery | copy-on-write package commit and bounded mutation footprint |
| Native provider | Current locally installed desktop Microsoft Word | live state, native behavior, layout, field update, compatibility, PDF export |
| Operating database | Corrected SQLite release with WAL and FTS5; never an affected pre-fix WAL-reset build | current correspondence, indexes, bounded deltas |
| Program Host | Node.js 24 LTS | disposable, non-agentic local typed programs |
| Source control | Git on Windows | implementation baseline and evidence |
| Local RPC | Named pipes or equivalently local authenticated structured transport | kernel/Word adapter/Program Host boundary |

Current primary sources establish that [.NET 10 is LTS](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support), [Node 24 is an LTS line](https://nodejs.org/en/about/previous-releases), [Open XML SDK 3.5.1 is the current SDK release](https://github.com/dotnet/Open-XML-SDK/releases), and the documented SQLite WAL-reset defect is fixed in [SQLite 3.51.3](https://sqlite.org/releaselog/3_51_3.html). Implementation must pin an actually corrected SQLite package and record the resolved version.

## 3. Target-host preflight

Before any source or fixture implementation, issue #1 must record:

- Windows edition, version, build, architecture, locale, timezone, and long-path policy;
- Microsoft Word product, semantic version, build, update channel, architecture, licensing/activation state, and Protected View/Trust Center constraints relevant to fixtures;
- installed .NET SDKs and runtimes;
- Node and npm versions;
- Git version and line-ending configuration;
- resolved Open XML SDK and SQLite native/managed package versions;
- fonts required by the deterministic fixture and their exact versions;
- printer/default-page environment relevant to Word pagination;
- availability and versions of CODEeye, SHELLeye, DESKTOPeye, and eyeBROWSE interfaces used by the build;
- filesystem capabilities at the repository, runtime, fixture, and temporary-output locations;
- Word automation bitness compatibility and a minimal open/close/repaginate/export smoke result;
- any endpoint security or policy that can block COM, named pipes, atomic replacement, temporary files, or child processes.

Nothing in the research reports proves the exact Word installation on STEALTHEYELLC. That fact remains a measured prerequisite, not a frozen assumption.

## 4. Process topology

| Process | Lifetime | Failure boundary | Canonical state |
| --- | --- | --- | --- |
| DOCSeye kernel | long-lived service for a session; restartable | must reconstruct from provider truth plus SQLite | no full canonical document copy |
| OpenXML provider | in-process initially | transaction abort on provider failure | current pinned package snapshot |
| Word adapter | isolated, restartable, timeout-governed | a hang cannot own or corrupt kernel state | Word-owned live document only while coherently attached |
| Program Host | disposable per invocation by default | death aborts uncommitted local program work | owns no durable truth |
| SQLite | kernel-owned | rebuildable index/correspondence validation after crash | operating state only |

The Word adapter must use explicit provider epochs. COM or Office.js objects from an earlier epoch are never reused after provider restart.

## 5. Filesystem and paths

The following path shape is a convention for implementation planning, not a claim that the directories already exist:

| Purpose | Planned convention |
| --- | --- |
| Repository checkout | `X:\\DOCSeye` or the STEALTHEYELLC engineering checkout root |
| Runtime state | `C:\\DOCSeye\\state` |
| Test fixtures | repository-owned deterministic fixture tree |
| Transaction candidates | an explicit same-volume temporary root near the target artifact when atomic replacement requires it |
| Logs/results | repository test artifacts during implementation; canonical measured summary in `docs/09-BUILD-001-RESULTS.md` only after acceptance |

The implementation must not rely on paths for logical document identity. Path configuration is operational plumbing. SHELLeye physical-file identity and DOCSeye controlled-operation lineage govern continuity.

## 6. Word provider contract

The provider must:

1. attach to or open the exact representation under a new provider epoch;
2. expose whether Word is ahead of, aligned with, or behind the persisted package;
3. refuse package-side writes that would overwrite newer unsaved Word state;
4. expose bounded native operations required by Build 001;
5. report save completion separately from raw filesystem events;
6. repaginate and return evidence that a layout revision is based on the requested document/provider revision;
7. export fixed-format PDF with exact source-revision metadata in DOCSeye correspondence;
8. time out, cancel where safely possible, and be restartable without fabricating continuity;
9. open accepted specimens without repair and surface any repair result as a failed gate;
10. never become the external logical identity authority or package preservation verifier.

## 7. SQLite contract

SQLite uses WAL plus FTS5 for one local writer and concurrent readers. The database contains current operating state and bounded recovery evidence, not a version-control product.

Required controls:

- pin and record a corrected SQLite version;
- run startup integrity and schema-version checks;
- make current package/provider truth sufficient to rebuild indexes;
- coordinate database commit publication with physical package commit so an acknowledged document revision is never half-published;
- recover conservatively after crash boundaries;
- return `resync_required` for expired delta cursors;
- never store a simplified reserialized document as canonical truth.

## 8. Sibling integration

| Substrate | DOCSeye consumes | DOCSeye does not steal |
| --- | --- | --- |
| SHELLeye | exact physical-file observations, rename/replacement correlation, locks, atomic replacement | semantic document identity or structure |
| DESKTOPeye | Word window/UI correlation and any truly UI-only operation | paragraph, comment, table, or layout semantic authority |
| CODEeye | repository/source correlation for engineering documents | source symbols, diagnostics, or code actions |
| eyeBROWSE | live-page correlation when a document derives from web state | live DOM/network/navigation ownership |

Build 001 must remain testable with deterministic adapters if a sibling service is unavailable. The substitute may emulate transport, not relax identity or atomicity semantics.

## 9. Security and active content

- DOCSeye does not execute macros, ActiveX, OLE payloads, embedded binaries, or external relationships.
- A `.docm` preservation experiment may copy VBA parts unchanged; it is not a macro-execution feature.
- Encrypted Office content remains inaccessible until an authorized provider supplies the necessary access; the system reports this truthfully.
- Signature coverage and impact are inspected. A mutation that changes signed coverage must not be reported as preserving signature validity.
- Program Host programs receive a capability-bounded SDK, not arbitrary kernel memory or an uncounted raw-ZIP escape hatch.
- Logs and errors must avoid dumping full sensitive document content by default.

## 10. Reproducibility record

The eventual results document must record:

- Git freeze and implementation commit SHAs;
- all exact toolchain/provider versions;
- fixture hashes and adversary version;
- Word update channel/build and installed-font manifest;
- operating-system and relevant printer/layout configuration;
- every experiment and hostile-case result;
- benchmark machine configuration;
- known provider normalization profiles;
- deviations from this frozen plan and the decision that authorized each deviation.

Until that record exists, platform compatibility and Build 001 acceptance remain **NOT RUN**.
