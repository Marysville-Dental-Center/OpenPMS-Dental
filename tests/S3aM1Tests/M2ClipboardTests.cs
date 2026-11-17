// Package S3a-M2: the Images module's copy puts NodeTypeAndKey on the clipboard as a JSON string under the program's own format
// (NodeTypeAndKeyClipboard, ControlImageDisplay.Clipboard.cs) instead of the object, which the clipboard stored with the BinaryFormatter, and
// the paste reads only that format. The round trips use private data objects in this process and never the system clipboard: a WPF
// DataObject as the copy makes it, read back directly and through its OLE interface by a WinForms DataObject, which is how the paste's
// Clipboard.GetDataObject sees a copy (the data goes through a memory block as on the clipboard).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;

namespace S3aM1Tests {
	[TestClass]
	public class M2ClipboardTests {
		private const string DB="opendental_example";

		[TestMethod]
		public void Json_RoundTrip_DocumentAndMount() {
			foreach(NodeTypeAndKey nodeTypeAndKey in new[] { new NodeTypeAndKey(EnumImageNodeType.Document,123),new NodeTypeAndKey(EnumImageNodeType.Mount,long.MaxValue) }) {
				string json=NodeTypeAndKeyClipboard.ToJson(nodeTypeAndKey,DB);
				NodeTypeAndKey nodeTypeAndKeyRead=NodeTypeAndKeyClipboard.FromJson(json,out string dbNameOrUri);
				Assert.IsTrue(nodeTypeAndKey.IsMatching(nodeTypeAndKeyRead),json);
				Assert.AreEqual(DB,dbNameOrUri);
			}
			Assert.AreEqual("{\"NodeType\":\"Document\",\"PriKey\":123,\"DbNameOrUri\":\"https://mt.example.test/ServiceMain.asmx\"}",
				NodeTypeAndKeyClipboard.ToJson(new NodeTypeAndKey(EnumImageNodeType.Document,123),"https://mt.example.test/ServiceMain.asmx"));
			Assert.AreEqual("{\"NodeType\":\"Mount\",\"PriKey\":5,\"DbNameOrUri\":\"\"}",NodeTypeAndKeyClipboard.ToJson(new NodeTypeAndKey(EnumImageNodeType.Mount,5),null));
		}

		[TestMethod]
		public void Json_Malformed_GivesNull() {
			string[] arrayJson=new[] {
				null,"","   ","not json","[1,2]","\"Document\"","{}",
				"{\"NodeType\":\"Document\",\"PriKey\":0,\"DbNameOrUri\":\"x\"}",
				"{\"NodeType\":\"Document\",\"PriKey\":-4,\"DbNameOrUri\":\"x\"}",
				"{\"NodeType\":\"Document\",\"PriKey\":99999999999999999999999,\"DbNameOrUri\":\"x\"}",
				"{\"NodeType\":\"Document\",\"PriKey\":\"12\",\"DbNameOrUri\":\"x\"}",
				"{\"NodeType\":\"Document\",\"PriKey\":1.5,\"DbNameOrUri\":\"x\"}",
				"{\"NodeType\":2,\"PriKey\":12,\"DbNameOrUri\":\"x\"}",
				"{\"NodeType\":\"2\",\"PriKey\":12,\"DbNameOrUri\":\"x\"}",
				"{\"NodeType\":\"document\",\"PriKey\":12,\"DbNameOrUri\":\"x\"}",
				"{\"NodeType\":\"Category\",\"PriKey\":12,\"DbNameOrUri\":\"x\"}",
				"{\"NodeType\":\"None\",\"PriKey\":12,\"DbNameOrUri\":\"x\"}",
				"{\"NodeType\":\"Document\",\"PriKey\":12}",
				"{\"NodeType\":\"Document\",\"PriKey\":12,\"DbNameOrUri\":7}",
				"{\"NodeType\":\"Document\",\"PriKey\":12,\"DbNameOrUri\":\""+new string('a',5000)+"\"}",
			};
			foreach(string json in arrayJson) {
				Assert.IsNull(NodeTypeAndKeyClipboard.FromJson(json,out string dbNameOrUri),json==null ? "null" : json.Substring(0,Math.Min(80,json.Length)));
				Assert.AreEqual("",dbNameOrUri);
			}
		}

		[TestMethod]
		public void WpfDataObject_RoundTrip_AsTheCopyMakesIt() {
			M2Support.RunSta(() => {
				System.Windows.DataObject dataObject=new System.Windows.DataObject();
				NodeTypeAndKeyClipboard.SetData(dataObject,new NodeTypeAndKey(EnumImageNodeType.Mount,77),DB);
				CollectionAssert.AreEqual(new[] { NodeTypeAndKeyClipboard.Format },dataObject.GetFormats(false),"One format, the program's own.");
				Assert.IsInstanceOfType(dataObject.GetData(NodeTypeAndKeyClipboard.Format,false),typeof(MemoryStream),"A stream: no serializer.");
				for(int i=0;i<2;i++) {//read twice: a paste reads the same data object again
					NodeTypeAndKey nodeTypeAndKey=NodeTypeAndKeyClipboard.GetData((System.Windows.IDataObject)dataObject,out string dbNameOrUri);
					Assert.IsNotNull(nodeTypeAndKey,"Read "+i+".");
					Assert.AreEqual(EnumImageNodeType.Mount,nodeTypeAndKey.NodeType);
					Assert.AreEqual(77,nodeTypeAndKey.PriKey);
					Assert.AreEqual(DB,dbNameOrUri);
				}
			});
		}

