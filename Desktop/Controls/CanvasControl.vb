
'Filename: Desktop/Controls/CanvasControl.vb
#Region "Info. & Imports"
'Option Strict On
'Imports System.Drawing
Imports System.Drawing.Drawing2D
'Imports System.Windows.Forms
Imports System.Text.Json
'Imports System.Linq
Imports Domain
Imports Domain.Entities
Imports Domain.Services
Imports Domain.Services.LayerManager


#End Region

#Region "canvas control"

''' <summary>
''' Interactive 2D drawing canvas with shape rendering and tool support.
''' </summary>
''' <remarks>
''' CanvasControl is the primary UI component for drawing operations.
''' It manages:
''' - Shape rendering with zoom/pan support
''' - Tool activation (Line, Rectangle, Select, Pan, Zoom)
''' - Selection highlighting and feedback
''' - Layer visibility and filtering
''' - Double-buffered rendering for smooth updates
''' 
''' Coordinate Systems:
''' - Physical: Screen pixels (0,0 at top-left)
''' - Logical: Canvas units (user coordinate system)
''' - Transform: physical = (logical * zoom) + pan
''' 
''' Related Use Cases:
''' - UC-001: Draw shapes
''' - UC-006: Edit multi-selection
''' - UC-008: Deployment (serialization)
''' </remarks>
Public Class CanvasControl
	Inherits UserControl

#Region "Fields and Properties"
	Private ReadOnly _shapes As New List(Of ShapeBase)()
	Private _selected As ShapeBase = Nothing
	Private _isDrawing As Boolean = False
	Private _startPt As PointF
	Private _currPt As PointF
	Private _tempShape As ShapeBase = Nothing
	Private _tool As ToolType = ToolType.SelectTool

	Private ReadOnly _shapeMenu As New ContextMenuStrip()

#Region "Size"
	Public Shared ReadOnly DefaultMinLogicalWindowSize As Size = New Size(2000, 2000)
	Public Shared ReadOnly DefaultMaxLogicalWindowSize As Size = New Size(100000000, 100000000)
#End Region

#Region "Dimensions and units of measurement"
	'Private myBorderStyle As BorderStyle = Windows.Forms.BorderStyle.FixedSingle
	'Private myUnitOfMeasure As MeasureSystem.enUniMis = DefaultUnitOfMeasure
	Private myMinLogicalWindowSize As Size = DefaultMinLogicalWindowSize
	Private myMaxLogicalWindowSize As Size = DefaultMaxLogicalWindowSize
#End Region

	''' <summary>Current layout being rendered.</summary>
	''' <remarks>Invariant: Never null. Set via SetLayout().</remarks>
	Private _currentLayout As CanvasLayout

	''' <summary>Zoom factor (1.0 = 100%).</summary>
	''' <remarks>Valid range: 0.1 to 10.0</remarks>
	Private _zoom As Single = 1.0F

	''' <summary>Pan offset in physical coordinates.</summary>
	''' <remarks>Represents top-left corner displacement.</remarks>
	Private _pan As PointF = New PointF(0, 0)
	Private _gridSize As Integer = 20
	Private _showGrid As Boolean = True
	Private _snapToGrid As Boolean = True
	Private _showRulers As Boolean = True

	Private _backBuffer As Bitmap = Nothing
	Private _backGraphics As Graphics = Nothing

	Public Property BusinessJson As String

	' draw image as background and apply transparency
	Private _backgroundImage As Image = Nothing
	Private _backgroundOpacity As Single = 0.5F

	Private _gridKind As GridKind = GridKind.Lines

	Private _snapEnabled As Boolean = False

	''' <summary>
	''' Event triggered when a shape is selected on the canvas.
	''' </summary>
	''' <param name="el"></param>
	Public Event ElementSelected(el As CanvasElement)
#End Region

