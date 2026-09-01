# Upstream PR draft — HiDPI foundation

**Do not open without operator review.**  
**Target:** `maforget/dev` ← `ChrisFab16/005-hidpi-upstream` (code + tests only; **no** `specs/` in upstream diff)  
**Fork tracking branch:** `005-hidpi-foundation` (Spec Kit artifacts stay on fork only)  
**Related:** Closes nothing; supersedes approach in #278. Tracks #118 long-term.

---

## Title

HiDPI foundation: PerMonitorV2 manifest, FormEx DPI lifecycle, non-mutating view config

---

## Body (paste into PR description)

### Summary

This PR is a **foundation-only** HiDPI change set, based on current `dev` and shaped by your [PR #278 review](https://github.com/maforget/ComicRackCE/pull/278#issuecomment-5490677717). It does **not** re-submit the layout/`Apply*Metrics()` work from #278.

Goal: make the process genuinely **PerMonitorV2-aware**, refresh DPI per window, and scale library display **without rewriting** persisted `ItemViewConfig`—before any follow-up layout PRs.

### Design & validation docs (fork only — linked, not in upstream diff)

Spec Kit artifacts live on my fork branch **`005-hidpi-foundation`** only. They are **not** included in the upstream PR file list; use these links for design context and validation notes:

| Doc | Link |
|-----|------|
| Feature spec | [spec.md](https://github.com/ChrisFab16/ComicRackCE/blob/005-hidpi-foundation/specs/005-hidpi-foundation/spec.md) |
| Implementation plan | [plan.md](https://github.com/ChrisFab16/ComicRackCE/blob/005-hidpi-foundation/specs/005-hidpi-foundation/plan.md) |
| Tasks & checkpoints | [tasks.md](https://github.com/ChrisFab16/ComicRackCE/blob/005-hidpi-foundation/specs/005-hidpi-foundation/tasks.md) |
| PR #278 research | [research.md](https://github.com/ChrisFab16/ComicRackCE/blob/005-hidpi-foundation/specs/005-hidpi-foundation/research.md) |
| Validation results (CI vs operator) | [validation-results.md](https://github.com/ChrisFab16/ComicRackCE/blob/005-hidpi-foundation/specs/005-hidpi-foundation/validation-results.md) |
| Analyze report (artifact gate) | [analyze-report.md](https://github.com/ChrisFab16/ComicRackCE/blob/005-hidpi-foundation/specs/005-hidpi-foundation/analyze-report.md) |
| Pre-implement checklist | [checklist-pre-implement.md](https://github.com/ChrisFab16/ComicRackCE/blob/005-hidpi-foundation/specs/005-hidpi-foundation/checklist-pre-implement.md) |
| Operator quickstart | [quickstart.md](https://github.com/ChrisFab16/ComicRackCE/blob/005-hidpi-foundation/specs/005-hidpi-foundation/quickstart.md) |
| Contracts (manifest, DPI, view config, CI) | [contracts/](https://github.com/ChrisFab16/ComicRackCE/tree/005-hidpi-foundation/specs/005-hidpi-foundation/contracts) |

Upstream PR branch: [`005-hidpi-upstream`](https://github.com/ChrisFab16/ComicRackCE/tree/005-hidpi-upstream) (product code, tests, CI scripts only).

### AI assistance disclosure

This PR was developed with **AI coding assistance** (Cursor). In response to the #278 review (stale `.res`, manual-only sign-off), I added the **validation gates below**—build-time manifest checks, unit tests, and structured review—so correctness is verified in CI where possible, not only by visual inspection at 150%.

### Quality: tests, validation, and checks

This PR adds infrastructure so the #278 class of mistakes is caught in **build/CI**, not only by manual inspection.

**Build-time gates (SC-001)**

- `scripts/verify-embedded-manifest.ps1` — asserts `PerMonitorV2` in `app.manifest`, `myressources.res`, and **linked `ComicRack.exe`**; rejects stale `system` dpiAwareness (including EXE binary fallback path)
- MSBuild `VerifyEmbeddedManifest` **AfterBuild** on `ComicRack.csproj` — every local/CI build fails if the manifest pipeline is wrong
- Wired into `pr-artifact-upload.yml` and `nightly.yml` after Release rebuild

**Unit tests (`ComicRack.Tests`, xUnit, net48 — SC-003)**

| Test | What it guards |
|------|----------------|
| `RefreshDpiScale_RaisesDpiScaleChangedWithSource` | DPI refresh event contract (`FormEx` lifecycle) |
| `ScaleDpiY_And_UnscaleDpiY_Are_Inverses_AtCachedScale` | Scale round-trip at process DPI |
| `LogicalThumb128_At150PercentScale_Displays192` | Logical vs display sizing at 1.5× |
| `ApplyLogicalDisplaySizes_ThumbnailMode_*` / `DetailMode_*` | Runtime apply path for thumb and row heights |
| `ToLogical_UnscalesDisplaySizedConfig` | Persist path returns logical sizes, not display pixels |
| `XmlRoundTrip_PreservesThumbnailSize` | `ItemViewConfig` XML semantics unchanged |
| `ComicBrowserControl_DoesNotDefineNormalizeViewConfigSizes` | Regression guard against #278 config mutation |

**Verified locally (before this PR)**

- [x] Debug + **Release** full rebuild
- [x] Manifest script pass on Debug and Release EXE
- [x] **8/8** unit tests pass (`LangVersion=13.0` msbuild + `dotnet test --no-build`)
- [x] Structured code review against spec/contracts (including stack persist and ComicBook pages paths)

**Still manual (not claimed as CI-complete)**

- [ ] Dual-monitor drag / cross-monitor DPI (SC-002)
- [ ] 150% regression smoke on Library/Folders (SC-006)
- [ ] Optional: `ProgressDialog` @ 125%/150% (SC-004 pilot)

See [validation-results.md](https://github.com/ChrisFab16/ComicRackCE/blob/005-hidpi-foundation/specs/005-hidpi-foundation/validation-results.md) for the full CI vs operator matrix.

### Alignment with PR #278 feedback

| Your feedback | This PR |
|---------------|---------|
| Manifest must be embedded in the EXE, not only `app.manifest` | `dpiAwareness` → `PerMonitorV2`; PreBuild still runs `compile_res_file.ps1`; **post-build script verifies** `app.manifest`, `myressources.res`, and **linked `ComicRack.exe`** (fails if stale `system` remains) |
| `GetDC(IntPtr.Zero)` / primary-monitor DPI is insufficient | `FormUtility.GetDpiScale(Control)` uses `GetDpiForWindow` / owner graphics; `RefreshDpiScale(source)` on DPI change |
| `Form.DpiChanged` / MainForm-only wiring unreliable | Central `WM_DPICHANGED` handling on **`FormEx`**; `MainForm` and `ReaderForm` subscribe and refresh |
| Do not mutate user config (`NormalizeViewConfigSizes`) | Runtime display via existing **`GetItemSize` / `SetItemSize`**; logical sizes preserved on save (`ItemViewConfigScaling`) |
| Scope too broad; unrelated noise | **No** DarkMode changes, **no** library/explorer layout sweep—foundation + one dialog pilot only |
| Use existing APIs | Reuses `FormEx`, `FormUtility`, `ComicBrowserControl` item-size APIs rather than new normalization layer |

I closed #278 on my side; happy to leave it as reference only.

### Path toward #118 (`AutoScaleMode.Dpi`)

[Issue #118](https://github.com/maforget/ComicRackCE/issues/118) proposes moving dialogs (and eventually the app) to **`AutoScaleMode.Dpi`** instead of manual `ScaleDpi` everywhere. This PR is **step 1** of that path, not the full migration:

1. **This PR (foundation)** — Process and hooks that #118 depends on:
   - Real **PerMonitorV2** in the binary (required for per-monitor `AutoScaleMode.Dpi` behavior)
   - **`FormEx` + `WM_DPICHANGED`** so all forms get a consistent DPI lifecycle
   - **Library / workspace**: keep `AutoScaleMode.None` on shell views and owner-draw (`ItemView`); scale display through existing item-size APIs so persisted XML stays logical

2. **Pilot (included, low risk)** — `ProgressDialog` switched to `AutoScaleMode.Dpi` as a **single-form experiment** toward #118; operator validation at 125%/150% documented separately.

3. **Follow-up (out of scope here)** — After foundation merges:
   - Re-validate layout-sensitive views on real PMv2 (separate small PRs)
   - Expand `AutoScaleMode.Dpi` to more simple dialogs (`ZoomDialog`, etc.)
   - Defer high-risk surfaces (`ComicBookDialog`, `PreferencesDialog`, custom `ItemView` rendering) until patterns are proven

```text
#278 manual Apply*Metrics (rejected)
        ↓
This PR: PMv2 + FormEx + config semantics + CI gates
        ↓
#118: gradual AutoScaleMode.Dpi on standard forms + targeted metrics on owner-draw
```

### What changed (high level)

- `ComicRack/app.manifest` — `PerMonitorV2`
- `cYo.Common.Windows` — `FormEx.WndProc`, `FormUtility` DPI refresh/events, `ItemViewConfigScaling`
- `ComicRack` — MainForm/ReaderForm DPI hooks; logical/display view config on browser and pages paths
- `ProgressDialog` — `AutoScaleMode.Dpi` pilot
- `scripts/verify-embedded-manifest.ps1` + MSBuild AfterBuild; CI test step
- `ComicRack.Tests` — manifest/config/scaling contract tests (xUnit, net48)

### Test plan

**Automated (CI / local)** — same gates as above; maintainer can re-run:

- [ ] `msbuild ComicRack/ComicRack.csproj /t:Rebuild /p:Configuration=Release /p:LangVersion=13.0`
- [ ] `scripts/verify-embedded-manifest.ps1 -ExePath ComicRack/bin/Release/net48/ComicRack.exe` — exit 0
- [ ] `msbuild ComicRack.Tests/ComicRack.Tests.csproj /t:Build /p:Configuration=Release /p:LangVersion=13.0` + `dotnet test --no-build` — 8/8 pass

**Manual (maintainer or me)**

- [ ] Two monitors, different scale: drag MainForm and ReaderForm; library thumbs refresh (SC-002)
- [ ] 150%: workspace saved at 100% still shows logical thumb size in XML; UI readable (SC-006)
- [ ] Optional: `ProgressDialog` at 125%/150% (SC-004 pilot)

### Out of scope (intentional)

- Layout/`Apply*Metrics()` changes from #278 or fork features 001–003
- Full-app `AutoScaleMode.Dpi` migration (#118 complete)
- Plugin host / IronPython UI
- DarkMode or unrelated UI tweaks

### Notes

- Branch is based on current `dev` (includes manifest PreBuild from upstream).
- Happy to split further if you prefer manifest-only first; this was kept as one focused foundation PR per your “smaller, clearer design” guidance.
- AI-assisted development with the validation layer above; happy to walk through any check or test if useful during review.

---

## Suggested labels / links

- Link: #118 (related, long-term)
- Link: #278 (supersedes approach; closed on my side)
- Mention: no database/schema changes
