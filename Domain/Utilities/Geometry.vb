' src\ CoNSoL.Domain \ Utilities \ Geometry.vb
' New: simple Geometry helpers (domain). Implementations are conservative but compile-safe.
Option Strict On
Imports System.Text.Json
'Imports System.Linq

Namespace Utilities ' CoNSoL.Domain.Entities
	''' <summary>
	''' Minimal geometry helpers used by TakeOffCalculator.
	''' Implement more exact geometry parsing if/when needed.
	''' </summary>
	Public Class Geometry
		Public Shared Function Length(json As String) As Decimal
			If String.IsNullOrWhiteSpace(json) Then Return 0D
			Try
				' Expecting JSON array of points: [{"x":0,"y":0},{"x":3,"y":4}]
				Dim doc = JsonDocument.Parse(json)
				Dim root = doc.RootElement
				If root.ValueKind = JsonValueKind.Array Then
					' Calculate total length by summing distances between consecutive points
					Dim pts = root.EnumerateArray().
						Select(Function(p) New With {Key .x = p.GetProperty("x").GetDouble(), Key .y = p.GetProperty("y").GetDouble()}).
						ToArray()
					Dim total As Double = 0
					' Use Pythagorean theorem to calculate distance between points
					For i = 0 To pts.Length - 2
						Dim dx = pts(i + 1).x - pts(i).x
						Dim dy = pts(i + 1).y - pts(i).y
						total += Math.Sqrt(dx * dx + dy * dy)
					Next
					Return CDec(total)
				End If
			Catch
				' swallow parsing errors — return 0
			End Try

			Return 0D
		End Function

		''' <summary>
		''' implement polygon area parsing if needed.
		''' </summary>
		''' <param name="json"></param>
		''' <returns></returns>
		Public Shared Function Area(json As String) As Decimal
			' Placeholder: implement polygon area parsing if needed.
			Return 0D
		End Function

		''' <summary>
		'''  
		''' </summary>
		''' <param name="json"></param>
		''' <param name="parameters"></param>
		''' <returns>  </returns>
		Public Shared Function Volume(json As String, parameters As Dictionary(Of String, Object)) As Decimal
			' Placeholder: implement if D3 is used in your domain.
			Return 0D
		End Function
	End Class
End Namespace
