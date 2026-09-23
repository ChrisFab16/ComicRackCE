# Feature Specification: WebView2 HTML info panels

**Feature Branch**: `008-webview-html-panels`  
**Created**: 2026-09-23  
**Status**: Draft  
**Depends on**: 006 (WebView2 package already referenced)  
**Input**: Phase 3 — replace IE `WebBrowser` in plugin HTML panels (`ComicInfoHtml` / `QuickOpenHtml`) with WebView2.

## User Scenarios & Testing

### User Story 1 — HTML info panel renders in WebView2 (Priority: P1)

As a user, ComicInfoHtml / QuickOpenHtml panels show plugin HTML (or `!url` navigation) using Edge WebView2 instead of IE.

**Independent Test**: Enable Sample.py Dummy Book Info HTML; open info sidebar; HTML table renders.

**Acceptance Scenarios**:

1. **Given** InfoFunction returns HTML markup, **When** ShowInfo runs, **Then** WebView2 displays the markup.
2. **Given** InfoFunction returns `!https://...`, **When** ShowInfo runs, **Then** WebView2 navigates to that URL.
3. **Given** WebView2 runtime missing, **When** panel loads, **Then** user sees a clear error (or empty panel with message), not a crash.

### User Story 2 — window.external compatibility (Priority: P1)

As a plugin author, existing `window.external.ComicRack.OpenBooks.OpenFile(...)` and `window.external.Config` usage from Sample.py still works.

**Independent Test**: Click Open in Dummy Book Info HTML; book opens / config round-trip on dispose.

**Acceptance Scenarios**:

1. **Given** HTML calls `window.external.ComicRack.OpenBooks.OpenFile`, **When** clicked, **Then** host opens the book.
2. **Given** HTML mutates `window.external.Config`, **When** panel disposes, **Then** SaveConfigFunction receives updated config.

### User Story 3 — No IE dependency for these panels (Priority: P2)

**Acceptance**: `HtmlComicPageControl` no longer instantiates `System.Windows.Forms.WebBrowser`.

## Requirements

- **FR-001**: `HtmlComicPageControl` MUST host content with WebView2.
- **FR-002**: MUST support HTML string and `!url` results (same as today).
- **FR-003**: MUST expose a `window.external`-compatible bridge for `Config` and `ComicRack.OpenBooks.OpenFile` (COM host object + polyfill).
- **FR-004**: MUST preserve SaveConfig-on-dispose behavior.
- **FR-005**: ScriptErrors / context menu settings SHOULD map to WebView2 settings where applicable.

## Success Criteria

- **SC-001**: Unit test for `!url` vs HTML payload parsing helper.
- **SC-002**: Operator: Sample ComicInfoHtml panel renders and Open works (or documented partial).
- **SC-003**: No `new WebBrowser()` in HtmlComicPageControl.
- **SC-004**: Analyze CRITICAL=0; IronPython hooks unchanged.

## Assumptions

- Full `PluginEnvironment` surface via host objects is not guaranteed; a focused COM proxy covers Sample + common OpenBooks/Config.
- NewsDialog IE usage is out of scope.
