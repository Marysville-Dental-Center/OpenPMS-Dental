// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ImagingDeviceManager.TwainProtocol {
	//The capture path of design 7.1 CAP-3, used by TwainCaptureSession on its worker thread: MSG_GET and MSG_QUERYSUPPORT for the
	//capabilities the capture session reads, and MSG_SET for the custom capabilities named in the device profile.
	internal sealed partial class TwainSession {
		///<summary>MSG_QUERYSUPPORT answers of the open source, by capability: the TWQC_ bits, or null when the source did not answer (the
		///condition code is in _dictQuerySupportCondition). Capabilities return to their defaults only at MSG_OPENDS (twain-contract 1.1
		///section 4), and what a source supports does not change while it is open, so each is asked once per open source.</summary>
		private readonly Dictionary<ushort,int?> _dictQuerySupport=new Dictionary<ushort,int?>();
		private readonly Dictionary<ushort,ushort> _dictQuerySupportCondition=new Dictionary<ushort,ushort>();

		#region Capabilities
		///<summary>Forgets what was learnt about the open source. Called whenever a source is opened or closed.</summary>
		private void ResetCaptureState() {
			_dictQuerySupport.Clear();
			_dictQuerySupportCondition.Clear();
			ResetCaptureTransferState();
		}

		///<summary>A DAT_CAPABILITY message that returns a container (MSG_GET, MSG_GETCURRENT, MSG_GETDEFAULT), with every item read. False
		///when the source does not answer (LastConditionCode says why) or the container cannot be read. The container handle is freed like
		///every handle from the source (ITwainMemory).</summary>
		internal bool TryGetCapabilityValues(ushort capability,ushort msg,out TwainCapabilityValues values) {
			values=null;
			if(State<4 || _memory==null) {
				return false;
			}
			IntPtr pCapability=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TW_CAPABILITY)));
			IntPtr hContainer=IntPtr.Zero;
			try {
				TW_CAPABILITY twCapability=new TW_CAPABILITY { Cap=capability,ConType=Tw.TWON_DONTCARE16,hContainer=IntPtr.Zero };
				Marshal.StructureToPtr(twCapability,pCapability,false);
				if(SourceEntry(Tw.DG_CONTROL,Tw.DAT_CAPABILITY,msg,pCapability)!=Tw.TWRC_SUCCESS) {
					return false;
				}
				twCapability=(TW_CAPABILITY)Marshal.PtrToStructure(pCapability,typeof(TW_CAPABILITY));
				hContainer=twCapability.hContainer;
				if(hContainer==IntPtr.Zero) {
					return false;
				}
				IntPtr pContainer=_memory.Lock(hContainer);
				if(pContainer==IntPtr.Zero) {
					return false;
				}
				try {
					values=ReadContainer(pContainer,twCapability.ConType,_memory.GetSize(hContainer));
					return values!=null;
				}
				finally {
					_memory.Unlock(hContainer);
				}
			}
			catch(Exception ex) {
				Log("Capability "+ToHex(capability)+" of \""+SourceName+"\" could not be read: "+ex.Message);
				values=null;
				return false;
			}
			finally {
				Marshal.FreeHGlobal(pCapability);
				if(hContainer!=IntPtr.Zero) {
					_memory.Free(hContainer);
				}
			}
		}

		///<summary>Reads a container. sizeBlock bounds an enumeration's or array's item list when the allocator knows the block size (0:
		///unknown); a list is never read past 1024 items either way. Null for a container type this library does not read.</summary>
		internal static TwainCapabilityValues ReadContainer(IntPtr pContainer,ushort conType,long sizeBlock) {
			const int maxItems=1024;
			TwainCapabilityValues values=new TwainCapabilityValues { ConType=conType };
			values.ItemType=(ushort)Marshal.ReadInt16(pContainer,0);
			int itemSize=TwContainer.ItemSize(values.ItemType);
			bool isNumeric=itemSize>=1 && itemSize<=4;
			switch(conType) {
				case Tw.TWON_ONEVALUE:
					if(isNumeric) {
						values.ListItems.Add(ReadItem(pContainer,TwContainer.OneValueItemOffset,values.ItemType,itemSize));
					}
					return values;
				case Tw.TWON_RANGE:
					values.RangeMin=Marshal.ReadInt32(pContainer,TwContainer.RangeMinValueOffset);
					values.RangeMax=Marshal.ReadInt32(pContainer,TwContainer.RangeMaxValueOffset);
					values.RangeStep=Marshal.ReadInt32(pContainer,TwContainer.RangeStepSizeOffset);
					values.RangeDefault=Marshal.ReadInt32(pContainer,TwContainer.RangeDefaultValueOffset);
					values.RangeCurrent=Marshal.ReadInt32(pContainer,TwContainer.RangeCurrentValueOffset);
					return values;
				case Tw.TWON_ENUMERATION:
				case Tw.TWON_ARRAY:
					bool isEnumeration=conType==Tw.TWON_ENUMERATION;
					int numItems=Marshal.ReadInt32(pContainer,isEnumeration ? TwContainer.EnumNumItemsOffset : TwContainer.ArrayNumItemsOffset);
					int offsetList=isEnumeration ? TwContainer.EnumItemListOffset : TwContainer.ArrayItemListOffset;
					if(isEnumeration) {
						values.CurrentIndex=Marshal.ReadInt32(pContainer,TwContainer.EnumCurrentIndexOffset);
						values.DefaultIndex=Marshal.ReadInt32(pContainer,TwContainer.EnumDefaultIndexOffset);
					}
					if(!isNumeric || numItems<=0) {
						return values;
					}
					numItems=Math.Min(numItems,maxItems);
					if(sizeBlock>0) {
						numItems=(int)Math.Min(numItems,Math.Max(0,(sizeBlock-offsetList)/itemSize));
					}
					for(int i=0;i<numItems;i++) {
						values.ListItems.Add(ReadItem(pContainer,offsetList+i*itemSize,values.ItemType,itemSize));
					}
					return values;
				default:
					return null;
			}
		}

		///<summary>MSG_GET, then MSG_GETCURRENT for a source that answers only that, read as one value: what the capture session reads of
		///the fixed capabilities (CAP_INDICATORS, ICAP_PIXELFLAVOR, ICAP_BITORDER and the others), which it never sets.</summary>
		internal bool TryGetCapabilityValue(ushort capability,out int value) {
			return TryGetCapability(capability,Tw.MSG_GET,out value) || TryGetCapabilityCurrent(capability,out value);
		}

		///<summary>MSG_QUERYSUPPORT, asked once per open source (_dictQuerySupport). Null when the source did not answer; conditionCode is then
		///its condition code (TWCC_CAPUNSUPPORTED for a capability it does not have), otherwise TWCC_SUCCESS.</summary>
		internal int? GetQuerySupport(ushort capability,out ushort conditionCode) {
			conditionCode=Tw.TWCC_SUCCESS;
			int? support;
			if(_dictQuerySupport.TryGetValue(capability,out support)) {
				if(support==null) {
					_dictQuerySupportCondition.TryGetValue(capability,out conditionCode);
				}
				return support;
			}
			int value;
			if(TryQuerySupport(capability,out value)) {
				support=value;
			}
			else {
				support=null;
				conditionCode=LastConditionCode;
				_dictQuerySupportCondition[capability]=conditionCode;
			}
			if(State>=4) {
				_dictQuerySupport[capability]=support;
			}
			return support;
		}

		///<summary>A custom TWTY_BOOL capability named in the device profile (design 7.1 CAP-3 and 9.3): MSG_QUERYSUPPORT first, then MSG_SET in
		///state 4. TWCC_CAPUNSUPPORTED (from a source that does not have it, such as a twain-contract 1.0 build) is logged and the feature counts
		///as absent. A source that does not answer MSG_QUERYSUPPORT (older than TWAIN 1.6) is left to MSG_SET. key names the capture.ini key,
		///for the log. Only TWRC_SUCCESS counts as set.</summary>
		internal TwainCustomCapOutcome SetCustomCapability(ushort capability,bool value,string key) {
			string what=key+" capability "+ToHex(capability)+" ("+(value ? "TRUE" : "FALSE")+")";
			if(State!=4) {
				Log("Not set: "+what+" for \""+SourceName+"\": the source is not in state 4.");
				return TwainCustomCapOutcome.NotSent;
			}
			ushort conditionQuery;
			int? support=GetQuerySupport(capability,out conditionQuery);
			if(support==null && conditionQuery==Tw.TWCC_CAPUNSUPPORTED) {
				Log("\""+SourceName+"\" does not support the "+what+" (TWCC_CAPUNSUPPORTED); the feature counts as absent.");
				return TwainCustomCapOutcome.Unsupported;
			}
			if(support!=null && (support.Value & Tw.TWQC_SET)==0) {
				Log("\""+SourceName+"\" does not allow setting the "+what+" (MSG_QUERYSUPPORT "+ToHex(support.Value)+"); the feature counts as absent.");
				return TwainCustomCapOutcome.NotSettable;
			}
			ushort? rc=SendCapabilitySet(capability,Tw.TWTY_BOOL,value ? 1 : 0);
			if(rc==null) {
				Log("Not set: "+what+" for \""+SourceName+"\": out of memory.");
				return TwainCustomCapOutcome.NotSent;
			}
			if(rc==Tw.TWRC_SUCCESS) {
				return TwainCustomCapOutcome.Set;
			}
			if(rc==Tw.TWRC_FAILURE && LastConditionCode==Tw.TWCC_CAPUNSUPPORTED) {
				Log("\""+SourceName+"\" does not support the "+what+" (TWCC_CAPUNSUPPORTED to MSG_SET); the feature counts as absent.");
				return TwainCustomCapOutcome.Unsupported;
			}
			Log("\""+SourceName+"\" did not accept the "+what+" ("+(rc==Tw.TWRC_FAILURE ? "condition code "+ToHex(LastConditionCode)
				: "return code "+rc)+"); the feature counts as absent.");
			return TwainCustomCapOutcome.Refused;
		}

		///<summary>The most TWTY_STR1024 chunks a settings description may have (about 64 KB): the description is untrusted input.</summary>
		private const int SettingsSchemaChunksMax=64;
		///<summary>The size of a TW_STR1024 item, terminating NUL included.</summary>
		private const int Str1024Size=1026;

		///<summary>The settings capability (twain-contract 1.4 "host settings", IMG-SRCSET): MSG_SET in state 4 with a TW_ONEVALUE of TWTY_STR1024
		///holding `Section.Key=value;...`. Only TWRC_SUCCESS counts as set. error: why not, for the operator ("" when set).</summary>
		internal TwainCustomCapOutcome SetSettingsCapability(ushort capability,string text,out string error) {
			error="";
			if(State!=4) {
				error="the source is not in state 4";
				return TwainCustomCapOutcome.NotSent;
			}
			byte[] arrayBytes=System.Text.Encoding.UTF8.GetBytes(text??"");
			if(arrayBytes.Length>=Str1024Size) {
				error="the settings are longer than the capability's 1025 bytes";
				return TwainCustomCapOutcome.NotSent;
			}
			IntPtr hContainer=_memory.Allocate((uint)(TwContainer.OneValueItemOffset+Str1024Size));
			if(hContainer==IntPtr.Zero) {
				error="out of memory";
				return TwainCustomCapOutcome.NotSent;
			}
			IntPtr pCapability=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TW_CAPABILITY)));
			ushort rc;
			try {
				IntPtr pContainer=_memory.Lock(hContainer);
				if(pContainer==IntPtr.Zero) {
					error="out of memory";
					return TwainCustomCapOutcome.NotSent;
				}
				byte[] arrayItem=new byte[Str1024Size];//NUL-filled
				Array.Copy(arrayBytes,arrayItem,arrayBytes.Length);
				Marshal.WriteInt16(pContainer,0,(short)Tw.TWTY_STR1024);
				Marshal.Copy(arrayItem,0,pContainer+TwContainer.OneValueItemOffset,arrayItem.Length);
				_memory.Unlock(hContainer);
				TW_CAPABILITY twCapability=new TW_CAPABILITY { Cap=capability,ConType=Tw.TWON_ONEVALUE,hContainer=hContainer };
				Marshal.StructureToPtr(twCapability,pCapability,false);
				rc=SourceEntry(Tw.DG_CONTROL,Tw.DAT_CAPABILITY,Tw.MSG_SET,pCapability);
			}
			finally {
				Marshal.FreeHGlobal(pCapability);
				_memory.Free(hContainer);
			}
			if(rc==Tw.TWRC_SUCCESS) {
				return TwainCustomCapOutcome.Set;
			}
			ushort conditionCode=rc==Tw.TWRC_FAILURE ? LastConditionCode : (ushort)0;
			if(conditionCode==Tw.TWCC_CAPUNSUPPORTED) {
				error="the source has no settings capability "+ToHex(capability)+" (TWCC_CAPUNSUPPORTED; a driver before twain-contract 1.4)";
				return TwainCustomCapOutcome.Unsupported;
			}
			error=rc==Tw.TWRC_FAILURE ? "the source refused them (condition code "+ToHex(conditionCode)+(conditionCode==Tw.TWCC_BADVALUE ? ": a key or value it "
				+"does not take; its log names it" : "")+")" : "return code "+rc;
			return TwainCustomCapOutcome.Refused;
		}

		///<summary>The driver's description of its settings (twain-contract 1.4, the settings-description capability): MSG_GET answers a TW_ARRAY
		///of TWTY_STR1024 chunks of one UTF-8 JSON document, joined in order. Null when it does not answer that way.</summary>
		internal string GetSettingsSchemaCapability(ushort capability) {
			if(State<4) {
				return null;
			}
			IntPtr pCapability=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TW_CAPABILITY)));
			try {
				Marshal.StructureToPtr(new TW_CAPABILITY { Cap=capability,ConType=Tw.TWON_DONTCARE16,hContainer=IntPtr.Zero },pCapability,false);
				ushort rc=SourceEntry(Tw.DG_CONTROL,Tw.DAT_CAPABILITY,Tw.MSG_GET,pCapability);
				if(rc!=Tw.TWRC_SUCCESS) {
					Log("\""+SourceName+"\" does not describe its settings (settings-description capability "+ToHex(capability)+": "
						+(rc==Tw.TWRC_FAILURE ? "condition code "+ToHex(LastConditionCode) : "return code "+rc)+").");
					return null;
				}
				TW_CAPABILITY twCapability=(TW_CAPABILITY)Marshal.PtrToStructure(pCapability,typeof(TW_CAPABILITY));
				if(twCapability.hContainer==IntPtr.Zero) {
					return null;
				}
				try {
					IntPtr pContainer=_memory.Lock(twCapability.hContainer);
					if(pContainer==IntPtr.Zero) {
						return null;
					}
					try {
						if(twCapability.ConType!=Tw.TWON_ARRAY || (ushort)Marshal.ReadInt16(pContainer,0)!=Tw.TWTY_STR1024) {
							Log("\""+SourceName+"\" answered its settings description in another form than a TW_ARRAY of TWTY_STR1024; ignored.");
							return null;
						}
						int count=Marshal.ReadInt32(pContainer,2);
						long sizeBlock=_memory.GetSize(twCapability.hContainer);
						if(sizeBlock>0 && count>=0 && count<=SettingsSchemaChunksMax && 6L+(long)count*Str1024Size>sizeBlock) {
							Log("\""+SourceName+"\" answered a settings description of "+count+" chunks in a block of "+sizeBlock+" bytes. Ignored.");
							return null;
						}
						if(count<0 || count>SettingsSchemaChunksMax) {
							Log("\""+SourceName+"\" answered a settings description of "+count+" chunks; at most "+SettingsSchemaChunksMax+" are read. Ignored.");
							return null;
						}
						List<byte> listBytes=new List<byte>();
						byte[] arrayItem=new byte[Str1024Size];
						for(int i=0;i<count;i++) {
							Marshal.Copy(pContainer+6+i*Str1024Size,arrayItem,0,arrayItem.Length);
							int length=Array.IndexOf(arrayItem,(byte)0);
							listBytes.AddRange(arrayItem.Take(length<0 ? arrayItem.Length : length));
						}
						return System.Text.Encoding.UTF8.GetString(listBytes.ToArray());
					}
					finally {
						_memory.Unlock(twCapability.hContainer);
					}
				}
				finally {
					_memory.Free(twCapability.hContainer);
				}
			}
			catch(Exception ex) {
				Log("The settings description could not be read: "+ex.Message);
				return null;
			}
			finally {
				Marshal.FreeHGlobal(pCapability);
			}
		}

		///<summary>The source's effective settings through the settings capability (MSG_GET, TWTY_STR1024), or null when it does not answer.</summary>
		internal string GetSettingsCapability(ushort capability) {
			if(State<4) {
				return null;
			}
			IntPtr pCapability=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TW_CAPABILITY)));
			try {
				Marshal.StructureToPtr(new TW_CAPABILITY { Cap=capability,ConType=Tw.TWON_DONTCARE16,hContainer=IntPtr.Zero },pCapability,false);
				ushort rc=SourceEntry(Tw.DG_CONTROL,Tw.DAT_CAPABILITY,Tw.MSG_GET,pCapability);
				if(rc!=Tw.TWRC_SUCCESS) {
					return null;
				}
				TW_CAPABILITY twCapability=(TW_CAPABILITY)Marshal.PtrToStructure(pCapability,typeof(TW_CAPABILITY));
				if(twCapability.hContainer==IntPtr.Zero) {
					return null;
				}
				try {
					IntPtr pContainer=_memory.Lock(twCapability.hContainer);
					if(pContainer==IntPtr.Zero) {
						return null;
					}
					try {
						if(twCapability.ConType!=Tw.TWON_ONEVALUE || (ushort)Marshal.ReadInt16(pContainer,0)!=Tw.TWTY_STR1024) {
							return null;
						}
						long sizeBlock=_memory.GetSize(twCapability.hContainer);
						int sizeItem=sizeBlock>0 ? (int)Math.Max(0,Math.Min(Str1024Size,sizeBlock-TwContainer.OneValueItemOffset)) : Str1024Size;
						byte[] arrayItem=new byte[sizeItem];
						Marshal.Copy(pContainer+TwContainer.OneValueItemOffset,arrayItem,0,arrayItem.Length);
						int length=Array.IndexOf(arrayItem,(byte)0);
						return System.Text.Encoding.UTF8.GetString(arrayItem,0,length<0 ? arrayItem.Length : length);
					}
					finally {
						_memory.Unlock(twCapability.hContainer);
					}
				}
				finally {
					_memory.Free(twCapability.hContainer);
				}
			}
			catch(Exception ex) {
				Log("The settings capability could not be read: "+ex.Message);
				return null;
			}
			finally {
				Marshal.FreeHGlobal(pCapability);
			}
		}

		///<summary>Called on the TWAIN thread with the TWDE_ code of each device event the source reports (MSG_DEVICEEVENT, then
		///DAT_DEVICEEVENT / MSG_GET). Null: device events are read and dropped.</summary>
		internal Action<uint> ActionDeviceEvent { get; set; }

		///<summary>CAP_DEVICEEVENT MSG_SET in state 4 with the TWDE_ codes to report (twain-contract 1.3 section 10.6). No MSG_QUERYSUPPORT: a
		///source without device events (any 1.2 build) answers TWCC_CAPUNSUPPORTED or TWCC_BADVALUE, and the feature then counts as absent.
		///TWRC_CHECKSTATUS (some codes left out) counts as set. Returns true when the source took it.</summary>
		internal bool SetDeviceEvents(ushort[] arrayCodes) {
			if(State!=4 || arrayCodes==null || arrayCodes.Length==0) {
				return false;
			}
			const int offsetItems=6;//TW_ARRAY: ItemType (UINT16), NumItems (UINT32), then the items (pack 2)
			IntPtr hContainer=_memory.Allocate((uint)(offsetItems+2*arrayCodes.Length));
			if(hContainer==IntPtr.Zero) {
				Log("Device events not asked for: out of memory.");
				return false;
			}
			IntPtr pCapability=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TW_CAPABILITY)));
			ushort rc;
			try {
				IntPtr pContainer=_memory.Lock(hContainer);
				if(pContainer==IntPtr.Zero) {
					return false;
				}
				Marshal.WriteInt16(pContainer,0,(short)Tw.TWTY_UINT16);
				Marshal.WriteInt32(pContainer,2,arrayCodes.Length);
				for(int i=0;i<arrayCodes.Length;i++) {
					Marshal.WriteInt16(pContainer,offsetItems+2*i,(short)arrayCodes[i]);
				}
				_memory.Unlock(hContainer);
				TW_CAPABILITY twCapability=new TW_CAPABILITY { Cap=Tw.CAP_DEVICEEVENT,ConType=Tw.TWON_ARRAY,hContainer=hContainer };
				Marshal.StructureToPtr(twCapability,pCapability,false);
				rc=SourceEntry(Tw.DG_CONTROL,Tw.DAT_CAPABILITY,Tw.MSG_SET,pCapability);
			}
			finally {
				Marshal.FreeHGlobal(pCapability);
				_memory.Free(hContainer);
			}
			if(rc==Tw.TWRC_SUCCESS || rc==Tw.TWRC_CHECKSTATUS) {
				Log("\""+SourceName+"\" reports device events ("+string.Join(", ",arrayCodes)+(rc==Tw.TWRC_CHECKSTATUS ? "; some left out" : "")+").");
				return true;
			}
			Log("\""+SourceName+"\" does not report device events (CAP_DEVICEEVENT: "+(rc==Tw.TWRC_FAILURE ? "condition code "+ToHex(LastConditionCode)
				: "return code "+rc)+"); the session shows the TWAIN states only.");
			return false;
		}

		///<summary>After MSG_DEVICEEVENT: DAT_DEVICEEVENT / MSG_GET, and ActionDeviceEvent with its Event field (the first member of
		///TW_DEVICEEVENT; the rest, battery and flash details, is not used). Never throws; a failure is logged.</summary>
		internal void ReadDeviceEvent() {
			const int sizeDeviceEvent=512;//TW_DEVICEEVENT is 294 bytes; zeroed room to spare
			IntPtr pDeviceEvent=Marshal.AllocHGlobal(sizeDeviceEvent);
			try {
				for(int offset=0;offset<sizeDeviceEvent;offset+=4) {
					Marshal.WriteInt32(pDeviceEvent,offset,0);
				}
				ushort rc=SourceEntry(Tw.DG_CONTROL,Tw.DAT_DEVICEEVENT,Tw.MSG_GET,pDeviceEvent);
				if(rc!=Tw.TWRC_SUCCESS) {
					Log("\""+SourceName+"\" sent MSG_DEVICEEVENT, but DAT_DEVICEEVENT failed ("+(rc==Tw.TWRC_FAILURE ? "condition code "+ToHex(LastConditionCode)
						: "return code "+rc)+").");
					return;
				}
				uint code=(uint)Marshal.ReadInt32(pDeviceEvent,0);
				Log("Device event "+code+" from \""+SourceName+"\".");
				ActionDeviceEvent?.Invoke(code);
			}
			catch(Exception ex) {
				Log("Device event not read: "+ex.Message);
			}
			finally {
				Marshal.FreeHGlobal(pDeviceEvent);
			}
		}

		///<summary>The TW_IDENTITY of the open source, or null below state 4.</summary>
		internal TW_IDENTITY? GetOpenSourceIdentity() {
			if(State<4 || _pSourceIdentity==IntPtr.Zero) {
				return null;
			}
			return ReadIdentity(_pSourceIdentity);
		}

		///<summary>The library name of the loaded DSM, or "".</summary>
		internal string DsmLibraryName => _dsm==null ? "" : (_dsm.LibraryName??"");
		#endregion Capabilities
	}
}
