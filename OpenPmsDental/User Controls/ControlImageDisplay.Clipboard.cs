// The clipboard data that lets a copied image or mount be pasted inside the program with its windowing, rotation and crop (package S3a-M2
// of the .NET 10 move, docs/port-notes/S3a-M2.md). It used to be the NodeTypeAndKey object itself, which the clipboard stores with the
// BinaryFormatter; the BinaryFormatter is gone on .NET 9 and later
// (https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-migration-guide/winforms-wpf-ole-guidance). Now it is a
// small JSON object under the program's own clipboard format, and the paste reads only that format.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using CodeBase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenPmsDental.UI;
using IComDataObject=System.Runtime.InteropServices.ComTypes.IDataObject;

namespace OpenPmsDental {
	///<summary>Puts a NodeTypeAndKey and the database it belongs to on a clipboard data object, and reads them back. The data is UTF-8 JSON,
	///{"NodeType":"Document","PriKey":123,"DbNameOrUri":"opendental"}, in a MemoryStream under Format: a stream goes to and from the clipboard
	///as its bytes (no serializer), in WPF and in WinForms, and in this process and another one. The paste reads those bytes through OLE
	///(ReadBytes), so no data under Format reaches a deserializer. Nothing else is read: an object of another
	///format, an old copy (the NodeTypeAndKey object of an earlier version) or malformed data gives null, and the paste then uses the bitmap or
	///the file names, as it does for a copy from another program.</summary>
	public static class NodeTypeAndKeyClipboard {
		///<summary>The clipboard format of the data.</summary>
		public const string Format="OpenPmsDental.NodeTypeAndKey.Json";
		///<summary>Larger data is not read. The JSON of a copy is under 200 bytes plus the database name or Middle Tier address.</summary>
		private const int MAX_BYTES=4096;
		///<summary>A memory block may be this much larger than the data it holds (it is then zero filled).</summary>
		private const int MAX_BYTES_ROUNDING=64;
		///<summary>The 16 bytes that start a memory block holding an object that the WinForms or WPF clipboard serialized with the
		///BinaryFormatter; their data objects deserialize such a block on GetData. A block under Format that starts with them is never read.</summary>
		public static readonly Guid GuidSerializedObject=new Guid("FD9EA796-3B13-4370-A679-56106BB288FB");

		///<summary>The JSON for the clipboard. dbNameOrUri is the database name, or the Middle Tier address, of the copy; null is written as "".</summary>
		public static string ToJson(NodeTypeAndKey nodeTypeAndKey,string dbNameOrUri) {
			JObject jObject=new JObject();
			jObject["NodeType"]=nodeTypeAndKey.NodeType.ToString();
			jObject["PriKey"]=nodeTypeAndKey.PriKey;
			jObject["DbNameOrUri"]=dbNameOrUri??"";
			return jObject.ToString(Formatting.None);
		}

		///<summary>Reads the JSON that ToJson writes. Returns null, and dbNameOrUri "", unless it is a JSON object with a NodeType of Document or
		///Mount (by name), a PriKey that is a whole number above 0 and a DbNameOrUri string.</summary>
		public static NodeTypeAndKey FromJson(string json,out string dbNameOrUri) {
			dbNameOrUri="";
			if(string.IsNullOrWhiteSpace(json) || json.Length>MAX_BYTES) {
				return null;
			}
			JObject jObject;
			try {
				JToken jToken=JToken.Parse(json);
				jObject=jToken as JObject;
			}
			catch(Exception ex) {//JsonReaderException for anything that is not JSON
				ex.DoNothing();
				return null;
			}
			if(jObject==null) {
				return null;
			}
			JToken jTokenNodeType=jObject["NodeType"];
			JToken jTokenPriKey=jObject["PriKey"];
			JToken jTokenDb=jObject["DbNameOrUri"];
			if(jTokenNodeType==null || jTokenNodeType.Type!=JTokenType.String
				|| jTokenPriKey==null || jTokenPriKey.Type!=JTokenType.Integer
				|| jTokenDb==null || jTokenDb.Type!=JTokenType.String)
			{
				return null;
			}
			string nodeTypeName=(string)jTokenNodeType;
			EnumImageNodeType enumImageNodeType;
			if(nodeTypeName==EnumImageNodeType.Document.ToString()) {
				enumImageNodeType=EnumImageNodeType.Document;
			}
			else if(nodeTypeName==EnumImageNodeType.Mount.ToString()) {
				enumImageNodeType=EnumImageNodeType.Mount;
			}
			else {
				return null;
			}
			long priKey;
			try {
				priKey=(long)jTokenPriKey;
			}
			catch(Exception ex) {//a whole number too large for a long
				ex.DoNothing();
				return null;
			}
			if(priKey<=0) {
				return null;
			}
			dbNameOrUri=(string)jTokenDb;
			return new NodeTypeAndKey(enumImageNodeType,priKey);
		}

