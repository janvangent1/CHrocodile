# Build script for creating CHRocodile GUI executable
# Run this script to create a single-file .exe

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CHRocodile GUI - Build Executable" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if virtual environment is activated
$venvActive = $env:VIRTUAL_ENV -ne $null
if (-not $venvActive) {
    Write-Host "Activating virtual environment..." -ForegroundColor Yellow
    & ".\venv\Scripts\Activate.ps1"
}

# Check if PyInstaller is installed
try {
    python -c "import PyInstaller" 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Installing PyInstaller..." -ForegroundColor Yellow
        pip install pyinstaller
    }
} catch {
    Write-Host "Installing PyInstaller..." -ForegroundColor Yellow
    pip install pyinstaller
}

Write-Host ""
Write-Host "Building executable..." -ForegroundColor Green
Write-Host ""

# Build using spec file
python -m PyInstaller CHRocodileGUI.spec --clean

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Build failed! Trying alternative method..." -ForegroundColor Yellow
    python build_exe.py
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Executable location: dist\CHRocodileGUI.exe" -ForegroundColor Green
Write-Host ""
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
