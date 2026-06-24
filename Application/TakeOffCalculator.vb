''' src\CoNSoL.Application\TakeOffCalculator.vb
''' Modify TakeOffContext — place in Application namespace and import domain entities (Block, Material, Formula stubs are in domain)

Imports System.Linq
Imports System.Text.Json
Imports Domain.Entities
Imports Domain.Utilities

Public Class TakeOffCalculator

	Public Function Calculate(layout As CanvasLayout, ctx As TakeOffContext) As TakeOffResult

		Dim result As New TakeOffResult()

		For Each element In layout.Elements
			If String.IsNullOrWhiteSpace(element.BusinessJson) Then Continue For
			Dim def = JsonSerializer.Deserialize(Of BusinessDefinition)(element.BusinessJson)
			Dim qty = CalculateElementQuantity(element, def, GetRelationships(layout))
			Dim breakdown = ApplyFormula(def, qty, ctx)

			' ? FIX: if no formula, fallback to raw qty
			If breakdown Is Nothing OrElse breakdown.Count = 0 Then
				result.Add(def.BlockCode, qty)
			Else
				Dim total = breakdown.Values.Sum()
				result.Add(def.BlockCode, total)
			End If
		Next

		Return result
	End Function

	Private Shared Function GetRelationships(layout As CanvasLayout) As IEnumerable(Of ElementRelationship)
		Return layout.Relationships
	End Function

	Private Function CalculateElementQuantity(element As CanvasElement, def As BusinessDefinition, rels As IEnumerable(Of ElementRelationship)) As Decimal
		Select Case def.DimensionMode
			Case "D0"
				Return 1D
			Case "D1"
				Return Geometry.Length(element.GeometryJson)
			Case "D2"
				Return Geometry.Area(element.GeometryJson)
			Case "D3"
				Return Geometry.Volume(element.GeometryJson, def.Parameters)
			Case Else
				Throw New InvalidOperationException("Unknown dimension mode")
		End Select
	End Function

	Private Shared Function ApplyFormula(
		def As BusinessDefinition,
		qty As Decimal,
		ctx As TakeOffContext
	) As Dictionary(Of String, Decimal)

		' ? SAFE fallback logic
		Dim result As New Dictionary(Of String, Decimal)

		If String.IsNullOrEmpty(def.BlockCode) Then Return result

		' If price exists ? calculate directly
		If ctx.Prices IsNot Nothing AndAlso ctx.Prices.ContainsKey(def.BlockCode) Then
			Dim price = ctx.Prices(def.BlockCode)
			result(def.BlockCode) = qty * price
		End If

		Return result

	End Function

End Class

'Imports Domain.Entities
'Imports Infrastructure.Logging
''Imports Newtonsoft.Json.Linq
'Imports Domain.Utilities
'Imports Domain
'Imports System.Text.Json
'Imports System.Linq
'Imports Newtonsoft.Json.Linq

'Namespace Services
'	''' <summary>
'	''' Core calculation engine for take-off quantity and cost computation.
'	''' </summary>
'	''' <remarks>
'	''' TakeOffCalculator implements the business logic for:
'	''' - Extracting quantities from shapes based on dimension mode (D0-D3)
'	''' - Handling nested objects (parent-child relationships)
'	''' - Computing costs (quantity × unit price)
'	''' - Aggregating results by material or layer
'	''' 
'	''' Dimension Modes:
'	''' - D0: Count (number of objects)
'	''' - D1: Length (line length or perimeter)
'	''' - D2: Area (width × height)
'	''' - D3: Volume (area × depth)
'	''' 
'	''' Related Use Cases:
'	''' - UC-004: Run take-off quantity summary
'	''' - UC-006: Edit multi-selection properties
'	''' </remarks>
'	Public Class TakeOffCalculator

'		Public ReadOnly _logger As ILogger

'		Public Sub New(logger As ILogger)
'			_logger = logger
'		End Sub

