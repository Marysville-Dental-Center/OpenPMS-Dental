// The check U0 of group U (package UI-0c of docs/specs/ui-refresh-plan.md, sections 7 and 8): every picture this commit's run took is compared
// with the owner's approvals in tests\WindowChecksTests\UiApproved\approved.tsv through the ONE masking and hashing rule of the galleries
// (tools\UiGallery\UiPictureMask.cs, UI-0a, linked into this project): its masked pixel SHA-256 (the masks of tools\ui-gallery-masks.tsv) and its
// size. A picture is "approved" (exactly the one the owner last approved), "changed since approval" (approved before, now different: for the
// owner, in the package's gallery) or "not approved yet" (never approved). Until the before set exists and the owner approves it, approved.tsv is
// empty and every picture is "not approved yet": that is reported, never a failure. With OPMS_WINDOW_CHECKS_UI_BEFORE (the before set's results
// folder) U0 also compares the walker's bounds record with the before set's (TextFitBoundsRecord.Compare; fit-fixes.tsv allowed).
// U0 runs once at the end of a run that includes group U (WinChkFixture.AssemblyCleanup), so it covers the pictures of every group of the run.
// It writes U0.tsv (each picture's state) and U0-approved-rows.tsv (approved.tsv rows for the pictures that are not approved, for the lead to
// append once the owner approves them in a gallery) into the results folder.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UiGalleryTools;
using UiRefreshTests.TextFit;

namespace WindowChecksTests {
	///<summary>One picture's state against approved.tsv.</summary>
	public class UiPictureState {
		public string Picture="";
		public int Width;
		public int Height;
		public string Sha256="";
		///<summary>UiApprovalCheck.STATE_*.</summary>
		public string State="";
		///<summary>The owner's last approval of the picture, or null.</summary>
		public UiApprovedRow ApprovedRow;
	}

	public static class UiApprovalCheck {
		public const string STATE_APPROVED="approved";
		public const string STATE_CHANGED="changed since approval";
		public const string STATE_NOT_APPROVED="not approved yet";
		public const string STATE_MISSING="missing";
		///<summary>The before set's results folder (it holds ui-walker\bounds-record.tsv): U0 also compares the bounds records.</summary>
		public const string VARIABLE_BEFORE="OPMS_WINDOW_CHECKS_UI_BEFORE";
		public const string TEXT_EMPTY="approved.tsv holds no approval yet, so every picture is reported as not approved yet, not as a failure: the owner "
			+"approves pictures in the UI refresh's galleries once the before set exists (docs/specs/ui-refresh-plan.md, section 8)";

		public static string PathMasks {
			get {
				return Path.Combine(WinChkEnv.RepoRoot,"tools","ui-gallery-masks.tsv");
			}
		}

		public static string PathApproved {
			get {
				return Path.Combine(WinChkEnv.RepoRoot,"tests","WindowChecksTests","UiApproved","approved.tsv");
			}
		}

		///<summary>The pictures of the results rows (Results.ReadAll: time, id, status, title, text, pictures, commit) written at the commit, each once,
		///in the order they were first written.</summary>
		public static List<string> GetPictures(List<string[]> listRows,string commit) {
			List<string> listPictures=new List<string>();
			HashSet<string> setPictures=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach(string[] arrayFields in listRows) {
				if(arrayFields.Length<7 || arrayFields[6]!=commit) {
					continue;
				}
				foreach(string picture in arrayFields[5].Split(',').Select(x => x.Trim()).Where(x => x!="")) {
					if(setPictures.Add(picture)) {
						listPictures.Add(picture);
					}
				}
			}
			return listPictures;
		}

		///<summary>Each picture of the folder (by file name) against the approvals, through the masking and hashing rule.</summary>
		public static List<UiPictureState> Classify(string folder,IEnumerable<string> enumerablePictures,IList<UiMaskRow> listMasks,
			IList<UiApprovedRow> listApproved)
		{
			List<UiPictureState> listStates=new List<UiPictureState>();
			foreach(string picture in enumerablePictures) {
				UiPictureState uiPictureState=new UiPictureState();
				uiPictureState.Picture=UiPictureMask.NormalizePicture(picture);
				uiPictureState.ApprovedRow=UiPictureMask.FindApproved(listApproved,uiPictureState.Picture);
				string path=Path.Combine(folder,uiPictureState.Picture);
				if(!File.Exists(path)) {
					uiPictureState.State=STATE_MISSING;
					listStates.Add(uiPictureState);
					continue;
				}
				UiPixels uiPixels=UiPictureMask.LoadPixels(path);
				uiPictureState.Width=uiPixels.Width;
				uiPictureState.Height=uiPixels.Height;
				uiPictureState.Sha256=UiPictureMask.GetMaskedPixelSha256(uiPixels,uiPictureState.Picture,listMasks);
				if(UiPictureMask.IsApproved(listApproved,uiPictureState.Picture,uiPictureState.Width,uiPictureState.Height,uiPictureState.Sha256)) {
					uiPictureState.State=STATE_APPROVED;
				}
				else if(uiPictureState.ApprovedRow!=null) {
					uiPictureState.State=STATE_CHANGED;
				}
				else {
					uiPictureState.State=STATE_NOT_APPROVED;
				}
				listStates.Add(uiPictureState);
			}
			return listStates;
		}

