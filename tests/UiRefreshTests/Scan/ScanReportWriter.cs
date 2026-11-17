// The scan's files (docs/port-notes/UI-0b.md, "The output"): types.tsv (every type, how it was built, or why not), per run overflow-<run>.tsv
// (new overflows only, with the pixels missing, the ink check, the free space and fixable or needs the owner), selfsizing-<run>.tsv (new
// overlaps and parent-edge passes of controls that size themselves), bounds-<run>.tsv (bounds differences at 100%), the bounds records, and
// summary.txt with the counts by window and by control type.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UiRefreshTests.TextFit;

namespace UiRefreshTests.Scan {
	public static class ScanReportWriter {
		public static void Write(ScanReport scanReport,ScanOptions scanOptions) {
			string folder=scanOptions.FolderOut;
			Directory.CreateDirectory(folder);
			WriteLines(Path.Combine(folder,"types.tsv"),GetTypeLines(scanReport));
			foreach(string run in scanReport.ListRuns) {
				WriteLines(Path.Combine(folder,"overflow-"+run+".tsv"),GetOverflowLines(scanReport.ListOverflows.Where(x => x.Run==run)));
				WriteLines(Path.Combine(folder,"selfsizing-"+run+".tsv"),GetSelfSizingLines(scanReport.ListSelfSizing.Where(x => x.Run==run)));
				WriteLines(Path.Combine(folder,"bounds-"+run+".tsv"),GetBoundsLines(scanReport.ListBoundsDifferences.Where(x => x.Item1==run).Select(x => x.Item2)));
			}
			WriteLines(Path.Combine(folder,"summary.txt"),GetSummaryLines(scanReport,scanOptions));
		}

		private static void WriteLines(string path,IEnumerable<string> listLines) {
			File.WriteAllLines(path,listLines,new UTF8Encoding(false));
		}

		private static string F(float value) {
			return value.ToString("0.##",CultureInfo.InvariantCulture);
		}

		public static IEnumerable<string> GetTypeLines(ScanReport scanReport) {
			yield return "assembly\ttype\tbuild\tstatus\tcontrols\tmeasured\tnote";
			foreach(ScanTypeResult result in scanReport.ListTypeResults) {
				string note=result.ScanType.Note;
				if(result.Reason!="") {
					note=(note=="" ? "" : note+"; ")+result.Reason;
				}
				yield return string.Join("\t",result.ScanType.Assembly,result.ScanType.WindowKey,result.ScanType.Kind.ToString(),result.Status,
					result.CountControls.ToString(CultureInfo.InvariantCulture),result.CountMeasured.ToString(CultureInfo.InvariantCulture),note);
			}
		}

		public static IEnumerable<string> GetOverflowLines(IEnumerable<ScanOverflow> listOverflows) {
			yield return "window\tbuild\tcontrol\ttype\tscale\tkind\tneed_width\tneed_height\tink\tavailable\ttext_line\tfree_right\tfree_below\tverdict";
			foreach(ScanOverflow overflow in listOverflows.OrderBy(x => x.Window,StringComparer.Ordinal).ThenBy(x => x.Scale).ThenBy(x => x.Path,StringComparer.Ordinal)) {
				string ink="";
				if(overflow.Result.IsInkChecked) {
					ink=overflow.Result.IsInkClipped ? "cut" : "inside";
				}
				yield return string.Join("\t",overflow.Window,overflow.Kind.ToString(),overflow.Path,overflow.ControlType,F(overflow.Scale),
					overflow.Result.Kind.ToString().ToLowerInvariant(),overflow.Result.NeedWidth.ToString(CultureInfo.InvariantCulture),
					overflow.Result.NeedHeight.ToString(CultureInfo.InvariantCulture),ink,
					overflow.Result.SizeAvailable.Width+"x"+overflow.Result.SizeAvailable.Height,overflow.Result.SizeLine.Width+"x"+overflow.Result.SizeLine.Height,
					overflow.FreeRight.ToString(CultureInfo.InvariantCulture),overflow.FreeBelow.ToString(CultureInfo.InvariantCulture),
					overflow.IsFixable ? "fixable" : "needs the owner");
			}
		}

