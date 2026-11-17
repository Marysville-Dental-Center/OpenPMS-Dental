// The group U walker (package UI-0c of docs/specs/ui-refresh-plan.md, tier 2 of the text-fit scan in section 5.4): once a window the harness
// opened has run its Load (so text set at run time is there), it measures every visible control with text and records every control's bounds
// with UI-0b's shared files (tests\UiRefreshTests\Shared\TextFit*.cs, linked into this project, not copied): the measurer
// (TextFitMeasurer.Measure, TextRenderer or MeasureString by control type, the 0.96 factor, the ink check), the free space and the verdict
// (TextFitMeasurer.GetFreeSpace), and the bounds-record format (TextFitBoundsRecord). With OPMS_WINDOW_CHECKS_UI_FONT (segoe9 or segoe825) it
// then swaps the window's fonts as the tier 1 scan does (TextFitFontSwap, the rule of section 3.4) and measures again: the new overflows and
// the bounds differences against the same window in today's font (TextFitBoundsRecord.Compare, fit-fixes.tsv allowed). The swap is made only on
// a window the check closes afterwards (never the main window), after its picture is taken.
// It also suggests mask rows for the gallery (tools\ui-gallery-masks.tsv): a control that shows today's date or the clock changes from run to
// run, so its rectangle in the picture is written to masks-suggested.tsv for the lead to check against the picture and add.
// Everything is written into <results folder>\ui-walker: text-fit.tsv, bounds-record.tsv (today's font), bounds-record-<font>.tsv and
// bounds-<font>.tsv (with the font option), masks-suggested.tsv and clip-notes.tsv (tools\New-UiGallery.ps1 -ClipNotes). Runs on the window's
// thread; nothing is sent to the window and nothing is clicked.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UiRefreshTests.TextFit;

namespace WindowChecksTests {
	///<summary>One measured control.</summary>
	public class UiWalkMeasure {
		public string Path="";
		public string Type="";
		public string Text="";
		public TextFitResult Result;
		public Point PointFree;

		public bool IsFixable {
			get {
				return TextFitMeasurer.IsFixable(Result,PointFree);
			}
		}

		public string Verdict {
			get {
				if(!Result.IsOverflow) {
					return "fits";
				}
				return IsFixable ? "fixable" : "needs the owner";
			}
		}
	}

	///<summary>A rectangle of a picture that shows today's date or the clock: a mask row the lead checks against the picture.</summary>
	public class UiMaskSuggestion {
		public string Picture="";
		public Size SizePicture;
		public Rectangle Rectangle;
		public string What="";

		///<summary>The row in the format of tools\ui-gallery-masks.tsv (anchored top-left, the picture's size).</summary>
		public string ToMaskLine() {
			return string.Join("\t",new[] { Picture,SizePicture.Width+"x"+SizePicture.Height,"top-left",Int(Rectangle.X),Int(Rectangle.Y),Int(Rectangle.Width),
				Int(Rectangle.Height),What });
		}

		private static string Int(int value) {
			return value.ToString(CultureInfo.InvariantCulture);
		}
	}

	///<summary>What the walker found in one window.</summary>
	public class UiWalkResult {
		///<summary>The window key of the bounds record: the picture's name without .png.</summary>
		public string Window="";
		public string Picture="";
		///<summary>The window's (or the root control's) type, full name.</summary>
		public string WindowType="";
		public Size SizePicture;
		public List<TextFitBoundsRow> ListBounds=new List<TextFitBoundsRow>();
		///<summary>Every measured control in today's font.</summary>
		public List<UiWalkMeasure> ListMeasures=new List<UiWalkMeasure>();
		///<summary>WPF content the WinForms walker does not measure (an ElementHost: the Frms already use Segoe UI, section 2 of the plan).</summary>
		public int CountWpfHosts;
		public List<UiMaskSuggestion> ListMasks=new List<UiMaskSuggestion>();
		///<summary>The font option measured after the swap, or "" for none.</summary>
		public string FontOption="";
		public string FontOptionSkipped="";
		public List<UiWalkMeasure> ListMeasuresOption=new List<UiWalkMeasure>();
		public List<TextFitBoundsRow> ListBoundsOption=new List<TextFitBoundsRow>();
		public List<TextFitBoundsDifference> ListBoundsDifferences=new List<TextFitBoundsDifference>();

