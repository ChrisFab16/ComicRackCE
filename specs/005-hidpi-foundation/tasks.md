# Tasks: HiDPI Foundation Redesign

**Input**: Design documents from `/specs/005-hidpi-foundation/`  
**Branch**: `005-hidpi-foundation`  
**Prerequisites**: spec.md, plan.md, research.md, contracts/, quickstart.md

**Tests**: CI-automated SC-001/SC-003/SC-005 per `contracts/validation-automation.md`; operator manual for SC-002/SC-004/SC-006.

**Organization**: Phase 2 foundation blocks all user stories. P1 stories (manifest + FormEx + automation) before P2 (view config) before P3 (pilot dialog).

**Analyze**: Re-run after validation-automation scope added (2026-09-01).

**Pre-implement checklist**: `checklist-pre-implement.md` (backfilled 2026-09-01 — use `specs/_templates/checklist-pre-implement.md` for 006+ **before** implement).

## Format: `[ID] [P?] [Story] Description`

---

## Phase 0: Pre-implement audit (blocking — no product code)

**Goal**: Complete `checklist-pre-implement.md` before `/speckit-implement`.

**Gate**: Analyze PASS + checklist gate signed (see AGENTS.md Spec Kit section).

- [x] T050 Copy/fill `checklist-pre-implement.md` from template (005 backfilled retrospectively)
- [x] T048 [P2] Add unit test: logical thumb size round-trip at non-1.0 scale (checklist §4 gap)
- [x] T049 [P2] ComicBookDialog `pagesView.ViewConfig` — apply display scale via `PagesView.SetViewConfigWithDisplayScale`
- [x] T051 [P2] Manifest script: forbidden `system` on EXE **binary** fallback path (checklist §1 FM-2)

**Checkpoint**: Phase 0 open items may proceed in parallel with implement only if operator-waived on checklist.

---

## Phase 1: Setup

- [ ] T001 Review contracts: `manifest-build.md`, `form-dpi-lifecycle.md`, `view-config-scaling.md`, `validation-automation.md`
- [ ] T002 Review `research.md` PR #278 findings and `pr278-response-draft.md` (operator posts when ready)
- [ ] T003 Confirm branch `005-hidpi-foundation` from `upstream/dev`; solution builds: `msbuild ComicRack/ComicRack.csproj /t:Rebuild /p:Configuration=Debug /p:LangVersion=13.0`
- [x] T004 Run `/speckit-analyze`; resolve CRITICAL/HIGH before `/speckit-implement`
- [x] T004b Re-run analyze after validation-automation artifacts; remediations applied 2026-09-01 (**PASS**)

---

## Phase 2: Foundational — Manifest pipeline (User Story 1, P1)

**Goal**: FR-001–FR-003, SC-001 — PerMonitorV2 embedded in EXE every build.

**Independent Test**: quickstart Scenario 1 (CI script).

- [ ] T005 [P] Verify `compile_res_file.ps1` at repo root (already on `upstream/dev`)
- [ ] T006 Verify PreBuild in `ComicRack/ComicRack.csproj` invokes `compile_res_file.ps1` (already on `upstream/dev`)
- [x] T007 Confirm `ComicRack/app.manifest` has `PerMonitorV2` in `dpiAwareness`
- [x] T008 Rebuild Debug; SC-001 pass via `scripts/verify-embedded-manifest.ps1` (replaces manual-only strings/mt)
- [ ] T009 Document VS Build Tools / rc.exe requirement in quickstart if build fails without them

**Checkpoint**: SC-001 CI pass — **gate for Phase 3**.

---

## Phase 2b: Validation automation (User Story 5, P1)

**Goal**: FR-011–FR-013, SC-005.

**Independent Test**: script exit 0 + `dotnet test` green on CI.

- [x] T031 [P] Create `scripts/verify-embedded-manifest.ps1` per `validation-automation.md`
- [x] T032 Wire MSBuild `AfterBuild` on `ComicRack.csproj` to invoke manifest script with `$(TargetPath)`
- [x] T033 Add manifest verify + `dotnet test` steps to `pr-artifact-upload.yml` and `nightly.yml`
- [x] T034 Add `ComicRack.Tests` (xUnit, net48) to solution; reference `cYo.Common.Windows`
- [x] T035 [P] Tests: `FormUtility.RefreshDpiScale` raises `DpiScaleChanged` with source
- [x] T036 [P] Tests: `ItemViewConfig` XML round-trip preserves thumb size (SC-003 contract)
- [x] T037 [P] Tests: regression guard — `ComicBrowserControl.cs` must not contain `NormalizeViewConfigSizes`
- [ ] T038 Update `validation-results.md` template usage in quickstart; record first CI run

**Checkpoint**: SC-001 + SC-003 CI pass locally before operator manual scenarios.

---

## Phase 3: User Story 2 — Per-window DPI (P1)

**Goal**: FR-004–FR-006, SC-002.

**Independent Test**: quickstart Scenario 2 (dual monitor) or logged scale change on DPI message.

