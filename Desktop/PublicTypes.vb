
#Region "Info. & Imports"
' =========================================================
' Filename: PublicTypes.vb
' Purpose:
'   Defines shared geometry primitives, enums, and helpers
'   used throughout the application (rectangles, lines,
'   segments, hit-testing, bounds calculations, etc.)
' =========================================================

Option Strict On    ' Enforces strict type checking (no implicit narrowing)
Option Infer On     ' Allows local variable type inference

' Core .NET namespaces required by the geometry types
Imports System
Imports System.Drawing
Imports System.Runtime.InteropServices
#End Region

'Namespace Desktop
#Region "Public enums"
' ---------------------------
' Public enums (keep contracts)
' ---------------------------
' Specifies how a grid should be rendered
Public Enum GridKind
		None
		Lines   ' Continuous grid lines
		Points     ' Grid as points
		Crosses    ' Grid as crosses
	End Enum

	Public Enum AxisVisibility
		None
		Horizontal
		Vertical
		Both
	End Enum

	' Define Theme Mode Light/Dark
	Public Enum ThemeMode
		Light
		Dark
	End Enum

	' Define Coordinate Space Logical/Physical
	Public Enum CoordinateSpace
		Logical
		Physical
	End Enum

	''' <summary>
	''' available Tools for the interactive canvas.
	''' </summary>
	Public Enum ToolType
		None
		''' <summary>Select and manipulate existing shapes.</summary>
		SelectTool
		''' <summary>Draw straight lines.</summary>
		Line
		''' <summary>Draw rectangles.</summary>
		Rectangle
		''' <summary>Draw ellipses.</summary>
		Ellipse
		''' <summary>Draw Polyline</summary>
		Polyline
		''' <summary>Pan the viewport.</summary>
		Pan
		''' <summary>Zoom in/out of the viewport, typically by dragging a rectangle to define the zoom area or using mouse wheel for incremental zooming.</summary>
		Zoom
	End Enum
	'Public Enum ToolType
	'	None
	'	SelectTool
	'	LineTool
	'	RectangleTool
	'	CircleTool
	'	PanTool
	'	ZoomTool
	'End Enum

	Public Enum InteractionState
		Idle
		Drawing
		Dragging
		Selecting
	End Enum

	Public Enum HitKind
		None
		Inside
		Edge
		Vertex
	End Enum

	' Defines mouse click actions supported by the editor/viewer
	Public Enum ClickAction
		None
		' Value 1 deliberately unused to preserve legacy API compatibility
		Zoom
		MeasureDistance
	End Enum
	' Determines how resize operations affect content
	Public Enum ResizeMode
		Normal = 0     ' Maintain aspect ratio / normal behavior
		Stretch = 1    ' Stretch content to fit
	End Enum
#End Region




#Region "Public Structure RECT"
	' ---------------------------
	' RECT (logical rectangle)
	' ---------------------------
	' A custom rectangle structure that mirrors Win32 RECT semantics
	' but adds rich helpers, conversions, and geometry operations.
	' It uses "left, top, right, bottom" instead of width/height storage.
	<CLSCompliant(True), Serializable(),
 StructLayout(LayoutKind.Sequential, Pack:=4),
 DebuggerDisplay("Left={left} Top={top} Right={right} Bottom={bottom} [Width={Width},Height={Height}]")>
	Public Structure RECT
#Region "Public fields"
		' ----------- Public fields -----------
		' Raw rectangle edges (legacy-compatible layout)
		Public left As Integer
		Public top As Integer
		Public right As Integer
		Public bottom As Integer
#End Region
		' Returns a special sentinel value representing an invalid point
		Public Shared Function InvalidPoint() As Point
			Return New Point(Integer.MaxValue, Integer.MaxValue)
		End Function
