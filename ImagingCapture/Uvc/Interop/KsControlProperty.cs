// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace ImagingCapture.Uvc.Interop {
	///<summary>The byte layouts IKsControl.KsProperty takes and returns for the two extended camera settings (ksmedia.h), built and parsed
	///here so they can be tested without a camera. KSPROPERTY_VIDEOPROCAMP_S and KSPROPERTY_CAMERACONTROL_S are 36 bytes: the KSPROPERTY
	///(Set GUID, Id, Flags), then Value at 24, Flags at 28 and Capabilities at 32. A BASICSUPPORT answer is a KSPROPERTY_DESCRIPTION (40
	///bytes) followed by member lists, each a KSPROPERTY_MEMBERSHEADER (16 bytes) and its members; a range is a KSPROPERTY_STEPPING_LONG
	///(delta, reserved, minimum, maximum) or a KSPROPERTY_BOUNDS_LONG (minimum, maximum). A DEFAULTVALUES answer has a values list whose
	///first member is the default.</summary>
	public static class KsControlProperty {
		public const int KSPROPERTY_TYPE_GET=0x00000001;
		public const int KSPROPERTY_TYPE_SET=0x00000002;
		public const int KSPROPERTY_TYPE_BASICSUPPORT=0x00000200;
		public const int KSPROPERTY_TYPE_DEFAULTVALUES=0x00001000;
		public const int KSPROPERTY_MEMBER_RANGES=1;
		public const int KSPROPERTY_MEMBER_STEPPEDRANGES=2;
		public const int KSPROPERTY_MEMBER_VALUES=3;
		///<summary>sizeof(KSPROPERTY_VIDEOPROCAMP_S), the same as KSPROPERTY_CAMERACONTROL_S.</summary>
		public const int SizeRequest=36;
		///<summary>The buffer given to BASICSUPPORT and DEFAULTVALUES; their answers for one LONG property are far smaller.</summary>
		public const int SizeSupportBuffer=256;
		private const int SizeDescription=40;
		private const int SizeMembersHeader=16;
		///<summary>PROPSETID_VIDCAP_VIDEOPROCAMP, the same GUID as IAMVideoProcAmp's IID.</summary>
		public static readonly Guid PropSetVideoProcAmp=new Guid("C6E13360-30AC-11D0-A18C-00A0C9118956");
		///<summary>PROPSETID_VIDCAP_CAMERACONTROL, the same GUID as IAMCameraControl's IID.</summary>
		public static readonly Guid PropSetCameraControl=new Guid("C6E13370-30AC-11D0-A18C-00A0C9118956");

		public static Guid GetPropSet(UvcControlInterface controlInterface) {
			return controlInterface==UvcControlInterface.CameraControl ? PropSetCameraControl : PropSetVideoProcAmp;
		}

		///<summary>A KSPROPERTY_*_S request: the property set, id and request type, and for a SET the value and its flags.</summary>
		public static byte[] BuildRequest(Guid propSet,int id,int requestType,int value,int valueFlags) {
			byte[] buffer=new byte[SizeRequest];
			Array.Copy(propSet.ToByteArray(),0,buffer,0,16);
			WriteInt(buffer,16,id);
			WriteInt(buffer,20,requestType);
			WriteInt(buffer,24,value);
			WriteInt(buffer,28,valueFlags);
			WriteInt(buffer,32,0);
			return buffer;
		}

		///<summary>The Value, Flags and Capabilities of a GET answer. False when the answer is shorter than the structure.</summary>
		public static bool TryParseValue(byte[] buffer,int bytesReturned,out int value,out int flags,out int capabilities) {
			value=0;
			flags=0;
			capabilities=0;
			if(buffer==null || bytesReturned<SizeRequest || buffer.Length<SizeRequest) {
				return false;
			}
			value=ReadInt(buffer,24);
			flags=ReadInt(buffer,28);
			capabilities=ReadInt(buffer,32);
			return true;
		}

		///<summary>The minimum, maximum and step of a BASICSUPPORT answer: the first range or stepped-range list. False when there is none or
		///the answer is malformed; step is 1 for a plain range.</summary>
		public static bool TryParseRange(byte[] buffer,int bytesReturned,out int min,out int max,out int step) {
			min=0;
			max=0;
			step=1;
			int length=Math.Min(bytesReturned,buffer?.Length??0);
			if(length<SizeDescription) {
				return false;
			}
			int countLists=ReadInt(buffer,32);
			int offset=SizeDescription;
			for(int i=0;i<countLists && i<16;i++) {
				if(offset+SizeMembersHeader>length) {
					return false;
				}
				int membersFlags=ReadInt(buffer,offset);
				int membersSize=ReadInt(buffer,offset+4);
				int membersCount=ReadInt(buffer,offset+8);
				int offsetMembers=offset+SizeMembersHeader;
				if(membersSize<0 || membersCount<0 || membersSize>length) {
					return false;
				}
				if(membersFlags==KSPROPERTY_MEMBER_STEPPEDRANGES && membersCount>=1 && offsetMembers+16<=length) {
					step=Math.Max(1,ReadInt(buffer,offsetMembers));
					min=ReadInt(buffer,offsetMembers+8);
					max=ReadInt(buffer,offsetMembers+12);
					return min<=max;
				}
				if(membersFlags==KSPROPERTY_MEMBER_RANGES && membersCount>=1 && offsetMembers+8<=length) {
					min=ReadInt(buffer,offsetMembers);
					max=ReadInt(buffer,offsetMembers+4);
					step=1;
					return min<=max;
				}
				offset=offsetMembers+membersSize*membersCount;
			}
			return false;
		}

		///<summary>The default of a DEFAULTVALUES answer: the first member of its first values list. False when there is none.</summary>
		public static bool TryParseDefault(byte[] buffer,int bytesReturned,out int defaultValue) {
			defaultValue=0;
			int length=Math.Min(bytesReturned,buffer?.Length??0);
			if(length<SizeDescription) {
				return false;
			}
			int countLists=ReadInt(buffer,32);
			int offset=SizeDescription;
			for(int i=0;i<countLists && i<16;i++) {
				if(offset+SizeMembersHeader>length) {
					return false;
				}
				int membersFlags=ReadInt(buffer,offset);
				int membersSize=ReadInt(buffer,offset+4);
				int membersCount=ReadInt(buffer,offset+8);
				int offsetMembers=offset+SizeMembersHeader;
				if(membersSize<0 || membersCount<0 || membersSize>length) {
					return false;
				}
				if(membersFlags==KSPROPERTY_MEMBER_VALUES && membersCount>=1 && offsetMembers+4<=length) {
					defaultValue=ReadInt(buffer,offsetMembers);
					return true;
				}
				offset=offsetMembers+membersSize*membersCount;
			}
			return false;
		}

		///<summary>The range assumed when a driver answers GET but not BASICSUPPORT: power line frequency 0 (off) to 2 (60 Hz), low light
		///compensation 0 (off) to 1 (on).</summary>
		public static void GetFallbackRange(UvcControlInterface controlInterface,int property,out int min,out int max) {
			min=0;
			max=controlInterface==UvcControlInterface.VideoProcAmp && property==UvcControls.VideoProcAmpPowerLineFrequency ? 2 : 1;
		}

		public static int ReadInt(byte[] buffer,int offset) {
			return BitConverter.ToInt32(buffer,offset);
		}

		public static void WriteInt(byte[] buffer,int offset,int value) {
			byte[] arrayBytes=BitConverter.GetBytes(value);
			Array.Copy(arrayBytes,0,buffer,offset,4);
		}
	}
}
