// The recording of the old tooth chart (DirectX 9, OpenGL and Simple 2D) for every synthetic scene: the "before" pictures of the WPF chart track
// (package TC-0 of docs/specs/wpf-tooth-chart-plan.md, launch check L-1). TC-9a retired the DirectX and OpenGL engines, so the recorder no longer
// records (a skipped test says so); the recording it made (Baselines\legacy, Baselines\legacy-smoothed) is kept (O-32) and still read: the parity
// probes, the gallery, the manifest and index, and the check of a recording against it (CompareLegacyRecording).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;

namespace ToothChartImageTests {
	///<summary>What the recorder does with the smoothed DirectX pictures of the gallery (LegacyRecorder.ListSmoothed).</summary>
	public enum SmoothedMode {
		///<summary>The default: the baseline pictures, then the smoothed ones.</summary>
		Also,
		///<summary>The baseline pictures only; the smoothed folder is left as it is.</summary>
		None,
		///<summary>The smoothed pictures only; the baseline pictures are left as they are, and the manifest and index are rewritten.</summary>
		Only,
	}

	///<summary>One smoothed DirectX picture of the gallery: a scene at one of its sizes.</summary>
	public class SmoothedPicture {
		public string SceneName;
		public ChartSize ChartSize;

		public SmoothedPicture(string sceneName,ChartSize chartSize) {
			SceneName=sceneName;
			ChartSize=chartSize;
		}

		///<summary>The picture's path in the smoothed folder, for example "410x307/restored-adult.png".</summary>
		public string PathRelative {
			get {
				return ChartSize.FolderName+"/"+SceneName+".png";
			}
		}
	}

