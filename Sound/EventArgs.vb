Namespace Sound
	''' <summary>Event arguments for when a sound buffer loops</summary>
	Public Class LoopCounterArgs
		Inherits EventArgs
		Public Property Counter As Integer
		Public Sub New(ByVal Counter As Integer)
			Me.Counter = Counter
		End Sub
	End Class
	''' <summary>Event arguments for when a position in a sound buffer is reached</summary>
	Public Class BufferHitArgs
		Inherits EventArgs
		Public Property Index As Integer
		Public Sub New(ByVal Index As Integer)
			Me.Index = Index
		End Sub
	End Class
End Namespace
