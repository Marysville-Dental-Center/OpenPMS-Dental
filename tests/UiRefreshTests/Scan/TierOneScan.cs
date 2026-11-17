// The tier 1 text-fit scan (docs/specs/ui-refresh-plan.md section 5.4, docs/port-notes/UI-0b.md). For every Form and UserControl type
// (WindowBuilder.GetScanTypes) it builds the before set (today's font, Microsoft Sans Serif 8.25, the exes' default) and each font option under
// test (the default set to the option's body font, then the test-side font swap), lays each LayoutManager window out at the scales (a
// simulated Windows scale through LayoutManagerForms.SetScaleMS; 1.2 also stands for the program's zoom step), and compares:
// - new overflows: a control whose text fits in the before set and not in the font under test (TextFitMeasurer), with the pixels missing, the
//   ink check, and the free space to the right and below inside the parent without touching a sibling ("fixable" when it is enough);
// - controls that size themselves: their size in the font under test (what WinForms gives them at 100%; at other scales their text part
//   measured at the scale and the rest scaled) and a new overlap with a sibling or a new pass over the parent's client edge, "fixable" when an
//   explicit size in the free space would hold the text (a text box: today's height still holds the new font) or "needs the owner";
// - the bounds record at 100% against the before set by TextFitBoundsRecord.Compare.
// The text-scale run sets the default font as Windows does at text size 125% and builds the windows that take the default font: the plain
// forms and the UserControls they host (a FormODBase window sets its own font, so the default never reaches it).
// Nothing is shown; the database guard refuses every connection; a type that cannot be built is listed with its reason.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UiRefreshTests.TextFit;

namespace UiRefreshTests.Scan {
	public class ScanOptions {
		///<summary>The font options under test, compared with the before set (today's font). Today's option itself checks that the scan reports
		///nothing new.</summary>
		public List<TextFitFontOption> ListFontOptions=new List<TextFitFontOption>();
		///<summary>The simulated Windows scales for LayoutManager windows. Other windows are measured at 1 only.</summary>
		public float[] ArrayScales=new float[] { 1f };
		///<summary>Also the text-scale run (125%) for today's font and for each option.</summary>
		public bool IsTextScaleRun;
		public float TextScale=1.25f;
		///<summary>Which types to build; null for all.</summary>
		public Func<ScanType,bool> FuncFilter;
		///<summary>The folder for the TSV files and the summary; null for none.</summary>
		public string FolderOut;
		///<summary>The scan fails when it takes longer (StaRunner).</summary>
		public TimeSpan Timeout=TimeSpan.FromMinutes(30);
	}

	///<summary>Why a type was not scanned.</summary>
	public class ScanTypeResult {
		public ScanType ScanType;
		///<summary>built, needs-database, throws, no-designer-code, not-loadable.</summary>
		public string Status="";
		public string Reason="";
		public int CountControls;
		public int CountMeasured;
	}

	///<summary>One new overflow.</summary>
	public class ScanOverflow {
		public string Run="";
		public string Window="";
		public BuildKind Kind;
		public string Path="";
		public string ControlType="";
		public float Scale;
		public TextFitResult Result;
		public int FreeRight;
		public int FreeBelow;

		public bool IsFixable {
			get { return Result.NeedWidth<=FreeRight && Result.NeedHeight<=FreeBelow; }
		}
	}

	///<summary>One new overlap or parent-edge pass of a control that sizes itself.</summary>
	public class ScanSelfSizing {
		public string Run="";
		public string Window="";
		public BuildKind Kind;
		public string Path="";
		public string ControlType="";
		public float Scale;
		///<summary>overlap or parent-edge.</summary>
		public string What="";
		public string Other="";
		public Rectangle BoundsBefore;
		public Rectangle BoundsAfter;
		public bool IsFixable;
	}

