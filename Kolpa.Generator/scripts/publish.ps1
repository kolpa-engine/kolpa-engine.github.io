# Publishes the generator as single-file self-contained executables into .\bins\<rid>.
# Run from the repo root. Targets Windows, macOS, and Linux (incl. Android/Termux arm64).
#
#   powershell -ExecutionPolicy Bypass -File Kolpa.Generator\scripts\publish.ps1          # all
#   powershell -ExecutionPolicy Bypass -File Kolpa.Generator\scripts\publish.ps1 linux-x64 # one
#   powershell -ExecutionPolicy Bypass -File Kolpa.Generator\scripts\publish.ps1 linux-x64,linux-arm64
#   (pass multiple RIDs comma-separated; note: `-File` binds only the first unquoted space-separated arg)

param(
    [string[]]$Rid
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$proj = Join-Path $root "Kolpa.Generator\Kolpa.Generator.csproj"
$out = Join-Path $root "bins"

if (-not $Rid -or $Rid.Count -eq 0) {
    $Rid = @("win-x64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64")
}

foreach ($r in $Rid) {
    $dest = Join-Path $out $r
    Write-Host ">> Publishing $r -> $dest"
    dotnet publish $proj -c Release -r $r --self-contained true -o $dest `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -p:DebugType=embedded `
        -p:InvariantGlobalization=true `
        -p:UseAppHost=true
}

Write-Host ">> Done."
Get-ChildItem (Join-Path $out "*") -Recurse -Filter "Kolpa.Generator*" -File | ForEach-Object { Write-Host $_.FullName }
