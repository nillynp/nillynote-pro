# ============================================================================
# NillyNote Pro — Clean Uninstaller (Zero Admin)
# created by Nilly graphics (Sotiris Christos) — https://nillygraphics.com/
# ============================================================================
param([switch]$Silent)

$ErrorActionPreference = 'SilentlyContinue'
Write-Host ''
Write-Host '  Αφαίρεση καταχωρίσεων & συντομεύσεων του NillyNote Pro...' -ForegroundColor Yellow

Stop-Process -Name 'NillyNotePro' -Force -ErrorAction SilentlyContinue

$desktopLnk = Join-Path ([Environment]::GetFolderPath('Desktop')) 'NillyNote Pro.lnk'
$startLnk = Join-Path ([Environment]::GetFolderPath('Programs')) 'Nilly graphics\NillyNote Pro.lnk'
if (Test-Path $desktopLnk) { Remove-Item $desktopLnk -Force }
if (Test-Path $startLnk) { Remove-Item $startLnk -Force }

$exts = @('.txt', '.log', '.nfo', '.md', '.markdown', '.srt', '.sub', '.csv', '.tsv', '.ini', '.cfg', '.conf', '.env', '.cue', '.m3u', '.m3u8')
foreach ($ext in $exts) {
    Remove-ItemProperty -Path "HKCU:\Software\Classes\$ext\OpenWithProgids" -Name 'NillyNotePro.Document' -ErrorAction SilentlyContinue
}

foreach ($rk in @(
    'HKCU:\Software\Classes\NillyNotePro.Document',
    'HKCU:\Software\Classes\Applications\NillyNotePro.exe',
    'HKCU:\Software\Nilly graphics\NillyNotePro'
)) {
    if (Test-Path $rk) { Remove-Item -Path $rk -Recurse -Force -ErrorAction SilentlyContinue }
}
& reg.exe delete "HKCU\Software\Classes\*\shell\NillyNotePro" /f 2>$null | Out-Null
Remove-ItemProperty -Path 'HKCU:\Software\RegisteredApplications' -Name 'NillyNote Pro' -ErrorAction SilentlyContinue

Write-Host '  OK -> Αφαιρέθηκαν καθαρά όλες οι εγγραφές & συντομεύσεις του NillyNote Pro.' -ForegroundColor Green
Write-Host ''
if (-not $Silent) { Read-Host '  Πάτησε Enter για κλείσιμο' }