	public class ScanReport {
		public List<ScanTypeResult> ListTypeResults=new List<ScanTypeResult>();
		public List<ScanOverflow> ListOverflows=new List<ScanOverflow>();
		public List<ScanSelfSizing> ListSelfSizing=new List<ScanSelfSizing>();
		///<summary>Bounds differences at 100% (overlaps are in ListSelfSizing), by run.</summary>
		public List<Tuple<string,TextFitBoundsDifference>> ListBoundsDifferences=new List<Tuple<string,TextFitBoundsDifference>>();
		///<summary>Overflows that are already in the before set, by run and scale: (run, scale) to count.</summary>
		public Dictionary<string,int> DictExistingOverflows=new Dictionary<string,int>();
		public List<string> ListRuns=new List<string>();
		public TimeSpan Elapsed;
		public float DpiX;
	}

	///<summary>What one built window gives at one scale: the measured controls and the bounds record.</summary>
	public class ScanSnapshot {
		public float Scale;
		public Dictionary<string,TextFitResult> DictResults=new Dictionary<string,TextFitResult>();
		public Dictionary<string,Point> DictFree=new Dictionary<string,Point>();
		public Dictionary<string,string> DictTypes=new Dictionary<string,string>();
		public List<TextFitBoundsRow> ListRows=new List<TextFitBoundsRow>();
		///<summary>For the self-sizing fix rule: the height of the measuring font of each control that sizes itself.</summary>
		public Dictionary<string,int> DictFontHeight=new Dictionary<string,int>();
		///<summary>The paths of the single-line text boxes among the controls that size themselves.</summary>
		public HashSet<string> SetTextBoxes=new HashSet<string>();
		public int CountControls;
	}

	public static class TierOneScan {
		public const string RUN_TEXT_SCALE_SUFFIX="-textscale125";

		public static ScanReport Run(ScanOptions scanOptions) {
			ScanReport scanReport=null;
			StaRunner.Run(() => scanReport=RunOnThisThread(scanOptions),scanOptions.Timeout);
			return scanReport;
		}

		public static ScanReport RunOnThisThread(ScanOptions scanOptions) {
			Stopwatch stopwatch=Stopwatch.StartNew();
			NoDatabaseGuard.Install();
			ScanReport scanReport=new ScanReport();
			using(Graphics g=Graphics.FromHwnd(IntPtr.Zero)) {
				scanReport.DpiX=g.DpiX;
			}
			HashSet<string> setFitFixes=FitFixRow.GetBoundsKeys(FitFixRow.ReadAll());
			List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
			if(scanOptions.FuncFilter!=null) {
				listScanTypes=listScanTypes.Where(scanOptions.FuncFilter).ToList();
			}
			foreach(TextFitFontOption fontOption in scanOptions.ListFontOptions) {
				scanReport.ListRuns.Add(fontOption.Name);
			}
			if(scanOptions.IsTextScaleRun) {
				scanReport.ListRuns.Add(TextFitFontOption.Today.Name+RUN_TEXT_SCALE_SUFFIX);
				foreach(TextFitFontOption fontOption in scanOptions.ListFontOptions.Where(x => !x.IsToday)) {
					scanReport.ListRuns.Add(fontOption.Name+RUN_TEXT_SCALE_SUFFIX);
				}
			}
			BoundsRecordFiles boundsRecordFiles=new BoundsRecordFiles(scanOptions.FolderOut,scanReport.ListRuns);
			try {
				for(int i=0;i<listScanTypes.Count;i++) {
					ScanType scanType=listScanTypes[i];
					if(scanOptions.FolderOut!=null) {
						//The type being built, so a run that stops can be followed up.
						File.WriteAllText(Path.Combine(scanOptions.FolderOut,"progress.txt"),(i+1)+" of "+listScanTypes.Count+": "+scanType.Type.FullName);
					}
					ScanOneType(scanType,scanOptions,scanReport,setFitFixes,boundsRecordFiles);
				}
			}
			finally {
				boundsRecordFiles.Dispose();
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
			}
			scanReport.Elapsed=stopwatch.Elapsed;
			if(scanOptions.FolderOut!=null) {
				ScanReportWriter.Write(scanReport,scanOptions);
			}
			return scanReport;
		}

