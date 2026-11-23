// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Runtime.InteropServices;

namespace ImagingCapture.Uvc.Interop {
	///<summary>The few Media Foundation calls the camera session's video recording needs (IMG-P4c), written in the repository from the Windows
	///SDK headers (mfapi.h, mfobjects.h, mfreadwrite.h, 10.0.26100): no NuGet package and no vendor DLL (design 146). The sink writer turns
	///RGB32 frames into H.264 in an MP4 file with Windows' own encoder, so no codec is installed or licensed separately. Methods that are not
	///called are Unused_ placeholders that keep each interface's vtable order (InteropLayoutTests pins the called slots).</summary>
	internal static class Mf {
		///<summary>MF_VERSION: MF_SDK_VERSION 0x0002 &lt;&lt; 16 | MF_API_VERSION 0x0070.</summary>
		public const int MF_VERSION=0x00020070;
		///<summary>MFSTARTUP_LITE: no sockets (local files only).</summary>
		public const int MFSTARTUP_LITE=1;
		///<summary>MFVideoInterlace_Progressive.</summary>
		public const int MFVideoInterlace_Progressive=2;

		public static readonly Guid MF_MT_MAJOR_TYPE=new Guid("48eba18e-f8c9-4687-bf11-0a74c9f96a8f");
		public static readonly Guid MF_MT_SUBTYPE=new Guid("f7e34c9a-42e8-4714-b74b-cb29d72c35e5");
		public static readonly Guid MF_MT_AVG_BITRATE=new Guid("20332624-fb0d-4d9e-bd0d-cbf6786c102e");
		public static readonly Guid MF_MT_INTERLACE_MODE=new Guid("e2724bb8-e676-4806-b4b2-a8d6efb44ccd");
		public static readonly Guid MF_MT_FRAME_SIZE=new Guid("1652c33d-d6b2-4012-b834-72030849a37d");
		public static readonly Guid MF_MT_FRAME_RATE=new Guid("c459a2e8-3d2c-4e44-b132-fee5156c7bb0");
		public static readonly Guid MF_MT_PIXEL_ASPECT_RATIO=new Guid("c6376a1e-8d0a-4027-be45-6d9a0ad39bb6");
		public static readonly Guid MF_MT_DEFAULT_STRIDE=new Guid("644b4e48-1e02-4516-b0eb-c01ca9d49ac6");
		public static readonly Guid MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS=new Guid("a634a91c-822b-41b9-a494-4de4643612b0");
		public static readonly Guid MF_TRANSCODE_CONTAINERTYPE=new Guid("150ff23f-4abc-478b-ac4f-e1916fba1cca");
		///<summary>Fragmented MP4: what is written stays playable if the program or Windows stops before the file is finished.</summary>
		public static readonly Guid MFTranscodeContainerType_FMPEG4=new Guid("9ba876f1-419f-4b77-a1e0-35959d9d4004");
		public static readonly Guid MFTranscodeContainerType_MPEG4=new Guid("dc6cd05d-b9d0-40ef-bd35-fa622c1ab28a");
		///<summary>Lets the source reader convert the decoded frames to RGB32 (IMG-P4d frames).</summary>
		public static readonly Guid MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING=new Guid("fb394f3d-ccf1-42ee-bbb3-f9b845d5681d");
		///<summary>The length of the presentation, a VT_UI8 in 100-nanosecond units.</summary>
		public static readonly Guid MF_PD_DURATION=new Guid("6c990d33-bb8e-477a-8598-0d5d96fcd88a");
		///<summary>MF_SOURCE_READER_FIRST_VIDEO_STREAM (0xfffffffc).</summary>
		public const int MF_SOURCE_READER_FIRST_VIDEO_STREAM=unchecked((int)0xfffffffc);
		///<summary>MF_SOURCE_READER_ALL_STREAMS (0xfffffffe).</summary>
		public const int MF_SOURCE_READER_ALL_STREAMS=unchecked((int)0xfffffffe);
		///<summary>MF_SOURCE_READER_MEDIASOURCE (0xffffffff).</summary>
		public const int MF_SOURCE_READER_MEDIASOURCE=unchecked((int)0xffffffff);
		///<summary>MF_SOURCE_READERF_ENDOFSTREAM.</summary>
		public const int MF_SOURCE_READERF_ENDOFSTREAM=0x2;
		public const ushort VT_I8=20;
		public const ushort VT_UI8=21;
		public static readonly Guid MFMediaType_Video=new Guid("73646976-0000-0010-8000-00aa00389b71");
		///<summary>FCC('H264').</summary>
		public static readonly Guid MFVideoFormat_H264=new Guid("34363248-0000-0010-8000-00aa00389b71");
		///<summary>D3DFMT_X8R8G8B8 (22): 32-bit BGRX.</summary>
		public static readonly Guid MFVideoFormat_RGB32=new Guid("00000016-0000-0010-8000-00aa00389b71");

