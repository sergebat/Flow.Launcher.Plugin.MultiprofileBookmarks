# Build the plugin
dotnet publish Flow.Launcher.Plugin.MultiprofileBookmarks -c Debug -r win-x64 --no-self-contained

# Define paths
$AppDataFolder = [Environment]::GetFolderPath("ApplicationData")
$FlowLauncherPlugins = "$AppDataFolder\FlowLauncher\Plugins"
$PluginName = "MultiprofileBookmarks"
$PluginPath = "$FlowLauncherPlugins\$PluginName"
$PublishPath = "Flow.Launcher.Plugin.MultiprofileBookmarks\bin\Debug\win-x64\publish"
$FlowLauncherExe = "$env:LOCALAPPDATA\FlowLauncher\Flow.Launcher.exe"

# 1. Kill Flow Launcher if running
$flowProcess = Get-Process -Name "Flow.Launcher" -ErrorAction SilentlyContinue
if ($flowProcess) {
    Write-Host "Stopping Flow Launcher..."
    Stop-Process -Name "Flow.Launcher" -Force
    Start-Sleep -Seconds 2
}

# 2. Cleanup old plugin version
if (Test-Path $PluginPath) {
    Write-Host "Removing old plugin version..."
    Remove-Item -Recurse -Force $PluginPath
}

# 3. Cleanup any leftover publish folder in Plugins (from failed runs)
if (Test-Path "$FlowLauncherPlugins\publish") {
    Write-Host "Removing leftover publish folder..."
    Remove-Item -Recurse -Force "$FlowLauncherPlugins\publish"
}

# 4. Copy new build
Write-Host "Deploying new version..."
Copy-Item $PublishPath "$FlowLauncherPlugins\" -Recurse -Force
Rename-Item -Path "$FlowLauncherPlugins\publish" -NewName $PluginName

# 5. Restart Flow Launcher
if (Test-Path $FlowLauncherExe) {
    Write-Host "Starting Flow Launcher..."
    Start-Process $FlowLauncherExe
}
else {
    Write-Warning "Flow.Launcher.exe not found at $FlowLauncherExe. Please start it manually."
}