		private static void ScanOneType(ScanType scanType,ScanOptions scanOptions,ScanReport scanReport,HashSet<string> setFitFixes,
			BoundsRecordFiles boundsRecordFiles)
		{
			ScanTypeResult scanTypeResult=new ScanTypeResult { ScanType=scanType };
			scanReport.ListTypeResults.Add(scanTypeResult);
			if(!WindowBuilder.HasDesignerCode(scanType.Type)) {
				scanTypeResult.Status="no-designer-code";
				scanTypeResult.Reason="no InitializeComponent: its controls are made in its constructor";
				return;
			}
			float[] arrayScales=scanType.Kind==BuildKind.LayoutManagerWindow || scanType.IsUnderLayoutManager ? scanOptions.ArrayScales : new float[] { 1f };
			List<ScanSnapshot> listBefore;
			try {
				listBefore=BuildAndSnapshot(scanType,TextFitFontOption.Today,1f,null,arrayScales);
			}
			catch(Exception exception) {
				scanTypeResult.Status=NoDatabaseGuard.IsDatabaseException(exception) ? "needs-database" : "throws";
				scanTypeResult.Reason=Describe(exception);
				return;
			}
			scanTypeResult.Status="built";
			scanTypeResult.CountControls=listBefore[0].CountControls;
			scanTypeResult.CountMeasured=listBefore[0].DictResults.Count;
			boundsRecordFiles.Write("before",listBefore[0].ListRows);
			foreach(TextFitFontOption fontOption in scanOptions.ListFontOptions) {
				CompareRun(scanType,fontOption.Name,fontOption,1f,arrayScales,listBefore,scanReport,setFitFixes,boundsRecordFiles);
			}
			bool takesDefaultFont=scanType.Kind==BuildKind.PlainWindow || scanType.Kind==BuildKind.UserControlPlain;
			if(scanOptions.IsTextScaleRun && takesDefaultFont) {
				CompareRun(scanType,TextFitFontOption.Today.Name+RUN_TEXT_SCALE_SUFFIX,TextFitFontOption.Today,scanOptions.TextScale,new float[] { 1f },
					listBefore,scanReport,setFitFixes,boundsRecordFiles);
				foreach(TextFitFontOption fontOption in scanOptions.ListFontOptions.Where(x => !x.IsToday)) {
					CompareRun(scanType,fontOption.Name+RUN_TEXT_SCALE_SUFFIX,fontOption,scanOptions.TextScale,new float[] { 1f },listBefore,scanReport,
						setFitFixes,boundsRecordFiles);
				}
			}
		}

