#Requires -Version 5.1
<#
.SYNOPSIS
    Verifies PerMonitorV2 is present in app.manifest, myressources.res, and the built EXE.

.DESCRIPTION
    Implements FR-011 / SC-001 (validation-automation contract).
    Exit 0 on success; non-zero on any failed assertion.

.PARAMETER ManifestPath
    Path to ComicRack/app.manifest

.PARAMETER ResPath
    Path to ComicRack/myressources.res

.PARAMETER ExePath
    Path to built ComicRack.exe
#>
param(
    [string]$ManifestPath = "",
    [string]$ResPath = "",
    [string]$ExePath = ""
)

$ErrorActionPreference = "Stop"
$StaleDpiAwarenessToken = 'dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">system<'
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

if ([string]::IsNullOrWhiteSpace($ManifestPath)) {
    $ManifestPath = Join-Path $repoRoot "ComicRack\app.manifest"
}
if ([string]::IsNullOrWhiteSpace($ResPath)) {
    $ResPath = Join-Path $repoRoot "ComicRack\myressources.res"
}
if ([string]::IsNullOrWhiteSpace($ExePath)) {
    $ExePath = Join-Path $repoRoot "ComicRack\bin\Debug\net48\ComicRack.exe"
}

function Test-TextContains {
    param(
        [string]$Path,
        [string]$Label,
        [string[]]$Required,
        [string[]]$Forbidden = @()
    )
    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Error "$Label not found: $Path"
    }
    $raw = [System.IO.File]::ReadAllText($Path)
    foreach ($token in $Required) {
        if ($raw -notmatch [regex]::Escape($token)) {
            Write-Error "$Label missing required token '$token' in $Path"
        }
    }
    foreach ($token in $Forbidden) {
        if ($raw -match [regex]::Escape($token)) {
            Write-Error "$Label contains forbidden token '$token' in $Path"
        }
    }
    Write-Host "OK $Label : $Path"
}

function Test-BinaryContains {
    param(
        [string]$Path,
        [string]$Label,
        [string]$Token,
        [string[]]$Forbidden = @()
    )
    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Error "$Label not found: $Path"
    }
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $ascii = [System.Text.Encoding]::ASCII.GetString($bytes)
    $unicode = [System.Text.Encoding]::Unicode.GetString($bytes)
    foreach ($forbiddenToken in $Forbidden) {
        if ($ascii -match [regex]::Escape($forbiddenToken) -or $unicode -match [regex]::Escape($forbiddenToken)) {
            Write-Error "$Label contains forbidden token '$forbiddenToken' in $Path"
        }
    }
    if ($ascii -notmatch [regex]::Escape($Token) -and $unicode -notmatch [regex]::Escape($Token)) {
        Write-Error "$Label missing '$Token' in binary $Path"
    }
    Write-Host "OK $Label binary contains '$Token': $Path"
}

function Test-ExeManifest {
    param(
        [string]$Path
    )
    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Error "EXE not found: $Path"
    }

    $mt = Get-Command mt.exe -ErrorAction SilentlyContinue
    if ($mt) {
        $outFile = Join-Path ([System.IO.Path]::GetTempPath()) ("crce-manifest-{0}.xml" -f [Guid]::NewGuid().ToString("N"))
        try {
            & $mt.Source -nologo -inputresource:"$Path;#1" -out:$outFile | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "mt.exe failed (exit $LASTEXITCODE); falling back to binary search"
            }
            else {
                Test-TextContains -Path $outFile -Label "EXE embedded manifest (mt)" -Required @("PerMonitorV2") -Forbidden @($StaleDpiAwarenessToken)
                return
            }
        }
        finally {
            Remove-Item -LiteralPath $outFile -ErrorAction SilentlyContinue
        }
    }

    Test-BinaryContains -Path $Path -Label "EXE embedded manifest (binary)" -Token "PerMonitorV2" -Forbidden @($StaleDpiAwarenessToken)
}

Write-Host "HiDPI manifest verification (SC-001)"
Write-Host "  Manifest: $ManifestPath"
Write-Host "  Res:      $ResPath"
Write-Host "  Exe:      $ExePath"

$staleDpiAwareness = $StaleDpiAwarenessToken

Test-TextContains -Path $ManifestPath -Label "Source app.manifest" -Required @("PerMonitorV2") -Forbidden @($staleDpiAwareness)
Test-BinaryContains -Path $ResPath -Label "Compiled myressources.res" -Token "PerMonitorV2" -Forbidden @($staleDpiAwareness)
Test-ExeManifest -Path $ExePath

Write-Host "SC-001 PASS: PerMonitorV2 verified in manifest pipeline"
exit 0
