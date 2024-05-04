#Region "Imports"
Imports System.IO
#End Region
Public Class frmPlayer
	' TODO: multiple tracks
#Region "Properties"
	Private _PlayState As PlayStates
	Public Property PlayState As PlayStates
		Get
			Return _PlayState
		End Get
		Set(value As PlayStates)
			If _PlayState <> value Then
				_PlayState = value
				UpdateForPlayState()
			End If
		End Set
	End Property
	Private _RepeatOn As Boolean
	Public Property RepeatOn As Boolean
		Get
			Return _RepeatOn
		End Get
		Set(value As Boolean)
			If _RepeatOn <> value Then
				_RepeatOn = value
				UpdateForRepeat()
			End If
		End Set
	End Property
	Private _LoadedFilePath As String
	Public Property LoadedFilePath As String
		Get
			Return _LoadedFilePath
		End Get
		Private Set(value As String)
			Dim FullPath As String = Nothing
			If Not String.IsNullOrEmpty(value) Then FullPath = Path.GetFullPath(value)
			If _LoadedFilePath <> FullPath Then
				_LoadedFilePath = FullPath
				If PlayState = PlayStates.Paused OrElse PlayState = PlayStates.Playing Then
					DoStop()
				End If
				UpdateForFilePath()
			End If
		End Set
	End Property
	Private _LoadedData As Byte()
	Public Property LoadedData As Byte()
		Get
			Return _LoadedData
		End Get
		Set(value As Byte())
			_LoadedData = value
		End Set
	End Property
	Public Property LoadedNspc As NspcFile
	Private _SongIndices As Integer()
	Public Property SongIndices As Integer()
		Get
			Return _SongIndices
		End Get
		Set(value As Integer())
			_SongIndices = value
			UpdateForSongIndices()
		End Set
	End Property
	Private _SelectedSongIndex As Integer
	Public Property SelectedSongIndex As Integer
		Get
			Return _SelectedSongIndex
		End Get
		Set(value As Integer)
			If _SelectedSongIndex <> value Then
				Debug.WriteLine("Index=" & value)
				_SelectedSongIndex = value
				UpdateForSelectedIndex()
			End If
		End Set
	End Property
	Public Property DetectedGame As Games
	Public Property SongLength As Integer ' TODO: put this in NspcFile in a song array instead of here
	Public Property SongLoopTime As Integer	' "
	Public Property SongPauseTime As Integer ' "
	Private WithEvents _Audio As NspcPlayer.Sound.NspcAudio
	Public Property Audio As NspcPlayer.Sound.NspcAudio
		Get
			Return _Audio
		End Get
		Set(value As NspcPlayer.Sound.NspcAudio)
			_Audio = value
		End Set
	End Property
	Public ReadOnly Property IsNspcLoaded As Boolean
		Get
			Return LoadedNspc IsNot Nothing
		End Get
	End Property
	Public ReadOnly Property IsFileLoaded As Boolean
		Get
			Return Not String.IsNullOrEmpty(LoadedFilePath)
		End Get
	End Property
