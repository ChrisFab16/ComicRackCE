# Tasks: Library Comic List HiDPI Layout Refresh

**Input**: Design documents from `/specs/003-library-hidpi-layout/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md  
**Branch**: `003-library-hidpi-layout`

**Tests**: Manual validation per quickstart.md only (Constitution III). No automated test tasks in v1.

**Organization**: Tasks grouped by user story (P1 → P2 → P3) after shared comic-browser foundation.

## Format: `[ID] [P?] [Story] Description`

---

## Phase 1: Setup

**Purpose**: Confirm artifacts and implementation entry points before code changes.

- [x] T001 Review acceptance criteria in `specs/003-library-hidpi-layout/contracts/comic-list-layout.md` and `specs/003-library-hidpi-layout/contracts/browser-chrome.md`
- [x] T002 Review pixel audit punch list in `specs/003-library-hidpi-layout/research.md` sections 1–4 and plan.md touch list
- [x] T003 Confirm local branch is `003-library-hidpi-layout`, features **001** and **002** are present, and solution builds via `msbuild ComicRack\ComicRack.csproj /t:Rebuild /p:Configuration=Debug /p:LangVersion=13.0`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Comic browser metrics helper and DPI hook MUST exist before user story work (plan Phase A, decisions D1/D3).

**⚠️ CRITICAL**: No user story implementation until this phase is complete.

- [x] T004 Add `ApplyComicBrowserMetrics()` public method skeleton in `ComicRack/Views/ComicBrowserControl.cs` as single entry point for init + DPI refresh (mirror `ComicExplorerView.ApplyExplorerShellMetrics()` pattern)
- [x] T005 Subscribe `FormUtility.DpiScaleChanged` in `ComicRack/Views/ComicBrowserControl.cs` with `BeginInvoke` guard matching `ComicRack/Views/ComicListFolderFilesBrowser.cs` `OnDpiScaleChanged`
- [x] T006 Refactor existing ctor scaling (column widths, row height, search images) to call `ApplyComicBrowserMetrics()` from constructor and `OnLoad` in `ComicRack/Views/ComicBrowserControl.cs` — single call site per D3
- [x] T007 Verify `FormUtility.DpiScaleChanged` and `RefreshDpiScale` from features 001/002 are used as-is — no duplicate DPI infrastructure

**Checkpoint**: `ComicBrowserControl` has callable metrics helper and DPI subscription — user story work can begin.

---

## Phase 3: User Story 1 — Thumb/tile covers readable (Priority: P1) 🎯 MVP

**Goal**: Default thumb and tile dimensions scale at 125%–200%; covers and labels proportionate (FR-001, FR-003, SC-001).

**Independent Test**: quickstart.md Scenarios 1–2 at 150% on Library and Folders tabs.

### Implementation for User Story 1

- [x] T008 [US1] Scale `itemView.ItemThumbSize` to `FormUtility.ScaleDpi(128×128)` when current height ≤ 128 in `ApplyComicBrowserMetrics()` in `ComicRack/Views/ComicBrowserControl.cs` per C-CLL-002
- [x] T009 [US1] Scale `itemView.ItemTileSize` to `(ScaleDpiY(96)*2, ScaleDpiY(96))` when tile height ≤ 96 in `ApplyComicBrowserMetrics()` in `ComicRack/Views/ComicBrowserControl.cs` per C-CLL-003
- [x] T010 [US1] Apply scaled thumb/tile defaults on fresh init before or independent of persisted `ViewConfig` in `ComicRack/Views/ComicBrowserControl.cs`
- [x] T011 [US1] Include thumb/tile sizes in `ApplyComicBrowserMetrics()` DPI refresh path in `ComicRack/Views/ComicBrowserControl.cs`
- [x] T012 [US1] Add `NormalizeViewConfigSizes(ItemViewConfig)` (or equivalent) and invoke from `ViewConfig` setter when thumb height ≤ 128 or tile height ≤ 96 per C-CLL-005 / FR-004 in `ComicRack/Views/ComicBrowserControl.cs`
- [x] T013 [US1] Verify thumb view on Library (`dbView`) and Folders (`fileView`) at 150% without tiny 96-DPI grid (SC-001)

**Checkpoint**: MVP complete — thumb/tile legible at HiDPI on both tabs; validate Scenarios 1–2 before US2.

---

## Phase 4: User Story 2 — List/detail rows and headers (Priority: P2)

**Goal**: Row height, column headers, and group headers scale at HiDPI (FR-002, SC-002).

**Independent Test**: quickstart.md Scenario 3 at 150% in list or detail view.

### Implementation for User Story 2

- [x] T014 [US2] Set `itemView.ItemRowHeight` and `itemView.ColumnHeaderHeight` to `itemView.Font.Height + FormUtility.ScaleDpiY(6)` in `ApplyComicBrowserMetrics()` in `ComicRack/Views/ComicBrowserControl.cs` per C-CLL-004
- [x] T015 [US2] Set `itemView.GroupHeaderHeight` to `FormUtility.ScaleDpiY(40)` when current value ≤ 40 in `ApplyComicBrowserMetrics()` in `ComicRack/Views/ComicBrowserControl.cs`
- [x] T016 [US2] Extend `NormalizeViewConfigSizes` to scale detail row height when ≤ unscaled baseline (ctor default before first metrics apply) in `ComicRack/Views/ComicBrowserControl.cs`
- [x] T017 [US2] Evaluate `itemView.Font` — use `SystemFonts.MessageBoxFont` on metrics refresh if IconTitleFont too small at 150% per research D6; document choice in validation if unchanged
- [x] T018 [US2] Include row/header/group metrics in DPI refresh path in `ComicRack/Views/ComicBrowserControl.cs`
- [x] T019 [US2] Verify list/detail view row text and column headers at 150% without vertical clipping (SC-002)

**Checkpoint**: List/detail proportionate at HiDPI; Scenario 3 passes.

---

## Phase 5: User Story 3 — Toolbar and context menus (Priority: P3)

**Goal**: Toolbar and context menus scale at HiDPI (FR-006, FR-007, SC-003).

**Independent Test**: quickstart.md Scenarios 4–5 at 150%.

### Implementation for User Story 3

- [x] T020 [P] [US3] Store original bitmap references for toolbar buttons (sortUp, sortDown, group, stack, view, browse, undo/redo, sidebar) in `ComicRack/Views/ComicBrowserControl.cs`
- [x] T021 [US3] Add `ApplyToolbarMetrics()` inside `ApplyComicBrowserMetrics()` scaling toolbar images via `.ScaleDpi()` and button sizes to `ScaleDpi(23×22)`, strip height `ScaleDpiY(25)` per C-CBC-001/002 (pattern from `ComicListFolderFilesBrowser.cs`)
- [x] T022 [US3] Update runtime sort icon assignment (`tbbSort.Image = sortUp/sortDown`) to use scaled images from stored originals in `ComicRack/Views/ComicBrowserControl.cs`
- [x] T023 [P] [US3] Set scaled `ImageScalingSize` and font on `contextMenuItems` (and `contextRating` / `contextMarkAs` if icons clip) in `ApplyComicBrowserMetrics()` in `ComicRack/Views/ComicBrowserControl.cs` per C-CBC-003
- [x] T024 [US3] Re-scale `tsQuickSearch` search/clear button images on DPI refresh in `ApplyComicBrowserMetrics()` in `ComicRack/Views/ComicBrowserControl.cs`
- [x] T025 [US3] Verify toolbar buttons clickable and context menu readable at 150% (SC-003, FR-007)

**Checkpoint**: Browser chrome scaled; Scenarios 4–5 pass.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Regression checks, legacy workspace, operator sign-off (plan Phase E, SC-004).

- [x] T026 Smoke-test Folders tab comic list after `ComicBrowserControl` changes (shared instance with Library)
- [x] T027 Run quickstart.md Scenario 6 at 150% — select, sort, group, search, open comic, ctrl+wheel resize (FR-005, SC-004)
- [x] T028 Run quickstart.md Scenario 7 — legacy workspace/view config at 100% saved, load at 150% (FR-004)
- [x] T029 Run quickstart.md Scenarios 1–8 at 125%, 150%, and 200%; record results in `specs/003-library-hidpi-layout/validation-results.md`
- [x] T030 Run quickstart.md Scenario 8 (dark mode) at 150% per C-CLL-007
- [x] T031 Note DPI refresh latency in `validation-results.md` — whether `ApplyComicBrowserMetrics()` feels instant (<100 ms subjective) on scale change

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — **BLOCKS all user stories**
- **User Story 1 (Phase 3)**: Depends on Foundational — **MVP**; includes ViewConfig thumb/tile normalization
- **User Story 2 (Phase 4)**: Depends on Foundational; extends metrics helper for list/detail
- **User Story 3 (Phase 5)**: Depends on Foundational; can parallelize toolbar/menu work with US2 after Phase 2 if coordinated
- **Polish (Phase 6)**: Depends on US1–US3 complete

### User Story Dependencies

- **User Story 1 (P1)**: After Phase 2 — no dependency on US2/US3
- **User Story 2 (P2)**: After Phase 2 — extends same `ApplyComicBrowserMetrics()` method
- **User Story 3 (P3)**: After Phase 2 — same file; prefer US1 → US2 → US3 sequential to avoid merge conflicts

### Parallel Opportunities

- **T020**, **T023** [P] can proceed alongside other US3 tasks with coordination
- **US3 toolbar** could start after Phase 2 while US1/US2 complete if different developer

---

## Parallel Example: User Story 3 after Phase 2

```text
Developer A (metrics core):
  T008 → T009 → T010 → T011 → T012 → T013   (US1 thumb/tile)
  T014 → T015 → T016 → T017 → T018 → T019   (US2 list/detail)

Developer B (chrome — after T004–T007 only):
  T020 → T021 → T022 → T023 → T024 → T025   (US3 toolbar/menus)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart Scenarios 1–2 at 150% on Library + Folders
5. Demo thumb/tile fix before list/toolbar work

### Incremental Delivery

1. Setup + Foundational → metrics helper ready
2. US1 → thumb/tile at HiDPI → validate (MVP)
3. US2 → list/detail rows → validate Scenario 3
4. US3 → toolbar/menus → validate Scenarios 4–5
5. Polish → full matrix + validation-results.md

### Suggested MVP Scope

**User Story 1 only** (Phases 1–3): scaled thumb/tile comic list on Library and Folders at 125%–200% with ViewConfig load compatibility.

---

## Notes

- Reuse `FormUtility.ScaleDpi*` only (FR-008)
- `ComicBrowserControl` serves both `MainView.dbView` and `MainView.fileView` — one implementation
- Persisted sizes above baseline must not be re-scaled (C-CLL-005)
- Commit after each task or logical group; run `/speckit-analyze` before `/speckit-implement`
