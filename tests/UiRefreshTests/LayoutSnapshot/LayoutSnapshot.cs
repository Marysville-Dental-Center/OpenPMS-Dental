// The layout snapshot (docs/specs/ui-refresh-plan.md section 3.7, docs/port-notes/UI-0b.md): every designer control's position and size, as the
// designer code writes them (this.x.Location = new System.Drawing.Point(..); this.x.Size = new System.Drawing.Size(..); the window's own Size
// and ClientSize as "."), read from InitializeComponent in every designer file of the product projects. Data\layout-snapshot.tsv is the record;
// LayoutSnapshotTests fails on any change that fit-fixes.tsv does not list. The scan's bounds record covers what the designer code cannot show
// (scaling at run time).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UiRefreshTests.Scan;

namespace UiRefreshTests.LayoutSnapshot {
	///<summary>One designer control: where InitializeComponent puts it.</summary>
	public class LayoutSnapshotRow {
		///<summary>The designer file, relative to the repository root, with forward slashes.</summary>
		public string File="";
		///<summary>The control's field name, or "." for the window or UserControl itself.</summary>
		public string Control="";
		public string Location="";
		public string Size="";
		public string ClientSize="";

		public string Key {
			get { return File+"\t"+Control; }
		}

		public string ToLine() {
			return File+"\t"+Control+"\t"+Location+"\t"+Size+"\t"+ClientSize;
		}
	}

	public static class LayoutSnapshotBuilder {
		public const string HEADER="file\tcontrol\tlocation\tsize\tclient_size";
		public const string VARIABLE_WRITE="OPMS_UI_LAYOUT_SNAPSHOT_WRITE";
		private static readonly Regex _regexLine=new Regex(
			@"^\s*this\.(?:(\w+)\.)?(Location|Size|ClientSize)\s*=\s*new\s+(?:System\.Drawing\.)?(?:Point|Size)\(\s*(-?\d+)\s*,\s*(-?\d+)\s*\)\s*;",
			RegexOptions.Compiled);
		private static readonly Regex _regexInitializeComponent=new Regex(@"\bvoid\s+InitializeComponent\s*\(\s*\)",RegexOptions.Compiled);

		///<summary>The snapshot of the repository's designer files now, in a stable order (file, then the order in the file).</summary>
		public static List<LayoutSnapshotRow> Build(string root) {
			List<LayoutSnapshotRow> listRows=new List<LayoutSnapshotRow>();
			List<string> listFiles=new List<string>();
			foreach(string folder in SourceIndex.ArrayFolders) {
				string path=Path.Combine(root,folder);
				if(!Directory.Exists(path)) {
					continue;
				}
				foreach(string file in Directory.EnumerateFiles(path,"*.cs",SearchOption.AllDirectories)) {
					string normalized=file.Replace('\\','/');
					if(normalized.Contains("/bin/") || normalized.Contains("/obj/")) {
						continue;
					}
					listFiles.Add(file);
				}
			}
			foreach(string file in listFiles.OrderBy(x => ToRelative(root,x),StringComparer.Ordinal)) {
				string text=System.IO.File.ReadAllText(file);
				if(!_regexInitializeComponent.IsMatch(text)) {//a quick look; Parse checks without comments
					continue;
				}
				listRows.AddRange(Parse(ToRelative(root,file),text));
			}
			return listRows;
		}

		private static string ToRelative(string root,string file) {
			return file.Substring(root.Length).TrimStart('\\','/').Replace('\\','/');
		}

		///<summary>The rows of one file: the Location, Size and ClientSize lines inside InitializeComponent, one row per control in the order
		///each control first appears.</summary>
		public static List<LayoutSnapshotRow> Parse(string fileRelative,string text) {
			List<LayoutSnapshotRow> listRows=new List<LayoutSnapshotRow>();
			Dictionary<string,LayoutSnapshotRow> dictRows=new Dictionary<string,LayoutSnapshotRow>();
			//The braces are matched in the text without comments and strings (the same length), the lines read from the text itself.
			string textStripped=SourceIndex.StripCommentsAndStrings(text);
			Match matchMethod=_regexInitializeComponent.Match(textStripped);
			while(matchMethod.Success) {
				int start;
				int end;
				string body="";
				if(SourceIndex.GetBodyBounds(textStripped,textStripped.IndexOf('(',matchMethod.Index),out start,out end)) {
					body=text.Substring(start,end-start+1);
				}
				foreach(string line in body.Split('\n')) {
					Match match=_regexLine.Match(line);
					if(!match.Success) {
						continue;
					}
					string control=match.Groups[1].Success ? match.Groups[1].Value : ".";
					LayoutSnapshotRow row;
					if(!dictRows.TryGetValue(control,out row)) {
						row=new LayoutSnapshotRow { File=fileRelative,Control=control };
						dictRows[control]=row;
						listRows.Add(row);
					}
					string value=match.Groups[3].Value+","+match.Groups[4].Value;
					switch(match.Groups[2].Value) {
						case "Location":
							row.Location=value;
							break;
						case "Size":
							row.Size=value;
							break;
						default:
							row.ClientSize=value;
							break;
					}
				}
				matchMethod=matchMethod.NextMatch();
			}
			return listRows;
		}

