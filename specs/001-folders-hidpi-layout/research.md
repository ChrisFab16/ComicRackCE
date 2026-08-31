# Research: Folders Tab Fixed-Pixel & DPI Audit

**Feature**: `001-folders-hidpi-layout`  
**Date**: 2026-08-31  
**Scope**: Folders tab UI — left sidebar (`ComicListFolderFilesBrowser`), its hosting shell (`ComicExplorerView` / `MainView.fileView`), and shared controls (`FolderTreeView`, `FolderViewItem`, `ItemView` favorites, `SizableContainer`).

**Legend**

| Symbol | Meaning |
|--------|---------|
| ✅ | Passes through `FormUtility.ScaleDpi*` (or `.ScaleDpi()` extension) |
| ⚠️ | Partially scaled, or scaled only in code-path / runtime event — not at design default |
| ❌ | Fixed logical pixel (96-DPI assumption) |
| 🔄 | Derived from font / `SystemInformation` — scales only if font or OS metrics change |
| N/A | Non-layout (behavior, color, enum) |

**Global DPI context (affects entire Folders tab)**

| Item | Location | ScaleDpi? | Notes |
|------|----------|-----------|-------|
| Process DPI awareness | `app.manifest` | N/A | `dpiAwareness=system` (not PerMonitorV2) |
| Legacy DPI API | `Program.Main` → `SetProcessDPIAware()` | N/A | Pre–Win8.1 API; no per-monitor v2 |
| WinForms HiDPI auto-resize | `app.config` | N/A | `EnableWindowsFormsHighDpiAutoResizing=true` but views use `AutoScaleMode.None` |
| Cached scale factor | `FormUtility.DpiScale` | ⚠️ | Computed **once** from primary monitor `GetDeviceCaps`; never refreshed on move/scale change |
| Main shell auto-scale | `MainForm`, explorer views | ❌ | `AutoScaleMode.None` throughout Folders stack |

---

## 1. `ComicListFolderFilesBrowser` (Designer + code)

Primary file: `ComicRack/Views/ComicListFolderFilesBrowser.Designer.cs`

| Control / property | Value (96-DPI design) | ScaleDpi? | File:line (approx) |
|--------------------|----------------------|-----------|---------------------|
| `AutoScaleMode` | `None` | ❌ | Designer:282 |
| Root `Size` | 379×454 | ❌ | Designer:287 |
| **toolStrip** height | 25 | ❌ | Designer:154 |
| ToolStripButton size | 23×22 (each) | ❌ | Designer:164,174,188,198,213 |
| ToolStripSeparator width | 6 | ❌ | Designer:180,205 |
| Context menu item height | 22 | ❌ | Designer:85,92,104,… |
| Context menu width | 196–197 | ❌ | Designer:79,… |
| **favContainer** initial size | 379×**160** | ❌ | Designer:225 |
| `favContainer.Padding` top | **6** | ❌ | Designer:224 |
| **favView** location Y | 6 (inside padding) | ❌ | Designer:238 |
| **favView** initial size | 379×**148** | ❌ | Designer:241 |
| **tvFolders.Indent** | **15** | ❌ | Designer:133 |
| `tvFolders` design bounds | Y=185, H=269 | ❌ | Designer:134–137 |
| `tvFolders.Font` | `SystemFonts.IconTitleFont` | 🔄 | `.cs`:103 — small legacy font, not scaled explicitly |
| `favView.ItemTileSize` height | **50** (width dynamic) | ✅ height only | `.cs`:196 — `ScaleDpiY(50)` on **Resize** only |
| `favView_Resize` width margin | `VerticalScrollBarWidth + **8**` | ⚠️ | `.cs`:195 — scrollbar is 🔄; **8** is ❌ |
| `ItemView` default tile (before resize) | 192×96 | ❌ | `ItemView.cs` default; favView uses Tile mode until first resize |
| Toolbar / menu images | embedded bitmaps | ❌ | Designer — no `.ScaleDpi()` in this view (contrast Library prefs) |

**Runtime-only scaling**

- `favView_Resize` is the **only** explicit ScaleDpi call in this view.
- If resize does not fire before paint, favorites use default 192×96 tile from `ItemView`.

---

## 2. `FolderTreeView` + tree rendering

Files: `cYo.Common.Windows/Forms/FolderTreeView.cs`, `TreeViewEx.cs`

