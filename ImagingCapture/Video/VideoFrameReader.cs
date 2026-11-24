// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ImagingCapture.Uvc.Interop;

namespace ImagingCapture.Video {
	///<summary>Reads one frame of a video file at its full size with Windows' Media Foundation source reader (IMG-P4d): "Save frame as photo" and
	///the tree's picture of a video. No window and no player: it decodes from the key frame before the position up to the frame shown at it
	///(the last frame that starts at or before the position, as a player shows it).</summary>
	public static class VideoFrameReader {
		///<summary>The most samples read to reach a position (a file that cannot seek is decoded from its start): about 11 minutes at 30 fps.
		///</summary>
		private const int SamplesMax=20000;

		///<summary>The picture's geometry in the decoded RGB32 frames: the buffer's width, height and stride, and the visible area (a 1080-row
		///picture is decoded in 1088-row frames: MF_MT_MINIMUM_DISPLAY_APERTURE says which rows are the picture).</summary>
		internal sealed class Geometry {
			public int Width;
			public int Height;
			public int Stride;
			public int AreaX;
			public int AreaY;
			public int AreaWidth;
			public int AreaHeight;
		}

		///<summary>The frame shown at positionMs (or the last one when the position is at or past the end), and the video's length in
		///milliseconds (0 when unknown). Throws MfException or IOException when the file cannot be read or decoded, or the position cannot be
		///reached.</summary>
		public static Bitmap ReadFrame(string path,int positionMs,out int durationMs) {
			durationMs=0;
			Mf.Check(Mf.MFStartup(Mf.MF_VERSION,Mf.MFSTARTUP_LITE),"MFStartup");
			IMFAttributes attributes=null;
			IMFSourceReader sourceReader=null;
			IMFAttributes mediaType=null;
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
				Geometry geometry=ReadGeometry(sourceReader);
				PropVariantLong propVariantDuration;
				if(sourceReader.GetPresentationAttribute(Mf.MF_SOURCE_READER_MEDIASOURCE,Mf.MF_PD_DURATION,out propVariantDuration)>=0) {
					durationMs=(int)(propVariantDuration.Value/10000);
				}
				long target100ns=Math.Max(0,(long)positionMs*10000);
				if(durationMs>0 && target100ns>=(long)durationMs*10000) {
					target100ns=long.MaxValue;//at or past the end: the last frame
				}
				else if(target100ns>0) {
					PropVariantLong propVariantPosition=new PropVariantLong { vt=Mf.VT_I8,Value=target100ns };
					//a file that cannot seek (MF_E_INVALID_POSITION) is decoded from its start up to the position instead
					sourceReader.SetCurrentPosition(Guid.Empty,ref propVariantPosition);
				}
				//the frame shown at the position: the last sample that starts at or before it (1 ms of rounding allowed)
				Geometry geometrySample=geometry;
				bool isReached=false;
				for(int i=0;i<SamplesMax;i++) {
					int streamIndex;
					int streamFlags;
					long timestamp;
					IMFSample sampleRead;
					Mf.Check(sourceReader.ReadSample(Mf.MF_SOURCE_READER_FIRST_VIDEO_STREAM,0,out streamIndex,out streamFlags,out timestamp,out sampleRead),
						"ReadSample");
					if((streamFlags&Mf.MF_SOURCE_READERF_CURRENTMEDIATYPECHANGED)!=0) {
						geometry=ReadGeometry(sourceReader);//the decoder settled on another size or stride
					}
					if(sampleRead!=null) {
						bool isAfter=target100ns!=long.MaxValue && timestamp>target100ns+10000;
						if(isAfter && sample!=null) {
							Marshal.ReleaseComObject(sampleRead);//the frame before this one is the one shown at the position
							isReached=true;
							break;
						}
						if(sample!=null) {
							Marshal.ReleaseComObject(sample);
						}
						sample=sampleRead;
						geometrySample=geometry;
						if(isAfter) {
							isReached=true;//the seek landed after the position: its first frame is the nearest
							break;
						}
					}
					if((streamFlags&Mf.MF_SOURCE_READERF_ENDOFSTREAM)!=0) {
						isReached=true;
						break;
					}
				}
				if(sample==null) {
					throw new MfException("The video has no frame to read.",0);
				}
				if(!isReached) {
					throw new MfException("The frame at that position could not be reached in this file.",0);
				}
				return ToBitmap(sample,geometrySample);
			}
			finally {
				if(sample!=null) {
					Marshal.ReleaseComObject(sample);
				}
				foreach(object comObject in new object[] { mediaType,sourceReader,attributes }) {
					if(comObject!=null) {
						Marshal.ReleaseComObject(comObject);
					}
				}
				Mf.MFShutdown();
			}
		}