		private static void CompareRun(ScanType scanType,string run,TextFitFontOption fontOption,float textScale,float[] arrayScales,
			List<ScanSnapshot> listBefore,ScanReport scanReport,HashSet<string> setFitFixes,BoundsRecordFiles boundsRecordFiles)
		{
			List<ScanSnapshot> listAfter;
			try {
				listAfter=BuildAndSnapshot(scanType,fontOption,textScale,fontOption.IsToday ? null : fontOption,arrayScales);
			}
			catch(Exception exception) {
				//Built in the before set but not in this run: a difference in itself.
				ScanTypeResult scanTypeResult=scanReport.ListTypeResults.Last();
				scanTypeResult.Reason+=(scanTypeResult.Reason=="" ? "" : "; ")+run+" throws: "+Describe(exception);
				return;
			}
			string window=scanType.Type.FullName;
			foreach(ScanSnapshot snapshotAfter in listAfter) {
				ScanSnapshot snapshotBefore=listBefore.First(x => x.Scale==snapshotAfter.Scale);
				string keyExisting=run+"\t"+snapshotAfter.Scale.ToString(CultureInfo.InvariantCulture);
				int countExisting=0;
				foreach(KeyValuePair<string,TextFitResult> pair in snapshotAfter.DictResults) {
					TextFitResult resultBefore;
					bool isBefore=snapshotBefore.DictResults.TryGetValue(pair.Key,out resultBefore);
					if(isBefore && resultBefore.IsOverflow) {
						countExisting++;
						continue;
					}
					if(!pair.Value.IsOverflow) {
						continue;
					}
					Point pointFree=snapshotAfter.DictFree[pair.Key];
					scanReport.ListOverflows.Add(new ScanOverflow {
						Run=run,Window=window,Kind=scanType.Kind,Path=pair.Key,ControlType=snapshotAfter.DictTypes[pair.Key],Scale=snapshotAfter.Scale,
						Result=pair.Value,FreeRight=pointFree.X,FreeBelow=pointFree.Y,
					});
				}
				int countOld;
				scanReport.DictExistingOverflows.TryGetValue(keyExisting,out countOld);
				scanReport.DictExistingOverflows[keyExisting]=countOld+countExisting;
				AddSelfSizing(scanType,run,snapshotBefore,snapshotAfter,scanReport,setFitFixes);
				if(snapshotAfter.Scale==1f) {
					boundsRecordFiles.Write(run,snapshotAfter.ListRows);
					foreach(TextFitBoundsDifference difference in TextFitBoundsRecord.Compare(snapshotBefore.ListRows,snapshotAfter.ListRows,setFitFixes)) {
						if(difference.Kind=="overlap" || difference.Kind=="parent-edge") {
							continue;
						}
						scanReport.ListBoundsDifferences.Add(Tuple.Create(run,difference));
					}
				}
			}
		}

		private static void AddSelfSizing(ScanType scanType,string run,ScanSnapshot snapshotBefore,ScanSnapshot snapshotAfter,ScanReport scanReport,
			HashSet<string> setFitFixes)
		{
			HashSet<string> setBefore=new HashSet<string>(TextFitBoundsRecord.FindOverlaps(snapshotBefore.ListRows).Select(x => x.Kind+"\t"+x.Row.Path+"\t"+x.Other));
			Dictionary<string,TextFitBoundsRow> dictRowsBefore=snapshotBefore.ListRows.ToDictionary(x => x.Path);
			foreach(TextFitBoundsDifference difference in TextFitBoundsRecord.FindOverlaps(snapshotAfter.ListRows)) {
				if(setBefore.Contains(difference.Kind+"\t"+difference.Row.Path+"\t"+difference.Other)) {
					continue;
				}
				if(setFitFixes.Contains(TextFitBoundsRecord.GetFitFixKey(difference.Row.Window,difference.Row.Name))) {
					continue;
				}
				TextFitBoundsRow rowBefore;
				dictRowsBefore.TryGetValue(difference.Row.Path,out rowBefore);
				Rectangle rectangleBefore=rowBefore==null ? difference.Row.Bounds : rowBefore.Bounds;
				ScanSelfSizing scanSelfSizing=new ScanSelfSizing {
					Run=run,Window=scanType.Type.FullName,Kind=scanType.Kind,Path=difference.Row.Path,ControlType=difference.Row.Type,Scale=snapshotAfter.Scale,
					What=difference.Kind,Other=difference.Other,BoundsBefore=rectangleBefore,BoundsAfter=difference.Row.Bounds,
				};
				scanSelfSizing.IsFixable=IsSelfSizingFixable(snapshotAfter,difference.Row,rectangleBefore);
				scanReport.ListSelfSizing.Add(scanSelfSizing);
			}
		}

