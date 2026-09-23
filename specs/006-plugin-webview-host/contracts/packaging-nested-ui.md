# Packaging — nested unzip + Configure UI hot-reload

## Nested package paths (required)

Today [`PackageManager.Package.UnzipFile`](../../../ComicRack.Engine/PackageManager.cs) writes `Path.GetFileName(entry.Name)` only, which **collapses** `ui/dist/index.html` into a flat folder. SPA packages require nested paths.

**Target behavior:**

1. For each zip entry that is a file, compute a safe relative path under `targetPath`.
2. Create parent directories as needed.
3. **Zip-slip guard:** reject entries whose resolved full path is outside `targetPath` (after `GetFullPath`).
4. Preserve forward/backslashes from zip as `Path.DirectorySeparatorChar`.
5. Skip directory-only entries (existing filter).

**Compatibility:** Existing flat `.crplugin` packages keep working (entries with no directory segment).

## Hot-reload Configure UI (no app restart)

| Event | Behavior |
|-------|----------|
| Configure dialog open | Load `plugin.json` → `ui.configure` HTML via `file://` or WebView2 virtual host mapping |
| File change under configure entry directory | If `ui.hotReload` ≠ false, debounce (~300ms) and `CoreWebView2.Reload()` |
| New hooks / `.py` changes | Still require app restart (IronPython discovery at startup) — document in quickstart |
| Pending install commit | Unchanged restart-on-commit for package folder swap; after restart, SPA loads new `ui/dist` |

## Manifest + Package.ini

- Install still keyed by `Package.ini` `Name`.
- `plugin.json` optional; when present with `ui.configure`, host can open SPA Configure without WinForms.
- Version strings should match between `Package.ini` and `plugin.json` (warn in logs if mismatch; do not block).
