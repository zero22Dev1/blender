Imports System.Collections.Generic

Public Interface ICardRepository
    Function GetAll() As IReadOnlyList(Of Card)
    Function GetById(id As Integer) As Card
    Sub Save(card As Card)
End Interface