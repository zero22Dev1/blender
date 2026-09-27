---
name: git-worktree-feature
description: Git worktree、機能ブランチ、作業ツリーの分離、別フォルダでの機能開発、並行開発について依頼されたときに使う。新機能、バグ修正、リファクタリングを専用ブランチとworktreeで実装・テストしたい場合に発火しやすい。
---

# 機能ごとのGit worktree

このSkillは、1つの機能を1つのブランチとworktreeに分けて作業するための手順です。

## このSkillが発火する場面

次のような依頼で自動発火しやすくなります。

- 「Git worktreeを使って機能ごとに開発したい」
- 「ログイン機能用に別ブランチと別フォルダを作って」
- 「メインの作業ツリーを汚さずにバグ修正したい」
- 「複数の機能を並行して開発できるようにして」
- 「featureブランチとworktreeを作成して」
- 「別の作業ツリーでリファクタリングしたい」

依頼文に`git worktree`、`worktree`、`機能ブランチ`、`featureブランチ`、`別フォルダ`、`作業ツリーを分ける`、`並行開発`などの言葉があると、さらに意図が伝わりやすくなります。

### このSkillを使わない場面

次の依頼だけでは、通常このSkillは使いません。

- 単にコミットメッセージを考える
- 既存ブランチの名前を確認する
- Gitの一般的な使い方を説明する
- 現在の差分やログを表示するだけ
- すでに開いているworktree内でコードを修正する

### 確実に発火させる方法

自動発火に任せず、確実に使いたい場合は次のslash commandを入力します。

```text
/git-worktree-feature
```

VB.NETの機能をworktree上でTDD開発する場合は、先にこのSkillでworktreeを作り、その後`/vbnet-tdd`を使います。

## 基本方針

- メインの作業ツリーを直接編集しない。
- worktreeを作る前に、Gitリポジトリと作業状態を確認する。
- 未コミットの変更、既存ブランチ、既存worktreeを上書きしない。
- worktreeを作った後は、対象worktreeの絶対パスで作業する。
- merge、rebase、push、worktreeの削除は、ユーザーに頼まれたときだけ行う。

## 手順

### 1. 要件を確認する

機能名、ASCIIのslug、受け入れ条件、テスト方法が分からない場合は、実装を始めずに確認する。

日本語の機能名には、Git用のslugを別に指定する。

```text
機能名: ユーザー認証
slug: user-authentication
```

### 2. worktreeを作る

元のリポジトリを`$workspace`に指定し、次のスクリプトを実行する。

```powershell
$workspace = 'C:\path\to\repository'
$script = Join-Path $workspace '.cline\skills\git-worktree-feature\scripts\new-feature-worktree.ps1'

Set-Location -LiteralPath $workspace
& $script -Feature 'ユーザー認証' -Slug 'user-authentication'
```

スクリプトは、Gitリポジトリ、未コミット変更、ブランチ、worktree、対象パスを確認してから作成する。出力された`worktreePath`とブランチ名を記録する。

### 3. 対象worktreeを開く

```powershell
code '<worktreePath>'
```

以後の検索、編集、テスト、ビルド、Git操作は、必ず対象worktreeで行う。プロジェクトの開発には、必要に応じて`/vbnet-tdd`を続けて使う。

### 4. 実装と検証を行う

1. 対象worktreeのブランチと初期状態を確認する。
2. プロジェクトの規約とテスト方法を読む。
3. 受け入れ条件を満たす最小の変更を行う。
4. テスト、lint、型チェック、ビルドを実行する。
5. 差分と生成物を確認する。

### 5. 完了を報告する

機能名、ブランチ、worktreeの絶対パス、変更ファイル、実行した検証、結果、未解決の問題を報告する。コミットした場合はコミットIDも報告する。

## 詳細

- 命名、作成条件、コミット、merge、削除: [docs/worktree-lifecycle.md](docs/worktree-lifecycle.md)
- 失敗時の切り分け: [docs/troubleshooting.md](docs/troubleshooting.md)