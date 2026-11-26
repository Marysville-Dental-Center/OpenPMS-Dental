// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Runtime.InteropServices;

namespace ImagingCapture.Uvc.Interop {
	//Camera control interfaces, declared by hand from the Windows SDK documentation (strmif.h, ks.h, ksmedia.h). This layer reads and
	//writes every standard camera setting the driver offers (design 8.7, decision 13 of 2026-11-26): GetRange, Get and Set, and IKsControl
	//for the two settings the interfaces lack (power line frequency, low light compensation). Set and KsProperty with SET are called only
	//from DirectShowFrameSource.WriteControlOnWorker; a unit test checks it. The rules are in DeviceEnumInterfaces.cs.

	///<summary>IAMCameraControl, exposed by the capture source filter. Properties 0-6 (CameraControlProperty).</summary>
	[ComImport,Guid("C6E13370-30AC-11D0-A18C-00A0C9118956"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAMCameraControl {
		[PreserveSig]
		int GetRange(int Property,out int pMin,out int pMax,out int pSteppingDelta,out int pDefault,out int pCapsFlags);
		///<summary>Called only by DirectShowFrameSource.WriteControlOnWorker. Flags 1 = automatic, 2 = manual.</summary>
		[PreserveSig]
		int Set(int Property,int lValue,int Flags);
		[PreserveSig]
		int Get(int Property,out int lValue,out int Flags);
	}

	///<summary>IAMVideoProcAmp, exposed by the capture source filter. Properties 0-9 (VideoProcAmpProperty).</summary>
	[ComImport,Guid("C6E13360-30AC-11D0-A18C-00A0C9118956"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAMVideoProcAmp {
		[PreserveSig]
		int GetRange(int Property,out int pMin,out int pMax,out int pSteppingDelta,out int pDefault,out int pCapsFlags);
		///<summary>Called only by DirectShowFrameSource.WriteControlOnWorker. Flags 1 = automatic, 2 = manual.</summary>
		[PreserveSig]
		int Set(int Property,int lValue,int Flags);
		[PreserveSig]
		int Get(int Property,out int lValue,out int Flags);
	}

	///<summary>IKsControl (ks.h), exposed by the capture source filter (ksproxy). Used only for the two property ids the IAM interfaces do not
	///cover (KsControlProperty). The buffers are unmanaged; KsMethod and KsEvent keep their vtable slots and are not called.</summary>
	[ComImport,Guid("28F54685-06FD-11D2-B27A-00A0C9223196"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IKsControl {
		[PreserveSig]
		int KsProperty(IntPtr Property,int PropertyLength,IntPtr PropertyData,int DataLength,out int BytesReturned);
		[PreserveSig]
		int KsMethod(IntPtr Method,int MethodLength,IntPtr MethodData,int DataLength,out int BytesReturned);
		[PreserveSig]
		int KsEvent(IntPtr Event,int EventLength,IntPtr EventData,int DataLength,out int BytesReturned);
	}
}