#Region "Layout and shape management"

	''' <summary>
	''' Set a background image for the canvas with specified opacity.
	''' </summary>
	''' <param name="img"></param>
	''' <param name="opacity"></param>
	Public Sub SetBackgroundImage(img As Image, Optional opacity As Single = 0.5F)
		_backgroundImage = img
		_backgroundOpacity = opacity
		Invalidate()
	End Sub

	''' <summary>Sets the layout to render and clears selection.</summary>
	''' <param name="layout">Canvas layout to display</param>
	''' <remarks>
	''' Resets zoom/pan to defaults and clears any selection.
	''' Triggers full repaint of canvas.
	''' </remarks>
	''' <exception cref="ArgumentNullException">If layout is Nothing</exception>
	Public Sub SetLayout(layout As CanvasLayout)
		_currentLayout = layout
		_zoom = 1.0F
		_pan = New PointF(0, 0)
		Invalidate()
	End Sub

	''' <summary>Find a shape on the canvas by its associated domain element ID.</summary>
	''' <param name="id"></param>
	''' <returns>The <see cref="ShapeBase"/> instance with a matching <see cref="ShapeBase.ElementId"/>, or <c>Nothing</c> if no match is found.</returns>
	Private Function FindShapeByElementId(id As Guid) As ShapeBase
		Return _shapes.FirstOrDefault(Function(s) s.ElementId = id)
	End Function

	''' <summary>
	''' Find a shape on the canvas by its associated domain element ID, given as a string.
	''' Parses the string to a Guid and searches for a matching shape.
	''' </summary>
	''' <param name="id"></param>
	''' <returns> The <see cref="ShapeBase"/> instance with a matching <see cref="ShapeBase.ElementId"/>, or <c>Nothing</c> if no match is found or if the ID is not a valid Guid.</returns>
	Private Function FindShapeByElementId(id As String) As ShapeBase
		Dim gid As Guid
		If Not Guid.TryParse(id, gid) Then Return Nothing

		Return _shapes.FirstOrDefault(Function(s) s.ElementId = gid)
	End Function

	''' <summary>Draw a dashed outline around the given shape to indicate relationships (e.g., nesting, exclusion).</summary>
	''' <param name="g"></param>
	''' <param name="shape"></param>
	''' <param name="zoom"></param>
	''' <param name="pan"></param>
	''' <param name="color"></param>
	''' <param name="thickness"></param>
	Private Sub DrawDashedOutline(
		g As Graphics,
		shape As ShapeBase,
		zoom As Single,
		pan As PointF,
		color As Color,
		thickness As Single)

		Using pen As New Pen(color, thickness)
			pen.DashStyle = DashStyle.Dash
			Dim r = shape.GetBounds(zoom, pan)
			r.Inflate(3, 3) ' small visual offset
			g.DrawRectangle(pen, r)
		End Using
	End Sub

	''' <summary>Draw visual indicators for nested and exclusion relationships based on the current layout's relationships.</summary>
	''' <param name="g"></param>
	''' <param name="zoom"></param>
	''' <param name="pan"></param>
	Private Sub DrawNestedOverlays(g As Graphics, zoom As Single, pan As PointF)
		If _currentLayout Is Nothing Then Return
		If _currentLayout.Relationships Is Nothing Then Return

		For Each rel In _currentLayout.Relationships
			Dim parent = FindShapeByElementId(rel.ParentElementId)
			Dim child = FindShapeByElementId(rel.ChildElementId)

			If parent Is Nothing OrElse child Is Nothing Then Continue For

			Select Case rel.RelationshipType
				Case ElementRelationshipType.Nested
					' Child is part of parent (logical containment)
					DrawDashedOutline(g, child, zoom, pan, Color.DimGray, 1)
					' Parent gets a heavier outline
					DrawDashedOutline(g, parent, zoom, pan, Color.Gray, 2)
				Case ElementRelationshipType.Exclusion
					' Exclusion (subtract from parent)
					DrawDashedOutline(g, child, zoom, pan, Color.Red, 2)
			End Select
		Next
	End Sub

	''' <summary>
	''' Context menu action to assign a block (business data) to the currently selected shape.
	''' </summary>
	Private Sub AssignBlockToSelectedShape()
		If _selected Is Nothing Then Return
		Using dlg As New BlockAssignmentForm()
			dlg.BusinessJson = _selected.BusinessJson

			If dlg.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
				_selected.BusinessJson = dlg.BusinessJson
			End If
		End Using
	End Sub

	''' <summary>
	''' Initializes a new instance of <see cref="CanvasControl"/>.
	''' Sets default styles for smooth painting and initializes the cursor/background.
	''' </summary>
	Public Sub New()
		Me.DoubleBuffered = True
		Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer, True)
		Me.BackColor = Color.White
		Me.Cursor = Cursors.Cross

		Dim assignBlockItem = New ToolStripMenuItem("Assign Block...")
		AddHandler assignBlockItem.Click, AddressOf AssignBlockToSelectedShape
		_shapeMenu.Items.Add(assignBlockItem)
	End Sub
#Region "Functions that prevent serialization of these properties"
	<EditorBrowsable(EditorBrowsableState.Never)>
	Private Function ShouldSerializeMinLogicalWindowSize() As Boolean
		Return MinLogicalWindowSize <> DefaultMinLogicalWindowSize
	End Function
	<EditorBrowsable(EditorBrowsableState.Never)>
	Private Function ShouldSerializeMaxLogicalWindowSize() As Boolean
		Return MaxLogicalWindowSize <> DefaultMaxLogicalWindowSize
	End Function
#End Region
#End Region

#Region "Public API"
	''' <summary>
	''' Switches the active tool used by the canvas.
	''' </summary>
	''' <param name="tool">The tool to set.</param>
	Public Sub SetTool(tool As ToolType)
		_tool = tool
		_isDrawing = False
		_tempShape = Nothing
		Me.Cursor = If(tool = ToolType.Pan, Cursors.Hand, Cursors.Cross)
		Invalidate()
	End Sub

	''' <summary>
	''' Increase the canvas zoom by a fixed factor and request repaint.
	''' Zooms in 25%.
	''' </summary>
	'Public Sub ZoomIn()
	'    _zoom *= 1.2F
	'    Invalidate()
	'End Sub

	''' <summary>
	''' Zooms in 25%.</summary>
	Public Sub ZoomIn()
		_zoom = Math.Min(_zoom * 1.25F, 10.0F)
		Invalidate()
	End Sub

	'''' <summary>
	'''' Decrease the canvas zoom by a fixed factor (Zooms out 20%.) and request repaint.
	'''' Zoom is clamped to a small positive minimum.</summary>
	Public Sub ZoomOut()
		_zoom = Math.Max(_zoom * 0.8F, 0.1F)
		Invalidate()
	End Sub

	''' <summary>
	''' Toggle rendering of the snap/grid overlay and request repaint.
	''' </summary>
	Public Sub ToggleGrid()
		_showGrid = Not _showGrid
		Invalidate()
	End Sub

	''' <summary>
	''' Toggle snapping behavior when placing or drawing shapes.
	''' </summary>
	Public Sub ToggleSnap()
		_snapToGrid = Not _snapToGrid
	End Sub

	''' <summary>
	''' Remove all shapes from the canvas and clear selection.
	''' </summary>
	Public Sub Clear()
		_shapes.Clear()
		_selected = Nothing
		Invalidate()
	End Sub

	''' <summary>
	''' Export current canvas contents into a domain <see cref="CanvasLayout"/> instance.
	''' </summary>
	''' <returns>A <see cref="CanvasLayout"/> representing current elements on the canvas.</returns>
	Public Function ToLayout() As CanvasLayout
		Dim layout As New CanvasLayout() With {.Unit = "meter", .ScaleFactor = 1.0}
		For Each s In _shapes
			Dim elem As New CanvasElement() With {
			.Type = If(TypeOf s Is LineShape, "line",
					If(TypeOf s Is RectShape, "rectangle",
					If(TypeOf s Is EllipseShape, "ellipse",
					If(TypeOf s Is PolylineShape, "polyline", "unknown")))),
			.Layer = "default",
			.GeometryJson = s.ToGeometryJson(),
			.BusinessJson = "{}"
			}
			layout.Elements.Add(elem)
		Next
		Return layout
	End Function

	''' <summary>
	''' Load shapes from a <see cref="CanvasLayout"/> instance, replacing current contents.
	''' </summary>
	''' <param name="layout">The layout to load from.</param>
	Public Sub LoadFromLayout(layout As CanvasLayout)
		_shapes.Clear()
		For Each e In layout.Elements
			Select Case e.Type
				Case "line"
					Dim ls As New LineShape()
					ls.FromGeometryJson(e.GeometryJson)
					_shapes.Add(ls)
				Case "rectangle"
					Dim rs As New RectShape()
					rs.FromGeometryJson(e.GeometryJson)
					_shapes.Add(rs)
				Case "ellipse"
					Dim es As New EllipseShape()
					es.FromGeometryJson(e.GeometryJson)
					_shapes.Add(es)
				Case "polyline"
					Dim ps As New PolylineShape()
					ps.FromGeometryJson(e.GeometryJson)
					_shapes.Add(ps)
			End Select
		Next
		Invalidate()
	End Sub
#End Region

#Region "Input handling and rendering"
	''' <summary>
	''' Custom painting logic renders grid, rulers, shapes and transient drawing state to a back-buffer.
	''' </summary>
	Protected Overrides Sub OnPaint(e As PaintEventArgs)
		MyBase.OnPaint(e)

		If _backBuffer Is Nothing OrElse _backBuffer.Size <> Me.ClientSize Then
			_backBuffer = New Bitmap(Me.Width, Me.Height)
			_backGraphics = Graphics.FromImage(_backBuffer)
		End If

		_backGraphics.SmoothingMode = SmoothingMode.AntiAlias
		_backGraphics.Clear(Me.BackColor)

		If _showGrid Then DrawGrid(_backGraphics)
		If _showRulers Then DrawRulers(_backGraphics)

		For Each s In _shapes
			s.Draw(_backGraphics, _zoom, _pan)
		Next

		If _selected IsNot Nothing Then
			Using pen As New Pen(Color.DarkOrange, 2)
				pen.DashStyle = DashStyle.Dash
				_backGraphics.DrawRectangle(pen, _selected.GetBounds(_zoom, _pan))
			End Using
		End If

		If _isDrawing AndAlso _tempShape IsNot Nothing Then
			Using pen As New Pen(Color.SteelBlue, 2)
				pen.DashStyle = DashStyle.Dot
				_tempShape.Draw(_backGraphics, _zoom, _pan, pen)
			End Using
		End If

		If _backgroundImage IsNot Nothing Then
			Dim cm As New Imaging.ColorMatrix()
			cm.Matrix33 = _backgroundOpacity
			Dim ia As New Imaging.ImageAttributes()
			ia.SetColorMatrix(cm)
			Dim rect As New Rectangle(0, 0, Width, Height)
			e.Graphics.DrawImage(
		_backgroundImage,
		rect,
		0, 0,
		_backgroundImage.Width,
		_backgroundImage.Height,
		GraphicsUnit.Pixel,
		ia)

		End If
		DrawNestedOverlays(_backGraphics, _zoom, _pan)
		e.Graphics.DrawImageUnscaled(_backBuffer, 0, 0)
	End Sub

	''' <summary>
	''' Mouse down handler: begins drawing, selection or panning depending on the active tool.
	''' </summary>
	Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
		MyBase.OnMouseDown(e)

		Dim lp = ScreenToWorld(e.Location)
		If _snapToGrid Then lp = Snap(lp)

		If e.Button = MouseButtons.Right Then
			_selected = HitTest(lp)
			If _selected IsNot Nothing Then
				_shapeMenu.Show(Me, e.Location)
				Return
			End If
		End If

		Select Case _tool
			Case ToolType.Pan
				_startPt = e.Location
				Cursor = Cursors.SizeAll
				Return
			Case ToolType.SelectTool
				_selected = HitTest(lp)
				Invalidate()
			Case ToolType.Line
				_isDrawing = True
				_startPt = lp
				_tempShape = New LineShape() With {.Start = lp, .[End] = lp}
			Case ToolType.Rectangle
				_isDrawing = True
				_startPt = lp
				_tempShape = New RectShape() With {.TopLeft = lp, .Width = 0, .Height = 0}
			Case ToolType.Polyline
				_isDrawing = True
				_tempShape = New PolylineShape()
				CType(_tempShape, PolylineShape).Points.Add(lp)
			Case ToolType.Ellipse
				_isDrawing = True
				_startPt = lp
				_tempShape = New EllipseShape() With {
				.Center = lp,
				.RadiusX = 0,
				.RadiusY = 0
				}
		End Select
		_selected = HitTest(lp)
		' OLD Code
		' RaiseEvent ElementSelected(_selected)
		' Convert the selected ShapeBase to a CanvasElement expected by the event
		Dim selElement As CanvasElement = Nothing
		If _selected IsNot Nothing Then
			Dim typeName As String = If(TypeOf _selected Is LineShape, "line",
										If(TypeOf _selected Is RectShape, "rectangle",
										If(TypeOf _selected Is EllipseShape, "ellipse",
										If(TypeOf _selected Is PolylineShape, "polyline", "unknown"))))

			selElement = New CanvasElement() With {
				.Type = typeName,
				.Layer = _selected.LayerId,
				.GeometryJson = _selected.ToGeometryJson(),
				.BusinessJson = If(String.IsNullOrEmpty(_selected.BusinessJson), "{}", _selected.BusinessJson)
			}
		End If
		RaiseEvent ElementSelected(selElement)

	End Sub

	''' <summary>
	''' Mouse move handler: updates drawing preview or pans viewport when appropriate.
	''' </summary>
	Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
		MyBase.OnMouseMove(e)
		Dim lp = ScreenToWorld(e.Location)
		If _snapToGrid Then lp = Snap(lp)

		If _tool = ToolType.Pan AndAlso e.Button = MouseButtons.Left Then
			Dim dx = e.Location.X - _startPt.X
			Dim dy = e.Location.Y - _startPt.Y
			_pan = New PointF(_pan.X + dx, _pan.Y + dy)
			_startPt = e.Location
			Invalidate()
			Return
		End If

		If _isDrawing Then
			_currPt = lp
			If TypeOf _tempShape Is LineShape Then
				CType(_tempShape, LineShape).[End] = lp
			ElseIf TypeOf _tempShape Is RectShape Then
				Dim r = CType(_tempShape, RectShape)
				r.Width = Math.Abs(lp.X - _startPt.X)
				r.Height = Math.Abs(lp.Y - _startPt.Y)
				r.TopLeft = New PointF(Math.Min(_startPt.X, lp.X), Math.Min(_startPt.Y, lp.Y))
			ElseIf TypeOf _tempShape Is EllipseShape Then
				Dim ee = CType(_tempShape, EllipseShape)
				ee.RadiusX = Math.Abs(lp.X - _startPt.X)
				ee.RadiusY = Math.Abs(lp.Y - _startPt.Y)
				ee.Center = New PointF(Math.Min(_startPt.X, lp.X), Math.Min(_startPt.Y, lp.Y))
			ElseIf TypeOf _tempShape Is PolylineShape Then
				'If _isDrawing Then
				Dim pl = CType(_tempShape, PolylineShape)
				pl.Points.Add(lp)
				Invalidate()
				'End If
			End If
			Invalidate()
		End If
	End Sub

	''' <summary>
	''' Mouse up handler: finalizes transient drawing state and commits new shapes.
	''' </summary>
	Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
		MyBase.OnMouseUp(e)

		' For polylines, we allow multiple clicks to add points, and a double-click to finish the shape.
		If _tool = ToolType.Polyline AndAlso e.Clicks = 2 Then
			If _tempShape IsNot Nothing AndAlso _tempShape.IsValid() Then
				_shapes.Add(_tempShape)
			End If
			_isDrawing = False
			_tempShape = Nothing
			Invalidate()
			Return
		End If

		' Pan tool does not create shapes, just updates viewport on mouse move, so we exit early here.
		If _tool = ToolType.Pan Then
			Cursor = Cursors.Hand
			Return
		End If

		' For other tools, we finalize the shape on mouse up.
		If Not _isDrawing Then
			Return
		End If

		' Validate and commit the shape if it has a valid geometry (e.g., non-zero size).
		If _tempShape IsNot Nothing AndAlso _tempShape.IsValid() Then
			_shapes.Add(_tempShape)
		End If

		_isDrawing = False
		_tempShape = Nothing
		Invalidate()
	End Sub

	''' <summary>
	''' Converts screen pixel coordinates to canvas logical coordinates.
	''' </summary>
	''' <param name="screenPoint">Point in screen pixels</param>
	''' <returns>Point in canvas logical units</returns>
	''' <remarks>
	''' Formula: logical = (physical - pan) / zoom
	''' Used for tool interaction (mouse clicks, drags).
	''' </remarks>
	Private Function PhysicalToLogical(screenPoint As PointF) As PointF
		Return New PointF(
				(screenPoint.X - _pan.X) / _zoom,
				(screenPoint.Y - _pan.Y) / _zoom)
	End Function

	''' <summary>
	''' Convert a screen coordinate to world/canvas coordinates applying pan and zoom.
	''' </summary>
	''' <param name="p">Screen point in control coordinates.</param>
	''' <returns>Point in world/canvas coordinate space.</returns>
	Private Function ScreenToWorld(p As Point) As PointF
		Return New PointF((p.X - _pan.X) / _zoom, (p.Y - _pan.Y) / _zoom)
	End Function

	''' <summary>
	''' Convert a world/canvas coordinate to screen coordinates applying pan and zoom.
	''' </summary>
	''' <param name="p">Point in world/canvas coordinate space.</param>
	''' <returns>Point in control (screen) coordinate space.</returns>
	Private Function WorldToScreen(p As PointF) As Point
		Return New Point(CInt(p.X * _zoom + _pan.X), CInt(p.Y * _zoom + _pan.Y))
	End Function

	''' <summary>
	''' Snap a world point to the current grid.
	''' </summary>
	''' <param name="p">Point in world coordinates.</param>
	''' <returns>Snapped point in world coordinates.</returns>
	Private Function Snap(p As PointF) As PointF
		' ? If disabled, return original
		If Not _snapEnabled Then Return p
		Dim sx = Math.Round(p.X / _gridSize) * _gridSize
		Dim sy = Math.Round(p.Y / _gridSize) * _gridSize
		Return New PointF(CSng(sx), CSng(sy))
	End Function

	' ' Original Ver
	''' <summary>
	''' Hit-test shapes from top-most to bottom-most and return the first matching shape.
	''' </summary>
	''' <param name="lp">Location in world coordinates.</param>
	''' <returns>The hit <see cref="ShapeBase"/> or <c>Nothing</c> if none match.</returns>
	Private Function HitTest(lp As PointF) As ShapeBase
		For i = _shapes.Count - 1 To 0 Step -1
			If _shapes(i).HitTest(lp) Then Return _shapes(i)
		Next
		Return Nothing
	End Function

	''' <summary>
	''' Draw the grid lines to the provided graphics surface.
	''' </summary>
	''' <param name="g">Graphics surface to draw on.</param>
	Private Sub DrawGrid(g As Graphics)
		Using pen As New Pen(Color.Gainsboro)
			For x = 0 To Me.Width Step CInt(_gridSize * _zoom)
				g.DrawLine(pen, x + _pan.X Mod (CInt(_gridSize * _zoom)), 0, x + _pan.X Mod (CInt(_gridSize * _zoom)), Me.Height)
			Next
			For y = 0 To Me.Height Step CInt(_gridSize * _zoom)
				g.DrawLine(pen, 0, y + _pan.Y Mod (CInt(_gridSize * _zoom)), Me.Width, y + _pan.Y Mod (CInt(_gridSize * _zoom)))
			Next
		End Using
	End Sub

	''' <summary>
	''' Draw horizontal and vertical rulers on the top and left edges of the control.
	''' </summary>
	''' <param name="g">Graphics surface to draw on.</param>
	Private Sub DrawRulers(g As Graphics)
		Using br As New SolidBrush(Color.LightSteelBlue)
			g.FillRectangle(br, 0, 0, Me.Width, 20)
			g.FillRectangle(br, 0, 0, 20, Me.Height)
		End Using
		Using pen As New Pen(Color.Black)
			g.DrawLine(pen, 20, 20, Me.Width, 20)
			g.DrawLine(pen, 20, 20, 20, Me.Height)
		End Using
	End Sub
