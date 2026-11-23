// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Runtime.InteropServices;
using ImagingCapture.Uvc.Interop;

namespace ImagingCapture.Video {
	///<summary>Writes video frames to a file: one implementation encodes MP4 (Mp4Writer); tests use their own.</summary>
	public interface IVideoWriter:IDisposable {
		///<summary>One frame: top-down rows of 4 bytes per pixel (BGRX), stride bytes apart, at timestamp and lasting duration (100 ns units).
		///</summary>
		void WriteFrame(byte[] pixels,int stride,long timestamp100ns,long duration100ns);
		///<summary>Completes the file. After it, the writer only disposes.</summary>
		void Finish();
	}

	///<summary>An MP4 (H.264) file written with Windows' Media Foundation sink writer (IMG-P4c; design section 4: "MP4 with H.264, encoded by
	///Windows itself"). Fragmented MP4, so what is written stays playable if the program stops before Finish. Width and height are made even
	///(H.264 needs it) by leaving out the last column or row. Use it from one thread; the sink writer is free-threaded, so an MTA thread.</summary>
	public sealed class Mp4Writer:IVideoWriter {
		private IMFSinkWriter _sinkWriter;
		private int _streamIndex;
		private readonly int _width;
		private readonly int _height;
		private bool _isStarted;
		private bool _isFinished;

		private Mp4Writer(int width,int height) {
			_width=width;
			_height=height;
		}

		public int Width => _width;
		public int Height => _height;

		///<summary>The H.264 bit rate for a picture size: about 8 Mbit/s for 1080p at 30 frames a second (design: "1080p at about 8 Mbit/s"),
		///scaled by the pixels and the frame rate, at least 1 Mbit/s and at most 16.</summary>
		public static int GetBitRate(int width,int height,double framesPerSecond) {
			double fps=framesPerSecond>0 ? framesPerSecond : 30;
			double bitRate=8000000d*width*height/(1920d*1080d)*fps/30d;
			return (int)Math.Max(1000000,Math.Min(16000000,bitRate));
		}

		///<summary>The frame rate as Media Foundation's ratio: frames per second times 1000 over 1000; 30 when unknown.</summary>
		public static void GetFrameRate(double framesPerSecond,out int numerator,out int denominator) {
			double fps=framesPerSecond>0 && framesPerSecond<=240 ? framesPerSecond : 30;
			numerator=(int)Math.Round(fps*1000);
			denominator=1000;
		}

		///<summary>Starts Media Foundation and opens path for an H.264 stream of the frame size (made even). Throws MfException when Windows has
		///no H.264 encoder or the file cannot be written; nothing stays open then.</summary>
		public static Mp4Writer Create(string path,int width,int height,double framesPerSecond) {
			int widthEven=width&~1;
			int heightEven=height&~1;
			if(widthEven<2 || heightEven<2) {
				throw new ArgumentException("The picture is too small to record.");
			}
			Mf.Check(Mf.MFStartup(Mf.MF_VERSION,Mf.MFSTARTUP_LITE),"MFStartup");
			Mp4Writer writer=new Mp4Writer(widthEven,heightEven);
			try {
				writer.Open(path,framesPerSecond);
			}
			catch {
				writer.Dispose();
				throw;
			}
			return writer;
		}

		private void Open(string path,double framesPerSecond) {
			int fpsNumerator;
			int fpsDenominator;
			GetFrameRate(framesPerSecond,out fpsNumerator,out fpsDenominator);
			IMFAttributes attributes;
			Mf.Check(Mf.MFCreateAttributes(out attributes,3),"MFCreateAttributes");
			try {
				Mf.Check(attributes.SetGUID(Mf.MF_TRANSCODE_CONTAINERTYPE,Mf.MFTranscodeContainerType_FMPEG4),"Container type");
				Mf.Check(attributes.SetUINT32(Mf.MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS,1),"Hardware transforms");
				Mf.Check(attributes.SetUINT32(Mf.MF_SINK_WRITER_DISABLE_THROTTLING,1),"Throttling");
				Mf.Check(Mf.MFCreateSinkWriterFromURL(path,IntPtr.Zero,attributes,out _sinkWriter),"MFCreateSinkWriterFromURL");
			}
			finally {
				Marshal.ReleaseComObject(attributes);
			}
			IMFAttributes mediaTypeOut=CreateVideoType(Mf.MFVideoFormat_H264,fpsNumerator,fpsDenominator);
			IMFAttributes mediaTypeIn=null;
			try {
				Mf.Check(mediaTypeOut.SetUINT32(Mf.MF_MT_AVG_BITRATE,GetBitRate(_width,_height,framesPerSecond)),"Bit rate");
				Mf.Check(_sinkWriter.AddStream(mediaTypeOut,out _streamIndex),"IMFSinkWriter::AddStream");
				mediaTypeIn=CreateVideoType(Mf.MFVideoFormat_RGB32,fpsNumerator,fpsDenominator);
				//A positive stride: the first row in memory is the top of the picture, as UvcFrame keeps it.
				Mf.Check(mediaTypeIn.SetUINT32(Mf.MF_MT_DEFAULT_STRIDE,_width*4),"Stride");
				Mf.Check(_sinkWriter.SetInputMediaType(_streamIndex,mediaTypeIn,null),"IMFSinkWriter::SetInputMediaType (no H.264 encoder for this size?)");
				Mf.Check(_sinkWriter.BeginWriting(),"IMFSinkWriter::BeginWriting");
				_isStarted=true;
			}
			finally {
				Marshal.ReleaseComObject(mediaTypeOut);
				if(mediaTypeIn!=null) {
					Marshal.ReleaseComObject(mediaTypeIn);
				}
			}
		}

