// IMG-P4d: video documents in the Imaging module: which files are videos, the trim rules, the times shown, the description of a frame saved as
// a photo, the tree's picture, and how the module shows a video (the player in place of the image). The module cannot be opened in a test
// run, so the wiring is read from the sources. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the
// repository root.
using System.Drawing;
using System.IO;
using ForkMigrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingTests {
	[TestClass]
	public class VideoMediaTests {
		private static string Read(params string[] arrayParts) {
			return File.ReadAllText(TestDb.FindRepoFile(Path.Combine(arrayParts)));
		}

		private static string MethodBody(string source,string signature) {
			int idxStart=source.IndexOf(signature);
			Assert.IsTrue(idxStart>=0,signature+" not found.");
			int idxEnd=source.IndexOf("\n\t\t}",idxStart);
			Assert.IsTrue(idxEnd>idxStart,signature+" has no closing brace.");
			return source.Substring(idxStart,idxEnd-idxStart);
		}

		[TestMethod]
		public void IsVideoPath_TheVideoFiles_AnyCase() {
			foreach(string fileName in new[] { "a.mp4","B.MP4","c.m4v","d.mov","e.wmv","f.avi"," g.mp4 " }) {
				Assert.IsTrue(VideoMedia.IsVideoPath(fileName),fileName);
			}
			foreach(string fileName in new[] { null,"","x.jpg","x.mp4.jpg","mp4","x.pdf" }) {
				Assert.IsFalse(VideoMedia.IsVideoPath(fileName),fileName??"null");
			}
		}

		[TestMethod]
		public void Trim_Rules() {
			Assert.IsTrue(VideoMedia.TryMakeTrim(5000,31000,42000,out int start,out int end,out string error));
			Assert.AreEqual(5000,start);
			Assert.AreEqual(31000,end);
			Assert.IsTrue(VideoMedia.TryMakeTrim(5000,42000,42000,out start,out end,out error));
			Assert.AreEqual(0,end,"An end at the end of the video is no trim on that side.");
			Assert.IsTrue(VideoMedia.TryMakeTrim(0,0,42000,out start,out end,out error),"Clear trim.");
			Assert.IsFalse(VideoMedia.TryMakeTrim(20000,10000,42000,out start,out end,out error),"The end before the start.");
			Assert.AreEqual("The start must be before the end.",error);
			Assert.IsFalse(VideoMedia.TryMakeTrim(20000,20100,42000,out start,out end,out error),"Less than 0.2 s left.");
			Assert.IsFalse(VideoMedia.TryMakeTrim(41900,0,42000,out start,out end,out error),"A start at the very end.");
			Assert.AreEqual(5000,VideoMedia.GetPlayStartMs(5000,42000));
			Assert.AreEqual(0,VideoMedia.GetPlayStartMs(50000,42000),"A start past the end (a shorter file) is ignored.");
			Assert.AreEqual(42000,VideoMedia.GetPlayEndMs(0,42000));
			Assert.AreEqual(31000,VideoMedia.GetPlayEndMs(31000,42000));
			Assert.AreEqual("0:05 to 0:31",VideoMedia.DescribeTrim(5000,31000,42000));
			Assert.AreEqual("",VideoMedia.DescribeTrim(0,0,42000));
		}

		[TestMethod]
		public void Times_AndTheFrameDescription() {
			Assert.AreEqual("0:00",VideoMedia.FormatTime(0));
			Assert.AreEqual("0:17",VideoMedia.FormatTime(17400));
			Assert.AreEqual("2:00",VideoMedia.FormatTime(120000));
			Assert.AreEqual("1:02:05",VideoMedia.FormatTime(3725000));
			Assert.AreEqual("From video #30 at 0:17",VideoMedia.GetFrameDescription(30,17000));
		}

		[TestMethod]
		public void Thumbnail_100Square_FrameFittedWithAPlayMark() {
			using(Bitmap bitmapFrame=new Bitmap(320,180)) {
				using(Graphics g=Graphics.FromImage(bitmapFrame)) {
					g.Clear(Color.Red);
				}
				using(Bitmap bitmap=VideoMedia.MakeThumbnail(bitmapFrame,42000)) {
					Assert.AreEqual(100,bitmap.Width);
					Assert.AreEqual(100,bitmap.Height);
					Assert.AreEqual(Color.Black.ToArgb(),bitmap.GetPixel(50,5).ToArgb(),"Letterboxed on black above a wide frame.");
					Assert.IsTrue(bitmap.GetPixel(10,50).R>200,"The frame fills the width.");
					Assert.IsTrue(bitmap.GetPixel(48,50).G>200,"The white play mark in the middle.");
				}
			}
		}

		[TestMethod]
		public void Module_ShowsAVideoInThePlayer_TreePictureAndDelete() {
			string display=Read("OpenPmsDental","User Controls","ControlImageDisplay.cs");
			StringAssert.Contains(display,"else if(VideoMedia.IsVideoPath(document.FileName)) {");
			StringAssert.Contains(MethodBody(display,"public void SelectTreeNode2("),"_controlVideoPlayer.Stop();");
			string delete=MethodBody(display,"public void DeleteDocument(");
			Assert.IsTrue(delete.IndexOf("_controlVideoPlayer.StopAndRelease();")>delete.IndexOf("Delete image?"),"The player lets go after the checks and the question.");
			StringAssert.Contains(MethodBody(Read("OpenPmsDental","Main Modules","ControlImages.cs"),"public void ModuleUnselected("),"StopVideo();");
			string documents=Read("OpenPmsDentalBusiness","Data Interface","Documents.cs");
			StringAssert.Contains(MethodBody(documents,"public static Bitmap GetThumbnail("),"VideoMedia.LoadThumbnail(document,patFolder)");
			string player=Read("OpenPmsDental","User Controls","ControlVideoPlayer.cs");
			StringAssert.Contains(MethodBody(player,"private void SetTrim("),"ImageSignaturePermissions.IsAuthorizedToEdit(_document)");
			StringAssert.Contains(MethodBody(player,"private void SetTrim("),"DocumentAdjusts.Upsert(");
			StringAssert.Contains(MethodBody(player,"private async void SaveFrame("),"Security.IsAuthorized(EnumPermType.ImageCreate)");
			StringAssert.Contains(MethodBody(player,"private async void SaveFrame("),"VideoFrameReader.ReadFrame(");
			StringAssert.Contains(player,"Settings.AreDefaultContextMenusEnabled=false;","Edge's menu could save the patient's file outside Export.");
			StringAssert.Contains(player,"video::-webkit-media-controls{display:none !important}","Edge's own controls (and their download) are hidden.");
			string session=Read("OpenPmsDental","User Controls","ControlCameraSession.cs");
			StringAssert.Contains(MethodBody(session,"private void StopRecording("),"VideoMedia.SaveThumbnail(");
		}
	}
}
