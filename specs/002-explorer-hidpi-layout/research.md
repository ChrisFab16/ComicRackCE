# Research: Explorer Shell Fixed-Pixel & DPI Audit

**Feature**: `002-explorer-hidpi-layout`  
**Date**: 2026-08-31  
**Scope**: `ComicExplorerView` shell, `MainView` explorer tab strip. Baseline audit from [001 research §5–6](../001-folders-hidpi-layout/research.md).

**Legend**: Same as 001 (✅ ⚠️ ❌ 🔄 N/A)

**Prerequisite**: Feature **001** merged — PerMonitorV2, `FormUtility.RefreshDpiScale()`, `DpiScaleChanged`, `NormalizeLegacySplits()` on workspace load.

---

## 1. `ComicExplorerView` shell

Files: `ComicRack/Views/ComicExplorerView.Designer.cs`, `ComicExplorerView.cs`

| Control / property | Design value (96-DPI) | ScaleDpi? (pre-002) | Notes |
|--------------------|----------------------|---------------------|-------|
| `AutoScaleMode` | `None` | ❌ | Designer:140 |
| Root size | 700×538 | ❌ | Designer:146 |
| **sidePanel** size | **252** × 538 | ❌ | Designer:85 — `SplitterDistance` / `BrowserSplit` |
| `treePanel` size | 246×331 | ❌ | Designer:94 — follows sidePanel |
| **previewPane** size | 246×**207** | ❌ | Designer:105 — `PreviewSplit` |
| `smallComicPreview.CaptionMargin` | **2** all sides | ❌ | Designer:52 |
| `comicBrowser.CaptionMargin` | **2** | ❌ | Designer:64 |
| `previewPane` bottom padding | **6** (runtime) | ❌ | `.cs`:280 — `UpdatePreviewPadding` |
| `pluginContainer` bottom padding | **6** (runtime) | ❌ | `.cs`:281 |
| `SizableContainer.gripWidth` | 6 | ✅ | Scaled in 001 `SizableContainer` ctor |

### Persisted splits — post-001

| Setting | Constructor default (001) | Load normalization (001) | Runtime default apply (002 gap) |
|---------|---------------------------|----------------------------|--------------------------------|
| `BrowserSplit` | `ScaleDpiY(250)` | C-WSP-001 if ≤250 | Designer **252** not applied at init |
| `PreviewSplit` | `ScaleDpiY(200)` | C-WSP-001 if ≤200 | Designer **207** not applied at init |
| `TopBrowserSplit` | `ScaleDpiY(150)` | C-WSP-001 | Handled in 001 Folders browser |

**Gap**: Settings ctor scales **new** objects; designer-fixed `sidePanel`/`previewPane` dimensions and caption margins remain 96-DPI until user drags splits or loads workspace.

---

## 2. `MainView` tab strip (research §6)

File: `ComicRack/Views/MainView.cs`

| Item | Value | ScaleDpi? (pre-002) |
|------|-------|---------------------|
| `tsbLibrary.Padding` | (8,0,0,0) | ❌ |
| `tsbFolders.Padding` | (0,0,8,0) | ❌ |
| `tsbPages.Padding` | (0,0,8,0) | ❌ |
| Tab images | `Resources.Library`, `FileBrowser`, `ComicPage` | ❌ — raw bitmaps |

`TabBar` item height uses `Font.Height + ScaleDpiY(12)` internally — tab **icons** and **padding** are the gap.

---

## 3. Shared with Library explorer

Both `MainView.dbView` and `MainView.fileView` are `ComicExplorerView` instances. **One** `ApplyExplorerShellMetrics()` implementation covers Library and Folders explorer layouts.

---

## 4. Out of scope (confirmed)

- `ComicBrowserControl` comic list thumbs/tiles (001 research §7)
- `ComicListFolderFilesBrowser` inner sidebar (001 complete)
- Info browser / plugin container expanded sizes beyond padding (unless needed for SC-002 caption — caption margin only in v1)

---

## Punch list (implementation order)

### P0 — Explorer shell

1. `ApplyExplorerShellMetrics()` on `ComicExplorerView`
2. Default `sidePanel.ExpandedWidth` from `ScaleDpiX(252)` when at/below design baseline
3. Default `previewPane.ExpandedWidth` from `ScaleDpiY(207)` when at/below design baseline
4. Scale caption margins (2) and preview/plugin padding (6)
5. Hook `FormUtility.DpiScaleChanged`

### P1 — MainView tabs

6. Scale tab images from originals on load + DPI refresh
7. Scale tab horizontal padding (8)

### P2 — Validation

8. quickstart Scenarios 1–5; Library + Folders both exercised

---

## Phase 0 Design Decisions

### D1: Metrics helper pattern

- **Decision**: Mirror 001 — `ApplyExplorerShellMetrics()` on `ComicExplorerView`, called from ctor/OnLoad and `DpiScaleChanged`.
- **Rationale**: Constitution V; proven pattern from `ComicListFolderFilesBrowser`.
- **Alternatives**: AutoScaleMode.Font on explorer (rejected — blast radius).

### D2: Default vs persisted splits

- **Decision**: Metrics helper adjusts **design baselines only** (252, 207, margins); **never** re-scale values coming from `ViewSettings` setter (001 normalization owns load path).
- **Rationale**: FR-004 double-scale guard.
- **Alternatives**: Re-run normalization in metrics helper (rejected).

### D3: Sidebar width baseline

- **Decision**: Use design **252** as 96-DPI baseline for `sidePanel.ExpandedWidth`; align with `BrowserSplit` default `ScaleDpiY(250)` within ±2px tolerance when applying fresh defaults.
- **Rationale**: Designer uses 252; settings ctor uses 250 scaled — document both; apply `ScaleDpiX(252)` for shell init.
- **Alternatives**: Change designer to 250 (rejected — unnecessary churn).

### D4: MainView tab scaling

- **Decision**: Store original tab images; re-apply `.ScaleDpi()` on DPI refresh; padding via `ScaleDpiX(8)`.
- **Rationale**: Same as 001 toolbar / PreferencesDialog pattern.
- **Alternatives**: Replace TabBar (rejected).

### D5: Dependency on 001

- **Decision**: Feature 002 **requires** 001 DPI foundation on branch; do not re-implement manifest/RefreshDpiScale.
- **Rationale**: Spec Assumptions; avoid duplicate PR surface for upstream.

---

## Ready for planning

Phase 0 complete. See [plan.md](./plan.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md).
