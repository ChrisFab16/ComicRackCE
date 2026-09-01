# Implementation Plan: Plugin HiDPI Host (Deferred)

**Branch**: `004-plugins-hidpi-host` | **Date**: 2026-09-01 | **Spec**: [spec.md](./spec.md)

**Status**: **Deferred** — CE host hooks shelved; delivery via Library Organizer plugin fork.

## Summary

Third-party IronPython plugins (Library Organizer) ship WinForms UI at fixed 96-DPI coordinates. Host-side scaling (`DpiAwareForm`, `PluginDpiScaler`, `PluginDpiHost`) was prototyped on this branch but does **not** reliably fix plugin-authored layouts (absolute `Location`/`Size`, TableLayoutPanel rows). **Plugin-local HiDPI relayout** in the fork is the chosen approach; CE host hooks are postponed indefinitely.

## Technical Context

**Language/Version**: C# / .NET Framework 4.8 (CE); IronPython 2.7 + WinForms (plugins)  
**Primary Dependencies**: `ComicRack.Plugins`, `cYo.Common.Windows`; plugin `lodpi.py`, `configureform.py`  
**Storage**: N/A (no CE persistence)  
**Testing**: Manual quickstart at 125%/150%/200% on Windows (Constitution III)  
**Target Platform**: Windows 10/11 desktop  
**Project Type**: CE spec tracks **operator validation**; implementation in external plugin repo  
**Performance Goals**: N/A for CE (no host hook in production path)  
**Constraints**: Constitution I (no large CE diff for upstream); upstream-first PRs from `master` only  
**Scale/Scope**: CE — spec artifacts + quickstart only; plugin — `external/comicrack-library-organizer` → [PR #25](https://github.com/Stonepaw/comicrack-library-organizer/pull/25)

## Constitution Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Upstream-first, focused | ✅ PASS | Defer CE host WIP; small plugin PR to Stonepaw |
| II. Preserve WinForms stack | ✅ PASS | Plugin stays IronPython WinForms |
| III. Real Windows HiDPI validation | ✅ PASS | quickstart.md scale matrix; operator sign-off |
| IV. Spec before implement | ✅ PASS | Deferred path documented before closure |
| V. Simplicity | ✅ PASS | Plugin `lodpi` relayout vs. recursive host scaler |

**Gate status**: PASS for **deferred closure** (no CE host implement).

## Decision Record

| Option | Outcome |
|--------|---------|
| CE `PluginDpiScaler` host hook | **Rejected** — does not fix absolute layouts; `ShowDialog` patch timing issues |
| Plugin fork `lodpi.py` + relayout passes | **Accepted** — operator pass at 2.1.14–2.1.16 |
| Merge CE WIP to `master` | **Blocked** — out of scope for upstream PR |

## Project Structure

```text
specs/004-plugins-hidpi-host/
├── spec.md
├── plan.md              # This file
├── tasks.md
├── quickstart.md        # Operator validation (plugin install)
├── analyze-report.md
└── validation-results.md  # Operator fills after quickstart

external/comicrack-library-organizer/   # gitignored; separate repo
├── lodpi.py
├── configureform.py
├── configformcontrols.py
└── docs/HiDPI-remediation.md
```

## CE Branch WIP (shelved)

Unmerged prototypes (do **not** cherry-pick to `master` without re-spec):

- `cYo.Common.Windows/Forms/DpiAwareForm.cs`
- `cYo.Common.Windows/Forms/PluginDpiScaler.cs`
- `cYo.Common.Windows/Forms/PluginDpiHost.cs`
- `ComicRack.Plugins/Command.cs`, `PythonCommand.cs`, `PluginEnvironment.cs` hooks
- `ComicRack/MainForm.cs`, `UIComicPageControl.cs` touches

## Validation Handoff

Operator validates **installed plugin** (not CE build) per [quickstart.md](./quickstart.md). Evidence recorded in `validation-results.md`. Plugin release tracked on PR #25 branch `hidpi-configure-form` (version **2.1.16**).
