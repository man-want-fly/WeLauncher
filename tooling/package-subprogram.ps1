param(
    [Parameter(Mandatory=$true)]
    [string]$SourcePath,
    [Parameter(Mandatory=$true)]
    [string]$OutputPath,
    [string]$EntryExe = ""
)

$ErrorActionPreference = "Stop"

$SourcePath = Resolve-Path $SourcePath
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot ".." )

# 1. Build Wrapper
Write-Host "Building Wrapper..."
$WrapperProj = Join-Path $RepoRoot "wrappers/MinimalWrapper/MinimalWrapper.csproj"
$WrapperBuildDir = Join-Path $RepoRoot "artifacts/wrapper_build"
dotnet publish $WrapperProj -c Release -o $WrapperBuildDir -r win-x64 --self-contained false /p:SingleFile=true

# 2. Prepare Temp Dir
$TempDir = Join-Path $RepoRoot "artifacts/temp_pkg_$(New-Guid)"
New-Item -ItemType Directory -Force -Path $TempDir | Out-Null
$AppDir = Join-Path $TempDir "app"
$WrapperDir = Join-Path $TempDir "wrapper"
$MetaDir = Join-Path $TempDir "meta"

New-Item -ItemType Directory -Force -Path $AppDir | Out-Null
New-Item -ItemType Directory -Force -Path $WrapperDir | Out-Null
New-Item -ItemType Directory -Force -Path $MetaDir | Out-Null

# 3. Copy Wrapper
Copy-Item (Join-Path $WrapperBuildDir "MinimalWrapper.exe") (Join-Path $WrapperDir "WrapperApp.exe")

# 4. Copy App Content
Copy-Item "$SourcePath\*" $AppDir -Recurse

# 5. Create Meta (Optional)
if (-not [string]::IsNullOrWhiteSpace($EntryExe)) {
    $metaJson = @{
        entryExe = $EntryExe
    } | ConvertTo-Json
    $metaJson | Out-File (Join-Path $MetaDir "app.json") -Encoding utf8
}

# 6. Zip
Write-Host "Zipping to $OutputPath..."
Compress-Archive -Path "$TempDir\*" -DestinationPath $OutputPath -Force

# Cleanup
Remove-Item $TempDir -Recurse -Force
Remove-Item $WrapperBuildDir -Recurse -Force
Write-Host "Done."
