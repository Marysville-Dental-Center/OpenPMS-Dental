// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ImagingCapture.Uvc.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingCapture.Tests {
	///<summary>Pins the hand-written DirectShow declarations to the Windows SDK: structure sizes and offsets in a 32-bit and a 64-bit process
	///(an x86 and an x64 column), and the vtable slot of every member that is called. A wrong slot would call the wrong method at run time.</summary>
	[TestClass]
	public class InteropLayoutTests {
		private static int Offset(Type type,string field) {
			return (int)Marshal.OffsetOf(type,field).ToInt64();
		}

		///<summary>x86 or x64 by the bitness of this test process: the expected sizes and offsets have one column for each (S3a-12 of the .NET 10
		///move). The lane runs this assembly in both.</summary>
		private static int Pick(int x86,int x64) {
			return IntPtr.Size==8 ? x64 : x86;
		}

		///<summary>(structure, size in a 32-bit process, size in a 64-bit process), from the Windows SDK headers with their default (natural)
		///alignment: AM_MEDIA_TYPE (strmif.h) holds an IUnknown pointer and a BYTE pointer, so it grows from 72 to 88 bytes; the others hold no
		///pointer (REFERENCE_TIME and LONGLONG are 8 bytes and 8-aligned in both).</summary>
		private static readonly List<Tuple<Type,int,int>> _listSizes=new List<Tuple<Type,int,int>> {
			Tuple.Create(typeof(AM_MEDIA_TYPE),72,88),
			Tuple.Create(typeof(RECT),16,16),
			Tuple.Create(typeof(SIZE),8,8),
			Tuple.Create(typeof(BITMAPINFOHEADER),40,40),
			Tuple.Create(typeof(VIDEOINFOHEADER),88,88),
			Tuple.Create(typeof(VIDEO_STREAM_CONFIG_CAPS),128,128),
		};

		///<summary>(structure, field, offset in a 32-bit process, offset in a 64-bit process), the fields this layer reads or writes.</summary>
		private static readonly List<Tuple<Type,string,int,int>> _listOffsets=new List<Tuple<Type,string,int,int>> {
			Tuple.Create(typeof(AM_MEDIA_TYPE),"subtype",16,16),
			Tuple.Create(typeof(AM_MEDIA_TYPE),"lSampleSize",40,40),
			Tuple.Create(typeof(AM_MEDIA_TYPE),"formattype",44,44),
			Tuple.Create(typeof(AM_MEDIA_TYPE),"pUnk",60,64),//8-aligned in a 64-bit process: 4 bytes of padding after formattype
			Tuple.Create(typeof(AM_MEDIA_TYPE),"cbFormat",64,72),
			Tuple.Create(typeof(AM_MEDIA_TYPE),"pbFormat",68,80),//8-aligned: 4 bytes of padding after cbFormat
			Tuple.Create(typeof(VIDEOINFOHEADER),"rcTarget",16,16),
			Tuple.Create(typeof(VIDEOINFOHEADER),"AvgTimePerFrame",40,40),
			Tuple.Create(typeof(VIDEOINFOHEADER),"bmiHeader",48,48),
			Tuple.Create(typeof(BITMAPINFOHEADER),"biBitCount",14,14),
			Tuple.Create(typeof(BITMAPINFOHEADER),"biCompression",16,16),
			Tuple.Create(typeof(VIDEO_STREAM_CONFIG_CAPS),"MinOutputSize",60,60),
			Tuple.Create(typeof(VIDEO_STREAM_CONFIG_CAPS),"MaxOutputSize",68,68),
			Tuple.Create(typeof(VIDEO_STREAM_CONFIG_CAPS),"MinFrameInterval",104,104),
			Tuple.Create(typeof(VIDEO_STREAM_CONFIG_CAPS),"MaxFrameInterval",112,112),
			Tuple.Create(typeof(VIDEO_STREAM_CONFIG_CAPS),"MaxBitsPerSecond",124,124),
		};

		[TestMethod]
		public void Process_IsX86OrX64() {
			CollectionAssert.Contains(new[] { 4,8 },IntPtr.Size,"IntPtr.Size is "+IntPtr.Size+".");
			Assert.AreEqual(Environment.Is64BitProcess,IntPtr.Size==8);
			Console.WriteLine("DirectShow layout tests ran in a "+(IntPtr.Size*8)+"-bit process.");
		}

		[TestMethod]
		public void StructSizes_MatchTheSdk() {
			foreach(Tuple<Type,int,int> tuple in _listSizes) {
				Assert.AreEqual(Pick(tuple.Item2,tuple.Item3),Marshal.SizeOf(tuple.Item1),tuple.Item1.Name);
			}
			//Every structure of the interop layer has a row.
			List<Type> listTypes=typeof(Ds).Assembly.GetTypes()
				.Where(x => x.IsValueType && !x.IsEnum && x.Namespace==typeof(Ds).Namespace && x.IsLayoutSequential)
				.ToList();
			foreach(Type type in listTypes) {
				Assert.IsTrue(_listSizes.Any(x => x.Item1==type),type.Name+" has no row in the size table.");
			}
		}

		[TestMethod]
		public void FieldOffsets_MatchTheSdk() {
			foreach(Tuple<Type,string,int,int> tuple in _listOffsets) {
				Assert.AreEqual(Pick(tuple.Item3,tuple.Item4),Offset(tuple.Item1,tuple.Item2),tuple.Item1.Name+"."+tuple.Item2);
			}
		}

		[TestMethod]
		public void MediaType_PointersRoundTripThroughUnmanagedMemory() {
			//pUnk and pbFormat keep all their bits, which a 4-byte field in a 64-bit process would not.
			AM_MEDIA_TYPE mediaType=new AM_MEDIA_TYPE();
			mediaType.formattype=Ds.MEDIATYPE_Video;
			mediaType.pUnk=new IntPtr(IntPtr.Size==8 ? unchecked((long)0x7FFE12345678ABC0) : 0x12345670);
			mediaType.cbFormat=88;
			mediaType.pbFormat=new IntPtr(IntPtr.Size==8 ? unchecked((long)0x00007FF0DEADBEE8) : 0x0DEADBE8);
			IntPtr p=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(AM_MEDIA_TYPE)));
			try {
				Marshal.StructureToPtr(mediaType,p,false);
				Assert.AreEqual(mediaType.pUnk,Marshal.ReadIntPtr(p,Offset(typeof(AM_MEDIA_TYPE),"pUnk")));
				Assert.AreEqual(88,Marshal.ReadInt32(p,Offset(typeof(AM_MEDIA_TYPE),"cbFormat")));
				Assert.AreEqual(mediaType.pbFormat,Marshal.ReadIntPtr(p,Offset(typeof(AM_MEDIA_TYPE),"pbFormat")));
				AM_MEDIA_TYPE copy=(AM_MEDIA_TYPE)Marshal.PtrToStructure(p,typeof(AM_MEDIA_TYPE));
				Assert.AreEqual(mediaType.pbFormat,copy.pbFormat);
			}
			finally {
				Marshal.FreeHGlobal(p);
			}
		}

		///<summary>(interface, member, slot). Slots count from 0 and include IUnknown (3 members) and, for dual interfaces,
		///IDispatch (4 more). Each expected slot is the member's position in the SDK header's vtable.</summary>
		private static readonly List<Tuple<Type,string,int>> _listSlots=new List<Tuple<Type,string,int>> {
			Tuple.Create(typeof(ICreateDevEnum),"CreateClassEnumerator",3),
			Tuple.Create(typeof(IEnumMoniker),"Next",3),
			//IMoniker: IPersist (1) and IPersistStream (4) come before IMoniker's own members.
			Tuple.Create(typeof(IMoniker),"BindToObject",8),
			Tuple.Create(typeof(IMoniker),"BindToStorage",9),
			Tuple.Create(typeof(IMoniker),"GetDisplayName",20),
			Tuple.Create(typeof(IPropertyBag),"Read",3),
			Tuple.Create(typeof(IGraphBuilder),"AddFilter",3),
			Tuple.Create(typeof(IGraphBuilder),"RemoveFilter",4),
			Tuple.Create(typeof(IGraphBuilder),"Disconnect",9),
			Tuple.Create(typeof(ICaptureGraphBuilder2),"SetFiltergraph",3),
			Tuple.Create(typeof(ICaptureGraphBuilder2),"FindInterface",6),
			Tuple.Create(typeof(ICaptureGraphBuilder2),"RenderStream",7),
			Tuple.Create(typeof(ICaptureGraphBuilder2),"FindPin",11),
			Tuple.Create(typeof(IMediaControl),"Run",7),
			Tuple.Create(typeof(IMediaControl),"Stop",9),
			Tuple.Create(typeof(IMediaControl),"GetState",10),
			Tuple.Create(typeof(IMediaEventEx),"GetEventHandle",7),
			Tuple.Create(typeof(IMediaEventEx),"GetEvent",8),
			Tuple.Create(typeof(IMediaEventEx),"FreeEventParams",12),
			//IBaseFilter: IPersist (1) and IMediaFilter (6) come before EnumPins.
			Tuple.Create(typeof(IBaseFilter),"EnumPins",10),
			Tuple.Create(typeof(IEnumPins),"Next",3),
			Tuple.Create(typeof(IPin),"ConnectedTo",6),
			Tuple.Create(typeof(IPin),"QueryDirection",9),
			Tuple.Create(typeof(IAMStreamConfig),"SetFormat",3),
			Tuple.Create(typeof(IAMStreamConfig),"GetNumberOfCapabilities",5),
			Tuple.Create(typeof(IAMStreamConfig),"GetStreamCaps",6),
			Tuple.Create(typeof(ISampleGrabber),"SetOneShot",3),
			Tuple.Create(typeof(ISampleGrabber),"SetMediaType",4),
			Tuple.Create(typeof(ISampleGrabber),"GetConnectedMediaType",5),
			Tuple.Create(typeof(ISampleGrabber),"SetBufferSamples",6),
			Tuple.Create(typeof(ISampleGrabber),"SetCallback",9),
			Tuple.Create(typeof(ISampleGrabberCB),"SampleCB",3),
			Tuple.Create(typeof(ISampleGrabberCB),"BufferCB",4),
			//IAMCameraControl and IAMVideoProcAmp: GetRange, Set, Get. Set is a placeholder that is never called.
			Tuple.Create(typeof(IAMCameraControl),"GetRange",3),
			Tuple.Create(typeof(IAMCameraControl),"Unused_Set",4),
			Tuple.Create(typeof(IAMCameraControl),"Get",5),
			Tuple.Create(typeof(IAMVideoProcAmp),"GetRange",3),
			Tuple.Create(typeof(IAMVideoProcAmp),"Unused_Set",4),
			Tuple.Create(typeof(IAMVideoProcAmp),"Get",5),
			//Media Foundation (IMG-P4c video recording; mfobjects.h and mfreadwrite.h, SDK 10.0.26100): IMFAttributes' 30 methods come first in IMFSample.
			Tuple.Create(typeof(IMFAttributes),"GetUINT32",7),
			Tuple.Create(typeof(IMFAttributes),"GetUINT64",8),
			Tuple.Create(typeof(IMFAttributes),"GetBlob",15),
			Tuple.Create(typeof(IMFAttributes),"SetUINT32",21),
			Tuple.Create(typeof(IMFAttributes),"SetUINT64",22),
			Tuple.Create(typeof(IMFAttributes),"SetGUID",24),
			Tuple.Create(typeof(IMFSample),"SetSampleTime",36),
			Tuple.Create(typeof(IMFSample),"SetSampleDuration",38),
			Tuple.Create(typeof(IMFSample),"ConvertToContiguousBuffer",41),
			Tuple.Create(typeof(IMFSample),"AddBuffer",42),
			Tuple.Create(typeof(IMFMediaBuffer),"Lock",3),
			Tuple.Create(typeof(IMFMediaBuffer),"Unlock",4),
			Tuple.Create(typeof(IMFMediaBuffer),"SetCurrentLength",6),
			Tuple.Create(typeof(IMFSinkWriter),"AddStream",3),
			Tuple.Create(typeof(IMFSinkWriter),"SetInputMediaType",4),
			Tuple.Create(typeof(IMFSinkWriter),"BeginWriting",5),
			Tuple.Create(typeof(IMFSinkWriter),"WriteSample",6),
			Tuple.Create(typeof(IMFSinkWriter),"FinalizeWriting",11),
			//IMFSourceReader (IMG-P4d frames)
			Tuple.Create(typeof(IMFSourceReader),"SetStreamSelection",4),
			Tuple.Create(typeof(IMFSourceReader),"GetCurrentMediaType",6),
			Tuple.Create(typeof(IMFSourceReader),"SetCurrentMediaType",7),
			Tuple.Create(typeof(IMFSourceReader),"SetCurrentPosition",8),
			Tuple.Create(typeof(IMFSourceReader),"ReadSample",9),
			Tuple.Create(typeof(IMFSourceReader),"GetPresentationAttribute",12),
		};

		///<summary>The interface's methods in the order they are declared, which is the order of their vtable slots: the runtime lays out a COM
		///interface's vtable in metadata order (the MethodDef rows, numbered in source order), after IUnknown's three slots, or after IDispatch's
		///seven for a dual or IDispatch interface. Read from the metadata tokens, not from Marshal.GetComSlotForMethodInfo (S3a-8 of the .NET 10
		///move: that is a .NET Framework COM interop call; Type.GetMethods promises no order).</summary>
		internal static List<MethodInfo> GetMethodsInSlotOrder(Type type) {
			return type.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)
				.OrderBy(x => x.MetadataToken).ToList();
		}

		///<summary>The vtable slot of each declared method of the interface (GetMethodsInSlotOrder), by name.</summary>
		internal static Dictionary<string,int> GetComSlots(Type type) {
			InterfaceTypeAttribute interfaceTypeAttribute=(InterfaceTypeAttribute)Attribute.GetCustomAttribute(type,typeof(InterfaceTypeAttribute));
			Assert.IsNotNull(interfaceTypeAttribute,type.Name+" names no InterfaceType.");
			int slotFirst=interfaceTypeAttribute.Value==ComInterfaceType.InterfaceIsIUnknown ? 3 : 7;//IUnknown; IDispatch (dual, IDispatch only)
			List<MethodInfo> listMethods=GetMethodsInSlotOrder(type);
			Dictionary<string,int> dictSlots=new Dictionary<string,int>();
			for(int i=0;i<listMethods.Count;i++) {
				dictSlots.Add(listMethods[i].Name,slotFirst+i);
			}
			return dictSlots;
		}

		[TestMethod]
		public void VtableSlots_MatchTheSdkOrder() {
			foreach(Tuple<Type,string,int> tuple in _listSlots) {
				MethodInfo methodInfo=tuple.Item1.GetMethod(tuple.Item2);
				Assert.IsNotNull(methodInfo,tuple.Item1.Name+"."+tuple.Item2+" is not declared.");
				Assert.AreEqual(tuple.Item3,GetComSlots(tuple.Item1)[tuple.Item2],tuple.Item1.Name+"."+tuple.Item2);
			}
		}

		[TestMethod]
		public void Interfaces_DeclareOnlyCalledMembersAndPlaceholders() {
			//Every member is either listed above (it is called, and its slot is pinned) or a placeholder (Unused_*). The last
			//declared member of each interface is a called one, so no placeholder trails the vtable.
			List<Type> listTypes=typeof(Ds).Assembly.GetTypes()
				.Where(x => x.IsInterface && x.Namespace==typeof(Ds).Namespace && x.IsDefined(typeof(ComImportAttribute),false))
				.ToList();
			Assert.AreEqual(21,listTypes.Count,"COM interfaces declared in ImagingCapture.Uvc.Interop (16 DirectShow, 5 Media Foundation).");
			foreach(Type type in listTypes) {
				MethodInfo[] arrayMethods=type.GetMethods();
				Assert.IsTrue(arrayMethods.Length>0,type.Name);
				foreach(MethodInfo methodInfo in arrayMethods) {
					bool isPinned=_listSlots.Any(x => x.Item1==type && x.Item2==methodInfo.Name);
					bool isPlaceholder=methodInfo.Name.StartsWith("Unused_");
					Assert.IsTrue(isPinned || isPlaceholder,type.Name+"."+methodInfo.Name+" is neither pinned nor a placeholder.");
					if(isPlaceholder) {
						Assert.AreEqual(0,methodInfo.GetParameters().Length,type.Name+"."+methodInfo.Name+" placeholder has parameters.");
					}
				}
				MethodInfo methodLast=GetMethodsInSlotOrder(type).Last();
				Assert.IsFalse(methodLast.Name.StartsWith("Unused_"),type.Name+" ends with a placeholder.");
			}
		}

		[TestMethod]
		public void FourCCSubtypes_ShareTheFourCCBase() {
			byte[] arrayBase=Ds.FourCCBase.ToByteArray();
			foreach(Guid guid in new[] { Ds.MEDIASUBTYPE_MJPG,Ds.MEDIASUBTYPE_YUY2,Ds.MEDIATYPE_Video }) {
				byte[] arrayBytes=guid.ToByteArray();
				CollectionAssert.AreEqual(arrayBase.Skip(4).ToArray(),arrayBytes.Skip(4).ToArray(),guid.ToString());
			}
			Assert.AreEqual(0x47504A4D,BitConverter.ToInt32(Ds.MEDIASUBTYPE_MJPG.ToByteArray(),0));//'M','J','P','G'
			Assert.AreEqual(0x32595559,BitConverter.ToInt32(Ds.MEDIASUBTYPE_YUY2.ToByteArray(),0));//'Y','U','Y','2'
		}
	}
}
