// IMG-P3a: the saved adjust filters (documentadjust) are applied wherever the module turns an image into its picture after brightness/contrast,
// and in the shared export/print path. The windows cannot be opened in a test run, so these tests read the sources. Licensed under the GNU
// General Public License, version 2 or later. See LICENSE in the repository root.
using System.IO;
using System.Text.RegularExpressions;
using ForkMigrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingTests {
	[TestClass]
	public class AdjustWiringTests {
		private static string Read(params string[] arrayParts) {
			return File.ReadAllText(TestDb.FindRepoFile(Path.Combine(arrayParts)));
		}

		///<summary>The text of one method: from its signature to the first closing brace at the method's own indent (two tabs).</summary>
		private static string MethodBody(string source,string signature) {
			int idxStart=source.IndexOf(signature);
			Assert.IsTrue(idxStart>=0,signature+" not found.");
			int idxEnd=source.IndexOf("\n\t\t}",idxStart);
			Assert.IsTrue(idxEnd>idxStart,signature+" has no closing brace.");
			return source.Substring(idxStart,idxEnd-idxStart);
		}

		private static string Display() {
			return Read("OpenPmsDental","User Controls","ControlImageDisplay.cs");
		}

		[TestMethod]
		public void Display_AppliesTheFiltersAfterEveryBrightnessContrast() {
			string display=Display();
			string bodyLoad=MethodBody(display,"private void LoadBitmap(int idx,EnumLoadBitmapType loadBitmapType){");
			Assert.AreEqual(3,Regex.Matches(bodyLoad,Regex.Escape("ApplyAdjust(idx);")).Count,"A single image (DICOM and other) and a mount item.");
			string bodyInvalidate=MethodBody(display,"public void InvalidateSettingsColor() {");
			StringAssert.Contains(bodyInvalidate,"ApplyAdjust(0);","The brightness/contrast slider, a single image.");
			StringAssert.Contains(bodyInvalidate,"ApplyAdjust(_idxSelectedInMount);","The slider, a mount item.");
			StringAssert.Contains(display,"ImageAdjust.Apply(_bitmapArrayShowing[0],GetAdjustSettings(listDocuments[i]));","Print all in category.");
		}

		[TestMethod]
		public void Display_TheRawShortcut_OnlyWhenNoFilterChangedThePicture() {
			string bodyLoad=MethodBody(Display(),"private void LoadBitmap(int idx,EnumLoadBitmapType loadBitmapType){");
			StringAssert.Contains(bodyLoad,"&& IsAdjustNeutral(_documentArrayShowing[idx]))","Copying the picture as the raw image would filter it twice.");
		}

		[TestMethod]
		public void Display_ReadsEachDocumentsFiltersOnce_AMountInOneQuery() {
			string display=Display();
			string body=MethodBody(display,"public ImageAdjustSettings GetAdjustSettings(Document document){");
			StringAssert.Contains(body,"DocumentAdjusts.GetForDocs(listDocNums);");
			StringAssert.Contains(MethodBody(display,"public void ClearObjects(){"),"_dictAdjustSettings.Clear();","A new selection reads them again.");
		}

		[TestMethod]
		public void ExportAndPrint_ApplyTheFiltersToo() {
			string body=MethodBody(Read("OpenPmsDentalBusiness","Imaging","ImageHelper.cs"),"public static Bitmap ApplyDocumentSettingsToImage(");
			StringAssert.Contains(body,"ImageAdjust.Apply(bitmap,DocumentAdjusts.GetForDoc(document.DocNum)?.ToSettings());");
			Assert.IsTrue(body.IndexOf("ImageAdjust.Apply(")>body.IndexOf("ImageSettingFlags.COLORFUNCTION"),"After brightness/contrast.");
		}
	}
}
