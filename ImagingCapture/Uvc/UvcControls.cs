// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using ImagingCapture.Uvc.Interop;

namespace ImagingCapture.Uvc {
	///<summary>The two standard camera control interfaces a UVC source filter exposes. The values are the [Trigger.ControlPoll]
	///and [Level.N] Interface names in capture.ini.</summary>
	public enum UvcControlInterface {
		///<summary>IAMCameraControl, properties 0-6.</summary>
		CameraControl=0,
		///<summary>IAMVideoProcAmp, properties 0-9.</summary>
		VideoProcAmp=1,
	}

	///<summary>The outcome of reading a camera control.</summary>
	public enum UvcControlStatus {
		Ok=0,
		///<summary>The device does not support this property (E_PROP_ID_UNSUPPORTED and similar). A trigger on it is disabled.</summary>
		Unsupported,
		///<summary>The device does not expose this control interface at all.</summary>
		NoInterface,
		///<summary>The device was removed.</summary>
		DeviceLost,
		///<summary>Any other failure. HResult says which.</summary>
		Failed,
		///<summary>The frame source was disposed.</summary>
		Closed,
	}

	///<summary>A control value read with Get. Value and Flags are meaningful only when Status is Ok.</summary>
	public struct UvcControlValue {
		public UvcControlStatus Status { get; }
		public int Value { get; }
		///<summary>The driver's flags: 1 = automatic, 2 = manual (CameraControl_Flags and VideoProcAmp_Flags).</summary>
		public int Flags { get; }
		public int HResult { get; }

		public UvcControlValue(UvcControlStatus status,int value,int flags,int hResult) {
			Status=status;
			Value=value;
			Flags=flags;
			HResult=hResult;
		}

		public bool IsOk {
			get {
				return Status==UvcControlStatus.Ok;
			}
		}
	}

	///<summary>A control range read with GetRange. The numbers are meaningful only when Status is Ok.</summary>
	public struct UvcControlRange {
		public UvcControlStatus Status { get; }
		public int Min { get; }
		public int Max { get; }
		public int Step { get; }
		public int Default { get; }
		///<summary>What the control supports: 1 = automatic, 2 = manual (both may be set).</summary>
		public int CapsFlags { get; }
		public int HResult { get; }

		///<summary>The camera can set this control automatically.</summary>
		public bool CanAuto {
			get {
				return (CapsFlags & UvcControls.FlagsAuto)!=0;
			}
		}

		public UvcControlRange(UvcControlStatus status,int min,int max,int step,int defaultValue,int capsFlags,int hResult) {
			Status=status;
			Min=min;
			Max=max;
			Step=step;
			Default=defaultValue;
			CapsFlags=capsFlags;
			HResult=hResult;
		}

		public bool IsOk {
			get {
				return Status==UvcControlStatus.Ok;
			}
		}
	}

	///<summary>The standard property numbers of the two control interfaces and their Windows SDK names (CameraControlProperty,
	///VideoProcAmpProperty), plus the two settings the driver's own property pages show that the two interfaces do not have: power line
	///frequency (VideoProcAmp 13) and low light compensation (CameraControl 19, KSPROPERTY_CAMERACONTROL_AUTO_EXPOSURE_PRIORITY), which are
	///read and written through IKsControl. Nothing here is specific to any camera.</summary>
	public static class UvcControls {
		///<summary>CameraControl_Flags_Auto, VideoProcAmp_Flags_Auto.</summary>
		public const int FlagsAuto=1;
		///<summary>CameraControl_Flags_Manual, VideoProcAmp_Flags_Manual.</summary>
		public const int FlagsManual=2;
		///<summary>KSPROPERTY_VIDEOPROCAMP_POWERLINE_FREQUENCY.</summary>
		public const int VideoProcAmpPowerLineFrequency=13;
		///<summary>KSPROPERTY_CAMERACONTROL_AUTO_EXPOSURE_PRIORITY, which the driver's page calls Low Light Compensation.</summary>
		public const int CameraControlLowLightCompensation=19;
		private static readonly int[] _arrayCameraControlProperties={ 0,1,2,3,4,5,6,CameraControlLowLightCompensation };
		private static readonly string[] _arrayCameraControlNames={ "Pan","Tilt","Roll","Zoom","Exposure","Iris","Focus","LowLightCompensation" };
		private static readonly int[] _arrayVideoProcAmpProperties={ 0,1,2,3,4,5,6,7,8,9,VideoProcAmpPowerLineFrequency };
		private static readonly string[] _arrayVideoProcAmpNames={ "Brightness","Contrast","Hue","Saturation","Sharpness","Gamma",
			"ColorEnable","WhiteBalance","BacklightCompensation","Gain","PowerLineFrequency" };

