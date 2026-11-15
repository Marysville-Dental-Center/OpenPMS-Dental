// The clipboard data that lets a copied image or mount be pasted inside the program with its windowing, rotation and crop (package S3a-M2
// of the .NET 10 move, docs/port-notes/S3a-M2.md). It used to be the NodeTypeAndKey object itself, which the clipboard stores with the
// BinaryFormatter; the BinaryFormatter is gone on .NET 9 and later
// (https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-migration-guide/winforms-wpf-ole-guidance). Now it is a
// small JSON object under the program's own clipboard format, and the paste reads only that format.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Text;
using CodeBase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenPmsDental.UI;

namespace OpenPmsDental {
	///<summary>Puts a NodeTypeAndKey and the database it belongs to on a clipboard data object, and reads them back. The data is UTF-8 JSON,
	///{"NodeType":"Document","PriKey":123,"DbNameOrUri":"opendental"}, in a MemoryStream under Format: a stream goes to and from the clipboard
	///as its bytes (no serializer), in WPF and in WinForms, and in this process and another one. Nothing else is read: an object of another
	///format, an old copy (the NodeTypeAndKey object of an earlier version) or malformed data gives null, and the paste then uses the bitmap or
	///the file names, as it does for a copy from another program.</summary>
	public static class NodeTypeAndKeyClipboard {
		///<summary>The clipboard format of the data.</summary>
		public const string Format="OpenPmsDental.NodeTypeAndKey.Json";
		///<summary>Larger data is not read. The JSON of a copy is under 200 bytes plus the database name or Middle Tier address.</summary>
		private const int MAX_BYTES=4096;

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

		///<summary>The copy on a WinForms data object (the paste reads the clipboard this way), or null if it has none.</summary>
		public static NodeTypeAndKey GetData(System.Windows.Forms.IDataObject iDataObject,out string dbNameOrUri) {
			dbNameOrUri="";
			if(iDataObject==null || !iDataObject.GetDataPresent(Format,false)) {
				return null;
			}
			return FromData(iDataObject.GetData(Format,false),out dbNameOrUri);
		}

		///<summary>The copy on a WPF data object, or null if it has none.</summary>
		public static NodeTypeAndKey GetData(System.Windows.IDataObject iDataObject,out string dbNameOrUri) {
			dbNameOrUri="";
			if(iDataObject==null || !iDataObject.GetDataPresent(Format,false)) {
				return null;
			}
			return FromData(iDataObject.GetData(Format,false),out dbNameOrUri);
		}

		///<summary>The data of Format: the stream (from the clipboard, or the one SetData made), or a string.</summary>
		private static NodeTypeAndKey FromData(object data,out string dbNameOrUri) {
			dbNameOrUri="";
			string json;
			if(data is Stream stream) {
				if(!stream.CanRead || (stream.CanSeek && stream.Length>MAX_BYTES)) {
					return null;
				}
				if(stream.CanSeek) {
					stream.Position=0;
				}
				byte[] byteArray=new byte[MAX_BYTES+1];
				int count=0;
				int countRead;
				while(count<byteArray.Length && (countRead=stream.Read(byteArray,count,byteArray.Length-count))>0) {
					count+=countRead;
				}
				if(count>MAX_BYTES) {
					return null;
				}
				//A stream from the clipboard can be longer than the data: its memory block is rounded up and filled with zeros.
				json=Encoding.UTF8.GetString(byteArray,0,count).TrimEnd('\0');
			}
			else if(data is string str) {
				json=str;
			}
			else {
				return null;
			}
			return FromJson(json,out dbNameOrUri);
		}
	}
}