	///<summary>The recorder of the legacy chart pictures (retired in TC-9a with the DirectX and OpenGL engines: RecordLegacyChart is skipped) and the
	///check of a recording against the kept one. Until TC-9a, as follows.
	///
	///Record (the lead's run, L-1; from the repository root, after a build of OpenDental.Local.slnf through the build lane) with the lane's vstest
	///of Visual Studio Build Tools 2026, which
	///  "%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -version [18.0,19.0) -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe
	///finds:
	///  set OPMS_RECORD_LEGACY_CHART=1
	///  "&lt;that vstest.console.exe&gt;" tests\ToothChartImageTests\bin\Debug\ToothChartImageTests.dll /Platform:x86 /Tests:RecordLegacyChart /logger:console;verbosity=detailed
	///(or through the lane itself: odbuild.ps1 -Root . -TestOnly -Test tests\ToothChartImageTests\bin\Debug\ToothChartImageTests.dll
	///-TestFilter Name=RecordLegacyChart, which prints only the result lines, not the recorder's summary).
	///The pictures go to tests\ToothChartImageTests\Baselines\legacy\&lt;mode&gt;\&lt;width&gt;x&lt;height&gt;\&lt;scene&gt;.png, each with a .txt
	///next to it, plus manifest.txt and index.html for the whole folder. DirectX uses the first standard device format that works, with
	///multisampling turned off so that the same scene gives the same pixels (see FindDirectXFormat); each .txt names both formats, the adapter that
	///drew the picture and whether a second picture of the same state was identical. Then a few DirectX pictures for the gallery only (ListSmoothed)
	///are drawn with the standard format as it is, multisampled as the program draws them, into Baselines\legacy-smoothed\&lt;width&gt;x&lt;height&gt;\
	///(the output folder's name plus "-smoothed"); they are never compared. Optional: OPMS_RECORD_LEGACY_CHART_OUT (another folder),
	///OPMS_RECORD_LEGACY_CHART_ENGINES (for example "DirectX"), OPMS_RECORD_LEGACY_CHART_SCENES (scene names, for a quick look),
	///OPMS_RECORD_LEGACY_CHART_DIRECTX_FORMAT (a device format string instead of the first standard format that works; then there was no search,
	///so no smoothed pictures are drawn and their folder is left as it is) and OPMS_RECORD_LEGACY_CHART_SMOOTHED ("0": no smoothed pictures;
	///"only": the smoothed pictures alone).
	///
	///Check a re-recording (after a change of the chart's code, on the same PC): record into a scratch folder with OPMS_RECORD_LEGACY_CHART_OUT
	///(and OPMS_RECORD_LEGACY_CHART_SMOOTHED=0), then
	///  set OPMS_COMPARE_LEGACY_CHART=&lt;that folder&gt;
	///  "&lt;the same vstest.console.exe&gt;" tests\ToothChartImageTests\bin\Debug\ToothChartImageTests.dll /Platform:x86 /Tests:CompareLegacyRecording /logger:console;verbosity=detailed
	///which fails unless every baseline picture of the compared modes and scenes (OPMS_RECORD_LEGACY_CHART_ENGINES and _SCENES, all when unset) is
	///in that folder, has the same bytes as its baseline, and its .txt has the same "Pixels SHA-256". The PNG bytes also depend on the framework's
	///DeflateStream (PngWriter), so a check across the x64 or .NET 10 switch (TC-10, S3b-5) sets OPMS_COMPARE_LEGACY_CHART_PIXELS_ONLY=1: the decoded
	///pixels and the .txt hashes are compared, not the file bytes.</summary>
	[TestClass]
	public class LegacyRecorder {
		public const string EnvRecord="OPMS_RECORD_LEGACY_CHART";
		public const string EnvOut="OPMS_RECORD_LEGACY_CHART_OUT";
		public const string EnvEngines="OPMS_RECORD_LEGACY_CHART_ENGINES";
		public const string EnvScenes="OPMS_RECORD_LEGACY_CHART_SCENES";
		public const string EnvDirectXFormat="OPMS_RECORD_LEGACY_CHART_DIRECTX_FORMAT";
		public const string EnvSmoothed="OPMS_RECORD_LEGACY_CHART_SMOOTHED";
		public const string EnvCompare="OPMS_COMPARE_LEGACY_CHART";
		public const string EnvComparePixelsOnly="OPMS_COMPARE_LEGACY_CHART_PIXELS_ONLY";
		///<summary>The modes in the order they are recorded.</summary>
		public static readonly DrawingMode[] ArrayEngines={ DrawingMode.DirectX,DrawingMode.OpenGL,DrawingMode.Simple2D };
		///<summary>The smoothed DirectX pictures of the gallery (owner answer of 2026-11-08): the real look of today's chart on a computer whose
		///standard format multisamples, next to the unsmoothed baseline of the same scene and size. Whole mouths at both chart sizes, a bridge, a
		///denture, a root canal with post and crown, the empty chart, the big window and the perio chart.</summary>
		public static readonly List<SmoothedPicture> ListSmoothed=new List<SmoothedPicture> {
			new SmoothedPicture("empty-universal",ChartScenes.SizeModule),
			new SmoothedPicture("restored-adult",ChartScenes.SizeModule),
			new SmoothedPicture("restored-adult",ChartScenes.SizePrintout),
			new SmoothedPicture("treatment-plan",ChartScenes.SizeModule),
			new SmoothedPicture("treatment-plan",ChartScenes.SizePrintout),
			new SmoothedPicture("bridge-anterior",ChartScenes.SizeModule),
			new SmoothedPicture("denture-upper-dark",ChartScenes.SizeModule),
			new SmoothedPicture("rct-with-post-and-crown",ChartScenes.SizeModule),
			new SmoothedPicture("size-big-window",ChartScenes.SizeBigWindow),
			new SmoothedPicture("perio-full-exam",ChartScenes.SizePerio),
		};

		///<summary>Recorded Baselines\legacy (DirectX, OpenGL and Simple 2D, without multisampling) and Baselines\legacy-smoothed (a few DirectX pictures
		///multisampled, for the gallery) on a visible window of the lead's desktop, through each engine's GetBitmap, with a .txt of the facts of each
		///picture. TC-9a deleted the DirectX and OpenGL engines and with them the recording code (the host form, the DirectX format search without
		///multisampling, the adapter and pixel-format facts); the 715 pictures of TC-0's recording, and those recorded since, are kept (O-32) and can
		///never be recorded again. The Simple 2D chart that stays is guarded by its own baselines (Baselines\simple2d-net48).</summary>
		[TestMethod]
		[Ignore("The DirectX and OpenGL engines were retired in TC-9a: today's chart cannot be recorded again. The recording (Baselines\\legacy and "
			+"Baselines\\legacy-smoothed) is kept and read by the parity probes, the gallery and CompareLegacyRecording.")]
		public void RecordLegacyChart() {
			Assert.Inconclusive("Retired in TC-9a.");
		}