- [x] T010 Add `WM_DPICHANGED` handling to `cYo.Common.Windows/Forms/FormEx.cs` (WndProc or protected virtual hook)
- [x] T011 Add `FormUtility.GetDpiScale(Control owner)` using window/control DC (`GetDpiForWindow` / `CreateGraphics`)
- [x] T012 Refactor `RefreshDpiScale` to accept `Control source`; extend `DpiScaleChanged` with sender/source in EventArgs
- [x] T013 Update `ComicRack/MainForm.cs`: subscribe to new event; call existing metrics refresh (minimal—prove hook only)
- [x] T014 Update `ComicRack/ReaderForm.cs`: same subscription pattern as MainForm
- [x] T015 Remove or reduce reliance on `GetDC(IntPtr.Zero)` as sole DPI source in `FormUtility.DpiScale` getter
- [x] T016 Keep `DisplaySettingsChanged` fallback; document interaction with WM_DPICHANGED in code comment

**Checkpoint**: Scenario 2 operator pass or documented single-monitor limitation.

---

## Phase 4: User Story 3 — Non-mutating view config (P2)

**Goal**: FR-007, SC-003.

**Independent Test**: T036–T037 CI tests + optional operator Scenario 3.

- [x] T017 Remove or replace `NormalizeViewConfigSizes` mutation in `ComicRack/Views/ComicBrowserControl.cs` per `view-config-scaling.md` (N/A on dev — guard test passes)
- [x] T018 Ensure workspace load applies scaled **display** via `GetItemSize`/`SetItemSize` or view init without writing XML (T041)
- [x] T019 Audit `ViewConfig` setter path — no new config clone with scaled integers on load (pass-through on dev)
- [ ] T020 SC-003 CI pass via T036–T037; optional operator XML check per quickstart

**Checkpoint**: SC-003 CI pass.

---

## Phase 5: User Story 4 — AutoScaleMode.Dpi pilot (P3)

**Goal**: FR-010, explore issue #118 direction.

**Independent Test**: quickstart Scenario 4 (operator).

- [x] T021 [P] Select pilot dialog (`ProgressDialog` or `ZoomDialog`) per research.md
- [x] T022 Set `AutoScaleMode = Dpi` on pilot; remove redundant manual `ScaleDpi` on that form only if safe
- [ ] T023 Validate pilot at 100%, 125%, 150% in validation-results.md (operator)

---

## Phase 6: Polish & upstream prep

- [ ] T024 Remove any DarkMode/HiDPI unrelated diffs if present on branch (FR-009)
- [ ] T025 Run **SC-005** CI gates (T008, T036–T037) + operator SC-002/SC-004/SC-006; optional DPI refresh latency spot-check during SC-002; record `validation-results.md`
- [ ] T026 Operator review and post `pr278-response-draft.md` on PR #278 (**operator approval required**)
- [ ] T027 Push `005-hidpi-upstream` to `origin`; open upstream PR to **`maforget/dev`** when operator approves (`005-hidpi-foundation` keeps Spec Kit artifacts on fork only; link docs in PR body)

---

## Phase 7: Code-review remediations (2026-09-01 converge)

**Source**: `/universal-code-review` on foundation + automation diff.

- [x] T039 [P1] Call `FormUtility.RefreshDpiScale(this)` at MainForm startup before first `ScaleDpi()` use
- [x] T040 [P1] MainForm DPI handlers refresh browser display via `RefreshDisplayItemSizeForDpi()` + `OnUpdateGui()`
- [x] T041 [P1] T018: logical view-config persist + runtime display scale in `ComicBrowserControl` / `ComicPagesView` (`GetLogicalViewConfig`, `ApplyDisplayItemSize`, unscale on save)
- [x] T042 [P2] Manifest script rejects stale `system` dpiAwareness token (manifest, `.res`, embedded EXE)
- [x] T043 [P2] `ReaderForm` unsubscribes `DpiScaleChanged` on close
- [x] T044 [P2] `ProgressDialog`: set `AutoScaleMode.Dpi` + `AutoScaleDimensions` before `InitializeComponent`
- [x] T045 [P2] Document process-wide `dpiScale` cache limitation in `FormUtility` (SC-002 operator gate; per-window cache deferred)
- [x] T046 [P2] Unit test: `ScaleDpiY` / `UnscaleDpiY` round-trip at cached scale
- [x] T047 Record local SC-001/SC-005 pass in `validation-results.md` after T008
- [ ] T038 Update `validation-results.md` with first CI run after push
- [x] T052 [P1] Stack close: `SetStackViewConfig` uses `GetLogicalViewConfig()` not raw `itemView.ViewConfig`
- [x] T053 [P1] `PagesView.SetViewConfigWithDisplayScale`: do not restore row height after display apply (Detail mode)

**Checkpoint**: T008 + T047 before operator SC-002/SC-006 sign-off.

---

## Phase E (follow-up — out of implement scope for 005)

- [ ] T028 Re-run features 001–003 quickstarts on foundation branch
- [ ] T029 Open focused upstream PR to `dev`: foundation only (operator approval required)
- [ ] T030 Separate PR or commits for layout re-validation if needed after foundation merge

---

## Dependencies

```text
Phase 1 → Phase 2 (manifest) → Phase 2b (automation) → Phase 3 (FormEx)
                                      ↓
                               Phase 4 (view config / SC-003 tests)
                                      ↓
                               Phase 5 (pilot, optional)
Phase 6 after P1+P2+2b minimum (T025 gate)
Phase E after 005 merged to upstream dev
```

## Parallel opportunities

- T031 + T034 can start after T001 (different paths)
- T010 + T031 parallel after Phase 1
- T035–T037 parallel after T034
- T021–T023 parallel to T017–T019 after Phase 3 checkpoint
