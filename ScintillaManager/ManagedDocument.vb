Partial Public Class ScintillaManager(Of T As ParserToolKit.GrammarElement)
    Public Class ManagedDocument
        Inherits ObjectModel.KeyedCollection(Of Integer, DocumentLine)
        Public Event LastStyledLineChanged As EventHandler

        Public ReadOnly Property LastStyledLine As DocumentLine

        Protected Overrides Function GetKeyForItem(item As DocumentLine) As Integer
            Return item.LineId
        End Function

        Protected Overridable Sub OnLastStyledLineChanged(e As EventArgs)
            RaiseEvent LastStyledLineChanged(Me, e)
        End Sub

        Protected Friend Sub SetLastStyled(line As DocumentLine)
            If Not _LastStyledLine Is line Then
                _LastStyledLine = line
                OnLastStyledLineChanged(EventArgs.Empty)
            End If
        End Sub
    End Class
End Class