End Class
#End Region

#End Region

#Region "Shape definitions"

''' <summary>
''' Base class for drawable shapes on the canvas.
''' Implementations must provide drawing, hit testing, bounds and (de)serialization behavior.
''' </summary>
Public MustInherit Class ShapeBase
	''' <summary>Unique identifier for this shape, linking to domain elements.</summary>
	Public Property ElementId As Guid = Guid.NewGuid()

	''' <summary>Layer identifier for organizing shapes (e.g., "default", "overlay").</summary>
	Public Property LayerId As String = "default"

	Public Property BusinessJson As String
	''' <summary>
	''' Draw the shape to the given graphics surface using the specified zoom and pan.
	''' </summary>
	''' <param name="g">Graphics surface to draw on.</param>
	''' <param name="zoom">Current zoom factor.</param>
	''' <param name="pan">Current pan offset.</param>
	''' <param name="pen">Optional pen to use for drawing (default is shape-specific).</param>
	Public MustOverride Sub Draw(g As Graphics, zoom As Single, pan As PointF, Optional pen As Pen = Nothing)
	''' <summary>
	''' Determine whether the given world point intersects the shape.
	''' </summary>
	''' <param name="lp">Point in world coordinates.</param>
	''' <returns><c>True</c> if the point hits the shape; otherwise <c>False</c>.</returns>
	Public MustOverride Function HitTest(lp As PointF) As Boolean
	''' <summary>
	''' Return the bounding rectangle in screen coordinates for selection visuals.
	''' </summary>
	''' <param name="zoom">Current zoom factor.</param>
	''' <param name="pan">Current pan offset.</param>
	''' <returns>A <see cref="Rectangle"/> representing screen-space bounds.</returns>
	Public MustOverride Function GetBounds(zoom As Single, pan As PointF) As Rectangle
	''' <summary>
	''' Validate whether the shape has sufficient size/definition to be committed.
	''' </summary>
	''' <returns><c>True</c> if valid; otherwise <c>False</c>.</returns>
	Public MustOverride Function IsValid() As Boolean
	''' <summary>
	''' Serialize the shape geometry to a JSON string appropriate for storage in a <see cref="CanvasElement"/>.
	''' </summary>
	''' <returns>JSON string representing the shape geometry.</returns>
	Public MustOverride Function ToGeometryJson() As String
	''' <summary>
	''' Populate shape geometry from a previously serialized JSON payload.
	''' </summary>
	''' <param name="json">JSON geometry payload.</param>
	Public MustOverride Sub FromGeometryJson(json As String)
