
Imports Domain.Common

Namespace Entities

	''' <summary>
	''' Represents a logical grouping of drawing elements.
	''' Controls visibility, locking, and rendering behavior.
	''' </summary>
	Public Class Layer
		''' <summary>
		''' The unique identifier for the layer. Initialized to a new GUID by default.
		''' </summary>
		Public Property Id As Guid = Guid.NewGuid()
		Public Property LayerId
		''' <summary>
		''' The name of the layer. This should be unique and descriptive.
		''' </summary>
		Public Property Name As String
		''' <summary>
		''' Indicates whether the layer is currently visible on the canvas. If false, elements on this layer will not be rendered.
		''' </summary>
		Public Property IsVisible As Boolean = True
		''' <summary>
		''' Indicates whether the layer is locked. If true, elements on this layer cannot be modified or moved.
		''' </summary>
		Public Property IsLocked As Boolean = False
		''' <summary>
		''' The color associated with the layer, represented as a hex string (e.g., "#FFFFFF" for white).
		''' This can be used for rendering elements on this layer with a specific color.</summary>
		''' <summary>
		''' Indicates whether elements on this layer should be included in calculations (e.g., take-off summaries). 
		''' If false, elements on this layer will be ignored during calculation processes.
		''' </summary>
		''' <returns></returns>
		Public Property IncludeInCalculation As Boolean = True
		''' <summary>
		''' The color associated with the layer, represented as a hex string (e.g., "#FFFFFF" for white). This can be used for rendering elements on this layer with a specific color.</summary>
		Public Property Color As String = "#FFFFFF"

		''' <summary>
		''' Validates layer invariants.
		''' </summary>
		Public Sub Validate()
			If String.IsNullOrWhiteSpace(Name) Then
				Throw New ValidationException("Layer name cannot be empty")
			End If
		End Sub
	End Class
End Namespace