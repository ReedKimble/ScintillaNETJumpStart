Imports System.Drawing

Public Class StyleDefinition
    Public Shared Function FromStyleDefinitionDocument(styleDocument As XDocument) As IEnumerable(Of StyleDefinition)
        Return (From element In styleDocument.Root.<style> Select FromStyleDefinitionElement(element)).ToArray
    End Function

    Public Shared Function FromStyleDefinitionElement(element As XElement) As StyleDefinition
        Dim result As New StyleDefinition
        result.BackColor = ParseColorString(element.@backcolor)
        result.Bold = CBool(element.@bold)
        result.Case = [Enum].Parse(GetType(ScintillaNET.StyleCase), element.@case)
        result.FillLine = CBool(element.@fillline)
        result.Font = element.@font
        result.ForeColor = ParseColorString(element.@forecolor)
        result.HotSpot = CBool(element.@hotspot)
        result._Index = CInt(element.@index)
        result.Italic = CBool(element.@italic)
        result.Name = element.@name
        result.Size = CInt(element.@size)
        result.Underline = CBool(element.@underline)
        result.Weight = CInt(element.@weight)
        Return result
    End Function

    Private Shared Function ParseColorString(text As String) As Color
        Dim kc As KnownColor
        If [Enum].TryParse(Of KnownColor)(text, kc) Then
            Return Color.FromKnownColor(kc)
        End If
        Dim argb As Integer
        If Integer.TryParse(text, Globalization.NumberStyles.HexNumber, Nothing, argb) Then
            Return Color.FromArgb(argb)
        End If
        Return Color.Black
    End Function

    Public Property BackColor As Color = Color.Empty
    Public Property Bold As Boolean = False
    Public Property [Case] As ScintillaNET.StyleCase = ScintillaNET.StyleCase.Mixed
    Public Property FillLine As Boolean = False
    Public Property Font As String = "Consolas"
    Public Property ForeColor As Color = Color.Black
    Public Property HotSpot As Boolean = False
    Public ReadOnly Property Index As Integer = 0
    Public Property Italic As Boolean = False
    Public Property Name As String = String.Empty
    Public Property Size As Integer = 10
    Public Property Underline As Boolean = False
    Public Property Weight As Integer = 400

    Protected Friend Sub SetIndex(scintillaIndex As Integer)
        _Index = scintillaIndex
    End Sub

    Public Function ToStyleDefinitionElement() As XElement
        Return <style backcolor=<%= BackColor.ToArgb.ToString("X8") %>
                   bold=<%= Bold %>
                   case=<%= [Case] %>
                   fillline=<%= FillLine %>
                   font=<%= Font %>
                   forecolor=<%= ForeColor.ToArgb.ToString("X8") %>
                   hostspot=<%= HotSpot %>
                   index=<%= Index %>
                   italic=<%= Italic %>
                   name=<%= Name %>
                   size=<%= Size %>
                   underline=<%= Underline %>
                   weight=<%= Weight %>
               />
    End Function
End Class
