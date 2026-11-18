// The print guard's comparison (package UI-2a-3): a render runs once with today's font tokens and default font and once with UI-2b's
// (UiTheme.SetFontForTest(UiFontOption.UI2b), the default font Segoe UI 9), and the two outputs must be the same bytes. Outputs are bitmap
// pixels or a PDF's pages (size, fonts and content stream). PRINT_GUARD_RECORD_DIR=<folder> writes each output's SHA-256 there, one file per
// output, to compare two builds (before and after a change) with the same command.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PrintGuardTests {
	public static class Guard {
		///<summary>UI-2b's default font, as each exe's Main will set it (section 5.1 of the plan).</summary>
		public static Font NewFontDefaultUi2b() {
			return new Font("Segoe UI",9f);
		}

		///<summary>Today's default font, as each exe's Main sets it (Microsoft Sans Serif 8.25; a .NET 10 test process starts with Segoe UI 9).</summary>
		public static Font NewFontDefaultToday() {
			return new Font(UiTheme.FONT_FAMILY_TODAY,8.25f);
		}

		///<summary>Runs funcRender on its own STA thread with today's fonts, then with UI-2b's, puts today's back, and asserts the two outputs are
		///the same bytes. beforeEach runs on that thread before each render (the database connection, for one). Returns today's output.</summary>
		public static byte[] AssertSameWithUi2bFonts(string name,Func<byte[]> funcRender,Action beforeEach=null) {
			byte[][] arrayOutputs=RenderTodayAndUi2b(funcRender,beforeEach);
			byte[] arrayToday=arrayOutputs[0];
			byte[] arrayUi2b=arrayOutputs[1];
			byte[] arrayTodayAgain=arrayOutputs[2];
			Assert.IsTrue(arrayToday!=null && arrayToday.Length>0,name+": the render produced nothing.");
			Record(name,arrayToday);
			//The render must give the same output each time with the same fonts, or a difference would prove nothing.
			if(!arrayToday.SequenceEqual(arrayTodayAgain)) {
				string dir=WriteOutputs(name,arrayToday,arrayTodayAgain,"today-again");
				Assert.Fail(name+": two renders with today's fonts differ (SHA-256 "+Sha256(arrayToday)+" and "+Sha256(arrayTodayAgain)
					+"); the render is not repeatable. Outputs: "+dir);
			}
			if(!arrayToday.SequenceEqual(arrayUi2b)) {
				WriteOutputs(name,arrayToday,arrayUi2b,"ui2b");
				Assert.Fail(name+": the output with UI-2b's fonts differs from today's ("+arrayToday.Length+" and "+arrayUi2b.Length+" bytes, SHA-256 "
					+Sha256(arrayToday)+" and "+Sha256(arrayUi2b)+"). A print path takes a screen font.");
			}
			return arrayToday;
		}

		///<summary>Today's output, UI-2b's, and today's again (after UI-2b's), without comparing them (the positive controls: the screen does
		///change).</summary>
		public static byte[][] RenderTodayAndUi2b(Func<byte[]> funcRender,Action beforeEach=null) {
			byte[] arrayToday=null;
			byte[] arrayUi2b=null;
			byte[] arrayTodayAgain=null;
			StaRunner.Run(() => {
				Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
				Thread.CurrentThread.CurrentUICulture=new CultureInfo("en-US");
				Font fontDefaultBefore=Control.DefaultFont;
				try {
					UiTheme.SetFontForTest(null);
					DefaultFont.Set(NewFontDefaultToday());
					beforeEach?.Invoke();
					arrayToday=funcRender();
					UiTheme.SetFontForTest(UiFontOption.UI2b);
					DefaultFont.Set(NewFontDefaultUi2b());
					beforeEach?.Invoke();
					arrayUi2b=funcRender();
					UiTheme.SetFontForTest(null);
					DefaultFont.Set(NewFontDefaultToday());
					beforeEach?.Invoke();
					arrayTodayAgain=funcRender();
				}
				finally {
					UiTheme.SetFontForTest(null);
					DefaultFont.Set(fontDefaultBefore);
				}
			});
			return new byte[][] { arrayToday,arrayUi2b,arrayTodayAgain };
		}

		///<summary>Writes two outputs that differ to a folder for a look (PRINT_GUARD_RECORD_DIR, or the temp folder) and returns the folder.</summary>
		private static string WriteOutputs(string name,byte[] arrayToday,byte[] arrayOther,string suffixOther) {
			string dir=Environment.GetEnvironmentVariable("PRINT_GUARD_RECORD_DIR");
			if(string.IsNullOrEmpty(dir)) {
				dir=Path.Combine(Path.GetTempPath(),"printguard");
			}
			Directory.CreateDirectory(dir);
			File.WriteAllBytes(Path.Combine(dir,name+"-today.bin"),arrayToday);
			File.WriteAllBytes(Path.Combine(dir,name+"-"+suffixOther+".bin"),arrayOther);
			return dir;
		}

		///<summary>Writes the SHA-256 of an output to PRINT_GUARD_RECORD_DIR\name.sha256 when that variable is set.</summary>
		public static void Record(string name,byte[] arrayBytes) {
			string dir=Environment.GetEnvironmentVariable("PRINT_GUARD_RECORD_DIR");
			if(string.IsNullOrEmpty(dir)) {
				return;
			}
			Directory.CreateDirectory(dir);
			File.WriteAllText(Path.Combine(dir,name+".sha256"),Sha256(arrayBytes)+"\r\n");
		}

		public static string Sha256(byte[] arrayBytes) {
			using(SHA256 sha256=SHA256.Create()) {
				return string.Concat(sha256.ComputeHash(arrayBytes).Select(x => x.ToString("x2")));
			}
		}

		///<summary>A white 96 dpi bitmap of the size, drawn by actionDraw, as its size and 32-bit pixels.</summary>
		public static byte[] DrawToPixels(int width,int height,Action<Graphics> actionDraw) {
			using(Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb)) {
				bitmap.SetResolution(96,96);
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.White);
					actionDraw(g);
				}
				return GetPixels(bitmap);
			}
		}

		///<summary>The bitmap's size and its pixels as 32-bit ARGB, row by row.</summary>
		public static byte[] GetPixels(Bitmap bitmap) {
			using(Bitmap bitmap32=new Bitmap(bitmap.Width,bitmap.Height,PixelFormat.Format32bppArgb)) {
				using(Graphics g=Graphics.FromImage(bitmap32)) {
					g.DrawImageUnscaled(bitmap,0,0);
				}
				Rectangle rectangle=new Rectangle(0,0,bitmap32.Width,bitmap32.Height);
				BitmapData bitmapData=bitmap32.LockBits(rectangle,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
				try {
					byte[] arrayPixels=new byte[bitmapData.Stride*bitmapData.Height];
					Marshal.Copy(bitmapData.Scan0,arrayPixels,0,arrayPixels.Length);
					byte[] arraySize=Encoding.ASCII.GetBytes(bitmap32.Width+"x"+bitmap32.Height+"\n");
					return arraySize.Concat(arrayPixels).ToArray();
				}
				finally {
					bitmap32.UnlockBits(bitmapData);
				}
			}
		}

		///<summary>The document's pages as they print: each page's size, its fonts' names (without a subset tag) and its content stream, read back
		///from the saved file. The file's dates and IDs, which differ with every save, are left out.</summary>
		public static byte[] GetPdfPages(PdfDocument pdfDocument) {
			byte[] arrayFile;
			using(MemoryStream memoryStream=new MemoryStream()) {
				pdfDocument.Save(memoryStream,false);
				arrayFile=memoryStream.ToArray();
			}
			PdfDocument pdfDocumentRead;
			using(MemoryStream memoryStream=new MemoryStream(arrayFile)) {
				pdfDocumentRead=PdfReader.Open(memoryStream,PdfDocumentOpenMode.Import);
			}
			using(MemoryStream memoryStreamOut=new MemoryStream()) {
				foreach(PdfPage pdfPage in pdfDocumentRead.Pages) {
					StringBuilder stringBuilder=new StringBuilder();
					stringBuilder.Append("page "+pdfPage.Width.Point.ToString(CultureInfo.InvariantCulture)+" x "
						+pdfPage.Height.Point.ToString(CultureInfo.InvariantCulture)+"\n");
					PdfDictionary dictFonts=pdfPage.Elements.GetDictionary("/Resources")?.Elements.GetDictionary("/Font");
					if(dictFonts!=null) {
						foreach(string key in dictFonts.Elements.Keys.OrderBy(x => x,StringComparer.Ordinal)) {
							string baseFont=dictFonts.Elements.GetDictionary(key)?.Elements.GetName("/BaseFont")??"";
							int idxPlus=baseFont.IndexOf('+');
							if(idxPlus==7) {//"/ABCDEF+Arial": the subset tag
								baseFont="/"+baseFont.Substring(8);
							}
							stringBuilder.Append("font "+key+" "+baseFont+"\n");
						}
					}
					byte[] arrayHead=Encoding.UTF8.GetBytes(stringBuilder.ToString());
					memoryStreamOut.Write(arrayHead,0,arrayHead.Length);
					if(pdfPage.Elements.ContainsKey("/Contents") && pdfPage.Contents.Elements.Count>0) {
						byte[] arrayContent=pdfPage.Contents.CreateSingleContent().Stream.UnfilteredValue;
						memoryStreamOut.Write(arrayContent,0,arrayContent.Length);
					}
				}
				return memoryStreamOut.ToArray();
			}
		}
	}

	///<summary>The process's default font (Control.DefaultFont), as Application.SetDefaultFont sets it in each exe's Main. SetDefaultFont refuses
	///once a window handle exists, so this sets the fields Application keeps, as UI-0b's scan does (tests\UiRefreshTests\Scan\ScanSupport.cs).
	///</summary>
	public static class DefaultFont {
		public static void Set(Font font) {
			List<FieldInfo> listFields=typeof(Application).GetFields(BindingFlags.Static|BindingFlags.NonPublic).Where(x => x.FieldType==typeof(Font)).ToList();
			FieldInfo fieldDefaultFont=listFields.FirstOrDefault(x => x.Name.IndexOf("Scaled",StringComparison.OrdinalIgnoreCase)<0);
			FieldInfo fieldDefaultFontScaled=listFields.FirstOrDefault(x => x.Name.IndexOf("Scaled",StringComparison.OrdinalIgnoreCase)>=0);
			if(fieldDefaultFont==null) {
				throw new InvalidOperationException("Application's default font field was not found.");
			}
			fieldDefaultFont.SetValue(null,font);
			fieldDefaultFontScaled?.SetValue(null,null);
			//Control keeps the default font it first read (and its GDI handle); those take the new one too.
			foreach(FieldInfo fieldInfo in typeof(Control).GetFields(BindingFlags.Static|BindingFlags.NonPublic)) {
				if(fieldInfo.FieldType==typeof(Font)) {
					fieldInfo.SetValue(null,font);
				}
				else if(fieldInfo.FieldType.Name.Contains("FontHandleWrapper")) {
					fieldInfo.SetValue(null,null);
				}
			}
			Font fontNow=Control.DefaultFont;
			if(fontNow.Name!=font.Name || fontNow.SizeInPoints!=font.SizeInPoints) {
				throw new InvalidOperationException("The default font is "+fontNow.Name+" "+fontNow.SizeInPoints+" after setting "+font.Name+" "
					+font.SizeInPoints+".");
			}
		}
	}

	///<summary>Runs an action on a new STA thread and waits for it; what it throws is thrown again on the test thread with its own type, so
	///Assert.Inconclusive and Assert.Fail keep their meaning. Shuts down the thread's WPF dispatcher, if any, before the thread ends.</summary>
	public static class StaRunner {
		public static void Run(Action action) {
			ExceptionDispatchInfo exceptionDispatchInfo=null;
			Thread thread=new Thread(() => {
				try {
					action();
				}
				catch(Exception ex) {
					exceptionDispatchInfo=ExceptionDispatchInfo.Capture(ex);
				}
				finally {
					System.Windows.Threading.Dispatcher dispatcher=System.Windows.Threading.Dispatcher.FromThread(Thread.CurrentThread);
					if(dispatcher!=null && !dispatcher.HasShutdownStarted) {
						try {
							dispatcher.InvokeShutdown();
						}
						catch(Exception ex) {
							ex.ToString();//the thread ends anyway
						}
					}
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			thread.Join();
			exceptionDispatchInfo?.Throw();
		}
	}
}
