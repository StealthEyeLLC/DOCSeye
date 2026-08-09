# 09 - Build 001 Results

Status: **ACCEPTED**

Architecture freeze: `0b10e8ed6ada2e0aaef3f1196de19bf7a4631bec`

Accepted architecture family: **NATIVE SEMANTIC AUTHORITY WITH NON-AUTHORITATIVE PROVIDER FACETS**

Prior DOCX-first baseline: `47b5280b71773ae497b5a56c4b590b9070b4bca5` - retained as a superseded architecture baseline, not relabeled as a failed implementation.

## 1. Acceptance statement

DOCSeye Build 001 is accepted against the frozen native-authority contract. The final clean reproduction run passed every required experiment and milestone from a cleaned runtime-artifact state, with a zero-warning/zero-error build, no Microsoft Word dependency, and no architecture falsifier triggered.

The canonical reproduction command is:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/run-build001-acceptance.ps1 -CleanRuntimeArtifacts
```

The final reproduction receipt is `evidence/reproduction-run.json`. It records `PASS`, the SHA-256 and byte length of every required evidence file, build warnings/errors of `0/0`, Program Host raw-escape hits of `0`, and the Word-zero gate. Runtime artifacts can be removed without touching tracked source/evidence with:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/run-build001-acceptance.ps1 -CleanupOnly
```

The last clean reproduction run completed successfully from a cleaned `artifacts/` directory. Generated runtime/render/package artifacts remain ignored; their authoritative acceptance facts are captured in tracked evidence.

## 2. Parent gate summary

| Gate | Measured result |
| --- | --- |
| E-01 through E-20 | **PASS / qualified PASS**. Every experiment evidence row records its frozen question, measured bounded conclusion, and required failure action. |
| Milestone A | **PASS** - portable identity/recovery, divergence/fork/merge/copy policy, invalid artifact handling. |
| Milestone B | **PASS** - three atomic transactions, exact retained-boundary behavior, deltas/refusals, attributable HTML and PDF/UA-1. |
| Milestone C | **PASS 32/32** - all hard-zero H-01..H-18 = 0; all positive P-01..P-07 = 100%. |
| DOCX supplement X | **PASS 4/4** - exact source reuse/bounded patch, required-unknown blocking, Word-free package/schema validation, separately scoped LibreOffice observation. |
| Milestone D | **PASS 96/96** - one Node 24 invocation, 48 mutations, three commits, six delta calls, two material renders, one material DOCX export, zero intermediate model calls. |
| Scale | **PASS** - 10/500/5,000 page-equivalent locality and 1 GiB streamed-asset measurements recorded with no invented threshold. |
| Comparative benchmark | **PASS; F-09 not triggered** - native operational correspondence advantage measured together with its format/runtime costs. |
| Word independence | **PASS** - WINWORD executable/process/COM/API/Word-produced acceptance artifacts all zero; Microsoft 365 trial not started. |

## 3. Experiments E-01 through E-20

Primary evidence is split into four tracked files: `evidence/early-experiments.json`, `evidence/experiments-e05-e11.json`, `evidence/e12-e13-early.json`, and `evidence/experiments-e14-e20.json`.