		[DllImport("mfplat.dll",ExactSpelling=true)]
		public static extern int MFStartup(int version,int dwFlags);

		[DllImport("mfplat.dll",ExactSpelling=true)]
		public static extern int MFShutdown();

		[DllImport("mfplat.dll",ExactSpelling=true)]
		public static extern int MFCreateAttributes(out IMFAttributes ppMFAttributes,int cInitialSize);

		[DllImport("mfplat.dll",ExactSpelling=true)]
		public static extern int MFCreateMediaType(out IMFAttributes ppMFType);

		[DllImport("mfplat.dll",ExactSpelling=true)]
		public static extern int MFCreateSample(out IMFSample ppIMFSample);

		[DllImport("mfplat.dll",ExactSpelling=true)]
		public static extern int MFCreateMemoryBuffer(int cbMaxLength,out IMFMediaBuffer ppBuffer);

		[DllImport("mfreadwrite.dll",ExactSpelling=true)]
		public static extern int MFCreateSourceReaderFromURL([MarshalAs(UnmanagedType.LPWStr)] string pwszURL,IMFAttributes pAttributes,
			out IMFSourceReader ppSourceReader);

		[DllImport("mfreadwrite.dll",ExactSpelling=true)]
		public static extern int MFCreateSinkWriterFromURL([MarshalAs(UnmanagedType.LPWStr)] string pwszOutputURL,IntPtr pByteStream,
			IMFAttributes pAttributes,out IMFSinkWriter ppSinkWriter);

		///<summary>Two 32-bit values packed as Media Foundation packs a size or a ratio: high, then low.</summary>
		public static long Pack(int high,int low) {
			return ((long)(uint)high<<32)|(uint)low;
		}

		public static void Check(int hr,string what) {
			if(hr<0) {
				throw new MfException(what+" failed (HRESULT 0x"+hr.ToString("X8")+").",hr);
			}
		}
	}

	///<summary>A Media Foundation call that failed, with its HRESULT.</summary>
	public sealed class MfException:Exception {
		public MfException(string message,int hResult):base(message) {
			HResult=hResult;
		}
	}

