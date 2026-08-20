# Generates sample.flac next to this script, then prints the MD5 of the
# decoded output so it can be pinned as `expectedMd5` in Hashing_Tests.
#
# Usage: .\make-test-assets.ps1 -Flac 'C:\path\to\flac.exe'

param(
    [Parameter(Mandatory = $true)]
    [string]$Flac
)

$ErrorActionPreference = 'Stop'

$sampleRate = 44100
$channels = 2
$bitsPerSample = 16
$durationSeconds = 0.5
$frequency = 440

$assetsDir = $PSScriptRoot

$wavPath = Join-Path ([System.IO.Path]::GetTempPath()) 'flachash-sample.wav'
$flacPath = Join-Path $assetsDir 'sample.flac'

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

Remove-Item $flacPath -ErrorAction SilentlyContinue
& $Flac --best --silent --output-name=$flacPath $wavPath
if ($LASTEXITCODE -ne 0) { throw "flac encoding failed with exit code $LASTEXITCODE" }
Remove-Item $wavPath

Write-Host "Created $flacPath ($((Get-Item $flacPath).Length) bytes)"

# The app hashes the decoder's stdout and feeds it the file via stdin, so the expected
# value has to be produced exactly the same way
$decodedPath = Join-Path ([System.IO.Path]::GetTempPath()) 'flachash-decoded.wav'
& cmd.exe /c "`"$Flac`" --decode --silent - < `"$flacPath`" > `"$decodedPath`""
if ($LASTEXITCODE -ne 0) { throw "flac decoding failed with exit code $LASTEXITCODE" }

$hash = (Get-FileHash -Path $decodedPath -Algorithm MD5).Hash.ToLowerInvariant()
Remove-Item $decodedPath

Write-Host "expectedMd5 = `"$hash`""