| ID | Result | Bounded conclusion |
| --- | --- | --- |
| E-01 | PASS | Scalar-position retained boundaries, explicit affinity, deterministic delete/split/merge transforms, Unicode 17 grapheme handling, cold restart, and independent-writer mutation remained exact without permanent character history. |
| E-02 | PASS | Rollback-journal fault cuts recovered old-or-new truth only; no acknowledged partial transaction; idempotency stayed exact. |
| E-03 | PASS | Quiescent DND copies remain coherent snapshots; identity-changing copies remint as required; live publication is coordinated rather than main-file-only copying. |
| E-04 | PASS | Extension coverage permits safe disjoint edits, requires declared transforms, refuses unsafe intersections, and preserves exact payloads. |
| E-05 | PASS | Sparse 128-bit order behavior stayed local/deterministic through 5,000-page-equivalent pressure; maintenance rebalance was semantic-root neutral. |
| E-06 | PASS | C# and independent Node canonical-CBOR vectors were byte-identical for valid vectors and agreed on invalid classifications. |
| E-07 | PASS | Domain-separated current-state root stayed deterministic across independent construction, VACUUM/reorder/index rebuild, with bounded local hash-path recomputation. |
| E-08 | PASS | Independent public-spec writer commits were accepted without private repair or identity remint; malformed variants were rejected. |
| E-09 | PASS | Local query/edit remained bounded at 10, 500, and 5,000 page-equivalent tiers without whole-document materialization. |
| E-10 | PASS | A 1 GiB embedded asset was streamed with bounded semantic touch, preserved figure identity, and old-or-new crash behavior. |
| E-11 | PASS | Total runtime-index loss rebuilt exactly from DND; expired delta cursors returned explicit `resync_required`. |
| E-12 | PASS_EARLY_QUALIFICATION | Pinned Typst expressed the required early layout/reflow fixture and returned exact semantic source mappings. |
| E-13 | PASS_EARLY_QUALIFICATION | Base and reflow PDFs independently validated as PDF/UA-1 with zero failed rules/checks under the recorded font/provider environment. |
| E-14 | PASS | Pinned Chrome via local CDP produced deterministic continuous HTML, 64/64 source attribution, expected AX roles, and zero external resource fetches. |
| E-15 | PASS | Adversarial F-001 import classified every feature and retained exact source-capsule evidence without dual authority. |
| E-16 | PASS | Native state exported a schema-valid, Word-free DOCX with an explicit translation outcome and declared losses rather than a false fidelity claim. |
| E-17 | PASS | LibreOffice alternate-provider behavior was observed and normalization/loss was reported separately from DOCSeye translation truth. |
| E-18 | PASS | Malformed/resource-limit corpus refused unsafe SQLite/CBOR/XML/ZIP paths with bounded behavior and zero active/remote execution. |
| E-19 | PASS | Generation-1 evolution vectors preserved optional unknown data and blocked unsupported required capability intersections. |
| E-20 | PASS | Native Presentation MathML remained authoritative while TeX/OMML facets retained explicit precedence/staleness semantics. |

No experiment failure action was required because no corresponding falsifier condition was observed. The exact failure action remains embedded in each experiment evidence row.

## 4. Deterministic fixtures and independent implementation

### N-001 native fixture

- deterministic generator: `tests/DOCSeye.Fixtures/N001Generator.cs`
- manifest: `fixtures/N-001.manifest.json`
- family: `bc279791-084b-4090-baec-dcc6dca35f11`
- semantic root: `d85817cacb067521e60b75f1082ee0b18519a3238ea4c2c85359222d679c9879`
- objects: 148
- retained ranges: 6

### F-001 adversarial foreign DOCX fixture

- deterministic generator: `tests/DOCSeye.Fixtures/F001Generator.cs`
- manifest: `fixtures/F-001.manifest.json`
- generated package SHA-256: `bba9fea0dde6f1a9e93d1019434e54ce37108258c268827df83cb991508b1a5a`
- generated package size: 16,830 bytes
- required classified feature count: 25

The generated `.docx` remains an ignored runtime fixture by repository policy; the deterministic generator and digest manifest are tracked source truth.

### Independent native writer

`tools/independent-writer/independent-writer.mjs` implements public-format operations independently of product-private serializers. It is used for cross-language canonicalization, valid external commits, malformed vectors, and Milestone D's one external public-spec mutation.

## 5. Milestone A

`evidence/milestone-a.json` records PASS for the native identity/recovery slice. The accepted suite covers unchanged recovery, cold restart, coherent carrier movement, exact external descendants, divergence/fork behavior, cross-branch copy/paste policy, merge, independent duplicate/template policy, and invalid-artifact refusals.

