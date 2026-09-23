# Migration guide: IronPython / WinForms → web-kind SPA

**Audience:** Plugin authors  
**Host API:** v1 (`specs/006-plugin-webview-host/contracts/`)  
**Status:** Dual-host — IronPython remains supported (see [deprecation-policy.md](./deprecation-policy.md)).

## Quick map

| Today (legacy) | Target |
|----------------|--------|
| `#@Hook ConfigScript` + WinForms `Form` | `plugin.json` `ui.configure` → WebView2 SPA |
| `ComicRack.App.*` / mutate books in dialog | Host API JSON-RPC (`host.getLibraryBooks`, `host.config.*`, …) |
| Absolute `Location`/`Size` layouts | CSS / modern SPA (HiDPI-free) |
| `Package.ini` only | Keep `Package.ini` for install; add `plugin.json` |
| Logic in `.py` hooks (`Books`, scrapers, …) | **Keep `.py`** for now (`kind: python`); or ship UI-only as `kind: web` |

## Choose a kind

### `kind: python` (default)

- Keep `#@` hooks and `.py` logic.
- Optional: add SPA Configure via `ui.configure` and call `ComicRack.ShowWebConfigure()` from ConfigScript (see `WebConfigureSample`).

### `kind: web` (Phase 2)

- **No `.py` required.**
- `plugin.json` must set `"kind": "web"`, `apiVersion: 1`, and `ui.configure`.
- Host registers a Preferences-visible command and Configure → SPA.
- Suitable for settings-only / UI-first plugins. Not for smartlists, path parsers, GDI overlays, or scrapers yet (still use Python).

## Minimal `plugin.json` (web)

```json
{
  "id": "MyPlugin",
  "version": "1.0.0",
  "apiVersion": 1,
  "kind": "web",
  "ui": {
    "configure": "ui/dist/index.html",
    "hotReload": true
  },
  "hooks": [
    { "type": "Books", "key": "MyPlugin", "name": "My Plugin" }
  ]
}
```

Also ship `Package.ini` (`Name`, `Version`, …) and nested `ui/dist/` (zip must preserve paths).

## SPA bridge

Use the sample `host-bridge.js` pattern: JSON-RPC over `chrome.webview.postMessage`. Methods: see `specs/006-plugin-webview-host/contracts/host-api-v1.md`.

## Packaging

1. Build SPA into `ui/dist`.
2. Zip preserving folders → `.crplugin`.
3. Install; **restart** ComicRack (hook discovery is at startup).
4. SPA asset edits can hot-reload while Configure is open; `.py` / new hooks still need restart.

## Precedence

If the same package still has a Python `ConfigScript` for the same `Key`, **Python Configure wins**. Remove the Python ConfigScript when the SPA is authoritative.
