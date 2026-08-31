# Data Model: Library Comic List HiDPI Layout

**Feature**: `003-library-hidpi-layout`  
**Date**: 2026-08-31

Logical entities for comic list layout. No new database tables.

---

## ComicListLayout (runtime, in-memory)

Ephemeral layout held by `ComicBrowserControl` / `ItemView`.

| Field | Type | Source | Notes |
|-------|------|--------|-------|
| `ViewMode` | enum | `itemView.ItemViewMode` | Thumb / Tile / Detail |
| `ThumbSize` | Size | `itemView.ItemThumbSize` | Baseline 128×128 |
| `TileSize` | Size | `itemView.ItemTileSize` | Baseline 192×96 (height 96) |
| `RowHeight` | int | `itemView.ItemRowHeight` | Detail/list rows |
| `ColumnHeaderHeight` | int | `itemView.ColumnHeaderHeight` | Matches row metrics |
| `GroupHeaderHeight` | int | `itemView.GroupHeaderHeight` | Baseline 40 |
| `CurrentDpiScale` | PointF | `FormUtility.DpiScale` | Refreshed via 001 infrastructure |

**Validation**

- Fresh workspace at 150% thumb mode: `ThumbSize` ≈ `ScaleDpi(128×128)`.
- Tile mode at 150%: height ≈ `ScaleDpiY(96)`.

**State transitions**

- DPI change → `FormUtility.DpiScaleChanged` → `ApplyComicBrowserMetrics()` → invalidate layout.
- Workspace load → `ViewConfig` setter → normalize sizes ≤ baseline → **do not** double-scale user sizes above baseline.

---

## BrowserChrome (runtime)

| Field | Type | Source | Notes |
|-------|------|--------|-------|
| `ToolbarHeight` | int | `toolStrip.Height` | Baseline ~25 |
| `ToolbarButtonSize` | Size | ToolStripButton | Baseline ~23×22 |
| `ContextMenuImageScale` | Size | `contextMenuItems.ImageScalingSize` | Scaled from 16×16 baseline |
| `ContextMenuFont` | Font | `contextMenuItems.Font` | MessageBoxFont or scaled default |

---

## ItemViewConfig (persisted — unchanged schema)

From existing workspace / list config:

| Field | Baseline (96-DPI) | Normalization threshold (003) |
|-------|-------------------|-------------------------------|
| `ThumbnailSize` | 128×128 typical | Scale if height ≤ 128 |
| `TileSize` | 192×96 typical | Scale if height ≤ 96 |
| Row height in config | via ItemRowHeight | Scale if ≤ unscaled detail baseline |

No schema change. Normalization in apply path only.

---

## Relationships

```text
ComicExplorerView
  └── comicBrowser: ComicBrowserControl
        └── itemView: ItemView
              └── items: CoverViewItem[]

MainView
  ├── dbView → ComicBrowserControl (Library)
  └── fileView → ComicBrowserControl (Folders)
```

---

## Dependencies

- **001-folders-hidpi-layout**: DPI infrastructure, toolbar pattern reference.
- **002-explorer-hidpi-layout**: Explorer shell scaled; comic list is independent pane.