		public List<UiWalkMeasure> ListOverflowsToday {
			get {
				return ListMeasures.Where(x => x.Result.IsOverflow).ToList();
			}
		}

		///<summary>Controls that fit in today's font and not in the font option.</summary>
		public List<UiWalkMeasure> ListNewOverflows {
			get {
				HashSet<string> setFitToday=new HashSet<string>(ListMeasures.Where(x => !x.Result.IsOverflow).Select(x => x.Path));
				return ListMeasuresOption.Where(x => x.Result.IsOverflow && setFitToday.Contains(x.Path)).ToList();
			}
		}

		///<summary>One line for the check's notes.</summary>
		public string Summary {
			get {
				StringBuilder stringBuilder=new StringBuilder();
				stringBuilder.Append("Walker ("+Picture+"): "+ListBounds.Count+" controls recorded, "+ListMeasures.Count+" measured in today's font, "
					+ListOverflowsToday.Count+" already overflowing (not counted as new; listed in ui-walker\\text-fit.tsv)");
				if(CountWpfHosts>0) {
					stringBuilder.Append(", "+CountWpfHosts+" WPF host(s) not measured (WPF already uses Segoe UI)");
				}
				if(ListMasks.Count>0) {
					stringBuilder.Append(", "+ListMasks.Count+" date or clock rectangle(s) suggested for the masks");
				}
				if(FontOption!="") {
					List<UiWalkMeasure> listNew=ListNewOverflows;
					stringBuilder.Append("; in "+FontOption+": "+listNew.Count+" new overflow(s) ("+listNew.Count(x => x.IsFixable)+" fixable), "
						+ListBoundsDifferences.Count+" bounds difference(s)");
				}
				else if(FontOptionSkipped!="") {
					stringBuilder.Append("; font option not measured: "+FontOptionSkipped);
				}
				return stringBuilder.ToString();
			}
		}
	}

	public static class UiWalker {
		///<summary>The font option the walker also measures (TextFitFontOption's name: segoe9 or segoe825); empty: today's font only.</summary>
		public const string VARIABLE_FONT="OPMS_WINDOW_CHECKS_UI_FONT";
		public const string HEADER_TEXT_FIT="picture\twindow_type\tfont\tpath\ttype\tmethod\tkind\tneed_width\tneed_height\tink\tfree_right\tfree_below\tverdict\ttext";
		public const string HEADER_MASKS="picture\tsize\tanchor\tx\ty\twidth\theight\twhat";
		public const string HEADER_BOUNDS_DIFFERENCES="picture\twindow_type\tfont\tkind\tpath\ttype\tother\tbefore\tafter";
		public const string HEADER_CLIP_NOTES="picture\tnote";
		private static readonly object _lock=new object();

		///<summary>The font option of OPMS_WINDOW_CHECKS_UI_FONT, or null for none. An unknown name throws, so a typo never measures nothing.</summary>
		public static TextFitFontOption GetFontOption() {
			string name=(Environment.GetEnvironmentVariable(VARIABLE_FONT)??"").Trim();
			return GetFontOption(name);
		}

		public static TextFitFontOption GetFontOption(string name) {
			if(name=="") {
				return null;
			}
			foreach(TextFitFontOption fontOption in new[] { TextFitFontOption.Segoe9,TextFitFontOption.Segoe825 }) {
				if(string.Equals(fontOption.Name,name,StringComparison.OrdinalIgnoreCase)) {
					return fontOption;
				}
			}
			throw new ArgumentException(VARIABLE_FONT+" is segoe9 or segoe825, not '"+name+"'.");
		}

