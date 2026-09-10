# Creates a corrupt FLAC by keeping only the first half of a valid one.
# Unlike other corrupt inputs, this one makes the decoder produce audio before it fails,
# which is what makes it possible to detect a hash computed over partial input.
#
# Usage: .\make-truncated-asset.ps1

param(
    [string]$SourceFileName = 'sample.flac',
    [string]$OutputFileName = 'sample-truncated.flac'
)

$ErrorActionPreference = 'Stop'

$assetsDir = $PSScriptRoot
$sourcePath = Join-Path $assetsDir $SourceFileName
$targetPath = Join-Path $assetsDir $OutputFileName

if (-not (Test-Path $sourcePath)) {
    throw "Source file not found: $sourcePath. Generate it with make-test-assets.ps1 first."
}

$bytes = [System.IO.File]::ReadAllBytes($sourcePath)
$halfLength = [int]($bytes.Length / 2)

[System.IO.File]::WriteAllBytes($targetPath, $bytes[0..($halfLength - 1)])

Write-Host "Created $targetPath ($halfLength bytes out of $($bytes.Length))"
