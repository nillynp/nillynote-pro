# ============================================================================
# NillyNote Pro — Universal Windows 10/11 Installer (Zero Admin · Portable)
# created by Nilly graphics (Sotiris Christos) — https://nillygraphics.com/
# Ultra-Light Native Windows 11 Pro Notepad (Tabs, Auto-Session, Pin & Tools)
# ============================================================================
param([switch]$Silent)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$exePath = Join-Path $root 'NillyNotePro.exe'
$icoPath = Join-Path $root 'NillyNotePro.ico'
$srcPath = Join-Path $root 'desktop\NillyNotePro.cs'

Write-Host ''
Write-Host '  ============================================================' -ForegroundColor Cyan
Write-Host '   NillyNote Pro — Ultra-Light Windows 11 Pro Notepad' -ForegroundColor White
Write-Host '   created by Nilly graphics (Sotiris Christos)' -ForegroundColor DarkGray
Write-Host '   https://nillygraphics.com/' -ForegroundColor DarkGray
Write-Host '  ============================================================' -ForegroundColor Cyan
Write-Host ''

# 1. Unblock files if extracted from GitHub Portable ZIP
Get-ChildItem -Path $root -Recurse -File -ErrorAction SilentlyContinue | Unblock-File -ErrorAction SilentlyContinue

# 2. Auto-compile NillyNotePro.exe if missing or older than source
$needBuild = (-not (Test-Path $exePath)) -or ((Test-Path $srcPath) -and ((Get-Item $srcPath).LastWriteTimeUtc -gt (Get-Item $exePath).LastWriteTimeUtc))
if ($needBuild) {
    $fxDir = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
    if (-not (Test-Path "$fxDir\csc.exe")) { $fxDir = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319' }
    $csc = Join-Path $fxDir 'csc.exe'
    if (-not (Test-Path $csc)) { throw 'Δεν βρέθηκε ο .NET Framework 4.x C# compiler (csc.exe).' }
    Write-Host '  [1/3] Μεταγλώττιση NillyNotePro.exe (100% Native Windows 11 Pro Notepad)...' -ForegroundColor Yellow
    $cscArgs = @(
        '/nologo', '/target:winexe', '/optimize+', '/utf8output', '/platform:anycpu', "/out:$exePath",
        '/r:System.dll', '/r:System.Core.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll'
    )
    if (Test-Path $icoPath) { $cscArgs += "/win32icon:$icoPath" }
    $cscArgs += $srcPath
    & $csc @cscArgs
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path $exePath)) {
        throw "Αποτυχία μεταγλώττισης NillyNotePro.exe (exit $LASTEXITCODE)"
    }
    Write-Host ("        OK -> " + [math]::Round((Get-Item $exePath).Length / 1KB, 1) + " KB") -ForegroundColor Green
} else {
    Write-Host '  [1/3] NillyNotePro.exe έτοιμο.' -ForegroundColor Green
}

# 3. Safe Text & Notes File Associations (Zero Admin · HKCU)
#    Strictly safe text formats ONLY (NEVER executable scripts like .bat/.cmd/.vbs/.reg!)
Write-Host '  [2/3] Εγγραφή τύπων αρχείων κειμένου & Default Apps (Zero Admin)...' -ForegroundColor Yellow
$iconVal = if (Test-Path $icoPath) { "$icoPath,0" } else { "$exePath,0" }
$progId = 'NillyNotePro.Document'
$exts = @('.txt', '.log', '.nfo', '.md', '.markdown', '.srt', '.sub', '.csv', '.tsv', '.ini', '.cfg', '.conf', '.env', '.cue', '.m3u', '.m3u8')

New-Item -Path "HKCU:\Software\Classes\$progId\DefaultIcon" -Force | Out-Null
New-Item -Path "HKCU:\Software\Classes\$progId\shell\open\command" -Force | Out-Null
Set-ItemProperty -Path "HKCU:\Software\Classes\$progId" -Name '(Default)' -Value 'NillyNote Pro Text Document'
Set-ItemProperty -Path "HKCU:\Software\Classes\$progId\DefaultIcon" -Name '(Default)' -Value $iconVal
Set-ItemProperty -Path "HKCU:\Software\Classes\$progId\shell\open\command" -Name '(Default)' -Value "`"$exePath`" `"%1`""

