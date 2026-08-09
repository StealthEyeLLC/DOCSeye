# DOCSeye Native Format

Status: **NORMATIVE PREIMPLEMENTATION SPECIFICATION / FROZEN FOR BUILD 001**  
Format generation: **1**  
Build 001 filename suffix: **`.dnd` (provisional research suffix)**

## 1. Normative language

`MUST`, `MUST NOT`, `SHOULD`, `SHOULD NOT`, and `MAY` are normative. A mechanism explicitly assigned to a numbered Build 001 experiment is not yet a measured result; the invariant surrounding that mechanism remains normative.

This document specifies the canonical logical format and its authority semantics. It intentionally does not contain a SQL schema, generated code, fixture bytes, or an implementation-private object model.

## 2. Native artifact definition and authority

> A DOCSeye Native Document (DND) is a portable SQLite application file whose canonical logical state carries document-family and branch identity, the current revision and semantic root, public semantic-object and retained-boundary identities, containment and order, UTF-8 text, relations, styles and themes, comments and suggestions, fields and controls, notes, figures, assets, math, layout intent, required extension records, bounded lifecycle and lineage witnesses, optional provider capsules/facets, and integrity metadata; in native mode it is the sole editable semantic authority.

### 2.1 Authority map

| Authority | Owner | Normative meaning |
| --- | --- | --- |
| Semantic authority | current validated DND logical state | Sole authority for current authored meaning in native mode. |
| Provider-source evidence | immutable capsule bytes plus digest and provenance | Evidence of what was imported; after conversion it never overrides native semantics. |
| Provider-specific semantic facet | scoped mapped or opaque provider record | Preserves provider-only meaning and may constrain/refuse export or an intersecting edit; it never silently overrides current native semantics. |
| Layout authority | native layout intent | Authored pagination/placement intent; a concrete realization is derived. |
| Layout realization | qualified layout provider and `LayoutRevision` | Authority only for the page/region mapping produced under its declared environment. |
| Render authority | immutable render plus source attribution | Authority only for those output bytes and their declared validation; it is not editable semantic truth. |
| Physical carrier authority | SHELLeye | Authority for carrier identity, coherent snapshot, movement, and publication; never semantic family/branch/object identity. |

Untouched provider facets MAY force an export to reuse preserved source material, apply a bounded patch, declare loss, or refuse. They MUST NOT overwrite, mask, or take precedence over current native semantics.

### 2.2 Explicit modes

- **native-authored**: a DND is authoritative from birth;
- **converted/imported**: a DND becomes authoritative at the explicit conversion commit; the source may be retained as non-authoritative evidence;
- **foreign-managed**: an intentionally unconverted provider artifact, initially DOCX, remains authoritative and uses the historical correspondence architecture.

A document state MUST carry exactly one mode. Conversion MUST create a native family/branch/revision and a capability report. Opening, copying, or rendering MUST NOT change mode. No API may write native semantics while reporting the same state as foreign-managed.

## 3. Scope of native truth

Native truth covers authored reflowable-document information and bounded layout intent. It is a native ontology, not a universal normalized ontology for every foreign format.

The core MUST NOT absorb complete spreadsheet computation, presentation animation, source repositories, applications, arbitrary database state, browser DOMs, full temporal media, every OOXML/ODF/PDF object, or executable foreign content. Such material stays with DATAeye, a presentation substrate, CODEeye, SHELLeye, eyeBROWSE, MEDIAeye, or an inert provider facet/capsule.

### 3.1 Included current state

A self-contained DND carries:

- format/version/capability metadata;
- `family_id`, `branch_id`, current branch head, revision tuple, and bounded parents;
- public semantic objects and relations;
- containment and deterministic logical order;
- text and explicitly retained boundaries/ranges;
- styles, themes, direct overrides, and layout profiles;
- comments/threads/replies and active suggestions;
- fields, controls, notes, citations, references, figures, math, and assets;
- extension envelopes, their coverage/edit contracts, and fallbacks;
- bounded retirement, lineage, idempotency, delta, and unresolved-merge witnesses required by current behavior;
- optional provider facets and source capsules with digests and alignment state;
- canonical record digests and the semantic root.

### 3.2 Excluded from canonical native truth

The following MUST be external and rebuildable or derived:

