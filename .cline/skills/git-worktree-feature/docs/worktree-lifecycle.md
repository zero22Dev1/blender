# Git worktreeの運用ルール

## 命名

ブランチ名とworktree名には、ASCIIの小文字英数字とハイフンだけを使う。Windowsでは日本語のGit出力が文字化けすることがあるため、機能名とslugを分ける。

```text
機能名: ユーザー認証
slug: user-authentication
ブランチ: feature/user-authentication
worktree: <リポジトリの親>\<リポジトリ名>-worktrees\user-authentication
```

## 作成前の確認

次を確認してから作成する。

```powershell
$workspace = 'C:\path\to\repository'
git -C $workspace rev-parse --show-toplevel
git -C $workspace status --short --branch
git -C $workspace worktree list
```

次の場合は、スクリプトを実行せずユーザーに確認する。

- Gitリポジトリではない
- ベースにしたいコミットがない
- 未コミットの変更がある
- 既存のブランチやworktreeを使いたい

未コミットの変更は、新しいworktreeへ自動では引き継がれない。必要な変更は、ユーザーの指示に従ってcommitまたはstashする。

## 作成スクリプト

```powershell
$skillRoot = 'C:\path\to\repository\.cline\skills\git-worktree-feature'
Set-Location -LiteralPath 'C:\path\to\repository'
& "$skillRoot\scripts\new-feature-worktree.ps1" `
    -Feature 'ユーザー認証' `
    -Slug 'user-authentication'
```

確認だけ行う場合は`-DryRun`を付ける。

```powershell
& "$skillRoot\scripts\new-feature-worktree.ps1" `
    -Feature 'ユーザー認証' `
    -Slug 'user-authentication' `
    -DryRun
```

スクリプトは次を確認する。

- 通常のGitリポジトリであること
- ベアリポジトリでないこと
- 元の作業ツリーに未コミット変更がないこと
- slugとGit refが有効であること
- 対象パスが存在しないこと
- 対象ブランチが別のworktreeで使われていないこと

## worktreeでの作業

作成後は、スクリプトの出力にある絶対パスを使う。

```powershell
$worktreePath = '<worktreePath>'
code $worktreePath

git -C $worktreePath branch --show-current
git -C $worktreePath status --short
```

`.env`、`node_modules`、`bin`、`obj`などのignored filesは自動でコピーされない。必要なセットアップは対象worktree内で行う。

## コミット

実装と検証が終わり、ユーザーの方針がコミットを許可している場合だけコミットする。

```powershell
git -C '<worktreePath>' add .
git -C '<worktreePath>' commit -m 'feat: implement <feature-slug>'
```

コミット前に次を確認する。

- 意図しないファイルが含まれていない
- 秘密情報や生成物が含まれていない
- テスト結果を記録している

## mergeと削除

mergeはユーザーに頼まれたときだけ行う。メイン作業ツリーがクリーンであることを確認する。

```powershell
git -C '<mainRepoPath>' status --short --branch
git -C '<mainRepoPath>' merge feature/<feature-slug>
```

worktreeの削除も、ユーザーに頼まれたときだけ行う。

```powershell
git -C '<mainRepoPath>' worktree remove '<worktreePath>'
git -C '<mainRepoPath>' branch -d feature/<feature-slug>
```

未コミット変更があるworktreeを`--force`で削除しない。先に差分を確認し、保存、commit、破棄の選択肢を提示する。

## 完了報告

次の項目を報告する。

- 機能名とslug
- ブランチ名
- worktreeの絶対パス
- 変更ファイル
- 実行したテスト、lint、型チェック、ビルド
- 各結果
- コミットID
- 未解決の問題
- mergeや削除をまだ実行していないこと