Imports System.IO
Imports System.Text

Public Class ExcelExporter
	''' <summary>
	''' Exports the takeoff results to a simple CSV file.
	''' </summary>
	''' <param name="result">
	''' The takeoff results to export, containing a dictionary of block names and their corresponding values.
	''' </param>
	''' <param name="path">
	''' The file path where the export file will be saved.
	''' </param>
	Public Shared Sub Export(result As TakeOffResult, path As String)
		Dim builder As New StringBuilder()
		builder.AppendLine("Block,Value")

		For Each kv In result.Results
			builder.AppendLine($"{EscapeCsv(kv.Key)},{kv.Value}")
		Next

		File.WriteAllText(path, builder.ToString(), Encoding.UTF8)
	End Sub

	Private Shared Function EscapeCsv(value As String) As String
		If value Is Nothing Then Return ""
		If value.Contains("""") Then
			value = value.Replace("""", """""")
		End If
		If value.Contains(",") OrElse value.Contains(vbCr) OrElse value.Contains(vbLf) OrElse value.Contains("""") Then
			Return $"""{value}"""
		End If
		Return value
	End Function
End Class
