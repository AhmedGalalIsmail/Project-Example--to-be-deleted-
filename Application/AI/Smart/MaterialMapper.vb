Imports System.Text.Json
Imports Domain.Entities

''' <summary>
''' MaterialMapper is responsible for assigning business definitions to detected elements based on their classification.</summary>
Public Class MaterialMapper
	''' <summary>
	''' Assign business definitions automatically
	''' </summary>
	Public Sub Apply(
		elements As List(Of CanvasElement),
		classification As Dictionary(Of Guid, String)
	)
		For Each el In elements
			Dim category = classification(el.Id)
			Dim def As New BusinessDefinition()
			Select Case category
				Case "Concrete"
					def.BlockCode = "CONCRETE"
					def.DimensionMode = "D2"
				Case "Plumbing"
					def.BlockCode = "PIPE"
					def.DimensionMode = "D1"
				Case "Electrical"
					def.BlockCode = "CABLE"
					def.DimensionMode = "D1"
				Case Else
					Continue For
			End Select
			el.BusinessJson = JsonSerializer.Serialize(def)
		Next
	End Sub
End Class