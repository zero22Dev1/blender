---
name: git-worktree-feature
description: Git worktreeを使って、機能単位のブランチと分離された作業フォルダを作成し、そのworktree内だけで実装・検証・コミットする。新機能、修正、リファクタリングをメイン作業ツリーから安全に分離したいときに使用する。
---

# Git Worktree Feature

機能ごとに独立したGit worktreeを作成し、メインの作業ツリーを汚さずに実装するためのSkillです。

## 絶対ルール

1. 作業開始前に、現在のワークスペースがGitリポジトリか確認する。
2. Gitリポジトリでない場合は、勝手に初期化せず停止する。ユーザーに `git init` または clone が必要だと伝える。
3. worktree作成後は、すべての読み書き、検索、テスト、ビルド、Git操作を対象worktreeの絶対パスで行う。
4. メイン作業ツリーのファイルを、機能実装のために直接編集しない。
5. ユーザーの未コミット変更を対象worktreeへコピーしない。必要な変更は、ユーザーの明示的な依頼がある場合だけ扱う。
6. 既存の変更、既存ブランチ、既存worktreeを削除・上書きしない。
7. 機能名、変更範囲、受け入れ条件、使用するテストコマンドが不明なら、作業を始めず確認する。
8. 実装後は対象worktree内でテストまたはビルドを実行し、結果を報告する。
9. ユーザーの明示的な依頼なしに、マージ、rebase、force push、worktree削除を行わない。

## 標準の命名規則

- ブランチとworktreeのslugは、Windows環境での文字コード問題を避けるため、ASCIIの小文字英数字とハイフンだけにする。
- 機能名が日本語などASCII以外を含む場合は、表示用の `-Feature` とは別に `-Slug` を指定する。
- ブランチ名: `feature/<slug>`
- worktreeパス: リポジトリの親にある `<リポジトリ名>-worktrees/<slug>`
- 例:
  - 機能名: `ユーザー認証`
  - slug: `user-authentication`
  - ブランチ: `feature/user-authentication`
  - worktree: `C:\path\to\<repo>-worktrees\user-authentication`

## 実行手順

### 1. 事前確認

対象ワークスペースの絶対パスを確認してから、次を実行する。

```powershell
$workspace = (Get-Location).Path
git -C $workspace rev-parse --show-toplevel
git -C $workspace status --short --branch
git -C $workspace worktree list
```

`git rev-parse` が失敗した場合は、次のどちらかをユーザーに依頼して停止する。

- 既存リポジトリを clone する
- 空のプロジェクトでよければ、ユーザーの確認後に `git init` を実行する

作業ツリーに未コミット変更がある場合、変更は新しいworktreeには含まれないことを明示する。ベースにするコミットが必要なら、先にユーザーへコミットまたはstashを依頼する。

### 2. 機能用worktreeの作成

このSkillに含まれるスクリプトを使用する。

```powershell
$skillRoot = 'C:\path\to\workspace\.cline\skills\git-worktree-feature'
& "$skillRoot\scripts\new-feature-worktree.ps1" -Feature '機能名' -Slug 'feature-slug'
```

実際の実行時は、`$skillRoot`をこのSkillのディレクトリの絶対パスに置き換える。スクリプトは次を検査する。

- Gitリポジトリであること
- bare repositoryでないこと
- メイン作業ツリーに未コミット変更がないこと
- ASCIIのslugが有効な形式であること
- 対象パスが存在しないこと
- 対象ブランチが既にworktreeで使用されていないこと

既存ブランチを再利用する場合は、作成スクリプトを無理に使わず、まず `git worktree list` と `git branch --list` で状態を確認する。既存worktreeの再利用は、ユーザーが明示的に指定した場合だけ行う。

### 3. VS CodeとClineを対象worktreeへ切り替える

スクリプトの出力にある `worktreePath` を使う。

```powershell
code '<worktreePath>'
```

新しいVS Codeウィンドウで対象worktreeを開いた後、Clineに実装を依頼する。Clineが元のウィンドウで継続する場合は、すべてのファイルパスとコマンドに対象worktreeの絶対パスを付ける。

### 4. 対象worktreeで実装する

実装前に次を確認する。

- `git -C '<worktreePath>' branch --show-current` が `feature/<slug>` である
- `git -C '<worktreePath>' status --short` の初期状態が想定どおりである
- プロジェクトの既存構成、規約、テスト方法を読んでいる

その後、次の順で進める。

1. 機能要件と受け入れ条件を整理する。
2. 必要最小限のファイルを対象worktree内に作成・変更する。
3. 既存のテスト、lint、型チェック、ビルドを実行する。
4. 必要ならテストを追加する。
5. 変更差分とテスト結果を確認する。
6. ユーザーの承認方針に従い、コミットするか、コミット可能な状態で停止する。

コミット例:

```powershell
git -C '<worktreePath>' add .
git -C '<worktreePath>' commit -m 'feat: implement <feature-slug>'
```

### 5. 統合と後片付け

マージはユーザーの明示的な依頼がある場合だけ行う。依頼された場合は、メイン作業ツリーがクリーンであることを確認してから実行する。

```powershell
git -C '<mainRepoPath>' status --short --branch
git -C '<mainRepoPath>' merge feature/<slug>
git -C '<mainRepoPath>' worktree list
```

worktree削除もユーザーの明示的な依頼がある場合だけ行う。

```powershell
git -C '<mainRepoPath>' worktree remove '<worktreePath>'
git -C '<mainRepoPath>' branch -d feature/<slug>
```

未コミット変更があるworktreeは、`--force`を使って削除しない。先に差分を確認し、ユーザーへ選択肢を提示する。

## Clineへの入力例

```text
/git-worktree-feature

機能名: ユーザー認証
slug: user-authentication
要件:
- メールアドレスとパスワードでログインできる
- バリデーションエラーを表示する
- 既存のテスト規約に合わせてテストを追加する
受け入れ条件:
- 正常系と異常系のテストが通る
- lintと型チェックが通る
- READMEまたは関連ドキュメントに使い方を追記する

まず `-Feature 'ユーザー認証' -Slug 'user-authentication'` で機能用worktreeを作成し、対象worktreeのパスとブランチを報告してください。worktreeをVS Codeで開き直した後、対象worktree内だけで実装してください。
```

## 完了時の報告形式

最後に次を報告する。

- 機能名
- ブランチ名
- worktreeの絶対パス
- 変更ファイル一覧
- 実行したテスト、lint、型チェック、ビルド
- 各コマンドの結果
- コミットID（コミットした場合）
- 未解決の問題
- マージまたは削除をまだ実行していないこと

## 制限事項

- worktreeはGitリポジトリのコミットをベースに作成される。未コミット変更は自動で引き継がれない。
- `.env`、`node_modules`、ビルド成果物などのignored filesはworktreeへ自動コピーされない。対象worktree内で必要なセットアップを行う。
- Git worktreeを使うには、対象プロジェクトがGitリポジトリとして初期化またはclone済みである必要がある。