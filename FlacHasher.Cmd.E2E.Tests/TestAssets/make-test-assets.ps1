# Generates a "pure" WAV file and encodes it to <name>.flac and <name>.ape.
# The WAV is a minimal, pure-PCM file (RIFF/WAVE + fmt + data chunks only)
# - no metadata, tags  or padding; so a lossless round-trip decode reproduces it byte-for-byte
# and the hash computed on the file is as good as it gets.
# Computes an MD5 hash on the original WAV file and prints it.
#
# Usage: .\make-test-assets.ps1 -FlacEncoderPath 'C:\path\to\flac.exe' -ApeEncoderPath 'C:\path\to\MAC.exe'

param(
    [string]$FlacEncoderPath,
    [string]$ApeEncoderPath,
    [int]$Frequency = 440,
    [double]$DurationSec = 1,
    [string]$Filename = 'sample'
)

$ErrorActionPreference = 'Stop'

if (-not $FlacEncoderPath -and -not $ApeEncoderPath) {
    throw "At least one of -FlacEncoderPath or -ApeEncoderPath must be provided."
}

$sampleRate = 44100
$channels = 2
$bitsPerSample = 16
$durationSeconds = $DurationSec
$frequency = $Frequency

$assetsDir = $PSScriptRoot

$wavPath = Join-Path $assetsDir "$Filename-source.wav"
$flacPath = Join-Path $assetsDir "$Filename.flac"
$apePath = Join-Path $assetsDir "$Filename.ape"

$frameCount = [int]($sampleRate * $durationSeconds)
$blockAlign = $channels * ($bitsPerSample / 8)
$dataSize = $frameCount * $blockAlign

$stream = [System.IO.File]::Create($wavPath)
try {
    $writer = New-Object System.IO.BinaryWriter($stream)

    $writer.Write([System.Text.Encoding]::ASCII.GetBytes('RIFF'))
    $writer.Write([int](36 + $dataSize))
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes('WAVE'))
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes('fmt '))
    $writer.Write([int]16)
    $writer.Write([int16]1)                                    # PCM
    $writer.Write([int16]$channels)
    $writer.Write([int]$sampleRate)
    $writer.Write([int]($sampleRate * $blockAlign))
    $writer.Write([int16]$blockAlign)
    $writer.Write([int16]$bitsPerSample)
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes('data'))
    $writer.Write([int]$dataSize)

    for ($i = 0; $i -lt $frameCount; $i++) {
        $sample = [int16]([Math]::Round([Math]::Sin(2 * [Math]::PI * $frequency * $i / $sampleRate) * 20000))
        for ($c = 0; $c -lt $channels; $c++) {
            $writer.Write($sample)
        }
    }

    $writer.Flush()
}
finally {
    $stream.Dispose()
}

Write-Host "Created $wavPath ($((Get-Item $wavPath).Length) bytes)"

$hash = (Get-FileHash -Path $wavPath -Algorithm MD5).Hash.ToLowerInvariant()
Write-Host "expectedMd5 = `"$hash`""

if ($FlacEncoderPath) {
    Remove-Item $flacPath -ErrorAction SilentlyContinue
    & $FlacEncoderPath --best --silent --output-name=$flacPath $wavPath
    if ($LASTEXITCODE -ne 0) { throw "flac encoding failed with exit code $LASTEXITCODE" }

    Write-Host "Created $flacPath ($((Get-Item $flacPath).Length) bytes)"
}

if ($ApeEncoderPath) {
    Remove-Item $apePath -ErrorAction SilentlyContinue
    & $ApeEncoderPath $wavPath $apePath '-c2000'
    if ($LASTEXITCODE -ne 0) { throw "Monkey's Audio encoding failed with exit code $LASTEXITCODE" }

    Write-Host "Created $apePath ($((Get-Item $apePath).Length) bytes)"
}
