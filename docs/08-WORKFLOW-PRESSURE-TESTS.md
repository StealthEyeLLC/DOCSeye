# 08 — Workflow Pressure Tests

Status: **CANONICAL ARCHITECTURE PRESSURE TESTS / NOT EXECUTED**

These workflows test authority, identity, failure, and capability honesty. They do not claim implementation results.

## 1. Create a native professional document

**Setup:** no provider source; create a native family/branch and the N-001 ontology features.  
**Required path:** typed objects, roles/styles, assets/math, fields/controls, layout intent, one atomic initial revision/root, then HTML/PDF providers.  
**Pressure:** artifact and runtime state are separated; output provider warnings are visible.  
**Success:** native semantics remain complete without Typst/Chromium/Word; renders attribute exact source/layout revision.  
**Failure:** generated Typst/HTML becomes editable truth, pages become semantic IDs, or Word absence degrades native support.

## 2. Edit a retained span across revisions

**Setup:** two overlapping retained ranges, co-located independent boundaries, repeated quotation, combining/emoji/bidi text.  
**Required path:** explicit edge policy; insert/delete/replace; split/merge owner; consume deltas.  
**Pressure:** no permanent transaction history and no text-similarity recovery.  
**Success:** exact live/collapsed/orphaned/destroyed/multi-interval states and zero wrong anchor mutation.  
**Failure:** a comment rebounds to repeated text, a cross-reference retargets, or an ordinary read rewrites the document.

## 3. Reorder duplicate table rows

**Setup:** two visually and byte-equivalent rows with distinct row/cell IDs.  
**Required path:** query exact IDs, move one row, insert another, duplicate one, merge/split cells.  
**Pressure:** coordinates and visible values are indistinguishable evidence.  
**Success:** moved IDs survive, copied IDs remint, cell lifecycle is exact, header/topology remains valid.  
**Failure:** coordinate/value matching mutates the wrong row or a copied cell aliases its source.

## 4. Raw replica divergence, fork, and merge

**Setup:** copy `A.dnd` byte-for-byte to `B.dnd`, then commit independently.  
**Required path:** SHELLeye distinguishes carriers; DOCSeye observes common branch/base and divergent heads; writes stop; caller selects fork or merge.  
**Pressure:** embedded IDs necessarily cloned and timestamps/paths appear tempting.  
**Success:** `divergent_heads`; fork remints all public IDs/boundaries; merge records explicit mappings/bounded parents.  
**Failure:** last-writer wins silently, path defines branch, open mutates IDs, or source handle actuates fork.

## 5. Copy/paste and template instantiation

**Setup:** section with comments, fields, controls, figures sharing an asset, retained anchors, and provider facets.  
**Required path:** within-branch copy, cross-branch paste, cross-family paste, and template instantiate.  
**Pressure:** deep subgraph references and optional/required unknown facets.  
**Success:** every occurrence/boundary remints; asset digest may share; facets transform exactly or refuse/declare omission; provenance bounded.  
**Failure:** copied comments/controls alias source, unknown required meaning drops, or byte equality collapses families.

## 6. Comments and suggestions under deletion

**Setup:** open/resolved threads and all six suggestion kinds over retained targets.  
**Required path:** partial deletion, whole-target deletion, accept/reject, owner deletion, reply/resolution.  
**Pressure:** quoted text remains elsewhere.  
**Success:** comments orphan conservatively, suggestions apply/reject atomically then retire, no accepted-change ledger.  
**Failure:** quote rebound, accepted suggestion mistaken for revision history, or partial review mutation commits.

## 7. Global style change without semantic-role drift

**Setup:** headings/body/list/table styles using theme tokens plus direct override.  
**Required path:** change one theme token; query effective style/provenance; render both profiles.  
**Pressure:** direct override and media profile must resolve deterministically.  
**Success:** heading roles/IDs remain; affected effective styles and layout invalidations are exact.  
**Failure:** font change reclassifies heading, full CSS ambiguity, or provider computed formatting overwrites native sources.

## 8. Full repagination

**Setup:** early text edit before large table, footnote, wrapped figure, headers/footer, and page-count field.  
**Required path:** semantic commit invalidates layout; provider creates a new LayoutRevision and render.  
**Pressure:** most pages move while semantics mostly survive.  
**Success:** semantic IDs stable; old pages remain scoped to old layout; page field is stale then qualified; source mapping exact.  
**Failure:** pages acquire semantic continuity, stale layout is labeled current, or render uses wrong source revision.

## 9. Render accessible HTML and PDF

**Setup:** N-001 typography/accessibility fixture under pinned fonts/providers.  
**Required path:** semantic mapping to HTML and Typst, browser/accessibility check, tagged PDF/UA-1 validation, source maps.  
**Pressure:** Arabic/bidi, CJK, Indic, combining, emoji, fallback, table headers, note order, alt text.  
**Success:** 100% required-object mapping; output/profile warnings reported; Word counters remain zero.  
**Failure:** output existence is called professional without validation, tags/source attribution missing, or Word-generated oracle is hidden.

## 10. Import an adversarial DOCX

