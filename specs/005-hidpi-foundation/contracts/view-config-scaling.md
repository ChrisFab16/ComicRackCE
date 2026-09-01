# Contract: View Config Scaling (Non-Mutating)

**Feature**: `005-hidpi-foundation`  
**Implements**: FR-007, SC-003

## Requirements

1. Loading workspace `ItemViewConfig` MUST NOT call logic that writes scaled thumb/tile/row values back to persisted config.
2. Display at HiDPI MUST use existing `ComicBrowserControl.GetItemSize()` / `SetItemSize()` or equivalent runtime apply that scales from stored logical values.
3. User-initiated resize via UI MUST continue to persist the user's chosen size (unchanged behavior).
4. Remove or gut `NormalizeViewConfigSizes` if it creates a new `ItemViewConfig` with rewritten dimensions on load.

## Verification

1. Save workspace at 100% with known thumb height (e.g. 128).
2. Set Windows to 150%; launch app; load workspace.
3. Confirm workspace XML still shows 128 for thumb height.
4. Confirm UI displays proportionate thumbs at 150%.

## Migration

No DB migration. One-time load may look different if users already have configs mutated by PR #278 branch—document as fork-only; upstream users unaffected.
