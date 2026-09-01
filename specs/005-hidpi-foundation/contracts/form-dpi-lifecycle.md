# Contract: Form DPI Lifecycle

**Feature**: `005-hidpi-foundation`  
**Implements**: FR-004, FR-005, FR-006, SC-002

## Requirements

1. `FormEx` overrides `WndProc`; on `WM_DPICHANGED` (0x02E0), call base then notify DPI refresh for **this** form.
2. `FormUtility` provides `GetDpiScale(Control owner)` using the control's window DC (e.g. `GetDpiForWindow` when handle exists, else `CreateGraphics()` on owner).
3. `RefreshDpiScale(Control source)` invalidates relevant cache and raises `DpiScaleChanged` with `source` identifiable (sender or EventArgs).
4. `MainForm` and at least `ReaderForm` subscribe and re-invoke existing metrics helpers (no new layout logic in 005).
5. `DisplaySettingsChanged` may remain as coarse fallback; must not be the only path.
6. `FormUtility.DpiScale` cache is process-wide; multi top-level window / cross-monitor scenarios are validated under SC-002 (operator). Per-window cache is out of scope for 005.

## Non-requirements (this feature)

- Full metrics re-apply on every dialog (only prove hook works on MainForm + ReaderForm + one dialog).
- Replacing all `ScaleDpi` call sites.

## Verification

Quickstart Scenario 2: two monitors, different scale, drag window, observe layout refresh or logged scale ≠ primary monitor scale.

## API sketch (implement phase)

```csharp
// FormEx — conceptual
protected override void WndProc(ref Message m) {
    base.WndProc(ref m);
    if (m.Msg == WM_DPICHANGED) OnDpiChangedCore(...);
}

// FormUtility — conceptual
public static PointF GetDpiScale(Control owner);
public static void RefreshDpiScale(Control source);
public static event EventHandler<DpiScaleChangedEventArgs> DpiScaleChanged;
```
