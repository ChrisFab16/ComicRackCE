# Pre-Implement Checklist: [FEATURE NAME]

**Feature**: `NNN-short-name`  
**Branch**: `[branch]`  
**Date**: YYYY-MM-DD  
**Gate**: Required **after** `/speckit-analyze` PASS, **before** `/speckit-implement` or feature code.

## Purpose

`/speckit-analyze` checks **artifact consistency** (spec ↔ plan ↔ tasks ↔ constitution).  
This checklist checks **implementability**: call sites, failure modes, init order, test design, and CI proof **before** writing product code.

Do **not** mark implement started until every **Blocking** item is `[x]` or explicitly waived by operator with reason.

---

## 1. Failure modes (from research / post-mortems)

Copy each known failure mode from `research.md` or prior incidents. One row = one blocking task if applicable.

| ID | Failure mode | How we detect (automated) | How we detect (operator) | Blocking task ID | Done |
|----|--------------|---------------------------|----------------------------|------------------|------|
| FM-1 | | | | | [ ] |
| FM-2 | | | | | [ ] |

---

## 2. Call-site / data-path audit (read-only)

Grep/inventory **before** implement. Record paths that bypass the contract wrapper.

| Pattern / subsystem | Command or file list | Must go through | Bypass risk | Done |
|---------------------|----------------------|-----------------|-------------|------|
| | `rg '…' ComicRack/` | | | [ ] |

**Sign-off:** All writers/readers of persisted state mapped to contract invariant.

---

## 3. Lifecycle & init order

| Question | Answer / decision | Done |
|----------|-------------------|------|
| When is first `ScaleDpi()` / metrics call relative to DPI cache warm? | | [ ] |
| Which forms subscribe to global events; who unsubscribes? | | [ ] |
| Coarse fallback (`DisplaySettingsChanged`) vs per-window (`WM_DPICHANGED`) — duplicate work? | | [ ] |
| Process-wide cache vs per-window scale — accepted limitation documented? | | [ ] |

---

## 4. Contract invariants → tests (design before code)

Each contract file: invariant + **test type** assigned before implement.

| Contract | Invariant (one line) | CI test | Operator test | Test task ID | Done |
|----------|----------------------|---------|---------------|--------------|------|
| | | unit / script / grep guard | quickstart scenario | | [ ] |

**Rule:** If FR claims CI coverage, test task must exist in `tasks.md` **before** implement phase.

---

## 5. Build & CI proof plan

| Check | Debug | Release | CI workflow | Done |
|-------|-------|---------|-------------|------|
| Full rebuild path documented | [ ] | [ ] | | [ ] |
| Post-build gate wired (MSBuild + workflow) | [ ] | [ ] | | [ ] |
| Negative assertion (forbidden stale tokens) | [ ] | [ ] | | [ ] |
| `LangVersion` / toolchain constraints in tasks | [ ] | | | [ ] |

---

## 6. Scope & upstream boundaries

| Item | In scope | Out of scope / Phase E | Done |
|------|----------|------------------------|------|
| Files/modules touched | | | [ ] |
| Unrelated diffs excluded (e.g. DarkMode) | | | [ ] |
| Upstream PR target branch | | | [ ] |

---

## 7. Optional: design review (no code)

Run `/universal-code-review` mentally against **contracts + tasks only**. Record gaps as new tasks.

| Finding | Severity | Added as task |
|---------|----------|---------------|
| | P0–P3 | |

---

## Gate

| Gate | Status | Date |
|------|--------|------|
| `/speckit-analyze` PASS | | |
| Pre-implement checklist complete | | |
| Operator waive (if any) | none / link | |

**Proceed to `/speckit-implement`:** [ ] yes  [ ] no — reason: ___________

---

## Reuse notes (006+ layout / HiDPI follow-ups)

When branching layout features after foundation:

- Re-run **§2** for every `ViewConfig`, `ScaleDpi`, `Apply*Metrics` touch.
- Re-run **§1** manifest/EXE check if touching `app.manifest` or build pipeline.
- Operator SC-006 smoke is **not** a substitute for §4 CI tests.
