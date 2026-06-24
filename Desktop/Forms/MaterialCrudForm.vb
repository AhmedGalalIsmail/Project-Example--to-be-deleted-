Option Strict On

Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Windows.Forms

Public Class MaterialCrudForm
	Inherits Form

	Private ReadOnly _dataFile As String = Path.Combine(AppContext.BaseDirectory, "data", "material_blocks.json")
	Private ReadOnly _items As New BindingList(Of CatalogItem)()
	Private ReadOnly _tree As New TreeView()
	Private ReadOnly _searchBox As New TextBox()
	Private ReadOnly _editorScroll As New Panel()
	Private ReadOnly _editorRoot As New TableLayoutPanel()
	Private _kindLabel As Label
	Private ReadOnly _codeBox As New TextBox()
	Private ReadOnly _nameBox As New TextBox()
	Private ReadOnly _dimensionBox As New ComboBox()
	Private ReadOnly _unitBox As New TextBox()
	Private ReadOnly _descriptionBox As New TextBox()
	Private ReadOnly _densityBox As New ComboBox()
	Private ReadOnly _priceBox As New TextBox()
	Private ReadOnly _componentGrid As New DataGridView()
	Private _dirtyLabel As Label
	Private _saveButton As Button
	Private _revertButton As Button
	Private _deleteButton As Button
	Private _newBlockButton As Button
	Private _newMaterialButton As Button
	Private _addRowButton As Button
	Private ReadOnly _toolbar As New Panel()
	Private ReadOnly _footer As New Panel()

	Private _currentItem As CatalogItem
	Private _loading As Boolean
	Private _dirty As Boolean

	Public Sub New()
		Text = "Materials & Blocks"
		MinimumSize = New Size(1120, 720)
		StartPosition = FormStartPosition.CenterParent
		BackColor = Color.FromArgb(31, 33, 37)
		ForeColor = Color.FromArgb(217, 224, 238)

		BuildUi()
		LoadCatalog()
		RefreshTree()
		ShowCurrent(Nothing)
	End Sub

	Private Sub BuildUi()
		Dim root As New TableLayoutPanel With {
			.Dock = DockStyle.Fill,
			.ColumnCount = 2,
			.RowCount = 3,
			.BackColor = BackColor,
			.Padding = New Padding(0)
		}
		root.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 320))
		root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
		root.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
		root.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
		root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

		BuildTitleBar(root)
		BuildToolbar(root)
		BuildTreePanel(root)
		BuildEditorPanel(root)

		Controls.Add(root)
	End Sub

	Private Sub BuildTitleBar(root As TableLayoutPanel)
		Dim titleBar As New Panel With {.Dock = DockStyle.Fill, .BackColor = Color.FromArgb(43, 46, 52)}
		Dim title As New Label With {
			.Dock = DockStyle.Left,
			.AutoSize = True,
			.Padding = New Padding(12, 8, 0, 0),
			.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
			.Text = "Materials & Blocks",
			.ForeColor = Color.White
		}
		titleBar.Controls.Add(title)
		root.Controls.Add(titleBar, 0, 0)
		root.SetColumnSpan(titleBar, 2)
	End Sub

	Private Sub BuildToolbar(root As TableLayoutPanel)
		_toolbar.Dock = DockStyle.Fill
		_toolbar.BackColor = Color.FromArgb(37, 40, 45)
		_toolbar.Padding = New Padding(10, 6, 10, 6)

		Dim searchPanel As New Panel With {.Dock = DockStyle.Left, .Width = 320}
		_searchBox.Dock = DockStyle.Fill
		_searchBox.PlaceholderText = "Search materials and blocks"
		_searchBox.BackColor = Color.FromArgb(31, 33, 37)
		_searchBox.ForeColor = Color.FromArgb(217, 224, 238)
		AddHandler _searchBox.TextChanged, AddressOf OnSearchChanged
		searchPanel.Controls.Add(_searchBox)
		_toolbar.Controls.Add(searchPanel)

		Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Right, .AutoSize = True, .WrapContents = False, .BackColor = Color.Transparent}
		_newMaterialButton = MakeToolbarButton("? Material", AddressOf OnNewMaterial)
		_newBlockButton = MakeToolbarButton("? Block", AddressOf OnNewBlock)
		_deleteButton = MakeToolbarButton("?? Delete", AddressOf OnDelete)
		buttons.Controls.AddRange({_deleteButton, _newMaterialButton, _newBlockButton})
		_toolbar.Controls.Add(buttons)

		root.Controls.Add(_toolbar, 0, 1)
		root.SetColumnSpan(_toolbar, 2)
	End Sub

	Private Function MakeToolbarButton(text As String, handler As EventHandler) As Button
		Dim btn As New Button With {
			.Text = text,
			.AutoSize = True,
			.Height = 26,
			.Margin = New Padding(6, 0, 0, 0),
			.FlatStyle = FlatStyle.Flat,
			.BackColor = Color.FromArgb(54, 58, 64),
			.ForeColor = Color.White
		}
		btn.FlatAppearance.BorderSize = 0
		AddHandler btn.Click, handler
		Return btn
	End Function

	Private Sub BuildTreePanel(root As TableLayoutPanel)
		Dim treePanel As New Panel With {.Dock = DockStyle.Fill, .BackColor = Color.FromArgb(37, 40, 45), .Padding = New Padding(10)}
		Dim header As New Label With {
			.Dock = DockStyle.Top,
			.Height = 24,
			.Text = "Library",
			.ForeColor = Color.FromArgb(217, 224, 238),
			.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
		}
		_tree.Dock = DockStyle.Fill
		_tree.BackColor = Color.FromArgb(31, 33, 37)
		_tree.ForeColor = Color.FromArgb(217, 224, 238)
		_tree.BorderStyle = BorderStyle.FixedSingle
		AddHandler _tree.AfterSelect, AddressOf OnTreeSelected
		treePanel.Controls.Add(_tree)
		treePanel.Controls.Add(header)
		root.Controls.Add(treePanel, 0, 2)
	End Sub

	Private Sub BuildEditorPanel(root As TableLayoutPanel)
		_editorScroll.Dock = DockStyle.Fill
		_editorScroll.AutoScroll = True
		_editorScroll.BackColor = Color.FromArgb(31, 33, 37)
		_editorScroll.Padding = New Padding(16)

		_editorRoot.Dock = DockStyle.Top
		_editorRoot.AutoSize = True
		_editorRoot.ColumnCount = 2
		_editorRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160))
		_editorRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
		_editorRoot.BackColor = Color.FromArgb(31, 33, 37)

		KindRow("Kind")
		TextRow("Code", _codeBox)
		TextRow("Display Name", _nameBox)
		ComboRow("Dimension Mode", _dimensionBox, {"D0 — Count", "D1 — Length", "D2 — Area", "D3 — Volume"})
		TextRow("Unit", _unitBox)
		TextRow("Description", _descriptionBox, multiline:=True, height:=72)
		ComboRow("Density", _densityBox, {"N/A", "Light", "Medium", "Heavy"})
		TextRow("Base Price", _priceBox)

		BuildComponentGrid()

		Dim componentHeader As New Label With {
			.Dock = DockStyle.Top,
			.Height = 24,
			.Text = "Composition",
			.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
			.ForeColor = Color.FromArgb(217, 224, 238),
			.Padding = New Padding(0, 12, 0, 6)
		}
		_editorRoot.Controls.Add(componentHeader, 0, _editorRoot.RowCount)
		_editorRoot.SetColumnSpan(componentHeader, 2)
		_editorRoot.RowCount += 1

		_editorRoot.Controls.Add(_componentGrid, 0, _editorRoot.RowCount)
		_editorRoot.SetColumnSpan(_componentGrid, 2)
		_editorRoot.RowCount += 1

		_addRowButton = MakeToolbarButton("? Add Component", AddressOf OnAddComponent)
		_editorRoot.Controls.Add(_addRowButton, 1, _editorRoot.RowCount)
		_editorRoot.RowCount += 1

		BuildFooter()
		_editorRoot.Controls.Add(_footer, 0, _editorRoot.RowCount)
		_editorRoot.SetColumnSpan(_footer, 2)

		_editorScroll.Controls.Add(_editorRoot)
		root.Controls.Add(_editorScroll, 1, 2)
	End Sub

	Private Sub KindRow(label As String)
		Dim title As New Label With {.Dock = DockStyle.Fill, .ForeColor = Color.White, .TextAlign = ContentAlignment.MiddleLeft}
		_editorRoot.Controls.Add(New Label With {.Text = label, .Dock = DockStyle.Fill, .ForeColor = Color.FromArgb(140, 146, 154)}, 0, _editorRoot.RowCount)
		_editorRoot.Controls.Add(title, 1, _editorRoot.RowCount)
		_editorRoot.RowCount += 1
	End Sub

	Private Sub TextRow(label As String, box As TextBox, Optional multiline As Boolean = False, Optional height As Integer = 28)
		_editorRoot.Controls.Add(New Label With {.Text = label, .Dock = DockStyle.Fill, .ForeColor = Color.FromArgb(140, 146, 154), .TextAlign = ContentAlignment.MiddleLeft}, 0, _editorRoot.RowCount)
		box.Dock = DockStyle.Fill
		box.BackColor = Color.FromArgb(37, 40, 45)
		box.ForeColor = Color.FromArgb(217, 224, 238)
		box.BorderStyle = BorderStyle.FixedSingle
		box.Multiline = multiline
		If multiline Then
			box.Height = height
			box.ScrollBars = ScrollBars.Vertical
		End If
		AddHandler box.TextChanged, AddressOf OnValueChanged
		_editorRoot.Controls.Add(box, 1, _editorRoot.RowCount)
		_editorRoot.RowCount += 1
	End Sub

	Private Sub ComboRow(label As String, combo As ComboBox, values As IEnumerable(Of String))
		_editorRoot.Controls.Add(New Label With {.Text = label, .Dock = DockStyle.Fill, .ForeColor = Color.FromArgb(140, 146, 154), .TextAlign = ContentAlignment.MiddleLeft}, 0, _editorRoot.RowCount)
		combo.Dock = DockStyle.Fill
		combo.DropDownStyle = ComboBoxStyle.DropDownList
		combo.BackColor = Color.FromArgb(37, 40, 45)
		combo.ForeColor = Color.FromArgb(217, 224, 238)
		combo.Items.AddRange(values.ToArray())
		AddHandler combo.SelectedIndexChanged, AddressOf OnValueChanged
		_editorRoot.Controls.Add(combo, 1, _editorRoot.RowCount)
		_editorRoot.RowCount += 1
	End Sub

	Private Sub BuildComponentGrid()
		_componentGrid.Dock = DockStyle.Top
		_componentGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
		_componentGrid.AllowUserToAddRows = False
		_componentGrid.AllowUserToDeleteRows = False
		_componentGrid.RowHeadersVisible = False
		_componentGrid.Height = 260
		_componentGrid.BackgroundColor = Color.FromArgb(37, 40, 45)
		_componentGrid.BorderStyle = BorderStyle.FixedSingle
		_componentGrid.EnableHeadersVisualStyles = False
		_componentGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(43, 46, 52)
		_componentGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
		_componentGrid.DefaultCellStyle.BackColor = Color.FromArgb(31, 33, 37)
		_componentGrid.DefaultCellStyle.ForeColor = Color.FromArgb(217, 224, 238)
		_componentGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204)
		_componentGrid.DefaultCellStyle.SelectionForeColor = Color.White
		_componentGrid.RowTemplate.Height = 26
		_componentGrid.Columns.Clear()
		_componentGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Material", .DataPropertyName = NameOf(ComponentRow.Name)})
		_componentGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Qty / m³", .DataPropertyName = NameOf(ComponentRow.Quantity)})
		_componentGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Unit", .DataPropertyName = NameOf(ComponentRow.Unit)})
		_componentGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Unit Price", .DataPropertyName = NameOf(ComponentRow.UnitPrice)})
		_componentGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Line Cost", .DataPropertyName = NameOf(ComponentRow.LineCost), .ReadOnly = True})
		AddHandler _componentGrid.CellEndEdit, AddressOf OnGridEdited
	End Sub

	Private Sub BuildFooter()
		_footer.Dock = DockStyle.Top
		_footer.Height = 46
		_footer.BackColor = Color.FromArgb(37, 40, 45)

		_dirtyLabel = New Label()
		_dirtyLabel.Dock = DockStyle.Left
		_dirtyLabel.Width = 320
		_dirtyLabel.Padding = New Padding(8, 14, 0, 0)
		_dirtyLabel.ForeColor = Color.FromArgb(140, 146, 154)
		_dirtyLabel.Text = "Saved"

		Dim actions As New FlowLayoutPanel With {.Dock = DockStyle.Right, .AutoSize = True, .WrapContents = False, .BackColor = Color.Transparent}
		_revertButton = MakeToolbarButton("Revert", AddressOf OnRevert)
		_saveButton = MakeToolbarButton("Save Block", AddressOf OnSave)
		_saveButton.BackColor = Color.FromArgb(224, 126, 57)
		actions.Controls.Add(_revertButton)
		actions.Controls.Add(_saveButton)

		_footer.Controls.Add(actions)
		_footer.Controls.Add(_dirtyLabel)
	End Sub

	Private Sub LoadCatalog()
		Directory.CreateDirectory(Path.GetDirectoryName(_dataFile))

		If File.Exists(_dataFile) Then
			Dim json = File.ReadAllText(_dataFile)
			Dim list = JsonSerializer.Deserialize(Of List(Of CatalogItem))(json)
			If list IsNot Nothing AndAlso list.Count > 0 Then
				_items.Clear()
				For Each item In list
					_items.Add(item)
				Next
				Return
			End If
		End If

		SeedCatalog()
		SaveCatalog()
	End Sub

	Private Sub SeedCatalog()
		_items.Clear()
		_items.Add(New CatalogItem With {
			.Kind = "Block",
			.Code = "COL-CONCRETE-01",
			.DisplayName = "Concrete Column",
			.DimensionMode = "D3",
			.Unit = "m³",
			.Description = "Standard reinforced concrete column",
			.Density = "Heavy",
			.PricePerUnit = 450D,
			.Components = New List(Of ComponentRow) From {
				New ComponentRow With {.Name = "Cement", .Quantity = 320D, .Unit = "kg", .UnitPrice = 0.18D},
				New ComponentRow With {.Name = "Sand", .Quantity = 0.52D, .Unit = "m³", .UnitPrice = 12.5D},
				New ComponentRow With {.Name = "Stone (Aggregate)", .Quantity = 0.88D, .Unit = "m³", .UnitPrice = 19.5D},
				New ComponentRow With {.Name = "Water", .Quantity = 180D, .Unit = "L", .UnitPrice = 0.02D}
			}
		})
		_items.Add(New CatalogItem With {
			.Kind = "Block",
			.Code = "BRK-WALL-01",
			.DisplayName = "Brick Wall",
			.DimensionMode = "D2",
			.Unit = "m²",
			.Description = "Brick wall quantity basis",
			.Density = "Medium",
			.PricePerUnit = 28D,
			.Components = New List(Of ComponentRow) From {
				New ComponentRow With {.Name = "Brick", .Quantity = 60D, .Unit = "pc", .UnitPrice = 0.55D},
				New ComponentRow With {.Name = "Mortar", .Quantity = 0.08D, .Unit = "m³", .UnitPrice = 8.5D}
			}
		})
		_items.Add(New CatalogItem With {
			.Kind = "Material",
			.Code = "MAT-TILE-01",
			.DisplayName = "Ceramic Tile",
			.DimensionMode = "D0",
			.Unit = "pc",
			.Description = "Standalone material",
			.Density = "N/A",
			.PricePerUnit = 2.25D,
			.Components = New List(Of ComponentRow)()
		})
	End Sub

	Private Sub SaveCatalog()
		Directory.CreateDirectory(Path.GetDirectoryName(_dataFile))
		Dim json = JsonSerializer.Serialize(_items.ToList(), New JsonSerializerOptions With {.WriteIndented = True})
		File.WriteAllText(_dataFile, json)
	End Sub

	Private Sub RefreshTree()
		_tree.BeginUpdate()
		_tree.Nodes.Clear()

		Dim filter = _searchBox.Text.Trim().ToLowerInvariant()

		Dim blockRoot = _tree.Nodes.Add("Blocks")
		Dim materialRoot = _tree.Nodes.Add("Materials")

		For Each item In _items
			If filter <> "" AndAlso Not item.DisplayName.ToLowerInvariant().Contains(filter) AndAlso Not item.Code.ToLowerInvariant().Contains(filter) Then
				Continue For
			End If

			Dim parent = If(item.Kind = "Block", blockRoot, materialRoot)
			Dim node = parent.Nodes.Add(item.DisplayName)
			node.Tag = item
		Next

		blockRoot.Expand()
		materialRoot.Expand()
		_tree.EndUpdate()
	End Sub

	Private Sub ShowCurrent(item As CatalogItem)
		_loading = True
		_currentItem = item
		_componentGrid.DataSource = Nothing

		If item Is Nothing Then
			_kindLabel.Text = "No selection"
			_codeBox.Clear()
			_nameBox.Clear()
			_dimensionBox.SelectedIndex = -1
			_unitBox.Clear()
			_descriptionBox.Clear()
			_densityBox.SelectedIndex = -1
			_priceBox.Clear()
			SetDirty(False)
			_loading = False
			Return
		End If

		_kindLabel.Text = $"{item.Kind}"
		_codeBox.Text = item.Code
		_nameBox.Text = item.DisplayName
		_dimensionBox.SelectedItem = DimensionLabel(item.DimensionMode)
		_unitBox.Text = item.Unit
		_descriptionBox.Text = item.Description
		_densityBox.SelectedItem = item.Density
		_priceBox.Text = item.PricePerUnit.ToString("0.00")
		If item.Components Is Nothing Then item.Components = New List(Of ComponentRow)()
		_componentGrid.DataSource = New BindingList(Of ComponentRow)(item.Components)
		RecalculateCosts()
		SetDirty(False)
		_loading = False
	End Sub

	Private Function DimensionLabel(mode As String) As String
		Return mode & " — " & If(mode = "D0", "Count", If(mode = "D1", "Length", If(mode = "D2", "Area", "Volume")))
	End Function

	Private Sub OnTreeSelected(sender As Object, e As TreeViewEventArgs)
		Dim item = TryCast(e.Node.Tag, CatalogItem)
		If item Is Nothing Then Return
		ShowCurrent(item)
	End Sub

	Private Sub OnSearchChanged(sender As Object, e As EventArgs)
		RefreshTree()
	End Sub

	Private Sub OnValueChanged(sender As Object, e As EventArgs)
		If _loading Then Return
		SetDirty(True)
	End Sub

	Private Sub OnGridEdited(sender As Object, e As DataGridViewCellEventArgs)
		If _loading Then Return
		RecalculateCosts()
		SetDirty(True)
	End Sub

	Private Sub RecalculateCosts()
		If _componentGrid.DataSource Is Nothing Then Return

		For Each row As DataGridViewRow In _componentGrid.Rows
			Dim qty = ParseDecimal(row.Cells(1).Value)
			Dim price = ParseDecimal(row.Cells(3).Value)
			row.Cells(4).Value = (qty * price).ToString("0.00")
		Next

		Dim total = 0D
		For Each row As DataGridViewRow In _componentGrid.Rows
			total += ParseDecimal(row.Cells(4).Value)
		Next

		_dirtyLabel.Text = If(_dirty, $"Unsaved changes — component total {total:0.00}", $"Saved — component total {total:0.00}")
	End Sub

	Private Function ParseDecimal(value As Object) As Decimal
		If value Is Nothing Then Return 0D
		Dim result As Decimal
		If Decimal.TryParse(value.ToString(), result) Then Return result
		Return 0D
	End Function

	Private Sub SetDirty(value As Boolean)
		_dirty = value
		_dirtyLabel.Text = If(value, "Unsaved changes", "Saved")
	End Sub

	Private Sub OnAddComponent(sender As Object, e As EventArgs)
		If _currentItem Is Nothing Then Return
		If _currentItem.Components Is Nothing Then _currentItem.Components = New List(Of ComponentRow)()
		_currentItem.Components.Add(New ComponentRow With {.Name = "New component", .Quantity = 1D, .Unit = "u", .UnitPrice = 0D})
		ShowCurrent(_currentItem)
		SetDirty(True)
	End Sub

	Private Sub OnNewBlock(sender As Object, e As EventArgs)
		Dim item As New CatalogItem With {
			.Kind = "Block",
			.Code = $"BLOCK-{_items.Count + 1:000}",
			.DisplayName = "New Block",
			.DimensionMode = "D3",
			.Unit = "m³",
			.Description = "",
			.Density = "Medium",
			.PricePerUnit = 0D,
			.Components = New List(Of ComponentRow)()
		}
		_items.Add(item)
		RefreshTree()
		ShowCurrent(item)
		SetDirty(True)
	End Sub

	Private Sub OnNewMaterial(sender As Object, e As EventArgs)
		Dim item As New CatalogItem With {
			.Kind = "Material",
			.Code = $"MAT-{_items.Count + 1:000}",
			.DisplayName = "New Material",
			.DimensionMode = "D0",
			.Unit = "pc",
			.Description = "",
			.Density = "N/A",
			.PricePerUnit = 0D,
			.Components = New List(Of ComponentRow)()
		}
		_items.Add(item)
		RefreshTree()
		ShowCurrent(item)
		SetDirty(True)
	End Sub

	Private Sub OnDelete(sender As Object, e As EventArgs)
		If _currentItem Is Nothing Then Return
		_items.Remove(_currentItem)
		RefreshTree()
		ShowCurrent(Nothing)
		SetDirty(True)
	End Sub

	Private Sub OnSave(sender As Object, e As EventArgs)
		If _currentItem Is Nothing Then Return

		_currentItem.Kind = If(_kindLabel.Text.Contains("Material"), "Material", "Block")
		_currentItem.Code = _codeBox.Text.Trim()
		_currentItem.DisplayName = _nameBox.Text.Trim()
		_currentItem.DimensionMode = If(_dimensionBox.SelectedIndex >= 0, _dimensionBox.SelectedItem.ToString().Split(" "c)(0), "D0")
		_currentItem.Unit = _unitBox.Text.Trim()
		_currentItem.Description = _descriptionBox.Text.Trim()
		_currentItem.Density = If(_densityBox.SelectedItem Is Nothing, "N/A", _densityBox.SelectedItem.ToString())
		Dim parsed As Decimal
		If Decimal.TryParse(_priceBox.Text, parsed) Then _currentItem.PricePerUnit = parsed

		SyncComponentsToItem()
		SaveCatalog()
		RefreshTree()
		SetDirty(False)
	End Sub

	Private Sub OnRevert(sender As Object, e As EventArgs)
		LoadCatalog()
		RefreshTree()
		If _currentItem Is Nothing Then
			ShowCurrent(Nothing)
			Return
		End If

		Dim refreshed = _items.FirstOrDefault(Function(x) x.Code = _currentItem.Code)
		ShowCurrent(refreshed)
		SetDirty(False)
	End Sub

	Private Sub SyncComponentsToItem()
		If _currentItem Is Nothing Then Return
		Dim list As New List(Of ComponentRow)()
		For Each row As DataGridViewRow In _componentGrid.Rows
			If row.IsNewRow Then Continue For
			list.Add(New ComponentRow With {
				.Name = If(row.Cells(0).Value IsNot Nothing, row.Cells(0).Value.ToString(), ""),
				.Quantity = ParseDecimal(row.Cells(1).Value),
				.Unit = If(row.Cells(2).Value IsNot Nothing, row.Cells(2).Value.ToString(), ""),
				.UnitPrice = ParseDecimal(row.Cells(3).Value)
			})
		Next
		_currentItem.Components = list
	End Sub

	Private NotInheritable Class CatalogItem
		Public Property Kind As String
		Public Property Code As String
		Public Property DisplayName As String
		Public Property DimensionMode As String
		Public Property Unit As String
		Public Property Description As String
		Public Property Density As String
		Public Property PricePerUnit As Decimal
		Public Property Components As List(Of ComponentRow)
	End Class

	Private NotInheritable Class ComponentRow
		Public Property Name As String
		Public Property Quantity As Decimal
		Public Property Unit As String
		Public Property UnitPrice As Decimal

		Public ReadOnly Property LineCost As Decimal
			Get
				Return Quantity * UnitPrice
			End Get
		End Property
	End Class
End Class