'		''' <summary>
'		''' Calculates quantities and costs for all elements in layout.
'		''' </summary>
'		''' <param name="layout">Canvas layout to calculate</param>
'		''' <param name="context">Calculation context (unit system, options)</param>
'		''' <returns>Take-off results grouped by material</returns>
'		''' <remarks>
'		''' Main orchestration method. Steps:
'		''' 1. Validate inputs
'		''' 2. Group elements by material
'		''' 3. For each group: extract quantity based on dimension mode
'		''' 4. Apply nested object logic (subtract children)
'		''' 5. Calculate total cost (qty × price)
'		''' 6. Return aggregated results
'		''' 
'		''' Guarantees:
'		''' - Deterministic: same input always produces same output
'		''' - No mutations: layout/elements not modified
'		''' - All quantities >= 0 (no negative quantities)
'		''' </remarks>
'		''' <exception cref="ArgumentNullException">If layout or context is Nothing</exception>
'		''' <exception cref="InvalidOperationException">If calculation fails</exception>
'		'Public Function Calculate(layout As CanvasLayout, ctx As TakeOffContext) As TakeOffResult

'		'	Dim result As New TakeOffResult()

'		'	For Each element In layout.Elements
'		'		If String.IsNullOrWhiteSpace(element.BusinessJson) Then Continue For

'		'		Dim def = JsonSerializer.Deserialize(Of BusinessDefinition)(element.BusinessJson)
'		'		Dim qty = CalculateElementQuantity(element, def, GetRelationships(layout))
'		'		'Dim breakdown = ApplyFormula(def, qty, ctx)

'		'		Dim breakdown = ApplyFormula(def, qty, ctx)
'		'		Dim total = breakdown.Values.Sum()
'		'		result.Add(def.BlockCode, total)

'		'		'result.Add(def.BlockCode, breakdown)
'		'	Next
'		'	Return result
'		'End Function

'		Private Shared Function GetRelationships(layout As CanvasLayout) As Object
'			Return layout.Relationships
'		End Function

'		'''<sammry>Calculate Element Quantity</sammry>
'		Private Function CalculateElementQuantity(
'		element As CanvasElement,
'		def As BusinessDefinition,
'		rels As IEnumerable(Of ElementRelationship)) As Decimal
'			Select Case def.DimensionMode
'				Case "D0" : Return 1
'				Case "D1" : Return Geometry.Length(element.GeometryJson)
'				Case "D2" : Return Geometry.Area(element.GeometryJson)
'				Case "D3" : Return Geometry.Volume(element.GeometryJson, def.Parameters)
'				Case Else : Throw New InvalidOperationException("Unknown dimension mode")
'			End Select
'		End Function

'		''' <summary>
'		''' Apply a formula to calculate breakdown costs/quantities from a quantity and context.
'		''' </summary>
'		''' <param name="def">The business definition containing formula references.</param>
'		''' <param name="qty">The calculated quantity.</param>
'		''' <param name="ctx">The takeoff context with material costs and pricing.</param>
'		''' <returns>A breakdown dictionary of costs by material.</returns>
'		Private Shared Function ApplyFormula(def As BusinessDefinition, qty As Decimal, ctx As TakeOffContext) As Dictionary(Of String, Decimal)
'			' TODO: Implement formula application logic
'			' For now, return empty breakdown
'			Return New Dictionary(Of String, Decimal)()
'		End Function

'		''' <summary>
'		''' Extracts length dimension from shape geometry.
'		''' </summary>
'		''' <param name="shapeType">Type of shape</param>
'		''' <param name="geometry">Geometry JSON</param>
'		''' <returns>Length in canvas units</returns>
'		''' <remarks>
'		''' Used for D1 (length) dimension mode.
'		''' 
'		''' Calculations:
'		''' - Line: distance(start, end)
'		''' - Rectangle: width OR height
'		''' - Circle: 2?r (circumference)
'		''' - Polyline: sum of segment lengths
'		''' 
'		''' Returns 0 for invalid types or missing geometry.
'		''' </remarks>
'		Private Function ExtractLength(shapeType As String, geometry As JObject) As Double
'			' Implementation (BUS-005)
'			Return 0.0
'		End Function



'		Public Function Calculate(layout As CanvasLayout, context As TakeOffContext) As TakeOffResult

'			_logger.Info("Starting TakeOff Calculation")

'			Dim totalQty As Double = 0
'			Dim totalCost As Double = 0

'			For Each el In layout.Elements
'				If String.IsNullOrEmpty(el.BusinessJson) Then Continue For