- FTS indexes, embeddings, query accelerators, and model summaries;
- runtime ropes, piece tables, editor selection, and undo UI state;
- layout, raster, glyph, thumbnail, and render caches;
- live provider processes, COM objects, browser DOMs, and open-document registries;
- physical carrier registries and path bindings;
- Program Host state;
- accepted-action history, a permanent transaction log, or a historical Merkle DAG.

Deleting all such state MUST NOT destroy native identity or current semantics.

## 4. Identity

### 4.1 Identifier profile

`family_id`, `branch_id`, `revision_id`, public `object_id`, retained `boundary_id`, extension instance IDs, and other opaque semantic IDs use **RFC 9562 UUID version 4**.

The choice is deliberate:

- v4 is opaque, has no time or ordering semantics, leaks no creation time, and is simple to generate independently;
- v7's index locality does not justify exposing time-order implications in semantic identity;
- logical order and database locality are separate concerns and MUST be handled separately;
- collision resistance is supplied by 122 random bits and validation; no format logic may rely on chronological sorting of IDs.

Canonical CBOR encodes a UUID as tag 37 containing exactly 16 bytes. Human display uses lowercase hyphenated hexadecimal. IDs are type-agnostic; object type is a validated record field, not encoded in the ID. A content digest MUST NOT be a semantic object ID.

### 4.2 Identity scope and handles

Object identity is branch-scoped. Within a resolved family, `(branch_id, object_id)` is sufficient for direct actuation. A portable external write handle MUST include:

`(family_id, branch_id, object_id, expected_revision_id)`.

The expected revision is a precondition, not part of the object's enduring identity. Bare `object_id`, paths, provider IDs, bytes, hashes, text, coordinates, offsets, ordinals, and pages MUST NOT authorize a write.

DOCSeye logical concept IDs are not duplicated for ordinary native objects. A separate opaque correspondence concept MAY exist only when it adds information: cross-provider mapping, cross-branch concept linkage, split/merge lineage, imported foreign concepts, or detached derived/provider registries. Actuation identity and correspondence identity remain distinct.

### 4.3 Family identity

An authored family is the set of branches intentionally declared to descend from the same authored origin. Intent is recorded by a controlled operation; path and byte equality are insufficient.

| Operation | Family result | Branch/object result |
| --- | --- | --- |
| ordinary save | same family | same branch and surviving IDs |
| rename or physical move | same family | same branch and IDs |
| controlled replica/copy | same family | same branch replica until divergence |
| generic Save As | same family | explicit fork: new branch and all public IDs reminted |
| explicit fork | same family | new branch and all public IDs/boundaries reminted |
| independent duplicate | new family | new branch and all public IDs/boundaries reminted |
| template instantiation | new family | new branch and all public IDs/boundaries reminted; template provenance bounded |
| foreign import | new family unless explicitly reconciling an existing import | new branch and native IDs minted |
| same-family merge | same family | explicit target branch commit with bounded parents and mappings |
| cross-family combination | target family | typed paste/import; all incoming occurrence IDs reminted |
| export | unchanged | no native identity change |

An API MUST distinguish `replicate`, `fork`, and `duplicate`; a filesystem path operation MUST NOT guess authored intent.

### 4.4 Branch identity and heads

Every DND carries one `branch_id` and one current committed head. Separate byte-identical copies initially represent replicas of the same committed branch snapshot. Opening another path MUST NOT mutate either artifact.

If two replicas commit independently from one base and later coexist, the observed state is **`divergent_heads`**. Neither head may silently supersede the other. Write authority against the combined observation stops until the caller explicitly:

- forks one head into a new branch, reminting every public semantic and boundary ID while recording bounded origin mappings; or
- performs a same-family merge with explicit object/anchor mappings and creates a bounded two-parent revision.

False branch continuity is forbidden. SHELLeye supplies distinct physical carrier identities and coherent snapshots, but those facts do not create semantic branches.

### 4.5 Fork rule

Every new branch MUST remint every branch-local public semantic object ID and retained boundary ID. The fork revision carries an `origin_ref` mapping to the source branch/revision/ID while that mapping is needed by active references, deltas, merge bases, or the bounded retention policy.

This rule applies to Save As, explicit fork, and branch creation from a divergent replica. It spends mapping space once to prevent bare-ID accidents, cross-branch stale-handle writes, and ambiguous continuation. Asset digests are not reminted because they identify immutable content, not semantic occurrences.

## 5. Revision and integrity model

### 5.1 Revision tuple

Every committed branch head contains:

