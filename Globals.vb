#Region "Imports"
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Threading
#End Region
Public Module Globals
	Public Const SongTable_SM As UInt16 = &H5820
	Public Const SongTable_SMW As UInt16 = &H1360
	Public Const SongTable_ALTTP1 As UInt16 = &H2A00
	Public Const SongTable_ALTTP2 As UInt16 = &H2900
	Public Const SongTable_ALTTP3 As UInt16 = &H2B00
	Public Const SongTable_ALTTP4 As UInt16 = &HD000
	Public Const SongIndex_SM As UInt16 = &HF4
	Public Const SongIndex_SMW As UInt16 = &H0
	Public Const SongIndex_ALTTP As UInt16 = &H4
	''' <summary>Array of all valid intrinsic Integer types</summary>
	Private _IntegerTypes As Type() = {GetType(SByte), GetType(Byte), GetType(Int16), GetType(UInt16), GetType(Int32), GetType(UInt32), GetType(Int64), GetType(UInt64)}
	''' <summary>Array of all valid intrinsic Numeric types</summary>
	Private _NumberTypes As Type() = {GetType(SByte), GetType(Byte), GetType(Int16), GetType(UInt16), GetType(Int32), GetType(UInt32), GetType(Int64), GetType(UInt64), GetType(Single), GetType(Double), GetType(Decimal)}
	''' <summary>A callback function that should return True when WaitForTrue() should complete</summary>
	Public Delegate Function WaitBool() As Boolean
	''' <summary>Sets the variable to the specified value, in place, truncating as necessary, for the target data type</summary>
	<Extension()> Public Sub Clamp(ByRef obj As Object, ByVal value As Object)
		If Not _IntegerTypes.Contains(value.GetType) Then
			Throw New ArgumentException("Not a vaild number type")
		End If
		Select Case obj.GetType()
			Case GetType(SByte)
				obj = Math.Min(SByte.MaxValue, Math.Max(SByte.MinValue, value))
			Case GetType(Byte)
				obj = Math.Min(Byte.MaxValue, Math.Max(Byte.MinValue, value))
			Case GetType(Int16)
				obj = Math.Min(Int16.MaxValue, Math.Max(Int16.MinValue, value))
			Case GetType(UInt16)
				obj = Math.Min(UInt16.MaxValue, Math.Max(UInt16.MinValue, value))
			Case GetType(Int32)
				obj = Math.Min(Int32.MaxValue, Math.Max(Int32.MinValue, value))
			Case GetType(UInt32)
				obj = Math.Min(UInt32.MaxValue, Math.Max(UInt32.MinValue, value))
			Case GetType(Int64)
				obj = Math.Min(Int64.MaxValue, Math.Max(Int64.MinValue, value))
			Case GetType(UInt64)
				obj = Math.Min(UInt64.MaxValue, Math.Max(UInt64.MinValue, value))
			Case Else
				Throw New ArgumentException("Not a vaild number type")
		End Select
	End Sub
	''' <summary>Sets the variable to the specified value, in place, clamped within the specified range</summary>
	<Extension()> Public Sub Clamp(ByRef obj As Object, ByVal Min As Object, ByVal Max As Object)
		If Not _NumberTypes.Contains(obj.GetType) OrElse Not _NumberTypes.Contains(Min.GetType) OrElse Not _NumberTypes.Contains(Max.GetType) Then
			Throw New ArgumentException("Not a vaild number type")
		End If
		If Min > Max Then Throw New ArgumentException("Minimum value cannot be greater than maximum value")
		If obj < Min Then
			obj = Min
		ElseIf obj > Max Then
			obj = Max
		End If
	End Sub
	''' <summary>Converts the unsigned byte to a signed byte</summary>
	<Extension()> Public Function ToSByte(ByVal obj As Byte) As SByte
		If obj >= &H80 Then
			Dim TempShort As Short = -&H100
			TempShort += obj
			Return TempShort
		Else
			Return obj
		End If
	End Function
	Public Function WaitForTrue(ByRef obj As Boolean, ByVal Timeout As Long, ByVal Interval As Long) As Boolean
		Return WaitForTrue_Pvt(obj, Timeout, Interval, False)
	End Function
	Private Function WaitForTrue_Pvt(ByRef obj As Object, ByVal Timeout As Long, ByVal Interval As Long, ByVal IsCallback As Boolean) As Boolean
		Dim Counter As Long = 0
		Dim NumLoops As Double = CDbl(Timeout) / CDbl(Interval)
		Dim FullLoops As Long = Math.Floor(NumLoops)
		Dim LastLoop As Long = Timeout - (FullLoops * Interval)
		For i As Integer = 0 To FullLoops - 1
			If WaitForTrue_Eval(obj, IsCallback) Then Return True
			Thread.Sleep(Interval)
		Next
		If LastLoop > 0 Then
			If WaitForTrue_Eval(obj, IsCallback) Then Return True
			Thread.Sleep(LastLoop)
		End If
		Return WaitForTrue_Eval(obj, IsCallback)
	End Function
	Private Function WaitForTrue_Eval(ByRef obj As Object, ByVal IsCallback As Boolean) As Boolean
		If IsCallback Then
			Return CType(obj, WaitBool).Invoke()
		Else
			Return obj
		End If
	End Function
	<Extension()> Public Function ReadUInt16(ByVal obj As Byte(), ByVal Offset As Integer) As UInt16
		Dim RetVal As Integer = obj(Offset + 1)
		RetVal = RetVal << 8
		RetVal = RetVal + obj(Offset)
		Return CUShort(RetVal)
	End Function
	<Extension()> Public Sub WriteByte(ByVal obj As Byte(), ByVal Offset As Integer, ByVal Data As Byte)
		obj(Offset) = Data
	End Sub
	<Extension()> Public Sub SetLabelEllipsisText(ByVal obj As Label, ByVal Text As String)
		If obj.AutoSize OrElse obj.AutoEllipsis Then
			obj.Text = Text
		Else
			Try
				Using g As Graphics = Graphics.FromHwnd(obj.Handle)
					Dim LabelSize As Size = obj.ClientSize
					Dim TextWidth As Integer = g.MeasureString(Text, obj.Font).Width
					If TextWidth < LabelSize.Width Then
						obj.Text = Text
					Else
						Dim Filename As String = Path.GetFileName(Text)
						Dim PreFilename As String = Text.Substring(0, Text.Length - Filename.Length)
						Dim PathRoot As String = PreFilename.Substring(0, PreFilename.IndexOf("\"))
						Dim PathMid As String = PreFilename.Substring(PathRoot.Length, PreFilename.Length - PathRoot.Length)
						If PathMid.EndsWith("\") Then PathMid = PathMid.Substring(0, PathMid.Length - 1)
						Dim PrevString As String = PathRoot & "...\" & Filename
						For i As Integer = 1 To PathMid.Length - 1
							Dim TestString As String = PathRoot & PathMid.Substring(0, i) & "...\" & Filename
							Dim TestWidth As Integer = g.MeasureString(TestString, obj.Font).Width
							If TestWidth > LabelSize.Width Then
								obj.Text = PrevString
								Return
							End If
							PrevString = TestString
						Next
						obj.Text = PrevString
					End If
				End Using
			Catch ex As Exception
				obj.Text = Text
			End Try
		End If
	End Sub
End Module
Public Enum PlayStates As Integer
	NotLoaded = 0
	UnsupportedGame
	NoSongSelected
	Stopped
	Paused
	Playing
End Enum
Public Enum Games As Integer
	None
	Unknown
	SuperMetroid
	SuperMarioWorld
	ALinkToThePast
End Enum