#Region "Public Operators"
		' ----------- Operators -----------
		' Equality compares all four rectangle edges
		Public Shared Operator =(ByVal R1 As RECT, ByVal R2 As RECT) As Boolean
			Return R1.top = R2.top AndAlso R1.left = R2.left _
		   AndAlso R1.right = R2.right AndAlso R1.bottom = R2.bottom
		End Operator
		' Inequality is simply the inverse of equality
		Public Shared Operator <>(ByVal R1 As RECT, ByVal R2 As RECT) As Boolean
			Return Not (R1 = R2)
		End Operator
		' Converts RECT to System.Drawing.Rectangle
		' Width/Height are clamped to >= 0 to avoid invalid rectangles
		Public Shared Widening Operator CType(ByVal r As RECT) As Rectangle
			Return New Rectangle(
			r.left,
			r.top,
			Math.Max(0, r.right - r.left),
			Math.Max(0, r.bottom - r.top)
		)
		End Operator

		' Converts RECT to RectangleF (floating-point equivalent)
		Public Shared Widening Operator CType(ByVal r As RECT) As RectangleF
			Return New RectangleF(
			r.left,
			r.top,
			Math.Max(0, CSng(r.right - r.left)),
			Math.Max(0, CSng(r.bottom - r.top))
		)
		End Operator

		' Converts RectangleF back into a RECT (rounded outwards)
		Public Shared Widening Operator CType(ByVal r As RectangleF) As RECT
			Return New RECT(r)
		End Operator
#End Region
#Region "Constructors"
		' ----------- Constructors -----------

		' Copy constructor
		Public Sub New(ByVal inRect As RECT)
			Me.top = inRect.top
			Me.left = inRect.left
			Me.right = inRect.right
			Me.bottom = inRect.bottom
		End Sub

		' Construct from System.Drawing.Rectangle
		Public Sub New(ByVal inRect As Rectangle)
			Me.top = inRect.Y
			Me.left = inRect.X
			Me.right = inRect.X + inRect.Width
			Me.bottom = inRect.Y + inRect.Height
		End Sub

		' Construct from RectangleF, expanding to cover full area
		Public Sub New(ByVal inRect As RectangleF)
			Me.top = CInt(Math.Floor(inRect.Y))
			Me.left = CInt(Math.Floor(inRect.X))
			Me.right = CInt(Math.Ceiling(inRect.X + inRect.Width))
			Me.bottom = CInt(Math.Ceiling(inRect.Y + inRect.Height))
		End Sub

		' Construct a bounding rectangle from an array of points
		Public Sub New(ByVal vector() As Point)
			If vector Is Nothing OrElse vector.Length = 0 Then Exit Sub

			left = vector(0).X : right = vector(0).X
			top = vector(0).Y : bottom = vector(0).Y

			' Expand bounds to include each point
			For i As Integer = 1 To vector.Length - 1
				Dim p = vector(i)
				If p.X > right Then right = p.X Else If p.X < left Then left = p.X
				If p.Y > bottom Then bottom = p.Y Else If p.Y < top Then top = p.Y
			Next
		End Sub

		' Construct from explicit left/top/right/bottom values
		Public Sub New(ByVal left As Integer, ByVal top As Integer, ByVal right As Integer, ByVal bottom As Integer)
			Me.left = left : Me.top = top : Me.right = right : Me.bottom = bottom
		End Sub

		' Construct from a top-left point and a size
		Public Sub New(ByVal topLeft As Point, ByVal size As Size)
			Me.left = topLeft.X : Me.top = topLeft.Y
			Me.right = topLeft.X + size.Width
			Me.bottom = topLeft.Y + size.Height
		End Sub

		' Construct from two corner points
		Public Sub New(ByVal topLeft As Point, ByVal bottomRight As Point)
			Me.left = topLeft.X : Me.top = topLeft.Y
			Me.right = bottomRight.X : Me.bottom = bottomRight.Y
		End Sub
