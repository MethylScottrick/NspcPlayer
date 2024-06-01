#Region "Imports"
Imports System.IO
Imports System.Runtime.InteropServices
#End Region
Namespace Sound
	''' <summary>Audio processing unit</summary>
	<DebuggerDisplay("Inited:{IsInited} SpcLoaded:{IsSpcLoaded} SampleCounter:{SampleCounter}")> Public Class APU
		Implements IDisposable
#Region "Constants"
		Private Const MaxSeekValue As ULong = 64000000
		Private Const MaxSeekStep As UInt32 = 640000
		Public Const RELOAD_SPC_ON_RESTART As Boolean = True ' seems to be required to restart at beginning (though the simple test didn't need it???). replace with only setting regs/lower RAM.
#End Region
#Region "Events"
		Public Event UpdateDebugInfo()
#End Region
#Region "Properties"
		Private _IsDisposed As Boolean = False
		Public Property IsDisposed As Boolean
			Get
				Return _IsDisposed
			End Get
			Set(value As Boolean)
				_IsDisposed = value
			End Set
		End Property
		Private _IsInited As Boolean
		Public Property IsInited As Boolean
			Get
				Return _IsInited
			End Get
			Set(value As Boolean)
				_IsInited = value
				RaiseUpdateDebugInfo()
			End Set
		End Property
		Private Property SpcData As Byte()
		Private Property SpcDataPtr As IntPtr
		Private _SampleCounter As Long
		Public Property SampleCounter As Long
			Get
				Return _SampleCounter
			End Get
			Private Set(value As Long)
				_SampleCounter = value
				RaiseUpdateDebugInfo()
			End Set
		End Property
		Private _PrevSampleCounter As Long
		Public Property PrevSampleCounter As Long
			Get
				Return _PrevSampleCounter
			End Get
			Set(value As Long)
				_PrevSampleCounter = value
			End Set
		End Property
		Public ReadOnly Property IsSpcLoaded As Boolean
			Get
				Return (Not (SpcDataPtr = IntPtr.Zero))
			End Get
		End Property
#End Region
#Region "Constructors/Destructors"
		Public Sub New()
			IsInited = False
			SpcDataPtr = IntPtr.Zero
			Init()
		End Sub
#End Region
#Region "Methods"
		Public Sub Init()
			If IsInited Then Return
			ResetApu()
			ResetCounters()
			IsInited = True
		End Sub
		Public Sub UnInit()
			If Not IsInited Then Return
			If IsSpcLoaded Then FreeSPC()
			ResetApu()
			ResetCounters()
			IsInited = False
		End Sub
		Public Sub Reset()
			If Not IsInited Then Return
			UnInit()
			Init()
		End Sub
		Public Sub Restart()
			If Not IsInited Then Return
			ResetApu()
			If RELOAD_SPC_ON_RESTART Then ReloadSPC()
			ResetCounters()
		End Sub
		Public Sub ResetCounters()
			SampleCounter = 0
			PrevSampleCounter = 0
		End Sub
		Public Sub ResetApu()
			NativeMethods.ResetAPU(24) ' -1 for unchanged
		End Sub
		Public Sub LoadSPC(ByVal Path As String)
			LoadSPC(File.ReadAllBytes(Path))
		End Sub
		Public Sub LoadSPC(ByVal SpcData As Byte())
			If Not IsInited Then Return
			If IsSpcLoaded Then FreeSPC()
			Me.SpcData = SpcData
			SpcDataPtr = Marshal.AllocHGlobal(SpcData.Length)
			Marshal.Copy(SpcData, 0, SpcDataPtr, SpcData.Length)
			NativeMethods.LoadSPCFile(SpcDataPtr)
		End Sub
		Public Sub ReloadSPC()
			If Not IsInited Then Return
			If Me.SpcData Is Nothing Then Return
			LoadSPC(Me.SpcData)
		End Sub
		Public Sub FreeSPC()
			If Not IsSpcLoaded Then Return
			Marshal.FreeHGlobal(SpcDataPtr)
			SpcDataPtr = IntPtr.Zero
		End Sub
		Public Sub SeekAbsSamples(ByVal Samples As Long, Optional ByVal Fast As Boolean = False)
			If Not IsInited Then Return
			Samples = Math.Max(Math.Min(Samples, MaxSeekValue), 0)
			If Samples < SampleCounter Then
				Restart()
				If Samples <> 0 Then SeekSamples(Samples)
			ElseIf Samples > SampleCounter Then
				Dim DeltaSamples As Long = Samples - SampleCounter
				If DeltaSamples <> 0 Then SeekSamples(DeltaSamples)
			End If
			PrevSampleCounter = SampleCounter
			SampleCounter = Samples
		End Sub
		Public Sub SeekAbsSeconds(ByVal Seconds As Double, Optional ByVal Fast As Boolean = False)
			If Not IsInited Then Return
			Dim AbsSamples As Long = Seconds * 32000
			SeekAbsSamples(AbsSamples, Fast)
		End Sub
		Public Sub SeekSamples(ByVal Samples As Long, Optional ByVal Fast As Boolean = False)
			If Not IsInited Then Return
			If Samples = 0 Then Return
			If Samples < 0 Then
				SeekNegativeSamples(-Samples, Fast)
				Return
			End If
			Samples = Math.Min(Samples, MaxSeekValue)
			' if samples is greater than MaxSeekStep, break it up into multiple seek operations of MaxSeekStep each, then the remainder
			Dim SamplesToGo As Long = Samples
			While SamplesToGo >= MaxSeekStep
				NativeMethods.SeekAPU(MaxSeekStep, Fast)
				SamplesToGo -= MaxSeekStep
			End While
			If SamplesToGo > 0 Then
				NativeMethods.SeekAPU(SamplesToGo, Fast)
			End If
			PrevSampleCounter = SampleCounter
			SampleCounter += Samples
		End Sub
		Public Sub SeekSeconds(ByVal Seconds As Double, Optional ByVal Fast As Boolean = False)
			If Not IsInited Then Return
			If Seconds = 0 Then Return
			Dim Samples As Long = Seconds * 32000
			SeekSamples(Samples, Fast)
		End Sub
		Public Sub SeekNegativeSamples(ByVal Samples As Long, Optional ByVal Fast As Boolean = False)
			If Not IsInited Then Return
			If Samples = 0 Then Return
			Dim AbsSamples As Long = Math.Max(SampleCounter - Samples, 0)
			SeekAbsSamples(AbsSamples, Fast)
		End Sub
		Public Sub SeekNegativeSeconds(ByVal Seconds As Double, Optional ByVal Fast As Boolean = False)
			If Not IsInited Then Return
			If Seconds = 0 Then Return
			Dim Samples As Long = Seconds * 32000
			SeekNegativeSamples(Samples, Fast)
		End Sub
		Public Function GetOutput(ByVal NumSamples As Integer) As Byte()
			Dim AudioData((NumSamples * 4) - 1) As Byte
			Dim AudioPtr As IntPtr = Marshal.AllocHGlobal(NumSamples * 4)
			NativeMethods.EmuAPU(AudioPtr, NumSamples, 1)
			Marshal.Copy(AudioPtr, AudioData, 0, NumSamples * 4)
			Marshal.FreeHGlobal(AudioPtr)
			PrevSampleCounter = SampleCounter
			SampleCounter += NumSamples
			Return AudioData
		End Function
		Public Sub RaiseUpdateDebugInfo()
#If DEBUG Then
			RaiseEvent UpdateDebugInfo()
#End If
		End Sub
#End Region
#Region "IDisposable Support"
		Private disposedValue As Boolean ' To detect redundant calls
		' IDisposable
		Protected Overridable Sub Dispose(disposing As Boolean)
			If Not Me.disposedValue Then
				If disposing Then
					' TODO: dispose managed state (managed objects).
					If Me.IsInited Then Me.UnInit()
				End If
				' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
				' TODO: set large fields to null.
			End If
			Me.IsDisposed = True
			Me.disposedValue = True
		End Sub
		' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
		'Protected Overrides Sub Finalize()
		'    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
		'    Dispose(False)
		'    MyBase.Finalize()
		'End Sub
		' This code added by Visual Basic to correctly implement the disposable pattern.
		Public Sub Dispose() Implements IDisposable.Dispose
			' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
			Dispose(True)
			GC.SuppressFinalize(Me)
		End Sub
#End Region
	End Class
End Namespace