Kernel/provider death and rebuildable runtime-state loss did not create semantic discontinuity. Identity authority remained in the portable DND artifact.

## 6. Milestone B

`evidence/milestone-b.json` records PASS.

- three coherent semantic transactions committed atomically;
- one independent external descendant reconciled between transactions;
- stale and required-extension paths refused exactly;
- all six canonical retained-range topologies retained their required identity/lifecycle behavior;
- exactly one declared final-render exemption exists: the retired source object deliberately consumed by the B-T3 split/merge operation;
- required final render mapping: 64 objects;
- HTML mapping: 100%;
- PDF mapping: 100%;
- base PDF: 3 pages;
- final PDF: 4 pages;
- reflow affected the required table, footnote, header/footer occurrence behavior, and page-field mapping;
- final PDF independently validated as PDF/UA-1.

## 7. Milestone C and DOCX supplement X

### C-01 through C-32

`evidence/milestone-c.json` records **PASS 32/32**. Every case contains its setup/adversary, expected and actual classification/result, artifact and revision/root evidence, target IDs, provider profile where relevant, and metric observations.

Hard-zero metrics H-01 through H-18 all equal `0`. Positive metrics P-01 through P-07 all equal `100%`.

The matrix covers identity/copy/fork behavior, exact/stale/divergent writers, text split/merge and retained anchors, review/comment/control/reference behavior, assets/facets/extensions, grapheme boundaries, transaction fault points, idempotency expiry, parser limits, runtime-index rebuild/delta expiry, HTML/PDF correspondence, font-manifest requalification, reflow, and required provider-output validation.

### X-01 through X-04

`evidence/milestone-x.json` records **PASS 4/4**.

- X-01: all 25 F-001 features classified; exact source capsule retained.
- X-02: exact source reuse first, then one bounded preserved patch; untouched required entries remained exact.
- X-03: required unknown intersection blocked instead of being silently overwritten.
- X-04: source DOCX package/schema validation passed and LibreOffice was used only as `alternate_provider_observed` evidence.

LibreOffice's resave normalized many entries, removed opaque/media parts, added its own parts, and produced schema diagnostics. Those observations are deliberately reported rather than promoted to DOCSeye fidelity or Microsoft evidence. They do not invalidate the schema-valid source DOCX produced/validated by DOCSeye.

## 8. Milestone D - exact 96-call Program Host workflow

`evidence/milestone-d.json` records **PASS 96/96** from one Node 24 Program Host invocation with zero intermediate model calls.

| Class | Count |
| --- | ---: |
| Q query/inspect/plan | 18 |
| M semantic mutations | 48 |
| T transaction controls | 6 |
| G delta consumption | 6 |
| V postcondition/refusal | 6 |
| R external reconciliation | 4 |
| L layout/render | 4 |
| O DOCX capability/export | 4 |
| **Total** | **96** |

The 48 mutation IDs appeared exactly once across three 16-mutation, expected-revision-conditioned atomic transactions and their typed semantic deltas. The final measured positive metrics were:

- P-01 recovery: 32/32;
- P-02 requested postconditions: 48/48;
- P-03 refusal classifications: 2/2;
- P-04 crash outcomes: 7/7, sourced from the accepted C crash matrix;
- P-05 required extension/facet preservation: 1/1;
- P-06 committed mutation delta coverage: 48/48;
- P-07 required render correspondence: 64/64.

H-01 through H-18 were all zero.

The stale-revision and required-extension refusal branches wrote zero artifact bytes. One independent public-spec external commit reconciled as an exact descendant, recovered 32/32 sentinels, rebuilt runtime index fragments, and required no full semantic rediscovery. The two material renders were accessible HTML and tagged PDF/UA-1; both carried the final semantic revision/root attribution. The one material DOCX export was OPC-valid and schema-valid and reported `translated_with_declared_loss`, not a Microsoft observation.

