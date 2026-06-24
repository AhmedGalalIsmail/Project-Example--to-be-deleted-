Option Strict On

Imports System.Drawing

Namespace Utilities
	''' <summary>
	''' Selection behavior used by the drawing canvas.
	''' </summary>
	Public Enum SelectionMode
		[Single]
		Multiple
		Window
		Crossing
	End Enum

	''' <summary>
	''' Helper rules for canvas selection gestures.
	''' </summary>
	Public NotInheritable Class SelectionRules
		Private Sub New()
		End Sub

		''' <summary>
		''' Resolves the selection mode from a drag gesture.
		''' Left-to-right gestures become window selection; right-to-left gestures become crossing selection.
		''' </summary>
		Public Shared Function ResolveMode(startPoint As PointF, endPoint As PointF) As SelectionMode
			Return If(endPoint.X >= startPoint.X, SelectionMode.Window, SelectionMode.Crossing)
		End Function

		''' <summary>
		''' Determines whether a target bounds rectangle should be selected by the drag bounds.
		''' </summary>
		Public Shared Function ShouldSelect(targetBounds As RectangleF, selectionBounds As RectangleF, mode As SelectionMode) As Boolean
			Select Case mode
				Case SelectionMode.Window
					Return selectionBounds.Contains(targetBounds)
				Case SelectionMode.Crossing
					Return selectionBounds.IntersectsWith(targetBounds)
				Case SelectionMode.[Single], SelectionMode.Multiple
					Return selectionBounds.Contains(targetBounds) OrElse selectionBounds.IntersectsWith(targetBounds)
				Case Else
					Return False
			End Select
		End Function
	End Class
End Namespace
