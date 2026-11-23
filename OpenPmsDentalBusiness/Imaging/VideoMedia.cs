// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using CodeBase;

namespace OpenPmsDentalBusiness {
	///<summary>Video documents in the Imaging module (IMG-P4d; owner imaging design, camera and video, section 4 "Watching a video"): which files
	///are videos, the trim rules (TrimStartMs/TrimEndMs of documentadjust: the file is never cut, the trim points are stored like a crop), the
	///times shown, the description of a frame saved as a photo, and the tree's picture of a video (its first frame with a play mark and its
	///length, saved as Thumbnails\[file name] like an image's).</summary>
	public static class VideoMedia {
		///<summary>The video files the module plays (Windows' own media support).</summary>
		public static readonly string[] ArrayExtensions={ ".mp4",".m4v",".mov",".wmv",".avi" };
		///<summary>A trim shorter than this is refused: nothing would play.</summary>
		public const int TrimMinimumMs=200;
		public const int ThumbnailSize=100;

		public static bool IsVideoPath(string fileName) {
			if(string.IsNullOrWhiteSpace(fileName)) {
				return false;
			}
			string extension=Path.GetExtension(fileName.Trim()).ToLowerInvariant();
			return ArrayExtensions.Contains(extension);
		}

		///<summary>"0:17", or "1:02:05" past an hour.</summary>
		public static string FormatTime(int milliseconds) {
			int seconds=Math.Max(0,milliseconds)/1000;
			if(seconds>=3600) {
				return (seconds/3600).ToString(CultureInfo.InvariantCulture)+":"+(seconds/60%60).ToString("00",CultureInfo.InvariantCulture)+":"
					+(seconds%60).ToString("00",CultureInfo.InvariantCulture);
			}
			return (seconds/60).ToString(CultureInfo.InvariantCulture)+":"+(seconds%60).ToString("00",CultureInfo.InvariantCulture);
		}

		///<summary>Where playback starts: the trim start, or 0.</summary>
		public static int GetPlayStartMs(int trimStartMs,int durationMs) {
			if(trimStartMs<=0 || (durationMs>0 && trimStartMs>=durationMs)) {
				return 0;
			}
			return trimStartMs;
		}

		///<summary>Where playback stops: the trim end, or the end of the video (durationMs; 0 when unknown).</summary>
		public static int GetPlayEndMs(int trimEndMs,int durationMs) {
			if(trimEndMs<=0 || (durationMs>0 && trimEndMs>durationMs)) {
				return durationMs;
			}
			return trimEndMs;
		}

		///<summary>A trim to store from a start and an end the user set (either 0 for "not set"). False with error when it would leave less than
		///TrimMinimumMs. An end at the end of the video, or a start at 0, is stored as 0 (no trim on that side).</summary>
		public static bool TryMakeTrim(int startMs,int endMs,int durationMs,out int trimStartMs,out int trimEndMs,out string error) {
			error="";
			trimStartMs=Math.Max(0,startMs);
			trimEndMs=Math.Max(0,endMs);
			if(durationMs>0) {
				trimStartMs=Math.Min(trimStartMs,durationMs);
				if(trimEndMs>=durationMs) {
					trimEndMs=0;
				}
			}
			int endEffective=trimEndMs>0 ? trimEndMs : durationMs;
			if(endEffective>0 && endEffective-trimStartMs<TrimMinimumMs) {
				error="The start must be before the end.";
				return false;
			}
			return true;
		}

		///<summary>"0:05 to 0:31" for a trim, "" for none.</summary>
		public static string DescribeTrim(int trimStartMs,int trimEndMs,int durationMs) {
			if(trimStartMs<=0 && trimEndMs<=0) {
				return "";
			}
			return FormatTime(GetPlayStartMs(trimStartMs,durationMs))+" to "+FormatTime(GetPlayEndMs(trimEndMs,durationMs));
		}

