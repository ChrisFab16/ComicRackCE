# Tasks: Folders Tab HiDPI Layout Refresh

**Input**: Design documents from `/specs/001-folders-hidpi-layout/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md  
**Branch**: `001-folders-hidpi-layout`

**Tests**: Manual validation per quickstart.md only (Constitution III). No automated test tasks in v1.

**Organization**: Tasks grouped by user story (P1 → P2 → P3) after shared DPI foundation.

**Analyze**: Remediations from `analyze-report.md` incorporated (2026-08-31).

## Format: `[ID] [P?] [Story] Description`

---

## Phase 1: Setup

**Purpose**: Confirm artifacts and implementation entry points before code changes.

- [x] T001 Review acceptance criteria in `specs/001-folders-hidpi-layout/contracts/folders-sidebar-layout.md` and `specs/001-folders-hidpi-layout/contracts/workspace-dpi-splits.md`
- [x] T002 Review pixel audit punch list in `specs/001-folders-hidpi-layout/research.md` sections 1–5 for file touch list
- [x] T003 Confirm local branch is `001-folders-hidpi-layout` and solution builds via `msbuild ComicRack.sln /p:Configuration=Debug`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: DPI infrastructure MUST complete before user story work (plan Phase A, decisions D1/D5).

**⚠️ CRITICAL**: No user story implementation until this phase is complete.

- [x] T004 Upgrade `ComicRack/app.manifest` to `PerMonitorV2` in `dpiAwareness` (remove or supersede system-only awareness)
- [x] T005 Remove or guard redundant `SetProcessDPIAware()` in `ComicRack/Program.cs` if manifest provides equivalent awareness
- [x] T006 Add `FormUtility.RefreshDpiScale()` in `cYo.Common.Windows/Forms/FormUtility.cs` to invalidate cached `DpiScale` and return new scale
- [x] T007 Wire `DpiChanged` handling in `ComicRack/Program.cs` or `ComicRack/MainForm.cs` to call `FormUtility.RefreshDpiScale()` and re-apply Folders sidebar metrics
- [x] T008 Add `ApplyFoldersSidebarMetrics()` method in `ComicRack/Views/ComicListFolderFilesBrowser.cs` as single entry point for init + DPI refresh (tree, favorites, toolbar metrics)

**Checkpoint**: DPI scale refreshes on change; Folders view has callable metrics helper — user story work can begin.

---

## Phase 3: User Story 1 — Readable folder navigation (Priority: P1) 🎯 MVP

**Goal**: Folder tree text and row spacing scale at 125%–200%; labels not clipped (FR-001, FR-002, SC-002).

**Independent Test**: quickstart.md Scenario 1 at 150% and 200% — expand 3+ levels, no vertical label clipping.

### Implementation for User Story 1

- [x] T009 [US1] Attach `NiceTreeSkin` to `tvFolders` in `ComicRack/Views/ComicListFolderFilesBrowser.cs` (follow pattern in `ComicRack/Views/ComicListLibraryBrowser.cs`)
- [x] T010 [US1] Replace `SystemFonts.IconTitleFont` with `SystemFonts.MessageBoxFont` for `tvFolders` in `ComicRack/Views/ComicListFolderFilesBrowser.cs`
- [x] T011 [US1] Set `tvFolders.Indent = FormUtility.ScaleDpiX(15)` in `ApplyFoldersSidebarMetrics()` per C-FSL-002
- [x] T012 [US1] Set `tvFolders.ItemHeight = Font.Height + FormUtility.ScaleDpiY(8)` in `ApplyFoldersSidebarMetrics()` in `ComicRack/Views/ComicListFolderFilesBrowser.cs` (NiceTreeSkin owner-draw; `cYo.Common.Windows/Forms/FolderTreeView.cs` ImageList already uses ScaleDpi — no FolderTreeView ctor change unless regression found)
- [x] T013 [US1] Call `ApplyFoldersSidebarMetrics()` from constructor/OnLoad and after DPI refresh in `ComicRack/Views/ComicListFolderFilesBrowser.cs`
- [x] T014 [US1] Verify folder tree selection and expand/collapse behavior unchanged (FR-007) while testing Scenario 1
- [x] T015 [US1] Verify dark-mode tree labels and selection readability at 150% after T009 (quickstart Scenario 6 tree portion) per C-FSL-007

**Checkpoint**: MVP complete — Folders tree legible at HiDPI including dark mode; stop and validate Scenarios 1 + 6 (tree) before US2.

---

## Phase 4: User Story 2 — Favorites strip clarity (Priority: P2)

**Goal**: Favorites tiles allocate height from scaled typography; name + path readable (FR-003, FR-004).

**Independent Test**: quickstart.md Scenario 2 at 150% with long-path favorite and narrow sidebar.

### Implementation for User Story 2

- [x] T016 [P] [US2] Scale `favContainer` top padding and default `ExpandedWidth` using `FormUtility.ScaleDpiY` in `ComicRack/Views/ComicListFolderFilesBrowser.cs` per C-FSL-003
- [x] T017 [US2] Compute `favView.ItemTileSize` height as `max(ScaleDpiY(50), 2 * lineHeight + ScaleDpiY(12))` on load in `ComicRack/Views/ComicListFolderFilesBrowser.cs` (not resize-only)
- [x] T018 [US2] Update `favView_Resize` width margin to `FormUtility.ScaleDpiX(8)` in `ComicRack/Views/ComicListFolderFilesBrowser.cs`
- [x] T019 [P] [US2] Scale `FolderViewItem` border `Size(2,2)` and TextLine spacing (2, 5) via `FormUtility.ScaleDpi` in `ComicRack/Controls/FolderViewItem.cs`
- [x] T020 [US2] Derive mosaic thumbnail base size from scaled tile height instead of fixed `341×512` in `ComicRack/Controls/FolderViewItem.cs` `GetFolderImage`
- [x] T021 [US2] Include favorites metrics in `ApplyFoldersSidebarMetrics()` and DPI refresh path in `ComicRack/Views/ComicListFolderFilesBrowser.cs`

**Checkpoint**: Favorites readable at 150%+; Scenario 2 passes independently of toolbar work.

---

## Phase 5: User Story 3 — Toolbar and split proportions (Priority: P3)

**Goal**: Toolbar hit targets and splits feel proportionate; ≥5 tree rows with favorites expanded (FR-005, FR-006, SC-003).

**Independent Test**: quickstart.md Scenario 3 at 150% on **fresh workspace** (legacy workspace splits validated in Phase 6 Scenario 5).

### Implementation for User Story 3

- [x] T022 [P] [US3] Scale toolbar button images with `.ScaleDpi()` in `ComicRack/Views/ComicListFolderFilesBrowser.cs` OnLoad (pattern from `ComicRack/Dialogs/PreferencesDialog.cs`)
- [x] T023 [US3] Scale `toolStrip` height and each `ToolStripButton` size to baseline `ScaleDpi(23×22)` and strip height `ScaleDpiY(25)` in `ApplyFoldersSidebarMetrics()` in `ComicRack/Views/ComicListFolderFilesBrowser.cs` per C-FSL-004 (effective target ≥ `ScaleDpiY(22)`)
- [x] T024 [US3] Scale `SizableContainer` default `gripWidth` using `FormUtility.ScaleDpiX(6)` in `cYo.Common.Windows/Forms/SizableContainer.cs` per C-FSL-004
- [x] T025 [US3] Adjust default `TopBrowserSplit` / fav container height in `ApplyFoldersSidebarMetrics()` or settings ctor so SC-003 ≥5 tree rows at 150% with ≤3 favorites
- [x] T026 [US3] Include toolbar image and button metrics in `ApplyFoldersSidebarMetrics()` DPI refresh path in `ComicRack/Views/ComicListFolderFilesBrowser.cs`

**Checkpoint**: Scenario 3 passes on fresh workspace; toolbar icons sharp and hit targets scaled at 150%.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Workspace compatibility, regression checks, sign-off (plan Phases E–F, FR-008).

- [x] T027 Implement C-WSP-001 legacy normalization on load in `ComicRack/Views/ComicExplorerView.cs` `ViewSettings` setter for `TopBrowserSplit`, `BrowserSplit`, `PreviewSplit`, and `InfoBrowserSize` (apply `ScaleDpiY` or `.ScaleDpi()` per contract thresholds)
- [x] T028 Add per-load double-scaling guard and C-WSP-004 fallback to `ComicExplorerViewSettings()` constructor defaults when values are ≤0 or normalization throws in `ComicRack/Views/ComicExplorerView.cs` or `ComicRack/Views/ComicExplorerViewSettings.cs`
- [x] T029 Smoke-test Library tab sidebar after `SizableContainer` grip change in `cYo.Common.Windows/Forms/SizableContainer.cs` (Constitution Check post-design)
- [x] T030 Run quickstart.md Scenarios 1–8 at 125%, 150%, and 200%; record results in `specs/001-folders-hidpi-layout/validation-results.md` (include SC-001 subjective legibility note per tester)
- [x] T031 Verify FR-007 behaviors via quickstart.md Scenario 4 at 150% (selection, subfolders toggle, refresh, favorites)
- [x] T032 Run quickstart.md Scenario 5 (workspace migration) **after T027–T028**; confirm C-WSP-003 (favorites not collapsed to sliver, sidebar draggable)
- [x] T033 Spot-check DPI refresh latency per plan performance goal: after T007, note in `validation-results.md` whether `ApplyFoldersSidebarMetrics()` feels instant (<100 ms subjective) on DPI change or monitor move

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — **BLOCKS all user stories**
- **User Story 1 (Phase 3)**: Depends on Foundational — **MVP**
- **User Story 2 (Phase 4)**: Depends on Foundational; benefits from US1 `ApplyFoldersSidebarMetrics()` but independently testable via Scenario 2
- **User Story 3 (Phase 5)**: Depends on Foundational; Scenario 3 uses **fresh** workspace only
- **Polish (Phase 6)**: Depends on US1–US3; **T032 Scenario 5 requires T027–T028**

### User Story Dependencies

| Story | Depends on | Independent test |
|-------|------------|------------------|
| US1 (P1) | Phase 2 | quickstart Scenario 1 + Scenario 6 (tree) |
| US2 (P2) | Phase 2 | quickstart Scenario 2 |
| US3 (P3) | Phase 2 | quickstart Scenario 3 (fresh workspace) |

US2/US3 do not require US1 code complete for testing favorites-only or toolbar-only checks, but **Phase 2 is mandatory** for all.

### Within Each User Story

- Call `ApplyFoldersSidebarMetrics()` after story-specific metric changes
- Re-test FR-007 after each story checkpoint

### Parallel Opportunities

| Tasks | Notes |
|-------|-------|
| T016 [P] + T019 [P] | Different files (view vs FolderViewItem) after T008 exists |
| T022 [P] + T024 [P] | Toolbar images vs SizableContainer — after Phase 2 |
| T027 → T028 | T028 follows T027 normalization logic |

---

## Parallel Example: User Story 2

```bash
# After Phase 2 complete, parallelize:
T016 — favContainer padding/height in ComicListFolderFilesBrowser.cs
T019 — FolderViewItem border/spacing in FolderViewItem.cs

