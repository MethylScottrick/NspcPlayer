Public Class NspcFile
	Private _Blocks As NspcBlock()
	Public Property Blocks As NspcBlock()
		Get
			Return _Blocks
		End Get
		Private Set(value As NspcBlock())
			_Blocks = value
			UpdateForBlocks()
		End Set
	End Property
	Private Property RangeDict As SortedDictionary(Of UInt16, UInt16)
	Public ReadOnly Property IsDataPresent(ByVal Address As UInt16, ByVal Length As UInt16) As Boolean
		Get
			For Each range In RangeDict.ToArray()
				If Address >= range.Key AndAlso ((Address + Length) - 1) <= range.Value Then
					Return True
				End If
			Next
			Return False
		End Get
	End Property
	Public ReadOnly Property IsAnyDataPresent(ByVal Address As UInt16, ByVal Length As UInt16) As Boolean
		Get
			For Each range In RangeDict.ToArray()
				If ((Address + Length) - 1) >= range.Key AndAlso Address <= range.Value Then ' TODO: TEST THIS
					Return True
				End If
			Next
			Return False
		End Get
	End Property
	Public Sub New()
		RangeDict = New SortedDictionary(Of UInt16, UInt16)()
	End Sub
	Public Sub New(ByVal Blocks As NspcBlock())
		MyClass.New()
		Me.Blocks = Blocks
	End Sub
	Public Sub UpdateForBlocks()
		RangeDict.Clear()
		For Each block As NspcBlock In Blocks
			If block.Length = 0 Then Continue For
			RangeDict.Add(block.Address, (block.Address + block.Length) - 1)
			'RangeDict.Add(New AddressRange(block.Address, (block.Address + block.Length) - 1))
		Next
		' merge adjacent ranges
		Dim PrevFirstAddress As UInt16 = 0
		Dim PrevLastAddress As UInt16 = 0
		Dim NewRangeDict As New SortedDictionary(Of UInt16, UInt16)()
		For i As Integer = 0 To RangeDict.Keys.Count - 1
			Dim RangeFirst As UInt16 = RangeDict.Keys(i)
			Dim RangeLast As UInt16 = RangeDict(RangeDict.Keys(i))
			If (PrevLastAddress > 0) AndAlso (RangeFirst = PrevLastAddress + 1) Then
				NewRangeDict(PrevFirstAddress) = RangeLast
			Else
				NewRangeDict.Add(RangeFirst, RangeLast)
				PrevFirstAddress = RangeFirst
			End If
			PrevLastAddress = RangeLast
		Next
		RangeDict.Clear()
		RangeDict = NewRangeDict
	End Sub
End Class
