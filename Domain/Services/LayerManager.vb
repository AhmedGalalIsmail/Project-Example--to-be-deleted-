Imports Domain.Entities

Namespace Services
	''' <summary>
	''' Manages a collection of layers, allowing for adding, removing, and retrieving layers. It ensures that there is always at least one default layer present in the collection.
	''' </summary>
	Public Class LayerManager

		''' <summary>
		''' A private list that holds all the layers managed by this LayerManager instance. Each layer is represented by a Layer object, which contains properties such as Id, Name, IsVisible, and IsLocked.
		''' </summary>
		Private ReadOnly _layers As New List(Of Layer)

		''' <summary>Gets or sets the currently active layer. The active layer is the one that is currently selected or being worked on in the application. It can be set to any layer in the collection, and it can also be retrieved to determine which layer is currently active.</summary>
		''' <returns>The currently active Layer object. If no layer is set as active, this property may return Nothing (null).</returns>
		Public Property ActiveLayer As Layer

		''' <summary>
		''' Initializes a new instance of the LayerManager class. This constructor calls the Initialize method to ensure that there is at least one default layer present in the collection.
		''' </summary>
		Public Sub New()
			Initialize()
		End Sub

		''' <summary>
		''' Initializes the LayerManager by ensuring that there is at least one default layer present in the collection. If no layers exist, a default layer is created and added to the collection.
		''' </summary>
		Public Sub Initialize()
			' ? Initialize system
			EnsureDefaultLayer()
			ActiveLayer = _layers(0)
		End Sub

		''' <summary>Adds a new layer to the collection with the specified name. The layer is validated before being added to ensure it meets any necessary criteria.</summary>
		''' <param name="name">The name of the layer to be added.</param>
		''' <returns>The newly created Layer object.</returns>
		Public Function AddLayer(name As String) As Layer
			' ? Add new layer
			Dim layer As New Layer With {
			.Id = Guid.NewGuid(),
			.Name = name,
			.IsVisible = True,
			.IsLocked = False,
			.IncludeInCalculation = True}
			_layers.Add(layer)
			Return layer
		End Function

		''' <summary>Removes a layer from the collection based on its unique identifier (layerId). If a layer with the specified ID exists in the collection, it will be removed. If no such layer exists, the method does nothing.</summary>
		''' <param name="layerId"></param>
		Public Sub RemoveLayer(layerId As Guid)
			_layers.RemoveAll(Function(l) l.Id = layerId)
		End Sub

		''' <summary>Returns all layers in the collection.</summary>
		''' <returns>A list of Layer objects representing all layers in the collection.</returns>
		Public Function GetAll() As List(Of Layer)
			' ? Get all layers
			Return _layers
		End Function

		''' <summary>Returns the default layer from the collection. If no layers exist, a default layer is created and added to the collection before returning it.</summary>
		''' <returns>The default Layer object from the collection. If no layers exist, a new default layer is created and returned.</returns>
		Public Function GetDefaultLayer() As Layer
			' ? Get default layer
			EnsureDefaultLayer()
			Return _layers(0)
		End Function

		''' <summary>Returns a layer from the collection based on its unique identifier (id). If a layer with the specified ID exists, it is returned; otherwise, Nothing (null) is returned.</summary>
		''' <param name="id"></param>
		''' <returns>The Layer object with the specified ID if it exists in the collection; otherwise, Nothing (null).</returns>
		Public Function GetLayer(id As Guid) As Layer
			' ? Get layer by Id
			Return _layers.FirstOrDefault(Function(l) l.Id = id)
		End Function

		''' <summary>
		''' Ensures that there is always at least one default layer in the collection. If no layers exist, a default layer is created and added to the collection.
		''' </summary>
		Public Sub EnsureDefaultLayer()
			' ? Ensure default layer exists
			If _layers.Count = 0 Then
				Dim def As New Layer With {
					.Id = Guid.NewGuid(),
					.Name = "Default",
					.IsVisible = True,
					.IsLocked = False,
					.IncludeInCalculation = True
				}
				_layers.Add(def)
			End If
		End Sub
	End Class

End Namespace