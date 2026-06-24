'src\ CoNSoL.Domain \ Entities \ BusinessDefinition.vb
'New: domain business types required by other projects
Option Strict On
Imports System.Collections.Generic
Imports Domain.Entities

Namespace Entities
	'Namespace BusinessDefinition
	''' <summary>
	''' Minimal business definition used when shapes have block assignments.
	''' Kept intentionally small — expand as needed.
	''' </summary>
	Public Class BusinessDefinition
		''' <summary>
		''' The block code assigned to this element, e.g., "CONCRETE", "PIPE", etc.
		''' </summary>
		Public Property BlockCode As String
		''' <summary>
		''' The dimension mode for this element, e.g., "D1" for 1D elements, "D2" for 2D elements.
		''' </summary>
		Public Property DimensionMode As String
		''' <summary>
		''' Additional parameters for this business definition, stored as key-value pairs.
		''' </summary>
		Public Property Parameters As New Dictionary(Of String, Object)()
	End Class
End Namespace