		///<summary>The fix of section 5.4 for a control that sizes itself (AutoSize = false with an explicit Size in the free space) works: its
		///grown size fits the free space around its old bounds, or (a text box) today's height still holds the new font.</summary>
		private static bool IsSelfSizingFixable(ScanSnapshot snapshotAfter,TextFitBoundsRow row,Rectangle rectangleBefore) {
			List<TextFitBoundsRow> listSiblings=snapshotAfter.ListRows.Where(x => x.Window==row.Window && x.ParentPath==row.ParentPath && x.Path!=row.Path
				&& x.IsVisible && x.Dock==DockStyle.None).ToList();
			TextFitBoundsRow rowParent=snapshotAfter.ListRows.FirstOrDefault(x => x.Path==row.ParentPath);
			Size sizeParent=rowParent==null ? new Size(int.MaxValue/2,int.MaxValue/2) : rowParent.ClientSize;
			Point pointFree=GetFreeSpace(rectangleBefore,listSiblings.Select(x => x.Bounds).ToList(),sizeParent);
			int growWidth=row.Bounds.Width-rectangleBefore.Width;
			int growHeight=row.Bounds.Height-rectangleBefore.Height;
			if(growWidth<=pointFree.X && growHeight<=pointFree.Y) {
				return true;
			}
			int fontHeight;
			if(snapshotAfter.SetTextBoxes.Contains(row.Path) && snapshotAfter.DictFontHeight.TryGetValue(row.Path,out fontHeight)) {
				//A single-line text box with a 3D border shows its text in its height less 2 px on each side.
				return fontHeight<=rectangleBefore.Height-4 && growWidth<=pointFree.X;
			}
			return false;
		}

		///<summary>The free pixels to the right of and below a rectangle inside its parent, without touching a sibling that lies beside it (right)
		///or under it (below). A sibling that already overlaps the rectangle (a panel behind it) does not count.</summary>
		public static Point GetFreeSpace(Rectangle rectangle,List<Rectangle> listSiblings,Size sizeParentClient) {
			int freeRight=sizeParentClient.Width-rectangle.Right;
			int freeBelow=sizeParentClient.Height-rectangle.Bottom;
			foreach(Rectangle rectangleSibling in listSiblings) {
				if(rectangleSibling.IntersectsWith(rectangle)) {
					continue;
				}
				bool isBeside=rectangleSibling.Top<rectangle.Bottom && rectangleSibling.Bottom>rectangle.Top;
				if(isBeside && rectangleSibling.Left>=rectangle.Right) {
					freeRight=Math.Min(freeRight,rectangleSibling.Left-rectangle.Right);
				}
				bool isUnder=rectangleSibling.Left<rectangle.Right && rectangleSibling.Right>rectangle.Left;
				if(isUnder && rectangleSibling.Top>=rectangle.Bottom) {
					freeBelow=Math.Min(freeBelow,rectangleSibling.Top-rectangle.Bottom);
				}
			}
			return new Point(Math.Max(0,freeRight),Math.Max(0,freeBelow));
		}

		///<summary>Builds the type with the default font of fontOption (times textScale), swaps its fonts to fontSwap when given, and snapshots it
		///at each scale.</summary>
		public static List<ScanSnapshot> BuildAndSnapshot(ScanType scanType,TextFitFontOption fontOption,float textScale,TextFitFontOption fontSwap,
			float[] arrayScales)
		{
			DefaultFontOverride.Set(fontOption,textScale);
			List<ScanSnapshot> listSnapshots=new List<ScanSnapshot>();
			using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
				CheckNothingShown(builtWindow);
				if(fontSwap!=null) {
					TextFitFontSwap.Swap(builtWindow.Form,fontSwap);
				}
				foreach(float scale in arrayScales) {
					builtWindow.LayOutAt(scale);
					listSnapshots.Add(Snapshot(builtWindow));
				}
				if(builtWindow.Scale!=1f) {
					builtWindow.LayOutAt(1f);
				}
				CheckNothingShown(builtWindow);
			}
			return listSnapshots;
		}

