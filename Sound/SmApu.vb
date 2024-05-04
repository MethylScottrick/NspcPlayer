Namespace Sound
	Public Class SmApu
#Region "Shared Methods"
		Public Shared Function GetMusicTrackStatus() As MusicTrackStatuses
			Dim Buffer As Byte() = SnesApu.GetApuRam(&HC, 1)
			Select Case Buffer(0)
				Case 0
					Return MusicTrackStatuses.Playing
				Case 1
					Return MusicTrackStatuses.ReadyToPlay
				Case 2
					Return MusicTrackStatuses.TrackLoadedNeedInit
				Case Else
					Return MusicTrackStatuses.Unknown
			End Select
		End Function
		Public Shared Function GetNoteModifiedFlag() As Byte
			Return SnesApu.GetApuRamByte(&H13)
		End Function
		Public Shared Function GetTrackerTimer() As Byte
			Return SnesApu.GetApuRamByte(&H42)
		End Function
		Public Shared Function GetTrackIndex() As Byte
			Return SnesApu.GetApuRamByte(&H44)
		End Function
		Public Shared Function GetKeyOnFlags() As Byte
			Return SnesApu.GetApuRamByte(&H45)
		End Function
		Public Shared Function GetKeyOffFlags() As Byte
			Return SnesApu.GetApuRamByte(&H46)
		End Function
		Public Shared Function GetMusicTrackClock() As Byte
			Return SnesApu.GetApuRamByte(&H51)
		End Function
		Public Shared Function GetTrackNoteTimers() As Byte()
			Dim Buffer As Byte() = SnesApu.GetApuRam(&H70, 16)
			Return {Buffer(0), Buffer(2), Buffer(4), Buffer(6), Buffer(8), Buffer(10), Buffer(12), Buffer(14)}
		End Function
		Public Shared Function GetTrackNoteRingTimers() As Byte()
			Dim Buffer As Byte() = SnesApu.GetApuRam(&H71, 15)
			Return {Buffer(0), Buffer(2), Buffer(4), Buffer(6), Buffer(8), Buffer(10), Buffer(12), Buffer(14)}
		End Function
		Public Shared Function GetTrackNoteTimer(ByVal TrackIndex As Byte) As Byte
			Return SnesApu.GetApuRamByte(&H70 + (TrackIndex * 2))
		End Function
		Public Shared Function GetTrackNoteRingTimer(ByVal TrackIndex As Byte) As Byte
			Return SnesApu.GetApuRamByte(&H71 + (TrackIndex * 2))
		End Function
#End Region
#Region "Enums"
		Public Enum MusicTrackStatuses As Byte
			Playing = 0
			ReadyToPlay = 1
			TrackLoadedNeedInit = 2
			Unknown
		End Enum
#End Region
#Region "Subclasses"
		' DEBUG
		Public Class DebugData ' TODO: get rid of me?
#Region "Properties"
			Public Property MusicTrackStatus As Byte
			Public Property NoteModifiedFlag As Byte
			Public Property TrackerTimer As Byte
			Public Property TrackIndex As Byte
			Public Property KeyOnFlags As Byte
			Public Property KeyOffFlags As Byte
			Public Property MusicTrackClock As Byte
			Public Property TrackNoteTimer As Byte
			Public Property TrackNoteRingTimer As Byte
#End Region
#Region "Methods"
			Public Sub GetData()
				MusicTrackStatus = GetMusicTrackStatus()
				NoteModifiedFlag = GetNoteModifiedFlag()
				TrackerTimer = GetNoteModifiedFlag()
				TrackIndex = GetTrackIndex()
				KeyOnFlags = GetKeyOnFlags()
				KeyOffFlags = GetKeyOffFlags()
				MusicTrackClock = GetMusicTrackClock()
				TrackNoteTimer = GetTrackNoteTimer(0)
				TrackNoteRingTimer = GetTrackNoteRingTimer(0)
			End Sub
#End Region
		End Class
#End Region
	End Class
End Namespace