- `revision_id`: opaque UUIDv4 commit identity;
- `revision_sequence`: unsigned branch-local convenience counter;
- `semantic_root`: SHA-256 digest of the canonical current semantic state;
- `parent_revision_ids`: zero for origin, one for an ordinary commit, at most two for a merge, plus a source parent witness for a fork;
- bounded commit/idempotency metadata needed for recovery.

`semantic_root` identifies current state. `revision_id` distinguishes commits, including distinct commits that produce the same state. `revision_sequence` is ordering convenience only: two divergent children may have the same sequence, and sequence never proves ancestry. Parent witnesses prove bounded ancestry/merge context and are not a permanent history.

### 5.2 Semantic root contract

The root MUST cover the canonical logical values of:

- family/branch identity and format capability requirements;
- semantic objects, types, containment, and normalized logical sibling order;
- exact UTF-8 text and retained boundaries/anchor state;
- relations, styles/themes, comments, suggestions, fields, controls, notes, and layout intent;
- required and optional extension envelopes, payload digests, coverage, and policies;
- referenced embedded asset digests and required linked-asset commitments;
- provider facet/capsule digests when the current state intentionally commits to them;
- bounded retirement/lineage facts that affect current resolution.

It MUST exclude SQLite pages, freelists, row IDs, indexes, record insertion order, rollback-journal history, `VACUUM` history, FTS, caches, renderer output, live provider state, and physical carrier data.

Build 001 freezes a **domain-separated Merkle-style current-state tree** so a local mutation can recompute bounded branches. It does not freeze a permanent historical DAG. Experiment E-07 chooses the exact tree fan-out/domain layout and must prove identical roots across independent implementations, record reorder, index rebuild, and `VACUUM`.

Semantically neutral storage rewrites MUST NOT change `semantic_root`, `revision_id`, `revision_sequence`, or `DocumentRevision`.

## 6. Semantic objects

### 6.1 Identity tiers

| Tier | Members | Identity rule |
| --- | --- | --- |
| Public cross-revision objects | document root; flow/story; section; text block/paragraph; list; list item; table; row; column; cell/logical region; inline link/citation/reference; named anchor/retained range; thread/comment/reply; suggestion; field; control; note and note occurrence; figure; math object; named style; theme/token set; layout profile; extension object; provider facet | UUIDv4 object ID, exact typed lifecycle |
| Value-like properties | heading/semantic role, block kind, list marker/start/restart rules, author/timestamp values, direct formatting, cell coordinate/span values, language, field policy, control constraints, figure placement, layout properties | change with owner; no separate ID unless promoted by an explicit future decision |
| Content-addressed values | embedded asset bytes, capsule bytes, canonical payload blobs | SHA-256 digest for integrity/deduplication, never occurrence identity |
| Revision-local derived objects | pages, lines, glyph runs, browser DOM nodes, shaped runs, search hits, ordinary integer-offset query ranges, computed layout regions | qualified by semantic/layout/render revision; never persistent semantic IDs |

`generic block` is a closed extensible category, not an extra wrapper object. `heading` is a semantic role on a text block, not a different identity species. Runs are derived style/shaping fragments and MUST NOT receive public persistent IDs. Pages are `LayoutRevision`-scoped and MUST NOT receive native semantic identity. Integer offsets are legal only in revision-local query and mutation input after expected-revision validation.

### 6.2 Containment and order

Containment is explicit and acyclic. Identity and order are independent. Moving an object changes its parent/order relation but not its semantic ID.

The Build 001 mechanism starts with unsigned 128-bit sparse sibling order keys and bounded local rebalance. Canonical semantic hashing uses the resulting ordered sequence of child IDs, not the physical key bytes, so maintenance that preserves order is semantically neutral. Keys must be unique within a sibling list; ties are invalid. Experiment E-05 may replace the storage mechanism with chunked arrays/B-tree sequence records if sparse keys fail scale or mutation-amplification requirements, but it may not weaken deterministic order, stable identity on move, local update, or root neutrality.

CRDT actor suffixes and fractional collaborative positions are not part of generation 1.

## 7. Text and retained boundaries

### 7.1 Canonical text

Text is valid UTF-8 representing Unicode scalar values. Unpaired surrogates and invalid sequences are forbidden. The artifact preserves the exact authored code-point sequence; writers MUST NOT silently normalize NFC, NFD, NFKC, or NFKD for storage or hashing. Search indexes may carry normalized projections but must return exact source spans.

