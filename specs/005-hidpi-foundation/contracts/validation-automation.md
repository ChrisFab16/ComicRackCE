# Contract: HiDPI Validation Automation

**Feature**: `005-hidpi-foundation`  
**Implements**: FR-011, FR-012, FR-013, SC-005  
**Cross-repo**: Codesync `AGENTS.md` §33; ComicRackCE `AGENTS.md` Validation automation

## Gate ownership

| Quickstart scenario | SC | Owner | Mechanism |
|---------------------|-----|-------|-----------|
| 1 Embedded PerMonitorV2 | SC-001 | **CI required** | `scripts/verify-embedded-manifest.ps1` after build |
| 3 Workspace config not mutated | SC-003 | **CI required** | `ComicRack.Tests` contract tests (see scope below) |
| — CI automation rollup | SC-005 | **CI required** | SC-001 script + SC-003 tests both pass |
| 2 Cross-monitor DPI | SC-002 | Operator | Dual-monitor drag |
| 4 Pilot dialog | SC-004 | Operator | Visual @ 125%/150% (optional) |
| 5 Regression smoke | SC-006 | Operator | Subjective legibility @ 150% |

**Rule:** SC-005 (CI) MUST pass before operator sign-off counts for foundation complete. Operator UI at 150% does NOT substitute for SC-001 or SC-003.

## FR-011 — Post-build manifest verification

1. Script path: `scripts/verify-embedded-manifest.ps1`.
2. Inputs: `ComicRack/app.manifest`, `ComicRack/myressources.res`, built `ComicRack.exe` (Debug or Release).
3. Assertions:
   - Source `app.manifest` contains `PerMonitorV2`.
   - `myressources.res` contains `PerMonitorV2` and MUST NOT contain stale `system`-only `dpiAwareness`.
   - Linked EXE embeds `PerMonitorV2` (via `mt -inputresource` when available, else binary string search).
4. Exit code non-zero on any failure.
5. Invoked from MSBuild `AfterBuild` on `ComicRack.csproj` and from GitHub Actions after `msbuild`.

## FR-012 — CI integration

1. `pr-artifact-upload.yml` and `nightly.yml` run manifest verification and `ComicRack.Tests` after successful build on **fork** CI (`ChrisFab16/ComicRackCE`); upstream inherits on merge.
2. Failed verification or tests fails the workflow job.
3. Manifest script may run twice (MSBuild AfterBuild + workflow step)—intentional parity; either failure fails the job.

## FR-013 — Unit tests (SC-003 contract scope)

1. New project: `ComicRack.Tests` (xUnit, `net48`).
2. **In scope (CI):**
   - `FormUtility.RefreshDpiScale` raises `DpiScaleChanged` with identifiable source.
   - `ItemViewConfig` XML round-trip preserves `ThumbnailSize` (persistence semantics).
   - `ComicBrowserControl` source MUST NOT define `NormalizeViewConfigSizes` (regression guard).
3. **Out of scope (005 CI):** Full app workspace load at 150% with XML file diff—optional operator quickstart Scenario 3.
4. Tests run via `msbuild` + `dotnet test --no-build` on CI (Windows); `LangVersion=13.0` required for dependency projects.

## validation-results.md schema

Record separate sections: **CI (SC-001, SC-003, SC-005)** and **Operator (SC-002, SC-004, SC-006)**.

## Out of scope (005)

- FlaUI / WinAppDriver full UI automation.
- Screenshot visual regression.
- Dual-monitor simulation in CI.
