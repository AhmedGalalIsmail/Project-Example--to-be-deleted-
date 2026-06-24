Option Strict On
Imports System.Text.Json
Imports System.Windows.Forms

Public Class BlockAssignmentForm
    Inherits Form

    Public Property BusinessJson As String = "{}"

    Private cmbBlock As ComboBox
    Private cmbDimMode As ComboBox
    Private grdParams As DataGridView
    Private chkNested As CheckBox
    Private cmbParent As ComboBox
    Private cmbRelation As ComboBox

    Public Sub New()
        Me.Text = "Assign Block Definition"
        Me.Width = 500
        Me.Height = 420
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        InitializeComponent()
        BuildUI()
    End Sub

    Private Sub BuildUI()
        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .RowCount = 7,
            .ColumnCount = 2
        }
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 35))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 65))

        layout.Controls.Add(New Label With {.Text = "Block", .Dock = DockStyle.Fill}, 0, 0)
        cmbBlock = New ComboBox With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDownList}
        cmbBlock.Items.AddRange(New Object() {"WALL-01", "DOOR-01", "WINDOW-01"})
        layout.Controls.Add(cmbBlock, 1, 0)

        layout.Controls.Add(New Label With {.Text = "Dimension Mode", .Dock = DockStyle.Fill}, 0, 1)
        cmbDimMode = New ComboBox With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDownList}
        cmbDimMode.Items.AddRange(New Object() {"D0", "D1", "D2", "D3"})
        layout.Controls.Add(cmbDimMode, 1, 1)

        layout.Controls.Add(New Label With {.Text = "Parameters", .Dock = DockStyle.Fill}, 0, 2)
        grdParams = New DataGridView With {
            .Dock = DockStyle.Fill,
            .AllowUserToAddRows = True,
            .RowHeadersVisible = False
        }
        grdParams.Columns.Add("Param", "Parameter")
        grdParams.Columns.Add("Value", "Value")
        layout.Controls.Add(grdParams, 1, 2)

        chkNested = New CheckBox With {.Text = "Nested in another element", .Dock = DockStyle.Fill}
        layout.Controls.Add(chkNested, 1, 3)

        layout.Controls.Add(New Label With {.Text = "Parent Element", .Dock = DockStyle.Fill}, 0, 4)
        cmbParent = New ComboBox With {.Dock = DockStyle.Fill}
        layout.Controls.Add(cmbParent, 1, 4)

        layout.Controls.Add(New Label With {.Text = "Relationship", .Dock = DockStyle.Fill}, 0, 5)
        cmbRelation = New ComboBox With {.Dock = DockStyle.Fill}
        cmbRelation.Items.AddRange(New Object() {"Nested", "Exclusion"})
        layout.Controls.Add(cmbRelation, 1, 5)

        Dim pnlButtons As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft}
        Dim btnOk As New Button With {.Text = "Assign"}
        AddHandler btnOk.Click, AddressOf OnAssign
        Dim btnCancel As New Button With {.Text = "Cancel"}
        AddHandler btnCancel.Click, Sub() Me.DialogResult = DialogResult.Cancel
        pnlButtons.Controls.Add(btnOk)
        pnlButtons.Controls.Add(btnCancel)
        layout.Controls.Add(pnlButtons, 1, 6)

        Me.Controls.Add(layout)
    End Sub

    Private Sub OnAssign(sender As Object, e As EventArgs)
        Dim model As New BlockAssignmentModel With {
            .BlockCode = CStr(cmbBlock.SelectedItem),
            .DimensionMode = CStr(cmbDimMode.SelectedItem)
        }

        For Each row As DataGridViewRow In grdParams.Rows
            If row.Cells(0).Value IsNot Nothing Then
                model.Parameters(CStr(row.Cells(0).Value)) = row.Cells(1).Value
            End If
        Next

        If chkNested.Checked Then
            model.Nested = New NestedInfo With {
                .ParentElementId = CStr(cmbParent.SelectedItem),
                .RelationshipType = CStr(cmbRelation.SelectedItem)
            }
        End If

        BusinessJson = JsonSerializer.Serialize(model, New JsonSerializerOptions With {.WriteIndented = True})
        Me.DialogResult = DialogResult.OK
    End Sub
End Class
