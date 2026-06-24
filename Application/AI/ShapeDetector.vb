Imports System.Text.Json
Imports Domain.Entities

''' <summary>
''' ShapeDetector is responsible for analyzing the drawing (image) and detecting shapes, dimensions, and geometry information.
''' </summary>
Public Class ShapeDetector
	''' <summary>
	''' Detect shapes from the drawing, using OCR and geometry analysis.
	''' </summary>
	''' <param name="imagePath"></param>
	''' <returns>A list of detected CanvasElements with geometry information filled in.</returns>
	Public Function DetectShapes(imagePath As String) As List(Of CanvasElement)
		Dim elements As New List(Of CanvasElement)
		' Offline-friendly placeholder: the OCR/geometry pipeline can run without OpenCvSharp.
		' For Wave 1 we keep the method shape-compatible and return an empty candidate set.
		Return elements
	End Function

End Class
