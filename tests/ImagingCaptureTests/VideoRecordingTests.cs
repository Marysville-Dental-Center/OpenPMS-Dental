// IMG-P4c: video recording. VideoRecorder's timeline (stream times, pauses taken out, the limit, dropped frames) with a writer of the
// test's own, and Mp4Writer writing a real MP4 with Windows' Media Foundation from synthetic frames into a temporary folder (no camera).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using ImagingCapture.Uvc;
using ImagingCapture.Uvc.Interop;
using ImagingCapture.Video;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingCapture.Tests {
	[TestClass]
	public class VideoRecordingTests {
		private sealed class FakeWriter:IVideoWriter {
			public readonly List<long> ListTimestamps=new List<long>();
			public int Width;
			public int Height;
			public bool IsFinished;
			public bool IsDisposed;
			public int DelayMs;
			public bool IsFailing;

			public void WriteFrame(byte[] pixels,int stride,long timestamp100ns,long duration100ns) {
				if(IsFailing) {
					throw new Exception("The disk is full.");
				}
				if(DelayMs>0) {
					Thread.Sleep(DelayMs);
				}
				lock(ListTimestamps) {
					ListTimestamps.Add(timestamp100ns);
				}
			}

			public void Finish() {
				IsFinished=true;
			}

			public void Dispose() {
				IsDisposed=true;
			}
		}

		private static UvcFrame Frame(double sampleTime,int width=8,int height=6) {
			UvcFrame frame=new UvcFrame(width,height);
			frame.SampleTime=sampleTime;
			return frame;
		}

		private static void WaitFor(Func<bool> func) {
			for(int i=0;i<200 && !func();i++) {
				Thread.Sleep(10);
			}
		}

		[TestMethod]
		public void Timeline_FromStreamTimes_PausesTakenOut() {
			FakeWriter writer=new FakeWriter();
			VideoRecorder recorder=new VideoRecorder((w,h) => { writer.Width=w; writer.Height=h; return writer; },framesPerSecond:10);
			recorder.Start();
			recorder.OfferFrame(Frame(5.0));
			WaitFor(() => writer.ListTimestamps.Count==1);
			recorder.OfferFrame(Frame(5.1));
			WaitFor(() => writer.ListTimestamps.Count==2);
			recorder.Pause();
			recorder.OfferFrame(Frame(5.2));//paused: not recorded
			recorder.Resume();
			recorder.OfferFrame(Frame(9.2));//4 seconds later: the gap is taken out
			WaitFor(() => writer.ListTimestamps.Count==3);
			VideoRecordingResult result=recorder.Stop();
			CollectionAssert.AreEqual(new long[] { 0,1000000,2000000 },writer.ListTimestamps);
			Assert.AreEqual(3,result.FramesWritten);
			Assert.AreEqual(TimeSpan.FromSeconds(0.3),result.Duration);
			Assert.AreEqual(8,writer.Width);
			Assert.IsTrue(writer.IsFinished && writer.IsDisposed);
			Assert.AreEqual("",result.Error);
			Assert.IsNotNull(result.FrameFirst);
			recorder.Stop();//again: nothing happens
		}

		[TestMethod]
		public void Limit_StopsAccepting_AndWarnsBefore() {
			FakeWriter writer=new FakeWriter();
			VideoRecorder recorder=new VideoRecorder((w,h) => writer,framesPerSecond:1,limitSeconds:20);
			recorder.Start();
			for(int i=0;i<25;i++) {
				recorder.OfferFrame(Frame(i));
				WaitFor(() => writer.ListTimestamps.Count==Math.Min(i+1,20));
				if(i==3) {
					Assert.IsFalse(recorder.IsNearLimit,"4 of 20 seconds recorded.");
				}
				if(i==4) {
					Assert.IsTrue(recorder.IsNearLimit,"5 of 20 seconds: warned 15 seconds before the limit.");
				}
			}
			Assert.IsTrue(recorder.IsLimitReached);
			VideoRecordingResult result=recorder.Stop();
			Assert.AreEqual(20,result.FramesWritten,"Frames from the limit on are not recorded.");
			Assert.IsTrue(result.IsLimitReached);
		}

		[TestMethod]
		public void SlowEncoder_DropsFrames_NeverBlocksTheStream() {
			FakeWriter writer=new FakeWriter { DelayMs=200 };
			VideoRecorder recorder=new VideoRecorder((w,h) => writer,framesPerSecond:30);
			recorder.Start();
			DateTime dateTimeStart=DateTime.Now;
			for(int i=0;i<40;i++) {
				recorder.OfferFrame(Frame(i/30d));
			}
			Assert.IsTrue((DateTime.Now-dateTimeStart).TotalMilliseconds<1000,"Offering frames returns at once.");
			VideoRecordingResult result=recorder.Stop();
			Assert.IsTrue(result.FramesDropped>0);
			Assert.AreEqual(40,result.FramesWritten+result.FramesDropped);
		}

		[TestMethod]
		public void WriterFailure_IsReported_TheStreamGoesOn() {
			FakeWriter writer=new FakeWriter { IsFailing=true };
			VideoRecorder recorder=new VideoRecorder((w,h) => writer,framesPerSecond:30);
			recorder.Start();
			recorder.OfferFrame(Frame(0));
			WaitFor(() => recorder.Stop().Error!="");
			VideoRecordingResult result=recorder.Stop();
			StringAssert.Contains(result.Error,"The disk is full.");
			Assert.IsTrue(writer.IsDisposed);
		}

		[TestMethod]
		public void SizeChange_EndsTheVideo_IsFailedSet() {
			FakeWriter writer=new FakeWriter();
			VideoRecorder recorder=new VideoRecorder((w,h) => writer,framesPerSecond:10);
			recorder.Start();
			recorder.OfferFrame(Frame(0,8,6));
			WaitFor(() => writer.ListTimestamps.Count==1);
			recorder.OfferFrame(Frame(0.1,10,6));
			WaitFor(() => recorder.IsFailed);
			Assert.IsTrue(recorder.IsFailed,"The window stops the recording and says why.");
			VideoRecordingResult result=recorder.Stop();
			StringAssert.Contains(result.Error,"picture size changed");
			Assert.AreEqual(1,result.FramesWritten);
			Assert.IsFalse(result.IsTimedOut);
			recorder.Dispose();
		}

		[TestMethod]
		public void BitRateAndFrameRate() {
			Assert.AreEqual(8000000,Mp4Writer.GetBitRate(1920,1080,30));
			Assert.AreEqual(1000000,Mp4Writer.GetBitRate(320,240,30),"At least 1 Mbit/s.");
			Assert.AreEqual(16000000,Mp4Writer.GetBitRate(3840,2160,60),"At most 16.");
			Mp4Writer.GetFrameRate(29.97,out int numerator,out int denominator);
			Assert.AreEqual(29970,numerator);
			Assert.AreEqual(1000,denominator);
			Mp4Writer.GetFrameRate(0,out numerator,out denominator);
			Assert.AreEqual(30000,numerator);
		}

		[TestMethod]
		public void FrameReader_ReadsAFrameAtFullSize_RightWayUp_AtThePosition() {
			string folder=Path.Combine(Path.GetTempPath(),"opms-frame-"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(folder);
			string path=Path.Combine(folder,"frames.mp4");
			try {
				Mp4Writer writer;
				try {
					writer=Mp4Writer.Create(path,320,240,15);
				}
				catch(MfException ex) {
					Assert.Inconclusive("No H.264 encoder here: "+ex.Message);
					return;
				}
				using(writer) {
					byte[] arrayPixels=new byte[320*240*4];
					for(int i=0;i<30;i++) {
						for(int p=0;p<arrayPixels.Length;p+=4) {
							bool isTop=p/4/320<120;
							bool isSecondHalf=i>=15;
							arrayPixels[p]=(byte)(!isTop ? 220 : 20);//blue: the bottom
							arrayPixels[p+1]=(byte)(isSecondHalf ? 200 : 20);//green: the second second
							arrayPixels[p+2]=(byte)(isTop ? 220 : 20);//red: the top
							arrayPixels[p+3]=255;
						}
						writer.WriteFrame(arrayPixels,320*4,i*666667L,666667L);
					}
					writer.Finish();
				}
				int durationMs;
				using(Bitmap bitmap=VideoFrameReader.ReadFrame(path,0,out durationMs)) {
					Assert.AreEqual(320,bitmap.Width);
					Assert.AreEqual(240,bitmap.Height);
					Color colorTop=bitmap.GetPixel(160,40);
					Color colorBottom=bitmap.GetPixel(160,200);
					Assert.IsTrue(colorTop.R>150 && colorTop.B<100,"Red at the top: "+colorTop);
					Assert.IsTrue(colorBottom.B>150 && colorBottom.R<100,"Blue at the bottom: "+colorBottom);
					Assert.IsTrue(colorTop.G<100,"The first second has no green: "+colorTop);
				}
				Assert.IsTrue(Math.Abs(durationMs-2000)<=200,"Length "+durationMs+" ms");
				using(Bitmap bitmap=VideoFrameReader.ReadFrame(path,1500,out durationMs)) {
					Color colorTop=bitmap.GetPixel(160,40);
					Assert.IsTrue(colorTop.G>150,"At 1.5 s the frame is from the second second: "+colorTop);
				}
				using(Bitmap bitmap=VideoFrameReader.ReadFrame(path,60000,out durationMs)) {
					Assert.IsNotNull(bitmap,"Past the end: the last frame.");
				}
			}
			finally {
				Directory.Delete(folder,true);
			}
		}

		[TestMethod]
		public void Mp4Writer_WritesAPlayableMp4_WithWindowsOwnEncoder() {
			string folder=Path.Combine(Path.GetTempPath(),"opms-mp4-"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(folder);
			string path=Path.Combine(folder,"test.mp4");
			try {
				Mp4Writer writer;
				try {
					writer=Mp4Writer.Create(path,321,241,15);//odd sizes: made even
				}
				catch(MfException ex) {
					Assert.Inconclusive("Media Foundation or its H.264 encoder is not available here (Windows N without the Media Feature Pack?): "
						+ex.Message);
					return;
				}
				using(writer) {
					Assert.AreEqual(320,writer.Width);
					Assert.AreEqual(240,writer.Height);
					UvcFrame frame=new UvcFrame(321,241);
					for(int i=0;i<30;i++) {
						for(int p=0;p<frame.Pixels.Length;p+=4) {
							frame.Pixels[p]=(byte)(i*8);//blue changes over time
							frame.Pixels[p+1]=(byte)(p/4%321);
							frame.Pixels[p+2]=128;
							frame.Pixels[p+3]=255;
						}
						writer.WriteFrame(frame.Pixels,frame.Stride,i*666667L,666667L);
					}
					writer.Finish();
				}
				FileInfo fileInfo=new FileInfo(path);
				Assert.IsTrue(fileInfo.Length>1000,"Size: "+fileInfo.Length);
				byte[] arrayBytes=File.ReadAllBytes(path);
				Assert.AreEqual("ftyp",Encoding.ASCII.GetString(arrayBytes,4,4),"An MP4 file.");
				string text=Encoding.ASCII.GetString(arrayBytes);
				StringAssert.Contains(text,"avc1","H.264 video.");
				StringAssert.Contains(text,"moof","Fragmented: playable even if the program stops early.");
			}
			finally {
				Directory.Delete(folder,true);
			}
		}
	}
}