		private IMFAttributes CreateVideoType(Guid subtype,int fpsNumerator,int fpsDenominator) {
			IMFAttributes mediaType;
			Mf.Check(Mf.MFCreateMediaType(out mediaType),"MFCreateMediaType");
			try {
				Mf.Check(mediaType.SetGUID(Mf.MF_MT_MAJOR_TYPE,Mf.MFMediaType_Video),"Major type");
				Mf.Check(mediaType.SetGUID(Mf.MF_MT_SUBTYPE,subtype),"Subtype");
				Mf.Check(mediaType.SetUINT32(Mf.MF_MT_INTERLACE_MODE,Mf.MFVideoInterlace_Progressive),"Interlace mode");
				Mf.Check(mediaType.SetUINT64(Mf.MF_MT_FRAME_SIZE,Mf.Pack(_width,_height)),"Frame size");
				Mf.Check(mediaType.SetUINT64(Mf.MF_MT_FRAME_RATE,Mf.Pack(fpsNumerator,fpsDenominator)),"Frame rate");
				Mf.Check(mediaType.SetUINT64(Mf.MF_MT_PIXEL_ASPECT_RATIO,Mf.Pack(1,1)),"Pixel aspect ratio");
			}
			catch {
				Marshal.ReleaseComObject(mediaType);
				throw;
			}
			return mediaType;
		}

		public void WriteFrame(byte[] pixels,int stride,long timestamp100ns,long duration100ns) {
			if(!_isStarted || _isFinished) {
				throw new InvalidOperationException("The video file is not open.");
			}
			int strideOut=_width*4;
			int length=strideOut*_height;
			IMFMediaBuffer buffer;
			Mf.Check(Mf.MFCreateMemoryBuffer(length,out buffer),"MFCreateMemoryBuffer");
			IMFSample sample=null;
			try {
				IntPtr pData;
				int maxLength;
				int currentLength;
				Mf.Check(buffer.Lock(out pData,out maxLength,out currentLength),"IMFMediaBuffer::Lock");
				try {
					for(int row=0;row<_height;row++) {
						Marshal.Copy(pixels,row*stride,pData+row*strideOut,strideOut);
					}
				}
				finally {
					buffer.Unlock();
				}
				Mf.Check(buffer.SetCurrentLength(length),"IMFMediaBuffer::SetCurrentLength");
				Mf.Check(Mf.MFCreateSample(out sample),"MFCreateSample");
				Mf.Check(sample.AddBuffer(buffer),"IMFSample::AddBuffer");
				Mf.Check(sample.SetSampleTime(timestamp100ns),"IMFSample::SetSampleTime");
				Mf.Check(sample.SetSampleDuration(Math.Max(1,duration100ns)),"IMFSample::SetSampleDuration");
				Mf.Check(_sinkWriter.WriteSample(_streamIndex,sample),"IMFSinkWriter::WriteSample");
			}
			finally {
				if(sample!=null) {
					Marshal.ReleaseComObject(sample);
				}
				Marshal.ReleaseComObject(buffer);
			}
		}

		public void Finish() {
			if(!_isStarted || _isFinished) {
				return;
			}
			_isFinished=true;
			Mf.Check(_sinkWriter.FinalizeWriting(),"IMFSinkWriter::Finalize");
		}

		public void Dispose() {
			if(_sinkWriter!=null) {
				Marshal.ReleaseComObject(_sinkWriter);
				_sinkWriter=null;
			}
			Mf.MFShutdown();//balances MFStartup in Create
		}
	}
}
