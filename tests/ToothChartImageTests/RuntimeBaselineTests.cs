// The chart's pictures on .NET 10 x64 (package S3b-5W, docs/port-notes/S3b-5W.md; the S3b-5 item of docs/specs/wpf-tooth-chart-plan.md's 3b
// section): the full .NET 10 x64 sets beside the .NET Framework ones (Baselines\wpf-net10-x64, Baselines\simple2d-net10-x64 and the host
// tests' Baselines\net10-x64), and gallery G5 (plan section 8): every .NET 10 x64 picture against its .NET Framework 4.8 x64 record (the x86
// picture where TC-10 kept no x64 copy, because the 64-bit .NET Framework draws it within tolerance), with the image tests' own tolerance
// (ChartImageCompare). The gallery reads the committed pictures only: it draws nothing, starts nothing and shows no window.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToothChartImageTests {
	[TestClass]
	public class RuntimeBaselineTests {
		///<summary>The folder gallery G5 is written to (new, empty, or an earlier gallery; any other folder is refused, WpfGallery.PrepareFolder).
		///The zip is written beside it, as &lt;folder&gt;.zip.</summary>
		public const string EnvG5Gallery="OPMS_G5_GALLERY";
		///<summary>An HTML fragment for the top of G5: what changed and why, in plain words for the owner (written after the comparison).</summary>
		public const string EnvG5Summary="OPMS_G5_SUMMARY";
		///<summary>The branch and commit the gallery shows, for its header.</summary>
		public const string EnvG5Commit="OPMS_G5_COMMIT";

		///<summary>The pictures of each set: TC-1 to TC-8 recorded them on .NET Framework 4.8 x86.</summary>
		public const int CountWpfPictures=326;
		public const int CountSimple2DPictures=35;
		public const int CountHostPictures=9;

		///<summary>tests\ToothChartHostTests\Baselines: the host tests' pictures (.NET Framework 4.8 x86, TC-8), and in net10-x64 their .NET 10 x64 set.</summary>
		public static string HostBaselines {
			get {
				return Path.Combine(TestPaths.RepoRoot,"tests","ToothChartHostTests","Baselines");
			}
		}

		[TestMethod]
		public void WpfRendersInThisSession() {
			//The picture tests' precondition (WINCHK-3: a locked or disconnected remote desktop session renders nothing, not even a red square). Not a
			//failure without it: the picture tests then fail on their own, and this result says why.
			bool canRender=StaRunner.Run(() => CanRenderWpf());
			if(!canRender) {
				Assert.Inconclusive("WPF's RenderTargetBitmap draws nothing in this session (locked or disconnected): the picture tests cannot pass here.");
			}
		}

		///<summary>Whether WPF's RenderTargetBitmap draws at all in this session: a red square rendered with it and read back (WindowChecksTests'
		///Shot.CanRenderWpf). Call on an STA thread.</summary>
		public static bool CanRenderWpf() {
			try {
				System.Windows.Media.Imaging.RenderTargetBitmap renderTargetBitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(8,8,96,96,
					System.Windows.Media.PixelFormats.Pbgra32);
				System.Windows.Media.DrawingVisual drawingVisual=new System.Windows.Media.DrawingVisual();
				using(System.Windows.Media.DrawingContext drawingContext=drawingVisual.RenderOpen()) {
					drawingContext.DrawRectangle(System.Windows.Media.Brushes.Red,null,new System.Windows.Rect(0,0,8,8));
				}
				renderTargetBitmap.Render(drawingVisual);
				byte[] arrayPixels=new byte[8*8*4];
				renderTargetBitmap.CopyPixels(arrayPixels,8*4,0);
				int index=(4*8+4)*4;
				return arrayPixels[index+2]>200 && arrayPixels[index+3]>200;
			}
			catch(Exception) {
				return false;
			}
		}

		[TestMethod]
		public void Net10X64Sets_HoldEveryPicture_RecordedOnNet10X64() {
			//S3b-5W records the .NET 10 x64 sets in full: every picture of each .NET Framework set has its copy at the same path, with its .txt, recorded
			//by a 64-bit .NET 10 process, and nothing else is there. The .NET Framework sets stay as they are.
			List<string> listFailures=new List<string>();
			CheckSet(WpfCharts.BaselinesWpf,WpfCharts.BaselinesWpfNet10X64,true,CountWpfPictures,listFailures);
			CheckSet(Simple2DImageTests.Baselines2D,WpfCharts.Baselines2DNet10X64,true,CountSimple2DPictures,listFailures);
			CheckSet(HostBaselines,Path.Combine(HostBaselines,"net10-x64"),false,CountHostPictures,listFailures);
			Assert.AreEqual(0,listFailures.Count,string.Join("\r\n",listFailures));
		}

		private static void CheckSet(string folderNet48,string folderNet10,bool isRecursive,int countExpected,List<string> listFailures) {
			List<string> listNet48=GetPictures(folderNet48,isRecursive);
			List<string> listNet10=GetPictures(folderNet10,isRecursive);
			if(listNet48.Count!=countExpected) {
				listFailures.Add(folderNet48+": "+listNet48.Count+" pictures, not "+countExpected+".");
			}
			foreach(string relative in listNet48.Except(listNet10)) {
				listFailures.Add(folderNet10+": no copy of "+relative+".");
			}
			foreach(string relative in listNet10.Except(listNet48)) {
				listFailures.Add(folderNet10+": "+relative+" is not in "+folderNet48+".");
			}
			foreach(string relative in listNet10.Intersect(listNet48)) {
				string pathText=Path.ChangeExtension(Path.Combine(folderNet10,relative),".txt");
				if(!File.Exists(pathText)) {
					listFailures.Add(pathText+": missing.");
					continue;
				}
				string lineFramework=File.ReadAllLines(pathText).FirstOrDefault(x => x.StartsWith("Framework: "));
				if(lineFramework==null || !lineFramework.StartsWith("Framework: .NET 10",StringComparison.Ordinal) || !lineFramework.EndsWith(", x64",StringComparison.Ordinal)) {
					listFailures.Add(pathText+": not recorded on .NET 10 x64 ("+(lineFramework??"no Framework line")+").");
				}
				if(LegacyComparison.ReadPixelsSha256(pathText)==null) {
					listFailures.Add(pathText+": no Pixels SHA-256.");
				}
			}
		}

		///<summary>The .png files under folder, as paths relative to it with backslashes, in ordinal order. The host set's net10-x64 sub folder is not
		///part of the .NET Framework set (isRecursive false).</summary>
		private static List<string> GetPictures(string folder,bool isRecursive) {
			if(!Directory.Exists(folder)) {
				return new List<string>();
			}
			return Directory.GetFiles(folder,"*.png",isRecursive?SearchOption.AllDirectories:SearchOption.TopDirectoryOnly)
				.Select(x => x.Substring(folder.TrimEnd('\\').Length+1)).OrderBy(x => x,StringComparer.Ordinal).ToList();
		}

		#region Gallery G5
		[TestMethod]
		public void WriteG5Gallery() {
			string folder=Environment.GetEnvironmentVariable(EnvG5Gallery);
			if(string.IsNullOrWhiteSpace(folder)) {
				Assert.Inconclusive("Set "+EnvG5Gallery+" to a folder to write gallery G5 (.NET Framework 4.8 x64 against .NET 10 x64).");
			}
			folder=Path.GetFullPath(folder);
			List<G5Picture> listPictures=CompareAll();
			WpfGallery.PrepareFolder(folder);//refuses a folder that is not a gallery
			StaRunner.Run(() => {
				foreach(G5Picture picture in listPictures) {
					WritePictures(picture,folder);
				}
			});
			string summary=null;
			string pathSummary=Environment.GetEnvironmentVariable(EnvG5Summary);
			if(!string.IsNullOrWhiteSpace(pathSummary) && File.Exists(pathSummary)) {
				summary=File.ReadAllText(pathSummary);
			}
			File.WriteAllText(Path.Combine(folder,"index.html"),GetHtml(listPictures,summary,Environment.GetEnvironmentVariable(EnvG5Commit)),new UTF8Encoding(false));
			File.WriteAllLines(Path.Combine(folder,"comparison.tsv"),GetTsv(listPictures),new UTF8Encoding(false));
			File.WriteAllText(Path.Combine(folder,"summary.txt"),GetSummaryText(listPictures),new UTF8Encoding(false));
			string pathZip=folder.TrimEnd('\\')+".zip";
			WriteZip(folder,pathZip);
			Console.WriteLine(GetSummaryText(listPictures));
			Console.WriteLine("Gallery G5 written to "+folder+" and "+pathZip+".");
		}

		///<summary>Every .NET 10 x64 picture against its .NET Framework 4.8 x64 record, WPF first, then Simple 2D, then the hosts.</summary>
		public static List<G5Picture> CompareAll() {
			List<G5Picture> listPictures=new List<G5Picture>();
			foreach(string relative in GetPictures(WpfCharts.BaselinesWpf,true)) {
				string pathNet48=Path.Combine(WpfCharts.BaselinesWpf,relative);
				string pathRecord=WpfCharts.GetNet48X64Record(pathNet48);
				listPictures.Add(Compare("WPF chart",relative,pathRecord,pathRecord==pathNet48?"wpf-net48 (x86; TC-10: x64 within tolerance)":"wpf-net48-x64",
					Path.Combine(WpfCharts.BaselinesWpfNet10X64,relative)));
			}
			foreach(string relative in GetPictures(Simple2DImageTests.Baselines2D,true)) {
				string pathNet48=Path.Combine(Simple2DImageTests.Baselines2D,relative);
				string pathRecord=WpfCharts.GetNet48X64Record(pathNet48);
				listPictures.Add(Compare("Simple 2D chart",relative,pathRecord,pathRecord==pathNet48?"simple2d-net48 (x86; TC-10: x64 within tolerance)"
					:"simple2d-net48-x64",Path.Combine(WpfCharts.Baselines2DNet10X64,relative)));
			}
			foreach(string relative in GetPictures(HostBaselines,false)) {
				listPictures.Add(Compare("Program host",relative,Path.Combine(HostBaselines,relative),"host Baselines (x86 only: the host tests were x86 until S3b-5)",
					Path.Combine(HostBaselines,"net10-x64",relative)));
			}
			return listPictures;
		}

		private static G5Picture Compare(string engine,string relative,string pathNet48,string recordName,string pathNet10) {
			G5Picture picture=new G5Picture();
			picture.Engine=engine;
			picture.Relative=relative.Replace('\\','/');
			picture.PathNet48=pathNet48;
			picture.PathNet10=pathNet10;
			picture.RecordName=recordName;
			string[] arrayParts=picture.Relative.Split('/');
			picture.SceneName=Path.GetFileNameWithoutExtension(arrayParts[arrayParts.Length-1]);
			picture.SizeFolder=arrayParts.Length>1?arrayParts[0]:"";
			if(engine=="Program host") {
				picture.Group="Program hosts";
				picture.Description=GetHostDescription(picture.SceneName);
			}
			else {
				ChartScene scene=ChartScenes.Get(picture.SceneName);
				picture.Group=scene.Group;
				picture.Description=scene.Description;
				if(picture.SizeFolder=="perio-print") {
					picture.Description="The print of the graphical perio chart, around its 649x696 picture. "+picture.Description;
				}
				else if(picture.SizeFolder=="perio-save") {
					picture.Description="The saved image (Save to Images) of the graphical perio chart. "+picture.Description;
				}
			}
			if(!File.Exists(pathNet10)) {
				picture.Failure="no .NET 10 x64 picture";
				return picture;
			}
			using Bitmap bitmapNet48=WpfImageTests.LoadBitmap(pathNet48);
			using Bitmap bitmapNet10=WpfImageTests.LoadBitmap(pathNet10);
			picture.Width=bitmapNet10.Width;
			picture.Height=bitmapNet10.Height;
			picture.Result=ChartImageCompare.Compare(bitmapNet48,bitmapNet10);
			if(!picture.Result.IsSameSize) {
				picture.Failure="the sizes differ ("+bitmapNet48.Width+"x"+bitmapNet48.Height+" and "+bitmapNet10.Width+"x"+bitmapNet10.Height+")";
				return picture;
			}
			byte[] array48=ChartImageCompare.GetPixelsArgb(bitmapNet48);
			byte[] array10=ChartImageCompare.GetPixelsArgb(bitmapNet10);
			int xMin=int.MaxValue,yMin=int.MaxValue,xMax=-1,yMax=-1;
			for(int i=0;i<array48.Length/4;i++) {
				int diff=0;
				for(int c=0;c<4;c++) {
					diff=Math.Max(diff,Math.Abs(array48[i*4+c]-array10[i*4+c]));
				}
				if(diff==0) {
					continue;
				}
				picture.CountChanged++;
				picture.MaxChannelDifference=Math.Max(picture.MaxChannelDifference,diff);
				int x=i%picture.Width;
				int y=i/picture.Width;
				xMin=Math.Min(xMin,x);
				yMin=Math.Min(yMin,y);
				xMax=Math.Max(xMax,x);
				yMax=Math.Max(yMax,y);
			}
			if(xMax>=0) {
				picture.BoundsChanged=Rectangle.FromLTRB(xMin,yMin,xMax+1,yMax+1);
			}
			return picture;
		}

		private static string GetHostDescription(string name) {
			switch(name) {
				case "big-chart": return "The big chart window (FormToothChartingBig), 926x858.";
				case "dashboard": return "The Patient Dashboard's chart, made on an MTA worker thread (DashToothChart).";
				case "perio-chart": return "The graphical perio chart's print (FormPerioGraphical).";
				case "perio-save": return "The graphical perio chart's saved image (FormPerioGraphical, Save to Images).";
				case "referral-letter": return "The referral letter's chart picture (the automation sheet).";
				case "sheet-editor-preview": return "The sheet editor's preview of a sheet's chart (FormSheetDefEdit).";
				case "sheet-simple2d": return "A sheet's chart on a computer set to the Simple 2D chart (O-30).";
				case "sheet": return "A sheet's chart through SheetPrinting.GetToothChartHelper.";
				case "treatplan-sheet": return "The Treatment Plan sheet printout's chart, printout colours.";
			}
			return "A host's chart picture.";
		}

		///<summary>Copies both pictures into the gallery and writes the difference image: the .NET Framework picture in grey at half contrast, pixels that
		///changed within the tolerance (no channel more than 8 of 255 apart) in orange, pixels beyond it in red.</summary>
		private static void WritePictures(G5Picture picture,string folder) {
			string folderEngine=picture.Engine=="WPF chart"?"wpf":(picture.Engine=="Simple 2D chart"?"simple2d":"host");
			picture.PathGalleryNet48="net48-x64/"+folderEngine+"/"+picture.Relative;
			picture.PathGalleryNet10="net10-x64/"+folderEngine+"/"+picture.Relative;
			CopyInto(picture.PathNet48,folder,picture.PathGalleryNet48);
			if(picture.Failure!=null) {
				return;
			}
			CopyInto(picture.PathNet10,folder,picture.PathGalleryNet10);
			if(picture.CountChanged==0) {
				return;
			}
			picture.PathGalleryDiff="diff/"+folderEngine+"/"+picture.Relative;
			string pathDiff=Path.Combine(folder,picture.PathGalleryDiff.Replace('/','\\'));
			Directory.CreateDirectory(Path.GetDirectoryName(pathDiff));
			using Bitmap bitmapNet48=WpfImageTests.LoadBitmap(picture.PathNet48);
			using Bitmap bitmapNet10=WpfImageTests.LoadBitmap(picture.PathNet10);
			using Bitmap bitmapDiff=MakeDiffImage(bitmapNet48,bitmapNet10);
			PngWriter.Save(bitmapDiff,pathDiff);
		}

		private static void CopyInto(string pathSource,string folder,string relative) {
			string pathTarget=Path.Combine(folder,relative.Replace('/','\\'));
			Directory.CreateDirectory(Path.GetDirectoryName(pathTarget));
			File.Copy(pathSource,pathTarget,true);
		}

		public static Bitmap MakeDiffImage(Bitmap bitmapExpected,Bitmap bitmapActual) {
			int width=bitmapExpected.Width;
			int height=bitmapExpected.Height;
			byte[] arrayExpected=ChartImageCompare.GetPixelsArgb(bitmapExpected);
			byte[] arrayActual=ChartImageCompare.GetPixelsArgb(bitmapActual);
			byte[] arrayDiff=new byte[arrayExpected.Length];
			for(int i=0;i<arrayExpected.Length/4;i++) {
				int diff=0;
				for(int c=0;c<4;c++) {
					diff=Math.Max(diff,Math.Abs(arrayExpected[i*4+c]-arrayActual[i*4+c]));
				}
				if(diff>ChartImageCompare.ChannelTolerance) {
					arrayDiff[i*4]=0;
					arrayDiff[i*4+1]=0;
					arrayDiff[i*4+2]=255;
				}
				else if(diff>0) {
					arrayDiff[i*4]=0;
					arrayDiff[i*4+1]=165;
					arrayDiff[i*4+2]=255;
				}
				else {
					byte grey=(byte)(128+(arrayExpected[i*4]*114+arrayExpected[i*4+1]*587+arrayExpected[i*4+2]*299)/1000/2);
					arrayDiff[i*4]=grey;
					arrayDiff[i*4+1]=grey;
					arrayDiff[i*4+2]=grey;
				}
				arrayDiff[i*4+3]=255;
			}
			Bitmap bitmapDiff=new Bitmap(width,height,PixelFormat.Format32bppArgb);
			BitmapData bitmapData=bitmapDiff.LockBits(new Rectangle(0,0,width,height),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
			try {
				for(int y=0;y<height;y++) {
					Marshal.Copy(arrayDiff,y*width*4,bitmapData.Scan0+y*bitmapData.Stride,width*4);
				}
			}
			finally {
				bitmapDiff.UnlockBits(bitmapData);
			}
			return bitmapDiff;
		}

		public static string GetSummaryText(List<G5Picture> listPictures) {
			StringBuilder sb=new StringBuilder();
			sb.AppendLine("G5: "+listPictures.Count+" pictures; identical "+listPictures.Count(x => x.Kind==G5Kind.Identical)+", within tolerance "
				+listPictures.Count(x => x.Kind==G5Kind.WithinTolerance)+", beyond tolerance "+listPictures.Count(x => x.Kind==G5Kind.Changed)
				+(listPictures.Any(x => x.Kind==G5Kind.Failed)?", failed "+listPictures.Count(x => x.Kind==G5Kind.Failed):"")+".");
			foreach(IGrouping<string,G5Picture> grouping in listPictures.GroupBy(x => x.Engine+" / "+x.Group)) {
				List<G5Picture> list=grouping.ToList();
				sb.AppendLine(grouping.Key+": "+list.Count+" pictures; identical "+list.Count(x => x.Kind==G5Kind.Identical)+", within "
					+list.Count(x => x.Kind==G5Kind.WithinTolerance)+", beyond "+list.Count(x => x.Kind==G5Kind.Changed)
					+"; changed pixels max "+list.Max(x => x.CountChanged)+", beyond-tolerance pixels max "+list.Max(x => x.Result?.CountDifferent??0)
					+", worst block max "+list.Max(x => x.Result?.CountDifferentInWorstBlock??0)+", largest channel difference "
					+list.Max(x => x.MaxChannelDifference)+".");
			}
			return sb.ToString();
		}

		public static List<string> GetTsv(List<G5Picture> listPictures) {
			List<string> listLines=new List<string>();
			listLines.Add("Engine\tPicture\tGroup\tRecord\tResult\tSize\tChanged pixels\tBeyond tolerance\tWorst 16x16 block\tWorst block at\tLargest channel difference\tBounding box of changes");
			foreach(G5Picture picture in GetOrdered(listPictures)) {
				listLines.Add(picture.Engine+"\t"+picture.Relative+"\t"+picture.Group+"\t"+picture.RecordName+"\t"+picture.GetKindText()+"\t"+picture.Width+"x"+picture.Height
					+"\t"+picture.CountChanged+"\t"+(picture.Result?.CountDifferent??0)+"\t"+(picture.Result?.CountDifferentInWorstBlock??0)+"\t"
					+(picture.Result==null?"":picture.Result.PointWorstBlock.X+","+picture.Result.PointWorstBlock.Y)+"\t"+picture.MaxChannelDifference+"\t"
					+picture.GetBoundsText());
			}
			return listLines;
		}

		///<summary>Beyond tolerance (most pixels first), then within tolerance (most changed pixels first), then identical; within each, by engine,
		///group, scene and size.</summary>
		private static List<G5Picture> GetOrdered(List<G5Picture> listPictures) {
			return listPictures.OrderBy(x => (int)x.Kind).ThenByDescending(x => x.Kind==G5Kind.Changed?(x.Result?.CountDifferent??0):x.CountChanged)
				.ThenBy(x => x.Engine,StringComparer.Ordinal).ThenBy(x => x.Group,StringComparer.Ordinal).ThenBy(x => x.Relative,StringComparer.Ordinal).ToList();
		}

		private static string GetHtml(List<G5Picture> listPictures,string summary,string commit) {
			StringBuilder sb=new StringBuilder();
			sb.Append("<!doctype html>\r\n<html lang=\"en\">\r\n<head>\r\n<meta charset=\"utf-8\">\r\n");
			sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\r\n");
			sb.Append("<title>Tooth chart gallery G5</title>\r\n<style>\r\n");
			sb.Append("body{font-family:Segoe UI,Arial,sans-serif;margin:16px;background:#fff;color:#111}\r\n");
			sb.Append("table{border-collapse:collapse}td,th{border:1px solid #bbb;padding:4px;vertical-align:top;font-size:12px;text-align:left}\r\n");
			sb.Append("img{display:block;max-width:420px;height:auto}.cap{color:#555}.name{font-weight:bold}.warn{color:#b00}.ok{color:#070}.mid{color:#a60}\r\n");
			sb.Append(".num{text-align:right;font-variant-numeric:tabular-nums}.sum td{font-size:13px}\r\n");
			sb.Append("</style>\r\n</head>\r\n<body>\r\n<h1>Tooth chart gallery G5: .NET Framework 4.8 against .NET 10 (64-bit)</h1>\r\n");
			sb.Append("<p>Stage 3b moves the program to .NET 10, 64-bit. This gallery shows every picture the chart tests keep, as the program draws it today "
				+"on .NET Framework 4.8 (64-bit) and as it draws it on .NET 10 (64-bit), with the difference between them. Every scene is synthetic: no patient "
				+"data. The host pictures come from a scratch database with synthetic patients only.</p>\r\n");
			if(!string.IsNullOrWhiteSpace(commit)) {
				sb.Append("<p class=\"cap\">Made "+DateTime.Now.ToString("yyyy-MM-dd HH:mm",CultureInfo.InvariantCulture)+" at "+WebUtility.HtmlEncode(commit)+".</p>\r\n");
			}
			int countIdentical=listPictures.Count(x => x.Kind==G5Kind.Identical);
			int countWithin=listPictures.Count(x => x.Kind==G5Kind.WithinTolerance);
			int countChanged=listPictures.Count(x => x.Kind==G5Kind.Changed);
			int countFailed=listPictures.Count(x => x.Kind==G5Kind.Failed);
			sb.Append("<h2>Summary</h2>\r\n<p><b>"+listPictures.Count+" pictures: "+countIdentical+" identical, "+countWithin+" within the tests' tolerance, "
				+countChanged+" beyond it"+(countFailed>0?", "+countFailed+" could not be compared":"")+".</b> The tolerance is the image tests' own "
				+"(plan section 7): a pixel counts as different when a colour channel differs by more than 8 of 255; a picture passes when at most 0.1% of its "
				+"pixels differ and no 16x16 block has more than 24 different pixels.</p>\r\n");
			if(!string.IsNullOrWhiteSpace(summary)) {
				sb.Append(summary+"\r\n");
			}
			sb.Append("<table class=\"sum\"><thead><tr><th>Engine</th><th>Group</th><th class=\"num\">Pictures</th><th class=\"num\">Identical</th>"
				+"<th class=\"num\">Within tolerance</th><th class=\"num\">Beyond tolerance</th><th class=\"num\">Most changed pixels</th>"
				+"<th class=\"num\">Most beyond tolerance</th><th class=\"num\">Largest channel difference</th></tr></thead><tbody>\r\n");
			foreach(IGrouping<string,G5Picture> grouping in listPictures.GroupBy(x => x.Engine+"\t"+x.Group)) {
				List<G5Picture> list=grouping.ToList();
				string[] arrayKey=grouping.Key.Split('\t');
				sb.Append("<tr><td>"+WebUtility.HtmlEncode(arrayKey[0])+"</td><td>"+WebUtility.HtmlEncode(arrayKey[1])+"</td><td class=\"num\">"+list.Count
					+"</td><td class=\"num\">"+list.Count(x => x.Kind==G5Kind.Identical)+"</td><td class=\"num\">"+list.Count(x => x.Kind==G5Kind.WithinTolerance)
					+"</td><td class=\"num\">"+list.Count(x => x.Kind==G5Kind.Changed)+"</td><td class=\"num\">"+list.Max(x => x.CountChanged)
					+"</td><td class=\"num\">"+list.Max(x => x.Result?.CountDifferent??0)+"</td><td class=\"num\">"+list.Max(x => x.MaxChannelDifference)+"</td></tr>\r\n");
			}
			sb.Append("</tbody></table>\r\n");
			sb.Append("<p class=\"cap\">Columns of the rows: the picture of .NET Framework 4.8 (64-bit: TC-10's x64 copy where it has one, otherwise the "
				+"32-bit picture, which the 64-bit .NET Framework draws within tolerance; the host pictures were only ever made in 32-bit), the picture of "
				+".NET 10 (64-bit), and the difference: the old picture in grey, pixels that changed within the tolerance in orange, pixels beyond it in red. "
				+"Numbers: changed pixels (any channel apart), pixels beyond the tolerance, the worst 16x16 block, the largest channel difference (of 255) "
				+"and the box around every changed pixel.</p>\r\n");
			sb.Append("<p><label>Group: <select id=\"group\"><option value=\"\">all</option>");
			foreach(string group in listPictures.Select(x => x.Group).Distinct().OrderBy(x => x,StringComparer.Ordinal)) {
				sb.Append("<option>"+WebUtility.HtmlEncode(group)+"</option>");
			}
			sb.Append("</select></label></p>\r\n");
			List<G5Picture> listOrdered=GetOrdered(listPictures);
			foreach(G5Kind kind in new[] { G5Kind.Failed,G5Kind.Changed,G5Kind.WithinTolerance,G5Kind.Identical }) {
				List<G5Picture> listKind=listOrdered.Where(x => x.Kind==kind).ToList();
				if(listKind.Count==0) {
					continue;
				}
				sb.Append("<h2>"+GetKindHeading(kind)+" ("+listKind.Count+")</h2>\r\n");
				sb.Append("<table><thead><tr><th>Scene</th><th>.NET Framework 4.8, 64-bit</th><th>.NET 10, 64-bit</th><th>Difference</th></tr></thead>\r\n");
				foreach(G5Picture picture in listKind) {
					sb.Append("<tbody data-group=\""+WebUtility.HtmlEncode(picture.Group)+"\"><tr>");
					sb.Append("<td style=\"max-width:260px\"><div class=\"name\">"+WebUtility.HtmlEncode(picture.SceneName)+"</div><div class=\"cap\">"
						+WebUtility.HtmlEncode(picture.Engine)+" / "+WebUtility.HtmlEncode(picture.Group)+(picture.SizeFolder!=""?" / "+WebUtility.HtmlEncode(picture.SizeFolder):"")
						+"</div><div>"+WebUtility.HtmlEncode(picture.Description)+"</div><div class=\""+GetKindClass(kind)+"\">"+WebUtility.HtmlEncode(picture.GetKindText())
						+"</div><div class=\"cap\">"+WebUtility.HtmlEncode(picture.GetNumbersText())+"</div><div class=\"cap\">Old picture: "
						+WebUtility.HtmlEncode(picture.RecordName)+"</div></td>");
					sb.Append("<td>"+GetImageCell(picture.PathGalleryNet48)+"</td>");
					sb.Append("<td>"+GetImageCell(picture.Failure==null?picture.PathGalleryNet10:null)+"</td>");
					sb.Append("<td>"+(picture.PathGalleryDiff!=null?GetImageCell(picture.PathGalleryDiff):"<div class=\"cap\">"
						+(picture.Failure!=null?WebUtility.HtmlEncode(picture.Failure):"no pixel changed")+"</div>")+"</td>");
					sb.Append("</tr></tbody>\r\n");
				}
				sb.Append("</table>\r\n");
			}
			sb.Append("<script>\r\n");
			sb.Append("document.getElementById('group').addEventListener('change',function(){var g=this.value;"
				+"document.querySelectorAll('tbody[data-group]').forEach(function(b){b.style.display=(!g||b.getAttribute('data-group')===g)?'':'none';});});\r\n");
			sb.Append("</script>\r\n</body>\r\n</html>\r\n");
			return sb.ToString();
		}

		private static string GetImageCell(string path) {
			if(path==null) {
				return "<div class=\"cap\">none</div>";
			}
			string pathEncoded=WebUtility.HtmlEncode(path);
			return "<a href=\""+pathEncoded+"\"><img loading=\"lazy\" src=\""+pathEncoded+"\" alt=\""+pathEncoded+"\"></a>";
		}

		private static string GetKindHeading(G5Kind kind) {
			switch(kind) {
				case G5Kind.Failed: return "Not compared";
				case G5Kind.Changed: return "Beyond the tolerance";
				case G5Kind.WithinTolerance: return "Within the tolerance";
				default: return "Identical";
			}
		}

		private static string GetKindClass(G5Kind kind) {
			switch(kind) {
				case G5Kind.Failed:
				case G5Kind.Changed:
					return "warn";
				case G5Kind.WithinTolerance:
					return "mid";
				default:
					return "ok";
			}
		}

		///<summary>A zip of the folder with forward-slash entry names (as tools\New-ToothChartGallery.ps1 writes them), so it opens on any system.</summary>
		private static void WriteZip(string folder,string pathZip) {
			if(File.Exists(pathZip)) {
				File.Delete(pathZip);
			}
			using FileStream fileStream=new FileStream(pathZip,FileMode.CreateNew);
			using ZipArchive zipArchive=new ZipArchive(fileStream,ZipArchiveMode.Create);
			string folderFull=folder.TrimEnd('\\');
			string nameRoot=Path.GetFileName(folderFull);
			foreach(string path in Directory.GetFiles(folderFull,"*",SearchOption.AllDirectories).OrderBy(x => x,StringComparer.Ordinal)) {
				string entryName=nameRoot+"/"+path.Substring(folderFull.Length+1).Replace('\\','/');
				ZipArchiveEntry zipArchiveEntry=zipArchive.CreateEntry(entryName,CompressionLevel.Optimal);
				using Stream stream=zipArchiveEntry.Open();
				using FileStream fileStreamSource=File.OpenRead(path);
				fileStreamSource.CopyTo(stream);
			}
		}
		#endregion Gallery G5
	}

	public enum G5Kind {
		Failed,
		Changed,
		WithinTolerance,
		Identical,
	}

	///<summary>One picture of gallery G5: its .NET Framework 4.8 x64 record and its .NET 10 x64 picture, compared.</summary>
	public class G5Picture {
		public string Engine;
		public string Group;
		public string SceneName;
		public string SizeFolder;
		public string Description;
		///<summary>The picture's path in its set, with forward slashes (410x307/restored-adult.png; sheet.png for a host).</summary>
		public string Relative;
		public string PathNet48;
		public string PathNet10;
		public string RecordName;
		public string Failure;
		public int Width;
		public int Height;
		public ChartImageCompareResult Result;
		///<summary>Pixels where any channel differs at all.</summary>
		public int CountChanged;
		public int MaxChannelDifference;
		public Rectangle BoundsChanged;
		public string PathGalleryNet48;
		public string PathGalleryNet10;
		public string PathGalleryDiff;

		public G5Kind Kind {
			get {
				if(Failure!=null || Result==null) {
					return G5Kind.Failed;
				}
				if(CountChanged==0) {
					return G5Kind.Identical;
				}
				return Result.IsPass?G5Kind.WithinTolerance:G5Kind.Changed;
			}
		}

		public string GetKindText() {
			switch(Kind) {
				case G5Kind.Failed: return "Not compared: "+Failure+".";
				case G5Kind.Identical: return "Identical.";
				case G5Kind.WithinTolerance: return "Within the tolerance.";
				default: return "Beyond the tolerance.";
			}
		}

		public string GetBoundsText() {
			if(CountChanged==0) {
				return "";
			}
			return BoundsChanged.X+","+BoundsChanged.Y+" "+BoundsChanged.Width+"x"+BoundsChanged.Height;
		}

		public string GetNumbersText() {
			if(Result==null) {
				return "";
			}
			if(CountChanged==0) {
				return Width+"x"+Height+": no pixel changed.";
			}
			return Width+"x"+Height+": "+CountChanged+" pixels changed, "+Result.CountDifferent+" beyond the tolerance; the worst 16x16 block (at "
				+Result.PointWorstBlock.X+","+Result.PointWorstBlock.Y+") has "+Result.CountDifferentInWorstBlock+"; largest channel difference "
				+MaxChannelDifference+"; changes inside "+GetBoundsText()+".";
		}
	}
}
