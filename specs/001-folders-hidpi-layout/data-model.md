# Data Model: Folders Tab HiDPI Layout

**Feature**: `001-folders-hidpi-layout`  
**Date**: 2026-08-31

Logical entities for layout state and DPI behavior. No new database tables; persistence uses existing workspace/config XML.

---

## FoldersSidebarLayout (runtime, in-memory)

Ephemeral layout state held by `ComicListFolderFilesBrowser` and parent `ComicExplorerView`.

| Field | Type | Source | Notes |
|-------|------|--------|-------|
| `ToolbarVisible` | bool | always true | Dock top |
| `FavoritesExpanded` | bool | `favContainer.Expanded` / `TopBrowserVisible` | User toggle via toolbar |
| `FavoritesHeight` | int (px) | `favContainer.ExpandedWidth` | Vertical split when docked top |
| `TreeClientArea` | Rectangle | computed | Remaining space below favorites |
| `TreeIndent` | int (px) | `tvFolders.Indent` | Must scale with DPI |
| `TreeItemHeight` | int (px) | `tvFolders.ItemHeight` | `Font.Height + ScaleDpiY(8)` when skinned |
| `TreeFont` | Font | `tvFolders.Font` | UI font, not IconTitleFont |
| `FavoritesTileSize` | Size | `favView.ItemTileSize` | Width = client width − scrollbar − margin |
| `CurrentDpiScale` | PointF | `FormUtility.DpiScale` | Cached; must refresh on DpiChanged |

**Validation**

- `TreeItemHeight` ≥ `Font.Height` at all supported scales (125%–200%).
- `FavoritesHeight` when expanded ≥ `FavoritesTileSize.Height` + scaled padding.

**State transitions**

- DPI change → refresh `CurrentDpiScale` → recompute all scaled metrics → `Invalidate` tree + favView.
- Favorites toggle → `FavoritesExpanded` flips; tree area grows/shrinks (no metric re-scale required).

---

## FavoriteFolder (existing, presentation only)

| Field | Type | Notes |
|-------|------|-------|
| `Path` | string | Full directory path; `Tag` on `FolderViewItem` |
| `DisplayName` | string | `FileUtility.GetSafeFileName(Path)` — primary text line |
| `ThumbnailKey` | ThumbnailKey | Mosaic from child comics |

No persistence change; display rules in [contracts/folders-sidebar-layout.md](./contracts/folders-sidebar-layout.md).

---

## FolderTreeNode (existing, shell-backed)

| Field | Type | Notes |
|-------|------|-------|
| `ShellFolder` | ShellFolder | `TreeNode.Tag` |
| `DisplayName` | string | `TreeNode.Text` |
| `PhysicalPath` | string | From `ShellPidl` when available |
| `IsExpanded` | bool | Tree state |
| `IsSelected` | bool | Selection state |

Behavior unchanged (FR-007).

---

## ComicExplorerViewSettings (persisted)

Existing serializable settings; fields relevant to Folders tab:

| Field | Type | Default (new) | Persisted | HiDPI note |
|-------|------|---------------|-----------|------------|
| `ShowTopBrowser` | bool | false | yes | Favorites visibility |
| `TopBrowserSplit` | int | `ScaleDpiY(150)` | yes | Favorites height — legacy load heuristic |
| `BrowserSplit` | int | `ScaleDpiY(250)` | yes | Sidebar width |
| `PreviewSplit` | int | `ScaleDpiY(200)` | yes | Preview pane (explorer) |
| `ShowBrowser` | bool | true | yes | Sidebar expanded |

**Future optional extension** (deferred):

| Field | Type | Purpose |
|-------|------|---------|
| `SavedAtDpiPercent` | int | e.g. 150 — exact reload scaling |

---

## DisplayWorkspace (existing aggregate)

Contains `FileView: ComicExplorerViewSettings` for Folders tab. `MainView.StoreWorkspace` / `SetWorkspace` read/write path unchanged; apply logic gains DPI normalization per workspace contract.

---

## Relationships

```text
DisplayWorkspace
  └── FileView: ComicExplorerViewSettings
        └── applied to ComicExplorerView
              └── ComicListBrowser: ComicListFolderFilesBrowser
                    ├── favView (ItemView → FolderViewItem[])
                    └── tvFolders (FolderTreeView)
```
