# Research: HiDPI Foundation Redesign

**Feature**: `005-hidpi-foundation`  
**Date**: 2026-09-01

## PR #278 review findings (maforget)

Source: [PR #278 comment](https://github.com/maforget/ComicRackCE/pull/278#issuecomment-5490677717)

| Finding | Evidence in fork | Decision |
|---------|------------------|----------|
| Manifest not embedded | `app.manifest` → PerMonitorV2; `myressources.res` still contains `system` | **D1**: PreBuild recompile `.rc` → `.res` |
| `Form.DpiChanged` unreliable | Wired on MainForm only; may not fire without PMv2 | **D2**: `FormEx.WndProc` + `WM_DPICHANGED` (0x02E0) |
| Global DPI wrong | `FormUtility.DpiScale` uses `GetDC(IntPtr.Zero)` | **D3**: Per-window DPI via `GetDpiForWindow` or control graphics |
| Config mutation | `NormalizeViewConfigSizes` rewrites `ItemViewConfig` | **D4**: Remove mutation; runtime scale via `GetItemSize`/`SetItemSize` |
| Scope too narrow | MainForm only | **D5**: All `FormEx` forms get DPI hook; custom controls subscribe |
| Duplicate API | `GetItemSize` already scales | **D4** |
| Noise | DarkMode on Folders tree | **Exclude** from foundation PR |
| Long-term | Issue #118 AutoScaleMode.Dpi | **D6**: Pilot one dialog in P3 |
| Validation | PR #278 manual-only sign-off | **D7**: Tier 1 manifest script + CI; Tier 2 `ComicRack.Tests`; operator for SC-002/005 only |

## Manifest pipeline audit

```
ComicRack/app.manifest
    ↓ referenced by
ComicRack/myressources.rc  (line: 1 24 "app.manifest")
    ↓ compiled to
ComicRack/myressources.res  ← Win32Resource in csproj
    ↓ linked into
ComicRack.exe
```

**Failure mode (001–003 / PR #278)**: Edited `app.manifest` without recompiling `.res`. Binary check: `strings myressources.res | grep PerMonitorV2` → absent.

**Upstream fix** (`upstream/dev`): PreBuild runs `compile_res_file.ps1` via vswhere + `rc.exe`.

## FormEx inventory

~35 forms inherit `FormEx` (MainForm, ReaderForm, ComicBookDialog, PreferencesDialog, etc.). Central `WndProc` hook covers all without per-form duplication.

Main shell views use `AutoScaleMode.None` (MainForm, ComicBrowserControl, ComicExplorerView)—remain on explicit metrics after foundation.

## GetItemSize / SetItemSize (existing)

`ComicBrowserControl.GetItemSize()` returns min/max/current with `ScaleDpiY` applied to bounds. `SetItemSize()` clamps and updates `itemView` only—does not touch workspace XML directly.

`NormalizeViewConfigSizes` (PR #278) creates new `ItemViewConfig` with scaled dimensions when loading—**violates** persist-user-config principle.

## WM_DPICHANGED (.NET 4.8)

- Message: 0x02E0; `lParam` contains suggested rect.
- `Form.DpiChanged` exists on .NET 4.7+ but requires process PMv2 + correct manifest.
- Fallback: override `WndProc` in `FormEx`, call base, then refresh scale.

## AutoScaleMode pilot candidates

| Dialog | AutoScaleMode today | Risk |
|--------|---------------------|------|
| ProgressDialog | Font (default) | Low—simple layout |
| ZoomDialog | Font | Low |
| ComicBookDialog | Font | High—large, many controls |
| PreferencesDialog | Font | High |

**Pilot**: `ProgressDialog` or `ZoomDialog` first.

## Relationship to features 001–003

| Feature | Status | After 005 |
|---------|--------|-----------|
| 001 Folders | Validated pre-foundation | Re-validate with real PMv2 |
| 002 Explorer | Validated pre-foundation | Re-validate |
| 003 Library | Validated pre-foundation | Re-validate; fix config mutation |
| 004 Plugins | Closed (plugin fork) | Unchanged |

## Open questions (resolved)

| Question | Resolution |
|----------|------------|
| Cherry-pick upstream `compile_res_file.ps1`? | Yes, adapt PreBuild in `ComicRack.csproj` |
| Keep `DisplaySettingsChanged`? | Yes, coarse fallback |
| Remove `RefreshDpiScale` PointF return? | Optional cleanup in implement |
