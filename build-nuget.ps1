# ===================================================================
# mfprobe / mfsr NuGetパッケージビルドスクリプト
# ===================================================================

$version = "1.0.5"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " mfprobe / mfsr NuGet Package Build v$version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# ルートディレクトリを保存
$rootDir = Get-Location

# クリーンアップ
Write-Host "`n[1/5] クリーンアップ中..." -ForegroundColor Yellow
dotnet clean -c Release

# NuGetパッケージフォルダ作成
Write-Host "`n[2/5] NuGetフォルダ作成中..." -ForegroundColor Yellow
$nugetDir = Join-Path $rootDir "nuget-packages"
if (Test-Path $nugetDir) {
    Remove-Item $nugetDir -Recurse -Force
}
New-Item -ItemType Directory -Path $nugetDir | Out-Null

# mfprobe パッケージ作成
Write-Host "`n[3/5] mfprobe パッケージ作成中..." -ForegroundColor Yellow
dotnet pack .\mfprobe\mfprobe.csproj -c Release -o $nugetDir
if ($LASTEXITCODE -ne 0) { 
    Write-Host "    エラー: mfprobe のパッケージ作成に失敗しました" -ForegroundColor Red
    exit 1 
}

# mfsr パッケージ作成
Write-Host "`n[4/5] mfsr パッケージ作成中..." -ForegroundColor Yellow
dotnet pack .\mfsr\mfsr.csproj -c Release -o $nugetDir
if ($LASTEXITCODE -ne 0) { 
    Write-Host "    エラー: mfsr のパッケージ作成に失敗しました" -ForegroundColor Red
    exit 1 
}

# 完了
Write-Host "`n[5/5] 完了！" -ForegroundColor Green
Write-Host "`n作成されたパッケージ:" -ForegroundColor Cyan
Get-ChildItem -Path $nugetDir -Filter "*.nupkg" | ForEach-Object {
    $size = [math]::Round($_.Length / 1KB, 2)
    Write-Host "  - $($_.Name) ($size KB)" -ForegroundColor White
}

Write-Host "`nNuGetパッケージディレクトリ: $((Get-Item $nugetDir).FullName)" -ForegroundColor Cyan

# ローカルテストの方法を表示
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " ローカルテスト方法" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`n1. ローカルフィードからインストール:" -ForegroundColor Yellow
Write-Host "   dotnet tool install -g mfprobe --add-source $nugetDir" -ForegroundColor White
Write-Host "   dotnet tool install -g mfsr --add-source $nugetDir" -ForegroundColor White
Write-Host "`n2. アンインストール:" -ForegroundColor Yellow
Write-Host "   dotnet tool uninstall -g mfprobe" -ForegroundColor White
Write-Host "   dotnet tool uninstall -g mfsr" -ForegroundColor White
Write-Host "`n3. NuGet.org へ公開:" -ForegroundColor Yellow
Write-Host "   dotnet nuget push ""$nugetDir\mfprobe.$version.nupkg"" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor White
Write-Host "   dotnet nuget push ""$nugetDir\mfsr.$version.nupkg"" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor White
Write-Host ""

