# Implementation Plan: Folders Tab HiDPI Layout Refresh

**Branch**: `001-folders-hidpi-layout` | **Date**: 2026-08-31 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-folders-hidpi-layout/spec.md`

## Summary

Improve legibility and spacing of the **Folders** tab sidebar (toolbar, favorites strip, folder tree) on Windows displays at 125%–200% scaling. Technical approach: enable **PerMonitorV2** DPI awareness, refresh `FormUtility.DpiScale` on DPI changes, scale all audited fixed-pixel layout values via existing `ScaleDpi` helpers, attach **NiceTreeSkin** to the folder tree for row-height parity with Library, and normalize persisted workspace splits on load. No framework migration; surgical WinForms changes only (Constitution II).

## Technical Context

**Language/Version**: C# / .NET Framework 4.8  
**Primary Dependencies**: WinForms (`System.Windows.Forms`), `cYo.Common.Windows` (`FormUtility`, `ItemView`, `FolderTreeView`, `SizableContainer`, `NiceTreeSkin`), shell COM (`ShellFolder`)  
**Storage**: User workspace XML (`DisplayWorkspace` / `ComicExplorerViewSettings` in config); no schema migration required if splits re-scaled at load time  
**Testing**: Manual quickstart at 100%/125%/150%/200% (Constitution III); optional future unit tests for `FormUtility` DPI math — not blocking v1  
**Target Platform**: Windows 10/11 desktop  
**Project Type**: Desktop WinForms application (ComicRackCE)  
**Performance Goals**: No perceptible lag on DPI change or folder tree expand; layout refresh < 100 ms on typical machines  
**Constraints**: FR-007 (no behavior change); FR-008 (workspace compatibility); upstream-ready focused diff (Constitution I); reuse `ScaleDpi` not parallel scaling API (Constitution V)  
**Scale/Scope**: ~10 source files, Folders sidebar + shared DPI utilities + workspace apply path; comic list pane out of scope except sidebar width side-effects

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Pre-Phase 0 | Post-Phase 1 |
|-----------|-------------|----------------|
| I. Upstream-first, focused | ✅ Sidebar-only scope; no unrelated refactors | ✅ Contracts limit blast radius; shared `SizableContainer` change tested on Library |
| II. Preserve WinForms stack | ✅ No WPF/WinUI | ✅ Extends existing controls/skins |
| III. Real Windows HiDPI validation | ✅ quickstart.md defines scale matrix | ✅ quickstart tied to SC-001–SC-003 |
| IV. Spec before implement | ✅ spec + research complete | ✅ plan + contracts ready for tasks |
| V. Simplicity | ✅ Reuse ScaleDpi, NiceTreeSkin, PreferencesDialog image pattern | ✅ No new NuGet deps |

**Gate status**: PASS — proceed to tasks (`/speckit-tasks`).

## Project Structure

### Documentation (this feature)

```text
specs/001-folders-hidpi-layout/
├── plan.md              # This file
├── research.md          # Audit + Phase 0 decisions
├── data-model.md        # Layout/settings entities
├── quickstart.md        # Manual validation
├── contracts/           # UI layout + workspace contracts
└── tasks.md             # (/speckit-tasks — next)
```

### Source Code (touch list)

```text
ComicRack/
├── app.manifest                    # PerMonitorV2
├── Program.cs                      # DpiChanged → refresh scale
├── Views/
│   ├── ComicListFolderFilesBrowser.cs
│   ├── ComicListFolderFilesBrowser.Designer.cs
│   ├── ComicExplorerView.cs
│   └── ComicExplorerViewSettings.cs
└── Controls/
    └── FolderViewItem.cs

cYo.Common.Windows/Forms/
├── FormUtility.cs                  # DpiScale invalidate + helper
├── FolderTreeView.cs               # Init scaling hooks
├── SizableContainer.cs             # gripWidth scale (shared)
└── NiceTreeSkin.cs                 # (reuse, no change unless needed)

.specify/feature.json
```

**Structure Decision**: Single desktop solution; changes concentrated in `ComicRack` view layer and `cYo.Common.Windows` shared forms helpers already used by Library.

## Phase 0: Research (complete)

See [research.md](./research.md) for full pixel audit and [Design Decisions](#design-decisions-consolidated) below.

All technical unknowns resolved — no NEEDS CLARIFICATION remaining.

## Phase 1: Design

### Design artifacts

| Artifact | Path |
|----------|------|
| Data model | [data-model.md](./data-model.md) |
| UI layout contract | [contracts/folders-sidebar-layout.md](./contracts/folders-sidebar-layout.md) |
| Workspace contract | [contracts/workspace-dpi-splits.md](./contracts/workspace-dpi-splits.md) |
| Validation guide | [quickstart.md](./quickstart.md) |

### Implementation phases (for tasks.md)

**Phase A — DPI foundation (P0)**  
1. `app.manifest`: `PerMonitorV2`; remove redundant `SetProcessDPIAware` if manifest suffices  
2. `FormUtility`: add `RefreshDpiScale()`, call from `Program` on `DpiChanged` (MainForm or message filter)  
3. Central helper `ApplyFoldersSidebarMetrics(ComicListFolderFilesBrowser)` for init + DPI refresh  

**Phase B — Folder tree (P0 / FR-001, FR-002)**  
4. Scale `Indent`, set `ItemHeight = Font.Height + ScaleDpiY(8)`  
5. Replace `IconTitleFont` with `MessageBoxFont` (or scaled default UI font)  
6. Attach `NiceTreeSkin` to `tvFolders` (match Library); verify dark mode  

**Phase C — Favorites strip (P0/P1 / FR-003, FR-004)**  
7. Compute `ItemTileSize` height from font metrics + scaled padding on load and resize  
8. Scale `FolderViewItem` border/spacing/mosaic relative to tile bounds  
9. Scale `favContainer` default height/padding at init  

**Phase D — Toolbar & chrome (P1 / FR-005, FR-006)**  
10. Scale toolbar button images (PreferencesDialog pattern)  
11. Scale grip width / fav margin 8 / explorer default splits where in scope  

**Phase E — Workspace persistence (P0 / FR-008)**  
12. On `ComicExplorerViewSettings` apply: if stored splits appear 96-DPI-native, multiply by current `DpiScale` once (see workspace contract)  
13. Document behavior in quickstart for users with old workspaces  

**Phase F — Validation**  
14. Execute [quickstart.md](./quickstart.md); record results in `validation-results.md` (create at implement time)

### Design Decisions (consolidated)

Documented in detail at end of [research.md](./research.md#phase-0-design-decisions).

## Complexity Tracking

> No constitution violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| — | — | — |

## Next Steps

1. `/speckit-tasks` — break phases A–F into ordered tasks with acceptance links to contracts  
2. `/speckit-analyze` — mandatory before implement  
3. Implement on `001-folders-hidpi-layout`; PR to fork `development`, then cherry-pick to `master` for upstream when ready
