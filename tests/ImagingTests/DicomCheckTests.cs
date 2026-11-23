// IMG dicom-fix: one DICOM test for the whole program (ImageHelper.IsDicomPath: .dcm and .dicom in any case), and the windowing of 8-bit data
// stored in 16-bit words (DicomHelper.GetRawPerWindowingStep from BitsStored). Licensed under the GNU General Public License, version 2 or
// later. See LICENSE in the repository root.
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ForkMigrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingTests {
	[TestClass]
	public class DicomCheckTests {
		[TestMethod]
		public void IsDicomPath_DcmAndDicom_InAnyCase() {
			foreach(string fileName in new[] { "X.DCM","x.dcm","scan.Dcm","pano.dicom","PANO.DICOM",@"C:\images\a b\X.DCM"," y.dcm " }) {
				Assert.IsTrue(ImageHelper.IsDicomPath(fileName),fileName);
				Assert.IsTrue(ImageHelper.IsDicomFileName(fileName),"ImageHelper.IsDicomFileName: "+fileName);
				Assert.IsTrue(ClaimAttachDicom.IsDicomFileName(fileName),"ClaimAttachDicom.IsDicomFileName: "+fileName);
			}
			foreach(string fileName in new[] { null,""," ","x.jpg","dcm","x.dcm.jpg","x.dcmx","X.DIC" }) {
				Assert.IsFalse(ImageHelper.IsDicomPath(fileName),fileName??"null");
				Assert.IsFalse(ClaimAttachDicom.IsDicomFileName(fileName),"claims: "+(fileName??"null"));
			}
		}

		[TestMethod]
		public void NoCaseSensitiveDcmTestLeft_InTheProgram() {
			string root=Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TestDb.FindRepoFile(Path.Combine("OpenPmsDentalBusiness","Imaging","ImageHelper.cs")))));
			Regex regex=new Regex(@"(FileName|fileName)(\.ToLower\(\))?\.EndsWith\(""\.dcm""\)|GetExtension\([^)]*\)\.ToLower\(\)==""\.dcm""");
			List<string> listOffenders=new List<string>();
			foreach(string folder in new[] { "OpenPmsDental","OpenPmsDentalBusiness","WpfControlsOD" }) {
				foreach(string path in Directory.EnumerateFiles(Path.Combine(root,folder),"*.cs",SearchOption.AllDirectories)) {
					if(path.Contains(Path.DirectorySeparatorChar+"bin"+Path.DirectorySeparatorChar) || path.Contains(Path.DirectorySeparatorChar+"obj"+Path.DirectorySeparatorChar)) {
						continue;
					}
					if(regex.IsMatch(File.ReadAllText(path))) {
						listOffenders.Add(path.Substring(root.Length));
					}
				}
			}
			Assert.AreEqual(0,listOffenders.Count,"Use ImageHelper.IsDicomPath: "+string.Join(", ",listOffenders));
		}

		[TestMethod]
		public void Windowing_EightBitStoredInSixteenBitWords_IsNotDividedBy16() {
			BitmapDicom bitmapDicom=new BitmapDicom { Width=3,Height=1,BitsAllocated=16,BitsStored=8,PhotometricInterp=EnumDicomPhotometricInterp.MONOCHROME2,
				ArrayDataRaw=new ushort[] { 0,128,255 } };
			Assert.AreEqual(1f,DicomHelper.GetRawPerWindowingStep(bitmapDicom));
			using Bitmap bitmap=DicomHelper.ApplyWindowing(bitmapDicom,0,0);
			Assert.AreEqual(255,bitmap.GetPixel(2,0).R,"White is white (it was 15, nearly black).");
			Assert.IsTrue(bitmap.GetPixel(1,0).R>=126 && bitmap.GetPixel(1,0).R<=129,"Mid grey: "+bitmap.GetPixel(1,0).R);
			Assert.AreEqual(0,bitmap.GetPixel(0,0).R);
		}

		[TestMethod]
		public void Windowing_OtherDepths_Unchanged() {
			Assert.AreEqual(1f,DicomHelper.GetRawPerWindowingStep(new BitmapDicom { BitsAllocated=8,BitsStored=8 }));
			Assert.AreEqual(16f,DicomHelper.GetRawPerWindowingStep(new BitmapDicom { BitsAllocated=16,BitsStored=12 }));
			Assert.AreEqual(16f,DicomHelper.GetRawPerWindowingStep(new BitmapDicom { BitsAllocated=16,BitsStored=10 }),"Other depths keep the factor they had.");
			Assert.AreEqual(256f,DicomHelper.GetRawPerWindowingStep(new BitmapDicom { BitsAllocated=16,BitsStored=16 }));
			Assert.AreEqual(16f,DicomHelper.GetRawPerWindowingStep(new BitmapDicom { BitsAllocated=16,BitsStored=0 }),"Unknown stored bits: as before.");
		}
	}
}
