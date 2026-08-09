# 03 — STEALTHEYELLC Platform

Status: **CANONICAL PREIMPLEMENTATION PLATFORM PROFILE**  
Observation date: **2026-08-09**

This document separates measured host facts from implementation prerequisites. No dependency was installed or changed during the architecture freeze.

## 1. Measured facts

### 1.1 Node runtime

Installed absolute directory:

`C:\AgentBrowser\tools\node-v24.18.1-win-x64\`

Measured executables:

| Tool | Version | Availability |
| --- | --- | --- |
| Node | `v24.18.1` | usable by absolute path |
| npm | `11.16.0` | usable by absolute path |
| npx | `11.16.0` | usable by absolute path |

The directory is not on machine or user `PATH`. Build 001 must invoke the executable by exact absolute path or use a process-local path. A global PATH change is neither required nor authorized by this freeze.

Node 24 is an LTS line. The measured runtime is valid evidence for the Program Host baseline; implementation preflight must still record its binary digest and verify required APIs.

### 1.2 Microsoft Word/Office

Measured:

- desktop Microsoft Word is not installed;
- no `WINWORD.EXE` was found;
- no `Word.Application` COM registration was found;
- no Office Click-to-Run desktop installation was found;
- no Microsoft 365 trial has been started.

This state is deliberately compatible with native Build 001. It is not a degradation of native capability and must remain the A–D acceptance condition.

## 2. Provisioning classification

| Component | Classification at freeze | Build 001 role |
| --- | --- | --- |
| Node `v24.18.1` / npm `11.16.0` | **ALREADY PRESENT** | one non-agentic Program Host invocation by absolute path |
| desktop Microsoft Word / COM | **NOT PRESENT / NOT REQUIRED** | zero use in native A–D and DOCX X supplement |
| Microsoft 365 trial | **NOT STARTED / NOT REQUIRED** | must not be started for native acceptance |
| .NET 10 LTS SDK/runtime | **IMPLEMENTATION MUST VERIFY/PROVISION** | C# kernel and provider contracts |
| supported SQLite native library | **IMPLEMENTATION MUST VERIFY/PROVISION** | DND container and external runtime indexes |
| deterministic CBOR implementation(s) | **IMPLEMENTATION MUST PROVISION** | primary codec plus independent canonical-vector implementation |
| Typst | **IMPLEMENTATION MUST PROVISION** | first paginated/PDF provider candidate |
| Chrome for Testing/Chromium | **IMPLEMENTATION MUST PROVISION/PIN** | HTML/accessibility projection and browser checks |
| Open XML SDK | **IMPLEMENTATION MUST PROVISION** | typed OOXML interpretation/validation beside raw OPC/XML |
| LibreOffice | **IMPLEMENTATION MUST PROVISION FOR X-04** | alternate-provider smoke only; never Microsoft evidence |
| PDF validator (for example veraPDF where profile support is suitable) | **IMPLEMENTATION MUST PROVISION/PIN** | independent tagged/PDF-UA validation |
| fonts and hyphenation data | **IMPLEMENTATION MUST PROVISION/PIN** | qualified LayoutRevision and typography fixture |
| SHELLeye integration surface | **IMPLEMENTATION MUST VERIFY** | carrier identity, coherent snapshot, and publication |

“Must provision” is a later implementation prerequisite, not authorization to install during synthesis.

## 3. Version-selection rule

At Build 001 preflight, select the latest stable supported patch compatible with the frozen major/platform choices, then record:

- exact version and release channel;
- upstream source/release URL;
- package/binary digest and architecture;
- license;
- relevant security/advisory status;
- configuration, locale, timezone, fonts, hyphenation data, and environment variables;
- whether the component is product, provider, validator, or test-only.

Pin acceptance after its early qualifying experiment. Do not freeze an incidental minor version merely because a research pass observed it.

Current external reference points on 2026-08-09 are SQLite 3.53.4, Typst 0.15.1, Open XML SDK 3.5.1, .NET 10 LTS, and Node 24 LTS. These are re-verification facts, not package manifests.

## 4. Native artifact profile on Windows

The DND is a SQLite application file using rollback-journal `DELETE` mode. A quiescent portable artifact is one main file with no required `-wal`/`-shm` companion. A live database file must never be copied as though quiescent: implementation uses SQLite-supported snapshot/backup behavior and SHELLeye coordination.

External rebuildable runtime/index databases use WAL and may create `-wal` and `-shm`. Those files are operating state, not part of portable semantic truth. Milestone A deletes the entire runtime directory.

Build 001 uses a provisional `.dnd` suffix. File associations, MIME registration, installer behavior, and a public extension decision are outside acceptance.

## 5. Process and isolation requirements

The acceptance harness must record all child processes and network requests. Native open/validate/query/mutate/layout/render must:

- launch no Word process;
- activate no Word COM/API;
- execute no macro, OLE, ActiveX, extension payload, field code, or foreign script;
- perform no implicit remote asset/external-relationship fetch;
- isolate Typst, browser, LibreOffice, and independent mutator temp/cache directories;
- pin and report provider executable paths and digests;
- preserve old/new artifacts around crash injection for postmortem validation.

## 6. Renderer environment

Every `LayoutRevision` and accepted render records:

- semantic revision/root and layout profile;
- provider name/version/binary digest;
- complete font manifest/digests and fallback order;
- locale, language, writing mode, and hyphenation data/version;
- relevant page/PDF/HTML configuration;
- warnings, unsupported capabilities, and source-map digest.

The browser test uses a pinned non-auto-updating Chrome for Testing or equivalent pinned Chromium build. Typst source is generated into test-only derived state. Neither source nor DOM is canonical.

## 7. Word-free gate

For native A–D and provider X-01–X-04:

| Counter | Required |
| --- | ---: |
| required `WINWORD.EXE` | 0 |
| observed Word processes | 0 |
| Word COM activations/calls | 0 |
| Word API calls | 0 |
| Word-produced acceptance artifacts/oracles | 0 |

If a test attempts Word use, the native acceptance run is invalid rather than “degraded.” A future conditional Microsoft suite runs elsewhere and records a fully qualified Microsoft environment.

## 8. Repository-state facts after Build 001 acceptance

The freeze-time statements in Git history have now changed through the authorized Build 001 work:

- Product source: present under `src/`.
- Test/acceptance projects: present under `tests/`.
- Deterministic N-001 and F-001 generators/manifests: present; generated runtime artifacts remain ignored.
- Dependency/environment evidence: captured in `evidence/preflight.json` and `evidence/final-environment.json`.
- `docs/09-BUILD-001-RESULTS.md`: present after all parent gates passed.
- Build 001: implemented and accepted.
- Acceptance: clean reproduction PASS; receipt in `evidence/reproduction-run.json`.

Microsoft Word remained absent throughout native acceptance. Provider/version/font/locale claims are scoped to the final environment evidence and must not be generalized beyond it.