		///<summary>U0's result for the states: PASSED when no picture changed since its approval (pictures not approved yet are reported, not
		///failed), PARTLY with the changed ones for the owner. Writes U0.tsv and U0-approved-rows.tsv into folderOut.</summary>
		public static CheckResult Check(List<UiPictureState> listStates,int countApproved,string folderOut,string commit,string date) {
			CheckResult checkResult=new CheckResult("U0","The pictures against the owner's approvals (approved.tsv)");
			List<UiPictureState> listApproved=listStates.Where(x => x.State==STATE_APPROVED).ToList();
			List<UiPictureState> listChanged=listStates.Where(x => x.State==STATE_CHANGED).ToList();
			List<UiPictureState> listNotApproved=listStates.Where(x => x.State==STATE_NOT_APPROVED).ToList();
			List<UiPictureState> listMissing=listStates.Where(x => x.State==STATE_MISSING).ToList();
			checkResult.Verified(listStates.Count+" picture(s) of this run hashed by the gallery's masking rule (tools\\UiGallery\\UiPictureMask.cs, the masks of "
				+"tools\\ui-gallery-masks.tsv) and compared with approved.tsv ("+countApproved+" approval row(s)): "+listApproved.Count+" "+STATE_APPROVED+", "
				+listChanged.Count+" "+STATE_CHANGED+", "+listNotApproved.Count+" "+STATE_NOT_APPROVED+(listMissing.Count==0 ? "" : ", "+listMissing.Count+" "
				+STATE_MISSING));
			if(countApproved==0) {
				checkResult.Note(TEXT_EMPTY);
			}
			else if(listNotApproved.Count>0) {
				checkResult.Note("Not approved yet (never shown to the owner in a gallery): "+string.Join(", ",listNotApproved.Select(x => x.Picture)));
			}
			if(listChanged.Count>0) {
				checkResult.ForPerson("the pictures changed since the owner approved them, for the package's gallery: "+string.Join(", ",listChanged.Select(x => x.Picture
					+" ("+x.Width+"x"+x.Height+(x.ApprovedRow.Width==x.Width && x.ApprovedRow.Height==x.Height ? "" : ", approved at "+x.ApprovedRow.Width+"x"
					+x.ApprovedRow.Height)+", approved in "+x.ApprovedRow.Gallery+" at "+x.ApprovedRow.Commit+")")));
			}
			if(listMissing.Count>0) {
				checkResult.Note("Listed in results.tsv but not in the results folder: "+string.Join(", ",listMissing.Select(x => x.Picture)));
			}
			if(folderOut!=null) {
				Directory.CreateDirectory(folderOut);
				StringBuilder stringBuilder=new StringBuilder();
				stringBuilder.Append("picture\tsize\tsha256\tstate\tapproved_size\tapproved_sha256\tapproved_gallery\tapproved_commit\r\n");
				foreach(UiPictureState uiPictureState in listStates) {
					UiApprovedRow row=uiPictureState.ApprovedRow;
					stringBuilder.Append(string.Join("\t",new[] { uiPictureState.Picture,uiPictureState.Width+"x"+uiPictureState.Height,uiPictureState.Sha256,
						uiPictureState.State,row==null ? "" : row.Width+"x"+row.Height,row==null ? "" : row.Sha256,row==null ? "" : row.Gallery,row==null ? "" : row.Commit })
						+"\r\n");
				}
				File.WriteAllText(Path.Combine(folderOut,"U0.tsv"),stringBuilder.ToString(),new UTF8Encoding(false));
				stringBuilder.Clear();
				stringBuilder.Append(UiPictureMask.ApprovedHeader+"\r\n");
				foreach(UiPictureState uiPictureState in listStates.Where(x => x.State==STATE_CHANGED || x.State==STATE_NOT_APPROVED)) {
					stringBuilder.Append(string.Join("\t",new[] { uiPictureState.Picture,uiPictureState.Width+"x"+uiPictureState.Height,uiPictureState.Sha256,"G-UI?",
						commit,date })+"\r\n");
				}
				File.WriteAllText(Path.Combine(folderOut,"U0-approved-rows.tsv"),stringBuilder.ToString(),new UTF8Encoding(false));
				checkResult.Note("U0.tsv lists each picture's state; U0-approved-rows.tsv holds approved.tsv rows for the pictures not approved (the gallery's "
					+"name G-UI? to be filled in), for the lead to append once the owner approves them");
			}
			return checkResult;
		}