	///<summary>IMFAttributes (mfobjects.h), to SetGUID/SetUINT32/SetUINT64. A media type is an IMFAttributes (IMFMediaType derives from it), so
	///MFCreateMediaType's object is used through this interface and passed on as itself.</summary>
	[ComImport,Guid("2cd2d921-c447-44a7-a13c-4adabfc247e3"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMFAttributes {
		int Unused_GetItem();
		int Unused_GetItemType();
		int Unused_CompareItem();
		int Unused_Compare();
		[PreserveSig]
		int GetUINT32([In,MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,out int punValue);
		[PreserveSig]
		int GetUINT64([In,MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,out long punValue);
		int Unused_GetDouble();
		int Unused_GetGUID();
		int Unused_GetStringLength();
		int Unused_GetString();
		int Unused_GetAllocatedString();
		int Unused_GetBlobSize();
		int Unused_GetBlob();
		int Unused_GetAllocatedBlob();
		int Unused_GetUnknown();
		int Unused_SetItem();
		int Unused_DeleteItem();
		int Unused_DeleteAllItems();
		[PreserveSig]
		int SetUINT32([In,MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,int unValue);
		[PreserveSig]
		int SetUINT64([In,MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,long unValue);
		int Unused_SetDouble();
		[PreserveSig]
		int SetGUID([In,MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,[In,MarshalAs(UnmanagedType.LPStruct)] Guid guidValue);
	}

	///<summary>IMFSample (mfobjects.h): IMFAttributes' 30 methods, then the sample's own.</summary>
	[ComImport,Guid("c40a00f2-b93a-4d80-ae8c-5a1c634f58e4"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMFSample {
		int Unused_GetItem();
		int Unused_GetItemType();
		int Unused_CompareItem();
		int Unused_Compare();
		int Unused_GetUINT32();
		int Unused_GetUINT64();
		int Unused_GetDouble();
		int Unused_GetGUID();
		int Unused_GetStringLength();
		int Unused_GetString();
		int Unused_GetAllocatedString();
		int Unused_GetBlobSize();
		int Unused_GetBlob();
		int Unused_GetAllocatedBlob();
		int Unused_GetUnknown();
		int Unused_SetItem();
		int Unused_DeleteItem();
		int Unused_DeleteAllItems();
		int Unused_SetUINT32();
		int Unused_SetUINT64();
		int Unused_SetDouble();
		int Unused_SetGUID();
		int Unused_SetString();
		int Unused_SetBlob();
		int Unused_SetUnknown();
		int Unused_LockStore();
		int Unused_UnlockStore();
		int Unused_GetCount();
		int Unused_GetItemByIndex();
		int Unused_CopyAllItems();
		int Unused_GetSampleFlags();
		int Unused_SetSampleFlags();
		int Unused_GetSampleTime();
		///<summary>In 100-nanosecond units.</summary>
		[PreserveSig]
		int SetSampleTime(long hnsSampleTime);
		int Unused_GetSampleDuration();
		[PreserveSig]
		int SetSampleDuration(long hnsSampleDuration);
		int Unused_GetBufferCount();
		int Unused_GetBufferByIndex();
		[PreserveSig]
		int ConvertToContiguousBuffer(out IMFMediaBuffer ppBuffer);
		[PreserveSig]
		int AddBuffer(IMFMediaBuffer pBuffer);
	}

	///<summary>IMFMediaBuffer (mfobjects.h).</summary>
	[ComImport,Guid("045FA593-8799-42b8-BC8D-8968C6453507"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMFMediaBuffer {
		[PreserveSig]
		int Lock(out IntPtr ppbBuffer,out int pcbMaxLength,out int pcbCurrentLength);
		[PreserveSig]
		int Unlock();
		int Unused_GetCurrentLength();
		[PreserveSig]
		int SetCurrentLength(int cbCurrentLength);
	}

	///<summary>IMFSinkWriter (mfreadwrite.h).</summary>
	[ComImport,Guid("3137f1cd-fe5e-4805-a5d8-fb477448cb3d"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMFSinkWriter {
		[PreserveSig]
		int AddStream(IMFAttributes pTargetMediaType,out int pdwStreamIndex);
		[PreserveSig]
		int SetInputMediaType(int dwStreamIndex,IMFAttributes pInputMediaType,IMFAttributes pEncodingParameters);
		[PreserveSig]
		int BeginWriting();
		[PreserveSig]
		int WriteSample(int dwStreamIndex,IMFSample pSample);
		int Unused_SendStreamTick();
		int Unused_PlaceMarker();
		int Unused_NotifyEndOfSegment();
		int Unused_Flush();
		///<summary>IMFSinkWriter::Finalize: completes the file.</summary>
		[PreserveSig]
		int FinalizeWriting();
	}

	///<summary>A PROPVARIANT holding a 64-bit integer (VT_I8 or VT_UI8): vt at 0, the value at 8. 24 bytes, the size of a PROPVARIANT in a
	///64-bit process (16 in a 32-bit one, where the last 8 are not read).</summary>
	[StructLayout(LayoutKind.Explicit,Size=24)]
	internal struct PropVariantLong {
		[FieldOffset(0)]
		public ushort vt;
		[FieldOffset(8)]
		public long Value;
	}

	///<summary>IMFSourceReader (mfreadwrite.h): reads a video file's frames, for "Save frame as photo" and the tree's picture (IMG-P4d).</summary>
	[ComImport,Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMFSourceReader {
		int Unused_GetStreamSelection();
		[PreserveSig]
		int SetStreamSelection(int dwStreamIndex,[MarshalAs(UnmanagedType.Bool)] bool fSelected);
		int Unused_GetNativeMediaType();
		[PreserveSig]
		int GetCurrentMediaType(int dwStreamIndex,out IMFAttributes ppMediaType);
		[PreserveSig]
		int SetCurrentMediaType(int dwStreamIndex,IntPtr pdwReserved,IMFAttributes pMediaType);
		[PreserveSig]
		int SetCurrentPosition([In,MarshalAs(UnmanagedType.LPStruct)] Guid guidTimeFormat,[In] ref PropVariantLong varPosition);
		[PreserveSig]
		int ReadSample(int dwStreamIndex,int dwControlFlags,out int pdwActualStreamIndex,out int pdwStreamFlags,out long pllTimestamp,
			out IMFSample ppSample);
		int Unused_Flush();
		int Unused_GetServiceForStream();
		[PreserveSig]
		int GetPresentationAttribute(int dwStreamIndex,[In,MarshalAs(UnmanagedType.LPStruct)] Guid guidAttribute,out PropVariantLong pvarAttribute);
	}
}
