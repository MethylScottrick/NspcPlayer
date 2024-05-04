#Region "Imports"
Imports System.Runtime.InteropServices
#End Region
Namespace Sound
	Public Class SnesApu
#Region "Shared Methods"
		Public Shared Function GetApuRam(ByVal Address As UInt16, ByVal Length As UInt16) As Byte()
			Dim pRAM As IntPtr = IntPtr.Zero
			Dim pDummy As IntPtr = IntPtr.Zero
			NativeMethods.GetAPUData(pRAM, pDummy, pDummy, pDummy, pDummy, pDummy, pDummy, pDummy)
			pRAM += Address
			Dim RetVal(Length - 1) As Byte
			Marshal.Copy(pRAM, RetVal, 0, Length)
			Return RetVal
		End Function
		Public Shared Function GetApuRamByte(ByVal Address As UInt16) As Byte
			Dim Buffer As Byte() = GetApuRam(Address, 1)
			Return Buffer(0)
		End Function
		Public Shared Function GetApuRamSByte(ByVal Address As UInt16) As SByte
			Dim Buffer As Byte() = GetApuRam(Address, 1)
			Return Buffer(0).ToSByte()
		End Function
		Public Shared Function GetApuRamUInt16(ByVal Address As UInt16) As UInt16
			Dim Buffer As Byte() = GetApuRam(Address, 2)
			Return BitConverter.ToUInt16(Buffer, 0)
		End Function
		Public Shared Function GetApuRamInt16(ByVal Address As UInt16) As Int16
			Dim Buffer As Byte() = GetApuRam(Address, 2)
			Return BitConverter.ToInt16(Buffer, 0)
		End Function
		Public Shared Function ReadAPUData() As APUData
			Dim pRAM As IntPtr = IntPtr.Zero
			Dim pXRAM As IntPtr = IntPtr.Zero
			Dim pOutPort As IntPtr = IntPtr.Zero
			Dim pCounter64 As IntPtr = IntPtr.Zero
			Dim pDSPRam As IntPtr = IntPtr.Zero
			Dim pVoices As IntPtr = IntPtr.Zero
			Dim pMaxVolL As IntPtr = IntPtr.Zero
			Dim pMaxVolR As IntPtr = IntPtr.Zero
			NativeMethods.GetAPUData(pRAM, pXRAM, pOutPort, pCounter64, pDSPRam, pVoices, pMaxVolL, pMaxVolR)
			Dim Data As New APUData()
			Marshal.Copy(pRAM, Data.RAM, 0, UInt16.MaxValue)
			Marshal.Copy(pXRAM, Data.XRAM, 0, 128)
			Marshal.Copy(pOutPort, Data.OutPort, 0, 4)
			Dim U32Bytes(3) As Byte
			Marshal.Copy(pCounter64, U32Bytes, 0, 4)
			Data.Counter64 = BitConverter.ToUInt32(U32Bytes, 0)
			Marshal.Copy(pDSPRam, Data.DSPRam, 0, 128)
			' TODO: voices
			Marshal.Copy(pMaxVolL, U32Bytes, 0, 4)
			Data.MaxVolL = BitConverter.ToUInt32(U32Bytes, 0)
			Marshal.Copy(pMaxVolR, U32Bytes, 0, 4)
			Data.MaxVolR = BitConverter.ToUInt32(U32Bytes, 0)
			Return Data
		End Function
#End Region
#Region "Structures"
		Public Class APUData
			Public RAM(UInt16.MaxValue) As Byte
			Public XRAM(127) As Byte
			Public OutPort(3) As Byte
			Public Counter64 As UInt32
			Public DSPRam(127) As Byte
			Public Voices As Object	' TODO: this
			Public MaxVolL As UInt32
			Public MaxVolR As UInt32
		End Class
#End Region
	End Class
End Namespace
