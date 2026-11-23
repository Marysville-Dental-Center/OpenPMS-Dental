// IMG-P3b's review: the Enhance window's save (ImageEnhance.Save) checks the permission again, writes the window and the filters, keeps a
// video's trim and logs, also without the module's display. On the imaging test database. Licensed under the GNU General Public License,
// version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingTests {
	[TestClass]
	public class EnhanceSaveDbTests {
		[TestInitialize]
		public void TestInitialize() {
			ImagingDb.Begin();
			ImagingDb.SetStorage(DataStorageType.InDatabase);
		}

		[TestCleanup]
		public void TestCleanup() {
			Security.CurUser=new Userod();
		}

		[TestMethod]
		public void Save_WritesTheWindowAndTheFilters_KeepsTheTrim_Logs() {
			Patient patient=ImagingDb.CreatePatient("enh1");
			Document document=ImagingDb.CreateDocument(patient,"",DateTime.Now);
			DocumentAdjusts.Upsert(new DocumentAdjust { DocNum=document.DocNum,Gamma=1,TrimStartMs=1500,TrimEndMs=9000 });
			ImagingDb.LogInWith(EnumPermType.ImageEdit);
			Document documentOld=document.Copy();
			document.WindowingMin=25;
			document.WindowingMax=230;
			ImageAdjustSettings settingsNew=new ImageAdjustSettings { Gamma=1.2,Sharpen=40 };
			Assert.IsTrue(ImageEnhance.Save(document,documentOld,new ImageAdjustSettings(),settingsNew,patient.PatNum,patFolder:null));
			Document documentDb=Documents.GetByNum(document.DocNum);
			Assert.AreEqual(25,documentDb.WindowingMin);
			Assert.AreEqual(230,documentDb.WindowingMax);
			DocumentAdjust documentAdjust=DocumentAdjusts.GetForDoc(document.DocNum);
			Assert.AreEqual(1.2,documentAdjust.Gamma,0.0001);
			Assert.AreEqual(40,documentAdjust.Sharpen);
			Assert.AreEqual(1500,documentAdjust.TrimStartMs,"A video's trim stays.");
			Assert.AreEqual(9000,documentAdjust.TrimEndMs);
			List<SecurityLog> listLogs=ImagingDb.GetLogs(EnumPermType.ImageEdit,patient.PatNum);
			Assert.IsTrue(listLogs.Any(x => x.LogText.Contains("enhanced: window 0-0 to 25-230, filters 'none' to 'gamma 1.20, sharpen 40'")),
				string.Join(" | ",listLogs.Select(x => x.LogText)));
		}

		[TestMethod]
		public void Save_WithoutImageEdit_SavesNothing() {
			Patient patient=ImagingDb.CreatePatient("enh2");
			Document document=ImagingDb.CreateDocument(patient,"",DateTime.Now);
			ImagingDb.LogInWith(EnumPermType.ImageCreate);
			Document documentOld=document.Copy();
			document.WindowingMin=40;
			document.WindowingMax=200;
			Assert.IsFalse(ImageEnhance.Save(document,documentOld,null,new ImageAdjustSettings { IsInverted=true },patient.PatNum,patFolder:null));
			Assert.AreEqual(0,Documents.GetByNum(document.DocNum).WindowingMax,"The window is not written.");
			Assert.IsNull(DocumentAdjusts.GetForDoc(document.DocNum),"No filter row.");
		}

		[TestMethod]
		public void Save_ASignedImage_NeedsSignedImageEdit() {
			Patient patient=ImagingDb.CreatePatient("enh3");
			Document document=ImagingDb.CreateDocument(patient,"synthetic-signature",DateTime.Now);
			ImagingDb.LogInWith(EnumPermType.ImageEdit);
			Assert.IsFalse(ImageEnhance.Save(document,document.Copy(),null,new ImageAdjustSettings { IsInverted=true },patient.PatNum,patFolder:null),
				"Signed: Image Edit alone is not enough (the review's bypass after signing).");
			ImagingDb.LogInWith(EnumPermType.ImageEdit,EnumPermType.SignedImageEdit);
			Assert.IsTrue(ImageEnhance.Save(document,document.Copy(),null,new ImageAdjustSettings { IsInverted=true },patient.PatNum,patFolder:null));
		}
	}
}