'				Dim business = JObject.Parse(el.BusinessJson)
'				Dim mode = business("dimensionMode")?.ToString()
'				Dim price = business("unitPrice")?.ToObject(Of Double)()
'				Dim geometry = JObject.Parse(el.GeometryJson)
'				Dim qty = ExtractQuantity(mode, geometry)

'				totalQty += qty
'				totalCost += qty * price

'				_logger.Debug($"Element {el.Id} => Qty={qty}, Cost={qty * price}")
'			Next

'			_logger.Info("Calculation Complete")

'			Return New TakeOffResult With {
'				.TotalQuantity = totalQty,
'				.totalCost = totalCost
'				}

'		End Function

'		Private Function ExtractQuantity(mode As String, geom As JObject) As Double

'			Select Case mode
'				Case "D1" ' Length
'					Return Distance(geom("start"), geom("end"))
'				Case "D2" ' Area
'					Dim w = geom("width")?.ToObject(Of Double)()
'					Dim h = geom("height")?.ToObject(Of Double)()
'					Return w * h
'				Case "D0"
'					Return 1
'				Case Else
'					Return 0
'			End Select

'		End Function

'		Private Function Distance(p1 As JToken, p2 As JToken) As Double
'			Dim dx = p1("x") - p2("x")
'			Dim dy = p1("y") - p2("y")
'			Return Math.Sqrt(dx * dx + dy * dy)
'		End Function

'		'Imports Domain.Utilities
'		'Imports Domain.Entities
'		'Imports Domain
'		'Imports System.Text.Json
'		'Imports System.Linq



'		'''' <summary>
'		'''' Core calculation engine for take-off quantity and cost computation.
'		'''' </summary>
'		'''' <remarks>
'		'''' TakeOffCalculator implements the business logic for:
'		'''' - Extracting quantities from shapes based on dimension mode (D0-D3)
'		'''' - Handling nested objects (parent-child relationships)
'		'''' - Computing costs (quantity × unit price)
'		'''' - Aggregating results by material or layer
'		'''' 
'		'''' Dimension Modes:
'		'''' - D0: Count (number of objects)
'		'''' - D1: Length (line length or perimeter)
'		'''' - D2: Area (width × height)
'		'''' - D3: Volume (area × depth)
'		'''' 
'		'''' Related Use Cases:
'		'''' - UC-004: Run take-off quantity summary
'		'''' - UC-006: Edit multi-selection properties
'		'''' </remarks>
'		'Public Class TakeOffCalculator
'		''' <summary>
'		''' Calculates quantities and costs for all elements in layout.
'		''' </summary>
'		''' <param name="layout">Canvas layout to calculate</param>
'		''' <param name="context">Calculation context (unit system, options)</param>
'		''' <returns>Take-off results grouped by material</returns>
'		''' <remarks>
'		''' Main orchestration method. Steps:
'		''' 1. Validate inputs
'		''' 2. Group elements by material
'		''' 3. For each group: extract quantity based on dimension mode
'		''' 4. Apply nested object logic (subtract children)
'		''' 5. Calculate total cost (qty × price)
'		''' 6. Return aggregated results
'		''' 
'		''' Guarantees:
'		''' - Deterministic: same input always produces same output
'		''' - No mutations: layout/elements not modified
'		''' - All quantities >= 0 (no negative quantities)
'		''' </remarks>
'		''' <exception cref="ArgumentNullException">If layout or context is Nothing</exception>
'		''' <exception cref="InvalidOperationException">If calculation fails</exception>
'		'Public Function Calculate(layout As CanvasLayout, ctx As TakeOffContext) As TakeOffResult

'		'	Dim result As New TakeOffResult()

'		'	For Each element In layout.Elements
'		'		If String.IsNullOrWhiteSpace(element.BusinessJson) Then Continue For

'		'		Dim def = JsonSerializer.Deserialize(Of BusinessDefinition)(element.BusinessJson)
'		'		Dim qty = CalculateElementQuantity(element, def, GetRelationships(layout))
'		'		'Dim breakdown = ApplyFormula(def, qty, ctx)

'		'		Dim breakdown = ApplyFormula(def, qty, ctx)
'		'		Dim total = breakdown.Values.Sum()
'		'		result.Add(def.BlockCode, total)

'		'		'result.Add(def.BlockCode, breakdown)
'		'	Next
'		'	Return result
'		'End Function
'	End Class
'End Namespace




