
#Region "Info. & Imports"
'Option Strict On
Imports System.Drawing
Imports System.Text.Json

#End Region

Namespace Controls
#Region "CanvasControl and related types"

	'''' <summary>
	'''' available Tools for the interactive canvas.
	'''' </summary>
	'Public Enum ToolType
	'	''' <summary>Select and manipulate existing shapes.</summary>
	'	SelectTool
	'	''' <summary>Draw straight lines.</summary>
	'	Line
	'	''' <summary>Draw rectangles.</summary>
	'	Rectangle
	'	''' <summary>Draw ellipses.</summary>
	'	Ellipse
	'	''' <summary>Draw Polyline</summary>
	'	Polyline
	'	''' <summary>Pan the viewport.</summary>
	'	Pan
	'End Enum

#Region "Specific shape implementations"
	''' <summary>
	''' Straight line shape defined by a start and end point in world coordinates.
	''' </summary>
	Public Class LineShape
		Inherits ShapeBase
		''' <summary>Start point in world coordinates.</summary>
		Public Property Start As PointF
		''' <summary>End point in world coordinates.</summary>
		Public Property [End] As PointF

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
	End Class
#End Region

#End Region

End Namespace

'-----------------
'#Region "Info. & Imports"

'' Enforce strict typing rules (no implicit narrowing conversions)
'Option Strict On

'' Core GDI+ drawing types (Graphics, Pen, PointF, etc.)
'Imports System.Drawing

'' Advanced drawing features (smoothing, dash styles, transforms)
'Imports System.Drawing.Drawing2D

'' Windows Forms base classes (UserControl, MouseEventArgs, etc.)
'Imports System.Windows.Forms

'' JSON serialization for geometry persistence
'Imports System.Text.Json

'' Domain entities used for saving/loading layouts
'Imports CoNSoL.Domain.Entities
'#End Region


'Namespace Controls

'    ''' <summary>
'    ''' Tools available for the interactive canvas.
'    ''' Controls how mouse input is interpreted.
'    ''' </summary>
'    Public Enum ToolType
'        ''' <summary>Select and manipulate existing shapes.</summary>
'        SelectTool
'        ''' <summary>Draw straight lines.</summary>
'        Line
'        ''' <summary>Draw rectangles.</summary>
'        Rectangle
'        ''' <summary>Draw ellipses.</summary>
'        Ellipse
'        ''' <summary>Draw connected line segments.</summary>
'        Polyline
'        ''' <summary>Pan (move) the viewport.</summary>
'        Pan
'    End Enum


'#Region "canvas control"

'    ''' <summary>
'    ''' Interactive canvas control that supports drawing, selection,
'    ''' panning, zooming, grid snapping, rulers, and serialization.
'    ''' </summary>
'    Public Class CanvasControl
'        Inherits UserControl

'        ' Collection of all committed shapes on the canvas
'        Private ReadOnly _shapes As New List(Of ShapeBase)()

'        ' Currently selected shape (if any)
'        Private _selected As ShapeBase = Nothing

'        ' Indicates an active draw operation
'        Private _isDrawing As Boolean = False

'        ' Mouse positions in world coordinates
'        Private _startPt As PointF
'        Private _currPt As PointF

'        ' Temporary shape while drawing (preview)
'        Private _tempShape As ShapeBase = Nothing

'        ' Currently active tool
'        Private _tool As ToolType = ToolType.SelectTool

'        ' View transform state
'        Private _zoom As Single = 1.0F
'        Private _pan As PointF = New PointF(0, 0)

'        ' Grid and snapping configuration
'        Private _gridSize As Integer = 20
'        Private _showGrid As Boolean = True
'        Private _snapToGrid As Boolean = True
'        Private _showRulers As Boolean = True

'        ' Manual back-buffering to avoid flicker
'        Private _backBuffer As Bitmap = Nothing
'        Private _backGraphics As Graphics = Nothing


'        ''' <summary>
'        ''' Constructor sets up painting styles and defaults.
'        ''' </summary>
'        Public Sub New()
'            Me.DoubleBuffered = True
'            Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or
'                        ControlStyles.UserPaint Or
'                        ControlStyles.OptimizedDoubleBuffer, True)

'            Me.BackColor = Color.White
'            Me.Cursor = Cursors.Cross
'        End Sub


'#Region "Public API"

