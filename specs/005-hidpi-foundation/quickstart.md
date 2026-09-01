# Quickstart: HiDPI Foundation Validation

**Feature**: `005-hidpi-foundation`  
**Branch**: `005-hidpi-foundation`  
**Prerequisite**: Visual Studio 2022 or Build Tools with `rc.exe` (full build required for SC-001; **CI is authoritative** if local tooling missing)

## Gate ownership

| Scenario | SC | CI (automated) | Operator (manual) |
|----------|-----|----------------|-------------------|
| 1 Embedded manifest | SC-001 | **Required** | Optional spot-check |
| 3 Workspace not mutated | SC-003 | **Required** (`ComicRack.Tests`) | Optional Scenario 3 confirm |
| CI rollup | SC-005 | **Required** (SC-001 + SC-003) | — |
| 2 Cross-monitor DPI | SC-002 | — | **Required** if dual-monitor available |
| 4 Pilot dialog | SC-004 | — | Optional |
| 5 Regression smoke | SC-006 | — | **Required** @ 150% |

Record results in `validation-results.md` with separate **CI** and **Operator** sections.

## Build

```bash
msbuild ComicRack/ComicRack.csproj /t:Rebuild /p:Configuration=Debug /p:LangVersion=13.0
```

Post-build runs `scripts/verify-embedded-manifest.ps1` automatically (FR-011). If PreBuild fails (no `rc.exe`), fix VS Build Tools—do not treat a partial build as SC-001 pass.

Manual manifest check:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-embedded-manifest.ps1 `
  -ExePath ComicRack/bin/Debug/net48/ComicRack.exe
```

## Run automated tests

```bash
msbuild ComicRack.Tests/ComicRack.Tests.csproj /t:Build /p:Configuration=Debug /p:LangVersion=13.0
dotnet test ComicRack.Tests/ComicRack.Tests.csproj -c Debug --no-build
```

## Scenario 1 — Embedded manifest (SC-001) — CI

**Pass**: `verify-embedded-manifest.ps1` exit 0 after Rebuild.

## Scenario 2 — Cross-monitor DPI (SC-002) — Operator

**Requires**: Two monitors at different scale (e.g. 100% + 150%).

1. Launch ComicRack from Debug build.
2. Drag MainForm to secondary monitor; repeat with ReaderForm if open.
3. Observe: layout updates or logged scale factor matches secondary monitor.

**Pass**: Operator confirms or documents single-monitor N/A.

## Scenario 3 — Workspace config not mutated (SC-003) — CI + optional operator

**CI pass**: `ComicRack.Tests` (contract scope per `validation-automation.md`).

**Optional operator end-to-end confirm**:

1. At 100%, set library thumb size; save workspace; note XML thumb height.
2. Set display 150%; restart; load workspace.
3. XML thumb height unchanged; UI thumbs readable.

## Scenario 4 — Pilot dialog (SC-004) — Operator optional

Open ProgressDialog at 125% and 150%; confirm proportionate layout.

## Scenario 5 — Regression smoke (SC-006) — Operator

At 150% primary monitor: Folders tree legible; Library thumbs visible (sanity only; Phase E re-validates 001–003).

## Foundation gate

**Do not** open upstream layout PR until:

- **SC-005 CI**: SC-001 script + SC-003 tests pass (record in validation-results.md)
- **Operator**: SC-002 (if dual-monitor) + **SC-006** smoke pass

Optional: subjective DPI refresh latency spot-check during Scenario 2 (&lt;100 ms feel; plan performance goal).

Copy results to `validation-results.md`.
