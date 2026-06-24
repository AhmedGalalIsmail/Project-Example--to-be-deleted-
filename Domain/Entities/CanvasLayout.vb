Option Strict On

Namespace Entities
	''' <summary>
	''' Represents the entire state of a 2D drawing canvas.
	''' Manages collection of elements, layers, and coordinate system parameters.
	''' </summary>
	''' <remarks>
	''' The CanvasLayout is the root domain entity that holds all drawing data.
	''' It implements invariant validation to ensure data integrity across operations.
	''' 
	''' Related Use Cases:
	''' - UC-001: Draw shapes on canvas
	''' - UC-002: Assign objects to layers
	''' - UC-004: Run take-off summary
	''' </remarks>
	Public Class CanvasLayout
		''' <summary>
		''' Unique identifier for this canvas/drawing.
		''' </summary>
		''' <remarks>
		''' Auto-generated as Guid when instance created.
		''' Used for persistence and file identification.
		''' </remarks>
		Public Property CanvasId As Guid = Guid.NewGuid()

		''' <summary>
		''' Unit system used for all coordinates and measurements.
		''' </summary>
		''' <remarks>
		''' Valid values: "meter", "foot", "inch", "centimeter"
		''' Used for dimension calculations and exports.
		''' Invariant: Must be non-empty string.
		''' </remarks>
		Public Property Unit As String = "meter"

		''' <summary>
		''' Scale factor for converting logical coordinates to pixels.
		''' </summary>
		''' <remarks>
		''' Default: 1.0 (1 logical unit = 1 pixel)
		''' Used by rendering layer for coordinate transformation.
		''' Invariant: Must be > 0
		''' </remarks>
		Public Property ScaleFactor As Double = 1.0

		''' <summary>Collection of all drawn elements on this canvas.</summary>
		''' <remarks>Each element has geometry (visual) and business (metadata). Invariant: Never null, may be empty</remarks>
		Public Property Elements As New List(Of CanvasElement)()

		''' <summary>Collection of relationships between elements (e.g. parent-child, connections).</summary>
		''' <returns></returns>
		Public Property Relationships As New List(Of CanvasElement)()

		''' <summary>
		''' Validates the layout state against all invariants.
		''' </summary>
		''' <remarks>
		''' Called before persistence or calculation operations.
		''' Throws InvalidOperationException if invariant violated.
		''' </remarks>
		''' <exception cref="InvalidOperationException">If invariant check fails</exception>
		Public Sub Validate()
			' Implementation to be added (FND-003)
			If Me.CanvasId = Guid.Empty Then
				Throw New InvalidOperationException("CanvasId cannot be empty")
			End If
		End Sub
	End Class
End Namespace