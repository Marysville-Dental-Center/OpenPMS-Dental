// The outcome of each check, appended to results.tsv in the results folder (one line per check and run), from which the report's table is
// made. Statuses: PASSED (headless, with what was verified), PARTLY (what was verified, and what only a person can judge), NEEDS-SCREEN (why
// headless cannot reach it), SKIPPED (elevation, hardware, or the checklist's section 3) and FAILED (with the evidence). Each line also holds
// the commit the run was built from, so the report can tell this commit's results from older ones.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TestProbeHost;

namespace WindowChecksTests {
	public static class CheckStatus {
		public const string Passed="PASSED";
		public const string Partly="PARTLY";
		public const string NeedsScreen="NEEDS-SCREEN";
		public const string Skipped="SKIPPED";
		public const string Failed="FAILED";
	}

	///<summary>What one check found. Built up while the check runs (notes, screenshots, failures), then written once. An in-process check's
	///result crosses from the session's process as text (ToText, FromText).</summary>
	[Serializable]
	public class CheckResult {
		public string Id;
		public string Title;
		public List<string> ListVerified=new List<string>();
		public List<string> ListForPerson=new List<string>();
		public List<string> ListFailures=new List<string>();
		public List<string> ListShots=new List<string>();
		public List<string> ListNotes=new List<string>();
		///<summary>Steps of the checklist the harness does not script (done by the business layer, or not done): a gap of the harness, not an item
		///for a person to judge. They do not change the status; the report lists them on their own.</summary>
		public List<string> ListNotScripted=new List<string>();
		///<summary>Row numbers a check made, for a later check that undoes them, perhaps in another session (another process): InProcHost carries
		///them from one session to the next, so an undo deletes only the rows its check added.</summary>
		public Dictionary<string,long> DictKeys=new Dictionary<string,long>();
		///<summary>Set for NEEDS-SCREEN and SKIPPED: why.</summary>
		public string Reason="";
		public string StatusForced="";

		public CheckResult(string id,string title) {
			Id=id;
			Title=title;
		}

		public void Verified(string text) {
			ListVerified.Add(text);
		}

		public void ForPerson(string text) {
			ListForPerson.Add(text);
		}

		public void Fail(string text) {
			ListFailures.Add(text);
		}

		public void Note(string text) {
			ListNotes.Add(text);
		}

		public void NotScripted(string text) {
			ListNotScripted.Add(text);
		}

		///<summary>The whole result as one line of ProbeText fields, for the way back from the session's process (InProcHost); FromText reads it.
		///</summary>
		public string ToText() {
			return ProbeText.Join(new[] { Id,Title,ProbeText.JoinList(ListVerified),ProbeText.JoinList(ListForPerson),ProbeText.JoinList(ListFailures),
				ProbeText.JoinList(ListShots),ProbeText.JoinList(ListNotes),ProbeText.JoinList(ListNotScripted),
				ProbeText.JoinList(DictKeys.SelectMany(x => new[] { x.Key,x.Value.ToString(CultureInfo.InvariantCulture) })),Reason,StatusForced });
		}

		public static CheckResult FromText(string text) {
			string[] arrayFields=ProbeText.Split(text);
			if(arrayFields.Length!=11) {
				throw new FormatException("A check result has 11 fields, not "+arrayFields.Length+".");
			}
			CheckResult checkResult=new CheckResult(arrayFields[0],arrayFields[1]);
			checkResult.ListVerified=ProbeText.SplitList(arrayFields[2]).ToList();
			checkResult.ListForPerson=ProbeText.SplitList(arrayFields[3]).ToList();
			checkResult.ListFailures=ProbeText.SplitList(arrayFields[4]).ToList();
			checkResult.ListShots=ProbeText.SplitList(arrayFields[5]).ToList();
			checkResult.ListNotes=ProbeText.SplitList(arrayFields[6]).ToList();
			checkResult.ListNotScripted=ProbeText.SplitList(arrayFields[7]).ToList();
			string[] arrayKeys=ProbeText.SplitList(arrayFields[8]);
			for(int i=0;i<arrayKeys.Length;i+=2) {
				checkResult.DictKeys[arrayKeys[i]]=long.Parse(arrayKeys[i+1],CultureInfo.InvariantCulture);
			}
			checkResult.Reason=arrayFields[9];
			checkResult.StatusForced=arrayFields[10];
			return checkResult;
		}

