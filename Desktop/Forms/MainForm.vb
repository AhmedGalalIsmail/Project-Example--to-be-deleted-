'Filename: Desktop/Forms/MainForm.vb
Option Strict On
Imports System.Security.Cryptography
Imports Desktop.CompositionRoot
Imports Desktop.Controls
Imports Domain.Entities
Imports Domain.Services
Imports Infrastructure.IO
Imports Application
Imports Application.AI
Imports Desktop


Namespace Forms
	''' <summary>
	''' Main application window for CoNSoL-TakeOff.
	''' 
	''' Provides the primary user interface including:
	''' - Drawing canvas (CanvasControl)
	''' - Tool buttons (Select, Line, Rectangle, Circle, Ellipse, Polyline, Pan, Zoom)
	''' - File operations (New, Open, Save)
	''' - Utility functions (Grid toggle)
	''' - Status bar for feedback
	''' </summary>
	''' <remarks>
	''' Architecture:
	''' - CanvasControl: 2D drawing surface with rendering and tool support
	''' - Left Panel: Contains tool buttons and file operations
	''' - StatusStrip: Shows messages and application state
	''' - PropertiesPanel: (Future) Shows properties of selected objects
	''' 
	''' The form coordinates between UI and domain layers:
	''' - Receives user input (drawing, tool selection)
	''' - Calls application services for persistence
	''' - Updates canvas with drawing state
	''' 
	''' Related Use Cases:
	''' - UC-001: Draw shapes on canvas
	''' - UC-008: File operations (New, Open, Save)
	''' </remarks>
	Public Class MainForm
		Inherits Form

		''' <summary>Primary 2D drawing canvas control.</summary>
		Private ReadOnly _canvas As New CanvasControl With {.Dock = DockStyle.Fill}

		''' <summary>Left sidebar panel containing tool buttons.</summary>
		Private ReadOnly _left As New Panel With {.Dock = DockStyle.Left, .Width = 250}

		''' <summary>Right sidebar panel containing tool buttons.</summary>
		Private ReadOnly _right As New Panel With {.Dock = DockStyle.Right, .Width = 250}

		''' <summary>Status bar for displaying messages and state.</summary>
		Private ReadOnly _status As New StatusStrip()

		''' <summary>Properties panel for editing selected object properties (future use).</summary>
		Private ReadOnly _propertiesPanel As New PropertiesPanel()

		''' <summary>Stores the last takeoff result for export and reference.</summary>
		Private ReadOnly lastResult As TakeOffResult

		''' <summary>Current drawing layout (canvas state).</summary>
		Private CurrentLayout As CanvasLayout

		' ? CONTROL NAME: layerPanel
		Private _layerPanel As LayerPanel
		Private layerManager As New LayerManager()

		''' <summary>
		''' Initializes the main form with UI components and event handlers.
		''' </summary>
		''' <remarks>
		''' Initialization sequence (order is critical):
		''' 1. Call InitializeComponent() (designer-generated code)
		''' 2. Set form properties (title, size)
		''' 3. Create and wire tool buttons
		''' 4. Add controls to form
		''' 5. Load README files (informational)
		''' 
		''' All exceptions are logged but not re-thrown to allow application startup.
		''' </remarks>
		Public Sub New()
			_layerPanel = New LayerPanel()
			_layerPanel.Initialize(layerManager)
			Try
				Logger.Info("Initializing MainForm")
				' ? MUST BE FIRST: Designer-generated initialization
				InitializeComponent()
				' Set form properties
				Me.Text = "CoNSoL-TakeOff"
				Me.Width = 1200
				Me.Height = 800

				' Initialize UI components
				InitializeToolButtons()

				' Add controls to form

				Me.Controls.Add(_canvas)
				Me.Controls.Add(_left)
				Me.Controls.Add(_right)
				Me.Controls.Add(_status)
				'Me.Controls.Add(_layerPanel)
				Me.Controls.Add(_propertiesPanel)

				AddHandler _canvas.ElementSelected, AddressOf OnElementSelected

				' Load documentation (non-critical)
				'LoadReadmeFiles()

				Logger.Info("MainForm initialization complete")

			Catch ex As Exception
				Logger.Error($"MainForm initialization failed: {ex.Message}", ex)
				MessageBox.Show(
					$"Failed to initialize application: {ex.Message}",
					"Initialization Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error)
			End Try
		End Sub

		''' <summary>
		''' Event handler for when a canvas element is selected.
		''' Updates the properties panel to show the selected element's properties.
		''' </summary>
		''' <param name="el"></param>
		Private Sub OnElementSelected(el As CanvasElement)
			_propertiesPanel.SetElement(el)
		End Sub


		''' <summary>
		''' Initializes all tool buttons and their event handlers.
		''' </summary>
		''' <remarks>
		''' Creates buttons for:
		''' - Drawing tools: Select, Line, Rectangle, Ellipse, Polyline
		''' - Navigation: Pan, Zoom In/Out
		''' - Utilities: Toggle Grid
		''' - File ops: New Layout, Open Layout, Save Layout
		''' 
		''' Buttons are added in reverse order to left panel (docking from top).
		''' </remarks>
		Private Sub InitializeToolButtons()
			Try
				Logger.Info("Initializing tool buttons")

				' Create tool buttons
				Dim btnSelect = CreateToolButton("Select", Sub() HandleToolClick(ToolType.SelectTool, "Select"))
				Dim btnLine = CreateToolButton("Line", Sub() HandleToolClick(ToolType.Line, "Line"))
				Dim btnRect = CreateToolButton("Rectangle", Sub() HandleToolClick(ToolType.Rectangle, "Rectangle"))
				Dim btnEllipse = CreateToolButton("Ellipse", Sub() HandleToolClick(ToolType.Ellipse, "Ellipse"))
				Dim btnPolyline = CreateToolButton("Polyline", Sub() HandleToolClick(ToolType.Polyline, "Polyline"))
				Dim btnPan = CreateToolButton("Pan", Sub() HandleToolClick(ToolType.Pan, "Pan"))

				' Create utility buttons
				Dim btnZoomIn = CreateToolButton("Zoom +", Sub() HandleZoomIn())
				Dim btnZoomOut = CreateToolButton("Zoom -", Sub() HandleZoomOut())
				Dim btnGrid = CreateToolButton("Toggle Grid", Sub() HandleGridToggle())

				' Create file operation buttons
				Dim btnNew = CreateToolButton("New Layout", Sub() NewLayout(), heightOverride:=40)
				Dim btnOpen = CreateToolButton("Open Layout", Sub() OpenLayout(), heightOverride:=40)
				Dim btnSave = CreateToolButton("Save Layout", Sub() SaveLayout(), heightOverride:=40)
				Dim btnExportExcel = CreateToolButton("Export Excel", Sub() ExportExcel_Click(), heightOverride:=40)

				' Import AI button 
				Dim btnImportAI = CreateToolButton("Import", Sub() ImportAI_Click(), heightOverride:=40)
				'btnImportAI.Dock = DockStyle.Top

				' Add buttons to panel (in reverse order due to docking from top)
				_left.Controls.Add(_layerPanel)
				_left.Controls.Add(btnSave)
				_left.Controls.Add(btnOpen)
				_left.Controls.Add(btnNew)
				_left.Controls.Add(btnGrid)
				_left.Controls.Add(btnZoomOut)
				_left.Controls.Add(btnZoomIn)
				_left.Controls.Add(btnPan)
				_left.Controls.Add(btnRect)
				_left.Controls.Add(btnLine)
				_left.Controls.Add(btnSelect)
				_left.Controls.Add(btnEllipse)
				_left.Controls.Add(btnPolyline)
				_left.Controls.Add(btnExportExcel)
				_left.Controls.Add(btnImportAI)


				_right.Controls.Add(_propertiesPanel)

				layerManager.Initialize()
				layerManager.EnsureDefaultLayer()

				Logger.Info("Tool buttons created and added to panel")
			Catch ex As Exception
				Logger.Error($"Failed to initialize tool buttons: {ex.Message}", ex)
				Throw
			End Try
		End Sub

		''' <summary>
		''' Handles the Import button click event to process a drawing file using AI services.<br></br>
		''' <br></br>
		''' Workflow:<br></br>
		''' 1. Open file dialog to select image (PNG, JPG)<br></br>
		''' 2. Use AiIntakeService to process the image and extract elements and scale<br></br>
		''' 3. Show detected scale and ask for confirmation<br></br>
		''' 4. Load detected elements into a new CanvasLayout and display on canvas<br></br>
		''' 5. If user rejects detected scale, prompt for manual input
		''' </summary>

		Private Sub ImportAI_Click()

			Dim dlg As New OpenFileDialog()
			dlg.Filter = "Images|*.png;*.jpg;*.jpeg"
			If dlg.ShowDialog() <> DialogResult.OK Then Exit Sub
			' ? 1. Load background image
			Dim img = Image.FromFile(dlg.FileName)
			_canvas.SetBackgroundImage(img, 0.5F)
			' ? 2. Run AI
			Dim ai As New AiIntakeService()
			Dim result = ai.ProcessDrawing(dlg.FileName)
			If result Is Nothing OrElse result.DetectedElements Is Nothing Then
				MessageBox.Show("AI failed ?")
				Exit Sub
			End If
			' ? 3. Scale confirmation
			Dim scale = If(result.DetectedScale, "1:100")
			If MessageBox.Show($"Detected Scale: {scale}" & vbCrLf & "Confirm?",
					   "Scale",
					   MessageBoxButtons.YesNo) = DialogResult.No Then
				scale = InputBox("Enter correct scale:", "Scale", scale)
			End If
			' ? 4. Prepare layout
			Dim layout As New CanvasLayout()
			Dim defaultLayer = layerManager.AddLayer("Default") 'GetDefaultLayer()
			For Each el In result.DetectedElements
				el.Id = Guid.NewGuid()
				' ? FIX Layer binding
				If el.LayerId = Guid.Empty Then
					el.LayerId = defaultLayer.Id
				End If
				layout.Elements.Add(el)
			Next
			' ? 5. Load into canvas
			_canvas.LoadFromLayout(layout)
			MessageBox.Show($"? AI Loaded {layout.Elements.Count} elements")
		End Sub

		' Original Ver
		'Private Sub ImportAI_Click() 'sender As Object, e As EventArgs)
		'	Dim dlg As New OpenFileDialog()
		'	dlg.Filter = "Images|*.png;*.jpg;*.jpeg"
		'	If dlg.ShowDialog() <> DialogResult.OK Then Exit Sub
		'	Dim ai As New AiIntakeService()
		'	Dim result = ai.ProcessDrawing(dlg.FileName)
		'	Dim scale = result.DetectedScale
		'	If String.IsNullOrEmpty(scale) Then
		'		scale = "1:100" ' ? fallback default
		'	End If
		'	Dim confirmed = MessageBox.Show(
		'		$"Detected Scale: {scale}" & vbCrLf & "Confirm?",
		'		"Scale Detection",
		'		MessageBoxButtons.YesNo)
		'	' After confirmation
		'	Dim layout As New CanvasLayout()
		'	For Each el In result.DetectedElements
		'		layout.Elements.Add(el)
		'	Next
		'	' Fix: call instance method on the _canvas instance instead of referencing the type.
		'	' Use the same method used elsewhere in this class for consistency.
		'	_canvas.LoadFromLayout(layout)
		'	MessageBox.Show($"Detected {layout.Elements.Count} elements!")
		'	' User cancel/select No
		'	If confirmed = DialogResult.No Then
		'		scale = InputBox("Enter correct scale:", "Scale", scale)
		'	End If
		'	MessageBox.Show($"Final Scale: {scale}")
		'End Sub

		''' <summary>
		''' Exports the takeoff results to an Excel file using the ExcelExporter service.
		''' </summary>
		''' <param name=""></param>
		Private Sub ExportExcel_Click()
			If lastResult Is Nothing Then
				MessageBox.Show("No calculation result available ?")
				Exit Sub
			End If
			Dim exporter As New ExcelExporter()
			ExcelExporter.Export(lastResult, "takeoff.xlsx")
			MessageBox.Show("Exported ?")
		End Sub

		''' <summary>
		''' Creates a tool button with standard styling.
		''' </summary>
		''' <param name="text">Button label text</param>
		''' <param name="clickHandler">Event handler for button click</param>
		''' <param name="heightOverride">Optional custom height (default: 40)</param>
		''' <returns>Configured button control</returns>
		Private Function CreateToolButton(text As String, clickHandler As Action, Optional heightOverride As Integer = 40) As Button
			Dim btn = New Button With {
				.Text = text,
				.Dock = DockStyle.Top,
				.Height = heightOverride,
				.BackColor = SystemColors.Control,
				.FlatStyle = FlatStyle.Standard
			}
			AddHandler btn.Click, Sub() clickHandler()
			Return btn
		End Function

		''' <summary>Handles tool button click with logging.</summary>
		''' <param name="toolType">Tool type to activate</param>
		''' <param name="toolName">Human-readable tool name for logging</param>
		Private Sub HandleToolClick(toolType As ToolType, toolName As String)
			Try
				Logger.Info($"Tool clicked: {toolName}")
				_canvas.SetTool(toolType)
				UpdateStatusBar($"Tool: {toolName}")
			Catch ex As Exception
				Logger.Error($"Failed to set tool {toolName}: {ex.Message}", ex)
				UpdateStatusBar($"Error setting {toolName}", isError:=True)
			End Try
		End Sub

		''' <summary>
		''' Handles Zoom In button click.</summary>
		Private Sub HandleZoomIn()
			Try
				Logger.Info("Zoom In clicked")
				_canvas.ZoomIn()
				UpdateStatusBar("Zoom In")
			Catch ex As Exception
				Logger.Error($"Zoom In failed: {ex.Message}", ex)
				UpdateStatusBar("Zoom In failed", isError:=True)
			End Try
		End Sub

		''' <summary>
		''' Handles Zoom Out button click.</summary>
		Private Sub HandleZoomOut()
			Try
				Logger.Info("Zoom Out clicked")
				_canvas.ZoomOut()
				UpdateStatusBar("Zoom Out")
			Catch ex As Exception
				Logger.Error($"Zoom Out failed: {ex.Message}", ex)
				UpdateStatusBar("Zoom Out failed", isError:=True)
			End Try
		End Sub

		''' <summary>
		''' Handles Grid Toggle button click.</summary>
		Private Sub HandleGridToggle()
			Try
				Logger.Info("Grid toggle clicked")
				_canvas.ToggleGrid()
				UpdateStatusBar("Grid toggled")
			Catch ex As Exception
				Logger.Error($"Grid toggle failed: {ex.Message}", ex)
				UpdateStatusBar("Grid toggle failed", isError:=True)
			End Try
		End Sub

		''' <summary>
		''' Updates status bar message.
		''' </summary>
		''' <param name="message">Message to display</param>
		''' <param name="isError">Whether this is an error message (false for info)</param>
		Private Sub UpdateStatusBar(message As String, Optional isError As Boolean = False)
			Dim item = New ToolStripStatusLabel With {
				.Text = $"{DateTime.Now:HH:mm:ss} | {message}",
				.ForeColor = If(isError, Color.Red, SystemColors.ControlText)
			}
			_status.Items.Clear()
			_status.Items.Add(item)
		End Sub

		''' <summary>
		''' Loads and displays README files from each layer.
		''' </summary>
		''' <remarks>
		''' Non-critical function. Displays README files in message boxes.
		''' Errors are logged but do not prevent application startup.
		''' </remarks>
		Private Sub LoadReadmeFiles()
			Try
				Logger.Info("Loading README files")

				Dim readmePaths = New String() {
					"E:\Users\GoingIForMal\CoNSoL-TakeOff\Desktop\README.md", '"Desktop/README.md", 'E:\Users\GoingIForMal\CoNSoL-TakeOff\Desktop\README.md
					"E:\Users\GoingIForMal\CoNSoL-TakeOff\Infrastructure\README.md", '"Infrastructure/README.md",
					"E:\Users\GoingIForMal\CoNSoL-TakeOff\Application\README.md", '"Application/README.md",
					"E:\Users\GoingIForMal\CoNSoL-TakeOff\Domain\README.md" '"Domain/README.md"
				}

				Dim foundCount = 0

				For Each path In readmePaths
					Try
						Dim fullPath = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)
						If IO.File.Exists(fullPath) Then
							Logger.Info($"Found README: {path}")
							' Intentionally skip showing these to avoid message boxes on startup
							'Dim content = IO.File.ReadAllText(fullPath)
							'MessageBox.Show(content, $"README - {IO.Path.GetFileNameWithoutExtension(path)}", MessageBoxButtons.OK, MessageBoxIcon.Information)
							foundCount += 1
						Else
							Logger.Info($"README not found: {path}")
						End If
					Catch ex As Exception
						Logger.Warn($"Failed to load README {path}: {ex.Message}")
					End Try
				Next

				Logger.Info($"README load complete: {foundCount} files found")

			Catch ex As Exception
				Logger.Error($"Unexpected error loading README files: {ex.Message}", ex)
				' Non-critical, don't show error dialog
			End Try
		End Sub

		''' <summary>
		''' Creates a new blank drawing layout.
		''' </summary>
		''' <remarks>
		''' Clears the canvas and starts with a fresh CanvasLayout.
		''' The layout is empty until elements are drawn.
		''' </remarks>
		Private Sub NewLayout()
			Try
				Logger.Info("Creating new layout")

				' Create new layout
				CurrentLayout = New CanvasLayout()
				Logger.Info($"New layout created: {CurrentLayout.CanvasId}")

				' Clear canvas
				_canvas.Clear()

				UpdateStatusBar("New layout created")
				Logger.Info("New layout ready for drawing")

			Catch ex As Exception
				Logger.Error($"Failed to create new layout: {ex.Message}", ex)
				UpdateStatusBar("Error creating layout", isError:=True)
				MessageBox.Show(
					$"Failed to create new layout: {ex.Message}",
					"New Layout Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error)
			End Try
		End Sub

		''' <summary>
		''' Opens a previously saved drawing layout from disk.
		''' </summary>
		''' <remarks>
		''' Supports two formats:
		''' - .takeoff: Encrypted binary format
		''' - .json: Unencrypted JSON format
		''' 
		''' File format is auto-detected by extension.
		''' </remarks>
		Private Sub OpenLayout()
			Dim openPath As String = Nothing
			Try
				Logger.Info("Opening layout file dialog")
				Using ofd As New OpenFileDialog With {
					.Filter = "TakeOff (*.takeoff)|*.takeoff|JSON (*.json)|*.json",
					.Title = "Open Drawing Layout"
					}
					If ofd.ShowDialog() <> DialogResult.OK Then
						Logger.Info("Open dialog canceled")
						Return
					End If
					openPath = ofd.FileName
					Logger.Info($"Opening file: {openPath}")
				End Using
				' Determine if file is encrypted based on extension
				Dim encrypted = openPath.EndsWith(".takeoff", StringComparison.OrdinalIgnoreCase)

				' Load layout from file
				Dim store = New TakeOffFileStore(CompositionRoot.Crypto)
				CurrentLayout = store.Load(openPath, encrypted:=encrypted)
				Logger.Info($"File loaded. Layout ID: {CurrentLayout.CanvasId}")

				' Update canvas with loaded layout
				_canvas.LoadFromLayout(CurrentLayout)

				UpdateStatusBar($"Layout opened: {IO.Path.GetFileName(openPath)}")
				Logger.Info($"Layout opened successfully: {CurrentLayout.CanvasId}")

			Catch ex As Exception
				Logger.Error($"Failed to open layout: {ex.Message}", ex)
				UpdateStatusBar("Failed to open layout", isError:=True)
				MessageBox.Show(
					$"Failed to open layout: {ex.Message}",
					"Open Layout Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error)
			End Try
		End Sub

		''' <summary>
		''' Saves the current drawing layout to disk.
		''' </summary>
		''' <remarks>
		''' Supports two formats:
		''' - .takeoff: Encrypted binary format (encrypted with AES + HMAC)
		''' - .json: Unencrypted JSON format
		''' 
		''' File format is auto-detected by extension.
		''' A random nonce is generated for each save to ensure encryption security.
		''' </remarks>
		Private Sub SaveLayout()
			Using sfd As New SaveFileDialog With {.Filter = "TakeOff (*.takeoff)|*.takeoff|JSON (*.json)|*.json"}
				If sfd.ShowDialog() = DialogResult.OK Then
					Dim store = New TakeOffFileStore(CompositionRoot.Crypto)
					Dim encrypted = sfd.FileName.EndsWith(".takeoff", StringComparison.OrdinalIgnoreCase)
					Dim nonce = New Byte(11) {}
					RandomNumberGenerator.Fill(nonce)
					Dim layout = _canvas.ToLayout()
					store.Save(sfd.FileName, layout, encrypt:=encrypted, nonce:=nonce)
					CompositionRoot.Logger.Info("Layout saved")
				End If
			End Using
		End Sub

		'Private Sub SaveLayout()
		'    Dim savePath As String = Nothing

		'    Try
		'        Logger.Info("Saving layout file dialog")

		'        Using sfd As New SaveFileDialog With {
		'            .Filter = "TakeOff (*.takeoff)|*.takeoff|JSON (*.json)|*.json",
		'            .Title = "Save Drawing Layout",
		'            .DefaultExt = ".takeoff"
		'        }

		'            If sfd.ShowDialog() <> DialogResult.OK Then
		'                Logger.Info("Save dialog canceled")
		'                Return
		'            End If

		'            savePath = sfd.FileName
		'            Logger.Info($"Saving to file: {savePath}")

		'        End Using

		'        ' Validate layout before saving
		'        If CurrentLayout Is Nothing Then
		'            Logger.Warn("No layout to save (CurrentLayout is Nothing)")
		'            UpdateStatusBar("No layout to save", isError:=True)
		'            Return
		'        End If

		'        ' Determine if file should be encrypted based on extension
		'        Dim encrypted = savePath.EndsWith(".takeoff", StringComparison.OrdinalIgnoreCase)

		'        ' Generate random nonce for encryption
		'        Dim nonce = New Byte(11) {}
		'        RandomNumberGenerator.Fill(nonce)

		'        ' Convert canvas to layout and save
		'        Dim layout = _canvas.ToLayout()
		'        Logger.Info($"Canvas converted to layout (elements: {layout.Elements.Count})")

		'        ' Save file
		'        Dim store = New TakeOffFileStore(CompositionRoot.Crypto)
		'        store.Save(savePath, layout, encrypt:=encrypted, nonce:=nonce)

		'        UpdateStatusBar($"Layout saved: {IO.Path.GetFileName(savePath)}")
		'        Logger.Info($"Layout saved successfully: {savePath}")

		'    Catch ex As Exception
		'        Logger.Error($"Failed to save layout: {ex.Message}", ex)
		'        UpdateStatusBar("Failed to save layout", isError:=True)
		'        MessageBox.Show(
		'            $"Failed to save layout: {ex.Message}",
		'            "Save Layout Error",
		'            MessageBoxButtons.OK,
		'            MessageBoxIcon.Error)
		'    End Try
		'End Sub

		''' <summary>
		''' Designer-generated initialization method (auto-generated code).
		''' </summary>
		''' <remarks>
		''' This method is generated by the Windows Forms designer.
		''' It initializes designer-managed components.
		''' 
		''' WARNING: Do not manually edit this method. Changes may be lost
		''' when the designer regenerates the code.
		''' </remarks>
		Private Sub InitializeComponent()
			SuspendLayout()
			' 
			' MainForm
			' 
			ClientSize = New Size(606, 428)
			Name = "MainForm"
			ResumeLayout(False)
		End Sub
	End Class
End Namespace

' V1.1 Original version
'Namespace Forms
'    Public Class MainForm
'        Inherits Form

'        Private ReadOnly _canvas As New CanvasControl With {.Dock = DockStyle.Fill}
'        Private ReadOnly _left As New Panel With {.Dock = DockStyle.Left, .Width = 250}
'        Private ReadOnly _status As New StatusStrip()
'        Private ReadOnly _propertiesPanel As New PropertiesPanel()

'        Private CurrentLayout As CanvasLayout

'        Public Sub New()
'            InitializeComponent()
'            Me.Text = "CoNSoL-TakeOff (WinForms)"
'            Me.Width = 1200
'            Me.Height = 800
'            Me.Controls.Add(_propertiesPanel)

'            ' Tools
'            ' Select, Line, Rectangle, Ellipse, Polyline, Pan, Zoom In/Out, Toggle Grid

'            ' Select
'            Dim btnSelect As New Button With {.Text = "Select", .Dock = DockStyle.Top, .Height = 34}
'            AddHandler btnSelect.Click, Sub() _canvas.SetTool(ToolType.SelectTool)

'            ' Line
'            Dim btnLine As New Button With {.Text = "Line", .Dock = DockStyle.Top, .Height = 34}
'            AddHandler btnLine.Click, Sub() _canvas.SetTool(ToolType.Line)

'            ' Rectangle
'            Dim btnRect As New Button With {.Text = "Rectangle", .Dock = DockStyle.Top, .Height = 34}
'            AddHandler btnRect.Click, Sub() _canvas.SetTool(ToolType.Rectangle)

'            ' Ellipse
'            Dim btnEllipse As New Button With {.Text = "Ellipse", .Dock = DockStyle.Top, .Height = 34}
'            AddHandler btnEllipse.Click, Sub() _canvas.SetTool(ToolType.Ellipse)

'            ' Polyline 
'            Dim btnPolyline As New Button With {.Text = "Polyline", .Dock = DockStyle.Top, .Height = 34}
'            AddHandler btnPolyline.Click, Sub() _canvas.SetTool(ToolType.Polyline)

'            ' Pan
'            Dim btnPan As New Button With {.Text = "Pan", .Dock = DockStyle.Top, .Height = 34}
'            AddHandler btnPan.Click, Sub() _canvas.SetTool(ToolType.Pan)

'            ' Zoom In/Out
'            Dim btnZoomIn As New Button With {.Text = "Zoom +", .Dock = DockStyle.Top, .Height = 34}
'            AddHandler btnZoomIn.Click, Sub() _canvas.ZoomIn()

'            ' Zoom Out
'            Dim btnZoomOut As New Button With {.Text = "Zoom -", .Dock = DockStyle.Top, .Height = 34}
'            AddHandler btnZoomOut.Click, Sub() _canvas.ZoomOut()

'            ' Toggle Grid
'            Dim btnGrid As New Button With {.Text = "Toggle Grid", .Dock = DockStyle.Top, .Height = 34}
'            AddHandler btnGrid.Click, Sub() _canvas.ToggleGrid()

'            ' Layout management
'            Dim btnNew As New Button With {.Text = "New Layout", .Dock = DockStyle.Top, .Height = 40}
'            AddHandler btnNew.Click, Sub() NewLayout()

'            ' Open Layout
'            Dim btnOpen As New Button With {.Text = "Open Layout", .Dock = DockStyle.Top, .Height = 40}
'            AddHandler btnOpen.Click, Sub() OpenLayout()

'            ' Save Layout
'            Dim btnSave As New Button With {.Text = "Save Layout", .Dock = DockStyle.Top, .Height = 40}
'            AddHandler btnSave.Click, Sub() SaveLayout()

'            _left.Controls.Add(btnSave)
'            _left.Controls.Add(btnOpen)
'            _left.Controls.Add(btnNew)
'            _left.Controls.Add(btnGrid)
'            _left.Controls.Add(btnZoomOut)
'            _left.Controls.Add(btnZoomIn)
'            _left.Controls.Add(btnPan)
'            _left.Controls.Add(btnRect)
'            _left.Controls.Add(btnLine)
'            _left.Controls.Add(btnSelect)
'            _left.Controls.Add(btnEllipse)
'            _left.Controls.Add(btnPolyline)

'            Me.Controls.Add(_canvas)
'            Me.Controls.Add(_left)
'            Me.Controls.Add(_status)

'            ' Load per-project README files
'            ' LoadReadmeFiles()
'        End Sub

'        Private Sub LoadReadmeFiles()
'            Try
'                Dim readmePaths = New String() {
'                    "src/CoNSoL.Desktop/README.md",
'                    "src/CoNSoL.Infrastructure/README.md",
'                    "src/CoNSoL.Application/README.md",
'                    "src/CoNSoL.Domain/README.md"
'                }

'                For Each path In readmePaths
'                    Dim fullPath = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)
'                    If IO.File.Exists(fullPath) Then
'                        Dim content = IO.File.ReadAllText(fullPath)
'                        ' You might want to show this content in a dedicated UI component, like a TextBox or a WebBrowser control
'                        MessageBox.Show(content, $"README - {IO.Path.GetFileNameWithoutExtension(path)}", MessageBoxButtons.OK, MessageBoxIcon.Information)
'                    End If
'                Next
'            Catch ex As Exception
'                MessageBox.Show($"Error loading README files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
'            End Try
'        End Sub

'        Private Sub NewLayout()
'            CurrentLayout = New CanvasLayout()
'            CompositionRoot.Logger.Info("New layout created")
'            _canvas.Clear()
'        End Sub

'        Private Sub OpenLayout()
'            Using ofd As New OpenFileDialog With {.Filter = "TakeOff (*.takeoff)|*.takeoff|JSON (*.json)|*.json"}
'                If ofd.ShowDialog() = DialogResult.OK Then
'                    Dim store = New TakeOffFileStore(CompositionRoot.Crypto)
'                    Dim encrypted = ofd.FileName.EndsWith(".takeoff", StringComparison.OrdinalIgnoreCase)
'                    CurrentLayout = store.Load(ofd.FileName, encrypted:=encrypted)
'                    _canvas.LoadFromLayout(CurrentLayout)
'                    CompositionRoot.Logger.Info($"Loaded layout: {CurrentLayout.CanvasId}")
'                End If
'            End Using
'        End Sub

'        Private Sub InitializeComponent()

'        End Sub

'        Private Sub SaveLayout()
'            Using sfd As New SaveFileDialog With {.Filter = "TakeOff (*.takeoff)|*.takeoff|JSON (*.json)|*.json"}
'                If sfd.ShowDialog() = DialogResult.OK Then
'                    Dim store = New TakeOffFileStore(CompositionRoot.Crypto)
'                    Dim encrypted = sfd.FileName.EndsWith(".takeoff", StringComparison.OrdinalIgnoreCase)
'                    Dim nonce = New Byte(11) {}
'                    RandomNumberGenerator.Fill(nonce)
'                    Dim layout = _canvas.ToLayout()
'                    store.Save(sfd.FileName, layout, encrypt:=encrypted, nonce:=nonce)
'                    CompositionRoot.Logger.Info("Layout saved")
'                End If
'            End Using
'        End Sub
'    End Class
'End Namespace



' V10.2 added Copailot comments for documentations