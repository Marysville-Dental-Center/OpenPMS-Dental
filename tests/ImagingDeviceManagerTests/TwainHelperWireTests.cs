// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ImagingDeviceManager.Capture;
using ImagingDeviceManager.Helper32;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingDeviceManager.Tests {
	///<summary>TwainHelperWire (S3b-8): every public property (and field) of each structure the TWAIN module's commands carry survives the bytes,
	///set by reflection to a value that is not its default, so a property added later and not written fails here; Tag never crosses (the
	///program keeps it). A cut or padded message, a wrong pixel kind and a list or array over the bounds are refused before a large allocation.
	///</summary>
	[TestClass]
	public class TwainHelperWireTests {
		private int _seed=1;

		///<summary>A value of type that is not its default, different for each call.</summary>
		private object NewValue(Type type) {
			int n=_seed++;
			Type typeNullable=Nullable.GetUnderlyingType(type);
			if(typeNullable!=null) {
				return NewValue(typeNullable);
			}
			if(type==typeof(string)) return "text "+n+" éß 日";
			if(type==typeof(int)) return 1000+n;
			if(type==typeof(long)) return 5000000000L+n;
			if(type==typeof(uint)) return (uint)(70000+n);
			if(type==typeof(ushort)) return (ushort)(0x8000+n);
			if(type==typeof(bool)) return true;
			if(type==typeof(double)) return 300.25+n;
			if(type==typeof(TimeSpan)) return TimeSpan.FromMilliseconds(12345+n);
			if(type==typeof(DateTime)) return new DateTime(2026,11,16,10,20,30,DateTimeKind.Local).AddTicks(n);
			if(type.IsEnum) {
				Array arrayValues=Enum.GetValues(type);
				return arrayValues.GetValue(arrayValues.Length-1);
			}
			if(type==typeof(List<string>)) return new List<string> { "a"+n,null,"" };
			if(type==typeof(List<int>)) return new List<int> { 8,14,16+n };
			if(type==typeof(List<ushort>)) return new List<ushort> { 0x8001,(ushort)(0x8002+n) };
			if(type==typeof(List<KeyValuePair<string,string>>)) return new List<KeyValuePair<string,string>> {
				new KeyValuePair<string,string>("MountNum",n.ToString()),new KeyValuePair<string,string>("Empty","") };
			if(type==typeof(ushort[])) return new ushort[] { 1,65535,(ushort)n };
			if(type==typeof(byte[])) return new byte[] { 0,255,(byte)n };
			if(type==typeof(TwainDriverIdentity)) return new TwainDriverIdentity("Driver "+n,2,3,"2.3.1 build "+n);
			if(type==typeof(List<TwainCustomCapabilityInfo>)) return new List<TwainCustomCapabilityInfo> {
				new TwainCustomCapabilityInfo { Key="SafeDisableCap",Number=0x8100,QuerySupport=3,ConditionCode=null },
				new TwainCustomCapabilityInfo { Key="RedeliverCap",Number=0x8101,QuerySupport=null,ConditionCode=0x0006 } };
			if(type==typeof(TwainCaptureSourceInfo)) return Fill(new TwainCaptureSourceInfo());
			throw new AssertInconclusiveException("No test value for "+type.FullName+": add one, and write the property in TwainHelperWire.");
		}

		///<summary>Sets every writable public property and field but Tag, and those named in except.</summary>
		private T Fill<T>(T value,params string[] except) {
			foreach(PropertyInfo property in typeof(T).GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)) {
				if(!property.CanWrite || property.Name=="Tag" || except.Contains(property.Name) || property.SetMethod==null || property.SetMethod.IsPrivate) {
					continue;
				}
				property.SetValue(value,NewValue(property.PropertyType));
			}
			foreach(FieldInfo field in typeof(T).GetFields(BindingFlags.Public|BindingFlags.Instance)) {
				if(field.Name=="Tag" || except.Contains(field.Name)) {
					continue;
				}
				field.SetValue(value,NewValue(field.FieldType));
			}
			return value;
		}

		private static void AssertSame(object expected,object actual,string path) {
			if(expected==null || actual==null) {
				Assert.AreEqual(expected,actual,path);
				return;
			}
			Type type=expected.GetType();
			if(type.IsPrimitive || type==typeof(string) || type.IsEnum || type==typeof(decimal) || type==typeof(TimeSpan)) {
				Assert.AreEqual(expected,actual,path);
				return;
			}
			if(type==typeof(DateTime)) {
				Assert.AreEqual(((DateTime)expected).Ticks,((DateTime)actual).Ticks,path);
				Assert.AreEqual(((DateTime)expected).Kind,((DateTime)actual).Kind,path+" (kind)");
				return;
			}
			if(expected is IEnumerable enumerableExpected) {
				List<object> listExpected=enumerableExpected.Cast<object>().ToList();
				List<object> listActual=((IEnumerable)actual).Cast<object>().ToList();
				Assert.AreEqual(listExpected.Count,listActual.Count,path+" (count)");
				for(int i=0;i<listExpected.Count;i++) {
					AssertSame(listExpected[i],listActual[i],path+"["+i+"]");
				}
				return;
			}
			if(type.IsGenericType && type.GetGenericTypeDefinition()==typeof(KeyValuePair<,>)) {
				AssertSame(type.GetProperty("Key").GetValue(expected),type.GetProperty("Key").GetValue(actual),path+".Key");
				AssertSame(type.GetProperty("Value").GetValue(expected),type.GetProperty("Value").GetValue(actual),path+".Value");
				return;
			}
			foreach(PropertyInfo property in type.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)) {
				if(property.Name=="Tag" || property.GetIndexParameters().Length>0) {
					continue;
				}
				AssertSame(property.GetValue(expected),property.GetValue(actual),path+"."+property.Name);
			}
			foreach(FieldInfo field in type.GetFields(BindingFlags.Public|BindingFlags.Instance)) {
				if(field.Name=="Tag") {
					continue;
				}
				AssertSame(field.GetValue(expected),field.GetValue(actual),path+"."+field.Name);
			}
		}

		private static T RoundTrip<T>(T value,Action<BinaryWriter,T> actionWrite,Func<BinaryReader,T> funcRead) {
			byte[] data=TwainHelperWire.Write(w => actionWrite(w,value));
			return TwainHelperWire.Read(data,funcRead);
		}

		[TestMethod]
		public void Options_EveryProperty_IncludingTheInternalOnes() {
			TwainCaptureOptions options=Fill(new TwainCaptureOptions());
			TwainCaptureOptions read=RoundTrip(options,TwainHelperWire.WriteOptions,TwainHelperWire.ReadOptions);
			AssertSame(options,read,"options");
			Assert.AreEqual(options.TickMilliseconds,read.TickMilliseconds);
			Assert.AreEqual(options.RefusedDisableWait,read.RefusedDisableWait);
		}

		[TestMethod]
		public void EnableRequest_EveryFieldButTag() {
			TwainEnableRequest request=Fill(new TwainEnableRequest());
			request.Tag=new object();
			TwainEnableRequest read=RoundTrip(request,TwainHelperWire.WriteRequest,TwainHelperWire.ReadRequest);
			AssertSame(request,read,"request");
			Assert.IsNull(read.Tag,"The Tag stays in the program.");
			TwainEnableRequest requestNulls=new TwainEnableRequest { ReasonFallback=null,ByteOrder16=null,ShowUI=null,TargetLabel=null,SidecarFields=null };
			AssertSame(requestNulls,RoundTrip(requestNulls,TwainHelperWire.WriteRequest,TwainHelperWire.ReadRequest),"request with nulls");
		}

		[TestMethod]
		public void OpenResult_AndSourceInfo_EveryProperty() {
			TwainCaptureOpenResult result=Fill(new TwainCaptureOpenResult());
			AssertSame(result,RoundTrip(result,TwainHelperWire.WriteOpenResult,TwainHelperWire.ReadOpenResult),"open result");
			TwainCaptureOpenResult resultBare=new TwainCaptureOpenResult { Error=TwainCaptureOpenError.SourceNotFound,ErrorText="not found" };
			AssertSame(resultBare,RoundTrip(resultBare,TwainHelperWire.WriteOpenResult,TwainHelperWire.ReadOpenResult),"open result without source info");
			TwainCaptureSourceInfo infoNulls=new TwainCaptureSourceInfo { ListBitDepths=null,ListSupportedExtImageInfo=null,DriverIdentity=null };
			AssertSame(infoNulls,RoundTrip(infoNulls,TwainHelperWire.WriteSourceInfo,TwainHelperWire.ReadSourceInfo),"source info with nulls");
		}

		[TestMethod]
		public void StatusAndEnded_EveryPropertyButTag() {
			TwainCaptureStatus status=Fill(new TwainCaptureStatus());
			AssertSame(status,RoundTrip(status,TwainHelperWire.WriteStatus,TwainHelperWire.ReadStatus),"status");
			TwainCaptureEnded ended=Fill(new TwainCaptureEnded());
			ended.Tag=new object();
			TwainCaptureEnded read=RoundTrip(ended,TwainHelperWire.WriteEnded,TwainHelperWire.ReadEnded);
			AssertSame(ended,read,"ended");
			Assert.IsNull(read.Tag);
		}

		[TestMethod]
		public void Image_EveryPropertyButTag_PixelsInlineOrApart() {
			AcquiredImage image=Fill(new AcquiredImage(),"Dib");
			image.Tag=new object();
			AcquiredImage read=RoundTrip(image,(w,x) => TwainHelperWire.WriteImage(w,x,true),TwainHelperWire.ReadImage);
			AssertSame(image,read,"image with Gray16");
			Assert.IsNull(read.Tag);
			AcquiredImage imageDib=Fill(new AcquiredImage(),"Gray16");
			AssertSame(imageDib,RoundTrip(imageDib,(w,x) => TwainHelperWire.WriteImage(w,x,true),TwainHelperWire.ReadImage),"image with a DIB");
			AcquiredImage readApart=RoundTrip(image,(w,x) => TwainHelperWire.WriteImage(w,x,false),TwainHelperWire.ReadImage);
			Assert.IsNull(readApart.Gray16,"Without its pixels, which ReadData carries.");
			Assert.AreEqual(image.MetadataJson,readApart.MetadataJson);
			Assert.AreEqual(image.BitDepthInEffect,readApart.BitDepthInEffect);
		}

		[TestMethod]
		public void Events_AndSources_InOrder() {
			List<TwainHelperEvent> listEvents=new List<TwainHelperEvent> {
				new TwainHelperEvent { Kind=TwainHelperEventKind.Status,Status=Fill(new TwainCaptureStatus()) },
				new TwainHelperEvent { Kind=TwainHelperEventKind.ImageAcquired,Image=Fill(new AcquiredImage(),"Dib","Gray16"),PayloadId="p1",PayloadLength=12,
					IsPayloadGray16=true },
				new TwainHelperEvent { Kind=TwainHelperEventKind.Logged,Line="a line" },
				new TwainHelperEvent { Kind=TwainHelperEventKind.Ended,Ended=Fill(new TwainCaptureEnded()) },
				new TwainHelperEvent { Kind=TwainHelperEventKind.ImageQuarantined,Image=Fill(new AcquiredImage(),"Dib","Gray16") },
			};
			List<TwainHelperEvent> listRead=RoundTrip(listEvents,TwainHelperWire.WriteEvents,TwainHelperWire.ReadEvents);
			AssertSame(listEvents,listRead,"events");
			List<TwainSourceEntry> listSources=new List<TwainSourceEntry> { Fill(new TwainSourceEntry()),new TwainSourceEntry { ProductName="B" } };
			AssertSame(listSources,RoundTrip(listSources,TwainHelperWire.WriteSources,TwainHelperWire.ReadSources),"sources");
		}

		[TestMethod]
		public void BrokenData_Refused() {
			byte[] data=TwainHelperWire.Write(w => TwainHelperWire.WriteStatus(w,Fill(new TwainCaptureStatus())));
			Assert.ThrowsExactly<InvalidDataException>(() => TwainHelperWire.Read(data.Take(data.Length-1).ToArray(),TwainHelperWire.ReadStatus),"Cut short.");
			Assert.ThrowsExactly<InvalidDataException>(() => TwainHelperWire.Read(data.Concat(new byte[] { 0 }).ToArray(),TwainHelperWire.ReadStatus),
				"A byte too many.");
			Assert.ThrowsExactly<InvalidDataException>(() => TwainHelperWire.Read<TwainCaptureStatus>(null,TwainHelperWire.ReadStatus));
			byte[] dataList=TwainHelperWire.Write(w => w.Write(int.MaxValue));
			Assert.ThrowsExactly<InvalidDataException>(() => TwainHelperWire.Read(dataList,TwainHelperWire.ReadSources),"A list over the bound.");
			byte[] dataImage=TwainHelperWire.Write(w => TwainHelperWire.WriteImage(w,new AcquiredImage(),false));
			dataImage[dataImage.Length-1]=7;
			Assert.ThrowsExactly<InvalidDataException>(() => TwainHelperWire.Read(dataImage,TwainHelperWire.ReadImage),"No such pixel kind.");
			Assert.ThrowsExactly<InvalidDataException>(() => TwainHelperWire.BytesToGray16(new byte[3]),"An odd number of bytes.");
		}
	}
}
