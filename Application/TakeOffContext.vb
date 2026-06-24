' src\CoNSoL.Application\TakeOffContext.vb
' Modify TakeOffContext — place in Application namespace and import domain entities (Block, Material, Formula stubs are in domain)

Option Strict On
Imports Domain.Entities.BlockModels

'Namespace CoNSoL.Application


''' <summary>
''' Context class for TakeOff calculations, providing necessary data for block definitions, materials, formulas, and pricing.
''' </summary>
Public Class TakeOffContext

    ''' <summary>
    ''' Dictionaries for quick lookup of blocks, materials, formulas, and prices by their codes.
    ''' </summary>
    ''' <returns></returns>
    Public Property Blocks As IReadOnlyDictionary(Of String, Block)

    ''' <summary>
    ''' Materials with their unit costs, used for cost calculations in formulas. Keyed by material code.
    ''' </summary>
    ''' <returns></returns>
    Public Property Materials As IReadOnlyDictionary(Of String, Material)

    ''' <summary>
    ''' Formulas with their expressions, used to calculate breakdowns from quantities. Keyed by formula code.
    ''' </summary>
    ''' <returns></returns>
    Public Property Formulas As IReadOnlyDictionary(Of String, Formula)

    ''' <summary>
    ''' Optional: Pricing information for blocks or materials, if needed for cost calculations. Keyed by code (block or material).
    ''' </summary>
    ''' <returns></returns>
    Public Property Prices As IReadOnlyDictionary(Of String, Decimal)
End Class
'End Namespace