A boundary may not split a UTF-8 sequence or Unicode scalar. Human-facing character operations default to extended grapheme clusters under the implementation-preflight Unicode version. A typed low-level operation may address scalar boundaries explicitly. Bidi operations use logical sequence order, never visual left/right.

The runtime MAY use a rope or piece table. Neither is canonical persistence. The persistent mechanism must expose the logical UTF-8 value plus stable retained boundaries; experiment E-01 adjudicates stable segments versus a balanced boundary-bearing text tree without changing the semantics below.

### 7.2 Query ranges and retention

An ordinary query range is revision-local:

`(branch_id, text_block_id, start_scalar_offset, end_scalar_offset, revision_id)`.

Reading does not rewrite a DND. `retain_range` is an explicit semantic transaction that creates independent zero-width `boundary_id` objects and a retained range/anchor relation. Co-located boundaries remain independently addressable even if an implementation physically interns their location. Overlapping ranges may share a position but do not share lifecycle implicitly.

Each boundary is owned by exactly one live text block at a time and records a logical position and insertion policy. It may move to a successor block through a typed split/merge mapping while retaining its own ID.

### 7.3 Edge insertion policies

For a non-collapsed interval, each edge independently declares `include_at_edge` or `exclude_at_edge`:

- start/include: insertion at the start becomes part of the range;
- start/exclude: the start advances after inserted text;
- end/include: the end advances after inserted text;
- end/exclude: the end remains before inserted text.

A collapsed point uses `before_insertion` or `after_insertion`. These are logical sequence semantics and are unaffected by bidi display.

Defaults are normative:

| Anchor type | Start | End / point |
| --- | --- | --- |
| comment target | exclude | exclude |
| named range | exclude | exclude |
| named point anchor | — | before insertion |
| style range | include | include |
| active text suggestion | exclude | exclude |
| field/query target | exclude | exclude |
| extension interval | extension-declared; otherwise must-understand | extension-declared; otherwise must-understand |

Callers MAY select a different supported policy explicitly when creating an anchor. The policy is part of semantic state and root coverage.

### 7.4 Deletion, replacement, move, split, and merge

Anchor resolution states are `live`, `collapsed`, `orphaned`, `destroyed`, and `ambiguous`. `ambiguous` never authorizes a write.

| Event | Required behavior |
| --- | --- |
| partial deletion | surviving endpoints map exactly and the range shrinks; no quote search |
| complete deletion | comment target becomes orphaned with quoted display evidence; style range is destroyed; named range becomes collapsed only if its contract permits; field/reference becomes unresolved; extension follows its policy |
| owner-block deletion | boundary/range is destroyed or type-specifically orphaned; it never jumps to a similar block |
| typed replacement | explicit replacement mapping determines whether boundaries enclose replacement; absent such mapping, complete-deletion behavior applies |
| owner move | live boundaries move with the owner without ID change |
| text-block split | original block retires; both block successors receive new IDs; each boundary keeps its ID and maps left/right by position and edge policy; a spanning range may become multi-interval |
| text-block merge | all input blocks retire; result gets a new ID; boundary IDs survive and receive exact positions in the result |

Comments, named ranges, and style ranges may be multi-interval after a split. Text insert/delete/replace suggestions may be multi-interval only when their typed operation remains unambiguous. Point anchors, field expression targets, cross-reference targets, and topology extensions may not fragment; the operation must transform them exactly, mark them unresolved/orphaned, or refuse.

Duplicate quoted text is display evidence only. A comment or cross-reference MUST NOT rebound by similarity. A valid independent external writer must emit explicit boundary/lifecycle transformations; an incomplete mapping makes the artifact invalid for writing.

## 8. Object lifecycle, copying, and bounded retirement

### 8.1 Copy and paste

Within-branch copy of a block, row, section, list item, control, field, figure, comment, or other occurrence remints every public ID in the copied subgraph. Text is copied by value. Stable anchor IDs are not copied unless a typed `copy_with_annotations` operation is requested, in which case new boundaries, ranges, threads/comments/replies, and related objects are minted.

Fields are copied as new field occurrences and their cached derived results become stale. Controls copy values only under their declared copy policy. Figures receive new occurrence IDs while immutable asset digests may be shared. Provider facets are copied only when their declared coverage and generic transform permit complete ID/boundary remapping; otherwise required facets cause refusal and optional facets cause an explicit omission/loss result.

Cross-branch and cross-family paste always remints the incoming public object and boundary subgraph. Cross-family paste discards family lineage as authority but MAY retain one bounded provenance reference. No copy operation creates a permanent genealogy requirement.