		///<summary>Fails the scan when a window became visible: building must never show one.</summary>
		private static void CheckNothingShown(BuiltWindow builtWindow) {
			if(builtWindow.Form!=null && TextFitBoundsRecord.IsDesignedVisible(builtWindow.Form) && builtWindow.Form.IsHandleCreated) {
				throw new InvalidOperationException("Building "+builtWindow.ScanType.Type.FullName+" showed its window.");
			}
			foreach(Form form in Application.OpenForms) {
				if(form.Visible) {
					throw new InvalidOperationException("A window is visible while the scan builds "+builtWindow.ScanType.Type.FullName+": "+form.GetType().FullName);
				}
			}
		}

		///<summary>Measures every visible control with text under the built control and records the bounds, at the window's current scale. The
		///bounds of a control that sizes itself are its size at that scale (WinForms' at 100%; modelled above it).</summary>
		public static ScanSnapshot Snapshot(BuiltWindow builtWindow) {
			ScanSnapshot snapshot=new ScanSnapshot();
			snapshot.Scale=builtWindow.Scale;
			string window=builtWindow.ScanType.Type.FullName;
			Control root=builtWindow.Root;
			snapshot.ListRows=TextFitBoundsRecord.Collect(window,root);
			Dictionary<string,TextFitBoundsRow> dictRows=snapshot.ListRows.ToDictionary(x => x.Path);
			snapshot.CountControls=snapshot.ListRows.Count;
			WalkMeasure(root,root,".",true,snapshot,dictRows);
			return snapshot;
		}

		private static void WalkMeasure(Control root,Control parent,string pathParent,bool isParentVisible,ScanSnapshot snapshot,
			Dictionary<string,TextFitBoundsRow> dictRows)
		{
			for(int i=0;i<parent.Controls.Count;i++) {
				Control control=parent.Controls[i];
				string path=TextFitBoundsRecord.GetChildPath(pathParent,parent,i);
				bool isVisible=isParentVisible && (IsTabPage(control) || TextFitBoundsRecord.IsDesignedVisible(control));
				if(isVisible) {
					TextFitResult result=TextFitMeasurer.Measure(control,snapshot.Scale);
					if(result.Method!=TextFitMethod.None) {
						snapshot.DictResults[path]=result;
						snapshot.DictTypes[path]=control.GetType().FullName;
						List<Rectangle> listSiblings=new List<Rectangle>();
						foreach(Control sibling in parent.Controls) {
							if(sibling!=control && sibling.Dock==DockStyle.None && TextFitBoundsRecord.IsDesignedVisible(sibling)) {
								listSiblings.Add(sibling.Bounds);
							}
						}
						snapshot.DictFree[path]=GetFreeSpace(control.Bounds,listSiblings,parent.ClientSize);
					}
					TextFitBoundsRow row;
					if(dictRows.TryGetValue(path,out row) && row.IsSizingItself) {
						row.Bounds=new Rectangle(row.Bounds.Location,GetSizeAtScale(control,snapshot.Scale));
						snapshot.DictFontHeight[path]=GetFontHeightAtScale(control,snapshot.Scale);
						if(control is TextBoxBase) {
							snapshot.SetTextBoxes.Add(path);
						}
					}
				}
				WalkMeasure(root,control,path,isVisible,snapshot,dictRows);
			}
		}

		private static bool IsTabPage(Control control) {
			return control is TabPage || control.GetType().Name=="TabPage";
		}

		///<summary>The font's height in pixels at the scale (the measuring font).</summary>
		private static int GetFontHeightAtScale(Control control,float scale) {
			Font font=TextFitMeasurer.CreateMeasuringFont(control,scale);
			try {
				return font.Height;
			}
			finally {
				if(!ReferenceEquals(font,control.Font)) {
					font.Dispose();
				}
			}
		}

