using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OpenPmsDentalBusiness {
	///<summary>Noise reduction strength of the adjust tools (imaging design P3): a median filter, 3x3 (Low) or 5x5 (High).</summary>
	public enum ImageSmooth {
		///<summary>0</summary>
		Off,
		///<summary>1 - a 3x3 median.</summary>
		Low,
		///<summary>2 - a 5x5 median.</summary>
		High,
	}

	///<summary>The adjust tools' settings for one image beyond brightness/contrast (which stay the document's windowing). A new object is neutral.</summary>
	public class ImageAdjustSettings {
		///<summary>0.5 to 2.0; 1 changes nothing. Above 1 lifts the mid-greys, below 1 darkens them. 0 or less is read as 1.</summary>
		public double Gamma=1;
		///<summary>Unsharp mask strength, 0 (off) to 100.</summary>
		public int Sharpen;
		public ImageSmooth Smooth;
		///<summary>Adaptive local contrast (CLAHE).</summary>
		public bool IsAutoContrast;
		public bool IsInverted;

		///<summary>True when nothing is set: Apply changes no pixel. False means the image shows filtered, which the module tags "Adjusted" (D4).</summary>
		public bool IsNeutral() {
			return !HasGamma() && Sharpen<=0 && Smooth==ImageSmooth.Off && !IsAutoContrast && !IsInverted;
		}

		public bool HasGamma() {
			return Gamma>0 && Math.Abs(Gamma-1)>=0.005;
		}

		public ImageAdjustSettings Copy() {
			return (ImageAdjustSettings)MemberwiseClone();
		}
	}

	///<summary>The image filters of the Imaging module's adjust tools (imaging design P3, modelled on Apteryx XrayVision's toolbox): median noise
	///reduction, adaptive local contrast (CLAHE), unsharp-mask sharpening, gamma and invert, in that order. They run on the 8-bit picture after
	///brightness/contrast (windowing), and only on what is shown, printed or exported: the file on disk never changes. A grey image (R=G=B) is
	///filtered as one plane and stays grey; a colour image is filtered per channel, with the local contrast taken from its luminance.</summary>
	public static class ImageAdjust {
		///<summary>The radius of the sharpening blur, in pixels of the image.</summary>
		private const double SHARPEN_SIGMA=1.5;
		///<summary>Sharpen 100 adds 1.5 times the detail (the image minus its blur).</summary>
		private const double SHARPEN_AMOUNT_MAX=1.5;
		private const int CLAHE_TILES=8;
		private const double CLAHE_CLIP=2.0;

		///<summary>Filters bitmap in place with settings. Does nothing for null, a neutral setting, or a pixel format other than 24 or 32 bits per pixel.</summary>
		public static void Apply(Bitmap bitmap,ImageAdjustSettings settings) {
			if(bitmap==null || settings==null || settings.IsNeutral()) {
				return;
			}
			if(!IsFormatSupported(bitmap.PixelFormat)) {
				return;//ApplyToCopy filters a copy of any other format
			}
			int width=bitmap.Width;
			int height=bitmap.Height;
			if(width<1 || height<1) {
				return;
			}
			Rectangle rectangle=new Rectangle(0,0,width,height);
			BitmapData bitmapData=bitmap.LockBits(rectangle,ImageLockMode.ReadWrite,bitmap.PixelFormat);
			try {
				int bytesPerPixel=Image.GetPixelFormatSize(bitmap.PixelFormat)/8;
				int stride=bitmapData.Stride;
				int strideAbs=Math.Abs(stride);
				//A negative stride is a bottom-up bitmap: Scan0 is the top row, and the rows run backwards in memory from the last one.
				IntPtr pointerFirstInMemory=stride>=0 ? bitmapData.Scan0 : IntPtr.Add(bitmapData.Scan0,stride*(height-1));
				Func<int,int> funcRowOffset=y => stride>=0 ? y*strideAbs : (height-1-y)*strideAbs;
				byte[] arrayBytes=new byte[strideAbs*height];
				Marshal.Copy(pointerFirstInMemory,arrayBytes,0,arrayBytes.Length);
				byte[] planeB=new byte[width*height];
				byte[] planeG=new byte[width*height];
				byte[] planeR=new byte[width*height];
				bool isGrey=true;
				for(int y=0;y<height;y++) {
					int row=funcRowOffset(y);
					for(int x=0;x<width;x++) {
						int i=row+x*bytesPerPixel;
						int p=y*width+x;
						planeB[p]=arrayBytes[i];
						planeG[p]=arrayBytes[i+1];
						planeR[p]=arrayBytes[i+2];
						if(isGrey && (planeB[p]!=planeG[p] || planeG[p]!=planeR[p])) {
							isGrey=false;
						}
					}
				}
				if(isGrey) {
					byte[] plane=FilterPlane(planeB,width,height,settings,null);
					planeB=plane;
					planeG=plane;
					planeR=plane;
				}
				else {
					//Colour: smoothed first, per channel, so the local contrast map comes from the smoothed luminance; that map is added to every
					//channel, so the hue stays.
					ImageAdjustSettings settingsAfterSmooth=settings;
					if(settings.Smooth!=ImageSmooth.Off) {
						int radius=settings.Smooth==ImageSmooth.Low ? 1 : 2;
						planeB=Median(planeB,width,height,radius);
						planeG=Median(planeG,width,height,radius);
						planeR=Median(planeR,width,height,radius);
						settingsAfterSmooth=settings.Copy();
						settingsAfterSmooth.Smooth=ImageSmooth.Off;
					}
					short[] arrayDeltaContrast=null;
					if(settings.IsAutoContrast) {
						byte[] planeY=new byte[width*height];
						for(int p=0;p<planeY.Length;p++) {
							planeY[p]=(byte)Math.Round(0.299*planeR[p]+0.587*planeG[p]+0.114*planeB[p]);
						}
						byte[] planeYContrast=Clahe(planeY,width,height,CLAHE_TILES,CLAHE_TILES,CLAHE_CLIP);
						arrayDeltaContrast=new short[planeY.Length];
						for(int p=0;p<planeY.Length;p++) {
							arrayDeltaContrast[p]=(short)(planeYContrast[p]-planeY[p]);
						}
					}
					planeB=FilterPlane(planeB,width,height,settingsAfterSmooth,arrayDeltaContrast);
					planeG=FilterPlane(planeG,width,height,settingsAfterSmooth,arrayDeltaContrast);
					planeR=FilterPlane(planeR,width,height,settingsAfterSmooth,arrayDeltaContrast);
				}
				for(int y=0;y<height;y++) {
					int row=funcRowOffset(y);
					for(int x=0;x<width;x++) {
						int i=row+x*bytesPerPixel;
						int p=y*width+x;
						arrayBytes[i]=planeB[p];
						arrayBytes[i+1]=planeG[p];
						arrayBytes[i+2]=planeR[p];
					}
				}
				Marshal.Copy(arrayBytes,0,pointerFirstInMemory,arrayBytes.Length);
			}
			finally {
				bitmap.UnlockBits(bitmapData);
			}
		}

		///<summary>A filtered 32-bit copy of bitmap, for a picture whose pixel format Apply does not filter in place (an 8-bit indexed greyscale
		///JPEG, for example) or that must stay unchanged. Null when there is nothing to do (null bitmap, null or neutral settings); the caller then
		///keeps its bitmap. The caller disposes the copy.</summary>
		public static Bitmap ApplyToCopy(Bitmap bitmap,ImageAdjustSettings settings) {
			if(bitmap==null || settings==null || settings.IsNeutral()) {
				return null;
			}
			Bitmap bitmapCopy=new Bitmap(bitmap.Width,bitmap.Height,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmapCopy)) {
				g.DrawImage(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));
			}
			Apply(bitmapCopy,settings);
			return bitmapCopy;
		}

		///<summary>A DICOM image's 8-bit picture with its window and its adjust filters (IMG-P3c-2). Noise reduction and sharpening run on the
		///16-bit (or 12-bit) data before the window, where they have the full depth: a sensor's fine grey steps that the window would merge are
		///still there for them. Local contrast, gamma and invert then run on the windowed picture, because they are display mappings of what the
		///window shows. Neutral or null settings: exactly DicomHelper.ApplyWindowing. bitmapDicom is not changed. The caller disposes the bitmap.
		///</summary>
		public static Bitmap WindowDicom(BitmapDicom bitmapDicom,int windowingMin,int windowingMax,ImageAdjustSettings settings) {
			if(bitmapDicom==null) {
				return null;
			}
			if(settings==null || settings.IsNeutral()) {
				return DicomHelper.ApplyWindowing(bitmapDicom,windowingMin,windowingMax);
			}
			BitmapDicom bitmapDicomFiltered=FilterBeforeWindow(bitmapDicom,settings)??bitmapDicom;
			Bitmap bitmap=DicomHelper.ApplyWindowing(bitmapDicomFiltered,windowingMin,windowingMax);
			ImageAdjustSettings settingsAfter=settings.Copy();
			settingsAfter.Smooth=ImageSmooth.Off;
			settingsAfter.Sharpen=0;
			if(IsFormatSupported(bitmap.PixelFormat)) {
				Apply(bitmap,settingsAfter);
				return bitmap;
			}
			Bitmap bitmapCopy=ApplyToCopy(bitmap,settingsAfter);
			if(bitmapCopy==null) {
				return bitmap;
			}
			bitmap.Dispose();
			return bitmapCopy;
		}

		///<summary>A copy of bitmapDicom with the noise reduction and sharpening of settings on its raw data (IMG-P3c-2), clamped to its stored bits;
		///null when settings have neither. bitmapDicom is not changed.</summary>
		public static BitmapDicom FilterBeforeWindow(BitmapDicom bitmapDicom,ImageAdjustSettings settings) {
			if(bitmapDicom?.ArrayDataRaw==null || settings==null || (settings.Smooth==ImageSmooth.Off && settings.Sharpen<=0)) {
				return null;
			}
			int width=bitmapDicom.Width;
			int height=bitmapDicom.Height;
			if(width<1 || height<1 || bitmapDicom.ArrayDataRaw.Length<width*height) {
				return null;
			}
			int bitsStored=bitmapDicom.BitsStored>0 && bitmapDicom.BitsStored<=16 ? bitmapDicom.BitsStored : 16;
			int maxValue=(1<<bitsStored)-1;
			ushort[] arrayData=(ushort[])bitmapDicom.ArrayDataRaw.Clone();
			if(settings.Smooth!=ImageSmooth.Off) {
				arrayData=MedianUshort(arrayData,width,height,settings.Smooth==ImageSmooth.Low ? 1 : 2);
			}
			if(settings.Sharpen>0) {
				double amount=Math.Min(100,settings.Sharpen)/100.0*SHARPEN_AMOUNT_MAX;
				float[] arrayBlur=GaussianBlurUshort(arrayData,width,height,SHARPEN_SIGMA);
				for(int p=0;p<width*height;p++) {
					double value=arrayData[p]+amount*(arrayData[p]-arrayBlur[p]);
					arrayData[p]=(ushort)Math.Max(0,Math.Min(maxValue,Math.Round(value)));
				}
			}
			BitmapDicom bitmapDicomFiltered=new BitmapDicom();
			bitmapDicomFiltered.ArrayDataRaw=arrayData;
			bitmapDicomFiltered.Width=width;
			bitmapDicomFiltered.Height=height;
			bitmapDicomFiltered.BitsAllocated=bitmapDicom.BitsAllocated;
			bitmapDicomFiltered.BitsStored=bitmapDicom.BitsStored;
			bitmapDicomFiltered.WindowingMin=bitmapDicom.WindowingMin;
			bitmapDicomFiltered.WindowingMax=bitmapDicom.WindowingMax;
			bitmapDicomFiltered.PhotometricInterp=bitmapDicom.PhotometricInterp;
			return bitmapDicomFiltered;
		}

		///<summary>Median of 16-bit data over a (2*radius+1) square; the edges repeat the border pixels.</summary>
		public static ushort[] MedianUshort(ushort[] arrayData,int width,int height,int radius) {
			ushort[] result=new ushort[width*height];
			int size=2*radius+1;
			ushort[] window=new ushort[size*size];
			for(int y=0;y<height;y++) {
				for(int x=0;x<width;x++) {
					int n=0;
					for(int dy=-radius;dy<=radius;dy++) {
						int yy=Math.Max(0,Math.Min(height-1,y+dy))*width;
						for(int dx=-radius;dx<=radius;dx++) {
							ushort value=arrayData[yy+Math.Max(0,Math.Min(width-1,x+dx))];
							int k=n-1;
							while(k>=0 && window[k]>value) {
								window[k+1]=window[k];
								k--;
							}
							window[k+1]=value;
							n++;
						}
					}
					result[y*width+x]=window[n/2];
				}
			}
			return result;
		}

		///<summary>A separable Gaussian blur of 16-bit data; the edges repeat the border pixels.</summary>
		public static float[] GaussianBlurUshort(ushort[] arrayData,int width,int height,double sigma) {
			float[] arrayPlane=new float[width*height];
			for(int p=0;p<arrayPlane.Length;p++) {
				arrayPlane[p]=arrayData[p];
			}
			return GaussianBlurFloat(arrayPlane,width,height,sigma);
		}

		private static float[] GaussianBlurFloat(float[] plane,int width,int height,double sigma) {
			int radius=Math.Max(1,(int)Math.Ceiling(3*sigma));
			float[] arrayKernel=new float[2*radius+1];
			double sum=0;
			for(int i=-radius;i<=radius;i++) {
				double value=Math.Exp(-(i*i)/(2*sigma*sigma));
				arrayKernel[i+radius]=(float)value;
				sum+=value;
			}
			for(int i=0;i<arrayKernel.Length;i++) {
				arrayKernel[i]=(float)(arrayKernel[i]/sum);
			}
			float[] arrayHorizontal=new float[plane.Length];
			for(int y=0;y<height;y++) {
				int row=y*width;
				for(int x=0;x<width;x++) {
					float value=0;
					for(int k=-radius;k<=radius;k++) {
						value+=arrayKernel[k+radius]*plane[row+Math.Max(0,Math.Min(width-1,x+k))];
					}
					arrayHorizontal[row+x]=value;
				}
			}
			float[] result=new float[plane.Length];
			for(int y=0;y<height;y++) {
				for(int x=0;x<width;x++) {
					float value=0;
					for(int k=-radius;k<=radius;k++) {
						value+=arrayKernel[k+radius]*arrayHorizontal[Math.Max(0,Math.Min(height-1,y+k))*width+x];
					}
					result[y*width+x]=value;
				}
			}
			return result;
		}

		///<summary>The spot enhancer (IMG-P3c, Apteryx's Spot Enhancer): stretches the picture's grey values so that its darkest and lightest
		///percent (by luminance) reach black and white, the same mapping for every channel. In place; 24 or 32-bit only. Display only: nothing is
		///saved.</summary>
		public static void StretchContrast(Bitmap bitmap) {
			if(bitmap==null || !IsFormatSupported(bitmap.PixelFormat) || bitmap.Width<1 || bitmap.Height<1) {
				return;
			}
			Rectangle rectangle=new Rectangle(0,0,bitmap.Width,bitmap.Height);
			BitmapData bitmapData=bitmap.LockBits(rectangle,ImageLockMode.ReadWrite,bitmap.PixelFormat);
			try {
				int bytesPerPixel=Image.GetPixelFormatSize(bitmap.PixelFormat)/8;
				int strideAbs=Math.Abs(bitmapData.Stride);
				IntPtr pointerFirstInMemory=bitmapData.Stride>=0 ? bitmapData.Scan0 : IntPtr.Add(bitmapData.Scan0,bitmapData.Stride*(bitmap.Height-1));
				byte[] arrayBytes=new byte[strideAbs*bitmap.Height];
				Marshal.Copy(pointerFirstInMemory,arrayBytes,0,arrayBytes.Length);
				int[] arrayHistogram=new int[256];
				int count=bitmap.Width*bitmap.Height;
				for(int y=0;y<bitmap.Height;y++) {
					for(int x=0;x<bitmap.Width;x++) {
						int i=y*strideAbs+x*bytesPerPixel;
						arrayHistogram[(int)Math.Round(0.114*arrayBytes[i]+0.587*arrayBytes[i+1]+0.299*arrayBytes[i+2])]++;
					}
				}
				int low=0;
				int high=255;
				int cumulative=0;
				for(int v=0;v<256;v++) {
					cumulative+=arrayHistogram[v];
					if(cumulative>count/100) {
						low=v;
						break;
					}
				}
				cumulative=0;
				for(int v=255;v>=0;v--) {
					cumulative+=arrayHistogram[v];
					if(cumulative>count/100) {
						high=v;
						break;
					}
				}
				if(high-low<2) {
					return;//flat: nothing to stretch
				}
				byte[] arrayLut=new byte[256];
				for(int v=0;v<256;v++) {
					arrayLut[v]=ClampByte(255.0*(v-low)/(high-low));
				}
				for(int y=0;y<bitmap.Height;y++) {
					for(int x=0;x<bitmap.Width;x++) {
						int i=y*strideAbs+x*bytesPerPixel;
						arrayBytes[i]=arrayLut[arrayBytes[i]];
						arrayBytes[i+1]=arrayLut[arrayBytes[i+1]];
						arrayBytes[i+2]=arrayLut[arrayBytes[i+2]];
					}
				}
				Marshal.Copy(arrayBytes,0,pointerFirstInMemory,arrayBytes.Length);
			}
			finally {
				bitmap.UnlockBits(bitmapData);
			}
		}

		///<summary>The pixel formats Apply filters in place: 24-bit RGB and 32-bit RGB or ARGB (not premultiplied).</summary>
		public static bool IsFormatSupported(PixelFormat pixelFormat) {
			return pixelFormat==PixelFormat.Format24bppRgb || pixelFormat==PixelFormat.Format32bppArgb || pixelFormat==PixelFormat.Format32bppRgb;
		}

		///<summary>The filters on one 8-bit plane, in order: smooth, local contrast (from arrayDeltaContrast for a colour image, else CLAHE of this
		///plane), sharpen, gamma, invert. Returns a new array when anything changed it.</summary>
		public static byte[] FilterPlane(byte[] plane,int width,int height,ImageAdjustSettings settings,short[] arrayDeltaContrast) {
			byte[] result=plane;
			if(settings.Smooth==ImageSmooth.Low) {
				result=Median(result,width,height,1);
			}
			else if(settings.Smooth==ImageSmooth.High) {
				result=Median(result,width,height,2);
			}
			if(settings.IsAutoContrast) {
				if(arrayDeltaContrast==null) {
					result=Clahe(result,width,height,CLAHE_TILES,CLAHE_TILES,CLAHE_CLIP);
				}
				else {
					byte[] resultContrast=new byte[result.Length];
					for(int p=0;p<result.Length;p++) {
						resultContrast[p]=ClampByte(result[p]+arrayDeltaContrast[p]);
					}
					result=resultContrast;
				}
			}
			if(settings.Sharpen>0) {
				result=UnsharpMask(result,width,height,SHARPEN_SIGMA,Math.Min(100,settings.Sharpen)/100.0*SHARPEN_AMOUNT_MAX);
			}
			if(settings.HasGamma() || settings.IsInverted) {
				byte[] arrayLut=new byte[256];
				double gamma=settings.HasGamma() ? Math.Max(0.1,Math.Min(10,settings.Gamma)) : 1;
				for(int v=0;v<256;v++) {
					double value=255*Math.Pow(v/255.0,1/gamma);
					if(settings.IsInverted) {
						value=255-value;
					}
					arrayLut[v]=ClampByte(value);
				}
				byte[] resultLut=new byte[result.Length];
				for(int p=0;p<result.Length;p++) {
					resultLut[p]=arrayLut[result[p]];
				}
				result=resultLut;
			}
			return result;
		}

		///<summary>A median filter over a (2*radius+1) square; the edges repeat the border pixels.</summary>
		public static byte[] Median(byte[] plane,int width,int height,int radius) {
			byte[] result=new byte[plane.Length];
			int size=2*radius+1;
			byte[] window=new byte[size*size];
			for(int y=0;y<height;y++) {
				for(int x=0;x<width;x++) {
					int n=0;
					for(int dy=-radius;dy<=radius;dy++) {
						int yy=Math.Max(0,Math.Min(height-1,y+dy))*width;
						for(int dx=-radius;dx<=radius;dx++) {
							int xx=Math.Max(0,Math.Min(width-1,x+dx));
							//insertion into the sorted window
							byte value=plane[yy+xx];
							int k=n-1;
							while(k>=0 && window[k]>value) {
								window[k+1]=window[k];
								k--;
							}
							window[k+1]=value;
							n++;
						}
					}
					result[y*width+x]=window[n/2];
				}
			}
			return result;
		}

		///<summary>A separable Gaussian blur; the edges repeat the border pixels.</summary>
		public static float[] GaussianBlur(byte[] plane,int width,int height,double sigma) {
			int radius=Math.Max(1,(int)Math.Ceiling(3*sigma));
			float[] arrayKernel=new float[2*radius+1];
			double sum=0;
			for(int i=-radius;i<=radius;i++) {
				double value=Math.Exp(-(i*i)/(2*sigma*sigma));
				arrayKernel[i+radius]=(float)value;
				sum+=value;
			}
			for(int i=0;i<arrayKernel.Length;i++) {
				arrayKernel[i]=(float)(arrayKernel[i]/sum);
			}
			float[] arrayHorizontal=new float[plane.Length];
			for(int y=0;y<height;y++) {
				int row=y*width;
				for(int x=0;x<width;x++) {
					float value=0;
					for(int k=-radius;k<=radius;k++) {
						int xx=Math.Max(0,Math.Min(width-1,x+k));
						value+=arrayKernel[k+radius]*plane[row+xx];
					}
					arrayHorizontal[row+x]=value;
				}
			}
			float[] result=new float[plane.Length];
			for(int y=0;y<height;y++) {
				for(int x=0;x<width;x++) {
					float value=0;
					for(int k=-radius;k<=radius;k++) {
						int yy=Math.Max(0,Math.Min(height-1,y+k));
						value+=arrayKernel[k+radius]*arrayHorizontal[yy*width+x];
					}
					result[y*width+x]=value;
				}
			}
			return result;
		}

		///<summary>Unsharp mask: each pixel plus amount times its difference from the Gaussian blur.</summary>
		public static byte[] UnsharpMask(byte[] plane,int width,int height,double sigma,double amount) {
			float[] arrayBlur=GaussianBlur(plane,width,height,sigma);
			byte[] result=new byte[plane.Length];
			for(int p=0;p<plane.Length;p++) {
				result[p]=ClampByte(plane[p]+amount*(plane[p]-arrayBlur[p]));
			}
			return result;
		}

		///<summary>Contrast-limited adaptive histogram equalization: a clipped, equalized histogram per tile of a tilesX by tilesY grid, blended
		///bilinearly between the four nearest tile centres. clipLimit is a multiple of the tile's mean bin count.</summary>
		public static byte[] Clahe(byte[] plane,int width,int height,int tilesX,int tilesY,double clipLimit) {
			tilesX=Math.Max(1,Math.Min(tilesX,width));
			tilesY=Math.Max(1,Math.Min(tilesY,height));
			byte[,][] arrayMaps=new byte[tilesY,tilesX][];
			for(int ty=0;ty<tilesY;ty++) {
				int y0=ty*height/tilesY;
				int y1=(ty+1)*height/tilesY;
				for(int tx=0;tx<tilesX;tx++) {
					int x0=tx*width/tilesX;
					int x1=(tx+1)*width/tilesX;
					int[] arrayHistogram=new int[256];
					for(int y=y0;y<y1;y++) {
						for(int x=x0;x<x1;x++) {
							arrayHistogram[plane[y*width+x]]++;
						}
					}
					int count=(x1-x0)*(y1-y0);
					int limit=Math.Max(1,(int)(clipLimit*count/256.0));
					int excess=0;
					for(int v=0;v<256;v++) {
						if(arrayHistogram[v]>limit) {
							excess+=arrayHistogram[v]-limit;
							arrayHistogram[v]=limit;
						}
					}
					int add=excess/256;
					int remainder=excess%256;
					for(int v=0;v<256;v++) {
						arrayHistogram[v]+=add+(v<remainder ? 1 : 0);
					}
					byte[] arrayMap=new byte[256];
					long cumulative=0;
					for(int v=0;v<256;v++) {
						cumulative+=arrayHistogram[v];
						arrayMap[v]=count==0 ? (byte)v : ClampByte(255.0*cumulative/count);
					}
					arrayMaps[ty,tx]=arrayMap;
				}
			}
			byte[] result=new byte[plane.Length];
			for(int y=0;y<height;y++) {
				//the position between tile centres
				double fy=(y+0.5)*tilesY/(double)height-0.5;
				int ty0=Math.Max(0,(int)Math.Floor(fy));
				int ty1=Math.Min(tilesY-1,ty0+1);
				double wy=Math.Max(0,Math.Min(1,fy-ty0));
				for(int x=0;x<width;x++) {
					double fx=(x+0.5)*tilesX/(double)width-0.5;
					int tx0=Math.Max(0,(int)Math.Floor(fx));
					int tx1=Math.Min(tilesX-1,tx0+1);
					double wx=Math.Max(0,Math.Min(1,fx-tx0));
					byte v=plane[y*width+x];
					double top=arrayMaps[ty0,tx0][v]*(1-wx)+arrayMaps[ty0,tx1][v]*wx;
					double bottom=arrayMaps[ty1,tx0][v]*(1-wx)+arrayMaps[ty1,tx1][v]*wx;
					result[y*width+x]=ClampByte(top*(1-wy)+bottom*wy);
				}
			}
			return result;
		}

		private static byte ClampByte(double value) {
			if(value<=0) {
				return 0;
			}
			if(value>=255) {
				return 255;
			}
			return (byte)Math.Round(value);
		}
	}
}
