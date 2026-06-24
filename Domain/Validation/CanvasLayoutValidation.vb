Imports Domain.Entities
Imports Domain.Common
Imports System.Collections.Generic

Namespace Validation
	''' <summary>
	''' Validates a CanvasLayout object, ensuring that it and its elements are not null.
	''' </summary>
	Public Class CanvasLayoutValidation
		Private Shared ReadOnly ValidUnits As HashSet(Of String) =
			New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
				"mm", "cm", "m", "in", "ft",
				"meter", "centimeter", "inch", "foot"
			}

		''' <summary>
		''' Validates the provided CanvasLayout object. Throws a ValidationException if the layout or its elements are null.
		''' </summary>
		''' <param name="layout"></param>
		Public Shared Sub Validate(layout As CanvasLayout)
			If layout Is Nothing Then
				Throw New ValidationException("Layout cannot be null")
			End If

			If layout.Elements Is Nothing Then
				Throw New ValidationException("Elements collection cannot be null")
			End If

			If layout.ScaleFactor <= 0 Then
				Throw New ValidationException("ScaleFactor must be greater than zero")
			End If

			If String.IsNullOrWhiteSpace(layout.Unit) OrElse Not ValidUnits.Contains(layout.Unit) Then
				Throw New ValidationException($"Invalid unit: {layout.Unit}")
			End If

			For Each el In layout.Elements
				CanvasElementValidation.Validate(el)
			Next
		End Sub
	End Class
End Namespace
