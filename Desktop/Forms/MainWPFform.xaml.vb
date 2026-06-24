Option Strict On

Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media

Partial Public Class MainWPFform
	Inherits Window

	Private Const PixelsPerUnit As Double = 10.0R
	Private ReadOnly _layers As New ObservableCollection(Of LayerRow)()
	Private _activeTool As String = "Select"
	Private _gridEnabled As Boolean = True
	Private _snapEnabled As Boolean = True
	Private _currentZoom As Double = 1.0R
	Private _currentFile As String = ""

	Public Sub New()
		InitializeComponent()
		SeedLayers()
		LayerGrid.ItemsSource = _layers
		CanvasGridLayer.Visibility = Visibility.Visible
		ApplyZoom()
		UpdateShellState("Ready")
		UpdateToolStatus("Select")
	End Sub

	Private Sub SeedLayers()
		_layers.Clear()
		_layers.Add(New LayerRow("Walls", 4, True, False, True, "#555555"))
		_layers.Add(New LayerRow("Doors", 1, True, False, True, "#8B6914"))
		_layers.Add(New LayerRow("Slabs", 2, True, True, False, "#4e8cff"))
		_layers.Add(New LayerRow("Columns", 4, True, False, True, "#888888"))
	End Sub

	Private Sub OnOpenCrudClick(sender As Object, e As RoutedEventArgs)
		Dim dlg As New MaterialCrudFormWPF() With {.Owner = Me}
		dlg.ShowDialog()
		UpdateShellState("Materials & Blocks opened")
	End Sub

	Private Sub OnToolClick(sender As Object, e As RoutedEventArgs)
		Dim tagText As String = Nothing
		If TypeOf sender Is Button Then
			tagText = TryCast(CType(sender, Button).Tag, String)
		ElseIf TypeOf sender Is MenuItem Then
			tagText = TryCast(CType(sender, MenuItem).Tag, String)
		End If

		If String.IsNullOrWhiteSpace(tagText) Then Return
		_activeTool = tagText
		UpdateToolStatus(tagText)
	End Sub

	Private Sub OnGridClick(sender As Object, e As RoutedEventArgs)
		_gridEnabled = Not _gridEnabled
		CanvasGridLayer.Visibility = If(_gridEnabled, Visibility.Visible, Visibility.Hidden)
		UpdateShellState($"Grid {(If(_gridEnabled, "on", "off"))}")
	End Sub

	Private Sub OnZoomInClick(sender As Object, e As RoutedEventArgs)
		_currentZoom = Math.Min(2.0R, Math.Round(_currentZoom + 0.1R, 2))
		ApplyZoom()
		UpdateShellState($"Zoom {_currentZoom:P0}")
	End Sub

	Private Sub OnZoomOutClick(sender As Object, e As RoutedEventArgs)
		_currentZoom = Math.Max(0.5R, Math.Round(_currentZoom - 0.1R, 2))
		ApplyZoom()
		UpdateShellState($"Zoom {_currentZoom:P0}")
	End Sub

	Private Sub OnNewClick(sender As Object, e As RoutedEventArgs)
		SeedLayers()
		LayerGrid.Items.Refresh()
		_currentFile = ""
		UpdateShellState("New workspace created")
	End Sub

	Private Sub OnOpenClick(sender As Object, e As RoutedEventArgs)
		Using dlg As New System.Windows.Forms.OpenFileDialog()
			dlg.Filter = "Take-Off workspace (*.takeoff)|*.takeoff|All files (*.*)|*.*"
			dlg.Title = "Open workspace"
			If dlg.ShowDialog() <> System.Windows.Forms.DialogResult.OK Then Return
			LoadWorkspace(dlg.FileName)
			_currentFile = dlg.FileName
		End Using
		UpdateShellState("Workspace opened")
	End Sub

	Private Sub OnSaveClick(sender As Object, e As RoutedEventArgs)
		If String.IsNullOrWhiteSpace(_currentFile) Then
			Using dlg As New System.Windows.Forms.SaveFileDialog()
				dlg.Filter = "Take-Off workspace (*.takeoff)|*.takeoff|All files (*.*)|*.*"
				dlg.Title = "Save workspace"
				dlg.FileName = "workspace.takeoff"
				If dlg.ShowDialog() <> System.Windows.Forms.DialogResult.OK Then Return
				_currentFile = dlg.FileName
			End Using
		End If

		SaveWorkspace(_currentFile)
		UpdateShellState("Workspace saved")
	End Sub

	Private Sub OnImportClick(sender As Object, e As RoutedEventArgs)
		Using dlg As New System.Windows.Forms.OpenFileDialog()
			dlg.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp|PDF|*.pdf|All files (*.*)|*.*"
			dlg.Title = "Import drawing"
			If dlg.ShowDialog() <> System.Windows.Forms.DialogResult.OK Then Return
			UpdateShellState($"Imported {Path.GetFileName(dlg.FileName)}")
		End Using
	End Sub

	Private Sub OnExportClick(sender As Object, e As RoutedEventArgs)
		Using dlg As New System.Windows.Forms.SaveFileDialog()
			dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
			dlg.Title = "Export summary"
			dlg.FileName = "takeoff-summary.csv"
			If dlg.ShowDialog() <> System.Windows.Forms.DialogResult.OK Then Return

			Dim lines As New List(Of String) From {
				"Layer,ObjectCount,Visible,Locked,Printable,Color"
			}

			For Each layer In _layers
				lines.Add($"{EscapeCsv(layer.Name)},{layer.ObjectCount},{layer.Visible},{layer.Locked},{layer.Printable},{EscapeCsv(layer.ColorTag)}")
			Next

			File.WriteAllLines(dlg.FileName, lines, Encoding.UTF8)
		End Using

		UpdateShellState("Summary exported")
	End Sub

	Private Sub OnLayerAddClick(sender As Object, e As RoutedEventArgs)
		Dim nextIndex = _layers.Count + 1
		_layers.Add(New LayerRow($"Layer {nextIndex}", 0, True, False, True, "#7E8AA2"))
		LayerGrid.Items.Refresh()
		UpdateShellState($"Layer {nextIndex} added")
	End Sub

	Private Sub OnLayerDeleteClick(sender As Object, e As RoutedEventArgs)
		Dim selected = TryCast(LayerGrid.SelectedItem, LayerRow)
		If selected Is Nothing Then
			UpdateShellState("Select a layer before deleting")
			Return
		End If
		If _layers.Count <= 1 Then
			UpdateShellState("At least one layer must remain")
			Return
		End If

		_layers.Remove(selected)
		LayerGrid.Items.Refresh()
		UpdateShellState($"Layer {selected.Name} deleted")
	End Sub

	Private Sub OnLayerSettingsClick(sender As Object, e As RoutedEventArgs)
		UpdateShellState("Layer settings are being drafted next")
	End Sub

	Private Sub OnCanvasMouseMove(sender As Object, e As MouseEventArgs)
		Dim point = e.GetPosition(CanvasSurface)
		Dim originX = CanvasSurface.ActualWidth / 2.0R
		Dim originY = CanvasSurface.ActualHeight / 2.0R
		Dim logicalX = (point.X - originX) / (PixelsPerUnit * _currentZoom)
		Dim logicalY = (originY - point.Y) / (PixelsPerUnit * _currentZoom)
		StatusCoordsText.Text = $"Cursor: {logicalX:0.0}, {logicalY:0.0}"
	End Sub

	Private Sub OnMinimizeClick(sender As Object, e As RoutedEventArgs)
		WindowState = WindowState.Minimized
	End Sub

	Private Sub OnMaximizeClick(sender As Object, e As RoutedEventArgs)
		If WindowState = WindowState.Maximized Then
			WindowState = WindowState.Normal
		Else
			WindowState = WindowState.Maximized
		End If
	End Sub

	Private Sub OnCloseClick(sender As Object, e As RoutedEventArgs)
		Close()
	End Sub

	Private Sub ApplyZoom()
		CanvasScaleTransform.ScaleX = _currentZoom
		CanvasScaleTransform.ScaleY = _currentZoom
		StatusZoomText.Text = $"Zoom: {_currentZoom:P0}"
		StatusGridText.Text = $"Grid: {(If(_gridEnabled, "On", "Off"))} | Snap: {(If(_snapEnabled, "On", "Off"))}"
	End Sub

	Private Sub LoadWorkspace(fileName As String)
		Dim json = File.ReadAllText(fileName, Encoding.UTF8)
		Dim snapshot = JsonSerializer.Deserialize(Of WorkspaceSnapshot)(json)
		If snapshot Is Nothing Then
			UpdateShellState("Open failed: workspace file is empty")
			Return
		End If

		_layers.Clear()
		If snapshot.Layers IsNot Nothing Then
			For Each layer In snapshot.Layers
				_layers.Add(New LayerRow(layer.Name, layer.ObjectCount, layer.Visible, layer.Locked, layer.Printable, layer.ColorTag))
			Next
		End If

		If _layers.Count = 0 Then
			SeedLayers()
		End If

		_gridEnabled = snapshot.GridEnabled
		_snapEnabled = snapshot.SnapEnabled
		_activeTool = If(String.IsNullOrWhiteSpace(snapshot.ActiveTool), "Select", snapshot.ActiveTool)
		_currentZoom = If(snapshot.ZoomFactor > 0, snapshot.ZoomFactor, 1.0R)
		CanvasGridLayer.Visibility = If(_gridEnabled, Visibility.Visible, Visibility.Hidden)
		ApplyZoom()
		LayerGrid.Items.Refresh()
		UpdateToolStatus(_activeTool)
	End Sub

	Private Sub SaveWorkspace(fileName As String)
		Dim snapshot As New WorkspaceSnapshot With {
			.GridEnabled = _gridEnabled,
			.SnapEnabled = _snapEnabled,
			.ZoomFactor = _currentZoom,
			.ActiveTool = _activeTool,
			.Layers = _layers.Select(Function(layer) New LayerSnapshot With {
				.Name = layer.Name,
				.ObjectCount = layer.ObjectCount,
				.Visible = layer.Visible,
				.Locked = layer.Locked,
				.Printable = layer.Printable,
				.ColorTag = layer.ColorTag
			}).ToList()
		}

		Dim options As New JsonSerializerOptions With {.WriteIndented = True}
		File.WriteAllText(fileName, JsonSerializer.Serialize(snapshot, options), Encoding.UTF8)
	End Sub

	Private Function EscapeCsv(value As String) As String
		If value Is Nothing Then Return ""
		Dim needsQuotes = value.Contains(","c) OrElse value.Contains(""""c) OrElse value.Contains(vbCr) OrElse value.Contains(vbLf)
		Dim escaped = value.Replace("""", """""")
		Return If(needsQuotes, $"""{escaped}""", escaped)
	End Function

	Private Sub UpdateShellState(message As String)
		StatusMessageText.Text = message
		StatusSelectionText.Text = "Selection: none"
		StatusLayerText.Text = $"Layer: {If(_layers.Count > 0, _layers(0).Name, "none")}"
		StatusToolText.Text = $"Tool: {_activeTool}"
		StatusGridText.Text = $"Grid: {(If(_gridEnabled, "On", "Off"))} | Snap: {(If(_snapEnabled, "On", "Off"))}"
		StatusZoomText.Text = $"Zoom: {_currentZoom:P0}"
	End Sub

	Private Sub UpdateToolStatus(toolName As String)
		StatusToolText.Text = $"Tool: {toolName}"
		StatusMessageText.Text = $"Tool set to {toolName}"
	End Sub

	Private NotInheritable Class LayerRow
		Public Sub New(name As String, objectCount As Integer, visible As Boolean, locked As Boolean, printable As Boolean, colorTag As String)
			Me.Name = name
			Me.ObjectCount = objectCount
			Me.Visible = visible
			Me.Locked = locked
			Me.Printable = printable
			Me.ColorTag = colorTag
		End Sub

		Public Property Name As String
		Public Property ObjectCount As Integer
		Public Property Visible As Boolean
		Public Property Locked As Boolean
		Public Property Printable As Boolean
		Public Property ColorTag As String
	End Class

	Public NotInheritable Class WorkspaceSnapshot
		Public Property GridEnabled As Boolean
		Public Property SnapEnabled As Boolean
		Public Property ZoomFactor As Double
		Public Property ActiveTool As String
		Public Property Layers As List(Of LayerSnapshot)
	End Class

	Public NotInheritable Class LayerSnapshot
		Public Property Name As String
		Public Property ObjectCount As Integer
		Public Property Visible As Boolean
		Public Property Locked As Boolean
		Public Property Printable As Boolean
		Public Property ColorTag As String
	End Class
End Class
