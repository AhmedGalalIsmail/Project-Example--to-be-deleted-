Namespace Common
	''' <summary>
	''' Represents an exception that is thrown when a validation error occurs. This exception can be used to indicate that input data or parameters do not meet the required validation criteria.
	''' </summary>
	Public Class ValidationException
		Inherits Exception
		''' <summary>
		''' Initializes a new instance of the ValidationException class with a specified error message.
		''' </summary>
		''' <param name="message"></param>
		Public Sub New(message As String)
			MyBase.New(message)
		End Sub
	End Class
End Namespace