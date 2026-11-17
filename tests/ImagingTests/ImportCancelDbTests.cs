// S3a-1c of the .NET 10 plan (docs/port-notes/S3a-1.md, section 9): ImageStore.Import under a cancel of the progress window it runs in. The
// cancel comes right after the document row's INSERT (the Query Monitor of the test's thread sees the command end), the moment that S3a-1b left
// open: Documents.Insert is an insert, a read and an update outside the import's try, so the refused read left a row without its file. Now the
// row with its file name is one unit; the import goes on to its security log entry, which the database layer refuses, and the catch deletes the
// row. The rethrow keeps the stack trace of where the cancel was refused ("throw;", no longer "throw ex;"). Against the A-IMG scratch database
// of this run (ImagingDb; no other database is used), with a temp folder as the A-to-Z folder. Synthetic data only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using CodeBase;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.FileIO;

namespace ImagingTests {
	[TestClass]
	public class ImportCancelDbTests {
		private readonly List<string> _listPathsToDelete=new List<string>();

		[TestInitialize]
		public void TestInitialize() {
			ImagingDb.Begin();
			ImagingDb.RefreshCaches();
			Security.CurUser=new Userod { UserNum=1,UserName="s3a1ctest" };
		}

		[TestCleanup]
		public void TestCleanup() {
			QueryMonitor.Monitor=null;
			if(ImagingDb.SkipReason==null) {
				ImagingDb.SetStorage(DataStorageType.InDatabase);//also clears the A-to-Z override path
			}
			Security.CurUser=new Userod();
			foreach(string path in _listPathsToDelete) {
				try {
					if(Directory.Exists(path)) {
						Directory.Delete(path,recursive:true);
					}
					else if(File.Exists(path)) {
						File.Delete(path);
					}
				}
				catch(Exception) {
					//Temp files only.
				}
			}
		}

		#region Helpers
		///<summary>Uses a new, empty temp folder as this computer's A-to-Z folder. Returns it.</summary>
		private string UseAtoZ() {
			string atoZ=Path.Combine(Path.GetTempPath(),"s3a1c_atoz_"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(atoZ);
			_listPathsToDelete.Add(atoZ);
			ImagingDb.SetStorage(DataStorageType.LocalAtoZ,atoZ);
			return atoZ;
		}

		private static byte[] MakeJpgBytes() {
			using(Bitmap bitmap=new Bitmap(40,30))
			using(MemoryStream memoryStream=new MemoryStream()) {
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.White);
					g.FillRectangle(Brushes.Black,5,5,10,10);
				}
				bitmap.Save(memoryStream,ImageFormat.Jpeg);
				return memoryStream.ToArray();
			}
		}

		private string MakeJpgFile() {
			string path=Path.Combine(Path.GetTempPath(),"s3a1c_"+Guid.NewGuid().ToString("N")+".jpg");
			_listPathsToDelete.Add(path);
			File.WriteAllBytes(path,MakeJpgBytes());
			return path;
		}

		///<summary>Runs action with the token of a progress window on this thread (as FrmProgressAuto sets it on its worker), and cancels it the moment
		///this thread's first command starting with commandStart has finished. Returns what action threw, or null. isCancelled tells whether the
		///cancel came.</summary>
		private static Exception RunCancelledAfterCommand(string commandStart,Action action,out bool isCancelled) {
			isCancelled=false;
			bool isCancelledLocal=false;
			using CancellationTokenSource cancellationTokenSource=new CancellationTokenSource();
			QueryMonitor queryMonitor=new QueryMonitor();
			queryMonitor.IsMonitoring=true;
			queryMonitor.Fired+=(ODEventArgs e) => {
				//Fired when a command starts and again, with DateTimeStop set, when it has ended.
				if(isCancelledLocal || !(e.Tag is DbQueryObj dbQueryObj) || dbQueryObj.IsPayload || dbQueryObj.DateTimeStop==DateTime.MinValue) {
					return;
				}
				if(dbQueryObj.Command.TrimStart().StartsWith(commandStart,StringComparison.OrdinalIgnoreCase)) {
					isCancelledLocal=true;
					cancellationTokenSource.Cancel();
				}
			};
			QueryMonitor.Monitor=queryMonitor;
			try {
				using(ODCancellation.Use(cancellationTokenSource.Token)) {
					action();
				}
				return null;
			}
			catch(Exception ex) {
				return ex;
			}
			finally {
				QueryMonitor.Monitor=null;
				isCancelled=isCancelledLocal;
			}
		}

