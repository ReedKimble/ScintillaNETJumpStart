Partial Public Class ScintillaManager(Of T As ParserToolKit.GrammarElement)
    Public Property TreatTextInQuotesAsWord As Boolean = True

    Public Class DocumentLine
        Public Property LineId As Integer
        Public Property Line As ScintillaNET.Line
        Public Property Grammars As IEnumerable(Of T)
        Public Property Tokens As IEnumerable(Of ParserToolKit.Lexer.Token)

        Public Function GrammarAtCurrentPosition(manager As ScintillaManager(Of T)) As T
            If Grammars Is Nothing Then Return Nothing
            Dim scintilla As ScintillaNET.Scintilla = manager.scintilla
            Dim pos = scintilla.CurrentPosition - 1
            Dim lineId = scintilla.LineFromPosition(pos)

            Dim word As String
            If manager.TreatTextInQuotesAsWord Then
                word = manager.GetStringElementFromPosition()
            Else
                word = scintilla.GetWordFromPosition(pos)
            End If

            For Each g In Grammars
                If g.Element.ToLower = word.ToLower Then Return g
            Next
            Return Nothing
        End Function
    End Class

    Public Function GetStringElementFromPosition() As String
        Dim result As New Text.StringBuilder
        result.Append(scintilla.GetWordFromPosition(scintilla.CurrentPosition - 1))
        Dim line = scintilla.Lines(scintilla.CurrentLine)
        Using reader As New ParserToolKit.CharacterReader(line.Text)
            If reader.CompareAt(reader.Length - 2, ControlChars.Quote) AndAlso Not reader.StartsWith(ControlChars.Quote) Then
                Dim curPos = scintilla.CurrentPosition - line.Position
                reader.SetPosition(curPos - 3)
                Dim idx = reader.SeekBackward(ControlChars.Quote)
                If idx > -1 Then
                    result.Insert(0, reader.GetSubString(idx, curPos - idx - result.Length - 1))
                End If
            ElseIf reader.StartsWith(ControlChars.Quote) AndAlso Not reader.CompareAt(reader.Length - 2, ControlChars.Quote) Then
                Dim curPos = scintilla.CurrentPosition - line.Position
                reader.SetPosition(curPos)
                Dim idx = reader.SeekForward(ControlChars.Quote)
                If idx > -1 Then
                    result.Append(reader.GetSubString(curPos, idx - curPos))
                End If
            End If
        End Using
        Return result.ToString
    End Function
End Class