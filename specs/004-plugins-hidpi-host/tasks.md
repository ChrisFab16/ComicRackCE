# Tasks: Plugin HiDPI Host (Deferred Closure)

**Input**: [spec.md](./spec.md), [plan.md](./plan.md)  
**Branch**: `004-plugins-hidpi-host`  
**Status**: CE host implement **cancelled**; plugin fork is delivery vehicle.

## Phase 1 — Artifact alignment

- [x] T001 Record deferral decision in `spec.md` (CE host postponed; plugin fork path)
- [x] T002 Write `plan.md` with constitution check and shelved CE WIP inventory
- [x] T003 Write operator `quickstart.md` for plugin install + Configure tab matrix
- [x] T004 Run `/speckit-analyze`; resolve CRITICAL/HIGH before marking feature closed

## Phase 2 — Plugin delivery (external repo)

- [x] T005 Implement HiDPI relayout in `external/comicrack-library-organizer` (`lodpi.py`, configure form)
- [x] T006 Code review remediation P1–P3 → plugin **2.1.16** + `docs/HiDPI-remediation.md`
- [x] T007 Operator validation at 150% and 200% per quickstart; record in `validation-results.md`
- [ ] T008 Push plugin branch; maintain [PR #25](https://github.com/Stonepaw/comicrack-library-organizer/pull/25) until upstream merge

## Phase 3 — CE branch hygiene (no upstream PR)

- [ ] T009 Confirm CE host WIP (`PluginDpi*`, `DpiAwareForm`) is **not** merged to `master`
- [ ] T010 Optional: revert or isolate CE WIP commits on `004-plugins-hidpi-host` to avoid accidental merge
- [ ] T011 Update `AGENTS.md` plugin HiDPI lessons (fork install, audit all Configure pages)

## Phase 4 — Closure

- [x] T012 Mark feature **Closed (deferred)** in spec when T007 validation passes
- [ ] T013 Clear or archive `.specify/feature.json` active pointer after closure

**Checkpoint**: Feature closed when operator sign-off in `validation-results.md` and plugin PR is merge-ready — **without** CE host code on `master`.