		public static string FolderOut {
			get {
				return System.IO.Path.Combine(WinChkEnv.OutFolder,"ui-walker");
			}
		}

		#region Walk
		///<summary>Measures the window in today's font and records its bounds and the date and clock rectangles. root is the window (a Form) or a
		///control; rectangleWindow is the window's rectangle on the screen (Native.GetRect), whose picture the mask rectangles are measured in, or
		///Rectangle.Empty for a window that is not shown (the rectangles are then the controls' places in root). On the window's thread.</summary>
		public static UiWalkResult Walk(Control root,string window,string picture,Rectangle rectangleWindow,DateTime dateTimeNow) {
			UiWalkResult result=new UiWalkResult();
			result.Window=window;
			result.Picture=picture;
			result.WindowType=root.GetType().FullName;
			result.SizePicture=rectangleWindow.IsEmpty ? root.Size : rectangleWindow.Size;
			result.ListBounds=TextFitBoundsRecord.Collect(window,root);
			result.ListMeasures=Measure(root);
			result.CountWpfHosts=AllControls(root).Count(x => x.GetType().FullName=="System.Windows.Forms.Integration.ElementHost");
			result.ListMasks=FindDateAndClock(root,picture,rectangleWindow,result.SizePicture,dateTimeNow);
			return result;
		}

		///<summary>Swaps the window's fonts to the option (TextFitFontSwap, as the tier 1 scan) and measures again; the window keeps the swapped fonts,
		///so the caller closes it afterwards. setFitFixes holds the GetFitFixKey keys of fit-fixes.tsv for this window (ReadFitFixKeys).</summary>
		public static void WalkOption(Control root,UiWalkResult result,TextFitFontOption fontOption,ICollection<string> setFitFixes) {
			TextFitFontSwap.Swap(root,fontOption);
			result.FontOption=fontOption.Name;
			result.ListMeasuresOption=Measure(root);
			result.ListBoundsOption=TextFitBoundsRecord.Collect(result.Window,root);
			result.ListBoundsDifferences=TextFitBoundsRecord.Compare(result.ListBounds,result.ListBoundsOption,setFitFixes);
		}

		///<summary>Every visible control with text, measured the control's way at 100% (TextFitMeasurer), with the free space around it. Visible: the
		///control's own Visible state (TextFitBoundsRecord.IsDesignedVisible) under visible parents, a tab page counting as visible, as the tier 1
		///scan walks.</summary>
		public static List<UiWalkMeasure> Measure(Control root) {
			List<UiWalkMeasure> listMeasures=new List<UiWalkMeasure>();
			MeasureChildren(root,".",true,listMeasures);
			return listMeasures;
		}

		private static void MeasureChildren(Control parent,string pathParent,bool isParentVisible,List<UiWalkMeasure> listMeasures) {
			for(int i=0;i<parent.Controls.Count;i++) {
				Control control=parent.Controls[i];
				string path=TextFitBoundsRecord.GetChildPath(pathParent,parent,i);
				bool isVisible=isParentVisible && (IsTabPage(control) || TextFitBoundsRecord.IsDesignedVisible(control));
				if(isVisible) {
					TextFitResult result=TextFitMeasurer.Measure(control,1f);
					if(result.Method!=TextFitMethod.None) {
						UiWalkMeasure uiWalkMeasure=new UiWalkMeasure();
						uiWalkMeasure.Path=path;
						uiWalkMeasure.Type=control.GetType().FullName;
						uiWalkMeasure.Text=control.Text;
						uiWalkMeasure.Result=result;
						uiWalkMeasure.PointFree=TextFitMeasurer.GetFreeSpace(control);
						listMeasures.Add(uiWalkMeasure);
					}
				}
				MeasureChildren(control,path,isVisible,listMeasures);
			}
		}