A source audit over `ProgramHost*.cs` and `tools/program-host/program-host.mjs` found zero direct raw CBOR, SQL, ZIP/XML/OOXML, or VBA escape paths. Typed delta parsing is below the Host boundary in storage/session APIs.

## 9. Scale measurements

No aggressive latency or memory threshold was frozen; these values are evidence, not a performance promise.

### E-09 local document tiers

| Page-equivalent tier | DND bytes | Cold validation | Local query | Local edit | Logical records read/touched | Merkle path nodes | Payload read / written | Whole-document local materialization |
| ---: | ---: | ---: | ---: | ---: | --- | ---: | ---: | --- |
| 10 | 352,256 | 2 ms | 0 ms | 4 ms | 1 / 1 | 33 | 3,591 / 3,604 B | no |
| 500 | 6,995,968 | 37 ms | 0 ms | 38 ms | 1 / 1 | 33 | 3,592 / 3,605 B | no |
| 5,000 | 68,329,472 | 397 ms | 0 ms | 55 ms | 1 / 1 | 33 | 3,593 / 3,606 B | no |

The measured local mutation did not require whole-document semantic materialization or rewrite at any tier.

### E-10 1 GiB asset

- asset bytes: 1,073,741,824;
- chunk size: 4 MiB;
- add: 3,002 ms in the final reproduction;
- replace: 2,187 ms;
- figure identity preserved: yes;
- unrelated semantic records touched: 0;
- streaming path: yes;
- injected crash recovered old truth; successful run published new truth.

## 10. Comparative DOCX-first/native benchmark

`evidence/comparative-benchmark.json` runs the same 10-call logical workflow in isolated child processes against the native model and an executable reconstruction of the prior frozen DOCX-first operating model.

The reconstruction is explicitly labeled because commit `47b5280...` was architecture-only and never had an implemented product binary. No historical runtime result is invented. The reconstructed baseline uses DOCX as representation truth and a retained `w14:paraId` provider witness; after an external move it performs a full provider rescan to recover that witness.

Three cold-process samples per route were run in alternating order; medians are reported.

| Metric | Native | Reconstructed DOCX-first |
| --- | ---: | ---: |
| Initial artifact bytes | 9,162,752 | 582,098 |
| Program Host calls / invocations | 10 / 1 | 10 / 1 |
| Useful operations | 10 | 10 |
| Query calls | 2 | 2 |
| Full provider rediscoveries | **0** | **2** |
| Correspondence-recovery attempts after external move | **0** | **1** |
| Model-facing bytes | 5,069 | 1,919 |
| Semantic/operational delta bytes | 1,156 | 332 |
| Artifact representation diff bytes | **180,224** | **1,164,242** |
| Median workflow latency | 2,783 ms | 449 ms |
| Median peak working set | 219,947,008 B | 76,967,936 B |
| Exact target recovered | yes | yes |
| Wrong/stale/ambiguous accepted writes | 0 / 0 / 0 | 0 / 0 / 0 |

Touched-physical-record units differ by representation and are therefore reported, not falsely treated as identical units:

- native: 44 changed 4,096-byte DND file blocks, 180,224 bytes;
- reconstructed DOCX-first: 2 changed uncompressed OPC part payloads, 2,192,314 bytes.

The benchmark shows a real tradeoff. Native materially improves persistent correspondence operations: no full provider rediscovery, no candidate recovery after the external move, and substantially lower raw artifact rewrite amplification in the measured workflow. It also imposes real costs in this implementation: a much larger native artifact, higher latency, higher peak memory, more model-facing bytes, and a larger semantic delta.

**F-09 is not triggered.** The native model demonstrated a material safety/operational correspondence improvement, while its permanent format/runtime costs are reported rather than hidden. No performance threshold was invented after measurement.

## 11. Provider and environment evidence

`evidence/preflight.json` remains the historical initial dependency/environment capture. `evidence/final-environment.json` records the environment used for final acceptance and explicitly reports any drift since preflight.

