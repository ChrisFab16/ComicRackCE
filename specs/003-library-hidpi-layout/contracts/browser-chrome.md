# Contract: Comic Browser Chrome

**Feature**: `003-library-hidpi-layout`  
**Applies to**: `ComicBrowserControl` toolbar and primary context menus

## Purpose

Scale comic browser toolbar and context menu metrics for HiDPI (FR-006, FR-007, user story 3).

---

## C-CBC-001: Toolbar images

- **MUST** scale toolbar button images via `.ScaleDpi()` from stored originals at init and on `DpiScaleChanged`.
- Applies to sort, group, stack, view, browse, undo/redo, sidebar buttons as present on strip.

**Acceptance**: At 150%, toolbar icons not soft-upscaled from 16×16-only without DPI scaling step (SC-003).

---

## C-CBC-002: Toolbar dimensions

| Metric | Base | Rule |
|--------|------|------|
| Strip height | 25 | `ScaleDpiY(25)` |
| Button size | 23×22 | `ScaleDpi(23×22)` |

---

## C-CBC-003: Context menus

- **MUST** set scaled `ImageScalingSize` on primary `contextMenuItems` (and rating/mark-as if icons clip).
- **MAY** set `Font` to `SystemFonts.MessageBoxFont` on refresh if default menu font too small at 150%.

**Acceptance**: At 150%, menu items readable and selectable without overlap.

---

## C-CBC-004: Behavior preservation

Toolbar and menu actions **MUST** behave identically to pre-change baseline.
