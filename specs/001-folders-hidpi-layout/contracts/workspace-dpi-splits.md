# Contract: Workspace Split DPI Normalization

**Feature**: `001-folders-hidpi-layout`  
**Applies to**: `ComicExplorerView.ViewSettings` setter, `ComicExplorerViewSettings`, `DisplayWorkspace.FileView`

## Purpose

Ensure saved sidebar split dimensions remain usable after HiDPI fixes when users upgrade from builds that stored **unscaled** pixel values (FR-008).

---

## C-WSP-001: Legacy split heuristic

When **loading** settings into `ComicExplorerView` (Folders / file view):

| Setting | Legacy threshold | Action if `DpiScale.Y > 1.01` and value ≤ threshold |
|---------|------------------|-----------------------------------------------------|
| `TopBrowserSplit` | 150 | `value = ScaleDpiY(value)` |
| `BrowserSplit` | 250 | `value = ScaleDpiY(value)` |
| `PreviewSplit` | 200 | `value = ScaleDpiY(value)` |
| `InfoBrowserSize` | 200×150 | `value = value.ScaleDpi()` |

Apply each multiplier **at most once per load** (guard flag or compare to scaled defaults to avoid double-scaling saves from fixed build).

---

## C-WSP-002: Save behavior

On **save** (`ViewSettings` getter → workspace store):

- Persist actual pixel dimensions as today (effective sizes after user drag).
- No requirement to persist DPI metadata in v1.

---

## C-WSP-003: User-visible regression

After upgrade:

- Favorites pane **MUST NOT** collapse to unusable sliver (< `ScaleDpiY(40)`) unless user explicitly resized it there.
- Sidebar width **MUST** remain draggable; restored width ≥ `ScaleDpiX(100)` unless user saved smaller.

---

## C-WSP-004: Failure mode

If normalization throws or receives invalid (≤0) values:

- Fall back to `ComicExplorerViewSettings()` constructor defaults (already ScaleDpi-aware).

---

## Test cases

1. Workspace saved at 100% with `TopBrowserSplit=150` → open at 150% display → favorites height ≈ 225px (150 × 1.5).
2. Workspace saved at 150% with already-scaled `TopBrowserSplit=225` → open at 150% → no double scale (≈225 remains).
3. Fresh install at 150% → defaults match constructor ScaleDpi values.
