Imports Domain.Entities
Imports Domain.Common

Namespace Validation
	''' <summary>
	''' Provides validation logic for CanvasElement instances, ensuring they meet required criteria for ID, type, and geometry.
	''' </summary>
	Public Class CanvasElementValidation
		''' <summary>
		''' Defines the valid types of CanvasElement that can be processed.
		''' </summary>
		Private Shared ReadOnly ValidTypes As String() =
			{"Line", "Rectangle", "Circle", "Ellipse"}

		''' <summary>
		''' Validates a CanvasElement instance, ensuring it has a valid ID, type, and geometry.
		''' </summary>
		''' <param name="element"></param>
		Public Shared Sub Validate(element As CanvasElement)
			If element.Id = Guid.Empty Then
				Throw New ValidationException("Element ID invalid")
			End If

			If Not ValidTypes.Contains(element.Type) Then
				Throw New ValidationException($"Invalid element type: {element.Type}")
			End If

			If String.IsNullOrWhiteSpace(element.GeometryJson) Then
				Throw New ValidationException("Geometry cannot be empty")
			End If
		End Sub
	End Class
End Namespace