		[TestMethod]
		public void WinFormsDataObject_OverOle_ReadsTheCopy() {
			M2Support.RunSta(() => {
				//The copy's WPF data object, with the bitmap's and file names' formats left out (the test is about NodeTypeAndKey).
				System.Windows.DataObject dataObjectWpf=new System.Windows.DataObject();
				NodeTypeAndKeyClipboard.SetData(dataObjectWpf,new NodeTypeAndKey(EnumImageNodeType.Document,4567),"opendental");
				//WinForms reads it through its OLE interface, as the paste's Clipboard.GetDataObject does: the stream is copied into a memory block
				//(rounded up, zero filled) and read back as a stream, so nothing is deserialized.
				System.Windows.Forms.DataObject dataObjectForms=new System.Windows.Forms.DataObject(
					(System.Runtime.InteropServices.ComTypes.IDataObject)dataObjectWpf);
				Assert.IsTrue(dataObjectForms.GetDataPresent(NodeTypeAndKeyClipboard.Format,false));
				for(int i=0;i<2;i++) {
					NodeTypeAndKey nodeTypeAndKey=NodeTypeAndKeyClipboard.GetData(dataObjectForms,out string dbNameOrUri);
					Assert.IsNotNull(nodeTypeAndKey,"Read "+i+".");
					Assert.AreEqual(EnumImageNodeType.Document,nodeTypeAndKey.NodeType);
					Assert.AreEqual(4567,nodeTypeAndKey.PriKey);
					Assert.AreEqual("opendental",dbNameOrUri);
				}
			});
		}

		[TestMethod]
		public void OtherFormatsAndObjects_AreNotRead() {
			M2Support.RunSta(() => {
				//An earlier version's copy: the NodeTypeAndKey object under its own type's format, and the database name as a string.
				System.Windows.Forms.DataObject dataObjectOld=new System.Windows.Forms.DataObject();
				dataObjectOld.SetData(new NodeTypeAndKey(EnumImageNodeType.Document,12));
				dataObjectOld.SetData("stringDbNameOrUri",DB);
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData(dataObjectOld,out string dbNameOrUriOld));
				Assert.AreEqual("",dbNameOrUriOld);
				//An object under the program's format is not a stream or a string: not read either.
				System.Windows.Forms.DataObject dataObjectObject=new System.Windows.Forms.DataObject();
				dataObjectObject.SetData(NodeTypeAndKeyClipboard.Format,new NodeTypeAndKey(EnumImageNodeType.Document,12));
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData(dataObjectObject,out string _));
				//Too long a stream, and bytes that are not the JSON.
				System.Windows.Forms.DataObject dataObjectLong=new System.Windows.Forms.DataObject();
				dataObjectLong.SetData(NodeTypeAndKeyClipboard.Format,new MemoryStream(new byte[5000]));
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData(dataObjectLong,out string _));
				System.Windows.Forms.DataObject dataObjectBytes=new System.Windows.Forms.DataObject();
				dataObjectBytes.SetData(NodeTypeAndKeyClipboard.Format,new MemoryStream(Encoding.UTF8.GetBytes("{\"NodeType\":\"Category\",\"PriKey\":3,\"DbNameOrUri\":\"x\"}")));
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData(dataObjectBytes,out string _));
				//Zeros after the JSON, as in a clipboard memory block, are allowed.
				System.Windows.Forms.DataObject dataObjectZeros=new System.Windows.Forms.DataObject();
				byte[] byteArrayJson=Encoding.UTF8.GetBytes(NodeTypeAndKeyClipboard.ToJson(new NodeTypeAndKey(EnumImageNodeType.Document,9),DB));
				dataObjectZeros.SetData(NodeTypeAndKeyClipboard.Format,new MemoryStream(byteArrayJson.Concat(new byte[16]).ToArray()));
				Assert.AreEqual(9,NodeTypeAndKeyClipboard.GetData(dataObjectZeros,out string dbNameOrUriZeros).PriKey);
				Assert.AreEqual(DB,dbNameOrUriZeros);
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData((System.Windows.Forms.IDataObject)null,out string _));
				Assert.IsNull(NodeTypeAndKeyClipboard.GetData((System.Windows.Forms.IDataObject)new System.Windows.Forms.DataObject("text"),out string _));
			});
		}

		[TestMethod]
		public void ImageDisplay_CopyAndPaste_UseTheFormat() {
			string source=M2Support.ReadRepoFile("OpenPmsDental","User Controls","ControlImageDisplay.cs");
			StringAssert.Contains(source,"NodeTypeAndKeyClipboard.SetData(dataObject,nodeTypeAndKey,dbNameOrUri);");
			StringAssert.Contains(source,"nodeTypeAndKey=NodeTypeAndKeyClipboard.GetData(iDataObject,out dbNameOrUriCopied);");
			Assert.IsFalse(source.Contains("typeof(NodeTypeAndKey)"),"The paste no longer asks for the object.");
			Assert.IsFalse(source.Contains("dataObject.SetData(nodeTypeAndKey)"),"The copy no longer puts the object on the clipboard.");
			Assert.IsFalse(source.Contains("\"stringDbNameOrUri\""),"The database name is in the JSON.");
		}
	}
}
