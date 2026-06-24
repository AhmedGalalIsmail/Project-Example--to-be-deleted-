Option Strict On

Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.IO
Imports System.Text.Json
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives

Partial Public Class MaterialCrudFormWPF
	Inherits Window

	Private ReadOnly _dataFile As String = Path.Combine(AppContext.BaseDirectory, "data", "material_blocks.json")
	Private ReadOnly _items As New ObservableCollection(Of CatalogItem)()
	Private _currentItem As CatalogItem
	Private _dirty As Boolean
	Private _loading As Boolean

	Public Sub New()
		InitializeComponent()
		LoadCatalog()
		RefreshTree()
		ShowCurrent(Nothing)
	End Sub

	Private Sub LoadCatalog()
		Dim folder = Path.GetDirectoryName(_dataFile)
		If Not String.IsNullOrWhiteSpace(folder) Then
			Directory.CreateDirectory(folder)
		End If

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
			.Components = New ObservableCollection(Of ComponentRow) From {
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
			.Components = New ObservableCollection(Of ComponentRow) From {
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
			.Components = New ObservableCollection(Of ComponentRow)()
		})
	End Sub

	Private Sub SaveCatalog()
		Directory.CreateDirectory(Path.GetDirectoryName(_dataFile))
		Dim json = JsonSerializer.Serialize(_items.ToList(), New JsonSerializerOptions With {.WriteIndented = True})
		File.WriteAllText(_dataFile, json)
	End Sub

	Private Sub RefreshTree(Optional filter As String = "")
		LibraryTree.Items.Clear()
		Dim blocksRoot As New TreeViewItem With {.Header = "Blocks", .IsExpanded = True}
		Dim materialsRoot As New TreeViewItem With {.Header = "Materials", .IsExpanded = True}

		For Each item In _items
			If filter <> "" AndAlso Not item.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase) AndAlso Not item.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) Then
				Continue For
			End If

			Dim node As New TreeViewItem With {.Header = item.DisplayName, .Tag = item}
			If item.Kind = "Block" Then
				blocksRoot.Items.Add(node)
			Else
				materialsRoot.Items.Add(node)
			End If
		Next

		LibraryTree.Items.Add(blocksRoot)
		LibraryTree.Items.Add(materialsRoot)
	End Sub

	Private Sub ShowCurrent(item As CatalogItem)
		_loading = True
		_currentItem = item
		ComponentsGrid.ItemsSource = Nothing

		If item Is Nothing Then
			KindBox.Text = ""
			CodeBox.Text = ""
			NameBox.Text = ""
			UnitBox.Text = ""
			DescriptionBox.Text = ""
			PriceBox.Text = ""
			ModeBox.SelectedIndex = 0
			DensityBox.SelectedIndex = 0
			ComponentsGrid.ItemsSource = Nothing
			DirtyText.Text = "Saved"
			_dirty = False
			_loading = False
			Return
		End If

		KindBox.Text = item.Kind
		CodeBox.Text = item.Code
		NameBox.Text = item.DisplayName
		UnitBox.Text = item.Unit
		DescriptionBox.Text = item.Description
		PriceBox.Text = item.PricePerUnit.ToString(CultureInfo.CurrentCulture)
		SelectComboValue(ModeBox, item.DimensionMode)
		SelectComboValue(DensityBox, item.Density)
		ComponentsGrid.ItemsSource = item.Components
		DirtyText.Text = If(_dirty, "Unsaved changes", "Saved")
		_loading = False
	End Sub

	Private Sub SelectComboValue(combo As ComboBox, value As String)
		For Each obj In combo.Items
			Dim container = TryCast(obj, ComboBoxItem)
			If container IsNot Nothing AndAlso container.Content IsNot Nothing AndAlso container.Content.ToString().StartsWith(value, StringComparison.OrdinalIgnoreCase) Then
				combo.SelectedItem = container
				Return
			End If
		Next
	End Sub

	Private Function CurrentMode() As String
		Dim item = TryCast(ModeBox.SelectedItem, ComboBoxItem)
		If item Is Nothing OrElse item.Content Is Nothing Then Return "D0"
		Return item.Content.ToString().Split(" "c)(0)
	End Function

	Private Function CurrentDensity() As String
		Dim item = TryCast(DensityBox.SelectedItem, ComboBoxItem)
		If item Is Nothing OrElse item.Content Is Nothing Then Return "N/A"
		Return item.Content.ToString()
	End Function

	Private Sub OnSearchChanged(sender As Object, e As TextChangedEventArgs)
		RefreshTree(SearchBox.Text.Trim())
	End Sub

	Private Sub OnTreeSelectionChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Object))
		Dim node = TryCast(e.NewValue, TreeViewItem)
		If node Is Nothing Then Return
		Dim item = TryCast(node.Tag, CatalogItem)
		If item Is Nothing Then Return
		ShowCurrent(item)
	End Sub

	Private Sub OnNewMaterialClick(sender As Object, e As RoutedEventArgs)
		Dim item As New CatalogItem With {
			.Kind = "Material",
			.Code = $"MAT-{_items.Count + 1:000}",
			.DisplayName = "New Material",
			.DimensionMode = "D0",
			.Unit = "pc",
			.Description = "",
			.Density = "N/A",
			.PricePerUnit = 0D,
			.Components = New ObservableCollection(Of ComponentRow)()
		}
		_items.Add(item)
		RefreshTree(SearchBox.Text.Trim())
		ShowCurrent(item)
		SetDirty(True)
	End Sub

	Private Sub OnNewBlockClick(sender As Object, e As RoutedEventArgs)
		Dim item As New CatalogItem With {
			.Kind = "Block",
			.Code = $"BLOCK-{_items.Count + 1:000}",
			.DisplayName = "New Block",
			.DimensionMode = "D3",
			.Unit = "m³",
			.Description = "",
			.Density = "Medium",
			.PricePerUnit = 0D,
			.Components = New ObservableCollection(Of ComponentRow)()
		}
		_items.Add(item)
		RefreshTree(SearchBox.Text.Trim())
		ShowCurrent(item)
		SetDirty(True)
	End Sub

	Private Sub OnDeleteClick(sender As Object, e As RoutedEventArgs)
		If _currentItem Is Nothing Then Return
		_items.Remove(_currentItem)
		RefreshTree(SearchBox.Text.Trim())
		ShowCurrent(Nothing)
		SetDirty(True)
	End Sub

	Private Sub OnAddRowClick(sender As Object, e As RoutedEventArgs)
		If _currentItem Is Nothing Then Return
		If _currentItem.Components Is Nothing Then _currentItem.Components = New ObservableCollection(Of ComponentRow)()
		_currentItem.Components.Add(New ComponentRow With {.Name = "New component", .Quantity = 1D, .Unit = "u", .UnitPrice = 0D})
		ComponentsGrid.Items.Refresh()
		SetDirty(True)
	End Sub

	Private Sub OnRevertClick(sender As Object, e As RoutedEventArgs)
		LoadCatalog()
		RefreshTree(SearchBox.Text.Trim())
		If _currentItem Is Nothing Then
			ShowCurrent(Nothing)
		Else
			Dim refreshed = _items.FirstOrDefault(Function(x) x.Code = _currentItem.Code)
			ShowCurrent(refreshed)
		End If
		SetDirty(False)
	End Sub

	Private Sub OnSaveClick(sender As Object, e As RoutedEventArgs)
		If _currentItem Is Nothing Then Return
		_currentItem.Kind = If(KindBox.Text.Contains("Material", StringComparison.OrdinalIgnoreCase), "Material", "Block")
		_currentItem.Code = CodeBox.Text.Trim()
		_currentItem.DisplayName = NameBox.Text.Trim()
		_currentItem.DimensionMode = CurrentMode()
		_currentItem.Unit = UnitBox.Text.Trim()
		_currentItem.Description = DescriptionBox.Text.Trim()
		_currentItem.Density = CurrentDensity()
		Dim parsed As Decimal
		If Decimal.TryParse(PriceBox.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, parsed) Then
			_currentItem.PricePerUnit = parsed
		End If
		SaveCatalog()
		RefreshTree(SearchBox.Text.Trim())
		SetDirty(False)
	End Sub

	Private Sub SetDirty(value As Boolean)
		_dirty = value
		DirtyText.Text = If(value, "Unsaved changes", "Saved")
	End Sub

	Private Sub OnValueChanged(sender As Object, e As EventArgs)
		If _loading Then Return
		SetDirty(True)
	End Sub

	Public NotInheritable Class CatalogItem
		Public Property Kind As String
		Public Property Code As String
		Public Property DisplayName As String
		Public Property DimensionMode As String
		Public Property Unit As String
		Public Property Description As String
		Public Property Density As String
		Public Property PricePerUnit As Decimal
		Public Property Components As ObservableCollection(Of ComponentRow)
	End Class

	Public NotInheritable Class ComponentRow
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
