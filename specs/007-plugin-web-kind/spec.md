# Feature Specification: Plugin kind=web dual-host

**Feature Branch**: `007-plugin-web-kind`  
**Created**: 2026-09-23  
**Status**: Draft  
**Depends on**: `006-plugin-webview-host` (Host API v1, WebView2 Configure)  
**Input**: Phase 2 — dual-host `kind: python` | `kind: web`, migration guide, IronPython deprecation policy.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Install a web-only plugin (Priority: P1)

As a user, I install a package that has `plugin.json` with `"kind": "web"` and `ui.configure`, **no** `.py` files, and after restart I can open Configure (SPA) from Preferences.

**Why this priority**: Proves dual-host without IronPython for UI plugins.

**Independent Test**: Install `WebKindSample`; select it in Scripts; Configure opens Host API SPA.

**Acceptance Scenarios**:

1. **Given** a `kind: web` package with valid `plugin.json` + `ui.configure`, **When** PluginEngine initializes, **Then** a command is registered with Configure linked.
2. **Given** that command, **When** Configure is clicked, **Then** WebView2 SPA loads (same Host API v1 as 006).
3. **Given** missing `ui.configure` on `kind: web`, **When** initialize runs, **Then** no command is registered (or clear skip; no crash).

---

### User Story 2 — Python plugins unchanged (Priority: P1)

As a user of existing IronPython plugins (Library Organizer, scrapers), behavior is unchanged when web-kind support is added.

**Why this priority**: Dual-host must not break the ecosystem.

**Independent Test**: `WebConfigureSample` (kind python) and built-in Sample.py still discover; unit regression on initializer.

**Acceptance Scenarios**:

1. **Given** a `kind: python` (or omitted kind) package with `.py` hooks, **When** engine loads, **Then** Python initializer still owns those commands.
2. **Given** both a `.py` ConfigScript and `plugin.json` in the same package, **When** engine loads, **Then** Python ConfigScript wins for that Key (no duplicate Configure).

---

### User Story 3 — Migration + deprecation docs (Priority: P2)

As a plugin author, I can read a migration guide (hooks → manifest, WinForms → SPA, `ComicRack.App` → Host API) and a published IronPython deprecation timeline.

**Why this priority**: Sets expectations before any removal.

**Independent Test**: Docs exist under `specs/007-plugin-web-kind/` and are linked from `AGENTS.md`.

**Acceptance Scenarios**:

1. **Given** the migration guide, **When** an author maps ConfigScript + WinForms dialog, **Then** the guide shows SPA + `ShowWebConfigure` / `kind: web` path.
2. **Given** the deprecation policy, **When** read, **Then** it states dual-run window and that IronPython is not removed in this feature.

---

### Edge Cases

- `kind: web` with unsupported `apiVersion` → skip package, no crash.
- Duplicate Keys across packages → existing first-wins behavior.
- `hooks` array empty → default one Books (or Library) placeholder command for Preferences listing.
- Zip nested `ui/dist` still required (006 packaging).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Host MUST discover `plugin.json` files and, when `kind` is `web`, register managed commands without requiring `.py`.
- **FR-002**: `kind: web` with `ui.configure` MUST expose Configure that opens WebView2 Host API v1 UI.
- **FR-003**: Existing IronPython discovery MUST remain the default for packages without `kind: web`.
- **FR-004**: When both Python ConfigScript and web auto-Configure share a Key, Python ConfigScript MUST take precedence.
- **FR-005**: Repo MUST include a migration guide and IronPython deprecation policy (docs only; no removal in this feature).
- **FR-006**: Sample `WebKindSample` (`kind: web`, no `.py`) MUST ship under `ComicRack/Output/Scripts/`.

### Key Entities

- **WebPluginCommand**: Managed `Command` for web-kind hooks / Configure.
- **JsonPluginInitializer**: Loads commands from `plugin.json`.

## Success Criteria *(mandatory)*

- **SC-001**: Unit tests cover web-kind command creation from manifest and python-precedence for Configure.
- **SC-002**: `WebKindSample` loads without `.py` and Configure opens (operator or local smoke).
- **SC-003**: IronPython Sample / WebConfigureSample still discoverable (no regression).
- **SC-004**: Migration guide + deprecation policy present; analyze CRITICAL=0.
- **SC-005**: This feature does **not** remove IronPython or break `kind: python` packages.

## Assumptions

- Full JS runtime for Books/Library/smartlist logic hooks is **out of scope**; web-kind MVP is SPA Configure (+ placeholder primary hook for listing).
- Deprecation is policy/docs only in 007; removal is a future feature after dual-run.