#End Region
#Region "Properties"
		' ----------- Properties -----------

		' X-coordinate of rectangle (left edge)
		' Setting X keeps the width constant
		Public Property X() As Integer
			Get
				Return left
			End Get
			Set(ByVal value As Integer)
				Dim w = Width
				left = value
				right = left + w
			End Set
		End Property

		' Y-coordinate of rectangle (top edge)
		' Setting Y keeps the height constant
		Public Property Y() As Integer
			Get
				Return top
			End Get
			Set(ByVal value As Integer)
				top = value
				bottom = top + Height
			End Set
		End Property

		' Width derived from right - left
		Public Property Width() As Integer
			Get
				Return right - left
			End Get
			Set(ByVal value As Integer)
				right = left + value
			End Set
		End Property

		' Height derived from bottom - top
		Public Property Height() As Integer
			Get
				Return bottom - top
			End Get
			Set(ByVal value As Integer)
				bottom = top + value
			End Set
		End Property

		' Integer center point of the rectangle
		Public ReadOnly Property CenterPoint() As Point
			Get
				Return New Point((left + right) \ 2, (top + bottom) \ 2)
			End Get
		End Property

		' Size represented as System.Drawing.Size
		Public ReadOnly Property Size() As Size
			Get
				Return New Size(Width, Height)
			End Get
		End Property

		' Common anchor points used for resizing, snapping, etc.
		Public Property TopLeft() As Point
			Get
				Return New Point(left, top)
			End Get
			Set(ByVal value As Point)
				left = value.X : top = value.Y
			End Set
		End Property

		Public Property TopRight() As Point
			Get
				Return New Point(right, top)
			End Get
			Set(ByVal value As Point)
				right = value.X : top = value.Y
			End Set
		End Property

		Public Property BottomRight() As Point
			Get
				Return New Point(right, bottom) : End Get
			Set(ByVal value As Point)
				right = value.X : bottom = value.Y
			End Set
		End Property

		Public ReadOnly Property BottomCenter() As Point
			Get
				Return New Point((left + right) \ 2, bottom) : End Get
		End Property

		Public ReadOnly Property TopCenter() As Point
			Get
				Return New Point((left + right) \ 2, top) : End Get
		End Property

		Public ReadOnly Property LeftCenter() As Point
			Get
				Return New Point(left, (top + bottom) \ 2) : End Get
		End Property

		Public ReadOnly Property RightCenter() As Point
			Get
				Return New Point(right, (top + bottom) \ 2) : End Get
		End Property

		Public Property BottomLeft() As Point
			Get
				Return New Point(left, bottom) : End Get
			Set(ByVal value As Point)
				left = value.X : bottom = value.Y
			End Set
		End Property

		' Size state helpers
		Public ReadOnly Property IsZeroSized() As Boolean
			Get
				Return Height = 0 AndAlso Width = 0 : End Get
		End Property

		Public ReadOnly Property IsNonZeroSized() As Boolean
			Get
				Return Not IsZeroSized : End Get
		End Property

		' Normalized means right >= left and bottom >= top
		Public ReadOnly Property IsNormalized() As Boolean
			Get
				Return (right >= left) AndAlso (bottom >= top)
			End Get
		End Property
#End Region
#Region "Normalization"
		' ----------- Normalization -----------

		' Debug-only validation for inverted rectangles
		Public Sub AssertIfNotNormalized()
			If Not IsNormalized() Then
				Debug.Assert(right >= left, "RECT.right and RECT.left are inverted!")
				Debug.Assert(bottom >= top, "RECT.bottom and RECT.top are inverted!")
			End If
		End Sub

		' Swaps edges to enforce proper left/top/right/bottom ordering
		Public Sub NormalizeRect()
			If right < left Then
				Dim tmp = right : right = left : left = tmp
			End If
			If bottom < top Then
				Dim tmp = bottom : bottom = top : top = tmp
			End If
		End Sub
#End Region
#Region "Move & Resize"
		' ----------- Move & Resize -----------

		' Moves the rectangle by an offset
		Public Sub Offset(ByVal x As Integer, ByVal y As Integer)
			left += x : right += x
			top += y : bottom += y
		End Sub

		Public Sub Offset(ByVal offs As Point)
			Offset(offs.X, offs.Y)
		End Sub

		' Expands the rectangle equally in all directions
		Public Sub Inflate(ByVal size As Size)
			Inflate(size.Width, size.Height)
		End Sub

		Public Sub Inflate(ByVal width As Integer, ByVal height As Integer)
			left -= width : right += width
			top -= height : bottom += height
		End Sub

		' Asymmetric inflation (per-edge)
		Public Sub Inflate(ByVal leftInflate As Integer, ByVal topInflate As Integer,
					   ByVal rightInflate As Integer, ByVal bottomInflate As Integer)
			left -= leftInflate
			top -= topInflate
			right += rightInflate
			bottom += bottomInflate
		End Sub

		' Scales the rectangle relative to a fixed reference point (zoom behavior)
		Public Function ExpandFromFixedPoint(ByVal zoomMultiplier As Single, ByVal fixedPoint As Point) As RECT
			Dim dx = (left - fixedPoint.X) * zoomMultiplier
			Dim dy = (top - fixedPoint.Y) * zoomMultiplier
			Dim newX = fixedPoint.X + dx
			Dim newY = fixedPoint.Y + dy
			Dim newW = zoomMultiplier * Width
			Dim newH = zoomMultiplier * Height

			Return New RECT(
			CInt(newX),
			CInt(newY),
			CInt(newX + newW),
			CInt(newY + newH)
		)
		End Function
