# Git worktreeのトラブル対応

## Gitリポジトリではない

`git rev-parse`が失敗した場合、Skillは勝手に`git init`しない。既存リポジトリをcloneするか、ユーザーの確認を得てから初期化する。

## 未コミット変更がある

新しいworktreeはコミットを基準に作られる。作業中の変更は自動で移らない。

```powershell
git -C '<mainRepoPath>' status --short
git -C '<mainRepoPath>' diff
```

必要なら、ユーザーの指示に従ってcommitまたはstashする。

## 対象パスが存在する

既存フォルダを削除したり上書きしたりしない。`git worktree list`とフォルダの内容を確認し、別slugを使うか既存worktreeを指定する。

## ブランチが別worktreeで使われている

同じブランチを同時に複数のworktreeへcheckoutできない。既存worktreeを使うか、新しいブランチ名を用いる。

```powershell
git -C '<mainRepoPath>' worktree list
git -C '<mainRepoPath>' branch --list
```

## 日本語のパスや機能名で表示が崩れる

Windows PowerShell 5.1では、GitのUTF-8出力が文字化けすることがある。リポジトリの検出には相対パスを使い、ブランチとworktreeのslugはASCIIにする。

## worktreeでファイルが足りない

ignored filesは自動コピーされない。対象worktreeで依存関係のインストールや環境ファイルの準備を行う。秘密情報はGitへ追加しない。