		[TestMethod]
		public void CompareLegacyRecording() {
			string folderNew=Environment.GetEnvironmentVariable(EnvCompare);
			if(string.IsNullOrWhiteSpace(folderNew)) {
				Assert.Inconclusive("Set "+EnvCompare+" to a folder recorded with "+EnvOut+" to compare it with Baselines\\legacy.");
			}
			List<DrawingMode> listEngines=ParseEngines(Environment.GetEnvironmentVariable(EnvEngines));
			List<string> listNames=ParseSceneNames(Environment.GetEnvironmentVariable(EnvScenes));
			listNames?.ForEach(x => ChartScenes.Get(x));//a misspelt name throws here, rather than comparing nothing
			bool isPixelsOnly=Environment.GetEnvironmentVariable(EnvComparePixelsOnly)=="1";
			LegacyComparison legacyComparison=LegacyComparison.Compare(TestPaths.BaselinesLegacy,folderNew,listEngines,listNames,isPixelsOnly);
			Console.WriteLine(legacyComparison.GetSummary());
			Assert.IsTrue(legacyComparison.IsPass,legacyComparison.GetSummary());
		}

		///<summary>"DirectX,OpenGL" gives those modes in recording order; empty gives all three.</summary>
		public static List<DrawingMode> ParseEngines(string engines) {
			if(string.IsNullOrWhiteSpace(engines)) {
				return ArrayEngines.ToList();
			}
			List<DrawingMode> listEngines=engines.Split(',').Select(x => (DrawingMode)Enum.Parse(typeof(DrawingMode),x.Trim(),true)).ToList();
			return ArrayEngines.Where(x => listEngines.Contains(x)).ToList();
		}

		///<summary>"a, b" gives the scene names a and b; empty gives null, which means every scene.</summary>
		public static List<string> ParseSceneNames(string scenes) {
			if(string.IsNullOrWhiteSpace(scenes)) {
				return null;
			}
			return scenes.Split(',').Select(x => x.Trim()).Where(x => x!="").ToList();
		}

		///<summary>"0" or "no" gives None, "only" gives Only, anything else (unset, "1") gives Also.</summary>
		public static SmoothedMode ParseSmoothed(string smoothed) {
			string value=(smoothed??"").Trim().ToLowerInvariant();
			if(value=="0" || value=="no") {
				return SmoothedMode.None;
			}
			if(value=="only") {
				return SmoothedMode.Only;
			}
			return SmoothedMode.Also;
		}

		///<summary>The folder of the smoothed gallery pictures of a recording folder: its path plus "-smoothed", so Baselines\legacy gives
		///Baselines\legacy-smoothed, beside it and never inside it.</summary>
		public static string GetSmoothedFolder(string folderOut) {
			return folderOut.TrimEnd('\\','/')+"-smoothed";
		}
	}

	///<summary>What is left of the recorder after TC-9a: the reading of a recording's .txt files, its manifest and index, and the rule that decided
	///whether a run drew the smoothed pictures (kept with its test, as the record of how Baselines\legacy-smoothed was made).</summary>
	public class LegacyRecording {
		///<summary>Whether this run can draw the smoothed pictures, decided from the DirectX formats alone (no device): formatUsed is the baseline's
		///format (null when DirectX cannot start), formatSearched the standard search's own and isFormatGiven whether formatUsed came from
		///OPMS_RECORD_LEGACY_CHART_DIRECTX_FORMAT. They are drawn only with a device format the standard search found that differs from the
		///baseline's, which is the multisampled format the program saves for a computer with no format saved. Not when DirectX cannot start, not when
		///the format was given (there was no search: formatSearched then names no format, and a smoothed picture drawn with it would be a fallback's
		///under "DirectX, smoothed"), not when formatSearched is no device format, and not when the search's format is the baseline's (it does not
		///multisample, or does not start without multisampling): the pictures would be the baseline's. reasonSkip says why not, or is null.</summary>
		public static bool CanRecordSmoothed(bool isFormatGiven,string formatSearched,string formatUsed,out string reasonSkip) {
			reasonSkip=null;
			if(formatUsed==null) {
				reasonSkip="DirectX cannot start.";
			}
			else if(isFormatGiven) {
				reasonSkip="the DirectX format was given ("+LegacyRecorder.EnvDirectXFormat+"="+formatUsed+"), so there is no standard search's format to draw "
					+"them with.";
			}
			else if(formatSearched==null || formatSearched.IndexOf(';')<0) {
				reasonSkip="the standard search found no device format ("+(formatSearched??"none")+").";
			}
			else if(formatSearched==formatUsed) {
				reasonSkip="the DirectX format used ("+formatUsed+") is the standard search's own, so they would be the baseline's.";
			}
			return reasonSkip==null;
		}

