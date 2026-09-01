# Data Model: HiDPI Foundation

**Feature**: `005-hidpi-foundation`

## Entities

### EmbeddedApplicationManifest

| Field | Type | Source | Notes |
|-------|------|--------|-------|
| dpiAwareness | string | `app.manifest` | Must be `PerMonitorV2` in linked EXE |
| BuildArtifact | binary | `myressources.res` | Regenerated every build |

### FormDpiContext

| Field | Type | Source | Notes |
|-------|------|--------|-------|
| Form | FormEx | HWND owner | One context per top-level form |
| ScaleX / ScaleY | float | WM_DPICHANGED or GetDpiForWindow | Replaces global-only cache |
| LastDpi | int | WM_DPICHANGED wParam | For debugging |

### DpiScaleCache (FormUtility)

| Field | Type | Before 005 | After 005 |
|-------|------|------------|-----------|
| dpiScale | PointF | Global, primary monitor | Per-control query preferred; global fallback only when no HWND |

### ItemViewConfig (persisted — unchanged schema)

| Field | Type | Persisted | HiDPI rule |
|-------|------|-----------|------------|
| ThumbnailSize | Size | Yes | Store user logical size; scale at render |
| TileSize | Size | Yes | Same |
| ItemRowHeight | int | Yes | Same |

**Invariant (FR-007)**: Load at 150% must not rewrite these fields unless user explicitly resizes.

## Events

| Event | Publisher | Subscribers (examples) |
|-------|-----------|------------------------|
| DpiScaleChanged | FormUtility | ComicListFolderFilesBrowser, ComicExplorerView, ComicBrowserControl |

## State transitions

```
Build → compile_res_file.ps1 → myressources.res updated
Process start → PMv2 active → FormEx created
Form moved → WM_DPICHANGED → RefreshDpiScale(form) → subscribers re-apply metrics
Workspace load → ItemViewConfig read → display scale applied (no XML write)
```
