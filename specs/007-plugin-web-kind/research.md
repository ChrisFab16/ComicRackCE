# Research: kind=web dual-host

## Decision: Manifest-driven commands, not JS hook VM

**Rationale:** Phase 2 goal is SPA-first packages without Python. A full in-process JS hook runtime (Books/smartlists/overlays) is a separate project. MVP registers managed commands that open Host API Configure.

## Decision: Python ConfigScript wins on Key collision

**Rationale:** Existing plugins that add SPA later keep working ConfigScript until they delete it.

## Decision: Docs-only deprecation

**Rationale:** Ecosystem needs a dual-run window; removing IronPython in the same PR as web-kind would strand Library Organizer et al.
