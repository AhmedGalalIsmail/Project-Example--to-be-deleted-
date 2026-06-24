Option Strict On

Imports Domain.Entities

Namespace Entities
	''' <summary>
	''' Represents a single element drawn on the canvas, 
	''' including its geometry, business data, and relationships.
	''' </summary>
	Public Class CanvasElement
		''' <summary>
		''' The ID of the parent element, if this element is a child in a relationship.
		''' </summary>
		Public Property ParentElementId As String
		''' <summary>
		''' The unique identifier for this canvas element. Initialized to a new GUID by default.
		''' </summary>
		Public Property Id As Guid = Guid.NewGuid()
		''' <summary>
		''' The type of the canvas element, such as "Rectangle", "Line", etc.
		''' </summary>
		Public Property Type As String
		''' <summary>
		''' The layer name or identifier where this element is located on the canvas.
		''' </summary>
		Public Property Layer As String
		''' <summary>
		''' The JSON representation of the geometry of the element, which can be deserialized into a specific geometry object.
		''' </summary>
		Public Property GeometryJson As String
		''' <summary>
		''' The JSON representation of the business definition associated with this element, which can be deserialized into a BusinessDefinition object.
		''' </summary>
		Public Property BusinessJson As String
		''' <summary>
		''' The type of relationship this element has with its parent element, if applicable. This can be used to define hierarchical or associative relationships between elements.
		''' </summary>
		Public Property RelationshipType As ElementRelationshipType
		''' <summary>
		''' The ID of the child element, if this element has a child in a relationship. This can be used to establish a link to another CanvasElement that is considered a child of this element.
		''' </summary>
		Public Property ChildElementId As String
		''' <summary>
		''' The unique identifier for the layer this element belongs to. This can be used to group elements together on the same layer, allowing for easier management and manipulation of related elements.
		''' </summary>
		Public Property LayerId As Guid = Guid.NewGuid()
	End Class
End Namespace