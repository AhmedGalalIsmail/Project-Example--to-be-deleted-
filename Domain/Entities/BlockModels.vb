' src\ CoNSoL.Domain \ Entities \ BlockModels.vb
' New: simple block / material / formula stubs (domain)
Option Strict On

Namespace Entities 'BlockModels
	''' <summary>
	''' Represents the core domain models for block definitions, materials, and formulas.
	''' </summary>
	Public Class BlockModels
		''' <summary>
		''' Block Component Info - represents a single component within a block, with quantity and optional parameters.
		''' </summary>
		Public Class Block
			''' <summary>
			''' Block Component Info - represents a single component within a block, with quantity and optional parameters.
			''' </summary>
			Public Property Code As String
			''' <summary>
			''' Description of the block, e.g., "Concrete Block", "Steel Beam", etc.
			''' </summary>
			Public Property Description As String
			''' <summary>
			''' Dimension mode of the block, e.g., D0, D1, D2, D3, indicating how dimensions are interpreted.
			''' </summary>
			Public Property DimensionMode As String ' D0/D1/D2/D3
			''' <summary>
			''' List of components that make up this block, each with its own code, quantity, and optional parameters.
			''' </summary>
			Public Property Components As New List(Of BlockComponent)
		End Class

		''' <summary>
		''' Material Info - represents a material with code, unit, density, and price information.
		''' </summary>
		Public Class Material
			Public Property Code As String          ' e.g. CEMENT_BAG
			Public Property Unit As String          ' bag, piece, item
			Public Property Density As String       ' optional / descriptive for now
			Public Property PricePerUnit As Decimal ' cost per unit
		End Class

		''' <summary>
		''' Formula Info - represents a calculation formula with a code and expression.
		''' </summary>
		Public Class Formula
			Public Property Code As String
			Public Property Expression As String
		End Class
	End Class
End Namespace