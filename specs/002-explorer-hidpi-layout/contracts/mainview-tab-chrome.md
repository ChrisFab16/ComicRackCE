# Contract: MainView Explorer Tab Chrome

**Feature**: `002-explorer-hidpi-layout`  
**Applies to**: `MainView` (`tsbLibrary`, `tsbFolders`, `tsbPages`)

## Purpose

Scale explorer tab strip icons and padding for HiDPI (FR-006, user story 3).

---

## C-MTC-001: Tab images

- **MUST** scale tab images via `.ScaleDpi()` from stored original bitmaps at init and on `DpiScaleChanged`.
- Applies to: Library, Folders, Pages tab items (and any explorer tabs using fixed embedded images).

**Acceptance**

- At 150%, icons not visibly soft-upscaled from 16×16-only without DPI scaling step.

---

## C-MTC-002: Tab padding

| Tab | Base padding | Rule |
|-----|--------------|------|
| Library | (8,0,0,0) | `(ScaleDpiX(8), 0, 0, 0)` |
| Folders | (0,0,8,0) | `(0, 0, ScaleDpiX(8), 0)` |
| Pages | (0,0,8,0) | `(0, 0, ScaleDpiX(8), 0)` |

**Acceptance**

- At 150%, icons not cramped against tab captions or neighbors.

---

## C-MTC-003: Behavior preservation

- Tab click, selection, and view switching **MUST** behave identically to pre-change baseline.