### 8.2 Split and merge identity

For text blocks, list items, table cells, and logical table regions:

- split retires the original and mints every successor;
- merge retires every input and mints one result.

No side is selected as the “same” object merely for convenience. This avoids false continuity and aligns copy/fork safety. A bounded lineage witness maps the retired object to exact successors/result.

Retired IDs resolve as:

- `destroyed` — no semantic successor;
- `split` — exact successor set known;
- `merged` — exact result known;
- `unknown_retired` — retirement is known but detailed mapping has been garbage-collected.

Text similarity MUST NOT turn any retired state back into a live object.

### 8.3 Witness retention and garbage collection

A retirement, lineage, or idempotency witness remains while referenced by any live anchor, active suggestion, required extension, current relation, non-expired delta cursor, supported expected-revision window, unresolved merge base, or current provider alignment. It is eligible for GC only after all dependencies are gone and the documented bounded revision/delta retention floor has elapsed.

After GC, detailed lineage may degrade to `unknown_retired`; it must never degrade to an inferred live target. The retention floor and pressure behavior are selected by Build 001 measurements, not an unbounded ledger.

## 9. Native object-family semantics

### 9.1 Flows, sections, blocks, and lists

A document root contains ordered flows/stories. A flow contains sections or blocks according to its role. Sections own layout-profile selection and section-scoped header/footer/page-numbering intent. Blocks include text blocks, lists, tables, figures, math, controls, and extension-defined fallbacks.

Lists and list items are first-class. A list item contains one or more blocks. Membership and nesting are semantic; displayed markers are derived. A list carries ordered style-level rules and explicit `start`/`restart` intent without adopting Word's numbering object hierarchy as native ontology.

### 9.2 Tables

A table has public row, column, and cell identities. Coordinates are locations, not identities. A cell represents one contiguous logical grid region with origin row/column and row/column span; covered slots refer to that cell. Nested tables are ordinary table blocks inside a cell.

Row insertion mints only the new row and cells. Reordering changes order, not surviving IDs. Duplicating a visually identical row remints the entire copied row/cell subgraph. Cell merge retires all input cells and mints one spanning cell; split retires the spanning cell and mints all results. Header row/column/region semantics are explicit properties/relations.

DOCSeye owns authored table structure and values. Formulas, large computational models, external datasets, and deep chart computation belong to DATAeye.

### 9.3 Styles and semantic roles

Semantic role, named style, theme/design token, direct override, and layout-profile/media variant are separate facts. Changing a font never changes heading identity or role.

Generation 1 uses a small deterministic resolution order:

1. specification defaults;
2. theme/design tokens;
3. semantic-role mapping;
4. acyclic named-style inheritance;
5. direct overrides;
6. layout-profile/media overrides.

There are no arbitrary selectors, specificity contests, or full CSS cascade. `effective_style` returns the computed value, source layer, source object/token, semantic revision, and layout profile.

### 9.4 Comments and suggestions

A thread owns ordered comments; a comment records author reference, authored timestamp, body, target, and optional quoted display evidence. Replies are comments with an explicit parent. Thread state is `open` or `resolved`; target state is independent and may become orphaned. Provider comment IDs are correspondence evidence only.

Native suggestion kinds are `insert`, `delete`, `replace`, `move`, `style_property`, and `structural_table`. Suggestions are current document content, not revision history. Accept/reject is a typed atomic transaction. Resolution applies or discards the proposal, then retires the suggestion; only bounded witnesses remain where required. No permanent accepted-change ledger is required.

### 9.5 Fields and controls

Field classes are limited to document metadata, date/time with explicit evaluation policy, page/page-count/section-page values, cross-reference, table of contents, list of figures, simple count, and bounded document query.

A field separates source/expression, typed dependencies, cached result, result semantic revision, optional layout/provider revision, and staleness. Evaluation policy is `manual`, `on_semantic_commit`, or `on_layout`; date/time additionally requires `fixed` or an explicit transaction-time input. Ambient code or arbitrary execution is forbidden.

Controls include text, rich text, number, boolean, date, enum, repeating group, and object/reference selector. Constraints and values are semantic. External bindings are inert declarations until a typed DATAeye or provider operation is explicitly invoked; parse/render never fetches them.

### 9.6 Notes, citations, links, and references

Footnotes and endnotes are note objects with their own body flow and one or more reference occurrences. Named anchors are human-readable aliases over stable object/boundary identity; bookmarks are not required for internal identity.

