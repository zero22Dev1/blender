---
name: vbnet-tdd
description: Visual Studio 2022のVB.NETプロジェクトをTDD（Red-Green-Refactor）で開発する。MSTestを標準候補にしつつ、既存のテストフレームワークとプロジェクト規約を優先し、SDK形式の.NETと従来形式の.NET Frameworkの両方でテスト作成・実行・失敗分析・検証を行うときに使用する。
---

# VB.NET TDD

Visual Studio 2022で使うVB.NETプロジェクトを、テスト先行のRed → Green → Refactorサイクルで開発するためのSkillです。

## 最優先ルール

1. 実装前に、対象のソリューション、VBプロジェクト、Target Framework、既存テストプロジェクト、テストフレームワークを確認する。
2. 既存のテストフレームワーク、NuGetバージョン、命名規則、`Option Strict`、ビルド構成を尊重する。
3. 既存テストプロジェクトがある場合、新しいテストプロジェクトを勝手に追加しない。
4. テスト基盤がない場合、MSTestを標準候補として提案するが、NuGetパッケージやプロジェクトファイルを追加する前に対象Frameworkとの互換性を確認する。
5. まず失敗するテストを書く。テストが失敗することを確認する前に本実装を書かない。
6. Redの失敗が、期待したアサーション失敗なのか、コンパイル・テスト検出・環境設定の失敗なのかを区別する。
7. Greenでは、テストを通すための最小限の実装だけを行う。先回りした機能や不要な抽象化を追加しない。
8. Refactorでは、テストが通っている状態を維持しながら設計・重複・命名・可読性を改善する。
9. テストを弱める、アサーションを削除する、`Ignore`を追加する、例外を握りつぶすことで成功扱いにしない。
10. ユーザーの明示的な依頼なしに、既存テストの削除、テストフレームワークの置換、パッケージのメジャーアップデート、Gitのmerge・rebase・pushを行わない。

## Git worktreeとの併用

機能単位の隔離が必要な場合は、先に `/git-worktree-feature` を使用して専用worktreeを作成する。その後、このSkillを対象worktreeで使用する。

- worktree作成後は、すべてのファイル操作とテスト実行を対象worktreeの絶対パスで行う。
- メイン作業ツリーのファイルを直接変更しない。
- テスト成果物、`bin`、`obj`、TRXファイルの場所を対象worktree内に限定する。

## 開始時のプロジェクト調査

最初に、対象ワークスペースの絶対パスを確定する。次の読み取り専用スクリプトを使用する。

```powershell
$skillRoot = 'C:\path\to\workspace\.cline\skills\vbnet-tdd'
& "$skillRoot\scripts\inspect-vbnet-project.ps1" -Workspace 'C:\path\to\workspace'
```

次のファイルを確認する。

- `*.sln`または`*.slnx`
- `*.vbproj`
- `global.json`
- `Directory.Build.props`
- `Directory.Build.targets`
- `NuGet.config`
- `.runsettings`
- 既存テストプロジェクトの`PackageReference`
- CIのビルド・テスト定義

確認結果として、少なくとも次を整理する。

- ソースプロジェクトとテストプロジェクト
- SDK形式か従来形式か
- Target Framework（例: `net8.0`、`net48`）
- `AnyCPU`、`x86`、`x64`などのPlatform
- Debug/ReleaseなどのConfiguration
- MSTest、NUnit、xUnitなどの既存テストフレームワーク
- テストアダプターと`Microsoft.NET.Test.Sdk`の有無
- テスト実行に必要な外部依存（DB、ファイル、HTTP、環境変数）
- 現在のテストコマンド

プロジェクトが見つからない場合は、プロジェクトを推測して作り始めず、`.sln`または`.vbproj`の場所をユーザーに確認する。

## テストフレームワークの選択

### 既存プロジェクトがある場合

既存テストの属性、Assert、セットアップ、テスト実行方法をそのまま使う。例えばMSTestが使われていればMSTest、NUnitが使われていればNUnitを使う。混在させない。

### 新規にテスト基盤を作る場合

特別な指定がなければMSTestを第一候補にする。SDK形式の新規テストプロジェクトでは、対象Frameworkに合う公式テンプレートと、リポジトリで管理しているパッケージバージョンを使用する。

```powershell
dotnet new mstest -lang VB -n Product.Tests
dotnet add Product.Tests\Product.Tests.vbproj reference Product\Product.vbproj
```

