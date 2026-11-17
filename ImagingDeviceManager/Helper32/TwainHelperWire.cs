// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ImagingDeviceManager.Capture;

namespace ImagingDeviceManager.Helper32 {
	///<summary>What the helper's TWAIN session raised, in order (TwainHelperCommands.Poll).</summary>
	internal enum TwainHelperEventKind:byte {
		Status=1,
		Ended=2,
		ImageAcquired=3,
		ImageQuarantined=4,
		Logged=5,
	}

	///<summary>One event of a Poll. For an image, the pixels stay in the helper until ReadData takes them: PayloadId and PayloadLength name them,
	///and Image has no Gray16 or Dib until then.</summary>
	internal sealed class TwainHelperEvent {
		public TwainHelperEventKind Kind;
		public TwainCaptureStatus Status;
		public TwainCaptureEnded Ended;
		public AcquiredImage Image;
		///<summary>"" when the image has no pixels.</summary>
		public string PayloadId="";
		///<summary>Bytes: twice the sample count of a Gray16 image, the length of a Dib.</summary>
		public long PayloadLength;
		///<summary>The payload is Gray16 samples (little-endian), else a packed DIB.</summary>
		public bool IsPayloadGray16;
		public string Line;
	}

	///<summary>A source a DSM lists (TwainHelperCommands.ListSources, TwainSourcePath).</summary>
	public sealed class TwainSourceEntry {
		public string ProductName { get; set; }="";
		public string Manufacturer { get; set; }="";
		public string ProductFamily { get; set; }="";
		public int VersionMajor { get; set; }
		public int VersionMinor { get; set; }
		public string VersionInfo { get; set; }="";
		///<summary>The DSM that lists it: Native (the program's own process) or Helper32 (the 32-bit DSM through the helper).</summary>
		public TwainDsmKind Dsm { get; set; }
		///<summary>TWAINDSM.dll or twain_32.dll.</summary>
		public string DsmLibrary { get; set; }="";

		///<summary>The text to store for this source (TwainSourceChoice.Format).</summary>
		public string StoredText => TwainSourceChoice.Format(ProductName,Dsm);
	}

	///<summary>The byte layout of the structures the TWAIN module's commands carry (TwainHelperCommands.VERSION), compiled into both ends from
	///this file, so they read and write the same bytes. Little-endian (BinaryWriter); a string is a presence byte and BinaryWriter's UTF-8
	///string, so null and "" stay apart; a list is a count (-1 for null) and its items; a nullable value a presence byte and the value; a
	///DateTime its ToBinary (its kind kept); a double its bits. Every public property of the structures crosses except the caller's Tag, which
	///the program keeps (TwainHelperCaptureSession). TwainHelperWireTests round-trips every property of each structure by reflection, so a
	///property added later and not written here fails a test.</summary>
	internal static class TwainHelperWire {
		///<summary>An image's pixels in the message itself (QuarantineImage, ConfirmRedelivered).</summary>
		private const byte PIXELS_NONE=0;
		private const byte PIXELS_GRAY16=1;
		private const byte PIXELS_DIB=2;
		///<summary>Upper bounds of what a reader accepts, so a broken message is refused before a large allocation.</summary>
		private const int LIST_MAX=100000;
		private const int ARRAY_MAX=1<<30;

		#region Framing
		public static byte[] Write(Action<BinaryWriter> actionWrite) {
			using(MemoryStream memoryStream=new MemoryStream())
			using(BinaryWriter binaryWriter=new BinaryWriter(memoryStream,new UTF8Encoding(false,true))) {
				actionWrite(binaryWriter);
				binaryWriter.Flush();
				return memoryStream.ToArray();
			}
		}

		public static T Read<T>(byte[] data,Func<BinaryReader,T> funcRead) {
			if(data==null) {
				throw new InvalidDataException("The TWAIN helper sent no data.");
			}
			using(MemoryStream memoryStream=new MemoryStream(data,false))
			using(BinaryReader binaryReader=new BinaryReader(memoryStream,new UTF8Encoding(false,true))) {
				try {
					T value=funcRead(binaryReader);
					if(memoryStream.Position!=memoryStream.Length) {
						throw new InvalidDataException("The TWAIN helper's data has "+(memoryStream.Length-memoryStream.Position)+" bytes too many.");
					}
					return value;
				}
				catch(EndOfStreamException exception) {
					throw new InvalidDataException("The TWAIN helper's data was cut short.",exception);
				}
			}
		}
		#endregion Framing

