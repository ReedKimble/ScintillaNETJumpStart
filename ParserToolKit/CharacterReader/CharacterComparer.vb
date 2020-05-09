Namespace ParserToolKit
    Partial Public Class CharacterReader
        Public Class CharacterComparer
            Public Property CompareTo As Char = ControlChars.NullChar
            Public Property CompareBy As Func(Of Char, Boolean)

            Public Sub New(comparison As Char)
                CompareTo = comparison
                CompareBy = Nothing
            End Sub

            Public Sub New(comparison As Func(Of Char, Boolean))
                CompareBy = comparison
            End Sub

            Public Function Compare(value As Char) As Boolean
                If CompareBy IsNot Nothing Then Return CompareBy(value)
                Return Char.Equals(value, CompareTo)
            End Function

            Shared Widening Operator CType(value As Char) As CharacterComparer
                Return New CharacterComparer(value)
            End Operator

            Shared Widening Operator CType(value As Func(Of Char, Boolean)) As CharacterComparer
                Return New CharacterComparer(value)
            End Operator

            Shared Widening Operator CType(value As [Delegate]) As CharacterComparer
                If TypeOf value Is Func(Of Char, Boolean) Then
                    Return New CharacterComparer(DirectCast(value, Func(Of Char, Boolean)))
                End If
                Throw New Exception(CharacterReaderErrorMessages.DelegateMismatch)
            End Operator
        End Class

        Private NotInheritable Class CharacterReaderErrorMessages
            Friend Const DelegateMismatch As String = "Delegate type does not match Func(Of Char, Boolean)"
        End Class
    End Class
End Namespace