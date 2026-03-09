# ===================================================================
# mfprobe / mfsr リリースビルドスクリプト
# ===================================================================

$version = "1.0.3"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " mfprobe / mfsr Release Build v$version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# ルートディレクトリを保存
$rootDir = Get-Location

# クリーンアップ
Write-Host "`n[1/6] クリーンアップ中..." -ForegroundColor Yellow
dotnet clean -c Release

# ビルド
Write-Host "`n[2/6] 発行中..." -ForegroundColor Yellow

# Windows (Native AOT)
Write-Host "  - Windows (Native AOT)..." -ForegroundColor Gray
Write-Host "    mfprobe..." -ForegroundColor DarkGray
dotnet publish .\mfprobe\mfprobe.csproj -c Release -r win-x64 -o ".\mfprobe\bin\Release\net10.0\publish\win-x64"
if ($LASTEXITCODE -ne 0) { 
    Write-Host "    エラー: mfprobe (win-x64) の発行に失敗しました" -ForegroundColor Red
    exit 1 
}

Write-Host "    mfsr..." -ForegroundColor DarkGray
dotnet publish .\mfsr\mfsr.csproj -c Release -r win-x64 -o ".\mfsr\bin\Release\net10.0\publish\win-x64"
if ($LASTEXITCODE -ne 0) { 
    Write-Host "    エラー: mfsr (win-x64) の発行に失敗しました" -ForegroundColor Red
    exit 1 
}

# macOS (Framework-Dependent)
Write-Host "  - macOS (Framework-Dependent)..." -ForegroundColor Gray
Write-Host "    mfprobe..." -ForegroundColor DarkGray
dotnet publish .\mfprobe\mfprobe.csproj -c Release -r osx-x64 --self-contained false /p:PublishAot=false -o ".\mfprobe\bin\Release\net10.0\publish\osx-x64"
if ($LASTEXITCODE -ne 0) { 
    Write-Host "    エラー: mfprobe (osx-x64) の発行に失敗しました" -ForegroundColor Red
    exit 1 
}

Write-Host "    mfsr..." -ForegroundColor DarkGray
dotnet publish .\mfsr\mfsr.csproj -c Release -r osx-x64 --self-contained false /p:PublishAot=false -o ".\mfsr\bin\Release\net10.0\publish\osx-x64"
if ($LASTEXITCODE -ne 0) { 
    Write-Host "    エラー: mfsr (osx-x64) の発行に失敗しました" -ForegroundColor Red
    exit 1 
}

# Linux (Framework-Dependent)
Write-Host "  - Linux (Framework-Dependent)..." -ForegroundColor Gray
Write-Host "    mfprobe..." -ForegroundColor DarkGray
dotnet publish .\mfprobe\mfprobe.csproj -c Release -r linux-x64 --self-contained false /p:PublishAot=false -o ".\mfprobe\bin\Release\net10.0\publish\linux-x64"
if ($LASTEXITCODE -ne 0) { 
    Write-Host "    エラー: mfprobe (linux-x64) の発行に失敗しました" -ForegroundColor Red
    exit 1 
}

Write-Host "    mfsr..." -ForegroundColor DarkGray
dotnet publish .\mfsr\mfsr.csproj -c Release -r linux-x64 --self-contained false /p:PublishAot=false -o ".\mfsr\bin\Release\net10.0\publish\linux-x64"
if ($LASTEXITCODE -ne 0) { 
    Write-Host "    エラー: mfsr (linux-x64) の発行に失敗しました" -ForegroundColor Red
    exit 1 
}

# リリースフォルダ作成
Write-Host "`n[3/6] リリースフォルダ作成中..." -ForegroundColor Yellow
$releaseDir = Join-Path $rootDir "release"
if (Test-Path $releaseDir) {
    Remove-Item $releaseDir -Recurse -Force
}
New-Item -ItemType Directory -Path $releaseDir | Out-Null

# tar.gz作成（macOS/Linux用）
Write-Host "`n[4/6] tar.gz アーカイブ作成中..." -ForegroundColor Yellow

