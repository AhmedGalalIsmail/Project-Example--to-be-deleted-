Option Strict On
Imports System.Text.Json
Imports System.Windows.Forms
Imports System.Linq
Imports Application.Services
Imports Domain.Entities
Imports Desktop.Controls.LineShape
Imports Domain.Entities.CanvasElement

Namespace Controls
    ''' <summary>
    ''' PropertiesPanel is a user control that displays and allows editing of the properties of a selected shape or canvas element. It shows information such as type, geometry, block code, dimension mode, layer, and nested relationships. It also allows editing of parameters defined in the BusinessJson of a shape. When parameters are edited, it updates the BusinessJson and raises an event to notify the change.
    ''' </summary>
    Public Class PropertiesPanel
		Inherits UserControl

		Private _shape As ShapeBase

		' ? Controls naming (your style)
		Private lblType As New Label()
		Private txtCategory As New TextBox()
		Private btnApply As New Button()

		Private _selected As CanvasElement

		Public Event BusinessJsonChanged(shape As ShapeBase)

		'    ' UI controls
		'Private lblType As Label
		Private lblGeometry As Label
		Private txtBlock As TextBox
		Private txtDimMode As TextBox
		Private gridParams As DataGridView
		Private txtLayer As TextBox
		Private txtNestedInfo As TextBox

		''' <summary>
		''' Initialize the properties panel with a fixed width and dock it to the right side of the main form. Set up the UI controls for displaying and editing the type, category, geometry, block code, dimension mode, parameters, layer, and nested relationships of a selected shape or canvas element. Add an Apply button for saving changes to the BusinessJson of a canvas element.
		''' </summary>
		Public Sub New()
			Me.Width = 200
			Me.Dock = DockStyle.Right

			lblType.Dock = DockStyle.Top
			txtCategory.Dock = DockStyle.Top
			btnApply.Dock = DockStyle.Top

			btnApply.Text = "Apply"
			BuildUI()
			AddHandler btnApply.Click, AddressOf Apply_Click

			Me.Controls.Add(btnApply)
			Me.Controls.Add(txtCategory)
			Me.Controls.Add(lblType)
		End Sub


		'    ' ---------------- UI ----------------
		''' <summary>
		''' Build the user interface of the properties panel, including labels for type and geometry, text boxes for block code, dimension mode, layer, and nested info, and a data grid for parameters. Arrange these controls in a TableLayoutPanel for organized display. The parameters grid allows editing of parameter values, which will trigger updates to the BusinessJson when edited.</summary>
		Private Sub BuildUI()
			Dim root As New TableLayoutPanel With {
				.Dock = DockStyle.Right,
				.ColumnCount = 1,
				.RowCount = 7
			}

			root.RowStyles.Add(New RowStyle(SizeType.Absolute, 60)) ' title
			root.RowStyles.Add(New RowStyle(SizeType.Absolute, 60)) ' type
			root.RowStyles.Add(New RowStyle(SizeType.Absolute, 60)) ' geometry
			root.RowStyles.Add(New RowStyle(SizeType.Absolute, 60)) ' block
			root.RowStyles.Add(New RowStyle(SizeType.Percent, 100)) ' params
			root.RowStyles.Add(New RowStyle(SizeType.Absolute, 60)) ' layer
			root.RowStyles.Add(New RowStyle(SizeType.Absolute, 50)) ' nested
			root.Controls.Add(New Label With {
			.Text = "Properties",
			.Font = New Drawing.Font("Segoe UI", 10, Drawing.FontStyle.Bold),
			.Dock = DockStyle.Fill
		}, 0, 0)

			lblType = New Label With {.Dock = DockStyle.Right}
			root.Controls.Add(lblType, 0, 1)

			lblGeometry = New Label With {.Dock = DockStyle.Right}
			root.Controls.Add(lblGeometry, 0, 2)

			Dim blockPanel As New FlowLayoutPanel With {.Dock = DockStyle.Right}
			blockPanel.Controls.Add(New Label With {.Text = "Block:"})
			txtBlock = New TextBox With {.Width = 100, .ReadOnly = True}
			blockPanel.Controls.Add(txtBlock)
			blockPanel.Controls.Add(New Label With {.Text = "Mode:"})
			txtDimMode = New TextBox With {.Width = 40, .ReadOnly = True}
			blockPanel.Controls.Add(txtDimMode)
			root.Controls.Add(blockPanel, 0, 3)

			gridParams = New DataGridView With {
				.Dock = DockStyle.Fill,
				.AllowUserToAddRows = False,
				.RowHeadersVisible = False
			}
			gridParams.Columns.Add("Param", "Parameter")
			gridParams.Columns.Add("Value", "Value")
			AddHandler gridParams.CellEndEdit, AddressOf OnParamsEdited
			root.Controls.Add(gridParams, 0, 4)

			txtLayer = New TextBox With {.Dock = DockStyle.Fill, .ReadOnly = True}
			root.Controls.Add(txtLayer, 0, 5)

			txtNestedInfo = New TextBox With {
				.Dock = DockStyle.Fill,
				.Multiline = True,
				.ReadOnly = True
			}
			root.Controls.Add(txtNestedInfo, 0, 6)

			Me.Controls.Add(root)
		End Sub

		' ---------------- Binding ----------------
		''' <summary>
		''' Bind a ShapeBase object to the properties panel, displaying its type, geometry, block code, dimension mode, layer, and any nested relationships. If the shape is null, clear the UI. If BusinessJson is present, parse it to extract block code, dimension mode, and parameters. Also check the relationships to see if this shape is nested inside another element and display that info.
		''' </summary>
		''' <param name="shape"></param>
		''' <param name="relationships"></param>
		Public Sub Bind(shape As ShapeBase, relationships As IEnumerable(Of ElementRelationship))
			_shape = shape
			gridParams.Rows.Clear()

			If shape Is Nothing Then
				ClearUI()
				Return
			End If

			lblType.Text = $"Type: {shape.GetType().Name}"
			lblGeometry.Text = $"ElementId: {shape.ElementId}"
			txtLayer.Text = $"Layer: {shape.LayerId}"

			If String.IsNullOrWhiteSpace(shape.BusinessJson) Then
				txtBlock.Text = "-"
				txtDimMode.Text = "-"
				txtNestedInfo.Text = ""
				Return
			End If

			Dim doc = JsonDocument.Parse(shape.BusinessJson)
			Dim root = doc.RootElement

			txtBlock.Text = root.GetProperty("BlockCode").GetString()
			txtDimMode.Text = root.GetProperty("DimensionMode").GetString()

			If root.TryGetProperty("Parameters", Nothing) Then
				For Each p In root.GetProperty("Parameters").EnumerateObject()
					gridParams.Rows.Add(p.Name, p.Value.ToString())
				Next
			End If

			' Dim rel = relationships.FirstOrDefault(Function(r) r.ChildElementId = shape.ElementId)
			Dim rel = GetRel(shape, relationships)

			If rel IsNot Nothing Then
				txtNestedInfo.Text =
				$"Nested in: {rel.ParentElementId}{Environment.NewLine}" &
				$"Type: {rel.RelationshipType}"
			Else
				txtNestedInfo.Text = "Not nested"
			End If
		End Sub

		''' <summary>
		''' Reset the properties panel to an empty state when no shape is selected.
		''' </summary>
		Private Sub ClearUI()
			lblType.Text = ""
			lblGeometry.Text = ""
			txtBlock.Text = ""
			txtDimMode.Text = ""
			txtLayer.Text = ""
			txtNestedInfo.Text = ""
			gridParams.Rows.Clear()
		End Sub

		''' <summary>
		''' Helper function to find the relationship where the given shape is the child. Returns null if not found.
		''' </summary>
		''' <param name="shape"></param>
		''' <param name="relationships"></param>
		''' <returns></returns>
		Private Shared Function GetRel(shape As ShapeBase, relationships As IEnumerable(Of ElementRelationship)) As ElementRelationship
			Return relationships.FirstOrDefault(Function(r) r.ChildElementId = shape.ElementId)
		End Function

		' ---------------- Editing ----------------

		''' <summary>
		''' When a parameter cell is edited, update the BusinessJson of the shape with the new parameters. This involves parsing the existing BusinessJson, updating the Parameters property, and then re-serializing it back to JSON. Finally, raise an event to notify that the BusinessJson has changed.
		''' </summary>
		''' <param name="sender"></param>
		''' <param name="e"></param>
		Private Sub OnParamsEdited(sender As Object, e As DataGridViewCellEventArgs)
			If _shape Is Nothing Then Return

			Dim doc = JsonDocument.Parse(_shape.BusinessJson)
			Dim root = doc.RootElement

			Dim params As New Dictionary(Of String, Object)
			For Each row As DataGridViewRow In gridParams.Rows
				If row.Cells(0).Value IsNot Nothing Then
					params(CStr(row.Cells(0).Value)) = row.Cells(1).Value
				End If
			Next

			Dim updated = New With {
				.BlockCode = root.GetProperty("BlockCode").GetString(),
				.DimensionMode = root.GetProperty("DimensionMode").GetString(),
				.Parameters = params
			}

			_shape.BusinessJson = JsonSerializer.Serialize(updated, New JsonSerializerOptions With {.WriteIndented = True})
			RaiseEvent BusinessJsonChanged(_shape)
		End Sub





		''' <summary>
		''' Bind a canvas element to the properties panel, displaying its type and category for editing.
		''' </summary>
		''' <param name="el"></param>
		Public Sub SetElement(el As CanvasElement)
			_selected = el
			If el Is Nothing Then
				ClearUI()
				Exit Sub
			End If
			lblType.Text = $"Type: {el.Type}"
			txtCategory.Text = GetCategory(el)
		End Sub

		''' <summary>
		''' Extract the category (BlockCode) from the BusinessJson of the canvas element. If BusinessJson is empty or invalid, returns "Unknown".
		''' </summary>
		''' <param name="el"></param>
		''' <returns></returns>
		Private Function GetCategory(el As CanvasElement) As String
			If String.IsNullOrEmpty(el.BusinessJson) Then Return "Unknown"
			Dim def = JsonSerializer.Deserialize(Of BusinessDefinition)(el.BusinessJson)
			Return def.BlockCode
		End Function

		''' <summary>
		''' When the Apply button is clicked, update the BusinessJson of the selected canvas element with the new category and a default DimensionMode. Then show a confirmation message.
		''' </summary>
		''' <param name="sender"></param>
		''' <param name="e"></param>
		Private Sub Apply_Click(sender As Object, e As EventArgs)
			If _selected Is Nothing Then Exit Sub
			Dim def As New BusinessDefinition With {
				.BlockCode = txtCategory.Text,
				.DimensionMode = "D2"
			}
			_selected.BusinessJson = JsonSerializer.Serialize(def)
			MessageBox.Show("Updated ?")
		End Sub
	End Class
End Namespace

' Original Code

'Public Class PropertiesPanel
'    Inherits UserControl

'    Private _shape As ShapeBase

'    ' UI controls
'    Private lblType As Label
'    Private lblGeometry As Label
'    Private txtBlock As TextBox
'    Private txtDimMode As TextBox
'    Private gridParams As DataGridView
'    Private txtLayer As TextBox
'    Private txtNestedInfo As TextBox

'    Public Event BusinessJsonChanged(shape As ShapeBase)

'    Public Sub New()
'        Me.Dock = DockStyle.Fill
'        BuildUI()
'    End Sub

'    ' ---------------- UI ----------------

'    Private Sub BuildUI()
'        Dim root As New TableLayoutPanel With {
'            .Dock = DockStyle.Fill,
'            .ColumnCount = 1,
'            .RowCount = 7
'        }

'        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 30)) ' title
'        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 30)) ' type
'        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 30)) ' geometry
'        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 60)) ' block
'        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100)) ' params
'        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 30)) ' layer
'        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 50)) ' nested

'        root.Controls.Add(New Label With {
'            .Text = "Properties",
'            .Font = New Drawing.Font("Segoe UI", 10, Drawing.FontStyle.Bold),
'            .Dock = DockStyle.Fill
'        }, 0, 0)

'        lblType = New Label With {.Dock = DockStyle.Fill}
'        root.Controls.Add(lblType, 0, 1)

'        lblGeometry = New Label With {.Dock = DockStyle.Fill}
'        root.Controls.Add(lblGeometry, 0, 2)

'        Dim blockPanel As New FlowLayoutPanel With {.Dock = DockStyle.Fill}
'        blockPanel.Controls.Add(New Label With {.Text = "Block:"})
'        txtBlock = New TextBox With {.Width = 100, .ReadOnly = True}
'        blockPanel.Controls.Add(txtBlock)
'        blockPanel.Controls.Add(New Label With {.Text = "Mode:"})
'        txtDimMode = New TextBox With {.Width = 40, .ReadOnly = True}
'        blockPanel.Controls.Add(txtDimMode)
'        root.Controls.Add(blockPanel, 0, 3)

'        gridParams = New DataGridView With {
'            .Dock = DockStyle.Fill,
'            .AllowUserToAddRows = False,
'            .RowHeadersVisible = False
'        }
'        gridParams.Columns.Add("Param", "Parameter")
'        gridParams.Columns.Add("Value", "Value")
'        AddHandler gridParams.CellEndEdit, AddressOf OnParamsEdited
'        root.Controls.Add(gridParams, 0, 4)

'        txtLayer = New TextBox With {.Dock = DockStyle.Fill, .ReadOnly = True}
'        root.Controls.Add(txtLayer, 0, 5)

'        txtNestedInfo = New TextBox With {
'            .Dock = DockStyle.Fill,
'            .Multiline = True,
'            .ReadOnly = True
'        }
'        root.Controls.Add(txtNestedInfo, 0, 6)

'        Me.Controls.Add(root)
'    End Sub

'    ' ---------------- Binding ----------------

'    Public Sub Bind(shape As ShapeBase, relationships As IEnumerable(Of ElementRelationship))
'        _shape = shape
'        gridParams.Rows.Clear()

'        If shape Is Nothing Then
'            ClearUI()
'            Return
'        End If

'        lblType.Text = $"Type: {shape.GetType().Name}"
'        lblGeometry.Text = $"ElementId: {shape.ElementId}"
'        txtLayer.Text = $"Layer: {shape.LayerId}"

'        If String.IsNullOrWhiteSpace(shape.BusinessJson) Then
'            txtBlock.Text = "-"
'            txtDimMode.Text = "-"
'            txtNestedInfo.Text = ""
'            Return
'        End If

'        Dim doc = JsonDocument.Parse(shape.BusinessJson)
'        Dim root = doc.RootElement

'        txtBlock.Text = root.GetProperty("BlockCode").GetString()
'        txtDimMode.Text = root.GetProperty("DimensionMode").GetString()

'        If root.TryGetProperty("Parameters", Nothing) Then
'            For Each p In root.GetProperty("Parameters").EnumerateObject()
'                gridParams.Rows.Add(p.Name, p.Value.ToString())
'            Next
'        End If

'        ' Dim rel = relationships.FirstOrDefault(Function(r) r.ChildElementId = shape.ElementId)
'        Dim rel =
'        GetRel(shape, relationships)

'        If rel IsNot Nothing Then
'            txtNestedInfo.Text =
'            $"Nested in: {rel.ParentElementId}{Environment.NewLine}" &
'            $"Type: {rel.RelationshipType}"
'        Else
'            txtNestedInfo.Text = "Not nested"
'        End If
'    End Sub


'    Private Shared Function GetRel(shape As ShapeBase, relationships As IEnumerable(Of ElementRelationship)) As ElementRelationship

'        Return relationships.FirstOrDefault(Function(r) r.ChildElementId = shape.ElementId)
'    End Function




'    Private Sub ClearUI()
'        lblType.Text = ""
'        lblGeometry.Text = ""
'        txtBlock.Text = ""
'        txtDimMode.Text = ""
'        txtLayer.Text = ""
'        txtNestedInfo.Text = ""
'        gridParams.Rows.Clear()
'    End Sub

'    Private Sub InitializeComponent()
'        SuspendLayout()
'        ' 
'        ' PropertiesPanel
'        ' 
'        Name = "PropertiesPanel"
'        Size = New System.Drawing.Size(142, 148)
'        ResumeLayout(False)

'    End Sub

'    ' ---------------- Editing ----------------

'    Private Sub OnParamsEdited(sender As Object, e As DataGridViewCellEventArgs)
'        If _shape Is Nothing Then Return

'        Dim doc = JsonDocument.Parse(_shape.BusinessJson)
'        Dim root = doc.RootElement

'        Dim params As New Dictionary(Of String, Object)
'        For Each row As DataGridViewRow In gridParams.Rows
'            If row.Cells(0).Value IsNot Nothing Then
'                params(CStr(row.Cells(0).Value)) = row.Cells(1).Value
'            End If
'        Next

'        Dim updated = New With {
'            .BlockCode = root.GetProperty("BlockCode").GetString(),
'            .DimensionMode = root.GetProperty("DimensionMode").GetString(),
'            .Parameters = params
'        }

'        _shape.BusinessJson = JsonSerializer.Serialize(updated, New JsonSerializerOptions With {.WriteIndented = True})
'        RaiseEvent BusinessJsonChanged(_shape)
'    End Sub

'End Class