A cross-reference targets an exact object or named anchor and becomes `unresolved` if that target is destroyed. It never retargets by text similarity. Citations consist of occurrence objects plus a bounded bibliographic record (key, title, contributors, year/date, locator, identifier, and optional style hint). DOCSeye does not become a full bibliography database.

### 9.7 Assets, figures, math, drawings, and charts

An asset is reusable content; a figure is an authored occurrence with placement, crop/wrap, caption, alt text, and asset reference. Embedded assets are addressed/deduplicated by SHA-256. Replacing a figure's asset does not replace figure identity.

Asset states are:

- `embedded` — bytes in DND, digest verified, self-contained;
- `linked_local` — expected digest plus SHELLeye-resolved reference, non-self-contained;
- `remote` — URI plus expected digest, never fetched on parse/render without explicit action;
- `unresolved` — unavailable or digest mismatch.

Native math objects require Presentation MathML semantic content. An optional TeX source facet may aid authoring, and OMML remains a non-authoritative provider facet. Experiment E-20 freezes the exact MathML canonicalization and edit precedence; provider facets may constrain export but never override the current native MathML value.

Generation 1 has no generic native drawing program. SVG is inert reusable vector asset content subject to parser limits. A chart occurrence may carry caption/placement and a bounded snapshot/facet; its data and computation belong to DATAeye. Macros, OLE, ActiveX, and embedded executables remain inert provider content.

## 10. Extension architecture

### 10.1 Envelope

Every extension instance contains:

- opaque UUIDv4 `extension_id`;
- stable namespace URI, type name, and `major.minor` version;
- declared payload encoding and exact opaque payload;
- typed references to native object/boundary IDs;
- one or more machine-readable coverage descriptors;
- an edit/intersection policy for each coverage item;
- optional safe native/static fallback;
- SHA-256 payload digest;
- `required` or `optional` capability status.

Preservation means exact bytes for byte-string encodings and exact canonical value for deterministic-CBOR encodings. “Unknown bytes preserved” without coverage and edit behavior is non-conforming.

### 10.2 Coverage model

Coverage kinds are `object`, `property`, `subtree`, `text_interval`, `relation`, `topology_region`, `layout_profile`, and `document_global`. A descriptor uses only public object/boundary IDs and spec-known structural selectors, so an older implementation can detect an intersection without understanding payload semantics.

### 10.3 Edit/intersection policies

The normative policies are:

- `independent` — declared edit classes do not affect payload meaning;
- `move_with_target` — exact move/reparent preserves payload and coverage;
- `generic_transform` — the envelope declares spec-defined ID/boundary/topology transformations an unknown implementation can apply;
- `invalidate` — an intersecting edit may proceed only for an optional extension, preserving payload but marking it invalid and using/reporting fallback;
- `must_understand_before_edit` — any intersecting edit is refused unless a capable implementation handles it.

Default for an unknown intersection is `must_understand_before_edit`.

| Operation | Unknown-extension behavior |
| --- | --- |
| read | validate envelope/digest; expose capability state |
| preserve | retain exact payload/value and all envelope fields |
| render | use safe fallback or report unsupported; never execute payload |
| unrelated edit | proceed and preserve unchanged |
| move | follow `move_with_target`/`generic_transform`; otherwise invalidate optional or refuse |
| copy | remap only through declared generic transform; otherwise omit optional with declared loss or refuse required |
| target deletion | apply declared deletion transform; otherwise invalidate optional or refuse required |
| split/merge | require explicit generic transform; otherwise invalidate optional or refuse required |
| text insertion/deletion | use boundary-aware declared transform; otherwise invalidate optional or refuse required |
| topology change | use declared topology transform; otherwise invalidate optional or refuse required |
| export | preserve through a provider mapping/capsule, use fallback with declared loss, or block |

### 10.4 Version and capability behavior

An unsupported major version is incompatible. If required, the document is read-only and affected render/export is blocked; if optional, it may be preserved and rendered through fallback while disjoint edits remain allowed. An unknown minor version within a supported major may be preserved and processed only through the known envelope contract; unknown fields MUST NOT be dropped by a writer.

Capability reporting is technical, not user-permission policy. It states `supported`, `supported_with_transform`, `preserved_opaque`, `fallback_only`, `unavailable_provider`, `unsupported`, `blocked_required_extension`, or `invalid` at the relevant scope.

