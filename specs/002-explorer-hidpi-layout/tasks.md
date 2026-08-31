# Tasks: Explorer Shell HiDPI Layout Refresh

**Input**: Design documents from `/specs/002-explorer-hidpi-layout/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md  
**Branch**: `002-explorer-hidpi-layout`

**Tests**: Manual validation per quickstart.md only (Constitution III). No automated test tasks in v1.

**Organization**: Tasks grouped by user story (P1 → P2 → P3) after shared explorer-shell foundation.

## Format: `[ID] [P?] [Story] Description`

---

## Phase 1: Setup

**Purpose**: Confirm artifacts and implementation entry points before code changes.

- [x] T001 Review acceptance criteria in `specs/002-explorer-hidpi-layout/contracts/explorer-shell-layout.md` and `specs/002-explorer-hidpi-layout/contracts/mainview-tab-chrome.md`
- [x] T002 Review pixel audit punch list in `specs/002-explorer-hidpi-layout/research.md` sections 1–2 and plan.md touch list for file paths
- [x] T003 Confirm local branch is `002-explorer-hidpi-layout`, feature **001** DPI foundation is present, and solution builds via `msbuild ComicRack\ComicRack.csproj /t:Rebuild /p:Configuration=Debug /p:LangVersion=13.0`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Explorer shell metrics helper and DPI hook MUST exist before user story work (plan Phase A, decisions D1/D5).

**⚠️ CRITICAL**: No user story implementation until this phase is complete.

- [x] T004 Add `ApplyExplorerShellMetrics()` public method skeleton in `ComicRack/Views/ComicExplorerView.cs` as single entry point for init + DPI refresh (mirror `ComicListFolderFilesBrowser.ApplyFoldersSidebarMetrics()` pattern)
- [x] T005 Subscribe `FormUtility.DpiScaleChanged` in `ComicRack/Views/ComicExplorerView.cs` with `BeginInvoke` guard matching `ComicRack/Views/ComicListFolderFilesBrowser.cs` `OnDpiScaleChanged`
- [x] T006 Call `ApplyExplorerShellMetrics()` from `ComicExplorerView` constructor or `OnLoad` in `ComicRack/Views/ComicExplorerView.cs` after `InitializeComponent()`
- [x] T007 Verify `FormUtility.DpiScaleChanged`, `RefreshDpiScale`, and `ComicExplorerViewSettings.NormalizeLegacySplits` from feature 001 are used as-is — no duplicate manifest or DPI infrastructure in this feature

**Checkpoint**: `ComicExplorerView` has callable metrics helper and DPI subscription — user story work can begin.

---

## Phase 3: User Story 1 — Explorer sidebar width (Priority: P1) 🎯 MVP

**Goal**: Sidebar default width scales at 125%–200%; grip remains usable; legacy workspaces stay proportionate (FR-001, FR-004, SC-001, SC-003).

**Independent Test**: quickstart.md Scenario 1 at 150% on Folders and Library tabs; Scenario 2 legacy workspace load.

### Implementation for User Story 1

- [x] T008 [US1] Scale `sidePanel.ExpandedWidth` to `FormUtility.ScaleDpiX(252)` when current value ≤ 252 in `ApplyExplorerShellMetrics()` in `ComicRack/Views/ComicExplorerView.cs` per C-ESL-002
- [x] T009 [US1] Apply scaled sidebar default on fresh init (before or independent of persisted `ViewSettings`) in `ComicRack/Views/ComicExplorerView.cs` so designer 96-DPI 252 is not left unscaled
- [x] T010 [US1] Ensure `ViewSettings` setter in `ComicRack/Views/ComicExplorerView.cs` does not re-scale loaded `BrowserSplit`/`PreviewSplit` after 001 `NormalizeLegacySplits` — split bump only when value ≤ design baseline per C-ESL-006
- [x] T011 [US1] Include sidebar width in `ApplyExplorerShellMetrics()` DPI refresh path in `ComicRack/Views/ComicExplorerView.cs`
- [x] T012 [US1] Verify sidebar grip draggable at 150% on Folders (`fileView`) and Library (`dbView`) tabs without regression (SC-003, FR-005)

**Checkpoint**: MVP complete — explorer sidebar proportionate at HiDPI on both Library and Folders; validate Scenarios 1–2 before US2.

---

## Phase 4: User Story 2 — Preview pane proportions (Priority: P2)

**Goal**: Preview height, caption margins, and bottom padding scale at HiDPI (FR-002, FR-003, SC-002).

**Independent Test**: quickstart.md Scenario 3 at 150% with preview pane expanded and comic selected.

### Implementation for User Story 2

- [x] T013 [US2] Scale `previewPane.ExpandedWidth` to `FormUtility.ScaleDpiY(207)` when current value ≤ 207 in `ApplyExplorerShellMetrics()` in `ComicRack/Views/ComicExplorerView.cs` per C-ESL-003
- [x] T014 [P] [US2] Scale `smallComicPreview.CaptionMargin` and `comicBrowser.CaptionMargin` to `new Padding(FormUtility.ScaleDpiX(2))` in `ApplyExplorerShellMetrics()` in `ComicRack/Views/ComicExplorerView.cs` per C-ESL-004
- [x] T015 [US2] Update `UpdatePreviewPadding()` in `ComicRack/Views/ComicExplorerView.cs` to use `FormUtility.ScaleDpiY(6)` for `previewPane` and `pluginContainer` bottom padding when plugin docked bottom per C-ESL-003
- [x] T016 [US2] Include preview pane height and caption margins in `ApplyExplorerShellMetrics()` DPI refresh path in `ComicRack/Views/ComicExplorerView.cs`
- [x] T017 [US2] Verify preview caption readable without clipping at 150% with standard single-comic preview (SC-002) and collapse/re-expand preview pane retains scaled defaults (edge case)

**Checkpoint**: Preview pane proportionate at HiDPI; Scenario 3 passes independently of tab strip work.

---

## Phase 5: User Story 3 — Main tab strip icons and spacing (Priority: P3)

**Goal**: Library/Folders/Pages tab icons and padding scale at 125%–200% (FR-006, user story 3).

**Independent Test**: quickstart.md Scenario 5 at 150% — sharp icons, comfortable padding.

### Implementation for User Story 3

- [x] T018 [P] [US3] Store original bitmap references for `tsbLibrary`, `tsbFolders`, and `tsbPages` images in `ComicRack/Views/MainView.cs` (Resources.Library, FileBrowser, ComicPage)
- [x] T019 [US3] Add `ApplyMainViewTabMetrics()` in `ComicRack/Views/MainView.cs` scaling tab images via `.ScaleDpi()` from stored originals per C-MTC-001
- [x] T020 [US3] Scale `tsbLibrary.Padding` to `(ScaleDpiX(8), 0, 0, 0)`, `tsbFolders`/`tsbPages` to `(0, 0, ScaleDpiX(8), 0)` in `ApplyMainViewTabMetrics()` in `ComicRack/Views/MainView.cs` per C-MTC-002
- [x] T021 [US3] Call `ApplyMainViewTabMetrics()` from `MainView` init/OnLoad and subscribe `FormUtility.DpiScaleChanged` with same BeginInvoke pattern in `ComicRack/Views/MainView.cs`
- [x] T022 [US3] Verify tab click, selection, and view switching unchanged at 150% (FR-005, C-MTC-003)

**Checkpoint**: Tab strip scaled at HiDPI; Scenario 5 passes.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Settings alignment, regression checks, operator sign-off (plan Phase D, SC-004).

- [x] T023 [P] Verify `ComicExplorerViewSettings` constructor defaults in `ComicRack/Views/ComicExplorerViewSettings.cs` remain consistent with scaled shell baselines (250/200 ctor vs 252/207 designer — document tolerance only; no schema change)
- [x] T024 Smoke-test Library tab explorer shell after `ComicExplorerView` changes (both `dbView` instances share one implementation)
- [x] T025 Run quickstart.md Scenario 4 at 150% — folder select, comic list update, preview toggle, split drag persistence (FR-005, SC-004)
- [x] T026 Run quickstart.md Scenarios 1–5 at 125%, 150%, and 200%; record results in `specs/002-explorer-hidpi-layout/validation-results.md`
- [x] T027 Run quickstart.md Scenario 6 (dark mode) at 150% for preview caption and tab strip per C-ESL-007
- [x] T028 Note DPI refresh latency in `validation-results.md` — whether `ApplyExplorerShellMetrics()` feels instant (<100 ms subjective) on scale change or monitor move

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — **BLOCKS all user stories**
- **User Story 1 (Phase 3)**: Depends on Foundational — **MVP**
- **User Story 2 (Phase 4)**: Depends on Foundational; logically follows US1 (same file/method) but independently testable via Scenario 3
- **User Story 3 (Phase 5)**: Depends on Foundational only — **can parallelize with US1/US2** (`MainView.cs` vs `ComicExplorerView.cs`)
- **Polish (Phase 6)**: Depends on US1–US3 complete

### User Story Dependencies

- **User Story 1 (P1)**: After Phase 2 — no dependency on US2/US3
- **User Story 2 (P2)**: After Phase 2 — extends `ApplyExplorerShellMetrics()`; testable via preview pane without tab strip
- **User Story 3 (P3)**: After Phase 2 — independent file (`MainView.cs`); can run parallel to US1/US2

### Within Each User Story

- Foundational metrics helper before split/margin tasks
- Sidebar (US1) before or alongside preview (US2) in same method — prefer US1 → US2 sequential to avoid merge conflicts
- Tab metrics (US3) parallelizable with US1/US2

### Parallel Opportunities

- **T014** [P] caption margins can proceed alongside T013 if coordinated in same method
- **T018** [P] original image storage in MainView while US1/US2 work on ComicExplorerView
- **T023** [P] settings review while validation prep
- **US3 entire phase** can run in parallel with US1+US2 by different implementer

---

## Parallel Example: User Story 3 vs User Stories 1–2

```text
Developer A (ComicExplorerView.cs):
  T008 → T009 → T010 → T011 → T012   (US1 sidebar)
  T013 → T014 → T015 → T016 → T017   (US2 preview)

Developer B (MainView.cs) — after Phase 2 only:
  T018 → T019 → T020 → T021 → T022   (US3 tab strip)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart Scenarios 1–2 at 150%
5. Demo sidebar fix on Folders + Library before preview/tab work

### Incremental Delivery

1. Setup + Foundational → metrics helper ready
2. US1 → sidebar at HiDPI → validate (MVP)
3. US2 → preview pane → validate Scenario 3
4. US3 → tab strip → validate Scenario 5
5. Polish → full matrix + validation-results.md

### Suggested MVP Scope

**User Story 1 only** (Phases 1–3): scaled explorer sidebar on Library and Folders at 125%–200% with workspace load compatibility.

---

## Notes

- Reuse `FormUtility.ScaleDpi*` only (FR-007); do not add parallel scaling helpers
- `ApplyExplorerShellMetrics()` covers both `MainView.dbView` and `MainView.fileView` — one implementation, two instances
- Caption margins and padding may always re-scale on DPI change; split widths only when ≤ baseline (C-ESL-006)
- Commit after each task or logical group; run `/speckit-analyze` before `/speckit-implement`
