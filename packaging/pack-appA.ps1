param(
  [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent
$outDir = Join-Path $root "dist/apps/appA/$Version"
$zipPath = Join-Path $outDir "appA-$Version.zip"

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

dotnet publish "$root/samples/AppA/AppA.csproj" -c Release -r win-x64 --self-contained false -o "$outDir/app"
dotnet publish "$root/wrappers/MinimalWrapper/MinimalWrapper.csproj" -c Release -r win-x64 --self-contained false -o "$outDir/wrapper"

New-Item -ItemType Directory -Force -Path "$outDir/meta" | Out-Null
@"
{
  "EntryExe": "App.exe",
  "Version": "$Version",
  "Icon": ""
}
"@ | Set-Content "$outDir/meta/app.json" -Encoding UTF8

if (Test-Path "$outDir/wrapper/MinimalWrapper.exe") {
  Rename-Item "$outDir/wrapper/MinimalWrapper.exe" -NewName "WrapperApp.exe" -Force
}

Compress-Archive -Path "$outDir/app","$outDir/wrapper","$outDir/meta" -DestinationPath $zipPath -Force

$sha256 = (Get-FileHash -Algorithm SHA256 -Path $zipPath).Hash.ToLower()
Write-Host "Zip: $zipPath"
Write-Host "SHA256: $sha256"
