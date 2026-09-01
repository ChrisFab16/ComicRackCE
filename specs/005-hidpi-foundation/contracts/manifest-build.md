# Contract: Manifest Build Pipeline

**Feature**: `005-hidpi-foundation`  
**Implements**: FR-001, FR-002, FR-003, SC-001

## Requirements

1. `ComicRack/app.manifest` declares `<dpiAwareness>PerMonitorV2</dpiAwareness>`.
2. PreBuild runs `compile_res_file.ps1` targeting `ComicRack/myressources.rc` → `ComicRack/myressources.res`.
3. `ComicRack.csproj` links `<Win32Resource>myressources.res</Win32Resource>` unchanged.
4. Clean build on reference machine produces EXE whose embedded manifest contains `PerMonitorV2`.

## Verification

```bash
# After msbuild (Windows, mt.exe from Windows SDK)
mt -inputresource:"ComicRack/bin/Debug/net48/ComicRack.exe;#1" -out:extracted.manifest
grep -i PerMonitorV2 extracted.manifest
```

Alternative: `scripts/verify-embedded-manifest.ps1` after build (FR-011) — preferred for CI and local gates.

## Failure modes

| Symptom | Cause | Fix |
|---------|-------|-----|
| EXE still `system` | `.res` not recompiled | Run PreBuild script; check rc.exe on PATH |
| Build fails PreBuild | No VS C++ tools | Install VS Build Tools; document in quickstart |
| Manifest edit ignored | Edited wrong file | Edit `app.manifest`, not `.res` directly |
