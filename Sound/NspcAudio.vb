#Region "Imports"
Imports SlimDX
Imports SlimDX.DirectSound
Imports SlimDX.Multimedia
Imports System.IO
Imports System.Threading
#End Region
Namespace Sound
	''' <summary>Singleton class that handles audio playback (directly) and SNESAPU.DLL calls (indirectly)</summary>
	Public Class NspcAudio
		Implements IDisposable
#Region "Events"
		Public Event BufferHit(sender As Object, e As BufferHitArgs)
		Public Event LoopCounterChanged(sender As Object, e As LoopCounterArgs)
		Public Event PlaybackStarted()
		Public Event PlaybackEnded(sender As Object, e As EventArgs)
		Public Event Loaded()
		Public Event Unloaded()
		Public Event SpcLoaded()
		Public Event Resetted()
		Public Event BufferStopped()
		Public Event PlaybackThreadStarted()
		Public Event PlaybackThreadStopped()
		Public Event StateChanged()
		Public Event SeekQueued(ByVal SeekType As SeekCommand.SeekTypes, ByVal SeekValue As Object)
		Public Event Seeked(ByVal SeekType As SeekCommand.SeekTypes, ByVal SeekValue As Object)
#End Region
#Region "Constants"
		Public Const NUM_CHANNELS As Integer = 2
		Public Const BITS_PER_SAMPLE As Integer = 16
		Public Const SAMPLES_PER_SECOND As Integer = SAMPLE_RATE
		Public Const BYTES_PER_SAMPLE As Integer = (NUM_CHANNELS * BITS_PER_SAMPLE) / 8
		Public Const BUFFER_MILLISECONDS As Integer = 100
		Public Const BUFFER_SAMPLES As Integer = (SAMPLE_RATE * BUFFER_MILLISECONDS) / 1000
		Public Const BUFFER_BYTES As Integer = BUFFER_SAMPLES * BYTES_PER_SAMPLE
		Public Const BUFFER_BLOCKS As Integer = 2
		Public Const BLOCK_SAMPLES As Integer = BUFFER_SAMPLES / BUFFER_BLOCKS
		Public Const BLOCK_BYTES As Integer = BLOCK_SAMPLES * BYTES_PER_SAMPLE
		Public Const INIT_VOLUME As Integer = -200
		Public Const THREAD_TIMEOUT As Integer = 500
		Public Const ENABLE_AMPLIFY As Boolean = False
		Public Const DEFAULT_AMPLIFY As Single = 5
		Public Const THREAD_PRIORITY As Single = ThreadPriority.AboveNormal
#End Region
#Region "Fields"
		Public BufferDesc As SoundBufferDescription
		Private _BlockSilence As Short() = Nothing
		Public ReadOnly Property BlockSilence As Short()
			Get
				If _BlockSilence Is Nothing Then
					Dim Buffer(BLOCK_SAMPLES - 1) As Short
					For i As Integer = 0 To BLOCK_SAMPLES - 1
						Buffer(i) = 0
					Next
					_BlockSilence = Buffer
				End If
				Return _BlockSilence
			End Get
		End Property
		Public _HalfSilence As Short() = Nothing
		Public ReadOnly Property HalfSilence As Short()
			Get
				If _HalfSilence Is Nothing Then
					Dim Buffer((BUFFER_SAMPLES / 2) - 1) As Short
					For i As Integer = 0 To (BUFFER_SAMPLES / 2) - 1
						Buffer(i) = 0
					Next
					_HalfSilence = Buffer
				End If
				Return _HalfSilence
			End Get
		End Property
		Public _FullSilence As Short() = Nothing
		Public ReadOnly Property FullSilence As Short()
			Get
				If _FullSilence Is Nothing Then
					Dim Buffer(BUFFER_SAMPLES - 1) As Short
					For i As Integer = 0 To BUFFER_SAMPLES - 1
						Buffer(i) = 0
					Next
					_FullSilence = Buffer
				End If
				Return _FullSilence
			End Get
		End Property
		Public _ZeroedBuffer As Byte() = Nothing
		Public ReadOnly Property ZeroedBuffer As Byte()
			Get
				If _ZeroedBuffer Is Nothing Then
					Dim Buffer(BUFFER_BYTES - 1) As Byte
					For i As Integer = 0 To BUFFER_BYTES - 1
						Buffer(i) = 0
					Next
					_ZeroedBuffer = Buffer
				End If
				Return _ZeroedBuffer
			End Get
		End Property
		Public StopPlaybackThreadHandle As AutoResetEvent = New AutoResetEvent(False)
		Public PlaybackThreadExitedHandle As AutoResetEvent = New AutoResetEvent(False)
		''' <summary>Set to true on entry of StopBuffer(). Set to false on start of playback.</summary>
		Private StopBufferCalled As Boolean = False
