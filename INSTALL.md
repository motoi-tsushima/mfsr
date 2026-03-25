# mfprobe / mfsr - 配布・インストールガイド

## 🚀 推奨インストール方法（.NET Tool）

**全プラットフォーム対応 - 最も簡単なインストール方法です。**

```bash
# インストール（Windows / macOS / Linux 共通）
dotnet tool install -g mfprobe
dotnet tool install -g mfsr

# アップデート
dotnet tool update -g mfprobe
dotnet tool update -g mfsr

# アンインストール
dotnet tool uninstall -g mfprobe
dotnet tool uninstall -g mfsr
```

### 前提条件
- ✅ **.NET 10 SDK または Runtime** が必要
  - [公式ダウンロード](https://dotnet.microsoft.com/download/dotnet/10.0)

---

## 📥 その他のインストール方法

### Windows: winget（Native AOT版）

**.NET Runtime 不要**の高速実行ファイルをインストールできます。

```cmd
winget install motoi.tsushima.mfprobe
winget install motoi.tsushima.mfsr
```

### 手動インストール

最新リリースは [Releases](https://github.com/motoi-tsushima/mfsr/releases) からダウンロードできます。

#### プラットフォーム別ダウンロード

| プラットフォーム | ファイル | .NET Runtime | ファイルサイズ |
|----------------|---------|--------------|---------------|
| **Windows 64bit** | `mfprobe-win-x64-vX.X.X.zip`<br>`mfsr-win-x64-vX.X.X.zip` | ❌ **不要**（Native AOT） | ~15MB |
| **macOS Intel/ARM** | `mfprobe-osx-x64-vX.X.X.tar.gz`<br>`mfsr-osx-x64-vX.X.X.tar.gz` | ✅ **必要** | ~500KB |
| **Linux 64bit** | `mfprobe-linux-x64-vX.X.X.tar.gz`<br>`mfsr-linux-x64-vX.X.X.tar.gz` | ✅ **必要** | ~500KB |

#### Windows（手動インストール）

```cmd
# 1. ZIPファイルをダウンロードして展開
# 2. 実行ファイルを実行
mfprobe.exe -v
mfsr.exe -v
```

#### macOS（手動インストール）

```bash
# .NET 10 Runtimeをインストール（初回のみ）
brew install dotnet-sdk

# ダウンロードしたファイルを展開
tar -xzf mfprobe-osx-x64-v1.0.3.tar.gz

# 実行権限を付与
chmod +x mfprobe

# 実行
./mfprobe -v

# (オプション) システム全体で使えるようにする
sudo cp mfprobe /usr/local/bin/
sudo cp *.dll /usr/local/bin/
```

#### Linux（手動インストール）

```bash
# .NET 10 Runtimeをインストール（初回のみ）
# Ubuntu/Debian
sudo apt-get install dotnet-runtime-10.0

# ダウンロードしたファイルを展開
tar -xzf mfprobe-linux-x64-v1.0.3.tar.gz

# 実行権限を付与
chmod +x mfprobe

# 実行
./mfprobe -v

# (オプション) システム全体で使えるようにする
sudo cp mfprobe /usr/local/bin/
sudo cp *.dll /usr/local/bin/
```

---

## 🚀 使用例

### mfprobe

```bash
# ファイル内容を探索
mfprobe "*.txt"

# 特定の拡張子のファイルを指定
mfprobe "*.cs"
```

### mfsr

```bash
# ファイルパターンを指定して、文字エンコーディング・BOM・改行コードを変更
mfsr "*.txt" /w:utf-8 /b:false /nl:unix

# 別の設定例
mfsr "*.cs" /w:utf-8 /b:true
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