		internal static Geometry ReadGeometry(IMFSourceReader sourceReader) {
			IMFAttributes mediaTypeCurrent;
			Mf.Check(sourceReader.GetCurrentMediaType(Mf.MF_SOURCE_READER_FIRST_VIDEO_STREAM,out mediaTypeCurrent),"GetCurrentMediaType");
			try {
				long frameSize;
				Mf.Check(mediaTypeCurrent.GetUINT64(Mf.MF_MT_FRAME_SIZE,out frameSize),"Frame size");
				Geometry geometry=new Geometry();
				geometry.Width=(int)(frameSize>>32);
				geometry.Height=(int)(frameSize&0xffffffff);
				if(mediaTypeCurrent.GetUINT32(Mf.MF_MT_DEFAULT_STRIDE,out geometry.Stride)<0) {
					geometry.Stride=geometry.Width*4;//not stated: top-down, as the frames of this program's recordings
				}
				geometry.AreaWidth=geometry.Width;
				geometry.AreaHeight=geometry.Height;
				//MFVideoArea: OffsetX (fract, value), OffsetY (fract, value), Area (cx, cy): 16 bytes
				byte[] arrayArea=new byte[16];
				int sizeBlob;
				if(mediaTypeCurrent.GetBlob(Mf.MF_MT_MINIMUM_DISPLAY_APERTURE,arrayArea,arrayArea.Length,out sizeBlob)>=0 && sizeBlob>=16) {
					int x=BitConverter.ToInt16(arrayArea,2);
					int y=BitConverter.ToInt16(arrayArea,6);
					int width=BitConverter.ToInt32(arrayArea,8);
					int height=BitConverter.ToInt32(arrayArea,12);
					if(x>=0 && y>=0 && width>0 && height>0 && x+width<=geometry.Width && y+height<=geometry.Height) {
						geometry.AreaX=x;
						geometry.AreaY=y;
						geometry.AreaWidth=width;
						geometry.AreaHeight=height;
					}
				}
				return geometry;
			}
			finally {
				Marshal.ReleaseComObject(mediaTypeCurrent);
			}
		}

		///<summary>The visible area of the sample's RGB32 pixels as a bitmap; a negative stride means the first row in memory is the bottom.
		///</summary>
		private static Bitmap ToBitmap(IMFSample sample,Geometry geometry) {
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
					Bitmap bitmap=new Bitmap(geometry.AreaWidth,geometry.AreaHeight,PixelFormat.Format32bppRgb);
					BitmapData bitmapData=bitmap.LockBits(new Rectangle(0,0,geometry.AreaWidth,geometry.AreaHeight),ImageLockMode.WriteOnly,
						PixelFormat.Format32bppRgb);
					try {
						byte[] arrayRow=new byte[geometry.AreaWidth*4];
						for(int row=0;row<geometry.AreaHeight;row++) {
							int rowFrame=geometry.AreaY+row;
							int rowSource=geometry.Stride<0 ? geometry.Height-1-rowFrame : rowFrame;
							Marshal.Copy(pData+rowSource*strideAbs+geometry.AreaX*4,arrayRow,0,arrayRow.Length);
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