		public static IEnumerable<string> GetSelfSizingLines(IEnumerable<ScanSelfSizing> listSelfSizing) {
			yield return "window\tbuild\tcontrol\ttype\tscale\twhat\tother\tbounds_before\tbounds_after\tverdict";
			foreach(ScanSelfSizing item in listSelfSizing.OrderBy(x => x.Window,StringComparer.Ordinal).ThenBy(x => x.Scale).ThenBy(x => x.Path,StringComparer.Ordinal)) {
				yield return string.Join("\t",item.Window,item.Kind.ToString(),item.Path,item.ControlType,F(item.Scale),item.What,item.Other,
					Rect(item.BoundsBefore),Rect(item.BoundsAfter),item.IsFixable ? "fixable" : "needs the owner");
			}
		}

		public static IEnumerable<string> GetBoundsLines(IEnumerable<TextFitBoundsDifference> listDifferences) {
			yield return "window\tcontrol\ttype\tkind\tbefore\tafter";
			foreach(TextFitBoundsDifference difference in listDifferences.OrderBy(x => x.Row.Window,StringComparer.Ordinal).ThenBy(x => x.Row.Path,StringComparer.Ordinal)) {
				yield return string.Join("\t",difference.Row.Window,difference.Row.Path,difference.Row.Type,difference.Kind,
					difference.RowBefore==null ? "" : Rect(difference.RowBefore.Bounds),Rect(difference.Row.Bounds));
			}
		}

		private static string Rect(System.Drawing.Rectangle rectangle) {
			return rectangle.X+","+rectangle.Y+","+rectangle.Width+"x"+rectangle.Height;
		}

		private static string ShortType(string typeFullName) {
			int index=typeFullName.LastIndexOf('.');
			string name=typeFullName.Substring(index+1);
			if(typeFullName.StartsWith("OpenPmsDental.UI.")) {
				return "UI."+name;
			}
			return name;
		}

