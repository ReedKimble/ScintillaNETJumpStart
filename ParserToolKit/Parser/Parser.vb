
Namespace ParserToolKit
    Public Class Parser(Of T As GrammarElement)
        Public Event AnalyzeTokens As EventHandler(Of AnalyzeTokensEventArgs)

        Public Property Lexer As New Lexer

        Public Function Parse(text As String, ByRef tokens As IEnumerable(Of Lexer.Token)) As IEnumerable(Of T)
            Dim reader As New CharacterReader(text)
            tokens = Lexer.GetTokens(reader)
            Dim args As New AnalyzeTokensEventArgs(reader, tokens)
            OnAnalyzeTokens(args)
            Return args.Result
        End Function

        Protected Overridable Sub OnAnalyzeTokens(e As AnalyzeTokensEventArgs)
            RaiseEvent AnalyzeTokens(Me, e)
        End Sub
    End Class

    Public Class Parser
        Inherits Parser(Of GrammarElement)
    End Class

End Namespace