#End Region
#Region "Containment & intersection"
		' ----------- Containment & intersection -----------

		' Returns True if this rectangle is fully inside aRect
		Public Function IsContainedIn(ByRef aRect As RECT) As Boolean
			AssertIfNotNormalized() : aRect.AssertIfNotNormalized()
			Return bottom <= aRect.bottom AndAlso top >= aRect.top _
		   AndAlso left >= aRect.left AndAlso right <= aRect.right
		End Function

		Public Function Contains(ByRef aRect As RECT) As Boolean
			Return aRect.IsContainedIn(Me)
		End Function

		Public Function Contains(ByRef aRect As Rectangle) As Boolean
			Return New RECT(aRect).IsContainedIn(Me)
		End Function

		' Point containment checks (integer and float)
		Public Function Contains(ByRef pt As PointF) As Boolean
			AssertIfNotNormalized()
			Return Not (pt.X > right OrElse pt.X < left OrElse pt.Y > bottom OrElse pt.Y < top)
		End Function

		Public Function Contains(ByRef pt As Point) As Boolean
			AssertIfNotNormalized()
			Return Not (pt.X > right OrElse pt.X < left OrElse pt.Y > bottom OrElse pt.Y < top)
		End Function

		' Checks whether two rectangles overlap
		Public Function IntersectsWith(ByRef rect As RECT) As Boolean
			AssertIfNotNormalized() : rect.AssertIfNotNormalized()
			Return Not RECT.Intersect(Me, rect).IsZeroSized
		End Function


		' Debug / utility helpers
		Public Overrides Function ToString() As String
			Return $"Left={left} Top={top} Right={right} Bottom={bottom} [Width={Width},Height={Height}]"
		End Function

		' Returns rectangle edges as a closed polygon
		Public Function ToPointArray() As Point()
			Return {
			New Point(left, top),
			New Point(left, bottom),
			New Point(right, bottom),
			New Point(right, top),
			New Point(left, top)
		}
		End Function

		Public Function ToRectangle() As Rectangle
			Return New Rectangle(left, top, Width, Height)
		End Function
#End Region
#Region "Static helpers"
		' ----------- Static helpers -----------

		' Union = smallest rectangle covering both
		Public Shared Function Union(ByRef a As RECT, ByRef b As RECT) As RECT
			Return New RECT(Rectangle.Union(CType(a, Rectangle), CType(b, Rectangle)))
		End Function

		' Union ignoring zero-sized rectangles
		Public Shared Function UnionWithoutZeroSized(ByRef a As RECT, ByRef b As RECT) As RECT
			If a.IsZeroSized Then Return b
			If b.IsZeroSized Then Return a
			Return Union(a, b)
		End Function

		' Intersection = overlapping area
		Public Shared Function Intersect(ByRef a As RECT, ByRef b As RECT) As RECT
			Return New RECT(Rectangle.Intersect(CType(a, Rectangle), CType(b, Rectangle)))
		End Function

		Public Shared Function IntersectWithoutInvalid(ByVal a As RECT, ByVal b As RECT) As RECT
			If a.IsZeroSized Then Return b
			If b.IsZeroSized Then Return a
			Return Intersect(a, b)
		End Function

		'''<returns> Returns a new inflated copy</returns>
		Public Shared Function Inflate(ByVal r As RECT, ByVal x As Integer, ByVal y As Integer) As RECT
			Dim tmp = r : tmp.Inflate(x, y) : Return tmp
		End Function


		'''<returns>Returns rectangle bounds covering a set of points</returns>
		Public Shared Function CoordsBoundaries(ByVal coords() As Point) As RECT
			Dim ret As RECT
			If coords IsNot Nothing AndAlso coords.Length > 0 Then
				ret.left = coords(0).X : ret.right = coords(0).X
				ret.top = coords(0).Y : ret.bottom = coords(0).Y
				For Each p In coords
					If p.X > ret.right Then ret.right = p.X
					If p.X < ret.left Then ret.left = p.X
					If p.Y > ret.bottom Then ret.bottom = p.Y
					If p.Y < ret.top Then ret.top = p.Y
				Next
			End If
			Return ret
		End Function

		' Special invalid rectangle sentinel
		Public Shared ReadOnly Property Invalid As RECT
			Get
				Return New RECT(Integer.MaxValue, Integer.MaxValue, Integer.MinValue, Integer.MinValue)
			End Get
		End Property
