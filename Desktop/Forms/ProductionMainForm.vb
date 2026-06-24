Option Strict On

Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.ComponentModel
Imports System.Linq
Imports System.Windows.Forms
Imports Application
Imports Application.AI
Imports Desktop.Controls
Imports Domain.Entities
Imports Infrastructure.IO

Namespace Forms
	''' <summary>
	''' Clean runtime-built main form for the production-style desktop shell.
	''' </summary>
	Public Class ProductionMainForm
		Inherits Form

		Private ReadOnly _canvas As New CanvasControl()
		Private ReadOnly _propertiesPanel As New PropertiesPanel()
		Private ReadOnly _fileStore As New TakeOffFileStore()
		Private ReadOnly _aiIntake As New AiIntakeService()

		Private ReadOnly _root As New TableLayoutPanel()
		Private ReadOnly _titleBar As New Panel()
		Private ReadOnly _titleText As New Label()
		Private ReadOnly _windowButtons As New FlowLayoutPanel()
		Private ReadOnly _menuBar As New MenuStrip()
		Private ReadOnly _toolbar As New ToolStrip()
		Private ReadOnly _contentSplit As New SplitContainer()
		Private ReadOnly _canvasHost As New TableLayoutPanel()
		Private ReadOnly _toolRail As New FlowLayoutPanel()
		Private ReadOnly _rightInspector As New TabControl()
		Private ReadOnly _propertiesTab As New TabPage("Properties")
		Private ReadOnly _tagsTab As New TabPage("Tags")
		Private ReadOnly _marksTab As New TabPage("Marks")
		Private ReadOnly _tagsPlaceholder As New Label()
		Private ReadOnly _marksPlaceholder As New Label()
		Private ReadOnly _layerArea As New Panel()
		Private ReadOnly _layerHeader As New Panel()
		Private ReadOnly _layerTitle As New Label()
		Private ReadOnly _layerButtons As New FlowLayoutPanel()
		Private ReadOnly _layerGrid As New DataGridView()
		Private ReadOnly _status As New StatusStrip()
		Private ReadOnly _statusMessage As New ToolStripStatusLabel()
		Private ReadOnly _statusSelection As New ToolStripStatusLabel()
		Private ReadOnly _statusLayer As New ToolStripStatusLabel()
		Private ReadOnly _statusTool As New ToolStripStatusLabel()
		Private ReadOnly _statusCoords As New ToolStripStatusLabel()
		Private ReadOnly _statusZoom As New ToolStripStatusLabel()
		Private ReadOnly _statusGrid As New ToolStripStatusLabel()

		Private ReadOnly _layers As New BindingList(Of LayerRow)()
		Private _currentFile As String = ""
		Private _loaded As Boolean
		Private _activeTool As ToolType = ToolType.SelectTool
		Private _gridEnabled As Boolean = True
		Private _snapEnabled As Boolean = True

		Public Sub New()
			AutoScaleMode = AutoScaleMode.Dpi
			StartPosition = FormStartPosition.CenterScreen
			Text = "CoNSoL-TakeOff"
			MinimumSize = New Size(1360, 860)
			BackColor = Color.FromArgb(31, 33, 37)

			SuspendLayout()
			BuildFrame()
			ResumeLayout(False)

			AddHandler Load, AddressOf OnLoadBuild
		End Sub

		Private Sub OnLoadBuild(sender As Object, e As EventArgs)
			If _loaded Then Return
			_loaded = True

			SeedLayers()
			BindLayerGrid()
			WireEvents()
			ResetWorkspace()
			UpdateShellState("Ready")
		End Sub

		Private Sub BuildFrame()
			_root.Dock = DockStyle.Fill
			_root.ColumnCount = 1
			_root.RowCount = 6
			_root.BackColor = BackColor
			_root.Padding = New Padding(0)
			_root.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
			_root.RowStyles.Add(New RowStyle(SizeType.Absolute, 26))
			_root.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
			_root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
			_root.RowStyles.Add(New RowStyle(SizeType.Absolute, 180))
			_root.RowStyles.Add(New RowStyle(SizeType.Absolute, 24))

			BuildTitleBar()
			BuildMenuBar()
			BuildToolbar()
			BuildContent()
			BuildLayerArea()
			BuildStatusBar()

			_root.Controls.Add(_titleBar, 0, 0)
			_root.Controls.Add(_menuBar, 0, 1)
			_root.Controls.Add(_toolbar, 0, 2)
			_root.Controls.Add(_contentSplit, 0, 3)
			_root.Controls.Add(_layerArea, 0, 4)
			_root.Controls.Add(_status, 0, 5)
			Controls.Add(_root)
			MainMenuStrip = _menuBar
		End Sub

		Private Sub BuildTitleBar()
			_titleBar.Dock = DockStyle.Fill
			_titleBar.BackColor = Color.FromArgb(43, 46, 52)
			_titleBar.Padding = New Padding(12, 0, 8, 0)

			_titleText.AutoSize = True
			_titleText.ForeColor = Color.FromArgb(217, 224, 238)
			_titleText.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
			_titleText.Text = "CoNSoL-TakeOff"
			_titleText.Dock = DockStyle.Left
			_titleText.TextAlign = ContentAlignment.MiddleLeft

			_windowButtons.Dock = DockStyle.Right
			_windowButtons.FlowDirection = FlowDirection.LeftToRight
			_windowButtons.WrapContents = False
			_windowButtons.AutoSize = True
			_windowButtons.BackColor = Color.Transparent

			For Each item In {
				CreateWindowButton("?", AddressOf MinimizeWindow),
				CreateWindowButton("?", AddressOf ToggleMaximizeWindow),
				CreateWindowButton("?", AddressOf CloseWindow)
			}
				_windowButtons.Controls.Add(item)
			Next

			_titleBar.Controls.Add(_windowButtons)
			_titleBar.Controls.Add(_titleText)
		End Sub

		Private Function CreateWindowButton(text As String, action As EventHandler) As Button
			Dim btn As New Button() With {
				.Text = text,
				.Width = 42,
				.Height = 30,
				.FlatStyle = FlatStyle.Flat,
				.BackColor = Color.FromArgb(43, 46, 52),
				.ForeColor = Color.FromArgb(200, 205, 214),
				.TabStop = False
			}
			btn.FlatAppearance.BorderSize = 0
			AddHandler btn.Click, action
			Return btn
		End Function

		Private Sub BuildMenuBar()
			_menuBar.Dock = DockStyle.Fill
			_menuBar.BackColor = Color.FromArgb(37, 40, 45)
			_menuBar.ForeColor = Color.FromArgb(217, 224, 238)
			_menuBar.RenderMode = ToolStripRenderMode.System
			_menuBar.GripStyle = ToolStripGripStyle.Hidden

			Dim fileMenu As New ToolStripMenuItem("File")
			fileMenu.DropDownItems.Add(CreateMenuItem("New", AddressOf NewLayout))
			fileMenu.DropDownItems.Add(CreateMenuItem("Open", AddressOf OpenLayout))
			fileMenu.DropDownItems.Add(CreateMenuItem("Save", AddressOf SaveLayout))
			fileMenu.DropDownItems.Add(New ToolStripSeparator())
			fileMenu.DropDownItems.Add(CreateMenuItem("Exit", AddressOf CloseWindow))

			Dim editMenu As New ToolStripMenuItem("Edit")
			editMenu.DropDownItems.Add(CreateMenuItem("Materials & Blocks", AddressOf OpenMaterialsCrud))

			Dim viewMenu As New ToolStripMenuItem("View")
			viewMenu.DropDownItems.Add(CreateMenuItem("Grid", AddressOf ToggleGrid))
			viewMenu.DropDownItems.Add(CreateMenuItem("Zoom In", AddressOf ZoomIn))
			viewMenu.DropDownItems.Add(CreateMenuItem("Zoom Out", AddressOf ZoomOut))

			Dim toolsMenu As New ToolStripMenuItem("Tools")
			toolsMenu.DropDownItems.Add(CreateMenuItem("Select", Sub() SetTool(ToolType.SelectTool)))
			toolsMenu.DropDownItems.Add(CreateMenuItem("Line", Sub() SetTool(ToolType.Line)))
			toolsMenu.DropDownItems.Add(CreateMenuItem("Rectangle", Sub() SetTool(ToolType.Rectangle)))
			toolsMenu.DropDownItems.Add(CreateMenuItem("Ellipse", Sub() SetTool(ToolType.Ellipse)))
			toolsMenu.DropDownItems.Add(CreateMenuItem("Polyline", Sub() SetTool(ToolType.Polyline)))
			toolsMenu.DropDownItems.Add(CreateMenuItem("Pan", Sub() SetTool(ToolType.Pan)))

			_menuBar.Items.AddRange(New ToolStripItem() {fileMenu, editMenu, viewMenu, toolsMenu})
		End Sub

		Private Function CreateMenuItem(text As String, handler As EventHandler) As ToolStripMenuItem
			Dim item As New ToolStripMenuItem(text)
			AddHandler item.Click, handler
			Return item
		End Function

		Private Sub BuildToolbar()
			_toolbar.Dock = DockStyle.Fill
			_toolbar.RenderMode = ToolStripRenderMode.System
			_toolbar.GripStyle = ToolStripGripStyle.Hidden
			_toolbar.BackColor = Color.FromArgb(43, 46, 52)
			_toolbar.ForeColor = Color.FromArgb(217, 224, 238)
			_toolbar.Padding = New Padding(8, 4, 8, 4)
			_toolbar.ImageScalingSize = New Size(18, 18)

			AddToolbarButton("New", AddressOf NewLayout)
			AddToolbarButton("Open", AddressOf OpenLayout)
			AddToolbarButton("Save", AddressOf SaveLayout)
			_toolbar.Items.Add(New ToolStripSeparator())
			AddToolbarButton("Import", AddressOf ImportDrawing)
			AddToolbarButton("Export", AddressOf ExportLayout)
			AddToolbarButton("Materials", AddressOf OpenMaterialsCrud)
			_toolbar.Items.Add(New ToolStripSeparator())
			AddToolbarButton("Select", Sub() SetTool(ToolType.SelectTool))
			AddToolbarButton("Line", Sub() SetTool(ToolType.Line))
			AddToolbarButton("Rect", Sub() SetTool(ToolType.Rectangle))
			AddToolbarButton("Ellipse", Sub() SetTool(ToolType.Ellipse))
			AddToolbarButton("Poly", Sub() SetTool(ToolType.Polyline))
			AddToolbarButton("Pan", Sub() SetTool(ToolType.Pan))
			_toolbar.Items.Add(New ToolStripSeparator())
			AddToolbarButton("Grid", AddressOf ToggleGrid)
			AddToolbarButton("Zoom+", AddressOf ZoomIn)
			AddToolbarButton("Zoom-", AddressOf ZoomOut)
		End Sub

		Private Sub AddToolbarButton(text As String, action As EventHandler)
			Dim btn As New ToolStripButton(text)
			btn.DisplayStyle = ToolStripItemDisplayStyle.Text
			btn.Margin = New Padding(2, 0, 2, 0)
			btn.ForeColor = Color.FromArgb(217, 224, 238)
			AddHandler btn.Click, action
			_toolbar.Items.Add(btn)
		End Sub

		Private Sub BuildContent()
			_contentSplit.Dock = DockStyle.Fill
			_contentSplit.Orientation = Orientation.Vertical
			_contentSplit.FixedPanel = FixedPanel.Panel2
			_contentSplit.SplitterDistance = 990
			_contentSplit.Panel1MinSize = 720
			_contentSplit.Panel2MinSize = 280
			_contentSplit.BackColor = Color.FromArgb(31, 33, 37)

			BuildCanvasHost()
			BuildInspector()

			_contentSplit.Panel1.Controls.Add(_canvasHost)
			_contentSplit.Panel2.Controls.Add(_rightInspector)
		End Sub

		Private Sub BuildCanvasHost()
			_canvasHost.Dock = DockStyle.Fill
			_canvasHost.ColumnCount = 2
			_canvasHost.RowCount = 1
			_canvasHost.BackColor = Color.FromArgb(31, 33, 37)
			_canvasHost.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 46))
			_canvasHost.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))

			BuildToolRail()
			_canvasHost.Controls.Add(_toolRail, 0, 0)
			_canvasHost.Controls.Add(_canvas, 1, 0)

			_canvas.Dock = DockStyle.Fill
			_canvas.BackColor = Color.White
			_canvas.Margin = New Padding(0)
		End Sub

		Private Sub BuildToolRail()
			_toolRail.Dock = DockStyle.Fill
			_toolRail.FlowDirection = FlowDirection.TopDown
			_toolRail.WrapContents = False
			_toolRail.Padding = New Padding(6, 8, 6, 8)
			_toolRail.BackColor = Color.FromArgb(37, 40, 45)

			For Each toolInfo In {
				New With {.Text = "S", .Tool = ToolType.SelectTool},
				New With {.Text = "L", .Tool = ToolType.Line},
				New With {.Text = "R", .Tool = ToolType.Rectangle},
				New With {.Text = "O", .Tool = ToolType.Ellipse},
				New With {.Text = "P", .Tool = ToolType.Polyline},
				New With {.Text = "?", .Tool = ToolType.Pan}
			}
				Dim btn As New Button() With {
					.Width = 32,
					.Height = 32,
					.Margin = New Padding(0, 0, 0, 6),
					.Text = toolInfo.Text,
					.BackColor = Color.FromArgb(54, 58, 64),
					.ForeColor = Color.FromArgb(217, 224, 238),
					.FlatStyle = FlatStyle.Flat,
					.Tag = toolInfo.Tool
				}
				btn.FlatAppearance.BorderSize = 0
				AddHandler btn.Click, AddressOf OnToolRailClick
				_toolRail.Controls.Add(btn)
			Next
		End Sub

		Private Sub BuildInspector()
			_rightInspector.Dock = DockStyle.Fill
			_rightInspector.Appearance = TabAppearance.Normal
			_rightInspector.SizeMode = TabSizeMode.Fixed
			_rightInspector.ItemSize = New Size(90, 28)
			_rightInspector.BackColor = Color.FromArgb(31, 33, 37)

			_propertiesPanel.Dock = DockStyle.Fill
			_propertiesPanel.BackColor = Color.White
			_propertiesTab.BackColor = Color.White
			_propertiesTab.Controls.Add(_propertiesPanel)

			_tagsPlaceholder.Dock = DockStyle.Fill
			_tagsPlaceholder.TextAlign = ContentAlignment.MiddleCenter
			_tagsPlaceholder.ForeColor = Color.FromArgb(140, 146, 154)
			_tagsPlaceholder.Text = "Tags view is reserved for the same selection context."
			_tagsTab.Controls.Add(_tagsPlaceholder)

			_marksPlaceholder.Dock = DockStyle.Fill
			_marksPlaceholder.TextAlign = ContentAlignment.MiddleCenter
			_marksPlaceholder.ForeColor = Color.FromArgb(140, 146, 154)
			_marksPlaceholder.Text = "Marks view is reserved for annotation and review."
			_marksTab.Controls.Add(_marksPlaceholder)

			_rightInspector.TabPages.Add(_propertiesTab)
			_rightInspector.TabPages.Add(_tagsTab)
			_rightInspector.TabPages.Add(_marksTab)
		End Sub

		Private Sub BuildLayerArea()
			_layerArea.Dock = DockStyle.Fill
			_layerArea.BackColor = Color.FromArgb(31, 33, 37)
			_layerArea.Padding = New Padding(10, 8, 10, 8)

			_layerHeader.Dock = DockStyle.Top
			_layerHeader.Height = 32
			_layerHeader.BackColor = Color.FromArgb(37, 40, 45)

			_layerTitle.Dock = DockStyle.Left
			_layerTitle.AutoSize = True
			_layerTitle.Text = "Layers"
			_layerTitle.ForeColor = Color.FromArgb(217, 224, 238)
			_layerTitle.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
			_layerTitle.Padding = New Padding(8, 7, 0, 0)

			_layerButtons.Dock = DockStyle.Right
			_layerButtons.AutoSize = True
			_layerButtons.WrapContents = False
			_layerButtons.BackColor = Color.Transparent

			Dim layerActions As String() = {"?", "?", "?"}
			For Each actionText In layerActions
				Dim btn As New Button() With {
					.Width = 30,
					.Height = 24,
					.Margin = New Padding(4, 4, 0, 4),
					.Text = actionText,
					.FlatStyle = FlatStyle.Flat,
					.BackColor = Color.FromArgb(54, 58, 64),
					.ForeColor = Color.FromArgb(217, 224, 238)
				}
				btn.FlatAppearance.BorderSize = 0
				If actionText = "?" Then
					AddHandler btn.Click, AddressOf AddLayer
				ElseIf actionText = "?" Then
					AddHandler btn.Click, AddressOf DeleteLayer
				Else
					AddHandler btn.Click, Sub() UpdateShellState("Layer settings not configured yet")
				End If
				_layerButtons.Controls.Add(btn)
			Next

			_layerHeader.Controls.Add(_layerButtons)
			_layerHeader.Controls.Add(_layerTitle)

			_layerGrid.Dock = DockStyle.Fill
			_layerGrid.BackgroundColor = Color.FromArgb(37, 40, 45)
			_layerGrid.BorderStyle = BorderStyle.None
			_layerGrid.AutoGenerateColumns = False
			_layerGrid.AllowUserToAddRows = False
			_layerGrid.AllowUserToDeleteRows = False
			_layerGrid.RowHeadersVisible = False
			_layerGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
			_layerGrid.MultiSelect = False
			_layerGrid.ReadOnly = False
			_layerGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
			_layerGrid.EnableHeadersVisualStyles = False
			_layerGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(43, 46, 52)
			_layerGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(217, 224, 238)
			_layerGrid.DefaultCellStyle.BackColor = Color.FromArgb(31, 33, 37)
			_layerGrid.DefaultCellStyle.ForeColor = Color.FromArgb(217, 224, 238)
			_layerGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204)
			_layerGrid.DefaultCellStyle.SelectionForeColor = Color.White
			_layerGrid.RowTemplate.Height = 26

			_layerGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(LayerRow.ColorTag), .HeaderText = "Color", .FillWeight = 14, .ReadOnly = True})
			_layerGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(LayerRow.Name), .HeaderText = "Name", .FillWeight = 38})
			_layerGrid.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = NameOf(LayerRow.ObjectCount), .HeaderText = "Obj", .FillWeight = 12, .ReadOnly = True})
			_layerGrid.Columns.Add(New DataGridViewCheckBoxColumn With {.DataPropertyName = NameOf(LayerRow.Visible), .HeaderText = "Vis", .FillWeight = 10})
			_layerGrid.Columns.Add(New DataGridViewCheckBoxColumn With {.DataPropertyName = NameOf(LayerRow.Locked), .HeaderText = "Lock", .FillWeight = 10})
			_layerGrid.Columns.Add(New DataGridViewCheckBoxColumn With {.DataPropertyName = NameOf(LayerRow.Printable), .HeaderText = "Print", .FillWeight = 10})
			_layerGrid.DataSource = _layers

			_layerArea.Controls.Add(_layerGrid)
			_layerArea.Controls.Add(_layerHeader)
		End Sub

		Private Sub BuildStatusBar()
			_status.Dock = DockStyle.Fill
			_status.SizingGrip = False
			_status.BackColor = Color.FromArgb(37, 40, 45)
			_status.ForeColor = Color.FromArgb(217, 224, 238)

			_statusMessage.Text = "Ready"
			_statusSelection.Text = "Selection: none"
			_statusLayer.Text = "Layer: none"
			_statusTool.Text = "Tool: Select"
			_statusCoords.Text = "Cursor: --, --"
			_statusZoom.Text = "Zoom: 100%"
			_statusGrid.Text = "Grid: On | Snap: On"

			_status.Items.AddRange(New ToolStripItem() {
				_statusMessage,
				New ToolStripStatusLabel With {.Spring = True},
				_statusSelection,
				New ToolStripStatusLabel With {.BorderSides = ToolStripStatusLabelBorderSides.Left},
				_statusLayer,
				New ToolStripStatusLabel With {.BorderSides = ToolStripStatusLabelBorderSides.Left},
				_statusTool,
				New ToolStripStatusLabel With {.BorderSides = ToolStripStatusLabelBorderSides.Left},
				_statusCoords,
				New ToolStripStatusLabel With {.BorderSides = ToolStripStatusLabelBorderSides.Left},
				_statusZoom,
				New ToolStripStatusLabel With {.BorderSides = ToolStripStatusLabelBorderSides.Left},
				_statusGrid
			})
		End Sub

		Private Sub WireEvents()
			AddHandler _canvas.ElementSelected, AddressOf OnElementSelected
			AddHandler _canvas.MouseMove, AddressOf OnCanvasMouseMove
			AddHandler _layerGrid.SelectionChanged, AddressOf OnLayerSelectionChanged
		End Sub

		Private Sub SeedLayers()
			_layers.Clear()
			_layers.Add(New LayerRow("Walls", 4, True, False, True, "?"))
			_layers.Add(New LayerRow("Doors", 1, True, False, True, "?"))
			_layers.Add(New LayerRow("Slabs", 2, True, True, False, "?"))
			_layers.Add(New LayerRow("Columns", 4, True, False, True, "?"))
			If _layers.Count > 0 Then
				_layers(0).Active = True
			End If
		End Sub

		Private Sub BindLayerGrid()
			_layerGrid.AutoGenerateColumns = False
			_layerGrid.DataSource = _layers
			If _layerGrid.Rows.Count > 0 Then
				_layerGrid.ClearSelection()
				_layerGrid.Rows(0).Selected = True
			End If
		End Sub

		Private Sub ResetWorkspace()
			_canvas.Clear()
			_canvas.SetTool(ToolType.SelectTool)
			_propertiesPanel.SetElement(Nothing)
			_activeTool = ToolType.SelectTool
			_gridEnabled = True
			_snapEnabled = True
			_statusSelection.Text = "Selection: none"
			_statusLayer.Text = $"Layer: {GetActiveLayerName()}"
			_statusTool.Text = "Tool: Select"
			_statusZoom.Text = "Zoom: 100%"
			_statusGrid.Text = "Grid: On | Snap: On"
			_statusCoords.Text = "Cursor: --, --"
		End Sub

		Private Sub SetTool(tool As ToolType)
			_activeTool = tool
			_canvas.SetTool(tool)
			_statusTool.Text = $"Tool: {tool}"
			UpdateShellState($"Tool set to {tool}")
			HighlightToolRail(tool)
		End Sub

		Private Sub OnToolRailClick(sender As Object, e As EventArgs)
			Dim btn = TryCast(sender, Button)
			If btn Is Nothing OrElse btn.Tag Is Nothing Then Return
			SetTool(CType(btn.Tag, ToolType))
		End Sub

		Private Sub HighlightToolRail(tool As ToolType)
			For Each btn As Button In _toolRail.Controls.OfType(Of Button)()
				Dim selected = CType(btn.Tag, ToolType) = tool
				btn.BackColor = If(selected, Color.FromArgb(224, 126, 57), Color.FromArgb(54, 58, 64))
				btn.ForeColor = If(selected, Color.White, Color.FromArgb(217, 224, 238))
			Next
		End Sub

		Private Sub ToggleGrid(sender As Object, e As EventArgs)
			_gridEnabled = Not _gridEnabled
			_canvas.ToggleGrid()
			_statusGrid.Text = $"Grid: {(If(_gridEnabled, "On", "Off"))} | Snap: {(If(_snapEnabled, "On", "Off"))}"
			UpdateShellState("Grid toggled")
		End Sub

		Private Sub ZoomIn(sender As Object, e As EventArgs)
			_canvas.ZoomIn()
			_statusZoom.Text = "Zoom: increased"
			UpdateShellState("Zoomed in")
		End Sub

		Private Sub ZoomOut(sender As Object, e As EventArgs)
			_canvas.ZoomOut()
			_statusZoom.Text = "Zoom: decreased"
			UpdateShellState("Zoomed out")
		End Sub

		Private Sub NewLayout(sender As Object, e As EventArgs)
			ResetWorkspace()
			_currentFile = ""
			_statusMessage.Text = "New workspace created"
		End Sub

		Private Sub OpenLayout(sender As Object, e As EventArgs)
			Using dlg As New OpenFileDialog()
				dlg.Filter = "Take-Off files (*.takeoff)|*.takeoff|All files (*.*)|*.*"
				dlg.Title = "Open project"
				If dlg.ShowDialog(Me) <> DialogResult.OK Then Return

				Dim layout = _fileStore.Load(dlg.FileName)
				_canvas.LoadFromLayout(layout)
				_currentFile = dlg.FileName
				_statusMessage.Text = "Project opened"
				_statusSelection.Text = $"Selection: {layout.Elements.Count} elements loaded"
				_statusLayer.Text = $"Layer: {GetActiveLayerName()}"
			End Using
		End Sub

		Private Sub SaveLayout(sender As Object, e As EventArgs)
			Dim layout = _canvas.ToLayout()

			If String.IsNullOrWhiteSpace(_currentFile) Then
				Using dlg As New SaveFileDialog()
					dlg.Filter = "Take-Off files (*.takeoff)|*.takeoff|All files (*.*)|*.*"
					dlg.Title = "Save project"
					dlg.FileName = "project.takeoff"
					If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
					_currentFile = dlg.FileName
				End Using
			End If

			_fileStore.Save(_currentFile, layout)
			_statusMessage.Text = "Project saved"
		End Sub

		Private Sub ExportLayout(sender As Object, e As EventArgs)
			Dim layout = _canvas.ToLayout()
			Dim result As New TakeOffResult()

			For Each element In layout.Elements
				If Not String.IsNullOrWhiteSpace(element.Type) Then
					result.Add(element.Type, 1D)
				End If
			Next

			Using dlg As New SaveFileDialog()
				dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
				dlg.Title = "Export take-off"
				dlg.FileName = "takeoff.csv"
				If dlg.ShowDialog(Me) <> DialogResult.OK Then Return

				ExcelExporter.Export(result, dlg.FileName)
				_statusMessage.Text = "Take-off exported"
			End Using
		End Sub

		Private Sub ImportDrawing(sender As Object, e As EventArgs)
			Using dlg As New OpenFileDialog()
				dlg.Filter = "Images|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
				dlg.Title = "Import drawing for AI intake"
				If dlg.ShowDialog(Me) <> DialogResult.OK Then Return

				Using sourceImage = Image.FromFile(dlg.FileName)
					_canvas.SetBackgroundImage(New Bitmap(sourceImage), 0.35F)
				End Using

				Dim result = _aiIntake.ProcessDrawing(dlg.FileName)
				Dim layout As New CanvasLayout()

				For Each element In result.DetectedElements
					layout.Elements.Add(element)
				Next

				If layout.Elements.Count > 0 Then
					_canvas.LoadFromLayout(layout)
					_statusSelection.Text = $"Selection: imported {layout.Elements.Count}"
					_statusMessage.Text = $"Imported {layout.Elements.Count} AI candidates"
				Else
					_statusMessage.Text = "Import completed, no candidates detected"
				End If
			End Using
		End Sub

		Private Sub OpenMaterialsCrud(sender As Object, e As EventArgs)
			Using dlg As New MaterialCrudForm()
				dlg.StartPosition = FormStartPosition.CenterParent
				dlg.ShowDialog(Me)
			End Using
		End Sub

		Private Sub AddLayer(sender As Object, e As EventArgs)
			Dim nextIndex = _layers.Count + 1
			_layers.Add(New LayerRow($"Layer {nextIndex}", 0, True, False, True, "?"))
			_statusMessage.Text = $"Layer {nextIndex} added"
		End Sub

		Private Sub DeleteLayer(sender As Object, e As EventArgs)
			If _layerGrid.SelectedRows.Count = 0 Then
				UpdateShellState("Select a layer before deleting")
				Return
			End If

			Dim selected = TryCast(_layerGrid.SelectedRows(0).DataBoundItem, LayerRow)
			If selected Is Nothing Then Return
			If _layers.Count <= 1 Then
				UpdateShellState("At least one layer must remain")
				Return
			End If

			_layers.Remove(selected)
			_layers(0).Active = True
			_statusMessage.Text = $"Layer {selected.Name} deleted"
			_statusLayer.Text = $"Layer: {GetActiveLayerName()}"
		End Sub

		Private Sub OnLayerSelectionChanged(sender As Object, e As EventArgs)
			If _layerGrid.SelectedRows.Count = 0 Then Return
			Dim row = TryCast(_layerGrid.SelectedRows(0).DataBoundItem, LayerRow)
			If row Is Nothing Then Return

			For Each layer In _layers
				layer.Active = Object.ReferenceEquals(layer, row)
			Next
			_statusLayer.Text = $"Layer: {row.Name}"
		End Sub

		Private Sub OnElementSelected(el As CanvasElement)
			_propertiesPanel.SetElement(el)
			_statusSelection.Text = If(el Is Nothing, "Selection: none", $"Selection: {el.Type}")
		End Sub

		Private Sub OnCanvasMouseMove(sender As Object, e As MouseEventArgs)
			Dim logical = ToLogicalPoint(e.Location)
			_statusCoords.Text = $"Cursor: {logical.X:0.##}, {logical.Y:0.##}"
		End Sub

		Private Function ToLogicalPoint(physical As Point) As PointF
			Dim zoom As Single = 1.0F
			Dim pan As New PointF(0, 0)

			Try
				Dim zoomField = GetType(CanvasControl).GetField("_zoom", BindingFlags.Instance Or BindingFlags.NonPublic)
				If zoomField IsNot Nothing Then zoom = CSng(zoomField.GetValue(_canvas))

				Dim panField = GetType(CanvasControl).GetField("_pan", BindingFlags.Instance Or BindingFlags.NonPublic)
				If panField IsNot Nothing Then pan = CType(panField.GetValue(_canvas), PointF)
			Catch
				zoom = 1.0F
				pan = New PointF(0, 0)
			End Try

			If zoom <= 0.0001F Then zoom = 1.0F
			Return New PointF((physical.X - pan.X) / zoom, (physical.Y - pan.Y) / zoom)
		End Function

		Private Sub UpdateShellState(message As String)
			_statusMessage.Text = message
			_statusLayer.Text = $"Layer: {GetActiveLayerName()}"
			_statusTool.Text = $"Tool: {_activeTool}"
			_statusGrid.Text = $"Grid: {(If(_gridEnabled, "On", "Off"))} | Snap: {(If(_snapEnabled, "On", "Off"))}"
		End Sub

		Private Function GetActiveLayerName() As String
			Dim active = _layers.FirstOrDefault(Function(x) x.Active)
			If active Is Nothing Then Return "none"
			Return active.Name
		End Function

		Private Sub MinimizeWindow(sender As Object, e As EventArgs)
			WindowState = FormWindowState.Minimized
		End Sub

		Private Sub ToggleMaximizeWindow(sender As Object, e As EventArgs)
			If WindowState = FormWindowState.Maximized Then
				WindowState = FormWindowState.Normal
			Else
				WindowState = FormWindowState.Maximized
			End If
		End Sub

		Private Sub CloseWindow(sender As Object, e As EventArgs)
			Close()
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
			Public Property Active As Boolean
		End Class
	End Class
End Namespace
