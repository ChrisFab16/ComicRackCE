# Quickstart: WebView2 plugin Configure

## Prerequisites

- Windows with [WebView2 Evergreen Runtime](https://developer.microsoft.com/microsoft-edge/webview2/)
- Build ComicRack CE (Visual Studio / `msbuild ComicRack.sln`)

## Sample package

Built-in: `Scripts/WebConfigureSample/` (copied from `ComicRack/Output/Scripts/WebConfigureSample`).

1. Launch ComicRack CE.
2. Preferences → Scripts → select **Web Configure Sample** (or use Automation menu entry).
3. Click **Configure**.
4. SPA should show product version (`host.getInfo`) and book list.
5. Edit note field → Save (`host.config.set`) → close → reopen → note restored.

## Hot-reload (dev)

1. Open Configure on the sample.
2. Edit `Scripts/WebConfigureSample/ui/dist/index.html` (or rebuild Vite into that folder).
3. WebView should reload within ~1s if `plugin.json` `ui.hotReload` is true.

## Rebuild sample UI from Vite

```bash
cd samples/webview-configure-sample
npm install
npm run build
# copies/writes into ComicRack/Output/Scripts/WebConfigureSample/ui/dist
```

## Nested package test (manual)

Zip a folder containing `ui/dist/index.html` + `Package.ini` + `plugin.json` as `.crplugin`, install, confirm nested path exists after restart/commit.

## Limitation

Changing IronPython `.py` hooks still requires restarting ComicRack.

## Preferences visibility

- **Packages** list only shows folders under `%AppData%\cYo\ComicRack Community Edition\Scripts\`.
- Built-in sample also ships next to the EXE under `Scripts\WebConfigureSample\` and appears in the **scripts/commands** list (e.g. “Web Configure Sample”) after a restart when running that build.
- For Packages-list visibility during testing, copy the sample folder into AppData `Scripts\`.