Final provider evidence is limited to the recorded versions/hashes/configuration, fonts, locale, and validation profiles. It must not be generalized beyond them.

Key final binary facts include:

- SQLite native 3.53.4 - SHA-256 `ab57d0437795ecc757cb693f32ea224173fa9856594d95cfa6b5033e645cd1ec`;
- Typst 0.15.1 - SHA-256 `081217a463adb006f8894b44227fe4b9c9e91fc85f5463d5948e8370db9bb31e`;
- Chrome for Testing 151.0.7922.77 - SHA-256 `73c78878416b28aff13df2c464a6273781c39e32d8be8ef700dd65fbff2b271c`;
- LibreOffice 26.2.5.2 - SHA-256 `40ebd75ff8c2d4a228166ffa9ac13b97b947f7ddef67152ae45cf4c98c1ae5e5`;
- Eclipse Temurin JRE 25.0.4 - SHA-256 `1dfe0b08636bc74b56db5e246f038cfe67c18f567053373fa601d310f29ed9da`;
- veraPDF 1.30.2 CLI jar - SHA-256 `889075253fb9df4db5482efb8f8208fb3b4f2e00f5f7e1b1e31edf6fb4b69bb6`.

The final environment capture reports one binary change since the initial preflight snapshot: Chrome for Testing. E-14 and final acceptance are bound to the final Chrome version/hash above. No font drift was observed.

## 12. Microsoft Word boundary

Build 001 native A-D and X acceptance ran with:

- `WINWORD.EXE` present: no;
- WINWORD processes: 0;
- Word COM calls: 0;
- Word API calls: 0;
- Word-produced expected acceptance artifacts: 0;
- Microsoft 365 trial: NOT STARTED.

No result in Build 001 is labeled `microsoft_observed`. A future separately licensed Microsoft-specific interoperability suite may add qualified evidence without changing native semantic authority.

## 13. Reproduction, cleanup, and evidence index

The final acceptance runner performs restore/build, E-01..E-20, A/B/C/X/D, the comparative benchmark, final environment capture, Program Host raw-escape audit, Word-zero gates, and `git diff --check`.

Tracked primary evidence:

- `evidence/preflight.json`
- `evidence/early-experiments.json`
- `evidence/experiments-e05-e11.json`
- `evidence/e12-e13-early.json`
- `evidence/experiments-e14-e20.json`
- `evidence/milestone-a.json`
- `evidence/milestone-b.json`
- `evidence/milestone-c.json`
- `evidence/milestone-x.json`
- `evidence/milestone-d.json`
- `evidence/comparative-benchmark.json`
- `evidence/final-environment.json`
- `evidence/reproduction-run.json`

`evidence/reproduction-run.json` contains SHA-256/size receipts for the acceptance evidence generated by the clean run.

## 14. Known boundaries and non-claims

Build 001 does **not** claim:

- Microsoft Word compatibility or Microsoft-specific observation;
- broad DOCX compatibility beyond the frozen F-001/X and native-export boundary;
- lossless translation of all native semantics to DOCX;
- LibreOffice normalization as preservation authority;
- a general-purpose PDF identity/editing model;
- a native typesetter;
- cloud coauthoring or CRDT collaboration;
- Build 002 capability.

The native DOCX exporter intentionally reports `translated_with_declared_loss` for unsupported native constructs. The alternate LibreOffice resave observation demonstrates why alternate-provider behavior remains evidence, not native truth.

## 15. Final architecture decision

No frozen architecture falsifier triggered during Build 001.

The measured result therefore retains the frozen **Native Semantic Authority** architecture for post-Build-001 work. The historical DOCX-first baseline remains available in Git history and the comparative evidence records where it is materially cheaper in this implementation. Future work should improve measured native size/latency/memory costs without weakening intrinsic identity, exact-or-refuse targeting, extension safety, delta authority, provider separation, or truthful interoperability classification.