		#region Facts
		private static string GetColorSetText(ChartColorSet chartColorSet) {
			switch(chartColorSet) {
				case ChartColorSet.Module:
					return "module (Chart Graphic Colors 10-13)";
				case ChartColorSet.Printout:
					return "printout (Chart Graphic Colors 14 and 15)";
				default:
					return "perio (white, black and the perio colours)";
			}
		}
		#endregion Facts

		#region Manifest and index
		///<summary>Reads the "Key: value" lines of a recording's .txt.</summary>
		public static Dictionary<string,string> ReadSidecar(string pathText) {
			Dictionary<string,string> dict=new Dictionary<string,string>();
			foreach(string line in File.ReadAllLines(pathText)) {
				int index=line.IndexOf(": ",StringComparison.Ordinal);
				if(index<=0) {
					continue;
				}
				string key=line.Substring(0,index);
				string value=line.Substring(index+2);
				if(dict.ContainsKey(key)) {
					dict[key]+="; "+value;
				}
				else {
					dict[key]=value;
				}
			}
			return dict;
		}

		///<summary>Rewrites manifest.txt (one tab-separated line per picture) and index.html (the pictures side by side, one row per scene and size)
		///from every .txt under folder, so a run that records one mode keeps the others in both. The smoothed pictures of the folder's smoothed folder
		///(LegacyRecorder.GetSmoothedFolder) get a column of the index next to DirectX, when there are any; the manifest lists the compared pictures
		///only.</summary>
		public static void WriteManifestAndIndex(string folder) {
			Dictionary<string,Dictionary<string,string>> dictSidecars=new Dictionary<string,Dictionary<string,string>>();
			foreach(DrawingMode engine in LegacyRecorder.ArrayEngines) {
				string folderEngine=Path.Combine(folder,engine.ToString());
				if(!Directory.Exists(folderEngine)) {
					continue;
				}
				foreach(string pathText in Directory.GetFiles(folderEngine,"*.txt",SearchOption.AllDirectories)) {
					string pathRelative=pathText.Substring(folder.TrimEnd('\\').Length+1).Replace('\\','/');
					dictSidecars[pathRelative.Substring(0,pathRelative.Length-4)+".png"]=ReadSidecar(pathText);
				}
			}
			//Smoothed pictures, keyed "<size>/<scene>.png" as in their folder.
			Dictionary<string,Dictionary<string,string>> dictSmoothed=new Dictionary<string,Dictionary<string,string>>();
			string folderSmoothed=LegacyRecorder.GetSmoothedFolder(folder);
			if(Directory.Exists(folderSmoothed)) {
				foreach(string pathText in Directory.GetFiles(folderSmoothed,"*.txt",SearchOption.AllDirectories)) {
					string pathRelative=pathText.Substring(folderSmoothed.Length+1).Replace('\\','/');
					dictSmoothed[pathRelative.Substring(0,pathRelative.Length-4)+".png"]=ReadSidecar(pathText);
				}
			}
			WriteManifest(folder,dictSidecars);
			WriteIndex(folder,dictSidecars,Path.GetFileName(folderSmoothed),dictSmoothed);
		}

		private static void WriteManifest(string folder,Dictionary<string,Dictionary<string,string>> dictSidecars) {
			string[] arrayColumns={ "Scene","Group","Requested mode","Started mode","Size","Colours","Bitmap","Alpha","Colours in the picture",
				"Pixels SHA-256","Repeatable","Device or pixel format","DirectX adapter","Failed" };
			List<string> listLines=new List<string>();
			listLines.Add("# Today's tooth chart, recorded by ToothChartImageTests.LegacyRecorder (package TC-0, docs/specs/wpf-tooth-chart-plan.md).");
			listLines.Add("# One line per picture, from the .txt next to it, which also names the OpenGL renderer, every adapter of the computer with its "
				+"driver, and the Windows build. \"DirectX adapter\" is the adapter that drew a DirectX picture.");
			listLines.Add("File\t"+string.Join("\t",arrayColumns));
			foreach(string pathPng in dictSidecars.Keys.OrderBy(x => x,StringComparer.Ordinal)) {
				Dictionary<string,string> dict=dictSidecars[pathPng];
				listLines.Add(pathPng+"\t"+string.Join("\t",arrayColumns.Select(x => dict.TryGetValue(x,out string value)?value:"")));
			}
			File.WriteAllLines(Path.Combine(folder,"manifest.txt"),listLines,new UTF8Encoding(false));
		}

