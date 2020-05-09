Namespace ParserToolKit
    Partial Public Class Lexer
        Public Structure Token
            Public Index As Integer
            Public Length As Integer
            Public Kind As Integer
            Public Sub New(i As Integer, l As Integer, k As Integer)
                Index = i
                Length = l
                Kind = k
            End Sub

            Public Function ReadFrom(reader As CharacterReader) As String
                Return reader.GetSubString(Index, Length)
            End Function

            Public Overrides Function ToString() As String
                Return $"Index: {Index}, Length: {Length}, Kind: {[Enum].GetName(GetType(DefaultTokenKind), Kind)}"
            End Function
        End Structure
    End Class
End Namespace
