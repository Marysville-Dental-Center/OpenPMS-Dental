// REPORT.md in the results folder, written by the harness at the end of every run (AssemblyCleanup, after Z) from results.tsv: the header
// (commit, the exes' times, the databases and the session of the runs in the report, the run's times), one line per group run with its P0 and
// Z, the table of the last result of each check on this commit, the failures with their evidence, the list for an on-screen pass (NEEDS-SCREEN
// with the reason, the PARTLY checks' "for a person" items, SKIPPED with the reason), the steps the harness does not script, and the results
// of older commits that this commit has not run again (listed apart, not counted).
// Several runs (split by OPMS_WINDOW_CHECKS_GROUPS) append to the same results.tsv, so the report written after the last run covers them all.
// No connection string, password or FreeDentalConfig.xml content goes into it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WindowChecksTests {
	public static class Report {
		///<summary>Cells of the checks table are cut at this length; results.tsv holds each check's full text.</summary>
		public const int LENGTH_CELL=700;

		public static string PathReport {
			get {
				return Path.Combine(WinChkEnv.OutFolder,"REPORT.md");
			}
		}

		///<summary>Writes REPORT.md from results.tsv. Returns "" or the problem (the caller logs it; a run never fails for its report).</summary>
		public static string Write() {
			try {
				File.WriteAllText(PathReport,Build(),new UTF8Encoding(false));
				return "";
			}
			catch(Exception ex) {
				return ex.GetType().Name+": "+ex.Message;
			}
		}

		///<summary>One group run: its P0 row and the Z row that ended it (null when the run was stopped before its clean-up).</summary>
		private class RunRows {
			public string[] P0;
			public string[] Z;
		}

		public static string Build() {
			List<string[]> listAll=Results.ReadAll();
			string commit=Results.Commit;
			//This commit's results; a row of another commit (or written before results.tsv held the commit) is older.
			List<string[]> listCurrent=listAll.Where(x => x[6]==commit).ToList();
			List<RunRows> listRuns=new List<RunRows>();
			foreach(string[] arrayFields in listCurrent) {
				if(arrayFields[1]=="P0") {
					listRuns.Add(new RunRows { P0=arrayFields });
				}
				else if(arrayFields[1]=="Z" && listRuns.Count>0 && listRuns[listRuns.Count-1].Z==null) {
					listRuns[listRuns.Count-1].Z=arrayFields;
				}
			}
			List<string[]> listRows=Latest(listCurrent.Where(x => x[1]!="P0" && x[1]!="Z"));
			List<string> listIdsCurrent=listRows.Select(x => x[1]).ToList();
			List<string[]> listOlder=Latest(listAll.Where(x => x[6]!=commit && x[1]!="P0" && x[1]!="Z" && !listIdsCurrent.Contains(x[1])));
			StringBuilder sb=new StringBuilder();
			sb.AppendLine("# Stage 1 gate: launch checks, headless (WINCHK-HEADLESS)");
			sb.AppendLine();
			sb.AppendLine("Written by the harness (tests/WindowChecksTests, Harness/Report.cs) at the end of the run, from results.tsv in this folder: the last "
				+"result of each check on commit "+commit+". Checklist: .git/od-tools/briefs/stage1-launch-checks.md. Every picture named below is in this folder.");
			sb.AppendLine();
			string counts=string.Join(", ",listRows.GroupBy(x => x[2]).OrderBy(x => x.Key).Select(x => x.Key+" "+x.Count()));
			sb.AppendLine("**Counts (last result per check, this commit):** "+(counts=="" ? "no results" : counts)+"; group runs: "+listRuns.Count
				+(listOlder.Count==0 ? "" : "; "+listOlder.Count+" older results of checks not run on this commit are listed apart at the end, not counted")+".");
			sb.AppendLine();
			sb.AppendLine("## Header");
			sb.AppendLine();
			foreach(string line in GetHeaderLines(listCurrent,listRuns)) {
				sb.AppendLine("- "+line);
			}
			sb.AppendLine();
			sb.AppendLine("## Group runs (P0 and Z of each)");
			sb.AppendLine();
			sb.AppendLine("Every run restores its own scratch copy, checks it (P0, the checklist's 0.3) and cleans up (Z) before the copy is dropped.");
			sb.AppendLine();
			sb.AppendLine("| Started | Groups | Database | Session at the start | P0 | Z |");
			sb.AppendLine("|---|---|---|---|---|---|");
			foreach(RunRows runRows in listRuns) {
				string textP0=runRows.P0[4];
				sb.AppendLine("| "+runRows.P0[0].Replace("T"," ")+" | "+Md(GetValue(textP0,"Groups: "))+" | "+Md(GetValue(textP0,"Database: "))+" | "
					+Md(GetValue(textP0,"Session: "))+" | "+runRows.P0[2]+(runRows.P0[2]==CheckStatus.Failed ? ": "+Cell(textP0,300) : "")+" | "
					+(runRows.Z==null ? "none: the run was stopped before its clean-up" : runRows.Z[2]+" ("+runRows.Z[0].Replace("T"," ")+")"
					+(runRows.Z[2]==CheckStatus.Failed ? ": "+Cell(runRows.Z[4],300) : ""))+" |");
			}
			sb.AppendLine();
			sb.AppendLine("## Checks");
			sb.AppendLine();
			sb.AppendLine("The 'What was verified' cells are cut at "+LENGTH_CELL+" characters (marked ' ...'); results.tsv in this folder holds each check's full text.");
			sb.AppendLine();
			sb.AppendLine("| Check | Status | Title | What was verified | Screenshots | Run |");
			sb.AppendLine("|---|---|---|---|---|---|");
			foreach(string[] arrayFields in listRows) {
				sb.AppendLine("| "+arrayFields[1]+" | "+arrayFields[2]+" | "+Cell(arrayFields[3],200)+" | "+Cell(arrayFields[4],LENGTH_CELL)+" | "+Cell(arrayFields[5],300)
					+" | "+arrayFields[0].Replace("T"," ")+" |");
			}
			sb.AppendLine();
			sb.AppendLine("## Failures");
			sb.AppendLine();
			List<string[]> listFailed=listRows.Where(x => x[2]==CheckStatus.Failed).ToList();
			if(listFailed.Count==0) {
				sb.AppendLine("None.");
				sb.AppendLine();
			}
			foreach(string[] arrayFields in listFailed) {
				sb.AppendLine("### "+arrayFields[1]+": "+Md(arrayFields[3]));
				sb.AppendLine();
				sb.AppendLine("- Run: "+arrayFields[0].Replace("T"," "));
				sb.AppendLine("- Evidence (the check's full text, failures first): "+Md(arrayFields[4]));
				if(arrayFields[5]!="") {
					sb.AppendLine("- Pictures: "+Md(arrayFields[5]));
				}
				sb.AppendLine();
			}
			sb.AppendLine("## For an on-screen pass");
			sb.AppendLine();
			List<string[]> listNeedsScreen=listRows.Where(x => x[2]==CheckStatus.NeedsScreen).ToList();
			sb.AppendLine("NEEDS-SCREEN ("+listNeedsScreen.Count+"): headless does not reach them.");
			sb.AppendLine();
			foreach(string[] arrayFields in listNeedsScreen) {
				sb.AppendLine("- "+arrayFields[1]+" "+Md(arrayFields[3])+": "+Md(arrayFields[4]));
			}
			sb.AppendLine();
			List<string[]> listPartly=listRows.Where(x => x[2]==CheckStatus.Partly).ToList();
			sb.AppendLine("PARTLY ("+listPartly.Count+"): what code verified passed; these items are for a person to judge.");
			sb.AppendLine();
			foreach(string[] arrayFields in listPartly) {
				sb.AppendLine("- "+arrayFields[1]+" "+Md(arrayFields[3])+": "+Md(GetPart(arrayFields[4],"For a person: ")));
			}
			sb.AppendLine();
			List<string[]> listSkipped=listRows.Where(x => x[2]==CheckStatus.Skipped).ToList();
			sb.AppendLine("SKIPPED ("+listSkipped.Count+"):");
			sb.AppendLine();
			foreach(string[] arrayFields in listSkipped) {
				sb.AppendLine("- "+arrayFields[1]+" "+Md(arrayFields[3])+": "+Md(arrayFields[4]));
			}
			sb.AppendLine();
			sb.AppendLine("## Not scripted");
			sb.AppendLine();
			List<string[]> listNotScripted=listRows.Where(x => x[4].Contains("Not scripted: ")).ToList();
			sb.AppendLine("Steps of the checklist the harness does not drive as a click would (done by the business layer, or not done): gaps of the harness, not "
				+"items for a person to judge. They do not change a check's status ("+listNotScripted.Count+" checks).");
			sb.AppendLine();
			foreach(string[] arrayFields in listNotScripted) {
				sb.AppendLine("- "+arrayFields[1]+" "+Md(arrayFields[3])+" ("+arrayFields[2]+"): "+Md(GetPart(arrayFields[4],"Not scripted: ")));
			}
			sb.AppendLine();
			sb.AppendLine("## Older results (not run on this commit)");
			sb.AppendLine();
			if(listOlder.Count==0) {
				sb.AppendLine("None: every check in results.tsv has a result on this commit, or results.tsv has no older ones.");
			}
			else {
				sb.AppendLine("The last result of each check that has no result on commit "+commit+": from an older commit, or written before results.tsv held "
					+"the commit. Not counted above; run their groups again to bring them up to date.");
				sb.AppendLine();
				sb.AppendLine("| Check | Status | Title | Commit | Run |");
				sb.AppendLine("|---|---|---|---|---|");
				foreach(string[] arrayFields in listOlder) {
					sb.AppendLine("| "+arrayFields[1]+" | "+arrayFields[2]+" | "+Cell(arrayFields[3],200)+" | "+(arrayFields[6]=="" ? "not recorded" : arrayFields[6])
						+" | "+arrayFields[0].Replace("T"," ")+" |");
				}
			}
			sb.AppendLine();
			return sb.ToString();
		}

		///<summary>The last row of each check id, in the report's order (group letter, then number).</summary>
		private static List<string[]> Latest(IEnumerable<string[]> rows) {
			Dictionary<string,string[]> dictionary=new Dictionary<string,string[]>();
			foreach(string[] arrayFields in rows) {
				dictionary[arrayFields[1]]=arrayFields;
			}
			return dictionary.Values.OrderBy(x => SortKey(x[1])).ToList();
		}

		///<summary>The header's lines: the commit, the exes, the databases, the session and the times of the results in the table.</summary>
		private static List<string> GetHeaderLines(List<string[]> listCurrent,List<RunRows> listRuns) {
			List<string> listLines=new List<string>();
			listLines.Add("Commit: "+RunGit("rev-parse --abbrev-ref HEAD")+", "+RunGit("log -1 --format=\"%h %cd\"")+" (the results' commit field: "+Results.Commit
				+"; '+changes' when tracked files differed from it)");
			foreach(string[] arrayExe in new[] { new[] { "Debug",WinChkEnv.DebugFolder },new[] { "Release",WinChkEnv.ReleaseFolder } }) {
				string pathExe=Path.Combine(arrayExe[1],"OpenPmsDental.exe");
				if(!File.Exists(pathExe)) {
					listLines.Add(arrayExe[0]+" exe: none at <worktree>\\"+Relative(pathExe));
					continue;
				}
				//On .NET the exe is the apphost, which the build rewrites only when it changes: the time of the build is the managed assembly's,
				//OpenPmsDental.dll beside it (WINCHK-5; ProgramAssembly gives the .exe on .NET Framework).
				pathExe=ProjectFileTests.ProgramAssembly.Find(arrayExe[1],"OpenPmsDental")??pathExe;
				DateTime dateWrite=File.GetLastWriteTime(pathExe);
				listLines.Add(arrayExe[0]+" program <worktree>\\"+Relative(pathExe)+": "+dateWrite.ToString("yyyy-MM-dd HH:mm:ss")+" local ("
					+dateWrite.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")+" UTC)");
			}
			listLines.Add("Database: a scratch copy per run, winchk_<MMddHHmmss> on "+ScratchDb.Server+":"+ScratchDb.Port+", restored from the dump "
				+Path.GetFileName(WinChkEnv.DumpPath??"")+" and dropped at the end of the run; its A to Z folder is a scratch folder in %TEMP%. Databases of the runs "
				+"in this report: "+string.Join(", ",GetDatabases(listRuns)));
			try {
				System.Windows.Forms.Screen[] arrayScreens=System.Windows.Forms.Screen.AllScreens;
				int dpi;
				using(System.Drawing.Graphics g=System.Drawing.Graphics.FromHwnd(IntPtr.Zero)) {
					dpi=(int)g.DpiX;
				}
				listLines.Add("Display: "+arrayScreens.Length+" screen(s), primary "+System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width+"x"
					+System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height+", "+dpi+" DPI (as the test host sees it). The checks need no visible desktop: windows "
					+"are printed with PrintWindow, DrawToBitmap or RenderTargetBitmap; a DirectX surface prints blank while the screen is locked.");
			}
			catch(Exception ex) {
				listLines.Add("Display: not read ("+ex.Message+")");
			}
			listLines.Add("Session: "+Native.GetSessionState()+" when this report was written; each run's session at its start is in its P0 line below");
			List<DateTime> listTimes=listCurrent.Select(x => DateTime.ParseExact(x[0],"s",CultureInfo.InvariantCulture)).ToList();
			if(listTimes.Count>0) {
				listLines.Add("Results of this commit: "+listTimes.Min().ToString("yyyy-MM-dd HH:mm:ss")+" to "+listTimes.Max().ToString("yyyy-MM-dd HH:mm:ss")
					+" local; this report written "+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+" after the run of groups "
					+(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(WinChkEnv.VARIABLE_GROUPS)) ? "all" : Environment.GetEnvironmentVariable(WinChkEnv.VARIABLE_GROUPS))+".");
			}
			listLines.Add("Statuses: PASSED (code verified every item the check scripts; steps it does not script are named under 'Not scripted'), PARTLY "
				+"(what code verified passed; some items are for a person), NEEDS-SCREEN (headless does not reach it), SKIPPED (elevation, or optional), FAILED "
				+"(with the evidence).");
			listLines.Add("Program exits (exits.txt): "+(File.Exists(Path.Combine(WinChkEnv.OutFolder,"exits.txt"))
				? File.ReadAllLines(Path.Combine(WinChkEnv.OutFolder,"exits.txt")).Length+" timed WM_CLOSE exits recorded" : "none recorded"));
			return listLines;
		}

		///<summary>The scratch copies of the runs in the report (each P0 names its own), with what run.log says of their drop.</summary>
		private static List<string> GetDatabases(List<RunRows> listRuns) {
			string pathLog=Path.Combine(WinChkEnv.OutFolder,"run.log");
			string log=File.Exists(pathLog) ? File.ReadAllText(pathLog) : "";
			List<string> listNames=listRuns.Select(x => GetValue(x.P0[4],"Database: ")).Where(x => x.StartsWith(ScratchDb.PREFIX)).Distinct().ToList();
			if(ScratchDb.IsMade && !listNames.Contains(ScratchDb.Name)) {
				listNames.Add(ScratchDb.Name);
			}
			if(listNames.Count==0) {
				return new List<string> { "none named" };
			}
			return listNames.Select(x => x+(Regex.IsMatch(log,"Dropped "+x+"\\b") ? " (dropped)" : (ScratchDb.IsMade && x==ScratchDb.Name ? " (this run's)"
				: " (no drop in run.log)"))).ToList();
		}

		private static string Relative(string path) {
			string root=WinChkEnv.RepoRoot.TrimEnd('\\')+"\\";
			return path.StartsWith(root,StringComparison.OrdinalIgnoreCase) ? path.Substring(root.Length) : path;
		}

		///<summary>Runs git in the worktree and returns its output, or "(git ...)" when it cannot. Kills only the git process it started.</summary>
		public static string RunGit(string args) {
			try {
				ProcessStartInfo processStartInfo=new ProcessStartInfo("git",args) {
					WorkingDirectory=WinChkEnv.RepoRoot,
					UseShellExecute=false,
					RedirectStandardOutput=true,
					RedirectStandardError=true,
					CreateNoWindow=true,
				};
				using Process process=Process.Start(processStartInfo);
				System.Threading.Tasks.Task<string> taskError=process.StandardError.ReadToEndAsync();
				string output=process.StandardOutput.ReadToEnd();
				if(!process.WaitForExit(20000)) {
					process.Kill();
					return "(git did not answer)";
				}
				return output.Trim();
			}
			catch(Exception ex) {
				return "(git: "+ex.Message+")";
			}
		}

		///<summary>The value after the label in a P0 text ("Groups: A,B; ..."), up to the next " / " or ";", or "".</summary>
		private static string GetValue(string text,string label) {
			Match match=Regex.Match(text??"",Regex.Escape(label)+"([^;/]*)");
			return match.Success ? match.Groups[1].Value.Trim() : "";
		}

		///<summary>The text after the marker up to the next part ("Not scripted: ", "Notes: "), or the whole text when the marker is absent.</summary>
		private static string GetPart(string summary,string marker) {
			int index=summary.IndexOf(marker,StringComparison.Ordinal);
			if(index<0) {
				return summary;
			}
			string part=summary.Substring(index+marker.Length);
			foreach(string markerNext in new[] { " Not scripted: "," Notes: " }) {
				int indexNext=part.IndexOf(markerNext,StringComparison.Ordinal);
				if(indexNext>=0) {
					part=part.Substring(0,indexNext);
				}
			}
			return part;
		}

		///<summary>Group letter, then the check's number, then the rest ("P0" first, "Z" last).</summary>
		private static string SortKey(string id) {
			Match match=Regex.Match(id,"^([A-Z]+)(\\d*)(.*)$");
			if(!match.Success) {
				return "Y"+id;
			}
			string letter=match.Groups[1].Value;
			string prefix=letter=="P" ? "0" : (letter=="Z" ? "Z" : "1"+letter);
			int number=match.Groups[2].Value=="" ? 0 : int.Parse(match.Groups[2].Value);
			return prefix+number.ToString("000")+match.Groups[3].Value;
		}

		private static string Md(string text) {
			return (text??"").Replace("|","\\|").Replace("\r"," ").Replace("\n"," ");
		}

		private static string Cell(string text,int maxLength) {
			string cell=Md(text);
			return cell.Length<=maxLength ? cell : cell.Substring(0,maxLength)+" ...";
		}
	}
}