End Class
#End Region

#Region "Specific shape implementations"

#Region "LineShape"
''' <summary>
''' Straight line shape defined by a start and end point in world coordinates.
''' </summary>
Public Class LineShape
	Inherits ShapeBase
#Region "Public Properties"
	''' <summary>Start point in world coordinates.</summary>
	Public Property Start As PointF
	''' <summary>End point in world coordinates.</summary>
	Public Property [End] As PointF
#End Region

#Region "Public Overrides"
	''' <inheritdoc/>
	Public Overrides Sub Draw(g As Graphics, zoom As Single, pan As PointF, Optional pen As Pen = Nothing)
		Dim p1 = New PointF(Start.X * zoom + pan.X, Start.Y * zoom + pan.Y)
		Dim p2 = New PointF([End].X * zoom + pan.X, [End].Y * zoom + pan.Y)
		Using p As Pen = If(pen, New Pen(Color.DodgerBlue, 2))
			g.DrawLine(p, p1, p2)
		End Using
	End Sub

	''' <inheritdoc/>
	Public Overrides Function HitTest(lp As PointF) As Boolean
		Dim dx = [End].X - Start.X
		Dim dy = [End].Y - Start.Y
		Dim length2 = dx * dx + dy * dy
		If length2 = 0 Then Return False
		Dim t = ((lp.X - Start.X) * dx + (lp.Y - Start.Y) * dy) / length2
		t = Math.Max(0, Math.Min(1, t))
		Dim proj = New PointF(Start.X + t * dx, Start.Y + t * dy)
		Dim dist = Math.Sqrt((lp.X - proj.X) ^ 2 + (lp.Y - proj.Y) ^ 2)
		Return dist < 5
	End Function

	''' <inheritdoc/>
	Public Overrides Function GetBounds(zoom As Single, pan As PointF) As Rectangle
		Dim p1 = New Point(CInt(Start.X * zoom + pan.X), CInt(Start.Y * zoom + pan.Y))
		Dim p2 = New Point(CInt([End].X * zoom + pan.X), CInt([End].Y * zoom + pan.Y))
		Dim minX = Math.Min(p1.X, p2.X)
		Dim minY = Math.Min(p1.Y, p2.Y)
		Dim maxX = Math.Max(p1.X, p2.X)
		Dim maxY = Math.Max(p1.Y, p2.Y)
		Return New Rectangle(minX, minY, maxX - minX, maxY - minY)
	End Function

	''' <inheritdoc/>
	Public Overrides Function IsValid() As Boolean
		Return Math.Abs(Start.X - [End].X) + Math.Abs(Start.Y - [End].Y) > 2
	End Function

	''' <inheritdoc/>
	Public Overrides Function ToGeometryJson() As String
		Return JsonSerializer.Serialize(New With {.start = New With {.x = Start.X, .y = Start.Y}, .[end] = New With {.x = [End].X, .y = [End].Y}})
	End Function

	''' <inheritdoc/>
	Public Overrides Sub FromGeometryJson(json As String)
		Dim doc = JsonDocument.Parse(json)
		Dim s = doc.RootElement.GetProperty("start")
		Dim e = doc.RootElement.GetProperty("end")
		Start = New PointF(s.GetProperty("x").GetSingle(), s.GetProperty("y").GetSingle())
		[End] = New PointF(e.GetProperty("x").GetSingle(), e.GetProperty("y").GetSingle())
	End Sub
#End Region
End Class
#End Region

#Region "RectShape"
''' <summary>
''' Axis-aligned rectangle shape in world coordinates.
''' </summary>
Public Class RectShape
	Inherits ShapeBase
