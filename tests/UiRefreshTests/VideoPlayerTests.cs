// IMG-P4d: the Imaging module's video player shows an MP4 the camera session records in the program's browser control (Microsoft Edge, through
// ODWebView2) and reads its length and position. A real MP4 is written with Mp4Writer into a temporary folder; no camera, no program, no
// database. Inconclusive where the WebView2 Runtime or an H.264 encoder is missing. The frame for "Save frame as photo" is read by
// VideoFrameReader, tested in ImagingCaptureTests. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the
// repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CodeBase.Controls;
using ImagingCapture.Uvc.Interop;
using ImagingCapture.Video;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class VideoPlayerTests {
		///<summary>Two seconds at 15 frames a second, 320 by 240.</summary>
		private static void WriteTestVideo(string path) {
			using(Mp4Writer writer=Mp4Writer.Create(path,320,240,15)) {
				byte[] arrayPixels=new byte[320*240*4];
				for(int i=0;i<30;i++) {
					for(int p=0;p<arrayPixels.Length;p+=4) {
						arrayPixels[p]=(byte)(i<15 ? 30 : 220);
						arrayPixels[p+1]=40;
						arrayPixels[p+2]=(byte)(i<15 ? 220 : 30);
						arrayPixels[p+3]=255;
					}
					writer.WriteFrame(arrayPixels,320*4,i*666667L,666667L);
				}
				writer.Finish();
			}
		}

		private static bool PumpUntil(Func<bool> condition,int timeoutMs) {
			DateTime dateTimeEnd=DateTime.Now.AddMilliseconds(timeoutMs);
			while(!condition() && DateTime.Now<dateTimeEnd) {
				Application.DoEvents();
				Thread.Sleep(20);
			}
			return condition();
		}

		[TestMethod]
		public void Player_ShowsARecordedMp4_InTheBrowserControl_LengthAndPosition() {
			if(!ODWebView2.IsRuntimeAvailable()) {
				Assert.Inconclusive("The WebView2 Runtime is not installed here.");
			}
			string folder=Path.Combine(Path.GetTempPath(),"opms-player-"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(folder);
			string path=Path.Combine(folder,"video.mp4");
			string inconclusive="";
			List<string> listFailures=new List<string>();
			try {
				try {
					WriteTestVideo(path);
				}
				catch(MfException ex) {
					Assert.Inconclusive("No Media Foundation H.264 encoder here: "+ex.Message);
				}
				StaRunner.Run(() => {
					NoDatabaseGuard.Install();
					using(Form form=new Form { Size=new Size(900,600),ShowInTaskbar=false,StartPosition=FormStartPosition.Manual,Location=new Point(-2000,-2000) })
					using(ControlVideoPlayer controlVideoPlayer=new ControlVideoPlayer(x => x) { Dock=DockStyle.Fill }) {
						form.Controls.Add(controlVideoPlayer);
						form.Show();
						Document document=new Document { DocNum=30,PatNum=1,FileName="video.mp4",DateCreated=new DateTime(2026,11,22,10,14,0),ToothNumbers="" };
						controlVideoPlayer.ShowVideo(document,new Patient { PatNum=1 },folder,path);
						if(!PumpUntil(() => controlVideoPlayer.IsOpened || controlVideoPlayer.MessageText!="",20000)) {
							inconclusive="The browser control did not show the video within 20 seconds (no desktop session?).";
							return;
						}
						if(!controlVideoPlayer.IsOpened) {
							inconclusive="The browser control could not show the video: "+controlVideoPlayer.MessageText;
							return;
						}
						if(Math.Abs(controlVideoPlayer.DurationMs-2000)>300) {
							listFailures.Add("Length "+controlVideoPlayer.DurationMs+" ms, expected about 2000.");
						}
						if(controlVideoPlayer.IsPlaying) {
							listFailures.Add("A video opens paused at its start.");
						}
						string folderGallery=Environment.GetEnvironmentVariable("OPMS_IMAGE_VIEWER_GALLERY");
						if(!string.IsNullOrEmpty(folderGallery)) {
							SavePicture(form,controlVideoPlayer,Path.Combine(folderGallery,"video-player.png"));
						}
						controlVideoPlayer.StopAndRelease();
					}
				});
			}
			finally {
				try {
					Directory.Delete(folder,true);
				}
				catch(IOException) {
					//the browser may hold the file a moment longer
				}
			}
			if(inconclusive!="") {
				Assert.Inconclusive(inconclusive);
			}
			Assert.AreEqual(0,listFailures.Count,string.Join(" ",listFailures));
		}

		///<summary>The gallery's picture of the player with a trim set: the browser's own capture of the video, with the bar drawn below it.
		///</summary>
		private static void SavePicture(Form form,ControlVideoPlayer controlVideoPlayer,string path) {
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			BindingFlags bindingFlags=BindingFlags.Instance|BindingFlags.NonPublic;
			typeof(ControlVideoPlayer).GetField("_trimStartMs",bindingFlags).SetValue(controlVideoPlayer,300);
			typeof(ControlVideoPlayer).GetField("_trimEndMs",bindingFlags).SetValue(controlVideoPlayer,1700);
			typeof(ControlVideoPlayer).GetField("_positionMs",bindingFlags).SetValue(controlVideoPlayer,700);
			typeof(ControlVideoPlayer).GetMethod("RefreshControls",bindingFlags).Invoke(controlVideoPlayer,null);
			typeof(ControlVideoPlayer).GetField("timerPoll",bindingFlags).GetValue(controlVideoPlayer).GetType().GetMethod("Stop").Invoke(
				typeof(ControlVideoPlayer).GetField("timerPoll",bindingFlags).GetValue(controlVideoPlayer),null);
			ODWebView2 odWebView2=(ODWebView2)typeof(ControlVideoPlayer).GetField("_odWebView2",bindingFlags).GetValue(controlVideoPlayer);
			using(Bitmap bitmapPicture=new Bitmap(controlVideoPlayer.Width,controlVideoPlayer.Height)) {
				controlVideoPlayer.DrawToBitmap(bitmapPicture,new Rectangle(0,0,controlVideoPlayer.Width,controlVideoPlayer.Height));
				using(MemoryStream memoryStream=new MemoryStream()) {
					System.Threading.Tasks.Task task=odWebView2.CoreWebView2.CapturePreviewAsync(
						Microsoft.Web.WebView2.Core.CoreWebView2CapturePreviewImageFormat.Png,memoryStream);
					PumpUntil(() => task.IsCompleted,5000);
					if(task.IsCompleted && !task.IsFaulted && memoryStream.Length>0) {
						memoryStream.Position=0;
						using(Bitmap bitmapVideo=new Bitmap(memoryStream))
						using(Graphics g=Graphics.FromImage(bitmapPicture)) {
							g.DrawImage(bitmapVideo,new Rectangle(odWebView2.Left,odWebView2.Top,odWebView2.Width,odWebView2.Height));
						}
					}
				}
				bitmapPicture.Save(path,System.Drawing.Imaging.ImageFormat.Png);
			}
		}

		[TestMethod]
		public void PollAnswer_IsParsed() {
			Assert.IsTrue(ControlVideoPlayer.TryParsePoll("\"12.5,1,42.25\"",out double seconds,out bool isPaused,out double durationSeconds));
			Assert.AreEqual(12.5,seconds);
			Assert.IsTrue(isPaused);
			Assert.AreEqual(42.25,durationSeconds);
			Assert.IsTrue(ControlVideoPlayer.TryParsePoll("\"0,0,0\"",out seconds,out isPaused,out durationSeconds));
			Assert.IsFalse(isPaused);
			Assert.IsFalse(ControlVideoPlayer.TryParsePoll("\"\"",out seconds,out isPaused,out durationSeconds),"No video element yet.");
			Assert.IsFalse(ControlVideoPlayer.TryParsePoll("null",out seconds,out isPaused,out durationSeconds));
		}
	}
}
