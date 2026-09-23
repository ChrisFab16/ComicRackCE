# IronPython deprecation policy (ComicRackCE fork)

**Date:** 2026-09-23  
**Scope:** Fork `ChrisFab16/ComicRackCE` planning only — not an upstream commitment.

## Current state (after 006 + 007)

| Runtime | Role |
|---------|------|
| IronPython 2.7 | Default for logic hooks (`Books`, scrapers, smartlists, overlays, …) |
| WebView2 + Host API v1 | Configure / SPA UI; `kind: web` UI-only packages |

**IronPython is not removed in feature 007.** Dual-host is intentional.

## Timeline (proposed)

| Phase | Window | Action |
|-------|--------|--------|
| **A — Dual-run (now)** | Until Host API + web-kind are stable and ≥1 major community plugin has a SPA path | Document migration; accept both kinds |
| **B — Soft deprecate** | Announce in release notes / README after Phase A criteria | Mark new plugins: prefer `kind: web` for UI; discourage new WinForms-from-Python UIs |
| **C — Hard deprecate** | Separate Spec Kit feature after maintainer + operator OK | Warn on load for pure-WinForms Configure; still run Python |
| **D — Removal** | Future feature only | Drop IronPython host when migration coverage is acceptable |

Exact calendar dates are **not** set here. Each of B–D needs its own spec/analyze before code.

## What will not happen in 007

- Removing `ComicRack.Plugins` IronPython hosting
- Breaking Library Organizer / Comic Vine–class `.py` packages
- Requiring authors to rewrite logic hooks in JS

## Author guidance

- New **Configure UIs**: SPA + Host API (006/007).
- New **automation/scraper logic**: IronPython remains OK during Phase A–C.
- Track progress via fork `development` and `specs/007-plugin-web-kind/`.
