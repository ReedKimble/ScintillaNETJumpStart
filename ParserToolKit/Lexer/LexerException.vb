Namespace ParserToolKit
    Partial Public Class Lexer
        ''' <summary>
        ''' This exception occurs when an error is encountered during lexing of a text string.
        ''' </summary>
        Public Class LexerException
            Inherits Exception
            ''' <summary>
            ''' The raw text being lexed.
            ''' </summary>
            Public ReadOnly RawText As String
            ''' <summary>
            ''' The position within the string at which the error occured.
            ''' </summary>
            Public ReadOnly Index As Integer
            ''' <summary>
            ''' The type of the last token successfully discovered.
            ''' </summary>
            Public ReadOnly LastGoodTokenKind As Integer
            Protected Friend Sub New(message As String, text As String, idx As Integer, k As Integer)
                Me.New(message, text, idx, k, Nothing)
            End Sub
            Protected Friend Sub New(message As String, text As String, idx As Integer, k As Integer, inner As Exception)
                MyBase.New(message, inner)
                RawText = text
                Index = idx
                LastGoodTokenKind = k
            End Sub
        End Class
    End Class
End Namespace
