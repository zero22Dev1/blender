Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports CardApp

<TestClass>
Public Class CardServiceTests
    Private _repository As InMemoryCardRepository
    Private _service As CardService

    <TestInitialize>
    Public Sub Initialize()
        _repository = New InMemoryCardRepository({
            New Card With {.Id = 1, .Name = "炎の騎士", .Description = "火属性のカード", .Attack = 120, .Defense = 80},
            New Card With {.Id = 2, .Name = "森の守護者", .Description = "自然属性のカード", .Attack = 70, .Defense = 140}
        })
        _service = New CardService(_repository)
    End Sub

    <TestMethod>
    Public Sub GetCards_ReturnsAllCards()
        Dim cards = _service.GetCards()

        Assert.AreEqual(2, cards.Count)
        Assert.AreEqual("炎の騎士", cards(0).Name)
        Assert.AreEqual("森の守護者", cards(1).Name)
    End Sub

    <TestMethod>
    Public Sub GetCardForEdit_ExistingId_ReturnsEditableCopy()
        Dim card = _service.GetCardForEdit(1)

        Assert.IsNotNull(card)
        Assert.AreEqual(1, card.Id)
        Assert.AreEqual("炎の騎士", card.Name)
    End Sub

    <TestMethod>
    Public Sub GetCardForEdit_UnknownId_ReturnsNothing()
        Dim card = _service.GetCardForEdit(999)

        Assert.IsNull(card)
    End Sub

    <TestMethod>
    Public Sub SaveCard_ChangedCard_PersistsChanges()
        Dim card = _service.GetCardForEdit(1)
        card.Name = "炎の騎士・改"
        card.Attack = 160

        _service.SaveCard(card)

        Dim saved = _service.GetCardForEdit(1)
        Assert.AreEqual("炎の騎士・改", saved.Name)
        Assert.AreEqual(160, saved.Attack)
    End Sub

    <TestMethod>
    Public Sub SaveCard_EmptyName_ThrowsArgumentException()
        Dim card = _service.GetCardForEdit(1)
        card.Name = " "

        Assert.ThrowsException(Of ArgumentException)(Sub() _service.SaveCard(card))
    End Sub
End Class