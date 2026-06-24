' src\CoNSoL.Domain\Entities\ElementRelationship.vb
' New: element relationship model + enum
Option Strict On

Namespace Entities
	''' <summary>
	''' Define Element Relationship Type
	''' </summary>
	Public Enum ElementRelationshipType
		Nested
		Exclusion
	End Enum

	''' <summary>
	''' Represents a parent/child relationship between elements.
	''' IDs are Guids for type safety and to ensure valid GUIDs.
	''' </summary>
	Public Class ElementRelationship
		''' <summary>
		''' The unique identifier of the parent element in the relationship. This property is of type Guid to ensure that it holds a valid GUID value, which uniquely identifies the parent element in the system.
		''' </summary>
		Public Property ParentElementId As Guid
		''' <summary>
		''' The unique identifier of the child element in the relationship. This property is of type Guid to ensure that it holds a valid GUID value, which uniquely identifies the child element in the system.
		''' </summary>
		Public Property ChildElementId As Guid
		''' <summary>
		''' The type of relationship between the parent and child elements. This property is of type ElementRelationshipType, which is an enumeration that defines the possible types of relationships (e.g., Nested, Exclusion). It indicates how the parent and child elements are related to each other within the system.
		''' </summary>
		Public Property RelationshipType As ElementRelationshipType
	End Class
End Namespace