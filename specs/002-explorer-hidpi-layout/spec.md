# Feature Specification: Explorer Shell HiDPI Layout Refresh

**Feature Branch**: `002-explorer-hidpi-layout`

**Created**: 2026-08-31

**Status**: Draft

**Input**: User description: "Scale the ComicExplorerView shell layout (sidebar width, preview pane, margins) for HiDPI displays at 125%–200%. Follow-on to 001-folders-hidpi-layout research P3."

**Depends on**: `001-folders-hidpi-layout` (PerMonitorV2, `FormUtility.RefreshDpiScale`, workspace split normalization on load).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Explorer sidebar width feels usable at high scale (Priority: P1)

A user opens the **Folders** or **Library** explorer layout at 125%–200% display scale. The left sidebar (folder tree / library browser panel) is wide enough to read labels and use split grips without feeling stuck at a tiny 96-DPI default width.

**Why this priority**: Sidebar width frames all navigation; a narrow fixed panel undermines the Folders tab improvements from feature 001.

**Independent Test**: At 150% scale, open Folders tab with default or reset workspace; sidebar width and grip affordance feel proportionate to scaled content inside the panel.

**Acceptance Scenarios**:

1. **Given** display scaling is 150%, **When** the user opens the Folders tab with a fresh workspace, **Then** the sidebar default width accommodates scaled folder tree text without excessive horizontal scrolling for typical path lengths.
2. **Given** display scaling is 200%, **When** the user drags the sidebar width grip, **Then** the grip hit target and panel resize behavior remain comfortable and predictable.
3. **Given** a saved workspace from feature 001 normalization, **When** the user loads it at 150%, **Then** sidebar width remains usable and is not locked to an unscaled 96-DPI width.

---

### User Story 2 - Preview pane proportions at high scale (Priority: P2)

A user enables the comic preview pane in the explorer layout. At 125%–200% scale, the preview area height, caption spacing, and padding feel balanced relative to the comic list—not a thin strip with cramped caption text.

**Why this priority**: Preview pane is secondary to navigation but visible on the same explorer shell; fixed 96-DPI heights read as broken at HiDPI.

**Independent Test**: At 150%, enable preview pane on Folders tab; verify preview height and caption margin look proportionate and caption text is readable.

**Acceptance Scenarios**:

1. **Given** preview pane expanded at 150% scale, **When** the user selects a comic, **Then** preview image and caption area allocate space proportionate to display scale.
2. **Given** 150% scale, **When** the user resizes the preview split, **Then** default preview height on fresh workspace is not stuck at a 96-DPI pixel value.

---

### User Story 3 - Main tab strip icons and spacing (Priority: P3)

A user switches between **Library** and **Folders** (and related explorer tabs) at HiDPI scale. Tab icons and padding scale with display settings and remain easy to click.

**Why this priority**: Lower visibility than explorer splits but completes the “shell around the sidebar” experience identified in research §6.

**Independent Test**: At 150%, verify Library/Folders tab buttons in the main view strip have sharp icons and comfortable padding.

**Acceptance Scenarios**:

1. **Given** 150% display scale, **When** the user views the main explorer tab strip, **Then** tab icons are sharp and padding does not leave icons cramped against neighbors.

---

### Edge Cases

- User collapses sidebar or preview pane: scaled defaults apply when re-expanded without layout collapse.
- Mixed-DPI monitor move: explorer shell metrics refresh when DPI changes (best-effort, consistent with feature 001).
- Dark mode: preview caption and tab strip remain readable after scaling.
- Very narrow window: user-resized splits still honored; scaling affects defaults and DPI refresh only.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Default explorer **sidebar width** MUST scale with the active display scale factor on fresh workspace.
- **FR-002**: Default **preview pane height** and related preview chrome margins MUST scale with display scale on fresh workspace.
- **FR-003**: Explorer shell layout MUST re-apply scaled metrics when display scale changes, consistent with feature 001 DPI refresh behavior.
- **FR-004**: Saved workspace splits MUST remain compatible with feature 001 load-time normalization; this feature MUST NOT double-scale already-normalized values.
- **FR-005**: Layout changes MUST NOT alter explorer behavior (folder selection, comic list refresh, preview content, split drag persistence).
- **FR-006**: Main explorer tab strip icons and padding MUST scale at 125%, 150%, and 200% (user story 3).
- **FR-007**: Changes MUST reuse existing display-scale utilities rather than introducing parallel scaling systems.

### Key Entities

- **Explorer shell layout**: Sidebar panel, preview pane, split dimensions, caption margins—hosted by the file/library explorer view.
- **Workspace settings**: Persisted split widths/heights (already normalized on load by feature 001).
- **Main view tab chrome**: Library/Folders tab buttons and icons in the primary navigation strip.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At 150% scale on fresh workspace, ≥90% of test participants rate sidebar width as “adequate” or better for reading folder/library labels without magnifier (5-point scale, adequate = 4+).
- **SC-002**: At 150% scale with preview pane expanded, caption text is readable without clipping in a standard single-comic preview.
- **SC-003**: At 150% scale, sidebar grip and preview split grip remain draggable with no regression versus 100% baseline behavior.
- **SC-004**: Zero functional regressions in explorer navigation flows (folder select, comic list update, preview toggle) in manual regression pass.

## Assumptions

- Feature 001 is merged or available on the branch (PerMonitorV2, DPI refresh, workspace heuristic).
- Scope is **ComicExplorerView** shell and **MainView** tab strip padding/icons—not the comic list grid, preferences dialogs, or reader (separate follow-ons).
- Target scales: 125%, 150%, 200%; 100% remains supported.
- Manual quickstart on Windows is the validation gate (Constitution III).

## Out of Scope (v1)

- Comic list pane (`ComicBrowserControl`) thumb/tile layout.
- Library grid / smart list columns beyond shared explorer shell.
- Preferences dialogs and reader view HiDPI.
- Persisting `SavedAtDpiPercent` metadata in workspace XML (deferred from feature 001).
- Replacing WinForms or full-app HiDPI audit.
