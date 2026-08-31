# Quickstart: Explorer Shell HiDPI Layout Validation

**Feature**: `002-explorer-hidpi-layout`  
**Branch**: `002-explorer-hidpi-layout`

**Prerequisite**: Feature **001** on branch (PerMonitorV2, Folders sidebar HiDPI).

Manual validation required (Constitution III).

---

## Prerequisites

- Windows 10 or 11
- Build: `msbuild ComicRack\ComicRack.csproj /t:Rebuild /p:Configuration=Debug /p:LangVersion=13.0`
- Test library with comics for preview pane
- Folder tree for Folders tab

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

## Scenario 1: Sidebar width — Folders (P1 / SC-001)

1. At **150%**, reset Folders layout or use fresh profile.
2. Open **Folders** tab.
3. Verify sidebar width fits scaled folder tree labels.

**Expected**: Adequate width; grip draggable; no excessive horizontal scroll for typical paths.

Repeat on **Library** tab (same `ComicExplorerView` shell).

---

## Scenario 2: Sidebar width — legacy workspace (FR-004)

1. At **100%**, adjust sidebar width, save workspace (exit app).
2. Switch to **150%**, restart, load workspace.

**Expected**: Width proportionate (001 normalization); not stuck at tiny 100% size. No double-scaling.

---

## Scenario 3: Preview pane (P2 / SC-002)

1. At **150%**, enable preview pane on Folders or Library tab.
2. Select a comic with caption/metadata.

**Expected**: Preview height proportionate; caption readable; margins not cramped.

---

## Scenario 4: Explorer behavior (FR-005 / SC-004)

At **150%**:

1. Select folder/comic → list updates.
2. Toggle preview pane.
3. Drag sidebar and preview splits.
4. Save workspace, restart — splits restored.

**Expected**: Same behavior as pre-change baseline.

---

## Scenario 5: Main tab strip (P3 / FR-006)

1. At **150%**, view Library / Folders / Pages tabs.

**Expected**: Sharp icons; comfortable padding.

---

## Scenario 6: Dark mode

Enable dark theme. Repeat Scenarios 1 and 3 at 150%.

**Expected**: Readable preview caption and tab strip.

---

## Sign-off template

Record in `validation-results.md`:

```markdown
| Scenario | 100% | 125% | 150% | 200% | Tester | Date |
|----------|------|------|------|------|--------|------|
| 1 Sidebar Folders | | | | | | |
| 1 Sidebar Library | | | | | | |
| 2 Legacy workspace | | | | | | |
| 3 Preview pane | | | | | | |
| 4 Behavior | | | | | | |
| 5 Tab strip | | | | | | |
| 6 Dark | | | | | | |
```

**Done when**: Scenarios 1–5 pass at 125%, 150%, and 200%; no SC-004 regressions.