		private static void AssertNoRowOrEntryLeft(Patient patient,Exception ex,bool isCancelled) {
			Assert.IsTrue(isCancelled,"The cancel came after the document row's insert.");
			Assert.IsInstanceOfType(ex,typeof(OperationCanceledException),"The import stopped at the cancel: "+ex);
			StringAssert.Contains(ex.StackTrace,"ThrowIfCancellationRequested",
				"The rethrow keeps the stack trace of where the cancel was refused: "+ex.StackTrace);
			Assert.AreEqual(0,Documents.GetAllWithPat(patient.PatNum).Length,"No document row is left: the insert was a unit and the catch deleted the row.");
			Assert.AreEqual(0,ImagingDb.GetLogs(EnumPermType.ImageCreate,patient.PatNum).Count,"No log entry after the cancel.");
		}
		#endregion Helpers

		///<summary>Import from a file into the A-to-Z folder: the row is deleted, no log entry is written; the copied file stays in the patient's
		///folder without a row (as the port note says; "add missing files" picks it up, as after the old abort).</summary>
		[TestMethod]
		public void Import_FileToAtoZ_CancelRightAfterTheRowInsert_NoRowLeft_FileStays_StackTraceKept() {
			string atoZ=UseAtoZ();
			Patient patient=ImagingDb.CreatePatient("s3a1cfile");
			string pathJpg=MakeJpgFile();
			long docCategory=ImagingDb.GetImageCategory();
			Exception ex=RunCancelledAfterCommand("INSERT INTO document",() => ImageStore.Import(pathJpg,docCategory,patient),out bool isCancelled);
			AssertNoRowOrEntryLeft(patient,ex,isCancelled);
			string patFolder=ImageStore.GetPatientFolder(patient,atoZ);
			Assert.AreEqual(1,Directory.GetFiles(patFolder).Length,"The file copied before the refused log entry stays in the patient's folder.");
		}

		///<summary>Import of bytes with images in the database: nothing at all is left.</summary>
		[TestMethod]
		public void Import_BytesInDatabase_CancelRightAfterTheRowInsert_NothingLeft() {
			ImagingDb.SetStorage(DataStorageType.InDatabase);
			Patient patient=ImagingDb.CreatePatient("s3a1cbytes");
			byte[] arrayBytes=MakeJpgBytes();
			long docCategory=ImagingDb.GetImageCategory();
			Exception ex=RunCancelledAfterCommand("INSERT INTO document",
				() => ImageStore.Import(arrayBytes,docCategory,ImageType.Photo,patient),out bool isCancelled);
			AssertNoRowOrEntryLeft(patient,ex,isCancelled);
		}

		///<summary>Without a cancel the import is as before: one row with its file name and one log entry.</summary>
		[TestMethod]
		public void Import_BytesInDatabase_NoCancel_RowAndLogEntry() {
			ImagingDb.SetStorage(DataStorageType.InDatabase);
			Patient patient=ImagingDb.CreatePatient("s3a1cnocancel");
			Document document=ImageStore.Import(MakeJpgBytes(),ImagingDb.GetImageCategory(),ImageType.Photo,patient);
			Assert.AreEqual(1,Documents.GetAllWithPat(patient.PatNum).Length);
			Assert.AreNotEqual(".jpg",document.FileName,"The row has its file name.");
			Assert.AreEqual(1,ImagingDb.GetLogs(EnumPermType.ImageCreate,patient.PatNum).Count);
		}
	}
}
