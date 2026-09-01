# Pre-Implement Checklist (Spec Kit gate)

Generate and complete the **pre-implement checklist** after `/speckit-analyze` PASS and **before** `/speckit-implement` or feature code.

## When to run

- After `tasks.md` exists and analyze report shows PASS (no open CRITICAL/HIGH)
- Before writing product code for the feature
- Re-run when scope changes materially (new FR, new contract, new persistence path)

## What this is NOT

- **Not** a substitute for `/speckit-analyze` (artifact consistency only)
- **Not** a post-implement code review — use `/universal-code-review` after code exists
- **Not** operator sign-off — CI + quickstart still required after implement

## Steps

1. **Copy template**  
   `specs/_templates/checklist-pre-implement.md` → `specs/<feature>/checklist-pre-implement.md`

2. **Fill §1 Failure modes** from `research.md`, post-mortems (e.g. PR #278), and contract “Verification” sections. Each blocking failure mode needs a task ID in `tasks.md` **before** implement.

3. **Fill §2 Call-site audit** — run read-only grep/inventory:
   - Every read/write of persisted config
   - Every path that bypasses the contract wrapper
   - Add **Phase 0** tasks in `tasks.md` for any gap (no code yet)

4. **Fill §3 Lifecycle** — init order, event subscribe/unsubscribe, global vs per-window state. Document accepted limitations with SC/operator owner.

5. **Fill §4 Tests** — for each contract invariant, name the **CI test** (unit/script/guard) or mark operator-only with SC id. If FR says “CI required,” test task must exist before implement.

6. **Fill §5 Build/CI** — Debug **and** Release paths; negative assertions (forbidden tokens); workflow file names.

7. **Optional §7** — run `/universal-code-review` against contracts + tasks only; add findings as tasks.

8. **Gate** — set “Proceed to implement” only when all **Blocking** rows are `[x]` or operator-waived.

9. **Commit** — checklist is a Spec Kit artifact; git extension may auto-commit on `after_checklist` if enabled.

## Reference example

`specs/005-hidpi-foundation/checklist-pre-implement.md` (retrospective; shows what 006+ should do upfront).

## Workflow position

```text
/speckit-specify → /speckit-plan → /speckit-tasks → /speckit-analyze
  → /speckit-checklist-pre-implement  ← this skill
  → /speckit-implement → /universal-code-review → /speckit-converge (if needed)
```
