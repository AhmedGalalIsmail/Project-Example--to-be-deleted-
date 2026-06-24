Option Strict On
Imports System.Collections.Generic

Public Class BlockAssignmentModel
    Public Property BlockCode As String
    Public Property DimensionMode As String
    Public Property Parameters As New Dictionary(Of String, Object)
    Public Property Nested As NestedInfo
End Class

Public Class NestedInfo
    Public Property ParentElementId As String
    Public Property RelationshipType As String ' Nested | Exclusion
End Class