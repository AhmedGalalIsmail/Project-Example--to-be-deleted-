Option Strict On
'Imports CoNSoL.Domain.Entities
Imports Domain.Entities.BlockModels
Imports Domain.Entities

Namespace Services
    ''' <summary>
    ''' Application service for TakeOff layout calculations and operations.
    ''' </summary>
    Public Class TakeOffService
        ''' <summary>
        ''' Compute the net area from a canvas layout (gross area minus exclusions).
        ''' TODO: implement D2 area calculation minus exclusions.
        ''' </summary>
        ''' <param name="layout">The canvas layout to calculate from.</param>
        ''' <returns>The computed net area in square units.</returns>
        Public Function NetArea(layout As CanvasLayout) As Double
            ' TODO: implement D2 area minus exclusions
            Return 0.0
        End Function

        ''' <summary>
        ''' Add a block definition to the service's context, ensuring no circular 
        ''' references in the block components.
        ''' </summary>
        ''' <param name="block"></param>
        Public Sub AddBlock(block As Block)
            ' validate no circular references
        End Sub

        ''' <summary>
        ''' Calculate the total materials required for a given block and quantity, 
        ''' by applying the block's components and formulas recursively.
        ''' </summary>
        ''' <param name="block"></param>
        ''' <param name="quantity"></param>
        ''' <returns></returns>
        Public Function CalculateMaterials(
        block As Block,
        quantity As Decimal
    ) As Dictionary(Of String, Decimal)
            ' MaterialCode → TotalQuantity
            Return New Dictionary(Of String, Decimal)()
        End Function

    End Class
End Namespace