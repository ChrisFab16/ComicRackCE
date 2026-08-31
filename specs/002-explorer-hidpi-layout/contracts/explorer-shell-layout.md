# Contract: Explorer Shell Layout Metrics

**Feature**: `002-explorer-hidpi-layout`  
**Applies to**: `ComicExplorerView`, `ComicExplorerViewSettings` (defaults only)

## Purpose

Define measurable layout behavior for the **explorer shell** (sidebar panel, preview pane, caption margins) at Windows display scales 100%, 125%, 150%, and 200%. Implementations MUST satisfy these contracts after init and after DPI change.

**Workspace load**: Governed by [001 workspace contract](../001-folders-hidpi-layout/contracts/workspace-dpi-splits.md) — this contract covers **runtime defaults and refresh only**.

---

## C-ESL-001: DPI scale authority

- **MUST** use `FormUtility.DpiScale` and `ScaleDpi*` helpers.
- **MUST** refresh metrics when `FormUtility.DpiScaleChanged` fires (001 infrastructure).

---

## C-ESL-002: Sidebar (sidePanel)

| Metric | Base (100%) | Rule |
|--------|-------------|------|
| Default expanded width | 252 | `ScaleDpiX(252)` when applying fresh/design baseline |
| Grip | right | Uses scaled `SizableContainer.GripWidth` from 001 |

**Acceptance**

- At 150% fresh workspace, sidebar width ≥ `ScaleDpiX(240)` (allows ± tolerance vs 252×1.5).
- Sidebar grip remains draggable (SC-003).

---

## C-ESL-003: Preview pane

| Metric | Base (100%) | Rule |
|--------|-------------|------|
| Default expanded height | 207 | `ScaleDpiY(207)` when applying fresh/design baseline |
| Bottom padding (when plugin docked bottom) | 6 | `ScaleDpiY(6)` |
| Plugin container bottom padding | 6 | `ScaleDpiY(6)` |

**Acceptance**

- At 150% with preview expanded and comic selected, caption text readable without clipping (SC-002).

---

## C-ESL-004: Caption margins

| Control | Base | Rule |
|---------|------|------|
| `smallComicPreview.CaptionMargin` | 2 | `new Padding(ScaleDpiX(2))` all sides |
| `comicBrowser.CaptionMargin` | 2 | same |

---

## C-ESL-005: Behavior preservation (FR-005)

Layout changes **MUST NOT** alter:

- Folder/library selection → comic list refresh
- Preview content update / timer
- Split drag persistence and collapse/expand toggles
- Workspace save/load semantics

---

## C-ESL-006: No double-scale on load (FR-004)

When `ViewSettings` setter applies persisted splits (post-001 normalization):

- **MUST NOT** run design-baseline scaling on those values.
- `ApplyExplorerShellMetrics()` **MAY** scale caption margins and padding always; split widths **ONLY** when value ≤ design baseline (252 / 207).

---

## C-ESL-007: Dark mode

Scaled layout **MUST** retain readable preview caption and list chrome in dark theme.
