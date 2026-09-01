# Reply — PR #278 review

**Do not post without operator review.**  
Post as a comment on: [maforget/ComicRackCE#278](https://github.com/maforget/ComicRackCE/pull/278#issuecomment-5490677717)

---

Thanks for the review — I agree with your assessment and I'm closing this PR.

You were right about the manifest: I changed `app.manifest` to `PerMonitorV2` but never recompiled `myressources.res`, so the embedded manifest was still on the old `system` awareness. That undermines the whole approach in this diff — including what I thought I was validating manually.

I also take your points on `GetDC(IntPtr.Zero)` (primary monitor only, not per-window), `NormalizeViewConfigSizes` mutating user config instead of using `GetItemSize`/`SetItemSize`, MainForm-only DPI wiring, unrelated DarkMode changes, and the overall scope. I should have used the existing APIs and a narrower, clearer design before spreading `Apply*Metrics()` across the codebase.

On process: I used AI as a helper on this work, but the commits and co-authored lines were my mistake — I didn't review the output carefully enough (including the stale `.res` file).

**What I'll do next:** I'll open a new PR aligned with your design — PreBuild recompile of the manifest (as on `dev`), `WM_DPICHANGED` on `FormEx`, per-window DPI, no config mutation, and a much smaller scope. I'll sync from `dev` before I submit. Layout fixes from this branch can wait until that foundation is correct.

Whether you want to keep this PR open for reference or ignore it is entirely up to you. I won't ask for it to be merged.

Thanks for the time you spent on the review — the manifest catch alone was worth it.