		public static List<LayoutSnapshotRow> Read(IEnumerable<string> listLines) {
			List<LayoutSnapshotRow> listRows=new List<LayoutSnapshotRow>();
			foreach(string lineRaw in listLines) {
				string line=lineRaw.TrimEnd('\r');
				if(line=="" || line.StartsWith("#") || line==HEADER) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				if(arrayFields.Length!=5) {
					throw new FormatException("layout-snapshot.tsv: a row has "+arrayFields.Length+" fields, not 5: "+line);
				}
				listRows.Add(new LayoutSnapshotRow { File=arrayFields[0],Control=arrayFields[1],Location=arrayFields[2],Size=arrayFields[3],ClientSize=arrayFields[4] });
			}
			return listRows;
		}

		public static IEnumerable<string> ToLines(List<LayoutSnapshotRow> listRows) {
			yield return "# The layout snapshot of the UI refresh (docs/specs/ui-refresh-plan.md section 3.7, docs/port-notes/UI-0b.md): every designer";
			yield return "# control's Location, Size and ClientSize as InitializeComponent sets them, \".\" for the window or UserControl itself. Written by";
			yield return "# LayoutSnapshotTests with "+VARIABLE_WRITE+"=1; the test fails on any change that Data/fit-fixes.tsv does not list.";
			yield return "# One file per rollout slice (UI-12T): the designer files a slice owns; unsliced.tsv the rest (a slice file wins over it).";
			yield return HEADER;
			foreach(LayoutSnapshotRow row in listRows) {
				yield return row.ToLine();
			}
		}

		///<summary>Writes the snapshot as UTF-8 without a BOM, with the line ends the file at path already has, so a rewrite never flips them.
		///The repository has no eol rule for this file and core.autocrlf is true, so its blob keeps the line ends it was committed with (LF), while
		///the working copy is LF or CRLF depending on how it was checked out. The file's own line end is reused (CRLF when its first line ends in
		///CRLF), and git add with core.autocrlf=true stores either as the LF blob. A new file gets LF, the committed blob's line end.</summary>
		public static void Write(string path,List<LayoutSnapshotRow> listRows) {
			string lineEnd=LineEndOf(path);
			StringBuilder stringBuilder=new StringBuilder();
			foreach(string line in ToLines(listRows)) {
				stringBuilder.Append(line).Append(lineEnd);
			}
			System.IO.File.WriteAllText(path,stringBuilder.ToString(),new UTF8Encoding(false));
		}

		///<summary>UI-12T: the record of Data\layout-snapshot\<slice>.tsv. A slice file wins over unsliced.tsv for every designer file it holds
		///(a slice writes only its own file; split_grid_data.py prunes unsliced.tsv when the lead reruns it after the merge).</summary>
		public static List<LayoutSnapshotRow> ReadFolder(string folder) {
			List<LayoutSnapshotRow> listSliced=new List<LayoutSnapshotRow>();
			List<LayoutSnapshotRow> listUnsliced=new List<LayoutSnapshotRow>();
			foreach(string file in SliceData.Files(folder)) {
				List<LayoutSnapshotRow> listRows=Read(System.IO.File.ReadAllLines(file));
				if(SliceData.SliceOf(file)==SliceData.UNSLICED) {
					listUnsliced.AddRange(listRows);
				}
				else {
					listSliced.AddRange(listRows);
				}
			}
			HashSet<string> hashSetHeld=new HashSet<string>(listSliced.Select(x => x.File));
			return listSliced.Concat(listUnsliced.Where(x => !hashSetHeld.Contains(x.File))).ToList();
		}

