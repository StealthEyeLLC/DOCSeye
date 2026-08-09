# DOCSeye Authority

Status: **Canonical**

## Source hierarchy

1. The current `main` branch of `StealthEyeLLC/DOCSeye` is the project source of truth.
2. `00-CHARTER.md` governs mission, constraints, and domain boundaries.
3. `01-ARCHITECTURE.md` governs permanent architecture and semantics.
4. `02-BUILD-001-SLICE.md` governs Build 001 scope, experiments, fixtures, cases, metrics, and completion.
5. `06-DECISIONS.md` records the reasons and rejected alternatives for frozen choices.
6. The remaining numbered documents provide platform, roadmap, evidence, capability, and workflow detail.
7. GitHub issues may restate and track canonical requirements but cannot silently override the documents above.

## Freeze authority

The documentation-only `main` commit that contains this file and the complete corrected `docs/00`–`docs/08` baseline, recorded in parent issue #5 as the `architecture-freeze` SHA, is the canonical pre-implementation architecture freeze. Its exact SHA must also be included in the publication completion report.

## Implementation discipline

A separate implementation tab must begin by reading, in order:

1. `README.md`;
2. `docs/00-CHARTER.md`;
3. `docs/01-ARCHITECTURE.md`;
4. `docs/02-BUILD-001-SLICE.md`;
5. `docs/03-PLATFORM-STEALTHEYELLC.md`;
6. `docs/06-DECISIONS.md`;
7. GitHub issues #1–#5.

The independent research reports supplied on 2026-08-09 remain valuable evidence, but they are no longer direct implementation authority after the freeze. Where they disagree with this repository, this repository governs.

Build 001 is not complete until all four child workstream issues pass their measured gates, parent issue #5 closes, and a measured `docs/09-BUILD-001-RESULTS.md` is deliberately added. Architecture-freeze language must never be reported as product implementation or acceptance.
