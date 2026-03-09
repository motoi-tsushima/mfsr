# mfprobe / mfsr - 配布・インストールガイド

## 📥 ダウンロード

最新リリースは [Releases](https://github.com/motoi-tsushima/mfsr/releases) からダウンロードできます。

### プラットフォーム別ダウンロード

| プラットフォーム | ファイル | .NET Runtime | ファイルサイズ |
|----------------|---------|--------------|---------------|
| **Windows 64bit** | `mfprobe-win-x64-vX.X.X.zip`<br>`mfsr-win-x64-vX.X.X.zip` | ❌ **不要**（Native AOT） | ~15MB |
| **macOS Intel/ARM** | `mfprobe-osx-x64-vX.X.X.tar.gz`<br>`mfsr-osx-x64-vX.X.X.tar.gz` | ✅ **必要** | ~500KB |
| **Linux 64bit** | `mfprobe-linux-x64-vX.X.X.tar.gz`<br>`mfsr-linux-x64-vX.X.X.tar.gz` | ✅ **必要** | ~500KB |

---

## 🔧 前提条件

### Windows
- ✅ **不要**（すべて含まれています）

### macOS / Linux
- ✅ **.NET 10 Runtime** のインストールが必要です
  - [公式ダウンロード](https://dotnet.microsoft.com/download/dotnet/10.0)

---

## 📦 インストール方法

### Windows

1. ZIPファイルをダウンロード
2. 任意の場所に展開
3. `mfprobe.exe` または `mfsr.exe` を実行

```cmd
mfprobe.exe --version
mfsr.exe --version
```

### macOS

```bash
# .NET 10 Runtimeをインストール（初回のみ）
brew install dotnet-sdk

# ダウンロードしたファイルを展開
tar -xzf mfprobe-osx-x64-v1.0.3.tar.gz

# 実行権限を付与
chmod +x mfprobe

# 実行
./mfprobe --version

# (オプション) システム全体で使えるようにする
sudo cp mfprobe /usr/local/bin/
sudo cp *.dll /usr/local/bin/
```

### Linux

```bash
# .NET 10 Runtimeをインストール（初回のみ）
# Ubuntu/Debian
sudo apt-get install dotnet-runtime-10.0

# ダウンロードしたファイルを展開
tar -xzf mfprobe-linux-x64-v1.0.3.tar.gz

# 実行権限を付与
chmod +x mfprobe

# 実行
./mfprobe --version

# (オプション) システム全体で使えるようにする
sudo cp mfprobe /usr/local/bin/
sudo cp *.dll /usr/local/bin/
```

---

## 🚀 使用例

### mfprobe（ファイル検索）

```bash
# カレントディレクトリから検索
mfprobe -s "検索文字列"

# 特定のディレクトリを検索
mfprobe -s "検索文字列" -d /path/to/directory

# ファイル拡張子を指定
mfprobe -s "検索文字列" -d /path/to/directory -e "*.cs,*.txt"
```

### mfsr（文字列置換）

```bash
# カレントディレクトリで置換
mfsr -s "検索文字列" -r "置換文字列"

# 特定のディレクトリで置換
mfsr -s "検索文字列" -r "置換文字列" -d /path/to/directory

# 正規表現を使用
mfsr -s "pattern" -r "replacement" -d /path/to/directory --regex
```

---

## ❓ トラブルシューティング

### macOS: "開発元が未確認のため開けません"

```bash
# セキュリティ設定で許可
xattr -d com.apple.quarantine mfprobe
```

### macOS/Linux: "Permission denied"

```bash
# 実行権限を付与
chmod +x mfprobe
chmod +x mfsr
```

### macOS/Linux: ".NET Runtimeが見つかりません"

```bash
# .NET 10 Runtimeがインストールされているか確認
dotnet --list-runtimes

# インストールされていない場合
# macOS
brew install dotnet-sdk

# Linux (Ubuntu/Debian)
sudo apt-get install dotnet-runtime-10.0
```

---

## 📄 ライセンス

MIT License - 詳細は [LICENSE](LICENSE) を参照してください。

---

## 🤝 コントリビューション

プルリクエストを歓迎します！詳細は [CONTRIBUTING.md](CONTRIBUTING.md) を参照してください。
