# Tasks: 006-plugin-webview-host

## Phase 0 — Artifacts

- [x] T001 Draft Host API + plugin.json schemas under `contracts/`
- [x] T002 Write `spec.md`, `plan.md`, `research.md`, packaging contract
- [x] T003 Write `tasks.md` and run analyze (see `analyze-report.md`)
- [x] T004 Pre-implement checklist (`checklist-pre-implement.md`)

## Phase 1 — Packaging (US2 / FR-003 / SC-001)

- [x] T010 Fix `PackageManager.Package.UnzipFile` nested paths + zip-slip guard
- [x] T011 Add `ComicRack.Tests` project + `PackageUnzipNestedTests`
- [x] T012 Wire test project into `ComicRack.sln`

## Phase 2 — Host API + WebView Configure (US1 / FR-001–002, FR-007–008)

- [x] T020 Add WebView2 package reference to `ComicRack.Plugins`
- [x] T021 Implement `PluginManifest` load/validate
- [x] T022 Implement `HostJsonRpc` dispatcher (v1 methods)
- [x] T023 Unit tests for dispatcher (`HostJsonRpcTests`)
- [x] T024 Implement `WebViewPluginForm` (virtual host, hot-reload watcher)
- [x] T025 Add `IPluginEnvironment.ShowWebConfigure` + implementation
- [x] T026 Wire sample ConfigScript to `ShowWebConfigure`

## Phase 3 — Sample SPA pilot (US4 / FR-006)

- [x] T030 Create `WebConfigureSample` under `ComicRack/Output/Scripts/`
- [x] T031 Add Vite source under `samples/webview-configure-sample/`
- [x] T032 Document rebuild of `ui/dist` in sample README / quickstart

## Phase 4 — Hot-reload + docs (US3 / FR-004)

- [x] T040 Enable FileSystemWatcher when `hotReload` true
- [x] T041 Write `quickstart.md` + `validation-results.md` template
- [x] T042 Update repo `AGENTS.md` with plugin SPA host note

## Phase 5 — Validation

- [x] T050 Run unit tests (SC-001, SC-002)
- [x] T051 Smoke: build ComicsRack.Plugins + open sample Configure when WebView2 present (SC-003)
- [x] T052 Confirm IronPython Sample.py still parses (SC-004)
- [x] T053 Dedicated WebView2 UserDataFolders for PluginConfigure + HtmlPanels (hang when both share process default)
