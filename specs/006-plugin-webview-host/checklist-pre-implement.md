# Pre-implement checklist: 006-plugin-webview-host

**Date**: 2026-09-23 | **Status**: Complete (no waivers)

## Failure modes → detection

| Failure | Detection |
|---------|-----------|
| Flat unzip drops `ui/dist` | T011 nested path assert |
| Zip-slip write outside target | T011 reject assert |
| Missing WebView2 runtime | MessageBox / dialog text FR-007 |
| Unsupported apiVersion | RPC `-32001` + unit test |
| JSON-RPC unknown method | `-32601` unit test |
| Hot-reload storms | Debounce in form |
| IronPython regression | T052 Sample.py discover smoke |

## Call-site / bypass audit

| Path | Notes |
|------|-------|
| Preferences Configure | Invokes `command.Configure` → sample Python → `ShowWebConfigure` |
| Menu split-button Configure | Same |
| Direct `ShowWebConfigure` | Uses `CommandPath` + `plugin.json` |
| Legacy WinForms ConfigScript | Unaffected if no `ShowWebConfigure` call |
| `UnzipFile` | Only install/extract path — all package installs |

## Test design

- Unit: nested unzip, zip-slip, HostJsonRpc getInfo / unknown method / apiVersion
- Operator: open sample Configure (SC-003) when runtime present
- Non-goals: FlaUI full automation

## CI proof plan

- `dotnet test ComicRack.Tests` (or `vstest`) on Windows for SC-001/SC-002
- SC-003 remains operator/local until WebView2 is guaranteed in CI image

## Gate

**PASS** — proceed to implement.
