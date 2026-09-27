Imports System
Imports System.Collections.Generic

Public Class CardService
    Private ReadOnly _repository As ICardRepository

    Public Sub New(repository As ICardRepository)
        If repository Is Nothing Then
            Throw New ArgumentNullException(NameOf(repository))
        End If

        _repository = repository
    End Sub

    Public Function GetCards() As IReadOnlyList(Of Card)
        Return _repository.GetAll()
    End Function

    Public Function GetCardForEdit(id As Integer) As Card
        Return _repository.GetById(id)
    End Function

    Public Sub SaveCard(card As Card)
        If card Is Nothing Then
            Throw New ArgumentNullException(NameOf(card))
        End If

        If String.IsNullOrWhiteSpace(card.Name) Then
            Throw New ArgumentException("カード名を入力してください。", NameOf(card))
        End If

        _repository.Save(card)
    End Sub
End Class