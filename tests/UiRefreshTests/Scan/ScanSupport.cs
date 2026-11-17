// Support for the tier 1 scan (docs/port-notes/UI-0b.md): the fit-fixes list, the process's default font (what Application.SetDefaultFont
// sets in each exe's Main, set here the way the exe would have it for a font option or a Windows text scale), and the STA thread the scan runs
// on (WinForms and the WPF controls some windows host need one).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using UiRefreshTests.TextFit;

namespace UiRefreshTests.Scan {
	///<summary>tests\UiRefreshTests\Data\fit-fixes.tsv: the UI-10 fit fixes.</summary>
	public class FitFixRow {
		public string DesignerFile="";
		public string Window="";
		public string Control="";
		public string Fix="";
		public string Note="";

		public static List<FitFixRow> ReadAll() {
			return Read(File.ReadAllLines(RepoPaths.DataFile("fit-fixes.tsv")));
		}

		public static List<FitFixRow> Read(IEnumerable<string> listLines) {
			List<FitFixRow> listRows=new List<FitFixRow>();
			foreach(string line in listLines) {
				if(line.Trim()=="" || line.StartsWith("#") || line.StartsWith("designer_file\t")) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				if(arrayFields.Length<4) {
					throw new FormatException("fit-fixes.tsv: a row needs designer_file, window, control and fix: "+line);
				}
				listRows.Add(new FitFixRow {
					DesignerFile=arrayFields[0].Replace('\\','/'),
					Window=arrayFields[1],
					Control=arrayFields[2],
					Fix=arrayFields[3],
					Note=arrayFields.Length>4 ? arrayFields[4] : "",
				});
			}
			return listRows;
		}

		///<summary>The bounds comparison's keys (TextFitBoundsRecord.GetFitFixKey).</summary>
		public static HashSet<string> GetBoundsKeys(List<FitFixRow> listRows) {
			return new HashSet<string>(listRows.Select(x => TextFitBoundsRecord.GetFitFixKey(x.Window,x.Control)));
		}
	}

	///<summary>The process's default font (Control.DefaultFont), as Application.SetDefaultFont sets it in each exe's Main. SetDefaultFont itself
	///refuses once any window handle exists, so the scan sets the same fields Application keeps (the font, and the font after Windows' text scale).</summary>
	public static class DefaultFontOverride {
		private static FieldInfo _fieldDefaultFont;
		private static FieldInfo _fieldDefaultFontScaled;

		private static void LookUp() {
			if(_fieldDefaultFont!=null) {
				return;
			}
			List<FieldInfo> listFields=typeof(Application).GetFields(BindingFlags.Static|BindingFlags.NonPublic)
				.Where(x => x.FieldType==typeof(Font)).ToList();
			_fieldDefaultFont=listFields.FirstOrDefault(x => x.Name.IndexOf("Scaled",StringComparison.OrdinalIgnoreCase)<0);
			_fieldDefaultFontScaled=listFields.FirstOrDefault(x => x.Name.IndexOf("Scaled",StringComparison.OrdinalIgnoreCase)>=0);
			if(_fieldDefaultFont==null) {
				throw new InvalidOperationException("Application's default font field was not found: the scan cannot set the default font.");
			}
		}

		///<summary>Sets the default font to font (the exe's literal), and with textScale above 1 to that font as Windows' text size setting makes it
		///(SetDefaultFont multiplies its size). Checks that Control.DefaultFont gives it.</summary>
		public static void Set(Font font,float textScale) {
			LookUp();
			Font fontScaled=null;
			if(textScale!=1f) {
				fontScaled=new Font(font.FontFamily,font.SizeInPoints*textScale,font.Style,GraphicsUnit.Point);
			}
			_fieldDefaultFont.SetValue(null,font);
			_fieldDefaultFontScaled?.SetValue(null,fontScaled);
			Font fontExpected=fontScaled??font;
			//Control keeps the default font it first read (and its GDI handle); those take the new one too.
			foreach(FieldInfo fieldInfo in typeof(Control).GetFields(BindingFlags.Static|BindingFlags.NonPublic)) {
				if(fieldInfo.FieldType==typeof(Font)) {
					fieldInfo.SetValue(null,fontExpected);
				}
				else if(fieldInfo.FieldType.Name.Contains("FontHandleWrapper")) {
					fieldInfo.SetValue(null,null);
				}
			}
			Font fontNow=Control.DefaultFont;
			if(fontNow.Name!=fontExpected.Name || fontNow.SizeInPoints!=fontExpected.SizeInPoints) {
				throw new InvalidOperationException("The default font is "+fontNow.Name+" "+fontNow.SizeInPoints+" after setting "+fontExpected.Name+" "
					+fontExpected.SizeInPoints+".");
			}
		}

		public static void Set(TextFitFontOption fontOption,float textScale) {
			Set(fontOption.NewBodyFont(),textScale);
		}
	}

	///<summary>Runs an action on a new STA thread and rethrows what it throws, as it was thrown (an assertion stays an assertion).</summary>
	public static class StaRunner {
		///<summary>The first exception WinForms caught in a window procedure on this thread since TakeExceptionInCallback (Application.ThreadException).</summary>
		[ThreadStatic]
		public static Exception ExceptionInCallback;

		///<summary>Returns ExceptionInCallback and clears it.</summary>
		public static Exception TakeExceptionInCallback() {
			Exception exception=ExceptionInCallback;
			ExceptionInCallback=null;
			return exception;
		}

		public static void Run(Action action) {
			Run(action,TimeSpan.FromMinutes(10));
		}

		///<summary>Fails with a TimeoutException when the action takes longer than timeout (a window that waits for a person, say); the thread is
		///a background thread and ends with the test process.</summary>
		public static void Run(Action action,TimeSpan timeout) {
			Exception exceptionThrown=null;
			Thread thread=new Thread(() => {
				try {
					//An exception in a control's window procedure (a handle some designer code creates, a child shown inside a hidden window) is
					//kept for the builder, which lists the type, instead of WinForms showing its "Unhandled exception" dialog, a window the scan
					//must never show, or the exception ending the process from inside the native callback.
					Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException,true);
					Application.ThreadException+=(sender,e) => {
						if(ExceptionInCallback==null) {
							ExceptionInCallback=e.Exception;
						}
					};
					action();
				}
				catch(Exception exception) {
					exceptionThrown=exception;
				}
			},64*1024*1024);
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			if(!thread.Join(timeout)) {
				throw new TimeoutException("The STA thread did not finish in "+timeout.TotalMinutes+" minutes.");
			}
			if(exceptionThrown!=null) {
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exceptionThrown).Throw();
			}
		}
	}
}
