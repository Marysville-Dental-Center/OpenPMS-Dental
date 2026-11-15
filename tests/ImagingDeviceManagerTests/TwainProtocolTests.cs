// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ImagingDeviceManager.TwainProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingDeviceManager.Tests {
	[TestClass]
	public class TwainProtocolTests {
		///<summary>x86 or x64 by the bitness of this test process: the expected sizes and offsets below have one column for each (S3a-12 of the .NET 10
		///move). The lane runs this assembly in both.</summary>
		private static int Pick(int x86,int x64) {
			return IntPtr.Size==8 ? x64 : x86;
		}

		///<summary>(structure, size in a 32-bit process, size in a 64-bit process), from twain.h of TWAIN 2.x on Windows: two-byte packing in both
		///bitnesses (#pragma pack(push, before_twain) / pack(2)); TW_HANDLE, TW_MEMREF and TW_UINTPTR are pointer-sized, TW_UINT32 and TW_INT32 are
		///4 bytes (Windows long), TW_STR32 34 bytes, TW_IDENTITY.Id a TW_UINT32.</summary>
		private static readonly List<Tuple<Type,int,int>> _listSizes=new List<Tuple<Type,int,int>> {
			Tuple.Create(typeof(TW_FIX32),4,4),
			Tuple.Create(typeof(TW_VERSION),42,42),
			Tuple.Create(typeof(TW_IDENTITY),156,156),
			Tuple.Create(typeof(TW_USERINTERFACE),8,12),//ShowUI(2) ModalUI(2) hParent(TW_HANDLE)
			Tuple.Create(typeof(TW_EVENT),6,10),//pEvent(TW_MEMREF) TWMessage(2)
			Tuple.Create(typeof(TW_PENDINGXFERS),6,6),
			Tuple.Create(typeof(TW_STATUS),4,4),
			Tuple.Create(typeof(TW_STATUSUTF8),12,16),//Status(4) Size(4) UTF8string(TW_HANDLE)
			Tuple.Create(typeof(TW_CAPABILITY),8,12),//Cap(2) ConType(2) hContainer(TW_HANDLE)
			Tuple.Create(typeof(TW_IMAGEINFO),42,42),
			Tuple.Create(typeof(TW_SETUPMEMXFER),12,12),
			Tuple.Create(typeof(TW_MEMORY),12,16),//Flags(4) Length(4) TheMem(TW_MEMREF)
			Tuple.Create(typeof(TW_IMAGEMEMXFER),38,42),//Compression(2) six TW_UINT32 (24) Memory(TW_MEMORY)
			Tuple.Create(typeof(TW_PALETTE8),1028,1028),
			Tuple.Create(typeof(TW_INFO),12,16),//InfoID ItemType NumItems ReturnCode (2 each) Item(TW_UINTPTR)
			Tuple.Create(typeof(TW_ENTRYPOINT),24,44),//Size(4) five function pointers
		};

		///<summary>(structure, field, offset in a 32-bit process, offset in a 64-bit process), from the same header. With two-byte packing a pointer
		///field moves only when a pointer comes before it.</summary>
		private static readonly List<Tuple<Type,string,int,int>> _listOffsets=new List<Tuple<Type,string,int,int>> {
			Tuple.Create(typeof(TW_IDENTITY),"Version",4,4),
			Tuple.Create(typeof(TW_IDENTITY),"ProtocolMajor",46,46),
			Tuple.Create(typeof(TW_IDENTITY),"SupportedGroups",50,50),
			Tuple.Create(typeof(TW_IDENTITY),"Manufacturer",54,54),
			Tuple.Create(typeof(TW_IDENTITY),"ProductFamily",88,88),
			Tuple.Create(typeof(TW_IDENTITY),"ProductName",122,122),
			Tuple.Create(typeof(TW_USERINTERFACE),"hParent",4,4),
			Tuple.Create(typeof(TW_EVENT),"TWMessage",4,8),
			Tuple.Create(typeof(TW_PENDINGXFERS),"EOJ",2,2),
			Tuple.Create(typeof(TW_CAPABILITY),"hContainer",4,4),
			Tuple.Create(typeof(TW_STATUSUTF8),"Size",4,4),
			Tuple.Create(typeof(TW_STATUSUTF8),"UTF8string",8,8),
			Tuple.Create(typeof(TW_IMAGEINFO),"BitsPerSample",18,18),
			Tuple.Create(typeof(TW_IMAGEINFO),"BitsPerPixel",34,34),
			Tuple.Create(typeof(TW_IMAGEINFO),"PixelType",38,38),
			Tuple.Create(typeof(TW_MEMORY),"TheMem",8,8),
			Tuple.Create(typeof(TW_IMAGEMEMXFER),"BytesPerRow",2,2),
			Tuple.Create(typeof(TW_IMAGEMEMXFER),"BytesWritten",22,22),
			Tuple.Create(typeof(TW_IMAGEMEMXFER),"Memory",26,26),
			Tuple.Create(typeof(TW_INFO),"ReturnCode",6,6),
			Tuple.Create(typeof(TW_INFO),"Item",8,8),
			Tuple.Create(typeof(TW_ENTRYPOINT),"DSM_Entry",4,4),
			Tuple.Create(typeof(TW_ENTRYPOINT),"DSM_MemAllocate",8,12),
			Tuple.Create(typeof(TW_ENTRYPOINT),"DSM_MemUnlock",20,36),
		};

		[TestMethod]
		public void Process_IsX86OrX64() {
			CollectionAssert.Contains(new[] { 4,8 },IntPtr.Size,"IntPtr.Size is "+IntPtr.Size+".");
			Assert.AreEqual(Environment.Is64BitProcess,IntPtr.Size==8);
			Console.WriteLine("TWAIN layout tests ran in a "+(IntPtr.Size*8)+"-bit process.");
		}

		[TestMethod]
		public void Structs_HaveTwainSizes() {
			foreach(Tuple<Type,int,int> tuple in _listSizes) {
				Assert.AreEqual(Pick(tuple.Item2,tuple.Item3),Marshal.SizeOf(tuple.Item1),tuple.Item1.Name);
			}
		}

		[TestMethod]
		public void Structs_HaveTwainFieldOffsets() {
			foreach(Tuple<Type,string,int,int> tuple in _listOffsets) {
				Assert.AreEqual(Pick(tuple.Item3,tuple.Item4),Marshal.OffsetOf(tuple.Item1,tuple.Item2).ToInt64(),tuple.Item1.Name+"."+tuple.Item2);
			}
		}

		[TestMethod]
		public void Structs_TableCoversEveryTwainStruct() {
			//A new TW_ structure gets its sizes from twain.h, both columns.
			List<Type> listTypes=new List<Type>();
			foreach(Type type in typeof(TW_IDENTITY).Assembly.GetTypes()) {
				if(type.IsValueType && !type.IsEnum && type.Namespace==typeof(TW_IDENTITY).Namespace && type.Name.StartsWith("TW_")) {
					listTypes.Add(type);
				}
			}
			foreach(Type type in listTypes) {
				Assert.IsTrue(_listSizes.Exists(x => x.Item1==type),type.Name+" has no row in the size table.");
			}
			Assert.AreEqual(listTypes.Count,_listSizes.Count);
		}

		[TestMethod]
		public void Msg_HasTheWindowsLayout() {
			//MSG (winuser.h), natural alignment: hwnd, message, wParam, lParam, time, pt. 28 bytes in a 32-bit process, 48 in a 64-bit one.
			Assert.AreEqual(Pick(28,48),Marshal.SizeOf(typeof(NativeMethods.MSG)));
			Assert.AreEqual(0,NativeMethods.MsgOffsetHwnd);
			Assert.AreEqual(Pick(4,8),NativeMethods.MsgOffsetMessage);
			Assert.AreEqual(Pick(8,16),NativeMethods.MsgOffsetWParam);
			Assert.AreEqual(Pick(12,24),Marshal.OffsetOf(typeof(NativeMethods.MSG),"lParam").ToInt64());
			Assert.AreEqual(Pick(16,32),Marshal.OffsetOf(typeof(NativeMethods.MSG),"time").ToInt64());
			Assert.AreEqual(Pick(20,36),Marshal.OffsetOf(typeof(NativeMethods.MSG),"ptX").ToInt64());
			Assert.IsTrue(NativeMethods.MsgBufferSize>=Marshal.SizeOf(typeof(NativeMethods.MSG)),"The message buffers hold a whole MSG.");
			Assert.AreEqual(0,NativeMethods.MsgBufferSize%4,"The buffers are cleared four bytes at a time.");
		}

		[TestMethod]
		public void Msg_ReadsWhatPeekMessageWrites() {
			//A posted WM_TIMER-like message read back from a real MSG through the offsets the message loops use.
			const int WM_APP=0x8000;
			IntPtr pMsg=Marshal.AllocHGlobal(NativeMethods.MsgBufferSize);
			try {
				Assert.IsTrue(NativeMethods.PostMessage(IntPtr.Zero,WM_APP+7,new IntPtr(0x1234),new IntPtr(-2)),"PostMessage to the thread.");
				Assert.IsTrue(NativeMethods.PeekMessage(pMsg,IntPtr.Zero,WM_APP+7,WM_APP+7,NativeMethods.PM_REMOVE),"PeekMessage.");
				Assert.AreEqual(IntPtr.Zero,Marshal.ReadIntPtr(pMsg,NativeMethods.MsgOffsetHwnd));
				Assert.AreEqual(WM_APP+7,Marshal.ReadInt32(pMsg,NativeMethods.MsgOffsetMessage));
				Assert.AreEqual(new IntPtr(0x1234),Marshal.ReadIntPtr(pMsg,NativeMethods.MsgOffsetWParam));
				NativeMethods.MSG msg=(NativeMethods.MSG)Marshal.PtrToStructure(pMsg,typeof(NativeMethods.MSG));
				Assert.AreEqual((uint)(WM_APP+7),msg.message);
				Assert.AreEqual(new IntPtr(-2),msg.lParam,"LPARAM is signed and pointer-sized.");
			}
			finally {
				Marshal.FreeHGlobal(pMsg);
			}
		}

		[TestMethod]
		public void ExtImageInfo_OffsetsFollowTheInfoSize() {
			//TW_EXTIMAGEINFO: NumInfos (TW_UINT32) then the TW_INFO array, two-byte packing.
			Assert.AreEqual(4,TwExtImageInfo.GetInfoOffset(0));
			Assert.AreEqual(Pick(16,20),TwExtImageInfo.GetInfoOffset(1));
			Assert.AreEqual(Pick(28,36),TwExtImageInfo.GetSize(2));
		}

		[TestMethod]
		public void ExtImageInfo_ItemHandleRuleIsTheSameInBothBitnesses() {
			//TW_INFO.Item is a TW_UINTPTR (8 bytes in a 64-bit process), but the specification's rule is "four bytes": data of up to 4 bytes is in
			//the field, more is a handle the application frees (TWAIN 2.4 p.8-43). Two TW_UINT32 (8 bytes) would fit an 8-byte field and are still
			//a handle.
			Assert.AreEqual(4,TwExtImageInfo.MaxInlineItemBytes);
			Assert.IsFalse(TwExtImageInfo.IsItemHandle(Tw.TWTY_UINT16,1));
			Assert.IsFalse(TwExtImageInfo.IsItemHandle(Tw.TWTY_UINT16,2));
			Assert.IsFalse(TwExtImageInfo.IsItemHandle(Tw.TWTY_UINT32,1));
			Assert.IsFalse(TwExtImageInfo.IsItemHandle(Tw.TWTY_FIX32,1));
			Assert.IsTrue(TwExtImageInfo.IsItemHandle(Tw.TWTY_UINT16,3));
			Assert.IsTrue(TwExtImageInfo.IsItemHandle(Tw.TWTY_UINT32,2));
			Assert.IsTrue(TwExtImageInfo.IsItemHandle(Tw.TWTY_FRAME,1));
			Assert.IsTrue(TwExtImageInfo.IsItemHandle(Tw.TWTY_STR255,1));
			Assert.IsTrue(TwExtImageInfo.IsItemHandle(Tw.TWTY_HANDLE,1));
		}

		[TestMethod]
		public void ExtImageInfo_InlineItemReadsTheLowBytes() {
			//A TWTY_UINT16 item in the field: the source writes it into the low bytes of the TW_UINTPTR (little-endian), and the rest is zero.
			IntPtr p=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TW_INFO)));
			try {
				for(int i=0;i<Marshal.SizeOf(typeof(TW_INFO));i++) {
					Marshal.WriteByte(p,i,0);
				}
				Marshal.WriteInt16(p,(int)Marshal.OffsetOf(typeof(TW_INFO),"Item"),(short)Tw.TWPF_CHOCOLATE);
				TW_INFO info=(TW_INFO)Marshal.PtrToStructure(p,typeof(TW_INFO));
				Assert.AreEqual(Tw.TWPF_CHOCOLATE,(ushort)(info.Item.ToInt64() & 0xFFFF));
			}
			finally {
				Marshal.FreeHGlobal(p);
			}
		}

		[TestMethod]
		public void SetWindowLongPtr_SetsTheOwnerInEitherBitness() {
			//GWL_HWNDPARENT takes a window handle: SetWindowLongPtr in a 64-bit process, SetWindowLong in a 32-bit one. Two hidden windows, never shown.
			const uint GW_OWNER=4;
			using(ProxyWindow proxyWindowOwner=new ProxyWindow())
			using(ProxyWindow proxyWindow=new ProxyWindow()) {
				NativeMethods.SetWindowLongPtr(proxyWindow.Handle,NativeMethods.GWL_HWNDPARENT,proxyWindowOwner.Handle);
				Assert.AreEqual(proxyWindowOwner.Handle,GetWindow(proxyWindow.Handle,GW_OWNER));
				IntPtr hwndPrevious=NativeMethods.SetWindowLongPtr(proxyWindow.Handle,NativeMethods.GWL_HWNDPARENT,IntPtr.Zero);
				Assert.AreEqual(proxyWindowOwner.Handle,hwndPrevious,"The previous owner comes back whole.");
				Assert.AreEqual(IntPtr.Zero,GetWindow(proxyWindow.Handle,GW_OWNER));
			}
		}

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindow(IntPtr hWnd,uint uCmd);

		[TestMethod]
		public void Identity_ProductNameRoundTripsThroughUnmanagedMemory() {
			IntPtr p=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TW_IDENTITY)));
			try {
				TW_IDENTITY identity=new TW_IDENTITY { Id=7,ProductName="Scanner 3000",Manufacturer="Maker" };
				identity.Version.Info="v";
				Marshal.StructureToPtr(identity,p,false);
				Assert.AreEqual(7,Marshal.ReadInt32(p,0));
				Assert.AreEqual((byte)'S',Marshal.ReadByte(p,122));
				TW_IDENTITY copy=(TW_IDENTITY)Marshal.PtrToStructure(p,typeof(TW_IDENTITY));
				Assert.AreEqual("Scanner 3000",copy.ProductName);
				Assert.AreEqual("Maker",copy.Manufacturer);
			}
			finally {
				Marshal.FreeHGlobal(p);
			}
		}

		[TestMethod]
		public void Fix32_ConvertsBothWays() {
			TW_FIX32 fix=TW_FIX32.FromDouble(300);
			Assert.AreEqual(300,fix.Whole);
			Assert.AreEqual(0,fix.Frac);
			Assert.AreEqual(300.0,fix.ToDouble());
			fix=TW_FIX32.FromDouble(72.5);
			Assert.AreEqual(72,fix.Whole);
			Assert.AreEqual(32768,fix.Frac);
			Assert.AreEqual(72.5,fix.ToDouble());
			fix=TW_FIX32.FromDouble(-1.5);
			Assert.AreEqual(-1.5,fix.ToDouble());
			//In a TW_ONEVALUE, Whole is the low word of the Item field and Frac the high word.
			uint bits=TW_FIX32.FromDouble(150.25).ToUInt32();
			Assert.AreEqual(150u,bits&0xffff);
			Assert.AreEqual(16384u,bits>>16);
			Assert.AreEqual(150.25,TW_FIX32.FromUInt32(bits).ToDouble());
		}

		[TestMethod]
		public void ErrorCodes_KeepTheNumbersCallersCompareAgainst() {
			Assert.AreEqual(0,(int)EZTwainErrorCode.EZTEC_NONE);
			Assert.AreEqual(5,(int)EZTwainErrorCode.EZTEC_DS_FAILURE);
			Assert.AreEqual(19,(int)EZTwainErrorCode.EZTEC_USER_CANCEL);
			Assert.AreEqual(22,(int)EZTwainErrorCode.EZTEC_JPEG_DLL);
			Assert.AreEqual(38,(int)EZTwainErrorCode.EZTEC_0_PAGES);
			Assert.AreEqual(43,(int)EZTwainErrorCode.EZTEC_NO_PDF);
			Assert.AreEqual(76,(int)EZTwainErrorCode.EZTEC_DEVICE_PAPERJAM);
			Assert.AreEqual(2,EZTwain.XFERMECH_MEMORY);
		}

		[TestMethod]
		public void FindSource_PrefersExactMatchThenPrefix() {
			List<TW_IDENTITY> listIdentities=new List<TW_IDENTITY> {
				new TW_IDENTITY { ProductName="Sensor Pro 2" },
				new TW_IDENTITY { ProductName="Sensor" },
				new TW_IDENTITY { ProductName="Flatbed X" },
			};
			Assert.AreEqual("Sensor",TwainSession.FindSource(listIdentities,"sensor").Value.ProductName);
			Assert.AreEqual("Flatbed X",TwainSession.FindSource(listIdentities,"Flatbed").Value.ProductName);
			Assert.AreEqual("Sensor Pro 2",TwainSession.FindSource(listIdentities," Sensor Pro 2 ").Value.ProductName);
			Assert.IsNull(TwainSession.FindSource(listIdentities,"Camera"));
			Assert.IsNull(TwainSession.FindSource(listIdentities," "));
		}

		[TestMethod]
		public void ChooseBufferSize_RespectsSourceLimits() {
			Assert.AreEqual(65536,TwainSession.ChooseBufferSize(Tw.TWON_DONTCARE32,Tw.TWON_DONTCARE32,Tw.TWON_DONTCARE32,65536,1<<24));
			Assert.AreEqual(32000,TwainSession.ChooseBufferSize(1000,Tw.TWON_DONTCARE32,32000,65536,1<<24));
			Assert.AreEqual(100000,TwainSession.ChooseBufferSize(100000,Tw.TWON_DONTCARE32,Tw.TWON_DONTCARE32,65536,1<<24));
			Assert.AreEqual(20000,TwainSession.ChooseBufferSize(1000,20000,Tw.TWON_DONTCARE32,65536,1<<24));
			Assert.AreEqual(1<<24,TwainSession.ChooseBufferSize(0,Tw.TWON_DONTCARE32,unchecked((uint)int.MaxValue),65536,1<<24));
			Assert.AreEqual(1024,TwainSession.ChooseBufferSize(0,Tw.TWON_DONTCARE32,10,65536,1<<24));
		}

		[TestMethod]
		public void ConditionText_DescribesPaperJam() {
			StringAssert.Contains(TwainSession.GetConditionText(Tw.TWCC_PAPERJAM),"paper jam");
			StringAssert.Contains(TwainSession.GetConditionText(999),"999");
		}

		[TestMethod]
		public void NativeDsm_PrefersTwainDsmOverLegacyTwain32() {
			List<string> listPaths=NativeTwainDsm.GetCandidatePaths(false);
			Assert.AreEqual(3,listPaths.Count);
			StringAssert.EndsWith(listPaths[0],"TWAINDSM.dll");
			StringAssert.EndsWith(listPaths[1],"TWAINDSM.dll");
			StringAssert.EndsWith(listPaths[2],"twain_32.dll");
			foreach(string path in listPaths) {
				Assert.IsTrue(System.IO.Path.IsPathRooted(path),"DSM paths must be absolute: "+path);
			}
		}

		[TestMethod]
		public void NativeDsm_64BitProcessLoadsOnlyTheSystemTwainDsm() {
			//twain_32.dll is 32-bit only; a 64-bit process takes the 64-bit TWAINDSM.dll from System32 and nothing beside the program.
			List<string> listPaths=NativeTwainDsm.GetCandidatePaths(true);
			Assert.AreEqual(1,listPaths.Count);
			Assert.AreEqual(System.IO.Path.Combine(Environment.SystemDirectory,"TWAINDSM.dll"),listPaths[0]);
		}

		[TestMethod]
		public void NativeDsm_ThisProcessTriesTheListOfItsBitness() {
			CollectionAssert.AreEqual(NativeTwainDsm.GetCandidatePaths(Environment.Is64BitProcess),NativeTwainDsm.GetCandidatePaths());
			Assert.AreEqual(Environment.Is64BitProcess ? 1 : 3,NativeTwainDsm.GetCandidatePaths().Count);
		}

		[TestMethod]
		public void Twain_ActivateEZTwainIsANoOpThatSucceeds() {
			Assert.IsTrue(Twain.ActivateEZTwain());
		}
	}
}
