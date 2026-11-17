// Group U's list of windows (UiGallery\windows.tsv, package UI-0c of docs/specs/ui-refresh-plan.md): each row is one check and one picture,
// with the window's type, how the program opens it and the tables it could write. Read by the test's side (which checks to run, which to
// record as SKIPPED because the synthetic dump cannot fill them) and by the session's side (InProcChecks.U.cs, which opens and pictures each
// one), and checked against the program's source by UiHarnessTests without starting anything.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WindowChecksTests {
	///<summary>One row of windows.tsv.</summary>
	public class UiWindowRow {
		public const string OPENED_MENU="menu";
		public const string OPENED_HANDLER="handler";
		public const string OPENED_BUTTON="button";
		public const string OPENED_GRID="grid";
		public const string OPENED_ACCOUNTROW="accountrow";
		public const string OPENED_SCRIPT="script";
		public const string OPENED_MAIN="main";
		public const string OPENED_DASHBOARD="dashboard";
		public const string OPENED_NONE="none";
		public static readonly string[] ArrayOpened=new[] { OPENED_MENU,OPENED_HANDLER,OPENED_BUTTON,OPENED_GRID,OPENED_ACCOUNTROW,OPENED_SCRIPT,OPENED_MAIN,
			OPENED_DASHBOARD,OPENED_NONE };
		public string Id="";
		public string Picture="";
		public string Window="";
		public bool HasPatient;
		///<summary>An EnumModuleType name, or "" for none (the file's "-").</summary>
		public string Module="";
		public string Opened="";
		public string Target="";
		///<summary>The tables whose row counts are compared before and after the window (none for "-").</summary>
		public List<string> ListGuardTables=new List<string>();
		public string Note="";
		public int LineNumber;

		///<summary>The check's title in results.tsv and the report.</summary>
		public string Title {
			get {
				if(Opened==OPENED_NONE) {
					return Window+" (not filled by the synthetic dump)";
				}
				if(Opened==OPENED_MAIN) {
					return "Main window "+Target+", "+Module+(HasPatient ? ", patient #1" : ", no patient");
				}
				if(Opened==OPENED_DASHBOARD) {
					return "Patient dashboard with a widget, "+Target;
				}
				return Window+" ("+Opened+": "+Target+")";
			}
		}

		///<summary>The picture's name without .png: the walker's window key in the bounds record.</summary>
		public string WindowKey {
			get {
				return Path.GetFileNameWithoutExtension(Picture);
			}
		}

		///<summary>For menu: the path's items ("Setup", "Schedules").</summary>
		public string[] MenuPath {
			get {
				return Target.Split('>').Select(x => x.Trim()).Where(x => x!="").ToArray();
			}
		}

		///<summary>For handler, button and grid: the main window's field of the module control (controlAppt).</summary>
		public string ModuleField {
			get {
				string first=Target.Split('|')[0].Trim();
				int index=first.IndexOf('.');
				return index<0 ? "" : first.Substring(0,index);
			}
		}

		///<summary>For handler, button and grid: the method, button field or grid field after the module field.</summary>
		public string Member {
			get {
				string first=Target.Split('|')[0].Trim();
				int index=first.IndexOf('.');
				return index<0 ? first : first.Substring(index+1);
			}
		}

		///<summary>For grid: the row to double-click, "tag:a;b" or "text:...".</summary>
		public string GridMatch {
			get {
				int index=Target.IndexOf('|');
				return index<0 ? "" : Target.Substring(index+1).Trim();
			}
		}

		///<summary>For main and dashboard: the main window's size.</summary>
		public System.Drawing.Size MainSize {
			get {
				Match match=Regex.Match(Target,@"^(\d+)x(\d+)$");
				if(!match.Success) {
					return System.Drawing.Size.Empty;
				}
				return new System.Drawing.Size(int.Parse(match.Groups[1].Value),int.Parse(match.Groups[2].Value));
			}
		}
	}

	public static class UiWindows {
		public const string HEADER="id\tpicture\twindow\tpatient\tmodule\topened\ttarget\tguard\tnote";
		public static readonly string[] ArrayModules=new[] { "Appointments","Family","Account","TreatPlan","Chart","Imaging","Manage" };

		public static string PathTsv {
			get {
				return Path.Combine(WinChkEnv.RepoRoot,"tests","WindowChecksTests","UiGallery","windows.tsv");
			}
		}

		public static List<UiWindowRow> Read() {
			return Parse(File.ReadAllText(PathTsv),"windows.tsv");
		}

		///<summary>The rows of windows.tsv. Lines that start with # and the header are skipped; a malformed row throws a FormatException that names its line.
		///</summary>
		public static List<UiWindowRow> Parse(string text,string sourceName) {
			List<UiWindowRow> listRows=new List<UiWindowRow>();
			string[] arrayLines=(text??"").Replace("\r\n","\n").Split('\n');
			for(int i=0;i<arrayLines.Length;i++) {
				string line=arrayLines[i].TrimEnd('\r');
				if(line.Trim()=="" || line.StartsWith("#") || line==HEADER) {
					continue;
				}
				string where=sourceName+" line "+(i+1)+": ";
				string[] arrayFields=line.Split('\t');
				if(arrayFields.Length!=9) {
					throw new FormatException(where+"9 tab-separated fields expected ("+HEADER.Replace("\t",", ")+"), found "+arrayFields.Length+".");
				}
				UiWindowRow row=new UiWindowRow();
				row.LineNumber=i+1;
				row.Id=arrayFields[0].Trim();
				row.Picture=arrayFields[1].Trim();
				row.Window=arrayFields[2].Trim();
				string patient=arrayFields[3].Trim();
				row.Module=arrayFields[4].Trim()=="-" ? "" : arrayFields[4].Trim();
				row.Opened=arrayFields[5].Trim();
				row.Target=arrayFields[6].Trim();
				string guard=arrayFields[7].Trim();
				row.Note=arrayFields[8].Trim();
				if(!Regex.IsMatch(row.Id,@"^U(M|D)?[1-9]\d*$")) {
					throw new FormatException(where+"the id is U<n>, UM<n> or UD<n>: "+row.Id);
				}
				if(!row.Picture.StartsWith(row.Id+"-") || !row.Picture.EndsWith(".png") || !Regex.IsMatch(row.Picture,@"^[A-Za-z0-9.-]+$")) {
					throw new FormatException(where+"the picture is <id>-<name>.png (letters, digits, '-'): "+row.Picture);
				}
				if(!Regex.IsMatch(row.Window,@"^(Form|Frm)\w+$")) {
					throw new FormatException(where+"the window is a Form or Frm type name: "+row.Window);
				}
				if(patient!="yes" && patient!="no") {
					throw new FormatException(where+"patient is yes or no: "+patient);
				}
				row.HasPatient=patient=="yes";
				if(row.Module!="" && !ArrayModules.Contains(row.Module)) {
					throw new FormatException(where+"the module is - or one of "+string.Join(", ",ArrayModules)+": "+row.Module);
				}
				if(!UiWindowRow.ArrayOpened.Contains(row.Opened)) {
					throw new FormatException(where+"opened is one of "+string.Join(", ",UiWindowRow.ArrayOpened)+": "+row.Opened);
				}
				if(row.Target=="") {
					throw new FormatException(where+"the target is empty");
				}
				if(guard!="-") {
					row.ListGuardTables=guard.Split(',').Select(x => x.Trim()).ToList();
					if(row.ListGuardTables.Any(x => !Regex.IsMatch(x,"^[a-z0-9_]+$"))) {
						throw new FormatException(where+"guard is - or table names (lower case), comma-separated: "+guard);
					}
				}
				listRows.Add(row);
			}
			return listRows;
		}

		///<summary>The row of the check id, or null.</summary>
		public static UiWindowRow Get(string id) {
			return Read().FirstOrDefault(x => x.Id==id);
		}
	}
}
