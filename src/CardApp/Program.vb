Imports System
Imports System.Collections.Generic
Imports System.Windows.Forms

Public Module Program
    <STAThread>
    Public Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Dim repository As New InMemoryCardRepository(New List(Of Card) From {
            New Card With {
                .Id = 1,
                .Name = "炎の騎士",
                .Description = "火属性のカード",
                .Attack = 120,
                .Defense = 80
            },
            New Card With {
                .Id = 2,
                .Name = "森の守護者",
                .Description = "自然属性のカード",
                .Attack = 70,
                .Defense = 140
            },
            New Card With {
                .Id = 3,
                .Name = "蒼き魔導士",
                .Description = "水属性のカード",
                .Attack = 105,
                .Defense = 95
            }
        })
        Dim service As New CardService(repository)

        Application.Run(New CardListForm(service))
    End Sub
End Module