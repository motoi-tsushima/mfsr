# ===================================================================
# mfprobe / mfsr NuGet.org 公開スクリプト
# ===================================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ApiKey
)

$version = "1.0.4"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " mfprobe / mfsr NuGet.org 公開 v$version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# ルートディレクトリとパッケージディレクトリを設定
$rootDir = Get-Location
$nugetDir = Join-Path $rootDir "nuget-packages"

# パッケージファイルの存在確認
$mfprobePackage = Join-Path $nugetDir "mfprobe.$version.nupkg"
$mfsrPackage = Join-Path $nugetDir "mfsr.$version.nupkg"

if (-not (Test-Path $mfprobePackage)) {
    Write-Host "`nエラー: mfprobe パッケージが見つかりません: $mfprobePackage" -ForegroundColor Red
    Write-Host "先に build-nuget.ps1 を実行してください。" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path $mfsrPackage)) {
    Write-Host "`nエラー: mfsr パッケージが見つかりません: $mfsrPackage" -ForegroundColor Red
    Write-Host "先に build-nuget.ps1 を実行してください。" -ForegroundColor Yellow
    exit 1
}

# 確認
Write-Host "`n以下のパッケージを NuGet.org に公開します:" -ForegroundColor Yellow
Write-Host "  - mfprobe $version" -ForegroundColor White
Write-Host "  - mfsr $version" -ForegroundColor White
Write-Host "`nよろしいですか？ (y/n): " -ForegroundColor Yellow -NoNewline
$confirmation = Read-Host

if ($confirmation -ne 'y') {
    Write-Host "`n公開をキャンセルしました。" -ForegroundColor Yellow
    exit 0
}

# mfprobe を公開
Write-Host "`n[1/2] mfprobe を公開中..." -ForegroundColor Yellow
dotnet nuget push $mfprobePackage --api-key $ApiKey --source https://api.nuget.org/v3/index.json
if ($LASTEXITCODE -ne 0) { 
    Write-Host "    エラー: mfprobe の公開に失敗しました" -ForegroundColor Red
    exit 1 
}

# mfsr を公開
Write-Host "`n[2/2] mfsr を公開中..." -ForegroundColor Yellow
dotnet nuget push $mfsrPackage --api-key $ApiKey --source https://api.nuget.org/v3/index.json
if ($LASTEXITCODE -ne 0) { 
    Write-Host "    エラー: mfsr の公開に失敗しました" -ForegroundColor Red
    exit 1 
}

# 完了
Write-Host "`n========================================" -ForegroundColor Green
Write-Host " 公開完了！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "`nパッケージが NuGet.org に公開されました。" -ForegroundColor White
Write-Host "数分後に以下のコマンドでインストール可能になります:" -ForegroundColor Yellow
Write-Host "`n  dotnet tool install -g mfprobe" -ForegroundColor Cyan
Write-Host "  dotnet tool install -g mfsr" -ForegroundColor Cyan
Write-Host "`nパッケージページ:" -ForegroundColor Yellow
Write-Host "  https://www.nuget.org/packages/mfprobe/" -ForegroundColor Cyan
Write-Host "  https://www.nuget.org/packages/mfsr/" -ForegroundColor Cyan
Write-Host ""

