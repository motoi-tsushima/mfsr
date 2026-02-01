# テストプロジェクト

このソリューションには4つのテストプロジェクトが含まれています。

## プロジェクト構成

### 1. mfprobe.Tests
mfprobe プロジェクトのユニットテスト

**テストクラス:**
- `ValidationMessagesTests` - 多言語メッセージのテスト
- `HelpMessagesTests` - ヘルプメッセージと言語判定のテスト
- `EncodingHelperTests` - エンコーディング検出機能のテスト

### 2. mfsr.Tests
mfsr プロジェクトのユニットテスト

**テストクラス:**
- `ReplaceStringsInFilesTests` - ファイル内の文字列置換機能のテスト
- `ValidationMessagesTests` - 検証メッセージのテスト

### 3. MF.Shared.Tests
共有ライブラリ (MF.Shared) のユニットテスト

**テストクラス:**
- `RmsmfExceptionTests` - カスタム例外クラスのテスト
- `ColipexTests` - コマンドライン引数解析のテスト
- `ByteOrderMarkJudgmentTests` - BOM判定機能のテスト

### 4. IntegrationTests
複数コンポーネントを組み合わせた統合テスト

**テストクラス:**
- `MfProbeIntegrationTests` - mfprobe の統合テスト (実ファイルを使用)
- `MfsrIntegrationTests` - mfsr の統合テスト (実ファイルを使用)
- `MfSharedIntegrationTests` - MF.Shared の統合テスト (エンコーディング検出など)
- `EndToEndIntegrationTests` - エンドツーエンドのシナリオテスト (プローブ → 置換)

## テストの実行方法

### Visual Studio
1. テストエクスプローラーを開く (テスト > テストエクスプローラー)
2. "すべて実行" をクリック

### コマンドライン (.NET CLI)

```bash
# すべてのテストを実行
dotnet test

# 特定のプロジェクトのテストを実行
dotnet test mfprobe.Tests
dotnet test mfsr.Tests
dotnet test MF.Shared.Tests
dotnet test IntegrationTests

# 詳細出力
dotnet test --verbosity detailed

# カバレッジレポート付き
dotnet test --collect:"XPlat Code Coverage"
```

### PowerShell から実行

```powershell
# すべてのテストを実行
dotnet test

# テスト結果を確認
dotnet test --logger "console;verbosity=detailed"
```

## テスト対象の主要機能

### エンコーディング関連
- ? UTF-8 (BOM有/無) の検出
- ? UTF-16 LE/BE の検出
- ? Shift_JIS, EUC-JP などのエンコーディング検出
- ? エンコーディング変換

### ファイル操作
- ? 文字列の置換
- ? 複数ファイルの一括処理
- ? BOMの追加/削除
- ? 改行コードの変換 (CRLF, LF, CR)

### 多言語対応
- ? 日本語、英語、韓国語、中国語（簡体/繁体）のメッセージ
- ? カルチャーに応じた自動言語切り替え

### コマンドライン解析
- ✓ オプション解析 (/option:value)
- ✓ パラメータ解析
- ✓ 区切り文字のサポート (: と =)

## 統合テストの特徴

### 実ファイルベースのテスト
- 一時ディレクトリを使用した実際のファイル操作
- テスト終了時の自動クリーンアップ
- 実際の使用シナリオに基づいたテストケース

### 統合テストのシナリオ

#### MfProbeIntegrationTests
- 複数ファイルの並列処理
- 各種エンコーディング (UTF-8, Shift_JIS) での検索
- BOM の検出
- 複数検索ワードの処理

#### MfsrIntegrationTests
- 文字列の置換
- エンコーディング変換 (Shift_JIS → UTF-8)
- BOM の追加/削除
- 改行コードの変換 (CRLF ↔ LF)
- 複数ファイルの一括処理
- 特殊文字を含む置換

#### MfSharedIntegrationTests
- UTF-8/UTF-16/Shift_JIS のエンコーディング検出
- BOM 判定 (UTF-8, UTF-16 LE/BE)
- 多言語コンテンツの保持
- エンコーディングの往復変換

#### EndToEndIntegrationTests
- プローブ → 置換の連携処理
- 多言語ファイルの検索と置換
- 大量ファイルの一括処理 (100ファイル)
- 複数ステップの処理フロー
- エンコーディング変換と置換の組み合わせ

## 継続的インテグレーション (CI)

GitHub Actions や Azure DevOps でテストを自動実行できます。

**GitHub Actions の例:**

```yaml
name: .NET Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

## カバレッジレポート

コードカバレッジを確認するには:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

レポートは `TestResults` ディレクトリに生成されます。

## テストの追加

新しいテストを追加する場合:

1. 対応するテストプロジェクトに新しいクラスを作成
2. `[Fact]` 属性でテストメソッドを定義
3. Arrange-Act-Assert パターンに従う

**例:**

```csharp
[Fact]
public void MyTest_WhenCondition_ExpectedResult()
{
    // Arrange
    var input = "test";
    
    // Act
    var result = MyMethod(input);
    
    // Assert
    Assert.Equal("expected", result);
}
```

## トラブルシューティング

### テストが見つからない場合
```bash
dotnet clean
dotnet build
dotnet test
```

### 統合テストで Shift_JIS エラーが発生する場合
統合テストで Shift_JIS エンコーディングを使用するテストがあります。
.NET 10 では、追加のエンコーディングプロバイダーを登録する必要があります。

詳細は `IntegrationTests/README.md` を参照してください。

### Native AOT でのテスト
テストプロジェクトは Native AOT を無効にしているため、通常の .NET ランタイムで実行されます。

## 統合テストの詳細

統合テストの詳細については、以下を参照してください：
- [IntegrationTests/README.md](IntegrationTests/README.md) - 統合テストの概要と実行方法

## 参考資料

- [xUnit Documentation](https://xunit.net/)
- [.NET Testing](https://docs.microsoft.com/dotnet/core/testing/)
- [Visual Studio Test Explorer](https://docs.microsoft.com/visualstudio/test/run-unit-tests-with-test-explorer)