		#region Values
		private static void WriteString(BinaryWriter writer,string value) {
			writer.Write(value!=null);
			if(value!=null) {
				writer.Write(value);
			}
		}

		private static string ReadString(BinaryReader reader) {
			return reader.ReadBoolean() ? reader.ReadString() : null;
		}

		private static void WriteNullableInt(BinaryWriter writer,int? value) {
			writer.Write(value.HasValue);
			if(value.HasValue) {
				writer.Write(value.Value);
			}
		}

		private static int? ReadNullableInt(BinaryReader reader) {
			return reader.ReadBoolean() ? reader.ReadInt32() : (int?)null;
		}

		private static void WriteNullableUShort(BinaryWriter writer,ushort? value) {
			writer.Write(value.HasValue);
			if(value.HasValue) {
				writer.Write(value.Value);
			}
		}

		private static ushort? ReadNullableUShort(BinaryReader reader) {
			return reader.ReadBoolean() ? reader.ReadUInt16() : (ushort?)null;
		}

		private static void WriteNullableBool(BinaryWriter writer,bool? value) {
			writer.Write(value.HasValue);
			if(value.HasValue) {
				writer.Write(value.Value);
			}
		}

		private static bool? ReadNullableBool(BinaryReader reader) {
			return reader.ReadBoolean() ? reader.ReadBoolean() : (bool?)null;
		}

		private static int ReadCount(BinaryReader reader) {
			int count=reader.ReadInt32();
			if(count<-1 || count>LIST_MAX) {
				throw new InvalidDataException("The TWAIN helper's data has a list of "+count+" items.");
			}
			return count;
		}

		private static void WriteList<T>(BinaryWriter writer,List<T> list,Action<BinaryWriter,T> actionWrite) {
			if(list==null) {
				writer.Write(-1);
				return;
			}
			writer.Write(list.Count);
			foreach(T item in list) {
				actionWrite(writer,item);
			}
		}

		private static List<T> ReadList<T>(BinaryReader reader,Func<BinaryReader,T> funcRead) {
			int count=ReadCount(reader);
			if(count<0) {
				return null;
			}
			List<T> list=new List<T>(count);
			for(int i=0;i<count;i++) {
				list.Add(funcRead(reader));
			}
			return list;
		}

		private static void WritePairs(BinaryWriter writer,List<KeyValuePair<string,string>> list) {
			WriteList(writer,list,(w,x) => {
				WriteString(w,x.Key);
				WriteString(w,x.Value);
			});
		}

		private static List<KeyValuePair<string,string>> ReadPairs(BinaryReader reader) {
			return ReadList(reader,r => new KeyValuePair<string,string>(ReadString(r),ReadString(r)));
		}

		private static void WriteDateTime(BinaryWriter writer,DateTime value) {
			writer.Write(value.ToBinary());
		}

		private static DateTime ReadDateTime(BinaryReader reader) {
			return DateTime.FromBinary(reader.ReadInt64());
		}

		private static byte[] ReadBytes(BinaryReader reader,long length) {
			if(length<0 || length>ARRAY_MAX) {
				throw new InvalidDataException("The TWAIN helper's data has an array of "+length+" bytes.");
			}
			byte[] arrayBytes=reader.ReadBytes((int)length);
			if(arrayBytes.Length!=length) {
				throw new EndOfStreamException();
			}
			return arrayBytes;
		}

		///<summary>Gray16 samples as little-endian bytes (both ends are little-endian Windows processes).</summary>
		public static byte[] Gray16ToBytes(ushort[] arraySamples) {
			byte[] arrayBytes=new byte[arraySamples.Length*2];
			Buffer.BlockCopy(arraySamples,0,arrayBytes,0,arrayBytes.Length);
			return arrayBytes;
		}

