---
name: vbnet-tdd
description: VB.NET、Visual Basic、Visual Studio 2022、.NET Framework、MSTest、NUnit、xUnitのプロジェクトを、単体テスト先行とRed・Green・Refactorで開発するときに使う。テストプロジェクトの追加、dotnet test、Test Explorer、vstest.console.exeを使ったTDDの依頼で発火しやすい。
---

# VB.NETのTDD

このSkillは、VB.NETの機能をテストから始めて、安全に実装するための手順です。

## このSkillが発火する場面

次のような依頼で自動発火しやすくなります。

- 「VB.NETでTDD開発して」
- 「Visual Studio 2022のVBプロジェクトにテストを追加して」
- 「MSTestでログイン機能の単体テストを書いて」
- 「Red・Green・Refactorで実装して」
- 「.NET FrameworkのテストをTest Explorerで確認して」
- 「dotnet testが通るようにVB.NETのテストを作って」
- 「既存のVB.NET機能をテスト先行で修正して」
- 「テストプロジェクトがないのでMSTestを追加してTDDで進めて」

依頼文に`VB.NET`、`Visual Basic`、`.vbproj`、`Visual Studio 2022`、`.NET Framework`、`MSTest`、`NUnit`、`xUnit`、`単体テスト`、`テスト先行`、`TDD`、`Red-Green-Refactor`、`Test Explorer`、`dotnet test`などの言葉があると、さらに意図が伝わりやすくなります。

### このSkillを使わない場面

次の依頼だけでは、通常このSkillは使いません。

- VB.NETの文法やAPIについて説明するだけ
- 既存テストの結果を表示するだけ
- 単純なコード整形やコメント修正だけ
- TDDを使わず、設計資料だけを作成する
- C#、Python、JavaScriptなど別言語のテストを作る

### 確実に発火させる方法

自動発火に任せず、確実に使いたい場合は次のslash commandを入力します。

```text
/vbnet-tdd
```

機能単位で作業を分離する場合は、先に`/git-worktree-feature`でworktreeを作成し、そのworktreeを開いてからこのSkillを使います。

## 基本方針

- 先にプロジェクト構成と既存テストを確認する。
- 既存のテストフレームワークと命名規則をそのまま使う。
- いきなり実装せず、まず失敗するテストを書く。
- Red、Green、Refactorを小さく繰り返す。
- コンパイルエラーやテスト検出エラーを、TDDのRedと混同しない。
- テストを弱めたり、`Ignore`を追加したりして成功扱いにしない。
- 既存テストの削除、フレームワークの置換、パッケージの大幅更新は、ユーザーに確認してから行う。

## 手順

### 1. プロジェクトを調べる

対象ワークスペースの絶対パスを指定して、調査スクリプトを実行する。

```powershell
$workspace = 'C:\path\to\vb-project'
$skillRoot = Join-Path $workspace '.cline\skills\vbnet-tdd'

& "$skillRoot\scripts\inspect-vbnet-project.ps1" `
    -Workspace $workspace `
    -Json
```

`.sln`、`.vbproj`、Target Framework、SDK形式かどうか、テストプロジェクト、テストパッケージを確認する。プロジェクトが見つからなければ、推測して作業を始めず、ユーザーに場所を確認する。

### 2. テスト方法を決める

- 既存テストがあれば、同じフレームワークと実行方法を使う。
- テスト基盤がないSDK形式のプロジェクトでは、MSTestを第一候補にする。
- 従来形式の.NET Frameworkでは、Visual Studio Test Explorerまたは`vstest.console.exe`を使う。
- 新しいテストプロジェクトやNuGetパッケージを追加する場合は、対象Frameworkとの互換性を確認する。

### 3. Red → Green → Refactor

1. **Red**: 受け入れ条件を表すテストを1件書く。
2. テストを実行し、期待した理由で失敗することを確認する。
3. **Green**: テストを通す最小限の実装を書く。
4. 対象テストと関連テストを実行する。
5. **Refactor**: 動作を変えずに、重複、命名、責務、可読性を改善する。
6. リファクタリング後に同じテストを再実行する。
7. 正常系、境界値、空値、不正入力、依存先の失敗を順に追加する。

### 4. 完了を確認する

- 対象テストが通る。
- 関連テストが通る。
- 可能なら全テストが通る。
- ビルドが通る。
- 生成物や秘密情報を差分に含めていない。
- Redで確認した失敗、Greenの実装、Refactorの内容を説明できる。

## Git worktreeとの併用

機能を分離して開発する場合は、先に`/git-worktree-feature`でworktreeを作る。その後、このSkillを対象worktreeで使う。

- worktree作成後は、対象worktreeの絶対パスで作業する。
- メイン作業ツリーを直接変更しない。
- `bin`、`obj`、テスト結果、TRXファイルを対象worktree内に置く。

## 詳細

- TDDの進め方とテスト設計: [docs/tdd-workflow.md](docs/tdd-workflow.md)
- VB.NETとMSTestの書き方: [docs/vbnet-mstest.md](docs/vbnet-mstest.md)
- SDK形式・.NET Frameworkの実行方法: [docs/test-execution.md](docs/test-execution.md)
- テスト失敗の切り分けと完了報告: [docs/diagnosis-and-reporting.md](docs/diagnosis-and-reporting.md)