// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ImagingCapture.Uvc.Interop;

namespace ImagingCapture.Video {
	///<summary>Reads one frame of a video file at its full size with Windows' Media Foundation source reader (IMG-P4d): "Save frame as photo" and
	///the tree's picture of a video. No window and no player: it decodes from the key frame before the position up to the frame at it.</summary>
	public static class VideoFrameReader {
		///<summary>The frame at positionMs (or the last one before the end), and the video's length in milliseconds (0 when unknown). Throws
		///MfException or IOException when the file cannot be read or decoded.</summary>
		public static Bitmap ReadFrame(string path,int positionMs,out int durationMs) {
			durationMs=0;
			Mf.Check(Mf.MFStartup(Mf.MF_VERSION,Mf.MFSTARTUP_LITE),"MFStartup");
			IMFAttributes attributes=null;
			IMFSourceReader sourceReader=null;
			IMFAttributes mediaType=null;
			IMFAttributes mediaTypeCurrent=null;
			IMFSample sample=null;
			try {
				Mf.Check(Mf.MFCreateAttributes(out attributes,1),"MFCreateAttributes");
				Mf.Check(attributes.SetUINT32(Mf.MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING,1),"Video processing");
				Mf.Check(Mf.MFCreateSourceReaderFromURL(path,attributes,out sourceReader),"MFCreateSourceReaderFromURL");
				Mf.Check(sourceReader.SetStreamSelection(Mf.MF_SOURCE_READER_ALL_STREAMS,false),"SetStreamSelection");
				Mf.Check(sourceReader.SetStreamSelection(Mf.MF_SOURCE_READER_FIRST_VIDEO_STREAM,true),"SetStreamSelection (video)");
				Mf.Check(Mf.MFCreateMediaType(out mediaType),"MFCreateMediaType");
				Mf.Check(mediaType.SetGUID(Mf.MF_MT_MAJOR_TYPE,Mf.MFMediaType_Video),"Major type");
				Mf.Check(mediaType.SetGUID(Mf.MF_MT_SUBTYPE,Mf.MFVideoFormat_RGB32),"Subtype");
				Mf.Check(sourceReader.SetCurrentMediaType(Mf.MF_SOURCE_READER_FIRST_VIDEO_STREAM,IntPtr.Zero,mediaType),"SetCurrentMediaType (RGB32)");
				Mf.Check(sourceReader.GetCurrentMediaType(Mf.MF_SOURCE_READER_FIRST_VIDEO_STREAM,out mediaTypeCurrent),"GetCurrentMediaType");
				long frameSize;
				Mf.Check(mediaTypeCurrent.GetUINT64(Mf.MF_MT_FRAME_SIZE,out frameSize),"Frame size");
				int width=(int)(frameSize>>32);
				int height=(int)(frameSize&0xffffffff);
				int stride;
				if(mediaTypeCurrent.GetUINT32(Mf.MF_MT_DEFAULT_STRIDE,out stride)<0) {
					stride=width*4;//not stated: top-down, as the frames of this program's recordings
				}
				PropVariantLong propVariantDuration;
				if(sourceReader.GetPresentationAttribute(Mf.MF_SOURCE_READER_MEDIASOURCE,Mf.MF_PD_DURATION,out propVariantDuration)>=0) {
					durationMs=(int)(propVariantDuration.Value/10000);
				}
				long target100ns=Math.Max(0,(long)positionMs*10000);
				if(durationMs>0 && target100ns>=(long)durationMs*10000) {
					target100ns=Math.Max(0,((long)durationMs-100)*10000);//at or past the end: the last frame
				}
				if(target100ns>0) {
					PropVariantLong propVariantPosition=new PropVariantLong { vt=Mf.VT_I8,Value=target100ns };
					//a file that cannot seek (MF_E_INVALID_POSITION) is decoded from its start up to the position instead
					sourceReader.SetCurrentPosition(Guid.Empty,ref propVariantPosition);
				}
				//decode from the key frame before the position to the frame at it; keep the last frame read when the end comes first
				for(int i=0;i<1000;i++) {
					int streamIndex;
					int streamFlags;
					long timestamp;
					IMFSample sampleRead;
					Mf.Check(sourceReader.ReadSample(Mf.MF_SOURCE_READER_FIRST_VIDEO_STREAM,0,out streamIndex,out streamFlags,out timestamp,out sampleRead),
						"ReadSample");
					if(sampleRead!=null) {
						if(sample!=null) {
							Marshal.ReleaseComObject(sample);
						}
						sample=sampleRead;
						if(timestamp+200000>=target100ns) {//within 20 ms
							break;
						}
					}
					if((streamFlags&Mf.MF_SOURCE_READERF_ENDOFSTREAM)!=0) {
						break;
					}
				}
				if(sample==null) {
					throw new MfException("The video has no frame to read.",0);
				}
				return ToBitmap(sample,width,height,stride);
			}
			finally {
				if(sample!=null) {
					Marshal.ReleaseComObject(sample);
				}
				foreach(object comObject in new object[] { mediaTypeCurrent,mediaType,sourceReader,attributes }) {
					if(comObject!=null) {
						Marshal.ReleaseComObject(comObject);
					}
				}
				Mf.MFShutdown();
			}
		}

		///<summary>The sample's RGB32 pixels as a bitmap; a negative stride means the first row in memory is the bottom of the picture.</summary>
		private static Bitmap ToBitmap(IMFSample sample,int width,int height,int stride) {
			IMFMediaBuffer buffer;
			Mf.Check(sample.ConvertToContiguousBuffer(out buffer),"ConvertToContiguousBuffer");
			try {
				IntPtr pData;
				int maxLength;
				int currentLength;
				Mf.Check(buffer.Lock(out pData,out maxLength,out currentLength),"IMFMediaBuffer::Lock");
				try {
					int strideAbs=Math.Abs(stride);
					if(currentLength<strideAbs*height) {
						throw new MfException("The frame is shorter than its size says.",0);
					}
					Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format32bppRgb);
					BitmapData bitmapData=bitmap.LockBits(new Rectangle(0,0,width,height),ImageLockMode.WriteOnly,PixelFormat.Format32bppRgb);
					try {
						byte[] arrayRow=new byte[width*4];
						for(int row=0;row<height;row++) {
							int rowSource=stride<0 ? height-1-row : row;
							Marshal.Copy(pData+rowSource*strideAbs,arrayRow,0,arrayRow.Length);
							Marshal.Copy(arrayRow,0,bitmapData.Scan0+row*bitmapData.Stride,arrayRow.Length);
						}
					}
					finally {
						bitmap.UnlockBits(bitmapData);
					}
					return bitmap;
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