## 11. Serialization profile

Canonical logical records use CBOR under RFC 8949 core deterministic encoding with the following DOCSeye profile:

- definite lengths only;
- shortest well-formed integer and length encodings;
- maps sorted by deterministic encoded-key order;
- duplicate map keys prohibited before interpretation;
- schema map keys are exact ASCII text tokens; unknown keys are preserved according to version rules;
- canonical semantic numbers are signed/unsigned integers or explicit decimal/rational records; binary floating point, NaN, and infinities are forbidden in canonical semantics;
- UUIDs use tag 37 plus a 16-byte byte string;
- SHA-256 digests use untagged 32-byte byte strings in digest-typed fields;
- text is valid exact UTF-8 with no implicit normalization;
- one logical record may nest at most 64 levels, contain at most 1,000,000 array/map entries, and contain no text/byte scalar over 64 MiB;
- asset and source-capsule bytes are stored as separately streamed blobs and are not subject to the 64 MiB logical-record scalar limit;
- implementations must advertise and enforce total-object, total-asset, and total-artifact resource limits; exceeding a limit returns `resource_limit` and never writable partial interpretation.

The serializer is not the runtime object model. Independent canonical vectors MUST cover every scalar form, map ordering, UUID/digest, Unicode edge case, unknown extension, and invalid duplicate-key form.

## 12. SQLite application-file container

SQLite is the transaction/container substrate. The DOCSeye public semantic model is the canonical logical specification above. SQLite page bytes, SQL row order, internal row IDs, indexes, and initial schema are not semantic truth.

The native artifact uses rollback-journal `DELETE` mode for transactional editing and MUST be quiescent with no required sidecar before portable publication. A live copy of a main database file alone is not a coherent snapshot; publication uses SQLite's supported snapshot/backup mechanisms plus SHELLeye coordination. Hot journals are recovery state and must not be discarded.

External rebuildable runtime/index databases use WAL and may contain FTS, embeddings, query accelerators, layout/render caches, installed-provider status, open-document/process bindings, and physical carrier mappings. Deleting them is a required acceptance operation.

The implementation-preflight rule is to select the latest stable, supported SQLite patch available at Build 001 start, record its source and binary digest, and run E-02/E-03 before relying on it. The architecture does not pin an incidental research version.

The Build 001 `.dnd` suffix and provisional media type `application/vnd.docseye.native+sqlite` exist for dispatch and test isolation only. They are not semantic inputs and may change before public registration after collision/registration review.

## 13. Transactions, publication, and deltas

A semantic write requires one validated branch head and `expected_revision_id`. A transaction may contain multiple typed operations and produces one all-or-nothing revision, root, exact semantic delta, and layout invalidation set.

Crash invariant:

> After recovery, the artifact is either the entire old committed semantic revision or the entire new committed semantic revision.

No acknowledged partial revision is valid. On ambiguous post-commit response loss, the client MUST query a bounded idempotency witness; it MUST NOT replay blindly. Idempotency witnesses expire under the bounded retention policy and then return `outcome_unknown`, requiring read/replan.

SHELLeye may publish a committed artifact by coherent atomic carrier replacement. Physical publication completion is a separate clock; it does not alter the committed semantic revision.

Deltas are branch/revision scoped and cover every semantic object, relation, text/boundary, extension, asset commitment, and layout-intent change. Cursors are bounded. A gap or expired cursor returns `cursor_expired`/`resync_required` with the current head; it never omits changes silently. A local mutation MUST NOT require whole-document retransmission when a bounded delta/snapshot fragment suffices.

## 14. Validation and repair

Validation is layered and stops at the first unsafe authority boundary:

1. container;
2. CBOR well-formedness and schema/version;
3. identity uniqueness, scope, and references;
4. containment/order/text/table semantic invariants;
5. extension envelope, coverage, and payload digest;
6. asset/capsule integrity;
7. semantic-root verification;
8. provider-specific validation where requested.

A failure at levels 1–7 removes native write authority. Validation MUST NOT silently mint or substitute identity. Repair is an explicit operation with a report. If original identity cannot be proved, repair creates a new artifact/branch with bounded provenance rather than falsely preserving IDs.

## 15. Parser and active-content safety

Opening, validating, indexing, or rendering MUST NOT execute code or automatically access the network. Native generation 1 has no implicit executable content.

