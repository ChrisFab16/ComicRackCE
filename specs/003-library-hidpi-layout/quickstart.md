# Quickstart: Library Comic List HiDPI Layout Validation

**Feature**: `003-library-hidpi-layout`  
**Branch**: `003-library-hidpi-layout`

**Prerequisite**: Features **001** and **002** on branch.

Manual validation required (Constitution III).

---

## Prerequisites

- Windows 10 or 11
- Build: `msbuild ComicRack\ComicRack.csproj /t:Rebuild /p:Configuration=Debug /p:LangVersion=13.0`
- Library with comics; folder tree for Folders tab

---

## Display scale matrix

| ID | Scale | Priority |
|----|-------|----------|
| A | 100% | Baseline |
| B | 125% | Required |
| C | 150% | Required (primary) |
| D | 200% | Required |

Change scale via **Settings → System → Display → Scale**; sign out/in or reboot if needed.

---

## Scenario 1: Thumb view — Library (P1 / SC-001)

1. At **150%**, open **Library** tab in thumb view.
2. Verify cover size and title text proportionate.

**Expected**: Readable without magnifier; covers not tiny 96-DPI grid.

Repeat on **Folders** tab comic list.

---

## Scenario 2: Tile view (P1 / SC-001)

1. At **150%**, switch to tile view on Library.
2. Scroll several rows.

**Expected**: Primary labels not clipped; tile height fits typography.

---

## Scenario 3: List/detail view (P2 / SC-002)

1. At **150%**, switch to list or detail view.
2. Verify column headers and row text.

**Expected**: No vertical clipping on standard single-line fields.

---

## Scenario 4: Toolbar (P3 / SC-003)

1. At **150%**, view comic browser toolbar (sort, group, layout).
2. Click sort and layout controls.

**Expected**: Sharp icons; comfortable hit targets; behavior unchanged.

---

## Scenario 5: Context menu (P3 / FR-007)

1. At **150%**, right-click a comic.
2. Open nested menus (Mark as, Rating) if available.

**Expected**: Readable items; icons proportionate.

---

## Scenario 6: Behavior regression (FR-005 / SC-004)

At **150%**:

1. Select comics; sort and group.
2. Quick search.
3. Open comic (read).
4. Ctrl+wheel resize thumb size; restart app — size restored.

**Expected**: Same behavior as pre-change baseline.

---

## Scenario 7: Legacy workspace / view config (FR-004)

1. At **100%**, set thumb size and layout; save workspace; exit.
2. At **150%**, restart and load workspace.

**Expected**: Sizes proportionate; not stuck at tiny 100% values.

---

## Scenario 8: Dark mode

Enable dark theme. Repeat Scenarios 1 and 3 at 150%.

**Expected**: List text and headers readable.

---

## Sign-off template

Record in `validation-results.md`:

```markdown
| Scenario | 100% | 125% | 150% | 200% | Tester | Date |
|----------|------|------|------|------|--------|------|
| 1 Thumb Library | | | | | | |
| 1 Thumb Folders | | | | | | |
| 2 Tile | | | | | | |
| 3 List/detail | | | | | | |
| 4 Toolbar | | | | | | |
| 5 Context menu | | | | | | |
| 6 Behavior | | | | | | |
| 7 Legacy config | | | | | | |
| 8 Dark | | | | | | |
```

**Done when**: Scenarios 1–6 pass at 125%, 150%, and 200%; no SC-004 regressions.