		///<summary>The size a control that sizes itself takes at a simulated scale. At 100%, and for the fully scaled types (whose font
		///LayoutManagerForms scales by the whole scale), the size WinForms gave it. Otherwise its text part is measured with the font at the
		///scale and the rest (padding, glyph) is scaled: for a label or a button-like control both ways, for a date picker or up-down box the
		///height only (its width comes from LayoutManager).</summary>
		public static Size GetSizeAtScale(Control control,float scale) {
			Size size=control.Size;
			if(scale==1f || TextFitMeasurer.IsFullyScaledFontType(control)) {
				return size;
			}
			Font font=TextFitMeasurer.CreateMeasuringFont(control,scale);
			try {
				if((control is Label || control is ButtonBase) && control.AutoSize && !string.IsNullOrEmpty(control.Text)) {
					TextFormatFlags flags=TextFitMeasurer.GetStockFlags(control);
					Size sizeTextOwn=TextFitMeasurer.MeasureLineGdi(control.Text,control.Font,flags);
					Size sizeTextScaled=TextFitMeasurer.MeasureLineGdi(control.Text,font,flags);
					return new Size((int)Math.Round((size.Width-sizeTextOwn.Width)*scale)+sizeTextScaled.Width,
						(int)Math.Round((size.Height-sizeTextOwn.Height)*scale)+sizeTextScaled.Height);
				}
				return new Size(size.Width,(int)Math.Round((size.Height-control.Font.Height)*scale)+font.Height);
			}
			finally {
				if(!ReferenceEquals(font,control.Font)) {
					font.Dispose();
				}
			}
		}

		public static string Describe(Exception exception) {
			Exception inner=exception;
			while(inner.InnerException!=null && (inner is TypeInitializationException || inner is System.Reflection.TargetInvocationException || inner is AggregateException)) {
				inner=inner.InnerException;
			}
			string frame="";
			StackTrace stackTrace=new StackTrace(inner,false);
			foreach(StackFrame stackFrame in stackTrace.GetFrames()??new StackFrame[0]) {
				System.Reflection.MethodBase methodBase=stackFrame.GetMethod();
				if(methodBase?.DeclaringType==null) {
					continue;
				}
				string nameType=methodBase.DeclaringType.FullName??"";
				if(nameType.StartsWith("System.") || nameType.StartsWith("UiRefreshTests.")) {
					continue;
				}
				frame=" at "+nameType+"."+methodBase.Name;
				break;
			}
			string message=(inner.Message??"").Replace("\r"," ").Replace("\n"," ").Replace("\t"," ");
			if(message.Length>200) {
				message=message.Substring(0,200)+"...";
			}
			return inner.GetType().Name+": "+message+frame;
		}
	}

	///<summary>The bounds records at 100%, one file per run and one for the before set, written as the scan goes so they need not be kept.</summary>
	public class BoundsRecordFiles : IDisposable {
		private readonly Dictionary<string,StreamWriter> _dictWriters=new Dictionary<string,StreamWriter>();
		private readonly string _folder;

		public BoundsRecordFiles(string folder,List<string> listRuns) {
			_folder=folder;
			if(folder==null) {
				return;
			}
			Directory.CreateDirectory(folder);
			foreach(string run in new string[] { "before" }.Concat(listRuns)) {
				StreamWriter streamWriter=new StreamWriter(Path.Combine(folder,"bounds-record-"+run+".tsv"),false,new UTF8Encoding(false));
				streamWriter.WriteLine(TextFitBoundsRecord.HEADER);
				_dictWriters[run]=streamWriter;
			}
		}

		public void Write(string run,List<TextFitBoundsRow> listRows) {
			StreamWriter streamWriter;
			if(_folder==null || !_dictWriters.TryGetValue(run,out streamWriter)) {
				return;
			}
			foreach(TextFitBoundsRow row in listRows) {
				streamWriter.WriteLine(TextFitBoundsRecord.ToLine(row));
			}
		}

		public void Dispose() {
			foreach(StreamWriter streamWriter in _dictWriters.Values) {
				streamWriter.Dispose();
			}
			_dictWriters.Clear();
		}
	}
}