'        ''' <summary>
'        ''' Switches the active drawing or interaction tool.
'        ''' Resets transient state.
'        ''' </summary>
'        Public Sub SetTool(tool As ToolType)
'            _tool = tool
'            _isDrawing = False
'            _tempShape = Nothing

'            ' Use a hand cursor for panning, crosshair otherwise
'            Me.Cursor = If(tool = ToolType.Pan, Cursors.Hand, Cursors.Cross)
'            Invalidate()
'        End Sub


'        ''' <summary>
'        ''' Zooms in by scaling view by a fixed multiplier.
'        ''' </summary>
'        Public Sub ZoomIn()
'            _zoom *= 1.2F
'            Invalidate()
'        End Sub


'        ''' <summary>
'        ''' Zooms out with a lower bound to avoid inversion.
'        ''' </summary>
'        Public Sub ZoomOut()
'            _zoom /= 1.2F
'            If _zoom < 0.1F Then _zoom = 0.1F
'            Invalidate()
'        End Sub


'        ''' <summary>
'        ''' Enable or disable background grid display.
'        ''' </summary>
'        Public Sub ToggleGrid()
'            _showGrid = Not _showGrid
'            Invalidate()
'        End Sub


'        ''' <summary>
'        ''' Enable or disable snapping to grid intersections.
'        ''' </summary>
'        Public Sub ToggleSnap()
'            _snapToGrid = Not _snapToGrid
'        End Sub


'        ''' <summary>
'        ''' Remove all shapes and reset selection.
'        ''' </summary>
'        Public Sub Clear()
'            _shapes.Clear()
'            _selected = Nothing
'            Invalidate()
'        End Sub


'        ''' <summary>
'        ''' Serialize all shapes into a domain CanvasLayout object.
'        ''' </summary>
'        Public Function ToLayout() As CanvasLayout
'            Dim layout As New CanvasLayout With {
'                .Unit = "meter",
'                .ScaleFactor = 1.0
'            }

'            ' Convert each shape to a CanvasElement
'            For Each s In _shapes
'                Dim elem As New CanvasElement With {
'                    .Type =
'                        If(TypeOf s Is LineShape, "line",
'                        If(TypeOf s Is RectShape, "rectangle",
'                        If(TypeOf s Is EllipseShape, "ellipse",
'                        If(TypeOf s Is PolylineShape, "polyline", "unknown")))),
'                    .Layer = "default",
'                    .GeometryJson = s.ToGeometryJson(),
'                    .BusinessJson = "{}"
'                }
'                layout.Elements.Add(elem)
'            Next

'            Return layout
'        End Function
'#End Region


'        ''' <summary>
'        ''' Clears the canvas and reconstructs shapes
'        ''' from a previously saved CanvasLayout.
'        ''' </summary>
'        Public Sub LoadFromLayout(layout As CanvasLayout)
'            _shapes.Clear()

'            For Each e In layout.Elements
'                Select Case e.Type
'                    Case "line"
'                        Dim ls As New LineShape()
'                        ls.FromGeometryJson(e.GeometryJson)
'                        _shapes.Add(ls)

'                    Case "rectangle"
'                        Dim rs As New RectShape()
'                        rs.FromGeometryJson(e.GeometryJson)
'                        _shapes.Add(rs)

'                    Case "ellipse"
'                        Dim es As New EllipseShape()
'                        es.FromGeometryJson(e.GeometryJson)
'                        _shapes.Add(es)

'                    Case "polyline"
'                        Dim ps As New PolylineShape()
'                        ps.FromGeometryJson(e.GeometryJson)
'                        _shapes.Add(ps)
'                End Select
'            Next

'            Invalidate()
'        End Sub


'#Region "Input handling and rendering"

'        ''' <summary>
'        ''' Main rendering routine.
'        ''' Draws grid, rulers, shapes, selection, and preview state.
'        ''' </summary>
'        Protected Overrides Sub OnPaint(e As PaintEventArgs)
'            MyBase.OnPaint(e)

'            ' Recreate back buffer if control size changes
'            If _backBuffer Is Nothing OrElse _backBuffer.Size <> Me.ClientSize Then
'                _backBuffer = New Bitmap(Me.Width, Me.Height)
'                _backGraphics = Graphics.FromImage(_backBuffer)
'            End If

