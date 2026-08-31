# Feature Specification: Library Comic List HiDPI Layout Refresh

**Feature Branch**: `003-library-hidpi-layout`

**Created**: 2026-08-31

**Status**: Draft

**Input**: User description: "Scale ComicBrowserControl comic list grid (thumbs, tiles, columns, context menus) for HiDPI at 125%–200%. Follow-on from 001/002 research — library grid deferred from explorer shell work."

**Depends on**: `001-folders-hidpi-layout`, `002-explorer-hidpi-layout` (PerMonitorV2, `FormUtility.RefreshDpiScale`, `DpiScaleChanged`, explorer shell scaled).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Comic covers readable in thumb/tile view (Priority: P1)

A user browses comics in **Library** or **Folders** tab using thumbnail or tile layout at 125%–200% display scale. Cover images and primary labels are sized proportionately—not stuck at tiny 96-DPI defaults with blurry upscaling.

**Why this priority**: The comic list is the main content area; sidebar/shell fixes from 001/002 leave the grid itself as the primary HiDPI gap (001 research §7).

**Independent Test**: At 150%, open Library with thumb or tile view; verify cover size and title text feel proportionate and readable without magnifier.

**Acceptance Scenarios**:

1. **Given** display scaling is 150%, **When** the user opens Library in default thumb view, **Then** cover thumbnails scale with display scale and titles are readable.
2. **Given** display scaling is 200%, **When** the user switches to tile view, **Then** tile height accommodates scaled typography without clipping primary labels.
3. **Given** 150% scale, **When** the user resizes the comic list pane, **Then** item sizes refresh proportionately (not locked to unscaled defaults).

---

### User Story 2 - Detail/list rows and column headers (Priority: P2)

A user uses list or detail layout modes at HiDPI scale. Row height, column spacing, and header text remain readable and aligned.

**Why this priority**: Many users use list/detail modes for metadata-heavy browsing; fixed row heights break at 150%+.

**Independent Test**: At 150%, switch Library to list or detail view; verify row height and column header text without vertical clipping.

**Acceptance Scenarios**:

1. **Given** 150% scale in list view, **When** the user scrolls a long series list, **Then** row text is not clipped vertically.
2. **Given** 150% scale, **When** the user views column headers, **Then** header labels are readable and sort affordances remain usable.

---

### User Story 3 - Browser toolbar and context menus (Priority: P3)

A user interacts with the comic browser toolbar (sort, group, layout, search) and context menus at HiDPI scale. Icons, padding, and menu item height feel proportionate and easy to click.

**Why this priority**: Toolbar/menus share fixed-pixel patterns with other unscaled chrome; completes the comic list pane experience.

**Independent Test**: At 150%, use toolbar buttons and open a comic context menu; verify icon sharpness and comfortable hit targets.

**Acceptance Scenarios**:

1. **Given** 150% scale, **When** the user views the comic browser toolbar, **Then** icons are sharp and strip height is proportionate.
2. **Given** 150% scale, **When** the user opens a comic context menu, **Then** menu items are readable and selectable without overlap.

---

### Edge Cases

- User changes display scale while app is running: comic list metrics refresh on DPI change (consistent with 001/002).
- Mixed view modes (thumb/tile/list/detail): each mode scales independently without breaking persisted view config.
- Dark mode: list text and toolbar remain readable after scaling.
- Very large libraries: scaling MUST NOT introduce perceptible scroll jank on refresh (subjective smoothness).
- Folders tab comic list shares `ComicBrowserControl` — both Library and Folders tabs MUST benefit from the same fixes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Default thumb and tile item dimensions MUST scale with active display scale on load and after DPI change.
- **FR-002**: List/detail row height and related spacing MUST scale proportionately at 125%, 150%, and 200%.
- **FR-003**: Comic browser layout MUST re-apply scaled metrics when display scale changes, consistent with 001/002 DPI refresh behavior.
- **FR-004**: Persisted view configuration (thumb size, layout mode, column widths) MUST remain compatible; scaling applies to defaults and refresh paths without corrupting user-saved layout preferences.
- **FR-005**: Layout changes MUST NOT alter comic selection, sorting, grouping, search, or open/read behaviors.
- **FR-006**: Comic browser toolbar icons, padding, and height MUST scale at HiDPI.
- **FR-007**: Context menu item metrics (height, icon size where present) MUST scale at HiDPI where fixed pixels are used.
- **FR-008**: Changes MUST reuse existing display-scale utilities rather than introducing parallel scaling systems.

### Key Entities

- **Comic list layout**: View mode (thumb/tile/list/detail), item dimensions, row height, column headers—hosted by the comic browser pane.
- **View configuration**: Persisted thumb size, columns, and layout settings tied to library/folders explorer views.
- **Browser chrome**: Toolbar strip, context menus, and related controls on the comic list pane.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At 150% scale, ≥90% of test participants rate comic cover/title legibility as “adequate” or better in thumb/tile view (5-point scale, adequate = 4+).
- **SC-002**: At 150% scale in list view, primary row text shows no vertical clipping for standard single-line fields.
- **SC-003**: At 150% scale, toolbar buttons remain clickable with no regression versus 100% baseline hit targets.
- **SC-004**: Zero functional regressions in comic list flows (select, sort, group, search, open comic) in manual regression pass.

## Assumptions

- Features 001 and 002 are on the branch (DPI foundation + explorer shell).
- Scope is **ComicBrowserControl** / shared **ItemView** defaults used by Library and Folders comic lists—not the reader, preferences dialogs, or smart-list editor dialogs (separate follow-ons).
- Target scales: 125%, 150%, 200%; 100% remains supported.
- Manual quickstart on Windows is the validation gate (Constitution III).

## Out of Scope (v1)

- Reader view HiDPI.
- Preferences dialogs and list layout editor dialogs.
- Library sidebar smart-list tree (covered by 001 patterns where shared; no dedicated re-audit unless regression found).
- Persisting `SavedAtDpiPercent` in workspace XML (deferred feature 004).
- Replacing WinForms text rendering engine (DirectWrite) — optional text-hint follow-on only if needed after metrics scaling.