ただし、従来形式の.NET Frameworkや既存のVisual Studioソリューションでは、既存のプロジェクト形式・NuGet管理方式・Platform設定を優先する。自動でSDK形式へ移行しない。

## TDDサイクル

1つの振る舞いにつき、次のサイクルを小さく繰り返す。

### 1. 要件をテスト可能な振る舞いにする

機能要件を次の形式に分解する。

- 入力または前提
- 実行する公開API、ユースケース、コマンド
- 期待する戻り値、状態変化、イベント、例外
- 境界値、異常系、依存先の失敗

受け入れ条件が曖昧な場合は、実装を始めず、確認すべきケースを提示する。

### 2. Red：失敗するテストを書く

- テスト名から振る舞いが分かるようにする。
- Arrange / Act / Assertを明確に分ける。
- まず1ケースだけ追加する。
- テスト対象の公開契約を通じて検証する。privateメソッドを直接テストしない。
- テストを実行し、期待した理由で失敗することを確認する。

コンパイルエラー、テスト未検出、依存関係エラーで失敗した場合はRed完了とみなさず、テスト環境を直す。

### 3. Green：最小実装で通す

- 失敗しているテストを通す最小限のコードを書く。
- まだ要求されていない一般化、抽象化、最適化を追加しない。
- テストを再実行する。
- 対象テストだけでなく、関連する既存テストも実行する。

### 4. Refactor：動作を変えずに改善する

- 重複を除去する。
- 命名と責務を改善する。
- 長いメソッドや過剰な条件分岐を分割する。
- 公開APIを変える場合は、先に受け入れ条件と影響範囲を確認する。
- リファクタリング前後でテストを実行する。

### 5. 次のケースへ進む

正常系だけで終わらせず、次の順でケースを検討する。

1. 基本的な正常系
2. 境界値
3. 空値・未設定・既定値
4. 不正入力
5. 依存先の失敗
6. 再実行、冪等性、状態遷移
7. 必要な並行性・タイムアウト・キャンセル

## VB.NETのMSTest規約

既存規約がなければ、次の形を標準とする。

```vb
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class CalculatorTests
    <TestMethod>
    Public Sub Add_TwoNumbers_ReturnsSum()
        ' Arrange
        Dim calculator As New Calculator()

        ' Act
        Dim result As Integer = calculator.Add(2, 3)

        ' Assert
        Assert.AreEqual(5, result)
    End Sub
End Class
```

MSTestを使う場合のルール:

- テストクラスに`<TestClass>`を付ける。
- 実行するテストメソッドに`<TestMethod>`を付ける。
- テストメソッドは原則`Public Sub`にする。
- 非同期テストでは`Async Function ... As Task`を使い、`Async Sub`を使わない。
- アサーションは、期待値、実際値、失敗理由が分かる形にする。
- 複数入力を同じ振る舞いで検証する場合は`<DataRow(...)>`または既存規約のデータ駆動機能を使う。
- セットアップと後処理は、テスト間で状態を共有しないようにする。
- private実装の直接テストではなく、公開された振る舞いをテストする。

VB.NETのデータ駆動テスト例:

```vb
<TestMethod>
<DataRow(-1)>
<DataRow(0)>
<DataRow(1)>
Public Sub IsPrime_ValuesLessThanTwo_ReturnsFalse(value As Integer)
    Dim service As New PrimeService()

    Dim result As Boolean = service.IsPrime(value)

    Assert.IsFalse(result, $"{value} should not be prime.")
End Sub
```

## テスト名と構成

既存規約がない場合は、`Method_Scenario_ExpectedResult`を使う。

例:

- `Add_TwoPositiveNumbers_ReturnsSum`
- `Withdraw_AmountExceedsBalance_ThrowsInsufficientFundsException`
- `Parse_EmptyText_ReturnsNone`
- `Save_WhenRepositoryFails_ReturnsFailure`

テストクラスは、クラス名またはユースケース単位で分ける。1つのテストメソッドで複数の無関係な振る舞いを検証しない。テストが失敗したとき、どの振る舞いが壊れたかが1件の失敗として分かる構成にする。

## テスト実行

### SDK形式の.NETプロジェクト

最初に対象テストプロジェクトだけを実行し、Green後に関連プロジェクトまたはソリューション全体を実行する。

