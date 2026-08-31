# Data Model: Explorer Shell HiDPI Layout

**Feature**: `002-explorer-hidpi-layout`  
**Date**: 2026-08-31

Logical entities for explorer shell layout. No new database tables; extends 001 workspace model.

---

## ExplorerShellLayout (runtime, in-memory)

Ephemeral layout held by `ComicExplorerView` (Library `dbView` and Folders `fileView`).

| Field | Type | Source | Notes |
|-------|------|--------|-------|
| `SidebarExpanded` | bool | `sidePanel.Expanded` | User toggle |
| `SidebarWidth` | int (px) | `sidePanel.ExpandedWidth` / `SplitterDistance` | Maps to `BrowserSplit` |
| `PreviewExpanded` | bool | `previewPane.Expanded` | User toggle |
| `PreviewHeight` | int (px) | `previewPane.ExpandedWidth` | Maps to `PreviewSplit` |
| `PreviewCaptionMargin` | Padding | `smallComicPreview.CaptionMargin` | Scale all sides from baseline 2 |
| `ComicListCaptionMargin` | Padding | `comicBrowser.CaptionMargin` | Scale from baseline 2 |
| `PreviewBottomPadding` | int | `previewPane.Padding.Bottom` | Runtime 0 or 6 — scale 6 |
| `CurrentDpiScale` | PointF | `FormUtility.DpiScale` | Refreshed via 001 infrastructure |

**Validation**

- Fresh workspace at 150%: `SidebarWidth` ≈ `ScaleDpiX(252)` unless user saved other.
- `PreviewHeight` ≈ `ScaleDpiY(207)` on fresh workspace at 150%.

**State transitions**

- DPI change → `FormUtility.DpiScaleChanged` → `ApplyExplorerShellMetrics()` → invalidate layout.
- Workspace load → `ViewSettings` setter (with 001 `NormalizeLegacySplits`) → **do not** call metrics helper on loaded values.

---

## MainViewTabChrome (runtime)

| Field | Type | Source | Notes |
|-------|------|--------|-------|
| `LibraryTabPadding` | Padding | `tsbLibrary.Padding` | Baseline (8,0,0,0) |
| `FoldersTabPadding` | Padding | `tsbFolders.Padding` | Baseline (0,0,8,0) |
| `PagesTabPadding` | Padding | `tsbPages.Padding` | Baseline (0,0,8,0) |
| `TabImagesScaled` | bool | internal flag | Re-scale from originals on DPI change |

---

## ComicExplorerViewSettings (persisted — unchanged schema)

From feature 001; relevant fields:

| Field | Baseline (96-DPI) | Constructor (001) | Load heuristic (001) |
|-------|-------------------|-------------------|----------------------|
| `BrowserSplit` | 250 (XML default 150) | `ScaleDpiY(250)` | Scale if ≤250 |
| `PreviewSplit` | 200 (XML default 150) | `ScaleDpiY(200)` | Scale if ≤200 |

Feature 002 does **not** add fields. Runtime shell init aligns designer **252/207** with scaled defaults.

---

## Relationships

```text
MainView
  ├── dbView: ComicExplorerView (Library)
  │     └── treePanel → ComicListLibraryBrowser
  └── fileView: ComicExplorerView (Folders)
        └── treePanel → ComicListFolderFilesBrowser

DisplayWorkspace
  ├── LibraryView: ComicExplorerViewSettings
  └── FileView: ComicExplorerViewSettings
```

---

## Dependencies

- **001-folders-hidpi-layout**: `FormUtility.RefreshDpiScale`, `DpiScaleChanged`, `NormalizeLegacySplits`, `SizableContainer` grip scaling.