		private static void WriteIndex(string folder,Dictionary<string,Dictionary<string,string>> dictSidecars,string folderNameSmoothed,
			Dictionary<string,Dictionary<string,string>> dictSmoothed)
		{
			bool hasSmoothed=dictSmoothed.Count>0;
			StringBuilder sb=new StringBuilder();
			sb.Append("<!doctype html>\r\n<html lang=\"en\">\r\n<head>\r\n<meta charset=\"utf-8\">\r\n");
			sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\r\n");
			sb.Append("<title>Tooth chart recordings</title>\r\n<style>\r\n");
			sb.Append("body{font-family:Segoe UI,Arial,sans-serif;margin:16px;background:#fff;color:#111}\r\n");
			sb.Append("table{border-collapse:collapse}td,th{border:1px solid #bbb;padding:4px;vertical-align:top;font-size:12px;text-align:left}\r\n");
			sb.Append("img{display:block;max-width:100%;height:auto}.cap{color:#555}.name{font-weight:bold}.warn{color:#b00}.smooth{color:#05a}\r\n");
			sb.Append("</style>\r\n</head>\r\n<body>\r\n<h1>Today's tooth chart, as recorded</h1>\r\n");
			sb.Append("<p>The pictures of the DirectX, OpenGL and Simple 2D charts for every synthetic scene (package TC-0 of the WPF tooth chart plan). "
				+"They are the \"before\" column of the owner galleries and the target of the parity probes. Under each picture: the mode that really "
				+"drew it, the pixel format of the returned bitmap and its alpha, and for DirectX the adapter that drew it. Perio is drawn by DirectX "
				+"only. DirectX was recorded without multisampling, so its edges are harder than on a computer that uses four samples: with them the "
				+"same scene gave other pixels on every draw, and a later recording could not be compared.</p>\r\n");
			if(hasSmoothed) {
				sb.Append("<p class=\"smooth\">The column \"DirectX, smoothed\" shows a few scenes as the program draws them on this computer: with the "
					+"standard search's own format, multisampled. They are for looking only (folder "+WebUtility.HtmlEncode(folderNameSmoothed)
					+"), never compared; the unsmoothed picture to their left is the baseline.</p>\r\n");
			}
			List<ChartScene> listScenes=ChartScenes.GetAll();
			sb.Append("<p><label>Group: <select id=\"group\"><option value=\"\">all</option>");
			foreach(string group in listScenes.Select(x => x.Group).Distinct()) {
				sb.Append("<option>"+WebUtility.HtmlEncode(group)+"</option>");
			}
			sb.Append("</select></label></p>\r\n<table>\r\n<thead><tr><th>Scene</th><th>Size and colours</th>");
			foreach(DrawingMode engine in LegacyRecorder.ArrayEngines) {
				sb.Append("<th>"+engine+"</th>");
				if(engine==DrawingMode.DirectX && hasSmoothed) {
					sb.Append("<th>DirectX, smoothed</th>");
				}
			}
			sb.Append("</tr></thead>\r\n");
			foreach(ChartScene scene in listScenes) {
				List<ChartSize> listSizes=ChartScenes.GetSizes(scene);
				sb.Append("<tbody data-group=\""+WebUtility.HtmlEncode(scene.Group)+"\" id=\""+scene.Name+"\">\r\n");
				for(int i=0;i<listSizes.Count;i++) {
					sb.Append("<tr>");
					if(i==0) {
						sb.Append("<td rowspan=\""+listSizes.Count+"\"><div class=\"name\">"+scene.Name+"</div><div class=\"cap\">"
							+WebUtility.HtmlEncode(scene.Group)+"</div><div>"+WebUtility.HtmlEncode(scene.Description)+"</div></td>");
					}
					sb.Append("<td>"+listSizes[i].FolderName+"<div class=\"cap\">"+WebUtility.HtmlEncode(GetColorSetText(listSizes[i].ColorSet))+"</div></td>");
					foreach(DrawingMode engine in LegacyRecorder.ArrayEngines) {
						string pathPng=engine+"/"+listSizes[i].FolderName+"/"+scene.Name+".png";
						AppendIndexCell(sb,scene,listSizes[i],engine,pathPng,dictSidecars.TryGetValue(pathPng,out Dictionary<string,string> dict)?dict:null,false);
						if(engine==DrawingMode.DirectX && hasSmoothed) {
							string pathSmoothed=listSizes[i].FolderName+"/"+scene.Name+".png";
							if(dictSmoothed.TryGetValue(pathSmoothed,out Dictionary<string,string> dictSmooth)) {
								AppendIndexCell(sb,scene,listSizes[i],engine,"../"+folderNameSmoothed+"/"+pathSmoothed,dictSmooth,true);
							}
							else {
								sb.Append("<td class=\"cap\"></td>");
							}
						}
					}
					sb.Append("</tr>\r\n");
				}
				sb.Append("</tbody>\r\n");
			}
			sb.Append("</table>\r\n<script>\r\n");
			sb.Append("document.getElementById('group').addEventListener('change',function(){var g=this.value;"
				+"document.querySelectorAll('tbody[data-group]').forEach(function(b){b.style.display=(!g||b.getAttribute('data-group')===g)?'':'none';});});\r\n");
			sb.Append("</script>\r\n</body>\r\n</html>\r\n");
			File.WriteAllText(Path.Combine(folder,"index.html"),sb.ToString(),new UTF8Encoding(false));
		}