		///<summary>Puts the copy on a WPF data object, as ControlImageDisplay's copy does before it puts the data object on the clipboard.</summary>
		public static void SetData(System.Windows.DataObject dataObject,NodeTypeAndKey nodeTypeAndKey,string dbNameOrUri) {
			dataObject.SetData(Format,new MemoryStream(Encoding.UTF8.GetBytes(ToJson(nodeTypeAndKey,dbNameOrUri))));
		}

		///<summary>The copy on a WinForms data object (the paste reads the clipboard this way), or null if it has none. The bytes are read
		///through the data object's OLE interface (ReadBytes), never through GetData(Format), which on .NET Framework passes a memory block that
		///starts with GuidSerializedObject to the BinaryFormatter (S3a-FU2).</summary>
		public static NodeTypeAndKey GetData(System.Windows.Forms.IDataObject iDataObject,out string dbNameOrUri) {
			dbNameOrUri="";
			if(iDataObject==null || !iDataObject.GetDataPresent(Format,false)) {
				return null;
			}
			return FromBytes(ReadBytes(iDataObject as IComDataObject),out dbNameOrUri);
		}

		///<summary>The copy on a WPF data object, or null if it has none. Read as the WinForms one is.</summary>
		public static NodeTypeAndKey GetData(System.Windows.IDataObject iDataObject,out string dbNameOrUri) {
			dbNameOrUri="";
			if(iDataObject==null || !iDataObject.GetDataPresent(Format,false)) {
				return null;
			}
			return FromBytes(ReadBytes(iDataObject as IComDataObject),out dbNameOrUri);
		}

		///<summary>The bytes of Format's memory block on an OLE data object, copied as they are: IDataObject.GetData with TYMED_HGLOBAL, and no
		///serializer. Null if the data object is null, has no memory block for Format, fails, or gives a block larger than MAX_BYTES plus
		///MAX_BYTES_ROUNDING. The WinForms and WPF data objects in this process are OLE data objects too; a stream on them is copied into the
		///block as its bytes.</summary>
		public static byte[] ReadBytes(IComDataObject iComDataObject) {
			if(iComDataObject==null) {
				return null;
			}
			FORMATETC formatetc=new FORMATETC();
			formatetc.cfFormat=unchecked((short)(ushort)System.Windows.Forms.DataFormats.GetFormat(Format).Id);
			formatetc.dwAspect=DVASPECT.DVASPECT_CONTENT;
			formatetc.lindex=-1;
			formatetc.ptd=IntPtr.Zero;
			formatetc.tymed=TYMED.TYMED_HGLOBAL;
			STGMEDIUM stgmedium;
			try {
				iComDataObject.GetData(ref formatetc,out stgmedium);
			}
			catch(Exception ex) {//COMException for a format or medium the data object does not have; anything else from another program's data object
				ex.DoNothing();
				return null;
			}
			try {
				if(stgmedium.tymed!=TYMED.TYMED_HGLOBAL || stgmedium.unionmember==IntPtr.Zero) {
					return null;
				}
				ulong size=GlobalSize(stgmedium.unionmember).ToUInt64();
				if(size==0 || size>MAX_BYTES+MAX_BYTES_ROUNDING) {
					return null;
				}
				IntPtr pointer=GlobalLock(stgmedium.unionmember);
				if(pointer==IntPtr.Zero) {
					return null;
				}
				try {
					byte[] byteArray=new byte[(int)size];
					Marshal.Copy(pointer,byteArray,0,byteArray.Length);
					return byteArray;
				}
				finally {
					GlobalUnlock(stgmedium.unionmember);
				}
			}
			finally {
				ReleaseStgMedium(ref stgmedium);
			}
		}

		///<summary>Reads the bytes of Format's memory block: UTF-8 JSON, maybe followed by zeros (a memory block can be rounded up and zero
		///filled). Null for null, for a block that starts with GuidSerializedObject (an object the clipboard serialized: it is never
		///deserialized), for more than MAX_BYTES before the zeros, and for anything FromJson does not read.</summary>
		public static NodeTypeAndKey FromBytes(byte[] byteArray,out string dbNameOrUri) {
			dbNameOrUri="";
			if(byteArray==null) {
				return null;
			}
			byte[] byteArrayGuid=GuidSerializedObject.ToByteArray();
			if(byteArray.Length>=byteArrayGuid.Length) {
				bool isSerializedObject=true;
				for(int i=0;i<byteArrayGuid.Length;i++) {
					if(byteArray[i]!=byteArrayGuid[i]) {
						isSerializedObject=false;
						break;
					}
				}
				if(isSerializedObject) {
					return null;
				}
			}
			int count=byteArray.Length;
			while(count>0 && byteArray[count-1]==0) {
				count--;
			}
			if(count>MAX_BYTES) {
				return null;
			}
			return FromJson(Encoding.UTF8.GetString(byteArray,0,count),out dbNameOrUri);
		}

		[DllImport("kernel32.dll",SetLastError=true)]
		private static extern IntPtr GlobalLock(IntPtr hMem);

		[DllImport("kernel32.dll",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GlobalUnlock(IntPtr hMem);

		[DllImport("kernel32.dll",SetLastError=true)]
		private static extern UIntPtr GlobalSize(IntPtr hMem);

		[DllImport("ole32.dll")]
		private static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);
	}
}