'            _backGraphics.SmoothingMode = SmoothingMode.AntiAlias
'            _backGraphics.Clear(Me.BackColor)

'            If _showGrid Then DrawGrid(_backGraphics)
'            If _showRulers Then DrawRulers(_backGraphics)

'            ' Draw all committed shapes
'            For Each s In _shapes
'                s.Draw(_backGraphics, _zoom, _pan)
'            Next

'            ' Draw dashed selection rectangle
'            If _selected IsNot Nothing Then
'                Using pen As New Pen(Color.DarkOrange, 2)
'                    pen.DashStyle = DashStyle.Dash
'                    _backGraphics.DrawRectangle(pen, _selected.GetBounds(_zoom, _pan))
'                End Using
'            End If

'            ' Draw preview of currently drawn shape
'            If _isDrawing AndAlso _tempShape IsNot Nothing Then
'                Using pen As New Pen(Color.SteelBlue, 2)
'                    pen.DashStyle = DashStyle.Dot
'                    _tempShape.Draw(_backGraphics, _zoom, _pan, pen)
'                End Using
'            End If

'            ' Blit back buffer to screen
'            e.Graphics.DrawImageUnscaled(_backBuffer, 0, 0)
'        End Sub


'        ''' <summary>
'        ''' Mouse down initializes drawing, selection, or panning.
'        ''' </summary>
'        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
'            MyBase.OnMouseDown(e)

'            Dim lp = ScreenToWorld(e.Location)
'            If _snapToGrid Then lp = Snap(lp)

'            ' Start panning
'            If _tool = ToolType.Pan Then
'                _startPt = e.Location
'                Cursor = Cursors.SizeAll
'                Return
'            End If

'            Select Case _tool
'                Case ToolType.SelectTool
'                    _selected = HitTest(lp)
'                    Invalidate()

'                Case ToolType.Line
'                    _isDrawing = True
'                    _startPt = lp
'                    _tempShape = New LineShape With {.Start = lp, .End = lp}

'                Case ToolType.Rectangle
'                    _isDrawing = True
'                    _startPt = lp
'                    _tempShape = New RectShape With {.TopLeft = lp, .Width = 0, .Height = 0}
'            End Select
'        End Sub


'        ''' <summary>
'        ''' Mouse move updates panning or drawing preview.
'        ''' </summary>
'        Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
'            MyBase.OnMouseMove(e)

'            Dim lp = ScreenToWorld(e.Location)
'            If _snapToGrid Then lp = Snap(lp)

'            ' Pan while dragging
'            If _tool = ToolType.Pan AndAlso e.Button = MouseButtons.Left Then
'                Dim dx = e.Location.X - _startPt.X
'                Dim dy = e.Location.Y - _startPt.Y
'                _pan = New PointF(_pan.X + dx, _pan.Y + dy)
'                _startPt = e.Location
'                Invalidate()
'                Return
'            End If

'            ' Update drawing preview
'            If _isDrawing Then
'                _currPt = lp
'                If TypeOf _tempShape Is LineShape Then
'                    CType(_tempShape, LineShape).[End] = lp
'                ElseIf TypeOf _tempShape Is RectShape Then
'                    Dim r = CType(_tempShape, RectShape)
'                    r.Width = Math.Abs(lp.X - _startPt.X)
'                    r.Height = Math.Abs(lp.Y - _startPt.Y)
'                    r.TopLeft = New PointF(Math.Min(_startPt.X, lp.X), Math.Min(_startPt.Y, lp.Y))
'                ElseIf TypeOf _tempShape Is EllipseShape Then
'                    Dim ee = CType(_tempShape, EllipseShape)
'                    ee.Width = Math.Abs(lp.X - _startPt.X)
'                    ee.Height = Math.Abs(lp.Y - _startPt.Y)
'                    ee.TopLeft = New PointF(Math.Min(_startPt.X, lp.X), Math.Min(_startPt.Y, lp.Y))
'                ElseIf TypeOf _tempShape Is PolylineShape Then
'                    'preview handled naturally via Draw()
'                End If
'                Invalidate()
'                'If TypeOf _tempShape Is LineShape Then
'                '    CType(_tempShape, LineShape).[End] = lp