		public static ushort[] BytesToGray16(byte[] arrayBytes) {
			if(arrayBytes.Length%2!=0) {
				throw new InvalidDataException("The TWAIN helper sent "+arrayBytes.Length+" bytes of 16-bit samples, an odd number.");
			}
			ushort[] arraySamples=new ushort[arrayBytes.Length/2];
			Buffer.BlockCopy(arrayBytes,0,arraySamples,0,arrayBytes.Length);
			return arraySamples;
		}
		#endregion Values

		#region TwainCaptureOptions
		public static void WriteOptions(BinaryWriter writer,TwainCaptureOptions options) {
			writer.Write(options.ImagingDeviceNum);
			writer.Write((int)options.SessionMode);
			writer.Write(options.Bits);
			writer.Write((int)options.ByteOrder16);
			writer.Write((int)options.Polarity);
			writer.Write(options.Headless);
			writer.Write(options.DisableIsSafe);
			writer.Write(options.SessionTimeout.Ticks);
			WriteNullableUShort(writer,options.SafeDisableCap);
			WriteList(writer,options.RefusedDisableCodes,(w,x) => w.Write(x));
			WriteNullableUShort(writer,options.ContinuousCap);
			WriteNullableUShort(writer,options.RedeliverCap);
			WriteNullableUShort(writer,options.NoActivateCap);
			writer.Write(options.Allow16Bit);
			writer.Write(options.RestrictSources);
			WriteList(writer,options.AllowedSources,WriteString);
			writer.Write(options.JournalKeep);
			writer.Write(options.OpenTimeout.Ticks);
			WriteList(writer,options.ListWarnings,WriteString);
			WriteString(writer,options.ConfigPath);
			writer.Write(options.RefusedDisableWait.Ticks);
			writer.Write(options.TickMilliseconds);
		}

		public static TwainCaptureOptions ReadOptions(BinaryReader reader) {
			TwainCaptureOptions options=new TwainCaptureOptions();
			options.ImagingDeviceNum=reader.ReadInt64();
			options.SessionMode=(TwainSessionMode)reader.ReadInt32();
			options.Bits=reader.ReadInt32();
			options.ByteOrder16=(TwainByteOrder)reader.ReadInt32();
			options.Polarity=(TwainPolarity)reader.ReadInt32();
			options.Headless=reader.ReadBoolean();
			options.DisableIsSafe=reader.ReadBoolean();
			options.SessionTimeout=TimeSpan.FromTicks(reader.ReadInt64());
			options.SafeDisableCap=ReadNullableUShort(reader);
			options.RefusedDisableCodes=ReadList(reader,r => r.ReadUInt16());
			options.ContinuousCap=ReadNullableUShort(reader);
			options.RedeliverCap=ReadNullableUShort(reader);
			options.NoActivateCap=ReadNullableUShort(reader);
			options.Allow16Bit=reader.ReadBoolean();
			options.RestrictSources=reader.ReadBoolean();
			options.AllowedSources=ReadList(reader,ReadString);
			options.JournalKeep=reader.ReadInt32();
			options.OpenTimeout=TimeSpan.FromTicks(reader.ReadInt64());
			options.ListWarnings=ReadList(reader,ReadString);
			options.ConfigPath=ReadString(reader);
			options.RefusedDisableWait=TimeSpan.FromTicks(reader.ReadInt64());
			options.TickMilliseconds=reader.ReadInt32();
			return options;
		}
		#endregion TwainCaptureOptions

		#region TwainEnableRequest
		///<summary>Every field but Tag, which the program keeps.</summary>
		public static void WriteRequest(BinaryWriter writer,TwainEnableRequest request) {
			writer.Write(request.Bits);
			WriteString(writer,request.ReasonFallback);
			WriteNullableInt(writer,request.ByteOrder16.HasValue ? (int)request.ByteOrder16.Value : (int?)null);
			WriteNullableBool(writer,request.ShowUI);
			WriteString(writer,request.TargetLabel);
			WritePairs(writer,request.SidecarFields);
		}

		public static TwainEnableRequest ReadRequest(BinaryReader reader) {
			TwainEnableRequest request=new TwainEnableRequest();
			request.Bits=reader.ReadInt32();
			request.ReasonFallback=ReadString(reader);
			int? byteOrder=ReadNullableInt(reader);
			request.ByteOrder16=byteOrder.HasValue ? (TwainByteOrder)byteOrder.Value : (TwainByteOrder?)null;
			request.ShowUI=ReadNullableBool(reader);
			request.TargetLabel=ReadString(reader);
			request.SidecarFields=ReadPairs(reader);
			return request;
		}
		#endregion TwainEnableRequest