#End Region
	End Structure
#End Region


#Region "Public Structure SEGMENT"
	''' <summary>
	''' Defines a line segment in 2D space using two endpoints (P0 and P1). The segment is represented by four integer fields (X0, Y0, X1, Y1) corresponding to the coordinates of the endpoints. The structure provides properties to access the endpoints as System.Drawing.Point structures, as well as methods for geometric calculations such as length and direction.
	''' </summary>
	<CLSCompliantAttribute(True), Serializable(), StructLayout(LayoutKind.Sequential, Pack:=4)>
	<DebuggerDisplay("{GetDebuggerDisplay(),nq}")>
	Public Structure SEGMENT

#Region "Public Members"
		''' <summary>Defines a line segment in 2D space using two endpoints (P0 and P1).<br></br> The segment is represented by four integer fields (X0, Y0, X1, Y1) corresponding to the coordinates of the endpoints.<br></br> The structure provides properties to access the endpoints as System.Drawing.Point structures, as well as methods for geometric calculations such as length and direction.</summary>
		Dim X0 As Integer
		''' <summary>The Y coordinate of the first endpoint (P0) of the segment.</summary>
		Dim Y0 As Integer
		''' <summary>The X coordinate of the second endpoint (P1) of the segment.</summary>
		Dim X1 As Integer
		''' <summary>The Y coordinate of the second endpoint (P1) of the segment.</summary>
		Dim Y1 As Integer

		''' <summary>
		''' Gets or sets the first endpoint (P0) of the segment as a System.Drawing.Point structure. The getter constructs a Point from the X0 and Y0 fields, while the setter updates X0 and Y0 based on the provided Point's coordinates. Similarly, the P1 property manages the second endpoint using X1 and Y1.
		''' </summary>
		''' <returns>
		''' A System.Drawing.Point representing the first endpoint (P0) of the segment, constructed from the X0 and Y0 fields. Setting this property updates X0 and Y0 to match the coordinates of the provided Point. The P1 property behaves similarly for the second endpoint.
		''' </returns>
		Public Property P0() As System.Drawing.Point
			Get
				Return New System.Drawing.Point(X0, Y0)
			End Get
			Set(ByVal value As System.Drawing.Point)
				X0 = value.X
				Y0 = value.Y
			End Set
		End Property

		''' <summary>
		''' Gets or sets the second endpoint (P1) of the segment as a System.Drawing.Point structure. The getter constructs a Point from the X1 and Y1 fields, while the setter updates X1 and Y1 based on the provided Point's coordinates. The P0 property manages the first endpoint using X0 and Y0 in a similar manner.
		''' </summary>
		''' <returns>
		''' A System.Drawing.Point representing the second endpoint (P1) of the segment, constructed from the X1 and Y1 fields. Setting this property updates X1 and Y1 to match the coordinates of the provided Point. The P0 property behaves similarly for the first endpoint.
		''' </returns>
		Public Property P1() As System.Drawing.Point
			Get
				Return New System.Drawing.Point(X1, Y1)
			End Get
			Set(ByVal value As System.Drawing.Point)
				X1 = value.X
				Y1 = value.Y
			End Set
		End Property
#End Region

#Region "Constructors"
		''' <summary>
		''' Initializes a new instance of the SEGMENT structure by copying the coordinates from another SEGMENT instance.
		''' </summary>
		''' <param name="aSEGMENT"></param>
		Public Sub New(ByVal aSEGMENT As SEGMENT)
			Try
				Me.X0 = aSEGMENT.X0
				Me.Y0 = aSEGMENT.Y0
				Me.X1 = aSEGMENT.X1
				Me.Y1 = aSEGMENT.Y1
			Catch ex As Exception
				MsgBox(ex.Message)
			End Try
		End Sub

		''' <summary>
		''' Initializes a new instance of the SEGMENT structure with specified endpoint coordinates.
		''' </summary>
		''' <param name="X0"></param>
		''' <param name="Y0"></param>
		''' <param name="X1"></param>
		''' <param name="Y1"></param>
		Public Sub New(ByVal X0 As Integer, ByVal Y0 As Integer, ByVal X1 As Integer, ByVal Y1 As Integer)
			Try
				Me.X0 = X0
				Me.Y0 = Y0
				Me.X1 = X1
				Me.Y1 = Y1
			Catch ex As Exception
				MsgBox(ex.Message)
			End Try
		End Sub

		''' <summary>
		''' Initializes a new instance of the SEGMENT structure using two System.Drawing.Point structures as endpoints.
		''' </summary>
		''' <param name="P0"></param>
		''' <param name="P1"></param>
		Public Sub New(ByVal P0 As System.Drawing.Point, ByVal P1 As System.Drawing.Point)
			Try
				Me.X0 = P0.X
				Me.Y0 = P0.Y
				Me.X1 = P1.X
				Me.Y1 = P1.Y
			Catch ex As Exception
				MsgBox(ex.Message)
			End Try
		End Sub
#End Region

#Region "Public Functions"

		''' <summary>
		''' Determines if a given X coordinate falls within the horizontal bounds of the segment.
		''' </summary>
		''' <param name="XQuote">
		''' The X coordinate to test for containment within the segment's horizontal range.
		''' </param>
		''' <returns>
		''' True if XQuote is between the X coordinates of P0 and P1 (inclusive); otherwise, False.
		''' </returns>
		Public Function ContainsX(ByVal XQuote As Integer) As Boolean
			If (XQuote >= P0.X) AndAlso (XQuote <= P1.X) Then  '(Valido se P1 a sinistra di P0)
				Return True
			Else
				If (XQuote >= P1.X) AndAlso (XQuote <= P0.X) Then  '(Valido se P0 a sinistra di P1)
					Return True
				End If
			End If
			Return False
		End Function

		''' <summary>
		''' Determines if a given Y coordinate falls within the vertical bounds of the segment.
		''' </summary>
		''' <param name="YQuote">
		''' The Y coordinate to test for containment within the segment's vertical range.
		''' </param>
		''' <returns>
		''' True if YQuote is between the Y coordinates of P0 and P1 (inclusive); otherwise, False.
		''' </returns>
		Public Function MediumPoint() As System.Drawing.Point
			Try
				Dim retVal As System.Drawing.Point
				retVal.X = CInt((Me.X0 + Me.X1) / 2)
				retVal.Y = CInt((Me.Y0 + Me.Y1) / 2)
				Return retVal
			Catch ex As Exception
				MsgBox(ex.Message)
			End Try
		End Function

		''' <summary>
		''' Returns the length of the segment defined by points P0 and P1.
		''' </summary>
		''' <param name="P0">
		''' The starting point of the segment, represented as a System.Drawing.Point structure.
		''' </param>
		''' <param name="P1">
		''' The ending point of the segment, represented as a System.Drawing.Point structure.
		''' </param>
		''' <returns>
		''' The length of the segment as a Double. Returns 0 if an error occurs (e.g., overflow).
		''' </returns>
		Public Shared Function SegmentModule(ByVal P0 As System.Drawing.Point, ByVal P1 As System.Drawing.Point) As Double
			Try
				Return System.Math.Sqrt(System.Math.Pow(P1.X - P0.X, 2) + System.Math.Pow(P1.Y - P0.Y, 2))
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>
		''' Returns the length of the segment (distance between P0 and P1)
		''' </summary>
		''' <returns>
		''' The length of the segment as a Double. Returns 0 if an error occurs (e.g., overflow).
		''' </returns>
		Public Function SegmentModule() As Double
			Try
				Return System.Math.Sqrt(System.Math.Pow(X1 - X0, 2) + System.Math.Pow(Y1 - Y0, 2))
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>
		''' Returns the direction (angle in radians) of the segment ...
		''' </summary>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Function SegmentDirection() As Double
			Try
				Dim dblHyp As Double = 0
				Dim dblSin As Double = 0
				Dim RefX As Double = 0
				Dim RefY As Double = 0
				'I translate the segment so that it starts from the origin...
				RefX = X1 - X0
				RefY = -(Y1 - Y0)
				'Memo: In Windows, the Y-axis is inverted...
				'Reverting to the standard coordinate system to
				'apply trigonometric formulas...'

				If (RefY = 0) Then
					'Horizontal segment...
					If (RefX > 0) Then
						'Null angle ...
						Return 0
					Else
						'Flat angle...
						Return System.Math.PI
					End If
				End If

				If (RefX = 0) Then
					'Vertical segment...
					If (RefY > 0) Then
						Return System.Math.PI / 2
					Else
						Return -System.Math.PI / 2
					End If
				End If

				'If I've gotten this far,
				'the angle is not a multiple of Pi/2...
				If (RefX > 0) Then
					If (RefY > 0) Then
						'First quadrant ....
						dblHyp = System.Math.Sqrt((RefX * RefX + RefY * RefY))    'Ipotenusa ...
						dblSin = RefY / dblHyp
						Return System.Math.Atan(dblSin / System.Math.Sqrt(-dblSin * dblSin + 1))
					Else
						'Fourth quadrant ...
						RefY = -RefY
						dblHyp = System.Math.Sqrt((RefX * RefX + RefY * RefY))    'Ipotenusa ...
						dblSin = RefY / dblHyp
						Return (2 * System.Math.PI) - System.Math.Atan(dblSin / System.Math.Sqrt(-dblSin * dblSin + 1))
					End If
				Else
					If (RefY > 0) Then
						'Second quadrant ...
						RefX = -RefX
						dblHyp = System.Math.Sqrt((RefX * RefX + RefY * RefY))  'Ipotenusa ...
						dblSin = CDbl(RefY) / dblHyp
						Return -System.Math.Atan(dblSin / System.Math.Sqrt(-dblSin * dblSin + 1)) + System.Math.PI
					Else
						'Third quadrant ...
						RefX = -RefX
						RefY = -RefY
						dblHyp = System.Math.Sqrt((RefX * RefX + RefY * RefY))  'Ipotenusa ...
						dblSin = RefY / dblHyp
						Return System.Math.Atan(dblSin / System.Math.Sqrt(-dblSin * dblSin + 1)) + System.Math.PI
					End If
				End If
			Catch ex As Exception
				Return 0
			End Try
		End Function