'                'ElseIf TypeOf _tempShape Is RectShape Then
'                '    Dim r = CType(_tempShape, RectShape)
'                '    r.Width = Math.Abs(lp.X - _startPt.X)
'                '    r.Height = Math.Abs(lp.Y - _startPt.Y)
'                '    r.TopLeft = New PointF(Math.Min(_startPt.X, lp.X), Math.Min(_startPt.Y, lp.Y))
'                'End If

'                'Invalidate()
'            End If
'        End Sub


'        ''' <summary>
'        ''' Mouse up finalizes drawing operations.
'        ''' </summary>
'        Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
'            MyBase.OnMouseUp(e)

'            If _tool = ToolType.Pan Then
'                Cursor = Cursors.Hand
'                Return
'            End If

'            If Not _isDrawing Then Return

'            If _tempShape IsNot Nothing AndAlso _tempShape.IsValid() Then
'                _shapes.Add(_tempShape)
'            End If

'            _isDrawing = False
'            _tempShape = Nothing
'            Invalidate()
'        End Sub


'        ' ---------------- Coordinate helpers ----------------

'        ' Convert from screen pixels to world coordinates
'        Private Function ScreenToWorld(p As Point) As PointF
'            Return New PointF((p.X - _pan.X) / _zoom, (p.Y - _pan.Y) / _zoom)
'        End Function

'        ' Convert from world coordinates to screen pixels
'        Private Function WorldToScreen(p As PointF) As Point
'            Return New Point(CInt(p.X * _zoom + _pan.X), CInt(p.Y * _zoom + _pan.Y))
'        End Function

'        ' Snap point to nearest grid intersection
'        Private Function Snap(p As PointF) As PointF
'            Dim sx = Math.Round(p.X / _gridSize) * _gridSize
'            Dim sy = Math.Round(p.Y / _gridSize) * _gridSize
'            Return New PointF(CSng(sx), CSng(sy))
'        End Function


'        ' ---------------- Hit testing & overlays ----------------

'        ' Returns top-most shape under the cursor
'        Private Function HitTest(lp As PointF) As ShapeBase
'            For i = _shapes.Count - 1 To 0 Step -1
'                If _shapes(i).HitTest(lp) Then Return _shapes(i)
'            Next
'            Return Nothing
'        End Function

'        ' Draws background grid lines
'        Private Sub DrawGrid(g As Graphics)
'            Using pen As New Pen(Color.Gainsboro)
'                For x = 0 To Me.Width Step CInt(_gridSize * _zoom)
'                    g.DrawLine(pen, x + _pan.X Mod (CInt(_gridSize * _zoom)), 0,
'                                     x + _pan.X Mod (CInt(_gridSize * _zoom)), Me.Height)
'                Next
'                For y = 0 To Me.Height Step CInt(_gridSize * _zoom)
'                    g.DrawLine(pen, 0, y + _pan.Y Mod (CInt(_gridSize * _zoom)),
'                                     Me.Width, y + _pan.Y Mod (CInt(_gridSize * _zoom)))
'                Next
'            End Using
'        End Sub

'        ' Draws top and left rulers
'        Private Sub DrawRulers(g As Graphics)
'            Using br As New SolidBrush(Color.LightSteelBlue)
'                g.FillRectangle(br, 0, 0, Me.Width, 20)
'                g.FillRectangle(br, 0, 0, 20, Me.Height)
'            End Using
'            Using pen As New Pen(Color.DarkSlateGray)
'                g.DrawLine(pen, 20, 20, Me.Width, 20)
'                g.DrawLine(pen, 20, 20, 20, Me.Height)
'            End Using
'        End Sub

'    End Class
'#End Region


'    ' =========================================================================================
'    ' SHAPE MODEL LAYER
'    ' =========================================================================================

'#Region "Shape definitions"

'    ''' <summary>
'    ''' Abstract base class for all drawable canvas shapes.
'    ''' </summary>
'    Public MustInherit Class ShapeBase
'        Public MustOverride Sub Draw(g As Graphics, zoom As Single, pan As PointF, Optional pen As Pen = Nothing)
'        Public MustOverride Function HitTest(lp As PointF) As Boolean
'        Public MustOverride Function GetBounds(zoom As Single, pan As PointF) As Rectangle
'        Public MustOverride Function IsValid() As Boolean
'        Public MustOverride Function ToGeometryJson() As String
'        Public MustOverride Sub FromGeometryJson(json As String)
'    End Class

'#End Region
'#End Region


'End Namespace
'-------------------------------------------
