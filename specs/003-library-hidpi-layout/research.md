# Research: Comic List Pane Fixed-Pixel & DPI Audit

**Feature**: `003-library-hidpi-layout`  
**Date**: 2026-08-31  
**Scope**: `ComicBrowserControl`, hosted `ItemView`, toolbar, context menus. Baseline from [001 research §7](../001-folders-hidpi-layout/research.md).

**Legend**: Same as 001 (✅ ⚠️ ❌ 🔄 N/A)

**Prerequisite**: Features **001** and **002** on branch — PerMonitorV2, `FormUtility.RefreshDpiScale()`, `DpiScaleChanged`.

---

## 1. `ComicBrowserControl` constructor (partial scaling today)

File: `ComicRack/Views/ComicBrowserControl.cs` (post-`InitializeComponent` block ~849–867)

| Control / property | Design / runtime value | ScaleDpi? (pre-003) | Notes |
|--------------------|------------------------|---------------------|-------|
| Column `Width` (all) | various 16–100 | ✅ | `ScaleDpiX` in ctor loop |
| `tsQuickSearch` button images | Search, SmallCloseGray | ✅ | `.ScaleDpi()` in ctor |
| `itemView.ItemRowHeight` | `Font.Height + ScaleDpiY(6)` | ✅ | Detail/list rows |
| `itemView.ColumnHeaderHeight` | = ItemRowHeight | ✅ | Set in ctor |
| `itemView.Font` | `SystemFonts.IconTitleFont` | 🔄 | Not MessageBoxFont; no DPI refresh |
| **ItemThumbSize** default | **128×128** | ❌ | `ItemView` default until ViewConfig |
| **ItemTileSize** default | **192×96** | ❌ | `ItemView` default until ViewConfig |
| **GroupHeaderHeight** | **40** | ❌ | `ItemView` default |
| `GetItemSize` / `SetItemSize` min/max | Program.Min/Max* | ✅ | Clamps use ScaleDpiY |
| **DpiScaleChanged hook** | — | ❌ | No refresh on scale change |

**Gap**: Constructor scales columns and detail row height once; thumb/tile defaults and group headers stay 96-DPI until user resizes or loads workspace—and load path may restore unscaled sizes.

---

## 2. `ItemView` defaults

File: `cYo.Common.Windows/Forms/ItemView.cs`

| Property | Default (96-DPI) | ScaleDpi on init? |
|----------|------------------|------------------|
| `ItemThumbSize` | 128×128 | ❌ |
| `ItemTileSize` | 192×96 | ❌ |
| `ItemRowHeight` | 16 | ❌ (browser overrides in ctor) |
| `GroupHeaderHeight` | 40 | ❌ |
| `ItemPadding` | 1×1 | ❌ |

`ViewConfig` setter applies persisted `ThumbnailSize` / `TileSize` if ≥ 16 — **no DPI normalization** on load.

---

## 3. Toolbar (`toolStrip`)

File: `ComicRack/Views/ComicBrowserControl.cs` + `.Designer.cs`

| Item | Value | ScaleDpi? (pre-003) |
|------|-------|---------------------|
| Sort/group/stack/view button images | Resources.* | ❌ — `tbbSort.Image = sortUp/sortDown` raw at runtime |
| `sortUp` / `sortDown` fields | stored originals | ❌ not re-scaled on DPI change |
| `toolStrip` height | designer ~25 | ❌ |
| ToolStrip button sizes | designer | ❌ |
| Layout overflow logic | `toolStrip.Width - 20` | ❌ fixed margin |

Pattern exists in `ComicListFolderFilesBrowser.ApplyToolbarMetrics()` (001).

---

## 4. Context menus

File: `ComicRack/Views/ComicBrowserControl.Designer.cs`

| Menu | Typical item `Size.Height` | ScaleDpi? |
|------|---------------------------|-----------|
| `contextMenuItems` | 22 | ❌ |
| `contextRating` | 22 | ❌ |
| `contextMarkAs` | 22 | ❌ |
| Menu icons | embedded Resources | ❌ at menu level |

WinForms `ToolStripMenuItem` respects `ImageScalingSize` on parent `ContextMenuStrip` when set at runtime.

---

## 5. Shared instances

Both `MainView.dbView.ComicBrowser` and `MainView.fileView.ComicBrowser` are `ComicBrowserControl` instances. **One** `ApplyComicBrowserMetrics()` covers Library and Folders comic lists.

---

## 6. Out of scope (confirmed)

- Reader, Preferences dialogs, list layout editor
- `CoverViewItem` internal draw metrics (unless thumb label clip found in validation)
- `ComicPagesView` / `QuickOpenView` (separate controls; may benefit later from shared ItemView patterns but not in v1 tasks)
- DirectWrite / full text engine swap

---

## Punch list (implementation order)

### P0 — Comic list metrics

1. `ApplyComicBrowserMetrics()` on `ComicBrowserControl`
2. Scale thumb/tile defaults when ≤ baseline (128 / 96 height)
3. Refresh row height, column header, group header on DPI change
4. Hook `FormUtility.DpiScaleChanged`

### P1 — ViewConfig load

5. Normalize persisted thumb/tile/row sizes ≤ baseline on `ViewConfig` apply (FR-004)

### P2 — Toolbar

6. Scale toolbar images from originals; strip height and button sizes

### P3 — Context menus

7. Scale primary context menu strip metrics (`ImageScalingSize`, font)

### P4 — Validation

8. quickstart Scenarios 1–6; Library + Folders both exercised

---

## Phase 0 Design Decisions

### D1: Metrics helper pattern

- **Decision**: Mirror 001/002 — `ApplyComicBrowserMetrics()` on `ComicBrowserControl`, called from ctor/OnLoad and `DpiScaleChanged`.
- **Rationale**: Constitution V; proven pattern.
- **Alternatives**: AutoScaleMode.Font on browser (rejected — blast radius).

### D2: Thumb/tile defaults vs persisted config

- **Decision**: Metrics helper bumps **design baselines only** (128 thumb, 96 tile height); separate normalization when applying `ViewConfig` from workspace if sizes ≤ baseline (001 heuristic pattern).
- **Rationale**: FR-004; same double-scale guard as explorer shell.
- **Alternatives**: Always multiply on load (rejected).

### D3: Partial ctor scaling

- **Decision**: Keep existing column width and detail row scaling in ctor; metrics helper **re-applies** row/header/group metrics on DPI refresh rather than duplicating logic in two places—ctor calls helper once at end.
- **Rationale**: Avoid drift between init and refresh paths.
- **Alternatives**: Move all scaling only to helper (preferred refactor in implement—single call site).

### D4: Toolbar scaling

- **Decision**: Store original toolbar bitmaps; `ApplyToolbarMetrics()` inside browser metrics helper; pattern from `ComicListFolderFilesBrowser`.
- **Rationale**: FR-006; sort icons currently assigned unscaled at runtime (line ~1393).

### D5: Context menus

- **Decision**: Set scaled `ImageScalingSize` and `Font` on `contextMenuItems` (and rating/mark-as if needed for SC-004); do not redesign designer files.
- **Rationale**: FR-007 with minimal diff.
- **Alternatives**: Per-item Size in designer (rejected — churn).

### D6: Font choice

- **Decision**: Consider `SystemFonts.MessageBoxFont` for `itemView` on metrics refresh (align with 001 Folders tree) if validation shows IconTitleFont too small at 150%; default to refresh metrics with current font family first.
- **Rationale**: 001 precedent; validate in quickstart before forcing font change.

---

## Ready for planning

Phase 0 complete. See [plan.md](./plan.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md).