		private static bool IsTabPage(Control control) {
			return control is TabPage || control.GetType().Name=="TabPage";
		}

		public static IEnumerable<Control> AllControls(Control root) {
			foreach(Control control in root.Controls) {
				yield return control;
				foreach(Control controlChild in AllControls(control)) {
					yield return controlChild;
				}
			}
		}
		#endregion Walk

		#region Dates and the clock
		///<summary>The rectangles of the picture that show today's date or the clock: a visible control whose text holds today's date or a time
		///within 30 minutes of now, a date picker set to today, and a month calendar (it marks today). A control inside one already found is left
		///out.</summary>
		public static List<UiMaskSuggestion> FindDateAndClock(Control root,string picture,Rectangle rectangleWindow,Size sizePicture,DateTime dateTimeNow) {
			List<UiMaskSuggestion> listMasks=new List<UiMaskSuggestion>();
			FindDateAndClock(root,root,".",true,picture,rectangleWindow,sizePicture,dateTimeNow,listMasks);
			return listMasks;
		}

		private static void FindDateAndClock(Control root,Control parent,string pathParent,bool isParentVisible,string picture,Rectangle rectangleWindow,
			Size sizePicture,DateTime dateTimeNow,List<UiMaskSuggestion> listMasks)
		{
			for(int i=0;i<parent.Controls.Count;i++) {
				Control control=parent.Controls[i];
				string path=TextFitBoundsRecord.GetChildPath(pathParent,parent,i);
				bool isVisible=isParentVisible && (IsTabPage(control) || TextFitBoundsRecord.IsDesignedVisible(control));
				if(!isVisible || control.Width<=0 || control.Height<=0) {
					continue;
				}
				string what=GetDateOrClock(control,dateTimeNow);
				if(what!="") {
					Rectangle rectangle=GetRectangleInPicture(root,control,rectangleWindow);
					rectangle.Intersect(new Rectangle(Point.Empty,sizePicture));
					if(rectangle.Width>0 && rectangle.Height>0) {
						listMasks.Add(new UiMaskSuggestion { Picture=picture,SizePicture=sizePicture,Rectangle=rectangle,
							What=what+": "+path+" (suggested by the group U walker; check it against the picture before adding it)" });
					}
					continue;
				}
				FindDateAndClock(root,control,path,isVisible,picture,rectangleWindow,sizePicture,dateTimeNow,listMasks);
			}
		}

		///<summary>"date" or "clock" when the control shows today's date or the time now, else "".</summary>
		public static string GetDateOrClock(Control control,DateTime dateTimeNow) {
			if(control is MonthCalendar) {
				return "date";
			}
			if(control is DateTimePicker dateTimePicker) {
				return dateTimePicker.Value.Date==dateTimeNow.Date ? "date" : "";
			}
			if(control.GetType().Name=="MonthCalendarOD") {
				return "date";
			}
			return GetDateOrClock(control.Text,dateTimeNow);
		}

		///<summary>"date" when the text holds today's date (in the short or long form of this culture, or yyyy-MM-dd), "clock" when it holds a time
		///within 30 minutes of now, else "".</summary>
		public static string GetDateOrClock(string text,DateTime dateTimeNow) {
			if(string.IsNullOrWhiteSpace(text)) {
				return "";
			}
			foreach(Match match in Regex.Matches(text,@"\d{1,4}[/.-]\d{1,2}[/.-]\d{1,4}")) {
				if(DateTime.TryParse(match.Value,CultureInfo.CurrentCulture,DateTimeStyles.None,out DateTime date) && date.Date==dateTimeNow.Date) {
					return "date";
				}
				if(DateTime.TryParseExact(match.Value,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out date) && date.Date==dateTimeNow.Date) {
					return "date";
				}
			}
			foreach(string format in new[] { "D","MMMM d, yyyy","MMM d, yyyy","dddd, MMMM d" }) {
				if(text.IndexOf(dateTimeNow.ToString(format,CultureInfo.CurrentCulture),StringComparison.OrdinalIgnoreCase)>=0) {
					return "date";
				}
			}
			foreach(Match match in Regex.Matches(text,@"\b\d{1,2}:\d{2}(:\d{2})?(\s?[AaPp][Mm])?")) {
				if(DateTime.TryParse(match.Value,CultureInfo.CurrentCulture,DateTimeStyles.NoCurrentDateDefault,out DateTime time)
					&& Math.Abs((dateTimeNow.TimeOfDay-time.TimeOfDay).TotalMinutes)<=30)
				{
					return "clock";
				}
			}
			return "";
		}

