Namespace ParserToolKit
    Public Class GrammarElement
        Public Shared Property WhitespaceKind As Integer = 0

        Public Overridable ReadOnly Property Element As String
        Public Overridable ReadOnly Property ElementKind As Integer
        Public ReadOnly Property ErrorState As String = String.Empty
        Public ReadOnly Property HasError As Boolean
            Get
                Return Not String.IsNullOrEmpty(ErrorState)
            End Get
        End Property
        Public ReadOnly Property Index As Integer
        Public ReadOnly Property Length As Integer
        Public ReadOnly Property Position As Integer
        Public ReadOnly Property TokenKind As Integer

        Public Sub New(grammarText As String, grammarKind As Integer, indexInLine As Integer, token As Lexer.Token)
            Element = grammarText
            ElementKind = grammarKind
            Position = token.Index
            Length = grammarText.Length
            TokenKind = token.Kind
            Index = indexInLine
        End Sub

        Public Overridable Sub ClearErrorState()
            _ErrorState = String.Empty
        End Sub

        Public Overridable Sub SetErrorState(message As String)
            _ErrorState = message
        End Sub

        Public Overrides Function ToString() As String
            If Not String.IsNullOrEmpty(ErrorState) Then Return ErrorState
            Return $"[Position:{Position}][Length:{Length}][TokenKind:{TokenKind}][Element:{Element}]"
        End Function
    End Class

End Namespace