		public static IEnumerable<string> GetSummaryLines(ScanReport scanReport,ScanOptions scanOptions) {
			List<string> listLines=new List<string>();
			listLines.Add("UI-0b tier 1 text-fit scan (docs/port-notes/UI-0b.md). Screen dpi "+F(scanReport.DpiX)+"; "
				+scanReport.Elapsed.TotalMinutes.ToString("0.0",CultureInfo.InvariantCulture)+" min.");
			listLines.Add("Scales (LayoutManager windows): "+string.Join(", ",scanOptions.ArrayScales.Select(x => F(x)))+"; text-scale run: "
				+(scanOptions.IsTextScaleRun ? F(scanOptions.TextScale) : "no")+".");
			listLines.Add("");
			listLines.Add("== Types");
			foreach(IGrouping<BuildKind,ScanTypeResult> group in scanReport.ListTypeResults.GroupBy(x => x.ScanType.Kind).OrderBy(x => x.Key)) {
				listLines.Add(group.Key+": "+group.Count()+" ("+string.Join(", ",group.GroupBy(x => x.Status).OrderBy(x => x.Key).Select(x => x.Key+" "+x.Count()))+")");
			}
			listLines.Add("All: "+scanReport.ListTypeResults.Count+" ("+string.Join(", ",scanReport.ListTypeResults.GroupBy(x => x.Status).OrderBy(x => x.Key)
				.Select(x => x.Key+" "+x.Count()))+"); controls in the built ones: "+scanReport.ListTypeResults.Sum(x => x.CountControls)
				+", with measured text: "+scanReport.ListTypeResults.Sum(x => x.CountMeasured));
			foreach(string run in scanReport.ListRuns) {
				listLines.Add("");
				listLines.Add("== Run "+run);
				List<ScanOverflow> listOverflows=scanReport.ListOverflows.Where(x => x.Run==run).ToList();
				List<ScanSelfSizing> listSelfSizing=scanReport.ListSelfSizing.Where(x => x.Run==run).ToList();
				foreach(float scale in listOverflows.Select(x => x.Scale).Concat(listSelfSizing.Select(x => x.Scale)).Concat(new float[] { 1f }).Distinct().OrderBy(x => x)) {
					List<ScanOverflow> listAtScale=listOverflows.Where(x => x.Scale==scale).ToList();
					int existing;
					scanReport.DictExistingOverflows.TryGetValue(run+"\t"+scale.ToString(CultureInfo.InvariantCulture),out existing);
					List<ScanSelfSizing> listSelfAtScale=listSelfSizing.Where(x => x.Scale==scale).ToList();
					listLines.Add("Scale "+F(scale)+": new overflows "+listAtScale.Count+" in "+listAtScale.Select(x => x.Window).Distinct().Count()+" windows"
						+" (height "+listAtScale.Count(x => x.Result.Kind==TextFitKind.Height)
						+", of them ink-checked "+listAtScale.Count(x => x.Result.IsInkChecked)+": cut "+listAtScale.Count(x => x.Result.IsInkChecked && x.Result.IsInkClipped)
						+", inside "+listAtScale.Count(x => x.Result.IsInkChecked && !x.Result.IsInkClipped)
						+"; width "+listAtScale.Count(x => x.Result.Kind==TextFitKind.Width)+"; another line "+listAtScale.Count(x => x.Result.Kind==TextFitKind.Line)
						+"; fixable "+listAtScale.Count(x => x.IsFixable)+", needs the owner "+listAtScale.Count(x => !x.IsFixable)+")"
						+"; already overflowing in today's font "+existing
						+"; self-sizing: overlaps "+listSelfAtScale.Count(x => x.What=="overlap")+", parent-edge "+listSelfAtScale.Count(x => x.What=="parent-edge")
						+" in "+listSelfAtScale.Select(x => x.Window).Distinct().Count()+" windows (fixable "+listSelfAtScale.Count(x => x.IsFixable)
						+", needs the owner "+listSelfAtScale.Count(x => !x.IsFixable)+")");
				}
				List<ScanOverflow> listAt100=listOverflows.Where(x => x.Scale==1f).ToList();
				listLines.Add("New overflows at 100% by control type: "+string.Join(", ",listAt100.GroupBy(x => ShortType(x.ControlType))
					.OrderByDescending(x => x.Count()).ThenBy(x => x.Key).Select(x => x.Key+" "+x.Count())));
				listLines.Add("New overflows at 100% by window (the largest 30): "+string.Join(", ",listAt100.GroupBy(x => ShortType(x.Window))
					.OrderByDescending(x => x.Count()).ThenBy(x => x.Key).Take(30).Select(x => x.Key+" "+x.Count())));
				List<ScanSelfSizing> listSelfAt100=listSelfSizing.Where(x => x.Scale==1f).ToList();
				listLines.Add("Self-sizing at 100% by control type: "+string.Join(", ",listSelfAt100.GroupBy(x => ShortType(x.ControlType))
					.OrderByDescending(x => x.Count()).ThenBy(x => x.Key).Select(x => x.Key+" "+x.Count())));
				listLines.Add("Self-sizing at 100% by window (the largest 20): "+string.Join(", ",listSelfAt100.GroupBy(x => ShortType(x.Window))
					.OrderByDescending(x => x.Count()).ThenBy(x => x.Key).Take(20).Select(x => x.Key+" "+x.Count())));
				List<TextFitBoundsDifference> listBounds=scanReport.ListBoundsDifferences.Where(x => x.Item1==run).Select(x => x.Item2).ToList();
				listLines.Add("Bounds differences at 100%: "+listBounds.Count+" in "+listBounds.Select(x => x.Row.Window).Distinct().Count()+" windows ("
					+string.Join(", ",listBounds.GroupBy(x => x.Kind).OrderBy(x => x.Key).Select(x => x.Key+" "+x.Count()))+")");
				Dictionary<string,BuildKind> dictKinds=scanReport.ListTypeResults.ToDictionary(x => x.ScanType.WindowKey,x => x.ScanType.Kind);
				listLines.Add("Bounds differences at 100% by build: "+string.Join(", ",listBounds.GroupBy(x => dictKinds[x.Row.Window]).OrderBy(x => x.Key)
					.Select(x => x.Key+" "+x.Count()+" in "+x.Select(y => y.Row.Window).Distinct().Count()+" windows")));
				listLines.Add("Bounds differences at 100% by window (the largest 20): "+string.Join(", ",listBounds.GroupBy(x => ShortType(x.Row.Window))
					.OrderByDescending(x => x.Count()).ThenBy(x => x.Key).Take(20).Select(x => x.Key+" "+x.Count())));
			}
			listLines.Add("");
			listLines.Add("== Types not built");
			foreach(ScanTypeResult result in scanReport.ListTypeResults.Where(x => x.Status!="built").OrderBy(x => x.Status).ThenBy(x => x.ScanType.WindowKey)) {
				listLines.Add(result.Status+"\t"+result.ScanType.WindowKey+"\t"+result.Reason);
			}
			listLines.Add("not-loadable\tOpenPmsDentalHelper32 (every form)\t"+WindowBuilder.NOTE_HELPER32);
			return listLines;
		}
	}
}