		///<summary>The control's rectangle in the window's picture: from the screen when both have handles and the window's rectangle is known,
		///else the sum of the places up to root.</summary>
		public static Rectangle GetRectangleInPicture(Control root,Control control,Rectangle rectangleWindow) {
			if(!rectangleWindow.IsEmpty && control.Parent!=null && control.Parent.IsHandleCreated) {
				Rectangle rectangleScreen=control.Parent.RectangleToScreen(control.Bounds);
				rectangleScreen.Offset(-rectangleWindow.X,-rectangleWindow.Y);
				return rectangleScreen;
			}
			Point point=Point.Empty;
			for(Control c=control;c!=null && c!=root;c=c.Parent) {
				point.Offset(c.Location);
			}
			return new Rectangle(point,control.Size);
		}
		#endregion Dates and the clock

		#region Fit fixes
		///<summary>The GetFitFixKey keys (window key, control name) of the fit-fixes.tsv rows of the window type (tests\UiRefreshTests\Data\fit-fixes.tsv:
		///designer_file, window, control, fix, note).</summary>
		public static HashSet<string> ReadFitFixKeys(string window,string windowTypeFullName) {
			HashSet<string> setKeys=new HashSet<string>();
			string path=System.IO.Path.Combine(WinChkEnv.RepoRoot,"tests","UiRefreshTests","Data","fit-fixes.tsv");
			if(!File.Exists(path)) {
				return setKeys;
			}
			foreach(string line in File.ReadAllLines(path)) {
				if(line.Trim()=="" || line.StartsWith("#") || line.StartsWith("designer_file\t")) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				if(arrayFields.Length>=3 && arrayFields[1].Trim()==windowTypeFullName) {
					setKeys.Add(TextFitBoundsRecord.GetFitFixKey(window,arrayFields[2].Trim()));
				}
			}
			return setKeys;
		}
		#endregion Fit fixes

		#region Files
		///<summary>Appends the window's rows to the walker's files in folder (each file gets its header when it is new). UTF-8 without a BOM, so the
		///files read as the gallery builder and the mask file expect.</summary>
		public static void Write(UiWalkResult result,string folder) {
			lock(_lock) {
				Directory.CreateDirectory(folder);
				List<string> listLines=new List<string>();
				foreach(UiWalkMeasure uiWalkMeasure in result.ListMeasures) {
					listLines.Add(TextFitLine(result,"today",uiWalkMeasure));
				}
				foreach(UiWalkMeasure uiWalkMeasure in result.ListMeasuresOption) {
					listLines.Add(TextFitLine(result,result.FontOption,uiWalkMeasure));
				}
				Append(System.IO.Path.Combine(folder,"text-fit.tsv"),HEADER_TEXT_FIT,listLines);
				Append(System.IO.Path.Combine(folder,"bounds-record.tsv"),TextFitBoundsRecord.HEADER,result.ListBounds.Select(x => TextFitBoundsRecord.ToLine(x)));
				Append(System.IO.Path.Combine(folder,"masks-suggested.tsv"),HEADER_MASKS,result.ListMasks.Select(x => x.ToMaskLine()));
				List<string> listNotes=new List<string>();
				foreach(UiWalkMeasure uiWalkMeasure in result.ListOverflowsToday) {
					listNotes.Add(result.Picture+"\t"+ClipNote("today's font, already overflowing",uiWalkMeasure));
				}
				if(result.FontOption!="") {
					foreach(UiWalkMeasure uiWalkMeasure in result.ListNewOverflows) {
						listNotes.Add(result.Picture+"\t"+ClipNote(result.FontOption+", new",uiWalkMeasure));
					}
					Append(System.IO.Path.Combine(folder,"bounds-record-"+result.FontOption+".tsv"),TextFitBoundsRecord.HEADER,
						result.ListBoundsOption.Select(x => TextFitBoundsRecord.ToLine(x)));
					Append(System.IO.Path.Combine(folder,"bounds-"+result.FontOption+".tsv"),HEADER_BOUNDS_DIFFERENCES,
						result.ListBoundsDifferences.Select(x => DifferenceLine(result,x)));
				}
				Append(System.IO.Path.Combine(folder,"clip-notes.tsv"),HEADER_CLIP_NOTES,listNotes);
			}
		}

