Partial Public Class ScintillaManager(Of T As ParserToolKit.GrammarElement)
    Public Class PastingTextEventArgs
        Inherits EventArgs
        Public Property Text As String
        Protected Friend Sub New(text As String)
            Me.Text = text
        End Sub
    End Class

    Public Class ParseTokensIntoGrammarEventArgs
        Inherits ParserToolKit.Parser(Of T).AnalyzeTokensEventArgs
        Protected Friend Sub New(e As ParserToolKit.Parser(Of T).AnalyzeTokensEventArgs)
            MyBase.New(e.Reader, e.Tokens)
        End Sub
    End Class

    Public Class StyleNeededEventArgs
        Inherits EventArgs
        Public ReadOnly Grammar As T

        Public Property SelectedStyleID As Integer
        Protected Friend Sub New(grammar As T, styleID As Integer)
            Me.Grammar = grammar
            Me.SelectedStyleID = styleID
        End Sub
    End Class

    Public Class CallTipNeededEventArgs
        Inherits EventArgs
        Public ReadOnly Grammar As T
        Public ReadOnly Line As DocumentLine
        Public Property TipText As String
        Protected Friend Sub New(line As DocumentLine, grammar As T)
            Me.Grammar = grammar
            Me.Line = line
        End Sub
    End Class

    Public Class IntellisenseNeededEventArgs
        Inherits EventArgs
        Public ReadOnly Grammar As T
        Public ReadOnly Line As DocumentLine

        Public Property ListItems As IEnumerable(Of String)
        Public Property ItemsString As String

        Protected Friend Sub New(line As DocumentLine, grammar As T)
            Me.Grammar = grammar
            Me.Line = line
        End Sub
    End Class

End Class
