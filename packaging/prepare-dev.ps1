param(
  [string]$Version = "1.0.0",
  [int]$Port = 8000
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent
$zipPath = Join-Path $root "dist/apps/appA/$Version/appA-$Version.zip"
if (!(Test-Path $zipPath)) { throw "Zip not found: $zipPath" }
$sha256 = (Get-FileHash -Algorithm SHA256 -Path $zipPath).Hash.ToLower()

$distManifests = Join-Path $root "dist/manifests"
New-Item -ItemType Directory -Force -Path $distManifests | Out-Null
$manifestPath = Join-Path $distManifests "dev.manifest.json"

$downloadUrl = "http://localhost:$Port/apps/appA/$Version/appA-$Version.zip"
@"
{
  "schemaVersion": 1,
  "apps": [
    {
      "id": "appA",
      "name": "示例应用A",
      "version": "$Version",
      "downloadUrl": "$downloadUrl",
      "sha256": "$sha256",
      "visible": true,
      "launchArgs": ["--mode=prod"],
      "env": { "APP_ENV": "prod" },
      "wrapperRelativePath": "wrapper/WrapperApp.exe"
    }
  ]
}
"@ | Set-Content $manifestPath -Encoding UTF8

Write-Host "Manifest: $manifestPath"
Write-Host "DownloadUrl: $downloadUrl"
Write-Host "SHA256: $sha256"

