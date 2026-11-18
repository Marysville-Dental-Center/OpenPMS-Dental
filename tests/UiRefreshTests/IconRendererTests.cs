// Tests for package UI-7a (docs/specs/ui-refresh-plan.md, sections 3.5, 4.1 and 4.2, and section 7's UI-7a row; docs/port-notes/UI-7a.md): the
// icon pipeline and the control glyphs. Every manifest row draws non-empty at 16, 20, 22, 24, 32 and 48 px on GDI+, Direct2D and WPF; the OnlyGDI
// path (NoD2D.txt) and NoDpi mode draw the same glyphs; the generated table matches the manifest and the SVGs (tools/New-IconData.ps1 -Check and
// a C# check of every row, file and hash); the design rule of section 3.5; the files RdlDesign may link. Everything is drawn off screen, into
// bitmaps and render targets; no window is made. The Direct2D tests need Direct2dWrapper.dll (built by the full build); WPF's pixel test needs
// a session WPF renders in (a connected one) and is inconclusive otherwise, while the WPF geometry is checked in every session.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class IconRendererTests {
		///<summary>The slot sizes of the row's test list, in device pixels.</summary>
		public static readonly int[] ArraySlotSizes=new int[] { 16,20,22,24,32,48 };
		///<summary>The glyph subset of section 7's UI-7a row, with the Fluent icon each is made from.</summary>
		private static readonly string[][] _arrayGlyphSubset=new string[][] {
			new[] { "Subtract","subtract" },new[] { "Maximize","maximize" },new[] { "SquareMultiple","square_multiple" },new[] { "Dismiss","dismiss" },
			new[] { "ChevronUp","chevron_up" },new[] { "ChevronDown","chevron_down" },new[] { "ChevronLeft","chevron_left" },
			new[] { "ChevronRight","chevron_right" },new[] { "Checkmark","checkmark" },new[] { "Dash","line_horizontal_1" },
		};
		private static readonly object _lockDirect2d=new object();
		private static bool _isDirect2dLoaded;

		#region Helpers
		///<summary>One row of icons/manifest.json.</summary>
		public sealed class ManifestRow {
			public string EnumName;
			public string Name;
			public string Fluent;
			public string Custom;
			public string Style;
			public string Role;
			public int[] Sizes;

			///<summary>The SVG file of a size, relative to third-party/fluentui-system-icons/icons (or custom/... under icons).</summary>
			public string GetFileName(int size) {
				if(!string.IsNullOrEmpty(Custom)) {
					return "custom/"+Custom+"_"+size+".svg";
				}
				return Fluent+"_"+size+"_"+Style+".svg";
			}

			public override string ToString() {
				return EnumName+"."+Name;
			}
		}

		public static List<ManifestRow> ReadManifest(out string fluentVersion) {
			string json=File.ReadAllText(Path.Combine(RepoPaths.Root(),"icons","manifest.json"));
			List<ManifestRow> listRows=new List<ManifestRow>();
			using(JsonDocument jsonDocument=JsonDocument.Parse(json)) {
				fluentVersion=jsonDocument.RootElement.GetProperty("fluentVersion").GetString();
				foreach(JsonElement jsonElement in jsonDocument.RootElement.GetProperty("icons").EnumerateArray()) {
					ManifestRow manifestRow=new ManifestRow();
					manifestRow.EnumName=jsonElement.GetProperty("enum").GetString();
					manifestRow.Name=jsonElement.GetProperty("name").GetString();
					if(jsonElement.TryGetProperty("fluent",out JsonElement jsonElementFluent)) {
						manifestRow.Fluent=jsonElementFluent.GetString();
					}
					if(jsonElement.TryGetProperty("custom",out JsonElement jsonElementCustom)) {
						manifestRow.Custom=jsonElementCustom.GetString();
					}
					manifestRow.Style=jsonElement.GetProperty("style").GetString();
					manifestRow.Role=jsonElement.GetProperty("role").GetString();
					manifestRow.Sizes=jsonElement.GetProperty("sizes").EnumerateArray().Select(x => x.GetInt32()).ToArray();
					listRows.Add(manifestRow);
				}
			}
			return listRows;
		}

		public static List<ManifestRow> ReadManifest() {
			return ReadManifest(out string _);
		}

		///<summary>The design a row draws in a slot (section 3.5).</summary>
		private static IconDesign GetDesign(ManifestRow manifestRow,int sizeSlot) {
			IconDesign iconDesign=UiIcons.GetDesign(UiIcons.GetDesigns(manifestRow.EnumName,manifestRow.Name),sizeSlot);
			Assert.IsNotNull(iconDesign,manifestRow+" has no design in the table.");
			return iconDesign;
		}

		///<summary>The bitmap's pixels as ARGB values, row by row.</summary>
		public static int[] GetPixels(Bitmap bitmap) {
			BitmapData bitmapData=bitmap.LockBits(new Rectangle(0,0,bitmap.Width,bitmap.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
			try {
				int[] arrayPixels=new int[bitmap.Width*bitmap.Height];
				for(int y=0;y<bitmap.Height;y++) {
					Marshal.Copy(bitmapData.Scan0+y*bitmapData.Stride,arrayPixels,y*bitmap.Width,bitmap.Width);
				}
				return arrayPixels;
			}
			finally {
				bitmap.UnlockBits(bitmapData);
			}
		}

		///<summary>The ink of each pixel, 0 to 255: its alpha on a transparent bitmap, or how dark it is on a white one.</summary>
		private static int[] GetInk(Bitmap bitmap,bool isOnWhite) {
			int[] arrayPixels=GetPixels(bitmap);
			int[] arrayInk=new int[arrayPixels.Length];
			for(int i=0;i<arrayPixels.Length;i++) {
				Color color=Color.FromArgb(arrayPixels[i]);
				if(isOnWhite) {
					arrayInk[i]=255-(color.R+color.G+color.B)/3;
				}
				else {
					arrayInk[i]=color.A;
				}
			}
			return arrayInk;
		}

		///<summary>A white bitmap of the slot with the design filled in black with GDI+.</summary>
		private static Bitmap DrawGdiOnWhite(IconDesign iconDesign,int sizeSlot) {
			Bitmap bitmap=new Bitmap(sizeSlot,sizeSlot,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.White);
				UiIcons.Draw(g,iconDesign,new Rectangle(0,0,sizeSlot,sizeSlot),Color.Black);
			}
			return bitmap;
		}

		///<summary>Asserts two drawings of one glyph are the same shape: pixels that one draws clearly (ink above 160) and the other leaves clearly
		///empty (ink below 32) are at most a few, and the total ink is within 15%. Antialiasing differs a little between renderers.</summary>
		private static void AssertSameShape(int[] arrayInkA,int[] arrayInkB,string what) {
			Assert.AreEqual(arrayInkA.Length,arrayInkB.Length,what);
			int countMismatch=0;
			long sumA=0;
			long sumB=0;
			int countInk=0;
			for(int i=0;i<arrayInkA.Length;i++) {
				sumA+=arrayInkA[i];
				sumB+=arrayInkB[i];
				if(arrayInkA[i]>=128 || arrayInkB[i]>=128) {
					countInk++;
				}
				if((arrayInkA[i]>160 && arrayInkB[i]<32) || (arrayInkB[i]>160 && arrayInkA[i]<32)) {
					countMismatch++;
				}
			}
			Assert.IsTrue(sumA>0 && sumB>0,what+": one of the two drew nothing.");
			Assert.IsTrue(countMismatch<=Math.Max(2,countInk/20),what+": "+countMismatch+" pixels differ clearly ("+countInk+" inked).");
			double ratio=(double)sumA/sumB;
			Assert.IsTrue(ratio>0.85 && ratio<1.15,what+": the ink differs by "+((ratio-1)*100).ToString("0.0")+"%.");
		}

		///<summary>Runs the action on a new STA thread (WPF and the Direct2D wrapper, as on the program's UI thread) and rethrows its exception.</summary>
		public static void RunSta(Action action) {
			Exception exceptionThrown=null;
			System.Threading.Thread thread=new System.Threading.Thread(() => {
				try {
					action();
				}
				catch(Exception exception) {
					exceptionThrown=exception;
				}
			});
			thread.SetApartmentState(System.Threading.ApartmentState.STA);
			thread.Start();
			thread.Join();
			if(exceptionThrown!=null) {
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exceptionThrown).Throw();
			}
		}

		///<summary>Loads the program's Direct2dWrapper.dll into the process, so the wrapper's DllImports find it, or ends the test inconclusive when it
		///is not built (the full build builds it; the lane's scoped builds do only when xDirect2dWrapper is named), as Direct2dWrapperX64Tests do.</summary>
		private static void RequireDirect2d() {
			lock(_lockDirect2d) {
				if(_isDirect2dLoaded) {
					return;
				}
				string root=RepoPaths.Root();
				List<string> listCandidates=new List<string> {
					Path.Combine(AppContext.BaseDirectory,"Direct2dWrapper.dll"),
					Path.Combine(root,"OpenPmsDental","bin","Debug","Direct2dWrapper.dll"),
					Path.Combine(root,"OpenPmsDental","bin","Release","Direct2dWrapper.dll"),
					Path.Combine(root,"Direct2dWrapper","bin","x64","Debug","Direct2dWrapper.dll"),
					Path.Combine(root,"Direct2dWrapper","bin","x64","Release","Direct2dWrapper.dll"),
				};
				string path=listCandidates.FirstOrDefault(x => File.Exists(x));
				if(path==null) {
					Assert.Inconclusive("Direct2dWrapper.dll is not built (looked in "+string.Join(", ",listCandidates)+"): build the solution filter, "
						+"which builds xDirect2dWrapper.");
				}
				NativeLibrary.Load(path);
				_isDirect2dLoaded=true;
			}
		}

		///<summary>A white bitmap of the slot with the design filled in black by the program's Direct2D wrapper.</summary>
		private static Bitmap DrawDirect2dOnWhite(IconDesign iconDesign,int sizeSlot) {
			Bitmap bitmap=new Bitmap(sizeSlot,sizeSlot,PixelFormat.Format32bppArgb);
			using(Direct2d direct2d=Direct2d.FromBitmap(bitmap)) {
				direct2d.BeginDraw();
				direct2d.Clear(Color.White);
				direct2d.FillIcon(iconDesign,new Rectangle(0,0,sizeSlot,sizeSlot),Color.Black);
				direct2d.EndDraw();
			}
			return bitmap;
		}

		[DllImport("user32.dll")]
		private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr dpiContext);
		private static readonly IntPtr DPI_AWARENESS_CONTEXT_UNAWARE=new IntPtr(-1);
		private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2=new IntPtr(-4);

		///<summary>Runs the function with this thread in the DPI awareness context, as the program runs with NoDpi.txt (Dpi.SetUnaware) or without.</summary>
		private static T RunInDpiContext<T>(IntPtr dpiContext,Func<T> func) {
			IntPtr dpiContextOld=SetThreadDpiAwarenessContext(dpiContext);
			Assert.AreNotEqual(IntPtr.Zero,dpiContextOld,"SetThreadDpiAwarenessContext failed.");
			try {
				return func();
			}
			finally {
				SetThreadDpiAwarenessContext(dpiContextOld);
			}
		}

		private static string GetSha256(byte[] arrayBytes) {
			using(SHA256 sha256=SHA256.Create()) {
				return string.Concat(sha256.ComputeHash(arrayBytes).Select(x => x.ToString("x2")));
			}
		}

		///<summary>Runs tools\New-IconData.ps1 with the arguments in Windows PowerShell; returns the exit code and the output.</summary>
		private static int RunGenerator(string arguments,out string output) {
			string script=Path.Combine(RepoPaths.Root(),"tools","New-IconData.ps1");
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe","-NoProfile -ExecutionPolicy Bypass -File \""+script+"\" "+arguments);
			processStartInfo.UseShellExecute=false;
			processStartInfo.CreateNoWindow=true;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			using(Process process=Process.Start(processStartInfo)) {
				System.Threading.Tasks.Task<string> taskError=process.StandardError.ReadToEndAsync();
				string standardOutput=process.StandardOutput.ReadToEnd();
				Assert.IsTrue(process.WaitForExit(180000),"New-IconData.ps1 did not end in 3 minutes.");
				output=standardOutput+taskError.Result;
				return process.ExitCode;
			}
		}
		#endregion Helpers

		#region The table: manifest, SVGs and the generated file
		[TestMethod]
		public void Generator_Check_TheCommittedFileIsTheOutputOfTheManifestAndTheSvgs() {
			int exitCode=RunGenerator("-Check",out string output);
			Assert.AreEqual(0,exitCode,output);
			StringAssert.Contains(output,"is up to date");
		}

		[TestMethod]
		public void Table_HoldsEveryManifestRowAndSize_InOrder_WithTheSvgsHashes() {
			List<ManifestRow> listRows=ReadManifest(out string fluentVersion);
			Assert.AreEqual(fluentVersion,IconData.FLUENT_VERSION);
			string root=RepoPaths.Root();
			string folderVendored=Path.Combine(root,"third-party","fluentui-system-icons");
			string sourceTxt=File.ReadAllText(Path.Combine(folderVendored,"SOURCE.txt"));
			List<IconDesign> listDesigns=UiIcons.GetAllDesigns().ToList();
			int index=0;
			HashSet<string> hashSetFiles=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach(ManifestRow manifestRow in listRows) {
				Assert.IsTrue(Enum.IsDefined(typeof(EnumUiColor),manifestRow.Role),manifestRow+": the role "+manifestRow.Role+" is not a token.");
				foreach(int size in manifestRow.Sizes) {
					Assert.IsTrue(index<listDesigns.Count,"The table ends before "+manifestRow+" "+size+".");
					IconDesign iconDesign=listDesigns[index];
					index++;
					Assert.AreEqual(manifestRow.EnumName,iconDesign.EnumName);
					Assert.AreEqual(manifestRow.Name,iconDesign.Name);
					Assert.AreEqual(manifestRow.Role,iconDesign.Role);
					Assert.AreEqual(size,iconDesign.Size);
					Assert.AreEqual(manifestRow.GetFileName(size),iconDesign.FileName);
					string pathSvg=string.IsNullOrEmpty(manifestRow.Custom)
						? Path.Combine(folderVendored,"icons",iconDesign.FileName)
						: Path.Combine(root,"icons",iconDesign.FileName.Replace('/','\\'));
					Assert.IsTrue(File.Exists(pathSvg),pathSvg);
					hashSetFiles.Add(Path.GetFullPath(pathSvg));
					string sha256=GetSha256(File.ReadAllBytes(pathSvg));
					Assert.AreEqual(sha256,iconDesign.Sha256,iconDesign+": the SVG is not the one the table was made from. Run tools\\New-IconData.ps1.");
					if(string.IsNullOrEmpty(manifestRow.Custom)) {
						StringAssert.Contains(sourceTxt,sha256+"  icons/"+iconDesign.FileName,"SOURCE.txt lists "+iconDesign.FileName+" with its SHA-256.");
					}
					//The SVG's viewBox is the design size, and the figures stay inside it.
					string svg=File.ReadAllText(pathSvg);
					StringAssert.Contains(svg,"viewBox=\"0 0 "+size+" "+size+"\"",pathSvg);
					IconFigures iconFigures=iconDesign.GetFigures();
					Assert.IsTrue(iconFigures.CountFigures>=1,iconDesign+" has no figure.");
					RectangleF rectangleBounds=iconFigures.GetBounds();
					Assert.IsTrue(rectangleBounds.Width>0 && rectangleBounds.Height>0,iconDesign+" is empty.");
					Assert.IsTrue(rectangleBounds.Left>=-0.01f && rectangleBounds.Top>=-0.01f && rectangleBounds.Right<=size+0.01f
						&& rectangleBounds.Bottom<=size+0.01f,iconDesign+" leaves its design square: "+rectangleBounds);
				}
			}
			Assert.AreEqual(listDesigns.Count,index,"The table holds designs that are not in the manifest.");
			//Every vendored SVG is in the manifest, and nothing else is vendored but the licence and SOURCE.txt.
			foreach(string path in Directory.GetFiles(Path.Combine(folderVendored,"icons"),"*",SearchOption.AllDirectories)) {
				Assert.IsTrue(hashSetFiles.Contains(Path.GetFullPath(path)),path+" is in no manifest row.");
			}
			CollectionAssert.AreEquivalent(new[] { "LICENSE","SOURCE.txt" },Directory.GetFiles(folderVendored).Select(Path.GetFileName).ToArray());
			CollectionAssert.AreEquivalent(new[] { "icons" },Directory.GetDirectories(folderVendored).Select(Path.GetFileName).ToArray());
			//The manifest's hash (line ends as LF).
			string manifest=File.ReadAllText(Path.Combine(root,"icons","manifest.json")).Replace("\r\n","\n");
			Assert.AreEqual(GetSha256(new UTF8Encoding(false).GetBytes(manifest)),IconData.MANIFEST_SHA256,"Run tools\\New-IconData.ps1.");
		}

		[TestMethod]
		public void UiGlyph_IsNoneThenTheManifestsUiGlyphRows_AndHoldsTheRowsGlyphSubset() {
			List<ManifestRow> listRows=ReadManifest().FindAll(x => x.EnumName=="UiGlyph");
			CollectionAssert.AreEqual(new[] { "None" }.Concat(listRows.Select(x => x.Name)).ToArray(),Enum.GetNames(typeof(UiGlyph)));
			Assert.AreEqual(0,(int)UiGlyph.None);
			foreach(string[] arrayGlyph in _arrayGlyphSubset) {
				ManifestRow manifestRow=listRows.SingleOrDefault(x => x.Name==arrayGlyph[0]);
				Assert.IsNotNull(manifestRow,"The glyph "+arrayGlyph[0]+" of section 7's UI-7a row is missing.");
				Assert.AreEqual(arrayGlyph[1],manifestRow.Fluent,manifestRow.ToString());
				Assert.AreEqual("regular",manifestRow.Style,manifestRow.ToString());
			}
			Assert.AreEqual(0,UiIcons.GetDesigns(UiGlyph.None).Length);
			Assert.IsNull(UiIcons.GetDesign(UiGlyph.None,16));
		}

		[TestMethod]
		public void Generator_ConvertsArcsLinesAndRelativeCommands_AndRefusesAnUnlistedSvg() {
			string folderRoot=Path.Combine(Path.GetTempPath(),"ui7a-icondata-"+Guid.NewGuid().ToString("N"));
			try {
				Directory.CreateDirectory(Path.Combine(folderRoot,"icons","custom"));
				Directory.CreateDirectory(Path.Combine(folderRoot,"third-party","fluentui-system-icons","icons"));
				File.WriteAllText(Path.Combine(folderRoot,"icons","manifest.json"),"{ \"fluentVersion\": \"1.2.3\", \"icons\": [\n"
					+"{ \"enum\": \"UiGlyph\", \"name\": \"Square\", \"custom\": \"square\", \"style\": \"regular\", \"sizes\": [ 16 ], \"role\": \"Text\", \"purpose\": \"test\" },\n"
					+"{ \"enum\": \"UiGlyph\", \"name\": \"Ring\", \"custom\": \"ring\", \"style\": \"regular\", \"sizes\": [ 16 ], \"role\": \"Accent\", \"purpose\": \"test\" } ] }\n");
				//A square of H, V and relative lines; a circle of radius 6 around (8,8) from two arcs with packed flags, and a hole of radius 3.
				File.WriteAllText(Path.Combine(folderRoot,"icons","custom","square_16.svg"),
					"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 16 16\"><path d=\"M2 2h12v12H2z\"/></svg>");
				File.WriteAllText(Path.Combine(folderRoot,"icons","custom","ring_16.svg"),
					"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 16 16\"><path d=\"M2 8a6 6 0 1012 0A6 6 0 102 8zm3 0a3 3 0 006 0 3 3 0 00-6 0z\"/></svg>");
				string pathOut=Path.Combine(folderRoot,"IconData.g.cs");
				int exitCode=RunGenerator("-Root \""+folderRoot+"\" -OutFile \""+pathOut+"\"",out string output);
				Assert.AreEqual(0,exitCode,output);
				string generated=File.ReadAllText(pathOut);
				StringAssert.Contains(generated,"\"M 2 2 L 14 2 L 14 14 L 2 14 Z\"");
				StringAssert.Contains(generated,"public const string FLUENT_VERSION=\"1.2.3\";");
				StringAssert.Contains(generated,"\t\tSquare,\r\n");
				Assert.IsTrue(generated.All(x => x<127),"The output is ASCII.");
				string pathData=Regex.Match(generated,"\"Ring\",\"Accent\",16,\"custom/ring_16.svg\",\"[0-9a-f]{64}\",\"(?<d>[^\"]+)\"").Groups["d"].Value;
				Assert.AreNotEqual("",pathData,generated);
				Assert.IsFalse(Regex.IsMatch(pathData,"[^MLCZ0-9 .\\-]"),"Only M, L, C and Z: "+pathData);
				IconFigures iconFigures=IconFigures.Parse(pathData);
				Assert.AreEqual(2,iconFigures.CountFigures);
				//Every curve's points (ends and the curve at t=0.5) lie on the circles.
				RingSink ringSink=new RingSink();
				iconFigures.WriteTo(ringSink,0,0,1);
				Assert.IsTrue(ringSink.CountCurves>=6,"Each arc of 180 degrees became two curves or more: "+ringSink.CountCurves);
				foreach(PointF point in ringSink.ListPoints) {
					double radius=Math.Sqrt((point.X-8)*(point.X-8)+(point.Y-8)*(point.Y-8));
					Assert.IsTrue(Math.Abs(radius-6)<0.02 || Math.Abs(radius-3)<0.02,"A point off both circles: "+point+" (r="+radius+")");
				}
				//The same run with -Check passes, and fails once the file differs.
				Assert.AreEqual(0,RunGenerator("-Root \""+folderRoot+"\" -OutFile \""+pathOut+"\" -Check",out output),output);
				File.AppendAllText(pathOut,"//changed\r\n");
				Assert.AreEqual(1,RunGenerator("-Root \""+folderRoot+"\" -OutFile \""+pathOut+"\" -Check",out output),output);
				//An SVG that no row names is refused.
				File.WriteAllText(Path.Combine(folderRoot,"icons","custom","extra_16.svg"),File.ReadAllText(Path.Combine(folderRoot,"icons","custom","square_16.svg")));
				Assert.AreNotEqual(0,RunGenerator("-Root \""+folderRoot+"\" -OutFile \""+pathOut+"\"",out output));
				StringAssert.Contains(output,"extra_16.svg");
				StringAssert.Contains(output,"is in no manifest row");
			}
			finally {
				if(Directory.Exists(folderRoot)) {
					Directory.Delete(folderRoot,recursive:true);
				}
			}
		}

		///<summary>Collects the end points of every segment and the point at t=0.5 of every curve.</summary>
		private sealed class RingSink:IIconPathSink {
			public List<PointF> ListPoints=new List<PointF>();
			public int CountCurves;
			private PointF _pointCurrent;

			public void BeginFigure(float x,float y) {
				_pointCurrent=new PointF(x,y);
				ListPoints.Add(_pointCurrent);
			}

			public void AddLine(float x,float y) {
				_pointCurrent=new PointF(x,y);
				ListPoints.Add(_pointCurrent);
			}

			public void AddBezier(float x1,float y1,float x2,float y2,float x,float y) {
				float xMid=0.125f*_pointCurrent.X+0.375f*x1+0.375f*x2+0.125f*x;
				float yMid=0.125f*_pointCurrent.Y+0.375f*y1+0.375f*y2+0.125f*y;
				ListPoints.Add(new PointF(xMid,yMid));
				_pointCurrent=new PointF(x,y);
				ListPoints.Add(_pointCurrent);
				CountCurves++;
			}

			public void EndFigure() {
			}
		}
		#endregion The table: manifest, SVGs and the generated file

		#region Section 3.5: the design for a slot
		[TestMethod]
		public void Design_IsTheLargestThatFits_NeverStretched_Centred() {
			Assert.AreEqual(16,UiIcons.GetDesign(UiGlyph.Dismiss,16).Size);
			Assert.AreEqual(20,UiIcons.GetDesign(UiGlyph.Dismiss,22).Size,"The 22 px slot takes the 20 design, centred.");
			Assert.AreEqual(new Rectangle(1,1,20,20),UiIcons.GetDrawRectangle(UiIcons.GetDesign(UiGlyph.Dismiss,22),new Rectangle(0,0,22,22)));
			Assert.AreEqual(24,UiIcons.GetDesign(UiGlyph.Dismiss,31).Size,"24 to 31 px take the 24 design.");
			Assert.AreEqual(32,UiIcons.GetDesign(UiGlyph.Dismiss,47).Size);
			Assert.AreEqual(48,UiIcons.GetDesign(UiGlyph.Dismiss,100).Size);
			Assert.AreEqual(new Rectangle(26,26,48,48),UiIcons.GetDrawRectangle(UiIcons.GetDesign(UiGlyph.Dismiss,100),new Rectangle(0,0,100,100)));
			Assert.AreEqual(12,UiIcons.GetDesign(UiGlyph.Dismiss,15).Size);
			//A design Fluent does not have: the largest smaller one (Maximize has no 32, Subtract no 32).
			Assert.AreEqual(24,UiIcons.GetDesign(UiGlyph.Maximize,32).Size);
			Assert.AreEqual(24,UiIcons.GetDesign(UiGlyph.Subtract,32).Size);
			Assert.AreEqual(24,UiIcons.GetDesign(UiGlyph.Dash,48).Size);
			//Below the smallest design: the smallest, shrunk to the slot.
			IconDesign iconDesignDash=UiIcons.GetDesign(UiGlyph.Dash,12);
			Assert.AreEqual(16,iconDesignDash.Size);
			Assert.AreEqual(new Rectangle(0,0,12,12),UiIcons.GetDrawRectangle(iconDesignDash,new Rectangle(0,0,12,12)));
			//A slot that is not square: the largest square, centred.
			Assert.AreEqual(new Rectangle(10,2,16,16),UiIcons.GetDrawRectangle(UiIcons.GetDesign(UiGlyph.ChevronDown,16),new Rectangle(0,0,36,20)));
		}
		#endregion Section 3.5: the design for a slot

		#region GDI+
		[TestMethod]
		public void Gdi_EveryManifestRowDrawsAtEverySize() {
			foreach(ManifestRow manifestRow in ReadManifest()) {
				foreach(int size in ArraySlotSizes) {
					IconDesign iconDesign=GetDesign(manifestRow,size);
					Rectangle rectangleDraw=UiIcons.GetDrawRectangle(iconDesign,new Rectangle(0,0,size,size));
					using(Bitmap bitmap=UiIcons.GetBitmap(iconDesign,size,Color.Black)) {
						Assert.AreEqual(size,bitmap.Width);
						Assert.AreEqual(size,bitmap.Height);
						Assert.AreEqual(PixelFormat.Format32bppArgb,bitmap.PixelFormat);
						int[] arrayInk=GetInk(bitmap,isOnWhite:false);
						int countInk=arrayInk.Count(x => x>=128);
						Assert.IsTrue(countInk>0,manifestRow+" draws nothing at "+size+" px.");
						Assert.IsTrue(countInk<size*size*0.7,manifestRow+" fills most of the slot at "+size+" px.");
						//Nothing outside the drawn square (one pixel of antialiasing allowed).
						Rectangle rectangleAllowed=Rectangle.Inflate(rectangleDraw,1,1);
						for(int i=0;i<arrayInk.Length;i++) {
							if(arrayInk[i]>0) {
								Assert.IsTrue(rectangleAllowed.Contains(i%size,i/size),manifestRow+" at "+size+" px draws outside "+rectangleDraw);
							}
						}
					}
				}
			}
		}

		[TestMethod]
		public void Gdi_GlyphApiDrawsTheDesign_AndKeepsTheGraphicsSettings() {
			foreach(UiGlyph uiGlyph in Enum.GetValues(typeof(UiGlyph)).Cast<UiGlyph>().Where(x => x!=UiGlyph.None)) {
				foreach(int size in ArraySlotSizes) {
					using(Bitmap bitmapGlyph=UiIcons.GetBitmap(uiGlyph,size,Color.Black))
					using(Bitmap bitmapDesign=UiIcons.GetBitmap(UiIcons.GetDesign(uiGlyph,size),size,Color.Black)) {
						CollectionAssert.AreEqual(GetPixels(bitmapDesign),GetPixels(bitmapGlyph),uiGlyph+" "+size);
					}
				}
			}
			//Draw at an offset is the same picture moved, and leaves the Graphics' settings as they were.
			using(Bitmap bitmapAt0=UiIcons.GetBitmap(UiGlyph.Checkmark,20,Color.Black))
			using(Bitmap bitmapMoved=new Bitmap(40,40,PixelFormat.Format32bppArgb)) {
				using(Graphics g=Graphics.FromImage(bitmapMoved)) {
					g.Clear(Color.Transparent);
					g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.None;
					UiIcons.Draw(g,UiGlyph.Checkmark,new Rectangle(11,7,20,20),Color.Black);
					Assert.AreEqual(System.Drawing.Drawing2D.SmoothingMode.None,g.SmoothingMode);
					Assert.IsTrue(g.Transform.IsIdentity);
				}
				for(int y=0;y<20;y++) {
					for(int x=0;x<20;x++) {
						Assert.AreEqual(bitmapAt0.GetPixel(x,y).ToArgb(),bitmapMoved.GetPixel(x+11,y+7).ToArgb(),"("+x+","+y+")");
					}
				}
			}
			//The token colours and disabled (TextDisabled, section 4.2 item 6).
			using(Bitmap bitmap=new Bitmap(16,16,PixelFormat.Format32bppArgb)) {
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.Transparent);
					UiIcons.DrawDisabled(g,UiGlyph.Dismiss,new Rectangle(0,0,16,16));
				}
				AssertDrawnInColour(bitmap,UiTheme.GetColor(EnumUiColor.TextDisabled),"DrawDisabled");
			}
			using(GraphicsPathHolder graphicsPathHolder=new GraphicsPathHolder(UiIcons.GetPath(UiGlyph.ChevronDown,new Rectangle(4,4,16,16)))) {
				RectangleF rectangleBounds=graphicsPathHolder.GraphicsPath.GetBounds();
				Assert.IsTrue(rectangleBounds.Left>=4 && rectangleBounds.Right<=20 && rectangleBounds.Width>4,rectangleBounds.ToString());
			}
		}

		///<summary>The most covered pixel of a glyph on a transparent bitmap has the colour (GDI+ keeps the colour and puts the coverage in the alpha;
		///the thin strokes of a 16 px glyph cover no pixel fully).</summary>
		private static void AssertDrawnInColour(Bitmap bitmap,Color color,string what) {
			Color colorMost=GetPixels(bitmap).Select(x => Color.FromArgb(x)).OrderByDescending(x => x.A).First();
			Assert.IsTrue(colorMost.A>=128,what+": nothing drawn.");
			Assert.IsTrue(Math.Abs(colorMost.R-color.R)<=2 && Math.Abs(colorMost.G-color.G)<=2 && Math.Abs(colorMost.B-color.B)<=2,
				what+": "+colorMost+", not "+color);
		}

		private sealed class GraphicsPathHolder:IDisposable {
			public readonly System.Drawing.Drawing2D.GraphicsPath GraphicsPath;

			public GraphicsPathHolder(System.Drawing.Drawing2D.GraphicsPath graphicsPath) {
				GraphicsPath=graphicsPath;
			}

			public void Dispose() {
				GraphicsPath.Dispose();
			}
		}

		[TestMethod]
		public void Gdi_GetBitmapScales_AndGetImageListHoldsEveryGlyph() {
			using(Bitmap bitmap=UiIcons.GetBitmap(UiGlyph.ChevronRight,16,1.5f,Color.Black)) {
				Assert.AreEqual(24,bitmap.Width,"16 px at 150%.");
				Assert.AreEqual(96f,bitmap.HorizontalResolution);
			}
			using(Bitmap bitmap=UiIcons.GetBitmap(UiGlyph.ChevronRight,16,1.25f,UiTheme.GetColor(EnumUiColor.Accent))) {
				Assert.AreEqual(20,bitmap.Width,"16 px at 125%.");
				AssertDrawnInColour(bitmap,UiTheme.GetColor(EnumUiColor.Accent),"GetBitmap in Accent");
			}
			UiGlyph[] arrayGlyphs=Enum.GetValues(typeof(UiGlyph)).Cast<UiGlyph>().Where(x => x!=UiGlyph.None).ToArray();
			foreach(int size in new[] { 16,20,24,32 }) {
				using(ImageList imageList=UiIcons.GetImageList(arrayGlyphs,size,EnumUiColor.Text)) {
					Assert.AreEqual(ColorDepth.Depth32Bit,imageList.ColorDepth);
					Assert.AreEqual(new Size(size,size),imageList.ImageSize);
					Assert.AreEqual(arrayGlyphs.Length,imageList.Images.Count);
					for(int i=0;i<arrayGlyphs.Length;i++) {
						using(Bitmap bitmap=(Bitmap)imageList.Images[i]) {
							Assert.IsTrue(GetInk(bitmap,isOnWhite:false).Any(x => x>=128),arrayGlyphs[i]+" is empty in the ImageList at "+size+" px.");
						}
					}
				}
			}
		}

		[TestMethod]
		public void Gdi_DrawsOnTwoThreadsAtOnce() {
			//Each thread has its own paths (GDI+ objects are not thread-safe).
			Exception exceptionThrown=null;
			System.Threading.Thread[] arrayThreads=new System.Threading.Thread[2];
			for(int t=0;t<arrayThreads.Length;t++) {
				arrayThreads[t]=new System.Threading.Thread(() => {
					try {
						using(Bitmap bitmap=new Bitmap(48,48,PixelFormat.Format32bppArgb))
						using(Graphics g=Graphics.FromImage(bitmap)) {
							for(int i=0;i<400;i++) {
								foreach(int size in ArraySlotSizes) {
									UiIcons.Draw(g,(UiGlyph)(1+i%10),new Rectangle(0,0,size,size),Color.Black);
								}
							}
						}
					}
					catch(Exception exception) {
						exceptionThrown=exception;
					}
				});
				arrayThreads[t].Start();
			}
			foreach(System.Threading.Thread thread in arrayThreads) {
				thread.Join();
			}
			Assert.IsNull(exceptionThrown,exceptionThrown?.ToString());
		}
		#endregion GDI+

		#region Direct2D
		[TestMethod]
		public void Direct2d_EveryManifestRowDrawsAtEverySize_AsGdiDoes() {
			RequireDirect2d();
			RunSta(() => {
				foreach(ManifestRow manifestRow in ReadManifest()) {
					foreach(int size in ArraySlotSizes) {
						IconDesign iconDesign=GetDesign(manifestRow,size);
						using(Bitmap bitmapDirect2d=DrawDirect2dOnWhite(iconDesign,size))
						using(Bitmap bitmapGdi=DrawGdiOnWhite(iconDesign,size)) {
							int[] arrayInkDirect2d=GetInk(bitmapDirect2d,isOnWhite:true);
							Assert.IsTrue(arrayInkDirect2d.Count(x => x>=128)>0,manifestRow+" draws nothing on Direct2D at "+size+" px.");
							AssertSameShape(arrayInkDirect2d,GetInk(bitmapGdi,isOnWhite:true),manifestRow+" at "+size+" px, Direct2D and GDI+");
						}
					}
				}
			});
		}

		[TestMethod]
		public void Direct2d_OnlyGdiPath_DrawsTheSameGlyphs() {
			//NoD2D.txt sets IconLibrary.OnlyGDI (FormOpenDental); Direct2d.DrawGlyph then fills with GDI+.
			RequireDirect2d();
			RunSta(() => {
				bool isOnlyGdiOld=IconLibrary.OnlyGDI;
				try {
					foreach(UiGlyph uiGlyph in Enum.GetValues(typeof(UiGlyph)).Cast<UiGlyph>().Where(x => x!=UiGlyph.None)) {
						foreach(int size in ArraySlotSizes) {
							IconLibrary.OnlyGDI=false;
							int[] arrayInkDirect2d=DrawGlyphOnWhite(uiGlyph,size);
							Assert.IsFalse(IconLibrary.OnlyGDI,"Direct2D failed and DrawGlyph fell back to GDI+.");
							IconLibrary.OnlyGDI=true;
							int[] arrayInkGdi=DrawGlyphOnWhite(uiGlyph,size);
							AssertSameShape(arrayInkDirect2d,arrayInkGdi,uiGlyph+" at "+size+" px, Direct2D and OnlyGDI");
						}
					}
				}
				finally {
					IconLibrary.OnlyGDI=isOnlyGdiOld;
				}
			});
		}

		///<summary>The ink of Direct2d.DrawGlyph on a white bitmap, the slot at (3,2) of a slightly larger bitmap.</summary>
		private static int[] DrawGlyphOnWhite(UiGlyph uiGlyph,int size) {
			using(Bitmap bitmap=new Bitmap(size+6,size+4,PixelFormat.Format32bppArgb)) {
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.White);
					Direct2d.DrawGlyph(g,uiGlyph,new Rectangle(3,2,size,size),Color.Black,Color.White);
				}
				return GetInk(bitmap,isOnWhite:true);
			}
		}
		#endregion Direct2D

		#region NoDpi
		[TestMethod]
		public void NoDpi_DrawsTheSameGlyphs() {
			//With NoDpi.txt the program runs DPI-unaware (Dpi.SetUnaware) and lays out at 96 DPI: a slot's size is its 96 DPI size, scale 1. The
			//glyphs are the same pixels as at 100% in a DPI-aware thread, on GDI+ and on Direct2D.
			bool isDirect2d=true;
			try {
				RequireDirect2d();
			}
			catch(AssertInconclusiveException) {
				isDirect2d=false;
			}
			RunSta(() => {
				foreach(UiGlyph uiGlyph in Enum.GetValues(typeof(UiGlyph)).Cast<UiGlyph>().Where(x => x!=UiGlyph.None)) {
					foreach(int size96 in ArraySlotSizes) {
						int[] arrayAware=RunInDpiContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2,() => {
							using(Bitmap bitmap=UiIcons.GetBitmap(uiGlyph,size96,Color.Black)) {
								return GetPixels(bitmap);
							}
						});
						int[] arrayUnaware=RunInDpiContext(DPI_AWARENESS_CONTEXT_UNAWARE,() => {
							using(Bitmap bitmap=UiIcons.GetBitmap(uiGlyph,size96,1f,Color.Black)) {
								return GetPixels(bitmap);
							}
						});
						CollectionAssert.AreEqual(arrayAware,arrayUnaware,uiGlyph+" at "+size96+" px, GDI+ DPI-aware and NoDpi");
						if(!isDirect2d) {
							continue;
						}
						IconDesign iconDesign=UiIcons.GetDesign(uiGlyph,size96);
						int[] arrayAwareDirect2d=RunInDpiContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2,() => {
							using(Bitmap bitmap=DrawDirect2dOnWhite(iconDesign,size96)) {
								return GetPixels(bitmap);
							}
						});
						int[] arrayUnawareDirect2d=RunInDpiContext(DPI_AWARENESS_CONTEXT_UNAWARE,() => {
							using(Bitmap bitmap=DrawDirect2dOnWhite(iconDesign,size96)) {
								return GetPixels(bitmap);
							}
						});
						CollectionAssert.AreEqual(arrayAwareDirect2d,arrayUnawareDirect2d,uiGlyph+" at "+size96+" px, Direct2D DPI-aware and NoDpi");
					}
				}
			});
			if(!isDirect2d) {
				Assert.Inconclusive("GDI+ checked; the Direct2D half needs Direct2dWrapper.dll (the full build).");
			}
		}
		#endregion NoDpi

		#region WPF
		[TestMethod]
		public void Wpf_EveryManifestRowHasItsGeometryAtEverySize_AsGdiDraws() {
			//Geometry only (FillContains), which needs no rendering: it holds in a disconnected session too.
			RunSta(() => {
				foreach(ManifestRow manifestRow in ReadManifest()) {
					foreach(int size in ArraySlotSizes) {
						IconDesign iconDesign=GetDesign(manifestRow,size);
						System.Windows.Media.Geometry geometry=UiIcons.GetGeometryWpf(iconDesign,size,1);
						Assert.IsTrue(geometry.IsFrozen,manifestRow+" "+size);
						Assert.IsFalse(geometry.IsEmpty(),manifestRow+" has no WPF geometry at "+size+" px.");
						System.Windows.Rect rectBounds=geometry.Bounds;
						Assert.IsTrue(rectBounds.Left>=-0.01 && rectBounds.Top>=-0.01 && rectBounds.Right<=size+0.01 && rectBounds.Bottom<=size+0.01,
							manifestRow+" leaves its slot at "+size+" px: "+rectBounds);
						Assert.AreSame(geometry,UiIcons.GetGeometryWpf(iconDesign,size,1),"Cached.");
						using(Bitmap bitmap=UiIcons.GetBitmap(iconDesign,size,Color.Black)) {
							int[] arrayInk=GetInk(bitmap,isOnWhite:false);
							int countMismatch=0;
							for(int i=0;i<arrayInk.Length;i++) {
								if(arrayInk[i]!=255 && arrayInk[i]!=0) {
									continue;//an edge pixel
								}
								bool isInside=geometry.FillContains(new System.Windows.Point(i%size+0.5,i/size+0.5));
								if(isInside!=(arrayInk[i]==255)) {
									countMismatch++;
								}
							}
							Assert.IsTrue(countMismatch<=Math.Max(1,size*size/100),manifestRow+" at "+size+" px: "+countMismatch+" pixel centres differ from GDI+.");
						}
					}
				}
				//At 150% a 16 DIP slot is 24 device pixels: the 24 design, 16 DIPs wide.
				System.Windows.Media.Geometry geometry150=UiIcons.GetGeometryWpf(UiGlyph.Dismiss,16,1.5);
				Assert.IsTrue(geometry150.Bounds.Width>10 && geometry150.Bounds.Right<=16.01,geometry150.Bounds.ToString());
				Assert.IsTrue(UiIcons.GetGeometryWpf(UiGlyph.None,16).IsEmpty());
				System.Windows.Media.DrawingImage drawingImage=UiIcons.GetImageSourceWpf(UiGlyph.Checkmark,16,System.Windows.Media.Colors.Black);
				Assert.IsTrue(drawingImage.IsFrozen);
				Assert.AreEqual(16,drawingImage.Width,0.01);
				Assert.AreEqual(16,drawingImage.Height,0.01);
			});
		}

		[TestMethod]
		public void Wpf_EveryManifestRowRendersAtEverySize_AsGdiDraws() {
			//WPF's software renderer draws into a RenderTargetBitmap without a window; in a session that WPF does not render in (the owner's
			//desktop disconnected) the bitmap stays empty, so a plain square is drawn first and the test is inconclusive when even that is blank.
			string inconclusive=null;
			RunSta(() => {
				if(!RendersWpf()) {
					inconclusive="WPF draws nothing in this session (a filled square stays transparent): the WPF pixel test waits for a connected "
						+"session. The WPF geometry of every row is checked by Wpf_EveryManifestRowHasItsGeometryAtEverySize_AsGdiDraws.";
					return;
				}
				foreach(ManifestRow manifestRow in ReadManifest()) {
					foreach(int size in ArraySlotSizes) {
						IconDesign iconDesign=GetDesign(manifestRow,size);
						System.Windows.Media.DrawingVisual drawingVisual=new System.Windows.Media.DrawingVisual();
						using(System.Windows.Media.DrawingContext drawingContext=drawingVisual.RenderOpen()) {
							drawingContext.DrawGeometry(System.Windows.Media.Brushes.Black,null,UiIcons.GetGeometryWpf(iconDesign,size,1));
						}
						int[] arrayInkWpf=GetInk(RenderToBitmap(drawingVisual,size),isOnWhite:false);
						Assert.IsTrue(arrayInkWpf.Count(x => x>=128)>0,manifestRow+" renders nothing on WPF at "+size+" px.");
						using(Bitmap bitmapGdi=UiIcons.GetBitmap(iconDesign,size,Color.Black)) {
							AssertSameShape(arrayInkWpf,GetInk(bitmapGdi,isOnWhite:false),manifestRow+" at "+size+" px, WPF and GDI+");
						}
					}
				}
			});
			if(inconclusive!=null) {
				Assert.Inconclusive(inconclusive);
			}
		}

		///<summary>Whether WPF renders in this session: a filled square in a RenderTargetBitmap.</summary>
		private static bool RendersWpf() {
			System.Windows.Media.DrawingVisual drawingVisual=new System.Windows.Media.DrawingVisual();
			using(System.Windows.Media.DrawingContext drawingContext=drawingVisual.RenderOpen()) {
				drawingContext.DrawRectangle(System.Windows.Media.Brushes.Black,null,new System.Windows.Rect(2,2,12,12));
			}
			return GetInk(RenderToBitmap(drawingVisual,16),isOnWhite:false).Any(x => x>=128);
		}

		///<summary>A visual rendered at 96 DPI into a size x size bitmap (WPF's software renderer; no window), as a GDI+ bitmap.</summary>
		private static Bitmap RenderToBitmap(System.Windows.Media.Visual visual,int size) {
			System.Windows.Media.Imaging.RenderTargetBitmap renderTargetBitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(size,size,96,96,
				System.Windows.Media.PixelFormats.Pbgra32);
			renderTargetBitmap.Render(visual);
			byte[] arrayBytes=new byte[size*size*4];
			renderTargetBitmap.CopyPixels(arrayBytes,size*4,0);
			Bitmap bitmap=new Bitmap(size,size,PixelFormat.Format32bppPArgb);
			BitmapData bitmapData=bitmap.LockBits(new Rectangle(0,0,size,size),ImageLockMode.WriteOnly,PixelFormat.Format32bppPArgb);
			for(int y=0;y<size;y++) {
				Marshal.Copy(arrayBytes,y*size*4,bitmapData.Scan0+y*bitmapData.Stride,size*4);
			}
			bitmap.UnlockBits(bitmapData);
			return bitmap;
		}
		#endregion WPF

		#region Files RdlDesign may link
		[TestMethod]
		public void LinkableFiles_UseOnlySystemDrawingAndWinForms() {
			//The table and the GDI+ renderer use only System and System.Drawing (the ImageList part Windows Forms besides), so RdlDesign, which
			//references no CodeBase, can compile them in as linked files with UiTheme's (UI-9a-7). The WPF part is its own file.
			string folder=Path.Combine(RepoPaths.Root(),"CodeBase","UI","Icons");
			Dictionary<string,string[]> dictAllowed=new Dictionary<string,string[]> {
				{ "IconData.g.cs",new[] { "System" } },
				{ "UiIcons.cs",new[] { "System","System.Collections.Generic","System.Drawing" } },
				{ "UiIcons.Gdi.cs",new[] { "System","System.Collections.Generic","System.Drawing","System.Drawing.Drawing2D","System.Drawing.Imaging" } },
				{ "UiIcons.ImageList.cs",new[] { "System","System.Drawing","System.Windows.Forms" } },
				{ "UiIcons.Wpf.cs",new[] { "System","System.Collections.Generic","System.Windows","System.Windows.Media" } },
			};
			CollectionAssert.AreEquivalent(dictAllowed.Keys.ToArray(),Directory.GetFiles(folder).Select(Path.GetFileName).ToArray(),
				"A new file in CodeBase\\UI\\Icons: add it here, as linkable or as WPF.");
			Regex regexUsing=new Regex(@"^\s*using\s+(?<name>[\w\.]+)\s*;",RegexOptions.Multiline);
			Regex regexWpf=new Regex(@"\bSystem\s*\.\s*Windows\s*\.\s*(?!Forms\b)\w+");
			foreach(KeyValuePair<string,string[]> pair in dictAllowed) {
				string code=File.ReadAllText(Path.Combine(folder,pair.Key));
				foreach(Match match in regexUsing.Matches(code)) {
					CollectionAssert.Contains(pair.Value,match.Groups["name"].Value,pair.Key+" uses "+match.Groups["name"].Value+".");
				}
				StringAssert.Contains(code,"namespace CodeBase {",pair.Key);
				if(pair.Key!="UiIcons.Wpf.cs") {
					string codeNoComments=Regex.Replace(code,@"//[^\n]*","");
					Assert.IsFalse(regexWpf.IsMatch(codeNoComments),pair.Key+" names WPF; that belongs in UiIcons.Wpf.cs.");
					//C# 7.3 (RdlDesign's language version): no using declarations, switch expressions or ??=.
					Assert.IsFalse(Regex.IsMatch(codeNoComments,@"^\s*using\s+[\w\.<>]+\s+\w+\s*=",RegexOptions.Multiline),pair.Key+": a using declaration.");
					Assert.IsFalse(Regex.IsMatch(codeNoComments,@"\bswitch\s*\{"),pair.Key+": a switch expression is C# 8.");
					Assert.IsFalse(codeNoComments.Contains("??="),pair.Key+": ??= is C# 8.");
				}
			}
		}
		#endregion Files RdlDesign may link
	}
}