		#region Open result and source info
		public static void WriteOpenResult(BinaryWriter writer,TwainCaptureOpenResult result) {
			writer.Write(result.IsOpen);
			writer.Write((int)result.Error);
			WriteString(writer,result.ErrorText);
			WriteNullableUShort(writer,result.ConditionCode);
			writer.Write(result.SourceInfo!=null);
			if(result.SourceInfo!=null) {
				WriteSourceInfo(writer,result.SourceInfo);
			}
			writer.Write(result.IsSourceNotListed);
		}

		public static TwainCaptureOpenResult ReadOpenResult(BinaryReader reader) {
			TwainCaptureOpenResult result=new TwainCaptureOpenResult();
			result.IsOpen=reader.ReadBoolean();
			result.Error=(TwainCaptureOpenError)reader.ReadInt32();
			result.ErrorText=ReadString(reader);
			result.ConditionCode=ReadNullableUShort(reader);
			if(reader.ReadBoolean()) {
				result.SourceInfo=ReadSourceInfo(reader);
			}
			result.IsSourceNotListed=reader.ReadBoolean();
			return result;
		}

		public static void WriteSourceInfo(BinaryWriter writer,TwainCaptureSourceInfo info) {
			WriteString(writer,info.ProductName);
			WriteString(writer,info.Manufacturer);
			WriteString(writer,info.ProductFamily);
			writer.Write(info.VersionMajor);
			writer.Write(info.VersionMinor);
			WriteString(writer,info.VersionInfo);
			writer.Write(info.DriverIdentity!=null);
			if(info.DriverIdentity!=null) {
				WriteString(writer,info.DriverIdentity.ProductName);
				writer.Write(info.DriverIdentity.MajorNum);
				writer.Write(info.DriverIdentity.MinorNum);
				WriteString(writer,info.DriverIdentity.Info);
			}
			writer.Write(info.ProtocolMajor);
			writer.Write(info.ProtocolMinor);
			writer.Write(info.SupportedGroups);
			WriteString(writer,info.DsmName);
			WriteNullableBool(writer,info.UiControllable);
			WriteNullableBool(writer,info.DeviceOnline);
			WriteNullableBool(writer,info.Indicators);
			WriteNullableBool(writer,info.UndefinedImageSize);
			WriteNullableInt(writer,info.PixelType);
			WriteNullableInt(writer,info.PixelFlavor);
			WriteNullableInt(writer,info.BitOrder);
			WriteNullableInt(writer,info.PlanarChunky);
			WriteNullableInt(writer,info.SupportedSizes);
			WriteList(writer,info.ListBitDepths,(w,x) => w.Write(x));
			writer.Write(info.IsExtImageInfoSettable);
			WriteList(writer,info.ListSupportedExtImageInfo,(w,x) => w.Write(x));
			writer.Write(info.IsMetadataListed);
			WriteList(writer,info.ListCustomCapabilities,(w,x) => {
				WriteString(w,x.Key);
				w.Write(x.Number);
				WriteNullableInt(w,x.QuerySupport);
				WriteNullableUShort(w,x.ConditionCode);
			});
		}

