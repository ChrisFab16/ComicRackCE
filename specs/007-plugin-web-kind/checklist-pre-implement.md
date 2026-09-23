# Pre-implement checklist: 007-plugin-web-kind

**Date**: 2026-09-23 | **Status**: Complete

## Failure modes → detection

| Failure | Detection |
|---------|-----------|
| kind=web without configure registered | Unit: no commands / skip |
| Crash on bad plugin.json | TryLoad fail; initializer returns empty |
| Duplicate Configure from JSON + Python | FR-004: only attach if Configure null |
| IronPython regression | SC-003 / existing Sample still parses |

## Call-site audit

| Path | Notes |
|------|-------|
| PluginEngine.Initialize file walk | Adds JsonPluginInitializer |
| Preferences Configure | Uses command.Configure → WebPluginCommand |
| 006 ShowWebConfigure | Reused |

## Test design

- Unit: synthesize temp package dir with plugin.json kind=web; assert commands + Configure invoke path mocked or type check
- Operator: install WebKindSample to AppData; Configure opens

## Gate

**PASS**
