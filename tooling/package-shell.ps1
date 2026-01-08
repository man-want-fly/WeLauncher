param(
    [string]$ManifestJson = "",
    [string]$ManifestPath = "",
    [Parameter(Mandatory=$true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot ".." )

# 1. Build Shell
Write-Host "Building Shell..."
$ShellProj = Join-Path $RepoRoot "src/WeLauncher/WeLauncher.csproj"
$ShellBuildDir = Join-Path $RepoRoot "artifacts/shell_build"
dotnet publish $ShellProj -c Release -o $ShellBuildDir -r win-x64 --self-contained false /p:SingleFile=true

# 2. Prepare Manifest
if (-not [string]::IsNullOrWhiteSpace($ManifestJson)) {
    $ManifestJson | Out-File (Join-Path $ShellBuildDir "manifest.json") -Encoding utf8
}
elseif (-not [string]::IsNullOrWhiteSpace($ManifestPath)) {
    Copy-Item $ManifestPath (Join-Path $ShellBuildDir "manifest.json")
}
else {
    # Default sample if nothing provided
    $Sample = Join-Path $RepoRoot "manifests/manifest.sample.json"
    if (Test-Path $Sample) {
        Copy-Item $Sample (Join-Path $ShellBuildDir "manifest.json")
    }
}

# 3. Zip
Write-Host "Zipping to $OutputPath..."
Compress-Archive -Path "$ShellBuildDir\*" -DestinationPath $OutputPath -Force

# Cleanup
Remove-Item $ShellBuildDir -Recurse -Force
Write-Host "Done."
