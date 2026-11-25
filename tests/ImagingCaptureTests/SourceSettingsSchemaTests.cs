// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Linq;
using ImagingCapture.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingCaptureTests {
	///<summary>IMG-SRCSET: the driver's description of its settings is untrusted input; Parse takes it only within its limits.</summary>
	[TestClass]
	public class SourceSettingsSchemaTests {
		private const string Head="{\"schema\":\"source-settings-schema/1\",\"source\":\"Example\",\"version\":\"1.4\",\"settings\":[";

		private static string Doc(params string[] arraySettings) {
			return Head+string.Join(",",arraySettings)+"]}";
		}

		private static void AssertRefused(string json,string fragment) {
			string error;
			Assert.IsNull(SourceSettingsSchema.Parse(json,out error),json.Length>300 ? json.Substring(0,300) : json);
			StringAssert.Contains(error,fragment);
		}

		[TestMethod]
		public void Parse_AValidDescription_EveryType() {
			string error;
			SourceSettingsSchema schema=SourceSettingsSchema.Parse(Doc(
				"{\"key\":\"Sensor.XRayMa\",\"type\":\"choice\",\"values\":[\"0.2\",\"1\"],\"default\":\"\",\"help\":\"Tube current.\"}",
				"{\"key\":\"Capture.Timeout\",\"type\":\"integer\",\"min\":10,\"max\":600,\"default\":\"180\"}",
				"{\"key\":\"Adjust.Gamma\",\"type\":\"number\",\"min\":0.1,\"max\":5,\"changesImage\":true}",
				"{\"key\":\"Sensor.UsbDevice\",\"type\":\"text\"}"),out error);
			Assert.IsNotNull(schema,error);
			Assert.AreEqual("Example",schema.Source);
			CollectionAssert.AreEqual(new[] { "Sensor.XRayMa","Capture.Timeout","Adjust.Gamma","Sensor.UsbDevice" },schema.ListEntries.Select(x => x.Key).ToArray());
			CollectionAssert.AreEqual(new[] { "0.2","1" },schema.ListEntries[0].ListValues);
			Assert.AreEqual(10d,schema.ListEntries[1].Min);
			Assert.AreEqual(600d,schema.ListEntries[1].Max);
			Assert.IsTrue(schema.ListEntries[2].ChangesImage);
			Assert.IsFalse(schema.ListEntries[3].ChangesImage);
		}

		[TestMethod]
		public void Parse_Malformed_Refused() {
			AssertRefused("","is empty");
			AssertRefused("{\"schema\":\"source-settings-schema/1\",\"settings\":[","not a JSON object");
			AssertRefused("[1,2]","not a JSON object");
			AssertRefused("{\"schema\":\"source-settings-schema/2\",\"settings\":[{\"key\":\"A.B\",\"type\":\"text\"}]}","is not source-settings-schema/1");
			AssertRefused(Doc(),"no list of 1 to");
			AssertRefused("{\"schema\":\"source-settings-schema/1\",\"settings\":{}}","no list of 1 to");
			AssertRefused(Doc("\"Sensor.X\""),"not an object");
			AssertRefused(Doc("{\"key\":\"Sensor.X\",\"type\":\"blob\"}"),"type this program does not know");
			AssertRefused(Doc("{\"key\":\"Sensor.X\"}"),"type this program does not know");
			AssertRefused(Doc("{\"key\":\"Sensor.X\",\"type\":\"choice\"}"),"is a choice without");
			AssertRefused(Doc("{\"key\":\"Sensor.X\",\"type\":\"choice\",\"values\":[1]}"),"has a value that");
			AssertRefused(Doc("{\"key\":\"Sensor.X\",\"type\":\"integer\",\"min\":5,\"max\":1}"),"min above its max");
			AssertRefused(Doc("{\"key\":\"Sensor.X\",\"type\":\"text\"}","{\"key\":\"sensor.x\",\"type\":\"text\"}"),"lists sensor.x twice");
		}

		[TestMethod]
		public void Parse_Oversized_Refused() {
			AssertRefused(Doc("{\"key\":\"Sensor.X\",\"type\":\"text\",\"help\":\""+new string('a',SourceSettingsSchema.JsonLengthMax)+"\"}"),"is longer than");
			AssertRefused(Doc(Enumerable.Range(0,SourceSettingsSchema.SettingsMax+1).Select(i => "{\"key\":\"Sensor.X"+i+"\",\"type\":\"text\"}").ToArray()),
				"no list of 1 to");
			AssertRefused(Doc("{\"key\":\"Sensor.X\",\"type\":\"choice\",\"values\":["
				+string.Join(",",Enumerable.Range(0,SourceSettingsSchema.ValuesMax+1).Select(i => "\""+i+"\""))+"]}"),"is a choice without");
			AssertRefused(Doc("{\"key\":\"Sensor.X\",\"type\":\"text\",\"help\":\""+new string('a',SourceSettingsSchema.HelpLengthMax+1)+"\"}"),"help text longer");
			AssertRefused(Doc("{\"key\":\"Sensor.X\",\"type\":\"choice\",\"values\":[\""+new string('1',SourceSettingsSchema.ValueLengthMax+1)+"\"]}"),
				"has a value that");
			AssertRefused(Doc("{\"key\":\"Sensor."+new string('X',33)+"\",\"type\":\"text\"}"),"its key is not Section.Key");
		}

		[TestMethod]
		public void Parse_UnsafeKeysValuesAndTexts_Refused() {
			foreach(string key in new[] { "Sensor.X=Y","[Sensor].X","Sensor.X]","Sensor.X\\nY","Sensor.X\\r","Sensor.X;Y","Sensor X.Y","Sensor","Sensor.X.Y",
				".X","Sensor.","1Sensor.X","Sensor.\\u00e9","Source.Sensor.X\\u0000","" })
			{
				AssertRefused(Doc("{\"key\":\""+key+"\",\"type\":\"text\"}"),"its key is not Section.Key");
			}
			foreach(string value in new[] { "a;b","a=b","[a]","a\\nb","\\u0000","a\\\\b" }) {
				AssertRefused(Doc("{\"key\":\"Sensor.X\",\"type\":\"choice\",\"values\":[\""+value+"\"]}"),"has a value that");
				AssertRefused(Doc("{\"key\":\"Sensor.X\",\"type\":\"text\",\"default\":\""+value+"\"}"),"has a default with");
			}
			AssertRefused(Doc("{\"key\":\"Sensor.X\",\"type\":\"text\",\"help\":\"line\\nbreak\"}"),"control characters");
		}
	}
}
