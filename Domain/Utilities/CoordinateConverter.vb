Option Strict On

Imports System.Drawing

Namespace Utilities
	''' <summary>
	''' Converts between physical canvas units and logical model units.
	''' </summary>
	Public NotInheritable Class CoordinateConverter
		Private Sub New()
		End Sub

		''' <summary>
		''' Converts a physical scalar to logical units.
		''' </summary>
		Public Shared Function ToLogical(physical As Single, scaleFactor As Single) As Single
			ValidateScaleFactor(scaleFactor)
			Return physical / scaleFactor
		End Function

		''' <summary>
		''' Converts a logical scalar to physical units.
		''' </summary>
		Public Shared Function ToPhysical(logical As Single, scaleFactor As Single) As Single
			ValidateScaleFactor(scaleFactor)
			Return logical * scaleFactor
		End Function

		''' <summary>
		''' Converts a physical point to logical coordinates.
		''' </summary>
		Public Shared Function ToLogical(physical As PointF, scaleFactor As Single) As PointF
			Return New PointF(
				ToLogical(physical.X, scaleFactor),
				ToLogical(physical.Y, scaleFactor))
		End Function

		''' <summary>
		''' Converts a logical point to physical coordinates.
		''' </summary>
		Public Shared Function ToPhysical(logical As PointF, scaleFactor As Single) As PointF
			Return New PointF(
				ToPhysical(logical.X, scaleFactor),
				ToPhysical(logical.Y, scaleFactor))
		End Function

		Private Shared Sub ValidateScaleFactor(scaleFactor As Single)
			If scaleFactor <= 0 Then
				Throw New ArgumentOutOfRangeException(NameOf(scaleFactor), "Scale factor must be greater than zero.")
			End If
		End Sub
	End Class
End Namespace
