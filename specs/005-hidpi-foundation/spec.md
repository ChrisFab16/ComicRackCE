# Feature Specification: HiDPI Foundation Redesign

**Feature Branch**: `005-hidpi-foundation` (based on `upstream/dev`)

**Created**: 2026-09-01

**Status**: Draft

**Input**: Redesign HiDPI per [PR #278 review](https://github.com/maforget/ComicRackCE/pull/278#issuecomment-5490677717) and [issue #118](https://github.com/maforget/ComicRackCE/issues/118). Replace the ad-hoc `Apply*Metrics()` approach with correct manifest embedding, per-window DPI detection, and non-mutating view-config scaling before re-landing layout fixes from features 001–003.

**Supersedes (partially)**: PR #278 foundation assumptions; features 001–003 layout code remains valid **after** this foundation is verified.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Process actually runs PerMonitorV2 (Priority: P1)

A developer builds ComicRack CE from source after changing `app.manifest`. The **embedded** application manifest in `ComicRack.exe` reports `PerMonitorV2` (not legacy `system`). Windows applies per-monitor DPI awareness to the process.

**Why this priority**: Without this, all subsequent DPI work may only reflect primary-monitor manual scaling (PR #278 gap).

**Independent Test**: Build Debug; inspect EXE manifest (e.g. `mt -inputresource:ComicRack.exe;#1` or Resource Hacker); confirm `dpiAwareness` contains `PerMonitorV2`.

**Acceptance Scenarios**:

1. **Given** `app.manifest` sets `PerMonitorV2`, **When** the solution is built, **Then** `myressources.res` is regenerated from `myressources.rc` and the linked EXE embeds that manifest.
2. **Given** a clean build on a machine with VS/rc.exe available, **When** PreBuild runs, **Then** build succeeds without manual `.res` copy steps.

---

### User Story 2 — Per-window DPI on monitor change (Priority: P1)

A user moves a ComicRack window (MainForm, ReaderForm, or a standard dialog) from a 100% monitor to a 150% monitor. The window receives a DPI change notification and `FormUtility` reflects **that window's** scale—not only the primary display DPI.

**Why this priority**: maforget confirmed `Form.DpiChanged` did not fire with current setup; global `GetDC(IntPtr.Zero)` is insufficient.

**Independent Test**: Two monitors at different scale; drag MainForm and ReaderForm; log or breakpoint confirms `WM_DPICHANGED` handled on `FormEx`; `FormUtility` scale updates for the affected form.

**Acceptance Scenarios**:

1. **Given** PerMonitorV2 is active, **When** a `FormEx` window is moved to a higher-DPI monitor, **Then** `WndProc` handles `WM_DPICHANGED` and raises a DPI refresh for that form.
2. **Given** display scale changes while the app runs, **When** the user returns to a form, **Then** subscribed controls can re-apply metrics without requiring a full app restart.

---

### User Story 3 — View config is not rewritten on load (Priority: P2)

A user with an existing workspace opens the library at 150% scale. Thumb/tile/row sizes **display** scaled correctly but persisted `ItemViewConfig` values in workspace XML are **not** permanently overwritten by a one-time normalization heuristic.

**Why this priority**: maforget: use existing `GetItemSize`/`SetItemSize`; do not mutate user config.

**Independent Test**: Load workspace at 150%; verify XML thumb height unchanged; UI shows scaled sizes via runtime apply; user resize still persists intentional changes.

**Acceptance Scenarios**:

1. **Given** a workspace saved at 100% with thumb height 128, **When** opened at 150%, **Then** displayed thumbs scale for readability and workspace file still stores 128 (or user-changed value), not a rewritten scaled integer.
2. **Given** the user adjusts thumb size via UI, **When** workspace saves, **Then** the new value persists as the user's chosen logical size.

---

### User Story 4 — Pilot AutoScaleMode.Dpi on one dialog (Priority: P3)

A developer validates upstream direction ([issue #118](https://github.com/maforget/ComicRackCE/issues/118)): one low-risk `FormEx` dialog (e.g. `ProgressDialog` or `ZoomDialog`) uses `AutoScaleMode.Dpi` instead of `Font`, with manifest + FormEx foundation in place.

**Why this priority**: Explores maforget's preferred long-term approach without big-bang migration.

**Independent Test**: Open pilot dialog at 125% and 200%; layout proportionate; no regression at 100%.

**Acceptance Scenarios**:

1. **Given** foundation P1–P2 complete, **When** pilot dialog opens at 150%, **Then** controls and fonts scale without manual `ScaleDpi` on every child.
2. **Given** pilot dialog at 100%, **When** compared to pre-change baseline, **Then** no worse clipping or overlap.

---

### User Story 5 — Automated validation gates (Priority: P1)

A developer pushes HiDPI foundation changes. CI runs post-build manifest verification and unit tests for config semantics **without** operator manual steps. Operator quickstart covers only hardware-dependent or subjective scenarios.

**Why this priority**: PR #278 failed because SC-001 was never automated; UI sign-off at 150% did not prove PerMonitorV2 was embedded.

**Independent Test**: After `msbuild`, `scripts/verify-embedded-manifest.ps1` exits 0; `ComicRack.Tests` passes on CI.

**Acceptance Scenarios**:

1. **Given** a build on CI, **When** manifest verification runs, **Then** job fails if EXE lacks embedded `PerMonitorV2`.
2. **Given** view-config contract tests, **When** `dotnet test` runs, **Then** workspace thumb height round-trip and no `NormalizeViewConfigSizes` guard pass.

---

### Edge Cases

- Build machine without VS `rc.exe`: PreBuild fails; manifest verify requires successful full build. **CI is authoritative** for SC-001/SC-005; local dev needs VS Build Tools or skip building ComicRack until tooling installed.
- Single-monitor users: foundation must not regress 100% behavior.
- `DisplaySettingsChanged` remains as coarse fallback when `WM_DPICHANGED` unavailable.
- Custom owner-draw controls (ItemView, TabBar): remain on explicit metrics hooks—not AutoScaleMode—in this feature.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Build MUST embed `app.manifest` via regenerated `myressources.res` (not stale binary `.res` alone).
- **FR-002**: `app.manifest` MUST declare `PerMonitorV2` in `dpiAwareness`.
- **FR-003**: PreBuild MUST invoke upstream-aligned `compile_res_file.ps1` (or equivalent) before linking.
- **FR-004**: `FormEx` MUST handle `WM_DPICHANGED` and notify DPI subscribers for that form's HWND.
- **FR-005**: `FormUtility` MUST expose scale derived from a **control/window** DC (e.g. `GetDpiForWindow` / `CreateGraphics` on owner), not only `GetDC(IntPtr.Zero)`.
- **FR-006**: `DpiScaleChanged` (or successor) MUST identify which form triggered refresh where possible.
- **FR-007**: `NormalizeViewConfigSizes` (or equivalent) MUST NOT rewrite persisted workspace `ItemViewConfig`; scale at apply/display time using existing `GetItemSize`/`SetItemSize` patterns.
- **FR-008**: PR #278 scope MUST NOT be re-submitted until **SC-005 (CI)** and operator gates **SC-002 / SC-006** pass per quickstart (foundation gate).
- **FR-009**: Unrelated changes (e.g. DarkMode `SetSidePanelColor` on Folders-only paths) MUST NOT be included in foundation PR.
- **FR-010**: One pilot dialog MAY switch to `AutoScaleMode.Dpi` as documented experiment (P3).
- **FR-011**: Post-build script MUST verify embedded `PerMonitorV2` in EXE and `.res` (see `contracts/validation-automation.md`).
- **FR-012**: GitHub Actions workflows MUST run manifest verification after build; failure fails CI.
- **FR-013**: `ComicRack.Tests` MUST cover SC-003 **persistence contract** (XML round-trip, no `NormalizeViewConfigSizes` guard, FormUtility DPI events)—not full UI workspace load at 150%; operator Scenario 3 optional for end-to-end confirm.

### Key Entities

- **Embedded application manifest**: Win32 resource `#1` in `ComicRack.exe`.
- **Form DPI context**: Per-`FormEx` scale factor updated on `WM_DPICHANGED`.
- **ItemViewConfig (persisted)**: User/workspace thumb/tile/row preferences—logical design values, scaled at runtime.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Post-build manifest inspection shows `PerMonitorV2` in 100% of release/Debug builds on reference machine.
- **SC-002**: Moving MainForm across two monitors with different scale triggers at least one DPI refresh with non-primary scale factor (logged or quickstart sign-off).
- **SC-003**: Workspace XML thumb height unchanged after load at 150% when user makes no resize action (FR-007). **CI** contract tests (T036–T037) prove serialization + no load-time mutation helper; optional operator Scenario 3 confirms end-to-end workspace file.
- **SC-004**: Pilot dialog (`ProgressDialog` or chosen alternative) layout acceptable at 125% and 150% (operator, optional).
- **SC-005**: CI manifest script and unit tests pass on every build; recorded in `validation-results.md` **CI** column before operator sign-off.
- **SC-006**: Operator regression smoke at 150% primary monitor (Folders tree + Library thumbs legible); required before upstream PR. Constitution III satisfied for foundation scope via **SC-005 (CI) + SC-002/SC-006 (operator)** — full 100–200% matrix deferred to Phase E / layout features.

## Assumptions

- Target .NET Framework 4.8 WinForms on Windows 10/11; full AutoScaleMode.Dpi everywhere is a multi-release effort ([issue #118](https://github.com/maforget/ComicRackCE/issues/118)).
- Upstream `upstream/dev` provides `compile_res_file.ps1` as reference implementation.
- Features 001–003 layout helpers (`ApplyFoldersSidebarMetrics`, etc.) are **re-validated** on a branch after 005—not deleted in this feature.
- Plugin HiDPI (004) remains in plugin repo; out of scope here.

## Out of Scope

- Re-submitting PR #278 as-is.
- Scaling every form/dialog in one release.
- IronPython plugin host hooks (004 shelved).
- Replacing ItemView/TabBar owner-draw with AutoScaleMode.
