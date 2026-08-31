# Implementation Plan: Explorer Shell HiDPI Layout Refresh

**Branch**: `002-explorer-hidpi-layout` | **Date**: 2026-08-31 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-explorer-hidpi-layout/spec.md`

## Summary

Scale the **ComicExplorerView** shell (sidebar width, preview pane height, caption/preview margins) and **MainView** explorer tab strip (icons, padding) for Windows display scaling at 125%–200%. Builds on feature **001** (PerMonitorV2, `FormUtility.RefreshDpiScale`, workspace split normalization on load). Technical approach: central `ApplyExplorerShellMetrics()` on `ComicExplorerView`, hook `FormUtility.DpiScaleChanged`, scale MainView tab images/padding on load and DPI refresh—reuse `ScaleDpi` only; no new dependencies.

## Technical Context

**Language/Version**: C# / .NET Framework 4.8  
**Primary Dependencies**: WinForms, `cYo.Common.Windows` (`FormUtility`, `SizableContainer`, `TabBar`), existing `ComicExplorerViewSettings`  
**Storage**: Workspace XML unchanged; feature 001 `NormalizeLegacySplits` handles load; this feature scales **runtime defaults** and **DPI refresh** only  
**Testing**: Manual quickstart at 100%/125%/150%/200% (Constitution III)  
**Target Platform**: Windows 10/11 desktop  
**Project Type**: Desktop WinForms (ComicRackCE)  
**Performance Goals**: Explorer metrics refresh imperceptible on DPI change (<100 ms subjective, same as 001)  
**Constraints**: FR-004 (no double-scale on load); FR-005 (no behavior change); FR-007 (reuse ScaleDpi); Constitution I focused diff  
**Scale/Scope**: ~4 source files — `ComicExplorerView.cs`/`.Designer.cs`, `MainView.cs`, optional touch `ComicExplorerViewSettings.cs` for default alignment only

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Pre-Phase 0 | Post-Phase 1 |
|-----------|-------------|----------------|
| I. Upstream-first, focused | ✅ Explorer shell + tab strip only | ✅ Contracts scoped; no comic list grid |
| II. Preserve WinForms stack | ✅ ScaleDpi + existing SizableContainer | ✅ No framework change |
| III. Real Windows HiDPI validation | ✅ quickstart.md scale matrix | ✅ Scenarios tied to SC-001–SC-004 |
| IV. Spec before implement | ✅ spec complete | ✅ plan + contracts ready for tasks |
| V. Simplicity | ✅ Mirror 001 `ApplyFoldersSidebarMetrics` pattern | ✅ Single metrics helper per view |

**Gate status**: PASS — proceed to `/speckit-tasks`.

## Project Structure

### Documentation (this feature)

```text
specs/002-explorer-hidpi-layout/
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
├── ComicExplorerView.cs           # ApplyExplorerShellMetrics, DpiScaleChanged
├── ComicExplorerView.Designer.cs  # (reference only; metrics in code)
├── ComicExplorerViewSettings.cs   # Verify ctor defaults align with shell metrics
└── MainView.cs                    # Tab strip image + padding scaling

cYo.Common.Windows/Forms/
└── FormUtility.cs                 # (reuse DpiScaleChanged from 001 — no change unless hook gap)
```

**Structure Decision**: Single desktop solution; extends 001 DPI infrastructure. Library and Folders both use `ComicExplorerView` (`dbView`, `fileView` in `MainView`).

## Phase 0: Research (complete)

See [research.md](./research.md) for pixel audit (001 research §5–6) and design decisions D1–D5.

All technical unknowns resolved — no NEEDS CLARIFICATION remaining.

## Phase 1: Design

### Design artifacts

| Artifact | Path |
|----------|------|
| Data model | [data-model.md](./data-model.md) |
| Explorer shell contract | [contracts/explorer-shell-layout.md](./contracts/explorer-shell-layout.md) |
| MainView tab contract | [contracts/mainview-tab-chrome.md](./contracts/mainview-tab-chrome.md) |
| Workspace compatibility | Reuse [001 contracts/workspace-dpi-splits.md](../001-folders-hidpi-layout/contracts/workspace-dpi-splits.md) |
| Validation guide | [quickstart.md](./quickstart.md) |

### Implementation phases (for tasks.md)

**Phase A — Explorer shell metrics (P0 / FR-001–FR-003)**  
1. Add `ApplyExplorerShellMetrics()` on `ComicExplorerView`  
2. Scale `sidePanel.ExpandedWidth` default when ≤ design baseline (252)  
3. Scale `previewPane.ExpandedWidth` default when ≤ design baseline (207)  
4. Scale `smallComicPreview.CaptionMargin`, `comicBrowser.CaptionMargin`, `previewPane`/`pluginContainer` padding (6)  
5. Subscribe `FormUtility.DpiScaleChanged`; call metrics helper on refresh  

**Phase B — MainView tab strip (P1 / FR-006)**  
6. Scale `tsbLibrary`/`tsbFolders`/`tsbPages` images via `.ScaleDpi()` from stored originals  
7. Scale tab `Padding` (8px horizontal) via `ScaleDpi`  

**Phase C — Workspace compatibility (P0 / FR-004)**  
8. Do **not** multiply splits in `ApplyExplorerShellMetrics` when applying from `ViewSettings` — normalization stays in 001 `NormalizeLegacySplits` only  
9. Only bump designer/unscaled defaults on fresh init  

**Phase D — Validation**  
10. Execute [quickstart.md](./quickstart.md); record `validation-results.md`

### Design Decisions (consolidated)

Documented in [research.md](./research.md#phase-0-design-decisions).

## Complexity Tracking

> No constitution violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| — | — | — |

## Next Steps

1. `/speckit-tasks` — break phases A–D into ordered tasks  
2. `/speckit-analyze` — mandatory before implement  
3. Implement on `002-explorer-hidpi-layout`; cherry-pick to `master` for upstream when ready (after or with 001 PR)
