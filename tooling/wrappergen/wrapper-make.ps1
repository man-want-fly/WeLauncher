param(
  [Parameter(Mandatory=$true)][string]$AppId,
  [Parameter(Mandatory=$true)][string]$Version,
  [Parameter(Mandatory=$true)][string]$PayloadDir,
  [string]$EntryExe = "App.exe",
  [string[]]$Args = @(),
  [hashtable]$Env = @{},
  [string]$Name = "",
  [string]$BaseUrl = "",
  [bool]$Visible = $true,
  [string]$OutputRoot = ""
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $PayloadDir)) { throw "PayloadDir not found: $PayloadDir" }
if (-not $OutputRoot -or $OutputRoot -eq "") {
  $root = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent
  $OutputRoot = Join-Path $root "dist/apps/$AppId/$Version"
}

New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null
New-Item -ItemType Directory -Force -Path "$OutputRoot/app" | Out-Null
New-Item -ItemType Directory -Force -Path "$OutputRoot/wrapper" | Out-Null
New-Item -ItemType Directory -Force -Path "$OutputRoot/meta" | Out-Null

Copy-Item -Recurse -Force "$PayloadDir/*" "$OutputRoot/app/"

$meta = @{
  EntryExe = $EntryExe
  Version = $Version
  Icon = ""
}
$meta | ConvertTo-Json | Set-Content "$OutputRoot/meta/app.json" -Encoding UTF8

$root = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent
dotnet publish "$root/tooling/wrappergen/WrapperTemplate.csproj" -c Release -r win-x64 --self-contained false -o "$OutputRoot/wrapper"
if (Test-Path "$OutputRoot/wrapper/WrapperTemplate.exe") {
  Rename-Item "$OutputRoot/wrapper/WrapperTemplate.exe" -NewName "WrapperApp.exe" -Force
}

$zipPath = Join-Path $OutputRoot "$AppId-$Version.zip"
Compress-Archive -Path "$OutputRoot/app","$OutputRoot/wrapper","$OutputRoot/meta" -DestinationPath $zipPath -Force
$sha256 = (Get-FileHash -Algorithm SHA256 -Path $zipPath).Hash.ToLower()

Write-Host "Zip: $zipPath"
Write-Host "SHA256: $sha256"

# Manifest fragment
$distManifests = Join-Path ($root) "dist/manifests"
New-Item -ItemType Directory -Force -Path $distManifests | Out-Null
$fragPath = Join-Path $distManifests "$AppId-$Version.json"
$downloadUrl =
  (if ($BaseUrl -and $BaseUrl -ne "") {
      if ($BaseUrl.EndsWith("/")) { "$BaseUrl" } else { "$BaseUrl/" }
    } else { "" }) + "apps/$AppId/$Version/$AppId-$Version.zip"

if (-not $Name -or $Name -eq "") { $Name = $AppId }
$fragment = @{
  id = $AppId
  name = $Name
  version = $Version
  downloadUrl = $downloadUrl
  sha256 = $sha256
  visible = $Visible
  launchArgs = $Args
  env = $Env
  wrapperRelativePath = "wrapper/WrapperApp.exe"
}
$fragment | ConvertTo-Json -Depth 5 | Set-Content $fragPath -Encoding UTF8

Write-Host "Manifest fragment: $fragPath"
Write-Host "DownloadUrl: $downloadUrl"
