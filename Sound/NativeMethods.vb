#Region "Imports"
Imports System.Runtime.InteropServices
#End Region
Namespace Sound
	''' <summary>snesapu.dll imports</summary>
	Public Class NativeMethods
#Region "Function Imports"
		<DllImport("snesapu.dll")>
		Friend Shared Function GetSNESAPUContextSize() As UInt32 ' tested
		End Function
		'void LoadSPCFile(void *pFile);
		<DllImport("snesapu.dll")>
		Friend Shared Sub LoadSPCFile(ByVal pFile As IntPtr) ' tested
		End Sub
		'void* EmuAPU(void *pBuf, u32 len, u8 type)
		<DllImport("snesapu.dll")>
		Friend Shared Function EmuAPU(ByVal pBuf As IntPtr, ByVal len As UInt32, ByVal type As Byte) As IntPtr ' tested
		End Function
		'void GetAPUData(u8 **ppRAM, u8 **ppXRAM, u8 **ppOutPort, u32 **ppT64Cnt, DSPReg **ppDSP, Voice **ppVoice, u32 **ppVMMaxL, u32 **ppVMMaxR);
		<DllImport("snesapu.dll")>
		Friend Shared Sub GetAPUData(ByRef ppRAM As IntPtr, ByRef ppXRAM As IntPtr, ByRef ppOutPort As IntPtr, ByRef ppT64Cnt As IntPtr, ByRef ppDSP As IntPtr, ByRef ppVoice As IntPtr, ByRef ppVMMaxL As IntPtr, ByRef ppVMMaxR As IntPtr) ' tested
		End Sub
		'void ResetAPU(u32 amp);
		<DllImport("snesapu.dll")>
		Friend Shared Sub ResetAPU(ByVal amp As Int32) ' tested
		End Sub
		'void SeekAPU(u32 time, b8 fast);
		<DllImport("snesapu.dll")>
		Friend Shared Sub SeekAPU(ByVal time As UInt32, ByVal fast As Byte)	' tested
		End Sub
		'void SetAPURAM(u32 addr, u8 val);
		<DllImport("snesapu.dll")>
		Friend Shared Sub SetAPURAM(ByVal addr As UInt32, ByVal val As Byte)
		End Sub
		'CBFUNC SNESAPUCallback(CBFUNC pCbFunc, u32 cbMask);
		<DllImport("snesapu.dll")>
		Friend Shared Function SNESAPUCallback(ByVal pCbFunc As IntPtr, ByVal cbMask As UInt32) As IntPtr
		End Function
#End Region
	End Class
End Namespace
