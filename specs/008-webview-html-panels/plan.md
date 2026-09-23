# Plan: 008-webview-html-panels

Replace IE WebBrowser in `HtmlComicPageControl` with WebView2; add COM host-object proxy + `window.external` polyfill for Sample.py compatibility.

## Tasks

- T001 Spec/plan/tasks/analyze/checklist
- T010 HtmlInfoContent helper (`!url` vs HTML)
- T011 Rewrite HtmlComicPageControl + Designer for WebView2
- T012 COM proxy for external.ComicRack.OpenBooks / Config
- T013 Unit tests; build; validation docs
- T014 Fork PR → development
