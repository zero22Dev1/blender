Imports System
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

Public Class CardListForm
    Inherits Form

    Private ReadOnly _service As CardService
    Private ReadOnly _cardsGrid As DataGridView
    Private ReadOnly _editButton As Button
    Private ReadOnly _refreshButton As Button

    Public Sub New(service As CardService)
        If service Is Nothing Then
            Throw New ArgumentNullException(NameOf(service))
        End If

        _service = service
        _cardsGrid = New DataGridView()
        _editButton = New Button()
        _refreshButton = New Button()

        InitializeForm()
        InitializeControls()
        AddHandler Load, AddressOf CardListForm_Load
    End Sub

    Private Sub InitializeForm()
        Text = "カード一覧"
        StartPosition = FormStartPosition.CenterScreen
        MinimumSize = New Size(720, 420)
        ClientSize = New Size(900, 600)
    End Sub

    Private Sub InitializeControls()
        Dim buttonsPanel As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 48,
            .Padding = New Padding(8),
            .WrapContents = False
        }

        _editButton.Text = "選択カードを編集"
        _editButton.AutoSize = True
        AddHandler _editButton.Click, AddressOf EditButton_Click

        _refreshButton.Text = "再読み込み"
        _refreshButton.AutoSize = True
        AddHandler _refreshButton.Click, AddressOf RefreshButton_Click

        buttonsPanel.Controls.Add(_editButton)
        buttonsPanel.Controls.Add(_refreshButton)

        _cardsGrid.Dock = DockStyle.Fill
        _cardsGrid.AllowUserToAddRows = False
        _cardsGrid.AllowUserToDeleteRows = False
        _cardsGrid.AutoGenerateColumns = False
        _cardsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        _cardsGrid.MultiSelect = False
        _cardsGrid.ReadOnly = True
        _cardsGrid.RowHeadersVisible = False
        _cardsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        _cardsGrid.Columns.Add(CreateTextColumn("Name", "カード名", 2.0F))
        _cardsGrid.Columns.Add(CreateTextColumn("Description", "説明", 3.0F))
        _cardsGrid.Columns.Add(CreateTextColumn("Attack", "攻撃力", 1.0F))
        _cardsGrid.Columns.Add(CreateTextColumn("Defense", "防御力", 1.0F))
        AddHandler _cardsGrid.CellDoubleClick, AddressOf CardsGrid_CellDoubleClick

        Controls.Add(_cardsGrid)
        Controls.Add(buttonsPanel)
    End Sub

    Private Shared Function CreateTextColumn(dataPropertyName As String, headerText As String, fillWeight As Single) As DataGridViewTextBoxColumn
        Return New DataGridViewTextBoxColumn With {
            .DataPropertyName = dataPropertyName,
            .HeaderText = headerText,
            .FillWeight = fillWeight,
            .SortMode = DataGridViewColumnSortMode.NotSortable
        }
    End Function

    Private Sub CardListForm_Load(sender As Object, e As EventArgs)
        RefreshCards()
    End Sub

    Private Sub RefreshButton_Click(sender As Object, e As EventArgs)
        RefreshCards()
    End Sub

    Private Sub RefreshCards()
        _cardsGrid.DataSource = Nothing
        _cardsGrid.DataSource = _service.GetCards().ToList()
        If _cardsGrid.Rows.Count > 0 Then
            _cardsGrid.Rows(0).Selected = True
        End If
    End Sub

    Private Sub EditButton_Click(sender As Object, e As EventArgs)
        OpenSelectedCard()
    End Sub

    Private Sub CardsGrid_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex >= 0 Then
            OpenSelectedCard()
        End If
    End Sub

    Private Sub OpenSelectedCard()
        Dim selectedCard = TryGetSelectedCard()
        If selectedCard Is Nothing Then
            MessageBox.Show(Me, "編集するカードを選択してください。", "カード編集", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Using editForm As New CardEditForm(_service, selectedCard.Id)
            If editForm.ShowDialog(Me) = DialogResult.OK Then
                RefreshCards()
            End If
        End Using
    End Sub

    Private Function TryGetSelectedCard() As Card
        If _cardsGrid.CurrentRow Is Nothing Then
            Return Nothing
        End If

        Return TryCast(_cardsGrid.CurrentRow.DataBoundItem, Card)
    End Function
End Class