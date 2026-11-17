// S3a-FU2 (docs/port-notes/S3a-M2.md, section S3a-FU2): the paste reads the bytes of the program's clipboard format through the data object's
// OLE interface (IDataObject.GetData with TYMED_HGLOBAL, NodeTypeAndKeyClipboard.ReadBytes), so no data under the format reaches the
// BinaryFormatter. A private OLE data object of this test process holds one memory block under the format, as another program's data on the
// clipboard would: the program's JSON, a block that starts with the serialized-object GUID (an object that the WinForms data object's own
// GetData deserializes), and oversize or garbage blocks. It is read directly and wrapped by a WinForms or WPF DataObject, as the paste's
// Clipboard.GetDataObject wraps another program's data. Never the system clipboard; nothing is shown. The proofs that a block bites use
// .NET Framework's WinForms GetData, which deserialized it; on .NET 10 (stage 3b) that route no longer deserializes, and the proofs check the
// block with .NET's own reader of the format instead (S3b-5).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using IComDataObject=System.Runtime.InteropServices.ComTypes.IDataObject;

namespace S3aM1Tests {
	[TestClass]
	public class M2ClipboardOleTests {
		private const string DB="opendental_example";

		[TestMethod]
		public void PrivateOleDataObject_TheProgramsJson_IsReadThroughTheMemoryBlock() {
			M2Support.RunSta(() => {
				byte[] byteArrayJson=Encoding.UTF8.GetBytes(NodeTypeAndKeyClipboard.ToJson(new NodeTypeAndKey(EnumImageNodeType.Mount,31),DB));
				//As written, and zero filled after the JSON as a rounded-up memory block is.
				foreach(byte[] byteArrayBlock in new[] { byteArrayJson,byteArrayJson.Concat(new byte[40]).ToArray() }) {
					OleDataObjectPrivate oleDataObjectPrivate=new OleDataObjectPrivate(byteArrayBlock);
					CollectionAssert.AreEqual(byteArrayBlock,NodeTypeAndKeyClipboard.ReadBytes(oleDataObjectPrivate),"The block, byte for byte.");
					List<NodeTypeAndKey> listNodeTypeAndKeys=new List<NodeTypeAndKey>();
					List<string> listDbNameOrUris=new List<string>();
					System.Windows.Forms.DataObject dataObjectForms=new System.Windows.Forms.DataObject(oleDataObjectPrivate);
					listNodeTypeAndKeys.Add(NodeTypeAndKeyClipboard.GetData(dataObjectForms,out string dbNameOrUriForms));
					listDbNameOrUris.Add(dbNameOrUriForms);
					System.Windows.DataObject dataObjectWpf=new System.Windows.DataObject(oleDataObjectPrivate);
					listNodeTypeAndKeys.Add(NodeTypeAndKeyClipboard.GetData((System.Windows.IDataObject)dataObjectWpf,out string dbNameOrUriWpf));
					listDbNameOrUris.Add(dbNameOrUriWpf);
					for(int i=0;i<listNodeTypeAndKeys.Count;i++) {
						Assert.IsNotNull(listNodeTypeAndKeys[i],"Read "+i+".");
						Assert.AreEqual(EnumImageNodeType.Mount,listNodeTypeAndKeys[i].NodeType);
						Assert.AreEqual(31,listNodeTypeAndKeys[i].PriKey);
						Assert.AreEqual(DB,listDbNameOrUris[i]);
					}
					//Every read asked for the memory block only: no stream, file or other medium a wrapper could turn into an object.
					Assert.AreEqual(3,oleDataObjectPrivate.ListTymedsAsked.Count,"ReadBytes, then the WinForms and the WPF read.");
					Assert.IsTrue(oleDataObjectPrivate.ListTymedsAsked.All(x => x==TYMED.TYMED_HGLOBAL),
						string.Join(",",oleDataObjectPrivate.ListTymedsAsked));
				}
			});
		}

