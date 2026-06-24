Imports System.IO

Public Class DrawingLoader
	''' <summary>
	''' Loads an image from the specified path and returns it as a byte array.</summary>
	''' <param name="path"></param>
	''' <returns>A byte array containing the image data. The caller can then pass this byte array to the OCR and shape detection services.</returns>
	Public Function LoadImage(path As String) As Byte()
		If Not File.Exists(path) Then
			Throw New Exception("File not found")
		End If
		Return File.ReadAllBytes(path)
	End Function

End Class