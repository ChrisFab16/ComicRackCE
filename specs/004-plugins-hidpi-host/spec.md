# Feature: Plugin HiDPI Host

**Branch**: `004-plugins-hidpi-host`  
**Status**: **Closed (deferred)** — operator sign-off 2026-09-01; plugin **2.1.18**; CE host shelved.

Plugin HiDPI layout fixes belong in the plugin repo, not ComicRack CE host hooks. See `external/comicrack-library-organizer/` (fork of [Stonepaw/comicrack-library-organizer](https://github.com/Stonepaw/comicrack-library-organizer)).

## Problem

Third-party Python/IronPython plugins (e.g. Library Organizer) build WinForms UI at 96-DPI fixed sizes. At 125%–200% display scale, text fields clip or appear empty while core ComicRack UI (001–003) is scaled.

## Solution (original CE host proposal — shelved)

The following host-side approach was prototyped on branch `004-plugins-hidpi-host` but **is not pursued** for upstream CE. Plugin-local relayout in the Library Organizer fork is the delivery path (see §Deferred implementation).

1. **`DpiAwareForm`** — replaces `System.Windows.Forms.Form` in IronPython **before** plugin scripts import submodules (e.g. `configureform.py`)
2. **`PluginDpiScaler`** — recursive control metrics (sizes, fonts, TableLayoutPanel rows, FlowLayoutPanel)
3. **`PluginDpiHost`** — `Application.Idle` fallback scales any open form not yet tagged (catches star-import edge cases)
4. **Invoke / panel hooks** — scale returned controls and `UIComicPageControl` children

## Root cause (v1 failure)

Initial hook ran **after** plugin script execute and patched `Form.ShowDialog` on the class — does not intercept `configform.ShowDialog()` on instances. Library Organizer uses `class ConfigureForm(Form)` with fixed 96-DPI control sizes from `configformcontrols.py`.

## Limits

- Native C# plugin DLLs that call `ShowDialog` without IronPython are not hooked (rare)
- Complex layouts (TableLayoutPanel absolute rows) may need plugin-specific fixes
- DPI change while a plugin dialog is open does not re-scale (same as 001–003 v1)

## Deferred implementation (2026-09-01)

ComicRack CE host hooks (004) are **postponed**. HiDPI layout fixes for Library Organizer are developed in a **separate fork** cloned to `external/comicrack-library-organizer/` (gitignored; commits go to [ChrisFab16/comicrack-library-organizer](https://github.com/ChrisFab16/comicrack-library-organizer), upstream [Stonepaw/comicrack-library-organizer](https://github.com/Stonepaw/comicrack-library-organizer)).

## Validation

**FR-001**: At 125%–200% Windows display scale, Library Organizer **Configure** dialog fields are readable and non-overlapping on all tabs (see [quickstart.md](./quickstart.md)).

**SC-001**: Operator records pass for quickstart scenarios 1–10 at 150% (and spot-check at 200%) in `validation-results.md`.

**Out of scope for 004 v1**: Comic Vine Scraper and other plugins (may follow same `lodpi` pattern in their repos).