		///<summary>The property numbers of an interface, in order: CameraControl 0-6 and 19, VideoProcAmp 0-9 and 13.</summary>
		public static IReadOnlyList<int> GetProperties(UvcControlInterface controlInterface) {
			if(controlInterface==UvcControlInterface.CameraControl) {
				return _arrayCameraControlProperties;
			}
			if(controlInterface==UvcControlInterface.VideoProcAmp) {
				return _arrayVideoProcAmpProperties;
			}
			return new int[0];
		}

		public static bool IsValidProperty(UvcControlInterface controlInterface,int property) {
			return GetProperties(controlInterface).Contains(property);
		}

		///<summary>True for the two properties read and written through IKsControl instead of IAMCameraControl or IAMVideoProcAmp.</summary>
		public static bool IsExtendedProperty(UvcControlInterface controlInterface,int property) {
			return (controlInterface==UvcControlInterface.CameraControl && property==CameraControlLowLightCompensation)
				|| (controlInterface==UvcControlInterface.VideoProcAmp && property==VideoProcAmpPowerLineFrequency);
		}

		///<summary>The SDK name, for example "Focus" or "Gain", or the number for an invalid property.</summary>
		public static string GetPropertyName(UvcControlInterface controlInterface,int property) {
			if(!IsValidProperty(controlInterface,property)) {
				return property.ToString();
			}
			if(controlInterface==UvcControlInterface.CameraControl) {
				return _arrayCameraControlNames[Array.IndexOf(_arrayCameraControlProperties,property)];
			}
			return _arrayVideoProcAmpNames[Array.IndexOf(_arrayVideoProcAmpProperties,property)];
		}

		///<summary>The control whose SDK name is name (case ignored), across both interfaces; false when none. The names are unique.</summary>
		public static bool TryGetByName(string name,out UvcControlInterface controlInterface,out int property) {
			name=(name??"").Trim();
			foreach(UvcControlInterface controlInterfaceEach in new[] { UvcControlInterface.CameraControl,UvcControlInterface.VideoProcAmp }) {
				foreach(int propertyEach in GetProperties(controlInterfaceEach)) {
					if(string.Equals(GetPropertyName(controlInterfaceEach,propertyEach),name,StringComparison.OrdinalIgnoreCase)) {
						controlInterface=controlInterfaceEach;
						property=propertyEach;
						return true;
					}
				}
			}
			controlInterface=UvcControlInterface.CameraControl;
			property=-1;
			return false;
		}

		///<summary>Maps the HRESULT of IAMCameraControl or IAMVideoProcAmp Get or GetRange to a status.</summary>
		public static UvcControlStatus GetStatusFromHResult(int hResult) {
			if(hResult>=0) {
				return UvcControlStatus.Ok;
			}
			switch(hResult) {
				case Ds.E_PROP_ID_UNSUPPORTED:
				case Ds.E_PROP_SET_UNSUPPORTED:
				case UvcErrors.HR_ERROR_SET_NOT_FOUND:
				case Ds.E_NOTIMPL:
				case Ds.E_INVALIDARG:
					return UvcControlStatus.Unsupported;
				case Ds.E_NOINTERFACE:
					return UvcControlStatus.NoInterface;
			}
			if(UvcErrors.Classify(hResult)==UvcErrorKind.DeviceLost) {
				return UvcControlStatus.DeviceLost;
			}
			return UvcControlStatus.Failed;
		}
	}
}
