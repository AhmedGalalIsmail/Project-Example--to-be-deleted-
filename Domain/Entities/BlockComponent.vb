
Namespace Entities
	''' <summary>
	''' Represents a single component of a block, 
	''' linking to a material and specifying quantity per unit of block.
	''' </summary>
	Public Class BlockComponent
		''' <summary>
		''' The code of the material associated with this block component.
		''' </summary>
		Public Property MaterialCode As String

		''' <summary>
		''' The quantity of the material required per unit of the block.
		''' </summary>
		Public Property QuantityPerUnit As Decimal
	End Class
End Namespace