		///<summary>UI-12T: writes the snapshot into its slice files. Each slice's file gets the rows of the designer files it owns, from listNow;
		///unsliced.tsv gets the rows of the files no slice owns, from listNow, and keeps the rows it already has of files a slice owns, so a
		///slice's rewrite changes only its own file (and unsliced.tsv only where a file no slice owns changed). A slice file left with no rows
		///is removed. Every file keeps its line ends (Write).</summary>
		public static void WriteFolder(string folder,List<LayoutSnapshotRow> listNow,SliceData.SnapshotOwners snapshotOwners) {
			Directory.CreateDirectory(folder);
			string pathUnsliced=Path.Combine(folder,SliceData.UNSLICED+".tsv");
			List<LayoutSnapshotRow> listUnslicedOld=System.IO.File.Exists(pathUnsliced) ? Read(System.IO.File.ReadAllLines(pathUnsliced)) : new List<LayoutSnapshotRow>();
			Dictionary<string,List<LayoutSnapshotRow>> dictBySlice=new Dictionary<string,List<LayoutSnapshotRow>>();
			Dictionary<string,List<LayoutSnapshotRow>> dictUnsliced=new Dictionary<string,List<LayoutSnapshotRow>>();
			foreach(LayoutSnapshotRow row in listNow) {
				string slice=snapshotOwners.Owner(row.File);
				if(slice==SliceData.UNSLICED) {
					AddTo(dictUnsliced,row.File,row);
					continue;
				}
				AddTo(dictBySlice,slice,row);
			}
			foreach(LayoutSnapshotRow row in listUnslicedOld) {
				if(snapshotOwners.Owner(row.File)!=SliceData.UNSLICED) {
					AddTo(dictUnsliced,row.File,row);
				}
			}
			List<LayoutSnapshotRow> listUnsliced=new List<LayoutSnapshotRow>();
			foreach(string file in dictUnsliced.Keys.OrderBy(x => x,StringComparer.Ordinal)) {
				listUnsliced.AddRange(dictUnsliced[file]);
			}
			Write(pathUnsliced,listUnsliced);
			foreach(KeyValuePair<string,List<LayoutSnapshotRow>> pair in dictBySlice) {
				Write(Path.Combine(folder,pair.Key+".tsv"),pair.Value);
			}
			foreach(string file in SliceData.Files(folder)) {
				string slice=SliceData.SliceOf(file);
				if(slice!=SliceData.UNSLICED && !dictBySlice.ContainsKey(slice)) {
					System.IO.File.Delete(file);
				}
			}
		}

		private static void AddTo(Dictionary<string,List<LayoutSnapshotRow>> dict,string key,LayoutSnapshotRow row) {
			if(!dict.ContainsKey(key)) {
				dict[key]=new List<LayoutSnapshotRow>();
			}
			dict[key].Add(row);
		}

		///<summary>The line end of the file at path: "\r\n" when its first line ends in CRLF, otherwise "\n" (also for a missing or one-line file).</summary>
		public static string LineEndOf(string path) {
			if(!System.IO.File.Exists(path)) {
				return "\n";
			}
			using(FileStream fileStream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite)) {
				int bytePrevious=-1;
				int byteNow;
				while((byteNow=fileStream.ReadByte())!=-1) {
					if(byteNow=='\n') {
						return bytePrevious=='\r' ? "\r\n" : "\n";
					}
					bytePrevious=byteNow;
				}
			}
			return "\n";
		}

		///<summary>The changes from the record to now: changed, added and removed rows, except the controls fit-fixes.tsv lists.</summary>
		public static List<string> Compare(List<LayoutSnapshotRow> listRecord,List<LayoutSnapshotRow> listNow,ICollection<string> setFitFixKeys) {
			List<string> listChanges=new List<string>();
			Dictionary<string,LayoutSnapshotRow> dictRecord=new Dictionary<string,LayoutSnapshotRow>();
			foreach(LayoutSnapshotRow row in listRecord) {
				dictRecord[row.Key]=row;
			}
			HashSet<string> setNow=new HashSet<string>();
			foreach(LayoutSnapshotRow row in listNow) {
				setNow.Add(row.Key);
				if(setFitFixKeys.Contains(row.Key)) {
					continue;
				}
				LayoutSnapshotRow rowRecord;
				if(!dictRecord.TryGetValue(row.Key,out rowRecord)) {
					listChanges.Add("added: "+row.ToLine());
					continue;
				}
				if(rowRecord.ToLine()!=row.ToLine()) {
					listChanges.Add("changed: "+rowRecord.ToLine()+"  ->  "+row.Location+"\t"+row.Size+"\t"+row.ClientSize);
				}
			}
			foreach(LayoutSnapshotRow row in listRecord) {
				if(!setNow.Contains(row.Key) && !setFitFixKeys.Contains(row.Key)) {
					listChanges.Add("removed: "+row.ToLine());
				}
			}
			return listChanges;
		}
	}
}