**Setup:** F-001 contains mapped, partially mapped, unknown MC/custom, OMML, controls/revisions/comments, and inert active content.  
**Required path:** raw OPC/OOXML inspection, typed mapping, scoped provider facets, exact source capsule, capability report, native conversion commit.  
**Pressure:** importer cannot honestly normalize all features.  
**Success:** every feature classified; native IDs minted; original bytes remain evidence only; no active content executes.  
**Failure:** unsupported part disappears, provider ID becomes native ID, or capsule silently retains semantic authority.

## 11. Edit supported imported content

**Setup:** converted F-001 with exact capsule and mappings.  
**Required path:** edit a supported paragraph/list/table/comment disjoint from unknown coverage; update alignment; plan preserved patch.  
**Pressure:** untouched foreign data must survive while native edit wins semantically.  
**Success:** DND is sole current truth; source capsule immutable; disjoint provider facets preserved; export outcome exact.  
**Failure:** source bytes overwrite native edit, whole DOCX normalizes unnecessarily, or report says exact source reuse after a semantic change.

## 12. Encounter an unsupported provider feature

**Setup:** requested edit intersects an unknown required text/topology facet.  
**Required path:** evaluate extension/provider coverage before mutation.  
**Pressure:** payload can be preserved but not safely transformed.  
**Success:** `blocked_required_extension`/`blocked` with scope/reason; zero commit.  
**Failure:** edit proceeds, payload drops, scope escapes, or “best effort” is reported as conformant.

## 13. Export with a capability report

**Setup:** one untouched converted state, one disjoint edited state, and one unsafe-intersection request.  
**Required path:** exact source reuse, preserved patch, translation/loss/refusal as appropriate; package/schema and LibreOffice observation.  
**Pressure:** schema validity, alternate provider, and Microsoft behavior are distinct.  
**Success:** one of six semantic outcomes plus separate evidence; no Boolean high-fidelity claim; no `microsoft_observed`.  
**Failure:** LibreOffice is called Word evidence, schema validity is called Microsoft fidelity, or unknown loss is hidden.

## 14. Large local Program Host workflow

**Setup:** N-001, independent external writer checkpoint, and exact 96-call script.  
**Required path:** 18 queries, 48 mutations/16 families, 3 commits, 6 delta calls, 6 checks/refusals, 4 reconciliation, 4 layout/render, 4 export/capability; one invocation.  
**Pressure:** no intermediate model, no raw SQL/CBOR/OOXML escape, stale and extension refusal branches.  
**Success:** exact call arithmetic, 48 requested mutations, zero unrequested, exact external reconciliation, HTML/PDF/DOCX plan/report.  
**Failure:** padding/no-op calls, hidden giant raw script, model re-entry, lost delta, or uncounted semantic mutation.

## 15. Runtime loss and index rebuild

**Setup:** committed 5,000-page-equivalent DND with FTS/accelerators/caches.  
**Required path:** delete runtime directory, reopen artifact, rebuild, query/edit one paragraph, expire one cursor.  
**Pressure:** runtime previously contained all accelerators.  
**Success:** identity/root exact; indexes rebuild; local edit remains local; expired cursor resyncs explicitly.  
**Failure:** IDs remint, semantics depend on FTS/cache, whole-document response is required for every edit, or delta gap is silent.

## 16. Crash and ambiguous response

**Setup:** multi-object transaction with object, text, extension, root/head, and 1 GiB streamed-asset writes.  
**Required path:** inject failures at every publication stage and lose one post-commit response.  
**Pressure:** rollback journal/hot state and carrier publication may be in flight.  
**Success:** old or new whole revision only; bounded idempotency query prevents replay; SHELLeye publishes coherently.  
**Failure:** mixed root/data, acknowledged partial, hot journal discarded, or retry duplicates mutation.

## 17. Invalid native artifact and explicit repair

**Setup:** duplicate object/boundary IDs, broken reference, corrupt asset/extension, invalid root, malformed CBOR.  
**Required path:** layered validation, non-write classification, explicit repair proposal.  
**Pressure:** a superficially plausible object could be substituted.  
**Success:** invalid artifact never writable; no identity silently minted; uncertain repair creates new branch/artifact with provenance.  
**Failure:** validator regenerates IDs/root and proceeds as if continuity were proved.

## 18. Capability-unavailable versus unsupported

**Setup:** native document on STEALTHEYELLC with Word absent, plus a request for native render and a separate Microsoft-specific observation.  
**Required path:** native render uses Typst/HTML; Microsoft request reports provider unavailable.  
**Pressure:** product status dashboards often collapse provider absence into general degradation.  
**Success:** native capability remains supported; only Microsoft scope is `unavailable_provider`.  
**Failure:** Word becomes a hidden native dependency or unsupported is falsely reported as temporary absence.

## 19. Pressure-test completion rule

A workflow passes only when requested outcomes, non-outcomes, authority state, revision/root, target IDs, deltas, capability classifications, and hard metrics all match. A plausible render or semantically similar result is insufficient. These workflows become results only after implementation evidence is published.