#End Region

#Region "Operators"
		''' <summary>
		''' Determines if two segments are equal by comparing their endpoints.
		''' </summary>
		''' <param name="S1">
		''' The first segment to compare, represented as a SEGMENT structure with endpoints (X0, Y0) and (X1, Y1).
		''' </param>
		''' <param name="S2">
		''' The second segment to compare, represented as a SEGMENT structure with endpoints (X0, Y0) and (X1, Y1).
		''' </param>
		''' <returns>
		''' True if S1 and S2 have the same endpoints (regardless of order); otherwise, False.
		''' </returns>
		Public Shared Operator =(ByVal S1 As SEGMENT, ByVal S2 As SEGMENT) As Boolean
			If (S1.X0 = S2.X0) AndAlso (S1.X1 = S2.X1) AndAlso (S1.Y0 = S2.Y0) AndAlso (S1.Y1 = S2.Y1) Then
				Return True
			Else
				Return False
			End If
		End Operator

		''' <summary>
		''' Determines if two segments are not equal by comparing their endpoints.
		''' </summary>
		''' <param name="S1">
		''' The first segment to compare, represented as a SEGMENT structure with endpoints (X0, Y0) and (X1, Y1).
		''' </param>
		''' <param name="S2">
		''' The second segment to compare, represented as a SEGMENT structure with endpoints (X0, Y0) and (X1, Y1).
		''' </param>
		''' <returns>
		''' True if S1 and S2 have different endpoints; otherwise, False.
		''' </returns>
		Public Shared Operator <>(ByVal S1 As SEGMENT, ByVal S2 As SEGMENT) As Boolean
			If (S1.X0 <> S2.X0) Or (S1.X1 <> S2.X1) Or (S1.Y0 <> S2.Y0) Or (S1.Y1 <> S2.Y1) Then
				Return True
			Else
				Return False
			End If
		End Operator

	''' <summary
	''' Return Debugger Display
	''' </summary>
	Private Function GetDebuggerDisplay() As String
		Return ToString()
	End Function
#End Region

End Structure
#End Region
