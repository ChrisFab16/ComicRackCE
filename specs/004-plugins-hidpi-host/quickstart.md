# Quickstart: Library Organizer Plugin HiDPI Validation

**Feature**: `004-plugins-hidpi-host` (deferred CE host; plugin fork delivery)  
**Branch**: `004-plugins-hidpi-host`  
**Plugin**: Library Organizer **2.1.16+** from [PR #25](https://github.com/Stonepaw/comicrack-library-organizer/pull/25)

## Prerequisites

- Windows 10/11, ComicRack Community Edition (HiDPI core 001–003 validated)
- Display scale **150%** for primary pass; repeat key scenarios at **200%**
- Single Scripts install folder: `%AppData%\cYo\ComicRack Community Edition\Scripts\Library Organizer\`
- Verify `Package.ini` shows `Version=2.1.16` and Configure title shows `Library Organizer 2.1.16`

## Install (fork build)

1. Remove duplicate `Library Organizer` folders under `Scripts\` if present.
2. Copy fork contents from `external/comicrack-library-organizer/` into the single `Scripts\Library Organizer\` folder.
3. Restart ComicRack CE.

## Scenarios

| # | Action | Pass criteria |
|---|--------|---------------|
| 1 | Open **Library Organizer → Configure** | Dialog shell sized; no clipped title bar buttons |
| 2 | **Files** tab — Text Fields insert grid | Two columns; prefix/postfix fields readable; no overlap |
| 3 | **Files** — Multiple Value Fields | Wide rows (Characters, Genre, etc.) do not overlap |
| 4 | **Files** — Yes/No Fields | Manga / Series Complete stack; instructions below controls |
| 5 | **Files** — Calculated | Tall rows (Read %) stack; info label to the right |
| 6 | **Files** — Search tab → another tab → back | Layout restored; column labels centered |
| 7 | **Rules** — Folder Rules | List + Add/Remove aligned |
| 8 | **Rules** — Metadata Rules | Header row fits; rule rows fit panel width |
| 9 | **Options** tab | Month/illegal-character rows flow; empty-folder list + buttons aligned |
| 10 | **Empty values** tab | Substitution + failed-empty blocks chain vertically |

## Record results

Copy outcomes to [validation-results.md](./validation-results.md): scale %, build/plugin version, pass/fail per scenario, notes.

## Known limitation

DPI/monitor change **while Configure is open** does not relayout — close and reopen after changing Windows display scale.
