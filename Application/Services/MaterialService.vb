Option Strict On
Imports Application
Imports Domain.Entities
Imports Infrastructure.IO
Imports Infrastructure.IO.MaterialJsonStore
Imports System.Linq
Imports Domain.Entities.BlockModels


Namespace Services
    Public Class MaterialService

        Private ReadOnly _store As MaterialJsonStore

        Public Sub New(store As MaterialJsonStore)
            _store = store
        End Sub

        Public Function GetStore() As MaterialJsonStore
            Return _store
        End Function

        Public Function GetAll(_store As MaterialJsonStore) As List(Of Material)
            Return _store.LoadAll()
        End Function

        Public Sub AddOrUpdate(mat As Material)
            Dim all = _store.LoadAll()

            Dim existing = all.FirstOrDefault(Function(m) m.Code = mat.Code)
            If existing IsNot Nothing Then
                existing.Unit = mat.Unit
                existing.Density = mat.Density
                existing.PricePerUnit = mat.PricePerUnit
            Else
                all.Add(mat)
            End If

            _store.SaveAll(all)
        End Sub

        Public Sub Delete(code As String)
            Dim all = _store.LoadAll()
            all.RemoveAll(Function(m) m.Code = code)
            _store.SaveAll(all)
        End Sub

        Public Sub Save(m As Material)
            Throw New NotImplementedException()
        End Sub

        Public Function GetAll() As IEnumerable(Of Object)
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace