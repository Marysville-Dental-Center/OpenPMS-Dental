// The masking and hashing rule of the UI refresh's picture galleries (docs/specs/ui-refresh-plan.md, sections 7 and 8; docs/port-notes/UI-0a.md),
// and the comparison the gallery shows. ONE copy of the rule, used two ways:
// - tools/New-UiGallery.ps1 compiles this file with Add-Type in Windows PowerShell 5.1, so the file is C# 5 (no string interpolation, no nameof,
//   no ?. operator, no expression-bodied members) and uses only the .NET Framework and System.Drawing;
// - a test project compiles it in as a linked file (<Compile Include="..\..\tools\UiGallery\UiPictureMask.cs" Link="UiPictureMask.cs" />):
//   ReleaseFlowTests for the builder's self-tests, and tests/WindowChecksTests for its check U0 (UI-0c), which compares each picture's masked
//   pixel SHA-256 with tests/WindowChecksTests/UiApproved/approved.tsv through GetMaskedPixelSha256 and ReadApproved.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace UiGalleryTools {
	///<summary>The corner a mask rectangle is measured from. The harness takes windows at fixed sizes, but a window that grows keeps what is
	///anchored to its right or bottom edge at the same distance from that edge (the Appointments module's date panel, for example).</summary>
	public enum UiMaskAnchor {
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
	}

	///<summary>One row of tools/ui-gallery-masks.tsv: a rectangle of the pictures whose name matches Picture that changes from run to run (the clock,
	///a date, the copy's database name) and is left out of the hash, the share of changed pixels and the difference picture.</summary>
	public class UiMaskRow {
		///<summary>A wildcard (* and ?, any case). Without a slash it matches a picture's file name, with one its whole relative path.</summary>
		public string Picture;
		///<summary>0 for any size; otherwise the row applies only to a picture of exactly SizeWidth x SizeHeight pixels.</summary>
		public int SizeWidth;
		public int SizeHeight;
		public UiMaskAnchor Anchor;
		///<summary>Distance in pixels from the anchor corner's left or right edge to the rectangle's nearest side.</summary>
		public int X;
		///<summary>Distance in pixels from the anchor corner's top or bottom edge to the rectangle's nearest side.</summary>
		public int Y;
		public int Width;
		public int Height;
		///<summary>What the rectangle hides, in words: "clock: ...", "date: ...", "database: ...".</summary>
		public string What;
		///<summary>The row's line in its file, for messages.</summary>
		public int LineNumber;

		///<summary>True when the row applies to a picture of this size (its size column is * or this size).</summary>
		public bool AppliesToSize(int width,int height) {
			return SizeWidth==0 || (SizeWidth==width && SizeHeight==height);
		}

		public override string ToString() {
			return "line "+LineNumber+" ("+Picture+", "+(SizeWidth==0?"*":SizeWidth+"x"+SizeHeight)+", "+UiPictureMask.AnchorToText(Anchor)+" "+X+","+Y+" "
				+Width+"x"+Height+": "+What+")";
		}
	}

	///<summary>One row of tests/WindowChecksTests/UiApproved/approved.tsv: a picture the owner approved in a gallery.</summary>
	public class UiApprovedRow {
		///<summary>The picture's relative path with forward slashes, as the gallery names it (for the harness: its file name, like "U12-claims.png").</summary>
		public string Picture;
		public int Width;
		public int Height;
		///<summary>The masked pixel SHA-256 (GetMaskedPixelSha256), lower-case hex.</summary>
		public string Sha256;
		///<summary>The gallery that showed it, like G-UI3.</summary>
		public string Gallery;
		///<summary>The commit the gallery was made at.</summary>
		public string Commit;
		///<summary>The day the owner approved it, yyyy-MM-dd.</summary>
		public string Date;
		public int LineNumber;
	}

	///<summary>A picture's pixels as 32-bit BGRA, row by row with no row padding.</summary>
	public class UiPixels {
		public int Width;
		public int Height;
		public byte[] Bgra;
	}

	///<summary>How an after picture compares with its before picture, masks applied to both.</summary>
	public class UiPictureCompareResult {
		///<summary>Width and height of the area compared: the larger of the two pictures in each direction.</summary>
		public int Width;
		public int Height;
		///<summary>Pixels of that area that a mask covers (in either picture): not compared.</summary>
		public long CountMasked;
		///<summary>Pixels compared: the area less the masked ones.</summary>
		public long CountCompared;
		///<summary>Compared pixels that differ by more than the threshold in a channel, plus those only one picture has.</summary>
		public long CountChanged;
		///<summary>The smallest rectangle around every changed pixel; empty when none changed.</summary>
		public Rectangle BoundsChanged;
		public string Sha256Before;
		public string Sha256After;
		///<summary>The mask rows that applied to the after picture.</summary>
		public List<UiMaskRow> ListMasksApplied=new List<UiMaskRow>();

		///<summary>CountChanged as a percentage of CountCompared; 0 when nothing was compared.</summary>
		public double PercentChanged {
			get {
				if(CountCompared==0) {
					return 0;
				}
				return CountChanged*100.0/CountCompared;
			}
		}
	}

	///<summary>The masking and hashing rule, and the comparison. See the file header.</summary>
	public static class UiPictureMask {
		///<summary>The value every masked pixel gets before hashing: B, G, R and A all 0 (transparent black, which an opaque screen picture never has).</summary>
		public const byte MaskedByte=0;
		///<summary>The header line of approved.tsv.</summary>
		public const string ApprovedHeader="picture\tsize\tsha256\tgallery\tcommit\tdate";

		#region Pictures and their names
		///<summary>The picture's name as galleries and approved.tsv use it: the path relative to the picture folder, with forward slashes and without a
		///leading "./" or slash.</summary>
		public static string NormalizePicture(string relativePath) {
			string picture=(relativePath??"").Replace('\\','/').Trim();
			while(picture.StartsWith("./",StringComparison.Ordinal)) {
				picture=picture.Substring(2);
			}
			return picture.TrimStart('/');
		}

		///<summary>True when the wildcard pattern (* any run, ? one character, any case) matches the picture: the file name when the pattern has no
		///slash, the whole relative path when it has one.</summary>
		public static bool IsMatch(string pattern,string picture) {
			string name=NormalizePicture(picture);
			string patternNormal=NormalizePicture(pattern);
			if(patternNormal.IndexOf('/')<0) {
				int indexSlash=name.LastIndexOf('/');
				if(indexSlash>=0) {
					name=name.Substring(indexSlash+1);
				}
			}
			string regex="^"+Regex.Escape(patternNormal).Replace("\\*",".*").Replace("\\?",".")+"$";
			return Regex.IsMatch(name,regex,RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);
		}

		///<summary>The pixels as 32-bit BGRA, row by row with no row padding, from any pixel format (the layout of ChartImageCompare.GetPixelsArgb in
		///tests/ToothChartImageTests).</summary>
		public static UiPixels GetPixels(Bitmap bitmap) {
			UiPixels uiPixels=new UiPixels();
			uiPixels.Width=bitmap.Width;
			uiPixels.Height=bitmap.Height;
			uiPixels.Bgra=new byte[uiPixels.Width*uiPixels.Height*4];
			if(uiPixels.Bgra.Length==0) {
				return uiPixels;
			}
			BitmapData bitmapData=bitmap.LockBits(new Rectangle(0,0,uiPixels.Width,uiPixels.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
			try {
				for(int y=0;y<uiPixels.Height;y++) {
					Marshal.Copy(new IntPtr(bitmapData.Scan0.ToInt64()+(long)y*bitmapData.Stride),uiPixels.Bgra,y*uiPixels.Width*4,uiPixels.Width*4);
				}
			}
			finally {
				bitmap.UnlockBits(bitmapData);
			}
			return uiPixels;
		}

		///<summary>Reads a picture file (PNG, or any format GDI+ reads) without keeping the file open.</summary>
		public static UiPixels LoadPixels(string path) {
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path))) {
				using(Bitmap bitmap=new Bitmap(memoryStream)) {
					return GetPixels(bitmap);
				}
			}
		}
		#endregion Pictures and their names

		#region Masks
		public static string AnchorToText(UiMaskAnchor anchor) {
			switch(anchor) {
				case UiMaskAnchor.TopRight:
					return "top-right";
				case UiMaskAnchor.BottomLeft:
					return "bottom-left";
				case UiMaskAnchor.BottomRight:
					return "bottom-right";
				default:
					return "top-left";
			}
		}

		///<summary>Reads a mask file. See ParseMasks.</summary>
		public static List<UiMaskRow> ReadMasks(string path) {
			return ParseMasks(File.ReadAllText(path),Path.GetFileName(path));
		}

		///<summary>The rows of a mask file: tab-separated picture, size (* or WxH), anchor (top-left, top-right, bottom-left, bottom-right), x, y, width,
		///height, what. Empty lines, lines that start with # and a header line whose first field is "picture" are skipped. A row that does not have
		///these eight fields, with whole numbers (x and y 0 or more, width and height 1 or more) and a known anchor, throws a FormatException that names
		///its line.</summary>
		public static List<UiMaskRow> ParseMasks(string text,string sourceName) {
			List<UiMaskRow> listMaskRows=new List<UiMaskRow>();
			string[] arrayLines=(text??"").Replace("\r\n","\n").Split('\n');
			for(int i=0;i<arrayLines.Length;i++) {
				string line=arrayLines[i].TrimEnd('\r');
				if(line.Trim()=="" || line.TrimStart().StartsWith("#",StringComparison.Ordinal)) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				if(arrayFields[0].Trim().ToLowerInvariant()=="picture") {
					continue;
				}
				string where=sourceName+" line "+(i+1)+": ";
				if(arrayFields.Length!=8) {
					throw new FormatException(where+"8 tab-separated fields expected (picture, size, anchor, x, y, width, height, what), found "+arrayFields.Length+".");
				}
				UiMaskRow uiMaskRow=new UiMaskRow();
				uiMaskRow.LineNumber=i+1;
				uiMaskRow.Picture=NormalizePicture(arrayFields[0]);
				if(uiMaskRow.Picture=="") {
					throw new FormatException(where+"no picture name.");
				}
				string size=arrayFields[1].Trim();
				if(size!="*") {
					Match match=Regex.Match(size,@"^(\d+)x(\d+)$");
					if(!match.Success) {
						throw new FormatException(where+"size is * or WIDTHxHEIGHT, like 644x444: "+size);
					}
					uiMaskRow.SizeWidth=ParseInt(match.Groups[1].Value,1,where+"size");
					uiMaskRow.SizeHeight=ParseInt(match.Groups[2].Value,1,where+"size");
				}
				switch(arrayFields[2].Trim().ToLowerInvariant()) {
					case "top-left":
						uiMaskRow.Anchor=UiMaskAnchor.TopLeft;
						break;
					case "top-right":
						uiMaskRow.Anchor=UiMaskAnchor.TopRight;
						break;
					case "bottom-left":
						uiMaskRow.Anchor=UiMaskAnchor.BottomLeft;
						break;
					case "bottom-right":
						uiMaskRow.Anchor=UiMaskAnchor.BottomRight;
						break;
					default:
						throw new FormatException(where+"anchor is top-left, top-right, bottom-left or bottom-right: "+arrayFields[2]);
				}
				uiMaskRow.X=ParseInt(arrayFields[3],0,where+"x");
				uiMaskRow.Y=ParseInt(arrayFields[4],0,where+"y");
				uiMaskRow.Width=ParseInt(arrayFields[5],1,where+"width");
				uiMaskRow.Height=ParseInt(arrayFields[6],1,where+"height");
				uiMaskRow.What=arrayFields[7].Trim();
				if(uiMaskRow.What=="") {
					throw new FormatException(where+"say what the rectangle hides (clock, date, database, ...).");
				}
				listMaskRows.Add(uiMaskRow);
			}
			return listMaskRows;
		}

		private static int ParseInt(string text,int min,string what) {
			int value;
			if(!int.TryParse(text.Trim(),NumberStyles.None,CultureInfo.InvariantCulture,out value) || value<min) {
				throw new FormatException(what+" is a whole number of "+min+" or more: "+text);
			}
			return value;
		}

		///<summary>The mask rows that apply to the picture: its name matches and its size is the row's size (or the row's size is *).</summary>
		public static List<UiMaskRow> GetMasksFor(IList<UiMaskRow> listMaskRows,string picture,int width,int height) {
			List<UiMaskRow> listApplied=new List<UiMaskRow>();
			if(listMaskRows==null) {
				return listApplied;
			}
			foreach(UiMaskRow uiMaskRow in listMaskRows) {
				if(IsMatch(uiMaskRow.Picture,picture) && uiMaskRow.AppliesToSize(width,height)) {
					listApplied.Add(uiMaskRow);
				}
			}
			return listApplied;
		}

		///<summary>The mask rows whose name matches the picture but whose size does not: a sign that a window changed size and the row needs a look.</summary>
		public static List<UiMaskRow> GetMasksWrongSize(IList<UiMaskRow> listMaskRows,string picture,int width,int height) {
			List<UiMaskRow> listWrong=new List<UiMaskRow>();
			if(listMaskRows==null) {
				return listWrong;
			}
			foreach(UiMaskRow uiMaskRow in listMaskRows) {
				if(IsMatch(uiMaskRow.Picture,picture) && !uiMaskRow.AppliesToSize(width,height)) {
					listWrong.Add(uiMaskRow);
				}
			}
			return listWrong;
		}

		///<summary>The rectangle of a mask row in a picture of this size, clipped to the picture (empty when the row lies outside it).</summary>
		public static Rectangle GetRectangle(UiMaskRow uiMaskRow,int width,int height) {
			int left=uiMaskRow.X;
			int top=uiMaskRow.Y;
			if(uiMaskRow.Anchor==UiMaskAnchor.TopRight || uiMaskRow.Anchor==UiMaskAnchor.BottomRight) {
				left=width-uiMaskRow.X-uiMaskRow.Width;
			}
			if(uiMaskRow.Anchor==UiMaskAnchor.BottomLeft || uiMaskRow.Anchor==UiMaskAnchor.BottomRight) {
				top=height-uiMaskRow.Y-uiMaskRow.Height;
			}
			Rectangle rectangle=Rectangle.Intersect(new Rectangle(left,top,uiMaskRow.Width,uiMaskRow.Height),new Rectangle(0,0,width,height));
			if(rectangle.Width<=0 || rectangle.Height<=0) {
				return Rectangle.Empty;
			}
			return rectangle;
		}

		///<summary>For each pixel of a picture of this size, true when a mask row that applies to the picture covers it.</summary>
		public static bool[] GetMaskedPixels(IList<UiMaskRow> listMaskRows,string picture,int width,int height) {
			bool[] arrayMasked=new bool[width*height];
			foreach(UiMaskRow uiMaskRow in GetMasksFor(listMaskRows,picture,width,height)) {
				Rectangle rectangle=GetRectangle(uiMaskRow,width,height);
				for(int y=rectangle.Top;y<rectangle.Bottom;y++) {
					for(int x=rectangle.Left;x<rectangle.Right;x++) {
						arrayMasked[y*width+x]=true;
					}
				}
			}
			return arrayMasked;
		}
		#endregion Masks

		#region The rule: masked pixel SHA-256
		///<summary>THE RULE. The masked pixel SHA-256 of a picture: its pixels as 32-bit BGRA, row by row with no padding (GetPixels), every pixel that
		///a mask row covers (the rows of listMaskRows that apply to this picture's name and size) set to MaskedByte in all four bytes, then the SHA-256
		///of those bytes in lower-case hex. picture is the name the masks match: the path relative to the picture folder (for the harness's flat
		///folder, the file name). The size is not part of the hash; approved.tsv keeps it in its own column.</summary>
		public static string GetMaskedPixelSha256(UiPixels uiPixels,string picture,IList<UiMaskRow> listMaskRows) {
			byte[] arrayBytes=(byte[])uiPixels.Bgra.Clone();
			bool[] arrayMasked=GetMaskedPixels(listMaskRows,picture,uiPixels.Width,uiPixels.Height);
			for(int i=0;i<arrayMasked.Length;i++) {
				if(arrayMasked[i]) {
					arrayBytes[i*4]=MaskedByte;
					arrayBytes[i*4+1]=MaskedByte;
					arrayBytes[i*4+2]=MaskedByte;
					arrayBytes[i*4+3]=MaskedByte;
				}
			}
			return Sha256Hex(arrayBytes);
		}

		///<summary>THE RULE for a bitmap in hand (the harness's check U0). See GetMaskedPixelSha256(UiPixels,...).</summary>
		public static string GetMaskedPixelSha256(Bitmap bitmap,string picture,IList<UiMaskRow> listMaskRows) {
			return GetMaskedPixelSha256(GetPixels(bitmap),picture,listMaskRows);
		}

		///<summary>THE RULE for a picture file. See GetMaskedPixelSha256(UiPixels,...).</summary>
		public static string GetMaskedPixelSha256(string path,string picture,IList<UiMaskRow> listMaskRows) {
			return GetMaskedPixelSha256(LoadPixels(path),picture,listMaskRows);
		}

		///<summary>Lower-case hex SHA-256 of the bytes.</summary>
		public static string Sha256Hex(byte[] arrayBytes) {
			using(SHA256 sha256=SHA256.Create()) {
				byte[] arrayHash=sha256.ComputeHash(arrayBytes);
				StringBuilder stringBuilder=new StringBuilder(64);
				foreach(byte b in arrayHash) {
					stringBuilder.Append(b.ToString("x2",CultureInfo.InvariantCulture));
				}
				return stringBuilder.ToString();
			}
		}
		#endregion The rule: masked pixel SHA-256

		#region The comparison and the difference picture
		///<summary>Compares the after picture with the before picture over the larger of the two areas. A pixel that a mask covers in either picture
		///is not compared; a pixel only one picture has counts as changed; any other pixel counts as changed when a B, G, R or A value differs by more
		///than threshold (0: any difference). When pathDiff is given and a pixel changed, writes the difference picture there: the after picture faded
		///to grey, changed pixels red, the area only one picture has magenta, masked areas hatched in blue (the look of the WINCHK-5 pairs page, plus
		///the masks).</summary>
		public static UiPictureCompareResult Compare(string pathBefore,string pathAfter,string picture,IList<UiMaskRow> listMaskRows,int threshold,string pathDiff) {
			UiPixels uiPixelsBefore=LoadPixels(pathBefore);
			UiPixels uiPixelsAfter=LoadPixels(pathAfter);
			return Compare(uiPixelsBefore,uiPixelsAfter,picture,listMaskRows,threshold,pathDiff);
		}

		///<summary>See Compare(string,string,...).</summary>
		public static UiPictureCompareResult Compare(UiPixels uiPixelsBefore,UiPixels uiPixelsAfter,string picture,IList<UiMaskRow> listMaskRows,int threshold,
			string pathDiff)
		{
			UiPictureCompareResult result=new UiPictureCompareResult();
			result.Sha256Before=GetMaskedPixelSha256(uiPixelsBefore,picture,listMaskRows);
			result.Sha256After=GetMaskedPixelSha256(uiPixelsAfter,picture,listMaskRows);
			result.ListMasksApplied=GetMasksFor(listMaskRows,picture,uiPixelsAfter.Width,uiPixelsAfter.Height);
			int width=Math.Max(uiPixelsBefore.Width,uiPixelsAfter.Width);
			int height=Math.Max(uiPixelsBefore.Height,uiPixelsAfter.Height);
			result.Width=width;
			result.Height=height;
			bool[] arrayMaskedBefore=GetMaskedPixels(listMaskRows,picture,uiPixelsBefore.Width,uiPixelsBefore.Height);
			bool[] arrayMaskedAfter=GetMaskedPixels(listMaskRows,picture,uiPixelsAfter.Width,uiPixelsAfter.Height);
			byte[] arrayDiff=new byte[width*height*4];
			int xMin=int.MaxValue;
			int yMin=int.MaxValue;
			int xMax=-1;
			int yMax=-1;
			for(int y=0;y<height;y++) {
				for(int x=0;x<width;x++) {
					bool isInBefore=x<uiPixelsBefore.Width && y<uiPixelsBefore.Height;
					bool isInAfter=x<uiPixelsAfter.Width && y<uiPixelsAfter.Height;
					int indexBefore=isInBefore?(y*uiPixelsBefore.Width+x):-1;
					int indexAfter=isInAfter?(y*uiPixelsAfter.Width+x):-1;
					int indexDiff=(y*width+x)*4;
					bool isMasked=(isInBefore && arrayMaskedBefore[indexBefore]) || (isInAfter && arrayMaskedAfter[indexAfter]);
					if(isMasked) {
						result.CountMasked++;
						bool isHatch=(x+y)%6<2;
						SetBgr(arrayDiff,indexDiff,isHatch?(byte)190:(byte)235,isHatch?(byte)140:(byte)205,isHatch?(byte)90:(byte)160);
						continue;
					}
					result.CountCompared++;
					bool isChanged;
					if(!isInBefore || !isInAfter) {
						isChanged=true;
						SetBgr(arrayDiff,indexDiff,255,0,255);
					}
					else {
						isChanged=false;
						for(int k=0;k<4;k++) {
							if(Math.Abs(uiPixelsBefore.Bgra[indexBefore*4+k]-uiPixelsAfter.Bgra[indexAfter*4+k])>threshold) {
								isChanged=true;
								break;
							}
						}
						if(isChanged) {
							SetBgr(arrayDiff,indexDiff,0,0,255);
						}
						else {
							byte[] arrayAfter=uiPixelsAfter.Bgra;
							//Grey at half contrast, so the red stands out (ChartImageCompare.MakeDiffImage's fade).
							byte grey=(byte)(128+(arrayAfter[indexAfter*4]*114+arrayAfter[indexAfter*4+1]*587+arrayAfter[indexAfter*4+2]*299)/1000/2);
							SetBgr(arrayDiff,indexDiff,grey,grey,grey);
						}
					}
					if(isChanged) {
						result.CountChanged++;
						xMin=Math.Min(xMin,x);
						yMin=Math.Min(yMin,y);
						xMax=Math.Max(xMax,x);
						yMax=Math.Max(yMax,y);
					}
				}
			}
			if(result.CountChanged>0) {
				result.BoundsChanged=new Rectangle(xMin,yMin,xMax-xMin+1,yMax-yMin+1);
			}
			if(!string.IsNullOrEmpty(pathDiff) && result.CountChanged>0) {
				SavePng(arrayDiff,width,height,pathDiff);
			}
			return result;
		}

		private static void SetBgr(byte[] arrayBytes,int index,byte blue,byte green,byte red) {
			arrayBytes[index]=blue;
			arrayBytes[index+1]=green;
			arrayBytes[index+2]=red;
			arrayBytes[index+3]=255;
		}

		///<summary>Writes 32-bit BGRA pixels as a PNG, making its folder.</summary>
		public static void SavePng(byte[] arrayBgra,int width,int height,string path) {
			string folder=Path.GetDirectoryName(Path.GetFullPath(path));
			Directory.CreateDirectory(folder);
			using(Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb)) {
				BitmapData bitmapData=bitmap.LockBits(new Rectangle(0,0,width,height),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
				try {
					for(int y=0;y<height;y++) {
						Marshal.Copy(arrayBgra,y*width*4,new IntPtr(bitmapData.Scan0.ToInt64()+(long)y*bitmapData.Stride),width*4);
					}
				}
				finally {
					bitmap.UnlockBits(bitmapData);
				}
				bitmap.Save(path,ImageFormat.Png);
			}
		}
		#endregion The comparison and the difference picture

		#region approved.tsv
		///<summary>Reads approved.tsv. See ParseApproved.</summary>
		public static List<UiApprovedRow> ReadApproved(string path) {
			return ParseApproved(File.ReadAllText(path),Path.GetFileName(path));
		}

		///<summary>The rows of approved.tsv: tab-separated picture, size (WxH), masked pixel SHA-256 (64 lower-case hex), gallery, commit, date
		///(yyyy-MM-dd). The header line (ApprovedHeader), empty lines and lines that start with # are skipped. A malformed row throws a FormatException
		///that names its line.</summary>
		public static List<UiApprovedRow> ParseApproved(string text,string sourceName) {
			List<UiApprovedRow> listApprovedRows=new List<UiApprovedRow>();
			string[] arrayLines=(text??"").Replace("\r\n","\n").Split('\n');
			for(int i=0;i<arrayLines.Length;i++) {
				string line=arrayLines[i].TrimEnd('\r');
				if(line.Trim()=="" || line.TrimStart().StartsWith("#",StringComparison.Ordinal) || line==ApprovedHeader) {
					continue;
				}
				string where=sourceName+" line "+(i+1)+": ";
				string[] arrayFields=line.Split('\t');
				if(arrayFields.Length!=6) {
					throw new FormatException(where+"6 tab-separated fields expected ("+ApprovedHeader.Replace("\t",", ")+"), found "+arrayFields.Length+".");
				}
				UiApprovedRow uiApprovedRow=new UiApprovedRow();
				uiApprovedRow.LineNumber=i+1;
				uiApprovedRow.Picture=NormalizePicture(arrayFields[0]);
				Match matchSize=Regex.Match(arrayFields[1].Trim(),@"^(\d+)x(\d+)$");
				if(uiApprovedRow.Picture=="" || !matchSize.Success) {
					throw new FormatException(where+"a picture name and a size like 644x444 expected.");
				}
				uiApprovedRow.Width=int.Parse(matchSize.Groups[1].Value,CultureInfo.InvariantCulture);
				uiApprovedRow.Height=int.Parse(matchSize.Groups[2].Value,CultureInfo.InvariantCulture);
				uiApprovedRow.Sha256=arrayFields[2].Trim();
				if(!Regex.IsMatch(uiApprovedRow.Sha256,"^[0-9a-f]{64}$")) {
					throw new FormatException(where+"the SHA-256 is 64 lower-case hex digits: "+uiApprovedRow.Sha256);
				}
				uiApprovedRow.Gallery=arrayFields[3].Trim();
				uiApprovedRow.Commit=arrayFields[4].Trim();
				uiApprovedRow.Date=arrayFields[5].Trim();
				if(!Regex.IsMatch(uiApprovedRow.Date,@"^\d{4}-\d{2}-\d{2}$")) {
					throw new FormatException(where+"the date is yyyy-MM-dd: "+uiApprovedRow.Date);
				}
				listApprovedRows.Add(uiApprovedRow);
			}
			return listApprovedRows;
		}

		///<summary>The owner's latest approval of the picture (the last row with its name), or null when it has none. The caller compares its size and
		///SHA-256 with the picture's.</summary>
		public static UiApprovedRow FindApproved(IList<UiApprovedRow> listApprovedRows,string picture) {
			string name=NormalizePicture(picture);
			UiApprovedRow uiApprovedRowLast=null;
			if(listApprovedRows==null) {
				return null;
			}
			foreach(UiApprovedRow uiApprovedRow in listApprovedRows) {
				if(string.Equals(uiApprovedRow.Picture,name,StringComparison.OrdinalIgnoreCase)) {
					uiApprovedRowLast=uiApprovedRow;
				}
			}
			return uiApprovedRowLast;
		}

		///<summary>True when the picture of this size and masked pixel SHA-256 is the one the owner last approved (the check U0's "unchanged").</summary>
		public static bool IsApproved(IList<UiApprovedRow> listApprovedRows,string picture,int width,int height,string sha256) {
			UiApprovedRow uiApprovedRow=FindApproved(listApprovedRows,picture);
			return uiApprovedRow!=null && uiApprovedRow.Width==width && uiApprovedRow.Height==height && uiApprovedRow.Sha256==sha256;
		}
		#endregion approved.tsv
	}
}
