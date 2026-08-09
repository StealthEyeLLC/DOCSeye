# 00 — Charter

Status: **FINAL / FROZEN FOR BUILD 001**
Operator: **ChatGPT**
Product implementation: **NOT STARTED**

## 1. Mission

DOCSeye gives ChatGPT a persistent, programmable document world without fabricating a universal document model. It makes useful document concepts conservatively addressable across revisions and provider representations, executes exact structural transactions, preserves unsupported native content, and returns compact semantic state instead of requiring repeated full-document rediscovery.

DOCSeye is designed for ChatGPT as the product operator. It is not optimized around a human editor UI, selector authoring, macro recording, or a human-facing document-management workflow.

## 2. Five project constraints

1. **ChatGPT is the operator.** The normal interface is compact typed state, bounded deltas, exact actions, waits, and local programs—not a human productivity UI.
2. **Native systems retain authority.** OOXML, Word, PDF, ODF, HTML, layout engines, renderers, cloud providers, and sibling StealthEye substrates keep representation-specific truth. DOCSeye owns correspondence and transaction semantics above them.
3. **Identity must be conservative.** Logical IDs never imply a current exact binding. Position, path, text, visual similarity, and provider IDs cannot be promoted beyond their documented scope. Ambiguity stops mutation.
4. **Preservation is a correctness boundary.** An accepted edit changes only its declared semantic-effect envelope and serialization footprint. Unsupported surrounding provider truth survives or the operation is rejected/routed elsewhere.
5. **Operating state is bounded.** DOCSeye is not a permanent action ledger, document version-control system, provenance service, universal graph database, approval service, or duplicated canonical document store.

## 3. Provider-truth principle

> Each provider owns the truth native to the representation it defines. DOCSeye owns conservative logical correspondence, temporal coherence, exact targeting, and agent continuity among those truths.

The package provider owns stored OPC/XML truth. Word owns current unsaved Word state and Word-specific application behavior. A layout engine owns one pagination result. PDF owns stored fixed-layout structures. OCR owns an explicitly inferred observation, not native document semantics.

Normative standards and current application behavior may disagree. DOCSeye records both rather than flattening one into the other.

## 4. No-fake-identity principle

> Loss of continuity is acceptable. False continuity is not.

A retained concept can be `exact_current`, `exact_rebased`, `stale`, `ambiguous`, or `destroyed`. Its logical ID may continue to exist without a current mutation-authorizing binding. Text equality, path, paragraph ordinal, table coordinate, character offset, page number, visual similarity, and embeddings are never sufficient alone for destructive mutation.

## 5. Preservation principle

> Unsupported provider truth outside an accepted operation's declared mutation closure survives.

Preservation is measured at package, relationship, XML, semantic, native-feature, layout, and application-acceptance layers. Whole-ZIP byte equality is not required; unexplained semantic mutation or unsupported-content loss is forbidden.

## 6. Domain boundaries

| Substrate/provider | Owns |
| --- | --- |
| DOCSeye | logical documents and retained semantic concepts; revisions; correspondence; typed document transactions; semantic indexes/deltas; semantic-layout correlation |
| SHELLeye | physical files, paths, physical-file continuity/replacement, processes, locks, atomic physical actuation, filesystem signals |
| DESKTOPeye | Word windows, dialogs, ribbon, focus, caret, and physical UI interaction |
| eyeBROWSE | live pages, browser DOM/network/navigation/application state |
| CODEeye | source, symbols, builds, tests, repositories, and engineering semantics |
| future DATAeye | workbook, cell, formula, table, pivot, and deep chart-data semantics |
| future presentation substrate | decks, slides, masters, layouts, animation, timing, and presenter semantics |

Static/saved HTML may be a DOCSeye representation; a live page remains eyeBROWSE. Markdown can participate in CODEeye or DOCSeye according to the requested semantics. Deep PPTX and XLSX semantics are not DOCSeye merely because they share OPC packaging.

Cross-substrate relations stay sparse and evidence-bearing. There is no universal StealthEye graph.

## 7. Explicit non-goals

DOCSeye is not:

- a universal document AST;
- a flat-text extraction/regeneration system;
- a Word-only automation layer;
- a generic rich-document SDK;
- a lossless XML patcher without correspondence semantics;
- a full graph database or duplicated document store;
- a document-management/version-history product;
- a human document editor;
- a workflow, policy, approval, or multi-agent scheduling service;
- a permanent action/provenance/event ledger;
- a universal converter;
- a custom Word-compatible layout engine.

Build 001 has narrower non-goals in `02-BUILD-001-SLICE.md`.

## 8. Architecture authority order

When sources disagree, use this order:

1. `docs/AUTHORITY.md` and the current canonical repository state;
2. `docs/00-CHARTER.md` for mission and constraints;
3. `docs/01-ARCHITECTURE.md` for permanent semantics;
4. `docs/02-BUILD-001-SLICE.md` for Build 001 gates;
5. numbered decisions in `docs/06-DECISIONS.md`;
6. other canonical numbered documents;
7. current primary standards/API evidence;
8. implementation experiments and measured results after they are deliberately promoted;
9. the three independent research reports as historical input evidence.

Implementation evidence that falsifies a frozen mechanism must update the affected canonical document and decision. It must not create a contradictory shadow specification.