#End Region
#Region "Properties"
		Public Property IsLoaded As Boolean
		Public Property APU As APU
		Public Property Timestamp As Long
		Private _State As RendererStates
		Public Property State As RendererStates
			Get
				Return _State
			End Get
			Set(value As RendererStates)
				If _State <> value Then
					_State = value
					RaiseEvent StateChanged()
				End If
			End Set
		End Property
		Private WithEvents _Device As DirectSound
		Public Property Device As DirectSound
			Get
				Return _Device
			End Get
			Set(value As DirectSound)
				_Device = value
			End Set
		End Property
		Private WithEvents _Buffer As SecondarySoundBuffer
		Public Property Buffer As SoundBuffer
			Get
				Return _Buffer
			End Get
			Set(value As SoundBuffer)
				_Buffer = value
			End Set
		End Property
		Private _Format As WaveFormat
		Public Property Format As WaveFormat
			Get
				Return _Format
			End Get
			Set(value As WaveFormat)
				_Format = value
			End Set
		End Property
		Public Property NotifyPositionList As List(Of NotificationPosition)
		Public Property BufferHitEvents As AutoResetEvent()
		Public Property PlayPosition As Integer
			Get
				If _Buffer Is Nothing Then
					Return 0
				Else
					Return _Buffer.CurrentPlayPosition
				End If
			End Get
			Set(value As Integer)
				If _Buffer IsNot Nothing Then
					_Buffer.CurrentPlayPosition = value
				End If
			End Set
		End Property
		Private _LoopCounter As Integer
		Public Property LoopCounter As Integer
			Get
				Return _LoopCounter
			End Get
			Set(value As Integer)
				_LoopCounter = value
				RaiseEvent LoopCounterChanged(Me, New LoopCounterArgs(_LoopCounter))
			End Set
		End Property
		Private WithEvents _PlaybackThread As Thread
		Public Property PlaybackThread As Thread
			Get
				Return _PlaybackThread
			End Get
			Set(value As Thread)
				_PlaybackThread = value
			End Set
		End Property
		Private Property QueuedSeekCommand As SeekCommand
		Private Property QueuedSeekLock As Object
		''' <summary>Position most recent block was written to</summary>
		Public Property LastWritePosition As Integer
		Public Property PrevLastWritePosition As Integer
		Public Property LastPlayPosition As Integer
		Public Property WaveData As MemoryStream
		Public Property PlaybackMode As PlaybackModes
		Public Property StopOnNextHit As Boolean
		Private _StopPositionEvent As AutoResetEvent
		Public Property StopPositionEvent As AutoResetEvent
			Get
				If _StopPositionEvent Is Nothing Then _StopPositionEvent = New AutoResetEvent(False)
				Return _StopPositionEvent
			End Get
			Set(value As AutoResetEvent)
				_StopPositionEvent = value
			End Set
		End Property
		Public ReadOnly Property WritePosition As Integer
			Get
				If _Buffer Is Nothing Then
					Return 0
				Else
					Return _Buffer.CurrentWritePosition
				End If
			End Get
		End Property
		Public ReadOnly Property CurrentTimestamp As Long
			Get
				If State <> RendererStates.Playing Then
					Return 0
				Else
					Return (CLng(LoopCounter) * CLng(BUFFER_SAMPLES)) + _Buffer.CurrentPlayPosition
				End If
			End Get
		End Property
		Public ReadOnly Property IsBufferStopped As Boolean
			Get
				'If (Buffer Is Nothing) Then Return True
				'Return ((Buffer.Status Or BufferStatus.Playing) > 0)
				Return ((Buffer Is Nothing) OrElse ((Buffer.Status Or BufferStatus.Playing) > 0))
			End Get
		End Property
		Public ReadOnly Property IsApuLoaded As Boolean
			Get
				Return (APU IsNot Nothing AndAlso APU.IsLoaded)
			End Get
		End Property
		Public ReadOnly Property IsReadyForPlayback As Boolean
			Get
				Return (IsLoaded AndAlso IsApuLoaded AndAlso APU.IsSpcLoaded)
			End Get
		End Property
		Public ReadOnly Property SampleCounter As Long
			Get
				If IsApuLoaded Then
					Return APU.SampleCounter
				Else
					Return 0
				End If
			End Get
		End Property
		Public ReadOnly Property PrevSampleCounter As Long
			Get
				If IsApuLoaded Then
					Return APU.PrevSampleCounter
				Else
					Return 0
				End If
			End Get
		End Property
		Public ReadOnly Property CurrentPlaybackSample As Long
			Get
				Dim PlaySample As Integer = PlayPosition / BYTES_PER_SAMPLE
				Dim LastPlaySample As Integer = LastPlayPosition / BYTES_PER_SAMPLE
				If PlaySample > LastPlaySample Then
					Return Math.Max((PrevSampleCounter + (PlaySample - LastPlaySample)), 0)
				ElseIf PlaySample < LastPlaySample Then
					Return Math.Max(((PrevSampleCounter + BLOCK_SAMPLES + LastPlaySample) - PlaySample), 0)	' SHOULD THIS BE BUFFER_SAMPLES???
				Else
					Return Math.Max(PrevSampleCounter, 0)
				End If
			End Get
		End Property
		Public ReadOnly Property CurrentPlaybackSeconds As Double
			Get
				Return (CDbl(CurrentPlaybackSample) / CDbl(SAMPLES_PER_SECOND))
			End Get
		End Property
		Public ReadOnly Property SamplesPlayedFromCurrentBlock As Integer
			Get
				Dim PlaySamples As Integer = BlockBytesToSamples(PlayPosition)
				Dim WriteSamples As Integer = BlockBytesToSamples(LastWritePosition)
				Dim PrevWriteSamples As Integer = BlockBytesToSamples(PrevLastWritePosition)
				Dim SamplesFromLastWrite As Integer = BufferSamplesFromLastWrite(PlaySamples, WriteSamples)
				Dim SamplesFromPrevLastWrite As Integer = BufferSamplesFromLastWrite(PlaySamples, PrevWriteSamples)
				Return Math.Min(SamplesFromLastWrite, SamplesFromPrevLastWrite)
			End Get
		End Property
		Public ReadOnly Property BytesPlayedFromCurrentBlock As Integer
			Get
				Dim PlayBytes As Integer = PlayPosition
				Dim WriteBytes As Integer = LastWritePosition
				Dim PrevWriteBytes As Integer = PrevLastWritePosition
				Dim BytesFromLastWrite As Integer = BufferBytesFromLastWrite(PlayBytes, WriteBytes)
				Dim BytesFromPrevLastWrite As Integer = BufferBytesFromLastWrite(PlayBytes, PrevWriteBytes)
				Return Math.Min(BytesFromLastWrite, BytesFromPrevLastWrite)
			End Get
		End Property
		''' <summary>Gets the distance, in samples, *after* the play position is from last write position, with the buffer being circular.</summary>
		Public ReadOnly Property BufferSamplesFromLastWrite As Integer
			Get
				Dim PlaySamples As Integer = BlockBytesToSamples(PlayPosition)
				Dim WriteSamples As Integer = BlockBytesToSamples(LastWritePosition)
				If PlaySamples > WriteSamples Then
					Return PlaySamples - WriteSamples
				ElseIf PlaySamples < WriteSamples Then
					Return (BUFFER_SAMPLES + PlaySamples) - WriteSamples
				Else
					Return 0
				End If
			End Get
		End Property
		''' <summary>Gets the distance, in bytes, *after* the play position is from last write position, with the buffer being circular.</summary>
		Public ReadOnly Property BufferBytesFromLastWrite As Integer
			Get
				Dim PlayBytes As Integer = PlayPosition
				Dim WriteBytes As Integer = LastWritePosition
				If PlayBytes > WriteBytes Then
					Return PlayBytes - WriteBytes
				ElseIf PlayBytes < WriteBytes Then
					Return (BUFFER_BYTES + PlayBytes) - WriteBytes
				Else
					Return 0
				End If
			End Get
		End Property
		Public ReadOnly Property IsPlayingLatestBlock As Boolean
			Get
				Return (BufferSamplesFromLastWrite < BLOCK_SAMPLES)
			End Get
		End Property
		' so that calls to multiple properties don't reflect different position values
		Public ReadOnly Property SamplesPlayedFromCurrentBlock(ByVal PlaySamples As Integer, ByVal WriteSamples As Integer, ByVal PrevWriteSamples As Integer) As Integer
			Get
				Dim SamplesFromLastWrite As Integer = BufferSamplesFromLastWrite(PlaySamples, WriteSamples)
				Dim SamplesFromPrevLastWrite As Integer = BufferSamplesFromLastWrite(PlaySamples, PrevWriteSamples)
				Return Math.Min(SamplesFromLastWrite, SamplesFromPrevLastWrite)
			End Get
		End Property
		' so that calls to multiple properties don't reflect different position values
		Public ReadOnly Property BytesPlayedFromCurrentBlock(ByVal PlayBytes As Integer, ByVal WriteBytes As Integer, ByVal PrevWriteBytes As Integer) As Integer
			Get
				Dim BytesFromLastWrite As Integer = BufferBytesFromLastWrite(PlayBytes, WriteBytes)
				Dim BytesFromPrevLastWrite As Integer = BufferBytesFromLastWrite(PlayBytes, PrevWriteBytes)
				Return Math.Min(BytesFromLastWrite, BytesFromPrevLastWrite)
			End Get
		End Property
		' so that calls to multiple properties don't reflect different position values
		Public ReadOnly Property BufferSamplesFromLastWrite(ByVal PlaySamples As Integer, ByVal WriteSamples As Integer) As Integer
			Get
				If PlaySamples > WriteSamples Then
					Return PlaySamples - WriteSamples
				ElseIf PlaySamples < WriteSamples Then
					Return (BUFFER_SAMPLES + PlaySamples) - WriteSamples
				Else
					Return 0
				End If
			End Get
		End Property
		' so that calls to multiple properties don't reflect different position values
		Public ReadOnly Property BufferBytesFromLastWrite(ByVal PlayBytes As Integer, ByVal WriteBytes As Integer) As Integer
			Get
				If PlayBytes > WriteBytes Then
					Return PlayBytes - WriteBytes
				ElseIf PlayBytes < WriteBytes Then
					Return (BUFFER_BYTES + PlayBytes) - WriteBytes
				Else
					Return 0
				End If
			End Get
		End Property
		' so that calls to multiple properties don't reflect different position values
		Public ReadOnly Property IsPlayingLatestBlockBySamples(ByVal PlaySamples As Integer, ByVal WriteSamples As Integer, ByVal PrevWriteSamples As Integer) As Boolean
			Get
				Return (BufferSamplesFromLastWrite(PlaySamples, WriteSamples) < BLOCK_SAMPLES)
			End Get
		End Property
		Public ReadOnly Property IsPlayingLatestBlockByBytes(ByVal PlayBytes As Integer, ByVal WriteBytes As Integer, ByVal PrevWriteBytes As Integer) As Boolean
			Get
				Return (BufferBytesFromLastWrite(PlayBytes, WriteBytes) < BLOCK_BYTES)
			End Get
		End Property
		''' <summary>Audio samples, time-wise, that have been processed by the APU and played. Normally sum of all completed blocks plus number of samples played in current block.</summary>
		Public ReadOnly Property PlayedSampleCounter As Integer
			Get
				' use above properties instead of other properties??
				Dim Samples As Integer = SampleCounter
				Dim PrevSamples As Integer = PrevSampleCounter
				Dim PlaySamples As Integer = BlockBytesToSamples(PlayPosition)
				Dim WriteSamples As Integer = BlockBytesToSamples(LastWritePosition)
				Dim PrevWriteSamples As Integer = BlockBytesToSamples(PrevLastWritePosition)
				Dim BlockSamples As Integer = SamplesPlayedFromCurrentBlock(PlaySamples, WriteSamples, PrevWriteSamples)
				Dim LatestBlock As Boolean = IsPlayingLatestBlockBySamples(PlaySamples, WriteSamples, PrevWriteSamples)
				If LatestBlock Then
					Return Samples + BlockSamples
				Else
					Return PrevSamples + BlockSamples
				End If
			End Get
		End Property
		Public ReadOnly Property PlayedSecondsCounter As Double
			Get
				Return (CDbl(PlayedSampleCounter) / CDbl(SAMPLES_PER_SECOND))
			End Get
		End Property