		public static TwainCaptureSourceInfo ReadSourceInfo(BinaryReader reader) {
			TwainCaptureSourceInfo info=new TwainCaptureSourceInfo();
			info.ProductName=ReadString(reader);
			info.Manufacturer=ReadString(reader);
			info.ProductFamily=ReadString(reader);
			info.VersionMajor=reader.ReadInt32();
			info.VersionMinor=reader.ReadInt32();
			info.VersionInfo=ReadString(reader);
			if(reader.ReadBoolean()) {
				string productName=ReadString(reader);
				int major=reader.ReadInt32();
				int minor=reader.ReadInt32();
				info.DriverIdentity=new TwainDriverIdentity(productName,major,minor,ReadString(reader));
			}
			info.ProtocolMajor=reader.ReadInt32();
			info.ProtocolMinor=reader.ReadInt32();
			info.SupportedGroups=reader.ReadUInt32();
			info.DsmName=ReadString(reader);
			info.UiControllable=ReadNullableBool(reader);
			info.DeviceOnline=ReadNullableBool(reader);
			info.Indicators=ReadNullableBool(reader);
			info.UndefinedImageSize=ReadNullableBool(reader);
			info.PixelType=ReadNullableInt(reader);
			info.PixelFlavor=ReadNullableInt(reader);
			info.BitOrder=ReadNullableInt(reader);
			info.PlanarChunky=ReadNullableInt(reader);
			info.SupportedSizes=ReadNullableInt(reader);
			info.ListBitDepths=ReadList(reader,r => r.ReadInt32());
			info.IsExtImageInfoSettable=reader.ReadBoolean();
			info.ListSupportedExtImageInfo=ReadList(reader,r => r.ReadInt32());
			info.IsMetadataListed=reader.ReadBoolean();
			info.ListCustomCapabilities=ReadList(reader,r => new TwainCustomCapabilityInfo {
				Key=ReadString(r),
				Number=r.ReadUInt16(),
				QuerySupport=ReadNullableInt(r),
				ConditionCode=ReadNullableUShort(r),
			});
			return info;
		}
		#endregion Open result and source info

		#region Status and Ended
		public static void WriteStatus(BinaryWriter writer,TwainCaptureStatus status) {
			writer.Write((int)status.State);
			writer.Write(status.EnableId);
			WriteString(writer,status.Text);
			writer.Write(status.IsSafeDisableInEffect);
			WriteString(writer,status.LayoutUnusableText);
		}

		public static TwainCaptureStatus ReadStatus(BinaryReader reader) {
			return new TwainCaptureStatus {
				State=(TwainCaptureState)reader.ReadInt32(),
				EnableId=reader.ReadInt32(),
				Text=ReadString(reader),
				IsSafeDisableInEffect=reader.ReadBoolean(),
				LayoutUnusableText=ReadString(reader),
			};
		}

		///<summary>Every property but Tag, which the program keeps.</summary>
		public static void WriteEnded(BinaryWriter writer,TwainCaptureEnded ended) {
			writer.Write(ended.EnableId);
			writer.Write(ended.IsRedelivery);
			writer.Write((int)ended.Reason);
			writer.Write((int)ended.StopReason);
			WriteNullableUShort(writer,ended.ConditionCode);
			WriteString(writer,ended.ConditionText);
			writer.Write(ended.ImageCount);
			writer.Write(ended.ExtraImageCount);
			writer.Write(ended.DrainedImageCount);
			writer.Write(ended.TransferFailureCount);
			writer.Write(ended.SourceMayHoldImages);
			writer.Write(ended.IsSafeDisableInEffect);
			writer.Write(ended.WasDisableStartedUnsafe);
			writer.Write(ended.WasDisableRefused);
			writer.Write(ended.WasSecondDisableAfterWait);
			writer.Write(ended.BitsRequested);
			writer.Write(ended.BitDepthInEffect);
			WriteString(writer,ended.ReasonFallback);
			WriteString(writer,ended.LayoutUnusableText);
			WriteString(writer,ended.ErrorText);
			WriteDateTime(writer,ended.EndedAt);
		}

		public static TwainCaptureEnded ReadEnded(BinaryReader reader) {
			TwainCaptureEnded ended=new TwainCaptureEnded();
			ended.EnableId=reader.ReadInt32();
			ended.IsRedelivery=reader.ReadBoolean();
			ended.Reason=(TwainCaptureEndReason)reader.ReadInt32();
			ended.StopReason=(TwainCaptureStopReason)reader.ReadInt32();
			ended.ConditionCode=ReadNullableUShort(reader);
			ended.ConditionText=ReadString(reader);
			ended.ImageCount=reader.ReadInt32();
			ended.ExtraImageCount=reader.ReadInt32();
			ended.DrainedImageCount=reader.ReadInt32();
			ended.TransferFailureCount=reader.ReadInt32();
			ended.SourceMayHoldImages=reader.ReadBoolean();
			ended.IsSafeDisableInEffect=reader.ReadBoolean();
			ended.WasDisableStartedUnsafe=reader.ReadBoolean();
			ended.WasDisableRefused=reader.ReadBoolean();
			ended.WasSecondDisableAfterWait=reader.ReadBoolean();
			ended.BitsRequested=reader.ReadInt32();
			ended.BitDepthInEffect=reader.ReadInt32();
			ended.ReasonFallback=ReadString(reader);
			ended.LayoutUnusableText=ReadString(reader);
			ended.ErrorText=ReadString(reader);
			ended.EndedAt=ReadDateTime(reader);
			return ended;
		}
		#endregion Status and Ended

