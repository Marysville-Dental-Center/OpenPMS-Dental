// Self-tests of the UI refresh's gallery builder (package UI-0a, docs/port-notes/UI-0a.md; docs/specs/ui-refresh-plan.md, sections 7 and 8):
// tools/New-UiGallery.ps1, its mask file tools/ui-gallery-masks.tsv, the one masking and hashing rule tools/UiGallery/UiPictureMask.cs (compiled
// in here as a linked file, as the window checks' check U0 does) and tests/WindowChecksTests/UiApproved/approved.tsv. Every picture is synthetic,
// drawn here into a temp folder: no program, no database, no network.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiGalleryTools;

namespace ReleaseFlowTests {
	[TestClass]
	public class UiGalleryScriptTests {
		private const string SCRIPT="New-UiGallery.ps1";

		#region Helpers
		private class ScriptResult {
			public int ExitCode;
			public string Output;
		}

		private static string ToolsPath(params string[] arrayParts) {
			return Path.Combine(TestPaths.RepoRoot,"tools",Path.Combine(arrayParts));
		}

		private static string Quote(string arg) {
			return "\""+arg+"\"";
		}

		///<summary>Runs a script with Windows PowerShell (the shell the owner and the lead run the builder in), arguments as given.</summary>
		private static ScriptResult RunScript(string script,params string[] arrayArgs) {
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe");
			processStartInfo.Arguments="-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "+Quote(script)+" "+string.Join(" ",arrayArgs);
			processStartInfo.UseShellExecute=false;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.CreateNoWindow=true;
			using(Process process=Process.Start(processStartInfo)) {
				Task<string> taskOut=process.StandardOutput.ReadToEndAsync();
				Task<string> taskErr=process.StandardError.ReadToEndAsync();
				if(!process.WaitForExit(180000)) {
					process.Kill();
					Assert.Fail(Path.GetFileName(script)+" did not finish in 3 minutes.");
				}
				process.WaitForExit();
				ScriptResult scriptResult=new ScriptResult();
				scriptResult.ExitCode=process.ExitCode;
				scriptResult.Output=taskOut.Result+taskErr.Result;
				return scriptResult;
			}
		}

		///<summary>Runs the builder on the folders with the test's mask and approval files.</summary>
		private static ScriptResult RunBuilder(string before,string after,string outFolder,string masks,string approved,params string[] arrayMore) {
			List<string> listArgs=new List<string> { "-After",Quote(after),"-Out",Quote(outFolder),"-Masks",Quote(masks),"-Approved",Quote(approved),"-Commit","abc1234" };
			if(before!=null) {
				listArgs.AddRange(new[] { "-Before",Quote(before) });
			}
			listArgs.AddRange(arrayMore);
			return RunScript(ToolsPath(SCRIPT),listArgs.ToArray());
		}

		private static void DeleteFolder(string folder) {
			try {
				Directory.Delete(folder,true);
			}
			catch(IOException) { }
			catch(UnauthorizedAccessException) { }
		}

		///<summary>A synthetic picture: a colour pattern of its own seed, so no two pictures are alike.</summary>
		private static Bitmap NewPicture(int width,int height,int seed) {
			Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb);
			for(int y=0;y<height;y++) {
				for(int x=0;x<width;x++) {
					bitmap.SetPixel(x,y,Color.FromArgb(255,(x*7+seed*31)%256,(y*5+seed*17)%256,(x+y+seed*13)%256));
				}
			}
			return bitmap;
		}

		private static void Save(Bitmap bitmap,string path) {
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			bitmap.Save(path,ImageFormat.Png);
		}

		///<summary>Saves a copy of the picture with the pixels set to a colour no pattern pixel has (alpha 254), each a different colour.</summary>
		private static void SaveChanged(Bitmap bitmap,string path,IEnumerable<Point> listPoints) {
			using(Bitmap bitmapChanged=new Bitmap(bitmap)) {
				foreach(Point point in listPoints) {
					bitmapChanged.SetPixel(point.X,point.Y,Color.FromArgb(254,1,2,3));
				}
				Save(bitmapChanged,path);
			}
		}

		private static IEnumerable<Point> Block(int left,int top,int width,int height) {
			for(int y=top;y<top+height;y++) {
				for(int x=left;x<left+width;x++) {
					yield return new Point(x,y);
				}
			}
		}

		///<summary>The rows of gallery.tsv by picture: the header's names to the row's values.</summary>
		private static Dictionary<string,Dictionary<string,string>> ReadGalleryTsv(string path,out List<string> listOrder) {
			string[] arrayLines=File.ReadAllLines(path);
			string[] arrayHeader=arrayLines[0].Split('\t');
			Dictionary<string,Dictionary<string,string>> dict=new Dictionary<string,Dictionary<string,string>>();
			listOrder=new List<string>();
			foreach(string line in arrayLines.Skip(1).Where(x => x!="")) {
				string[] arrayFields=line.Split('\t');
				Assert.AreEqual(arrayHeader.Length,arrayFields.Length,line);
				Dictionary<string,string> dictRow=new Dictionary<string,string>();
				for(int i=0;i<arrayHeader.Length;i++) {
					dictRow[arrayHeader[i]]=arrayFields[i];
				}
				dict[arrayFields[0]]=dictRow;
				listOrder.Add(arrayFields[0]);
			}
			return dict;
		}

		private static List<UiMaskRow> Masks(string text) {
			return UiPictureMask.ParseMasks(text,"test");
		}
		#endregion Helpers

