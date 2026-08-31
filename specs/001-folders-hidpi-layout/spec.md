# Feature Specification: Folders Tab HiDPI Layout Refresh

**Feature Branch**: `001-folders-hidpi-layout`

**Created**: 2026-08-31

**Status**: Draft

**Input**: User description: "Improve the Folders tab layout and readability on high-resolution displays. Focus on folder tree, favorites strip, and sidebar chrome so text scales cleanly and spacing feels comparable to modern Windows file browsing—not tiny, blurry, or cramped."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Readable folder navigation at high display scale (Priority: P1)

A user opens the **Folders** tab on a 1440p or 4K monitor with Windows display scaling set to 125%–200%. They browse the folder tree and favorites without squinting: folder names use comfortable type size, tree rows have adequate height, and expand/collapse affordances are easy to hit.

**Why this priority**: The folder tree is the primary navigation surface; unreadable text blocks the core workflow.

**Independent Test**: Launch app at 150% and 200% system scale, open Folders tab, expand three directory levels, and confirm all labels are legible without OS-level magnification.

**Acceptance Scenarios**:

1. **Given** display scaling is 150%, **When** the user opens the Folders tab and expands the folder tree, **Then** folder names are legible at normal viewing distance and row height fits the scaled text without vertical clipping.
2. **Given** display scaling is 200%, **When** the user selects folders in the tree, **Then** selection highlight and label text remain readable and aligned with row bounds.
3. **Given** the user switches between 100% and 150% scale (restart or session per platform behavior), **When** they reopen the Folders tab, **Then** tree layout and text size remain proportionate—not locked to an earlier tiny 96-DPI layout.

---

### User Story 2 - Favorites strip shows folder paths clearly (Priority: P2)

A user with favorite folders configured views the favorites area above the tree. Each favorite shows the folder name and path (or equivalent identifying text) with clear hierarchy: primary label prominent, secondary path readable, ellipsis only when space is genuinely insufficient—not because the tile is fixed to a sub-96-DPI height.

**Why this priority**: Favorites are the fast path to deep library paths; cramped tiles undermine the feature.

**Independent Test**: Add three favorites with long paths, set 150% scale, resize the sidebar narrower and wider; verify text reflows or truncates gracefully.

**Acceptance Scenarios**:

1. **Given** at least one favorite with a long path, **When** the favorites panel is visible at 150% scale, **Then** the user can distinguish folder name from full path without overlapping or clipped descenders.
2. **Given** the sidebar width is reduced, **When** favorites tiles reflow, **Then** text truncates with ellipsis on the path portion while keeping the folder name visible.

---

### User Story 3 - Sidebar toolbar and split layout feel proportionate (Priority: P3)

A user interacts with the Folders sidebar toolbar (favorites toggle, subfolder inclusion, refresh, open in tab/window). Icons and hit targets scale with display settings; the split between favorites, tree, and comic list does not leave a disproportionate blank band or microscopic controls.

**Why this priority**: Polish and parity with native shell apps; lower than tree/favorites text.

**Independent Test**: At 125% and 200%, verify toolbar button targets meet comfortable touch/mouse sizing and default splitter positions preserve usable tree height.

**Acceptance Scenarios**:

1. **Given** 150% display scale, **When** the user hovers toolbar buttons, **Then** icons are sharp (not upscaled from tiny bitmaps alone) and targets are no smaller than adjacent standard Windows toolbar expectations.
2. **Given** default workspace on first open at 150%, **When** favorites are expanded, **Then** the folder tree retains enough vertical space to show at least five tree rows without scrolling.

---

### Edge Cases

- Mixed-DPI: user moves window from built-in panel to external monitor with different scale (behavior should not regress vs single-monitor; ideal: layout updates without requiring manual zoom hacks).
- Very long path segments and deep nesting: tree horizontal scroll or ellipsis remains usable.
- Dark mode enabled: text contrast and selection colors remain readable after layout changes.
- Empty favorites list: layout does not collapse awkwardly or leave dead space that breaks tree docking.
- Network/special shell folders (OneDrive, UNC): icons and labels still align after scaling changes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Folders tab folder tree MUST scale text size and row spacing in proportion to the active display scale factor.
- **FR-002**: The Folders tab folder tree MUST keep labels fully visible within row bounds at 125%, 150%, and 200% Windows display scaling (no systematic clipping of descenders or selection text).
- **FR-003**: The favorites panel MUST allocate vertical space for tile content based on scaled typography, not a fixed low-DPI height alone.
- **FR-004**: Favorite entries MUST present a primary folder name and secondary path (or equivalent) with predictable truncation rules when horizontal space is limited.
- **FR-005**: The Folders sidebar toolbar MUST present controls whose interactive targets scale with display settings.
- **FR-006**: Default splitter proportions for favorites vs tree MUST preserve a minimum usable tree viewport (see SC-003) at common scale factors.
- **FR-007**: Layout and typography improvements MUST NOT alter folder browsing behavior (selection, drill-down, include subfolders, refresh, open in tab/window).
- **FR-008**: Changes MUST remain compatible with existing saved workspace/layout preferences where possible; breaking changes require documented migration or reset behavior.

### Key Entities

- **Folder tree node**: Shell-backed directory entry with display name, expansion state, selection state, icon.
- **Favorite folder**: User-pinned path with display label, full path, thumbnail/tile presentation.
- **Folders sidebar layout**: Toolbar, favorites container, tree, and split dimensions persisted in workspace settings.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In moderated review at 150% and 200% scale, at least 90% of test participants rate folder tree text as "legible" or better without OS magnifier (5-point scale, legible = 4+).
- **SC-002**: At 150% scale, zero tree rows in a standard test folder hierarchy (≥20 nodes visible) exhibit clipped label text when selected or unselected.
- **SC-003**: At 150% scale with default workspace, the folder tree shows ≥5 full tree rows without scrolling when favorites are expanded with ≤3 entries.
- **SC-004**: After layout refresh, time to locate and open a known favorite folder does not increase versus pre-change baseline on the same machine (within 10% in timed task with 5 users / 3 trials).

## Assumptions

- Primary target is Windows 10/11 with system display scaling 125%–200%; 100% remains supported but is not the optimization focus.
- Scope is the **Folders** tab sidebar (toolbar, favorites, folder tree) within the main explorer layout; library-wide HiDPI and reader view are follow-on work unless explicitly added later.
- Validation uses manual quickstart on physical or VM Windows with scale changes; automated pixel tests are optional supplements, not substitutes (Constitution III).
- Visual parity with Windows 11 Explorer is aspirational; success means clearly improved legibility and spacing within ComicRackCE, not a full shell rewrite.
- Implementation remains within the existing desktop UI stack (Constitution II); no cross-platform UI scope.

## Out of Scope (v1)

- Replacing WinForms with WinUI/WPF/Avalonia.
- Full-application HiDPI audit (preferences dialogs, library grid, reader).
- Redesign of comic list pane layout beyond fixes required by shared sidebar width changes.
- New folder management features (sync, cloud providers, tagging).