		private static string TextFitLine(UiWalkResult result,string font,UiWalkMeasure uiWalkMeasure) {
			TextFitResult textFitResult=uiWalkMeasure.Result;
			string ink=textFitResult.IsInkChecked ? (textFitResult.IsInkClipped ? "cut" : "inside") : "";
			string text=Clean(uiWalkMeasure.Text);
			if(text.Length>120) {
				text=text.Substring(0,120)+"...";
			}
			return string.Join("\t",new[] { result.Picture,result.WindowType,font,Clean(uiWalkMeasure.Path),uiWalkMeasure.Type,textFitResult.Method.ToString(),
				textFitResult.Kind.ToString().ToLowerInvariant(),Int(textFitResult.NeedWidth),Int(textFitResult.NeedHeight),ink,Int(uiWalkMeasure.PointFree.X),
				Int(uiWalkMeasure.PointFree.Y),uiWalkMeasure.Verdict,text });
		}

		private static string ClipNote(string what,UiWalkMeasure uiWalkMeasure) {
			TextFitResult textFitResult=uiWalkMeasure.Result;
			return Clean(uiWalkMeasure.Path)+"\t"+what+": "+textFitResult.Kind.ToString().ToLowerInvariant()+" +"+Int(textFitResult.NeedWidth)+"x+"
				+Int(textFitResult.NeedHeight)+" px"+(textFitResult.IsInkChecked ? (textFitResult.IsInkClipped ? ", ink cut" : ", ink inside") : "")
				+"\t"+uiWalkMeasure.Verdict;
		}

		private static string DifferenceLine(UiWalkResult result,TextFitBoundsDifference difference) {
			return string.Join("\t",new[] { result.Picture,result.WindowType,result.FontOption,difference.Kind,Clean(difference.Row.Path),difference.Row.Type,
				Clean(difference.Other),difference.RowBefore==null ? "" : RectangleText(difference.RowBefore.Bounds),RectangleText(difference.Row.Bounds) });
		}

		private static string RectangleText(Rectangle rectangle) {
			return Int(rectangle.X)+","+Int(rectangle.Y)+" "+Int(rectangle.Width)+"x"+Int(rectangle.Height);
		}

		private static void Append(string path,string header,IEnumerable<string> enumerableLines) {
			StringBuilder stringBuilder=new StringBuilder();
			if(!File.Exists(path)) {
				stringBuilder.Append(header+"\r\n");
			}
			foreach(string line in enumerableLines) {
				stringBuilder.Append(line+"\r\n");
			}
			File.AppendAllText(path,stringBuilder.ToString(),new UTF8Encoding(false));
		}

		private static string Clean(string text) {
			return (text??"").Replace("\t"," ").Replace("\r"," ").Replace("\n"," ");
		}

		private static string Int(int value) {
			return value.ToString(CultureInfo.InvariantCulture);
		}
		#endregion Files
	}
}
