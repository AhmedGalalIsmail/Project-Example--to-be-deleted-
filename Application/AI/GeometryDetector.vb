Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports Domain.Entities

''' <summary>
''' GeometryDetector is responsible for analyzing the OCR text and detecting basic geometric shapes and dimensions, which can be used to create CanvasElements with geometry information for takeoff calculations.</summary>
Public Class GeometryDetector
	''' <summary>
	''' Detect basic shapes from OCR text</summary>
	Public Function Detect(textLines As List(Of String)) As List(Of CanvasElement)
		Dim elements As New List(Of CanvasElement)
		For Each line In textLines
			' ? Detect rectangle: "5.0 x 4.0"
			Dim rectMatch = Regex.Match(line, "(\d+(\.\d+)?)\s*x\s*(\d+(\.\d+)?)")
			' check if we have a rectangle match
			If rectMatch.Success Then
				Dim width = Decimal.Parse(rectMatch.Groups(1).Value)
				Dim height = Decimal.Parse(rectMatch.Groups(3).Value)
				Dim geom = New With {
					.type = "Rectangle",
					.width = width,
					.height = height
				}
				' create a new CanvasElement for the rectangle
				elements.Add(New CanvasElement With {
					.Id = Guid.NewGuid(),
					.Type = "Rectangle",
					.GeometryJson = JsonSerializer.Serialize(geom)})
				Continue For
			End If
			' ? Detect line: "10m", "12.5 m"
			Dim lineMatch = Regex.Match(line, "(\d+(\.\d+)?)\s*m")
			' check if we have a line match
			If lineMatch.Success Then
				Dim length = Decimal.Parse(lineMatch.Groups(1).Value)
				Dim geom = New With {
					.type = "Line",
					.start = New With {.x = 0, .y = 0},
					.end = New With {.x = length, .y = 0}
				}
				' create a new CanvasElement for the line
				elements.Add(New CanvasElement With {
					.Id = Guid.NewGuid(),
					.Type = "Line",
					.GeometryJson = JsonSerializer.Serialize(geom)
				})
			End If
		Next
		Return elements
	End Function
End Class