		[TestMethod]
		public void PrivateOleDataObject_ASerializedObjectBlock_IsNeverDeserialized() {
			M2Support.RunSta(() => {
				byte[] byteArrayWitness=SerializedObjectBlock(new DeserializationWitness());
				//The block is the serialized witness: .NET's own reader of the format (System.Formats.Nrbf) reads the witness's class from it.
				using(MemoryStream memoryStream=new MemoryStream(byteArrayWitness,16,byteArrayWitness.Length-16)) {
					System.Formats.Nrbf.SerializationRecord serializationRecord=System.Formats.Nrbf.NrbfDecoder.Decode(memoryStream);
					Assert.AreEqual(typeof(DeserializationWitness).FullName,serializationRecord.TypeName.FullName);
					Assert.AreEqual(typeof(DeserializationWitness).Assembly.FullName,serializationRecord.TypeName.AssemblyName.FullName);
				}
				//On .NET Framework the WinForms data object's own GetData, the route the paste took before, deserialized it: the proof that the block
				//bites (S3a-FU2). On .NET 10 (stage 3b) that route has no BinaryFormatter and deserializes nothing either, whatever it gives back.
				DeserializationWitness.CountDeserialized=0;
				System.Windows.Forms.DataObject dataObjectProof=new System.Windows.Forms.DataObject(new OleDataObjectPrivate(byteArrayWitness));
				object dataOldRoute=null;
				try {
					dataOldRoute=dataObjectProof.GetData(NodeTypeAndKeyClipboard.Format,false);
				}
				catch(Exception) {
					//Refused: nothing deserialized.
				}
				Assert.IsNotInstanceOfType(dataOldRoute,typeof(DeserializationWitness),"The old route on .NET 10.");
				Assert.AreEqual(0,DeserializationWitness.CountDeserialized,"The old route on .NET 10 deserializes nothing.");
				//The paste's read, through WinForms and WPF wrappers and directly: rejected, and never deserialized.
				DeserializationWitness.CountDeserialized=0;
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData(new System.Windows.Forms.DataObject(new OleDataObjectPrivate(byteArrayWitness)),
					out string dbNameOrUriForms));
				Assert.AreEqual("",dbNameOrUriForms);
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData(
					(System.Windows.IDataObject)new System.Windows.DataObject(new OleDataObjectPrivate(byteArrayWitness)),out string dbNameOrUriWpf));
				Assert.AreEqual("",dbNameOrUriWpf);
				Assert.IsNull(NodeTypeAndKeyClipboard.FromBytes(byteArrayWitness,out string _));
				Assert.AreEqual(0,DeserializationWitness.CountDeserialized,"Nothing was deserialized.");
				//A serialized string that holds the program's JSON: the old read took a string under the format as the JSON.
				string json=NodeTypeAndKeyClipboard.ToJson(new NodeTypeAndKey(EnumImageNodeType.Document,8),DB);
				byte[] byteArrayString=SerializedObjectBlock(json);
				Assert.AreEqual(json,new System.Windows.Forms.DataObject(new OleDataObjectPrivate(byteArrayString)).GetData(NodeTypeAndKeyClipboard.Format,false),
					"The old route gives the string.");
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData(new System.Windows.Forms.DataObject(new OleDataObjectPrivate(byteArrayString)),out string _));
				//The GUID followed by the JSON's bytes is rejected on the GUID alone.
				byte[] byteArrayGuidJson=NodeTypeAndKeyClipboard.GuidSerializedObject.ToByteArray().Concat(Encoding.UTF8.GetBytes(json)).ToArray();
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData(new System.Windows.Forms.DataObject(new OleDataObjectPrivate(byteArrayGuidJson)),out string _));
				Assert.IsNull(NodeTypeAndKeyClipboard.FromBytes(byteArrayGuidJson,out string _));
			});
		}

		[TestMethod]
		public void SerializedObjectGuid_IsTheOneTheWinFormsDataObjectWrites() {
			M2Support.RunSta(() => {
				//An earlier version's kind of copy: the NodeTypeAndKey object itself under the format. Its block is the GUID and the object.
				System.Windows.Forms.DataObject dataObjectObject=new System.Windows.Forms.DataObject();
				dataObjectObject.SetData(NodeTypeAndKeyClipboard.Format,new NodeTypeAndKey(EnumImageNodeType.Document,12));
				byte[] byteArrayBlock=NodeTypeAndKeyClipboard.ReadBytes(dataObjectObject);
				Assert.IsNotNull(byteArrayBlock,"The block of a small serialized object.");
				CollectionAssert.AreEqual(NodeTypeAndKeyClipboard.GuidSerializedObject.ToByteArray(),byteArrayBlock.Take(16).ToArray());
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData(dataObjectObject,out string _));
			});
		}

		[TestMethod]
		public void PrivateOleDataObject_OversizeGarbageAndFailures_GiveNull() {
			M2Support.RunSta(() => {
				byte[] byteArrayJson=Encoding.UTF8.GetBytes(NodeTypeAndKeyClipboard.ToJson(new NodeTypeAndKey(EnumImageNodeType.Document,5),DB));
				Random random=new Random(20261115);
				byte[] byteArrayRandom=new byte[300];
				random.NextBytes(byteArrayRandom);
				byteArrayRandom[0]=0x7b;//not the GUID's first byte, so the garbage is not rejected on the GUID
				List<Tuple<string,byte[]>> listBlocks=new List<Tuple<string,byte[]>> {
					Tuple.Create("empty",new byte[0]),
					Tuple.Create("zeros only",new byte[64]),
					Tuple.Create("larger than the limit plus rounding",Enumerable.Repeat((byte)'a',4096+65).ToArray()),
					Tuple.Create("very large",Enumerable.Repeat((byte)' ',100000).ToArray()),
					Tuple.Create("more than the limit before the zeros",Enumerable.Repeat((byte)' ',4097).Concat(new byte[8]).ToArray()),
					Tuple.Create("garbage",byteArrayRandom),
					Tuple.Create("not UTF-8",new byte[] { 0xff,0xfe,0xfd,0x80,0x81,0x00 }),
					Tuple.Create("UTF-16 JSON",Encoding.Unicode.GetBytes(Encoding.UTF8.GetString(byteArrayJson))),
					Tuple.Create("JSON and more",byteArrayJson.Concat(Encoding.UTF8.GetBytes("garbage")).ToArray()),
					Tuple.Create("JSON, a zero and more",byteArrayJson.Concat(new byte[] { 0,0x41 }).ToArray()),
					Tuple.Create("half the JSON",byteArrayJson.Take(byteArrayJson.Length/2).ToArray()),
				};
				foreach(Tuple<string,byte[]> tuple in listBlocks) {
					Assert.IsNull(NodeTypeAndKeyClipboard.GetData(new System.Windows.Forms.DataObject(new OleDataObjectPrivate(tuple.Item2)),out string dbNameOrUri),
						tuple.Item1);
					Assert.AreEqual("",dbNameOrUri,tuple.Item1);
					Assert.IsNull(NodeTypeAndKeyClipboard.FromBytes(tuple.Item2,out string _),tuple.Item1);
				}
				Assert.IsNull(NodeTypeAndKeyClipboard.ReadBytes(new OleDataObjectPrivate(Enumerable.Repeat((byte)'a',4096+65).ToArray())),"Oversize.");
				Assert.IsNull(NodeTypeAndKeyClipboard.ReadBytes(null));
				Assert.IsNull(NodeTypeAndKeyClipboard.FromBytes(null,out string _));
				//A data object that fails, or gives no memory block: no key, and nothing thrown out of the read.
				OleDataObjectPrivate oleDataObjectNullHandle=new OleDataObjectPrivate(byteArrayJson){ IsNullHandle=true };
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData(new System.Windows.Forms.DataObject(oleDataObjectNullHandle),out string _));
				OleDataObjectPrivate oleDataObjectThrows=new OleDataObjectPrivate(byteArrayJson){ ExceptionGetData=new InvalidOperationException("synthetic") };
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData(new System.Windows.Forms.DataObject(oleDataObjectThrows),out string _));
				OleDataObjectPrivate oleDataObjectComFails=new OleDataObjectPrivate(byteArrayJson){ ExceptionGetData=new COMException("synthetic",unchecked((int)0x80004005)) };
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData(new System.Windows.Forms.DataObject(oleDataObjectComFails),out string _));
				//The same JSON in a working block is read: the failures above are the data object's, not the JSON's.
				Assert.AreEqual(5,NodeTypeAndKeyClipboard.GetData(new System.Windows.Forms.DataObject(new OleDataObjectPrivate(byteArrayJson)),out string _).PriKey);
			});
		}

		///<summary>The memory block that the WinForms data object writes for an object under a custom format: the GUID, then the BinaryFormatter's
		///bytes of the object (the .NET Remoting Binary Format, MS-NRBF). .NET 10 has no BinaryFormatter (it throws PlatformNotSupportedException,
		///and SYSLIB0011 is an error), so the bytes are written here record by record, as the .NET Framework's BinaryFormatter wrote them for the
		///two kinds of object these tests use (S3b-5): a string (SerializationHeaderRecord, BinaryObjectString, MessageEnd) and an object of a
		///class without instance fields (SerializationHeaderRecord, BinaryLibrary with the assembly's full name, ClassWithMembersAndTypes with no
		///members, MessageEnd); https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-nrbf.</summary>
		private static byte[] SerializedObjectBlock(object obj) {
			using(MemoryStream memoryStream=new MemoryStream())
			using(BinaryWriter binaryWriter=new BinaryWriter(memoryStream,new UTF8Encoding(false))) {
				binaryWriter.Write(NodeTypeAndKeyClipboard.GuidSerializedObject.ToByteArray());
				//SerializationHeaderRecord: RootId 1, HeaderId -1, version 1.0.
				binaryWriter.Write((byte)0x00);
				binaryWriter.Write(1);
				binaryWriter.Write(-1);
				binaryWriter.Write(1);
				binaryWriter.Write(0);
				if(obj is string text) {
					//BinaryObjectString: ObjectId 1, the value as a LengthPrefixedString (BinaryWriter.Write(string) writes the same 7-bit length).
					binaryWriter.Write((byte)0x06);
					binaryWriter.Write(1);
					binaryWriter.Write(text);
				}
				else {
					Type type=obj.GetType();
					Assert.AreEqual(0,type.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Length,
						type.Name+": only a class without instance fields is written here.");
					//BinaryLibrary: LibraryId 2, the assembly's full name.
					binaryWriter.Write((byte)0x0C);
					binaryWriter.Write(2);
					binaryWriter.Write(type.Assembly.FullName);
					//ClassWithMembersAndTypes: ObjectId 1, the type's full name, no members, LibraryId 2.
					binaryWriter.Write((byte)0x05);
					binaryWriter.Write(1);
					binaryWriter.Write(type.FullName);
					binaryWriter.Write(0);
					binaryWriter.Write(2);
				}
				//MessageEnd.
				binaryWriter.Write((byte)0x0B);
				binaryWriter.Flush();
				return memoryStream.ToArray();
			}
		}
	}

	///<summary>Counts its deserializations: a block that holds it shows whether a read deserialized the block.</summary>
	[Serializable]
	public class DeserializationWitness {
		public static int CountDeserialized;

		[OnDeserialized]
		private void OnDeserialized(StreamingContext streamingContext) {
			CountDeserialized++;
		}
	}

	///<summary>An OLE data object of this test process that holds one memory block under the program's clipboard format, as another program's
	///data on the clipboard would. GetData makes a new block for each call (the caller frees it) and logs the medium it was asked for.</summary>
	public class OleDataObjectPrivate:IComDataObject {
		private const int DV_E_FORMATETC=unchecked((int)0x80040064);
		private const int OLE_E_ADVISENOTSUPPORTED=unchecked((int)0x80040003);
		private const uint GMEM_MOVEABLE=0x0002;
		private readonly byte[] _byteArray;
		///<summary>The tymed of each GetData call, in order.</summary>
		public readonly List<TYMED> ListTymedsAsked=new List<TYMED>();
		///<summary>GetData gives TYMED_HGLOBAL with no handle.</summary>
		public bool IsNullHandle;
		///<summary>GetData throws this.</summary>
		public Exception ExceptionGetData;

		public OleDataObjectPrivate(byte[] byteArray) {
			_byteArray=byteArray;
		}

		private static bool IsTheFormat(FORMATETC formatetc) {
			return formatetc.cfFormat==unchecked((short)(ushort)System.Windows.Forms.DataFormats.GetFormat(NodeTypeAndKeyClipboard.Format).Id)
				&& formatetc.dwAspect==DVASPECT.DVASPECT_CONTENT
				&& (formatetc.tymed & TYMED.TYMED_HGLOBAL)!=0;
		}

		public void GetData(ref FORMATETC format,out STGMEDIUM medium) {
			ListTymedsAsked.Add(format.tymed);
			if(ExceptionGetData!=null) {
				throw ExceptionGetData;
			}
			if(!IsTheFormat(format)) {
				throw new COMException("Not the format",DV_E_FORMATETC);
			}
			medium=new STGMEDIUM();
			medium.tymed=TYMED.TYMED_HGLOBAL;
			if(IsNullHandle) {
				return;
			}
			IntPtr handle=GlobalAlloc(GMEM_MOVEABLE,new UIntPtr((uint)_byteArray.Length));
			if(handle==IntPtr.Zero) {
				throw new OutOfMemoryException();
			}
			if(_byteArray.Length>0) {
				IntPtr pointer=GlobalLock(handle);
				Marshal.Copy(_byteArray,0,pointer,_byteArray.Length);
				GlobalUnlock(handle);
			}
			medium.unionmember=handle;
		}

		public void GetDataHere(ref FORMATETC format,ref STGMEDIUM medium) {
			throw new COMException("Not supported",DV_E_FORMATETC);
		}

		public int QueryGetData(ref FORMATETC format) {
			return IsTheFormat(format) ? 0 : DV_E_FORMATETC;
		}

		public int GetCanonicalFormatEtc(ref FORMATETC formatIn,out FORMATETC formatOut) {
			formatOut=formatIn;
			formatOut.ptd=IntPtr.Zero;
			return DV_E_FORMATETC;
		}

		public void SetData(ref FORMATETC formatIn,ref STGMEDIUM medium,bool release) {
			throw new NotSupportedException("The test's data object is read only.");
		}

		public IEnumFORMATETC EnumFormatEtc(DATADIR direction) {
			throw new NotSupportedException("Not used by the reads under test.");
		}

		public int DAdvise(ref FORMATETC pFormatetc,ADVF advf,IAdviseSink adviseSink,out int connection) {
			connection=0;
			return OLE_E_ADVISENOTSUPPORTED;
		}

		public void DUnadvise(int connection) {
			throw new COMException("Not supported",OLE_E_ADVISENOTSUPPORTED);
		}

		public int EnumDAdvise(out IEnumSTATDATA enumAdvise) {
			enumAdvise=null;
			return OLE_E_ADVISENOTSUPPORTED;
		}

		[DllImport("kernel32.dll",SetLastError=true)]
		private static extern IntPtr GlobalAlloc(uint uFlags,UIntPtr dwBytes);

		[DllImport("kernel32.dll",SetLastError=true)]
		private static extern IntPtr GlobalLock(IntPtr hMem);

		[DllImport("kernel32.dll",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GlobalUnlock(IntPtr hMem);
	}
}
