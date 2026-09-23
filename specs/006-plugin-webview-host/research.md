# Research: Plugin SPA host

## Decision: WebView2 + JSON-RPC messages (not AddHostObjectToScript)

**Rationale:** Message-based API versions cleanly (`apiVersion`), works with SPA frameworks, avoids COM-visible .NET host object fragility.

## Decision: Keep IronPython (Phase 1)

**Rationale:** Ecosystem cost; Configure UI is the painful WinForms/HiDPI part. Logic hooks stay Python until Phase 2.

## Decision: Nested unzip with zip-slip guard

**Rationale:** Required for `ui/dist`; flat unzip is a latent packaging bug for any nested assets.

## Decision: In-repo sample over Library Organizer port

**Rationale:** LO lives in external fork; sample proves host contract without blocking CE feature. LO can migrate later against Host API v1.

## Alternatives considered

| Alt | Why not now |
|-----|-------------|
| IE WebBrowser upgrade only | No modern SPA tooling |
| Python.NET | Does not deliver SPA UI |
| Full shell SPA | Multi-year; out of scope |
| Out-of-process plugins | Unnecessary for UI host spike |