		///<summary>One picture cell of the index: the picture (or why there is none) and what drew it.</summary>
		private static void AppendIndexCell(StringBuilder sb,ChartScene scene,ChartSize chartSize,DrawingMode engine,string src,Dictionary<string,string> dict,
			bool isSmoothed)
		{
			if(dict==null) {
				sb.Append("<td class=\"cap\">"+(scene.Kind==ChartSceneKind.Perio && engine!=DrawingMode.DirectX?"DirectX only":"not recorded")+"</td>");
				return;
			}
			sb.Append("<td>");
			if(dict.ContainsKey("Failed")) {
				sb.Append("<div class=\"warn\">Failed: "+WebUtility.HtmlEncode(dict["Failed"])+"</div>");
			}
			else {
				sb.Append("<img loading=\"lazy\" src=\""+src+"\" width=\""+chartSize.Size.Width+"\" height=\""+chartSize.Size.Height
					+"\" alt=\""+scene.Name+" in "+engine+(isSmoothed?", smoothed":"")+"\">");
			}
			string started=dict.TryGetValue("Started mode",out string value)?value:"?";
			string bitmap=dict.TryGetValue("Bitmap",out value)?value:"";
			string alpha=dict.TryGetValue("Alpha",out value)?value:"";
			bool isOtherMode=started!=engine.ToString();
			if(isSmoothed) {
				string format=dict.TryGetValue("Device or pixel format",out value)?value:"";
				sb.Append("<div class=\"smooth\">smoothed, for the gallery only (never compared): "+WebUtility.HtmlEncode(format)+"</div>");
			}
			sb.Append("<div class=\""+(isOtherMode?"warn":"cap")+"\">drawn by "+WebUtility.HtmlEncode(started)+"; "+WebUtility.HtmlEncode(bitmap)+"; "
				+WebUtility.HtmlEncode(alpha)+"</div>");
			if(dict.TryGetValue("DirectX adapter",out value)) {
				sb.Append("<div class=\"cap\">adapter "+WebUtility.HtmlEncode(value)+"</div>");
			}
			sb.Append("</td>");
		}
		#endregion Manifest and index
	}

