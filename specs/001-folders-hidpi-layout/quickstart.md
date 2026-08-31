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
- Optional: OneDrive or UNC path visible in shell tree for Scenario 8

---

## Build

```bash
cd H:/Syncthing/Codesync/ComicRackCE
msbuild ComicRack.sln /p:Configuration=Debug /v:m
```

Run: `ComicRack/bin/Debug/ComicRack.exe` (or VS F5).

---

## Display scale matrix

Run scenarios at each scale. Change scale via **Settings → System → Display → Scale**, then **sign out/in** or reboot if WinForms does not pick up change.

| ID | Scale | Priority |
|----|-------|------------|
| A | 100% | Baseline |
| B | 125% | Required |
| C | 150% | Required (primary) |
| D | 200% | Required |

### Scale-change method (record in validation-results)

For US1 acceptance (proportionate layout after scale change), note which method you used:

- **Restart**: change display scale → sign out/in or reboot → launch app (primary path; most reliable on .NET 4.8 WinForms)
- **Live / mixed-DPI**: change scale or move window without restart (best-effort; optional Mixed-DPI section below)

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

Repeat at **D (200%)** and **B (125%)**.

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

1. At **150%**, use a **fresh workspace** (new profile or reset Folders layout — not a legacy workspace saved at 100%).
2. Expand favorites with ≤3 entries.
3. Count visible tree rows without scrolling.

**Expected**

- ≥ 5 full tree rows (SC-003).
- Toolbar icons sharp; buttons easy to click (scaled hit targets, not image-only).

**Note**: Legacy workspace split behavior is validated separately in Scenario 5 **after** workspace normalization tasks (T027–T028).

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

## Scenario 5: Workspace migration (C-WSP-001 / C-WSP-003)

**When**: After Phase 6 tasks T027–T028 (normalization + fallback) are implemented.

1. At **100%**, set favorites height via splitter, save workspace (exit app normally).
2. Switch display to **150%**, restart app, load workspace.

**Expected**

- Favorites height proportionate (not stuck at tiny 100% pixel size).
- Sidebar still usable without manual reset.
- Favorites pane not collapsed to unusable sliver (< scaled 40px effective) unless user saved it that way.

---

## Scenario 6: Dark mode (C-FSL-007)

Enable dark theme in ComicRack preferences. Repeat Scenario 1 at 150%; also spot-check favorites tiles if US2 complete.

**Expected**: Readable text and selection on tree + favorites.

**Checkpoint**: US1 implementers run tree portion after attaching NiceTreeSkin (T015); full Scenario 6 at sign-off.

---

## Scenario 7: Empty favorites (spec edge case)

**Steps**

1. Remove all favorites (or use profile with none).
2. At **150%**, open Folders tab with favorites panel expanded and collapsed.

**Expected**

- Tree docks correctly; no awkward dead space or layout collapse.
- Toolbar and tree remain usable.

---

## Scenario 8: Special shell folders (spec edge case)

**Steps**

1. At **150%**, expand a OneDrive, network (UNC), or other special shell folder if available.
2. Select nodes; verify icons and labels align within row bounds.

**Expected**: Icons and labels align; no regression vs local folders.

---

## Optional: Mixed-DPI

Move window from primary (150%) to secondary monitor (100% or 125%) if available.

**Expected**: No crash; layout acceptable or refreshes after move (best-effort per spec edge case). Record whether restart was required.

---

## Optional: Performance spot-check (plan)

After DPI change or monitor move (with T007 wired):

1. Trigger layout refresh (move window or change scale if live refresh works).
2. Note subjective responsiveness — layout should update without noticeable stall (<100 ms target on typical hardware).

Record pass/fail and hardware notes in validation-results.

---

## Success criteria not in v1 sign-off

| ID | Reason |
|----|--------|
| SC-001 | Moderated 90% study — use subjective “legible?” column in sign-off template instead |
| SC-004 | Requires pre-change baseline timing study — deferred; not blocking v1 |

---

## Sign-off template

Record in `validation-results.md` (create at implement time):

```markdown
| Scenario | 100% | 125% | 150% | 200% | Tester | Date |
|----------|------|------|------|------|--------|------|
| 1 Tree   |      |      |      |      |        |      |
| 2 Favorites |   |      |      |      |        |      |
| 3 Splits |      |      |      |      |        |      |
| 4 Behavior |    |      |      |      |        |      |
| 5 Workspace |  |      |      |      |        |      |
| 6 Dark   |      |      |      |      |        |      |
| 7 Empty favs |  |      |      |      |        |      |
| 8 Shell folders | |    |      |      |        |      |
| Perf spot-check | |   |      |      |        |      |
| Scale method | restart / live |      |      |        |      |
| SC-001 legible (Y/N) | |  |      |      |        |      |
```

**Done when**: Scenarios **1–8** pass at **125%, 150%, and 200%**; Scenario **5** pass after T027–T028; no FR-007 regressions (Scenario 4); performance spot-check recorded (T033).
