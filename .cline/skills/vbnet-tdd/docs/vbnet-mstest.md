# VB.NETとMSTest

## 基本形

既存の規約がなければ、MSTestは次の形で書く。

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

## 属性とメソッド

- テストクラスに`<TestClass>`を付ける。
- 実行するメソッドに`<TestMethod>`を付ける。
- テストメソッドは原則`Public Sub`にする。
- 非同期テストは`Async Function ... As Task`にする。`Async Sub`は使わない。
- テスト間で共有する可変状態を持たない。

## アサーション

期待値、実際値、失敗理由が分かるアサーションを使う。

```vb
Assert.AreEqual(expected, actual, "合計金額が一致しません。")
Assert.IsTrue(condition, "条件を満たしていません。")
Assert.IsFalse(condition, "条件を満たしてはいけません。")
```

例外を確認する場合は、既存のMSTestバージョンで使えるAPIを確認してから書く。プロジェクトのバージョンを無視して新しいAPIへ書き換えない。

## データ駆動テスト

同じ振る舞いを複数の入力で確認する場合は、`DataRow`など既存の方式を使う。

```vb
<TestMethod>
<DataRow(-1)>
<DataRow(0)>
<DataRow(1)>
Public Sub IsPrime_ValuesLessThanTwo_ReturnsFalse(value As Integer)
    Dim service As New PrimeService()

    Dim result As Boolean = service.IsPrime(value)

    Assert.IsFalse(result, $"{value}は素数ではありません。")
End Sub
```

`DataRow`の型や利用可能な属性はMSTestのバージョンによって異なる。既存のテストコードとパッケージバージョンを先に確認する。

## テスト対象の範囲

privateメソッドを直接テストしない。公開された振る舞いを通して検証する。

privateメソッドをテストしたくなった場合は、まず次を検討する。

- 公開APIから検証できないか
- 責務を別クラスへ分ける必要がないか
- テストのためだけに公開範囲を広げていないか

## テストプロジェクトを新しく作る場合

テストプロジェクトがないSDK形式のプロジェクトでは、対象Frameworkと既存のパッケージ管理方法を確認してから作る。

```powershell
dotnet new mstest -lang VB -n Product.Tests
dotnet add Product.Tests\Product.Tests.vbproj reference Product\Product.vbproj
```

既存の.NET Frameworkプロジェクトを、確認なしにSDK形式へ移行しない。既存ソリューションのプロジェクト形式、Platform、NuGet管理方式を優先する。