#End Region
#Region "Constructors"
	Private Sub frmPlayer_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
		SongIndices = New Integer() {}
		'My.Settings.Reload()
		PlayState = PlayStates.NotLoaded
		RepeatOn = True
		DetectedGame = Games.None
		UpdateForPlayState()
		UpdateForRepeat()
		Audio = New NspcPlayer.Sound.NspcAudio()
		Me.Show()
		Application.DoEvents()
		CheckCmdLine()
	End Sub
	Private Sub frmPlayer_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
		If PlayState <> PlayStates.NotLoaded Then UnloadFile()
		'My.Settings.Save()
	End Sub
	Private Sub frmPlayer_FormClosed(sender As Object, e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
		If Audio IsNot Nothing Then
			Audio.Unload()
			Audio.Dispose()
			Audio = Nothing
		End If
	End Sub
#End Region
#Region "Methods"
	Private Sub CheckCmdLine()
		Dim Args As String() = Environment.GetCommandLineArgs()
		If Args.Length > 1 Then
			LoadFileAndPlay(Args(1))
		End If
	End Sub
	Private Sub UpdateForPlayState()
		Select Case PlayState
			Case PlayStates.NotLoaded
				btnPlay.Enabled = False
				btnPause.Enabled = False
				btnStop.Enabled = False
				SongLength = -1
				SongLoopTime = -1
				SongPauseTime = -1
				DetectedGame = Games.None
				SongIndices = New Integer() {}
			Case PlayStates.UnsupportedGame
				btnPlay.Enabled = False
				btnPause.Enabled = False
				btnStop.Enabled = False
				SongLength = -1
				SongLoopTime = -1
				SongPauseTime = -1
				SongIndices = New Integer() {}
			Case PlayStates.NoSongSelected
				btnPlay.Enabled = False
				btnPause.Enabled = False
				btnStop.Enabled = False
			Case PlayStates.Paused
				btnPlay.Enabled = True
				btnPause.Enabled = False
				btnStop.Enabled = True
			Case PlayStates.Playing
				btnPlay.Enabled = False
				btnPause.Enabled = True
				btnStop.Enabled = True
			Case PlayStates.Stopped
				btnPlay.Enabled = True
				btnPause.Enabled = False
				btnStop.Enabled = False
			Case Else
				btnPlay.Enabled = False
				btnPause.Enabled = False
				btnStop.Enabled = False
		End Select
	End Sub
	Private Sub UpdateForRepeat()
		If RepeatOn Then
			btnRepeat.Image = My.Resources.btn24RepeatOn
		Else
			btnRepeat.Image = My.Resources.btn24Repeat
		End If
	End Sub
	Private Sub UpdateForFilePath()
		If Not String.IsNullOrEmpty(LoadedFilePath) Then
			'lblFile.Text = LoadedFilePath
			lblFile.SetLabelEllipsisText(LoadedFilePath)
			If {Games.SuperMetroid, Games.ALinkToThePast}.Contains(DetectedGame) Then
				PlayState = PlayStates.Stopped
			Else
				PlayState = PlayStates.UnsupportedGame
			End If
		Else
			lblFile.Text = "(none)"
			lblGame.Text = ""
			PlayState = PlayStates.NotLoaded
		End If
	End Sub
	Private Sub UpdateForSongIndices()
		cboSongIndices.Items.Clear()
		For Each Index As Integer In SongIndices
			cboSongIndices.Items.Add((Index + 1).ToString())
		Next
		If cboSongIndices.Items.Count = 0 Then
			cboSongIndices.SelectedItem = Nothing
		Else
			cboSongIndices.SelectedIndex = 0
		End If
		UpdateComboSelectedIndex()
	End Sub
	Private Sub UpdateForSelectedIndex()
		If SelectedSongIndex = -1 Then
			If IsFileLoaded Then
				PlayState = PlayStates.NoSongSelected
			End If
		Else
			If IsFileLoaded Then
				If PlayState = PlayStates.Paused OrElse PlayState = PlayStates.Playing Then
					DoStop()
				End If
				PlayState = PlayStates.Stopped
				WriteSongIndex()
			End If
		End If
	End Sub
	Private Sub UpdateComboSelectedIndex()
		If cboSongIndices.SelectedItem IsNot Nothing Then
			SelectedSongIndex = Val(cboSongIndices.SelectedText) - 1
		Else
			SelectedSongIndex = -1
		End If
	End Sub
	Public Sub DoPlay()
		If IsFileLoaded Then
			Audio.PlayApu()
			PlayState = PlayStates.Playing
		End If
	End Sub
	Public Sub DoPause()
		If IsFileLoaded Then
			If PlayState = PlayStates.Playing Then
				Audio.Stop()
				PlayState = PlayStates.Paused
			End If
		End If
	End Sub
	Public Sub DoStop()
		If IsFileLoaded Then
			If PlayState = PlayStates.Playing Then
				Audio.Stop()
				'Audio.Reset()
				Audio.Restart()
				Audio.ResetPositions()
				'Audio.WriteSilenceAll()
				PlayState = PlayStates.Stopped
			End If
		End If
	End Sub
	Public Sub LoadFileAndPlay(ByVal Path As String)
		If File.Exists(Path) Then
			LoadFile(Path)
			DoPlay()
		End If
	End Sub
	Public Sub LoadFile(ByVal Path As String)
		If Not File.Exists(Path) Then Return
		If IsFileLoaded Then UnloadFile()
		Dim BlockList As New List(Of NspcBlock)()
		Dim SpcData As Byte() = Nothing
		' load nspc file
		Using NspcFileStream As New FileStream(Path, FileMode.Open)
			Using NspcFileReader As New BinaryReader(NspcFileStream)
				' build NpscBlock's
				While NspcFileStream.Position < NspcFileStream.Length
					Dim BlockLength As UInt16 = NspcFileReader.ReadUInt16()
					If BlockLength = 0 Then Exit While
					Dim NewBlock As New NspcBlock()
					NewBlock.Address = NspcFileReader.ReadUInt16()
					NewBlock.Data = NspcFileReader.ReadBytes(BlockLength)
					BlockList.Add(NewBlock)
				End While
			End Using
		End Using
		Dim BlockArray As NspcBlock() = BlockList.ToArray()
		Me.LoadedNspc = New NspcFile(BlockArray)
		' determine game
		'Dim Game As Games = DetectGame()
		DetectedGame = DetectGame()
		' load base spc for game
		'Select Case Game
		Select Case DetectedGame
			Case Games.SuperMetroid
				'DetectedGame = Games.SuperMetroid
				lblGame.Text = "Super Metroid"
				'ReDim SpcData(UInt16.MaxValue)
				ReDim SpcData(My.Resources.smorg.Length - 1)
				Array.Copy(My.Resources.smorg, SpcData, My.Resources.smorg.Length)
			Case Games.SuperMarioWorld
				'DetectedGame = Games.SuperMarioWorld
				lblGame.Text = "Super Mario World"
			Case Games.ALinkToThePast
				'DetectedGame = Games.ALinkToThePast
				lblGame.Text = "A Link To The Past"
				ReDim SpcData(My.Resources.alttporg.Length - 1)
				Array.Copy(My.Resources.alttporg, SpcData, My.Resources.alttporg.Length)
				'Case Games.Unknown
			Case Else
				'DetectedGame = Games.Unknown
				Throw New Exception("Unrecognized game")
		End Select
		' write nspc payloads
		Using SpcDataStream As New MemoryStream(SpcData)
			Using SpcDataWriter As New BinaryWriter(SpcDataStream)
				For Each block As NspcBlock In BlockArray
					SpcDataStream.Seek(block.Address + &H100, SeekOrigin.Begin)
					SpcDataWriter.Write(block.Data)
				Next
			End Using
		End Using
		' TODO: get song time and loop point
		' save result to LoadedData
		LoadedFilePath = Path
		If {Games.SuperMetroid, Games.ALinkToThePast}.Contains(DetectedGame) Then
			LoadedData = SpcData
			DetectSongIndices()
			Audio.LoadSPC(LoadedData)
			' DEBUG
			'File.WriteAllBytes("test.spc", LoadedData)
		Else
			LoadedData = Nothing
			SongIndices = {}
			PlayState = PlayStates.UnsupportedGame
		End If
	End Sub
	Public Sub UnloadFile()
		If PlayState = PlayStates.Playing Or PlayState = PlayStates.Paused Then DoStop()
		LoadedNspc = Nothing
		LoadedData = Nothing
		LoadedFilePath = Nothing
	End Sub
	Public Sub DetectSongIndices()
		' TODO: put this in NspcFile instead of form
		If Not IsFileLoaded OrElse DetectedGame = Games.Unknown OrElse DetectedGame = Games.None Then
			SongIndices = New Integer() {}
			Return
		End If
		Select Case DetectedGame
			Case Games.SuperMetroid
				DetectSongIndices_SM()
			Case Games.SuperMarioWorld
				DetectSongIndices_SMW()
			Case Games.ALinkToThePast
				DetectSongIndices_ALTTP()
			Case Else
		End Select
	End Sub
	Private Sub DetectSongIndices_SM()
		Dim StartingIndex As Integer = -1
		Dim LowestPointer As Integer = -1
		Dim IndexList As New List(Of Integer)()
		For i As Integer = 0 To 15
			Dim PtrAddress As UInt16 = SongTable_SM + (i * 2)
			If PtrAddress <= LowestPointer Then Exit For
			If LoadedNspc.IsDataPresent(PtrAddress, 2) Then
				Dim Pointer As UInt16 = LoadedData.ReadUInt16(PtrAddress + &H100)
				If StartingIndex = -1 Then
					StartingIndex = i
				End If
				If (Pointer < LowestPointer AndAlso Pointer > SongTable_SM) OrElse LowestPointer = -1 Then LowestPointer = Pointer
				If Pointer > PtrAddress OrElse Pointer < SongTable_SM Then
					IndexList.Add(i)
				Else
					Exit For
				End If
			End If
		Next
		IndexList.Sort()
		SongIndices = IndexList.ToArray()
	End Sub
	Private Sub DetectSongIndices_SMW()
		Dim StartingIndex As Integer = -1
		Dim LowestPointer As Integer = -1
		Dim IndexList As New List(Of Integer)()
		For i As Integer = 0 To 15
			Dim PtrAddress As UInt16 = SongTable_SMW + (i * 2)
			If PtrAddress <= LowestPointer Then Exit For
			If LoadedNspc.IsDataPresent(PtrAddress, 2) Then
				Dim Pointer As UInt16 = LoadedData.ReadUInt16(PtrAddress + &H100)
				If StartingIndex = -1 Then
					StartingIndex = i
				End If
				If (Pointer < LowestPointer AndAlso Pointer > SongTable_SMW) OrElse LowestPointer = -1 Then LowestPointer = Pointer
				If Pointer > PtrAddress OrElse Pointer < SongTable_SMW Then
					IndexList.Add(i)
				Else
					Exit For
				End If
			End If
		Next
		IndexList.Sort()
		SongIndices = IndexList.ToArray()
	End Sub
	Private Sub DetectSongIndices_ALTTP()
		Dim StartingIndex As Integer = -1
		Dim LowestPointer As Integer = -1
		Dim IndexList As New List(Of Integer)()
		For i As Integer = 0 To 15
			Dim PtrAddress As UInt16 = SongTable_ALTTP1 + (i * 2)
			If PtrAddress <= LowestPointer Then Exit For
			If LoadedNspc.IsDataPresent(PtrAddress, 2) Then
				Dim Pointer As UInt16 = LoadedData.ReadUInt16(PtrAddress + &H100)
				If StartingIndex = -1 Then
					StartingIndex = i
				End If
				If (Pointer < LowestPointer AndAlso Pointer > SongTable_ALTTP1) OrElse LowestPointer = -1 Then LowestPointer = Pointer
				If Pointer > PtrAddress OrElse Pointer < SongTable_ALTTP1 Then
					IndexList.Add(i)
				Else
					Exit For
				End If
			End If
		Next
		IndexList.Sort()
		SongIndices = IndexList.ToArray()
	End Sub
	Public Function DetectGame() As Games
		' TODO: this
		'If Not IsFileLoaded Then Return Games.None
		If Not IsNspcLoaded Then Return Games.None
		If LoadedNspc.IsAnyDataPresent(SongTable_SM, 32) Then
			Return Games.SuperMetroid
		ElseIf LoadedNspc.IsAnyDataPresent(SongTable_SMW, 32) Then
			Return Games.SuperMarioWorld
		ElseIf LoadedNspc.IsAnyDataPresent(SongTable_ALTTP1, 32) Then
			Return Games.ALinkToThePast
		Else
			Return Games.Unknown
		End If
	End Function
	Public Sub WriteSongIndex()
		If PlayState = PlayStates.NotLoaded OrElse PlayState = PlayStates.UnsupportedGame Then Return
		If PlayState = PlayStates.Playing OrElse PlayState = PlayStates.Paused Then DoStop()
		Select Case DetectedGame
			Case Games.ALinkToThePast
				Audio.APU.FreeSPC()
				LoadedData.WriteByte(SongIndex_ALTTP, CByte(Val(cboSongIndices.SelectedText) + 1))
				Audio.LoadSPC(LoadedData)
			Case Games.SuperMetroid
				Audio.APU.FreeSPC()
				LoadedData.WriteByte(SongIndex_SM, CByte(Val(cboSongIndices.SelectedText) + 1))
				Audio.LoadSPC(LoadedData)
			Case Else
		End Select
	End Sub
#End Region
#Region "Handlers"
	Private Sub frmPlayer_DragOver(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles Me.DragOver
		If e.Data.GetDataPresent("FileDrop") Then
			e.Effect = DragDropEffects.Link
		Else
			e.Effect = DragDropEffects.None
		End If
	End Sub
	Private Sub frmPlayer_DragDrop(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles Me.DragDrop
		Try
			If e.Data.GetDataPresent("FileDrop") Then
				Dim Path As String = e.Data.GetData("FileDrop")(0)
				LoadFileAndPlay(Path)
			End If
		Catch ex As Exception
		End Try
	End Sub
	Private Sub btnOpen_Click(sender As System.Object, e As System.EventArgs) Handles btnOpen.Click
		Using ofd As New OpenFileDialog
			If Not String.IsNullOrEmpty(My.Settings.LastDir) AndAlso Directory.Exists(My.Settings.LastDir) Then
				ofd.InitialDirectory = My.Settings.LastDir
			Else
				ofd.InitialDirectory = Environment.CurrentDirectory
			End If
			ofd.Filter = "N-SPC files (*.nspc)|*.nspc|All Files (*.*)|*.*"
			ofd.FilterIndex = 0
			ofd.Title = "Open N-SPC File"
			If ofd.ShowDialog() = Windows.Forms.DialogResult.OK Then
				My.Settings.LastDir = Path.GetDirectoryName(ofd.FileName)
				My.Settings.Save()
				LoadFile(ofd.FileName)
			End If
		End Using
	End Sub
	Private Sub btnPlay_Click(sender As System.Object, e As System.EventArgs) Handles btnPlay.Click
		If PlayState = PlayStates.Paused OrElse PlayState = PlayStates.Stopped Then
			DoPlay()
		End If
	End Sub
	Private Sub btnPause_Click(sender As System.Object, e As System.EventArgs) Handles btnPause.Click
		If PlayState = PlayStates.Playing Then
			DoPause()
		End If
	End Sub
	Private Sub btnStop_Click(sender As System.Object, e As System.EventArgs) Handles btnStop.Click
		If PlayState = PlayStates.Playing OrElse PlayState = PlayStates.Paused Then
			DoStop()
		End If
	End Sub
	Private Sub btnRepeat_Click(sender As System.Object, e As System.EventArgs) Handles btnRepeat.Click
		RepeatOn = Not RepeatOn
	End Sub
	Private Sub cboSongIndices_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cboSongIndices.SelectedIndexChanged
		UpdateComboSelectedIndex()
	End Sub
	Private Sub cboSongIndices_SelectedValueChanged(sender As Object, e As System.EventArgs) Handles cboSongIndices.SelectedValueChanged
		UpdateComboSelectedIndex()
	End Sub
	Private Sub _Audio_Loaded() Handles _Audio.Loaded
	End Sub
	Private Sub _Audio_PlaybackEnded(sender As Object, e As System.EventArgs) Handles _Audio.PlaybackEnded
		PlayState = PlayStates.Stopped
	End Sub
	Private Sub _Audio_PlaybackStarted() Handles _Audio.PlaybackStarted
	End Sub
	Private Sub _Audio_PlaybackThreadStarted() Handles _Audio.PlaybackThreadStarted
	End Sub
	Private Sub _Audio_PlaybackThreadStopped() Handles _Audio.PlaybackThreadStopped
	End Sub
	Private Sub _Audio_Resetted() Handles _Audio.Resetted
	End Sub
	Private Sub _Audio_Seeked(SeekType As Sound.NspcAudio.SeekCommand.SeekTypes, SeekValue As Object) Handles _Audio.Seeked
	End Sub
	Private Sub _Audio_SeekQueued(SeekType As Sound.NspcAudio.SeekCommand.SeekTypes, SeekValue As Object) Handles _Audio.SeekQueued
	End Sub
	Private Sub _Audio_SpcLoaded() Handles _Audio.SpcLoaded
	End Sub
	Private Sub _Audio_StateChanged() Handles _Audio.StateChanged
		Select Case Audio.State
			Case Sound.NspcAudio.RendererStates.Initializing
			Case Sound.NspcAudio.RendererStates.Playing
			Case Sound.NspcAudio.RendererStates.Starting
			Case Sound.NspcAudio.RendererStates.Stopped
			Case Sound.NspcAudio.RendererStates.Stopping
			Case Sound.NspcAudio.RendererStates.Uninitialized
			Case Else
		End Select
	End Sub
	Private Sub _Audio_Unloaded() Handles _Audio.Unloaded
	End Sub
#End Region
End Class
