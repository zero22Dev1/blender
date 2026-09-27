Imports System
Imports System.Drawing
Imports System.Windows.Forms

Public Class CardEditForm
    Inherits Form

    Private ReadOnly _service As CardService
    Private ReadOnly _card As Card
    Private ReadOnly _nameTextBox As TextBox
    Private ReadOnly _descriptionTextBox As TextBox
    Private ReadOnly _attackNumericUpDown As NumericUpDown
    Private ReadOnly _defenseNumericUpDown As NumericUpDown
    Private ReadOnly _saveButton As Button
    Private ReadOnly _cancelButton As Button

    Public Sub New(service As CardService, cardId As Integer)
        If service Is Nothing Then
            Throw New ArgumentNullException(NameOf(service))
        End If

        Dim cardForEdit = service.GetCardForEdit(cardId)
        If cardForEdit Is Nothing Then
            Throw New ArgumentException("指定されたカードが見つかりません。", NameOf(cardId))
        End If

        _service = service
        _card = cardForEdit
        _nameTextBox = New TextBox()
        _descriptionTextBox = New TextBox()
        _attackNumericUpDown = CreateStatControl()
        _defenseNumericUpDown = CreateStatControl()
        _saveButton = New Button()
        _cancelButton = New Button()

        InitializeForm()
        InitializeControls()
        BindCard()
    End Sub

    Private Sub InitializeForm()
        Text = "カード編集"
        StartPosition = FormStartPosition.CenterParent
        MinimumSize = New Size(460, 360)
        ClientSize = New Size(520, 390)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
    End Sub

    Private Sub InitializeControls()
        Dim table As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 5,
            .Padding = New Padding(16),
            .AutoSize = False
        }
        table.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100.0F))
        table.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        table.RowStyles.Add(New RowStyle(SizeType.Absolute, 36.0F))
        table.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        table.RowStyles.Add(New RowStyle(SizeType.Absolute, 36.0F))
        table.RowStyles.Add(New RowStyle(SizeType.Absolute, 36.0F))
        table.RowStyles.Add(New RowStyle(SizeType.Absolute, 52.0F))

        _nameTextBox.Dock = DockStyle.Fill
        _descriptionTextBox.Dock = DockStyle.Fill
        _descriptionTextBox.Multiline = True
        _descriptionTextBox.ScrollBars = ScrollBars.Vertical
        _descriptionTextBox.AcceptsReturn = True
        _descriptionTextBox.Margin = New Padding(3, 3, 3, 6)

        AddField(table, 0, "カード名", _nameTextBox)
        AddField(table, 1, "説明", _descriptionTextBox)
        AddField(table, 2, "攻撃力", _attackNumericUpDown)
        AddField(table, 3, "防御力", _defenseNumericUpDown)

        Dim buttonsPanel As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.RightToLeft,
            .WrapContents = False,
            .AutoSize = True
        }
        _saveButton.Text = "保存"
        _saveButton.Width = 90
        _saveButton.DialogResult = DialogResult.None
        AddHandler _saveButton.Click, AddressOf SaveButton_Click

        _cancelButton.Text = "キャンセル"
        _cancelButton.Width = 90
        _cancelButton.DialogResult = DialogResult.Cancel

        buttonsPanel.Controls.Add(_cancelButton)
        buttonsPanel.Controls.Add(_saveButton)
        table.Controls.Add(buttonsPanel, 0, 4)
        table.SetColumnSpan(buttonsPanel, 2)

        Controls.Add(table)
        AcceptButton = _saveButton
        CancelButton = _cancelButton
    End Sub

    Private Shared Sub AddField(table As TableLayoutPanel, row As Integer, labelText As String, control As Control)
        Dim label As New Label With {
            .Text = labelText,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Margin = New Padding(3, 3, 6, 3)
        }
        table.Controls.Add(label, 0, row)
        table.Controls.Add(control, 1, row)
    End Sub

    Private Shared Function CreateStatControl() As NumericUpDown
        Return New NumericUpDown With {
            .Dock = DockStyle.Left,
            .Width = 140,
            .Minimum = 0,
            .Maximum = 9999,
            .ThousandsSeparator = True
        }
    End Function

    Private Sub BindCard()
        _nameTextBox.Text = _card.Name
        _descriptionTextBox.Text = _card.Description
        _attackNumericUpDown.Value = _card.Attack
        _defenseNumericUpDown.Value = _card.Defense
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As EventArgs)
        Dim editedCard = _card.Copy()
        editedCard.Name = _nameTextBox.Text.Trim()
        editedCard.Description = _descriptionTextBox.Text.Trim()
        editedCard.Attack = CInt(_attackNumericUpDown.Value)
        editedCard.Defense = CInt(_defenseNumericUpDown.Value)

        Try
            _service.SaveCard(editedCard)
            DialogResult = DialogResult.OK
            Close()
        Catch ex As ArgumentException
            MessageBox.Show(Me, ex.Message, "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            _nameTextBox.Focus()
        End Try
    End Sub
End Class