Implementations must bound malformed SQLite/CBOR, record counts, nesting, strings, recursive references, asset/extension sizes, and decompression ratios. Provider readers must reject path traversal, XML external entities, unsafe URI resolution, and ZIP/decompression bombs. Remote assets remain inert until an explicit typed fetch. Foreign macros, scripts, ActiveX, OLE, external relationships, and embedded applications remain inert provider data unless separately invoked through the owning substrate.

## 16. Layout intent and render contracts

Native layout intent includes page size/orientation/margins, optional bleed, columns, explicit breaks, keep-with-next/keep-together, widow/orphan intent, headers/footers, page numbering, figure placement/wrap, table split/repeat-header, note placement, language/hyphenation, and writing mode. Provider quirks remain facets; DOCSeye does not clone Word's layout model.

A `LayoutRevision` is qualified by:

`(semantic revision, layout profile, provider, provider version, font manifest, locale, hyphenation data, relevant configuration)`.

Pages and regions are addressed by `(layout_revision_id, page_index/region_id)`. Reflow never mutates semantic identity. A render records family, branch, semantic revision/root, layout revision, provider/environment, output digest, warnings, and semantic-object-to-region correspondence.

DOCSeye owns semantic-to-layout/render contracts and provider mappings. It does not own a full typography engine in Build 001. Generated Typst source and browser DOM are derived provider state.

## 17. Provider capsules, DOCX, and fidelity reporting

### 17.1 Source capsule

A provider source capsule contains exact source bytes, SHA-256 digest, media/provider profile, import timestamp/tool profile, scoped correspondence, and alignment state. It is embedded by default when conversion requests round-trip preservation; otherwise inclusion is an explicit privacy/size choice. Build 001's adversarial DOCX import embeds it.

Alignment states are:

- `source_exact_unmodified`;
- `aligned_with_native_edits`;
- `partially_stale`;
- `unusable`.

After conversion the capsule remains authoritative only for its historical source bytes and provider evidence. It never becomes current semantic authority. Native edits update alignment/capability state, not source bytes.

### 17.2 Import

DOCX import performs raw OPC/OOXML inspection, maps honest native concepts, records partial features as provider facets, retains unsupported material as scoped opaque provider objects, optionally embeds the source capsule, records correspondence, and emits a feature-level capability report. Native UUIDv4 IDs are minted; provider IDs remain scoped evidence. Unsupported content MUST NOT silently disappear.

### 17.3 Export outcome vocabulary

Exactly one semantic translation outcome applies to each export scope:

- `exact_source_reuse`;
- `preserved_patch`;
- `translated_conformant`;
- `translated_with_declared_loss`;
- `unsupported`;
- `blocked`.

Validation/observation evidence is separate and may contain:

- `not_observed`;
- `package_validated`;
- `schema_validated`;
- `alternate_provider_observed`;
- `microsoft_observed`.

`microsoft_observed` requires the exact Word/Windows/font/locale/compatibility environment to be recorded. A schema-valid or LibreOffice-opened file is never labeled Microsoft-observed. “High fidelity” is not a valid Boolean capability.

## 18. Version evolution and conformance

The format is openly specified. Major format versions may change incompatible semantics and require explicit migration/new-branch behavior. Minor versions may add optional fields/object/extension types only when older writers can preserve them through the published envelope and capability rules. Unknown required capability removes write authority; unknown optional capability never permits silent loss.

A second implementation counts as conformance evidence only if it independently:

1. decodes and re-encodes all canonical vectors byte-exactly;
2. validates the root and all identity/reference invariants;
3. preserves unknown optional payloads and refuses unsafe required intersections;
4. creates a valid independent commit including new revision/root/delta witnesses;
5. performs the specified move/copy/split/merge and boundary transformations;
6. round-trips through the primary implementation with identical semantic root and no undeclared capability loss.

Build 001 does not implement signatures or encryption. The root/domain separation and extension envelope reserve explicit future signing/encryption profiles. No proprietary cryptography or partial signature claim is permitted without a proven dependency closure.

## 19. Mechanisms still assigned to Build 001 experiments

The following semantics are frozen while their mechanisms remain experimental:

- persistent text structure and boundary storage (E-01);
- SQLite crash/coherent-copy profile (E-02/E-03);
- sparse order-key storage versus a bounded alternative (E-05);
- semantic-root tree fan-out/domain layout (E-07);
- MathML canonicalization and TeX/OMML facet precedence (E-20).

An experiment may select a mechanism satisfying the invariant. It may not leave incompatible semantics open or silently weaken exactness.
