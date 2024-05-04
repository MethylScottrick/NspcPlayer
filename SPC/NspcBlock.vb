Public Class NspcBlock
	Public Property Address As UInt16
	Public Property Data As Byte()
	Public ReadOnly Property Length As UInt16
		Get
			Return Data.Length
		End Get
	End Property
	Public ReadOnly Property LastAddress As UInt16
		Get
			Return (Address + Data.Length) - 1
		End Get
	End Property
End Class