#Region "Public Properties"
	''' <summary>Top-left corner in world coordinates.</summary>
	Public Property TopLeft As PointF
	''' <summary>Width in world units.</summary>
	Public Property Width As Single
	''' <summary>Height in world units.</summary>
	Public Property Height As Single
#End Region

#Region "Public Overrides"
	''' <inheritdoc/>
	Public Overrides Sub Draw(g As Graphics, zoom As Single, pan As PointF, Optional pen As Pen = Nothing)
		Dim r = New RectangleF(TopLeft.X * zoom + pan.X, TopLeft.Y * zoom + pan.Y, Width * zoom, Height * zoom)
		Using p As Pen = If(pen, New Pen(Color.ForestGreen, 2))
			g.DrawRectangle(p, Rectangle.Round(r))
		End Using
	End Sub

	''' <inheritdoc/>
	Public Overrides Function HitTest(lp As PointF) As Boolean
		Return lp.X >= TopLeft.X AndAlso lp.X <= TopLeft.X + Width AndAlso lp.Y >= TopLeft.Y AndAlso lp.Y <= TopLeft.Y + Height
	End Function

	''' <inheritdoc/>
	Public Overrides Function GetBounds(zoom As Single, pan As PointF) As Rectangle
		Dim r = New RectangleF(TopLeft.X * zoom + pan.X, TopLeft.Y * zoom + pan.Y, Width * zoom, Height * zoom)
		Return Rectangle.Round(r)
	End Function

	''' <inheritdoc/>
	Public Overrides Function IsValid() As Boolean
		Return Width >= 5 AndAlso Height >= 5
	End Function

	''' <inheritdoc/>
	Public Overrides Function ToGeometryJson() As String
		Return JsonSerializer.Serialize(New With {.topLeft = New With {.x = TopLeft.X, .y = TopLeft.Y}, .width = Width, .height = Height})
	End Function

	''' <inheritdoc/>
	Public Overrides Sub FromGeometryJson(json As String)
		Dim doc = JsonDocument.Parse(json)
		Dim tl = doc.RootElement.GetProperty("topLeft")
		TopLeft = New PointF(tl.GetProperty("x").GetSingle(), tl.GetProperty("y").GetSingle())
		Width = doc.RootElement.GetProperty("width").GetSingle()
		Height = doc.RootElement.GetProperty("height").GetSingle()
	End Sub
#End Region
End Class
#End Region

#Region "EllipseShape"
''' <summary>
''' Axis-aligned ellipse shape defined by a bounding rectangle in world coordinates.
''' </summary>
Public Class EllipseShape
	Inherits ShapeBase
