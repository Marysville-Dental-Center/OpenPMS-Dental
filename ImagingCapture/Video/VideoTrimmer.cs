// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Runtime.InteropServices;
using ImagingCapture.Uvc.Interop;

namespace ImagingCapture.Video {
	///<summary>Writes the trimmed part of a video as a new MP4 (IMG-TRIM; owner imaging design, section 4: "Export writes the trim range"). The
	///stored file is never cut: a trim is kept like a crop (documentadjust TrimStartMs/TrimEndMs), and only an export writes it out. Windows'
	///Media Foundation decodes the source from the frame shown at the trim start up to the trim end, and Mp4Writer encodes those frames again
	///(H.264, as a recording), so the cut is exact to the frame and needs no key frame at the start. There is no sound: videos are recorded
	///without it (owner, 2026-11-23).</summary>
	public static class VideoTrimmer {
		///<summary>The most samples read: about 28 minutes at 30 frames a second, far above the longest recording (600 s).</summary>
		private const int SamplesMax=50000;

		///<summary>Writes pathSource from trimStartMs to trimEndMs (0: the start, or the end, of the video) to pathOut, an MP4 whatever its
		///extension, replacing a file there. As the player does (VideoMedia.GetPlayStartMs), a start at or past the end counts as 0. The first
		///frame is the one shown at the trim start, and the result starts at 0. durationOutMs is the length written. The file is written under a
		///temporary name next to pathOut and renamed when complete, so a failure or a cancel leaves no partial file. funcIsCancelled is asked
		///before each frame; when it says yes, OperationCanceledException is thrown. Throws MfException, IOException or
		///UnauthorizedAccessException (a target in use) when the source cannot be read or the result cannot be written. Run it off the UI
		///thread: it decodes and encodes every frame of the trim.</summary>
		public static void WriteTrimmed(string pathSource,string pathOut,int trimStartMs,int trimEndMs,out int durationOutMs,
			Func<bool> funcIsCancelled=null)
		{
			durationOutMs=0;
			string pathPartial=pathOut+".partial";
			Mf.Check(Mf.MFStartup(Mf.MF_VERSION,Mf.MFSTARTUP_LITE),"MFStartup");
			IMFAttributes attributes=null;
			IMFSourceReader sourceReader=null;
			IMFAttributes mediaType=null;
			Mp4Writer writer=null;
			try {
				Mf.Check(Mf.MFCreateAttributes(out attributes,1),"MFCreateAttributes");
				Mf.Check(attributes.SetUINT32(Mf.MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING,1),"Video processing");
				Mf.Check(Mf.MFCreateSourceReaderFromURL(pathSource,attributes,out sourceReader),"MFCreateSourceReaderFromURL");
				Mf.Check(sourceReader.SetStreamSelection(Mf.MF_SOURCE_READER_ALL_STREAMS,false),"SetStreamSelection");
				Mf.Check(sourceReader.SetStreamSelection(Mf.MF_SOURCE_READER_FIRST_VIDEO_STREAM,true),"SetStreamSelection (video)");
				Mf.Check(Mf.MFCreateMediaType(out mediaType),"MFCreateMediaType");
				Mf.Check(mediaType.SetGUID(Mf.MF_MT_MAJOR_TYPE,Mf.MFMediaType_Video),"Major type");
				Mf.Check(mediaType.SetGUID(Mf.MF_MT_SUBTYPE,Mf.MFVideoFormat_RGB32),"Subtype");
				Mf.Check(sourceReader.SetCurrentMediaType(Mf.MF_SOURCE_READER_FIRST_VIDEO_STREAM,IntPtr.Zero,mediaType),"SetCurrentMediaType (RGB32)");
				VideoFrameReader.Geometry geometry=VideoFrameReader.ReadGeometry(sourceReader);
				double framesPerSecond=ReadFramesPerSecond(sourceReader);
				long durationSource100ns=0;
				PropVariantLong propVariantDuration;
				if(sourceReader.GetPresentationAttribute(Mf.MF_SOURCE_READER_MEDIASOURCE,Mf.MF_PD_DURATION,out propVariantDuration)>=0) {
					durationSource100ns=propVariantDuration.Value;
				}
				long start100ns=Math.Max(0,(long)trimStartMs*10000);
				long end100ns=trimEndMs>0 ? (long)trimEndMs*10000 : long.MaxValue;
				if(durationSource100ns>0 && end100ns>durationSource100ns) {
					end100ns=durationSource100ns;
				}
				if(durationSource100ns>0 && start100ns>=durationSource100ns) {
					start100ns=0;
				}
				if(end100ns<=start100ns) {
					throw new MfException("The trim ends before it starts.",0);
				}
				if(start100ns>0) {
					PropVariantLong propVariantPosition=new PropVariantLong { vt=Mf.VT_I8,Value=start100ns };
					//a file that cannot seek (MF_E_INVALID_POSITION) is decoded from its start instead
					sourceReader.SetCurrentPosition(Guid.Empty,ref propVariantPosition);
				}
				if(File.Exists(pathPartial)) {
					File.Delete(pathPartial);
				}
				writer=Mp4Writer.Create(pathPartial,geometry.AreaWidth,geometry.AreaHeight,framesPerSecond);
				//Each frame is written once the next one is known, with the time to it as its length. A frame at or before the start replaces
				//the one held, so the frame shown at the start is the first one written, at time 0.
				byte[] arrayPixelsHeld=null;
				long timeHeldOut=0;
				long durationLast100ns=0;
				int stride=writer.Width*4;
				bool isEnded=false;
				for(int i=0;i<SamplesMax;i++) {
					if(funcIsCancelled!=null && funcIsCancelled()) {
						throw new OperationCanceledException();
					}
					int streamIndex;
					int streamFlags;
					long timestamp;
					IMFSample sample;
					Mf.Check(sourceReader.ReadSample(Mf.MF_SOURCE_READER_FIRST_VIDEO_STREAM,0,out streamIndex,out streamFlags,out timestamp,out sample),
						"ReadSample");
					if((streamFlags&Mf.MF_SOURCE_READERF_CURRENTMEDIATYPECHANGED)!=0) {
						geometry=VideoFrameReader.ReadGeometry(sourceReader);
					}
					if(sample!=null) {
						try {
							if(timestamp>=end100ns-10000) {//1 ms of rounding allowed, as at the start
								isEnded=true;
								break;
							}
							//At the start (1 ms of rounding allowed), or the first frame when a seek landed after the start: time 0.
							long timeOut=arrayPixelsHeld==null || timestamp<=start100ns+10000 ? 0 : timestamp-start100ns;
							if(arrayPixelsHeld!=null && timeOut>timeHeldOut) {
								durationLast100ns=timeOut-timeHeldOut;
								writer.WriteFrame(arrayPixelsHeld,stride,timeHeldOut,durationLast100ns);
							}
							arrayPixelsHeld=CopyArea(sample,geometry,writer.Width,writer.Height);
							timeHeldOut=timeOut;
						}
						finally {
							Marshal.ReleaseComObject(sample);
						}
					}
					if((streamFlags&Mf.MF_SOURCE_READERF_ENDOFSTREAM)!=0) {
						isEnded=true;
						break;
					}
				}
				if(!isEnded) {
					throw new MfException("The video is too long to export trimmed (more than "+SamplesMax+" frames).",0);
				}
				if(arrayPixelsHeld==null) {
					throw new MfException("The video has no frame in the trimmed part.",0);
				}
				//The last frame lasts to the trim end (or the end of the video), else as long as the frame before it.
				long endOut=end100ns!=long.MaxValue ? end100ns-start100ns : timeHeldOut+(durationLast100ns>0 ? durationLast100ns : 333333);
				long durationHeld=Math.Max(1,endOut-timeHeldOut);
				writer.WriteFrame(arrayPixelsHeld,stride,timeHeldOut,durationHeld);
				writer.Finish();
				writer.Dispose();
				writer=null;
				durationOutMs=(int)((timeHeldOut+durationHeld)/10000);
				File.Move(pathPartial,pathOut,true);
			}
			catch {
				if(writer!=null) {
					writer.Dispose();
					writer=null;
				}
				try {
					File.Delete(pathPartial);
				}
				catch {
				}
				throw;
			}
			finally {
				writer?.Dispose();
				foreach(object comObject in new object[] { mediaType,sourceReader,attributes }) {
					if(comObject!=null) {
						Marshal.ReleaseComObject(comObject);
					}
				}
				Mf.MFShutdown();
			}
		}