```powershell
dotnet restore '<testProject.vbproj>'
dotnet test '<testProject.vbproj>' --filter 'FullyQualifiedName~CalculatorTests' --logger 'console;verbosity=normal'
dotnet test '<solution.sln>' --no-restore --logger 'trx;LogFileName=tdd-results.trx'
```

テストが失敗した場合、`--no-build`を付けて結果を隠さない。ビルドエラーとテスト失敗を分けて確認する。

### 従来形式の.NET Frameworkプロジェクト

Visual Studio 2022のTest Explorerを第一候補とし、CLIが必要な場合はDeveloper PowerShellまたはDeveloper Command PromptからMSBuildでビルドし、`vstest.console.exe`でテストDLLを実行する。

```powershell
msbuild '<solution.sln>' /t:Build /p:Configuration=Debug /p:Platform='Any CPU'
vstest.console.exe '<path\to\bin\Debug\Product.Tests.dll>' /TestCaseFilter:'FullyQualifiedName~CalculatorTests' /Logger:'console;verbosity=detailed' /ResultsDirectory:'<absolute\path\to\TestResults>'
```

実際のDLLパス、Platform、Configurationはプロジェクトの設定を確認してから決める。`bin\Debug`に決め打ちしない。

### Visual Studio Test Explorer

- ソリューションを開く。
- Build ConfigurationとPlatformを確認する。
- Test Explorerで対象テストを選択して実行する。
- Redの失敗詳細を確認する。
- GreenとRefactor後に、対象テスト、関連テスト、全テストの順で実行する。

## 失敗分析の順序

テスト失敗時は、次の順に原因を切り分ける。

1. コンパイルエラー：Imports、参照、型、アクセス修飾子、Option設定
2. テスト検出エラー：属性、Public、テストアダプター、Target Framework、出力DLL
3. 実行環境エラー：作業ディレクトリ、環境変数、ファイル、DB、権限、Platform
4. テストコードの誤り：Arrange、入力、期待値、モックやスタブの設定
5. 本実装の誤り：期待する契約、状態変化、例外、境界条件

エラーメッセージを省略せず、最初の失敗とスタックトレースを確認する。複数の失敗を一度に推測で修正せず、最小の再現テストを1件ずつ扱う。

## 外部依存の扱い

- 単体テストでは、DB、HTTP、時刻、乱数、ファイルシステムなどを直接使わず、既存の抽象化を利用する。
- 既存設計に抽象化がない場合、まず公開契約と変更範囲を確認する。
- 実DBや実HTTPが必要なテストは、単体テストと統合テストを分け、名前・カテゴリ・実行方法を明確にする。
- 認証情報や本番接続文字列をテストコード、ログ、Skillの出力に書かない。
- テスト間で共有する静的状態、環境変数、ファイルを残さない。必要ならテスト終了時に確実に戻す。

## 変更・コミット方針

各TDDサイクルで、次を確認する。

- 追加したテストが要求する振る舞いを説明できる。
- Redを確認した。
- Green後に対象テストと関連テストが通った。
- Refactor後も同じテストが通った。
- 差分に不要な変更、生成物、秘密情報がない。

コミットする場合の例:

```powershell
git -C '<worktreePath>' add '<sourcePath>' '<testPath>'
git -C '<worktreePath>' commit -m 'feat: implement <feature-slug>'
```

Red状態の未完成コードを、完成した機能としてコミットしない。コミット方針が不明なら、コミットせず差分と検証結果を報告する。

## 完了条件

次を満たすまで完了と報告しない。

- 要件に対応するテストがある。
- 少なくとも1回、テストが期待どおりにRedになったことを確認している。
- 最小実装でGreenになっている。
- Refactor後も対象テスト、関連テスト、可能なら全テストが通っている。
- ビルド構成とテストコマンドが再現可能である。
- テスト結果、未実行のテスト、既知の制限を報告している。

## 完了時の報告形式

最後に次を報告する。

- 対象ソリューション・プロジェクト
- 対象Framework、Configuration、Platform
- 使用したテストフレームワークとアダプター
- 追加・変更したテスト一覧
- Redで確認した失敗内容
- Greenで行った最小実装
- Refactor内容
- 実行したコマンドまたはTest Explorer操作
- 対象テスト、関連テスト、全テストの結果
- ビルド結果
- 未実行テストと理由
- 変更ファイル一覧
- コミットID（コミットした場合）
- 未解決の問題