	///<summary>The check of a re-recording against the legacy baseline (LegacyRecorder.CompareLegacyRecording). Every baseline picture of the compared
	///modes and scenes must be in the new recording, and every new picture must have a baseline. A picture matches when the "Pixels SHA-256" lines of
	///both .txt files are equal and, by default, both PNG files have the same bytes; in pixels-only mode the decoded pixels are compared instead of the
	///bytes, because the PNG bytes depend on the framework's DeflateStream (PngWriter). Only the mode folders (DirectX, OpenGL, Simple2D) are read:
	///the smoothed gallery pictures beside the recording are never compared.</summary>
	public class LegacyComparison {
		///<summary>The baseline folder the new recording was compared with, as Compare was given it.</summary>
		public string FolderBaseline;
		public bool IsPixelsOnly;
		public List<DrawingMode> ListEngines;
		///<summary>Null means every scene.</summary>
		public List<string> ListSceneNames;
		///<summary>Paths relative to the recording folder, "DirectX/410x307/empty-universal.png", that match.</summary>
		public List<string> ListMatching=new List<string>();
		///<summary>Baseline pictures of the compared modes and scenes that the new recording does not have.</summary>
		public List<string> ListMissing=new List<string>();
		///<summary>New pictures with no baseline.</summary>
		public List<string> ListNoBaseline=new List<string>();
		///<summary>Pictures that do not match, each with the reason.</summary>
		public List<string> ListDifferent=new List<string>();

		///<summary>True when at least one picture was compared, every one matches, none is missing and none lacks a baseline.</summary>
		public bool IsPass {
			get {
				return ListMatching.Count>0 && ListMissing.Count==0 && ListNoBaseline.Count==0 && ListDifferent.Count==0;
			}
		}

		public static LegacyComparison Compare(string folderBaseline,string folderNew,List<DrawingMode> listEngines,List<string> listSceneNames,
			bool isPixelsOnly)
		{
			LegacyComparison legacyComparison=new LegacyComparison();
			legacyComparison.FolderBaseline=folderBaseline;
			legacyComparison.IsPixelsOnly=isPixelsOnly;
			legacyComparison.ListEngines=listEngines;
			legacyComparison.ListSceneNames=listSceneNames;
			List<string> listBaseline=GetPictures(folderBaseline,listEngines,listSceneNames);
			List<string> listNew=GetPictures(folderNew,listEngines,listSceneNames);
			legacyComparison.ListMissing.AddRange(listBaseline.Except(listNew));
			legacyComparison.ListNoBaseline.AddRange(listNew.Except(listBaseline));
			foreach(string pathRelative in listNew.Intersect(listBaseline)) {
				string reason=ComparePicture(Path.Combine(folderBaseline,pathRelative),Path.Combine(folderNew,pathRelative),isPixelsOnly);
				if(reason==null) {
					legacyComparison.ListMatching.Add(pathRelative);
				}
				else {
					legacyComparison.ListDifferent.Add(pathRelative+": "+reason);
				}
			}
			return legacyComparison;
		}

		///<summary>The PNG files under folder\&lt;mode&gt; for the given modes, and only of the given scenes unless listSceneNames is null, as
		///"&lt;mode&gt;/&lt;size&gt;/&lt;scene&gt;.png" in ordinal order.</summary>
		public static List<string> GetPictures(string folder,List<DrawingMode> listEngines,List<string> listSceneNames) {
			List<string> listPaths=new List<string>();
			foreach(DrawingMode engine in listEngines) {
				string folderEngine=Path.Combine(folder,engine.ToString());
				if(!Directory.Exists(folderEngine)) {
					continue;
				}
				foreach(string path in Directory.GetFiles(folderEngine,"*.png",SearchOption.AllDirectories)) {
					if(listSceneNames!=null && !listSceneNames.Contains(Path.GetFileNameWithoutExtension(path))) {
						continue;
					}
					listPaths.Add(path.Substring(folder.TrimEnd('\\','/').Length+1).Replace('\\','/'));
				}
			}
			return listPaths.OrderBy(x => x,StringComparer.Ordinal).ToList();
		}

