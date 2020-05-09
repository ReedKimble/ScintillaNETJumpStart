Imports System.Drawing
Imports ScintillaNET

Public Class ScintillaManager(Of T As ParserToolKit.GrammarElement)
    Public Const DefaultStyle As Integer = 32

    Public Event CallTipNeeded As EventHandler(Of CallTipNeededEventArgs)
    Public Event IntellisenseNeeded As EventHandler(Of IntellisenseNeededEventArgs)
    Public Event ParseTokensIntoGrammar As EventHandler(Of ParseTokensIntoGrammarEventArgs)
    Public Event PastingText As EventHandler(Of PastingTextEventArgs)
    Public Event StyleNeeded As EventHandler(Of StyleNeededEventArgs)

    Private WithEvents _Document As New ManagedDocument
    Public ReadOnly Property Document As ManagedDocument
        Get
            Return _Document
        End Get
    End Property

    Public Property FontName As String = "Consolas"
    Public Property FontSize As Integer = 12

    Public ReadOnly Property GrammarKindStyleMap As New Dictionary(Of Integer, String)

    Public Property IntellisenseTipTriggerCharacters As Char() = {" "c, "."c}

    Private WithEvents _Parser As New ParserToolKit.Parser(Of T)
    Public Property Parser As ParserToolKit.Parser(Of T)
        Get
            Return _Parser
        End Get
        Set(value As ParserToolKit.Parser(Of T))
            _Parser = value
        End Set
    End Property

    Private _Styles As New List(Of StyleDefinition)
    Public ReadOnly Property Styles As IEnumerable(Of StyleDefinition)
        Get
            Return _Styles
        End Get
    End Property

    Private _StyleNames As New Dictionary(Of String, Integer)
    Public ReadOnly Property StyleNames As IEnumerable(Of String)
        Get
            Return _StyleNames.Keys
        End Get
    End Property

    Public Property UseWindowsFormsToolTip As Boolean = True

    Private WithEvents scintilla As ScintillaNET.Scintilla
    Private shouldCheck As Boolean
    Private clipboardText As String = String.Empty
    Private commandToolTip As Windows.Forms.ToolTip

    Public Sub New(scintillaInstance As ScintillaNET.Scintilla)
        Me.scintilla = scintillaInstance
        commandToolTip = New Windows.Forms.ToolTip()
    End Sub

    Public Function CreateStyleDefinition(styleName As String) As StyleDefinition
        If _StyleNames.ContainsKey(styleName) Then
            _Styles.RemoveAt(_StyleNames(styleName))
        End If
        Dim result As New StyleDefinition
        _Styles.Add(result)
        _StyleNames(styleName) = _Styles.Count - 1
        Return result
    End Function

    Public Function LoadFromDefinitionDocument(Of TEnum As Structure)(document As XDocument) As Integer
        Dim result As Integer = 0
        Dim unnamedCount As Integer = 0
        For Each style In StyleDefinition.FromStyleDefinitionDocument(document)
            If String.IsNullOrEmpty(style.Name) Then
                unnamedCount += 1
                style.Name = $"(Unnamed{unnamedCount})"
            End If
            Dim styleName As String = style.Name
            If _StyleNames.ContainsKey(styleName) Then
                _Styles.RemoveAt(_StyleNames(styleName))
            End If
            _Styles.Add(style)
            _StyleNames(styleName) = _Styles.Count - 1
            Dim ev As TEnum = New TEnum
            If [Enum].TryParse(Of TEnum)(styleName, ev) Then
                _GrammarKindStyleMap([Enum].ToObject(GetType(TEnum), ev)) = styleName
            End If

            result += 1
        Next
        Return result
    End Function

    Public Sub Configure()
        scintilla.WordChars &= ControlChars.Quote

        scintilla.StyleResetDefault()
        scintilla.Styles(DefaultStyle).Font = FontName
        scintilla.Styles(DefaultStyle).Size = FontSize
        scintilla.StyleClearAll()
        scintilla.Styles(DefaultStyle).ForeColor = Color.Black

        For i = 0 To _Styles.Count - 1
            Dim s = _Styles(i)
            Dim idx = i + 1
            With scintilla.Styles(idx)
                .BackColor = s.BackColor
                .Bold = s.Bold
                .Case = s.Case
                .FillLine = s.FillLine
                .Font = s.Font
                .ForeColor = s.ForeColor
                .Hotspot = s.HotSpot
                .Italic = s.Italic
                .Size = s.Size
                .Underline = s.Underline
                .Weight = s.Weight
            End With
            s.SetIndex(idx)
        Next

        scintilla.Lexer = ScintillaNET.Lexer.Container
    End Sub

    Private Sub scintilla_StyleNeeded(sender As Object, e As ScintillaNET.StyleNeededEventArgs) Handles scintilla.StyleNeeded
        Dim startPos = scintilla.GetEndStyled()
        Dim endPos = e.Position
        Dim lineId = scintilla.LineFromPosition(endPos)
        Dim line = scintilla.Lines(lineId)
        startPos = line.Position
        'Dim length = line.Length

        'parse line
        Dim tokens As IEnumerable(Of ParserToolKit.Lexer.Token) = Nothing
        Dim parsedGrammars = Parser.Parse(line.Text, tokens)

        'Identify scintilla line and correlate with DocumentLine instance
        Dim currentLine As DocumentLine
        If Document.Contains(lineId) Then
            currentLine = Document(lineId)
        Else
            currentLine = New DocumentLine
            currentLine.LineId = lineId
            Document.Add(currentLine)
        End If

        currentLine.Line = line
        currentLine.Grammars = parsedGrammars
        currentLine.Tokens = tokens

        'begin styling
        scintilla.StartStyling(startPos)
        If parsedGrammars IsNot Nothing Then
            Dim i As Integer = -1
            For x = 0 To tokens.Count - 1
                If tokens(x).Kind = ParserToolKit.GrammarElement.WhitespaceKind Then
                    scintilla.SetStyling(tokens(x).Length, DefaultStyle)
                Else
                    i += 1
                    Dim g = parsedGrammars(i)
                    scintilla.SetStyling(g.Length, GetStyleID(g))
                End If
            Next
        End If

        Document.SetLastStyled(currentLine)
    End Sub

    'use to handle tool tips And selection drop down
    Private Sub scintilla_CharAdded(sender As Object, e As ScintillaNET.CharAddedEventArgs) Handles scintilla.CharAdded
        Dim c = Chr(e.Char)
        Dim line = Document.LastStyledLine
        If line IsNot Nothing Then
            Dim g = line.GrammarAtCurrentPosition(Me)
            If g IsNot Nothing AndAlso IntellisenseTipTriggerCharacters.Contains(c) Then
                'display tool tip, if any
                Dim tipArgs As New CallTipNeededEventArgs(line, g)
                OnCallTipNeeded(tipArgs)
                If Not String.IsNullOrEmpty(tipArgs.TipText) Then
                    If UseWindowsFormsToolTip Then
                        commandToolTip.Show(tipArgs.TipText, scintilla, scintilla.PointXFromPosition(scintilla.CurrentPosition), scintilla.PointYFromPosition(scintilla.CurrentPosition) - line.Line.Height)
                    Else
                        scintilla.CallTipShow(scintilla.CurrentPosition, tipArgs.TipText)
                        scintilla.CallTipSetPosition(True)
                    End If
                Else
                    If UseWindowsFormsToolTip Then
                        commandToolTip.Hide(scintilla)
                    Else
                        If scintilla.CallTipActive Then scintilla.CallTipCancel()
                    End If
                End If

                'display intellisense, if any
                Dim intelArgs As New IntellisenseNeededEventArgs(line, g)
                OnIntellisenseNeeded(intelArgs)
                If Not String.IsNullOrEmpty(intelArgs.ItemsString) Then
                    scintilla.AutoCShow(0, intelArgs.ItemsString)
                ElseIf intelArgs.ListItems IsNot Nothing AndAlso intelArgs.ListItems.Count > 0 Then
                    scintilla.AutoCShow(0, String.Join(scintilla.AutoCSeparator, intelArgs.ListItems))
                Else
                    If scintilla.AutoCActive Then scintilla.AutoCCancel()
                End If

            End If
        End If
    End Sub


    'use following two events together to handle paste editing
    Private Sub scintilla_InsertCheck(sender As Object, e As ScintillaNET.InsertCheckEventArgs) Handles scintilla.InsertCheck
        If shouldCheck Then
            If e.Text = clipboardText Then
                Dim args As New PastingTextEventArgs(clipboardText)
                OnPastingText(args)
                e.Text = args.Text
                clipboardText = String.Empty
            End If
        End If
    End Sub
    Private Sub scintilla_KeyDown(sender As Object, e As Windows.Forms.KeyEventArgs) Handles scintilla.KeyDown
        shouldCheck = (e.Control AndAlso e.KeyCode = Windows.Forms.Keys.V)
        If shouldCheck Then
            If Windows.Forms.Clipboard.ContainsText Then
                clipboardText = Windows.Forms.Clipboard.GetText
            Else
                clipboardText = String.Empty
            End If
        End If
    End Sub

    Protected Overridable Sub OnCallTipNeeded(e As CallTipNeededEventArgs)
        RaiseEvent CallTipNeeded(Me, e)
    End Sub

    Protected Overridable Sub OnIntellisenseNeeded(e As IntellisenseNeededEventArgs)
        RaiseEvent IntellisenseNeeded(Me, e)
    End Sub

    Protected Overridable Sub OnParseTokensIntoGrammar(e As ParseTokensIntoGrammarEventArgs)
        RaiseEvent ParseTokensIntoGrammar(Me, e)
    End Sub

    Protected Overridable Sub OnPastingText(e As PastingTextEventArgs)
        RaiseEvent PastingText(Me, e)
    End Sub

    Protected Overridable Sub OnStyleNeeded(e As StyleNeededEventArgs)
        RaiseEvent StyleNeeded(Me, e)
    End Sub

    Private Function GetStyleID(g As T) As Integer
        Dim result As Integer
        If GrammarKindStyleMap.ContainsKey(g.ElementKind) Then
            result = _Styles(_StyleNames(_GrammarKindStyleMap(g.ElementKind))).Index
        Else
            result = DefaultStyle
        End If
        Dim args As New StyleNeededEventArgs(g, result)
        OnStyleNeeded(args)
        Return args.SelectedStyleID
    End Function

    Private Sub _Parser_AnalyzeTokens(sender As Object, e As ParserToolKit.Parser(Of T).AnalyzeTokensEventArgs) Handles _Parser.AnalyzeTokens
        Dim args = New ParseTokensIntoGrammarEventArgs(e)
        OnParseTokensIntoGrammar(args)
        e.Result = args.Result
    End Sub

    Private Sub _Document_LastStyledLineChanged(sender As Object, e As EventArgs) Handles _Document.LastStyledLineChanged
        commandToolTip.Hide(scintilla)
    End Sub

    Private Sub scintilla_LostFocus(sender As Object, e As EventArgs) Handles scintilla.LostFocus
        commandToolTip.Hide(scintilla)
    End Sub
End Class
