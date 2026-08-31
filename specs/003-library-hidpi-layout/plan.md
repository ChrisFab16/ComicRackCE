# Implementation Plan: Library Comic List HiDPI Layout Refresh

**Branch**: `003-library-hidpi-layout` | **Date**: 2026-08-31 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-library-hidpi-layout/spec.md`

## Summary

Scale the **ComicBrowserControl** comic list pane (thumb/tile sizes, list row height, column headers, group headers, toolbar, context menus) for Windows display scaling at 125%–200%. Builds on features **001** and **002** (PerMonitorV2, `FormUtility.RefreshDpiScale`, `DpiScaleChanged`). Technical approach: central `ApplyComicBrowserMetrics()` on `ComicBrowserControl`, hook `FormUtility.DpiScaleChanged`, scale defaults when at/below 96-DPI baselines, normalize persisted thumb/tile/row sizes on `ViewConfig` load—reuse `ScaleDpi` only.

## Technical Context

**Language/Version**: C# / .NET Framework 4.8  
**Primary Dependencies**: WinForms, `cYo.Common.Windows` (`FormUtility`, `ItemView`, `CoverViewItem`), `ComicBrowserControl`  
**Storage**: `ItemViewConfig` / `DisplayListConfig` in workspace XML; apply legacy-size heuristic on load (FR-004)  
**Testing**: Manual quickstart at 100%/125%/150%/200% (Constitution III)  
**Target Platform**: Windows 10/11 desktop  
**Project Type**: Desktop WinForms (ComicRackCE)  
**Performance Goals**: Metrics refresh imperceptible on DPI change (<100 ms subjective, same as 001/002)  
**Constraints**: FR-004 (no corrupt user layouts); FR-005 (no behavior change); FR-008 (reuse ScaleDpi); Constitution I focused diff  
**Scale/Scope**: ~2–3 source files primary — `ComicBrowserControl.cs`, optional small touch `ItemView.cs` only if group-header baseline must be centralized

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Pre-Phase 0 | Post-Phase 1 |
|-----------|-------------|----------------|
| I. Upstream-first, focused | ✅ Comic list pane only | ✅ Contracts scoped; reader/prefs out |
| II. Preserve WinForms stack | ✅ ScaleDpi + ItemView | ✅ No framework change |
| III. Real Windows HiDPI validation | ✅ quickstart.md scale matrix | ✅ Scenarios tied to SC-001–SC-004 |
| IV. Spec before implement | ✅ spec complete | ✅ plan + contracts ready for tasks |
| V. Simplicity | ✅ Mirror 001/002 metrics helper pattern | ✅ Single helper on ComicBrowserControl |

**Gate status**: PASS — proceed to `/speckit-tasks`.

## Project Structure

### Documentation (this feature)

```text
specs/003-library-hidpi-layout/
├── plan.md              # This file
├── research.md          # Audit + Phase 0 decisions
├── data-model.md        # Layout entities
├── quickstart.md        # Manual validation
├── contracts/           # UI layout contracts
└── tasks.md             # (/speckit-tasks — next)
```

### Source Code (touch list)

```text
ComicRack/Views/
├── ComicBrowserControl.cs           # ApplyComicBrowserMetrics, DpiScaleChanged, ViewConfig normalization
└── ComicBrowserControl.Designer.cs  # (reference only; metrics in code)

cYo.Common.Windows/Forms/
└── ItemView.cs                      # (optional) only if group-header baseline cannot be set from browser
```

**Structure Decision**: `ComicBrowserControl` is shared by Library (`dbView`) and Folders (`fileView`) explorer layouts—one implementation covers both tabs.

## Phase 0: Research (complete)

See [research.md](./research.md) for pixel audit (001 research §7) and design decisions D1–D6.

All technical unknowns resolved — no NEEDS CLARIFICATION remaining.

## Phase 1: Design

### Design artifacts

| Artifact | Path |
|----------|------|
| Data model | [data-model.md](./data-model.md) |
| Comic list layout contract | [contracts/comic-list-layout.md](./contracts/comic-list-layout.md) |
| Browser chrome contract | [contracts/browser-chrome.md](./contracts/browser-chrome.md) |
| Workspace view config | Reuse pattern from [001 workspace contract](../001-folders-hidpi-layout/contracts/workspace-dpi-splits.md) (heuristic on load) |
| Validation guide | [quickstart.md](./quickstart.md) |

### Implementation phases (for tasks.md)

**Phase A — Comic list metrics (P0 / FR-001–FR-003)**  
1. Add `ApplyComicBrowserMetrics()` on `ComicBrowserControl`  
2. Scale default `ItemThumbSize` (128×128) when height ≤ 128  
3. Scale default `ItemTileSize` (192×96) when height ≤ 96  
4. Refresh `ItemRowHeight`, `ColumnHeaderHeight`, `GroupHeaderHeight` on DPI change  
5. Subscribe `FormUtility.DpiScaleChanged`  

**Phase B — Persisted view config (P0 / FR-004)**  
6. On `ViewConfig` apply, normalize thumb/tile/row sizes ≤ 96-DPI baselines (mirror 001 split heuristic)  
7. Do not re-scale user-custom sizes above baseline  

**Phase C — Toolbar (P1 / FR-006)**  
8. Scale toolbar button images from stored originals (sort/group/stack/view icons)  
9. Scale `toolStrip` height and button sizes (pattern from `ComicListFolderFilesBrowser`)  

**Phase D — Context menus (P2 / FR-007)**  
10. Scale `contextMenuItems` / key submenus: `ImageScalingSize`, font, where fixed 22px heights dominate  

**Phase E — Validation**  
11. Execute [quickstart.md](./quickstart.md); record `validation-results.md`

### Design Decisions (consolidated)

Documented in [research.md](./research.md#phase-0-design-decisions).

## Complexity Tracking

> No constitution violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| — | — | — |

## Next Steps

1. `/speckit-tasks` — break phases A–E into ordered tasks  
2. `/speckit-analyze` — mandatory before implement  
3. Implement on `003-library-hidpi-layout`; cherry-pick code to `master` for upstream when ready
