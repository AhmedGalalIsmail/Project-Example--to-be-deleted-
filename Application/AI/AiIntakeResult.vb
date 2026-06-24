Imports Domain.Entities

''' <summary>Represents the result of an AI intake process, containing detected text and scale information.</summary>
''' <returns>An AiIntakeResult object that encapsulates the detected text annotations, scale information, and any identified geometric elements from the architectural drawing. This result serves as the output of the AI intake process and can be used for further processing in the takeoff calculations and other downstream tasks in the construction workflow.</returns>
Public Class AiIntakeResult

#Region "Public Properties"
    ''' <summary>
    ''' Gets or sets the list of detected text strings from the AI intake process.</summary>
    Public Property DetectedText As List(Of String)
    ''' <summary>
    ''' Gets or sets the detected scale information from the AI intake process.</summary>
    Public Property DetectedScale As String
    ''' <summary>
    ''' Gets or sets the list of detected canvas elements from the AI intake process. This property holds the structured representation of elements identified in the input, which may include shapes, blocks, or other relevant entities.</summary>
    Public Property DetectedElements As List(Of CanvasElement) ' ? NEW
#End Region

End Class