		#region AcquiredImage
		///<summary>Every property but Tag (the program's), and the pixels (Gray16 or Dib) only with isPixelsInline; without, the reader's image has
		///neither, and the event names the payload that ReadData returns.</summary>
		public static void WriteImage(BinaryWriter writer,AcquiredImage image,bool isPixelsInline) {
			writer.Write(image.Width);
			writer.Write(image.Height);
			writer.Write(image.BitsPerPixel);
			writer.Write(image.BitsPerSample);
			writer.Write(image.PixelType);
			writer.Write(image.PixelFlavor);
			writer.Write(BitConverter.DoubleToInt64Bits(image.XDpi));
			writer.Write(BitConverter.DoubleToInt64Bits(image.YDpi));
			WriteString(writer,image.MetadataJson);
			WriteDateTime(writer,image.ReceivedAt);
			writer.Write(image.EnableId);
			WriteString(writer,image.TargetLabel);
			writer.Write((int)image.StopReason);
			writer.Write((int)image.Kind);
			writer.Write(image.IsRedelivery);
			writer.Write(image.RequestedBits);
			writer.Write(image.BitDepthInEffect);
			WriteString(writer,image.BitDepthReason);
			writer.Write(image.IsDeliveredAs8BitIn16BitEnable);
			writer.Write((int)image.ByteOrder16);
			writer.Write((int)image.TransferMechanism);
			WriteString(writer,image.SourceName);
			writer.Write(image.IndexInEnable);
			writer.Write(image.IsContinuousEnable);
			WriteString(writer,image.JournalPath);
			WriteString(writer,image.QuarantinePath);
			WritePairs(writer,image.SidecarFields);
			if(!isPixelsInline) {
				writer.Write(PIXELS_NONE);
				return;
			}
			if(image.Gray16!=null) {
				writer.Write(PIXELS_GRAY16);
				writer.Write((long)image.Gray16.Length*2);
				writer.Write(Gray16ToBytes(image.Gray16));
			}
			else if(image.Dib!=null) {
				writer.Write(PIXELS_DIB);
				writer.Write((long)image.Dib.Length);
				writer.Write(image.Dib);
			}
			else {
				writer.Write(PIXELS_NONE);
			}
		}

		public static AcquiredImage ReadImage(BinaryReader reader) {
			AcquiredImage image=new AcquiredImage();
			image.Width=reader.ReadInt32();
			image.Height=reader.ReadInt32();
			image.BitsPerPixel=reader.ReadInt32();
			image.BitsPerSample=reader.ReadInt32();
			image.PixelType=reader.ReadInt32();
			image.PixelFlavor=reader.ReadInt32();
			image.XDpi=BitConverter.Int64BitsToDouble(reader.ReadInt64());
			image.YDpi=BitConverter.Int64BitsToDouble(reader.ReadInt64());
			image.MetadataJson=ReadString(reader);
			image.ReceivedAt=ReadDateTime(reader);
			image.EnableId=reader.ReadInt32();
			image.TargetLabel=ReadString(reader);
			image.StopReason=(TwainCaptureStopReason)reader.ReadInt32();
			image.Kind=(AcquiredImageKind)reader.ReadInt32();
			image.IsRedelivery=reader.ReadBoolean();
			image.RequestedBits=reader.ReadInt32();
			image.BitDepthInEffect=reader.ReadInt32();
			image.BitDepthReason=ReadString(reader);
			image.IsDeliveredAs8BitIn16BitEnable=reader.ReadBoolean();
			image.ByteOrder16=(TwainByteOrder)reader.ReadInt32();
			image.TransferMechanism=(TwainTransferMechanism)reader.ReadInt32();
			image.SourceName=ReadString(reader);
			image.IndexInEnable=reader.ReadInt32();
			image.IsContinuousEnable=reader.ReadBoolean();
			image.JournalPath=ReadString(reader);
			image.QuarantinePath=ReadString(reader);
			image.SidecarFields=ReadPairs(reader);
			byte pixels=reader.ReadByte();
			switch(pixels) {
				case PIXELS_NONE:
					break;
				case PIXELS_GRAY16:
					image.Gray16=BytesToGray16(ReadBytes(reader,reader.ReadInt64()));
					break;
				case PIXELS_DIB:
					image.Dib=ReadBytes(reader,reader.ReadInt64());
					break;
				default:
					throw new InvalidDataException("The TWAIN helper's image has pixels of kind "+pixels+".");
			}
			return image;
		}
		#endregion AcquiredImage

