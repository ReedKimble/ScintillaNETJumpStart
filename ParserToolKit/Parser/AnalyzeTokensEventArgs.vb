Namespace ParserToolKit
    Partial Public Class Parser(Of T As GrammarElement)
        Public Class AnalyzeTokensEventArgs
            Inherits EventArgs
            Public ReadOnly Reader As CharacterReader
            Public ReadOnly Tokens As IEnumerable(Of Lexer.Token)
            Public Property Result As IEnumerable(Of T)
            Protected Friend Sub New(_reader As CharacterReader, _tokens As IEnumerable(Of Lexer.Token))
                Reader = _reader
                Tokens = _tokens
            End Sub
        End Class
    End Class
End Namespace