
''' <summary>Service for performing OCR (Optical Character Recognition) on drawing images to extract text information.
''' </summary>
Public Class OcrService
	''' <summary>
	''' Extract text from drawing image.
	''' This offline-friendly implementation returns a deterministic text sample
	''' so the rest of the AI pipeline can compile and run without native OCR packages.
	''' </summary>
	Public Function ExtractText(imagePath As String) As List(Of String)
		Return ExtractText(New Byte() {})
	End Function

	''' <summary>
	''' Extract text from drawing image bytes.
	''' </summary>
	''' <param name="imageBytes"></param>
	''' <returns>
	''' A list of detected text strings from the image. In a real implementation, this would be the output of an OCR engine.
	''' </returns>
	Public Function ExtractText(imageBytes As Byte()) As List(Of String)
		Dim detectedText As New List(Of String)
		detectedText.Add("SCALE 1:100")
		detectedText.Add("WALL 300mm")
		detectedText.Add("ROOM 5.0 x 4.0")
		Return detectedText
	End Function
End Class
