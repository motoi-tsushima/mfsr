# 統合テストプロジェクト

統合テスト (IntegrationTests) は、複数のコンポーネントを組み合わせた実際のシナリオをテストします。

## プロジェクト概要

- **プロジェクト名**: IntegrationTests
- **対象フレームワーク**: .NET 10
- **テストフレームワーク**: xUnit

## テストクラス

### 1. MfProbeIntegrationTests
`mfprobe` の統合テスト - 実際のファイルを作成して検索機能をテスト

**テストケース:**
- UTF-8 ファイルの検索
- 複数検索ワードの処理
- BOM 付き UTF-8 ファイルの検出
- 複数ファイルの並列処理

### 2. MfsrIntegrationTests  
`mfsr` の統合テスト - 実際のファイル置換をテスト

**テストケース:**
- 単純な文字列置換
- 複数置換ワードの処理
- 日本語テキストの置換
- エンコーディング変換 (Shift_JIS → UTF-8)
- BOM の追加/削除
- 改行コードの変換 (CRLF ↔ LF)
- 特殊文字の置換

### 3. MfSharedIntegrationTests
`MF.Shared` ライブラリの統合テスト - エンコーディング検出などをテスト

**テストケース:**
- UTF-8 (BOM あり/なし) の検出
- UTF-16 LE/BE の検出
- BOM 判定機能
- 多言語コンテンツの保持

### 4. EndToEndIntegrationTests
エンドツーエンドのシナリオテスト - プローブと置換を組み合わせたワークフロー

**テストケース:**
- プローブ → 置換の連携処理
- 多言語ファイルの検索と置換
- 大量ファイル (100 ファイル) の一括処理
- 複数ステップの処理フロー
- エンコーディング変換と置換の組み合わせ

## テストの実行方法

### すべての統合テストを実行
```bash
dotnet test IntegrationTests/IntegrationTests.csproj
```

### 詳細出力で実行
```bash
dotnet test IntegrationTests/IntegrationTests.csproj --verbosity detailed
```

### 特定のテストクラスのみ実行
```bash
dotnet test IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~MfProbe"
dotnet test IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~Mfsr"
dotnet test IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~EndToEnd"
```

## テストの特徴

### 一時ディレクトリの使用
各テストクラスは `IDisposable` を実装し、一時ディレクトリを使用してテストを実行します。
テスト完了後は自動的にクリーンアップされます。

### 実ファイルベースのテスト
モックではなく、実際のファイルシステムを使用してテストを行います。
これにより、実際の使用シナリオに近い状態でテストできます。

### 並列処理のテスト
実際のアプリケーションと同様に、複数ファイルの並列処理をテストします。

## エンコーディングのサポート

統合テストでは以下のエンコーディングをテストします：

- UTF-8 (BOM あり/なし)
- UTF-16 Little Endian
- UTF-16 Big Endian
- Shift_JIS (自動設定済み)

**実装済み**: すべてのテストクラスで `System.Text.Encoding.CodePages` パッケージを使用し、
静的コンストラクタで `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)` を呼び出しています。
これにより、Shift_JIS などのレガシーエンコーディングが自動的に利用可能になります。

## トラブルシューティング

### Shift_JIS エラーが発生する場合
**現在は不要**: プロジェクトで既に設定済みです。各テストクラスの静的コンストラクタで
自動的に `CodePagesEncodingProvider` が登録されます。

```csharp
static MfProbeIntegrationTests()
{
    // Shift_JIS などのレガシーエンコーディングを使用可能にする
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
}
```

### ファイルアクセスエラーが発生する場合
一時ディレクトリへの書き込み権限を確認してください。

### テストが失敗する場合
- 出力ディレクトリを確認
- ログを詳細モードで確認: `--verbosity detailed`
- 個別のテストケースを実行して問題を特定

## CI/CD での統合テストの実行

GitHub Actions での例：

```yaml
- name: Run Integration Tests
  run: dotnet test IntegrationTests/IntegrationTests.csproj --no-build --verbosity normal
```

## ベストプラクティス

1. **独立性**: 各テストは他のテストに依存しない
2. **クリーンアップ**: 一時ファイルは必ず削除
3. **明確なテスト名**: テスト名から何をテストしているか分かるようにする
4. **AAA パターン**: Arrange-Act-Assert パターンに従う

## 今後の拡張予定

- パフォーマンステスト
- 大容量ファイルのテスト
- ネットワークドライブでのテスト
- 異なる OS でのテスト
