# mfsr / mfprobe

.NET 10 で実装された高速テキスト処理ツール（Native AOT 対応）

## 概要

このプロジェクトは、.NET Framework 4.8 版から .NET 10 に移植された2つのコマンドラインツールを提供します：

- **mfsr** - .NET Framework 4.8 版の **rmsmf** から移植
  - 複数ファイルの複数文字列を一括置換するツール
  
- **mfprobe** - .NET Framework 4.8 版の **txprobe** から移植
  - ファイルのエンコーディング検出と文字列検索ツール

## 特徴

- ✅ **Native AOT 対応** - 超高速起動と低メモリ使用量
- ✅ **.NET 10** - 最新の .NET プラットフォームで実装
- ✅ **多言語対応** - 日本語、英語、韓国語、中国語（簡体字・繁体字）に対応
- ✅ **並列処理** - 複数ファイルの高速処理
- ✅ **包括的なエンコーディングサポート** - UTF-8, UTF-16, Shift_JIS など
- ✅ **BOM 操作** - BOM の追加・削除が可能
- ✅ **改行コード変換** - CRLF, LF, CR の相互変換
- ✅ **統合テスト完備** - 35件の統合テストで品質を保証

## ダウンロード

最新のバイナリは [Releases](../../releases) からダウンロードできます。

### システム要件

- **Windows**: Windows 10 以降（x64）
- **Linux**: 各種ディストリビューション（x64, ARM64）
- **macOS**: macOS 10.15 以降（x64, ARM64）

または、.NET 10 ランタイムがインストールされている環境

## インストール

### バイナリ版（推奨）

1. [Releases](../../releases) から最新の zip ファイルをダウンロード
2. 適当なフォルダに解凍
3. `mfsr.exe` / `mfprobe.exe` を実行

### ソースからビルド

```bash
git clone https://github.com/motoi-tsushima/mfsr.git
cd mfsr
dotnet build -c Release
```

## 使い方

### mfsr - 文字列一括置換ツール

#### 基本的な使い方

```bash
mfsr /r:replace.csv *.txt
```

#### オプション

| オプション | 説明 |
|----------|------|
| `/r:<CSVファイル>` | 置換単語リストCSVファイルを指定（必須） |
| `/f:<ファイルリスト>` | 処理対象ファイルリストを指定 |
| `/d` | サブディレクトリも検索対象に含める |
| `/b:<true\|false>` | BOMの追加(true)または削除(false) |
| `/nl:<crlf\|lf\|cr>` | 改行コードを変換 |
| `/c:<エンコーディング>` | 読み込みファイルのエンコーディング |
| `/w:<エンコーディング>` | 書き込みファイルのエンコーディング |
| `/rc:<エンコーディング>` | 置換単語リストのエンコーディング |
| `/fc:<エンコーディング>` | ファイルリストのエンコーディング |
| `/det:<0\|1\|3>` | エンコーディング自動判定モード（0=通常、1=厳密、3=推測） |
| `/ci:<カルチャー>` | カルチャー情報を設定（例: en-US, ja-JP） |

#### 置換単語リストCSVの形式

```csv
検索ワード1,置換ワード1
検索ワード2,置換ワード2
検索ワード3,置換ワード3
```

**注意**: 
- カンマ区切り（CSV形式）
- 改行コードは `\r\n` の形式で記述可能
- ダブルクォートで囲むことでカンマを含む文字列も扱える

#### 使用例

**例1: 複数ファイルの文字列置換**
```bash
mfsr /r:replace.csv *.txt
```

**例2: サブディレクトリも含めて処理**
```bash
mfsr /d /r:replace.csv *.cs
```

**例3: BOMを追加して保存**
```bash
mfsr /r:replace.csv /b:true *.txt
```

**例4: 改行コードをLFに変換**
```bash
mfsr /r:replace.csv /nl:lf *.sh
```

**例5: Shift_JIS から UTF-8 に変換しながら置換**
```bash
mfsr /r:replace.csv /c:shift_jis /w:utf-8 *.txt
```

**例6: ファイルリストから処理**
```bash
mfsr /r:replace.csv /f:filelist.txt
```

---

### mfprobe - エンコーディング検出・文字列検索ツール

#### 基本的な使い方

```bash
mfprobe *.txt
```

#### オプション

| オプション | 説明 |
|----------|------|
| `/s:<検索文字列一覧テキストファイル名>` | 検索する文字列を複数格納したファイルを指定 |
| `/f:<ファイルリスト>` | 処理対象ファイルリストを指定 |
| `/d` | サブディレクトリも検索対象に含める |
| `/c:<エンコーディング>` | 読み込みファイルのエンコーディング |
| `/o:<出力ファイル>` | 検索結果を出力するファイル名 |
| `/oc:<エンコーディング>` | 出力ファイルのエンコーディング |
| `/sc:<エンコーディング>` | 検索文字列一覧ファイルのエンコーディング |
| `/fc:<エンコーディング>` | ファイルリストのエンコーディング |
| `/det:<0\|1\|3>` | エンコーディング自動判定モード |
| `/ci:<カルチャー>` | カルチャー情報を設定 |
| `/p` または `/probe` | プローブモード（詳細情報を表示） |

#### 検索文字列一覧ファイルの形式

検索したい文字列を1行に1つずつ記述します。

```text
TODO
FIXME
ERROR
```

#### 使用例

**例1: ファイルのエンコーディングを確認**
```bash
mfprobe *.txt
```

**例2: 検索ワードリストを使ってファイルを検索**
```bash
# searchwords.txt に TODO, FIXME などを記述
mfprobe /s:searchwords.txt *.cs
```

**例3: 検索結果をファイルに出力（mfsrとの連携用）**
```bash
# keywords.txt に検索したいキーワードを記述
mfprobe /s:keywords.txt /o:target_files.txt *.cs
```