#Region "Public Properties"
	'Public Property TopLeft As PointF
	'Public Property Width As Single
	'Public Property Height As Single

	''' <summary>Center point of the ellipse in world coordinates.</summary>
	''' <returns>A <see cref="PointF"/> representing the center of the ellipse in world coordinates.</returns>
	Public Property Center As PointF
	''' <summary>Radius along the X-axis in world units.</summary>
	''' <returns>A <see cref="Single"/> representing the horizontal radius of the ellipse in world units.</returns>
	Public Property RadiusX As Single
	''' <summary>Radius along the Y-axis in world units.</summary>
	''' <returns>A <see cref="Single"/> representing the vertical radius of the ellipse in world units.</returns>
	Public Property RadiusY As Single
#End Region

#Region "Public Overrides"
	''' <summary>Draw the ellipse by calculating its bounding rectangle from the center and radii, applying zoom and pan transformations.</summary>
	''' <param name="g">Graphics surface to draw on.</param>
	''' <param name="zoom">Current zoom factor, used to scale the ellipse size appropriately for rendering.</param>
	''' <param name="pan">Current pan offset, used to translate the ellipse position for rendering in the viewport.</param>
	''' <param name="pen">Optional pen to use for drawing the ellipse. If not provided, a default pen will be used.</param>
	Public Overrides Sub Draw(g As Graphics, zoom As Single, pan As PointF, Optional pen As Pen = Nothing)
		Dim r = New RectangleF(
		Center.X * zoom + pan.X,
		Center.Y * zoom + pan.Y,
		RadiusX * zoom,
		RadiusY * zoom
	)
		Using p As Pen = If(pen, New Pen(Color.MediumPurple, 2))
			g.DrawEllipse(p, r)
		End Using
	End Sub

	''' <summary>Hit test the ellipse by normalizing the point to the ellipse's coordinate space and checking if it falls within the unit circle.</summary>
	''' <param name="lp"></param>
	''' <returns><c>True</c> if the point is inside the ellipse; otherwise <c>False</c>.</returns>
	Public Overrides Function HitTest(lp As PointF) As Boolean
		Dim rx = RadiusX / 2.0F
		Dim ry = RadiusY / 2.0F
		If rx <= 0 OrElse ry <= 0 Then Return False

		Dim cx = Center.X + rx
		Dim cy = Center.Y + ry

		Dim dx = (lp.X - cx) / rx
		Dim dy = (lp.Y - cy) / ry

		Return dx * dx + dy * dy <= 1.0F
	End Function

	''' <summary>Calculate the bounding rectangle for the ellipse based on its center and radii, applying <br></br> zoom and pan transformations to convert from world coordinates to screen coordinates.</summary>
	''' <param name="zoom"></param>
	''' <param name="pan"></param>
	''' <returns>A <see cref="Rectangle"/> that tightly bounds the ellipse in screen coordinates, used for  <br></br> selection highlighting and hit testing.</returns>
	Public Overrides Function GetBounds(zoom As Single, pan As PointF) As Rectangle
		Dim r = New RectangleF(
		Center.X * zoom + pan.X,
		Center.Y * zoom + pan.Y,
		RadiusX * zoom,
		RadiusY * zoom
	)
		Return Rectangle.Round(r)
	End Function

	''' <summary>Validate that the ellipse has non-trivial size by checking that both radii exceed a minimum threshold.</summary>
	''' <returns><c>True</c> if both <see cref="RadiusX"/> and <see cref="RadiusY"/> are greater than or equal to 5 units; otherwise <c>False</c>.</returns>
	Public Overrides Function IsValid() As Boolean
		Return RadiusX >= 5 AndAlso RadiusY >= 5
	End Function

	''' <summary>Serialize the ellipse geometry to JSON by representing the center point and radii as properties in a structured format.</summary>
	''' <returns>A JSON string representing the ellipse geometry, e.g.:
	''' {
	'''   "center": {"x": 50.0, "y": 50.0},
	'''   "radiusX": 20.0,
	'''   "radiusY": 10.0
	''' }
	''' </returns>
	Public Overrides Function ToGeometryJson() As String
		Return JsonSerializer.Serialize(New With {
		.topLeft = New With {.x = Center.X, .y = Center.Y},
		.width = RadiusX,
		.height = RadiusY
	})
	End Function

	''' <summary>Deserialize the ellipse geometry from JSON by parsing the center point and radii from the expected structured format.</summary>
	''' <param name="json">JSON string containing the ellipse geometry, expected to have properties for "center" (with "x" and "y") and "radiusX"/"radiusY".</param>
	Public Overrides Sub FromGeometryJson(json As String)
		Dim doc = JsonDocument.Parse(json)
		Dim tl = doc.RootElement.GetProperty("topLeft")
		Center = New PointF(tl.GetProperty("x").GetSingle(), tl.GetProperty("y").GetSingle())
		RadiusX = doc.RootElement.GetProperty("width").GetSingle()
		RadiusY = doc.RootElement.GetProperty("height").GetSingle()
	End Sub