$appKey = 'HKCU:\Software\Classes\Applications\NillyNotePro.exe'
New-Item -Path "$appKey\DefaultIcon" -Force | Out-Null
New-Item -Path "$appKey\shell\open\command" -Force | Out-Null
New-Item -Path "$appKey\SupportedTypes" -Force | Out-Null
Set-ItemProperty -Path $appKey -Name 'FriendlyAppName' -Value 'NillyNote Pro'
Set-ItemProperty -Path "$appKey\DefaultIcon" -Name '(Default)' -Value $iconVal
Set-ItemProperty -Path "$appKey\shell\open\command" -Name '(Default)' -Value "`"$exePath`" `"%1`""

$capBase = 'HKCU:\Software\Nilly graphics\NillyNotePro\Capabilities'
New-Item -Path "$capBase\FileAssociations" -Force | Out-Null
New-ItemProperty -Path $capBase -Name 'ApplicationName' -Value 'NillyNote Pro' -PropertyType String -Force | Out-Null
New-ItemProperty -Path $capBase -Name 'ApplicationDescription' -Value 'Ultra-Light Windows 11 Pro Notepad (Tabs, Auto-Session, Pin & Text Tools) by Nilly graphics' -PropertyType String -Force | Out-Null

foreach ($ext in $exts) {
    New-Item -Path "HKCU:\Software\Classes\$ext\OpenWithProgids" -Force | Out-Null
    New-ItemProperty -Path "HKCU:\Software\Classes\$ext\OpenWithProgids" -Name $progId -Value '' -PropertyType String -Force | Out-Null
    New-ItemProperty -Path "$appKey\SupportedTypes" -Name $ext -Value '' -PropertyType String -Force | Out-Null
    New-ItemProperty -Path "$capBase\FileAssociations" -Name $ext -Value $progId -PropertyType String -Force | Out-Null
}

New-Item -Path 'HKCU:\Software\RegisteredApplications' -Force | Out-Null
New-ItemProperty -Path 'HKCU:\Software\RegisteredApplications' -Name 'NillyNote Pro' -Value 'Software\Nilly graphics\NillyNotePro\Capabilities' -PropertyType String -Force | Out-Null

# Explorer right-click menu on files: "Άνοιγμα με NillyNote Pro" (via reg.exe so literal '*' is never expanded as wildcard!)
& reg.exe add "HKCU\Software\Classes\*\shell\NillyNotePro" /ve /t REG_SZ /d "Άνοιγμα με NillyNote Pro" /f | Out-Null
& reg.exe add "HKCU\Software\Classes\*\shell\NillyNotePro" /v "Icon" /t REG_SZ /d "$iconVal" /f | Out-Null
& reg.exe add "HKCU\Software\Classes\*\shell\NillyNotePro\command" /ve /t REG_SZ /d "`"$exePath`" `"%1`"" /f | Out-Null
Write-Host ("        OK -> Εγγράφηκαν " + $exts.Count + " formats κειμένου & δεξί κλικ Explorer!") -ForegroundColor Green

# 4. Desktop & Start Menu Shortcuts
Write-Host '  [3/3] Δημιουργία συντομεύσεων (Επιφάνεια Εργασίας & Start Menu)...' -ForegroundColor Yellow
$wsh = New-Object -ComObject WScript.Shell
$desktopDir = [Environment]::GetFolderPath('Desktop')
$startMenuDir = Join-Path ([Environment]::GetFolderPath('Programs')) 'Nilly graphics'
New-Item -ItemType Directory -Force -Path $startMenuDir | Out-Null

foreach ($lnkPath in @(
    (Join-Path $desktopDir 'NillyNote Pro.lnk'),
    (Join-Path $startMenuDir 'NillyNote Pro.lnk')
)) {
    $sc = $wsh.CreateShortcut($lnkPath)
    $sc.TargetPath = $exePath
    $sc.WorkingDirectory = $root
    $sc.Description = 'NillyNote Pro — Ultra-Light Native Windows 11 Pro Notepad | created by Nilly graphics'
    if (Test-Path $icoPath) { $sc.IconLocation = "$icoPath,0" }
    $sc.Save()
}
Write-Host '        OK -> Έτοιμες οι συντομεύσεις!' -ForegroundColor Green
Write-Host ''
Write-Host '  ΟΛΟΚΛΗΡΩΘΗΚΕ! Άνοιξε το NillyNote Pro από Desktop, Start Menu ή από το «Άνοιγμα με» στον Explorer.' -ForegroundColor Cyan
Write-Host ''

if (-not $Silent) {
    Start-Process -FilePath $exePath
}
