# Contract: Folders Sidebar Layout Metrics

**Feature**: `001-folders-hidpi-layout`  
**Applies to**: `ComicListFolderFilesBrowser`, `FolderTreeView`, `FolderViewItem`, `favView` (`ItemView`)

## Purpose

Define measurable layout behavior for the Folders tab **left sidebar** at Windows display scales 100%, 125%, 150%, and 200%. Implementations MUST satisfy these contracts at runtime after init and after DPI change.

---

## C-FSL-001: DPI scale authority

- **MUST** use `FormUtility.DpiScale` (or `ScaleDpi*` helpers) for all metrics marked *scaled* below.
- **MUST** refresh metrics when `DpiScale` is invalidated (DPI changed event).

---

## C-FSL-002: Folder tree

| Metric | Base (100%) | Rule |
|--------|-------------|------|
| Indent | 15 | `ScaleDpiX(15)` |
| Item height | — | `Font.Height + ScaleDpiY(8)` minimum |
| Font | MessageBoxFont | NOT `IconTitleFont` |
| Icon size | 16×16 | `new Size(16,16).ScaleDpi()` in image list |
| Skin | NiceTreeSkin | Owner-draw enabled when skin attached |

**Acceptance**

- At 150% and 200%, no selected/unselected label clips vertically (SC-002).
- Expand/collapse glyphs and folder icons align vertically within row bounds.

---

## C-FSL-003: Favorites strip

| Metric | Base (100%) | Rule |
|--------|-------------|------|
| Container top padding | 6 | `ScaleDpiY(6)` |
| Default expanded height | 160 | `ScaleDpiY(160)` initial; user resize allowed |
| Tile width | — | `ClientWidth - VerticalScrollBarWidth - ScaleDpiX(8)` |
| Tile height | 50 min | `max(ScaleDpiY(50), 2 * lineHeight + ScaleDpiY(12))` |
| Tile mode | Tile | unchanged |

**Text lines** (`FolderViewItem`)

- Line 1: folder name, bold, ~1.2× base font size.
- Line 2: full path, ~0.8× base font, ellipsis path trimming when width constrained.
- Border: `ScaleDpi(2)` per axis.
- Line spacing (2, 5 base): scale with `ScaleDpiY`.

**Acceptance**

- At 150%, name and path visually distinct; no descender clipping (FR-004, user story 2).

---

## C-FSL-004: Toolbar

- Toolstrip button images **MUST** pass through `.ScaleDpi()` at init (same pattern as `PreferencesDialog`).
- Interactive target **SHOULD** be ≥ `ScaleDpiY(22)` effective height at 100% baseline.

**Acceptance**

- At 150%, icons not visibly soft-upscaled from 16×16-only assets without DPI scaling step.

---

## C-FSL-005: Minimum tree viewport (SC-003)

**Given** default workspace at 150%, favorites expanded, ≤3 favorites:

- Folder tree client area **MUST** show ≥ 5 full tree rows without vertical scroll.

**Default split guidance**: `TopBrowserSplit` ≥ `ScaleDpiY(150)` but tree must meet row count via ItemHeight + container layout — adjust default fav height if needed during implement.

---

## C-FSL-006: Behavior preservation (FR-007)

Layout changes **MUST NOT** alter:

- Folder selection → comic list refresh
- Include subfolders toggle
- Refresh, open in tab/window, add favorite, add to library
- Shell folder drill-down / rename rules

---

## C-FSL-007: Dark mode

When `ThemeManager.IsDarkModeEnabled`, scaled layout **MUST** retain readable contrast for tree labels, favorites text, and selection highlights (no new colors required if existing theme tokens used).
