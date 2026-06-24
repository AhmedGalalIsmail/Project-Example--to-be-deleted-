Imports Domain.Entities

''' <summary>
''' LayerAutoBuilder is responsible for automatically creating layers in the canvas layout based on a classification of elements (e.g., by type, category, or other criteria). It takes a CanvasLayout and a classification dictionary mapping element IDs to layer names, and produces a list of Layer objects with elements assigned to the appropriate layers.</summary>
Public Class LayerAutoBuilder
	''' <summary>Build layers based on the provided classification. For each unique category in the classification, a new layer is created, and elements are assigned to their respective layers based on their classification. This allows for organized grouping of elements in the canvas layout, which can be useful for visualization and further processing in the takeoff calculations. In this implementation, it serves as a placeholder for a more complex layer-building logic that would be developed in future iterations of the AI pipeline.</summary>
	''' <param name="layout"></param>
	''' <param name="classification"></param>
	''' <returns>A list of Layer objects that have been created based on the classification of elements. Each Layer contains a name corresponding to the category and has its elements assigned according to the classification dictionary. This allows for organized grouping of elements in the canvas layout, which can be useful for visualization and further processing in the takeoff calculations.</returns>
	Public Function BuildLayers(
		layout As CanvasLayout,
		classification As Dictionary(Of Guid, String)
	) As List(Of Layer)
		Dim layers As New List(Of Layer)
		Dim categories = classification.Values.Distinct()
		For Each cat In categories
			Dim layer As New Layer With {
				.Name = cat
			}
			layers.Add(layer)
			' Assign elements
			For Each el In layout.Elements
				If classification(el.Id) = cat Then
					el.LayerId = layer.Id ' ? IMPORTANT
				End If
			Next
		Next
		Return layers
	End Function
End Class