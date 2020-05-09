Namespace ParserToolKit
    Partial Public Class Lexer
        Public Enum DefaultTokenKind
            Unknown = -1
            WhiteSpace = 0
            [Boolean]

            '(numeric group)
            NumericInteger
            NumericFloat
            NumericHex

            MathSymbol
            AssignmentOperator
            ComparisonOperator
            TextLiteral
            Identifier
            Period
            Punctuation

            '(balanced pair group)
            OpenParenthesis
            OpenBracket
            OpenBrace
            CloseParenthesis
            CloseBracket
            CloseBrace

            '(group elements for lexing order)
            NumericGroup
            BalancedPairGroup
        End Enum
    End Class
End Namespace
