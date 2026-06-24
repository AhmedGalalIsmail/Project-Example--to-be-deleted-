'Filename: Desktop/Program.vb
Option Strict On
Imports System.Threading
Imports System.Windows

' Namespace CoNSoL.Desktop.Program
Module Program
	Sub Main()
		AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnUnhandledException

		CompositionRoot.Bootstrap()
		Dim app As New System.Windows.Application()
		AddHandler app.DispatcherUnhandledException, AddressOf OnDispatcherUnhandledException
		app.Run(New MainWPFform())
	End Sub

	Private Sub OnDispatcherUnhandledException(sender As Object, e As System.Windows.Threading.DispatcherUnhandledExceptionEventArgs)
		If CompositionRoot.Logger IsNot Nothing Then
			CompositionRoot.Logger.Error("UI exception", e.Exception)
		End If
		System.Windows.MessageBox.Show("An unexpected error occurred. Details were logged.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
		e.Handled = True
	End Sub

	' v1.1 Original code had a bug where e.ExceptionObject was not cast to Exception, causing a crash when logging. Fixed by adding TryCast and null check.
	'Private Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
	'    Dim ex = TryCast(e.ExceptionObject, Exception)
	'    ' CompositionRoot.Logger.Error("Unhandled exception", ex)
	'End Sub

	' V1.2 Added null check for logger to prevent potential NullReferenceException if logger is not initialized.
	Private Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
		Dim ex = TryCast(e.ExceptionObject, Exception)
		If CompositionRoot.Logger IsNot Nothing Then
			CompositionRoot.Logger.Error("Unhandled exception", ex)
		End If
	End Sub


	Public NotInheritable Class Guard
		Public Shared Function NotNull(Of T)(value As T, name As String) As T
			If value Is Nothing Then
				Throw New ArgumentNullException(name)
			End If
			Return value
		End Function
	End Class
End Module
'End Namespace