		#region The rule: masked pixel SHA-256 (tools/UiGallery/UiPictureMask.cs)
		///<summary>The masked pixel SHA-256 is the SHA-256 of the picture's 32-bit BGRA pixels, row by row without padding, with every masked pixel set
		///to 0 in all four bytes; a pixel under a mask does not change it, every other pixel does, and a mask for another picture does not apply.</summary>
		[TestMethod]
		public void MaskedSha256_IsTheBgraSha256WithMaskedPixelsZero_AndOnlyUnmaskedPixelsCount() {
			List<UiMaskRow> listMasks=Masks("X1-win.png\t*\ttop-left\t5\t4\t10\t3\tclock: test\n");
			using Bitmap bitmap=NewPicture(40,30,1);
			//The rule written out by hand.
			byte[] arrayBytes=new byte[40*30*4];
			for(int y=0;y<30;y++) {
				for(int x=0;x<40;x++) {
					bool isMasked=x>=5 && x<15 && y>=4 && y<7;
					Color color=bitmap.GetPixel(x,y);
					int i=(y*40+x)*4;
					arrayBytes[i]=isMasked?(byte)0:color.B;
					arrayBytes[i+1]=isMasked?(byte)0:color.G;
					arrayBytes[i+2]=isMasked?(byte)0:color.R;
					arrayBytes[i+3]=isMasked?(byte)0:color.A;
				}
			}
			string expected=string.Concat(SHA256.HashData(arrayBytes).Select(x => x.ToString("x2")));
			string sha=UiPictureMask.GetMaskedPixelSha256(bitmap,"X1-win.png",listMasks);
			Assert.AreEqual(expected,sha);
			Assert.IsTrue(Regex.IsMatch(sha,"^[0-9a-f]{64}$"));
			//A path that ends in the file name matches a pattern without a slash.
			Assert.AreEqual(sha,UiPictureMask.GetMaskedPixelSha256(bitmap,"sub/X1-win.png",listMasks));
			using(Bitmap bitmapInside=new Bitmap(bitmap)) {
				bitmapInside.SetPixel(14,6,Color.Red);
				bitmapInside.SetPixel(5,4,Color.Blue);
				Assert.AreEqual(sha,UiPictureMask.GetMaskedPixelSha256(bitmapInside,"X1-win.png",listMasks),"A masked pixel does not count.");
				Assert.AreNotEqual(UiPictureMask.GetMaskedPixelSha256(bitmap,"X2-other.png",listMasks),
					UiPictureMask.GetMaskedPixelSha256(bitmapInside,"X2-other.png",listMasks),"Another picture's mask does not apply.");
			}
			foreach(Point point in new[] { new Point(15,6),new Point(4,4),new Point(14,7),new Point(14,3),new Point(0,0),new Point(39,29) }) {
				using Bitmap bitmapOutside=new Bitmap(bitmap);
				bitmapOutside.SetPixel(point.X,point.Y,Color.FromArgb(255,1,2,3));
				Assert.AreNotEqual(sha,UiPictureMask.GetMaskedPixelSha256(bitmapOutside,"X1-win.png",listMasks),"Pixel "+point+" is not masked.");
			}
			//Alpha counts too: a picture is opaque, so a masked 0 never equals a real pixel.
			using(Bitmap bitmapAlpha=new Bitmap(bitmap)) {
				Color color=bitmap.GetPixel(20,20);
				bitmapAlpha.SetPixel(20,20,Color.FromArgb(200,color.R,color.G,color.B));
				Assert.AreNotEqual(sha,UiPictureMask.GetMaskedPixelSha256(bitmapAlpha,"X1-win.png",listMasks));
			}
		}

