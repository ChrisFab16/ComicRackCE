# Host API v1 — SPA plugin bridge

**Transport:** WebView2 `PostWebMessageAsJson` / `WebMessageReceived` (UTF-8 JSON-RPC 2.0).  
**Machine schema:** [host-api-v1.schema.json](./host-api-v1.schema.json)  
**Manifest:** [plugin-manifest.schema.json](./plugin-manifest.schema.json)

## Methods

| Method | Params | Result |
|--------|--------|--------|
| `host.getInfo` | — | `{ productVersion, apiVersion: 1, pluginId, pluginVersion, commandPath }` |
| `host.getTheme` | — | `{ isDark, backColor, foreColor }` (#RRGGBB) |
| `host.getSelectedBooks` | `{ limit?: number }` | `{ books: BookSummary[] }` |
| `host.getLibraryBooks` | `{ limit?: number }` | `{ books: BookSummary[] }` |
| `host.askQuestion` | `{ question, buttonText?, optionText? }` | `{ result: number }` (host `AskQuestion` return) |
| `host.showComicInfo` | `{ bookIds: string[] }` | `{ ok: true }` |
| `host.config.get` | — | `{ config: string }` (per-command config file contents) |
| `host.config.set` | `{ config: string }` | `{ ok: true }` |
| `host.ui.reload` | — | `{ ok: true }` — reload current SPA |
| `host.ui.close` | `{ dialogResult?: "ok"\|"cancel" }` | `{ ok: true }` — close Configure dialog |

## BookSummary

`{ id, caption, series?, number?, volume?, year?, filePath? }`

## Errors

JSON-RPC `error.code`: `-32601` method not found, `-32602` invalid params, `-32000` host failure, `-32001` unsupported `apiVersion`.

## Frontend helper

SPA should wrap `chrome.webview.postMessage` / `window.chrome.webview.addEventListener('message', ...)`. Sample: `samples/webview-configure-sample`.
