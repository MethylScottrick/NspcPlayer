#Region "Imports"
Imports System.IO
#End Region
Namespace Sound
	''' <summary>Sound related globals</summary>
	<HideModuleName()> Public Module Globals
#Region "Constants"
		Public Const SAMPLE_RATE As Integer = 32000
		Public Const IncludeBufferMarkers As Boolean = False
		Public Const FromWriteCursor As Boolean = False
#End Region
#Region "Methods"
		''' <summary>nearest neighbor interpolation</summary>
		Public Function Resample1(ByVal Source As Short(), ByVal Factor As Double) As Short()
			Dim OutputLength As Integer = Source.Length / Factor
			Dim Output(OutputLength - 1) As Short
			For i As Integer = 0 To OutputLength - 1
				Dim srci As Integer = Math.Round(CDbl(i) * Factor)
				If srci > Source.Length - 1 Then
					Output(i) = 0
				Else
					Output(i) = Source(srci)
				End If
			Next
			Return Output
		End Function
		''' <summary>linear interpolation</summary>
		Public Function Resample2(ByVal Source As Short(), ByVal Factor As Double) As Short()
			Dim OutputLength As Integer = Source.Length / Factor
			Dim Output(OutputLength - 1) As Short
			For i As Integer = 0 To OutputLength - 1
				Dim srci As Double = CDbl(i) * Factor
				Dim srcif As Integer = Math.Floor(srci)
				Dim srcic As Integer = Math.Ceiling(srci)
				If srcif = srcic Then
					Output(i) = Source(srcif)
				Else
					Dim dist As Double = srci - CDbl(srcif)
					Dim valf As Integer = Source(srcif)
					Dim valc As Integer
					If srcic > Source.Length - 1 Then
						valc = 0
					Else
						valc = Source(srcic)
					End If
					Dim rng As Integer = valc - valf
					Dim rise As Integer = Math.Round(CDbl(rng) * dist)
					Output(i) = valf + rise
				End If
			Next
			Return Output
		End Function
		Public Function GetFreqFactor(ByVal Semitones As Integer) As Double
			Dim a As Double = 2 ^ (1.0 / 12.0)
			Return a ^ CDbl(Semitones)
		End Function
#End Region
	End Module
#Region "Classes"
	Public Class EnvelopeInfo
		Public Shared Function FromNspcBytes(ByVal Data As Byte()) As EnvelopeInfo
			Return Nothing
		End Function
	End Class
#End Region
#Region "Structures"
	Public Structure EnvelopeState
		Public Phase As EnvelopePhases
		Public Offset As Integer
	End Structure
#End Region
#Region "Enums"
	Public Enum EnvelopePhases
		Attack
		Decay
		Sustain
		Release
		Constant ' for when there is no adsr
		Finished ' after release ends (or when unlooped wave finishes)
	End Enum
#End Region
End Namespace
