# Quickstart: Folders Tab HiDPI Layout Validation

**Feature**: `001-folders-hidpi-layout`  
**Branch**: `001-folders-hidpi-layout`

Manual validation required (Constitution III). Automated tests optional supplement only.

---

## Prerequisites

- Windows 10 or 11
- Visual Studio 2022 or `msbuild ComicRack.sln /p:Configuration=Debug`
- Test folder tree with ≥20 subfolders (local disk)
- 3+ favorite folders including one with a long path (>60 chars)

---

## Build

```bash
cd H:/Syncthing/Codesync/ComicRackCE
msbuild ComicRack.sln /p:Configuration=Debug /v:m
```

Run: `ComicRack/bin/Debug/ComicRack.exe` (or VS F5).

---

## Display scale matrix

Run scenarios **A–D** at each scale. Change scale via **Settings → System → Display → Scale**, then **sign out/in** or reboot if WinForms does not pick up change.

| ID | Scale | Priority |
|----|-------|------------|
| A | 100% | Baseline |
| B | 125% | Required |
| C | 150% | Required (primary) |
| D | 200% | Required |

---

## Scenario 1: Folder tree legibility (P1 / SC-002)

**Steps**

1. Open ComicRackCE at scale **C (150%)**.
2. Select **Folders** tab.
3. Expand Desktop → 3+ nested folders (≥20 visible nodes total).
4. Select first, middle, and last visible nodes.

**Expected**

- All labels readable without Windows Magnifier.
- No vertical clipping on selected/unselected text.
- Row height comfortably fits text + icon.

Repeat at **D (200%)**.

---

## Scenario 2: Favorites strip (P2 / FR-004)

**Steps**

1. Add 3 favorites (one long path).
2. Ensure favorites panel expanded (toolbar star toggle).
3. At **150%**, narrow sidebar until horizontal scrollbar or truncation appears.

**Expected**

- Folder name remains visible on each tile.
- Path truncates with ellipsis; no overlapping lines.
- Tile height fits two lines without clipping descenders.

---

## Scenario 3: Toolbar & splits (P3 / SC-003)

**Steps**

1. At **150%**, fresh workspace (or reset Folders layout).
2. Expand favorites with ≤3 entries.
3. Count visible tree rows without scrolling.

**Expected**

- ≥ 5 full tree rows (SC-003).
- Toolbar icons sharp; buttons easy to click.

---

## Scenario 4: Behavior unchanged (FR-007)

At any scale:

1. Select folder → comic list updates.
2. Toggle **Include subfolders** → list count changes.
3. **Refresh** → no error.
4. **Add to favorites** → appears in strip.
5. Double-click favorite → tree drills to path.

**Expected**: Same behavior as pre-change baseline.

---

## Scenario 5: Workspace migration (C-WSP-001)

1. At **100%**, set favorites height via splitter, save workspace (exit app normally).
2. Switch display to **150%**, restart app, load workspace.

**Expected**

- Favorites height proportionate (not stuck at tiny 100% pixel size).
- Sidebar still usable without manual reset.

---

## Scenario 6: Dark mode (C-FSL-007)

Enable dark theme in ComicRack preferences. Repeat Scenario 1 at 150%.

**Expected**: Readable text and selection on tree + favorites.

---

## Optional: Mixed-DPI

Move window from primary (150%) to secondary monitor (100% or 125%) if available.

**Expected**: No crash; layout acceptable or refreshes after move (best-effort per spec edge case).

---

## Sign-off template

Record in `validation-results.md` (create at implement time):

```markdown
| Scenario | 100% | 125% | 150% | 200% | Tester | Date |
|----------|------|------|------|------|--------|------|
| 1 Tree   |      |      |      |      |        |      |
| 2 Favorites |   |      |      |      |        |      |
| 3 Splits |      |      |      |      |        |      |
| 4 Behavior |    | pass |      |      |        |      |
| 5 Workspace |  |      |      |      |        |      |
| 6 Dark   |      |      |      |      |        |      |
```

**Done when**: Scenarios 1–5 pass at 125%, 150%, and 200%; no FR-007 regressions.
