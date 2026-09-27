# VB.NETテストの実行方法

## SDK形式の.NET

対象テストプロジェクトを指定して、まず復元とテストを行う。

```powershell
dotnet restore '<testProject.vbproj>'
dotnet test '<testProject.vbproj>' --filter 'FullyQualifiedName~CalculatorTests' --logger 'console;verbosity=normal'
```

Green後は、関連プロジェクト、最後にソリューション全体を実行する。

```powershell
dotnet test '<solution.sln>' --no-restore --logger 'trx;LogFileName=tdd-results.trx'
```

`--no-build`は、ビルド済みのテストDLLだけを再実行するときに使う。失敗の原因を隠すために使わない。ビルドエラーとテスト失敗を分けて確認する。

フィルターは、実際のテスト名やカテゴリを確認してから指定する。

```powershell
dotnet test '<testProject.vbproj>' --filter 'FullyQualifiedName~CalculatorTests'
```

## 従来形式の.NET Framework

Visual Studio 2022のTest Explorerを第一候補にする。CLIが必要な場合は、Developer PowerShellまたはDeveloper Command Promptでビルドし、テストDLLを`vstest.console.exe`に渡す。

```powershell
msbuild '<solution.sln>' /t:Build /p:Configuration=Debug /p:Platform='Any CPU'
vstest.console.exe '<absolute path to Product.Tests.dll>' `
    /TestCaseFilter:'FullyQualifiedName~CalculatorTests' `
    /Logger:'console;verbosity=detailed' `
    /ResultsDirectory:'<absolute path to TestResults>'
```

DLLの場所を`bin\Debug`に決め打ちしない。Configuration、Platform、Target Framework、出力先をプロジェクトから確認する。

## Test Explorer

1. ソリューションを開く。
2. Build ConfigurationとPlatformを確認する。
3. Test Explorerで対象テストを実行する。
4. Redの失敗詳細とスタックトレースを読む。
5. Green後に対象テスト、関連テスト、全テストの順で実行する。

## テスト結果

テスト結果をファイルに残す場合は、TRXなど既存のCI方式に合わせる。テスト結果、ログ、ダンプには接続文字列やアクセストークンを出力しない。

## テストが実行できないとき

次を順に確認する。

- ソリューションが正しいか
- Target Frameworkが実行環境にあるか
- `Microsoft.NET.Test.Sdk`とテストアダプターが参照されているか
- テストクラスとメソッドが検出可能な公開範囲か
- ConfigurationとPlatformが一致しているか
- 出力DLLが存在するか
- 必要な環境変数、DB、ファイル、権限がそろっているか