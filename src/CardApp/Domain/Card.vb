Public Class Card
    Public Property Id As Integer
    Public Property Name As String
    Public Property Description As String
    Public Property Attack As Integer
    Public Property Defense As Integer

    Public Function Copy() As Card
        Return New Card With {
            .Id = Id,
            .Name = Name,
            .Description = Description,
            .Attack = Attack,
            .Defense = Defense
        }
    End Function
End Class