
Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap 64, 64
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.Clear([System.Drawing.Color]::Blue)
$bmp.Save("icon.png", [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host "Icon created successfully"