#End Region
End Class
#End Region

#Region "PolylineShape"
''' <summary>
''' Polyline shape consisting of multiple connected segments.
''' </summary>
Public Class PolylineShape
	Inherits ShapeBase

#Region "Public Properties"
	''' <summary>
	''' List of points defining the polyline vertices in world coordinates.
	''' </summary>
	''' <returns>
	''' List of <see cref="PointF"/> representing the vertices of the polyline.
	''' </returns>
	Public ReadOnly Property Points As New List(Of PointF)
#End Region

#Region "Public Overrides"
	''' <summary>
	''' Draw the polyline by connecting all points in sequence, applying zoom and pan transformations.
	''' </summary>
	''' <param name="g"></param>
	''' <param name="zoom"></param>
	''' <param name="pan"></param>
	''' <param name="pen"></param>
	Public Overrides Sub Draw(g As Graphics, zoom As Single, pan As PointF, Optional pen As Pen = Nothing)
		If Points.Count < 2 Then Return

		Using p As Pen = If(pen, New Pen(Color.IndianRed, 2))
			Dim screenPts = Points.Select(
				Function(pt) New PointF(pt.X * zoom + pan.X, pt.Y * zoom + pan.Y)
			).ToArray()

			g.DrawLines(p, screenPts)
		End Using
	End Sub

	''' <summary>
	''' Hit test the polyline by checking proximity to each segment defined by consecutive points.
	''' </summary>
	''' <param name="lp"></param>
	''' <returns>
	''' <c>True</c> if the point is within a certain distance of any segment; otherwise <c>False</c>.
	''' </returns>
	Public Overrides Function HitTest(lp As PointF) As Boolean
		For i = 0 To Points.Count - 2
			Dim a = Points(i)
			Dim b = Points(i + 1)
			Dim dx = b.X - a.X
			Dim dy = b.Y - a.Y
			Dim len2 = dx * dx + dy * dy
			If len2 = 0 Then Continue For
			Dim t = ((lp.X - a.X) * dx + (lp.Y - a.Y) * dy) / len2
			t = Math.Max(0, Math.Min(1, t))

			Dim proj = New PointF(a.X + t * dx, a.Y + t * dy)
			Dim dist = Math.Sqrt((lp.X - proj.X) ^ 2 + (lp.Y - proj.Y) ^ 2)

			If dist < 5 Then Return True
		Next
		Return False
	End Function

	''' <summary>
	''' Calculate the bounding rectangle that encompasses all points in the polyline, applying zoom and pan transformations.
	''' </summary>
	''' <param name="zoom"></param>
	''' <param name="pan"></param>
	''' <returns>
	''' A <see cref="Rectangle"/> that tightly bounds the polyline in screen coordinates, used for selection highlighting.
	''' </returns>
	Public Overrides Function GetBounds(zoom As Single, pan As PointF) As Rectangle
		Dim minX = Points.Min(Function(p) p.X)
		Dim minY = Points.Min(Function(p) p.Y)
		Dim maxX = Points.Max(Function(p) p.X)
		Dim maxY = Points.Max(Function(p) p.Y)

		Dim r = New RectangleF(
			minX * zoom + pan.X,
			minY * zoom + pan.Y,
			(maxX - minX) * zoom,
			(maxY - minY) * zoom
		)
		Return Rectangle.Round(r)
	End Function

	''' <summary>
	''' Validate that the polyline has at least two points to form a valid shape.
	''' </summary>
	''' <returns>
	''' <c>True</c> if the polyline has 2 or more points; otherwise <c>False</c>.
	''' </returns>
	Public Overrides Function IsValid() As Boolean
		Return Points.Count >= 2
	End Function

	''' <summary>
	''' Serialize the list of points to a JSON array format, where each point is represented as an object with "x" and "y" properties.
	''' </summary>
	''' <returns>
	''' A JSON string representing the polyline geometry, e.g.:
	''' [
	'''   {"x": 10.0, "y": 20.0},
	'''   {"x": 15.0, "y": 25.0},
	'''   ...
	''' ]
	''' </returns>
	Public Overrides Function ToGeometryJson() As String
		Return JsonSerializer.Serialize(
			Points.Select(Function(p) New With {.x = p.X, .y = p.Y})
		)
	End Function

	''' <summary>
	''' Deserialize the polyline geometry from a JSON array of point objects, populating the <see cref="Points"/> list.
	''' </summary>
	''' <param name="json">
	''' A JSON string representing the polyline geometry, expected to be an array of objects with "x" and "y" properties, e.g.:
	''' [
	'''   {"x": 10.0, "y": 20.0},
	'''   {"x": 15.0, "y": 25.0},
	'''   ...
	''' ]
	''' </param>
	Public Overrides Sub FromGeometryJson(json As String)
		Points.Clear()
		Dim doc = JsonDocument.Parse(json)
		For Each pt In doc.RootElement.EnumerateArray()
			Points.Add(New PointF(
				pt.GetProperty("x").GetSingle(),
				pt.GetProperty("y").GetSingle()
			))
		Next
	End Sub
#End Region
End Class
#End Region

#End Region
