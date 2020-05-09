Option Strict On

Imports System.IO
Imports System.Threading

Namespace ParserToolKit
    ''' <summary>
    ''' Provides a representation of a string optimized for fast, effecient, and versatile reading and comparison operations.
    ''' Functions as a String and Enumerable of Char, a Char Enumerator, and a Text Reader with additional features to facilitate
    ''' string reading as well as parsing token content from strings.
    ''' </summary>
    Public Class CharacterReader
        Inherits TextReader
        Implements IEnumerable(Of Char)
        Implements IEnumerator(Of Char)

        Private value As IEnumerable(Of Char)
        Private tempBuilder As New Text.StringBuilder
        Private valueLength As Integer

        ''' <summary>
        ''' Creates a new CharacterReader instance from the specified string.
        ''' </summary>
        ''' <param name="text"></param>
        Public Sub New(text As String)
            value = text
            valueLength = text.Length
        End Sub

        ''' <summary>
        ''' Gets the collection of characters which make up the value.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Characters As IEnumerable(Of Char)
            Get
                Return value
            End Get
        End Property

        ''' <summary>
        ''' Gets the character from the value at the specified index. Returns the null character is index is
        ''' outside the bounds of the value. Reading this property has no effect on the current position.
        ''' </summary>
        ''' <param name="index">The index of the character to get.</param>
        ''' <returns>The character at the specified index, or the null character if the index is invalid.</returns>
        Default Public ReadOnly Property Chars(index As Integer) As Char
            Get
                If index > -1 AndAlso index < valueLength Then Return value(index)
                Return ControlChars.NullChar
            End Get
        End Property

        ''' <summary>
        ''' Gets a range of characters as a substring of the value without affecting the current position. *Creates a new String instance.
        ''' </summary>
        ''' <param name="index">The position in the value at which the substring begins.</param>
        ''' <param name="length">The length of the substring within the value.</param>
        ''' <returns>A new String instance containing the specified substring of the value, or an empty string if the specified
        ''' index and length extend beyond the bounds of the value.</returns>
        Default Public ReadOnly Property Chars(index As Integer, length As Integer) As String
            Get
                If index > -1 AndAlso index + length <= valueLength Then Return DirectCast(value, String).Substring(index, length)
                Return String.Empty
            End Get
        End Property

        ''' <summary>
        ''' Gets the character at the current position in the CharacterReader, or the null character if the position is
        ''' before the beginning of the value or past the end of the value.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Current As Char Implements IEnumerator(Of Char).Current
            Get
                If _Position > -1 AndAlso _Position < valueLength Then Return value(_Position)
                Return ControlChars.NullChar
            End Get
        End Property

        Private ReadOnly Property IEnumerator_Current As Object Implements IEnumerator.Current
            Get
                Return Current
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating if the current position is past the end of the value.
        ''' </summary>
        ''' <returns>True if the current position is past the end of the value, otherwise false.</returns>
        Public ReadOnly Property EndOfString As Boolean
            Get
                Return _Position >= Length
            End Get
        End Property

        ''' <summary>
        ''' Gets the length of the value, or in other words, the number of characters in the value.
        ''' </summary>
        ''' <returns>An integer indicating the number of characters in the value.</returns>
        Public ReadOnly Property Length As Integer
            Get
                Return valueLength
            End Get
        End Property

        ''' <summary>
        ''' Gets the current position within the value string.
        ''' </summary>
        ''' <returns>An integer indicating the current position within the value.</returns>
        Public ReadOnly Property Position As Integer = -1

        ''' <summary>
        ''' Closes the CharacterReader, releasing all resources.  (calls Dispose)
        ''' </summary>
        Public Overrides Sub Close()
            Dispose()
        End Sub

        Public Function CompareAt(index As Integer, c As Char) As Boolean
            If index > -1 AndAlso index < Length Then
                Return value(index) = c
            End If
            Return False
        End Function

        ''' <summary>
        ''' Effeciently compare a character to the character in the value at the current position.
        ''' The effect on the current position depends on the value of the advanceMode parameter.
        ''' </summary>
        ''' <param name="c">The character to compare to the value at the current position.</param>
        ''' <param name="advanceMode">One of the ComparisonAdvancementMode enum values: Never, Always, IfTrue, or IfFalse</param>
        ''' <returns>True if the comparision is true, otherwise false.</returns>
        Public Function CompareCurrent(c As Char, advanceMode As ComparisonAdvancementMode) As Boolean
            If _Position > -1 AndAlso _Position <= Length Then
                If Not value(_Position) = c Then
                    If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfFalse Then _Position += 1
                    Return False
                End If
                If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfTrue Then _Position += 1
                Return True
            End If
            Return False
        End Function

        Public Function CompareCurrent(comparer As Func(Of Char, Boolean), advanceMode As ComparisonAdvancementMode) As Boolean
            If _Position > -1 AndAlso _Position <= Length Then
                If Not comparer(value(_Position)) Then
                    If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfFalse Then _Position += 1
                    Return False
                End If
                If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfTrue Then _Position += 1
                Return True
            End If
            Return False
        End Function

        Public Function CompareCurrent(comparer As CharacterComparer, advanceMode As ComparisonAdvancementMode) As Boolean
            If _Position > -1 AndAlso _Position <= Length Then
                If Not comparer.Compare(value(_Position)) Then
                    If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfFalse Then _Position += 1
                    Return False
                End If
                If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfTrue Then _Position += 1
                Return True
            End If
            Return False
        End Function

        ''' <summary>
        ''' Effeciently compares a string of characters to the characters in the value beginning at the current position.
        ''' The effect on the current position depends on the value of the advanceMode parameter.
        ''' </summary>
        ''' <param name="text">The string to compare to the value at the current position.</param>
        ''' <param name="advanceMode">One of the ComparisonAdvancementMode enum values: Never, Always, IfTrue, or IfFalse</param>
        ''' <returns>True if the comparision is true, otherwise false.</returns>
        Public Function CompareCurrent(text As String, advanceMode As ComparisonAdvancementMode) As Boolean
            If _Position > -1 AndAlso _Position + text.Length <= Length Then
                For i = 0 To text.Length - 1
                    If Not value(_Position + i) = text(i) Then
                        If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfFalse Then _Position += text.Length
                        Return False
                    End If
                Next
                If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfTrue Then _Position += text.Length
                Return True
            End If
            Return False
        End Function

        Public Function CompareCurrent(comparer As IEnumerable(Of CharacterComparer), compareModeAnd As Boolean, advanceMode As ComparisonAdvancementMode) As Boolean
            If _Position > -1 AndAlso _Position <= Length Then
                Dim isValid As Boolean = False
                For Each c In comparer
                    Dim compareResult = c.Compare(Current)
                    If compareModeAnd Then
                        isValid = compareResult
                        If Not isValid Then Exit For
                    Else
                        If compareResult Then isValid = True
                    End If
                Next
                If isValid Then
                    If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfTrue Then _Position += 1
                    Return True
                Else
                    If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfFalse Then _Position += 1
                    Return False
                End If
            End If
            Return False
        End Function

        Public Function CompareCurrent(comparer As IEnumerable(Of Func(Of Char, Boolean)), compareModeAnd As Boolean, advanceMode As ComparisonAdvancementMode) As Boolean
            If _Position > -1 AndAlso _Position <= Length Then
                Dim isValid As Boolean = False
                For Each c In comparer
                    Dim compareResult = c(Current)
                    If compareModeAnd Then
                        isValid = compareResult
                        If Not isValid Then Exit For
                    Else
                        If compareResult Then isValid = True
                    End If
                Next
                If isValid Then
                    If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfTrue Then _Position += 1
                    Return True
                Else
                    If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfFalse Then _Position += 1
                    Return False
                End If
            End If
            Return False
        End Function

        Public Function CompareRange(comparer As IEnumerable(Of CharacterComparer), advanceMode As ComparisonAdvancementMode) As Boolean
            If _Position > -1 AndAlso _Position + comparer.Count - 1 <= Length Then
                Dim isValid As Boolean = True
                Dim idx = _Position
                For Each c In comparer
                    isValid = c.Compare(value(idx))
                    If isValid Then
                        idx += 1
                    Else
                        Exit For
                    End If
                Next
                If isValid Then
                    If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfTrue Then _Position += comparer.Count
                    Return True
                Else
                    If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfFalse Then _Position += comparer.Count
                    Return False
                End If
            End If
            Return False
        End Function

        Public Function CompareRange(comparer As IEnumerable(Of Char), advanceMode As ComparisonAdvancementMode) As Boolean
            If _Position > -1 AndAlso _Position + comparer.Count - 1 <= Length Then
                Dim isValid As Boolean = True
                Dim idx = _Position
                For Each c In comparer
                    isValid = (c = value(idx))
                    If isValid Then
                        idx += 1
                    Else
                        Exit For
                    End If
                Next
                If isValid Then
                    If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfTrue Then _Position += comparer.Count
                    Return True
                Else
                    If advanceMode = ComparisonAdvancementMode.Always OrElse advanceMode = ComparisonAdvancementMode.IfFalse Then _Position += comparer.Count
                    Return False
                End If
            End If
            Return False
        End Function
        Public Overrides Function Equals(obj As Object) As Boolean
            If obj Is Nothing Then Return value Is Nothing
            If TypeOf obj Is CharacterReader Then Return DirectCast(value, String) = DirectCast(DirectCast(obj, CharacterReader).value, String)
            If TypeOf obj Is String Then Return DirectCast(value, String) = (DirectCast(obj, String))
            Return DirectCast(value, String) = obj.ToString
        End Function

        Public Function EndsWith(c As Char) As Boolean
            If valueLength = 0 Then Return False
            Return value.Last = c
        End Function

        Public Function GetEnumerator() As IEnumerator(Of Char) Implements IEnumerable(Of Char).GetEnumerator
            Return DirectCast(value.GetEnumerator, IEnumerator(Of Char))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return value.GetHashCode()
        End Function

        Public Function GetSubString(index As Integer, length As Integer) As String
            Return New String(value.Skip(index).Take(length).ToArray)
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

        ''' <summary>
        ''' Advances the current position within the value and returns true if the position is within the bounds of the value.
        ''' </summary>
        ''' <returns>True if the current position is within the bounds of the value, otherwise false.</returns>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            _Position += 1
            Return _Position > -1 AndAlso _Position < valueLength
        End Function

        Public Function MovePrevious() As Boolean
            _Position -= 1
            Return _Position > -1 AndAlso _Position < valueLength
        End Function

        ''' <summary>
        ''' Gets the character code of the character at the next position in the string, or -1 if the next position is
        ''' outside the bounds of the value.
        ''' </summary>
        ''' <returns>An integer represeting the character code of the character at the next position.</returns>
        Public Overrides Function Peek() As Integer
            Dim idx = _Position + 1
            If idx < 0 OrElse idx >= valueLength Then Return -1
            Return AscW(Chars(idx))
        End Function

        ''' <summary>
        ''' Gets the character code of the character at a position in the string specified by the current position plus
        ''' the length provided, or -1 if the next position is outside the bounds of the value.
        ''' </summary>
        ''' <param name="length">The number of characters after the current position to get.</param>
        ''' <returns>An integer represeting the character code of the character at the specified position.</returns>
        Public Overloads Function Peek(length As Integer) As Integer
            Dim idx = _Position + length
            If idx < 0 OrElse idx >= valueLength Then Return -1
            Return AscW(Chars(idx))
        End Function

        ''' <summary>
        ''' Gets a new string from the next position in the value for the specified number of characters. *Creates a new String instance.
        ''' </summary>
        ''' <param name="length">The number of characters to get from the next position in the value.</param>
        ''' <returns>A string containing the characters from the next position for the specified length.</returns>
        Public Overloads Function PeekString(length As Integer) As String
            Return Chars(_Position + 1, length)
        End Function

        ''' <summary>
        ''' Gets the character code of the character at the current position and advances the current position.
        ''' </summary>
        ''' <returns>An integer represeting the character code of the character at the current position.</returns>
        Public Overrides Function Read() As Integer
            Dim result = AscW(Chars(_Position))
            MoveNext()
            Return result
        End Function

        ''' <summary>
        ''' Reads characters from the current position into the specified buffer, beginning at the specified index within
        ''' the buffer and for a maximum number of characters specified by count, advancing the current position within
        ''' the CharacterReader for each character read.
        ''' </summary>
        ''' <param name="buffer">The buffer to read characters into.</param>
        ''' <param name="index">The starting position within the buffer to place characters read.</param>
        ''' <param name="count">The maximum number of characters to read.</param>
        ''' <returns>An integer representing the number of characters read into the buffer.</returns>
        Public Overrides Function Read(buffer() As Char, index As Integer, count As Integer) As Integer
            Dim initialCount As Integer = count
            While _Position < Length AndAlso index < buffer.Length AndAlso count > 0
                buffer(index) = Current
                index += 1
                count -= 1
                MoveNext()
            End While
            Return initialCount - count
        End Function

        ''' <summary>
        ''' Reads characters from the current position into the specified buffer, beginning at the specified index within
        ''' the buffer and for a maximum number of characters specified by count, advancing the current position within
        ''' the CharacterReader for each character read. (Completes Synchronously)
        ''' </summary>
        ''' <param name="buffer">The buffer to read characters into.</param>
        ''' <param name="index">The starting position within the buffer to place characters read.</param>
        ''' <param name="count">The maximum number of characters to read.</param>
        ''' <returns>An integer representing the number of characters read into the buffer.</returns>
        Public Overrides Function ReadAsync(buffer() As Char, index As Integer, count As Integer) As Task(Of Integer)
            Return Task.FromResult(Read(buffer, index, count))
        End Function

        ''' <summary>
        ''' Reads characters from the current position into the specified buffer, beginning at the specified index within
        ''' the buffer and for a maximum number of characters specified by count, advancing the current position within
        ''' the CharacterReader for each character read.
        ''' </summary>
        ''' <param name="buffer">The buffer to read characters into.</param>
        ''' <param name="index">The starting position within the buffer to place characters read.</param>
        ''' <param name="count">The maximum number of characters to read.</param>
        ''' <returns>An integer representing the number of characters read into the buffer.</returns>
        Public Overrides Function ReadBlock(buffer() As Char, index As Integer, count As Integer) As Integer
            Return Read(buffer, index, count)
        End Function

        ''' <summary>
        ''' Reads characters from the current position into the specified buffer, beginning at the specified index within
        ''' the buffer and for a maximum number of characters specified by count, advancing the current position within
        ''' the CharacterReader for each character read. (Completes Synchronously)
        ''' </summary>
        ''' <param name="buffer">The buffer to read characters into.</param>
        ''' <param name="index">The starting position within the buffer to place characters read.</param>
        ''' <param name="count">The maximum number of characters to read.</param>
        ''' <returns>An integer representing the number of characters read into the buffer.</returns>
        Public Overrides Function ReadBlockAsync(buffer() As Char, index As Integer, count As Integer) As Task(Of Integer)
            Return ReadAsync(buffer, index, count)
        End Function

        ''' <summary>
        ''' Gets a substring from the current position to the next new-line character, and advances the position within the
        ''' string. *Creates a new String instance.
        ''' </summary>
        ''' <returns>A new String instance containing the substring from the current position to the next new-line character, or an empty string
        ''' if the current position is beyond the bounds of the value.</returns>
        Public Overrides Function ReadLine() As String
            If _Position = -1 AndAlso Not MoveNext() Then Return Nothing
            tempBuilder.Clear()
            While Not Current = ControlChars.Cr AndAlso Not Current = ControlChars.Lf
                tempBuilder.Append(Current)
                If Not MoveNext() Then Return tempBuilder.ToString
            End While
            If Chars(Position) = ControlChars.Cr Then MoveNext()
            If Chars(Position) = ControlChars.Lf Then MoveNext()
            Return tempBuilder.ToString
        End Function

        ''' <summary>
        ''' Gets a substring from the current position to the next new-line character, and advances the position within the
        ''' string. *Creates a new String instance. (Completes Synchronously)
        ''' </summary>
        ''' <returns>A new String instance containing the substring from the current position to the next new-line character, or an empty string
        ''' if the current position is beyond the bounds of the value.</returns>
        Public Overrides Function ReadLineAsync() As Task(Of String)
            Return Task.FromResult(ReadLine)
        End Function

        ''' <summary>
        ''' Gets the remainder of the value from the current position as a substring, and advances the current position
        ''' past the end of the string. *Creates a new String instance.
        ''' </summary>
        ''' <returns>A new String instance containing the remainder of the value from the current position, or an empty string
        ''' if the current position is beyond the bounds of the value.</returns>
        Public Overrides Function ReadToEnd() As String
            If _Position < 0 OrElse _Position >= Length Then Return String.Empty
            _Position = Length
            Return DirectCast(value, String).Substring(_Position)
        End Function

        ''' <summary>
        ''' Gets the remainder of the value from the current position as a substring, and advances the current position
        ''' past the end of the string. *Creates a new String instance. (Completes Synchronously)
        ''' </summary>
        ''' <returns>A new String instance containing the remainder of the value from the current position, or an empty string
        ''' if the current position is beyond the bounds of the value.</returns>
        Public Overrides Function ReadToEndAsync() As Task(Of String)
            Return Task.FromResult(ReadToEnd)
        End Function

        ''' <summary>
        ''' Gets a substring from the current position to the next white-space character, and advances the position within the
        ''' string. *Creates a new String instance.
        ''' </summary>
        ''' <returns>A new String instance containing the substring from the current position to the next white-space character, or an empty string
        ''' if the current position is beyond the bounds of the value.</returns>
        Public Function ReadToWhiteSpace() As String
            If _Position = -1 AndAlso Not MoveNext() Then Return Nothing
            tempBuilder.Clear()
            While Not Char.IsWhiteSpace(Current)
                tempBuilder.Append(Current)
                If Not MoveNext() Then Return tempBuilder.ToString
            End While
            Return tempBuilder.ToString
        End Function

        ''' <summary>
        ''' Places the current position at the specified position within the bounds of the value,
        ''' or has no effect if the specified position would be outside the bounds of the value.
        ''' </summary>
        ''' <param name="characterPosition">The position to set the current position to.</param>
        ''' <returns>True if the current position was set, otherwise false.</returns>
        Public Function Seek(characterPosition As Integer) As Boolean
            If characterPosition > -1 AndAlso characterPosition < valueLength Then
                _Position = characterPosition
                Return True
            End If
            Return False
        End Function

        ''' <summary>
        ''' Places the current position at the specified position within the bounds of the value,
        ''' relative to the specified SeekOrigin (forward from the beginning, forward from the current position,
        ''' or backward from the end), or has no effect if the specified position would be outside the bounds of the value.
        ''' </summary>
        ''' <param name="value">The value to set the current position to, relative to the specified origin.</param>
        ''' <param name="origin">One of the IO.SeekOrigin enum values Begin, Current, or End.</param>
        ''' <returns>True if the current position was set, otherwise false.</returns>
        Public Function Seek(value As Integer, origin As IO.SeekOrigin) As Boolean
            Select Case origin
                Case IO.SeekOrigin.Begin
                    If value < 0 OrElse value >= Length Then Return False
                    _Position = value
                Case IO.SeekOrigin.Current
                    value += _Position
                    If value < 0 OrElse value >= Length Then Return False
                    _Position = value
                Case IO.SeekOrigin.End
                    value = Length - value
                    If value < 0 OrElse value >= Length Then Return False
                    _Position = value
                Case Else
                    Return False
            End Select
            Return True
        End Function

        Public Function SeekBackward(toChar As Char) As Integer
            If _Position < 1 Then Return -1
            While Not Current = toChar
                If Not MovePrevious() Then Return -1
            End While
            If Chars(Position) = toChar Then Return Position
            Return -1
        End Function

        ''' <summary>
        ''' Sets the current position past the end of the value.
        ''' </summary>
        Public Sub SeekEnd()
            _Position = valueLength
        End Sub

        ''' <summary>
        ''' Sets the current position at the end of the next line break, if found, otherwise advances to the end of the value.
        ''' </summary>
        ''' <returns>True if a new line was found, otherwise false.</returns>
        Public Function SeekEndOfLine() As Boolean
            If _Position = -1 AndAlso Not MoveNext() Then Return False
            While Not Current = ControlChars.Cr AndAlso Not Current = ControlChars.Lf
                If Not MoveNext() Then Return False
            End While
            If Chars(Position) = ControlChars.Cr Then MoveNext()
            If Chars(Position) = ControlChars.Lf Then MoveNext()
            Return True
        End Function

        ''' <summary>
        ''' Sets the current position to the first character of the value.
        ''' </summary>
        Public Sub SeekFirst()
            _Position = 0
        End Sub

        Public Function SeekForward(toChar As Char) As Integer
            If _Position = -1 AndAlso Not MoveNext() Then Return -1
            While Not Current = toChar
                If Not MoveNext() Then Return -1
            End While
            If Chars(Position) = toChar Then Return Position
            Return -1
        End Function

        ''' <summary>
        ''' Sets the current position to the last character of the value.
        ''' </summary>
        Public Sub SeekLast()
            _Position = valueLength - 1
        End Sub

        ''' <summary>
        ''' Sets the current position before the beginning of the value.
        ''' </summary>
        Public Sub SeekStart()
            _Position = -1
        End Sub

        ''' <summary>
        ''' Advances the reader until a white-space character is encounterd, otherwise to the end of the value.
        ''' </summary>
        ''' <returns>True if a white-space character was found, otherwise false.</returns>
        Public Function SeekWhiteSpace() As Boolean
            If _Position = -1 AndAlso Not MoveNext() Then Return False
            While Not Char.IsWhiteSpace(Current)
                If Not MoveNext() Then Return False
            End While
            Return True
        End Function

        Public Function SetPosition(value As Integer) As Boolean
            If value > -2 AndAlso value <= valueLength Then
                _Position = value
                Return True
            End If
            Return False
        End Function

        Public Function StartsWith(c As Char) As Boolean
            If valueLength = 0 Then Return False
            Return value.First = c
        End Function

        ''' <summary>
        ''' Gets the underlying String instance.
        ''' </summary>
        ''' <returns>The underlying String instance.</returns>
        Public Overrides Function ToString() As String
            Return DirectCast(value, String)
        End Function

        ''' <summary>
        ''' Resets the CharacterReader to its initial state.
        ''' </summary>
        Public Sub Reset() Implements IEnumerator.Reset
            _Position = -1
            tempBuilder.Clear()
        End Sub

        Public Shared Operator =(source As CharacterReader, target As CharacterReader) As Boolean
            Return DirectCast(source.value, String) = DirectCast(target.value, String)
        End Operator

        Public Shared Operator <>(source As CharacterReader, target As CharacterReader) As Boolean
            Return Not DirectCast(source.value, String) = DirectCast(target.value, String)
        End Operator

        Public Shared Operator =(source As CharacterReader, target As String) As Boolean
            Return DirectCast(source.value, String) = target
        End Operator

        Public Shared Operator <>(source As CharacterReader, target As String) As Boolean
            Return Not DirectCast(source.value, String) = target
        End Operator

        Public Shared Operator =(source As String, target As CharacterReader) As Boolean
            Return DirectCast(target.value, String) = source
        End Operator

        Public Shared Operator <>(source As String, target As CharacterReader) As Boolean
            Return Not DirectCast(target.value, String) = source
        End Operator

        Public Shared Widening Operator CType(value As String) As CharacterReader
            Return New CharacterReader(value)
        End Operator

        Public Shared Widening Operator CType(value As CharacterReader) As String
            Return DirectCast(value.value, String)
        End Operator

#Region "IDisposable Support"
        Private disposedValue As Boolean
        Protected Overridable Overloads Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    value = Nothing
                    tempBuilder.Clear()
                    tempBuilder = Nothing
                End If
            End If
            disposedValue = True
        End Sub
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
        End Sub
#End Region
    End Class
End Namespace