		#region Events and sources
		public static void WriteEvents(BinaryWriter writer,List<TwainHelperEvent> listEvents) {
			WriteList(writer,listEvents,(w,x) => {
				w.Write((byte)x.Kind);
				switch(x.Kind) {
					case TwainHelperEventKind.Status:
						WriteStatus(w,x.Status);
						break;
					case TwainHelperEventKind.Ended:
						WriteEnded(w,x.Ended);
						break;
					case TwainHelperEventKind.ImageAcquired:
					case TwainHelperEventKind.ImageQuarantined:
						WriteImage(w,x.Image,isPixelsInline:false);
						WriteString(w,x.PayloadId);
						w.Write(x.PayloadLength);
						w.Write(x.IsPayloadGray16);
						break;
					case TwainHelperEventKind.Logged:
						WriteString(w,x.Line);
						break;
					default:
						throw new InvalidDataException("No such TWAIN helper event: "+x.Kind);
				}
			});
		}

		public static List<TwainHelperEvent> ReadEvents(BinaryReader reader) {
			return ReadList(reader,r => {
				TwainHelperEvent helperEvent=new TwainHelperEvent();
				helperEvent.Kind=(TwainHelperEventKind)r.ReadByte();
				switch(helperEvent.Kind) {
					case TwainHelperEventKind.Status:
						helperEvent.Status=ReadStatus(r);
						break;
					case TwainHelperEventKind.Ended:
						helperEvent.Ended=ReadEnded(r);
						break;
					case TwainHelperEventKind.ImageAcquired:
					case TwainHelperEventKind.ImageQuarantined:
						helperEvent.Image=ReadImage(r);
						helperEvent.PayloadId=ReadString(r)??"";
						helperEvent.PayloadLength=r.ReadInt64();
						helperEvent.IsPayloadGray16=r.ReadBoolean();
						if(helperEvent.PayloadLength<0 || helperEvent.PayloadLength>ARRAY_MAX) {
							throw new InvalidDataException("The TWAIN helper's image has "+helperEvent.PayloadLength+" bytes of pixels.");
						}
						break;
					case TwainHelperEventKind.Logged:
						helperEvent.Line=ReadString(r);
						break;
					default:
						throw new InvalidDataException("No such TWAIN helper event: "+(int)helperEvent.Kind);
				}
				return helperEvent;
			});
		}

		public static void WriteSources(BinaryWriter writer,List<TwainSourceEntry> listSources) {
			WriteList(writer,listSources,(w,x) => {
				WriteString(w,x.ProductName);
				WriteString(w,x.Manufacturer);
				WriteString(w,x.ProductFamily);
				w.Write(x.VersionMajor);
				w.Write(x.VersionMinor);
				WriteString(w,x.VersionInfo);
				w.Write((int)x.Dsm);
				WriteString(w,x.DsmLibrary);
			});
		}

		public static List<TwainSourceEntry> ReadSources(BinaryReader reader) {
			return ReadList(reader,r => new TwainSourceEntry {
				ProductName=ReadString(r),
				Manufacturer=ReadString(r),
				ProductFamily=ReadString(r),
				VersionMajor=r.ReadInt32(),
				VersionMinor=r.ReadInt32(),
				VersionInfo=ReadString(r),
				Dsm=(TwainDsmKind)r.ReadInt32(),
				DsmLibrary=ReadString(r),
			});
		}
		#endregion Events and sources
	}
}
