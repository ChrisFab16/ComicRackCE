# Implementation Plan: Plugin WebView2 SPA Host

**Branch**: `006-plugin-webview-host` | **Date**: 2026-09-23  
**Spec**: [spec.md](./spec.md)

## Summary

Add WebView2-hosted Configure SPA support with a versioned JSON-RPC Host API, nested package unzip, hot-reload for UI assets, and an in-repo sample plugin. Keep IronPython for logic hooks.

## Technical Context

| Item | Choice |
|------|--------|
| Language | C# net48 WinForms |
| New dependency | `Microsoft.Web.WebView2` |
| JSON | `System.Text.Json` (already used in `cYo.Common`) |
| Tests | New `ComicRack.Tests` (xUnit net48) referencing Engine + Plugins |
| UI sample | Vanilla SPA in `ui/dist` + Vite source under `samples/webview-configure-sample` |

## Constitution Check

- Focused feature PR-sized; Spec Kit fork-only docs stay on this branch.
- No IronPython removal; dual-host Phase 1.
- Binary/runtime: document WebView2; unit tests cover unzip + RPC (not full UI automation).

## Project Structure

```
ComicRack.Plugins/WebView/
  PluginManifest.cs
  HostJsonRpc.cs
  WebViewPluginForm.cs
  WebViewPluginServices.cs
ComicRack.Engine/PackageManager.cs   # nested UnzipFile
ComicRack.Plugins/PluginEnvironment.cs / IPluginEnvironment.cs  # ShowWebConfigure
ComicRack/Output/Scripts/WebConfigureSample/
samples/webview-configure-sample/
ComicRack.Tests/Plugins/
specs/006-plugin-webview-host/
```

## Complexity Tracking

| Risk | Mitigation |
|------|------------|
| WebView2 runtime missing | FR-007 message + docs |
| file:// quirks | Prefer `SetVirtualHostNameToFolderMapping` |
| Zip slip | Explicit path guard + tests |

## Implementation phases (tasks.md)

1. Contracts already in `contracts/`
2. Nested unzip + tests
3. Manifest + HostJsonRpc + tests
4. WebViewPluginForm + ShowWebConfigure
5. Sample package + Vite source
6. Quickstart / validation notes