		///<summary>Expect: records a verified fact when true, a failure when false.</summary>
		public bool Expect(bool condition,string text,string evidence="") {
			if(condition) {
				ListVerified.Add(text);
			}
			else {
				ListFailures.Add(text+(evidence=="" ? "" : " ("+evidence+")"));
			}
			return condition;
		}

		public void AddShot(ShotInfo shotInfo,string whatItShows) {
			if(shotInfo==null) {
				return;
			}
			if(shotInfo.Error!="") {
				ListFailures.Add("Screenshot "+shotInfo.FileName+" not taken: "+shotInfo.Error);
				return;
			}
			ListShots.Add(shotInfo.FileName);
			if(!shotInfo.IsDrawn && shotInfo.BlankReason!="") {
				ForPerson(whatItShows+": "+shotInfo+" is blank: "+shotInfo.BlankReason);
				return;
			}
			Expect(shotInfo.IsDrawn,whatItShows+" is drawn ("+shotInfo+")",shotInfo.ToString());
		}

		public string Status {
			get {
				if(ListFailures.Count>0) {
					return CheckStatus.Failed;
				}
				if(StatusForced!="") {
					return StatusForced;
				}
				if(ListForPerson.Count>0) {
					return CheckStatus.Partly;
				}
				return CheckStatus.Passed;
			}
		}

		public string Summary {
			get {
				StringBuilder stringBuilder=new StringBuilder();
				if(Reason!="") {
					stringBuilder.Append(Reason+" ");
				}
				if(ListFailures.Count>0) {
					stringBuilder.Append("FAILED: "+string.Join("; ",ListFailures)+". ");
				}
				if(ListVerified.Count>0) {
					stringBuilder.Append("Verified: "+string.Join("; ",ListVerified)+". ");
				}
				if(ListForPerson.Count>0) {
					stringBuilder.Append("For a person: "+string.Join("; ",ListForPerson)+". ");
				}
				if(ListNotScripted.Count>0) {
					stringBuilder.Append("Not scripted: "+string.Join("; ",ListNotScripted)+". ");
				}
				if(ListNotes.Count>0) {
					stringBuilder.Append("Notes: "+string.Join("; ",ListNotes)+".");
				}
				return stringBuilder.ToString().Trim();
			}
		}
	}

	public static class Results {
		private static readonly object _lock=new object();
		private static string _commit;

		///<summary>The commit this run was built from: the worktree's HEAD, short, with "+changes" when tracked files differ from it. Read once per run.
		///</summary>
		public static string Commit {
			get {
				lock(_lock) {
					if(_commit==null) {
						string head=Report.RunGit("rev-parse --short HEAD");
						string changes=Report.RunGit("status --porcelain --untracked-files=no");
						_commit=head+(changes=="" || changes.StartsWith("(git") ? "" : "+changes");
					}
					return _commit;
				}
			}
		}

		public static string PathTsv {
			get {
				return Path.Combine(WinChkEnv.OutFolder,"results.tsv");
			}
		}

		public static void Write(CheckResult checkResult) {
			string line=string.Join("\t",new[] { DateTime.Now.ToString("s"),checkResult.Id,checkResult.Status,Clean(checkResult.Title),
				Clean(checkResult.Summary),string.Join(", ",checkResult.ListShots),Commit });
			lock(_lock) {
				File.AppendAllText(PathTsv,line+Environment.NewLine,Encoding.UTF8);
			}
		}

		public static void Log(string text) {
			lock(_lock) {
				File.AppendAllText(Path.Combine(WinChkEnv.OutFolder,"run.log"),DateTime.Now.ToString("HH:mm:ss.fff")+" "+text+Environment.NewLine,Encoding.UTF8);
			}
		}

		private static string Clean(string text) {
			return (text??"").Replace("\t"," ").Replace("\r"," ").Replace("\n"," ");
		}

		///<summary>Every result line of results.tsv in the order written, each with 7 fields (time, id, status, title, text, pictures, commit); a line
		///written before the commit field was added gets "" as its commit.</summary>
		public static List<string[]> ReadAll() {
			List<string[]> listRows=new List<string[]>();
			if(!File.Exists(PathTsv)) {
				return listRows;
			}
			foreach(string line in File.ReadAllLines(PathTsv,Encoding.UTF8).Where(x => x.Trim()!="")) {
				string[] arrayFields=line.Split('\t');
				if(arrayFields.Length<6) {
					continue;
				}
				if(arrayFields.Length<7) {
					arrayFields=arrayFields.Concat(new[] { "" }).ToArray();
				}
				listRows.Add(arrayFields);
			}
			return listRows;
		}
	}
}