#End Region
#Region "Constructors/Destructors"
		Public Sub New()
			IsLoaded = False
			State = RendererStates.Uninitialized
			ResetPositions()
			PlaybackMode = PlaybackModes.None
			APU = Nothing
			Device = Nothing
			Buffer = Nothing
			BufferDesc = Nothing
			Format = Nothing
			NotifyPositionList = Nothing
			PlaybackThread = Nothing
			BufferHitEvents = Nothing
			StopPositionEvent = New AutoResetEvent(False)
			StopOnNextHit = False
			QueuedSeekLock = New Object()
			If (BUFFER_BLOCKS Mod 2) Then Throw New Exception("Number of buffer blocks must be even")
			If (BUFFER_SAMPLES Mod BUFFER_BLOCKS) Then Throw New Exception("Buffer size must be multiple of " & BUFFER_BLOCKS.ToString())
		End Sub
#End Region
#Region "Methods"
		Public Sub Load()
			Load(Nothing)
		End Sub
		Public Sub Load(ByVal Owner As Control)
			ResetPositions()
			If IsLoaded Then Return
			Dim res As Result
			If State <> RendererStates.Uninitialized Then Throw New Exception("Already initialized")
			State = RendererStates.Initializing
			Try
				' Device
				Device = New DirectSound()
				If Owner Is Nothing Then
					res = Device.SetCooperativeLevel(Process.GetCurrentProcess().MainWindowHandle, CooperativeLevel.Normal)
				Else
					res = Device.SetCooperativeLevel(Owner.Handle, CooperativeLevel.Normal)
				End If
				' Wave Format
				Format = New WaveFormat()
				With Format()
					.BitsPerSample = BITS_PER_SAMPLE
					.Channels = NUM_CHANNELS
					.SamplesPerSecond = SAMPLE_RATE
					.BlockAlignment = CShort((.Channels * .BitsPerSample) / 8)
					.AverageBytesPerSecond = .BlockAlignment * .SamplesPerSecond
					.FormatTag = WaveFormatTag.Pcm
				End With
				' Buffer Description
				BufferDesc = New SoundBufferDescription()
				With BufferDesc
					.Format = Format()
					.SizeInBytes = BUFFER_SAMPLES * Format.BlockAlignment
					If .SizeInBytes <> BUFFER_BYTES Then Throw New Exception("Buffer size mismatch")
					.Flags = BufferFlags.ControlPan Or BufferFlags.ControlPositionNotify Or BufferFlags.ControlVolume Or BufferFlags.GetCurrentPosition2 Or BufferFlags.GlobalFocus Or BufferFlags.Defer
				End With
				' Buffer
				Buffer = New SecondarySoundBuffer(Device, BufferDesc)
				StopPositionEvent.Reset()
				NotifyPositionList = New List(Of NotificationPosition)()
				ReDim _BufferHitEvents(BUFFER_BLOCKS - 1)
				For i As Integer = 0 To BUFFER_BLOCKS - 1
					_BufferHitEvents(i) = New AutoResetEvent(False)
					Dim NotifyPosition As New NotificationPosition
					NotifyPosition.Event = _BufferHitEvents(i)
					NotifyPosition.Offset = i * BLOCK_SAMPLES * BYTES_PER_SAMPLE
					NotifyPositionList.Add(NotifyPosition)
				Next
				Buffer.SetNotificationPositions(NotifyPositionList.ToArray())
				StopOnNextHit = False
				SyncLock QueuedSeekLock
					QueuedSeekCommand = Nothing
				End SyncLock
				State = RendererStates.Stopped
			Catch ex As Exception
				Me.Dispose()
				Throw ex
			End Try
			APU = New APU()
			IsLoaded = True
			RaiseEvent Loaded()
		End Sub
		Public Sub LoadCheck()
			If Not IsLoaded Then
				Load()
			End If
		End Sub
		Public Sub LoadSPC(ByVal Path As String)
			LoadCheck()
			If Not IsLoaded Then Return
			If Not IsApuLoaded Then Return
			APU.LoadSPC(Path)
			RaiseEvent SpcLoaded()
		End Sub
		Public Sub LoadSPC(ByVal SpcData As Byte())
			LoadCheck()
			If Not IsLoaded Then Return
			If Not IsApuLoaded Then Return
			APU.LoadSPC(SpcData)
			RaiseEvent SpcLoaded()
		End Sub
		Public Sub Unload()
			If Not IsLoaded Then Return
			[Stop]()
			Buffer.Dispose()
			Device.Dispose()
			Timestamp = 0
			LoopCounter = 0
			APU.Unload()
			APU = Nothing
			Device = Nothing
			Buffer = Nothing
			BufferDesc = Nothing
			Format = Nothing
			NotifyPositionList = Nothing
			PlaybackThread = Nothing
			BufferHitEvents = Nothing
			StopPositionEvent.Reset()
			StopOnNextHit = False
			SyncLock QueuedSeekLock
				QueuedSeekCommand = Nothing
			End SyncLock
			State = RendererStates.Uninitialized
			IsLoaded = False
			RaiseEvent Unloaded()
		End Sub
		Public Sub Reset()
			Reset(Nothing)
		End Sub
		Public Sub Reset(ByVal Owner As Control)
			If Not IsLoaded Then Return
			Unload()
			Load(Owner)
			RaiseEvent Resetted()
		End Sub
		Public Sub PlayApu()
			LoadCheck()
			If Not IsReadyForPlayback Then Return
			If State = RendererStates.Playing Then Me.Stop() ' does this break things??
			If State = RendererStates.Uninitialized Then Throw New Exception("Not initialized")
			If State = RendererStates.Initializing Then Throw New Exception("Not done initializing")
			If State = RendererStates.Stopped Then
				StopBufferCalled = False
				PlaybackMode = PlaybackModes.SnesApu
				State = RendererStates.Starting
				StopOnNextHit = False
				PlaybackThread = New Thread(AddressOf PlaybackThreadProc)
				PlaybackThread.Name = "AudioPlaybackThread"
				PlaybackThread.Priority = THREAD_PRIORITY
				StopPlaybackThreadHandle.Reset()
				PlaybackThreadExitedHandle.Reset()
				State = RendererStates.Playing
				HandleQueuedSeekCommand() ' TODO: TEST THIS!
				PlaybackThread.Start()
			End If
			RaiseEvent PlaybackStarted()
		End Sub
		Private Sub PlaybackThreadProc()
			Try
				RaiseEvent PlaybackThreadStarted()
				Dim res As Result
				PlaybackThreadExitedHandle.Reset()
				PrevLastWritePosition = LastWritePosition
				LastWritePosition = WritePosition
				LastPlayPosition = PlayPosition
				DoBufferWrite()
				res = Buffer.Play(0, PlayFlags.Looping)
				Do
					If Buffer.Status Or BufferStatus.BufferLost Then Buffer.Restore()
					Dim EventList As New List(Of EventWaitHandle)()
					EventList.AddRange({StopPlaybackThreadHandle})
					Dim FirstBufferHitIndex As Integer = EventList.Count
					EventList.AddRange(BufferHitEvents)
					Dim LastBufferHitIndex As Integer = EventList.Count - 1
					Dim EventHit As Integer = EventWaitHandle.WaitAny(EventList.ToArray())
					If EventHit = 0 Then ' Stop Playback Thread
						Exit Do
					ElseIf EventHit = WaitHandle.WaitTimeout Then ' timed out
						Continue Do
					ElseIf EventHit >= FirstBufferHitIndex And EventHit <= LastBufferHitIndex Then
						Dim BufferHitIndex As Integer = EventHit - FirstBufferHitIndex
						RaiseEvent BufferHit(Me, New BufferHitArgs(BufferHitIndex))
					Else
					End If
					If State <> RendererStates.Playing Then Exit Do
				Loop
				StopBuffer()
				PlaybackThreadExitedHandle.Set()
			Finally
				RaiseEvent PlaybackThreadStopped()
			End Try
		End Sub
		Public Sub [Stop]()
			If State = RendererStates.Playing Then
				State = RendererStates.Stopping
				'PlaybackMode = PlaybackModes.None
				StopBuffer()
				EndPlaybackThread()
				ResetPositions()
				APU.ResetCounters()
				State = RendererStates.Stopped
				PlaybackMode = PlaybackModes.None
				RaiseEvent PlaybackEnded(Me, EventArgs.Empty)
			End If
		End Sub
		Private Sub StopBuffer()
			If StopBufferCalled Then Return
			StopBufferCalled = True
			If State = RendererStates.Playing Or State = RendererStates.Stopping Then
				SyncLock QueuedSeekLock
					QueuedSeekCommand = Nothing
				End SyncLock
				Dim res As Result
				res = Buffer.Stop()
				If Not res.IsSuccess Then
					Throw New Exception(res.Description)
				End If
				WaitForTrue(IsBufferStopped, 3000, 50)
				WriteSilenceAll()
				RaiseEvent BufferStopped()
			End If
		End Sub
		Public Sub EndPlaybackThread()
			If PlaybackThread IsNot Nothing AndAlso PlaybackThread.IsAlive Then
				StopPlaybackThreadHandle.Set()
				If Not PlaybackThread.Join(THREAD_TIMEOUT) Then
					PlaybackThread.Abort()
					If Not PlaybackThread.Join(THREAD_TIMEOUT) Then
						Throw New TimeoutException("Timed out waiting for playback thread to be aborted.")
					End If
				End If
			End If
			PlaybackThread = Nothing
		End Sub
		Public Shared Function GetSilence(ByVal Length As Integer) As Short()
			Dim Silence(Length - 1) As Short
			For i As Integer = 0 To Length - 1
				Silence(i) = 0
			Next
			Return Silence
		End Function
		Public Sub WriteSilence(ByVal Length As Integer, ByVal Offset As Integer)
			If State = RendererStates.Uninitialized Then
				Return
			ElseIf State = RendererStates.Initializing Then
				Return
			ElseIf State = RendererStates.Stopping Then
				'Return
			Else
				Dim Silence(Length - 1) As Short
				For i As Integer = 0 To Length - 1
					Silence(i) = 0
				Next
				Buffer.Write(Silence, 0, Silence.Length, Offset, LockFlags.None)
			End If
		End Sub
		Public Sub WriteSilenceAll()
			If State = RendererStates.Uninitialized Then
				Return
			ElseIf State = RendererStates.Initializing Then
				Return
			ElseIf State = RendererStates.Stopping Then
				'Return
			Else
				Dim res As Result
				res = Buffer.Write(ZeroedBuffer, 0, BUFFER_BYTES, 0, LockFlags.EntireBuffer)
				If Not res.IsSuccess Then Throw New Exception(res.Description)
			End If
		End Sub
		Public Sub MarkBufferStart()
			If State = RendererStates.Playing Then
				Dim Data As Short() = {Short.MaxValue, Short.MaxValue, Short.MinValue, Short.MinValue}
				Buffer.Write(Data, 0, Data.Length, 0, LockFlags.None)
			End If
		End Sub
		Public Function GetWritePositionForNotifyIndex(ByVal Index As Integer) As Integer
			Dim WriteIndex As Integer
			Dim HalfBlocks As Integer = BUFFER_BLOCKS / 2
			If Index >= HalfBlocks Then
				WriteIndex = Index - HalfBlocks
			Else
				WriteIndex = Index + HalfBlocks
			End If
			Return WriteIndex * BLOCK_BYTES
		End Function
		Public Sub SeekSeconds(ByVal Seconds As Double)
			If Not IsReadyForPlayback Then Return
			SyncLock QueuedSeekLock
				QueuedSeekCommand = New SeekCommand(SeekCommand.SeekTypes.RelSeconds, Seconds)
			End SyncLock
			RaiseEvent SeekQueued(SeekCommand.SeekTypes.RelSeconds, Seconds)
		End Sub
		Public Sub SeekSamples(ByVal Samples As Long)
			If Not IsReadyForPlayback Then Return
			SyncLock QueuedSeekLock
				QueuedSeekCommand = New SeekCommand(SeekCommand.SeekTypes.RelSamples, Samples)
			End SyncLock
			RaiseEvent SeekQueued(SeekCommand.SeekTypes.RelSamples, Samples)
		End Sub
		Public Sub SeekAbsSeconds(ByVal Seconds As Double)
			If Not IsReadyForPlayback Then Return
			SyncLock QueuedSeekLock
				QueuedSeekCommand = New SeekCommand(SeekCommand.SeekTypes.AbsSeconds, Seconds)
			End SyncLock
			RaiseEvent SeekQueued(SeekCommand.SeekTypes.AbsSeconds, Seconds)
		End Sub
		Public Sub SeekAbsSamples(ByVal Samples As Long)
			If Not IsReadyForPlayback Then Return
			SyncLock QueuedSeekLock
				QueuedSeekCommand = New SeekCommand(SeekCommand.SeekTypes.AbsSamples, Samples)
			End SyncLock
			RaiseEvent SeekQueued(SeekCommand.SeekTypes.AbsSamples, Samples)
		End Sub
		Public Sub Restart()
			If Not IsReadyForPlayback Then Return
			SyncLock QueuedSeekLock
				QueuedSeekCommand = New SeekCommand(SeekCommand.SeekTypes.Restart)
			End SyncLock
			RaiseEvent SeekQueued(SeekCommand.SeekTypes.Restart, Nothing)
		End Sub
		Public Function BlockBytesToSamples(ByVal Bytes As Integer) As Integer
			Return (Bytes / BYTES_PER_SAMPLE)
		End Function
		Public Function BlockSamplesToBytes(ByVal Samples As Integer) As Integer
			Return (Samples * BYTES_PER_SAMPLE)
		End Function
		Public Sub ResetPositions()
			Timestamp = 0
			LoopCounter = 0
			LastWritePosition = 0
			LastPlayPosition = 0
			PrevLastWritePosition = 0
			StopOnNextHit = False
			StopPositionEvent.Reset()
		End Sub
		''' <summary>Singular place where all writing blocks to buffer happens, so we can intercept for amplification</summary>
		Private Sub DoBufferWrite()
			Select Case PlaybackMode
				Case PlaybackModes.SnesApu
					DoBufferWrite_Apu()
				Case PlaybackModes.WaveData
					DoBufferWrite_Wave()
			End Select
		End Sub
		Private Sub DoBufferWrite_Apu()
			If ENABLE_AMPLIFY AndAlso DEFAULT_AMPLIFY <> 1.0 Then
				' this *might* be sssslllllloooooowwwww
				Dim BlockData As Byte() = APU.GetOutput(BLOCK_SAMPLES)
				For i As Integer = 0 To BlockData.Length - 2 Step 2
					Dim Sample As Int16 = BitConverter.ToInt16(BlockData, i)
					Dim SampleF As Single = CSng(Sample)
					SampleF *= DEFAULT_AMPLIFY
					Dim TempVal As Int32 = CInt(SampleF)
					TempVal.Clamp(Int16.MinValue, Int16.MaxValue)
					Sample = TempVal
					Dim NewBytes As Byte() = BitConverter.GetBytes(Sample)
					BlockData(i) = NewBytes(0)
					BlockData(i + 1) = NewBytes(1)
				Next
				Buffer.Write(BlockData, WritePosition, LockFlags.None)
			Else
				Buffer.Write(APU.GetOutput(BLOCK_SAMPLES), WritePosition, LockFlags.None)
			End If
		End Sub
		' new wave stuff
		Public Sub PlayWave(ByVal WaveStream As MemoryStream)
			LoadCheck()
			If Not IsLoaded Then Return
			If State = RendererStates.Playing Then Me.Stop()
			If State = RendererStates.Uninitialized Then Throw New Exception("Not initialized")
			If State = RendererStates.Initializing Then Throw New Exception("Not done initializing")
			If State = RendererStates.Stopped Then
				StopBufferCalled = False
				PlaybackMode = PlaybackModes.WaveData
				ResetPositions()
				State = RendererStates.Starting
				StopOnNextHit = False
				WaveData = WaveStream
				PlaybackThread = New Thread(AddressOf PlaybackThreadProc)
				PlaybackThread.Name = "AudioPlaybackThread"
				PlaybackThread.Priority = THREAD_PRIORITY
				StopPlaybackThreadHandle.Reset()
				PlaybackThreadExitedHandle.Reset()
				State = RendererStates.Playing
				PlaybackThread.Start()
			End If
			RaiseEvent PlaybackStarted()
		End Sub
		Private Sub DoBufferWrite_Wave()
			Dim WaveBuffer(BLOCK_BYTES - 1) As Byte
			Dim BytesRead As Integer = WaveData.Read(WaveBuffer, 0, BLOCK_BYTES)
			If BytesRead < BLOCK_BYTES OrElse WaveData.Position = WaveData.Length Then
				Dim BytesLeft As Integer = WaveData.Length - WaveData.Position
				Dim BytesLeftInBlock As Integer = BLOCK_BYTES - BytesRead
				If BytesLeftInBlock > 0 Then
					For i As Integer = BytesRead To BLOCK_BYTES - 1
						WaveBuffer(i) = 0
					Next
				End If
				StopOnNextHit = True
			End If
			Buffer.Write(WaveBuffer, WritePosition, LockFlags.None)
			If BytesRead <= 0 Then
				StopOnNextHit = True
			End If
		End Sub
		Private Sub HandleQueuedSeekCommand()
			If PlaybackMode = PlaybackModes.SnesApu AndAlso IsApuLoaded Then
				SyncLock QueuedSeekLock
					If QueuedSeekCommand IsNot Nothing Then
						Dim SeekType As SeekCommand.SeekTypes = QueuedSeekCommand.SeekType
						Dim SeekValue = Nothing
						Select Case QueuedSeekCommand.SeekType
							Case SeekCommand.SeekTypes.AbsSamples
								APU.SeekAbsSamples(QueuedSeekCommand.Samples)
								SeekValue = QueuedSeekCommand.Samples
							Case SeekCommand.SeekTypes.AbsSeconds
								APU.SeekAbsSeconds(QueuedSeekCommand.Seconds)
								SeekValue = QueuedSeekCommand.Seconds
							Case SeekCommand.SeekTypes.RelSamples
								APU.SeekSamples(QueuedSeekCommand.Samples)
								SeekValue = QueuedSeekCommand.Samples
							Case SeekCommand.SeekTypes.RelSeconds
								APU.SeekSeconds(QueuedSeekCommand.Seconds)
								SeekValue = QueuedSeekCommand.Seconds
							Case SeekCommand.SeekTypes.Restart
								APU.Restart()
								WriteSilenceAll() ' NEW (TODO: make sure this works and doesn't break anything)
								'SeekValue = 0
						End Select
						QueuedSeekCommand = Nothing
						RaiseEvent Seeked(SeekType, SeekValue)
					End If
				End SyncLock
			End If
		End Sub
#End Region
#Region "Handlers"
		Private Sub Renderer_BufferHit(sender As Object, e As BufferHitArgs) Handles Me.BufferHit
			If StopOnNextHit Then
				StopPlaybackThreadHandle.Set()
				Return
			End If
			Static FirstLoop As Boolean = True
			If e.Index = 0 Then
				If FirstLoop Then
					FirstLoop = False
				Else
					LoopCounter += 1
				End If
			End If
			Dim NotifyWritePosition As Integer = GetWritePositionForNotifyIndex(e.Index)
			HandleQueuedSeekCommand()
			Try
				PrevLastWritePosition = LastWritePosition
				LastWritePosition = NotifyWritePosition	' should be below?
				LastPlayPosition = PlayPosition
				DoBufferWrite()
			Catch ex As Exception
			End Try
		End Sub
#End Region
#Region "IDisposable Support"
		Private disposedValue As Boolean
		Protected Overridable Sub Dispose(disposing As Boolean)
			If Not Me.disposedValue Then
				If disposing Then
					' dispose managed state (managed objects).
				End If
				' free unmanaged resources (unmanaged objects) and override Finalize() below.
				If Buffer IsNot Nothing AndAlso Not Buffer.Disposed Then Buffer.Dispose()
				If Device IsNot Nothing AndAlso Not Device.Disposed Then Device.Dispose()
				' set large fields to null.
			End If
			Me.disposedValue = True
		End Sub
		' override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
		Protected Overrides Sub Finalize()
			' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
			Dispose(False)
			MyBase.Finalize()
		End Sub
		Public Sub Dispose() Implements IDisposable.Dispose
			' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
			Dispose(True)
			GC.SuppressFinalize(Me)
		End Sub
#End Region
#Region "Enums"
		Public Enum RendererStates As Integer
			Uninitialized = 0
			Stopped
			Playing
			Initializing
			Starting
			Stopping
		End Enum
		Public Enum PlaybackModes As Integer
			None = 0
			SnesApu
			WaveData
		End Enum
#End Region
#Region "Subclasses"
		Public Class SeekCommand
#Region "Properties"
			Public Property SeekType As SeekTypes
			Public Property Seconds As Double
			Public Property Samples As Long
#End Region
#Region "Constructors/Destructors"
			Public Sub New(ByVal SeekType As SeekTypes)
				If Not SeekType = SeekTypes.Restart Then Throw New Exception()
				Me.SeekType = SeekType
			End Sub
			Public Sub New(ByVal SeekType As SeekTypes, ByVal Samples As Long)
				If Not (SeekType = SeekTypes.AbsSamples OrElse SeekType = SeekTypes.RelSamples) Then Throw New Exception()
				Me.Samples = Samples
				Me.SeekType = SeekType
			End Sub
			Public Sub New(ByVal SeekType As SeekTypes, ByVal Seconds As Double)
				If Not (SeekType = SeekTypes.AbsSeconds OrElse SeekType = SeekTypes.RelSeconds) Then Throw New Exception()
				Me.Seconds = Seconds
				Me.SeekType = SeekType
			End Sub
#End Region
#Region "Enums"
			Public Enum SeekTypes
				RelSamples
				RelSeconds
				AbsSamples
				AbsSeconds
				Restart
			End Enum
#End Region
		End Class
#End Region
	End Class
End Namespace
