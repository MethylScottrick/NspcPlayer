#Region "Imports"
Imports System.IO
Imports System.Runtime.InteropServices
#End Region
Namespace Sound
	''' <summary>Audio processing unit</summary>
	<DebuggerDisplay("Loaded:{IsLoaded} SpcLoaded:{IsSpcLoaded} SampleCounter:{SampleCounter}")> Public Class APU
#Region "Constants"
		Private Const MaxSeekValue As ULong = 64000000
		Private Const MaxSeekStep As UInt32 = 640000
		Public Const RELOAD_SPC_ON_RESTART As Boolean = True ' seems to be required to restart at beginning (though the simple test didn't need it???). replace with only setting regs/lower RAM.
#End Region
#Region "Properties"
		Public Property IsLoaded As Boolean
		Private Property SpcData As Byte()
		Private Property SpcDataPtr As IntPtr
		Private _SampleCounter As Long
		Public Property PrevSampleCounter As Long
		Public ReadOnly Property IsSpcLoaded As Boolean
			Get
				Return (Not (SpcDataPtr = IntPtr.Zero))
			End Get
		End Property
		Public ReadOnly Property SampleCounter As Long
			Get
				Return _SampleCounter
			End Get
		End Property
#End Region
#Region "Constructors/Destructors"
		Public Sub New()
			IsLoaded = False
			_SampleCounter = 0
			PrevSampleCounter = 0
			SpcDataPtr = IntPtr.Zero
			Load()
		End Sub
#End Region
#Region "Methods"
		Public Sub Load()
			If IsLoaded Then Return
			ResetApu()
			IsLoaded = True
		End Sub
		Public Sub Unload()
			If Not IsLoaded Then Return
			If IsSpcLoaded Then FreeSPC()
			ResetApu()
			_SampleCounter = 0
			PrevSampleCounter = 0
			IsLoaded = False
		End Sub
		Public Sub Reset()
			If Not IsLoaded Then Return
			Unload()
			Load()
		End Sub
		Public Sub LoadSPC(ByVal Path As String)
			LoadSPC(File.ReadAllBytes(Path))
		End Sub
		Public Sub LoadSPC(ByVal SpcData As Byte())
			If Not IsLoaded Then Return
			If IsSpcLoaded Then FreeSPC()
			Me.SpcData = SpcData
			SpcDataPtr = Marshal.AllocHGlobal(SpcData.Length)
			Marshal.Copy(SpcData, 0, SpcDataPtr, SpcData.Length)
			NativeMethods.LoadSPCFile(SpcDataPtr)
		End Sub
		Public Sub ReloadSPC()
			If Not IsLoaded Then Return
			If Me.SpcData Is Nothing Then Return
			LoadSPC(Me.SpcData)
		End Sub
		Public Sub FreeSPC()
			If Not IsSpcLoaded Then Return
			Marshal.FreeHGlobal(SpcDataPtr)
			SpcDataPtr = IntPtr.Zero
		End Sub
		Public Sub SeekAbsSamples(ByVal Samples As Long, Optional ByVal Fast As Boolean = False)
			If Not IsLoaded Then Return
			Samples = Math.Max(Samples, 0)
			If Samples < 0 Then Return
			If Samples < SampleCounter Then
				Restart()
				If Samples <> 0 Then SeekSamples(Samples)
			ElseIf Samples > SampleCounter Then
				Dim DeltaSamples As Long = Samples - SampleCounter
				If DeltaSamples <> 0 Then SeekSamples(DeltaSamples)
			End If
		End Sub
		Public Sub SeekAbsSeconds(ByVal Seconds As Double, Optional ByVal Fast As Boolean = False)
			If Not IsLoaded Then Return
			Seconds = Math.Max(Seconds, 0.0)
			Dim AbsSamples As Long = Seconds * 32000
			If AbsSamples < SampleCounter Then
				Restart()
				If Seconds <> 0 Then SeekSeconds(Seconds)
			ElseIf AbsSamples > SampleCounter Then
				Dim DeltaSamples As Long = AbsSamples - SampleCounter
				If DeltaSamples <> 0 Then SeekSamples(DeltaSamples)
			End If
		End Sub
		Public Sub SeekSamples(ByVal Samples As Long, Optional ByVal Fast As Boolean = False)
			If Not IsLoaded Then Return
			If Samples = 0 Then
				Return
			End If
			Dim FastVal As Byte
			If Fast Then
				FastVal = 1
			Else
				FastVal = 0
			End If
			Dim TimeVal As Long = Samples * 2
			Dim TimeVal2 As Long = Math.Max(Math.Min(Math.Floor(TimeVal), MaxSeekValue), 0)
			Dim SampleDelta As Long = TimeVal2 / 2
			PrevSampleCounter = _SampleCounter
			_SampleCounter += SampleDelta
			If TimeVal2 <= 0 Then Return
			Do
				If TimeVal2 > MaxSeekStep Then
					NativeMethods.SeekAPU(MaxSeekStep, FastVal)
					TimeVal2 -= MaxSeekStep
				Else
					NativeMethods.SeekAPU(TimeVal2, FastVal)
					Exit Do
				End If
			Loop
		End Sub
		Public Sub SeekSeconds(ByVal Seconds As Double, Optional ByVal Fast As Boolean = False)
			If Not IsLoaded Then Return
			If Seconds = 0 Then
				Return
			End If
			If Seconds < 0 Then
				SeekNegativeSeconds(-Seconds, Fast)
			End If
			Dim FastVal As Byte
			If Fast Then
				FastVal = 1
			Else
				FastVal = 0
			End If
			Dim TimeVal As Double = Seconds * CDbl(64000)
			Dim TimeVal2 As Long = Math.Max(Math.Min(Math.Floor(TimeVal), MaxSeekValue), 0)
			Dim SampleDelta As Long = TimeVal2 / 2
			PrevSampleCounter = _SampleCounter
			_SampleCounter += SampleDelta
			If TimeVal2 <= 0 Then Return
			Do
				If TimeVal2 > MaxSeekStep Then
					NativeMethods.SeekAPU(MaxSeekStep, FastVal)
					TimeVal2 -= MaxSeekStep
				Else
					NativeMethods.SeekAPU(TimeVal2, FastVal)
					Exit Do
				End If
			Loop
		End Sub
		Public Sub SeekNegativeSamples(ByVal Samples As Long, Optional ByVal Fast As Boolean = False)
			If Not IsLoaded Then Return
			If Samples = 0 Then
				Return
			End If
			If Samples < 0 Then Return
			Dim AbsSamples As Double = CDbl(SampleCounter) / CDbl(32000)
			Dim NewSamples As Double = Math.Max(SampleCounter - Samples, 0)
			SeekAbsSamples(NewSamples, Fast)
		End Sub
		Public Sub SeekNegativeSeconds(ByVal Seconds As Double, Optional ByVal Fast As Boolean = False)
			If Not IsLoaded Then Return
			If Seconds = 0 Then
				Return
			End If
			If Seconds < 0 Then Return
			Dim AbsSeconds As Double = CDbl(SampleCounter) / CDbl(32000)
			Dim NewSeconds As Double = Math.Max(AbsSeconds - Seconds, 0)
			SeekAbsSeconds(NewSeconds, Fast)
		End Sub
		Public Sub Restart()
			If Not IsLoaded Then Return
			ResetApu()
			If RELOAD_SPC_ON_RESTART Then
				ReloadSPC()
			End If
			ResetCounters()
		End Sub
		Public Sub ResetCounters()
			_SampleCounter = 0
			PrevSampleCounter = 0
		End Sub
		Public Sub ResetApu()
			NativeMethods.ResetAPU(24) ' -1 for unchanged
		End Sub
		Public Function GetOutput(ByVal NumSamples As Integer) As Byte()
			Dim AudioData((NumSamples * 4) - 1) As Byte
			Dim AudioPtr As IntPtr = Marshal.AllocHGlobal(NumSamples * 4)
			NativeMethods.EmuAPU(AudioPtr, NumSamples, 1)
			Marshal.Copy(AudioPtr, AudioData, 0, NumSamples * 4)
			Marshal.FreeHGlobal(AudioPtr)
			PrevSampleCounter = _SampleCounter
			_SampleCounter += NumSamples
			Return AudioData
		End Function
#End Region
	End Class
End Namespace
