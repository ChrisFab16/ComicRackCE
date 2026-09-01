# Implementation Plan: HiDPI Foundation Redesign

**Branch**: `005-hidpi-foundation` (based on `upstream/dev`) | **Date**: 2026-09-01 | **Spec**: [spec.md](./spec.md)

**Input**: maforget PR #278 review + issue #118 direction

## Summary

Establish **correct HiDPI infrastructure** before any new upstream layout PR: (1) embed PerMonitorV2 manifest via automated `.res` compile, (2) per-window DPI via `FormEx` + `WM_DPICHANGED`, (3) stop mutating persisted `ItemViewConfig`, (4) optional `AutoScaleMode.Dpi` pilot on one dialog, (5) **automated CI gates** for manifest embed and config semantics (Codesync §33). Features 001–003 `Apply*Metrics()` code is **frozen** until foundation quickstart passes—then re-validated/cherry-picked without DarkMode noise or config rewrite.

## Technical Context

**Language/Version**: C# / .NET Framework 4.8  
**Primary Dependencies**: WinForms, `cYo.Common.Windows` (`FormEx`, `FormUtility`), Win32 (`WM_DPICHANGED`, `GetDpiForWindow`)  
**Storage**: Workspace XML (`ItemViewConfig`) — read-only transform at display time  
**Testing**: Tier 1 post-build manifest script + CI; Tier 2 `ComicRack.Tests` (xUnit); operator quickstart for SC-002/004/005 manual only (Constitution III + §33)  
**Target Platform**: Windows 10/11 desktop  
**Project Type**: Desktop WinForms  
**Performance Goals**: DPI refresh imperceptible (<100 ms subjective); optional spot-check during operator Scenario 2 (T025)  
**Constraints**: Constitution I (focused PR to upstream); no plugin host scope; no config mutation (FR-007)  
**Scale/Scope**: ~5–8 source files + build script; not full-app AutoScale migration

## Constitution Check

| Principle | Pre-design | Post-design |
|-----------|------------|-------------|
| I. Upstream-first, focused | ✅ Foundation-only PR suitable for maforget | ✅ No 001–003 re-bundle |
| II. Preserve WinForms stack | ✅ FormEx/WndProc pattern | ✅ No WPF |
| III. Real Windows HiDPI validation | ✅ CI (SC-005) + operator (SC-002/SC-006) | ✅ SC-004 pilot optional; full scale matrix Phase E |
| IV. Spec before implement | ✅ This artifact set | ✅ Analyze before implement |
| V. Simplicity | ✅ Reuse FormEx, GetItemSize/SetItemSize | ✅ No parallel PluginDpiScaler |

**Gate status**: PASS — proceed to tasks after analyze.

## Project Structure

```text
specs/005-hidpi-foundation/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── manifest-build.md
│   ├── form-dpi-lifecycle.md
│   ├── view-config-scaling.md
│   └── validation-automation.md

scripts/
└── verify-embedded-manifest.ps1

ComicRack.Tests/                  # xUnit net48 — SC-003 + FormUtility
├── ComicRack.Tests.csproj
└── HiDpi/

.github/workflows/
├── nightly.yml                   # + manifest verify + tests (fork CI)
└── pr-artifact-upload.yml        # + manifest verify + tests (fork CI)

specs/005-hidpi-foundation/
├── tasks.md
├── analyze-report.md
└── pr278-response-draft.md

ComicRack/
├── app.manifest
├── myressources.rc / myressources.res
├── ComicRack.csproj          # PreBuild → compile_res_file.ps1
└── Views/ComicBrowserControl.cs  # remove NormalizeViewConfigSizes mutation

compile_res_file.ps1            # from upstream/dev

cYo.Common.Windows/Forms/
├── FormEx.cs                   # WndProc WM_DPICHANGED
└── FormUtility.cs              # per-control DPI, DpiScaleChanged sender
```

## Phases

### Phase A — Manifest build (P1)

1. Verify `compile_res_file.ps1` and PreBuild wiring (present on `upstream/dev`).
2. Set `app.manifest` `dpiAwareness` to `PerMonitorV2` (source still `system` on dev).
3. Document manifest verify step in quickstart.

### Phase B — FormEx DPI lifecycle (P1)

1. `FormEx.OnDpiChanged` / `WndProc` for `WM_DPICHANGED`.
2. `FormUtility.GetDpiScale(Control)` using window DC.
3. Refactor `RefreshDpiScale(Control)` or pass sender on `DpiScaleChanged`.
4. MainForm: subscribe; remove sole reliance on broken global path.

### Phase C — View config semantics (P2)

1. Remove or replace `NormalizeViewConfigSizes` mutation in `ComicBrowserControl`.
2. Apply scaled display via existing `GetItemSize`/`SetItemSize` / view load path.
3. Quickstart: workspace XML unchanged after load.

### Phase D — AutoScaleMode pilot (P3)

1. One dialog → `AutoScaleMode.Dpi`; document results in validation-results.md.

### Phase F — Validation automation (P1)

1. `scripts/verify-embedded-manifest.ps1` + MSBuild AfterBuild hook.
2. CI steps in `pr-artifact-upload.yml` and `nightly.yml`.
3. `ComicRack.Tests` for FormUtility events + SC-003 contract guards.
4. `validation-results.md` CI vs operator columns.

### Phase E — Re-validation gate (post-005)

1. Re-run 001–003 quickstarts on branch with foundation.
2. Prepare new upstream PR (foundation only first; layout follow-up separate if needed).

## PR #278 handling

- Do **not** merge PR #278.
- Reply using [pr278-response-draft.md](./pr278-response-draft.md).
- Cherry-pick layout commits only after SC-001 passes.

## Risks

| Risk | Mitigation |
|------|------------|
| `rc.exe` missing on dev machine | Document VS Build Tools requirement; CI compiles `.res` |
| WndProc breaks FormEx subclasses | Test MainForm, ReaderForm, one dialog |
| Removing NormalizeViewConfigSizes breaks 150% display | Runtime scale path must match prior UX |
| Fork CI vs upstream | Workflows run on `ChrisFab16/ComicRackCE`; upstream gets gates on merge |
| Manifest verify runs twice (MSBuild + workflow) | Intentional; either failure fails job |