# Then sequential:
T017 — tile height formula (needs T016 layout context)
T020 — mosaic size (depends on T017 tile bounds)
```

---

## Implementation Strategy

### MVP First (User Story 1 only)

1. Phase 1: Setup (T001–T003)
2. Phase 2: Foundational (T004–T008) — **required**
3. Phase 3: User Story 1 (T009–T015)
4. **STOP** — run quickstart Scenarios 1 and 6 (tree) at 150% and 200%
5. Demo MVP before US2/US3

### Incremental Delivery

1. Setup + Foundational → DPI foundation ready
2. US1 → Scenarios 1 + 6 (tree) → MVP
3. US2 → Scenario 2 → favorites polish
4. US3 → Scenario 3 (fresh workspace) → toolbar/splits
5. Polish → T027–T028 then Scenario 5; full T030–T033 sign-off

### Task counts

| Phase | Tasks | Story |
|-------|-------|-------|
| Setup | 3 | — |
| Foundational | 5 | — |
| US1 | 7 | P1 MVP |
| US2 | 6 | P2 |
| US3 | 5 | P3 |
| Polish | 7 | — |
| **Total** | **33** | |

---

## Notes

- **Feature complete** — all 33 tasks done; validation signed off in `validation-results.md` (2026-08-31)
- Keep diffs focused for upstream PR (Constitution I)
- Do not change folder browsing logic while scaling layout (FR-007)
- SC-004 (timed favorite lookup baseline) deferred from v1 acceptance — see quickstart.md
- `[P]` = safe parallel when another developer/file owner available