		///<summary>The description of a frame saved as a photo (design: "From video #30 at 0:17").</summary>
		public static string GetFrameDescription(long docNumVideo,int positionMs) {
			return "From video #"+docNumVideo.ToString(CultureInfo.InvariantCulture)+" at "+FormatTime(positionMs);
		}

		///<summary>The tree's picture of a video: the frame fitted into 100 by 100 on black, a play mark, and the length (when known) at the bottom.
		///</summary>
		public static Bitmap MakeThumbnail(Bitmap bitmapFrame,int durationMs) {
			Bitmap bitmap=new Bitmap(ThumbnailSize,ThumbnailSize);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.Black);
				g.InterpolationMode=InterpolationMode.HighQualityBicubic;
				g.SmoothingMode=SmoothingMode.AntiAlias;
				if(bitmapFrame!=null && bitmapFrame.Width>0 && bitmapFrame.Height>0) {
					float scale=Math.Min((float)ThumbnailSize/bitmapFrame.Width,(float)ThumbnailSize/bitmapFrame.Height);
					float width=bitmapFrame.Width*scale;
					float height=bitmapFrame.Height*scale;
					g.DrawImage(bitmapFrame,(ThumbnailSize-width)/2,(ThumbnailSize-height)/2,width,height);
				}
				using(Brush brushCircle=new SolidBrush(Color.FromArgb(170,0,0,0))) {
					g.FillEllipse(brushCircle,34,34,32,32);
				}
				g.FillPolygon(Brushes.White,new[] { new PointF(44,41),new PointF(44,59),new PointF(59,50) });
				if(durationMs>0) {
					string text=FormatTime(durationMs);
					//The length label shows in the image tree, so it takes the grid's font token (UI-2a; shared, not disposed here).
					Font font=UiTheme.FontGrid;
					using(Brush brushBack=new SolidBrush(Color.FromArgb(170,0,0,0))) {
						SizeF sizeF=g.MeasureString(text,font);
						RectangleF rectangleF=new RectangleF(ThumbnailSize-sizeF.Width-4,ThumbnailSize-sizeF.Height-3,sizeF.Width+2,sizeF.Height);
						g.FillRectangle(brushBack,rectangleF);
						g.DrawString(text,font,Brushes.White,rectangleF.X+1,rectangleF.Y);
					}
				}
			}
			return bitmap;
		}

		///<summary>The path of a document's tree picture (Thumbnails\[file name] in the patient's folder), or null when the images are not in an
		///A to Z folder (cloud or database storage keep no thumbnail files).</summary>
		public static string GetThumbnailPath(Document document,string patFolder) {
			if(PrefC.AtoZfolderUsed!=DataStorageType.LocalAtoZ || document==null || string.IsNullOrEmpty(document.FileName) || string.IsNullOrEmpty(patFolder)) {
				return null;
			}
			return ODFileUtils.CombinePaths(patFolder,"Thumbnails",document.FileName);
		}

		///<summary>Saves the tree picture of a video. Never throws; false when it was not saved.</summary>
		public static bool SaveThumbnail(Document document,string patFolder,Bitmap bitmapFrame,int durationMs) {
			try {
				string path=GetThumbnailPath(document,patFolder);
				if(path==null) {
					return false;
				}
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				using(Bitmap bitmapThumb=MakeThumbnail(bitmapFrame,durationMs)) {
					bitmapThumb.Save(path,ImageFormat.Jpeg);
				}
				return true;
			}
			catch(Exception ex) {
				ex.DoNothing();
				return false;
			}
		}

		///<summary>The saved tree picture of a video, or null when there is none yet (the player saves one when the video is first shown).</summary>
		public static Bitmap LoadThumbnail(Document document,string patFolder) {
			try {
				string path=GetThumbnailPath(document,patFolder);
				if(path==null || !File.Exists(path)) {
					return null;
				}
				using(Bitmap bitmapFile=(Bitmap)Image.FromFile(path)) {
					return new Bitmap(bitmapFile);
				}
			}
			catch(Exception ex) {
				ex.DoNothing();
				return null;
			}
		}
	}
}
