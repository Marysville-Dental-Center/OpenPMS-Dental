// UI-COLS (docs/port-notes/UI-COLS.md): the columns of GridOD that hold a fixed-format value (a date, a date and time, a time, money, a
// percentage, a phone number, a tooth number, surfaces, a procedure code) hold their widest normal value on one line in the program's grid
// font (UiTheme.FontGrid, Segoe UI 9 since UI-2b), at 100% and 150%. The columns are read from the source: every "new GridColumn(" with a
// literal heading and a literal width, and every "new DisplayField(" (the defaults and the available fields of DisplayFields, which the
// display-field grids take their columns and widths from), in the program's folders (SourceIndex.ArrayFolders) except WpfControlsOD, whose
// WPF grid has its own font (Segoe UI 11.5 DIPs since Open Dental) and was not changed by the font flip. The value type comes from the heading
// (GetKind); Data\grid-col-known.tsv corrects it where the heading says less than the code (a "Pat" column that holds an X, a "Time" column
// that formats 12:00p, a date and time that wraps on two lines by design) with the reason. The width a value needs is measured as GridOD
// measures and draws a cell: Graphics.MeasureString and DrawString with its StringFormat (a tab stop at 50, wrapping on) and its text
// rendering hint, the font at the scale's size (GridOD.ComputeRowHeightOne), in a rectangle as wide as the column (GridOD.DrawRows).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CodeBase;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	///<summary>A column in the source: a GridColumn or a DisplayField with a literal heading and width.</summary>
	public class ColumnSite {
		///<summary>Relative to the repository root, with forward slashes.</summary>
		public string File="";
		public int Line;
		///<summary>"GridColumn" or "DisplayField".</summary>
		public string Source="";
		public string Title="";
		public int Width;
		///<summary>IsWidthDynamic in the statement or the statement after it: GridOD gives the column a share of the room left.</summary>
		public bool IsDynamic;
		///<summary>The value type (GridColumnFit.DictValues' keys), "other" for none; from the heading or Data\grid-col-known.tsv.</summary>
		public string Kind="";
		///<summary>The widest normal value the column shows.</summary>
		public string Value="";
		///<summary>The index of the column's block: the columns of one grid's fill code (statements no more than 12 lines apart), or of one
		///display-field category (Case).</summary>
		public int Block;
		///<summary>A DisplayField's method and category ("GetDefaultList.AccountModule"); null for a GridColumn.</summary>
		public string Case;
		///<summary>Left, Right or Center, from the GridColumn's arguments.</summary>
		public string Alignment="Left";
		///<summary>UI-COLS-2: the window's designer file sets HasAutoWrappedHeaders on a grid (GridOD wraps a heading at its spaces and makes the
		///header as high as the most lines): each word of the heading fits, not the whole line. Read per file, so it holds for every grid of it.</summary>
		public bool IsHeadingWrapped;

		public override string ToString() {
			return File+":"+Line+" "+Source+" \""+Title+"\" "+Width;
		}
	}

	///<summary>A row of Data\grid-col-known.tsv: a column whose value type or widest value is not the heading's.</summary>
	public class ColumnKnown {
		public string File="";
		public string Title="";
		public int Width;
		public string Kind="";
		public string Value="";
		public string Reason="";
		public bool IsUsed;
	}

	public static class GridColumnFit {
		public const string FILE_KNOWN="grid-col-known.tsv";
		public const string HEADER_KNOWN="file\ttitle\twidth\tkind\tvalue\treason";
		///<summary>The widest normal value of each value type, as the program formats it in en-US with the four-digit year it sets at its start
		///(DateTimeOD.NormalizeApplicationShortDateFormat).</summary>
		public static readonly Dictionary<string,string> DictValues=new Dictionary<string,string> {
			{ "date","12/31/2026" },
			{ "datetime","12/31/2026 11:00 AM" },//AM is wider than PM in Segoe UI (the gallery's first pass: 11:15 AM wrapped in 122 px)
			{ "time","11:00 AM" },
			{ "money","9,999.99" },
			{ "percent","100%" },
			{ "phone","(555)555-5555" },
			{ "tooth","32" },
			{ "surface","MODBL" },
			{ "code","D0120" },
		};
		///<summary>UI-COLS-2: the widest normal value of a money column that can show a negative amount (a balance, an adjustment): GetValue.</summary>
		public const string MONEY_NEGATIVE="-9,999.99";
		///<summary>The scales the columns are checked at.</summary>
		public static readonly float[] ArrayScales=new float[] { 1f,1.5f };
		///<summary>The clear pixels a value needs between its ink and a gridline (GetWidthNeededClear): the cell padding GDI+ gives it on the
		///aligned side (2 px at 100%) on the other side too.</summary>
		public const int CLEAR_MIN=2;
		private static readonly Dictionary<string,int> _dictNeeded=new Dictionary<string,int>();
		private static readonly Dictionary<string,int[]> _dictInk=new Dictionary<string,int[]>();
		private static readonly object _lock=new object();
		private static readonly Regex _regexGridColumn=new Regex(
			@"new\s+(?:OpenPmsDental\.UI\.)?GridColumn\(\s*(?:Lans?\.g\([^,""()]*(?:""[^""]*"")?[^,""()]*,\s*)?""(?<t>[^""]*)""\)?\s*,\s*(?<w>\d+)(?<rest>[^;\r\n]*)");
		private static readonly Regex _regexDisplayField=new Regex(@"new\s+DisplayField\(\s*""(?<t>[^""]*)""\s*,\s*(?<w>\d+)");

		///<summary>The value type a heading says, or null: dates (Date, Due, Sent, Entry, Received, DOB, ...), date and time, time, money (Fee,
		///Amount, Amt, Balance, Paid, Pat, Ins, Total, Charges, Credits, Adj, Est, ...), percentages, phone numbers, tooth numbers, surfaces and
		///procedure codes.</summary>
		public static string GetKind(string title) {
			string s=title.ToLowerInvariant().Trim();
			if(s=="") {
				return null;
			}
			if(Regex.IsMatch(s,@"%|percent|\bpct\b")) {
				return "percent";
			}
			if(Regex.IsMatch(s,@"phone|\bfax\b|wireless")) {
				return "phone";
			}
			if(s.Contains("date") && s.Contains("time")) {
				return "datetime";
			}
			if(Regex.IsMatch(s,@"date|^due$|^sent$|^entry$|^dob$|birth|^created$|^received$|^expires?$|^exp$|last visit")) {
				return "date";
			}
			if(Regex.IsMatch(s,@"^(start |stop |end )?time$|^time (in|out)$")) {
				return "time";
			}
			if(Regex.IsMatch(s,@"^(fee|fees|amount|amt|balance|bal|paid|pat|ins|total|charges?|credits?|adj|adjustments?|est|ins est|pat port|payment|payments"
				+@"|writeoff|write off|write-off|owed|price|cost|deductible|ded|copay|allowed|income|production|prod|collected|debit|credit|amt paid|ins paid"
				+@"|ins pay|pay amt|ins pay amt|pat portion|pri ins|sec ins|fee billed|ins pay est|amount due|amt due|due amt|unearned|over 90|0-30|31-60"
				+@"|61-90|over90)$"))
			{
				return "money";
			}
			if(Regex.IsMatch(s,@"\b(fee|amount|amt|balance|paid|writeoff|charges|credits|deductible)\b")) {
				return "money";
			}
			if(Regex.IsMatch(s,@"^(th|tth|tooth|tooth num|tooth #)$")) {
				return "tooth";
			}
			if(Regex.IsMatch(s,@"^(surf|surface|surfaces)$")) {
				return "surface";
			}
			if(Regex.IsMatch(s,@"^(code|proc code|proccode)$")) {
				return "code";
			}
			return null;
		}

		///<summary>UI-COLS-2: the widest normal value of the value type for a column with this heading: DictValues' value, MONEY_NEGATIVE for a
		///money column whose heading says a balance or an adjustment (the account module shows a credit balance and an adjustment as -50.00).
		///"" for "other".</summary>
		public static string GetValue(string kind,string title) {
			if(kind==null || !DictValues.ContainsKey(kind)) {
				return "";
			}
			if(kind=="money" && Regex.IsMatch(title.ToLowerInvariant(),@"\bbal(ance)?\b|\badj(ustments?)?\b")) {
				return MONEY_NEGATIVE;
			}
			return DictValues[kind];
		}

		///<summary>A width at the scale, as GridOD.ScaleI rounds it.</summary>
		public static int ScaleI(float width96,float scale) {
			return (int)Math.Round(width96*scale);
		}

		///<summary>The narrowest column, in pixels at the scale, that shows the text on one line: GridOD draws a cell with DrawString in a
		///rectangle as wide as the column (ClearType grid fit, its StringFormat with wrapping on, or off with the trimming at a character), so a
		///narrower column wraps the text or cuts it. The font is the grid font at the scale's size (GridOD.ComputeRowHeightOne measures so).</summary>
		public static int GetWidthNeeded(string text,float scale,bool isBold=false) {
			string key=text+"\t"+scale.ToString(CultureInfo.InvariantCulture)+"\t"+isBold;
			lock(_lock) {
				int widthCached;
				if(_dictNeeded.TryGetValue(key,out widthCached)) {
					return widthCached;
				}
				Font fontToken=isBold ? UiTheme.FontGridBold : UiTheme.FontGrid;
				using(Font font=new Font(fontToken.FontFamily,fontToken.SizeInPoints*scale,fontToken.Style))
				using(Bitmap bitmap=new Bitmap(8,8,PixelFormat.Format24bppRgb))
				using(Graphics g=Graphics.FromImage(bitmap))
				using(StringFormat stringFormat=NewGridStringFormat()) {
					g.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;
					float heightLine=g.MeasureString("X",font,10000,stringFormat).Height;
					int width=1;
					while(width<2000 && g.MeasureString(text,font,width,stringFormat).Height>heightLine*1.5f) {
						width++;
					}
					_dictNeeded[key]=width;
					return width;
				}
			}
		}

		///<summary>The pixels of a value drawn as GridOD draws a cell (GetWidthNeeded's font, hint and StringFormat, black on white) in a
		///rectangle this wide from x=0: the first and the last column with ink, -1 and -1 for none. Pixel 0 is where the gridline left of a
		///column stands and pixel width the next one, so the value has inkLeft-1 clear pixels on its left and width-1-inkRight on its right.</summary>
		public static int[] GetInk(string text,float scale,int width,string alignment="Left",bool isBold=false) {
			string key="ink\t"+text+"\t"+scale.ToString(CultureInfo.InvariantCulture)+"\t"+width+"\t"+alignment+"\t"+isBold;
			lock(_lock) {
				int[] arrayCached;
				if(_dictInk.TryGetValue(key,out arrayCached)) {
					return arrayCached;
				}
				Font fontToken=isBold ? UiTheme.FontGridBold : UiTheme.FontGrid;
				using(Font font=new Font(fontToken.FontFamily,fontToken.SizeInPoints*scale,fontToken.Style))
				using(Bitmap bitmap=new Bitmap(width+8,(int)Math.Ceiling(font.GetHeight(96f)*3)+4,PixelFormat.Format24bppRgb))
				using(Graphics g=Graphics.FromImage(bitmap))
				using(StringFormat stringFormat=NewGridStringFormat()) {
					g.Clear(Color.White);
					g.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;
					stringFormat.Alignment=alignment=="Right" ? StringAlignment.Far
						: (alignment=="Center" ? StringAlignment.Center : StringAlignment.Near);
					g.DrawString(text,font,Brushes.Black,new RectangleF(0,0,width,bitmap.Height),stringFormat);
					int inkLeft=-1;
					int inkRight=-1;
					for(int x=0;x<bitmap.Width;x++) {
						for(int y=0;y<bitmap.Height;y++) {
							Color color=bitmap.GetPixel(x,y);
							if(color.R<255 || color.G<255 || color.B<255) {
								if(inkLeft<0) {
									inkLeft=x;
								}
								inkRight=x;
								break;
							}
						}
					}
					int[] arrayInk=new int[] { inkLeft,inkRight };
					_dictInk[key]=arrayInk;
					return arrayInk;
				}
			}
		}

		///<summary>The narrowest column, in pixels at the scale, that shows the value on one line (GetWidthNeeded) with at least CLEAR_MIN pixels
		///clear between its ink and the gridline on the side the column's alignment leaves free: the right for Left and Center, the left for
		///Right. A value drawn up to the gridline runs into the next column's value ("06/10/2025|Alex"). The
		///side the alignment holds the value to keeps GDI+'s own padding (about 1/6 em), which no width changes.</summary>
		///<remarks>UI-COLS-2: isBoth checks both sides (the headings, which GridOD draws centered), isBold measures in the header font.</remarks>
		public static int GetWidthNeededClear(string text,float scale,string alignment="Left",bool isBold=false,bool isBoth=false) {
			int width=GetWidthNeeded(text,scale,isBold);
			while(width<2000 && GetClear(text,scale,width,alignment,isBold,isBoth)<CLEAR_MIN) {
				width++;
			}
			return width;
		}

		///<summary>The clear pixels on the side the alignment leaves free, at this width. See GetInk.</summary>
		public static int GetClear(string text,float scale,int width,string alignment="Left",bool isBold=false,bool isBoth=false) {
			int[] arrayInk=GetInk(text,scale,width,alignment,isBold);
			if(arrayInk[0]<0) {
				return width;
			}
			int clearLeft=arrayInk[0]-1;
			int clearRight=width-1-arrayInk[1];
			if(isBoth) {
				return Math.Min(clearLeft,clearRight);
			}
			if(alignment=="Right") {
				return clearLeft;
			}
			//Center: GDI+ rounds the odd pixel of the room to the left (12/31/2026 in 70 px: 3 left, 1 right), so the right side decides.
			return clearRight;
		}

		///<summary>GridOD's StringFormat on screen: a tab stop at 50, the top line, wrapping on (WrapText).</summary>
		public static StringFormat NewGridStringFormat() {
			StringFormat stringFormat=new StringFormat();
			stringFormat.SetTabStops(0.0f,new float[] { 50.0f });
			stringFormat.LineAlignment=StringAlignment.Near;
			return stringFormat;
		}

		///<summary>The folders read: the program's (SourceIndex.ArrayFolders) without WpfControlsOD (the WPF grid).</summary>
		public static List<string> GetFolders() {
			return SourceIndex.ArrayFolders.Where(x => x!="WpfControlsOD").ToList();
		}

		///<summary>Every GridColumn and DisplayField with a literal heading and width in the folders, with its value type ("other" when it has
		///none) and widest normal value, corrected by the known list (whose rows that matched are marked IsUsed).</summary>
		public static List<ColumnSite> ScanSource(string root,List<ColumnKnown> listKnown) {
			List<ColumnSite> listSites=new List<ColumnSite>();
			int block=0;
			foreach(string folder in GetFolders()) {
				string path=Path.Combine(root,folder);
				if(!Directory.Exists(path)) {
					continue;
				}
				foreach(string file in Directory.EnumerateFiles(path,"*.cs",SearchOption.AllDirectories).OrderBy(x => x,StringComparer.Ordinal)) {
					if(IsBuildFolder(file)) {
						continue;
					}
					string text=File.ReadAllText(file);
					if(!text.Contains("GridColumn(") && !text.Contains("DisplayField(")) {
						continue;
					}
					string fileRelative=file.Substring(root.Length).TrimStart('\\','/').Replace('\\','/');
					List<ColumnSite> listFile=new List<ColumnSite>();
					AddMatches(listFile,text,fileRelative,"GridColumn",_regexGridColumn);
					AddMatches(listFile,text,fileRelative,"DisplayField",_regexDisplayField);
					bool isHeadingWrapped=IsHeadingWrappedInDesigner(file);
					int lineBefore=-100;
					string caseBefore=null;
					foreach(ColumnSite columnSite in listFile.OrderBy(x => x.Line)) {
						if(columnSite.Line-lineBefore>12 || columnSite.Case!=caseBefore) {
							block++;
						}
						columnSite.Block=block;
						columnSite.IsHeadingWrapped=isHeadingWrapped;
						lineBefore=columnSite.Line;
						caseBefore=columnSite.Case;
						ApplyKind(columnSite,listKnown);
						listSites.Add(columnSite);
					}
				}
			}
			return listSites;
		}

		///<summary>UI-COLS-2: the file's designer file (Form.Designer.cs or Form.designer.cs next to Form.cs) sets HasAutoWrappedHeaders true.</summary>
		private static bool IsHeadingWrappedInDesigner(string file) {
			if(file.EndsWith(".designer.cs",StringComparison.OrdinalIgnoreCase)) {
				return false;
			}
			string pathDesigner=file.Substring(0,file.Length-3)+".Designer.cs";//Windows: the name's case does not matter
			if(!File.Exists(pathDesigner)) {
				return false;
			}
			return Regex.IsMatch(File.ReadAllText(pathDesigner),@"\.HasAutoWrappedHeaders\s*=\s*true");
		}

		private static void AddMatches(List<ColumnSite> listFile,string text,string fileRelative,string source,Regex regex) {
			foreach(Match match in regex.Matches(text)) {
				int indexLineStart=text.LastIndexOf('\n',Math.Max(0,match.Index-1))+1;
				string before=text.Substring(indexLineStart,match.Index-indexLineStart);
				if(before.Contains("//") || before.TrimStart().StartsWith("*")) {
					continue;//a comment
				}
				ColumnSite columnSite=new ColumnSite();
				columnSite.File=fileRelative;
				columnSite.Line=CountLines(text,match.Index);
				columnSite.Source=source;
				columnSite.Title=match.Groups["t"].Value;
				columnSite.Width=int.Parse(match.Groups["w"].Value,CultureInfo.InvariantCulture);
				columnSite.Alignment=match.Groups["rest"].Value.Contains("HorizontalAlignment.Right") ? "Right"
					: (match.Groups["rest"].Value.Contains("HorizontalAlignment.Center") ? "Center" : "Left");
				if(source=="DisplayField") {
					//The display fields of one category: the list and the method (GetDefaultList or GetAllAvailableList) from the case label above.
					int indexCase=text.LastIndexOf("case DisplayFieldCategory.",match.Index,StringComparison.Ordinal);
					int indexMethod=text.LastIndexOf("public static List<DisplayField> ",match.Index,StringComparison.Ordinal);
					if(indexCase>=0) {
						Match matchCase=Regex.Match(text.Substring(indexCase),@"^case DisplayFieldCategory\.(\w+)");
						Match matchMethod=indexMethod<0 ? Match.Empty : Regex.Match(text.Substring(indexMethod),@"^public static List<DisplayField> (\w+)");
						columnSite.Case=(matchMethod.Success ? matchMethod.Groups[1].Value+"." : "")+matchCase.Groups[1].Value;
					}
				}
				//IsWidthDynamic in the initializer, or in the next statement (col.IsWidthDynamic=true;).
				int indexEnd=text.IndexOf(';',match.Index);
				if(indexEnd>=0) {
					string statement=text.Substring(match.Index,indexEnd-match.Index);
					int indexNextEnd=text.IndexOf(';',indexEnd+1);
					string statementNext=indexNextEnd<0 ? "" : text.Substring(indexEnd+1,indexNextEnd-indexEnd-1);
					columnSite.IsDynamic=statement.Contains("IsWidthDynamic") || Regex.IsMatch(statementNext,@"^\s*\w+\.IsWidthDynamic\s*=\s*true\s*$");
				}
				listFile.Add(columnSite);
			}
		}

		private static void ApplyKind(ColumnSite columnSite,List<ColumnKnown> listKnown) {
			string kind=GetKind(columnSite.Title)??"other";
			string value=GetValue(kind,columnSite.Title);
			ColumnKnown columnKnown=listKnown?.FirstOrDefault(x => x.File==columnSite.File && x.Title==columnSite.Title && x.Width==columnSite.Width);
			if(columnKnown!=null) {
				columnKnown.IsUsed=true;
				kind=columnKnown.Kind;
				value=columnKnown.Value!="" ? columnKnown.Value : GetValue(kind,columnSite.Title);
			}
			columnSite.Kind=kind;
			columnSite.Value=value;
		}

		private static int CountLines(string text,int index) {
			int count=1;
			for(int i=0;i<index;i++) {
				if(text[i]=='\n') {
					count++;
				}
			}
			return count;
		}

		private static bool IsBuildFolder(string file) {
			string lower=file.Replace('/','\\').ToLowerInvariant();
			return lower.Contains("\\bin\\") || lower.Contains("\\obj\\");
		}

		///<summary>Data\grid-col-known.tsv.</summary>
		public static List<ColumnKnown> ReadKnown() {
			List<ColumnKnown> listKnown=new List<ColumnKnown>();
			foreach(string line in File.ReadAllLines(RepoPaths.DataFile(FILE_KNOWN))) {
				if(line.Trim()=="" || line.StartsWith("#") || line==HEADER_KNOWN) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				if(arrayFields.Length!=6) {
					throw new InvalidDataException(FILE_KNOWN+": "+HEADER_KNOWN+": "+line);
				}
				ColumnKnown columnKnown=new ColumnKnown();
				columnKnown.File=arrayFields[0];
				columnKnown.Title=arrayFields[1];
				columnKnown.Width=int.Parse(arrayFields[2],CultureInfo.InvariantCulture);
				columnKnown.Kind=arrayFields[3];
				columnKnown.Value=arrayFields[4];
				columnKnown.Reason=arrayFields[5];
				listKnown.Add(columnKnown);
			}
			return listKnown;
		}

		///<summary>The problems of a column at the scales: "" when its value fits on one line with CLEAR_MIN pixels clear of the gridline
		///(GetWidthNeededClear) and its heading fits on one line in the header font (GridOD.DrawHeaders draws it in UiTheme.FontGridBold, centered,
		///and the header is one line high, so a wider heading is cut: "Amoun"). A column without a value (no value type, or "other" without a
		///value in the known list) has its heading checked only (UI-COLS-2: every visible column's heading, 2 px clear of both gridlines); a
		///dynamic column and a hidden one (width 0, or sized by its grid, not by its width) have none.</summary>
		public static string GetProblem(ColumnSite columnSite) {
			//UI-COLS-2: a column without a value has its heading checked (GetProblemAt).
			if(columnSite.IsDynamic || columnSite.Width==0) {
				return "";
			}
			List<string> listProblems=new List<string>();
			foreach(float scale in ArrayScales) {
				string problem=GetProblemAt(columnSite.Title,columnSite.Kind,columnSite.Value,columnSite.Alignment,ScaleI(columnSite.Width,scale),scale,
					columnSite.IsHeadingWrapped);
				if(problem!="") {
					listProblems.Add(problem);
				}
			}
			return string.Join("; ",listProblems);
		}

		///<summary>The problem of a column of a value this wide (in pixels at the scale): its value or its heading does not fit (GetProblem), ""
		///when both do.</summary>
		public static string GetProblemAt(string title,string kind,string value,string alignment,int width,float scale,bool isHeadingWrapped=false) {
			List<string> listProblems=new List<string>();
			//UI-COLS-2: money stands CLEAR_MIN pixels clear of the gridline its alignment leaves free (9,999.99, -9,999.99 where it can be
			//negative), like a date; the side it is held to keeps GDI+'s padding (1 px at 100%, 3 px at 150% for 9,999.99), which no width changes.
			int widthNeeded=value=="" ? 0 : GetWidthNeededClear(value,scale,alignment);
			if(width<widthNeeded) {
				listProblems.Add("\""+value+"\" needs "+widthNeeded+(width<GetWidthNeeded(value,scale) ? "" : " (to stand "+CLEAR_MIN+" px clear)"));
			}
			//A heading of two lines ("Date\r\nShowing" in the source) has a header two lines high: each line fits. UI-COLS-2: every visible
			//column's heading, centered as GridOD.DrawHeaders draws it, with CLEAR_MIN pixels clear of both gridlines.
			List<string> listLines=title.Split(new string[] { "\\r\\n","\\n" },StringSplitOptions.None).ToList();
			if(isHeadingWrapped) {
				listLines=listLines.SelectMany(x => x.Split(' ')).ToList();//each word on a line of its own at the narrowest
			}
			foreach(string line in listLines) {
				if(line.Trim()=="") {
					continue;
				}
				int widthHeading=GetWidthNeededClear(line,scale,"Center",isBold:true,isBoth:true);
				if(width<widthHeading) {
					listProblems.Add("the heading \""+line+"\" needs "+widthHeading);
				}
			}
			if(listProblems.Count==0) {
				return "";
			}
			return (int)Math.Round(scale*100)+"%: "+width+" px, "+string.Join(", ",listProblems);
		}
	}
}