| Item | Value | ScaleDpi? | Notes |
|------|-------|-----------|-------|
| `ImageList.ImageSize` initial | 16×16 | ✅ | Constructor:230–233 → `.ScaleDpi()` |
| Shell folder icons | from `ShellPidl.GetImage` | 🔄 | OS-provided; size follows image list |
| `Indent` | **15** (designer) | ❌ | Not overridden in code |
| `ItemHeight` | WinForms default | ⚠️ | **No** `TreeViewSkinner` attached — unlike Library tree |
| Owner-draw skin | none | N/A | `NiceTreeSkin` used in `ComicListLibraryBrowser` only, **not** Folders tree |
| `TreeViewSkinner.ItemHeight` formula | `Font.Height + ScaleDpiY(8)` | ✅ (if skin used) | Not applied to Folders tree |
| `DragScrollRegion` | 10 | ❌ | `TreeViewEx.cs`:99 — default drag scroll band |
| Double-buffer / theme colors | P/Invoke | N/A | `SetSidePanelColor()` on Init |

**Text rendering (Folders tree)**

- Native `TreeView` label paint (not `NiceTreeSkin`).
- Library tree uses `TextRenderer` + `TextRenderingHint.SystemDefault` in `NiceTreeSkin` — Folders tree does not get that path.

---

## 3. Favorites tiles — `FolderViewItem` + `ThumbTileRenderer`

Files: `ComicRack/Controls/FolderViewItem.cs`, `ComicRack.Engine/Drawing/ThumbTileRenderer.cs`

| Item | Value | ScaleDpi? | Notes |
|------|-------|-----------|-------|
| `Border` | **2×2** | ❌ | `FolderViewItem.cs`:63 |
| Title font scale | `FC.GetRelative(font, **1.2f**, Bold)` | 🔄 | Relative to `ItemView.Font`, not DPI |
| Path font scale | `FC.GetRelative(font, **0.8f**)` | 🔄 | Same |
| TextLine `BeforeSpacing` / after | **0, 2** / **0, 5** | ❌ | `FolderViewItem.cs`:67–68 |
| Mosaic thumbnail base size | **341×512** | ❌ | `FolderViewItem.cs`:77 |
| Mosaic grid | **3×4** | N/A | |
| `ThumbTileRenderer` background pad | **4** | ❌ | Inflate -4,-4 |
| Image/text split | width/3, **+4** px | ❌ | `ThumbTileRenderer.cs`:86–91 |
| Default renderer font | `SystemFonts.IconTitleFont` | 🔄 | `ThumbTileRenderer.cs`:14 |
| Text draw API | `Graphics.DrawString` via `SimpleTextRenderer` | ❌ | GDI+ path; no ClearType hint set here |

---

## 4. `SizableContainer` (favorites splitter + explorer side panel)

File: `cYo.Common.Windows/Forms/SizableContainer.cs`

Used by: `favContainer` (favorites height), `ComicExplorerView.sidePanel` (sidebar width), `previewPane`.

| Item | Default | ScaleDpi? | Notes |
|------|---------|-----------|-------|
| `gripWidth` | **6** | ❌ | Lines 44, 125 |
| `SlideTime` | 100 ms | N/A | Animation |
| Click drag threshold | **4** px | ❌ | Line 585 |
| `ExpandedWidth` persisted | user/workspace | ⚠️ | Stored as raw pixels in settings XML |
| Designer default sizes | 160 / 252 / 207 | ❌ | See explorer designer |

---

## 5. `ComicExplorerView` shell (Folders tab host)

File: `ComicRack/Views/ComicExplorerView.Designer.cs` + `.cs`

Folders tab = `MainView.fileView` → `ComicExplorerView` hosting `ComicListFolderFilesBrowser` in `treePanel`.

| Control / property | Design value | ScaleDpi? | Notes |
|--------------------|-------------|-----------|-------|
| `AutoScaleMode` | `None` | ❌ | Designer:140 |
| Root size | 700×538 | ❌ | Designer:146 |
| **sidePanel** size | **252** × 538 | ❌ | Designer:85 — sidebar width |
| `treePanel` size | 246×331 | ❌ | Designer:94 |
| **previewPane** height | **207** | ❌ | Designer:105 |
| `smallComicPreview.CaptionMargin` | **2** | ❌ | Designer:52 |
| `previewPane` padding (runtime) | bottom **6** | ❌ | `.cs`:279–280 |
| `previewTimer.Interval` | 500 ms | N/A | |

