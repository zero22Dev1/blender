Imports System.Collections.Generic
Imports System.Linq

Public Class InMemoryCardRepository
    Implements ICardRepository

    Private ReadOnly _cards As New List(Of Card)()

    Public Sub New(initialCards As IEnumerable(Of Card))
        If initialCards Is Nothing Then
            Throw New ArgumentNullException(NameOf(initialCards))
        End If

        For Each card In initialCards
            _cards.Add(card.Copy())
        Next
    End Sub

    Public Function GetAll() As IReadOnlyList(Of Card) Implements ICardRepository.GetAll
        Return _cards.Select(Function(card) card.Copy()).ToList().AsReadOnly()
    End Function

    Public Function GetById(id As Integer) As Card Implements ICardRepository.GetById
        Dim card = _cards.FirstOrDefault(Function(item) item.Id = id)
        If card Is Nothing Then
            Return Nothing
        End If

        Return card.Copy()
    End Function

    Public Sub Save(card As Card) Implements ICardRepository.Save
        If card Is Nothing Then
            Throw New ArgumentNullException(NameOf(card))
        End If

        Dim index = _cards.FindIndex(Function(item) item.Id = card.Id)
        If index < 0 Then
            _cards.Add(card.Copy())
        Else
            _cards(index) = card.Copy()
        End If
    End Sub
End Class