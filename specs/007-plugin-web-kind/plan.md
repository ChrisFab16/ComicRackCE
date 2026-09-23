# Implementation Plan: 007-plugin-web-kind

**Branch**: `007-plugin-web-kind` | **Date**: 2026-09-23  
**Spec**: [spec.md](./spec.md)

## Summary

Add `JsonPluginInitializer` + `WebPluginCommand` so `kind: web` packages register without IronPython. Ship `WebKindSample`, migration guide, and deprecation policy. Keep IronPython path intact.

## Technical Context

| Item | Choice |
|------|--------|
| Discovery | New initializer on `plugin.json` (alongside XML/Python) |
| Configure | Reuse `WebViewPluginHost.ShowConfigure` from 006 |
| Precedence | Attach web ConfigScript only if Key has no Configure yet |
| Docs | `migration-guide.md`, `deprecation-policy.md` |

## Structure

```
ComicRack.Plugins/
  JsonPluginInitializer.cs
  WebView/WebPluginCommand.cs
ComicRack/Output/Scripts/WebKindSample/
specs/007-plugin-web-kind/
ComicRack.Tests/Plugins/JsonPluginInitializerTests.cs
```

## Constitution

- Fork-only Spec Kit docs; product code PR to fork `development` only (no upstream).
- No IronPython removal (SC-005).