### Persisted splits — `ComicExplorerViewSettings`

File: `ComicRack/Views/ComicExplorerViewSettings.cs`

| Setting | XML `DefaultValue` | Constructor default | ScaleDpi on **new**? | ScaleDpi on **load** from workspace? |
|---------|-------------------|---------------------|----------------------|--------------------------------------|
| `BrowserSplit` | 150 | ✅ `ScaleDpiY(250)` | ✅ new objects only | ❌ restored raw from XML |
| `PreviewSplit` | 150 | ✅ `ScaleDpiY(200)` | ✅ new objects only | ❌ restored raw from XML |
| `TopBrowserSplit` | 150 | ✅ `ScaleDpiY(150)` | ✅ new objects only | ❌ restored raw from XML |
| `InfoBrowserSize` | 200×150 | ✅ `.ScaleDpi()` | ✅ new objects only | ❌ restored raw from XML |

**Risk**: Users who saved workspace at 100% scale carry 150px favorites height to 200% display — sidebar stays physically tiny.

---

## 6. `MainView` tab chrome (Folders entry)

File: `ComicRack/Views/MainView.cs`

| Item | Value | ScaleDpi? |
|------|-------|-----------|
| `tsbFolders.Padding` | (0,0,**8**,0) | ❌ |
| `tsbLibrary.Padding` | (**8**,0,0,0) | ❌ |
| Tab icons | `Resources.FileBrowser` etc. | ❌ — bitmap, not scaled in MainView |

---

## 7. Comic list pane (right side — same Folders tab)

Not part of the folder **sidebar**, but visible on Folders tab: `ComicBrowserControl` inside `ComicExplorerView`. ItemView thumb/tile/row defaults (`128×128`, `192×96`, row height) follow global browser settings — same partial ScaleDpi patterns as Library. **Out of detailed audit** unless sidebar work changes shared `ItemView` defaults.

---

## Summary counts (Folders sidebar stack)

| Category | Count (approx) |
|----------|----------------|
| Explicit ❌ fixed layout pixels in Folders designer | **25+** properties |
| ✅ ScaleDpi at point of use | **3** (image list size, fav tile height on resize, settings ctor defaults) |
| ⚠️ partial / conditional | **6** (DpiScale cache, workspace reload, ItemHeight without skin, resize-only tile, scrollbar+8 margin, ExpandedWidth persistence) |
| Font 🔄 without DPI hook | **2** (`IconTitleFont` on tree; ItemView font for tiles) |

---

## Punch list (implementation order)

### P0 — HiDPI correctness

1. **Manifest + DPI lifecycle**: PerMonitorV2; remove redundant `SetProcessDPIAware`; handle `DpiChanged` / refresh `FormUtility.DpiScale`.
2. **`tvFolders.Indent`**: Replace `15` with `FormUtility.ScaleDpiX(15)` at init (and on DPI change).
3. **`tvFolders` row height**: Set `ItemHeight = Font.Height + ScaleDpiY(8)` (match `TreeViewSkinner` formula) even without owner-draw skin; consider attaching `NiceTreeSkin` for consistent text with Library.
4. **Tree font**: Replace `SystemFonts.IconTitleFont` with scaled `SystemFonts.MessageBoxFont` or `Font` from current DPI.
5. **`favView` tile height**: Scale on load, not only `Resize`; base height > `ScaleDpiY(50)` if two text lines + mosaic thumb (derive from font metrics).
6. **Workspace split reload**: Scale `TopBrowserSplit`, `BrowserSplit`, `PreviewSplit`, `InfoBrowserSize` when applying saved settings if stored at different DPI (or store DPI metadata).

### P1 — Layout polish

7. **favContainer** default height (**160**) and padding (**6**): scale at init.
8. **Toolbar**: `.ScaleDpi()` on button images (pattern from `PreferencesDialog.cs`); verify toolstrip height at 150%+.
9. **FolderViewItem** text spacing (**2**, **5**), border (**2**), mosaic size (**341×512**): scale or derive from tile bounds.
10. **favView_Resize** margin **8**: `ScaleDpiX(8)`.
11. **SizableContainer `gripWidth` (6)**: scale for side panel and favorites grip.

### P2 — Text quality

12. Unify tree/tile text rendering hints (ClearType / `TextRenderer` vs GDI+ `DrawString`).
13. Evaluate owner-draw `NiceTreeSkin` for folder tree parity with Library.

