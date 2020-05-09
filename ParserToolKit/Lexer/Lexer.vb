Imports ScintillaNETJumpStart.ParserToolKit.CharacterReader

Namespace ParserToolKit
    Public Class Lexer
        Const HexChars As String = "ABCDEFabcedf"

        Public Property AllowUnknownTokens As Boolean = True
        Public Property AllowZeroLengthTokens As Boolean = True
        Public Property AssignmentOperatorCharacters As IEnumerable(Of Char) = {"="c}
        Public Property ComparisonOperatorCharacters As IEnumerable(Of Char) = {"="c, "="c}
        Public Property FindTokenKindCheckOrder As IEnumerable(Of CheckKind) = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}

        Public Property OpenParenthesisCharacter As Char = "("c
        Public Property OpenBracketCharacter As Char = "["c
        Public Property OpenBraceCharacter As Char = "{"c
        Public Property CloseParenthesisCharacter As Char = ")"c
        Public Property CloseBracketCharacter As Char = "]"c
        Public Property CloseBraceCharacter As Char = "}"c

        Public Enum CheckKind
            None
            Whitespace
            AssignmentOperator
            ComparisonOperator
            Period
            Punctuation
            MathSymbol
            BalancedPairCharacter
            [Boolean]
            TextLiteral
            Number
            Identifier
        End Enum

        Protected Overridable Function FindTokenKind(text As CharacterReader, ByRef index As Integer, ByRef length As Integer) As Integer
            Dim tmpKind As Integer
            text.Seek(index)

            For Each i In FindTokenKindCheckOrder
                Select Case i
                    Case CheckKind.Whitespace
                        If OnCheckWhiteSpace(text, index, length) Then Return DefaultTokenKind.WhiteSpace
                    Case CheckKind.AssignmentOperator
                        If OnCheckAssignmentOperator(text, index, length) Then Return DefaultTokenKind.AssignmentOperator
                    Case CheckKind.ComparisonOperator
                        If OnCheckComparisonOperator(text, index, length) Then Return DefaultTokenKind.ComparisonOperator
                    Case CheckKind.Period
                        If OnCheckPeriod(text, index, length) Then Return DefaultTokenKind.Period
                    Case CheckKind.Punctuation
                        If OnCheckPunctuation(text, index, length) Then Return DefaultTokenKind.Punctuation
                    Case CheckKind.MathSymbol
                        If OnCheckMathSymbol(text, index, length) Then Return DefaultTokenKind.MathSymbol
                    Case CheckKind.BalancedPairCharacter
                        If OnCheckBalancedPairCharacter(text, index, length, tmpKind) Then Return tmpKind
                    Case CheckKind.Boolean
                        If OnCheckBoolean(text, index, length) Then Return DefaultTokenKind.Boolean
                    Case CheckKind.TextLiteral
                        If OnCheckTextLiteral(text, index, length) Then Return DefaultTokenKind.TextLiteral
                    Case CheckKind.Number
                        If OnCheckNumber(text, index, length, tmpKind) Then Return tmpKind
                    Case CheckKind.Identifier
                        If OnCheckIdentifier(text, index, length) Then Return DefaultTokenKind.Identifier
                End Select
            Next

            Return DefaultTokenKind.Unknown
        End Function

        Public Delegate Function LexerCheck(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef subkind As Integer) As Boolean
        Public Property CheckIdentifier As LexerCheck = Function(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef subkind As Integer)
                                                            text.SeekWhiteSpace()
                                                            length = text.Position - index
                                                            Return length > 0
                                                        End Function

        Protected Overridable Function OnCheckIdentifier(text As CharacterReader, ByRef index As Integer, ByRef length As Integer) As Boolean
            Return CheckIdentifier(text, index, length, Nothing)
        End Function

        Public Property CheckAssignmentOperator As LexerCheck = Function(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef subkind As Integer)
                                                                    If text.CompareRange(AssignmentOperatorCharacters, ComparisonAdvancementMode.IfTrue) Then
                                                                        length = AssignmentOperatorCharacters.Count
                                                                        Return True
                                                                    End If
                                                                    Return False
                                                                End Function
        Protected Overridable Function OnCheckAssignmentOperator(text As CharacterReader, ByRef index As Integer, ByRef length As Integer) As Boolean
            Return CheckAssignmentOperator(text, index, length, Nothing)
        End Function

        Public Property CheckComparisonOperator As LexerCheck = Function(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef subkind As Integer)
                                                                    If text.CompareRange(ComparisonOperatorCharacters, ComparisonAdvancementMode.IfTrue) Then
                                                                        length = ComparisonOperatorCharacters.Count
                                                                        Return True
                                                                    End If
                                                                    Return False
                                                                End Function
        Protected Overridable Function OnCheckComparisonOperator(text As CharacterReader, ByRef index As Integer, ByRef length As Integer) As Boolean
            Return CheckComparisonOperator(text, index, length, Nothing)
        End Function

        Public Property CheckBalancedPairCharacter As LexerCheck = Function(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef subKind As Integer)
                                                                       If text.CompareCurrent(OpenParenthesisCharacter, ComparisonAdvancementMode.IfTrue) Then
                                                                           subKind = DefaultTokenKind.OpenParenthesis
                                                                           Return True
                                                                       End If
                                                                       If text.CompareCurrent(OpenBracketCharacter, ComparisonAdvancementMode.IfTrue) Then
                                                                           subKind = DefaultTokenKind.OpenBracket
                                                                           Return True
                                                                       End If
                                                                       If text.CompareCurrent(OpenBraceCharacter, ComparisonAdvancementMode.IfTrue) Then
                                                                           subKind = DefaultTokenKind.OpenBrace
                                                                           Return True
                                                                       End If
                                                                       If text.CompareCurrent(CloseParenthesisCharacter, ComparisonAdvancementMode.IfTrue) Then
                                                                           subKind = DefaultTokenKind.CloseParenthesis
                                                                           Return True
                                                                       End If
                                                                       If text.CompareCurrent(CloseBracketCharacter, ComparisonAdvancementMode.IfTrue) Then
                                                                           subKind = DefaultTokenKind.CloseBracket
                                                                           Return True
                                                                       End If
                                                                       If text.CompareCurrent(CloseBraceCharacter, ComparisonAdvancementMode.IfTrue) Then
                                                                           subKind = DefaultTokenKind.CloseBrace
                                                                           Return True
                                                                       End If
                                                                       Return False
                                                                   End Function
        Protected Overridable Function OnCheckBalancedPairCharacter(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef bpKind As Integer) As Boolean
            Return CheckBalancedPairCharacter(text, index, length, bpKind)
        End Function

        Public Property CheckMathSymbol As LexerCheck = Function(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef subKind As Integer)
                                                            If text.CompareCurrent(Function(c As Char)
                                                                                       Return Char.GetUnicodeCategory(c) = Globalization.UnicodeCategory.MathSymbol
                                                                                   End Function, ComparisonAdvancementMode.IfTrue) Then
                                                                length = 1
                                                                Return True
                                                            End If
                                                            Return False
                                                        End Function
        Protected Overridable Function OnCheckMathSymbol(text As CharacterReader, ByRef index As Integer, ByRef length As Integer) As Boolean
            Return CheckMathSymbol(text, index, length, Nothing)
        End Function

        Public Property CheckPeriod As LexerCheck = Function(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef subKind As Integer)
                                                        If text.CompareCurrent("."c, ComparisonAdvancementMode.IfTrue) Then
                                                            length = 1
                                                            Return True
                                                        End If
                                                        Return False
                                                    End Function
        Protected Overridable Function OnCheckPeriod(text As CharacterReader, ByRef index As Integer, ByRef length As Integer) As Boolean
            Return CheckPeriod(text, index, length, Nothing)
        End Function

        Public Property CheckPunctuation As LexerCheck = Function(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef subKind As Integer)
                                                             If text.CompareCurrent(AddressOf Char.IsPunctuation, ComparisonAdvancementMode.IfTrue) Then
                                                                 length = 1
                                                                 Return True
                                                             End If
                                                             Return False
                                                         End Function
        Protected Overridable Function OnCheckPunctuation(text As CharacterReader, ByRef index As Integer, ByRef length As Integer) As Boolean
            Return CheckPunctuation(text, index, length, Nothing)
        End Function

        Public Property CheckBoolean As LexerCheck = Function(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef subKind As Integer)
                                                         If Boolean.TryParse(text, Nothing) Then
                                                             length = text.Length
                                                             Return True
                                                         End If
                                                         Return False
                                                     End Function
        Protected Overridable Function OnCheckBoolean(text As CharacterReader, ByRef index As Integer, ByRef length As Integer) As Boolean
            Return CheckBoolean(text, index, length, Nothing)
        End Function

        Public Property CheckTextLiteral As LexerCheck = Function(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef subKind As Integer)
                                                             If text.CompareCurrent(ControlChars.Quote, ComparisonAdvancementMode.IfTrue) Then
                                                                 length += 1
                                                                 If text.EndOfString Then Return True
                                                                 Dim closed As Boolean = False
                                                                 Do
                                                                     If text.CompareCurrent(ControlChars.Quote, ComparisonAdvancementMode.Always) Then
                                                                         closed = True
                                                                         Exit Do
                                                                     End If
                                                                     length += 1
                                                                     If text.EndOfString Then Exit Do
                                                                 Loop
                                                                 If closed Then length += 1
                                                                 Return closed
                                                             End If
                                                             Return False
                                                         End Function
        Protected Overridable Function OnCheckTextLiteral(text As CharacterReader, ByRef index As Integer, ByRef length As Integer) As Boolean
            Return CheckTextLiteral(text, index, length, Nothing)
        End Function

        Public Property CheckNumber As LexerCheck = Function(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef subKind As Integer)
                                                        If text.CompareCurrent(AddressOf Char.IsDigit, ComparisonAdvancementMode.Never) Then
                                                            If text.CompareRange(New Char() {"0"c, "x"c}, ComparisonAdvancementMode.IfTrue) Then
                                                                length += 2
                                                                While text.CompareCurrent({New CharacterComparer(AddressOf Char.IsDigit),
                                                                                      New CharacterComparer(Function(c As Char)
                                                                                                                Return HexChars.Contains(c)
                                                                                                            End Function)}, False, ComparisonAdvancementMode.IfTrue)
                                                                    length += 1
                                                                End While
                                                                If length > 2 Then
                                                                    subKind = DefaultTokenKind.NumericHex
                                                                    Return True
                                                                End If
                                                                length = 1
                                                                subKind = DefaultTokenKind.NumericInteger
                                                                Return True
                                                            End If
                                                            subKind = DefaultTokenKind.NumericInteger
                                                            length += 1
                                                            Dim foundDecimal As Boolean = False
                                                            Dim decChar = My.Application.Culture.NumberFormat.NumberDecimalSeparator.Chars(0)
                                                            Dim lastLength = 0
                                                            Do While Not lastLength = length
                                                                lastLength = length
                                                                While (text.CompareCurrent(AddressOf Char.IsDigit, ComparisonAdvancementMode.IfTrue))
                                                                    length += 1
                                                                End While
                                                                If Not foundDecimal AndAlso text.CompareCurrent(decChar, ComparisonAdvancementMode.IfTrue) Then
                                                                    foundDecimal = True
                                                                    length += 1
                                                                    subKind = DefaultTokenKind.NumericFloat
                                                                Else
                                                                    length -= 1
                                                                    Exit Do
                                                                End If
                                                            Loop
                                                            Return True
                                                        End If
                                                        Return False
                                                    End Function
        Protected Overridable Function OnCheckNumber(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef numberKind As Integer) As Boolean
            Return CheckNumber(text, index, length, numberKind)
        End Function

        Public Property CheckWhitespace As LexerCheck = Function(text As CharacterReader, ByRef index As Integer, ByRef length As Integer, ByRef subKind As Integer)
                                                            While Char.IsWhiteSpace(text.Current)
                                                                length += 1
                                                                text.Read()
                                                            End While
                                                            Return length > 0
                                                        End Function
        Protected Overridable Function OnCheckWhiteSpace(text As CharacterReader, ByRef index As Integer, ByRef length As Integer) As Boolean
            Return CheckWhitespace(text, index, length, Nothing)
        End Function

        Public Overridable Function GetTokens(text As CharacterReader) As IEnumerable(Of Token)
            Dim tokens As New List(Of Token)
            Dim index As Integer = 0
            Dim curIdIdx As Integer = -1
            Dim curLength As Integer = 0

            While index < text.Length
                Dim l As Integer = 0
                Try
                    Dim k = FindTokenKind(text, index, l)
                    If Not AllowUnknownTokens AndAlso k = DefaultTokenKind.Unknown Then Throw New LexerException(LexerErrorMessages.UnknownToken, text, index, tokens.LastOrDefault.Kind)
                    If Not AllowZeroLengthTokens AndAlso l = 0 Then Throw New LexerException(LexerErrorMessages.ZeroLengthToken, text, index, tokens.LastOrDefault.Kind)
                    If l = 0 Then
                        tokens.Add(New Token(index, l, DefaultTokenKind.Unknown))
                        Exit While
                    End If
                    If k = DefaultTokenKind.Identifier Then
                        If curIdIdx = -1 Then curIdIdx = index
                        curLength += l
                    Else
                        If curIdIdx > -1 Then
                            tokens.Add(New Token(curIdIdx, curLength, DefaultTokenKind.Identifier))
                            curIdIdx = -1
                            curLength = 0
                        End If
                        tokens.Add(New Token(index, l, k))
                    End If
                Catch ex As Exception
                    Throw New LexerException(LexerErrorMessages.Unexpected, text, index, tokens.LastOrDefault.Kind, ex)
                End Try
                index += l
            End While
            If curIdIdx > -1 Then tokens.Add(New Token(curIdIdx, curLength, DefaultTokenKind.Identifier))
            Return tokens
        End Function

        Private NotInheritable Class LexerErrorMessages
            Friend Const Unexpected As String = "Unexpected lexer error, see inner exception for details."
            Friend Const UnknownToken As String = "Unknown token encountered."
            Friend Const ZeroLengthToken As String = "Zero-length token encountered."
        End Class
    End Class
End Namespace

