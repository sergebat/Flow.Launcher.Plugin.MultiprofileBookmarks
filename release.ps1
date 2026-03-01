Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$projectPath = Join-Path $repoRoot "Flow.Launcher.Plugin.MultiprofileBookmarks/Flow.Launcher.Plugin.MultiprofileBookmarks.csproj"
$publishPath = Join-Path $repoRoot "Flow.Launcher.Plugin.MultiprofileBookmarks/bin/Release/win-x64/publish/*"
$zipPath = Join-Path $repoRoot "Flow.Launcher.Plugin.MultiprofileBookmarks/bin/MultiprofileBookmarks.zip"

dotnet publish $projectPath -c Release -r win-x64 --no-self-contained
Compress-Archive -Path $publishPath -DestinationPath $zipPath -Force