# mfprobe-osx-x64
Write-Host "  - mfprobe-osx-x64-v$version.tar.gz" -ForegroundColor Gray
$sourceDir = Join-Path $rootDir "mfprobe\bin\Release\net10.0\publish\osx-x64"
if (-not (Test-Path $sourceDir)) {
    Write-Host "    エラー: $sourceDir が見つかりません" -ForegroundColor Red
    exit 1
}
$archivePath = Join-Path $releaseDir "mfprobe-osx-x64-v$version.tar.gz"
Push-Location $sourceDir
tar -czf $archivePath *
Pop-Location

# mfprobe-linux-x64
Write-Host "  - mfprobe-linux-x64-v$version.tar.gz" -ForegroundColor Gray
$sourceDir = Join-Path $rootDir "mfprobe\bin\Release\net10.0\publish\linux-x64"
if (-not (Test-Path $sourceDir)) {
    Write-Host "    エラー: $sourceDir が見つかりません" -ForegroundColor Red
    exit 1
}
$archivePath = Join-Path $releaseDir "mfprobe-linux-x64-v$version.tar.gz"
Push-Location $sourceDir
tar -czf $archivePath *
Pop-Location

# mfsr-osx-x64
Write-Host "  - mfsr-osx-x64-v$version.tar.gz" -ForegroundColor Gray
$sourceDir = Join-Path $rootDir "mfsr\bin\Release\net10.0\publish\osx-x64"
if (-not (Test-Path $sourceDir)) {
    Write-Host "    エラー: $sourceDir が見つかりません" -ForegroundColor Red
    exit 1
}
$archivePath = Join-Path $releaseDir "mfsr-osx-x64-v$version.tar.gz"
Push-Location $sourceDir
tar -czf $archivePath *
Pop-Location

# mfsr-linux-x64
Write-Host "  - mfsr-linux-x64-v$version.tar.gz" -ForegroundColor Gray
$sourceDir = Join-Path $rootDir "mfsr\bin\Release\net10.0\publish\linux-x64"
if (-not (Test-Path $sourceDir)) {
    Write-Host "    エラー: $sourceDir が見つかりません" -ForegroundColor Red
    exit 1
}
$archivePath = Join-Path $releaseDir "mfsr-linux-x64-v$version.tar.gz"
Push-Location $sourceDir
tar -czf $archivePath *
Pop-Location

# ZIP作成（Windows用）
Write-Host "`n[5/6] ZIP アーカイブ作成中..." -ForegroundColor Yellow

Write-Host "  - mfprobe-win-x64-v$version.zip" -ForegroundColor Gray
$sourceDir = Join-Path $rootDir "mfprobe\bin\Release\net10.0\publish\win-x64"
if (-not (Test-Path $sourceDir)) {
    Write-Host "    エラー: $sourceDir が見つかりません" -ForegroundColor Red
    exit 1
}
$archivePath = Join-Path $releaseDir "mfprobe-win-x64-v$version.zip"
Compress-Archive -Path "$sourceDir\*" -DestinationPath $archivePath -Force

Write-Host "  - mfsr-win-x64-v$version.zip" -ForegroundColor Gray
$sourceDir = Join-Path $rootDir "mfsr\bin\Release\net10.0\publish\win-x64"
if (-not (Test-Path $sourceDir)) {
    Write-Host "    エラー: $sourceDir が見つかりません" -ForegroundColor Red
    exit 1
}
$archivePath = Join-Path $releaseDir "mfsr-win-x64-v$version.zip"
Compress-Archive -Path "$sourceDir\*" -DestinationPath $archivePath -Force

# 完了
Write-Host "`n[6/6] 完了！" -ForegroundColor Green
Write-Host "`n作成されたファイル:" -ForegroundColor Cyan
Get-ChildItem -Path $releaseDir | ForEach-Object {
    $size = [math]::Round($_.Length / 1MB, 2)
    Write-Host "  - $($_.Name) ($size MB)" -ForegroundColor White
}

Write-Host "`nリリースディレクトリ: $((Get-Item $releaseDir).FullName)" -ForegroundColor Cyan
