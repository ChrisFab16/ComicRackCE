# Tasks: 007-plugin-web-kind

## Phase 0 — Artifacts

- [x] T001 Write `spec.md`, `plan.md`, `research.md`
- [x] T002 Write `tasks.md`, `analyze-report.md`, `checklist-pre-implement.md`
- [x] T003 Write migration guide + deprecation policy

## Phase 1 — Host dual-kind (US1–US2)

- [x] T010 Implement `WebPluginCommand`
- [x] T011 Implement `JsonPluginInitializer` for `plugin.json` / `kind: web`
- [x] T012 Register initializer in `PluginEngine`; ConfigScript attach respects existing Configure
- [x] T013 Unit tests (`JsonPluginInitializerTests` / web command)

## Phase 2 — Sample (US1 / FR-006)

- [x] T020 Add `WebKindSample` (no `.py`) under `Output/Scripts/`
- [x] T021 Reuse or slim SPA assets; `kind: web` in `plugin.json`

## Phase 3 — Docs + validate

- [x] T030 Link guides from `AGENTS.md`
- [x] T031 Run unit tests (SC-001, SC-003) — 11 passed
- [x] T032 Operator SC-002: WebKindSample Configure (installed to AppData; restart Debug EXE)