		///<summary>The same pixels give the same hash whatever the file's pixel format (a 24-bit PNG and a 32-bit one), from a file or a bitmap.</summary>
		[TestMethod]
		public void MaskedSha256_IsTheSameForA24And32BitFile_AndForTheBitmap() {
			string folder=TestPaths.NewTempFolder("ui0a_formats");
			try {
				using Bitmap bitmap=NewPicture(31,17,2);
				string path32=Path.Combine(folder,"a32.png");
				string path24=Path.Combine(folder,"a24.png");
				Save(bitmap,path32);
				using(Bitmap bitmap24=bitmap.Clone(new Rectangle(0,0,31,17),PixelFormat.Format24bppRgb)) {
					Save(bitmap24,path24);
				}
				List<UiMaskRow> listMasks=Masks("a*.png\t31x17\tbottom-right\t1\t1\t3\t2\tdate: test\n");
				string sha=UiPictureMask.GetMaskedPixelSha256(bitmap,"a.png",listMasks);
				Assert.AreEqual(sha,UiPictureMask.GetMaskedPixelSha256(path32,"a.png",listMasks));
				Assert.AreEqual(sha,UiPictureMask.GetMaskedPixelSha256(path24,"a.png",listMasks));
				Assert.AreNotEqual(sha,UiPictureMask.GetMaskedPixelSha256(bitmap,"a.png",new List<UiMaskRow>()),"The mask applied.");
				//Reading did not keep the file open.
				File.Delete(path24);
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>A rectangle is measured from its anchor corner, clipped to the picture, and empty when it lies outside it.</summary>
		[TestMethod]
		public void MaskRectangles_FollowTheirAnchor_AndAreClipped() {
			List<UiMaskRow> listMasks=Masks("picture\tsize\tanchor\tx\ty\twidth\theight\twhat\n"
				+"a.png\t*\ttop-left\t2\t3\t10\t5\tclock: 1\n"
				+"a.png\t*\ttop-right\t2\t3\t10\t5\tclock: 2\n"
				+"a.png\t*\tbottom-left\t2\t3\t10\t5\tclock: 3\n"
				+"a.png\t*\tBottom-Right\t2\t3\t10\t5\tclock: 4\n"
				+"a.png\t*\ttop-left\t95\t20\t10\t10\tclock: 5\n"
				+"a.png\t*\ttop-left\t100\t0\t5\t5\tclock: 6\n");
			Assert.AreEqual(new Rectangle(2,3,10,5),UiPictureMask.GetRectangle(listMasks[0],100,50));
			Assert.AreEqual(new Rectangle(88,3,10,5),UiPictureMask.GetRectangle(listMasks[1],100,50));
			Assert.AreEqual(new Rectangle(2,42,10,5),UiPictureMask.GetRectangle(listMasks[2],100,50));
			Assert.AreEqual(new Rectangle(88,42,10,5),UiPictureMask.GetRectangle(listMasks[3],100,50));
			Assert.AreEqual(new Rectangle(95,20,5,10),UiPictureMask.GetRectangle(listMasks[4],100,50));
			Assert.AreEqual(Rectangle.Empty,UiPictureMask.GetRectangle(listMasks[5],100,50));
			//The masked pixels are those rectangles.
			bool[] arrayMasked=UiPictureMask.GetMaskedPixels(listMasks,"a.png",100,50);
			Assert.AreEqual(4*50+50,arrayMasked.Count(x => x));
			Assert.IsTrue(arrayMasked[3*100+2]);
			Assert.IsFalse(arrayMasked[3*100+1]);
			Assert.IsTrue(arrayMasked[46*100+97]);
			Assert.IsFalse(arrayMasked[47*100+97]);
		}

		///<summary>A pattern without a slash matches the file name, with one the relative path; any case; * and ?; a sized row only that size.</summary>
		[TestMethod]
		public void MaskRows_MatchByFileNameOrPath_AndBySize() {
			Assert.IsTrue(UiPictureMask.IsMatch("B1-appts-*.png","B1-appts-week.png"));
			Assert.IsTrue(UiPictureMask.IsMatch("b1-APPTS-*.PNG","U/B1-appts-day.png"));
			Assert.IsTrue(UiPictureMask.IsMatch("A?-about.png","A2-about.png"));
			Assert.IsFalse(UiPictureMask.IsMatch("A?-about.png","A12-about.png"));
			Assert.IsFalse(UiPictureMask.IsMatch("A2-about.png","A2-about.png.bak"));
			Assert.IsTrue(UiPictureMask.IsMatch("U/*.png","U/U1-claims.png"));
			Assert.IsTrue(UiPictureMask.IsMatch("U/*.png","U\\U1-claims.png"));
			Assert.IsFalse(UiPictureMask.IsMatch("U/*.png","U1-claims.png"));
			Assert.IsFalse(UiPictureMask.IsMatch("A1.png","A1+png"),"A dot is a dot.");
			List<UiMaskRow> listMasks=Masks("A2-about.png\t644x444\ttop-left\t1\t1\t5\t5\tdatabase: test\nA2-about.png\t*\ttop-left\t1\t1\t5\t5\tdate: test\n");
			Assert.AreEqual(2,UiPictureMask.GetMasksFor(listMasks,"A2-about.png",644,444).Count);
			Assert.AreEqual(1,UiPictureMask.GetMasksFor(listMasks,"A2-about.png",700,444).Count);
			Assert.AreEqual("database: test",UiPictureMask.GetMasksWrongSize(listMasks,"A2-about.png",700,444).Single().What);
			Assert.AreEqual(0,UiPictureMask.GetMasksWrongSize(listMasks,"A3-other.png",700,444).Count);
		}

		///<summary>A malformed mask row is refused with its line number, never skipped.</summary>
		[TestMethod]
		public void ParseMasks_RefusesABadRow_NamingItsLine() {
			string ok="a.png\t*\ttop-left\t0\t0\t1\t1\tclock: ok\n";
			foreach(string bad in new[] {
				"a.png\t*\ttop-left\t0\t0\t1\t1\n",
				"a.png\t*\ttop-left\t0\t0\t1\t1\tclock\textra\n",
				"a.png\t64\ttop-left\t0\t0\t1\t1\tclock: x\n",
				"a.png\t*\tmiddle\t0\t0\t1\t1\tclock: x\n",
				"a.png\t*\ttop-left\t-1\t0\t1\t1\tclock: x\n",
				"a.png\t*\ttop-left\t0\t0\t0\t1\tclock: x\n",
				"a.png\t*\ttop-left\t0\t0\t1\tx\tclock: x\n",
				"\t*\ttop-left\t0\t0\t1\t1\tclock: x\n",
				"a.png\t*\ttop-left\t0\t0\t1\t1\t \n",
			}) {
				FormatException formatException=Assert.ThrowsExactly<FormatException>(() => UiPictureMask.ParseMasks("# comment\n\n"+ok+bad,"m.tsv"),bad);
				StringAssert.Contains(formatException.Message,"m.tsv line 4: ",bad);
			}
			Assert.AreEqual(1,UiPictureMask.ParseMasks("# comment\r\n\r\n"+ok.Replace("\n","\r\n"),"m.tsv").Count);
		}

		///<summary>tools/ui-gallery-masks.tsv parses, has rows for the clock, dates and the copy's database name, and every row says what it hides.</summary>
		[TestMethod]
		public void TheRepositoryMaskFile_Parses_AndCoversTheClockDatesAndDatabaseName() {
			List<UiMaskRow> listMasks=UiPictureMask.ReadMasks(ToolsPath("ui-gallery-masks.tsv"));
			Assert.IsTrue(listMasks.Count>=10,"rows: "+listMasks.Count);
			string[] arrayKinds=new[] { "clock: ","date: ","database: ","path: ","machine state: " };
			foreach(UiMaskRow uiMaskRow in listMasks) {
				Assert.IsTrue(arrayKinds.Any(x => uiMaskRow.What.StartsWith(x,StringComparison.Ordinal)),uiMaskRow.ToString());
			}
			foreach(string kind in new[] { "clock: ","date: ","database: " }) {
				Assert.IsTrue(listMasks.Any(x => x.What.StartsWith(kind,StringComparison.Ordinal)),"A row for "+kind);
			}
			//Help > About's database name.
			Assert.IsTrue(UiPictureMask.GetMasksFor(listMasks,"A2-about.png",644,444).Any(x => x.What.StartsWith("database: ",StringComparison.Ordinal)));
		}
		#endregion The rule: masked pixel SHA-256

		#region approved.tsv
		///<summary>approved.tsv starts empty: only its header. Its rows parse, the last row of a picture is its approval, and a malformed row is refused.</summary>
		[TestMethod]
		public void ApprovedTsv_StartsEmpty_AndItsRowsParse() {
			string path=Path.Combine(TestPaths.RepoRoot,"tests","WindowChecksTests","UiApproved","approved.tsv");
			string[] arrayLines=File.ReadAllLines(path).Where(x => x!="").ToArray();
			CollectionAssert.AreEqual(new[] { UiPictureMask.ApprovedHeader },arrayLines);
			Assert.AreEqual(0,UiPictureMask.ReadApproved(path).Count);
			string sha1=new string('a',64);
			string sha2=new string('b',64);
			List<UiApprovedRow> listApproved=UiPictureMask.ParseApproved(UiPictureMask.ApprovedHeader+"\r\nA2-about.png\t644x444\t"+sha1+"\tG-UI3\tabc1234\t2026-11-20\r\n"
				+"# a note\r\nA2-about.png\t644x444\t"+sha2+"\tG-UI3\tdef5678\t2026-11-21\r\n","approved.tsv");
			Assert.AreEqual(2,listApproved.Count);
			UiApprovedRow uiApprovedRow=UiPictureMask.FindApproved(listApproved,"a2-about.png");
			Assert.AreEqual(sha2,uiApprovedRow.Sha256);
			Assert.AreEqual("def5678",uiApprovedRow.Commit);
			Assert.AreEqual(644,uiApprovedRow.Width);
			Assert.IsTrue(UiPictureMask.IsApproved(listApproved,"A2-about.png",644,444,sha2));
			Assert.IsFalse(UiPictureMask.IsApproved(listApproved,"A2-about.png",644,444,sha1),"An older approval does not count.");
			Assert.IsFalse(UiPictureMask.IsApproved(listApproved,"A2-about.png",645,444,sha2),"The size counts.");
			Assert.IsNull(UiPictureMask.FindApproved(listApproved,"A3.png"));
			foreach(string bad in new[] { "A2.png\t644x444\t"+sha1+"\tG\tc","A2.png\t644\t"+sha1+"\tG\tc\t2026-11-20","A2.png\t644x444\tABC\tG\tc\t2026-11-20",
				"A2.png\t644x444\t"+sha1+"\tG\tc\t20.11.2026" })
			{
				FormatException formatException=Assert.ThrowsExactly<FormatException>(() => UiPictureMask.ParseApproved(bad,"approved.tsv"),bad);
				StringAssert.Contains(formatException.Message,"approved.tsv line 1: ");
			}
		}
		#endregion approved.tsv

		#region The comparison
		///<summary>Changed pixels are counted over the unmasked ones, masked pixels in either picture are left out, the area only one picture has counts
		///as changed, and the difference picture marks them: red changed, magenta only one picture, blue hatched masked, grey the rest.</summary>
		[TestMethod]
		public void Compare_CountsChangedPixels_LeavesOutTheMasks_AndDrawsTheDifference() {
			string folder=TestPaths.NewTempFolder("ui0a_compare");
			try {
				List<UiMaskRow> listMasks=Masks("w.png\t*\ttop-left\t0\t0\t4\t2\tclock: test\n");
				using Bitmap bitmap=NewPicture(20,10,3);
				string pathBefore=Path.Combine(folder,"before.png");
				string pathAfter=Path.Combine(folder,"after.png");
				string pathDiff=Path.Combine(folder,"diff","w.png");
				Save(bitmap,pathBefore);
				SaveChanged(bitmap,pathAfter,Block(0,0,4,2).Concat(Block(10,5,3,2)));
				UiPictureCompareResult result=UiPictureMask.Compare(pathBefore,pathAfter,"w.png",listMasks,0,pathDiff);
				Assert.AreEqual(8,result.CountMasked);
				Assert.AreEqual(192,result.CountCompared);
				Assert.AreEqual(6,result.CountChanged);
				Assert.AreEqual(6*100.0/192,result.PercentChanged,1e-9);
				Assert.AreEqual(new Rectangle(10,5,3,2),result.BoundsChanged);
				Assert.AreEqual("clock: test",result.ListMasksApplied.Single().What);
				Assert.AreEqual(UiPictureMask.GetMaskedPixelSha256(pathAfter,"w.png",listMasks),result.Sha256After);
				Assert.AreEqual(UiPictureMask.GetMaskedPixelSha256(pathBefore,"w.png",listMasks),result.Sha256Before);
				using(Bitmap bitmapDiff=new Bitmap(pathDiff)) {
					Assert.AreEqual(Color.FromArgb(255,255,0,0),bitmapDiff.GetPixel(11,6));
					Color colorGrey=bitmapDiff.GetPixel(15,8);
					Assert.IsTrue(colorGrey.R==colorGrey.G && colorGrey.G==colorGrey.B && colorGrey.R>=128,colorGrey.ToString());
					Color colorMasked=bitmapDiff.GetPixel(3,1);
					Assert.IsTrue(colorMasked.B>colorMasked.R,"blue: "+colorMasked);
				}
				//Only the masked pixels differ: nothing changed, no difference picture.
				string pathAfterMaskOnly=Path.Combine(folder,"after2.png");
				string pathDiff2=Path.Combine(folder,"diff2.png");
				SaveChanged(bitmap,pathAfterMaskOnly,Block(0,0,4,2));
				UiPictureCompareResult resultSame=UiPictureMask.Compare(pathBefore,pathAfterMaskOnly,"w.png",listMasks,0,pathDiff2);
				Assert.AreEqual(0,resultSame.CountChanged);
				Assert.AreEqual(resultSame.Sha256Before,resultSame.Sha256After);
				Assert.IsFalse(File.Exists(pathDiff2));
				//A bigger after picture: the strip only it has is changed, and magenta.
				string pathWide=Path.Combine(folder,"wide.png");
				using(Bitmap bitmapWide=new Bitmap(22,10,PixelFormat.Format32bppArgb)) {
					using(Graphics g=Graphics.FromImage(bitmapWide)) {
						g.Clear(Color.White);
						g.DrawImageUnscaled(bitmap,0,0);
					}
					Save(bitmapWide,pathWide);
				}
				string pathDiff3=Path.Combine(folder,"diff3.png");
				UiPictureCompareResult resultWide=UiPictureMask.Compare(pathBefore,pathWide,"w.png",listMasks,0,pathDiff3);
				Assert.AreEqual(22,resultWide.Width);
				Assert.AreEqual(20,resultWide.CountChanged);
				using(Bitmap bitmapDiff3=new Bitmap(pathDiff3)) {
					Assert.AreEqual(Color.FromArgb(255,255,0,255),bitmapDiff3.GetPixel(21,9));
				}
				//The threshold: a difference of 1 in a channel counts at 0 and not at 1.
				string pathNear=Path.Combine(folder,"near.png");
				using(Bitmap bitmapNear=new Bitmap(bitmap)) {
					Color color=bitmap.GetPixel(15,5);
					bitmapNear.SetPixel(15,5,Color.FromArgb(255,color.R^1,color.G,color.B));
					Save(bitmapNear,pathNear);
				}
				Assert.AreEqual(1,UiPictureMask.Compare(pathBefore,pathNear,"w.png",listMasks,0,null).CountChanged);
				Assert.AreEqual(0,UiPictureMask.Compare(pathBefore,pathNear,"w.png",listMasks,1,null).CountChanged);
			}
			finally {
				DeleteFolder(folder);
			}
		}
		#endregion The comparison

		#region tools/New-UiGallery.ps1
		///<summary>The builder end to end on synthetic pictures: every picture copied, the changed ones with a difference picture, gallery.tsv with
		///the share of changed pixels and the masked pixel SHA-256 of the one rule (the script's Add-Type copy gives the same hash as the copy compiled
		///in here), the approvals and clip notes, index.html with the pictures at 1:1 device pixels, the zoom toggle, the filters, the changed ones
		///first and the Approve/Change column, and a zip whose entries use forward slashes.</summary>
		[TestMethod]
		public void Builder_MakesTheGallery_FromSyntheticPictures() {
			string folder=TestPaths.NewTempFolder("ui0a_gallery");
			try {
				string before=Path.Combine(folder,"before-set");
				string after=Path.Combine(folder,"after-set");
				string outFolder=Path.Combine(folder,"G-UI9");
				string masks=Path.Combine(folder,"masks.tsv");
				string approved=Path.Combine(folder,"approved.tsv");
				File.WriteAllText(masks,"# test masks\npicture\tsize\tanchor\tx\ty\twidth\theight\twhat\nA2-clock.png\t*\ttop-right\t2\t2\t20\t8\tclock: the test clock\n"
					+"B1-changed.png\t99x99\ttop-left\t0\t0\t5\t5\tdate: a row for another size\n",Encoding.ASCII);
				using Bitmap bitmapSame=NewPicture(64,48,1);
				using Bitmap bitmapClock=NewPicture(80,40,2);
				using Bitmap bitmapChanged=NewPicture(50,40,3);
				using Bitmap bitmapSmall=NewPicture(60,30,4);
				using Bitmap bitmapNew=NewPicture(30,20,5);
				using Bitmap bitmapGone=NewPicture(30,20,6);
				using Bitmap bitmapNested=NewPicture(40,40,7);
				Save(bitmapSame,Path.Combine(before,"A1-same.png"));
				Save(bitmapSame,Path.Combine(after,"A1-same.png"));
				Save(bitmapClock,Path.Combine(before,"A2-clock.png"));
				SaveChanged(bitmapClock,Path.Combine(after,"A2-clock.png"),Block(60,3,10,5));
				Save(bitmapChanged,Path.Combine(before,"B1-changed.png"));
				SaveChanged(bitmapChanged,Path.Combine(after,"B1-changed.png"),Block(10,10,20,10));
				Save(bitmapSmall,Path.Combine(before,"B2-small.png"));
				SaveChanged(bitmapSmall,Path.Combine(after,"B2-small.png"),Block(59,29,1,1));
				Save(bitmapNew,Path.Combine(after,"C1-new.png"));
				Save(bitmapGone,Path.Combine(before,"C2-gone.png"));
				Save(bitmapNested,Path.Combine(before,"sub","D1-nested.png"));
				SaveChanged(bitmapNested,Path.Combine(after,"sub","D1-nested.png"),Block(0,0,40,5));
				File.WriteAllText(Path.Combine(after,"clip-notes.tsv"),"picture\tnote\nB1-changed.png\tlabelTitle needs 6 px\tfixable\nsub/D1-nested.png\tbutOK <cut>\n",
					Encoding.ASCII);
				List<UiMaskRow> listMasks=UiPictureMask.ReadMasks(masks);
				string shaSame=UiPictureMask.GetMaskedPixelSha256(Path.Combine(after,"A1-same.png"),"A1-same.png",listMasks);
				File.WriteAllText(approved,UiPictureMask.ApprovedHeader+"\nA1-same.png\t64x48\t"+shaSame+"\tG-UI2\told0001\t2026-11-18\n"
					+"B1-changed.png\t50x40\t"+new string('0',64)+"\tG-UI2\told0001\t2026-11-18\n",Encoding.ASCII);
				ScriptResult scriptResult=RunBuilder(before,after,outFolder,masks,approved,"-Name","G-UI9","-Note",Quote("Synthetic test pictures."));
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				StringAssert.Contains(scriptResult.Output,"Gallery G-UI9: 7 pictures, 3 changed, 2 the same, 2 only in one set;");
				//The pictures.
				foreach(string picture in new[] { "A1-same.png","A2-clock.png","B1-changed.png","B2-small.png","C2-gone.png","sub/D1-nested.png" }) {
					Assert.IsTrue(File.Exists(Path.Combine(outFolder,"before",picture)),"before/"+picture);
				}
				foreach(string picture in new[] { "A1-same.png","A2-clock.png","B1-changed.png","B2-small.png","C1-new.png","sub/D1-nested.png" }) {
					Assert.IsTrue(File.Exists(Path.Combine(outFolder,"after",picture)),"after/"+picture);
					CollectionAssert.AreEqual(File.ReadAllBytes(Path.Combine(after,picture)),File.ReadAllBytes(Path.Combine(outFolder,"after",picture)),"copied as it is");
				}
				CollectionAssert.AreEquivalent(new[] { "B1-changed.png","B2-small.png","D1-nested.png" },
					Directory.GetFiles(Path.Combine(outFolder,"diff"),"*.png",SearchOption.AllDirectories).Select(Path.GetFileName).ToArray());
				Assert.IsTrue(File.Exists(Path.Combine(outFolder,"masks.tsv")));
				Assert.IsTrue(File.Exists(Path.Combine(outFolder,"ui-gallery.txt")));
				//gallery.tsv.
				Dictionary<string,Dictionary<string,string>> dictRows=ReadGalleryTsv(Path.Combine(outFolder,"gallery.tsv"),out List<string> listOrder);
				CollectionAssert.AreEqual(new[] { "sub/D1-nested.png","B1-changed.png","B2-small.png","C1-new.png","C2-gone.png","A1-same.png","A2-clock.png" },listOrder,
					"The changed ones first, most changed first; then the new, the gone and the same ones.");
				Assert.AreEqual("changed",dictRows["B1-changed.png"]["status"]);
				Assert.AreEqual("200",dictRows["B1-changed.png"]["changed"]);
				Assert.AreEqual("2000",dictRows["B1-changed.png"]["compared"]);
				Assert.AreEqual("10.0000",dictRows["B1-changed.png"]["percent"]);
				Assert.AreEqual("1",dictRows["B1-changed.png"]["clip_notes"]);
				Assert.AreEqual("differs",dictRows["B1-changed.png"]["approved"]);
				Assert.AreEqual("B",dictRows["B1-changed.png"]["group"]);
				Assert.AreEqual("1",dictRows["B2-small.png"]["changed"]);
				Assert.AreEqual("0.0556",dictRows["B2-small.png"]["percent"]);
				Assert.AreEqual("sub",dictRows["sub/D1-nested.png"]["group"]);
				Assert.AreEqual("same",dictRows["A2-clock.png"]["status"],"Only the masked clock changed.");
				Assert.AreEqual("160",dictRows["A2-clock.png"]["masked"]);
				Assert.AreEqual(dictRows["A2-clock.png"]["sha256_before"],dictRows["A2-clock.png"]["sha256_after"]);
				Assert.AreEqual("same",dictRows["A1-same.png"]["status"]);
				Assert.AreEqual("same",dictRows["A1-same.png"]["approved"],"Exactly as approved.");
				Assert.AreEqual("only after",dictRows["C1-new.png"]["status"]);
				Assert.AreEqual("30x20",dictRows["C1-new.png"]["after_size"]);
				Assert.AreEqual("",dictRows["C1-new.png"]["before_size"]);
				Assert.AreEqual("only before",dictRows["C2-gone.png"]["status"]);
				Assert.AreEqual("none",dictRows["C2-gone.png"]["approved"]);
				//One rule: the hash the script's Add-Type copy wrote is the hash of the copy compiled in here.
				foreach(string picture in new[] { "A1-same.png","A2-clock.png","B1-changed.png","B2-small.png","C1-new.png","sub/D1-nested.png" }) {
					Assert.AreEqual(UiPictureMask.GetMaskedPixelSha256(Path.Combine(after,picture),picture,listMasks),dictRows[picture]["sha256_after"],picture);
				}
				Assert.AreEqual(UiPictureMask.GetMaskedPixelSha256(Path.Combine(before,"C2-gone.png"),"C2-gone.png",listMasks),dictRows["C2-gone.png"]["sha256_before"]);
				//index.html.
				string html=File.ReadAllText(Path.Combine(outFolder,"index.html"));
				Assert.IsTrue(html.StartsWith("<!doctype html>",StringComparison.Ordinal));
				StringAssert.Contains(html,"<title>UI gallery G-UI9</title>");
				StringAssert.Contains(html,"image-rendering:pixelated");
				StringAssert.Contains(html,"window.devicePixelRatio");
				StringAssert.Contains(html,"getAttribute('data-w')*f/r");
				StringAssert.Contains(html,"<button type=\"button\" id=\"zoom\">Zoom: 1:1</button>");
				StringAssert.Contains(html,"<th>Your answer</th>");
				StringAssert.Contains(html,"value=\"Approve\"");
				StringAssert.Contains(html,"value=\"Change\"");
				StringAssert.Contains(html,"id=\"dl\">Download answers</button>");
				StringAssert.Contains(html,"'picture\\tsize\\tsha256\\tgallery\\tcommit\\tdate\\tanswer\\tnote'");
				StringAssert.Contains(html,"<select id=\"fGroup\"><option value=\"\">all</option><option>A</option><option>B</option><option>C</option><option>sub</option></select>");
				StringAssert.Contains(html,"<li>labelTitle needs 6 px; fixable</li>");
				StringAssert.Contains(html,"<li>butOK &lt;cut&gt;</li>","Clip notes are HTML-encoded.");
				StringAssert.Contains(html,"changed since approval (G-UI2 at old0001, 2026-11-18)");
				StringAssert.Contains(html,"approved: G-UI2 at old0001, 2026-11-18");
				StringAssert.Contains(html,"Mask row for another size, not applied: line 4 (B1-changed.png, 99x99");
				StringAssert.Contains(html,"masked: clock: the test clock");
				StringAssert.Contains(html,"Synthetic test pictures.");
				StringAssert.Contains(html,"<img class=\"px\" loading=\"lazy\" src=\"diff/sub/D1-nested.png\" width=\"40\" height=\"40\" data-w=\"40\" data-h=\"40\"");
				StringAssert.Contains(html,"data-picture=\"B1-changed.png\" data-group=\"B\" data-status=\"changed\" data-clip=\"1\" data-approved=\"differs\" data-size=\"50x40\" data-sha=\""
					+dictRows["B1-changed.png"]["sha256_after"]+"\"");
				Assert.IsTrue(html.IndexOf("id=\"p1\"",StringComparison.Ordinal)<html.IndexOf("id=\"p7\"",StringComparison.Ordinal));
				Assert.IsTrue(html.IndexOf("<a href=\"#p1\">sub/D1-nested.png</a>",StringComparison.Ordinal)>0,"The most changed first.");
				//Within its group the more changed picture comes first.
				Assert.IsTrue(html.IndexOf("<section class=\"pic\" id=\"p2\"",StringComparison.Ordinal)<html.IndexOf("<section class=\"pic\" id=\"p3\"",StringComparison.Ordinal));
				Assert.IsTrue(html.IndexOf("<h2>Group B</h2>",StringComparison.Ordinal)<html.IndexOf("id=\"p2\" data-id",html.IndexOf("<h2>Group B</h2>",StringComparison.Ordinal),StringComparison.Ordinal));
				Assert.IsFalse(html.Contains(folder),"No local path on the page.");
				Assert.IsFalse(html.Contains("src=\"before\\"),"Forward slashes in links.");
				Assert.IsTrue(Encoding.UTF8.GetBytes(html).All(x => x<0x80),"The page is ASCII.");
				//The zip: every file, forward slashes.
				using(ZipArchive zipArchive=ZipFile.OpenRead(outFolder+".zip")) {
					List<string> listEntries=zipArchive.Entries.Select(x => x.FullName).ToList();
					Assert.IsFalse(listEntries.Any(x => x.Contains('\\')),string.Join(", ",listEntries));
					CollectionAssert.Contains(listEntries,"index.html");
					CollectionAssert.Contains(listEntries,"gallery.tsv");
					CollectionAssert.Contains(listEntries,"after/sub/D1-nested.png");
					CollectionAssert.Contains(listEntries,"diff/sub/D1-nested.png");
					Assert.AreEqual(Directory.GetFiles(outFolder,"*",SearchOption.AllDirectories).Length,listEntries.Count);
				}
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>The builder empties only a gallery folder: a folder of other files is refused and left as it is; an earlier gallery is emptied and
		///written again. A gallery folder inside a picture folder is refused. -NoZip writes no zip; -Threshold leaves small differences out.</summary>
		[TestMethod]
		public void Builder_RefusesAFolderThatIsNotAGallery_AndRewritesAGallery() {
			string folder=TestPaths.NewTempFolder("ui0a_refuse");
			try {
				string before=Path.Combine(folder,"b");
				string after=Path.Combine(folder,"a");
				string masks=Path.Combine(folder,"masks.tsv");
				string approved=Path.Combine(folder,"approved.tsv");
				File.WriteAllText(masks,"",Encoding.ASCII);
				File.WriteAllText(approved,UiPictureMask.ApprovedHeader+"\n",Encoding.ASCII);
				using Bitmap bitmap=NewPicture(20,20,8);
				Save(bitmap,Path.Combine(before,"K1.png"));
				using(Bitmap bitmapNear=new Bitmap(bitmap)) {
					Color color=bitmap.GetPixel(5,5);
					bitmapNear.SetPixel(5,5,Color.FromArgb(255,color.R^8,color.G,color.B));
					Save(bitmapNear,Path.Combine(after,"K1.png"));
				}
				string other=Path.Combine(folder,"other");
				Directory.CreateDirectory(other);
				File.WriteAllText(Path.Combine(other,"keep.txt"),"keep");
				ScriptResult scriptResult=RunBuilder(before,after,other,masks,approved);
				Assert.AreEqual(1,scriptResult.ExitCode,scriptResult.Output);
				StringAssert.Contains(scriptResult.Output,"Not a gallery folder, so it is not emptied");
				Assert.IsTrue(File.Exists(Path.Combine(other,"keep.txt")));
				Assert.AreEqual(1,Directory.GetFileSystemEntries(other).Length);
				Assert.IsFalse(File.Exists(other+".zip"));
				scriptResult=RunBuilder(before,after,Path.Combine(after,"g"),masks,approved);
				Assert.AreEqual(1,scriptResult.ExitCode,scriptResult.Output);
				StringAssert.Contains(scriptResult.Output,"overlap");
				Assert.IsFalse(Directory.Exists(Path.Combine(after,"g")));
				string gallery=Path.Combine(folder,"g");
				scriptResult=RunBuilder(before,after,gallery,masks,approved,"-NoZip");
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				Assert.IsFalse(File.Exists(gallery+".zip"));
				StringAssert.Contains(File.ReadAllText(Path.Combine(gallery,"gallery.tsv")),"K1.png\tK\tchanged\t");
				File.WriteAllText(Path.Combine(gallery,"stale.txt"),"from an earlier run");
				scriptResult=RunBuilder(before,after,gallery,masks,approved,"-Threshold","8");
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				Assert.IsFalse(File.Exists(Path.Combine(gallery,"stale.txt")),"The earlier gallery was emptied.");
				StringAssert.Contains(File.ReadAllText(Path.Combine(gallery,"gallery.tsv")),"K1.png\tK\tsame\t","A difference of 8 is within -Threshold 8.");
				Assert.IsTrue(File.Exists(gallery+".zip"));
				//Without a before folder every picture is new.
				scriptResult=RunBuilder(null,after,gallery,masks,approved,"-NoZip");
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				StringAssert.Contains(File.ReadAllText(Path.Combine(gallery,"gallery.tsv")),"K1.png\tK\tonly after\t");
				//No after folder: refused.
				scriptResult=RunScript(ToolsPath(SCRIPT),"-Out",Quote(Path.Combine(folder,"g2")));
				Assert.AreEqual(1,scriptResult.ExitCode,scriptResult.Output);
				StringAssert.Contains(scriptResult.Output,"-After");
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>The owner's answers are kept by picture, not by place on the page. A gallery rebuilt under the same name and commit (so the same
		///browser storage key) with the pictures in another order still marks every Approve/Change input and note with its own picture's relative path
		///(HTML-encoded), never with its place ("p1"), and the page script counts a saved answer only for the masked SHA-256 and size it was given for,
		///so a changed picture shows as unanswered and is not written to the answers file.</summary>
		[TestMethod]
		public void Builder_KeysAnswersByPicture_WhenARebuildChangesTheOrder() {
			string folder=TestPaths.NewTempFolder("ui0a_order");
			try {
				string before=Path.Combine(folder,"b");
				string after=Path.Combine(folder,"a");
				string gallery=Path.Combine(folder,"G-ORDER");
				string masks=Path.Combine(folder,"masks.tsv");
				string approved=Path.Combine(folder,"approved.tsv");
				File.WriteAllText(masks,"",Encoding.ASCII);
				File.WriteAllText(approved,UiPictureMask.ApprovedHeader+"\n",Encoding.ASCII);
				const string pictureX="X1-first.png";
				const string pictureY="Y1-R&D.png";//Needs HTML encoding in an attribute.
				using Bitmap bitmapX=NewPicture(40,20,11);
				using Bitmap bitmapY=NewPicture(40,20,12);
				Save(bitmapX,Path.Combine(before,pictureX));
				Save(bitmapY,Path.Combine(before,pictureY));
				//First build: X changed most, so X is first (p1).
				SaveChanged(bitmapX,Path.Combine(after,pictureX),Block(0,0,20,10));
				SaveChanged(bitmapY,Path.Combine(after,pictureY),Block(0,0,2,2));
				ScriptResult scriptResult=RunBuilder(before,after,gallery,masks,approved,"-Name","G-ORDER","-NoZip");
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				string htmlFirst=File.ReadAllText(Path.Combine(gallery,"index.html"));
				//Rebuild under the same name and commit: now Y changed most, so Y is first.
				SaveChanged(bitmapX,Path.Combine(after,pictureX),Block(0,0,2,2));
				SaveChanged(bitmapY,Path.Combine(after,pictureY),Block(0,0,20,10));
				scriptResult=RunBuilder(before,after,gallery,masks,approved,"-Name","G-ORDER","-NoZip");
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				string htmlSecond=File.ReadAllText(Path.Combine(gallery,"index.html"));
				Assert.AreEqual(pictureX,PictureAt(htmlFirst,"p1"),"The first build puts X first.");
				Assert.AreEqual(pictureY,PictureAt(htmlSecond,"p1"),"The rebuild puts Y first: the places changed.");
				foreach(string html in new[] { htmlFirst,htmlSecond }) {
					//The same storage key in both builds: the gallery's name and commit only.
					StringAssert.Contains(html,"var GALLERY='G-ORDER',COMMIT='abc1234';");
					StringAssert.Contains(html,"key='ui-gallery:'+GALLERY+':'+COMMIT");
					List<string> listPics=Regex.Matches(html,"data-pic=\"([^\"]*)\"").Cast<Match>().Select(x => System.Net.WebUtility.HtmlDecode(x.Groups[1].Value)).ToList();
					Assert.AreEqual(12,listPics.Count,"Two radios and a note, in the table and on the card, for each of the two pictures.");
					Assert.IsFalse(listPics.Any(x => Regex.IsMatch(x,"^p[0-9]+$")),"data-pic is never a place on the page: "+string.Join(", ",listPics));
					CollectionAssert.AreEquivalent(new[] { pictureX,pictureY },listPics.Distinct().ToArray());
					StringAssert.Contains(html,"data-pic=\"Y1-R&amp;D.png\"","HTML-encoded.");
					//Every input sits in the row and card of the picture it names.
					MatchCollection matchCollection=Regex.Matches(html,"<(tr|section class=\"pic\" id=\"p[0-9]+\") data-id=\"(p[0-9]+)\" data-picture=\"([^\"]*)\"(.*?)</(tr|section)>",RegexOptions.Singleline);
					Assert.AreEqual(4,matchCollection.Count,"A row and a card for each picture.");
					foreach(Match match in matchCollection) {
						string picture=System.Net.WebUtility.HtmlDecode(match.Groups[3].Value);
						List<string> listInside=Regex.Matches(match.Groups[4].Value,"data-pic=\"([^\"]*)\"").Cast<Match>().Select(x => System.Net.WebUtility.HtmlDecode(x.Groups[1].Value)).ToList();
						Assert.AreEqual(3,listInside.Count,picture);
						Assert.IsTrue(listInside.All(x => x==picture),picture+": "+string.Join(", ",listInside));
					}
					//The page keeps the SHA-256 and size an answer was given for, and counts, shows and writes only an answer that still matches.
					StringAssert.Contains(html,"a=answers[p]={sha:pics[p].sha,size:pics[p].size}");
					StringAssert.Contains(html,"a.sha===c.sha&&a.size===c.size");
					StringAssert.Contains(html,"var a=got(el.getAttribute('data-pic'))||{};");
					StringAssert.Contains(html,"q('section.pic').forEach(function(el){var a=got(el.getAttribute('data-picture'));if(!a");
					Assert.IsFalse(html.Contains("answers[el.getAttribute('data-id')]"),"No answer is looked up by place.");
				}
				Assert.AreNotEqual(ShaOf(htmlFirst,pictureX),ShaOf(htmlSecond,pictureX),"X changed between the builds, so an answer saved for the first X does not count.");
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>The picture (decoded) of the card with the given place id.</summary>
		private static string PictureAt(string html,string id) {
			Match match=Regex.Match(html,"<section class=\"pic\" id=\""+id+"\" data-id=\""+id+"\" data-picture=\"([^\"]*)\"");
			Assert.IsTrue(match.Success,id);
			return System.Net.WebUtility.HtmlDecode(match.Groups[1].Value);
		}

		///<summary>The data-sha of the picture's card.</summary>
		private static string ShaOf(string html,string picture) {
			Match match=Regex.Match(html,"<section class=\"pic\"[^>]* data-picture=\""+Regex.Escape(System.Net.WebUtility.HtmlEncode(picture))+"\"[^>]* data-sha=\"([0-9a-f]{64})\"");
			Assert.IsTrue(match.Success,picture);
			return match.Groups[1].Value;
		}

		///<summary>The script's helpers, dot-sourced: the group of a picture and the JavaScript string of the gallery's name.</summary>
		[TestMethod]
		public void Builder_Helpers_GroupAndJsString() {
			string folder=TestPaths.NewTempFolder("ui0a_helpers");
			try {
				string driver=Path.Combine(folder,"driver.ps1");
				File.WriteAllText(driver,"$ErrorActionPreference='Stop'\r\n. '"+ToolsPath(SCRIPT)+"'\r\n"
					+"foreach($p in 'A1-main.png','U12-claims.png','sub/X.png','ab12.png','main.png','1a.png'){ \"GROUP|$p|$(Get-UiGalleryGroup -Picture $p)\" }\r\n"
					+"\"JS|$(ConvertTo-UiJsString -Text \"G-UI1 'x' </script>\")\"\r\n"
					+"\"PCT|$(Format-UiPercent 0.004)|$(Format-UiPercent 0)|$(Format-UiPercent 12.346)\"\r\n",Encoding.ASCII);
				ScriptResult scriptResult=RunScript(driver);
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				foreach(string expected in new[] { "GROUP|A1-main.png|A","GROUP|U12-claims.png|U","GROUP|sub/X.png|sub","GROUP|ab12.png|AB","GROUP|main.png|other",
					"GROUP|1a.png|other","JS|'G-UI1 \\u0027x\\u0027 \\u003c/script\\u003e'","PCT|<0.01|0.00|12.35" })
				{
					StringAssert.Contains(scriptResult.Output,expected);
				}
				Assert.IsFalse(scriptResult.Output.Contains("Gallery "),"Dot-sourcing runs nothing.");
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>The builder, its mask file and the rule's source are ASCII (Windows PowerShell 5.1 reads a file without a byte order mark in the ANSI
		///code page) and hold no secret: no thumbprint, no password or key value, no address but the loopback one and the documentation range, no host
		///name but example.test ones (the checks of the owner's scripts in LaunchCheckScriptTests). The script parses without errors.</summary>
		[TestMethod]
		public void Builder_IsAscii_AndHoldsNoSecret() {
			foreach(string path in new[] { ToolsPath(SCRIPT),ToolsPath("ui-gallery-masks.tsv"),ToolsPath("UiGallery","UiPictureMask.cs") }) {
				string name=Path.GetFileName(path);
				byte[] arrayBytes=File.ReadAllBytes(path);
				Assert.IsFalse(arrayBytes.Any(x => x>=0x80),name+": ASCII only.");
				string text=Encoding.ASCII.GetString(arrayBytes);
				Assert.IsFalse(Regex.IsMatch(text,@"\b[0-9A-Fa-f]{40}\b"),name+": no certificate thumbprint.");
				Assert.IsFalse(Regex.IsMatch(text,@"(?i)(password|pwd|secret|apikey|api_key)\s*[=:]\s*['""][^'""\s$]+['""]"),name+": no password or key value.");
				Assert.IsFalse(Regex.IsMatch(text,@"(?i)BEGIN (RSA |EC )?PRIVATE KEY"),name+": no key.");
				foreach(Match match in Regex.Matches(text,@"(?<![\d.])\d{1,3}(\.\d{1,3}){3}(?![\d.])")) {
					Assert.IsTrue(match.Value=="127.0.0.1" || match.Value.StartsWith("192.0.2."),name+": no address but 127.0.0.1 and 192.0.2.x ("+match.Value+").");
				}
				foreach(Match match in Regex.Matches(text,@"\b[a-z0-9-]+(\.[a-z0-9-]+)*\.(com|net|org|io|ms|cloud|local|lan|corp)\b")) {
					Assert.Fail(name+": no host name but example.test ones and localhost ("+match.Value+").");
				}
				Assert.IsFalse(Regex.IsMatch(text,@"(?i)[A-Z]:\\(Users|Dev)\\"),name+": no path of a developer PC.");
			}
			string folder=TestPaths.NewTempFolder("ui0a_parse");
			try {
				string driver=Path.Combine(folder,"parse.ps1");
				File.WriteAllText(driver,"$tokens=$null\r\n$errors=$null\r\n[void][System.Management.Automation.Language.Parser]::ParseFile('"+ToolsPath(SCRIPT)
					+"',[ref]$tokens,[ref]$errors)\r\n\"ERRORS=$($errors.Count)\"\r\n",Encoding.ASCII);
				ScriptResult scriptResult=RunScript(driver);
				StringAssert.Contains(scriptResult.Output,"ERRORS=0");
			}
			finally {
				DeleteFolder(folder);
			}
		}
		#endregion tools/New-UiGallery.ps1
	}
}
