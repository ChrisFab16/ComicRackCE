# Feature Specification: Plugin WebView2 SPA Host

**Feature Branch**: `006-plugin-webview-host`  
**Created**: 2026-09-23  
**Status**: Draft  
**Input**: Replace IronPython WinForms plugin Configure UI with WebView2-hosted SPA; Host API v1; nested package `ui/`; keep IronPython for logic (Phase 1).

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Open Configure as SPA (Priority: P1)

As a plugin user, I open Preferences → Configure (or menu Configure) for a package that ships `plugin.json` + `ui/dist`, and a Chromium WebView2 dialog shows the SPA instead of an IronPython WinForms form.

**Why this priority**: Unblocks modern plugin UI and HiDPI-safe layouts without rewriting the whole app shell.

**Independent Test**: Install `WebConfigureSample`; click Configure; dialog shows sample SPA; `host.getInfo` returns `apiVersion: 1`.

**Acceptance Scenarios**:

1. **Given** a package with valid `plugin.json` (`apiVersion: 1`, `ui.configure`), **When** Configure is invoked, **Then** WebView2 loads the entry HTML and Host API responds to `host.getInfo`.
2. **Given** WebView2 runtime is missing, **When** Configure is invoked, **Then** the user sees a clear message (not a silent failure).
3. **Given** `apiVersion` > host support, **When** Configure opens, **Then** bridge returns error `-32001` and UI can show unsupported message.

---

### User Story 2 — Nested package install (Priority: P1)

As a plugin author, I ship `ui/dist/index.html` inside a `.crplugin` zip and after install the nested path exists on disk.

**Why this priority**: Without nested unzip, SPA packaging is impossible.

**Independent Test**: Zip with nested `ui/dist/index.html`; install via PackageManager; assert file exists at `Scripts/<Name>/ui/dist/index.html`.

**Acceptance Scenarios**:

1. **Given** a zip with nested relative paths, **When** `UnzipFile` runs, **Then** directories are preserved.
2. **Given** a zip-slip entry (`../escape.txt`), **When** unzip runs, **Then** the entry is skipped/rejected and no file is written outside target.
3. **Given** a legacy flat zip, **When** unzip runs, **Then** files still land in the package root.

---

### User Story 3 — Hot-reload Configure UI (Priority: P2)

As a plugin developer, I replace files under `ui/dist` while Configure is open and the WebView reloads without restarting ComicRack.

**Why this priority**: Makes SPA iteration as easy as web dev; logic-hook restart still documented.

**Independent Test**: Open Configure; touch `index.html`; WebView reloads within ~1s when `hotReload` enabled.

**Acceptance Scenarios**:

1. **Given** `ui.hotReload` true (default), **When** configure entry files change, **Then** WebView reloads (debounced).
2. **Given** `ui.hotReload` false, **When** files change, **Then** WebView does not auto-reload.
3. **Given** only `.py` hook code changed, **When** user expects new behavior, **Then** docs state app restart is still required.

---

### User Story 4 — Sample SPA pilot (Priority: P2)

As a maintainer, I have an in-repo sample (`WebConfigureSample`) proving Host API methods and a Vite-built (or hand-built) SPA under `ui/dist`.

**Why this priority**: Reference for plugin authors; Library Organizer full port can follow out-of-band.

**Independent Test**: Built-in sample appears under Scripts; Configure shows book list from `host.getSelectedBooks` / library fallback; Save config round-trip.

**Acceptance Scenarios**:

1. **Given** sample installed with app, **When** Configure opens, **Then** SPA displays host product version and book captions.
2. **Given** SPA calls `host.config.set`, **When** Configure reopens, **Then** `host.config.get` returns saved value.

---

### Edge Cases

- Missing `plugin.json` but Python calls `ShowWebConfigure` → error message.
- Missing configure HTML path → error message.
- Empty library / no selection → empty `books` array, not exception.
- Concurrent Configure dialogs → each has own bridge/session.
- Non-ASCII paths under Scripts → WebView navigation still works (virtual host or proper file URI).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Host MUST open Configure UI via WebView2 when package `plugin.json` declares `ui.configure` (or Python invokes `ShowWebConfigure`).
- **FR-002**: Host MUST implement Host API v1 methods listed in `contracts/host-api-v1.md` over JSON-RPC WebMessages.
- **FR-003**: `PackageManager.UnzipFile` MUST preserve nested relative paths and reject zip-slip.
- **FR-004**: Host MUST support optional file-watch hot-reload for Configure SPA when `ui.hotReload` is not false.
- **FR-005**: IronPython hooks MUST continue to load as today (dual-host Phase 1; no removal of IronPython).
- **FR-006**: Sample package `WebConfigureSample` MUST ship under `ComicRack/Output/Scripts/` with `Package.ini`, `plugin.json`, Python stub, and `ui/dist`.
- **FR-007**: Host MUST surface WebView2 runtime absence with a user-visible error.
- **FR-008**: Host MUST refuse or error when plugin `apiVersion` is greater than implemented major version.

### Key Entities

- **PluginManifest**: Parsed `plugin.json`.
- **HostJsonRpc**: Dispatcher for SPA ↔ host methods.
- **WebViewPluginForm**: WinForms shell hosting WebView2.

## Success Criteria *(mandatory)*

- **SC-001**: Nested unzip unit test passes (nested path + zip-slip reject + flat compat).
- **SC-002**: Host JSON-RPC dispatcher unit tests pass for `host.getInfo` envelope and unknown method.
- **SC-003**: Sample Configure opens on a machine with WebView2 runtime (operator or local smoke).
- **SC-004**: Existing IronPython Sample.py hooks still discoverable (no regression in initializer).
- **SC-005**: Spec Kit analyze report CRITICAL=0 for this feature’s artifacts.

## Assumptions

- WebView2 Evergreen runtime is available on developer/operator Windows (document bootstrapper).
- Phase 2 (`kind: web` without IronPython) is out of scope for this feature’s must-ship code.
- Full Library Organizer SPA port is optional follow-up; in-repo sample satisfies pilot.
- Full ComicRack shell SPA rewrite is out of scope.
