# Pre-Implement Checklist: HiDPI Foundation Redesign

**Feature**: `005-hidpi-foundation`  
**Branch**: `005-hidpi-foundation` (based on `upstream/dev`)  
**Date**: 2026-09-01 (authored retrospectively after code review; use as reference for 006+)

**Gate**: Required after `/speckit-analyze` PASS, before `/speckit-implement`.

> **Note:** This checklist was backfilled after implement + `/universal-code-review`. Items marked **Retro** were missed pre-implement and fixed in T039–T047. Future features should complete this **before** coding.

---

## 1. Failure modes (from research / PR #278)

| ID | Failure mode | How we detect (automated) | How we detect (operator) | Task | Done |
|----|--------------|---------------------------|----------------------------|------|------|
| FM-1 | `app.manifest` changed but EXE still embeds `system` | `verify-embedded-manifest.ps1` on **EXE** + `.res` | Resource Hacker / mt | T008, T031 | [x] Retro |
| FM-2 | Stale `system` in `.res` while source has PerMonitorV2 | Script **forbidden** token on manifest + `.res` (+ EXE mt path) | — | T042 | [x] Retro (EXE binary fallback still open — T048) |
| FM-3 | Manual “150% looks OK” without PMv2 proof | SC-001 CI | — | T031–T033 | [x] |
| FM-4 | `NormalizeViewConfigSizes` mutates workspace XML | grep guard test | optional XML diff @ 150% | T037 | [x] |
| FM-5 | Build with ComicRack.exe running (file lock) | — | close app before Rebuild | T008 | [x] Retro |
| FM-6 | `dotnet test` without `LangVersion=13.0` msbuild | CI workflow command | local quickstart | T033 | [x] |

---

## 2. Call-site / data-path audit

| Pattern | Command / locations | Must go through | Bypass risk | Done |
|---------|---------------------|-----------------|-------------|------|
| `ViewConfig` assign | `rg 'ViewConfig\s*=' ComicRack/` | scaling wrapper | **ComicBookDialog** → fixed T049 | [x] |
| Stack persist | `CloseStack` / `SetStackViewConfig` | `GetLogicalViewConfig()` | was raw `itemView.ViewConfig` | [x] T052 |
| `ItemViewConfig` persist | `UpdateViewConfig`, workspace save | `GetLogicalViewConfig()` (unscale) | direct `itemView.ViewConfig` in save path | [x] Retro T041 |
| DPI refresh subscribers | `rg 'DpiScaleChanged'` | filter by `e.Source`; unsubscribe on close | MainForm never unsubscribes (app lifetime OK) | [x] Retro T043 |
| `ScaleDpi` in ctor | MainForm, ComicBrowserControl init | after `RefreshDpiScale(owner)` | child init **before** MainForm `RefreshDpiScale` | [ ] **Hypothesis — operator SC-006** |

**Sign-off:** Primary library path mapped; ComicBookDialog pages tab deferred to 006 or T048.

---

## 3. Lifecycle & init order

| Question | Answer / decision | Done |
|----------|-------------------|------|
| First `ScaleDpi()` vs DPI cache | MainForm: `InitializeComponent` → `RefreshDpiScale(this)` → `ScaleDpi`. Children created in step 1 may use `GetDC(0)`. | [x] documented |
| MainForm DPI handler targets | `SetWorkspaceDisplayOptions` (reader) + `RefreshBrowserDisplayItemSizes` (library thumbs) | [x] Retro T040 |
| Global `dpiScale` cache | Process-wide; per-window correctness needs `RefreshDpiScale(source)` per form; SC-002 operator | [x] T045 + contract §6 |
| `DisplaySettingsChanged` duplicate refresh | Handler + `DpiScaleChanged` both refresh browser — acceptable P3 | [x] noted |

---

## 4. Contract invariants → tests (designed before code)

| Contract | Invariant | CI test | Operator | Task | Done |
|----------|-----------|---------|----------|------|------|
| manifest-build | PerMonitorV2 in manifest, `.res`, EXE | post-build script | — | T031–T032 | [x] |
| form-dpi-lifecycle | `WM_DPICHANGED` → refresh; source identifiable | `RefreshDpiScale` raises event | SC-002 drag | T035, T010–T016 | [x] |
| view-config-scaling | logical persist, display scale at runtime | round-trip + grep guard | SC-006 @ 150% | T036–T037 | [x] partial |
| view-config-scaling | logical 128 → display 192 @ 1.5x | `ItemViewConfigScalingTests` | — | T048 | [x] |
| validation-automation | SC-005 = SC-001 + SC-003 | workflow + local | — | T033, T038 | [x] local / [ ] CI push |

---

## 5. Build & CI proof plan

| Check | Debug | Release | CI | Done |
|-------|-------|---------|-----|------|
| Rebuild + manifest script | [x] | [ ] | pr-artifact-upload, nightly | [x] / [ ] |
| Forbidden `system` token | [x] manifest + res | [x] | same | [x] partial (EXE binary fallback) |
| Tests after msbuild LangVersion=13.0 | [x] | [ ] | yml step | [x] local |

---

## 6. Scope & upstream boundaries

| Item | In scope | Out of scope | Done |
|------|----------|--------------|------|
| Foundation only | manifest, FormEx, FormUtility, MainForm/ReaderForm hooks, ProgressDialog pilot, CI tests | 001–003 layout re-apply, DarkMode, full AutoScale migration | [x] |
| Upstream PR target | `maforget/dev` | `master`, stacked PR #278 | [x] |

---

## 7. Design review (contracts + tasks only)

| Finding | Severity | Resolution |
|---------|----------|------------|
| Analyze PASS ≠ implementation complete | process | this checklist + AGENTS workflow |
| SC-003 tests don’t prove scaled display | P2 | T048 proposed |
| ComicBookDialog bypass | P2 | T049 or 006 scope |
| Uncommitted automation until push | P2 | commit before T038 |

---

## Gate (retrospective)

| Gate | Status | Date |
|------|--------|------|
| `/speckit-analyze` PASS | yes | 2026-09-01 |
| Pre-implement checklist complete | **backfilled** | 2026-09-01 |
| Proceed to implement | happened before checklist — use template for 006+ | |

**For 006+:** Copy `specs/_templates/checklist-pre-implement.md`; do not start implement until Gate row “Pre-implement checklist complete” is yes.