		#region Bounds against the before set
		///<summary>The bounds record's rows, the last record of each window kept (a results folder can hold several runs).</summary>
		public static List<TextFitBoundsRow> ReadBoundsRecord(string path) {
			List<TextFitBoundsRow> listRows;
			using(StreamReader streamReader=new StreamReader(path)) {
				listRows=TextFitBoundsRecord.Read(streamReader);
			}
			//A window recorded again starts again at its root (path "."): keep the rows from its last root on.
			Dictionary<string,int> dictLastRoot=new Dictionary<string,int>();
			for(int i=0;i<listRows.Count;i++) {
				if(listRows[i].Path==".") {
					dictLastRoot[listRows[i].Window]=i;
				}
			}
			List<TextFitBoundsRow> listKept=new List<TextFitBoundsRow>();
			Dictionary<string,int> dictCurrentRoot=new Dictionary<string,int>();
			for(int i=0;i<listRows.Count;i++) {
				if(listRows[i].Path==".") {
					dictCurrentRoot[listRows[i].Window]=i;
				}
				if(dictCurrentRoot.TryGetValue(listRows[i].Window,out int indexRoot) && dictLastRoot[listRows[i].Window]==indexRoot) {
					listKept.Add(listRows[i]);
				}
			}
			return listKept;
		}

		///<summary>This run's bounds record against the before set's, window by window (the windows both hold), by the rules of section 5.4; the fit
		///fixes of each window's type (its root row's type) are allowed.</summary>
		public static List<TextFitBoundsDifference> CompareBounds(List<TextFitBoundsRow> listBefore,List<TextFitBoundsRow> listAfter) {
			HashSet<string> setWindows=new HashSet<string>(listBefore.Select(x => x.Window));
			setWindows.IntersectWith(listAfter.Select(x => x.Window));
			HashSet<string> setFitFixes=new HashSet<string>();
			foreach(TextFitBoundsRow row in listAfter.Where(x => x.Path=="." && setWindows.Contains(x.Window))) {
				setFitFixes.UnionWith(UiWalker.ReadFitFixKeys(row.Window,row.Type));
			}
			return TextFitBoundsRecord.Compare(listBefore.Where(x => setWindows.Contains(x.Window)).ToList(),listAfter.Where(x => setWindows.Contains(x.Window)).ToList(),
				setFitFixes);
		}
		#endregion Bounds against the before set

		///<summary>U0 at the end of a run with group U: this commit's pictures in the results folder against approved.tsv, and (with
		///OPMS_WINDOW_CHECKS_UI_BEFORE) the walker's bounds record against the before set's. Recorded in results.tsv.</summary>
		public static void Run() {
			CheckResult checkResult;
			try {
				List<UiMaskRow> listMasks=UiPictureMask.ReadMasks(PathMasks);
				List<UiApprovedRow> listApproved=UiPictureMask.ReadApproved(PathApproved);
				List<string> listPictures=GetPictures(Results.ReadAll(),Results.Commit);
				List<UiPictureState> listStates=Classify(WinChkEnv.OutFolder,listPictures,listMasks,listApproved);
				checkResult=Check(listStates,listApproved.Count,WinChkEnv.OutFolder,Results.Commit,DateTime.Now.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture));
				AddBoundsComparison(checkResult);
			}
			catch(Exception ex) {
				checkResult=new CheckResult("U0","The pictures against the owner's approvals (approved.tsv)");
				checkResult.Fail("U0 could not compare: "+ex.GetType().Name+": "+ex.Message);
			}
			Results.Write(checkResult);
			Results.Log("U0 "+checkResult.Status+": "+checkResult.Summary);
		}

		private static void AddBoundsComparison(CheckResult checkResult) {
			string folderBefore=(Environment.GetEnvironmentVariable(VARIABLE_BEFORE)??"").Trim();
			if(folderBefore=="") {
				return;
			}
			string pathBefore=Path.Combine(folderBefore,"ui-walker","bounds-record.tsv");
			string pathAfter=Path.Combine(UiWalker.FolderOut,"bounds-record.tsv");
			if(!File.Exists(pathBefore) || !File.Exists(pathAfter)) {
				checkResult.Fail(VARIABLE_BEFORE+" is set, but "+(File.Exists(pathBefore) ? pathAfter : pathBefore)+" does not exist");
				return;
			}
			List<TextFitBoundsDifference> listDifferences=CompareBounds(ReadBoundsRecord(pathBefore),ReadBoundsRecord(pathAfter));
			StringBuilder stringBuilder=new StringBuilder("window\tkind\tpath\ttype\tother\r\n");
			foreach(TextFitBoundsDifference difference in listDifferences) {
				stringBuilder.Append(string.Join("\t",new[] { difference.Row.Window,difference.Kind,difference.Row.Path,difference.Row.Type,difference.Other })+"\r\n");
			}
			File.WriteAllText(Path.Combine(WinChkEnv.OutFolder,"U0-bounds.tsv"),stringBuilder.ToString(),new UTF8Encoding(false));
			if(listDifferences.Count==0) {
				checkResult.Verified("The walker's bounds record equals the before set's in every window both hold (fit-fixes.tsv allowed)");
			}
			else {
				checkResult.ForPerson(listDifferences.Count+" bounds difference(s) against the before set (U0-bounds.tsv), in "
					+string.Join(", ",listDifferences.Select(x => x.Row.Window).Distinct()));
			}
		}
	}
}