**例4: プローブモードで詳細情報を表示**
```bash
mfprobe /p *.txt
```

**例5: サブディレクトリも含めて検索**
```bash
# patterns.txt に検索パターンを記述
mfprobe /d /s:patterns.txt *.cs
```

**例6: Shift_JIS ファイルから検索**
```bash
# search_jp.txt に日本語の検索ワードを記述（Shift_JIS エンコーディング）
mfprobe /c:shift_jis /s:search_jp.txt /sc:shift_jis *.txt
```

---

### mfprobe と mfsr の連携

mfprobe で検索したファイルリストを mfsr で処理することができます。

**ワークフロー例**:

```bash
# 1. 検索ワードリストを作成（keywords.txt）
echo TODO > keywords.txt
echo FIXME >> keywords.txt

# 2. mfprobe で検索ワードを含むファイルを検索し、リストを出力
mfprobe /s:keywords.txt /o:todo_files.txt *.cs

# 3. mfsr でそのファイルリストを読み込んで一括置換
mfsr /r:replace.csv /f:todo_files.txt
```

この連携により、特定の条件に合致するファイルのみを効率的に処理できます。

## サポートされるエンコーディング

- UTF-8（BOM あり/なし）
- UTF-16 LE（Little Endian）
- UTF-16 BE（Big Endian）
- UTF-32
- Shift_JIS（日本語）
- EUC-JP（日本語）
- ISO-2022-JP（日本語）
- その他、.NET がサポートする各種エンコーディング

エンコーディング名の指定方法：
- コードページ番号（例: `932` = Shift_JIS）
- エンコーディング名（例: `shift_jis`, `utf-8`, `euc-jp`）

## エンコーディング自動判定モード

`/det` オプションで自動判定の精度を調整できます：

- **0** (通常): バランスの取れた判定（デフォルト）
- **1** (厳密): より厳密な判定（精度重視）
- **3** (推測): より積極的な推測（対応範囲重視）

## 改行コード

`/nl` オプションで以下の改行コードに変換できます：

- **crlf**: Windows形式（`\r\n`）
- **lf**: Unix/Linux形式（`\n`）
- **cr**: 古いMac形式（`\r`）

## 多言語サポート

システムの言語設定に応じて、自動的にヘルプメッセージやエラーメッセージが切り替わります。

**サポート言語**:
- 日本語 (ja-JP)
- 英語 (en-US)
- 韓国語 (ko-KR)
- 中国語簡体字 (zh-CN)
- 中国語繁体字 (zh-TW)

`/ci` オプションで明示的に言語を指定することも可能：
```bash
mfsr /ci:en-US /r:replace.csv *.txt
```

## テスト

プロジェクトには包括的なテストスイートが含まれています：

- **ユニットテスト**: 57件
- **統合テスト**: 35件
- **合計**: 92件

テストの実行：
```bash
dotnet test
```

詳細は [TESTING.md](TESTING.md) を参照してください。

## ビルド

### 通常ビルド
```bash
dotnet build -c Release
```

### Native AOT ビルド（推奨）
```bash
# Windows
dotnet publish mfsr/mfsr.csproj -c Release -r win-x64
dotnet publish mfprobe/mfprobe.csproj -c Release -r win-x64

# Linux
dotnet publish mfsr/mfsr.csproj -c Release -r linux-x64
dotnet publish mfprobe/mfprobe.csproj -c Release -r linux-x64

# macOS
dotnet publish mfsr/mfsr.csproj -c Release -r osx-x64
dotnet publish mfprobe/mfprobe.csproj -c Release -r osx-arm64
```

## 更新履歴

### v1.0.2.1 (2026-02-01)
- 🎉 **初回リリース** - .NET 10 に移植
- ✅ Native AOT 対応で高速化
- ✅ 統合テスト完備（35件）
- ✅ 多言語対応（5言語）
- ✅ ProbeFiles の出力ファイル書き込み問題を修正
- ✅ BOM 判定の精度向上
- ✅ エンコーディング検出の改善
- ✅ 並列処理の最適化（.NET 10）
- ✅ エラーハンドリングの強化

### .NET Framework 4.8 版からの主な変更点

#### 改善点
- **パフォーマンス**: Native AOT により起動速度が大幅に向上
- **メモリ効率**: 最適化されたメモリ管理
- **並列処理**: .NET 10 の最新APIを使用した高速処理
- **エラーハンドリング**: より詳細なエラー情報
- **テスト**: 包括的な統合テストで品質保証

#### 互換性
- コマンドラインオプションは .NET Framework 4.8 版と互換
- 置換単語リストCSVの形式は同じ
- 既存のスクリプトやバッチファイルをそのまま使用可能

## ライセンス

このプロジェクトは MIT ライセンスの下で提供されています。

Copyright © 2026 motoi.tsushima

詳細は [LICENSE](LICENSE) ファイルを参照してください。

### 使用しているサードパーティコンポーネント

This software includes the following third-party components:

**UTF.Unknown**
- Copyright (c) 2018 Nikolay Pultsin
- Licensed under MIT License
- https://github.com/CharsetDetector/UTF-unknown

## 貢献

バグ報告や機能要望は [Issues](../../issues) へお願いします。

プルリクエストも歓迎します！

## 参考資料

- [TESTING.md](TESTING.md) - テストの詳細
- [IntegrationTests/README.md](IntegrationTests/README.md) - 統合テストの詳細

## 作者

motoi.tsushima

## リンク

- GitHub: https://github.com/motoi-tsushima/mfsr
- Issues: https://github.com/motoi-tsushima/mfsr/issues
- Releases: https://github.com/motoi-tsushima/mfsr/releases