### P3 — Explorer shell (Folders tab width)

14. Default **sidePanel** width **252** and **previewPane** **207**: scale defaults; optional minimum tree rows check (spec SC-003).

---

## Files to touch (expected)

| File | Why |
|------|-----|
| `ComicRack/app.manifest` | PerMonitorV2 |
| `ComicRack/Program.cs` | DPI changed handling |
| `cYo.Common.Windows/Forms/FormUtility.cs` | Invalidate/recalculate DpiScale |
| `ComicRack/Views/ComicListFolderFilesBrowser.cs` | Font, init scaling, fav tile on load |
| `ComicRack/Views/ComicListFolderFilesBrowser.Designer.cs` | Or move metrics to ctor with ScaleDpi |
| `cYo.Common.Windows/Forms/FolderTreeView.cs` | Indent, ItemHeight |
| `ComicRack/Controls/FolderViewItem.cs` | Tile text/spacing/border |
| `ComicRack/Views/ComicExplorerViewSettings.cs` | Scale on deserialize / apply |
| `ComicRack/Views/ComicExplorerView.cs` | Apply scaled workspace |
| `cYo.Common.Windows/Forms/SizableContainer.cs` | gripWidth (shared — test Library too) |

---

## Phase 0 Design Decisions

Decisions below resolve all planning unknowns. Rationale references audit findings above.

### D1: DPI awareness mode

- **Decision**: Upgrade manifest to `PerMonitorV2`; add runtime `DpiChanged` handler to call `FormUtility.RefreshDpiScale()` and re-apply Folders sidebar metrics.
- **Rationale**: System DPI + cached primary-monitor scale causes wrong layout on mixed-DPI and after scale changes (global audit table).
- **Alternatives considered**: Keep system DPI only (rejected — fails edge cases in spec); full app AutoScaleMode.Font (rejected — high blast radius, Constitution V).

### D2: Folder tree rendering

- **Decision**: Attach existing `NiceTreeSkin` to `FolderTreeView` on Folders tab; set scaled `Indent` and font to `SystemFonts.MessageBoxFont` (or control default after DPI refresh).
- **Rationale**: Library tree already uses NiceTreeSkin + `ItemHeight = Font.Height + ScaleDpiY(8)`; Folders tree uses native paint with fixed indent (Section 2).
- **Alternatives considered**: Native tree only with manual ItemHeight (rejected — inconsistent text/selection vs Library); custom new skin (rejected — duplication).

### D3: Favorites tile height

- **Decision**: `ItemTileSize.Height = max(ScaleDpiY(50), fontMetrics * 2 lines + ScaleDpiY(12))` applied on control load, resize, and DPI change — not resize-only.
- **Rationale**: Two-line `FolderViewItem` text clipped at fixed 50px at 150%+ (Sections 1, 3).
- **Alternatives considered**: Increase fixed 50→80 without font metrics (rejected — still breaks at 200% or large fonts).

### D4: Workspace split compatibility (FR-008)

- **Decision**: On apply of `ComicExplorerViewSettings`, if `TopBrowserSplit` / `BrowserSplit` / `PreviewSplit` ≤ unscaled defaults (150/150/150) and current `DpiScale.Y > 1.01`, multiply by `DpiScale.Y` once (heuristic for legacy 96-DPI saves). New saves store effective pixel values at save time; optional future: persist `SavedAtDpiPercent` attribute (deferred).
- **Rationale**: Section 5 — saved workspace reload bypasses ScaleDpi.
- **Alternatives considered**: Reset all splits on upgrade (rejected — poor UX); always multiply on load (rejected — double-scale on new saves).

### D5: Shared control changes

- **Decision**: Scale `SizableContainer.GripWidth` via `ScaleDpiX(6)` at runtime when grip is shown; verify Library sidebar still acceptable.
- **Rationale**: Section 4; grip hit-target too small at 200%.
- **Alternatives considered**: Folders-only subclass (rejected — duplicate container logic).

### D6: Out of scope confirmation

- **Decision**: Comic list pane (`ComicBrowserControl`), full-app manifest side-effects beyond Folders validation, and text rendering engine swap (DirectWrite) deferred to follow-on.
- **Rationale**: spec.md Out of Scope; P2 text hints optional if time in tasks.

---

## Ready for planning

Phase 0 complete. See [plan.md](./plan.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md).