		///<summary>The decoded stream's frame rate (MF_MT_FRAME_RATE, numerator over denominator), or 30 when it is not stated.</summary>
		private static double ReadFramesPerSecond(IMFSourceReader sourceReader) {
			IMFAttributes mediaTypeCurrent;
			if(sourceReader.GetCurrentMediaType(Mf.MF_SOURCE_READER_FIRST_VIDEO_STREAM,out mediaTypeCurrent)<0) {
				return 30;
			}
			try {
				long frameRate;
				if(mediaTypeCurrent.GetUINT64(Mf.MF_MT_FRAME_RATE,out frameRate)<0) {
					return 30;
				}
				long numerator=frameRate>>32;
				long denominator=frameRate&0xffffffff;
				if(numerator<=0 || denominator<=0) {
					return 30;
				}
				return (double)numerator/denominator;
			}
			finally {
				Marshal.ReleaseComObject(mediaTypeCurrent);
			}
		}

		///<summary>The visible area of the sample's RGB32 pixels, top row first, as widthOut by heightOut (the writer's even size, at most the
		///area's: an odd last column or row is left out).</summary>
		private static byte[] CopyArea(IMFSample sample,VideoFrameReader.Geometry geometry,int widthOut,int heightOut) {
			IMFMediaBuffer buffer;
			Mf.Check(sample.ConvertToContiguousBuffer(out buffer),"ConvertToContiguousBuffer");
			try {
				IntPtr pData;
				int maxLength;
				int currentLength;
				Mf.Check(buffer.Lock(out pData,out maxLength,out currentLength),"IMFMediaBuffer::Lock");
				try {
					int strideAbs=Math.Abs(geometry.Stride);
					if(currentLength<strideAbs*geometry.Height || strideAbs<geometry.Width*4) {
						throw new MfException("The frame is shorter than its size says.",0);
					}
					int width=Math.Min(widthOut,geometry.AreaWidth);
					int height=Math.Min(heightOut,geometry.AreaHeight);
					byte[] arrayPixels=new byte[widthOut*4*heightOut];
					for(int row=0;row<height;row++) {
						int rowFrame=geometry.AreaY+row;
						int rowSource=geometry.Stride<0 ? geometry.Height-1-rowFrame : rowFrame;
						Marshal.Copy(pData+rowSource*strideAbs+geometry.AreaX*4,arrayPixels,row*widthOut*4,width*4);
					}
					return arrayPixels;
				}
				finally {
					buffer.Unlock();
				}
			}
			finally {
				Marshal.ReleaseComObject(buffer);
			}
		}
	}
}