		///<summary>Null when the new picture matches its baseline, otherwise why not. The .txt files' "Pixels SHA-256" lines must be equal; then the
		///file bytes (or, in pixels-only mode, the decoded pixels).</summary>
		public static string ComparePicture(string pathBaseline,string pathNew,bool isPixelsOnly) {
			string hashBaseline=ReadPixelsSha256(Path.ChangeExtension(pathBaseline,".txt"));
			string hashNew=ReadPixelsSha256(Path.ChangeExtension(pathNew,".txt"));
			List<string> listReasons=new List<string>();
			if(hashBaseline==null || hashNew==null) {
				listReasons.Add("no \"Pixels SHA-256\" in the "+(hashBaseline==null?"baseline's":"new")+" .txt");
			}
			else if(hashBaseline!=hashNew) {
				listReasons.Add("the .txt files' pixel SHA-256 differ (baseline "+hashBaseline+", new "+hashNew+")");
			}
			bool isSame;
			if(isPixelsOnly) {
				isSame=GetPixelsSha256(pathBaseline)==GetPixelsSha256(pathNew);
			}
			else {
				isSame=ChartImageCompare.AreFilesIdentical(pathBaseline,pathNew);
			}
			if(!isSame) {
				using Bitmap bitmapBaseline=new Bitmap(pathBaseline);
				using Bitmap bitmapNew=new Bitmap(pathNew);
				listReasons.Add((isPixelsOnly?"the pixels differ. ":"the bytes differ. ")+ChartImageCompare.Compare(bitmapBaseline,bitmapNew));
			}
			if(listReasons.Count==0) {
				return null;
			}
			return string.Join("; ",listReasons);
		}

		///<summary>The "Pixels SHA-256" of a recording's .txt, or null when there is no .txt or no such line.</summary>
		public static string ReadPixelsSha256(string pathText) {
			if(!File.Exists(pathText)) {
				return null;
			}
			return LegacyRecording.ReadSidecar(pathText).TryGetValue("Pixels SHA-256",out string value)?value:null;
		}

		///<summary>The SHA-256 of a PNG's decoded pixels as 32-bit ARGB, the hash the recorder writes in "Pixels SHA-256".</summary>
		public static string GetPixelsSha256(string pathPng) {
			using Bitmap bitmap=new Bitmap(pathPng);
			return ChartImageCompare.Sha256(ChartImageCompare.GetPixelsArgb(bitmap));
		}

		public string GetSummary() {
			StringBuilder stringBuilder=new StringBuilder();
			int countCompared=ListMatching.Count+ListDifferent.Count;
			stringBuilder.Append("Compared "+countCompared+" pictures of "+string.Join(", ",ListEngines)+" ("
				+(ListSceneNames==null?"every scene":"scenes "+string.Join(", ",ListSceneNames))+") with "+GetBaselineName()+", "
				+(IsPixelsOnly?"by their decoded pixels":"byte for byte")+" and by the \"Pixels SHA-256\" of their .txt: "+ListMatching.Count+" match, "
				+ListDifferent.Count+" differ, "+ListMissing.Count+" baseline pictures are missing from the new recording, "+ListNoBaseline.Count
				+" new pictures have no baseline.\r\n");
			foreach(DrawingMode engine in ListEngines) {
				foreach(IGrouping<string,string> grouping in ListMatching.Where(x => x.StartsWith(engine+"/")).GroupBy(x => x.Split('/')[1]).OrderBy(x => x.Key)) {
					stringBuilder.Append("  "+engine+" "+grouping.Key+": "+grouping.Count()+" match\r\n");
				}
			}
			foreach(string path in ListMissing) {
				stringBuilder.Append("Missing: "+path+"\r\n");
			}
			foreach(string path in ListNoBaseline) {
				stringBuilder.Append("No baseline: "+path+"\r\n");
			}
			foreach(string different in ListDifferent) {
				stringBuilder.Append("Different: "+different+"\r\n");
			}
			if(countCompared==0) {
				stringBuilder.Append("No pictures were compared.\r\n");
			}
			return stringBuilder.ToString();
		}

		///<summary>The baseline folder that was compared with, as Compare was given it: relative to the repository root when it is inside it (for
		///example "tests\ToothChartImageTests\Baselines\legacy"), otherwise the path as given.</summary>
		private string GetBaselineName() {
			if(string.IsNullOrWhiteSpace(FolderBaseline)) {
				return "no baseline folder";
			}
			string folder=FolderBaseline.TrimEnd('\\','/');
			string repoRoot;
			try {
				repoRoot=TestPaths.RepoRoot;
			}
			catch(InvalidOperationException) {
				return folder;
			}
			if(folder.StartsWith(repoRoot+"\\",StringComparison.OrdinalIgnoreCase)) {
				return folder.Substring(repoRoot.Length+1);
			}
			return folder;
		}
	}
}
