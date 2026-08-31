# Contract: Comic List Layout Metrics

**Feature**: `003-library-hidpi-layout`  
**Applies to**: `ComicBrowserControl`, `ItemView`

## Purpose

Define measurable layout behavior for the **comic list pane** (thumb/tile/detail modes) at Windows display scales 100%, 125%, 150%, and 200%.

---

## C-CLL-001: DPI scale authority

- **MUST** use `FormUtility.DpiScale` and `ScaleDpi*` helpers.
- **MUST** refresh metrics when `FormUtility.DpiScaleChanged` fires.

---

## C-CLL-002: Thumbnail mode

| Metric | Base (100%) | Rule |
|--------|-------------|------|
| Thumb size | 128×128 | `ScaleDpi(128×128)` when height ≤ 128 |

**Acceptance**: At 150% fresh/default thumb view, covers proportionate; titles readable (SC-001).

---

## C-CLL-003: Tile mode

| Metric | Base (100%) | Rule |
|--------|-------------|------|
| Tile size | 192×96 | height `ScaleDpiY(96)`, width `height × 2` when tile height ≤ 96 |

**Acceptance**: At 150% tile view, primary labels not vertically clipped (SC-001).

---

## C-CLL-004: Detail/list mode

| Metric | Base | Rule |
|--------|------|------|
| Row height | Font + 6 | `Font.Height + ScaleDpiY(6)` on refresh |
| Column header | = row height | Same as row height |
| Group header | 40 | `ScaleDpiY(40)` when ≤ baseline |

**Acceptance**: At 150% list view, row text not clipped (SC-002).

---

## C-CLL-005: Persisted view config (FR-004)

When applying `ViewConfig` from workspace:

- **MUST** normalize thumb/tile/row sizes ≤ 96-DPI baselines once (001 heuristic pattern).
- **MUST NOT** re-scale sizes clearly above baseline (user-custom).

---

## C-CLL-006: Behavior preservation (FR-005)

Layout changes **MUST NOT** alter selection, sort, group, stack, search, or open/read flows.

---

## C-CLL-007: Dark mode

Scaled layout **MUST** retain readable list text and group headers in dark theme.
