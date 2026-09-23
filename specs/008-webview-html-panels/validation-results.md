# Validation: 008-webview-html-panels

| Gate | Result | Notes |
|------|--------|-------|
| SC-001 HtmlInfoContent tests | **PASS** | unit |
| SC-003 No WebBrowser in panel control | **PASS** | WebView2 only |
| SC-002 Operator Sample ComicInfoHtml | pending | Enable Dummy Book Info HTML in Sample.py; open info sidebar |

## Quick operator check

1. Preferences → Scripts → enable **[Code Sample] Dummy Book Info HTML** (uncheck Hide samples if needed).
2. Restart if required; show Comic Info HTML panel.
3. Confirm table renders; try Open button (`window.external.ComicRack.OpenBooks.OpenFile`).
