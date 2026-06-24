' src\ CoNSoL.Application \ TakeOffResult.vb
' New (Application): minimal TakeOffResult used by TakeOffCalculator

Option Strict On
Imports System.Collections.Generic


''' <summary>
''' Minimal TakeOffResult used by TakeOffCalculator
''' </summary>
Public Class TakeOffResult
    Private ReadOnly _results As New Dictionary(Of String, Decimal)()

    ''' <summary>
    ''' block Code and value 
    ''' </summary>
    ''' <param name="blockCode"></param>
    ''' <param name="value"></param>
    Public Sub Add(blockCode As String, value As Decimal)
        If _results.ContainsKey(blockCode) Then
            _results(blockCode) += value
        Else
            _results(blockCode) = value
        End If
    End Sub

    ''' <returns>Results</returns>
    Public ReadOnly Property Results As IReadOnlyDictionary(Of String, Decimal)
        Get
            Return _results
        End Get
    End Property
End Class